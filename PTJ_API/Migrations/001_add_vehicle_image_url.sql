-- ============================================================
-- Migration: Add image_url column to vehicle table
-- Run this on the CarManager database
-- ============================================================

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.vehicle') AND name = 'image_url'
)
BEGIN
    ALTER TABLE [dbo].[vehicle] ADD [image_url] NVARCHAR(500) NULL;
    PRINT 'Column image_url added to vehicle table.';
END
ELSE
BEGIN
    PRINT 'Column image_url already exists, skipping.';
END
