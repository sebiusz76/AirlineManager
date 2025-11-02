# Relacja: ApplicationUser → UserLoginHistory

## 📊 Typ relacji
**One-to-Many (Jeden do wielu)**

## 🎯 Opis
Jeden użytkownik (`ApplicationUser`) może mieć wiele wpisów historii logowania (`UserLoginHistory`). Ta relacja pozwala na:
- Śledzenie historii logowań użytkownika
- Wykrywanie podejrzanych aktywności
- Audyt bezpieczeństwa
- Analiza wzorców logowania

## 📋 Struktura relacji

```
ApplicationUser (1) ─────< UserLoginHistory (∞)
        │     │
        └─────── UserId ─────────┘
```

## 🔑 Klucze

- **Primary Key**: `UserLoginHistory.Id`
- **Foreign Key**: `UserLoginHistory.UserId` → `ApplicationUser.Id`
- **Delete Behavior**: `Cascade` (usunięcie użytkownika usuwa jego historię logowań)

## 💾 Właściwości nawigacyjne

### W ApplicationUser.cs
```csharp
// Navigation Property - Kolekcja historii logowań
public virtual ICollection<UserLoginHistory> LoginHistories { get; set; } = new List<UserLoginHistory>();
```

### W UserLoginHistory.cs
```csharp
// Navigation Property - Odniesienie do użytkownika
[ForeignKey(nameof(UserId))]
public virtual ApplicationUser? User { get; set; }
```

## ⚙️ Konfiguracja w ApplicationDbContext

```csharp
builder.Entity<UserLoginHistory>(entity =>
{
    // ... inne konfiguracje ...
    
    // Konfiguracja relacji One-to-Many
    entity.HasOne(e => e.User)
        .WithMany(u => u.LoginHistories)
        .HasForeignKey(e => e.UserId)
      .OnDelete(DeleteBehavior.Cascade);
});
```

## 📖 Przykłady użycia

### 1. Pobranie historii logowań użytkownika (Eager Loading)

```csharp
// W kontrolerze lub serwisie
var user = await _context.Users
    .Include(u => u.LoginHistories)
    .FirstOrDefaultAsync(u => u.Id == userId);

var loginHistory = user.LoginHistories
    .OrderByDescending(h => h.LoginTime)
    .ToList();
```

### 2. Pobranie użytkownika z konkretnego wpisu historii

```csharp
var loginEntry = await _context.UserLoginHistories
    .Include(h => h.User)
    .FirstOrDefaultAsync(h => h.Id == loginId);

var userName = loginEntry.User?.UserName;
```

### 3. Filtrowanie niepowodzeń logowania

```csharp
var failedLogins = await _context.UserLoginHistories
    .Include(h => h.User)
    .Where(h => h.UserId == userId && !h.IsSuccessful)
    .OrderByDescending(h => h.LoginTime)
    .Take(10)
    .ToListAsync();
```

### 4. Statystyki logowania dla użytkownika

```csharp
var stats = await _context.UserLoginHistories
 .Where(h => h.UserId == userId)
    .GroupBy(h => h.IsSuccessful)
    .Select(g => new 
    {
        IsSuccessful = g.Key,
     Count = g.Count(),
        LastLogin = g.Max(h => h.LoginTime)
    })
  .ToListAsync();
```

### 5. Razor Pages - Wyświetlanie historii logowania

#### LoginHistory.cshtml.cs
```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirlineManager.Pages.Account
{
 public class LoginHistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginHistoryModel(ApplicationDbContext context)
  {
      _context = context;
        }

        public ApplicationUser CurrentUser { get; set; }
        public List<UserLoginHistory> LoginHistory { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        CurrentUser = await _context.Users
      .Include(u => u.LoginHistories)
                .FirstOrDefaultAsync(u => u.Id == userId);

        if (CurrentUser == null)
       {
                return NotFound();
}

    LoginHistory = CurrentUser.LoginHistories
         .OrderByDescending(h => h.LoginTime)
            .Take(50)
    .ToList();

 return Page();
        }
    }
}
```

#### LoginHistory.cshtml
```razor
@page
@model LoginHistoryModel
@{
    ViewData["Title"] = "Login History";
}

<h1>Login History</h1>
<p>Showing last @Model.LoginHistory.Count login attempts</p>

<table class="table table-striped">
    <thead>
        <tr>
            <th>Date & Time</th>
<th>IP Address</th>
            <th>Location</th>
            <th>Device</th>
            <th>Browser</th>
          <th>Status</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var login in Model.LoginHistory)
   {
  <tr class="@(login.IsSuccessful ? "" : "table-danger")">
  <td>@login.LoginTime.ToLocalTime().ToString("g")</td>
                <td>@login.IpAddress</td>
     <td>@login.City, @login.Country</td>
   <td>@login.Device (@login.OperatingSystem)</td>
     <td>@login.Browser</td>
 <td>
    @if (login.IsSuccessful)
        {
            <span class="badge bg-success">
        <i class="bi bi-check-circle"></i> Success
           </span>
     }
        else
        {
         <span class="badge bg-danger">
    <i class="bi bi-x-circle"></i> Failed
            </span>
          @if (!string.IsNullOrEmpty(login.FailureReason))
         {
               <br /><small>@login.FailureReason</small>
       }
              }
         </td>
       </tr>
        }
    </tbody>
</table>
```

### 6. Admin Panel - Historia logowań wszystkich użytkowników

```csharp
// Areas/Admin/Pages/LoginHistory/Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<UserLoginHistory> LoginHistory { get; set; }

    public async Task OnGetAsync(
        string? userId = null, 
        bool? onlyFailed = null,
        DateTime? fromDate = null)
    {
  var query = _context.UserLoginHistories
      .Include(h => h.User)
            .AsQueryable();

   if (!string.IsNullOrEmpty(userId))
        {
    query = query.Where(h => h.UserId == userId);
   }

 if (onlyFailed == true)
        {
    query = query.Where(h => !h.IsSuccessful);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(h => h.LoginTime >= fromDate.Value);
        }

   LoginHistory = await query
            .OrderByDescending(h => h.LoginTime)
            .Take(100)
         .ToListAsync();
    }
}
```

## 🔍 Indeksy

Relacja wykorzystuje następujące indeksy dla optymalizacji zapytań:

```csharp
entity.HasIndex(e => e.UserId);        // Szybkie wyszukiwanie po użytkowniku
entity.HasIndex(e => e.LoginTime);     // Sortowanie chronologiczne
entity.HasIndex(e => e.IsSuccessful);  // Filtrowanie powodzeń/niepowodzeń
```

## ⚠️ Uwagi dotyczące wydajności

1. **Eager Loading vs Lazy Loading**
   - Używaj `.Include()` tylko gdy potrzebujesz danych użytkownika
   - Dla prostych zapytań o historię, wystarczy samo `UserLoginHistory`

2. **Paginacja**
   - Historia logowań może rosnąć bardzo szybko
   - Zawsze stosuj `.Take()` lub paginację

3. **Archiwizacja**
   - Rozważ archiwizację starych wpisów (np. starszych niż rok)
   - Użyj polityki retencji danych z `DataRetentionService`

## 🛡️ Bezpieczeństwo

1. **Cascade Delete**
   - Usunięcie użytkownika automatycznie usuwa jego historię
   - Rozważ soft delete dla zgodności z audytem

2. **Dostęp do danych**
   - Użytkownicy powinni widzieć tylko swoją historię
   - Administratorzy mogą widzieć wszystkie wpisy
   - Implementuj autoryzację w kontrolerach/stronach

3. **Dane wrażliwe**
   - IP adresy i lokalizacje to dane osobowe
   - Stosuj RODO/GDPR przy przechowywaniu

## ✅ Korzyści z relacji

- **Integralność danych**: Foreign Key zapewnia spójność
- **Cascade Delete**: Automatyczne czyszczenie przy usunięciu użytkownika
- **Type Safety**: Strongly-typed navigation properties
- **LINQ Support**: Łatwe zapytania z `.Include()` i `.ThenInclude()`
- **Performance**: Indeksy przyspieszają zapytania

## 📊 Przykładowe zapytania SQL generowane przez EF Core

### Pobranie użytkownika z historią
```sql
SELECT u.*, h.*
FROM AspNetUsers u
LEFT JOIN UserLoginHistories h ON u.Id = h.UserId
WHERE u.Id = @userId
ORDER BY h.LoginTime DESC
```

### Statystyki logowania
```sql
SELECT 
    UserId,
  COUNT(*) as TotalAttempts,
    SUM(CASE WHEN IsSuccessful = 1 THEN 1 ELSE 0 END) as Successful,
    SUM(CASE WHEN IsSuccessful = 0 THEN 1 ELSE 0 END) as Failed
FROM UserLoginHistories
WHERE LoginTime >= DATEADD(day, -30, GETDATE())
GROUP BY UserId
```

## 🎓 Dobre praktyki

1. ✅ Zawsze używaj `Include()` gdy potrzebujesz danych użytkownika
2. ✅ Stosuj paginację dla dużych kolekcji
3. ✅ Filtruj na poziomie zapytania SQL (`.Where()`), nie w pamięci
4. ✅ Używaj projection (`.Select()`) gdy potrzebujesz tylko niektórych pól
5. ✅ Testuj zapytania pod kątem wydajności w środowisku produkcyjnym
6. ✅ Monitoruj rozmiar tabeli i implementuj politykę retencji

---

**Następna relacja do implementacji**: `ApplicationUser → UserSession`
