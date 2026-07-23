# Implementation Plan

## Overview

خطة التنفيذ الكاملة لإضافة دعم اللغتين العربية والإنجليزية إلى WhiteCodeAcademy Backend API. يتضمن هذا التنفيذ 11 مهمة مرتبة حسب التبعيات، تبدأ بالبنية التحتية وتنتهي بالتحقق من الاندماج الكامل.

## Tasks

- [x] 1. Create core localization infrastructure (SupportedCultures, MessageKeys, IMessageLocalizer interface, and LocalizationExtension)
- [x] 2. Create ValidationMessages resource files (EN + AR) for FluentValidation messages
- [x] 3. Create CommonMessages resource files (EN + AR) for Result\<T\> success/failure messages
- [x] 4. Create ExceptionMessages resource files (EN + AR) for GlobalHandleExceptionMiddleware messages
- [x] 5. Update GlobalHandleExceptionMiddleware to resolve messages from IStringLocalizer
- [x] 6. Update Authentication validators (RegisterRequestValidator, LoginRequestValidator, EmailResetPasswordValidator, NewPasswordValidator, ResendEmailConfirmationValidator) to use IMessageLocalizer
- [x] 7. Update Feature validators (Course, Department, Section, Instructor, Enrollment) to use IMessageLocalizer
- [x] 8. Update UpdateProfileValidator to use IMessageLocalizer
- [x] 9. Update all MediatR Handlers to return MessageKeys instead of hardcoded strings in Result\<T\>
- [x] 10. Update BaseController to inject IMessageLocalizer and resolve Result\<T\> messages before sending to client
- [x] 11. Verify full build and end-to-end localization behavior across all layers

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": [1] },
    { "wave": 2, "tasks": [2, 3, 4] },
    { "wave": 3, "tasks": [5, 6, 7, 8, 9] },
    { "wave": 4, "tasks": [10] },
    { "wave": 5, "tasks": [11] }
  ]
}
```

## Notes

### قرارات التصميم الحرجة

**IStringLocalizer في Middleware (Singleton issue):**
`GlobalHandleExceptionMiddleware` يُسجَّل كـ Singleton في pipeline الـ ASP.NET Core. إذا حُقن `IStringLocalizer` في الـ Constructor فسيُجمَّد على ثقافة الطلب الأول. الحل: يُطلَب من `context.RequestServices` داخل دالة `Invoke()` في كل طلب.

**Delegate في Validators (`() => localizer[key]`):**
FluentValidation قد يبني الـ Validators مرة واحدة (Singleton-style). استخدام `localizer[key]` مباشرة يُقيَّم عند البناء وليس عند التنفيذ. الـ delegate `() => localizer[key]` يضمن القراءة عند وقت التنفيذ الفعلي لكل طلب.

**MessageKeys في Handlers (ليس نص مترجم):**
الـ Handlers تُرجع `MessageKeys.Course.NotFound` كـ string، لا نص مترجم. الترجمة تحدث فقط في `BaseController.ResolveMessage()`. هذا يفصل منطق الأعمال تماماً عن الترجمة.

**ExceptionMessages في API Layer:**
`GlobalHandleExceptionMiddleware` ينتمي لـ API Layer. ملفات `.resx` الخاصة برسائل الاستثناءات توضع في `API/Resources/` وليس في `Application/Resources/` لاحترام حدود الطبقات.

### إضافة لغة جديدة مستقبلاً (مثلاً French)

1. أضف `public const string French = "fr";` في `SupportedCultures.cs`
2. أضف `French` إلى مصفوفة `All`
3. أنشئ `ValidationMessages.fr.resx`, `CommonMessages.fr.resx`, `ExceptionMessages.fr.resx`
4. لا شيء آخر — الـ Validators والـ Handlers والـ Middleware تعمل تلقائياً.
