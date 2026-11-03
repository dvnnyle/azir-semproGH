USE krisisdb;

SET @owner_column_exists = (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'Submissions'
      AND column_name = 'IdentityUserId'
);
SET @owner_column_ddl = IF(
    @owner_column_exists = 0,
    'ALTER TABLE Submissions ADD COLUMN IdentityUserId VARCHAR(255) NULL',
    'SELECT 1'
);
PREPARE owner_column_statement FROM @owner_column_ddl;
EXECUTE owner_column_statement;
DEALLOCATE PREPARE owner_column_statement;

SET @owner_fk_exists = (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = DATABASE()
      AND table_name = 'Submissions'
      AND constraint_name = 'FK_Submissions_AspNetUsers_IdentityUserId'
);
SET @owner_fk_ddl = IF(
    @owner_fk_exists = 0,
    'ALTER TABLE Submissions ADD CONSTRAINT FK_Submissions_AspNetUsers_IdentityUserId FOREIGN KEY (IdentityUserId) REFERENCES AspNetUsers (Id) ON DELETE SET NULL',
    'SELECT 1'
);
PREPARE owner_fk_statement FROM @owner_fk_ddl;
EXECUTE owner_fk_statement;
DEALLOCATE PREPARE owner_fk_statement;
