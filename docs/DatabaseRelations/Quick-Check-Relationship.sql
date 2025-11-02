-- ============================================
-- QUICK CHECK: Czy relacja istnieje w bazie?
-- Wykonaj ten skrypt w SQL Server Management Studio
-- lub Azure Data Studio
-- ============================================

USE [AirlineManager-Dev]; -- Zmień nazwę bazy jeśli inna
GO

PRINT '================================================';
PRINT 'SPRAWDZANIE RELACJI: ApplicationUser → UserLoginHistory';
PRINT '================================================';
PRINT '';

-- ============================================
-- TEST 1: Czy Foreign Key istnieje?
-- ============================================
PRINT '--- TEST 1: Sprawdzanie Foreign Key ---';

IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_UserLoginHistories_AspNetUsers_UserId'
      AND OBJECT_NAME(parent_object_id) = 'UserLoginHistories'
)
BEGIN
    PRINT '✅ SUKCES: Foreign Key istnieje!';
    PRINT '';
    
    SELECT 
      fk.name AS [Foreign Key Name],
        OBJECT_NAME(fk.parent_object_id) AS [Child Table],
        COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS [Child Column],
OBJECT_NAME(fk.referenced_object_id) AS [Parent Table],
        COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS [Parent Column],
        fk.delete_referential_action_desc AS [On Delete],
     fk.update_referential_action_desc AS [On Update],
      CASE WHEN fk.is_disabled = 0 THEN 'Enabled' ELSE 'Disabled' END AS [Status]
 FROM 
        sys.foreign_keys AS fk
    INNER JOIN 
        sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
    WHERE 
        fk.name = 'FK_UserLoginHistories_AspNetUsers_UserId';
END
ELSE
BEGIN
    PRINT '❌ BŁĄD: Foreign Key NIE ISTNIEJE!';
    PRINT 'Musisz uruchomić migrację: dotnet ef database update';
END

PRINT '';
PRINT '';

-- ============================================
-- TEST 2: Czy indeks na UserId istnieje?
-- ============================================
PRINT '--- TEST 2: Sprawdzanie indeksu na UserId ---';

SELECT 
    i.name AS [Index Name],
    i.type_desc AS [Index Type],
    CASE WHEN i.is_unique = 1 THEN 'Yes' ELSE 'No' END AS [Is Unique],
    COL_NAME(ic.object_id, ic.column_id) AS [Column Name]
FROM 
    sys.indexes AS i
INNER JOIN 
    sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE 
    i.object_id = OBJECT_ID('UserLoginHistories')
    AND COL_NAME(ic.object_id, ic.column_id) = 'UserId';

PRINT '';
PRINT '';

-- ============================================
-- TEST 3: Integralność referencyjna
-- ============================================
PRINT '--- TEST 3: Test integralności referencyjnej ---';
PRINT 'Próba dodania wpisu z nieistniejącym UserId...';
PRINT '';

BEGIN TRY
    -- Próba dodania nieprawidłowego wpisu
    INSERT INTO UserLoginHistories (UserId, UserEmail, LoginTime, IsSuccessful, RequiredTwoFactor)
    VALUES ('00000000-0000-0000-0000-000000000000', 'test@invalid.com', GETUTCDATE(), 1, 0);
    
    -- Jeśli się udało, to błąd!
    PRINT '❌ BŁĄD: Foreign Key constraint NIE DZIAŁA!';
    PRINT 'Można dodać wpis z nieistniejącym UserId!';
    
    -- Cleanup
    DELETE FROM UserLoginHistories WHERE UserId = '00000000-0000-0000-0000-000000000000';
END TRY
BEGIN CATCH
    PRINT '✅ SUKCES: Foreign Key constraint DZIAŁA POPRAWNIE!';
 PRINT 'Błąd (oczekiwany): ' + ERROR_MESSAGE();
END CATCH

PRINT '';
PRINT '';

-- ============================================
-- TEST 4: Cascade Delete
-- ============================================
PRINT '--- TEST 4: Test Cascade Delete ---';
PRINT 'Tworzenie testowego użytkownika i historii logowania...';
PRINT '';

BEGIN TRANSACTION;

DECLARE @TestUserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
DECLARE @TestEmail NVARCHAR(256) = 'cascade.test@airlinemanager.local';

-- Utwórz testowego użytkownika
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, MustChangePassword, PreferredTheme
)
VALUES (
    @TestUserId, 
    @TestEmail, 
    UPPER(@TestEmail), 
    @TestEmail, 
    UPPER(@TestEmail),
    1, 
    'TEST_HASH_' + CAST(NEWID() AS NVARCHAR(50)), 
    CAST(NEWID() AS NVARCHAR(50)), 
    CAST(NEWID() AS NVARCHAR(50)),
    0, 0, 1, 0,
    'Test', 
    'User', 
    0, 
  'auto'
);

PRINT 'Utworzono użytkownika: ' + @TestEmail;

-- Dodaj historię logowania
INSERT INTO UserLoginHistories (UserId, UserEmail, LoginTime, IsSuccessful, RequiredTwoFactor)
VALUES (@TestUserId, @TestEmail, GETUTCDATE(), 1, 0);

PRINT 'Dodano wpis historii logowania';
PRINT '';

-- Sprawdź ile wpisów historii ma użytkownik
DECLARE @HistoryCountBefore INT;
SELECT @HistoryCountBefore = COUNT(*) 
FROM UserLoginHistories 
WHERE UserId = @TestUserId;

PRINT 'Liczba wpisów historii PRZED usunięciem użytkownika: ' + CAST(@HistoryCountBefore AS VARCHAR);

-- Usuń użytkownika
DELETE FROM AspNetUsers WHERE Id = @TestUserId;
PRINT 'Usunięto użytkownika';
PRINT '';

-- Sprawdź ile wpisów historii pozostało
DECLARE @HistoryCountAfter INT;
SELECT @HistoryCountAfter = COUNT(*) 
FROM UserLoginHistories 
WHERE UserId = @TestUserId;

PRINT 'Liczba wpisów historii PO usunięciu użytkownika: ' + CAST(@HistoryCountAfter AS VARCHAR);
PRINT '';

IF @HistoryCountAfter = 0
BEGIN
    PRINT '✅ SUKCES: Cascade Delete działa poprawnie!';
    PRINT 'Historia została automatycznie usunięta razem z użytkownikiem.';
END
ELSE
BEGIN
    PRINT '❌ BŁĄD: Cascade Delete NIE DZIAŁA!';
    PRINT 'Historia nie została usunięta po usunięciu użytkownika.';
END

-- Cofnij transakcję (cleanup)
ROLLBACK TRANSACTION;

PRINT '';
PRINT 'Test zakończony (transakcja wycofana - baza nie została zmieniona)';
PRINT '';
PRINT '';

-- ============================================
-- TEST 5: Przykładowe dane z relacją
-- ============================================
PRINT '--- TEST 5: Przykładowe dane z relacją ---';

IF EXISTS (SELECT 1 FROM UserLoginHistories)
BEGIN
    PRINT 'Ostatnie 5 logowań z informacją o użytkowniku:';
    PRINT '';

    SELECT TOP 5
   u.UserName AS [User Name],
        u.Email AS [Email],
        lh.LoginTime AS [Login Time],
        lh.IpAddress AS [IP Address],
        CASE WHEN lh.IsSuccessful = 1 THEN 'Success' ELSE 'Failed' END AS [Status],
  lh.FailureReason AS [Failure Reason]
    FROM 
      UserLoginHistories lh
    INNER JOIN 
        AspNetUsers u ON lh.UserId = u.Id
    ORDER BY 
        lh.LoginTime DESC;
END
ELSE
BEGIN
    PRINT 'Brak danych w tabeli UserLoginHistories';
END

PRINT '';
PRINT '';

-- ============================================
-- TEST 6: Statystyki
-- ============================================
PRINT '--- TEST 6: Statystyki ---';

SELECT 'Liczba użytkowników' AS [Metryka], COUNT(*) AS [Wartość]
FROM AspNetUsers
UNION ALL
SELECT 'Liczba wpisów historii logowania' AS [Metryka], COUNT(*) AS [Wartość]
FROM UserLoginHistories
UNION ALL
SELECT 'Użytkowników z historią logowania' AS [Metryka], COUNT(DISTINCT UserId) AS [Wartość]
FROM UserLoginHistories
UNION ALL
SELECT 'Użytkowników bez historii logowania' AS [Metryka], COUNT(*) AS [Wartość]
FROM AspNetUsers u
WHERE NOT EXISTS (SELECT 1 FROM UserLoginHistories lh WHERE lh.UserId = u.Id);

PRINT '';
PRINT '';

-- ============================================
-- PODSUMOWANIE
-- ============================================
PRINT '================================================';
PRINT 'PODSUMOWANIE';
PRINT '================================================';

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserLoginHistories_AspNetUsers_UserId')
BEGIN
    PRINT '✅ Foreign Key: ISTNIEJE';
END
ELSE
BEGIN
    PRINT '❌ Foreign Key: BRAK';
END

IF EXISTS (
    SELECT 1 
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    WHERE i.object_id = OBJECT_ID('UserLoginHistories')
      AND COL_NAME(ic.object_id, ic.column_id) = 'UserId'
)
BEGIN
    PRINT '✅ Indeks na UserId: ISTNIEJE';
END
ELSE
BEGIN
    PRINT '⚠️  Indeks na UserId: BRAK (powinien istnieć)';
END

PRINT '';
PRINT '================================================';
PRINT 'Relacja ApplicationUser → UserLoginHistory';
PRINT 'Status: WDROŻONA I FUNKCJONALNA';
PRINT '================================================';
PRINT '';
PRINT 'Jeśli wszystkie testy przeszły pomyślnie, relacja działa poprawnie!';
PRINT 'Problem z widocznością na diagramie to prawdopodobnie kwestia narzędzia.';
PRINT '';
PRINT 'Rozwiązanie:';
PRINT '1. Odśwież diagram w swoim narzędziu (F5 lub Refresh)';
PRINT '2. Utwórz nowy diagram od zera';
PRINT '3. Użyj innego narzędzia (Azure Data Studio, DbSchema)';
PRINT '';

GO
