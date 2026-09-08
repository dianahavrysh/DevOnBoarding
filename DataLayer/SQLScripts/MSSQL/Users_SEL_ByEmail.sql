CREATE PROCEDURE Users_SEL_ByEmail
    @Email NVARCHAR(50)
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
        r.RoleName,
        ud.FirstName,
        ud.SecondName,
        ud.BirthDate
    FROM Users u
    INNER JOIN RoleTypes r
        ON u.RoleTypePK = r.RoleTypePK
    INNER JOIN UserData ud
        ON u.UserPK = ud.UserPK
    WHERE u.Email = @Email;
END
GO