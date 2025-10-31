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

### 4. **Error.js** ⭐ [NOWY]
**Lokalizacja**: `wwwroot/js/views/Shared/Error.js`

**Funkcje JavaScript**:
```javascript
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        // Display current time
   const currentTimeElement = document.getElementById('currentTime');
     if (currentTimeElement) {
       currentTimeElement.textContent = new Date().toLocaleString();
        }

        // Add animation on load
        const errorCard = document.querySelector('.error-card');
        if (errorCard) {
  errorCard.classList.add('animate-in');
    }
    });
})();

// Search functionality
function searchSite() {
    const searchTerm = prompt('What are you looking for?');
    if (searchTerm) {
     window.location.href = '/?search=' + encodeURIComponent(searchTerm);
    }
}

// Make searchSite available globally for onclick handler
window.searchSite = searchSite;
```

**Funkcjonalności**:
- ✅ **Display Time** - Pokazuje czas wystąpienia błędu
- ✅ **Card Animation** - Dodaje klasę `animate-in` do error card
- ✅ **Search Site** - Prompt z wyszukiwaniem i redirect
- ✅ **IIFE Pattern** - Izolacja scope
- ✅ **DOMContentLoaded** - Bezpieczne manipulacje DOM
- ✅ **Null Safety** - Sprawdzanie istnienia elementów
- ✅ **Global Export** - `window.searchSite` dla onclick

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

### Error.cshtml
```razor
<!-- PRZED: ~20 linii inline JavaScript -->
@section Scripts {
    <script>
        // Display current time
    document.getElementById('currentTime').textContent = new Date().toLocaleString();

        // Search functionality
        function searchSite() { ... }

        // Add animation on load
  document.addEventListener('DOMContentLoaded', function() { ... });
    </script>
}

<!-- PO: 1 linia z referencją do pliku -->
@section Scripts {
    <script src="~/js/views/Shared/Error.js" asp-append-version="true"></script>
}
```

**Korzyści**:
- ✅ **Cleaner HTML** - Brak inline JavaScript
- ✅ **Better Caching** - Browser cache dla JS
- ✅ **Separation of Concerns** - HTML oddzielony od JS
- ✅ **Easier Maintenance** - Kod w dedykowanym pliku
- ✅ **Better IntelliSense** - Pełne wsparcie IDE

## 📊 Porównanie: Przed vs Po

### Struktura Plików:

#### Przed:
```
Views/Shared/
└── Error.cshtml (350 linii z inline JS)

wwwroot/css/views/Shared/
└── Error.css
```

#### Po:
```
Views/Shared/
└── Error.cshtml (330 linii bez inline JS)

wwwroot/css/views/Shared/
└── Error.css

wwwroot/js/views/Shared/
└── Error.js  ← NOWY PLIK
```

### Kod w Widoku:

| Aspekt | Przed | Po | Zmiana |
|--------|-------|-----|---------|
| **Linie JS w widoku** | ~20 | 1 (script tag) | -95% |
| **Separacja** | Mixed | Separated | ✅ |
| **Cache** | Brak | Browser cache | ✅ |
| **Maintainability** | Trudniejsza | Łatwiejsza | ✅ |
| **IntelliSense** | Ograniczone | Pełne | ✅ |

## 📊 Szczegóły Implementacji

### JavaScript Functions

#### 1. **Display Current Time**
```javascript
const currentTimeElement = document.getElementById('currentTime');
if (currentTimeElement) {
    currentTimeElement.textContent = new Date().toLocaleString();
}
```

**Funkcja**:
- Znajduje element z id `currentTime`
- Ustawia jego zawartość na aktualny czas (locale format)
- Null safety - sprawdza czy element istnieje

**Output Example**:
```
Error Code: 404 • 12/28/2024, 3:45:30 PM
```

#### 2. **Card Animation**
```javascript
const errorCard = document.querySelector('.error-card');
if (errorCard) {
    errorCard.classList.add('animate-in');
}
```

**Funkcja**:
- Znajduje error card
- Dodaje klasę `animate-in`
- Uruchamia CSS transition (fade-in + slide-up)

**CSS Trigger**:
```css
.error-card.animate-in {
    opacity: 1;
    transform: translateY(0);
}
```

#### 3. **Search Site**
```javascript
function searchSite() {
    const searchTerm = prompt('What are you looking for?');
    if (searchTerm) {
        window.location.href = '/?search=' + encodeURIComponent(searchTerm);
    }
}
```

**Funkcja**:
- Wyświetla prompt z pytaniem
- Jeśli użytkownik wprowadzi tekst → redirect do home z query string
- `encodeURIComponent` - bezpieczne enkodowanie URL

**Usage**:
```html
<a href="#" onclick="searchSite(); return false;">
    Search Site
</a>
```

### Best Practices Applied

#### 1. **IIFE Pattern**
```javascript
(function () {
    'use strict';
    // Kod izolowany w closure
})();
```

**Korzyści**:
- ✅ Unika konfliktów z global scope
- ✅ Strict mode enforcement
- ✅ Better performance

#### 2. **DOMContentLoaded**
```javascript
document.addEventListener('DOMContentLoaded', function () {
    // Kod wykonuje się po załadowaniu DOM
});
```

**Korzyści**:
- ✅ Bezpieczne manipulacje DOM
- ✅ Nie wymaga umieszczenia script na końcu body
- ✅ Standard best practice

#### 3. **Null Safety**
```javascript
if (currentTimeElement) {
  // Kod wykonuje się tylko jeśli element istnieje
}
```

**Korzyści**:
- ✅ Zapobiega błędom
- ✅ Graceful degradation
- ✅ Better error handling

#### 4. **Global Export**
```javascript
window.searchSite = searchSite;
```

**Powód**:
- ✅ Funkcja dostępna dla onclick handlers
- ✅ Kompatybilność z inline event handlers
- ✅ Backwards compatibility

## 📁 File Organization

### Naming Convention:
```
wwwroot/js/views/{Area}/{Controller}/{Action}.js
```

### Error.js Location:
```
wwwroot/js/views/Shared/Error.js
```

**Reasoning**:
- ✅ `Shared` - Widok jest w `Views/Shared`
- ✅ `Error` - Nazwa widoku
- ✅ Spójność z innymi plikami (Profile.js, Login.js, etc.)

### Other View Scripts:
```
wwwroot/js/views/
├── Shared/
│   ├── _Layout.js
│   └── Error.js  ← NOWY
├── Admin/
│   ├── AppLogs/Index.js
│   └── Configuration/Edit.js
├── Account/
│   ├── Profile.js
│   └── ActiveSessions.js
└── Theme/
    └── Variants.js
```

## 🎯 Benefits

### 1. **Separation of Concerns** 🎭
- ✅ HTML/Razor oddzielone od JavaScript
- ✅ Czytelniejszy kod widoku
- ✅ Łatwiejsze utrzymanie

### 2. **Caching & Performance** ⚡
- ✅ Browser może cache'ować plik JS
- ✅ `asp-append-version` dla cache busting
- ✅ Mniejszy rozmiar HTML (inline JS removed)
- ✅ Możliwość minifikacji w production

### 3. **Code Organization** 📁
- ✅ Spójna struktura z innymi widokami
- ✅ Łatwe odnalezienie kodu
- ✅ Following project conventions

### 4. **Development Experience** 💻
- ✅ Syntax highlighting dla JS
- ✅ IntelliSense w dedykowanym pliku
- ✅ Easier debugging (source maps)
- ✅ Better code navigation
- ✅ ESLint/JSHint support

### 5. **Reusability** ♻️
- ✅ Możliwość współdzielenia funkcji
- ✅ Importowanie w innych miejscach
- ✅ Easier unit testing

## 🔍 Technical Details

### Script Loading:
```razor
@section Scripts {
    <script src="~/js/views/Shared/Error.js" 
  asp-append-version="true"></script>
}
```

**Tag Helper**:
- `asp-append-version="true"` dodaje hash pliku do URL
- Example: `/js/views/Shared/Error.js?v=abc123def456`
- Cache busting przy zmianach w pliku

### Function Execution Flow:

1. **Page Load**
   ```
   HTML loaded → Browser parses → Script tag encountered
   → Error.js downloaded → Script executed
   → IIFE runs → DOMContentLoaded listener registered
   ```

2. **DOM Ready**
   ```
   DOM fully loaded → DOMContentLoaded event fired
   → Display time function runs
   → Card animation function runs
   → Error card fades in
   ```

3. **User Interaction**
   ```
   User clicks "Search Site" → onclick fires
   → searchSite() called → Prompt displayed
   → User enters search term → Redirect to /?search=term
   ```

## 📊 Statistics

| Metric | Wartość |
|--------|---------|
| **Nowe pliki** | 1 (Error.js) |
| **Zmienione pliki** | 1 (Error.cshtml) |
| **Linie JS usunięte z widoku** | ~20 |
| **Linie JS dodane w pliku** | 32 |
| **Zmniejszenie "noise" w Razor** | ~6% |
| **Poprawa maintainability** | ✅ Wysoka |
| **Poprawa caching** | ✅ Tak |

## ✅ Testing Checklist

- [x] Error.js utworzony w poprawnej lokalizacji
- [x] IIFE pattern zastosowany
- [x] DOMContentLoaded handler obecny
- [x] Null safety checks dodane
- [x] searchSite exported to window
- [x] Widok zaktualizowany do zewnętrznego pliku
- [x] `asp-append-version` dodane
- [x] Build successful
- [x] Time display działa
- [x] Card animation działa
- [x] Search functionality działa

## 🔮 Future Improvements

Możliwe przyszłe ulepszenia:

- [ ] TypeScript migration dla type safety
- [ ] Unit tests dla JavaScript functions
- [ ] Bundle optimization w production
- [ ] JSDoc comments dla better IntelliSense
- [ ] ESLint dla code quality
- [ ] Advanced search with auto-complete

## 📚 Related Files

### JavaScript Files in Project:
```
wwwroot/js/
├── site.js (Global)
├── views/
│   ├── Shared/
│   │   ├── _Layout.js
│   │   └── Error.js  ← Nowy
│   ├── Admin/
│   │   ├── AppLogs/Index.js
│   │   └── Configuration/Edit.js
│   ├── Account/
│   │   ├── Profile.js
│   │   └── ActiveSessions.js
│   └── Theme/
│       └── Variants.js
```

## 🎉 Summary

Kod JavaScript został pomyślnie wyekstrahowany z widoku `Error.cshtml` do dedykowanego pliku `wwwroot/js/views/Shared/Error.js`.

### Zmiany:
- ✅ Utworzono `wwwroot/js/views/Shared/Error.js` (32 linie)
- ✅ Zaktualizowano `Views/Shared/Error.cshtml` (usunięto ~20 linii inline JS)
- ✅ Zachowano całą funkcjonalność
- ✅ Poprawiono organizację kodu

### Funkcje JavaScript:
- ✅ **Display Time** - Pokazuje czas błędu
- ✅ **Card Animation** - Fade-in effect
- ✅ **Search Site** - Wyszukiwarka z promptem

### Rezultat:
- 🎯 **Cleaner views** - mniej kodu w Razor
- ⚡ **Better caching** - browser cache dla JS
- 📁 **Organized structure** - spójna z projektem
- 💻 **Better DX** - łatwiejsze utrzymanie
- ♻️ **Reusable** - potencjał do reuse

Projekt teraz ma spójną strukturę JavaScript we wszystkich widokach, w tym na stronach błędów! 🚀✨

---

**Author**: GitHub Copilot  
**Date**: 2024  
**Version**: 1.1 (Updated with JavaScript extraction)
