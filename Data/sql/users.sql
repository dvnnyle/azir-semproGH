USE krisisdb;

-- HUSK: Submissions.UserId er nullable inntil innlogging finnes (se Data/sql/submissions.sql
-- og TODO-en i FormController.Submit()). Når auth er pa plass, sett Submissions.UserId til
-- den innloggede brukerens UserId ved innsending, i stedet for a la den sta null.

CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(255) NOT NULL,
    LastName VARCHAR(255) NOT NULL,
    Phone VARCHAR(30) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE
);