USE krisisdb;

CREATE TABLE Submissions (
    SubmissionId INT AUTO_INCREMENT PRIMARY KEY,
    -- Id til den innloggede brukeren i AspNetUsers (Identity). Ingen fremmednøkkel,
    -- siden AspNetUsers opprettes av migrasjonen etter at denne filen er kjørt.
    UserId INT NULL,
    Tittel VARCHAR(255) NOT NULL,
    Kategori VARCHAR(255) NOT NULL,
    -- "ressurs" (tilbyr hjelp) eller "behov" (trenger hjelp)
    Type VARCHAR(10) NOT NULL DEFAULT 'ressurs',
    -- Prioritet: rod (akutt), gul (planlagt) eller gronn (lav prioritet/tilgjengelig)
    Farge VARCHAR(10) NOT NULL DEFAULT 'gronn',
    Lokasjon VARCHAR(255) NOT NULL,
    PunkterJson TEXT NOT NULL,
    Beskrivelse TEXT,
    Tidspunkt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'ny'
);