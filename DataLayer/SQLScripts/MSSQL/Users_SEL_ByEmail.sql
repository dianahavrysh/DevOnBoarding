CREATE PROCEDURE dbo.Users_SEL_ByEmail
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
        u.RolePK,
        r.RoleName,
        ud.FirstName,
        ud.SecondName,
        ud.BirthDate
    FROM dbo.Users AS u
    JOIN dbo.Roles AS r
        ON u.RolePK = r.RolePK
    JOIN dbo.UserData AS ud
        ON u.UserPK = ud.UserPK
    WHERE u.Email = @Email;
END
GO