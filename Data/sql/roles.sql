USE krisisdb;
SET NAMES utf8mb4;

CREATE TABLE Roles (
    RoleId INT AUTO_INCREMENT PRIMARY KEY,
    Navn VARCHAR(50) NOT NULL UNIQUE,
    -- "offentlig", "privat" eller "admin" - styrer tilgang
    Gruppe VARCHAR(20) NOT NULL
);

INSERT INTO Roles (Navn, Gruppe) VALUES
    ('Admin', 'admin'),
    ('Heimevernet', 'offentlig'),
    ('Privatperson', 'privat'),
    ('Bedrift', 'privat');
