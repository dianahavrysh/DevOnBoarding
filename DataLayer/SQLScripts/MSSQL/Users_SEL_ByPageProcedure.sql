CREATE OR ALTER PROCEDURE dbo.Users_SEL_ByPage
	@CurrentPage INT = 1,
	@PageSize INT = 10,
	@SortExpression VARCHAR(100) = 'UserPK ASC',
	@SearchValue VARCHAR(100) = NULL,
	@SearchByUserName BIT = 0, 
	@SearchByEmail BIT = 0, 
	@SearchByFirstName BIT = 0,  
	@SearchBySecondName BIT = 0,   
	@IncludeInactive BIT = 0,
	@StrictMatch BIT = 0,
	@CurrentUserId INT = NULL
AS
BEGIN    
	SET NOCOUNT ON;
    DECLARE @CurrentUserRolePK TINYINT;    
	SELECT @CurrentUserRolePK = RoleTypePK    
	FROM dbo.Users     
	WHERE UserPK = @CurrentUserId;

    DECLARE @SearchPattern VARCHAR(102) = CASE 
		WHEN @SearchValue IS NULL OR @SearchValue = '' THEN NULL 
		WHEN @StrictMatch = 1 THEN @SearchValue        
		ELSE '%' + @SearchValue + '%'    
	END;

    SELECT         
		u.UserPK,
		u.UserName,   
		u.Email,    
		u.ActiveStatus,  
		u.RoleTypePK,    
		ud.FirstName,     
		ud.SecondName,    
		ud.BirthDate,    
		COUNT(*) OVER() AS TotalCount   
	FROM dbo.Users u   
	LEFT JOIN dbo.UserData ud ON u.UserPK = ud.UserPK   
	WHERE            
		(@IncludeInactive = 1 OR u.ActiveStatus = 1)               
	    AND (@CurrentUserRolePK = 1 OR u.RoleTypePK = 2)
        AND (            
			@SearchPattern IS NULL OR (  
				(@SearchByUserName = 1 AND ((@StrictMatch = 1 AND u.UserName = @SearchPattern) OR (@StrictMatch = 0 AND u.UserName LIKE @SearchPattern))) OR                
				(@SearchByEmail = 1 AND ((@StrictMatch = 1 AND u.Email = @SearchPattern) OR (@StrictMatch = 0 AND u.Email LIKE @SearchPattern))) OR               
				(@SearchByFirstName = 1 AND ((@StrictMatch = 1 AND ud.FirstName = @SearchPattern) OR (@StrictMatch = 0 AND ud.FirstName LIKE @SearchPattern))) OR         
				(@SearchBySecondName = 1 AND ((@StrictMatch = 1 AND ud.SecondName = @SearchPattern) OR (@StrictMatch = 0 AND ud.SecondName LIKE @SearchPattern)))            
			)       
		)    
	ORDER BY         
		CASE WHEN @SortExpression = 'UserPK ASC' THEN u.UserPK END ASC,      
		CASE WHEN @SortExpression = 'UserPK DESC' THEN u.UserPK END DESC,   
		CASE WHEN @SortExpression = 'UserName ASC' THEN u.UserName END ASC,   
		CASE WHEN @SortExpression = 'UserName DESC' THEN u.UserName END DESC,  
		CASE WHEN @SortExpression = 'Email ASC' THEN u.Email END ASC,   
		CASE WHEN @SortExpression = 'Email DESC' THEN u.Email END DESC,   
		CASE WHEN @SortExpression = 'FirstName ASC' THEN ud.FirstName END ASC,  
		CASE WHEN @SortExpression = 'FirstName DESC' THEN ud.FirstName END DESC    
	OFFSET (@CurrentPage - 1) * @PageSize ROWS   
	FETCH NEXT @PageSize ROWS ONLY;
END;
GO