CREATE PROCEDURE dbo.Users_SEL_ByPage
	@RequestingUserPK UNIQUEIDENTIFIER,
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

	DECLARE @StartRow INT = (@CurrentPage - 1) * @PageSize + 1;
	DECLARE @EndRow INT = @StartRow + @PageSize - 1;

	DECLARE @RequestingRoleName VARCHAR(50);

	SELECT @RequestingRoleName = r.RoleName
	FROM dbo.Users u WITH (NOLOCK)
	JOIN dbo.RoleTypes r WITH (NOLOCK)
		ON r.RoleTypePK = u.RoleTypePK
	WHERE u.UserPK = @RequestingUserPK;

	IF @RequestingRoleName IS NULL
	BEGIN
		RAISERROR('Requesting user not found.', 16, 1);
		RETURN;
	END;

	DECLARE @IsAdmin BIT =
		CASE
			WHEN @RequestingRoleName = 'Administrator' THEN 1
			ELSE 0
		END;

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

	DECLARE @Where NVARCHAR(MAX) = N' WHERE 1 = 1 ';
	DECLARE @SearchParam NVARCHAR(52) = NULL;

	IF @IncludeInactive = 0
		SET @Where += N' AND u.ActiveStatus = 1 ';

	IF @IsAdmin = 0
		SET @Where += N' AND r.RoleName <> ''Administrator'' ';

	IF @SearchValue IS NOT NULL
		AND LEN(@SearchValue) > 0
		AND (
			@SearchByUserName = 1
			OR @SearchByEmail = 1
			OR @SearchByFirstName = 1
			OR @SearchBySecondName = 1
		)
	BEGIN
		DECLARE @Op NVARCHAR(10) =
			CASE
				WHEN @StrictMatch = 1 THEN N'='
				ELSE N'LIKE'
			END;

		DECLARE @SearchConditions NVARCHAR(MAX) = N'';

		IF @SearchByUserName = 1
			SET @SearchConditions +=
				N' OR u.UserName ' + @Op + N' @SearchParam';

		IF @SearchByEmail = 1
			SET @SearchConditions +=
				N' OR u.Email ' + @Op + N' @SearchParam';

		IF @SearchByFirstName = 1
			SET @SearchConditions +=
				N' OR ud.FirstName ' + @Op + N' @SearchParam';

		IF @SearchBySecondName = 1
			SET @SearchConditions +=
				N' OR ud.SecondName ' + @Op + N' @SearchParam';

		SET @SearchConditions =
			STUFF(@SearchConditions, 1, 4, N'');

		SET @Where +=
			N' AND (' + @SearchConditions + N') ';

		SET @SearchParam =
			CASE
				WHEN @StrictMatch = 1 THEN @SearchValue
				ELSE N'%' + @SearchValue + N'%'
			END;
	END;

	DECLARE @Sql NVARCHAR(MAX) = N'
	;WITH PagedUsers AS
	(
		SELECT
			u.UserPK,
			u.UserName,
			u.Email,
			u.Password,
			u.ActiveStatus,
			u.RoleTypePK,
			r.RoleName,
			ud.FirstName,
			ud.SecondName AS LastName,
			ud.BirthDate,
			COUNT(*) OVER() AS TotalRows,

			ROW_NUMBER() OVER (
				ORDER BY
					CASE
						WHEN @SortField = ''UserName''
							AND @SortDirRaw <> ''DESC''
						THEN u.UserName
					END ASC,

					CASE
						WHEN @SortField = ''UserName''
							AND @SortDirRaw = ''DESC''
						THEN u.UserName
					END DESC,

					CASE
						WHEN @SortField = ''Email''
							AND @SortDirRaw <> ''DESC''
						THEN u.Email
					END ASC,

					CASE
						WHEN @SortField = ''Email''
							AND @SortDirRaw = ''DESC''
						THEN u.Email
					END DESC,

					CASE
						WHEN @SortField = ''ActiveStatus''
							AND @SortDirRaw <> ''DESC''
						THEN u.ActiveStatus
					END ASC,

					CASE
						WHEN @SortField = ''ActiveStatus''
							AND @SortDirRaw = ''DESC''
						THEN u.ActiveStatus
					END DESC,

					CASE
						WHEN @SortField = ''RoleName''
							AND @SortDirRaw <> ''DESC''
						THEN r.RoleName
					END ASC,

					CASE
						WHEN @SortField = ''RoleName''
							AND @SortDirRaw = ''DESC''
						THEN r.RoleName
					END DESC,

					CASE
						WHEN @SortField = ''FirstName''
							AND @SortDirRaw <> ''DESC''
						THEN ud.FirstName
					END ASC,

					CASE
						WHEN @SortField = ''FirstName''
							AND @SortDirRaw = ''DESC''
						THEN ud.FirstName
					END DESC,

					CASE
						WHEN @SortField = ''SecondName''
							AND @SortDirRaw <> ''DESC''
						THEN ud.SecondName
					END ASC,

					CASE
						WHEN @SortField = ''SecondName''
							AND @SortDirRaw = ''DESC''
						THEN ud.SecondName
					END DESC,

					CASE
						WHEN @SortField = ''BirthDate''
							AND @SortDirRaw <> ''DESC''
						THEN ud.BirthDate
					END ASC,

					CASE
						WHEN @SortField = ''BirthDate''
							AND @SortDirRaw = ''DESC''
						THEN ud.BirthDate
					END DESC,

					u.UserName ASC
			) AS RowNum

		FROM dbo.Users u WITH (NOLOCK)
		LEFT JOIN dbo.RoleTypes r WITH (NOLOCK)
			ON u.RoleTypePK = r.RoleTypePK
		LEFT JOIN dbo.UserData ud WITH (NOLOCK)
			ON u.UserPK = ud.UserPK'
		+ @Where +
	N'
	)
	SELECT
		UserPK,
		UserName,
		Email,
		Password,
		ActiveStatus,
		RoleTypePK,
		RoleName,
		FirstName,
		LastName,
		BirthDate,
		TotalRows
	FROM PagedUsers
	WHERE RowNum BETWEEN @StartRow AND @EndRow
	ORDER BY RowNum;';

	EXEC sp_executesql
		@Sql,
		N'@SearchParam NVARCHAR(52),
		  @StartRow INT,
		  @EndRow INT,
		  @SortField NVARCHAR(50),
		  @SortDirRaw NVARCHAR(10)',
		@SearchParam = @SearchParam,
		@StartRow = @StartRow,
		@EndRow = @EndRow,
		@SortField = @SortField,
		@SortDirRaw = @SortDirRaw;
END;
GO