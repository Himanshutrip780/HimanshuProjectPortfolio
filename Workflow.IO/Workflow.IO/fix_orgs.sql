DELETE FROM project."ProjectMembers"
WHERE ("ProjectId", "UserId") IN (
    SELECT pm."ProjectId", pm."UserId"
    FROM project."ProjectMembers" pm
    JOIN project."Projects" p ON pm."ProjectId" = p."ProjectId"
    JOIN identity."Users" u ON pm."UserId" = u."UserId"
    WHERE p."OrganizationId" != u."OrganizationId"
);
