-- Ejecutar después de 00_create_database.sql.
USE [SeguraVida];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008211639_InitialCreate'
)
BEGIN
    CREATE TABLE [Plans] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(40) NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [Premium] decimal(18,2) NOT NULL,
        [AvailableQuota] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Plans] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008211639_InitialCreate'
)
BEGIN
    CREATE TABLE [Sales] (
        [Id] uniqueidentifier NOT NULL,
        [AdvisorId] nvarchar(10) NOT NULL,
        [RequestReference] nvarchar(40) NOT NULL,
        [PlanId] int NOT NULL,
        [PlanCode] nvarchar(40) NOT NULL,
        [PlanName] nvarchar(120) NOT NULL,
        [BuyerName] nvarchar(120) NOT NULL,
        [BuyerEmail] nvarchar(200) NOT NULL,
        [Premium] decimal(18,2) NOT NULL,
        [AcceptedAt] datetimeoffset NOT NULL,
        [IsHistorical] bit NOT NULL,
        CONSTRAINT [PK_Sales] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008211639_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Plans_Code] ON [Plans] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008211639_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sales_AdvisorId_AcceptedAt] ON [Sales] ([AdvisorId], [AcceptedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008211639_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sales_AdvisorId_RequestReference] ON [Sales] ([AdvisorId], [RequestReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008211639_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261008211639_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

