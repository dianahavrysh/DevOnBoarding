CREATE PROCEDURE dbo.Users_UPD
	@UserPK UNIQUEIDENTIFIER,
	@UserName NVARCHAR(50),
	@Email NVARCHAR(50),
	@Password VARCHAR(50),
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
		UPDATE dbo.Users
		SET
			UserName = @UserName,
			Email = @Email,
			Password = @Password,
			ActiveStatus = @ActiveStatus,
			RoleTypePK = @RoleTypePK
		WHERE UserPK = @UserPK;

		UPDATE dbo.UserData
		SET
			FirstName = @FirstName,
			SecondName = @SecondName,
			BirthDate = @BirthDate
		WHERE UserPK = @UserPK;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO