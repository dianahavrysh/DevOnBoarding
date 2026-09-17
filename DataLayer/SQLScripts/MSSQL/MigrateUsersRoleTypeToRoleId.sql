IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
    THROW 50000, 'dbo.Roles already exists. Aborting.', 1;

CREATE TABLE dbo.Roles
(
    Id       TINYINT      NOT NULL PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT CK_Roles_Id CHECK (Id BETWEEN 1 AND 3)
);

INSERT INTO dbo.Roles (Id, RoleName)
VALUES
    (1, 'User'),
    (2, 'Manager'),
    (3, 'Administrator');


IF COL_LENGTH('dbo.Users', 'RoleId') IS NOT NULL
    THROW 50001, 'Users.RoleId already exists. Aborting.', 1;

ALTER TABLE dbo.Users
ADD RoleId TINYINT NULL;
GO


UPDATE u
SET u.RoleId = r.Id
FROM dbo.Users u
JOIN dbo.RoleTypes rt
    ON rt.RoleTypePK = u.RoleTypePK
JOIN dbo.Roles r
    ON r.RoleName = rt.RoleName;


IF EXISTS (SELECT 1 FROM dbo.Users WHERE RoleId IS NULL)
    THROW 50002, 'Migration failed: some users could not be mapped to a role.', 1;


ALTER TABLE dbo.Users
ALTER COLUMN RoleId TINYINT NOT NULL;

ALTER TABLE dbo.Users
ADD CONSTRAINT FK_Users_Roles
    FOREIGN KEY (RoleId)
    REFERENCES dbo.Roles(Id);


DECLARE @RoleTypeFK sysname;
DECLARE @Sql NVARCHAR(MAX);

SELECT @RoleTypeFK = fk.name
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc
    ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns c
    ON c.object_id = fkc.parent_object_id
    AND c.column_id = fkc.parent_column_id
WHERE fk.parent_object_id = OBJECT_ID('dbo.Users')
  AND c.name = 'RoleTypePK';

IF @RoleTypeFK IS NOT NULL
BEGIN
    SET @Sql = N'ALTER TABLE dbo.Users DROP CONSTRAINT ' + QUOTENAME(@RoleTypeFK);
    EXEC sp_executesql @Sql;
END;


ALTER TABLE dbo.Users
DROP COLUMN RoleTypePK;

DROP TABLE dbo.RoleTypes;
GO