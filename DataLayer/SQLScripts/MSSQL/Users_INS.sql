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
		SET @NewUserPK = NEWSEQUENTIALID();

		INSERT INTO dbo.Users (UserPK, UserName, Email, Password, ActiveStatus, RoleTypePK)
		VALUES (@NewUserPK, @UserName, @Email, @Password, @ActiveStatus, @RoleTypePK);

		INSERT INTO dbo.UserData (UserPK, FirstName, SecondName, BirthDate)
		VALUES (@NewUserPK, @FirstName, @SecondName, @BirthDate);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO
