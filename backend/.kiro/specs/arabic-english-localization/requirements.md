# Requirements Document — Arabic/English Localization

## Introduction

هذه الوثيقة تصف متطلبات إضافة دعم اللغتين العربية (`ar`) والإنجليزية (`en`) بشكل كامل إلى مشروع WhiteCodeAcademy Backend API المبني على ASP.NET Core / .NET 10.

الهدف هو تحويل الترجمة إلى **Cross-Cutting Concern** بالكامل، بحيث لا يحتاج أي Handler أو Controller أو Service إلى معرفة اللغة الحالية أو كتابة أي منطق لغوي صريح. يُحدَّد الإعداد بالكامل في طبقة API (Middleware + Infrastructure) ويُستهلَك تلقائياً عبر `IStringLocalizer`.

**المعنيون بالترجمة:**
- رسائل FluentValidation (422 Unprocessable Entity)
- رسائل Domain Exceptions (NotFoundException, BusinessRuleException)
- رسائل خطأ الـ GlobalHandleExceptionMiddleware (400, 403, 404, 409, 500)
- رسائل نجاح/فشل Result\<T\> التي تصل إلى العميل
- رسائل ProblemDetails / Error Responses

**غير المعنيين بالترجمة:** Logs، Emails، أسماء Entities، Enums الداخلية.

---

## Glossary

- **Localization_System**: النظام المسؤول عن تحديد اللغة وتقديم الترجمات الصحيحة.
- **RequestLocalizationMiddleware**: الـ Middleware المدمج في ASP.NET Core الذي يقرأ `Accept-Language` Header ويعيّن الـ `CultureInfo` الحالية على الـ Thread.
- **IStringLocalizer**: الواجهة المدمجة في ASP.NET Core التي تُستخدَم لاسترداد النصوص المترجمة من ملفات `.resx`.
- **Resource_File**: ملف `.resx` يحتوي على أزواج (مفتاح → نص مترجم) لكل لغة.
- **MessageKey**: مفتاح نصي ثابت يُستخدَم في الكود بدلاً من الكتابة المباشرة للرسالة.
- **GlobalHandleExceptionMiddleware**: الـ Middleware الموجود حالياً الذي يعترض جميع الاستثناءات غير المعالجة ويُعيد استجابة JSON منسّقة.
- **FluentValidation_Pipeline**: آلية التحقق من المدخلات عبر `AbstractValidator` قبل وصول الطلب إلى الـ Handler.
- **Result_T**: النمط المستخدم حالياً في جميع الـ Handlers لإرجاع نتيجة العملية (نجاح أو فشل) مع رسالة.
- **ValidationLocalizer**: مكوّن في Application Layer مسؤول عن توفير رسائل FluentValidation المترجمة عبر `IStringLocalizer`.
- **ExceptionLocalizer**: مكوّن في API Layer مسؤول عن ترجمة رسائل الاستثناءات داخل `GlobalHandleExceptionMiddleware`.
- **Supported_Culture**: أي ثقافة مدعومة (`ar` أو `en`). تُعرَّف مركزياً في مكان واحد.
- **Default_Culture**: الثقافة الافتراضية عند غياب `Accept-Language` أو عند تحديد لغة غير مدعومة، وهي `en`.

---

## Requirements

### Requirement 1: RequestLocalizationMiddleware Configuration

**User Story:** As a developer, I want `RequestLocalizationMiddleware` configured centrally, so that the correct culture is set on every HTTP request automatically without any code in Controllers or Handlers.

#### Acceptance Criteria

1. THE Localization_System SHALL support exactly the cultures `ar` and `en` as the only valid Supported_Cultures.
2. THE Localization_System SHALL define all Supported_Cultures in a single central location (a static class or configuration section) so that adding a new language in the future requires changing only that one location.
3. WHEN a request arrives with `Accept-Language: ar`, THE RequestLocalizationMiddleware SHALL set `CultureInfo.CurrentCulture` and `CultureInfo.CurrentUICulture` to `ar` for the duration of that request.
4. WHEN a request arrives with `Accept-Language: en`, THE RequestLocalizationMiddleware SHALL set `CultureInfo.CurrentCulture` and `CultureInfo.CurrentUICulture` to `en` for the duration of that request.
5. IF a request arrives with an unsupported or missing `Accept-Language` value, THEN THE RequestLocalizationMiddleware SHALL fall back to the Default_Culture (`en`) without returning an error.
6. THE RequestLocalizationMiddleware SHALL be registered in `Program.cs` in the correct position in the pipeline — before `GlobalHandleExceptionMiddleware` renders exception responses — so that culture is set before any response is generated.
7. THE Localization_System SHALL use only the `Accept-Language` HTTP header as the culture provider; query string and cookie culture providers SHALL be disabled.

---

### Requirement 2: Resource Files and Key Structure

**User Story:** As a developer, I want all translated messages stored in `.resx` files with a clear key structure, so that adding a new language is just adding a new `.resx` file without touching any C# code.

#### Acceptance Criteria

1. THE Localization_System SHALL store all translatable messages in `.resx` Resource_Files and SHALL NOT embed any translated text directly inside C# source code.
2. THE Localization_System SHALL organize Resource_Files by concern into at minimum these categories: `ValidationMessages`, `ExceptionMessages`, and `CommonMessages`.
3. THE Localization_System SHALL provide a Resource_File for the default language (`en`) without a culture suffix (e.g., `ValidationMessages.resx`) and a culture-specific file for each additional language (e.g., `ValidationMessages.ar.resx`).
4. WHEN a MessageKey is requested for culture `ar` and the key exists in the Arabic Resource_File, THE IStringLocalizer SHALL return the Arabic text.
5. IF a MessageKey is requested for culture `ar` and the key does NOT exist in the Arabic Resource_File, THEN THE IStringLocalizer SHALL fall back to the English Resource_File for that key without throwing an exception.
6. THE Localization_System SHALL use consistent MessageKey naming across all Resource_Files (e.g., `Course_NotFound`, `Field_Required`, `Instructor_AlreadyExists`) so that the same key resolves correctly regardless of which culture is active.
7. WHERE a new language is added in the future, THE Localization_System SHALL require only the creation of new `.resx` files and registration of the new culture in the central Supported_Cultures list, with zero changes to Handlers, Validators, or Controllers.

---

### Requirement 3: FluentValidation Message Translation

**User Story:** As a developer, I want FluentValidation error messages returned in the client's language automatically, so that Validators contain zero language-specific logic.

#### Acceptance Criteria

1. THE ValidationLocalizer SHALL resolve FluentValidation messages from Resource_Files using `IStringLocalizer` at request time, not at application startup.
2. WHEN a validation rule fails and `CultureInfo.CurrentUICulture` is `ar`, THE FluentValidation_Pipeline SHALL return the Arabic error message for that rule.
3. WHEN a validation rule fails and `CultureInfo.CurrentUICulture` is `en`, THE FluentValidation_Pipeline SHALL return the English error message for that rule.
4. THE ValidationLocalizer SHALL use a `WithMessage()` delegate that reads from `IStringLocalizer` at invocation time so that culture is evaluated per-request and not once at class construction.
5. THE Localization_System SHALL translate all built-in FluentValidation rule messages (`NotEmpty`, `MaximumLength`, `MinimumLength`, `EmailAddress`, `Equal`, `Matches`) that are used in existing validators in this project.
6. THE FluentValidation_Pipeline SHALL continue to return HTTP `422 Unprocessable Entity` with the same JSON error structure regardless of the active culture; only the message text value changes.
7. THE ValidationLocalizer SHALL reside in the Application Layer and SHALL NOT reference any API-layer types.

---

### Requirement 4: Domain Exception Translation in GlobalHandleExceptionMiddleware

**User Story:** As a developer, I want all exception messages translated automatically inside the existing Middleware, so that neither Handlers nor Domain exceptions need any localization knowledge.

#### Acceptance Criteria

1. THE ExceptionLocalizer SHALL be injected into `GlobalHandleExceptionMiddleware` and SHALL resolve translated messages from Resource_Files using `IStringLocalizer` based on `CultureInfo.CurrentUICulture` at the moment the exception is handled.
2. WHEN a `NotFoundException` is caught and `CultureInfo.CurrentUICulture` is `ar`, THE GlobalHandleExceptionMiddleware SHALL return the Arabic error message in the JSON response body.
3. WHEN a `BusinessRuleException` is caught and `CultureInfo.CurrentUICulture` is `ar`, THE GlobalHandleExceptionMiddleware SHALL return the Arabic error message in the JSON response body.
4. WHEN a `DbUpdateConcurrencyException`, `DbUpdateException`, `SqlException`, `InvalidOperationException`, or `ArgumentException` is caught, THE GlobalHandleExceptionMiddleware SHALL return the translated generic message from the ExceptionMessages Resource_File instead of the raw exception message.
5. THE GlobalHandleExceptionMiddleware SHALL use MessageKey-based lookup from ExceptionMessages Resource_Files and SHALL NOT use `if(language == "ar")` or any culture-conditional branching in its source code.
6. THE ExceptionLocalizer SHALL preserve the existing exception-to-HTTP-status-code mapping (NotFoundException → 404, BusinessRuleException → 409, etc.) without modification.
7. WHEN an exception carries dynamic data (e.g., entity name or ID), THE ExceptionLocalizer SHALL support parameterized message formatting (e.g., `string.Format` with indexed placeholders) to produce a grammatically correct localized string.

---

### Requirement 5: Result\<T\> Success and Failure Message Translation

**User Story:** As a developer, I want Result\<T\> messages that reach the client translated into the client's language automatically, without any changes to the Handlers.

#### Acceptance Criteria

1. THE Localization_System SHALL provide a mechanism to translate messages carried by `Result<T>` using MessageKeys from `CommonMessages` Resource_Files, resolved at the response point (BaseController or a response filter), NOT inside individual Handlers.
2. WHEN `Result<T>` carries a MessageKey and `CultureInfo.CurrentUICulture` is `ar`, THE Localization_System SHALL resolve and return the Arabic message to the client.
3. WHEN `Result<T>` carries a MessageKey and `CultureInfo.CurrentUICulture` is `en`, THE Localization_System SHALL resolve and return the English message to the client.
4. THE Localization_System SHALL resolve Result messages at the API Layer boundary (e.g., BaseController) so that Application Layer Handlers remain free of any localization dependency.
5. IF a MessageKey does not exist in the Resource_File for the current culture, THEN THE Localization_System SHALL return the key itself as a fallback value so that no message is silently lost.

---

### Requirement 6: Architectural Quality and Extensibility

**User Story:** As a developer, I want the localization system to follow Clean Architecture principles and be extensible with minimal effort so that future language additions are trivial.

#### Acceptance Criteria

1. THE Localization_System SHALL register all localization services (`IStringLocalizer`, `RequestLocalizationMiddleware`, `ValidationLocalizer`, `ExceptionLocalizer`) in a dedicated extension method (e.g., `AddLocalizationExtension`) called from `Program.cs`, keeping registration isolated from business logic.
2. THE Localization_System SHALL place Resource_Files that serve the Application Layer (`ValidationMessages`, `CommonMessages`) inside the Application Layer project, and Resource_Files that serve the API Layer (`ExceptionMessages` for middleware) inside the API Layer project, respecting Clean Architecture layer boundaries.
3. THE Localization_System SHALL NOT introduce any localization-aware code into Domain Layer entities, Domain Exception constructors, or MediatR Handlers.
4. THE Localization_System SHALL NOT use `if(language == "ar")` or any string-based culture comparisons anywhere in production code.
5. WHEN a developer adds a new validator rule, THE Localization_System SHALL require only adding a new MessageKey entry to the `.resx` files and referencing that key in `WithMessage()`; no other file shall need to change.
6. WHEN a developer adds a new language (e.g., French `fr`), THE Localization_System SHALL require only: (a) creating new `.resx` files for that language, and (b) adding `fr` to the central Supported_Cultures list — with zero changes to Controllers, Handlers, Validators, or Middleware logic.
7. THE Localization_System SHALL maintain full compatibility with the existing middleware pipeline order defined in `Program.cs`; `RequestLocalizationMiddleware` SHALL be inserted without reordering any existing middleware.

---

### Requirement 7: API Response Consistency and Testability

**User Story:** As a developer, I want API responses to remain structurally consistent regardless of language, and I want the localization system to be easily testable in isolation.

#### Acceptance Criteria

1. THE Localization_System SHALL preserve the existing JSON response structure for all error types (`{ "statusCode": 404, "message": "..." }` for exceptions, `{ "errors": {...} }` for validation) regardless of the active culture; only the message text value changes.
2. WHEN running integration tests, THE Localization_System SHALL allow passing an `Accept-Language` header to assert that the correct language is returned without requiring any test-specific changes to the production pipeline.
3. THE Localization_System SHALL be testable at the unit level by constructing `ValidationLocalizer` or `ExceptionLocalizer` with a mock `IStringLocalizer`, without requiring a running HTTP server or database.
4. THE Localization_System SHALL provide translations for all existing error messages currently present in `GlobalHandleExceptionMiddleware`, `RegisterRequestValidator`, `CreateCourseValidator`, and all other existing validators in this project.
5. IF the `Accept-Language` header contains a BCP-47 tag with a region suffix (e.g., `ar-SA`, `en-US`), THEN THE RequestLocalizationMiddleware SHALL match it to the correct Supported_Culture (`ar` or `en` respectively) without falling back to the default unnecessarily.
