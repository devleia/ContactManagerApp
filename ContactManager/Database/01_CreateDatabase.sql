IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'ContactManagerDb')
BEGIN
    CREATE DATABASE ContactManagerDb;
END
GO