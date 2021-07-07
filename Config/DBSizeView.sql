--==================== DATABASE SIZE VIEW ================================
CREATE VIEW dbo.db_size
AS
SELECT
     -- record cannot be smaller than the forwarding stub size =9 Bytes
    CASE
        WHEN [Max Size] >= 9
            THEN [Max Size]
        ELSE 9
        END AS [Max Size]
     -- record cannot be smaller than the forwarding stub size =9 Bytes
     , CASE
           WHEN [Min Size] >= 9
               THEN [Min Size]
           ELSE 9
    END     AS [Min Size]
     , [Table Name]
     , [Table Type]
     , [Total Number of Columns]
     , [Schema]
FROM (
         SELECT DISTINCT
                       -- Overhead for row header of a data row
                 4
                 +
                 -- Overhead for NULL bitmap
                 2 + cast(([Total Number of Columns] + 7) / 8 AS BIGINT) +
                 -- overhead for variable length
                 CASE
                     WHEN [IsVariableLength] > 0
                         THEN
                         2
                     ELSE
                         0
                     END
                 +
                 --- Sum is on record level
                 SUM(
                             a1.[max_length]
                             +
                             -- Overhead for variable-length columns
                             CASE
                                 WHEN
                                     -- varchar
                                         [System Type] = 'varchar'
                                         --(([system_type_id]=167) AND ([user_type_id]=167))
                                         OR
                                         -- nvarchar 
                                         [System Type] = 'nvarchar'
                                         --(([system_type_id]=231) AND ([user_type_id]=231))
                                         OR
                                         -- IMAGE
                                         (([system_type_id] = 34) OR ([user_type_id] = 34))
                                         OR
                                         -- TEXT
                                         (([system_type_id] = 35) OR ([user_type_id] = 35))
                                         OR
                                         --  NTEXT
                                         (([system_type_id] = 99) OR ([user_type_id] = 99))
                                         OR
                                         --  SQLVARIANT
                                         (([system_type_id] = 98) OR ([user_type_id] = 98))
                                         OR
                                         -- hierarchyid geometry geography
                                         (([system_type_id] = 240))
                                     THEN 2
                                 ELSE 0
                                 END
                     )
                             OVER (PARTITION BY a1.[Schema], a1.[Table Name]) AS [Max Size]

                       , -- Overhead for row header of a data row
                 4
                 +
                 -- Overhead for NULL bitmap
                 2 + cast(([Total Number of Columns] + 7) / 8 AS BIGINT) +
                 -- overhead for variable length
                 CASE
                     WHEN ([IsVariableLength] > 0) AND ([AnyFixedColumn] = 0)
                         THEN
                         2
                     ELSE
                         0
                     END
                 +
                 --- Sum is on record level
                 SUM(
                     -- overhead for variable length depending on number of variable columns
                         CASE
                             WHEN
                                 -- varchar
                                 --[System Type]='varchar'
                                     (([system_type_id] = 167) OR ([user_type_id] = 167))
                                     OR
                                     -- nvarchar 
                                     --[System Type]='nvarchar'
                                     (([system_type_id] = 231) OR ([user_type_id] = 231))
                                     OR
                                     -- IMAGE
                                     (([system_type_id] = 34) OR ([user_type_id] = 34))
                                     OR
                                     -- TEXT
                                     (([system_type_id] = 35) OR ([user_type_id] = 35))
                                     OR
                                     --  NTEXT
                                     (([system_type_id] = 99) OR ([user_type_id] = 99))
                                     --  VARBINARY
                                     OR
                                     (([system_type_id] = 165) OR ([user_type_id] = 165))
                                     OR
                                     --  SQLVARIANT
                                     (([system_type_id] = 98) OR ([user_type_id] = 98))
                                     OR
                                     -- hierarchyid geometry geography
                                     (([system_type_id] = 240))
                                     OR
                                     -- xml
                                     (([system_type_id] = 241))
                                 THEN
                                 CASE
                                     WHEN [Is Nullable] = 1
                                         THEN 0
                                     ELSE
                                         1
                                     END
                             ELSE
                                 CASE
                                     WHEN
                                         -- bit
                                             (([system_type_id] = 104) OR ([user_type_id] = 104))
                                             and [Is Nullable] = 1
                                         THEN 0
                                     ELSE
                                         a1.[max_length]
                                     END
                             END
                     -- 

                     )
                         OVER (PARTITION BY a1.[Schema], a1.[Table Name])     AS [Min Size]
                       , a1.[Table Name]
                       , [Table Type]
                       , [Total Number of Columns]
                       , a1.[Schema]
         FROM
             -- Start a1
             (SELECT (SELECT [name]
                      FROM [sys].[schemas]
                      WHERE [sys].[schemas].[schema_id] = [sys].[objects].[schema_id])
                                                                                                       AS [Schema]
                   , [sys].[objects].[name]                                                            AS [Table Name]
                   , [sys].[all_columns].[name]                                                        AS [Column Name]
                   , [sys].[all_columns].[system_type_id]
                   , (
                     SELECT name
                     FROM [sys].[types]
                     WHERE [sys].[types].[system_type_id] = [sys].[all_columns].[system_type_id]
                       AND [sys].[types].[user_type_id] = [sys].[all_columns].[user_type_id]
                 )                                                                                     AS [System Type]
                   , [sys].[all_columns].[user_type_id]
                   , CASE
                         WHEN
                             -- IMAGE
                             (([system_type_id] = 34) OR ([user_type_id] = 34))
                             THEN 2147483647
                     -- TEXT
                         WHEN (([system_type_id] = 35) OR ([user_type_id] = 35))
                             THEN 2147483647
                     --  NTEXT
                         WHEN (([system_type_id] = 99) OR ([user_type_id] = 99))
                             THEN 1073741823
                     -- varchar(max)
                         WHEN (([system_type_id] = 167) OR ([user_type_id] = 167)) AND
                              ([sys].[all_columns].[max_length] = -1)
                             THEN 2147483647
                     -- nvarchar(max) 
                         WHEN (([system_type_id] = 231) OR ([user_type_id] = 231)) AND
                              ([sys].[all_columns].[max_length] = -1)
                             THEN 2147483647
                     -- varbinary(max)
                         WHEN (([system_type_id] = 165) OR ([user_type_id] = 165)) AND
                              ([sys].[all_columns].[max_length] = -1)
                             THEN 2147483647
                     -- hierarchyid geometry geography
                         WHEN (([system_type_id] = 240))
                             THEN 2147483647
                     -- xml
                         WHEN (([system_type_id] = 241) AND ([sys].[all_columns].[max_length] = -1))
                             THEN 2147483647
                     -- bit
                         WHEN (([system_type_id] = 104) OR ([user_type_id] = 104))
                             THEN 1 / 8
                         ELSE
                             CAST([sys].[all_columns].[max_length] AS BIGINT)
                     END                                                                                  [max_length]
                   , [sys].[all_columns].[is_nullable]                                                 AS [Is Nullable]
                   , CASE
                         WHEN EXISTS
                             (
                                 SELECT type_desc
                                 FROM sys.indexes
                                 WHERE type_desc = 'CLUSTERED'
                                   AND [sys].[objects].[object_id] = [sys].[indexes].[object_id]
                             )
                             THEN 'CLUSTERED'
                         ELSE 'HEAP'
                     END                                                                               AS [Table Type]
                   , COUNT([sys].[all_columns].[name])
                           OVER (PARTITION BY [sys].[objects].[object_id])                             AS [Total Number of Columns]
                   , SUM(CASE
                             WHEN
                                 -- varchar
                                     (
                                             (([system_type_id] = 167) AND ([user_type_id] = 167))
                                             OR
                                             -- nvarchar 
                                             (([system_type_id] = 231) AND ([user_type_id] = 231))
                                         )
                                     AND [sys].[all_columns].[is_nullable] = 0
                                 THEN 1
                             ELSE 0
                             END)
                         OVER (PARTITION BY [sys].[objects].[name])                                    AS [IsNonNullableVariableLength]
                   , SUM(
                         CASE
                             WHEN
                                 -- varchar
                                     (([system_type_id] = 167) OR ([user_type_id] = 167))
                                     OR
                                     -- nvarchar 
                                     (([system_type_id] = 231) OR ([user_type_id] = 231))
                                     OR
                                     -- IMAGE
                                     (([system_type_id] = 34) OR ([user_type_id] = 34))
                                     OR
                                     -- TEXT
                                     (([system_type_id] = 35) OR ([user_type_id] = 35))
                                     OR
                                     --  NTEXT
                                     (([system_type_id] = 99) OR ([user_type_id] = 99))
                                     --  VARBINARY
                                     OR
                                     (([system_type_id] = 165) OR ([user_type_id] = 165))
                                     OR
                                     --  SQLVARIANT
                                     (([system_type_id] = 98) OR ([user_type_id] = 98))
                                     OR
                                     -- hierarchyid geometry geography
                                     (([system_type_id] = 240))
                                     OR
                                     -- xml
                                     (([system_type_id] = 241))
                                 THEN 1
                             ELSE 0
                             END) OVER (PARTITION BY [sys].[objects].[name])
                                                                                                       AS [IsVariableLength]
                   , SUM(
                         CASE
                             WHEN
                                 -- varchar
                                     (([system_type_id] = 167) OR ([user_type_id] = 167))
                                     OR
                                     -- nvarchar 
                                     (([system_type_id] = 231) OR ([user_type_id] = 231))
                                     OR
                                     -- IMAGE
                                     (([system_type_id] = 34) OR ([user_type_id] = 34))
                                     OR
                                     -- TEXT
                                     (([system_type_id] = 35) OR ([user_type_id] = 35))
                                     OR
                                     --  NTEXT
                                     (([system_type_id] = 99) OR ([user_type_id] = 99))
                                     --  VARBINARY
                                     OR
                                     (([system_type_id] = 165) OR ([user_type_id] = 165))
                                     OR
                                     --  SQLVARIANT
                                     (([system_type_id] = 98) OR ([user_type_id] = 98))
                                     OR
                                     -- hierarchyid geometry geography
                                     (([system_type_id] = 240))
                                     OR
                                     -- xml
                                     (([system_type_id] = 241))
                                 THEN 0
                             ELSE 1
                             END) OVER (PARTITION BY [sys].[objects].[name])
                                                                                                       AS [AnyFixedColumn]
              FROM [sys].[objects]
                       INNER JOIN sys.all_columns
                                  ON [sys].[objects].[object_id] = [sys].[all_columns].[object_id]
              WHERE type_desc = 'USER_TABLE'
             ) a1
     ) a2
