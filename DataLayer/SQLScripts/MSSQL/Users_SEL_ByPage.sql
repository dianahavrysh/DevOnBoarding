ALTER PROCEDURE dbo.Users_SEL_ByPage
	@RequestingUserRole NVARCHAR(50),
	@CurrentPage INT,
	@PageSize INT,
	@SortExpression NVARCHAR(100),
	@SearchValue NVARCHAR(50),
	@SearchByUserName BIT,
	@SearchByEmail BIT,
	@SearchByFirstName BIT,
	@SearchBySecondName BIT,
	@IncludeInactive BIT,
	@StrictMatch BIT
AS
BEGIN
	SET NOCOUNT ON;

	IF @CurrentPage IS NULL OR @CurrentPage < 1
		SET @CurrentPage = 1;

	IF @PageSize IS NULL OR @PageSize < 1
		SET @PageSize = 20;

	IF @RequestingUserRole IS NULL OR LEN(LTRIM(RTRIM(@RequestingUserRole))) = 0
	BEGIN
		RAISERROR('Requesting user role is required.', 16, 1);
		RETURN;
	END;

	DECLARE @StartRow INT = (@CurrentPage - 1) * @PageSize + 1;
	DECLARE @EndRow INT = @StartRow + @PageSize - 1;

	-- NOTE: the previous lookup of the requesting user's role via a
	-- Users/RoleTypes JOIN has been removed. The role is now trusted
	-- from the caller (it was already validated as part of the JWT).
	-- This saves one full table scan/join on every paged request.

	DECLARE @SortField NVARCHAR(50);
	DECLARE @SortDirRaw NVARCHAR(10);
	DECLARE @SpacePos INT;

	IF @SortExpression IS NOT NULL
		AND LEN(LTRIM(RTRIM(@SortExpression))) > 0
	BEGIN
		SET @SortExpression = LTRIM(RTRIM(@SortExpression));
		SET @SpacePos = CHARINDEX(' ', @SortExpression);

		IF @SpacePos > 0
		BEGIN
			SET @SortField = LEFT(@SortExpression, @SpacePos - 1);
			SET @SortDirRaw = LTRIM(SUBSTRING(@SortExpression, @SpacePos + 1, 10));
		END
		ELSE
		BEGIN
			SET @SortField = @SortExpression;
			SET @SortDirRaw = 'ASC';
		END;
	END
	ELSE
	BEGIN
		SET @SortField = 'UserName';
		SET @SortDirRaw = 'ASC';
	END;

	DECLARE @IsFilterUsed BIT =
		CASE
			WHEN @SearchValue IS NOT NULL
				AND LEN(@SearchValue) > 0
				AND (
					@SearchByUserName = 1
					OR @SearchByEmail = 1
					OR @SearchByFirstName = 1
					OR @SearchBySecondName = 1
				)
			THEN 1
			ELSE 0
		END;

	DECLARE @EscapedSearchValue NVARCHAR(150) =
		REPLACE(REPLACE(REPLACE(@SearchValue, '[', '[[]'), '%', '[%]'), '_', '[_]');

	DECLARE @LikeValue NVARCHAR(154) = CONCAT(N'%', @EscapedSearchValue, N'%');

	;WITH PagedUsers AS
	(
		SELECT
			u.UserPK,
			u.UserName,
			u.Email,
			u.Password,
			u.ActiveStatus,
			u.RoleId,
			r.RoleName,
			ud.FirstName,
			ud.SecondName,
			ud.BirthDate,
			COUNT(*) OVER() AS TotalRows,

			ROW_NUMBER() OVER (
				ORDER BY
					CASE
						WHEN @SortField = 'UserName'
							AND @SortDirRaw <> 'DESC'
						THEN u.UserName
					END ASC,

					CASE
						WHEN @SortField = 'UserName'
							AND @SortDirRaw = 'DESC'
						THEN u.UserName
					END DESC,

					CASE
						WHEN @SortField = 'Email'
							AND @SortDirRaw <> 'DESC'
						THEN u.Email
					END ASC,

					CASE
						WHEN @SortField = 'Email'
							AND @SortDirRaw = 'DESC'
						THEN u.Email
					END DESC,

					CASE
						WHEN @SortField = 'ActiveStatus'
							AND @SortDirRaw <> 'DESC'
						THEN u.ActiveStatus
					END ASC,

					CASE
						WHEN @SortField = 'ActiveStatus'
							AND @SortDirRaw = 'DESC'
						THEN u.ActiveStatus
					END DESC,

					CASE
						WHEN @SortField = 'RoleName'
							AND @SortDirRaw <> 'DESC'
						THEN r.RoleName
					END ASC,

					CASE
						WHEN @SortField = 'RoleName'
							AND @SortDirRaw = 'DESC'
						THEN r.RoleName
					END DESC,

					CASE
						WHEN @SortField = 'FirstName'
							AND @SortDirRaw <> 'DESC'
						THEN ud.FirstName
					END ASC,

					CASE
						WHEN @SortField = 'FirstName'
							AND @SortDirRaw = 'DESC'
						THEN ud.FirstName
					END DESC,

					CASE
						WHEN @SortField = 'SecondName'
							AND @SortDirRaw <> 'DESC'
						THEN ud.SecondName
					END ASC,

					CASE
						WHEN @SortField = 'SecondName'
							AND @SortDirRaw = 'DESC'
						THEN ud.SecondName
					END DESC,

					CASE
						WHEN @SortField = 'BirthDate'
							AND @SortDirRaw <> 'DESC'
						THEN ud.BirthDate
					END ASC,

					CASE
						WHEN @SortField = 'BirthDate'
							AND @SortDirRaw = 'DESC'
						THEN ud.BirthDate
					END DESC,

					u.UserName ASC
				) AS RowNum

		FROM dbo.Users u WITH (NOLOCK)
		LEFT JOIN dbo.Roles r WITH (NOLOCK)
			ON u.RoleId = r.Id
		LEFT JOIN dbo.UserData ud WITH (NOLOCK)
			ON u.UserPK = ud.UserPK
		WHERE
			(@IncludeInactive = 1 OR u.ActiveStatus = 1)
			AND (
				@RequestingUserRole = 'Administrator'
				OR (@RequestingUserRole = 'Manager' AND r.RoleName IN ('Manager', 'User'))
				OR (@RequestingUserRole = 'User' AND r.RoleName = 'User')
			)
			AND (
				@IsFilterUsed = 0
				OR (
					@IsFilterUsed = 1
					AND (
						(
							@SearchByUserName = 1
							AND (
								(@StrictMatch = 1 AND u.UserName = @SearchValue)
								OR (@StrictMatch = 0 AND u.UserName LIKE @LikeValue ESCAPE '[')
							)
						)
						OR (
							@SearchByEmail = 1
							AND (
								(@StrictMatch = 1 AND u.Email = @SearchValue)
								OR (@StrictMatch = 0 AND u.Email LIKE @LikeValue ESCAPE '[')
							)
						)
						OR (
							@SearchByFirstName = 1
							AND (
								(@StrictMatch = 1 AND ud.FirstName = @SearchValue)
								OR (@StrictMatch = 0 AND ud.FirstName LIKE @LikeValue ESCAPE '[')
							)
						)
						OR (
							@SearchBySecondName = 1
							AND (
								(@StrictMatch = 1 AND ud.SecondName = @SearchValue)
								OR (@StrictMatch = 0 AND ud.SecondName LIKE @LikeValue ESCAPE '[')
							)
						)
					)
				)
			)
	)
	SELECT
		UserPK,
		UserName,
		Email,
		Password,
		ActiveStatus,
		RoleId,
		RoleName,
		FirstName,
		SecondName,
		BirthDate,
		TotalRows
	FROM PagedUsers
	WHERE RowNum BETWEEN @StartRow AND @EndRow
	ORDER BY RowNum;
END;
GO