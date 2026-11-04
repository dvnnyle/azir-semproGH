USE krisisdb;

CREATE TABLE IF NOT EXISTS ResourceDrafts (
    ResourceDraftId INT AUTO_INCREMENT PRIMARY KEY,
    IdentityUserId INT NOT NULL,
    DataJson LONGTEXT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    INDEX IX_ResourceDrafts_IdentityUserId_UpdatedAt (IdentityUserId, UpdatedAt),
    CONSTRAINT FK_ResourceDrafts_AspNetUsers_IdentityUserId
        FOREIGN KEY (IdentityUserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
