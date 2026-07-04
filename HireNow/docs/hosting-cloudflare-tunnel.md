# Cloudflare Tunnel Public Hosting Guide

This guide explains how to host your Applicant Tracking System (ATS) application publicly and securely for free using **Cloudflare Tunnel**.

With this setup, you can access your locally running Dockerized ATS application from anywhere in the world without opening ports on your home router (no port forwarding) or exposing your home IP address.

---

## 🏗️ Architecture Overview

The ATS application consists of the following services:

| Service Name | Internal Container Name | Local Port | Internal Port | Exposed via Tunnel? |
| :--- | :--- | :--- | :--- | :--- |
| **Database** | `ats_sql_server` | `1433` | `1433` | ❌ **No** (Kept private) |
| **Backend API** | `ats_api_backend` | `5000` | `80` |      **Yes** (Required for API calls) |
| **Frontend UI** | `ats_angular_frontend` | `4200` | `80` |      **Yes** (Web interface) |
| **Tunnel Agent**| `ats_cloudflared` | N/A | N/A | N/A (Connects outbound to Cloudflare) |

---

## ⚙️ Step 1: Initial Local Setup

1. **Check local requirements**: Ensure you have **Docker Desktop** installed and running on Windows.
2. **Set up Environment Variables**:
   Copy the `.env.example` file in the project root to `.env`:
   ```powershell
   Copy-Item .env.example .env
   ```
3. **Verify the default configuration**: Open `.env` and make sure it has the placeholder variable:
   ```env
   CLOUDFLARE_TUNNEL_TOKEN=
   ```

---

## 🌐 Step 2: Create a Cloudflare Tunnel

To host the app publicly, you need a free Cloudflare Account and a domain managed by or pointed to Cloudflare.

1. Go to the [Cloudflare Zero Trust Dashboard](https://one.dash.cloudflare.com/).
2. Navigate to **Networks** ➡️ **Tunnels** and click **Create a Tunnel**.
3. Choose **Cloudflare** (managed) as the tunnel type.
4. Give your tunnel a descriptive name (e.g., `ats-production-tunnel`) and click **Save Tunnel**.
5. Under **Install and run a connector**, locate your **Tunnel Token**. It is the long string of letters and numbers after the command parameter `--token` (do not copy the command itself, just the token).
6. Open your local `.env` file and paste the token:
   ```env
   CLOUDFLARE_TUNNEL_TOKEN=your_copied_cloudflare_token_here
   ```

---

## 🌍 Step 3: Configure Hostname Mapping

While still in the Cloudflare Tunnel Dashboard, navigate to the **Public Hostname** tab. You need to map two subdomains:

### 1. Map Frontend (Web UI)
* **Subdomain**: `ats` (or leave blank if using the root domain)
* **Domain**: `yourdomain.com`
* **Path**: (leave blank)
* **Type**: `HTTP`
* **URL**: `frontend:80` (or `ats_angular_frontend:80`)

### 2. Map Backend (API)
* **Subdomain**: `api-ats`
* **Domain**: `yourdomain.com`
* **Path**: (leave blank)
* **Type**: `HTTP`
* **URL**: `backend:80` (or `ats_api_backend:80`)

> [!IMPORTANT]
> Since the Angular frontend runs on the client's browser, it must send HTTP requests to a publicly accessible API URL. Make sure to update the API base URL in your frontend service configuration (e.g. replacing `http://localhost:5000` references with `https://api-ats.yourdomain.com`).

---

## 🚀 Step 4: Run the Application Stack

Run the stack in **detached mode** (in the background) using Windows PowerShell:

```powershell
# Rebuild and start the containers
docker compose up -d --build
```

Docker Compose will automatically start `cloudflared`, which reads the token from the `.env` file and establishes a secure connection to Cloudflare.

---

## 🛠️ Management Commands

Use these exact PowerShell commands in the project root directory:

* **Start the stack**:
  ```powershell
  docker compose up -d
  ```
* **Stop the stack**:
  ```powershell
  docker compose down
  ```
* **View all container logs**:
  ```powershell
  docker compose logs -f
  ```
* **View Cloudflare Tunnel logs**:
  ```powershell
  docker compose logs -f cloudflared
  ```
* **Check container status**:
  ```powershell
  docker compose ps
  ```

---

## 🔒 Security Precautions

> [!WARNING]
> Exposing your database publicly can lead to data loss or compromise.

- **Private Database**: Do not expose `db` (port 1433) in the Cloudflare Tunnel. Keep it internal so only the backend container can talk to it inside the Docker bridge network.
- **Strong Credentials**: Always change the default SQL Server password (`MSSQL_SA_PASSWORD`) in your docker-compose environment variables to a unique password before hosting publicly.
- **Strict HTTPS**: Cloudflare Tunnel uses TLS by default. Ensure your public hostnames only allow connections over HTTPS.
- **Access Policies**: Consider adding **Cloudflare Access** policies (under Zero Trust -> Access -> Applications) to protect your admin dashboard or portals with email OTP, Google Auth, or GitHub OAuth before traffic even reaches your local machine.

---

## 🔍 Troubleshooting

### 1. Public URL returns a "502 Bad Gateway" or "Unable to Connect"
- **Cause**: The `cloudflared` container cannot reach the `frontend` container, or the frontend is not running.
- **Fix**: Check if the container is running:
  ```powershell
  docker compose ps
  ```
  If it shows exited, check the logs:
  ```powershell
  docker compose logs frontend
  ```

### 2. Tunnel displays "Inactive" or "Offline" on Cloudflare
- **Cause**: The tunnel token is missing, expired, or invalid.
- **Fix**: Check if `cloudflared` connected successfully:
  ```powershell
  docker compose logs cloudflared
  ```
  Verify that the token in `.env` matches the one on the dashboard exactly.

### 3. Website opens but displays "Network Error" or cannot load details
- **Cause**: The frontend is trying to call `http://localhost:5000` to access the backend API, which isn't publicly reachable.
- **Fix**: Map `api-ats.yourdomain.com` to `backend:80` in the Cloudflare dash, and update the API base URL in your frontend service configurations to point to `https://api-ats.yourdomain.com`.

### 4. App stops working when the Windows computer goes to sleep
- **Cause**: Windows puts the CPU or network adapter to sleep, severing the outbound Tunnel connection.
- **Fix**: Go to **Windows Settings** ➡️ **System** ➡️ **Power & sleep** and set **Sleep** to **Never** when plugged in.
