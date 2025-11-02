# ? Naprawa b��d�w SQL w skryptach weryfikacyjnych

## ?? Data naprawy: 2024-11-01

---

## ?? Wykryte problemy:

### 1. **Problem z u�ytkownikiem bazy danych**
```
Komunikat 916, poziom 14, stan 2, wiersz 6
The server principal "amappdbuser" is not able to access the database "AirlineManager-Dev" 
under the current security context.
```

**Przyczyna**: Niew�a�ciwe uprawnienia u�ytkownika bazy danych

**Rozwi�zanie**: Zmie� u�ytkownika SQL Server lub dodaj uprawnienia

#### Opcja A: U�yj Windows Authentication (zalecane)
```sql
-- Po��cz si� do SSMS u�ywaj�c Windows Authentication
-- Nie trzeba zmienia� uprawnie�
```

#### Opcja B: Dodaj uprawnienia dla amappdbuser
```sql
USE [AirlineManager-Dev];
GO

-- Dodaj u�ytkownika do bazy
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'amappdbuser')
BEGIN
    CREATE USER [amappdbuser] FOR LOGIN [amappdbuser];
END
GO

-- Przydziel uprawnienia do odczytu
ALTER ROLE db_datareader ADD MEMBER [amappdbuser];
GO

-- Dla test�w CASCADE DELETE potrzebne s� uprawnienia do zapisu
ALTER ROLE db_datawriter ADD MEMBER [amappdbuser];
GO
```

#### Opcja C: Zmie� u�ytkownika w connection string
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AirlineManager-Dev;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

### 2. **Problem z GROUP BY**
```
Komunikat 8120, poziom 16, stan 1, wiersz 75
Column 'sys.foreign_keys.delete_referential_action_desc' is invalid in the select list 
because it is not contained in either an aggregate function or the GROUP BY clause.
```

**Przyczyna**: SQL wymaga GROUP BY dla wszystkich kolumn nie u�ywaj�cych funkcji agreguj�cych

**Rozwi�zanie**: Dodano `MAX()` i `GROUP BY fk.name`

#### Przed (b��dne):
```sql
SELECT 
    'UserLoginHistory' AS [Relation],
    COUNT(*) AS [FK Verified],
  'CASCADE' AS [Expected Delete],
    fk.delete_referential_action_desc AS [Actual Delete],  -- ? B��D!
    CASE 
   WHEN fk.delete_referential_action_desc = 'CASCADE' THEN '? CORRECT'
        ELSE '? INCORRECT'
    END AS [Status]
FROM 
    sys.foreign_keys AS fk
WHERE 
    fk.name = 'FK_UserLoginHistories_AspNetUsers_UserId';
-- ? Brak GROUP BY!
```

#### Po (poprawne):
```sql
SELECT 
    'UserLoginHistory' AS [Relation],
    COUNT(*) AS [FK Verified],
    'CASCADE' AS [Expected Delete],
    MAX(fk.delete_referential_action_desc) AS [Actual Delete],  -- ? MAX()
    CASE 
        WHEN MAX(fk.delete_referential_action_desc) = 'CASCADE' THEN '? CORRECT'
   ELSE '? INCORRECT'
    END AS [Status]
FROM 
    sys.foreign_keys AS fk
WHERE 
    fk.name = 'FK_UserLoginHistories_AspNetUsers_UserId'
GROUP BY fk.name;  -- ? GROUP BY
```

---

## ? Co zosta�o naprawione:

### 1. **Comprehensive-Relationship-Check.sql**
- ? Dodano `MAX()` dla `delete_referential_action_desc`
- ? Dodano `GROUP BY fk.name` do wszystkich zapyta� FK
- ? Naprawiono 4 zapytania:
  - Relacja 1: UserLoginHistory
- Relacja 2: UserSession
  - Relacja 3: UserAuditLog (User)
  - Relacja 4: UserAuditLog (Modifier)

### 2. **Quick-Relationship-Check.sql**
- ? Bez zmian (ten skrypt nie mia� b��du GROUP BY)

---

## ?? Testowanie poprawek:

### Test 1: Sprawd� sk�adni� SQL
```sql
-- Uruchom w SSMS:
-- 1. Otw�rz: Comprehensive-Relationship-Check.sql
-- 2. Query ? Parse (Ctrl+F5)
-- 3. Sprawd� czy nie ma b��d�w sk�adni
```

### Test 2: Uruchom Comprehensive Check
```sql
-- W SSMS lub Azure Data Studio:
-- 1. File ? Open ? Comprehensive-Relationship-Check.sql
-- 2. Upewnij si� �e jeste� po��czony jako u�ytkownik z uprawnieniami
-- 3. F5 (Execute)
-- 4. Sprawd� wyniki
```

---

## ?? Oczekiwane wyniki (po naprawie):

### Cz�� 1: Wszystkie Foreign Keys
```
Foreign Key Name                  Child Table         On Delete
FK_UserLoginHistories_AspNetUsers_UserId      UserLoginHistories  CASCADE
FK_UserSessions_AspNetUsers_UserId   UserSessions        CASCADE
FK_UserAuditLogs_AspNetUsers_UserId             UserAuditLogs       CASCADE
FK_UserAuditLogs_AspNetUsers_ModifiedBy  UserAuditLogs       NO_ACTION
FK_AspNetUserRoles_AspNetUsers_UserId           AspNetUserRoles     CASCADE
FK_AspNetUserRoles_AspNetRoles_RoleId   AspNetUserRoles   CASCADE
```

### Cz�� 2: Szczeg�owa weryfikacja
```
? FK_UserLoginHistories_AspNetUsers_UserId: EXISTS
? FK_UserSessions_AspNetUsers_UserId: EXISTS
? FK_UserAuditLogs_AspNetUsers_UserId: EXISTS
? FK_UserAuditLogs_AspNetUsers_ModifiedBy: EXISTS
? FK_AspNetUserRoles_AspNetUsers_UserId: EXISTS
? FK_AspNetUserRoles_AspNetRoles_RoleId: EXISTS
```

### Cz�� 3: Testy integralno�ci
```
--- TEST 1: UserLoginHistory Foreign Key Constraint ---
? PASSED: FK constraint works correctly

--- TEST 2: UserSession Foreign Key Constraint ---
? PASSED: FK constraint works correctly

--- TEST 3: UserAuditLog (podmiot) Foreign Key Constraint ---
? PASSED: FK constraint works correctly
```

### Cz�� 7: Podsumowanie
```
IMPLEMENTATION PROGRESS:
---
Total Relations: 5
Implemented: 4
Missing: 1
Progress: 80%

STATUS BY RELATION:
---
1. UserLoginHistory: ? IMPLEMENTED
2. UserSession: ? IMPLEMENTED
3. UserAuditLog (podmiot): ? IMPLEMENTED
4. UserAuditLog (modyfikator): ? IMPLEMENTED
5. IdentityRole (Many-to-Many): ? IMPLEMENTED
```

---

## ?? Rozwi�zywanie dalszych problem�w:

### Problem: "Cannot access database"
**Sprawd�:**
1. Czy baza danych istnieje?
   ```sql
   SELECT name FROM sys.databases WHERE name = 'AirlineManager-Dev';
   ```

2. Czy u�ytkownik ma dost�p?
   ```sql
   USE [AirlineManager-Dev];
   SELECT USER_NAME();  -- Sprawd� aktualnego u�ytkownika
   ```

3. Czy u�ytkownik ma uprawnienia?
   ```sql
   SELECT 
       dp.name AS UserName,
  dp.type_desc AS UserType,
 r.name AS RoleName
   FROM sys.database_principals dp
   LEFT JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
   LEFT JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
   WHERE dp.name = 'amappdbuser';
   ```

### Problem: "Timeout expired"
**Rozwi�zanie:**
```sql
-- Zwi�ksz timeout w SSMS:
-- Tools ? Options ? Query Execution ? SQL Server ? General
-- Execution time-out: 600 (10 minut)
```

### Problem: "Transaction log full"
**Rozwi�zanie:**
```sql
-- Skrypt u�ywa ROLLBACK, wi�c nie powinno by� problemu
-- Ale je�li wyst�pi:
USE [AirlineManager-Dev];
GO
DBCC SHRINKFILE (AirlineManager-Dev_log, 1);
GO
```

---

## ? Checklist po naprawie:

- [x] Naprawiono b��dy GROUP BY w Comprehensive-Relationship-Check.sql
- [x] Dodano MAX() dla aggregate functions
- [x] Zweryfikowano sk�adni� SQL (Parse)
- [ ] Uruchomiono testy w SSMS/Azure Data Studio  ? **TY TERAZ**
- [ ] Sprawdzono �e wszystkie 4 FK s� wykryte
- [ ] Zweryfikowano CASCADE DELETE tests

---

## ?? Nast�pne kroki:

1. **Uruchom naprawiony skrypt**
   ```bash
   # W SSMS lub Azure Data Studio:
   # File ? Open ? Comprehensive-Relationship-Check.sql
   # F5 (Execute)
   ```

2. **Sprawd� wyniki**
   - Czy wszystkie 4 relacje s� wykryte?
   - Czy testy CASCADE DELETE przechodz�?
   - Czy Progress pokazuje 80%?

3. **Je�li wszystko OK**
   - Zapisz wyniki do pliku
   - Dokumentacja gotowa!
   - Mo�esz przej�� do opcjonalnej relacji #5 (IdentityRole)

---

## ?? Podsumowanie zmian:

| Plik | Linie zmienione | Typ zmiany |
|------|----------------|------------|
| `Comprehensive-Relationship-Check.sql` | 75, 91, 107, 123 | Dodano MAX() i GROUP BY |
| `Quick-Relationship-Check.sql` | - | Bez zmian (nie mia� b��du) |

---

**Data naprawy**: 2024-11-01 14:45  
**Status**: ? **NAPRAWIONE**  
**Gotowe do test�w**: TAK  
**Przetestowane**: ? Czeka na uruchomienie przez u�ytkownika
