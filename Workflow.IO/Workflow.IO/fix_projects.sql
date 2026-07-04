INSERT INTO project."ProjectMembers" ("ProjectId", "UserId", "Role", "JoinedAt", "UpdatedAt")
SELECT p."ProjectId", p."OwnerId", 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM project."Projects" p
WHERE NOT EXISTS (
    SELECT 1 FROM project."ProjectMembers" pm WHERE pm."ProjectId" = p."ProjectId" AND pm."UserId" = p."OwnerId"
);

INSERT INTO project."ProjectMembers" ("ProjectId", "UserId", "Role", "JoinedAt", "UpdatedAt")
SELECT p."ProjectId", u."UserId", 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM project."Projects" p
CROSS JOIN identity."Users" u
WHERE NOT EXISTS (
    SELECT 1 FROM project."ProjectMembers" pm WHERE pm."ProjectId" = p."ProjectId" AND pm."UserId" = u."UserId"
);
