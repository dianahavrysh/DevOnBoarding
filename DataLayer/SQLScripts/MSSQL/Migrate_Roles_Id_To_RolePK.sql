USE [DevOnBoarding];
GO

-- Add Roles.RolePK

ALTER TABLE dbo.Roles
ADD RolePK TINYINT NULL;
GO


-- Copy Id -> RolePK

UPDATE dbo.Roles
SET RolePK = Id;
GO


-- Validate

IF EXISTS (
    SELECT 1
    FROM dbo.Roles
    WHERE RolePK IS NULL
)
    THROW 50001, 'Migration failed: Roles.RolePK contains NULL values.', 1;
GO


--Make RolePK NOT NULL

ALTER TABLE dbo.Roles
ALTER COLUMN RolePK TINYINT NOT NULL;
GO


-- Remove old CHECK constraint

ALTER TABLE dbo.Roles
DROP CONSTRAINT CK_Roles_Id;
GO


-- Drop old PK

ALTER TABLE dbo.Roles
DROP CONSTRAINT PK__Roles__3214EC079A27632F;
GO


-- Create new PK

ALTER TABLE dbo.Roles
ADD CONSTRAINT PK_Roles
    PRIMARY KEY (RolePK);
GO


-- Remove old Id column

ALTER TABLE dbo.Roles
DROP COLUMN Id;
GO


-- Create FK Users.RolePK -> Roles.RolePK

ALTER TABLE dbo.Users
ADD CONSTRAINT FK_Users_Roles
    FOREIGN KEY (RolePK)
    REFERENCES dbo.Roles(RolePK);
GO