SELECT count(*) FROM identity."Users";
UPDATE identity."Users" SET "OrganizationId" = (SELECT "OrganizationId" FROM public."Organizations" LIMIT 1) WHERE "OrganizationId" IS NULL;

SELECT count(*) FROM project."ProjectMembers";
INSERT INTO project."ProjectMembers" ("ProjectId", "UserId", "Role", "JoinedAt")
SELECT p."ProjectId", p."OwnerId", 0, CURRENT_TIMESTAMP
FROM project."Projects" p
WHERE NOT EXISTS (
    SELECT 1 FROM project."ProjectMembers" pm WHERE pm."ProjectId" = p."ProjectId" AND pm."UserId" = p."OwnerId"
);

INSERT INTO project."ProjectMembers" ("ProjectId", "UserId", "Role", "JoinedAt")
SELECT p."ProjectId", u."UserId", 1, CURRENT_TIMESTAMP
FROM project."Projects" p
CROSS JOIN identity."Users" u
WHERE NOT EXISTS (
    SELECT 1 FROM project."ProjectMembers" pm WHERE pm."ProjectId" = p."ProjectId" AND pm."UserId" = u."UserId"
);
