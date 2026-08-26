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
		r.RoleName,
		ud.FirstName,
		ud.SecondName AS LastName,
		ud.BirthDate,
		COUNT(*) OVER() AS TotalRows
	FROM dbo.Users u
	LEFT JOIN dbo.RoleTypes r ON u.RoleTypePK = r.RoleTypePK
	LEFT JOIN dbo.UserData ud ON u.UserPK = ud.UserPK
	+ @Where +
	N' ORDER BY ' + @SortColumn + N' ' + @SortDirection +
	N' OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;';

	EXEC sp_executesql
		@Sql,
		N'@SearchParam NVARCHAR(52), @Offset INT, @PageSize INT',
		@SearchParam = @SearchParam,
		@Offset = @Offset,
		@PageSize = @PageSize;
END;
GO