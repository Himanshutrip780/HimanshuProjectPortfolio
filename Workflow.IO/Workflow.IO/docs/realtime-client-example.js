import * as signalR from "@microsoft/signalr";

const gatewayUrl = "http://localhost:5270";
const jwtToken = "<jwt-token>";
const projectId = "<project-id>";
const taskId = "<task-id>";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${gatewayUrl}/hubs/workflow.io`, {
    accessTokenFactory: () => jwtToken
  })
  .withAutomaticReconnect()
  .build();

connection.on("EventReceived", event => {
  console.log("global event", event);
});

connection.on("ProjectEventReceived", event => {
  console.log("project event", event);
});

connection.on("TaskEventReceived", event => {
  console.log("task event", event);
});

connection.on("NotificationReceived", event => {
  console.log("notification", event);
});

await connection.start();
await connection.invoke("JoinProject", projectId);
await connection.invoke("JoinTask", taskId);
