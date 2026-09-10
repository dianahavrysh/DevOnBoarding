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
    FROM Users AS u
    JOIN RoleTypes AS r
        ON u.RoleTypePK = r.RoleTypePK
    JOIN UserData AS ud
        ON u.UserPK = ud.UserPK
    WHERE u.Email = @Email;
END
GO