USE krisisdb;

-- litt sånn test data xoxo docker

INSERT INTO Submissions (CategoryId, Tittel, Type, Adresse, Prioritet, Frist, Beskrivelse, Status) VALUES
(4, 'Strømaggregat til utlån', 'ressurs', 'Vesterveien 8, 4621 Kristiansand', NULL, NULL, 'Ledig aggregat, kan hentes.', 'ny');
SET @id1 = LAST_INSERT_ID();
INSERT INTO Points (SubmissionId, Nr, Breddegrad, Lengdegrad) VALUES (@id1, 0, 58.1472, 7.9950);

INSERT INTO Submissions (CategoryId, Tittel, Type, Adresse, Prioritet, Frist, Beskrivelse, Status) VALUES
(4, 'Mangler strøm til eldresenter', 'behov', 'Lundsbroa 2, 4614 Kristiansand', 'høy', '2026-10-25 18:00:00', 'Aggregat trengs raskt.', 'ny');
SET @id2 = LAST_INSERT_ID();
INSERT INTO Points (SubmissionId, Nr, Breddegrad, Lengdegrad) VALUES (@id2, 0, 58.1700, 8.0400);

INSERT INTO Submissions (CategoryId, Tittel, Type, Adresse, Prioritet, Frist, Beskrivelse, Status) VALUES
(7, 'Trenger sandsekker mot flom', 'behov', 'Fiskåveien 15, 4621 Kristiansand', 'middels', '2026-10-28 12:00:00', NULL, 'vurderes');
SET @id3 = LAST_INSERT_ID();
INSERT INTO Points (SubmissionId, Nr, Breddegrad, Lengdegrad) VALUES (@id3, 0, 58.1450, 7.9900);
