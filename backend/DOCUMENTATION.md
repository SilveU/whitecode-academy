# WhiteCodeAcademy — Backend API Documentation

> **Base URL:** `https://unmultipliable-kelsey-unloyal.ngrok-free.dev`  
> **Local URL:** `https://localhost:7045`  
> **Framework:** .NET 10 / ASP.NET Core Web API  
> **Architecture:** Clean Architecture + CQRS (MediatR) + Repository Pattern

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Middleware Pipeline](#middleware-pipeline)
3. [Authentication & Security](#authentication--security)
4. [Rate Limiting](#rate-limiting)
5. [Caching (Redis)](#caching-redis)
6. [Idempotency](#idempotency)
7. [File Handling](#file-handling)
8. [Health Checks](#health-checks)
9. [Localization](#localization)
10. [API Endpoints](#api-endpoints)
    - [Authentication](#authentication)
    - [Courses](#courses)
    - [Departments](#departments)
    - [Enrollments](#enrollments)
    - [Instructors](#instructors)
    - [Sections](#sections)
    - [Students](#students)
    - [Profile](#profile)
11. [Common Patterns](#common-patterns)
12. [Roles & Authorization](#roles--authorization)
13. [Error Handling](#error-handling)
14. [Configuration Reference](#configuration-reference)

---

## Architecture Overview

```
API Layer          →  Controllers (thin, dispatch via MediatR)
Application Layer  →  Handlers (CQRS Commands/Queries), DTOs, Validators, Interfaces
Infrastructure     →  EF Core, Redis, Identity, File Storage, Email, ClamAV
Domain             →  Entities, Enums, Exceptions
```

**Key libraries:** MediatR · AutoMapper · FluentValidation · StackExchange.Redis · FFMpegCore · ClamAV · Serilog


---

## Middleware Pipeline

Requests pass through the pipeline in this exact order:

| # | Middleware | Purpose |
|---|---|---|
| 1 | `UseSecurityHeaders` | Adds HTTP security headers (CSP, X-Frame-Options, etc.) |
| 2 | `GlobalHandleExceptionMiddleware` | Catches all unhandled exceptions, returns structured JSON error |
| 3 | `UseHttpsRedirection` | Redirects HTTP → HTTPS |
| 4 | `UseHsts` | Enforces HSTS in production |
| 5 | `UseCors` | Applies the configured CORS policy |
| 6 | `UseStaticFiles` | Serves files from `wwwroot/` (uploaded videos, images, PDFs) |
| 7 | `UseAuthentication` | Validates JWT Bearer token, populates `context.User` |
| 8 | `TokenRevocationMiddleware` | Checks Redis session key — rejects revoked tokens instantly |
| 9 | `IdempotencyMiddleware` | Replays cached responses for duplicate requests |
| 10 | `UseRateLimiter` | Enforces rate limit policies per endpoint |
| 11 | `UseAuthorization` | Enforces `[Authorize]` role/policy checks |
| 12 | `UseSerilogRequestLogging` | Logs every HTTP request/response |

---

## Authentication & Security

### JWT Bearer

All protected endpoints require a JWT in the `Authorization` header:

```
Authorization: Bearer <access_token>
```

| Property | Value |
|---|---|
| Algorithm | HMAC-SHA256 |
| Issuer | `Jwt:Issuer` (from config) |
| Audience | `Jwt:Audience` (from config) |
| Expiry | `Jwt:ExpiryMinutes` (from config) |

### Refresh Token

- Stored as **HttpOnly cookie** named `RefreshToken`
- Hashed with SHA-256 before DB storage (raw token never persisted)
- Rotation on every `/refresh` call (old token revoked, new token issued)
- Expiry: 15 days

### Token Revocation (Redis Session Check)

On every authenticated request, `TokenRevocationMiddleware` checks:

```
Redis key: auth:token:active:{userId}
```

- **Key exists** → request proceeds
- **Key missing** → `401 Unauthorized` immediately (even if JWT signature is valid)
- **Redis down** → fail-open (request proceeds, logged as warning)

This means logout takes effect **instantly** — no need to wait for JWT expiry.

**Paths exempt from session check:**
- `POST /api/authentication/login`
- `POST /api/authentication/register`
- `POST /api/authentication/refresh`
- `GET  /api/authentication/confirm-email`
- `POST /api/authentication/resend-email-confirmation`
- `POST /api/authentication/reset-password`
- `GET  /api/authentication/confirm-reset-password`
- `POST /api/authentication/resend-reset-password`


---

## Rate Limiting

All policies use **Fixed Window** algorithm. Rejected requests → `429 Too Many Requests`.

| Policy | Limit | Window | Used On |
|---|---|---|---|
| `AuthPolicy` | 10 requests | 3 minutes | All `/api/authentication/*` endpoints |
| `OtpPolicy` | 5 requests | 3 minutes | Reserved for OTP endpoints |
| `HeavyPolicy` | 60 requests | 1 minute | All write operations (POST/PUT/DELETE) |
| `ReadPolicy` | 300 requests | 1 minute | All read operations (GET) |

Controllers apply a base policy at class level; individual endpoints can override with a stricter policy.

---

## Caching (Redis)

### Strategy: Cache-Aside

**Read (Queries):**
1. Check Redis with the computed key
2. Cache hit → return immediately (no DB call)
3. Cache miss → fetch from DB → store in Redis → return

**Write (Commands):**
- **Create** → invalidate list cache prefix + warm single-item cache
- **Update** → remove stale single-item key + invalidate list prefix + warm updated single-item cache
- **Delete** → remove single-item key + invalidate list prefix

### Redis Key Reference

| Entity | Single Item Key | List Prefix | Paginated Search Key |
|---|---|---|---|
| Course | `course:{id}` | `courses` | `courses:page{n}:size:{s}:sort:{sort}:search:{word}` |
| Department | `department:{id}` | `departments` | `departments:page{n}:size:{s}:sort:{sort}:search:{word}` |
| Instructor | `instructor:{id}` | `instructors` | `instructors:page{n}:size:{s}:sort:{sort}:search:{word}` |
| Student | `student:{id}` | `students` | `students:page{n}:size:{s}:sort:{sort}:search:{word}` |
| Section | `section:{id}` | `sections:course:{courseId}` | — |
| Enrollment (by course) | — | `enrollments:course:{courseId}` | — |
| Enrollment (by student) | — | `enrollments:student:{studentId}` | — |

### Auth & Email Keys

| Key | TTL | Purpose |
|---|---|---|
| `auth:token:active:{userId}` | = JWT expiry minutes | Active session marker; deleted on logout |
| `auth:refresh:active:{userId}` | `AuthTokenActiveCacheMinutes` | Stores hashed refresh token for active session |
| `email:verification:cooldown:{userId}` | `EmailVerificationResendCooldownMinutes` (10 min) | Prevents resend spam |

### Idempotency Keys

| Key | TTL | Purpose |
|---|---|---|
| `idempotency:response:{key}` | 15 minutes | Cached HTTP response for replay |
| `idempotency:lock:{key}` | 60 seconds | Distributed lock during in-flight processing |

### TTL Configuration (appsettings.json → `Redis` section)

| Key | Default |
|---|---|
| `CourseExpirationMinutes` | 60 min |
| `CoursesExpirationMinutes` | 15 min |
| `DepartmentExpirationMinutes` | 120 min |
| `DepartmentsExpirationMinutes` | 30 min |
| `InstructorExpirationMinutes` | 60 min |
| `InstructorsExpirationMinutes` | 15 min |
| `StudentExpirationMinutes` | 60 min |
| `StudentsExpirationMinutes` | 15 min |
| `SectionExpirationMinutes` | 60 min |
| `SectionsExpirationMinutes` | 15 min |
| `EnrollmentExpirationMinutes` | 30 min |
| `EnrollmentsExpirationMinutes` | 10 min |
| `EmailVerificationResendCooldownMinutes` | 10 min |
| `AuthTokenActiveCacheMinutes` | 60 min |
| `IdempotencyExpirationMinutes` | 30 min |


---

## Idempotency

Prevents duplicate side-effects when clients retry requests (network failures, timeouts).

### How to Use

Add the header to any mutating request:

```
Idempotency-Key: <unique-uuid-per-operation>
```

### Processing Flow

```
Request arrives with Idempotency-Key header
        │
        ▼
Check Redis: idempotency:response:{key}
        │
   ┌────┴────┐
   │  HIT    │  →  Replay stored response immediately (status + body)
   └─────────┘
        │
   ┌────┴────┐
   │  MISS   │  →  Try acquire lock: idempotency:lock:{key} (60s TTL)
   └─────────┘
        │
   ┌────┴────────────┐
   │  Lock acquired  │  →  Execute request normally
   │                 │  →  Cache response (200/201/400/422) for 15 min
   └─────────────────┘
        │
   ┌────┴────────────────┐
   │  Lock NOT acquired  │  →  Poll Redis 10× (200ms each)
   │  (another in-flight)│  →  If response appears → replay
   └─────────────────────┘  →  Else → 409 "Request is still processing"
```

**Cached status codes:** `200`, `201`, `400`, `422`  
**Redis unavailable fallback:** Falls back to database lookup (persisted idempotency records)

---

## File Handling

### Upload Endpoints

| Endpoint | File Fields | Storage Path |
|---|---|---|
| `POST /api/department` | `ImageFile` (optional) | `wwwroot/Departments/{departmentId}/Images/` |
| `PUT  /api/department/{id}` | `ImageFile` (optional) | Same, old file deleted first |
| `POST /api/section` | `VideoFile` (required), `PdfFile` (optional) | `wwwroot/Sections/{sectionId}/Videos/` and `/Pdfs/` |
| `PUT  /api/section/{id}` | `VideoFile` (optional), `PdfFile` (optional) | Same, old files deleted first |

All upload endpoints use `Content-Type: multipart/form-data`.

### Security Pipeline (every uploaded file)

1. **Format validation** — `ValidatePdfAsync` / `ValidateVideoAsync` checks extension + MIME type
2. **Antivirus scan** — ClamAV (`localhost:3310`) scans the file bytes
3. **Storage** — saved to local filesystem under `wwwroot/`

### Video Processing (FFMpegCore)

On section create/update with a video file:
- FFProbe analyses the file to extract duration
- `Section.StartAt`, `Section.EndAt`, `Section.DayOfWeek` set automatically
- `Course.TotalDurationInSeconds` updated accordingly
- FFmpeg binaries are local (bundled in `/API/FFmpeg/`)

---

## Health Checks

| Endpoint | What it checks | Healthy when |
|---|---|---|
| `GET /health/live` | App process only | App is running |
| `GET /health/ready` | SQL Server + Redis | Both dependencies respond within 5s |

**Response format:**
```json
{
  "status": "Healthy",
  "totalDurationInMilliseconds": 12,
  "checks": [
    {
      "name": "sqlserver",
      "status": "Healthy",
      "durationInMilliseconds": 8
    },
    {
      "name": "redis",
      "status": "Healthy",
      "durationInMilliseconds": 4
    }
  ]
}
```

SQL Server degraded → `Unhealthy`. Redis degraded → `Degraded` (not fully unhealthy).


---

## Localization

The API fully supports **Arabic (`ar`) and English (`en`)** with automatic language selection per request. Translation is a cross-cutting concern — no Controller, Handler, or Validator contains language-specific logic.

### How to Select a Language

Send the `Accept-Language` HTTP header with every request:

```
Accept-Language: ar
Accept-Language: en
```

| Header Value | Language | Notes |
|---|---|---|
| `ar` | Arabic | Full RTL support for all messages |
| `ar-SA`, `ar-EG`, etc. | Arabic | Region suffix accepted, resolves to `ar` |
| `en` | English | Default language |
| `en-US`, `en-GB`, etc. | English | Region suffix accepted, resolves to `en` |
| Missing or unsupported | English | Falls back to `en` silently |

> **Only `Accept-Language` header is supported.** Query string (`?culture=ar`) and cookie-based culture selection are disabled.

---

### What Gets Translated

| Category | Scope | Example |
|---|---|---|
| **Validation errors** | All `422 Unprocessable Entity` responses | `'Email' ليس عنوان بريد إلكتروني صالحاً.` |
| **Business rule failures** | All `409 Conflict` responses | `لا يمكن حذف هذه الدورة لأنها تحتوي على تسجيلات نشطة.` |
| **Not found errors** | All `404 Not Found` responses | `الدورة بالمعرّف {id} غير موجودة.` |
| **Forbidden / Unauthorized** | `403` and `401` responses | `الوصول مرفوض.` |
| **Success messages** | Auth and operation responses | `تم تسجيل الدخول بنجاح.` |
| **Exception messages** | `500`, `409` database errors | `حدث خطأ غير متوقع.` |

### What Does NOT Get Translated

- Log messages (always English for observability)
- Email content
- Internal entity names and enums
- Field names in JSON keys (only the message *values* change)

---

### Response Structure — Unchanged Regardless of Language

The JSON structure of all responses remains identical. Only the text *value* of `message` fields changes:

**English (`Accept-Language: en`):**
```json
{ "statusCode": 404, "message": "Course with ID abc not found." }
```

**Arabic (`Accept-Language: ar`):**
```json
{ "statusCode": 404, "message": "الدورة بالمعرّف abc غير موجودة." }
```

**Validation errors (same structure, translated values):**
```json
{
  "errors": {
    "email": ["'Email' ليس عنوان بريد إلكتروني صالحاً."]
  }
}
```

---

### Adding a New Language (Extensibility)

To add a new language (e.g., French `fr`):

1. Add `public const string French = "fr";` to `API/Localization/SupportedCultures.cs`
2. Add `French` to the `All` array in the same file
3. Create three `.resx` files:
   - `Application/Resources/ValidationMessages.fr.resx`
   - `Application/Resources/CommonMessages.fr.resx`
   - `API/Resources/ExceptionMessages.fr.resx`
4. No changes to Controllers, Handlers, Validators, or Middleware — the system picks up the new language automatically.

---

### Architecture — How Translation Flows

```
Request  →  Accept-Language: ar
            │
            ▼
   RequestLocalizationMiddleware
   (sets Thread.CurrentUICulture = ar)
            │
            ├──▶ FluentValidation Pipeline
            │    (IMessageLocalizer reads ar .resx per rule)
            │
            ├──▶ GlobalHandleExceptionMiddleware
            │    (IStringLocalizer<ExceptionMessages> resolves ar message)
            │
            └──▶ BaseController.Failure()
                 (IStringLocalizer<CommonMessages> resolves ar message)
                            │
                            ▼
            Response: { "message": "الدورة غير موجودة." }
```

---

## API Endpoints

### Common Query Parameters

All paginated list endpoints accept these query parameters:

| Parameter | Type | Default | Constraints | Description |
|---|---|---|---|---|
| `pageNumber` | int | 1 | ≥ 1 | Page number |
| `pageSize` | int | 10 | 1–50 | Items per page |
| `wordForSearch` | string | `"all"` | max 100 chars | Search keyword |
| `sortBy` | string | `"name"` | max 50 chars | Sort field (`name`, `price_asc`, `price_desc`) |

---

### Authentication

**Base route:** `/api/authentication`  
**Rate limit:** `AuthPolicy` — 10 requests / 3 minutes (entire controller)

---

#### `POST /api/authentication/register`

Register a new user account. Sends a confirmation email automatically.

**Request body (JSON):**
```json
{
  "firstName": "Ahmed",
  "lastName":  "Ali",
  "userName":  "ahmed.ali",
  "email":     "ahmed@example.com",
  "password":  "P@ssword1",
  "confirmPassword": "P@ssword1"
}
```

**Validation:**
- `firstName` / `lastName`: required, max 50 chars
- `userName`: 3–30 chars, alphanumeric + `@ . _ -` only
- `email`: valid email format
- `password`: min 8 chars, must include letter + digit + special character
- `confirmPassword`: must match `password`

**Response `200 OK`:**
```json
{
  "isAuthenticated": false,
  "id":       "user-guid",
  "userName": "ahmed.ali",
  "email":    "ahmed@example.com",
  "message":  "Account created successfully. Please check your email to confirm your account before logging in."
}
```

---

#### `POST /api/authentication/login`

Authenticate and receive an access token + refresh token cookie.

**Request body (JSON):**
```json
{
  "identity": "ahmed@example.com",
  "password": "P@ssword1"
}
```

`identity` can be email or username.

**Response `200 OK`:**
```json
{
  "isAuthenticated": true,
  "id":          "user-guid",
  "userName":    "ahmed.ali",
  "email":       "ahmed@example.com",
  "accessToken": "<jwt>",
  "expiration":  "2026-07-19T15:00:00Z",
  "message":     "Login successful."
}
```

The `RefreshToken` is set as an **HttpOnly cookie** — not in the response body.

**Possible failure messages:**
- `"Invalid email, username, phone number, or password."` → 200 with `isAuthenticated: false`
- `"Please confirm your email before logging in."` → 200 with `isAuthenticated: false`


---

#### `GET /api/authentication/confirm-email`

Confirm a user's email address using the token from the confirmation email.

**Query parameters:**

| Param | Type | Required |
|---|---|---|
| `userId` | string | ✅ |
| `token` | string (URL-encoded) | ✅ |

**Response `200 OK`:**
```json
{
  "isAuthenticated": true,
  "message": "Email confirmed successfully."
}
```

---

#### `POST /api/authentication/resend-email-confirmation`

Re-send the confirmation email. Blocked for 10 minutes after each send (Redis cooldown).

**Query parameters:**

| Param | Type | Required |
|---|---|---|
| `email` | string | ✅ |

**Response `200 OK`:**
```json
{
  "message": "Email confirmation link has been sent successfully."
}
```

**Blocked response (cooldown active):**
```json
{
  "isAuthenticated": false,
  "message": "A confirmation email was already sent. Please wait 10 minutes before requesting a new one."
}
```

---

#### `POST /api/authentication/refresh`

Rotate the refresh token and get a new access token.  
Reads the `RefreshToken` HttpOnly cookie automatically.

**No request body required.**

**Response `200 OK`:**
```json
{
  "isAuthenticated": true,
  "accessToken": "<new-jwt>",
  "expiration":  "2026-07-19T16:00:00Z",
  "message":     "Token refreshed successfully."
}
```

The new `RefreshToken` is set as a new HttpOnly cookie, old one is revoked.

---

#### `POST /api/authentication/logout`

Revoke the current refresh token and clear the Redis session key.

**No request body required** (reads cookie automatically).

**Response `200 OK`:**
```json
{ "message": "Logged out successfully." }
```

After logout, the `auth:token:active:{userId}` Redis key is deleted → any subsequent request with the old JWT is rejected immediately by `TokenRevocationMiddleware`.

---

#### `POST /api/authentication/logout-all`

Revoke **all** active refresh tokens for the current user across all devices.

**No request body required** (userId read from JWT).

**Response `200 OK`:**
```json
{ "message": "Logged out from all devices successfully." }
```

---

#### `POST /api/authentication/reset-password`

Send a password reset email to the user.

**No auth required**  
**Content-Type:** `application/json`

**Request body:**
```json
{
  "email": "ahmed@example.com"
}
```

**Response `200 OK`:**
```json
{
  "isAuthenticated": false,
  "message": "Email confirmation link has been sent successfully."
}
```

If the email does not exist, the same response is returned to prevent user enumeration.

---

#### `GET /api/authentication/confirm-reset-password`

Confirm the password reset using the token from the email and set a new password.

**No auth required**  
**Query parameters:**

| Param | Type | Required |
|---|---|---|
| `userId` | string | ✅ |
| `token` | string (URL-encoded) | ✅ |

**Request body (JSON):**
```json
{
  "newPassword":     "NewP@ssword1",
  "confirmPassword": "NewP@ssword1"
}
```

**Validation:**
- `newPassword`: must include letter + digit + special character
- `confirmPassword`: must match `newPassword`

**Response `200 OK`:**
```json
{
  "isAuthenticated": true,
  "message": "Reset Password Successfully."
}
```

**Response `400 Bad Request`:** Invalid or expired token.

---

#### `POST /api/authentication/resend-reset-password`

Re-send the password reset email if the previous one expired.

**No auth required**  
**Query parameters:**

| Param | Type | Required |
|---|---|---|
| `email` | string | ✅ |

**Response `200 OK`:**
```json
{
  "isAuthenticated": false,
  "message": "Email confirmation link has been sent successfully."
}
```

**Cooldown active response:**
```json
{
  "isAuthenticated": false,
  "message": "A confirmation email was already sent. Please wait 10 minutes before requesting a new one."
}
```


---

### Courses

**Base route:** `/api/course`  
**Base rate limit:** `ReadPolicy` (300 req/min) on GETs, `HeavyPolicy` (60 req/min) on writes

---

#### `GET /api/course`

Get a paginated, searchable list of courses.

**Auth:** `Bearer token` (any authenticated role)  
**Query params:** [Common Query Parameters](#common-query-parameters)

**Response `200 OK`:**
```json
[
  {
    "id":               "guid",
    "name":             "Introduction to C#",
    "description":      "...",
    "totalHours":       12.5,
    "totalSections":    5,
    "instructorId":     "guid",
    "departmentId":     "guid",
    "createdAt":        "2026-01-01T00:00:00Z"
  }
]
```

---

#### `GET /api/course/{id}`

Get a single course by ID.

**Auth:** `Bearer token` (any authenticated role)

**Response `200 OK`:** Single `CourseResponse` object.  
**Response `404 Not Found`:** `{ "error": "Course with ID {id} not found." }`

---

#### `POST /api/course`

Create a new course.

**Auth:** `Admin` or `Instructor`  
**Content-Type:** `application/json`

**Request body:**
```json
{
  "name":          "Advanced .NET",
  "description":   "Deep dive into .NET",
  "departmentId":  "guid",
  "instructorId":  "guid"
}
```

> **Note:** `instructorId` is required when called by **Admin**. When called by **Instructor**, it is ignored — the caller's instructor profile is used automatically.

**Response `201 Created`:** `CourseResponse`

---

#### `PUT /api/course/{id}`

Update a course. All fields are optional — only provided fields are updated.

**Auth:** `Admin` or `Instructor` (Instructor must own the course)  
**Content-Type:** `application/json`

**Request body:**
```json
{
  "name":         "Updated Name",
  "description":  "Updated description",
  "instructorId": "guid",
  "departmentId": "guid"
}
```

**Response `200 OK`:** Updated `CourseResponse`  
**Response `403 Forbidden`:** If Instructor doesn't own the course

---

#### `DELETE /api/course/{id}`

Delete a course. Fails if the course has active enrollments.

**Auth:** `Admin` or `Instructor` (Instructor must own the course)

**Response `204 No Content`**  
**Response `409 Conflict`:** `"Cannot delete this course because it has active enrollments."`


---

### Departments

**Base route:** `/api/department`  
**Base rate limit:** `ReadPolicy` on GETs, `HeavyPolicy` on writes  
**All write operations:** `Admin` only

---

#### `GET /api/department`

**Auth:** Any authenticated role  
**Query params:** [Common Query Parameters](#common-query-parameters)

**Response `200 OK`:**
```json
[
  {
    "id":          "guid",
    "name":        "Computer Science",
    "description": "...",
    "imageUrl":    "/Departments/guid/Images/file.jpg",
    "createdAt":   "2026-01-01T00:00:00Z"
  }
]
```

---

#### `GET /api/department/{id}`

**Auth:** Any authenticated role  
**Response `200 OK`:** Single `DepartmentResponse`

---

#### `POST /api/department`

**Auth:** `Admin`  
**Content-Type:** `multipart/form-data`

| Field | Type | Required |
|---|---|---|
| `name` | string | ✅ |
| `description` | string | ✅ |
| `imageFile` | file (image) | ❌ |

**Response `201 Created`:** `DepartmentResponse`

---

#### `PUT /api/department/{id}`

**Auth:** `Admin`  
**Content-Type:** `multipart/form-data`

All fields optional. If `imageFile` is provided, the old image is deleted and replaced.

**Response `200 OK`:** Updated `DepartmentResponse`

---

#### `DELETE /api/department/{id}`

**Auth:** `Admin`

Fails if the department has active courses or instructors assigned.

**Response `204 No Content`**  
**Response `409 Conflict`:** `"Cannot delete this department because it has active courses or instructors assigned to it."`

---

### Enrollments

**Base route:** `/api/enrollment`  
**Base rate limit:** `ReadPolicy` on GETs, `HeavyPolicy` on writes

---

#### `GET /api/enrollment/by-course/{courseId}`

Get all enrollments for a specific course.

**Auth:** `Admin` or `Instructor`

**Response `200 OK`:**
```json
[
  {
    "id":         "guid",
    "studentId":  "guid",
    "courseId":   "guid",
    "courseName": "Introduction to C#",
    "createdAt":  "2026-01-01T00:00:00Z"
  }
]
```

---

#### `GET /api/enrollment/by-student/{studentId}`

Get all enrollments for a specific student.

**Auth:** Any authenticated role

**Response `200 OK`:** Array of `EnrollmentResponse`

---

#### `POST /api/enrollment/{courseId}`

Enroll the currently authenticated user (as a student) in a course.

**Auth:** `User` role only  
**No request body** — student identity resolved from JWT

**Response `201 Created`:** `EnrollmentResponse`  
**Response `409 Conflict`:** `"Student is already enrolled in this course."`

---

#### `DELETE /api/enrollment`

Remove a student from a course.

**Auth:** `Admin`  
**Query params:**

| Param | Type | Required |
|---|---|---|
| `studentId` | Guid | ✅ |
| `courseId` | Guid | ✅ |

**Response `204 No Content`**


---

### Instructors

**Base route:** `/api/instructor`  
**Base rate limit:** `ReadPolicy` on GETs, `HeavyPolicy` on writes  
**All endpoints:** `Admin` only

---

#### `GET /api/instructor`

**Query params:** [Common Query Parameters](#common-query-parameters)

**Response `200 OK`:**
```json
[
  {
    "id":             "guid",
    "userId":         "identity-user-id",
    "firstName":      "Sara",
    "lastName":       "Mohamed",
    "email":          "sara@example.com",
    "departmentId":   "guid",
    "departmentName": "Computer Science",
    "createdAt":      "2026-01-01T00:00:00Z"
  }
]
```

---

#### `GET /api/instructor/{id}`

**Response `200 OK`:** Single `InstructorResponse`

---

#### `POST /api/instructor`

Assign the `Instructor` role to an existing registered user.

**Content-Type:** `application/json`

```json
{
  "userId":       "identity-user-id",
  "departmentId": "guid"
}
```

> `userId` is the user's Identity ID (string GUID from ASP.NET Identity), not the Instructor entity ID.

**Response `201 Created`:** `InstructorResponse`  
**Response `409 Conflict`:** `"This user is already assigned as an instructor."`

---

#### `PUT /api/instructor/{id}`

Update an instructor's department assignment.

```json
{
  "departmentId": "guid"
}
```

**Response `200 OK`:** Updated `InstructorResponse`

---

#### `DELETE /api/instructor/{id}`

Remove instructor profile and revoke the `Instructor` role from the user.

Fails if the instructor has active courses.

**Response `204 No Content`**  
**Response `409 Conflict`:** `"Cannot remove this instructor because they have active courses assigned."`

---

### Sections

**Base route:** `/api/section`  
**Base rate limit:** `ReadPolicy` on GETs, `HeavyPolicy` on writes

---

#### `GET /api/section/by-course/{courseId}`

Get all sections for a course.

**Auth:** Any authenticated role

**Response `200 OK`:**
```json
[
  {
    "id":          "guid",
    "name":        "Introduction",
    "description": "...",
    "videoUrl":    "/Sections/guid/Videos/video.mp4",
    "pdfUrl":      "/Sections/guid/Pdfs/notes.pdf",
    "startAt":     "09:00:00",
    "endAt":       "10:30:00",
    "dayOfWeek":   "Monday",
    "courseId":    "guid",
    "createdAt":   "2026-01-01T00:00:00Z"
  }
]
```

---

#### `POST /api/section`

Create a new section with a video file. Duration and schedule are extracted automatically from the video.

**Auth:** `Admin` or `Instructor` (must own the parent course)  
**Content-Type:** `multipart/form-data`

| Field | Type | Required |
|---|---|---|
| `name` | string | ✅ |
| `description` | string | ✅ |
| `courseId` | Guid | ✅ |
| `videoFile` | file (video) | ✅ |
| `pdfFile` | file (PDF) | ❌ |

**Processing:**
- Video is scanned by ClamAV
- FFProbe extracts duration → sets `startAt`, `endAt`, `dayOfWeek`
- `Course.TotalDurationInSeconds` and `Course.TotalSections` updated
- Course and sections list cache invalidated

**Response `201 Created`:** `SectionResponse`

---

#### `PUT /api/section/{id}`

Update a section. If a new video is uploaded, old video is deleted, duration recalculated.

**Auth:** `Admin` or `Instructor` (must own the parent course)  
**Content-Type:** `multipart/form-data`

All fields optional.

**Response `200 OK`:** Updated `SectionResponse`

---

#### `DELETE /api/section/{id}`

Delete a section. Video and PDF files are deleted from disk. Course duration updated.

**Auth:** `Admin` or `Instructor` (must own the parent course)

**Response `204 No Content`**


---

### Students

**Base route:** `/api/student`

---

#### `POST /api/student`

Register the currently authenticated user as a student.

**Auth:** `User` role only  
**Rate limit:** `HeavyPolicy`  
**No request body** — user identity resolved from JWT

**Response `201 Created`:**
```json
{
  "id":        "guid",
  "userId":    "identity-user-id",
  "firstName": "Ahmed",
  "lastName":  "Ali",
  "email":     "ahmed@example.com",
  "createdAt": "2026-01-01T00:00:00Z"
}
```

**Response `409 Conflict`:** `"This user is already registered as a student."`

---

#### `DELETE /api/student/{id}`

Delete a student profile and all their enrollments.

**Auth:** `Admin`  
**Rate limit:** `HeavyPolicy`

**Response `204 No Content`**

---

### Profile

**Base route:** `/api/profile`  
**All endpoints:** Any authenticated user (profile resolved from JWT)

---

#### `GET /api/profile`

Get the current authenticated user's profile.

**Auth:** `Bearer token` (any authenticated role)  
**No request body** — identity resolved from JWT

**Response `200 OK`:**
```json
{
  "firstName":   "Ahmed",
  "lastName":    "Ali",
  "userName":    "ahmed.ali",
  "email":       "ahmed@example.com",
  "phoneNumber": null,
  "imageUrl":    "/profiles/guid/Images/avatar.jpg"
}
```

**Response `404 Not Found`:** If user profile doesn't exist.

---

#### `PATCH /api/profile`

Update the current user's profile. All fields are optional — only provided fields are updated.

**Auth:** `Bearer token` (any authenticated role)  
**Content-Type:** `multipart/form-data`

| Field | Type | Required |
|---|---|---|
| `firstName` | string | ❌ |
| `lastName` | string | ❌ |
| `userName` | string | ❌ |
| `imageUrl` | file (image) | ❌ |

**Validation (when provided):**
- `firstName` / `lastName`: required if `userName` is provided, max 50 chars
- `userName`: 3–30 chars, alphanumeric + `@ . _ -` only
- `imageUrl`: must be `.jpg`, `.jpeg`, or `.png`, max 5 MB

**Response `200 OK`:** Updated `ProfileResponse`

```json
{
  "firstName":   "Ahmed",
  "lastName":    "Ali",
  "userName":    "ahmed.ali2",
  "email":       "ahmed@example.com",
  "phoneNumber": null,
  "imageUrl":    "/profiles/guid/Images/new-avatar.jpg"
}
```

---

## Common Patterns

### Result\<T\> — Handler Return Type

All MediatR handlers return `Result<T>`. Controllers map it to HTTP responses:

| Factory Method | HTTP Status | When to Use |
|---|---|---|
| `Result.Success(value)` | 200 | Successful read/update |
| `Result.Success(value, 201)` | 201 | Successful create |
| `Result.NotFound(message)` | 404 | Entity doesn't exist |
| `Result.Failure(message, 400)` | 400 | Validation / business rule failure |
| `Result.Failure(message, 409)` | 409 | Conflict (duplicate, active dependency) |
| `Result.Forbidden(message)` | 403 | Ownership check failed |

### BaseController — Shared Helper

All core controllers inherit `BaseController`:

```csharp
protected string GetCurrentUserId()
// Returns ClaimTypes.NameIdentifier from JWT
// Throws UnauthorizedAccessException if claim is missing
```

Used to pass `CurrentUserId` and `IsInstructor` to commands so handlers can enforce ownership rules without touching the controller.

---

## Roles & Authorization

| Role | Who has it | What they can do |
|---|---|---|
| `Admin` | Seeded at startup | Everything — manage users, departments, courses, instructors, students, enrollments |
| `Instructor` | Assigned by Admin via `POST /api/instructor` | Create/update/delete own courses and sections |
| `User` | Every registered user | Register as student, enroll in courses, read all content |

**Ownership rules (Instructor):**
- Can only update/delete courses they created
- Can only add/update/delete sections in courses they own
- Enforced in the handler, not the controller, using `InstructorId` comparison

**Role assignment flow:**
```
Register → Role: User
Admin calls POST /api/instructor → Role: User + Instructor added
User calls POST /api/student    → Student profile created (role stays User)
```

---

## Error Handling

All errors return a consistent JSON structure:

```json
{
  "statusCode": 404,
  "message":    "Course with ID {id} not found."
}
```

### HTTP Status Code Reference

| Code | Meaning | When |
|---|---|---|
| `200` | OK | Successful read / update |
| `201` | Created | Successful create |
| `204` | No Content | Successful delete |
| `400` | Bad Request | Validation failure, missing required fields |
| `401` | Unauthorized | Missing/invalid/revoked JWT |
| `403` | Forbidden | Valid JWT but insufficient role or ownership |
| `404` | Not Found | Entity doesn't exist |
| `409` | Conflict | Duplicate resource, active dependency blocking delete, concurrent modification |
| `422` | Unprocessable Entity | FluentValidation failure |
| `429` | Too Many Requests | Rate limit exceeded |
| `500` | Internal Server Error | Unhandled exception |

### FluentValidation Errors

Validation failures from FluentValidation return `422` with this structure:

```json
{
  "errors": {
    "email":    ["'Email' is not a valid email address."],
    "password": ["Password must be at least 8 characters."]
  }
}
```


---

## Configuration Reference

### appsettings.json — Full Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WhiteCodeAcademy;Trusted_Connection=True;...",
    "Redis": "localhost:6379"
  },
  "Redis": {
    "CourseExpirationMinutes":                  "60",
    "CoursesExpirationMinutes":                 "15",
    "DepartmentExpirationMinutes":              "120",
    "DepartmentsExpirationMinutes":             "30",
    "InstructorExpirationMinutes":              "60",
    "InstructorsExpirationMinutes":             "15",
    "StudentExpirationMinutes":                 "60",
    "StudentsExpirationMinutes":                "15",
    "SectionExpirationMinutes":                 "60",
    "SectionsExpirationMinutes":                "15",
    "EnrollmentExpirationMinutes":              "30",
    "EnrollmentsExpirationMinutes":             "10",
    "EmailVerificationExpirationMinutes":       "10",
    "EmailVerificationResendCooldownMinutes":   "10",
    "ResetPasswordResendCooldownMinutes":       "10",
    "AuthTokenActiveCacheMinutes":              "60",
    "IdempotencyExpirationMinutes":             "30"
  },
  "ClamAV": {
    "Host": "localhost",
    "Port": "3310"
  },
  "Jwt": {
    "Key":           "<secret>",
    "Issuer":        "<issuer>",
    "Audience":      "<audience>",
    "ExpireMinutes": "<minutes>"
  },
  "EmailSettings": {
    "Password":         "<smtp-password>",
    "FromEmail":        "<from@example.com>",
    "ReplyTo":          "<reply@example.com>",
    "FromName":         "White Code Academy",
    "FrontendBaseUrl":  "https://yourfrontend.com"
  },
  "CORS": {
    "CorsPolicy":    "MyCorsPolicy",
    "AllowedOrigins": ["https://yourfrontend.com", "", "", ""]
  },
  "FFmpeg": {
    "BinaryFolder": "FFmpeg"
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Warning" },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "Logs/app-.log", "rollingInterval": "Day", "retainedFileCountLimit": 7 } }
    ]
  },
  "SeedSettings": {
    "Admin":      { "Email": "", "UserName": "", "FirstName": "System", "LastName": "Admin" },
    "Instructor": { "Email": "", "UserName": "", "FirstName": "Instructor", "LastName": "Instructor" },
    "Student":    { "Email": "", "UserName": "", "FirstName": "Student", "LastName": "Student" }
  }
}
```

### Required Secrets (never commit)

| Key | Description |
|---|---|
| `Jwt:Key` | HMAC-SHA256 signing key (min 32 chars recommended) |
| `EmailSettings:Password` | SMTP password |
| `EmailSettings:FromEmail` | Sender email address |
| `SeedSettings:*.Email` | Seed user credentials |
| `SeedSettings:*.UserName` | Seed user credentials |

Use `dotnet user-secrets` for local development:
```bash
dotnet user-secrets set "Jwt:Key" "your-super-secret-key"
```

---

## Quick Start

### 1. Prerequisites
- .NET 10 SDK
- SQL Server (local or Docker)
- Redis (local or Docker)
- ClamAV (local or Docker) on port 3310
- FFmpeg binaries in `API/FFmpeg/`

### 2. Run with Docker (Redis + SQL)
```bash
docker run -d -p 6379:6379 redis
docker run -d -p 1433:1433 -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword" mcr.microsoft.com/mssql/server
```

### 3. Apply Migrations
```bash
dotnet ef database update --startup-project backend/API --project backend/Infrastructure
```

### 4. Run
```bash
dotnet run --project backend/API --launch-profile https
```

Swagger UI available at: `https://localhost:7045/swagger`

---

*Documentation generated from source — WhiteCodeAcademy Backend v1.0*
