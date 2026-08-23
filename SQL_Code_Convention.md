<!-- Do not rename this file. -->

# SQL Code convention

This document contains SQL code conventions for IOS Envi project.

For SQL Server and T-SQL performance-sensitive work, also follow [docs/code-quality/SQL Server Performance Best Practices.md](docs/code-quality/SQL%20Server%20Performance%20Best%20Practices.md).

## General

1. When naming database components, the author must use the following naming standards:

	1. When having logical noun/verb language separations, component has to be named by using Upper ***CamelCase*** style. E.g. a column for a user's date of birth would be named `DateOfBirth`;

	2. A table must be named according to the following naming convention: `MMIS_<tablename>`. Table names will be defined in the plural. This is because a table is itself a collection of 1 or more row entities (***plural***), and this naming convention is also better understood in the object-oriented world;

	3. Foreign key column names will be defined in the ***singular***;

	4. A primary key constraint must be named according to the following naming convention: `PK_<tablename>` (`PK_MMIS_UserOrganizations`);

	5. A unique constraint will be switched to unique non-clustered index, according to the following naming convention: `UX_<tablename>_<key_columns>` (`UX_MMIS_UserOrganizations_UserPK_OrganizationPK`);

	6. A foreign key constraint must be named according to the following naming convention: `FK_<thistablename>_<relatedtablename>` (`FK_MMIS_UserOrganizations_MMIS_Users`);

	7. A stored procedure must be named according to the following naming convention: `<tablename>_<operation>_<specificsuffix>` (`MMIS_UserOrganizations_SEL_Login`), where operation is: **SEL**, **UPD**, **INS**, **DEL**;

	8. A table-valued function must be named according to the same code conventions as a stored procedure;

	9. A scalar valued function must be named according to the following naming convention: `MMIS_<verb>_<shortdescription>` (`MMIS_Get_VendorAPNumber`).

2. For database objects names in code use only schema plus object name, do not hardcode server and database names in your code.

3. `SELECT *` should be avoided.

4. Use an asterisk (*) only in an archiving situation, where rows are being moved to another table that must have the same structure.

5. Always use aliases `AS` in `SELECT` queries.

6. No square brackets [] and reserved words in object names and alias, use only Latin symbols [A-z] and numeric [0-9].

7. All finished expressions should have a semicolon ; at the end.

8. Keywords should be in ***UPPERCASE***: `SELECT`, `FROM`, `GROUP BY`, etc. This increases the readability of the code.

9. When more than one logical operator is used always to use parentheses, even when they are not required.

10. Drop temporary tables at the end of the stored procedure.

11. Comments:

	1. All Tables must be commented;

	2. Column comments are optional but encouraged. They should be used whenever there is a possibility it might help in understanding;

	3. Column comments must be declared immediately preceding the column definition.

12. Column types declarations must be on the same line as the column name declaration.

13. Each Variables definition can be started from the new line, but also all variables of the same type can be declared in a single line.

14. Variable or column type definition should be in ***UPPERCASE***.

15. Before any changes to the Database structure, there should be verification whether a new/modified element already exists.

16. All specific dictionary values inserting should be after verification whether such data already exists.

17. Don't use column numbers in the `ORDER BY` clause.

18. Always use a list of columns in your `INSERT` statements.

19. All stored procedures should be run via DB Updater.

20. Use JSON as an input parameter instead of UDT.

21. Explicit statement that MSSQL is canonical and MySQL mirrors MSSQL with binary GUID adaptations.

22. Unique constraint naming set to `U_<TableName>` (original uses `UX_<TableName>_<key_columns>` as a unique non-clustered index).

23. Avoid `DISTINCT`, use `GROUP BY`; avoid `UNION`, use `UNION ALL`.

## T-SQL

1. All select should have `WITH(NOLOCK)` hint.

2. All code should be implemented without a loop `CURSOR`, `WHILE`, `CTE` (except JSON reader, complex execution - Submit SP, etc.).

3. `USE DATABASE` should be avoided.

4. Call `MMIS_DropIndex_ByColumnName_TableName` SP in case you should delete or change the type of column.

5. Avoid the use of `SELECT...INTO` for production code, use instead `CREATE TABLE + INSERT INTO ... ` approach. More details here: https://www.red-gate.com/hub/product-learning/sql-prompt/use-selectinto-statement.

6. Sometimes it is better to use a table variable and create an index on it to improve performance.

```sql
DECLARE @TableName TABLE (ItemPK UNIQUEIDENTIFIER INDEX IX_Item CLUSTERED)
INSERT INTO @TableName SELECT ...
```

8. Recommended to use

	| **Not Recommended** | **Recommended**   | **When and Why** |
	| --- | --- | --- |
	| [!=](https://docs.microsoft.com/sql/t-sql/language-elements/comparison-operators-transact-sql) | [<>](https://docs.microsoft.com/sql/t-sql/language-elements/comparison-operators-transact-sql) | `<>` is [ANSI](http://standards.iso.org/ittf/PubliclyAvailableStandards/c053681_ISO_IEC_9075-1_2011.zip), `!=` not ANSI; [both behave identically](https://dba.stackexchange.com/a/155670/107045). |
	| [CONVERT](https://docs.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql) | [CAST](https://docs.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql) | `CAST` is [ANSI](http://standards.iso.org/ittf/PubliclyAvailableStandards/c053681_ISO_IEC_9075-1_2011.zip). |
	| [ISNULL](https://docs.microsoft.com/sql/t-sql/functions/isnull-transact-sql) | [COALESCE](https://docs.microsoft.com/sql/t-sql/language-elements/coalesce-transact-sql) | `COALESCE` is [ANSI](http://standards.iso.org/ittf/PubliclyAvailableStandards/c053681_ISO_IEC_9075-1_2011.zip) and supports multiple arguments; `ISNULL` can implicitly trim strings. |
	| [DATEDIFF](https://docs.microsoft.com/en-us/sql/t-sql/functions/datediff-transact-sql) | [DATEADD](https://docs.microsoft.com/en-us/sql/t-sql/functions/dateadd-transact-sql) | Predicates like `MyDateTime < DATEADD(SECOND, -1, GETUTCDATE())` are [SARGable](https://www.sqlshack.com/how-to-use-sargable-expressions-in-t-sql-queries-performance-advantages-and-examples/). |
	| [SELECT (assign)](https://docs.microsoft.com/en-gb/sql/t-sql/language-elements/select-local-variable-transact-sql) | [SET (assign)](https://docs.microsoft.com/en-gb/sql/t-sql/language-elements/set-local-variable-transact-sql) | Use `SET` ([ANSI](http://standards.iso.org/ittf/PubliclyAvailableStandards/c053681_ISO_IEC_9075-1_2011.zip)) for variable assignment to correctly handle "Subquery returned more than 1 value". |
	| [STR](https://docs.microsoft.com/en-us/sql/t-sql/functions/str-transact-sql) | [CAST](https://docs.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql) | `STR` is not [ANSI](http://standards.iso.org/ittf/PubliclyAvailableStandards/c053681_ISO_IEC_9075-1_2011.zip), slow, limited to 15 digits, and can round; prefer `CAST` and concatenation. |
	| [ISNUMERIC](https://docs.microsoft.com/en-us/sql/t-sql/functions/isnumeric-transact-sql) | [TRY_CONVERT](https://docs.microsoft.com/en-us/sql/t-sql/functions/try-convert-transact-sql) | `ISNUMERIC` may cause conversion errors; on SQL Server < 2012, use `WHERE` with `LIKE`. |

9. Recommended to use temporary tables `#tempTable` over table variables.

## MySQL

1. Template of stored procedure. As delimiter you can use `$$`.

```sql
DROP PROCEDURE IF EXISTS <Stored_Procedure_Name>;

DELIMITER $$

CREATE PROCEDURE <Stored_Procedure_Name>

<list_of_input_output_prameters>

BEGIN

<stored_procedure_body>

END;
```

2. Always use aliases `AS` in `SELECT` queries, and when we have the same input parameter and row in the table.

3. Avoid using the `dbo` schema. In MySQL, there is no way to declare the owner of the table. ie. `dbo`.

4. When migrating column types adhere to these constants (T-SQL to MySQL):

	1. UNIQUEIDENTIFIER = BINARY(16)

	2. DATETIME2(7) = DATETIME(6)

	3. BIT or BIT(1) = TINYINT(1)

	4. NVARCHAR or VARCHAR = VARCHAR of equal length (remember about specifying collations)
		Example:	NVARCHAR(100) = VARCHAR(100)
					VARCHAR(500) = VARCHAR(500)

	5. NUMERIC or DECIMAL = DECIMAL

	6. IMAGE = LOGBLOB

	7. any length restrictions not covered here from T-SQL must be reflected in MySQL

5. Name of stored procedure, function, table etc should be less than 64 symbols.

6. Naming of local variables should start with `l_`.

7. *_TmpCall stored procedures are used to mimic the table valued functions in MsSql. The temp table is created inside such SPs and they are to be called from other SPs:

	1. `*_TmpCall` stored procedure should receive incoming parameters with binary type.

	2. Columns with PKs in temp table created inside `*_TmpCall` stored procedure should be of binary type.

	3. The temp table created inside `*_TmpCall` stored procedure must be dropped in the outer procedure.

8. Use .mysql file extension.

9. If a temp table is referenced multiple times in one statement, duplicate it (avoid can't reopen temp table error).

10. Stored procedures containing SELECT statements should contain isolation level `SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;` set. Until another level is necessary.

## Best practices

1. Preference for temporary tables over nested subselects/CTEs and not adding INDEX to temporary tables.

2. Prefer native `ROW_NUMBER` for legacy pagination.

3. Prefer `JOIN` + `WHERE` over correlated `EXISTS`.

4. JSON input parsing pattern: use `JSON_TABLE` for parsing JSON array inputs. 

```sql
INSERT INTO temp_table 
	(
		Col1,
		Col2,
		ColPK
	)
SELECT  jt.Col1,
        jt.Col2,
        MMIS_UuidToBin(jt.ColPK, 0)
FROM    JSON_TABLE(
            jsonVar,
            '$[*]' COLUMNS (
                Col1  VARCHAR(50) PATH '$.Col1',
                Col2  VARCHAR(50) PATH '$.Col2',
                ColPK CHAR(36)    PATH '$.ColPK'
            )
        ) AS jt;
```

5. Dictionary/static inserts with NOT EXISTS.

6. Existence checks before create/drop/alter.

7. GUID storage:
	- MSSQL uses UNIQUEIDENTIFIER; MySQL stores as BINARY(16) via helpers:
		- `MMIS_UuidToBin(uuid_char, swap_flag)`
		- `MMIS_BinToUuid(binary(16), swap_flag)`

	- Use swap_flag = 1 when generating new PK (`uuid()`), 0 for converting external incoming values.

	- Binary GUID local variable naming: `l_paramNameBin` (e.g., `l_facilityPKBin`).

## Script Templates

- For the existing table schema change use `ALTER_TABLE.sql` file as template:
    - Set transaction to false in `DBVersions.config` for corresponding scripts when using this template.
    - Provide corresponding table names to the `IF EXISTS` guard.
    - When shrinking a column is required, replace the default CDC block with `CDCForTableAlteringWithColumnShrinking.sql` as template; fill table name (`@tableName`), column name (`@columnName`), new size (`@newSize`), and DB version (`@dbVersion`).
    - When dropping/modifying a column is required, replace the default CDC block with `CDCForTableAlteringWithKeyDrop.sql` as template; fill table name (`@tableName`) and DB version (`@dbVersion`).
- For the new table creation use `CREATE_TABLE.sql` as template:
    - Set transaction to false in `DBVersions.config` for corresponding scripts when using this template.
    - Fill the `NOT EXISTS` guard with corresponding table names; provide table name (`@tableName`) and DB version (`@dbVersion`).
- For the table removal use `DROP_TABLE.sql` as template:
    - Fill the `NOT EXISTS` guard and table name; provide `@tableName`.
    - The CDC disablement and replication cleanup must be performed before the `DROP TABLE`.
- For the Stored Procedures deployment/update use `MMIS_MarketplaceAuthorizedOrganizations_INS.sql` as example pattern under ProgrammabilityScripts, not a schema template; highlight idempotent inserts and perform temp-table cleanup if needed.

## Pay attention

1. `LEFT JOIN` can cause duplicates items, rows.

2. Temporary table names should be different in nested SPs.