-- Kjøres én gang på databaser som ble opprettet før Kommune-kolonnen kom.
-- Nye databaser får kolonnen fra submissions.sql.
USE krisisdb;

ALTER TABLE Submissions ADD COLUMN IF NOT EXISTS Kommune VARCHAR(100) NULL AFTER Adresse;
