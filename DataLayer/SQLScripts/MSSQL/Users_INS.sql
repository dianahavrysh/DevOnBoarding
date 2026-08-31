CREATE PROCEDURE dbo.Users_INS
    @UserName NVARCHAR(50),
    @Email NVARCHAR(50),
    @Password NVARCHAR(50),
    @ActiveStatus BIT,
    @RoleTypePK UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(50),
    @SecondName NVARCHAR(50) = NULL,
    @BirthDate DATETIME2(7) = NULL,
    @NewUserPK UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @NewUser TABLE
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
        OUTPUT INSERTED.UserPK INTO @NewUser(UserPK)
        VALUES
        (
            @UserName,
            @Email,
            @Password,
            @ActiveStatus,
            @RoleTypePK
        );

        SELECT @NewUserPK = UserPK
        FROM @NewUser;

        INSERT INTO dbo.UserData
        (
            UserPK,
            FirstName,
            SecondName,
            BirthDate
        )
        VALUES
        (
            @NewUserPK,
            @FirstName,
            @SecondName,
            @BirthDate
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO