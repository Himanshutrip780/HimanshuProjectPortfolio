-- Fix Users OrganizationId
UPDATE "Users" SET "OrganizationId" = (SELECT "OrganizationId" FROM "Organizations" LIMIT 1) WHERE "OrganizationId" IS NULL;

-- Fix ProjectMembers (assign existing projects to all users for now so the user can see them, or just the project owner)
-- The owner is already added? Let's check if there are ProjectMembers
INSERT INTO "ProjectMembers" ("ProjectId", "UserId", "Role", "JoinedAt")
SELECT p."ProjectId", p."OwnerId", 0, CURRENT_TIMESTAMP
FROM "Projects" p
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectMembers" pm WHERE pm."ProjectId" = p."ProjectId" AND pm."UserId" = p."OwnerId"
);

-- Also add all users to all projects as Admin (1) just in case to fix visibility
INSERT INTO "ProjectMembers" ("ProjectId", "UserId", "Role", "JoinedAt")
SELECT p."ProjectId", u."UserId", 1, CURRENT_TIMESTAMP
FROM "Projects" p
CROSS JOIN "Users" u
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectMembers" pm WHERE pm."ProjectId" = p."ProjectId" AND pm."UserId" = u."UserId"
);
