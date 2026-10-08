-- =============================================================
-- Datos iniciales de la campaña (planes de SeguraVida).
-- Idempotente: solo inserta si el plan no existe.
-- El seed de ejecución de la API hace lo mismo; este script
-- permite cargar los planes manualmente sobre una BD limpia.
-- Ejecutar después de 01_schema.sql.
-- =============================================================

USE [SeguraVida];
GO

IF NOT EXISTS (SELECT 1 FROM [Plans] WHERE [Code] = N'ACC_ESENCIAL')
BEGIN
    INSERT INTO [Plans] ([Code], [Name], [Premium], [AvailableQuota])
    VALUES (N'ACC_ESENCIAL', N'Accidente Esencial', 50000.00, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Plans] WHERE [Code] = N'ACC_PLUS')
BEGIN
    INSERT INTO [Plans] ([Code], [Name], [Premium], [AvailableQuota])
    VALUES (N'ACC_PLUS', N'Accidente Plus', 90000.00, 5);
END;
GO
