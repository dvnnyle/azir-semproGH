USE krisisdb;

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
