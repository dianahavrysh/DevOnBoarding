ALTER PROCEDURE dbo.Users_INS
    @UserName NVARCHAR(50),
    @Email NVARCHAR(50),
    @Password NVARCHAR(50),
    @ActiveStatus BIT,
    @RoleTypePK UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(50),
    @SecondName NVARCHAR(50),
    @BirthDate DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @UserPK UNIQUEIDENTIFIER = NEWID();

        INSERT INTO dbo.Users
        (
            UserPK,
            UserName,
            Email,
            Password,
            ActiveStatus,
            RoleTypePK
        )
        VALUES
        (
            @UserPK,
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
        VALUES
        (
            @UserPK,
            @FirstName,
            @SecondName,
            @BirthDate
        );

        COMMIT TRANSACTION;

        SELECT @UserPK AS NewUserPK;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO