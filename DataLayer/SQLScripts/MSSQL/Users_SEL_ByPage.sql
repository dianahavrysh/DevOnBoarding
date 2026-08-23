CREATE PROCEDURE dbo.Users_SEL_ByPage
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		u.UserPK,
		u.UserName,
		u.Email,
		u.Password,
		u.ActiveStatus,
		r.RoleName AS RoleName,
		ud.FirstName,
		ud.SecondName AS LastName,
		ud.BirthDate
	FROM dbo.Users u
	LEFT JOIN dbo.RoleTypes r ON u.RoleTypePK = r.RoleTypePK
	LEFT JOIN dbo.UserData ud ON u.UserPK = ud.UserPK;
END;
GO
