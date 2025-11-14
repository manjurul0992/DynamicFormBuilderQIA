-- Database Schema
CREATE DATABASE DynamicFormBuilderDB;
GO

USE DynamicFormBuilderDB;
GO

-- Table: Forms
CREATE TABLE Forms (
    FormId INT PRIMARY KEY IDENTITY(1,1),
    FormTitle NVARCHAR(500) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Table: FormFields
CREATE TABLE FormFields (
    FieldId INT PRIMARY KEY IDENTITY(1,1),
    FormId INT NOT NULL,
    FieldLabel NVARCHAR(200) NOT NULL,
    FieldLevel INT NOT NULL, -- Level 1, Level 2, etc.
    IsRequired BIT DEFAULT 0,
    SelectedOption NVARCHAR(100),
    DisplayOrder INT DEFAULT 0,
    FOREIGN KEY (FormId) REFERENCES Forms(FormId) ON DELETE CASCADE
);
GO

-- Table: FieldOptions (Master data for dropdown options)
CREATE TABLE FieldOptions (
    OptionId INT PRIMARY KEY IDENTITY(1,1),
    OptionText NVARCHAR(100) NOT NULL,
    OptionValue NVARCHAR(100) NOT NULL
);
GO

-- Insert sample options
INSERT INTO FieldOptions (OptionText, OptionValue) VALUES 
('Option 1', 'option1'),
('Option 2', 'option2'),
('Option 3', 'option3'),
('Option 4', 'option4'),
('Option 5', 'option5');
GO

-- ============================================
-- Stored Procedures
-- ============================================

-- SP: Get All Field Options
CREATE PROCEDURE sp_GetAllFieldOptions
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OptionId, OptionText, OptionValue 
    FROM FieldOptions 
    ORDER BY OptionId;
END
GO

-- SP: Save Form with Fields
CREATE PROCEDURE sp_SaveForm
    @FormTitle NVARCHAR(500),
    @FormFieldsJson NVARCHAR(MAX),
    @FormId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        INSERT INTO Forms (FormTitle, CreatedDate, ModifiedDate)
        VALUES (@FormTitle, GETDATE(), GETDATE());
        
        SET @FormId = SCOPE_IDENTITY();
        
        INSERT INTO FormFields (FormId, FieldLabel, FieldLevel, IsRequired, SelectedOption, DisplayOrder)
        SELECT 
            @FormId,
            FieldLabel,
            FieldLevel,
            IsRequired,
            SelectedOption,
            DisplayOrder
        FROM OPENJSON(@FormFieldsJson)
        WITH (
            FieldLabel NVARCHAR(200) '$.FieldLabel',
            FieldLevel INT '$.FieldLevel',
            IsRequired BIT '$.IsRequired', 
            SelectedOption NVARCHAR(100) '$.SelectedOption', 
            DisplayOrder INT '$.DisplayOrder' 
        );
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
--CREATE PROCEDURE sp_SaveForm
--    @FormTitle NVARCHAR(500),
--    @FormFieldsJson NVARCHAR(MAX),
--    @FormId INT OUTPUT
--AS
--BEGIN
--    SET NOCOUNT ON;
--    BEGIN TRANSACTION;
    
--    BEGIN TRY
--        -- Insert Form
--        INSERT INTO Forms (FormTitle, CreatedDate, ModifiedDate)
--        VALUES (@FormTitle, GETDATE(), GETDATE());
        
--        SET @FormId = SCOPE_IDENTITY();
        
--        -- Insert Form Fields from JSON
--        INSERT INTO FormFields (FormId, FieldLabel, FieldLevel, IsRequired, SelectedOption, DisplayOrder)
--        SELECT 
--            @FormId,
--            FieldLabel,
--            FieldLevel,
--            IsRequired,
--            SelectedOption,
--            DisplayOrder
--        FROM OPENJSON(@FormFieldsJson)
--        WITH (
--            FieldLabel NVARCHAR(200) '$.fieldLabel',
--            FieldLevel INT '$.fieldLevel',
--            IsRequired BIT '$.isRequired',
--            SelectedOption NVARCHAR(100) '$.selectedOption',
--            DisplayOrder INT '$.displayOrder'
--        );
        
--        COMMIT TRANSACTION;
--    END TRY
--    BEGIN CATCH
--        ROLLBACK TRANSACTION;
--        THROW;
--    END CATCH
--END
--GO

-- SP: Get All Forms with Pagination
CREATE PROCEDURE sp_GetAllForms
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchValue NVARCHAR(500) = '',
    @SortColumn NVARCHAR(50) = 'FormId',
    @SortDirection NVARCHAR(4) = 'DESC',
    @TotalRecords INT OUTPUT,
    @FilteredRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get total records
    SELECT @TotalRecords = COUNT(*) FROM Forms;
    
    -- Create temp table for filtered results
    IF @SearchValue = '' OR @SearchValue IS NULL
    BEGIN
        SELECT @FilteredRecords = @TotalRecords;
        
        -- Return paginated results
        SELECT 
            FormId,
            FormTitle,
            CreatedDate,
            ModifiedDate,
            (SELECT COUNT(*) FROM FormFields WHERE FormId = f.FormId) AS FieldCount
        FROM Forms f
        ORDER BY 
            CASE WHEN @SortColumn = 'FormId' AND @SortDirection = 'ASC' THEN FormId END ASC,
            CASE WHEN @SortColumn = 'FormId' AND @SortDirection = 'DESC' THEN FormId END DESC,
            CASE WHEN @SortColumn = 'FormTitle' AND @SortDirection = 'ASC' THEN FormTitle END ASC,
            CASE WHEN @SortColumn = 'FormTitle' AND @SortDirection = 'DESC' THEN FormTitle END DESC,
            CASE WHEN @SortColumn = 'CreatedDate' AND @SortDirection = 'ASC' THEN CreatedDate END ASC,
            CASE WHEN @SortColumn = 'CreatedDate' AND @SortDirection = 'DESC' THEN CreatedDate END DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END
    ELSE
    BEGIN
        SELECT @FilteredRecords = COUNT(*) 
        FROM Forms 
        WHERE FormTitle LIKE '%' + @SearchValue + '%';
        
        -- Return filtered and paginated results
        SELECT 
            FormId,
            FormTitle,
            CreatedDate,
            ModifiedDate,
            (SELECT COUNT(*) FROM FormFields WHERE FormId = f.FormId) AS FieldCount
        FROM Forms f
        WHERE FormTitle LIKE '%' + @SearchValue + '%'
        ORDER BY 
            CASE WHEN @SortColumn = 'FormId' AND @SortDirection = 'ASC' THEN FormId END ASC,
            CASE WHEN @SortColumn = 'FormId' AND @SortDirection = 'DESC' THEN FormId END DESC,
            CASE WHEN @SortColumn = 'FormTitle' AND @SortDirection = 'ASC' THEN FormTitle END ASC,
            CASE WHEN @SortColumn = 'FormTitle' AND @SortDirection = 'DESC' THEN FormTitle END DESC,
            CASE WHEN @SortColumn = 'CreatedDate' AND @SortDirection = 'ASC' THEN CreatedDate END ASC,
            CASE WHEN @SortColumn = 'CreatedDate' AND @SortDirection = 'DESC' THEN CreatedDate END DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END
END
GO

-- SP: Get Form By Id with Fields
CREATE PROCEDURE sp_GetFormById
    @FormId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get Form Details
    SELECT FormId, FormTitle, CreatedDate, ModifiedDate
    FROM Forms
    WHERE FormId = @FormId;
    
    -- Get Form Fields
    SELECT 
        FieldId,
        FormId,
        FieldLabel,
        FieldLevel,
        IsRequired,
        SelectedOption,
        DisplayOrder
    FROM FormFields
    WHERE FormId = @FormId
    ORDER BY DisplayOrder;
END
GO

-- SP: Delete Form
CREATE PROCEDURE sp_DeleteForm
    @FormId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Forms WHERE FormId = @FormId;
END
GO

select * from Forms
select * from FormFields
select * from FieldOptions