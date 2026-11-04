USE krisisdb;
-- Uten denne tolker mysql-klienten init-scriptet med feil tegnsett, og
-- æøå blir lagret dobbelt-kodet (f.eks. "ø" -> "Ã¸" i databasen).
SET NAMES utf8mb4;

INSERT INTO Submissions (CategoryId, Tittel, Type, Adresse, Prioritet, Frist, Beskrivelse, Status) VALUES
(4, 'Strømaggregat til utlån', 'ressurs', 'Vesterveien 8, 4621 Kristiansand', NULL, NULL, 'Ledig aggregat, kan hentes.', 'ny');
SET @id1 = LAST_INSERT_ID();
INSERT INTO Points (SubmissionId, Nr, Breddegrad, Lengdegrad) VALUES (@id1, 0, 58.1472, 7.9950);
