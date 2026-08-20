CREATE PROCEDURE  dbo.Users_SEL_ByPage
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
	LEFT JOIN dbo.UserData ud ON u.UserPK = ud.UserPK;
END;
GO