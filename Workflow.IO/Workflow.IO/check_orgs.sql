SELECT pm."ProjectId", p."Name" AS "ProjectName", p."OrganizationId" AS "ProjectOrg", pm."UserId", u."OrganizationId" AS "UserOrg"
FROM project."ProjectMembers" pm
JOIN project."Projects" p ON pm."ProjectId" = p."ProjectId"
JOIN identity."Users" u ON pm."UserId" = u."UserId";
