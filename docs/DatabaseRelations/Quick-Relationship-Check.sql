-- ============================================
-- QUICK RELATIONSHIP CHECK
-- Szybkie sprawdzenie wszystkich relacji
-- ============================================

USE [AirlineManager-Dev];
GO

PRINT '?? QUICK RELATIONSHIP CHECK';
PRINT '==========================';
PRINT '';

-- Check all Foreign Keys
SELECT 
    CASE 
        WHEN fk.name = 'FK_UserLoginHistories_AspNetUsers_UserId' THEN '1??  UserLoginHistory'
        WHEN fk.name = 'FK_UserSessions_AspNetUsers_UserId' THEN '2??  UserSession'
  WHEN fk.name = 'FK_UserAuditLogs_AspNetUsers_UserId' THEN '3??  UserAuditLog (User)'
     WHEN fk.name = 'FK_UserAuditLogs_AspNetUsers_ModifiedBy' THEN '4??  UserAuditLog (Modifier)'
        WHEN fk.name = 'FK_AspNetUserRoles_AspNetUsers_UserId' THEN '5??  UserRoles (User?Role)'
        WHEN fk.name = 'FK_AspNetUserRoles_AspNetRoles_RoleId' THEN '5??  UserRoles (Role?User)'
        ELSE fk.name
    END AS [Relation],
    '?' AS [Status],
    fk.delete_referential_action_desc AS [On Delete],
    OBJECT_NAME(fk.parent_object_id) + '.' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS [FK Column],
    OBJECT_NAME(fk.referenced_object_id) + '.' + COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS [References]
FROM 
    sys.foreign_keys AS fk
INNER JOIN 
    sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
WHERE 
  fk.name IN (
     'FK_UserLoginHistories_AspNetUsers_UserId',
        'FK_UserSessions_AspNetUsers_UserId',
        'FK_UserAuditLogs_AspNetUsers_UserId',
   'FK_UserAuditLogs_AspNetUsers_ModifiedBy',
        'FK_AspNetUserRoles_AspNetUsers_UserId',
        'FK_AspNetUserRoles_AspNetRoles_RoleId'
    )
ORDER BY 
    fk.name;

PRINT '';
PRINT '?? Summary:';
PRINT '-----------';

DECLARE @Implemented INT = (
    SELECT COUNT(*)
FROM sys.foreign_keys
    WHERE name IN (
        'FK_UserLoginHistories_AspNetUsers_UserId',
     'FK_UserSessions_AspNetUsers_UserId',
     'FK_UserAuditLogs_AspNetUsers_UserId',
        'FK_UserAuditLogs_AspNetUsers_ModifiedBy',
        'FK_AspNetUserRoles_AspNetUsers_UserId',
        'FK_AspNetUserRoles_AspNetRoles_RoleId'
    )
);

PRINT 'Implemented: ' + CAST(@Implemented AS VARCHAR) + '/6 Foreign Keys';
PRINT 'Progress: ' + CAST((@Implemented * 100 / 6) AS VARCHAR) + '%';
PRINT '';

-- Check what's missing
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserLoginHistories_AspNetUsers_UserId')
    PRINT '? MISSING: UserLoginHistory relation';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserSessions_AspNetUsers_UserId')
    PRINT '? MISSING: UserSession relation';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserAuditLogs_AspNetUsers_UserId')
 PRINT '? MISSING: UserAuditLog (User) relation';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserAuditLogs_AspNetUsers_ModifiedBy')
    PRINT '? MISSING: UserAuditLog (Modifier) relation';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AspNetUserRoles_AspNetUsers_UserId')
    PRINT '? MISSING: AspNetUserRoles (User) relation';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AspNetUserRoles_AspNetRoles_RoleId')
    PRINT '? MISSING: AspNetUserRoles (Role) relation';

IF @Implemented = 6
    PRINT '? All relations implemented!';

GO
