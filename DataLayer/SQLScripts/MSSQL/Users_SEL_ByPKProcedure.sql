CREATE PROCEDURE  dbo.Users_SEL_ByPK
	@UserPK INT
AS
BEGIN 
	SET NOCOUNT ON;

	SELECT 
		u.UserPK,
		u.UserName,
		u.Email,
		u.Password,
		u.ActiveStatus,
		u.RoleTypePK,
		ud.FirstName,
		ud.LastName,
		ud.BirthDate
	FROM dbo.Users u
	LEFT JOIN dbo.UserData ud ON u.UserPK = ud.UserPK
	WHERE u.UserPK = @UserPK;
END;
GO