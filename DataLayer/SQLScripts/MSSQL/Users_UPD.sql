CREATE PROCEDURE dbo.Users_UPD
	@UserPK UNIQUEIDENTIFIER,
	@UserName NVARCHAR(50),
	@Email NVARCHAR(50),
	@Password NVARCHAR(50),
	@ActiveStatus BIT,
	@RoleTypePK UNIQUEIDENTIFIER,
	@FirstName NVARCHAR(50),
	@SecondName NVARCHAR(50) = NULL,
	@BirthDate DATETIME2(7) = NULL,
	@Found BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET @Found = 0;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE dbo.Users
		SET UserName = @UserName, Email = @Email, Password = @Password,
		    ActiveStatus = @ActiveStatus, RoleTypePK = @RoleTypePK
		WHERE UserPK = @UserPK;

		IF @@ROWCOUNT > 0
		BEGIN
			UPDATE dbo.UserData
			SET FirstName = @FirstName, SecondName = @SecondName, BirthDate = @BirthDate
			WHERE UserPK = @UserPK;

			SET @Found = 1;
		END

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO