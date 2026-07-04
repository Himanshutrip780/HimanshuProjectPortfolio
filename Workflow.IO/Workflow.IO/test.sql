\connect "Workflow.IOUserDb"
SELECT "Email", "Role", "IsActive" FROM "UserAccounts" LIMIT 15;
SELECT "UserId", "FirstName", "LastName", "OrganizationId" FROM "Users" LIMIT 15;
SELECT "OrganizationId", "Name" FROM "Organizations" LIMIT 5;

\connect "Workflow.IOProjectDb"
SELECT "ProjectId", "Name", "Key", "OrganizationId" FROM "Projects" LIMIT 5;
SELECT count(*) as "ProjectCount" FROM "Projects";

\connect "Workflow.IOTaskDb"
SELECT count(*) as "TaskCount" FROM "Tasks";
SELECT "IssueKey", "Title", "Status" FROM "Tasks" LIMIT 5;
