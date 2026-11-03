USE krisisdb;

-- Denne eldre Users-tabellen er ikke ASP.NET Identity sin AspNetUsers-tabell.
-- Innlogging bruker AspNetUsers; Submission.UserId forblir null til de to
-- brukermodellene eventuelt kobles sammen med en egen databaseskjemaendring.

CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    RoleId INT NOT NULL,
    FirstName VARCHAR(255) NOT NULL,
    LastName VARCHAR(255) NOT NULL,
    Phone VARCHAR(30) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);