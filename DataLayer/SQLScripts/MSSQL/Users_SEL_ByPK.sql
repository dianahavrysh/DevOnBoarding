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
		r.RoleName,
		ud.FirstName,
		ud.SecondName AS LastName,
		ud.BirthDate
	FROM dbo.Users u WITH (NOLOCK)
	LEFT JOIN dbo.RoleTypes r WITH (NOLOCK) ON u.RoleTypePK = r.RoleTypePK
	LEFT JOIN dbo.UserData ud WITH (NOLOCK) ON u.UserPK = ud.UserPK
	WHERE u.UserPK = @UserPK;
END;
GO