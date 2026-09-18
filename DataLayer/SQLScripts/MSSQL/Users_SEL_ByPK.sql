CREATE PROCEDURE dbo.Users_SEL_ByPK
	@UserPK UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		u.UserPK,
		u.UserName,
		u.Email,
		u.Password,
		u.ActiveStatus,
		u.RolePK,
		r.RoleName AS RoleName,
		ud.FirstName,
		ud.SecondName,
		ud.BirthDate
	FROM dbo.Users u
	LEFT JOIN dbo.Roles r ON u.RolePK = r.RolePK
	LEFT JOIN dbo.UserData ud ON u.UserPK = ud.UserPK
	WHERE u.UserPK = @UserPK;
END;
GO