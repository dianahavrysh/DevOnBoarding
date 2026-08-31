CREATE PROCEDURE dbo.Users_INS
    @UserName NVARCHAR(50),
    @Email NVARCHAR(50),
    @Password NVARCHAR(50),
    @ActiveStatus BIT,
    @RoleTypePK UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(50),
    @SecondName NVARCHAR(50) = NULL,
    @BirthDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @NewUserPK TABLE
        (
            UserPK UNIQUEIDENTIFIER
        );

        INSERT INTO dbo.Users
        (
            UserName,
            Email,
            Password,
            ActiveStatus,
            RoleTypePK
        )
        OUTPUT INSERTED.UserPK INTO @NewUserPK
        VALUES
        (
            @UserName,
            @Email,
            @Password,
            @ActiveStatus,
            @RoleTypePK
        );

        INSERT INTO dbo.UserData
        (
            UserPK,
            FirstName,
            SecondName,
            BirthDate
        )
        SELECT
            UserPK,
            @FirstName,
            @SecondName,
            @BirthDate
        FROM @NewUserPK;

        COMMIT TRANSACTION;

        SELECT UserPK
        FROM @NewUserPK;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO