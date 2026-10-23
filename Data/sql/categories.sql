USE krisisdb;
-- Uten denne tolker mysql-klienten init-scriptet med feil tegnsett, og
-- æøå blir lagret dobbelt-kodet (f.eks. "ø" -> "Ã¸" i databasen).
SET NAMES utf8mb4;

CREATE TABLE Categories (
    CategoryId INT AUTO_INCREMENT PRIMARY KEY,
    Navn VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO Categories (Navn) VALUES
    ('Kjøretøy'),
    ('Maskiner'),
    ('Droneoperatør'),
    ('Strømaggregat'),
    ('Kommunikasjonsutstyr'),
    ('Personell'),
    ('Sand og grus'),
    ('Drivstoff');
