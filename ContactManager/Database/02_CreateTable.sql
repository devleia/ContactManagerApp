USE ContactManagerDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Contacts')
BEGIN
    CREATE TABLE Contacts (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        Name          NVARCHAR(200)   NOT NULL,
        DateOfBirth   DATE            NOT NULL,
        Married       BIT             NOT NULL,
        Phone         NVARCHAR(50)    NOT NULL,
        Salary        DECIMAL(18, 2)  NOT NULL
    );
END
GO