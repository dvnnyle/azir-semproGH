USE krisisdb;

-- Matching mellom et behov og en ressurs (case.md). BehovId/RessursId peker
-- begge inn i Submissions - selve skillet (behov vs ressurs) ligger i
-- Submissions.Type, ikke her.
CREATE TABLE Requests (
    RequestId INT AUTO_INCREMENT PRIMARY KEY,
    BehovId INT NOT NULL,
    RessursId INT NOT NULL,
    -- "sendt", "godtatt" eller "avslatt"
    Status VARCHAR(20) NOT NULL DEFAULT 'sendt',
    Melding TEXT,
    Tidspunkt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (BehovId, RessursId),
    FOREIGN KEY (BehovId) REFERENCES Submissions(SubmissionId),
    FOREIGN KEY (RessursId) REFERENCES Submissions(SubmissionId)
);
