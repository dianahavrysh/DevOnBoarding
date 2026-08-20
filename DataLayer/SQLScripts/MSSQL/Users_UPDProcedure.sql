CREATE PROCEDURE  dbo.Users_UPD
	@UserPK INT,
	@UserName VARCHAR(50),
	@Email VARCHAR(50),
	@Password VARCHAR(50),
	@ActiveStatus BIT,
	@RoleTypePK TINYINT,
	@FirstName VARCHAR(50),
	@SecondName VARCHAR(50) = NULL,
	@BirthDate DATETIME2(7) = NULL,
	@NewUserPK INT OUTPUT
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

		IF EXISTS (SELECT 1 FROM dbo.UserData WHERE UserPK = @UserPK)
		BEGIN
			UPDATE dbo.UserData 
			SET
				FirstName = @FirstName,
				SecondName = @SecondName,
				BirthDate = @BirthDate
			WHERE UserPK = @UserPK;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.UserData (UserPK, FirstName, SecondName, BirthDate) 
			VALUES (@UserPK, @FirstName, @SecondName, @BirthDate);
		END

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH 
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO