-- =============================================================
-- Creación de la base de datos SeguraVida.
-- Idempotente: solo la crea si no existe.
-- Ejecutar primero, antes de 01_schema.sql y 02_seed_plans.sql.
-- =============================================================

IF DB_ID(N'SeguraVida') IS NULL
BEGIN
    CREATE DATABASE [SeguraVida];
END;
GO
