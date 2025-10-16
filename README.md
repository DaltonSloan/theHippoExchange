# The HippoExchange

Welcome to the HippoExchange backend! This repository contains the ASP.NET Core API that powers HippoExchange, a tool for tracking personal equipment, sharing assets, and recording maintenance.

This guide is written for new developers with little or no experience setting up .NET projects. Follow each section in order; every command shows exactly where to run it and what to expect.

## Table of Contents
1. Getting to Know the Project
2. Tooling Checklist
3. Project Setup Walkthrough
4. Verify the API
5. Working with Demo Data
6. Useful Commands
7. Troubleshooting
8. Deployment

## 1. Getting to Know the Project

- The API is written in C# with ASP.NET Core 8.
- MongoDB stores application data; assets, maintenance records, and users live in separate collections.
- Authentication currently relies on shared secrets supplied in request headers. Clerk provides user identities.
- Cloudinary hosts asset images.

You will run the API inside a VS Code Dev Container. The container ships with the .NET SDK, MongoDB tools, and other dependencies already installed.

## 2. Tooling Checklist

Install these before you start:

- Docker Desktop – required for Dev Containers and local MongoDB. Launch Docker Desktop after installing so it finishes initial setup.
- Visual Studio Code – the editor we use for Dev Containers.
- VS Code "Dev Containers" extension – enables the remote container environment.
- Git – used for cloning this repository.
- Optional but helpful: `curl` (ships with macOS/Linux; on Windows you can use PowerShell's `Invoke-WebRequest`), a password manager for storing secrets, and access to the HippoExchange backend Discord channel for shared credentials.

Create the accounts you will need:

- Cloudinary account – provides the `CLOUDINARY_URL` connection string.
- Clerk project access – ask a teammate for the webhook secret and demo user IDs.
- Mongo credentials – the backend team shares the values you will place in `.env`.

## 3. Project Setup Walkthrough

All commands below run inside your terminal. Lines that start with `#` are comments to explain what the command does.

### Step 1 – Clone the repository

```bash
# Clone with HTTPS (recommended for beginners)
git clone https://github.com/DaltonSloan/theHippoExchange.git
cd theHippoExchange
```

If you work from your own fork, replace the URL with your fork’s address.

### Step 2 – Open the repo in VS Code

```bash
code .
```

VS Code may prompt you to install the Dev Containers extension if it is missing.

### Step 3 – Reopen in the Dev Container

1. When VS Code opens, wait for the prompt **“Reopen in Container”** and click it.  
   If you miss the prompt, run `Ctrl+Shift+P` (or `Cmd+Shift+P` on macOS) → “Dev Containers: Reopen in Container”.
2. The first build can take a few minutes. Watch the progress tray in the lower-right corner.
3. When the container is ready VS Code opens a new terminal that already runs inside the container (`/workspace` prompt).

### Step 4 – Create the Mongo `.env` file

The backend uses environment variables located in `.env`. Create the file and paste the credentials from the backend Discord channel.

```bash
# Still inside /workspace
cat <<'ENV' > .env
MONGO_USERNAME=
MONGO_PASSWORD=
MONGO_DB_NAME=
MONGO_INITDB_DATABASE=
ENV
```

Replace each blank value with the real credential, then save.

### Step 5 – Configure application secrets

The API expects several secret values. The recommended way to store them locally is .NET user secrets, which keeps secrets outside the repo.

```bash
# Run once per machine (inside the container)
dotnet user-secrets init --project src/HippoExchange.Api

# Set each secret value
dotnet user-secrets set --project src/HippoExchange.Api "Auth:ApiKey" "<your-api-key>"
dotnet user-secrets set --project src/HippoExchange.Api "Auth:WebhookSecret" "<your-webhook-secret>"
dotnet user-secrets set --project src/HippoExchange.Api "CLOUDINARY_URL" "cloudinary://<api_key>:<api_secret>@<cloud_name>"
```

Need help generating secure values?

- macOS/Linux:

  ```bash
  openssl rand -hex 32
  ```

- Windows PowerShell:

  ```powershell
  [System.Convert]::ToHexString((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
  ```

Keep the generated values somewhere safe; you will use the API key when calling the service.

To review what you stored later:

```bash
dotnet user-secrets list --project src/HippoExchange.Api
```

#### Alternative: local secrets file

If you prefer a file you can edit, copy the example file and fill in the blanks. Git ignores the real secrets file.

```bash
cp src/HippoExchange.Api/appsettings.Secrets.example.json src/HippoExchange.Api/appsettings.Secrets.json
# Open the new file in VS Code and replace placeholder values.
```

### Step 6 – Start dependent containers (MongoDB, Mongo Express)

The Dev Container configuration usually starts these automatically. If you need to start them manually:

```bash
docker compose up -d
```

Check running containers with `docker ps`. You should see entries for Mongo and Mongo Express.

### Step 7 – Run the API

```bash
cd /workspace/src/HippoExchange.Api
dotnet run
```

The first run restores NuGet packages. Once the API is up you will see output similar to:

```
Now listening on: http://0.0.0.0:8080
Application started. Press Ctrl+C to shut down.
```

Leave this terminal window open while you test the API. To stop the app, press `Ctrl+C`.

## 4. Verify the API

Use a second terminal inside the Dev Container (`Terminal → New Terminal`).

- Health check:

  ```bash
  curl http://localhost:8080/join
  ```

  You should receive a welcome banner.

- Swagger UI: open [http://localhost:8080/swagger](http://localhost:8080/swagger) in your browser.

- Mongo Express: open [http://localhost:8081](http://localhost:8081) and log in with `admin` / `admin` to inspect the MongoDB collections.

### Calling secured endpoints

All API endpoints (except `/join`) require two headers:

```
X-Api-Key: <Auth:ApiKey value>
X-User-Id: <Clerk user id>
```

Example request that lists assets for a demo user:

```bash
curl \
  -H "X-Api-Key: <your-api-key>" \
  -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT" \
  http://localhost:8080/assets
```

Replace `<your-api-key>` with the value you stored earlier. Demo Clerk IDs are listed in the “Working with Demo Data” section.

## 5. Working with Demo Data

Populating MongoDB with sample content makes testing easier.

### Seed via command line

```bash
cd /workspace/src/HippoExchange.Api
dotnet run seed
```

The command creates demo users, assets, and maintenance records. You can run it multiple times; it is idempotent.

### Seed via API

| Endpoint | Description |
|----------|-------------|
| `POST /api/admin/seed` | Create demo data |
| `DELETE /api/admin/seed` | Remove demo data |
| `GET /api/admin/seed/status` | Check if demo data exists |

Example:

```bash
curl \
  -H "X-Api-Key: <your-api-key>" \
  -H "X-User-Id: <clerk-id>" \
  -X POST http://localhost:8080/api/admin/seed
```

### Demo Clerk IDs

- John Smith (Homeowner): `user_33UeIDzYloCoZABaaCR1WPmV7MT`
- Jane Doe (Hobbyist): `user_33UeKv6eNbmLb2HClHd1PN51AZ5`
- Bob Builder (Contractor): `user_33UeOCZ7LGxjHJ8dkwnAIozslO0`

Use these IDs when you need to impersonate a user while testing.

## 6. Useful Commands

- `dotnet watch run` – optional hot reload experience.
- `dotnet user-secrets list --project src/HippoExchange.Api` – view stored secrets.
- `docker compose logs mongo` – inspect MongoDB output.
- `docker compose down` – stop MongoDB and Mongo Express containers.

## 7. Troubleshooting

- **Dev Container build fails** – ensure Docker Desktop is running. On Windows, enable virtualization in BIOS if required.
- **Cannot connect to MongoDB** – verify `.env` values match the shared credentials and that the Mongo container is running (`docker ps`).
- **401 Unauthorized responses** – double-check that both `X-Api-Key` and `X-User-Id` headers are present and spelled correctly.
- **Webhook endpoint returns 500** – the webhook secret must be set via user secrets or the secrets file.
- **Cloudinary upload errors** – confirm the `CLOUDINARY_URL` matches the Cloudinary dashboard and that the account allows unsigned uploads.

Ask for help in the backend Discord channel if you get stuck; mention the step number you were following.

## 8. Deployment

The `main` branch deploys automatically to Google Cloud Run via Google Cloud Build. The `fixes` branch targets development work only. Do not push secrets to Git – rely on user secrets or the Git-ignored secrets file.

---

You are now ready to build features, write tests, and contribute! Keep this guide handy the first few times you set up the project, and feel free to update it with anything that would have helped you.
