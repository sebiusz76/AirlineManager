# Relacje: ApplicationUser → UserAuditLog (podwójna relacja)

## 📊 Typ relacji
**Two One-to-Many relationships** (Dwie relacje Jeden do wielu)

⚠️ **UWAGA**: To jest specjalny przypadek - **jedna tabela ma dwie relacje do drugiej tabeli**!

## 🎯 Opis

Tabela `UserAuditLog` ma **dwie różne relacje** do `ApplicationUser`:

### 1. ApplicationUser → UserAuditLog (jako podmiot)
**Kogo dotyczy zmiana?** - Historia zmian użytkownika
- Jeden użytkownik może mieć wiele wpisów audytu o sobie
- Foreign Key: `UserId`
- Navigation Property: `User` → `AuditLogs`
- Delete Behavior: **CASCADE**

### 2. ApplicationUser → UserAuditLog (jako modyfikator)
**Kto dokonał zmiany?** - Tracking administratorów
- Jeden użytkownik (admin) może utworzyć wiele wpisów audytu
- Foreign Key: `ModifiedBy`
- Navigation Property: `Modifier` → `ModifiedAuditLogs`
- Delete Behavior: **RESTRICT**

## 📋 Struktura relacji

```
┌─────────────────────┐
│   AspNetUsers│      
│ (ApplicationUser)   │
├─────────────────────┤
│ Id (PK)      │
│ UserName     │
│ Email      │
│ ...│
│   │
│ AuditLogs ──────────┼──┐ (jako podmiot)
│ ModifiedAuditLogs ──┼──┼──┐ (jako modyfikator)
└─────────────────────┘  │  │
 │  │
  ┌────────────────────────┐  │
    │  UserAuditLogs│  │
    ├────────────────────────┤  │
  │ Id (PK) │  │
    │ UserId (FK 1) ◄─────────┘  │
 │ ModifiedBy (FK 2) ◄────────┘
    │ Action        │
    │ ModifiedAt     │
    │ Changes   │
    │ User (nav to subject)│
    │ Modifier (nav to actor)│
└────────────────────────┘
```

## 🔑 Klucze

### Primary Keys
- **ApplicationUser**: `Id` (nvarchar(450))
- **UserAuditLog**: `Id` (int, IDENTITY)

### Foreign Keys
1. **UserAuditLog.UserId** → **ApplicationUser.Id** (CASCADE)
2. **UserAuditLog.ModifiedBy** → **ApplicationUser.Id** (RESTRICT)

### Delete Behaviors

| FK | Delete Behavior | Dlaczego? |
|----|----------------|-----------|
| `UserId` → `Id` | **CASCADE** | Usunięcie użytkownika usuwa jego historię audytu (RODO - prawo do zapomnienia) |
| `ModifiedBy` → `Id` | **RESTRICT** | NIE usuwamy audytu gdy admin zostanie usunięty (zachowanie odpowiedzialności) |

## 💾 Właściwości nawigacyjne

### W ApplicationUser.cs
```csharp
// Navigation property 1: User as subject of audit
public virtual ICollection<UserAuditLog> AuditLogs { get; set; } = new List<UserAuditLog>();

// Navigation property 2: User as modifier/actor
public virtual ICollection<UserAuditLog> ModifiedAuditLogs { get; set; } = new List<UserAuditLog>();
```

### W UserAuditLog.cs
```csharp
// Navigation property 1: Reference to the user being audited
[ForeignKey(nameof(UserId))]
public virtual ApplicationUser? User { get; set; }

// Navigation property 2: Reference to the user who made the change
[ForeignKey(nameof(ModifiedBy))]
public virtual ApplicationUser? Modifier { get; set; }
```

## ⚙️ Konfiguracja w ApplicationDbContext (Multiple Relationships)

```csharp
builder.Entity<UserAuditLog>(entity =>
{
    // ... property configuration ...

    // Relationship 1: ApplicationUser -> UserAuditLog (as subject)
    entity.HasOne(e => e.User)
        .WithMany(u => u.AuditLogs)
        .HasForeignKey(e => e.UserId)
  .OnDelete(DeleteBehavior.Cascade)  // ⚠️ CASCADE
        .IsRequired();

    // Relationship 2: ApplicationUser -> UserAuditLog (as modifier)
    entity.HasOne(e => e.Modifier)
    .WithMany(u => u.ModifiedAuditLogs)
     .HasForeignKey(e => e.ModifiedBy)
        .OnDelete(DeleteBehavior.Restrict)  // ⚠️ RESTRICT
        .IsRequired();
});
```

### ⚠️ Dlaczego różne Delete Behaviors?

#### CASCADE dla UserId
```csharp
// Prawo do zapomnienia (RODO/GDPR)
DELETE FROM AspNetUsers WHERE Id = 'user123';
// ↓
// Automatycznie usuwa wszystkie UserAuditLogs gdzie UserId = 'user123'
```

#### RESTRICT dla ModifiedBy
```csharp
// Zachowanie odpowiedzialności
DELETE FROM AspNetUsers WHERE Id = 'admin456';
// ↓
// BŁĄD! Nie można usunąć admina, który ma wpisy audytu
// Najpierw trzeba zmienić ModifiedBy lub przenieść logi
```

## 📖 Przykłady użycia

### 1. Historia zmian użytkownika (Eager Loading)
```csharp
// Pobierz użytkownika z historią zmian o nim
var user = await _context.Users
    .Include(u => u.AuditLogs)
    .ThenInclude(a => a.Modifier)  // Eager load również modyfikatorów
    .FirstOrDefaultAsync(u => u.Id == userId);

Console.WriteLine($"User: {user.UserName}");
Console.WriteLine($"Total audit entries: {user.AuditLogs.Count}");

foreach (var auditLog in user.AuditLogs.OrderByDescending(a => a.ModifiedAt))
{
    Console.WriteLine($"{auditLog.ModifiedAt}: {auditLog.Action} by {auditLog.Modifier.UserName}");
}
```

### 2. Audyt działań administratora
```csharp
// Pobierz wszystkie zmiany dokonane przez admina
var admin = await _context.Users
    .Include(u => u.ModifiedAuditLogs)
 .ThenInclude(a => a.User)  // Eager load użytkowników, których dotyczą zmiany
    .FirstOrDefaultAsync(u => u.Id == adminId);

Console.WriteLine($"Admin: {admin.UserName}");
Console.WriteLine($"Changes made: {admin.ModifiedAuditLogs.Count}");

foreach (var auditLog in admin.ModifiedAuditLogs.OrderByDescending(a => a.ModifiedAt).Take(10))
{
    Console.WriteLine($"{auditLog.ModifiedAt}: Modified {auditLog.User.UserName} - {auditLog.Action}");
}
```

### 3. Zapytanie z obiema relacjami
```csharp
// Pobierz wpisy audytu z pełnymi informacjami
var auditLogs = await _context.UserAuditLogs
    .Include(a => a.User)      // Kogo dotyczy
    .Include(a => a.Modifier)  // Kto dokonał zmiany
    .Where(a => a.ModifiedAt >= DateTime.UtcNow.AddDays(-30))
    .OrderByDescending(a => a.ModifiedAt)
    .Take(100)
    .ToListAsync();

foreach (var log in auditLogs)
{
    Console.WriteLine($"{log.ModifiedAt}: {log.Modifier.UserName} {log.Action} {log.User.UserName}");
    // Przykład: "2024-11-01 14:30: admin@example.com Updated john@example.com"
}
```

### 4. Raport audytu dla użytkownika
```csharp
// Szczegółowy raport zmian profilu użytkownika
var userAuditReport = await _context.UserAuditLogs
    .Include(a => a.Modifier)
.Where(a => a.UserId == userId)
    .OrderByDescending(a => a.ModifiedAt)
    .Select(a => new
    {
   Date = a.ModifiedAt,
    Action = a.Action,
        ModifiedBy = a.Modifier.UserName,
        ModifiedByEmail = a.ModifiedByEmail,
        Changes = a.Changes,
        OldValues = a.OldValues,
   NewValues = a.NewValues
    })
    .ToListAsync();
```

### 5. Statystyki aktywności administratorów
```csharp
// Top 10 najbardziej aktywnych administratorów
var adminStats = await _context.Users
    .Where(u => u.ModifiedAuditLogs.Any())
    .Select(u => new
    {
        Admin = u,
        TotalChanges = u.ModifiedAuditLogs.Count,
  LastActivity = u.ModifiedAuditLogs.Max(a => a.ModifiedAt),
        UsersModified = u.ModifiedAuditLogs.Select(a => a.UserId).Distinct().Count()
    })
    .OrderByDescending(s => s.TotalChanges)
    .Take(10)
    .ToListAsync();
```

### 6. Razor Pages - Historia zmian użytkownika

#### UserAudit.cshtml.cs
```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirlineManager.Pages.Account
{
    public class UserAuditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

public UserAuditModel(ApplicationDbContext context)
    {
  _context = context;
        }

        public ApplicationUser CurrentUser { get; set; }
  public List<UserAuditLog> AuditLogs { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

     CurrentUser = await _context.Users
        .Include(u => u.AuditLogs)
    .ThenInclude(a => a.Modifier)
  .FirstOrDefaultAsync(u => u.Id == userId);

            if (CurrentUser == null)
       {
       return NotFound();
  }

            AuditLogs = CurrentUser.AuditLogs
     .OrderByDescending(a => a.ModifiedAt)
             .Take(50)
  .ToList();

 return Page();
   }
    }
}
```

#### UserAudit.cshtml
```razor
@page
@model UserAuditModel
@{
    ViewData["Title"] = "Account History";
}

<h1>Account Change History</h1>
<p>Track all changes made to your account</p>

@if (!Model.AuditLogs.Any())
{
 <div class="alert alert-info">
        <i class="bi bi-info-circle"></i> No changes recorded yet.
    </div>
}
else
{
    <div class="table-responsive">
<table class="table table-striped">
     <thead>
                <tr>
             <th>Date</th>
       <th>Action</th>
         <th>Modified By</th>
      <th>Details</th>
      </tr>
          </thead>
            <tbody>
   @foreach (var log in Model.AuditLogs)
      {
          <tr>
        <td>@log.ModifiedAt.ToLocalTime().ToString("g")</td>
      <td>
       <span class="badge bg-@GetActionBadgeClass(log.Action)">
      @log.Action
    </span>
          </td>
            <td>
     @if (log.ModifiedBy == Model.CurrentUser.Id)
       {
   <span class="text-muted">You</span>
             }
    else
 {
       <span>@log.Modifier.UserName</span>
        }
      </td>
     <td>
        @if (!string.IsNullOrEmpty(log.Changes))
   {
           <button class="btn btn-sm btn-outline-primary" 
           data-bs-toggle="collapse" 
      data-bs-target="#changes-@log.Id">
              View Changes
           </button>
    <div id="changes-@log.Id" class="collapse mt-2">
               <pre class="bg-light p-2">@log.Changes</pre>
               </div>
       }
 </td>
           </tr>
                }
            </tbody>
        </table>
    </div>
}

@functions {
    string GetActionBadgeClass(string action)
    {
     return action switch
 {
            "Created" => "success",
 "Updated" => "primary",
      "Deleted" => "danger",
       "PasswordReset" => "warning",
       "2FAEnabled" => "info",
   "2FADisabled" => "warning",
   _ => "secondary"
        };
    }
}
```

### 7. Admin Panel - Audyt wszystkich użytkowników

```csharp
// Areas/Admin/Pages/Audit/Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<UserAuditLog> AuditLogs { get; set; }

    public async Task OnGetAsync(
        string? userId = null,
        string? modifiedBy = null,
        string? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.UserAuditLogs
            .Include(a => a.User)
            .Include(a => a.Modifier)
    .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.UserId == userId);

        if (!string.IsNullOrEmpty(modifiedBy))
            query = query.Where(a => a.ModifiedBy == modifiedBy);

        if (!string.IsNullOrEmpty(action))
      query = query.Where(a => a.Action == action);

        if (fromDate.HasValue)
          query = query.Where(a => a.ModifiedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.ModifiedAt <= toDate.Value);

        AuditLogs = await query
            .OrderByDescending(a => a.ModifiedAt)
         .Take(100)
        .ToListAsync();
    }
}
```

## 🔍 Indeksy

Relacje wykorzystują następujące indeksy dla optymalizacji:

```csharp
entity.HasIndex(e => e.UserId);      // FK 1 - szybkie wyszukiwanie po podmiocie
entity.HasIndex(e => e.ModifiedBy);  // FK 2 - szybkie wyszukiwanie po modyfikatorze
entity.HasIndex(e => e.ModifiedAt);  // Czasowy indeks do filtrowania
entity.HasIndex(e => e.Action);      // Indeks do filtrowania po akcji
```

## ⚠️ Ważne uwagi

### 1. **Różne Delete Behaviors**
```csharp
// CASCADE dla UserId - RODO compliance
await _userManager.DeleteAsync(user);
// ↓ Automatycznie usuwa AuditLogs gdzie UserId = user.Id

// RESTRICT dla ModifiedBy - Accountability
await _userManager.DeleteAsync(admin);
// ↓ BŁĄD jeśli admin ma wpisy w ModifiedBy
// Najpierw trzeba zmienić właściciela audytów
```

### 2. **Wydajność**
```csharp
// ❌ N+1 problem
var logs = await _context.UserAuditLogs.ToListAsync();
foreach (var log in logs)
{
    var userName = log.User.UserName;  // Dodatkowe zapytanie!
}

// ✅ Prawidłowo z Include
var logs = await _context.UserAuditLogs
    .Include(a => a.User)
    .Include(a => a.Modifier)
    .ToListAsync();
```

### 3. **Self-Audit**
Użytkownik może być jednocześnie podmiotem i modyfikatorem:
```csharp
var selfAudit = new UserAuditLog
{
    UserId = userId,        // Zmienia siebie
    ModifiedBy = userId,    // Sam dokonał zmiany
Action = "UpdatedProfile",
  ModifiedAt = DateTime.UtcNow
};
```

### 4. **JSON Serialization**
```csharp
// Zapisywanie zmian jako JSON
var changes = new
{
    Field = "Email",
    OldValue = "old@example.com",
    NewValue = "new@example.com"
};

auditLog.Changes = JsonSerializer.Serialize(changes);
```

## 🧪 Testy do wykonania

### Test 1: Eager Loading obu relacji
```csharp
[Fact]
public async Task LoadAuditLog_WithBothRelationships_Succeeds()
{
    // Arrange
    var user = await CreateTestUser();
    var admin = await CreateTestUser();
    
    var auditLog = new UserAuditLog
    {
        UserId = user.Id,
        UserEmail = user.Email,
        ModifiedBy = admin.Id,
        ModifiedByEmail = admin.Email,
        Action = "TEST",
        ModifiedAt = DateTime.UtcNow
    };
    
    _context.UserAuditLogs.Add(auditLog);
    await _context.SaveChangesAsync();
    
    // Act
  var loaded = await _context.UserAuditLogs
        .Include(a => a.User)
        .Include(a => a.Modifier)
        .FirstOrDefaultAsync(a => a.Id == auditLog.Id);
  
    // Assert
    Assert.NotNull(loaded);
    Assert.NotNull(loaded.User);
    Assert.NotNull(loaded.Modifier);
 Assert.Equal(user.Id, loaded.User.Id);
    Assert.Equal(admin.Id, loaded.Modifier.Id);
}
```

### Test 2: CASCADE delete dla UserId
```csharp
[Fact]
public async Task DeleteUser_CascadesDeleteToAuditLogs()
{
    // Arrange
    var user = await CreateTestUser();
    var admin = await CreateTestUser();
    
    var auditLog = new UserAuditLog
    {
        UserId = user.Id,
ModifiedBy = admin.Id,
        // ... other properties
    };
    _context.UserAuditLogs.Add(auditLog);
    await _context.SaveChangesAsync();
    
    // Act
 await _userManager.DeleteAsync(user);
    
    // Assert
    var logs = await _context.UserAuditLogs
        .Where(a => a.UserId == user.Id)
        .ToListAsync();
    
    Assert.Empty(logs);  // Audit logs should be deleted
}
```

### Test 3: RESTRICT delete dla ModifiedBy
```csharp
[Fact]
public async Task DeleteModifier_ThrowsException_WhenHasAuditLogs()
{
    // Arrange
    var user = await CreateTestUser();
    var admin = await CreateTestUser();
    
    var auditLog = new UserAuditLog
    {
   UserId = user.Id,
      ModifiedBy = admin.Id,
        // ... other properties
    };
    _context.UserAuditLogs.Add(auditLog);
    await _context.SaveChangesAsync();
    
    // Act & Assert
    await Assert.ThrowsAsync<DbUpdateException>(
     () => _userManager.DeleteAsync(admin)
    );
    
    // Verify admin still exists
    var adminExists = await _context.Users
        .AnyAsync(u => u.Id == admin.Id);
    Assert.True(adminExists);
}
```

## 📊 Przykładowe zapytania SQL generowane przez EF Core

### Pobranie audytu z obiema relacjami
```sql
SELECT a.*, u1.*, u2.*
FROM UserAuditLogs a
INNER JOIN AspNetUsers u1 ON a.UserId = u1.Id      -- User (subject)
INNER JOIN AspNetUsers u2 ON a.ModifiedBy = u2.Id  -- Modifier (actor)
WHERE a.ModifiedAt >= @fromDate
ORDER BY a.ModifiedAt DESC
```

### Statystyki administratorów
```sql
SELECT 
    u.UserName,
    COUNT(*) as TotalChanges,
    MAX(a.ModifiedAt) as LastActivity,
    COUNT(DISTINCT a.UserId) as UsersModified
FROM AspNetUsers u
INNER JOIN UserAuditLogs a ON u.Id = a.ModifiedBy
GROUP BY u.Id, u.UserName
ORDER BY TotalChanges DESC
```

## 🎓 Dobre praktyki

1. ✅ **Zawsze loguj zmiany wrażliwych danych**
2. ✅ **Używaj JSON dla Changes/OldValues/NewValues**
3. ✅ **Include() tylko gdy potrzebne** (wydajność)
4. ✅ **Implementuj politykę retencji** (365 dni domyślnie)
5. ✅ **Nie usuwaj audytów administratorów** (RESTRICT)
6. ✅ **Użyj Background Service do czyszczenia** starych logów
7. ✅ **Szyfruj wrażliwe dane w Changes**

---

## 🚀 Korzyści z implementacji

### Compliance 📋
- ✅ RODO/GDPR - pełny audyt zmian danych osobowych
- ✅ SOC 2 - tracking wszystkich modyfikacji
- ✅ ISO 27001 - accountability

### Bezpieczeństwo 🔒
- ✅ Śledzenie nieautoryzowanych zmian
- ✅ Wykrywanie podejrzanych działań
- ✅ Forensics - analiza incydentów

### User Experience 👤
- ✅ Użytkownik widzi historię swojego konta
- ✅ Transparentność działań admina
- ✅ Prawo do wglądu (RODO Article 15)

---

**Następna relacja**: ApplicationUser ↔ IdentityRole (opcjonalne)
