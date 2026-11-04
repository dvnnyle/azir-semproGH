USE krisisdb;

REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'krisisapp'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON krisisdb.* TO 'krisisapp'@'%';
