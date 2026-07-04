\connect "Workflow.IOUserDb"
SELECT count(*) as "UserCount" FROM public."Users";
SELECT "UserId", "FirstName", "LastName", "OrganizationId" FROM public."Users" LIMIT 5;
