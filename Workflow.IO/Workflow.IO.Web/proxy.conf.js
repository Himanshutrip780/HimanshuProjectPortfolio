const PROXY_CONFIG = {
  "/users": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/projects": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/tasks": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/teams": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/sprints": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/subtasks": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/comments": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/notifications": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/activities": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/attachments": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/analytics": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/issues": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/versions": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/links": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/filters": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true },
  "/hubs": { "target": "http://127.0.0.1:5270", "secure": false, "changeOrigin": true, "ws": true }
};

// Add a bypass function to every route to prevent proxying browser page reloads/navigation
for (const route in PROXY_CONFIG) {
  if (route !== "/hubs") { // Don't bypass WebSockets/SignalR hubs
    PROXY_CONFIG[route].bypass = function (req, res, proxyOptions) {
      if (req.method === "GET" && req.headers.accept && req.headers.accept.indexOf("text/html") !== -1) {
        console.log(`[Proxy Bypass] Browser request to "${req.url}" -> serving index.html`);
        return "/index.html";
      }
    };
  }
}

module.exports = PROXY_CONFIG;
