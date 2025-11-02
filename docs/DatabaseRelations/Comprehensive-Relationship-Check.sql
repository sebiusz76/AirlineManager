-- ============================================
-- COMPREHENSIVE RELATIONSHIP CHECK
-- Weryfikacja WSZYSTKICH relacji w AirlineManager
-- ============================================

USE [AirlineManager-Dev]; -- Zmień nazwę bazy jeśli inna
GO

SET NOCOUNT ON;

PRINT '================================================';
PRINT 'SPRAWDZANIE WSZYSTKICH RELACJI W AIRLINEMANAGER';
PRINT '================================================';
PRINT '';
PRINT 'Data wykonania: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '';

-- ============================================
-- CZĘŚĆ 1: PRZEGLĄD WSZYSTKICH FOREIGN KEYS
-- ============================================
PRINT '================================================';
PRINT 'CZĘŚĆ 1: WSZYSTKIE FOREIGN KEYS W BAZIE';
PRINT '================================================';
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
    OBJECT_NAME(fk.parent_object_id) IN (
      'UserLoginHistories',
        'UserSessions',
   'UserAuditLogs',
        'AspNetUserRoles'
  )
ORDER BY 
    OBJECT_NAME(fk.parent_object_id),
    fk.name;

PRINT '';
PRINT '';

-- ============================================
-- CZĘŚĆ 2: SZCZEGÓŁOWA WERYFIKACJA KAŻDEJ RELACJI
-- ============================================
PRINT '================================================';
PRINT 'CZĘŚĆ 2: SZCZEGÓŁOWA WERYFIKACJA RELACJI';
PRINT '================================================';
PRINT '';

-- --------------------------------------------
-- RELACJA 1: ApplicationUser → UserLoginHistory
-- --------------------------------------------
PRINT '--- RELACJA 1: ApplicationUser → UserLoginHistory ---';
PRINT '';

IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_UserLoginHistories_AspNetUsers_UserId'
)
BEGIN
    PRINT '✅ FK_UserLoginHistories_AspNetUsers_UserId: EXISTS';
    
    SELECT 
      'UserLoginHistory' AS [Relation],
     COUNT(*) AS [FK Verified],
  'CASCADE' AS [Expected Delete],
        MAX(fk.delete_referential_action_desc) AS [Actual Delete],
     CASE 
            WHEN MAX(fk.delete_referential_action_desc) = 'CASCADE' THEN '✅ CORRECT'
         ELSE '❌ INCORRECT'
        END AS [Status]
    FROM 
        sys.foreign_keys AS fk
    WHERE 
   fk.name = 'FK_UserLoginHistories_AspNetUsers_UserId'
    GROUP BY fk.name;
END
ELSE
BEGIN
    PRINT '❌ FK_UserLoginHistories_AspNetUsers_UserId: MISSING';
    SELECT 'UserLoginHistory' AS [Relation], 0 AS [FK Verified], '❌ MISSING' AS [Status];
END

PRINT '';

-- --------------------------------------------
-- RELACJA 2: ApplicationUser → UserSession
-- --------------------------------------------
PRINT '--- RELACJA 2: ApplicationUser → UserSession ---';
PRINT '';

IF EXISTS (
    SELECT 1 
 FROM sys.foreign_keys 
    WHERE name = 'FK_UserSessions_AspNetUsers_UserId'
)
BEGIN
    PRINT '✅ FK_UserSessions_AspNetUsers_UserId: EXISTS';
    
    SELECT 
        'UserSession' AS [Relation],
        COUNT(*) AS [FK Verified],
        'CASCADE' AS [Expected Delete],
        MAX(fk.delete_referential_action_desc) AS [Actual Delete],
        CASE 
    WHEN MAX(fk.delete_referential_action_desc) = 'CASCADE' THEN '✅ CORRECT'
    ELSE '❌ INCORRECT'
        END AS [Status]
    FROM 
        sys.foreign_keys AS fk
    WHERE 
        fk.name = 'FK_UserSessions_AspNetUsers_UserId'
    GROUP BY fk.name;
END
ELSE
BEGIN
    PRINT '❌ FK_UserSessions_AspNetUsers_UserId: MISSING';
    SELECT 'UserSession' AS [Relation], 0 AS [FK Verified], '❌ MISSING' AS [Status];
END

PRINT '';

-- --------------------------------------------
-- RELACJA 3: ApplicationUser → UserAuditLog (podmiot)
-- --------------------------------------------
PRINT '--- RELACJA 3: ApplicationUser → UserAuditLog (podmiot) ---';
PRINT '';

IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_UserAuditLogs_AspNetUsers_UserId'
)
BEGIN
    PRINT '✅ FK_UserAuditLogs_AspNetUsers_UserId: EXISTS';
    
    SELECT 
    'UserAuditLog (User)' AS [Relation],
  COUNT(*) AS [FK Verified],
        'CASCADE' AS [Expected Delete],
  MAX(fk.delete_referential_action_desc) AS [Actual Delete],
        CASE 
            WHEN MAX(fk.delete_referential_action_desc) = 'CASCADE' THEN '✅ CORRECT'
      ELSE '❌ INCORRECT'
        END AS [Status]
    FROM 
        sys.foreign_keys AS fk
    WHERE 
     fk.name = 'FK_UserAuditLogs_AspNetUsers_UserId'
    GROUP BY fk.name;
END
ELSE
BEGIN
    PRINT '❌ FK_UserAuditLogs_AspNetUsers_UserId: MISSING';
    SELECT 'UserAuditLog (User)' AS [Relation], 0 AS [FK Verified], '❌ MISSING' AS [Status];
END

PRINT '';

-- --------------------------------------------
-- RELACJA 4: ApplicationUser → UserAuditLog (modyfikator)
-- --------------------------------------------
PRINT '--- RELACJA 4: ApplicationUser → UserAuditLog (modyfikator) ---';
PRINT '';

IF EXISTS (
  SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_UserAuditLogs_AspNetUsers_ModifiedBy'
)
BEGIN
PRINT '✅ FK_UserAuditLogs_AspNetUsers_ModifiedBy: EXISTS';

    SELECT 
        'UserAuditLog (Modifier)' AS [Relation],
        COUNT(*) AS [FK Verified],
   'RESTRICT or NO_ACTION' AS [Expected Delete],
    MAX(fk.delete_referential_action_desc) AS [Actual Delete],
     CASE 
  WHEN MAX(fk.delete_referential_action_desc) IN ('NO_ACTION', 'RESTRICT') THEN '✅ CORRECT'
            ELSE '⚠️ CHECK NEEDED'
        END AS [Status]
    FROM 
   sys.foreign_keys AS fk
    WHERE 
        fk.name = 'FK_UserAuditLogs_AspNetUsers_ModifiedBy'
    GROUP BY fk.name;
END
ELSE
BEGIN
    PRINT '❌ FK_UserAuditLogs_AspNetUsers_ModifiedBy: MISSING';
    SELECT 'UserAuditLog (Modifier)' AS [Relation], 0 AS [FK Verified], '❌ MISSING' AS [Status];
END

PRINT '';

-- --------------------------------------------
-- RELACJA 5: ApplicationUser ↔ IdentityRole (Many-to-Many)
-- --------------------------------------------
PRINT '--- RELACJA 5: ApplicationUser ↔ IdentityRole (Many-to-Many) ---';
PRINT '';

DECLARE @UserRoleFK INT = 0;
DECLARE @RoleUserFK INT = 0;

IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_AspNetUserRoles_AspNetUsers_UserId'
)
BEGIN
  SET @UserRoleFK = 1;
    PRINT '✅ FK_AspNetUserRoles_AspNetUsers_UserId: EXISTS';
END
ELSE
BEGIN
    PRINT '❌ FK_AspNetUserRoles_AspNetUsers_UserId: MISSING';
END

IF EXISTS (
  SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_AspNetUserRoles_AspNetRoles_RoleId'
)
BEGIN
    SET @RoleUserFK = 1;
    PRINT '✅ FK_AspNetUserRoles_AspNetRoles_RoleId: EXISTS';
END
ELSE
BEGIN
    PRINT '❌ FK_AspNetUserRoles_AspNetRoles_RoleId: MISSING';
END

SELECT 
    'IdentityRole (Many-to-Many)' AS [Relation],
    @UserRoleFK + @RoleUserFK AS [FKs Verified],
    2 AS [Expected FKs],
    CASE 
        WHEN @UserRoleFK + @RoleUserFK = 2 THEN '✅ COMPLETE'
        WHEN @UserRoleFK + @RoleUserFK = 1 THEN '⚠️ PARTIAL'
        ELSE '❌ MISSING'
    END AS [Status];

PRINT '';
PRINT '';

-- ============================================
-- CZĘŚĆ 3: TESTY INTEGRALNOŚCI REFERENCYJNEJ
-- ============================================
PRINT '================================================';
PRINT 'CZĘŚĆ 3: TESTY INTEGRALNOŚCI REFERENCYJNEJ';
PRINT '================================================';
PRINT '';

-- Test 1: UserLoginHistory
PRINT '--- TEST 1: UserLoginHistory Foreign Key Constraint ---';
BEGIN TRY
    INSERT INTO UserLoginHistories (UserId, UserEmail, LoginTime, IsSuccessful, RequiredTwoFactor)
    VALUES ('00000000-0000-0000-0000-000000000000', 'invalid@test.com', GETUTCDATE(), 1, 0);
    
  PRINT '❌ FAILED: FK constraint does NOT work - orphan record created';
    DELETE FROM UserLoginHistories WHERE UserId = '00000000-0000-0000-0000-000000000000';
END TRY
BEGIN CATCH
    PRINT '✅ PASSED: FK constraint works correctly';
 PRINT '   Error: ' + ERROR_MESSAGE();
END CATCH

PRINT '';

-- Test 2: UserSession
PRINT '--- TEST 2: UserSession Foreign Key Constraint ---';
BEGIN TRY
    INSERT INTO UserSessions (UserId, UserEmail, SessionId, CreatedAt, LastActivityAt, IsActive, IsPersistent)
    VALUES ('00000000-0000-0000-0000-000000000000', 'invalid@test.com', 'TEST_SESSION', GETUTCDATE(), GETUTCDATE(), 1, 0);
    
    PRINT '❌ FAILED: FK constraint does NOT work - orphan record created';
    DELETE FROM UserSessions WHERE SessionId = 'TEST_SESSION';
END TRY
BEGIN CATCH
    PRINT '✅ PASSED: FK constraint works correctly';
    PRINT '   Error: ' + ERROR_MESSAGE();
END CATCH

PRINT '';

-- Test 3: UserAuditLog (podmiot)
PRINT '--- TEST 3: UserAuditLog (podmiot) Foreign Key Constraint ---';
BEGIN TRY
    INSERT INTO UserAuditLogs (UserId, UserEmail, ModifiedBy, ModifiedByEmail, ModifiedAt, Action)
    VALUES ('00000000-0000-0000-0000-000000000000', 'invalid@test.com', 'admin', 'admin@test.com', GETUTCDATE(), 'TEST');
    
    PRINT '❌ FAILED: FK constraint does NOT work - orphan record created';
    DELETE FROM UserAuditLogs WHERE UserId = '00000000-0000-0000-0000-000000000000';
END TRY
BEGIN CATCH
    PRINT '✅ PASSED: FK constraint works correctly';
    PRINT '   Error: ' + ERROR_MESSAGE();
END CATCH

PRINT '';
PRINT '';

-- ============================================
-- CZĘŚĆ 4: TESTY CASCADE DELETE
-- ============================================
PRINT '================================================';
PRINT 'CZĘŚĆ 4: TESTY CASCADE DELETE';
PRINT '================================================';
PRINT '';
PRINT 'Creating test user and related records...';
PRINT '';

BEGIN TRANSACTION;

DECLARE @TestUserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
DECLARE @TestEmail NVARCHAR(256) = 'cascade.test.comprehensive@airlinemanager.local';

-- Create test user
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, MustChangePassword, PreferredTheme
)
VALUES (
    @TestUserId, @TestEmail, UPPER(@TestEmail), @TestEmail, UPPER(@TestEmail),
    1, 'TEST_HASH', CAST(NEWID() AS NVARCHAR(50)), CAST(NEWID() AS NVARCHAR(50)),
    0, 0, 1, 0, 'Test', 'User', 0, 'auto'
);

PRINT 'Test user created: ' + @TestEmail;

-- Add UserLoginHistory
INSERT INTO UserLoginHistories (UserId, UserEmail, LoginTime, IsSuccessful, RequiredTwoFactor)
VALUES (@TestUserId, @TestEmail, GETUTCDATE(), 1, 0);
PRINT 'UserLoginHistory record created';

-- Add UserSession
INSERT INTO UserSessions (UserId, UserEmail, SessionId, CreatedAt, LastActivityAt, IsActive, IsPersistent)
VALUES (@TestUserId, @TestEmail, 'TEST_SESSION_' + @TestUserId, GETUTCDATE(), GETUTCDATE(), 1, 0);
PRINT 'UserSession record created';

-- Add UserAuditLog (if FK exists)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserAuditLogs_AspNetUsers_UserId')
BEGIN
    INSERT INTO UserAuditLogs (UserId, UserEmail, ModifiedBy, ModifiedByEmail, ModifiedAt, Action)
    VALUES (@TestUserId, @TestEmail, @TestUserId, @TestEmail, GETUTCDATE(), 'TEST_CASCADE');
    PRINT 'UserAuditLog record created';
END

PRINT '';
PRINT 'Counting records BEFORE delete...';

DECLARE @LoginHistoryBefore INT = (SELECT COUNT(*) FROM UserLoginHistories WHERE UserId = @TestUserId);
DECLARE @SessionsBefore INT = (SELECT COUNT(*) FROM UserSessions WHERE UserId = @TestUserId);
DECLARE @AuditLogsBefore INT = (SELECT COUNT(*) FROM UserAuditLogs WHERE UserId = @TestUserId);

PRINT 'UserLoginHistories: ' + CAST(@LoginHistoryBefore AS VARCHAR);
PRINT 'UserSessions: ' + CAST(@SessionsBefore AS VARCHAR);
PRINT 'UserAuditLogs: ' + CAST(@AuditLogsBefore AS VARCHAR);

PRINT '';
PRINT 'Deleting test user...';

-- Delete user (should cascade)
DELETE FROM AspNetUsers WHERE Id = @TestUserId;

PRINT '';
PRINT 'Counting records AFTER delete...';

DECLARE @LoginHistoryAfter INT = (SELECT COUNT(*) FROM UserLoginHistories WHERE UserId = @TestUserId);
DECLARE @SessionsAfter INT = (SELECT COUNT(*) FROM UserSessions WHERE UserId = @TestUserId);
DECLARE @AuditLogsAfter INT = (SELECT COUNT(*) FROM UserAuditLogs WHERE UserId = @TestUserId);

PRINT 'UserLoginHistories: ' + CAST(@LoginHistoryAfter AS VARCHAR);
PRINT 'UserSessions: ' + CAST(@SessionsAfter AS VARCHAR);
PRINT 'UserAuditLogs: ' + CAST(@AuditLogsAfter AS VARCHAR);

PRINT '';
PRINT 'CASCADE DELETE RESULTS:';
PRINT '---';

-- UserLoginHistory
IF @LoginHistoryAfter = 0 AND @LoginHistoryBefore > 0
    PRINT '✅ UserLoginHistory: Cascade Delete WORKS';
ELSE IF @LoginHistoryBefore = 0
    PRINT '⚠️ UserLoginHistory: No test data to verify';
ELSE
    PRINT '❌ UserLoginHistory: Cascade Delete FAILED';

-- UserSession
IF @SessionsAfter = 0 AND @SessionsBefore > 0
 PRINT '✅ UserSession: Cascade Delete WORKS';
ELSE IF @SessionsBefore = 0
    PRINT '⚠️ UserSession: No test data to verify';
ELSE
    PRINT '❌ UserSession: Cascade Delete FAILED';

-- UserAuditLog
IF @AuditLogsAfter = 0 AND @AuditLogsBefore > 0
    PRINT '✅ UserAuditLog: Cascade Delete WORKS';
ELSE IF @AuditLogsBefore = 0
    PRINT '⚠️ UserAuditLog: No FK or no test data';
ELSE
    PRINT '❌ UserAuditLog: Cascade Delete FAILED';

ROLLBACK TRANSACTION;

PRINT '';
PRINT 'Test transaction rolled back (database unchanged)';
PRINT '';
PRINT '';

-- ============================================
-- CZĘŚĆ 5: STATYSTYKI BAZY DANYCH
-- ============================================
PRINT '================================================';
PRINT 'CZĘŚĆ 5: STATYSTYKI BAZY DANYCH';
PRINT '================================================';
PRINT '';

SELECT 
  'Total Users' AS [Metric],
    COUNT(*) AS [Count]
FROM AspNetUsers
UNION ALL
SELECT 
  'Users with LoginHistory',
    COUNT(DISTINCT UserId)
FROM UserLoginHistories
UNION ALL
SELECT 
    'Users with Sessions',
    COUNT(DISTINCT UserId)
FROM UserSessions
UNION ALL
SELECT 
'Users with AuditLogs',
    COUNT(DISTINCT UserId)
FROM UserAuditLogs
UNION ALL
SELECT 
    'Total LoginHistory Records',
    COUNT(*)
FROM UserLoginHistories
UNION ALL
SELECT 
    'Total Active Sessions',
 COUNT(*)
FROM UserSessions
WHERE IsActive = 1
UNION ALL
SELECT 
    'Total AuditLog Records',
    COUNT(*)
FROM UserAuditLogs;

PRINT '';
PRINT '';

-- ============================================
-- CZĘŚĆ 6: PRZYKŁADOWE ZAPYTANIA Z RELACJAMI
-- ============================================
PRINT '================================================';
PRINT 'CZĘŚĆ 6: PRZYKŁADOWE DANE Z RELACJAMI';
PRINT '================================================';
PRINT '';

IF EXISTS (SELECT 1 FROM UserLoginHistories)
BEGIN
    PRINT '--- Last 5 Logins with User Info ---';
    SELECT TOP 5
        u.UserName,
        u.Email,
        lh.LoginTime,
lh.IpAddress,
    CASE WHEN lh.IsSuccessful = 1 THEN 'Success' ELSE 'Failed' END AS [Status]
    FROM 
    UserLoginHistories lh
    INNER JOIN 
     AspNetUsers u ON lh.UserId = u.Id
    ORDER BY 
        lh.LoginTime DESC;
    PRINT '';
END

IF EXISTS (SELECT 1 FROM UserSessions WHERE IsActive = 1)
BEGIN
    PRINT '--- Active Sessions with User Info ---';
    SELECT TOP 5
     u.UserName,
      u.Email,
        s.SessionId,
     s.CreatedAt,
     s.LastActivityAt,
        s.Device
    FROM 
        UserSessions s
    INNER JOIN 
        AspNetUsers u ON s.UserId = u.Id
    WHERE 
    s.IsActive = 1
    ORDER BY 
        s.LastActivityAt DESC;
  PRINT '';
END

IF EXISTS (SELECT 1 FROM UserAuditLogs)
BEGIN
    PRINT '--- Recent Audit Logs with User Info ---';
    SELECT TOP 5
        u.UserName AS [User],
        u.Email AS [User Email],
        al.Action,
 al.ModifiedAt,
 al.ModifiedByEmail AS [Modified By]
    FROM 
        UserAuditLogs al
    INNER JOIN 
     AspNetUsers u ON al.UserId = u.Id
    ORDER BY 
        al.ModifiedAt DESC;
    PRINT '';
END

PRINT '';

-- ============================================
-- CZĘŚĆ 7: PODSUMOWANIE I REKOMENDACJE
-- ============================================
PRINT '================================================';
PRINT 'CZĘŚĆ 7: PODSUMOWANIE';
PRINT '================================================';
PRINT '';

DECLARE @TotalRelations INT = 5;
DECLARE @ImplementedRelations INT = 0;

-- Count implemented relations
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserLoginHistories_AspNetUsers_UserId')
    SET @ImplementedRelations = @ImplementedRelations + 1;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserSessions_AspNetUsers_UserId')
    SET @ImplementedRelations = @ImplementedRelations + 1;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserAuditLogs_AspNetUsers_UserId')
    SET @ImplementedRelations = @ImplementedRelations + 1;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserAuditLogs_AspNetUsers_ModifiedBy')
    SET @ImplementedRelations = @ImplementedRelations + 1;

-- Check for Identity Role FKs (count as 1 relation)
DECLARE @IdentityRoleFKs INT = 0;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AspNetUserRoles_AspNetUsers_UserId')
    SET @IdentityRoleFKs = @IdentityRoleFKs + 1;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AspNetUserRoles_AspNetRoles_RoleId')
    SET @IdentityRoleFKs = @IdentityRoleFKs + 1;
IF @IdentityRoleFKs = 2
    SET @ImplementedRelations = @ImplementedRelations + 1;

PRINT 'IMPLEMENTATION PROGRESS:';
PRINT '---';
PRINT 'Total Relations: ' + CAST(@TotalRelations AS VARCHAR);
PRINT 'Implemented: ' + CAST(@ImplementedRelations AS VARCHAR);
PRINT 'Missing: ' + CAST(@TotalRelations - @ImplementedRelations AS VARCHAR);
PRINT 'Progress: ' + CAST((@ImplementedRelations * 100 / @TotalRelations) AS VARCHAR) + '%';
PRINT '';

PRINT 'STATUS BY RELATION:';
PRINT '---';

-- Relation 1
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserLoginHistories_AspNetUsers_UserId')
    PRINT '1. UserLoginHistory: ✅ IMPLEMENTED';
ELSE
    PRINT '1. UserLoginHistory: ❌ MISSING';

-- Relation 2
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserSessions_AspNetUsers_UserId')
    PRINT '2. UserSession: ✅ IMPLEMENTED';
ELSE
    PRINT '2. UserSession: ❌ MISSING';

-- Relation 3
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserAuditLogs_AspNetUsers_UserId')
    PRINT '3. UserAuditLog (podmiot): ✅ IMPLEMENTED';
ELSE
    PRINT '3. UserAuditLog (podmiot): ❌ MISSING';

-- Relation 4
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserAuditLogs_AspNetUsers_ModifiedBy')
    PRINT '4. UserAuditLog (modyfikator): ✅ IMPLEMENTED';
ELSE
    PRINT '4. UserAuditLog (modyfikator): ❌ MISSING';

-- Relation 5
IF @IdentityRoleFKs = 2
    PRINT '5. IdentityRole (Many-to-Many): ✅ IMPLEMENTED';
ELSE
    PRINT '5. IdentityRole (Many-to-Many): ⚠️ PARTIAL/MISSING';

PRINT '';
PRINT '================================================';
PRINT 'SPRAWDZANIE ZAKOŃCZONE';
PRINT '================================================';
PRINT '';
PRINT 'Aby odświeżyć diagram bazy danych:';
PRINT '1. W SSMS: Prawy przycisk na Database Diagrams → Refresh';
PRINT '2. W Azure Data Studio: F5 (Refresh)';
PRINT '3. Utwórz nowy diagram od zera';
PRINT '';
PRINT 'Jeśli wszystkie testy przeszły pomyślnie, relacje działają poprawnie!';
PRINT '';

GO
