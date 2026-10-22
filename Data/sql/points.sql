USE krisisdb;

CREATE TABLE Points (
    SubmissionId INT NOT NULL,
    -- Rekkefolgen punktet ble tegnet i - avgjor formen nar polygonet tegnes opp igjen
    Nr SMALLINT NOT NULL,
    Breddegrad DECIMAL(9,6) NOT NULL,
    Lengdegrad DECIMAL(9,6) NOT NULL,
    PRIMARY KEY (SubmissionId, Nr),
    FOREIGN KEY (SubmissionId) REFERENCES Submissions(SubmissionId) ON DELETE CASCADE
);
