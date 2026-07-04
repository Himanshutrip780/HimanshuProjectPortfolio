\connect "Workflow.IOUserDb"
SELECT count(*) as "UserCount" FROM "identity"."Users";
SELECT "UserId", "FirstName", "LastName", "OrganizationId" FROM "identity"."Users" LIMIT 5;
SELECT "OrganizationId", "Name" FROM public."Organizations" LIMIT 2;

\connect "Workflow.IOProjectDb"
SELECT count(*) as "ProjectCount" FROM "project"."Projects";
SELECT "ProjectId", "Name", "Key", "OrganizationId" FROM "project"."Projects" LIMIT 5;

\connect "Workflow.IOTaskDb"
SELECT count(*) as "TaskCount" FROM "task"."Tasks";
SELECT "IssueKey", "Title", "Status" FROM "task"."Tasks" LIMIT 5;
