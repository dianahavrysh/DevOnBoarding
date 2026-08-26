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
		DECLARE @InsertedIds TABLE (UserPK UNIQUEIDENTIFIER);
		DECLARE @NewUserPK UNIQUEIDENTIFIER;

		INSERT INTO dbo.Users
			(UserName, Email, Password, ActiveStatus, RoleTypePK)
		OUTPUT INSERTED.UserPK INTO @InsertedIds
		VALUES
			(@UserName, @Email, @Password, @ActiveStatus, @RoleTypePK);

		SELECT @NewUserPK = UserPK
		FROM @InsertedIds;

		INSERT INTO dbo.UserData
			(UserPK, FirstName, SecondName, BirthDate)
		VALUES
			(@NewUserPK, @FirstName, @SecondName, @BirthDate);

		COMMIT TRANSACTION;

		SELECT @NewUserPK AS NewUserPK;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END;
GO