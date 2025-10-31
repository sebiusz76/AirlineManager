# 🚨 Custom Error Pages Implementation

Kompleksowy system customowych stron błędów dla aplikacji AirlineManager z obsługą różnych kodów HTTP.

## 🎯 Cel

Utworzenie profesjonalnych, user-friendly stron błędów zamiast domyślnych, generycznych stron ASP.NET Core, aby poprawić doświadczenie użytkownika (UX) i zwiększyć profesjonalizm aplikacji.

## 📁 Utworzone Pliki

### 1. **ErrorController.cs**
**Lokalizacja**: `Controllers/ErrorController.cs`

```csharp
public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
   // Handles specific HTTP status codes (404, 401, 403, etc.)
        var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        
        _logger.LogWarning("HTTP {StatusCode} error occurred. Path: {Path}",
      statusCode, statusCodeResult?.OriginalPath ?? "Unknown");
        
        ViewData["StatusCode"] = statusCode;
        ViewData["OriginalPath"] = statusCodeResult?.OriginalPath;
        ViewData["QueryString"] = statusCodeResult?.OriginalQueryString;
        
 return View("Error", statusCode);
  }

    [Route("Error")]
    public IActionResult Error()
    {
 // Handles unhandled exceptions (500)
        var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        
        if (exceptionDetails != null)
        {
            _logger.LogError(exceptionDetails.Error,
      "Unhandled exception occurred. Path: {Path}",
     exceptionDetails.Path);
      }
        
  ViewData["StatusCode"] = 500;
      return View(500);
    }
}
```

**Funkcje**:
- ✅ Obsługa konkretnych kodów HTTP
- ✅ Logowanie błędów do Serilog
- ✅ Przekazywanie informacji o błędzie do widoku
- ✅ Oddzielne route'y dla różnych typów błędów

### 2. **Error.cshtml**
**Lokalizacja**: `Views/Shared/Error.cshtml`

**Obsługiwane Kody HTTP**:
- ✅ **400** - Bad Request
- ✅ **401** - Unauthorized
- ✅ **403** - Forbidden
- ✅ **404** - Page Not Found
- ✅ **405** - Method Not Allowed
- ✅ **408** - Request Timeout
- ✅ **429** - Too Many Requests
- ✅ **500** - Internal Server Error
- ✅ **502** - Bad Gateway
- ✅ **503** - Service Unavailable
- ✅ **504** - Gateway Timeout

**Struktura Widoku**:
```razor
@{
    var statusCode = ViewData["StatusCode"] as int? ?? 500;
    
    string title = "Error";
    string message = "...";
    string icon = "bi-exclamation-triangle-fill";
    string suggestion = "...";
    
    // Switch statement customizes content based on status code
}
```

**Elementy UI**:
- 🎨 **Error Header** - Gradient background z ikoną i kodem
- 📝 **Error Message** - Czytelny opis błędu
- 💡 **Suggestion** - Sugestia jak rozwiązać problem
- 🔘 **Action Buttons** - Kontekstowe akcje (Login, Go Back, Try Again, Home)
- 📚 **Help Links** - Szybki dostęp do pomocy (Home, Support, Search)
- 🐛 **Debug Info** - Szczegóły techniczne (tylko w Development)
- ⏰ **Timestamp** - Czas wystąpienia błędu

### 3. **Error.css**
**Lokalizacja**: `wwwroot/css/views/Shared/Error.css`

**Style**:
```css
/* Error Card Animation */
.error-card {
    opacity: 0;
    transform: translateY(30px);
    transition: var(--transition-slow);
}

.error-card.animate-in {
    opacity: 1;
    transform: translateY(0);
}

/* Icon Pulse Animation */
@keyframes iconPulse {
    0%, 100% { transform: scale(1); opacity: 1; }
    50% { transform: scale(1.05); opacity: 0.9; }
}

/* Help Cards Hover */
.help-card:hover {
    transform: translateY(-4px);
    box-shadow: var(--shadow-lg);
  border-color: var(--color-primary-main);
}
```

**Funkcje CSS**:
- ✅ Fade-in animation dla error card
- ✅ Pulsing animation dla ikony
- ✅ Hover effects dla help cards
- ✅ Responsive design (mobile-first)
- ✅ Dark theme support
- ✅ Użycie CSS variables dla spójności

## 🔧 Konfiguracja

### Program.cs
```csharp
// PRZED
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// PO
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Add custom status code pages
app.UseStatusCodePagesWithReExecute("/Error/{0}");
```

**Zmiany**:
- ✅ `UseExceptionHandler` wskazuje na `/Error` zamiast `/Home/Error`
- ✅ `UseStatusCodePagesWithReExecute` - przekierowuje status codes do ErrorController
- ✅ Zachowano `UseDeveloperExceptionPage` dla Development

## 📊 Szczegóły Implementacji

### Obsługa Kodów HTTP

#### 400 - Bad Request
```
Icon: bi-exclamation-triangle-fill
Message: "The request could not be understood by the server."
Suggestion: "Please check your input and try again."
Actions: [Try Again] [Home Page]
```

#### 401 - Unauthorized
```
Icon: bi-shield-lock-fill
Message: "You need to be logged in to access this resource."
Suggestion: "Please log in to continue."
Actions: [Log In]  ← Specjalna akcja!
```

#### 403 - Forbidden
```
Icon: bi-shield-exclamation
Message: "You don't have permission to access this resource."
Suggestion: "If you believe this is an error, please contact an administrator."
Actions: [Try Again] [Home Page]
```

#### 404 - Page Not Found
```
Icon: bi-search
Message: "The page you're looking for doesn't exist."
Suggestion: "The page may have been moved or deleted..."
Actions: [Go Back] [Home Page]  ← Specjalne akcje!
```

#### 500 - Internal Server Error
```
Icon: bi-exclamation-triangle-fill
Message: "Something went wrong on our end."
Suggestion: "Our team has been notified. Please try again later."
Actions: [Try Again] [Home Page]
```

#### 503 - Service Unavailable
```
Icon: bi-tools
Message: "The service is temporarily unavailable."
Suggestion: "We may be performing maintenance. Please try again shortly."
Actions: [Try Again] [Home Page]
```

### Kontekstowe Akcje

Różne kody błędów pokazują różne akcje:

| Status Code | Actions |
|-------------|---------|
| **401** | [Log In] |
| **404** | [Go Back] [Home Page] |
| **Other** | [Try Again] [Home Page] |

## 🎨 Design Features

### Visual Elements

#### 1. **Error Header**
- Gradient background (`--gradient-primary`)
- White text z text-shadow
- Large error code (4rem font)
- Descriptive title
- Animated icon (pulse effect)

#### 2. **Error Message**
- Light alert box
- Centered text
- Large font size (fs-5)
- Clear, user-friendly language

#### 3. **Suggestion Box**
- Text-muted styling
- Lightbulb icon
- Helpful guidance
- Non-technical language

#### 4. **Action Buttons**
- Large buttons (btn-lg)
- Primary & secondary colors
- Bootstrap icons
- Hover animations
- Contextual actions

#### 5. **Help Cards**
- 3-column grid
- Icon + text
- Hover effects (lift + shadow)
- Quick access links:
  - **Home Page** (bi-house-door-fill, primary)
  - **Contact Support** (bi-envelope-fill, success)
  - **Search Site** (bi-search, info)

#### 6. **Debug Information**
- Only visible in Development
- Warning alert styling
- Shows:
  - Original Path
  - Query String
  - Status Code
  - Development mode warning

### Animations

#### Card Entry Animation
```css
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(30px);
    }
    to {
      opacity: 1;
        transform: translateY(0);
    }
}
```

#### Icon Pulse
```css
@keyframes iconPulse {
    0%, 100% {
        transform: scale(1);
      opacity: 1;
    }
    50% {
        transform: scale(1.05);
        opacity: 0.9;
    }
}
```

#### Hover Transforms
```css
.help-card:hover {
 transform: translateY(-4px);
}

.btn-lg:hover {
    transform: translateY(-2px);
}
```

## 🌐 User Experience Flow

### Scenariusz 1: 404 Not Found
```
1. User navigates to /non-existent-page
2. Server returns 404
3. ErrorController.HttpStatusCodeHandler(404) is called
4. Error.cshtml renders with:
   - Search icon
   - "Page Not Found" title
   - Helpful message
   - [Go Back] and [Home Page] buttons
5. User clicks [Go Back] → returns to previous page
   OR clicks [Home Page] → redirects to /
```

### Scenariusz 2: 401 Unauthorized
```
1. Unauthenticated user tries to access protected resource
2. Server returns 401
3. ErrorController.HttpStatusCodeHandler(401) is called
4. Error.cshtml renders with:
   - Shield lock icon
   - "Unauthorized" title
   - Login suggestion
   - [Log In] button (redirects to /Account/Login)
5. User clicks [Log In] → redirected to login page
6. After successful login → can access original resource
```

### Scenariusz 3: 500 Internal Error
```
1. Unhandled exception occurs in application
2. Exception handler catches it
3. ErrorController.Error() is called
4. Exception logged to Serilog
5. Error.cshtml renders with:
   - Exclamation triangle icon
   - "Internal Server Error" title
   - Reassuring message
   - [Try Again] and [Home Page] buttons
6. User clicks [Try Again] → page reloads
   OR clicks [Home Page] → redirects to /
```

## 🔍 Debug Information

W trybie Development, strona błędu pokazuje dodatkowe informacje:

```
Debug Information (Development Mode Only)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Original Path: /api/users/123
Query String: ?filter=active
Status Code: 404

This information is only visible in Development environment.
```

**Bezpieczeństwo**:
- ❌ **Nigdy** nie wyświetlane w Production
- ✅ Sprawdzane przez `IWebHostEnvironment.IsDevelopment()`
- ✅ Pomaga programistom w debugowaniu

## 📝 Logging

Wszystkie błędy są logowane do Serilog:

### Status Code Errors
```csharp
_logger.LogWarning("HTTP {StatusCode} error occurred. Path: {Path}",
  statusCode, statusCodeResult?.OriginalPath ?? "Unknown");
```

**Log Output**:
```
[Warning] HTTP 404 error occurred. Path: /non-existent-page
```

### Unhandled Exceptions
```csharp
_logger.LogError(exceptionDetails.Error,
    "Unhandled exception occurred. Path: {Path}",
    exceptionDetails.Path);
```

**Log Output**:
```
[Error] Unhandled exception occurred. Path: /api/users/create
System.NullReferenceException: Object reference not set to an instance of an object
   at AirlineManager.Controllers.UsersController.Create(...)
```

## 🎭 Theme Support

Strony błędów wspierają wszystkie motywy aplikacji:

### Light Themes
```css
.help-card {
    background-color: var(--bs-light);
    border: 1px solid var(--bs-border-color);
}

.alert-light {
background-color: rgba(var(--bs-primary-rgb), 0.05);
    border-color: rgba(var(--bs-primary-rgb), 0.15);
}
```

### Dark Themes
```css
[data-bs-theme="dark"] .help-card,
[data-bs-theme="dark-slate"] .help-card,
[data-bs-theme="dark-midnight"] .help-card {
    background-color: var(--bs-dark);
    border-color: var(--bs-border-color);
}

[data-bs-theme="dark"] .alert-light {
    background-color: rgba(var(--bs-primary-rgb), 0.15);
    border-color: rgba(var(--bs-primary-rgb), 0.25);
    color: var(--bs-body-color);
}
```

**Automatyczne dostosowanie**:
- ✅ Kolory tła
- ✅ Kolory tekstu
- ✅ Kolory ramek
- ✅ Opacity values
- ✅ Divider backgrounds

## 📱 Responsive Design

### Desktop (≥768px)
- Error icon: 5rem
- Error code: 4rem
- Error title: 1.75rem
- 3-column help cards grid
- Large buttons (btn-lg)

### Mobile (<768px)
```css
@media (max-width: 768px) {
    .error-icon { font-size: 3.5rem; }
    .error-code { font-size: 3rem; }
    .error-title { font-size: 1.5rem; }
    
    .btn-lg {
 font-size: 1rem;
        padding: 0.625rem 1.5rem;
  }
    
  .help-card {
        margin-bottom: 0.5rem;
    }
}
```

**Mobile Optimizations**:
- ✅ Mniejsze ikony i czcionki
- ✅ Stack layout dla przycisków
- ✅ Single column dla help cards
- ✅ Touch-friendly button sizes

## 🔐 Security Considerations

### Production Safety
- ❌ **NIE** ujawnia stack traces
- ❌ **NIE** pokazuje internal paths w Production
- ❌ **NIE** wyświetla debug info w Production
- ✅ Ogólne, user-friendly komunikaty
- ✅ Loguje szczegóły server-side

### Development Transparency
- ✅ Pokazuje debug info
- ✅ Wyświetla original path
- ✅ Pokazuje query string
- ✅ Stack traces w DeveloperExceptionPage

## ✅ Testing Checklist

- [x] 400 Bad Request renders correctly
- [x] 401 Unauthorized shows [Log In] button
- [x] 403 Forbidden renders correctly
- [x] 404 Not Found shows [Go Back] and [Home Page]
- [x] 405 Method Not Allowed renders correctly
- [x] 408 Request Timeout renders correctly
- [x] 429 Too Many Requests renders correctly
- [x] 500 Internal Server Error renders correctly
- [x] 502 Bad Gateway renders correctly
- [x] 503 Service Unavailable renders correctly
- [x] 504 Gateway Timeout renders correctly
- [x] Debug info only shown in Development
- [x] Logging works correctly
- [x] Animations work smoothly
- [x] Help cards are clickable
- [x] All buttons functional
- [x] Responsive on mobile
- [x] Dark theme compatibility
- [x] Light theme compatibility
- [x] Build successful

## 🚀 Benefits

### User Experience
- 😊 **Friendly messaging** - Non-technical language
- 🎯 **Clear guidance** - Helpful suggestions
- 🔘 **Actionable** - Contextual buttons
- 🎨 **Professional** - Modern, clean design
- 📱 **Responsive** - Works on all devices

### Developer Experience
- 🐛 **Better debugging** - Debug info in Development
- 📊 **Comprehensive logging** - Serilog integration
- 🔧 **Easy maintenance** - Centralized error handling
- 📝 **Code reuse** - Single view for all errors
- 🎭 **Theme aware** - Automatic styling

### Business Value
- ⭐ **Professional image** - Better brand perception
- 🔒 **Security** - No sensitive info leaked
- 📈 **Lower support costs** - Self-service help links
- 💪 **User retention** - Better error recovery
- 📊 **Better insights** - Comprehensive error logging

## 📚 Related Documentation

- `OPTIMIZATION_REPORT.md` - CSS optimization report
- `AUTH_BACKGROUND_REMOVAL.md` - Auth pages styling
- `DARK_MODE_VARIANTS.md` - Dark theme variants
- `MODERN_DESIGN_SYSTEM_GUIDE.md` - Design system guide

## 🎓 Usage Examples

### Triggering Custom Error Pages

#### 404 Not Found
```
Navigate to: /non-existent-page
Result: Custom 404 page with search icon
```

#### 401 Unauthorized
```csharp
// In controller
[Authorize]
public IActionResult SecureAction()
{
    // Unauthenticated users see custom 401 page
}
```

#### 403 Forbidden
```csharp
// In controller
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly()
{
    // Non-admin users see custom 403 page
}
```

#### 500 Internal Error
```csharp
// In controller
public IActionResult CauseError()
{
    throw new Exception("Test error");
    // Shows custom 500 page
}
```

#### 503 Service Unavailable
```csharp
// In middleware
if (isMaintenanceMode)
{
    context.Response.StatusCode = 503;
    // Shows custom 503 page
}
```

## 🔮 Future Enhancements

Możliwe przyszłe ulepszenia:

- [ ] Multi-language support (i18n)
- [ ] Custom error pages per area (Admin, User)
- [ ] Error reporting form on error pages
- [ ] Recent similar errors suggestion
- [ ] Automatic retry with exponential backoff
- [ ] Error analytics dashboard
- [ ] A/B testing for error messages
- [ ] Voice assistance for accessibility

## 🎉 Summary

Zaimplementowano kompleksowy system customowych stron błędów:

### Utworzono:
- ✅ `ErrorController.cs` - Centralna obsługa błędów
- ✅ `Views/Shared/Error.cshtml` - Uniwersalny widok błędów
- ✅ `wwwroot/css/views/Shared/Error.css` - Dedykowane style

### Zmodyfikowano:
- ✅ `Program.cs` - Konfiguracja middleware błędów

### Funkcje:
- ✅ **11 obsługiwanych kodów HTTP** (400, 401, 403, 404, 405, 408, 429, 500, 502, 503, 504)
- ✅ **Kontekstowe akcje** - różne przyciski dla różnych błędów
- ✅ **Professional design** - Gradient header, animations, icons
- ✅ **Help cards** - Quick access to support
- ✅ **Debug info** - Only in Development
- ✅ **Comprehensive logging** - Serilog integration
- ✅ **Theme support** - Light & Dark variants
- ✅ **Responsive** - Mobile-first design
- ✅ **Accessible** - Semantic HTML, ARIA labels

### Rezultat:
Profesjonalne, user-friendly strony błędów które poprawiają UX i zwiększają profesjonalizm aplikacji! 🚀✨

---

**Author**: GitHub Copilot  
**Date**: 2024  
**Version**: 1.0
