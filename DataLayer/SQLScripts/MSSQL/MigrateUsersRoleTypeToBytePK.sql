USE [DevOnBoarding];
GO

IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
    THROW 50000, 'dbo.Roles already exists. Aborting.', 1;
GO

CREATE TABLE dbo.Roles
(
    RolePK   TINYINT      NOT NULL,
    RoleName NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_Roles
        PRIMARY KEY (RolePK),

    CONSTRAINT UQ_Roles_RoleName
        UNIQUE (RoleName)
);
GO

INSERT INTO dbo.Roles (RolePK, RoleName)
VALUES
    (1, 'User'),
    (2, 'Manager'),
    (3, 'Administrator');
GO


IF COL_LENGTH('dbo.Users', 'RolePK') IS NOT NULL
    THROW 50001, 'Users.RolePK already exists. Aborting.', 1;
GO

ALTER TABLE dbo.Users
ADD RolePK TINYINT NULL;
GO


UPDATE u
SET u.RolePK = r.RolePK
FROM dbo.Users u
JOIN dbo.RoleTypes rt
    ON rt.RoleTypePK = u.RoleTypePK
JOIN dbo.Roles r
    ON r.RoleName = rt.RoleName;
GO


IF EXISTS (
    SELECT 1
    FROM dbo.Users
    WHERE RolePK IS NULL
)
    THROW 50002, 'Migration failed: some users could not be mapped to a role.', 1;
GO


ALTER TABLE dbo.Users
ALTER COLUMN RolePK TINYINT NOT NULL;
GO


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
    SET @Sql =
        N'ALTER TABLE dbo.Users DROP CONSTRAINT '
        + QUOTENAME(@RoleTypeFK);

    EXEC sp_executesql @Sql;
END;
GO


ALTER TABLE dbo.Users
ADD CONSTRAINT FK_Users_Roles
    FOREIGN KEY (RolePK)
    REFERENCES dbo.Roles(RolePK);
GO


ALTER TABLE dbo.Users
DROP COLUMN RoleTypePK;
GO

DROP TABLE dbo.RoleTypes;
GO