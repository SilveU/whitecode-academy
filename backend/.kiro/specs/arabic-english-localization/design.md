# Design Document — Arabic/English Localization

## Overview

هذا المستند يصف التصميم المعماري الكامل لنظام Localization في مشروع WhiteCodeAcademy Backend API. الهدف هو تحويل الترجمة إلى **Cross-Cutting Concern** بالكامل عبر:

- **RequestLocalizationMiddleware** يقرأ `Accept-Language` ويضبط الـ Culture على الـ Thread.
- **IStringLocalizer** يُحقن في الطبقات المناسبة لقراءة الترجمات من `.resx` files.
- **جميع الرسائل تُخزَّن كـ MessageKeys** في ملفات `.resx`، ولا يُكتب نص مترجم داخل أي `.cs` file.
- **Handlers وValidators وControllers لا تُعدَّل منطقياً** — تبقى تعمل بنفس الباترن الحالي.

---

## Architecture: طريقة انتقال اللغة من الطلب إلى الرسالة

```
HTTP Request
    │  Accept-Language: ar
    │
    ▼
[1] RequestLocalizationMiddleware
    │  يقرأ Accept-Language Header
    │  يضبط Thread.CurrentCulture = ar
    │  يضبط Thread.CurrentUICulture = ar
    │
    ▼
[2] GlobalHandleExceptionMiddleware
    │  يستقبل الاستثناءات
    │  يستخدم IStringLocalizer<ExceptionMessages>
    │  يقرأ MessageKey من Resource File حسب CurrentUICulture
    │
    ▼
[3] FluentValidation Pipeline (PipelineBehavior)
    │  ValidatorLocalizer يُحقن في AbstractValidator
    │  WithMessage(() => localizer[key]) يُقيَّم per-request
    │
    ▼
[4] MediatR Handler
    │  يُرجع Result<T> مع MessageKey (ليس نص مترجم)
    │
    ▼
[5] BaseController.Resolve()
    │  يستقبل Result<T>
    │  يترجم MessageKey عبر IStringLocalizer<CommonMessages>
    │  يُرسل الرسالة المترجمة للعميل
    │
    ▼
HTTP Response
    Body: { "statusCode": 404, "message": "الدورة غير موجودة" }
```

---

## Layer Placement — مكان كل مكوّن

### Domain Layer — لا تعديل

لا يُضاف أي كود Localization للـ Domain. الاستثناءات تبقى كما هي (`NotFoundException`, `BusinessRuleException`). المنطق اللغوي خارج الـ Domain تماماً.

### Application Layer

| المكوّن | المسار | المسؤولية |
|---|---|---|
| `IMessageLocalizer` | `Application/Interfaces/Localization/` | Contract للترجمة — يُخفي `IStringLocalizer` عن باقي الطبقات |
| `ValidationMessages.resx` | `Application/Resources/` | مفاتيح رسائل FluentValidation (EN) |
| `ValidationMessages.ar.resx` | `Application/Resources/` | رسائل FluentValidation (AR) |
| `CommonMessages.resx` | `Application/Resources/` | مفاتيح رسائل النجاح/الفشل لـ Result\<T\> (EN) |
| `CommonMessages.ar.resx` | `Application/Resources/` | رسائل Result\<T\> (AR) |

### API Layer

| المكوّن | المسار | المسؤولية |
|---|---|---|
| `LocalizationExtension.cs` | `API/Extentions/` | تسجيل كل خدمات Localization في DI |
| `ExceptionMessages.resx` | `API/Resources/` | مفاتيح رسائل الاستثناءات (EN) |
| `ExceptionMessages.ar.resx` | `API/Resources/` | رسائل الاستثناءات (AR) |
| `SupportedCultures.cs` | `API/Localization/` | تعريف مركزي للغات المدعومة |
| `MessageKeys.cs` | `API/Localization/` | Constants لجميع المفاتيح (يمنع typos) |

---

## Component Design

### 1. SupportedCultures — المصدر الوحيد للغات

```csharp
// API/Localization/SupportedCultures.cs
namespace API.Localization
{
    public static class SupportedCultures
    {
        public const string English = "en";
        public const string Arabic  = "ar";
        public const string Default = English;

        public static readonly string[] All = [English, Arabic];
    }
}
```

**لماذا هنا؟** `RequestLocalizationMiddleware` يُسجَّل في API Layer. إضافة لغة جديدة = إضافة `const` واحدة + ملفات `.resx`.

---

### 2. LocalizationExtension — تسجيل مركزي

```csharp
// API/Extentions/LocalizationExtension.cs
public static class LocalizationExtension
{
    public static IServiceCollection AddLocalizationExtension(
        this IServiceCollection services)
    {
        services.AddLocalization(); // يُسجّل IStringLocalizerFactory + IStringLocalizer<T>

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supported = SupportedCultures.All
                .Select(c => new CultureInfo(c))
                .ToList();

            options.DefaultRequestCulture = new RequestCulture(SupportedCultures.Default);
            options.SupportedCultures     = supported;
            options.SupportedUICultures   = supported;

            // فقط Accept-Language Header — القراءة من QueryString والكوكيز مُعطَّلة
            options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
        });

        return services;
    }
}
```

---

### 3. IMessageLocalizer — Contract للترجمة

```csharp
// Application/Interfaces/Localization/IMessageLocalizer.cs
namespace Application.Interfaces.Localization
{
    public interface IMessageLocalizer
    {
        string this[string key] { get; }
        string this[string key, params object[] arguments] { get; }
    }
}
```

**لماذا Interface بدل IStringLocalizer مباشرةً؟**
- يُخفي تبعية `Microsoft.Extensions.Localization` عن Application Layer.
- يُسهّل الـ Unit Testing عبر Mock.
- يُمكِّن استبدال مصدر الترجمات مستقبلاً (Database بدلاً من `.resx`) دون تغيير أي Validator.

---

### 4. ValidationMessages Resource Files

**ملف:** `Application/Resources/ValidationMessages.resx` (EN — الافتراضي)  
**ملف:** `Application/Resources/ValidationMessages.ar.resx` (AR)

#### Naming Convention للمفاتيح

```
{Entity}_{Field}_{Rule}
```

| المفتاح | النص الإنجليزي | النص العربي |
|---|---|---|
| `Field_Required` | `'{PropertyName}' is required.` | `'{PropertyName}' مطلوب.` |
| `Field_MaxLength` | `'{PropertyName}' must not exceed {MaxLength} characters.` | `'{PropertyName}' يجب ألا يتجاوز {MaxLength} حرفاً.` |
| `Field_MinLength` | `'{PropertyName}' must be at least {MinLength} characters.` | `'{PropertyName}' يجب أن يكون على الأقل {MinLength} أحرف.` |
| `Field_InvalidEmail` | `'{PropertyName}' is not a valid email address.` | `'{PropertyName}' ليس عنوان بريد إلكتروني صالحاً.` |
| `Field_PasswordsMustMatch` | `Passwords do not match.` | `كلمتا المرور غير متطابقتين.` |
| `Field_InvalidPassword` | `Password must contain letter, number, and special character.` | `كلمة المرور يجب أن تحتوي على حرف ورقم وحرف خاص.` |
| `Field_InvalidUsername` | `Username can only contain letters, numbers, ., _, -, @` | `اسم المستخدم يمكن أن يحتوي فقط على حروف وأرقام و. و_ و- و@` |
| `Field_InvalidNameFormat` | `Name can only contain letters, numbers, and spaces.` | `الاسم يمكن أن يحتوي فقط على حروف وأرقام ومسافات.` |

---

### 5. ValidatorBase — كيف تُستهلك الترجمة في Validators

المنهج: كل Validator يستقبل `IMessageLocalizer` عبر **Constructor Injection** ويستخدم `WithMessage(() => ...)` كـ delegate يُقيَّم per-request لا عند بناء الكلاس.

```csharp
// مثال: RegisterRequestValidator بعد التعديل
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IMessageLocalizer localizer)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
                .WithMessage(() => localizer["Field_Required"])
            .MaximumLength(50)
                .WithMessage(() => localizer["Field_MaxLength"]);

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage(() => localizer["Field_Required"])
            .EmailAddress()
                .WithMessage(() => localizer["Field_InvalidEmail"]);

        // ... باقي القواعد بنفس النمط
    }
}
```

**لماذا `() => localizer[key]` وليس `localizer[key]` مباشرة؟**  
FluentValidation يبني الـ Validators عند أول استخدام وقد يُعيد استخدامها. الـ delegate يضمن أن قراءة الترجمة تحدث **وقت التنفيذ الفعلي للطلب** وليس وقت بناء الكلاس، فتكون الـ Culture الحالية صحيحة دائماً.

---

### 6. ExceptionMessages Resource Files

**ملف:** `API/Resources/ExceptionMessages.resx` (EN)  
**ملف:** `API/Resources/ExceptionMessages.ar.resx` (AR)

| المفتاح | النص الإنجليزي | النص العربي |
|---|---|---|
| `Exception_NotFound_Generic` | `Resource not found.` | `المورد غير موجود.` |
| `Exception_Unauthorized` | `Access denied.` | `الوصول مرفوض.` |
| `Exception_InvalidInput` | `Invalid input provided.` | `المدخلات غير صالحة.` |
| `Exception_Concurrency` | `The resource was modified by another user. Please reload and try again.` | `تم تعديل المورد من قِبل مستخدم آخر. يرجى إعادة التحميل والمحاولة مجدداً.` |
| `Exception_DatabaseUpdate` | `A database update error occurred.` | `حدث خطأ أثناء تحديث قاعدة البيانات.` |
| `Exception_Database` | `A database error occurred.` | `حدث خطأ في قاعدة البيانات.` |
| `Exception_InvalidOperation` | `Operation cannot be completed due to the current state.` | `لا يمكن إتمام العملية في الحالة الحالية.` |
| `Exception_Unexpected` | `An unexpected error occurred.` | `حدث خطأ غير متوقع.` |

---

### 7. GlobalHandleExceptionMiddleware — التعديل المطلوب

الـ Middleware الحالي يحتاج فقط لحقن `IStringLocalizer<ExceptionMessages>` ليُقدِّم الترجمات الصحيحة. **لا يتغير منطق الـ switch expression ولا ترتيب الاستثناءات ولا شكل الـ Response.**

```csharp
// التغيير الوحيد: حقن localizer واستبدال النصوص الثابتة بـ MessageKeys
public class GlobalHandleExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalHandleExceptionMiddleware> _logger;

    // IStringLocalizer<ExceptionMessages> يُحقن بدل النصوص المكتوبة مباشرة
    public GlobalHandleExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalHandleExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // نفس المنطق الحالي بالضبط —
        // الفرق الوحيد: استخدام IStringLocalizer لقراءة الرسالة
        // IStringLocalizer يُحقن كـ Scoped من HttpContext.RequestServices
        // لأن Middleware constructor يُبنى Singleton
    }
}
```

**ملاحظة تقنية مهمة:** `GlobalHandleExceptionMiddleware` يُسجَّل كـ Singleton (بطبيعة الـ Middleware). لذلك لا يُحقن `IStringLocalizer` في الـ Constructor بل يُطلَب من `context.RequestServices` (Scoped DI) داخل دالة `Invoke`.

---

### 8. CommonMessages — ترجمة Result\<T\> Messages

**ملف:** `Application/Resources/CommonMessages.resx` (EN)  
**ملف:** `Application/Resources/CommonMessages.ar.resx` (AR)

الـ Handlers تُرجع **MessageKey** في حقل `Error` أو `Message` الخاص بـ `Result<T>`. الـ `BaseController` يُرجم هذا المفتاح قبل الإرسال للعميل.

| المفتاح | النص الإنجليزي | النص العربي |
|---|---|---|
| `Course_NotFound` | `Course with ID {0} not found.` | `الدورة بالمعرّف {0} غير موجودة.` |
| `Course_Created` | `Course created successfully.` | `تم إنشاء الدورة بنجاح.` |
| `Course_Deleted` | `Course deleted successfully.` | `تم حذف الدورة بنجاح.` |
| `Course_HasActiveEnrollments` | `Cannot delete this course because it has active enrollments.` | `لا يمكن حذف هذه الدورة لأنها تحتوي على تسجيلات نشطة.` |
| `Instructor_NotFound` | `Instructor with ID {0} not found.` | `المدرّب بالمعرّف {0} غير موجود.` |
| `Instructor_AlreadyExists` | `This user is already assigned as an instructor.` | `هذا المستخدم معيَّن بالفعل كمدرّب.` |
| `Student_AlreadyEnrolled` | `Student is already enrolled in this course.` | `الطالب مسجَّل بالفعل في هذه الدورة.` |
| `Auth_LoginSuccess` | `Login successful.` | `تم تسجيل الدخول بنجاح.` |
| `Auth_AccountCreated` | `Account created successfully. Please check your email to confirm your account before logging in.` | `تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني لتأكيد حسابك قبل تسجيل الدخول.` |

---

### 9. MessageKeys Constants — منع Typos

```csharp
// API/Localization/MessageKeys.cs
namespace API.Localization
{
    public static class MessageKeys
    {
        public static class Validation
        {
            public const string Required          = "Field_Required";
            public const string MaxLength         = "Field_MaxLength";
            public const string MinLength         = "Field_MinLength";
            public const string InvalidEmail      = "Field_InvalidEmail";
            public const string PasswordsMustMatch = "Field_PasswordsMustMatch";
            public const string InvalidPassword   = "Field_InvalidPassword";
            public const string InvalidUsername   = "Field_InvalidUsername";
            public const string InvalidNameFormat = "Field_InvalidNameFormat";
        }

        public static class Exception
        {
            public const string NotFound         = "Exception_NotFound_Generic";
            public const string Unauthorized     = "Exception_Unauthorized";
            public const string InvalidInput     = "Exception_InvalidInput";
            public const string Concurrency      = "Exception_Concurrency";
            public const string DatabaseUpdate   = "Exception_DatabaseUpdate";
            public const string Database         = "Exception_Database";
            public const string InvalidOperation = "Exception_InvalidOperation";
            public const string Unexpected       = "Exception_Unexpected";
        }

        public static class Course
        {
            public const string NotFound            = "Course_NotFound";
            public const string HasActiveEnrollments = "Course_HasActiveEnrollments";
        }

        // ... باقي الـ Entities
    }
}
```

---

## Resource Files — هيكل التنظيم عند نمو المشروع

```
Application/
└── Resources/
    ├── ValidationMessages.resx          ← EN (default, no suffix)
    ├── ValidationMessages.ar.resx       ← AR
    ├── CommonMessages.resx              ← EN
    └── CommonMessages.ar.resx           ← AR

API/
└── Resources/
    ├── ExceptionMessages.resx           ← EN
    └── ExceptionMessages.ar.resx        ← AR
```

**عند نمو المشروع:** إذا أصبحت `CommonMessages.resx` كبيرة جداً، تُقسَّم بنفس المبدأ:

```
Application/Resources/
    ├── Courses/
    │   ├── CourseMessages.resx
    │   └── CourseMessages.ar.resx
    ├── Enrollments/
    │   ├── EnrollmentMessages.resx
    │   └── EnrollmentMessages.ar.resx
    └── Auth/
        ├── AuthMessages.resx
        └── AuthMessages.ar.resx
```

كل ملف يحتاج `IStringLocalizer<T>` خاص به حيث `T` هي الـ Marker Class.

---

## Middleware Pipeline — موضع RequestLocalizationMiddleware

وفقاً لمتطلب **عدم تغيير ترتيب الـ Middleware الحالي**، يُدرج `UseRequestLocalization` في الموضع الصحيح:

```
قبل التعديل:                          بعد التعديل:
─────────────────────────────────     ──────────────────────────────────────
1. UseSecurityHeaders                 1. UseSecurityHeaders
2. GlobalHandleExceptionMiddleware    ← يُضاف هنا: UseRequestLocalization (*)
3. UseHttpsRedirection                2. GlobalHandleExceptionMiddleware
4. UseHsts                            3. UseHttpsRedirection
5. UseCors                            4. UseHsts
6. UseStaticFiles                     5. UseCors
7. UseAuthentication                  6. UseStaticFiles
8. TokenRevocationMiddleware          7. UseAuthentication
9. IdempotencyMiddleware              8. TokenRevocationMiddleware
10. UseRateLimiter                    9. IdempotencyMiddleware
11. UseAuthorization                  10. UseRateLimiter
12. UseSerilogRequestLogging          11. UseAuthorization
                                      12. UseSerilogRequestLogging
```

> (*) `UseRequestLocalization` يأتي قبل `GlobalHandleExceptionMiddleware` حتى تكون الـ Culture مضبوطة قبل أن يُرسل الـ Middleware أي رسالة خطأ مترجمة.

---

## Naming Convention للمفاتيح

```
{Domain}_{Entity}_{Concept}

أمثلة:
  Field_Required              ← Validation: حقل مطلوب
  Field_MaxLength             ← Validation: طول أقصى
  Course_NotFound             ← Entity: غير موجود
  Course_HasActiveEnrollments ← Entity: Business Rule
  Auth_LoginSuccess           ← Feature: نجاح
  Exception_Unexpected        ← Middleware: استثناء عام
```

**القاعدة:** استخدم `PascalCase` دائماً. افصل الـ Domain عن الـ Entity وعن الـ Concept بـ underscore. لا تستخدم مسافات أو أحرف خاصة.

---

## الأخطاء الشائعة — تجنّبها

| الخطأ | الأثر | البديل الصحيح |
|---|---|---|
| حقن `IStringLocalizer` في Constructor الـ Middleware (Singleton) | يُجمَّد على Culture أول طلب — باقي الطلبات لا تُترجم صحيحاً | اطلبه من `context.RequestServices` داخل `Invoke` |
| استخدام `localizer["key"]` في Constructor الـ Validator | يُقيَّم مرة واحدة عند بناء الكلاس — تبقى اللغة الأولى | استخدم `() => localizer["key"]` كـ delegate |
| تخزين نص مترجم في `Result<T>.Error` داخل الـ Handler | الـ Handler يعرف اللغة — يكسر الـ Clean Architecture | خزّن MessageKey فقط، ترجمه في الـ BaseController |
| وضع `ExceptionMessages.resx` في Application Layer | تبعية غير صحيحة — Application Layer لا تعرف شيئاً عن الـ Middleware | `ExceptionMessages.resx` ينتمي لـ API Layer |
| عدم تعطيل QueryString وCookie culture providers | قد يتجاوز العميل الـ Header بـ `?culture=ar` — سلوك غير متوقع | عطّل كل الـ Providers ما عدا `AcceptLanguageHeaderRequestCultureProvider` |
| كتابة `if(language == "ar")` في أي مكان | يكسر الـ Open/Closed Principle — كل لغة جديدة = تعديل كود | دع `IStringLocalizer` يحدد النص حسب `CurrentUICulture` |
| نسيان تضمين ملفات `.resx` في الـ Build | الملفات موجودة لكن لا تُقرأ وقت التشغيل | تأكد من `<EmbeddedResource>` في `.csproj` |

---

## ملخص التعديلات المطلوبة على الملفات الحالية

| الملف | نوع التعديل | ما يتغير |
|---|---|---|
| `Program.cs` | إضافة | استدعاء `AddLocalizationExtension()` + `UseRequestLocalization()` |
| `GlobalHandleExceptionMiddleware.cs` | تعديل | حقن `IStringLocalizer<ExceptionMessages>` من RequestServices، استبدال النصوص بـ MessageKeys |
| `BaseController.cs` | تعديل | حقن `IMessageLocalizer`، إضافة `ResolveMessage()` لترجمة Result messages |
| `RegisterRequestValidator.cs` و باقي الـ Validators | تعديل | حقن `IMessageLocalizer`، استبدال النصوص بـ `() => localizer[MessageKey]` |
| `DIExtention.cs` | لا تعديل | تسجيل الـ Validators يبقى كما هو (`AddValidatorsFromAssemblyContaining`) |

**الملفات الجديدة:**
- `API/Localization/SupportedCultures.cs`
- `API/Localization/MessageKeys.cs`
- `API/Extentions/LocalizationExtension.cs`
- `API/Resources/ExceptionMessages.resx`
- `API/Resources/ExceptionMessages.ar.resx`
- `Application/Interfaces/Localization/IMessageLocalizer.cs`
- `Application/Resources/ValidationMessages.resx`
- `Application/Resources/ValidationMessages.ar.resx`
- `Application/Resources/CommonMessages.resx`
- `Application/Resources/CommonMessages.ar.resx`
- Implementation class for `IMessageLocalizer` in `Infrastructure` or `API/Localization/`
