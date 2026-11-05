USE krisisdb;
-- Uten denne tolker mysql-klienten init-scriptet med feil tegnsett, og
-- æøå blir lagret dobbelt-kodet (f.eks. "ø" -> "Ã¸" i databasen).
SET NAMES utf8mb4;

INSERT INTO Submissions (CategoryId, Tittel, Type, Adresse, Prioritet, Frist, Beskrivelse, Status) VALUES
(4, 'Strømaggregat til utlån', 'ressurs', 'Vesterveien 8, 4621 Kristiansand', NULL, NULL, 'Ledig aggregat, kan hentes.', 'ny');
SET @id1 = LAST_INSERT_ID();
INSERT INTO Points (SubmissionId, Nr, Breddegrad, Lengdegrad) VALUES (@id1, 0, 58.1472, 7.9950);

INSERT INTO Submissions (CategoryId, Tittel, Type, Adresse, Prioritet, Frist, Beskrivelse, Status) VALUES
(8, 'Trenger drivstoff til generator', 'behov', 'Elvegata 12, 4608 Kristiansand', 'høy', NULL, 'Akutt behov for diesel til nødstrøm.', 'ny');
SET @id2 = LAST_INSERT_ID();
INSERT INTO Points (SubmissionId, Nr, Breddegrad, Lengdegrad) VALUES (@id2, 0, 58.1599, 8.0182);

INSERT INTO Submissions (CategoryId, Tittel, Type, Adresse, Prioritet, Frist, Beskrivelse, Status) VALUES
(6, 'Trenger hjelp til rydding', 'behov', 'Lundsbroa 3, 4635 Kristiansand', 'middels', NULL, 'Søker frivillige til opprydding etter uvær.', 'ny');
SET @id3 = LAST_INSERT_ID();
INSERT INTO Points (SubmissionId, Nr, Breddegrad, Lengdegrad) VALUES (@id3, 0, 58.1467, 8.0025);
