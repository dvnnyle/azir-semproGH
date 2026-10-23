USE krisisdb;

CREATE TABLE Submissions (
    SubmissionId INT AUTO_INCREMENT PRIMARY KEY,
    -- Nullable inntil innlogging finnes - settes til den innloggede brukeren senere
    UserId INT NULL,
    CategoryId INT NOT NULL,
    Tittel VARCHAR(255) NOT NULL,
    -- "ressurs" (tilbyr hjelp) eller "behov" (trenger hjelp)
    Type VARCHAR(10) NOT NULL DEFAULT 'ressurs',
    Adresse VARCHAR(255) NOT NULL,
    -- "høy", "middels" eller "lav" - bare for behov. Ressurs er alltid gronn pa
    -- kartet (utledes fra Type), trenger ikke lagres.
    Prioritet VARCHAR(10) NULL,
    Frist DATETIME NULL,
    Beskrivelse TEXT,
    Tidspunkt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'ny',
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
