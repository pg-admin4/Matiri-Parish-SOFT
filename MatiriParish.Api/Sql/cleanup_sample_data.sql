-- Cleanup sample data created by the seeder
-- WARNING: Run on non-production environment only. Review before executing.

BEGIN;

-- Delete baptism records tagged as sample
DELETE FROM "Baptisms" WHERE "Remarks" LIKE '%sample:dev%';

-- Delete confirmations tagged as sample (if any)
DELETE FROM "Confirmations" WHERE "Remarks" LIKE '%sample:dev%';

-- Delete death registry entries if they contain sample marker
DELETE FROM "DeathRegistries" WHERE "Remarks" LIKE '%sample:dev%';

-- Delete marriages where Groom or Bride is a sample person (FirstName starts with 'DEV_')
DELETE FROM "Marriages" WHERE "GroomId" IN (SELECT "Id" FROM "Persons" WHERE "FirstName" LIKE 'DEV_%') OR "BrideId" IN (SELECT "Id" FROM "Persons" WHERE "FirstName" LIKE 'DEV_%');

-- Delete persons created with DEV_ prefix or linked to sample baptisms
DELETE FROM "Persons" WHERE "FirstName" LIKE 'DEV_%' OR "Id" IN (SELECT "PersonId" FROM "Baptisms" WHERE "Remarks" LIKE '%sample:dev%');

-- Delete users created for development
DELETE FROM "Users" WHERE "Username" LIKE 'dev_%' OR "Username" = 'dev_admin';

COMMIT;

-- Vacuum to reclaim space (Postgres)
VACUUM VERBOSE;
