# Relacja: ApplicationUser → UserSession

## 📊 Typ relacji
**One-to-Many (Jeden do wielu)**

## 🎯 Opis
Jeden użytkownik (`ApplicationUser`) może mieć wiele aktywnych sesji (`UserSession`). Ta relacja pozwala na:
- Zarządzanie wieloma aktywnymi sesjami (różne urządzenia/przeglądarki)
- Wylogowanie ze wszystkich urządzeń
- Wykrywanie podejrzanych sesji
- Audyt aktywności użytkowników

## 📋 Struktura relacji

```
ApplicationUser (1) ─────< UserSession (∞)
   │            │
   └─────── UserId ─────────┘
```

## 🔑 Klucze

- **Primary Key**: `UserSession.Id`
- **Foreign Key**: `UserSession.UserId` → `ApplicationUser.Id`
- **Delete Behavior**: `Cascade` (usunięcie użytkownika usuwa jego sesje)

## 💾 Właściwości nawigacyjne

### W ApplicationUser.cs
```csharp
// Navigation Property - Kolekcja aktywnych sesji
public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
```

### W UserSession.cs
```csharp
// Navigation Property - Odniesienie do użytkownika
[ForeignKey(nameof(UserId))]
public virtual ApplicationUser? User { get; set; }
```

## ⚙️ Konfiguracja w ApplicationDbContext

```csharp
builder.Entity<UserSession>(entity =>
{
 // ... inne konfiguracje ...
 
  // Konfiguracja relacji One-to-Many
    entity.HasOne(e => e.User)
 .WithMany(u => u.Sessions)
    .HasForeignKey(e => e.UserId)
 .OnDelete(DeleteBehavior.Cascade);
});
```

## 📖 Przykłady użycia

### 1. Pobranie aktywnych sesji użytkownika (Eager Loading)

```csharp
// W kontrolerze lub serwisie
var user = await _context.Users
    .Include(u => u.Sessions)
    .FirstOrDefaultAsync(u => u.Id == userId);

var activeSessions = user.Sessions
    .Where(s => s.IsActive)
    .OrderByDescending(s => s.LastActivityAt)
    .ToList();
```

### 2. Pobranie użytkownika z konkretnej sesji

```csharp
var session = await _context.UserSessions
    .Include(s => s.User)
    .FirstOrDefaultAsync(s => s.SessionId == sessionId);

var userName = session.User?.UserName;
```

### 3. Wylogowanie ze wszystkich urządzeń

```csharp
// Dezaktywuj wszystkie sesje użytkownika
await _context.UserSessions
    .Where(s => s.UserId == userId && s.IsActive)
    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));
```

### 4. Liczba aktywnych sesji per użytkownik

```csharp
var sessionStats = await _context.Users
    .Select(u => new
    {
  User = u,
        ActiveSessions = u.Sessions.Count(s => s.IsActive),
        TotalSessions = u.Sessions.Count(),
        LastActivity = u.Sessions
            .Where(s => s.IsActive)
 .Max(s => (DateTime?)s.LastActivityAt)
    })
    .ToListAsync();
```

### 5. Razor Pages - Wyświetlanie aktywnych sesji

#### ActiveSessions.cshtml.cs
```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirlineManager.Pages.Account
{
    public class ActiveSessionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ActiveSessionsModel(ApplicationDbContext context)
  {
     _context = context;
        }

        public ApplicationUser CurrentUser { get; set; }
        public List<UserSession> ActiveSessions { get; set; }

    public async Task<IActionResult> OnGetAsync()
      {
   var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
    CurrentUser = await _context.Users
.Include(u => u.Sessions)
       .FirstOrDefaultAsync(u => u.Id == userId);

if (CurrentUser == null)
 {
        return NotFound();
        }

       ActiveSessions = CurrentUser.Sessions
   .Where(s => s.IsActive)
 .OrderByDescending(s => s.LastActivityAt)
    .ToList();

   return Page();
        }

        public async Task<IActionResult> OnPostTerminateAsync(int sessionId)
        {
            var session = await _context.UserSessions.FindAsync(sessionId);
   
            if (session == null || session.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
          {
     return Forbid();
            }

            session.IsActive = false;
            await _context.SaveChangesAsync();

     return RedirectToPage();
}

      public async Task<IActionResult> OnPostTerminateAllAsync()
        {
     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentSessionId = HttpContext.Session.Id;

            await _context.UserSessions
     .Where(s => s.UserId == userId 
      && s.IsActive 
      && s.SessionId != currentSessionId)
       .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

         return RedirectToPage();
    }
    }
}
```

#### ActiveSessions.cshtml
```razor
@page
@model ActiveSessionsModel
@{
    ViewData["Title"] = "Active Sessions";
}

<h1>Active Sessions</h1>
<p>Manage your active sessions across different devices</p>

@if (Model.ActiveSessions.Count == 0)
{
    <div class="alert alert-info">
    <i class="bi bi-info-circle"></i> No active sessions found.
    </div>
}
else
{
    <div class="mb-3">
     <form method="post" asp-page-handler="TerminateAll">
          <button type="submit" class="btn btn-warning" 
    onclick="return confirm('Are you sure you want to terminate all other sessions? You will remain logged in on this device.')">
     <i class="bi bi-power"></i> Terminate All Other Sessions
            </button>
        </form>
    </div>

    <div class="row">
        @foreach (var session in Model.ActiveSessions)
        {
    <div class="col-md-6 mb-3">
 <div class="card @(session.SessionId == HttpContext.Session.Id ? "border-primary" : "")">
        <div class="card-body">
              @if (session.SessionId == HttpContext.Session.Id)
         {
       <span class="badge bg-primary mb-2">Current Session</span>
     }
        
    <h5 class="card-title">
    <i class="bi bi-@(session.Device == "Mobile" ? "phone" : session.Device == "Tablet" ? "tablet" : "laptop")"></i>
          @session.Device (@session.OperatingSystem)
         </h5>
            
           <p class="card-text">
            <strong>Browser:</strong> @session.Browser<br/>
  <strong>Location:</strong> @session.City, @session.Country<br/>
    <strong>IP Address:</strong> @session.IpAddress<br/>
         <strong>Created:</strong> @session.CreatedAt.ToLocalTime().ToString("g")<br/>
       <strong>Last Activity:</strong> @session.LastActivityAt.ToLocalTime().ToString("g")<br/>
       @if (session.ExpiresAt.HasValue)
{
               <strong>Expires:</strong> @session.ExpiresAt.Value.ToLocalTime().ToString("g")<br/>
           }
      @if (session.IsPersistent)
       {
        <span class="badge bg-info">Remember Me</span>
      }
          </p>
           
@if (session.SessionId != HttpContext.Session.Id)
         {
     <form method="post" asp-page-handler="Terminate" asp-route-sessionId="@session.Id">
        <button type="submit" class="btn btn-sm btn-danger"
        onclick="return confirm('Are you sure you want to terminate this session?')">
    <i class="bi bi-x-circle"></i> Terminate Session
         </button>
       </form>
            }
             </div>
          </div>
            </div>
        }
  </div>
}
```

### 6. Admin Panel - Wyświetlanie sesji wszystkich użytkowników

```csharp
// Areas/Admin/Pages/Sessions/Index.cshtml.cs
public class IndexModel : PageModel
{
private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
  {
        _context = context;
    }

    public List<UserSession> Sessions { get; set; }

    public async Task OnGetAsync(
        string? userId = null,
   bool? onlyActive = null,
        DateTime? fromDate = null)
    {
        var query = _context.UserSessions
.Include(s => s.User)
        .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
      query = query.Where(s => s.UserId == userId);
        }

        if (onlyActive == true)
        {
            query = query.Where(s => s.IsActive);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= fromDate.Value);
        }

        Sessions = await query
     .OrderByDescending(s => s.LastActivityAt)
     .Take(100)
          .ToListAsync();
    }
}
```

## 🔍 Indeksy

Relacja wykorzystuje następujące indeksy dla optymalizacji zapytań:

```csharp
entity.HasIndex(e => e.UserId);        // Szybkie wyszukiwanie po użytkowniku
entity.HasIndex(e => e.SessionId).IsUnique();  // Unikalny identyfikator sesji
entity.HasIndex(e => e.IsActive);      // Filtrowanie aktywnych sesji
entity.HasIndex(e => e.ExpiresAt);     // Czyszczenie wygasłych sesji
```

## ⚠️ Uwagi dotyczące wydajności

1. **Eager Loading vs Lazy Loading**
   - Używaj `.Include()` tylko gdy potrzebujesz danych sesji
   - Dla prostych zapytań o sesje, wystarczy samo `UserSession`

2. **Automatyczne czyszczenie**
   - System automatycznie czyści nieaktywne sesje (background service)
   - Polityka retencji: 30 dni (konfigurowane w `DataRetentionService`)

3. **Limit sesji**
   - Rozważ limit maksymalnych aktywnych sesji per użytkownik
   - Przykład: maksymalnie 5 aktywnych sesji

## 🛡️ Bezpieczeństwo

1. **Cascade Delete**
   - Usunięcie użytkownika automatycznie usuwa jego sesje
   - Zapobiega "orphaned sessions"

2. **Dostęp do danych**
   - Użytkownicy widzą tylko swoje sesje
   - Administratorzy mogą widzieć wszystkie sesje
   - Implementuj autoryzację w kontrolerach/stronach

3. **Session Hijacking Prevention**
   - Przechowuj IP address i User Agent
   - Wykrywaj podejrzane zmiany (zmiana IP, urządzenia)
   - Wymagaj re-authentication przy ważnych operacjach

4. **Session Expiration**
   - Automatyczne wygaszanie nieaktywnych sesji
   - "Remember Me" zwiększa czas wygaśnięcia
   - Timeout dla idle sessions (domyślnie 60 minut)

## ✅ Korzyści z relacji

- **Bezpieczeństwo**: Wykrywanie nieautoryzowanych sesji
- **User Experience**: "Wyloguj ze wszystkich urządzeń"
- **Audyt**: Śledzenie gdzie użytkownik się logował
- **Compliance**: RODO - prawo do wglądu w aktywne sesje
- **Troubleshooting**: Debugowanie problemów z sesjami

## 📊 Przykładowe zapytania SQL generowane przez EF Core

### Pobranie użytkownika z sesjami
```sql
SELECT u.*, s.*
FROM AspNetUsers u
LEFT JOIN UserSessions s ON u.Id = s.UserId
WHERE u.Id = @userId
  AND s.IsActive = 1
ORDER BY s.LastActivityAt DESC
```

### Statystyki sesji
```sql
SELECT 
    UserId,
  COUNT(*) as TotalSessions,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveSessions,
    MAX(LastActivityAt) as LastActivity,
    MIN(CreatedAt) as FirstSession
FROM UserSessions
GROUP BY UserId
```

## 🎓 Dobre praktyki

1. ✅ Zawsze używaj `Include()` gdy potrzebujesz danych użytkownika z sesji
2. ✅ Regularnie czyść nieaktywne i wygasłe sesje (background service)
3. ✅ Loguj podejrzane aktywności sesji (zmiana IP, device fingerprint)
4. ✅ Implementuj session timeout based on activity
5. ✅ Używaj secure cookies z `HttpOnly` i `Secure` flags
6. ✅ Przechowuj session fingerprint (IP + UserAgent hash)

## 🚨 Scenariusze użycia

### Wykrywanie podejrzanej aktywności
```csharp
// Sprawdź czy sesja zmienia lokalizację/urządzenie
var suspiciousSessions = await _context.UserSessions
    .Include(s => s.User)
    .Where(s => s.IsActive)
 .GroupBy(s => s.UserId)
    .Where(g => g.Select(s => s.IpAddress).Distinct().Count() > 3) // Więcej niż 3 różne IP
    .Select(g => new { UserId = g.Key, Sessions = g.ToList() })
    .ToListAsync();
```

### Wylogowanie użytkownika z wszystkich urządzeń (Admin)
```csharp
// Admin force logout
await _context.UserSessions
    .Where(s => s.UserId == targetUserId && s.IsActive)
    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

// Zaloguj akcję w audit log
await _auditService.LogAdminActionAsync(
    adminId,
    $"Forced logout all sessions for user {targetUserId}",
    targetUserId
);
```

---

**Następna relacja do implementacji**: `ApplicationUser → UserAuditLog (podmiot audytu)`
