CREATE PROCEDURE  dbo.Users_INS
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
		INSERT INTO dbo.Users (UserName, Email, Password, ActiveStatus, RoleTypePK)
		VALUES (@UserName, @Email, @Password, @ActiveStatus, @RoleTypePK);

		SET @NewUserPK = SCOPE_IDENTITY();

		INSERT INTO dbo.UserData (UserPK, FirstName, SecondName, BirthDate) 
		VALUES (@NewUserPK, @FirstName, @SecondName, @BirthDate);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH 
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO