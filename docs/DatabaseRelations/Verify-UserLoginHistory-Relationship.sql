-- ============================================
-- Weryfikacja relacji ApplicationUser → UserLoginHistory
-- ============================================

-- 1. Sprawdź czy Foreign Key istnieje
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTableName,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumnName,
    fk.delete_referential_action_desc AS DeleteAction,
    fk.update_referential_action_desc AS UpdateAction
FROM 
    sys.foreign_keys AS fk
INNER JOIN 
    sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
WHERE 
    OBJECT_NAME(fk.parent_object_id) = 'UserLoginHistories';

-- Oczekiwany wynik:
-- ForeignKeyName: FK_UserLoginHistories_AspNetUsers_UserId
-- TableName: UserLoginHistories
-- ColumnName: UserId
-- ReferencedTableName: AspNetUsers
-- ReferencedColumnName: Id
-- DeleteAction: CASCADE
-- UpdateAction: NO_ACTION

-- ============================================

-- 2. Sprawdź indeksy na kolumnie UserId
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM 
    sys.indexes AS i
INNER JOIN 
    sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE 
    i.object_id = OBJECT_ID('UserLoginHistories')
    AND COL_NAME(ic.object_id, ic.column_id) = 'UserId';

-- Oczekiwany wynik: Powinien istnieć indeks na kolumnie UserId

-- ============================================

-- 3. Test integralności - próba dodania wpisu z nieistniejącym UserId (powinno się nie udać)
BEGIN TRY
    -- To zapytanie POWINNO ZWRÓCIĆ BŁĄD jeśli relacja działa poprawnie
    INSERT INTO UserLoginHistories (UserId, UserEmail, LoginTime, IsSuccessful, RequiredTwoFactor)
    VALUES ('00000000-0000-0000-0000-000000000000', 'test@test.com', GETUTCDATE(), 1, 0);
    
PRINT 'UWAGA: Relacja NIE działa poprawnie - można dodać wpis z nieistniejącym UserId!';
    
    -- Usuń testowy wpis jeśli się udało go dodać
    DELETE FROM UserLoginHistories WHERE UserId = '00000000-0000-0000-0000-000000000000';
END TRY
BEGIN CATCH
  PRINT 'OK: Relacja działa poprawnie - nie można dodać wpisu z nieistniejącym UserId';
    PRINT 'Błąd: ' + ERROR_MESSAGE();
END CATCH;

-- ============================================

-- 4. Test Cascade Delete - sprawdź czy usunięcie użytkownika usuwa jego historię
BEGIN TRANSACTION;

    -- Utwórz testowego użytkownika
    DECLARE @TestUserId NVARCHAR(450) = NEWID();
    
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
      PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
          FirstName, LastName, MustChangePassword, PreferredTheme)
    VALUES (@TestUserId, 'testuser@cascade.com', 'TESTUSER@CASCADE.COM', 'testuser@cascade.com', 'TESTUSER@CASCADE.COM',
            1, 'DUMMY_HASH', NEWID(), NEWID(),
  0, 0, 1, 0,
      'Test', 'User', 0, 'auto');
    
    -- Dodaj wpis historii logowania
    INSERT INTO UserLoginHistories (UserId, UserEmail, LoginTime, IsSuccessful, RequiredTwoFactor)
    VALUES (@TestUserId, 'testuser@cascade.com', GETUTCDATE(), 1, 0);
    
    -- Sprawdź czy wpis został dodany
    DECLARE @LoginHistoryCount INT;
    SELECT @LoginHistoryCount = COUNT(*) FROM UserLoginHistories WHERE UserId = @TestUserId;
PRINT 'Liczba wpisów historii przed usunięciem użytkownika: ' + CAST(@LoginHistoryCount AS VARCHAR);
    
    -- Usuń użytkownika
    DELETE FROM AspNetUsers WHERE Id = @TestUserId;
  
 -- Sprawdź czy historia została automatycznie usunięta (CASCADE DELETE)
    SELECT @LoginHistoryCount = COUNT(*) FROM UserLoginHistories WHERE UserId = @TestUserId;
    PRINT 'Liczba wpisów historii po usunięciu użytkownika: ' + CAST(@LoginHistoryCount AS VARCHAR);
    
    IF @LoginHistoryCount = 0
    PRINT 'OK: Cascade Delete działa poprawnie!';
    ELSE
      PRINT 'UWAGA: Cascade Delete NIE działa - historia nie została usunięta!';

ROLLBACK TRANSACTION;

-- ============================================

-- 5. Pokaż przykładowe dane z relacją
SELECT TOP 10
    u.UserName,
    u.Email,
    u.FirstName,
    u.LastName,
    lh.LoginTime,
    lh.IpAddress,
    lh.Browser,
    lh.IsSuccessful,
    lh.FailureReason
FROM 
    AspNetUsers u
INNER JOIN 
    UserLoginHistories lh ON u.Id = lh.UserId
ORDER BY 
    lh.LoginTime DESC;

-- ============================================

-- 6. Statystyki relacji
SELECT 
    'Liczba użytkowników' AS Metryka,
    COUNT(*) AS Wartość
FROM AspNetUsers
UNION ALL
SELECT 
    'Liczba wpisów historii logowania' AS Metryka,
    COUNT(*) AS Wartość
FROM UserLoginHistories
UNION ALL
SELECT 
    'Użytkowników z historią logowania' AS Metryka,
    COUNT(DISTINCT UserId) AS Wartość
FROM UserLoginHistories
UNION ALL
SELECT 
    'Użytkowników bez historii logowania' AS Metryka,
    COUNT(*) AS Wartość
FROM AspNetUsers u
WHERE NOT EXISTS (SELECT 1 FROM UserLoginHistories lh WHERE lh.UserId = u.Id);

-- ============================================
-- KONIEC SKRYPTU WERYFIKACJI
-- ============================================
