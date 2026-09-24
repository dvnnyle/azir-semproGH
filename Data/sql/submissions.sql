USE krisisdb;

CREATE TABLE Submissions (
    SubmissionId INT AUTO_INCREMENT PRIMARY KEY,
    -- Nullable inntil innlogging finnes - settes til den innloggede brukeren senere
    UserId INT NULL,
    Tittel VARCHAR(255) NOT NULL,
    Kategori VARCHAR(255) NOT NULL,
    Farge VARCHAR(10) NOT NULL DEFAULT 'gronn',
    Lokasjon VARCHAR(255) NOT NULL,
    PunkterJson TEXT NOT NULL,
    Beskrivelse TEXT,
    Tidspunkt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'ny',
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
