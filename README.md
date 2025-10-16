# The HippoExchange

This is a simple API for managing user profiles.

## 🚀 Getting Started

### Prerequisites

- **Docker Desktop** running locally
- **Visual Studio Code** with the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
- **Git** for cloning the repository

### Step-by-Step Development Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/DaltonSloan/theHippoExchange.git
   cd theHippoExchange
   ```

2. **Open the project in VS Code**

   ```bash
   code .
   ```

3. **Reopen in the Dev Container**  
   When prompted, choose **Reopen in Container** so VS Code builds the environment described in `.devcontainer/devcontainer.json` and `docker-compose.dev.yml`. This installs the .NET SDK, MongoDB tools, and other dependencies inside the container.

4. **Create the Mongo `.env` file**  
   In the repo root, add a `.env` file with credentials provided in the backend Discord channel:

   ```env
   MONGO_USERNAME=
   MONGO_PASSWORD=
   MONGO_DB_NAME=
   MONGO_INITDB_DATABASE=
   ```

5. **Configure application secrets**  
   The API requires a Cloudinary URL, an API key, and a webhook secret. Manage them with .NET user-secrets (recommended) or a local secrets file.

   _Using user-secrets (inside the dev container):_

   ```bash
   # Run once per machine
   dotnet user-secrets init --project src/HippoExchange.Api

   # Set strong random values
   dotnet user-secrets set --project src/HippoExchange.Api "Auth:ApiKey" "<your-api-key>"
   dotnet user-secrets set --project src/HippoExchange.Api "Auth:WebhookSecret" "<your-webhook-secret>"
   dotnet user-secrets set --project src/HippoExchange.Api "CLOUDINARY_URL" "cloudinary://<api_key>:<api_secret>@<cloud_name>"
   ```

   _Using a local secrets file (git-ignored):_

   1. Copy `src/HippoExchange.Api/appsettings.Secrets.example.json` to `src/HippoExchange.Api/appsettings.Secrets.json`.
   2. Replace the placeholder values with real credentials.

6. **Run the API**

   ```bash
   cd /workspace/src/HippoExchange.Api
   dotnet run
   ```

   The API listens on <http://localhost:8080>. Swagger UI is available at `/swagger`.

7. **Call secured endpoints**  
   Every API request must supply both headers:

   ```text
   X-Api-Key: <Auth:ApiKey value>
   X-User-Id: <Clerk user id>
   ```

   Use the demo Clerk IDs in the section below or real IDs from Clerk. Without these headers the API returns `401 Unauthorized`.

## 🌱 Database Seeding

The application includes a database seeding feature to populate the database with realistic demo data for development and testing.

### Command Line Seeding

**Seed the database with demo data:**
```bash
cd /workspace/src/HippoExchange.Api
dotnet run seed
```

### API Endpoints for Seeding

You can also seed the database via API endpoints:

- **`POST /api/admin/seed`** - Seed database with demo data (idempotent)
- **`DELETE /api/admin/seed`** - Remove only demo data
- **`GET /api/admin/seed/status`** - Check if demo data exists

**Example:**
```bash
# Seed via API
curl -H "X-Api-Key: <your-api-key>" -H "X-User-Id: <clerk-id>" -X POST http://localhost:8080/api/admin/seed

# Check status
curl -H "X-Api-Key: <your-api-key>" -H "X-User-Id: <clerk-id>" http://localhost:8080/api/admin/seed/status

# Purge demo data
curl -H "X-Api-Key: <your-api-key>" -H "X-User-Id: <clerk-id>" -X DELETE http://localhost:8080/api/admin/seed
```

These endpoints are also available in Swagger UI under the "Admin" tag.

### Demo Data Overview

The seeding script creates:

- **3 Demo Users** with different personas:
  - **John Smith** (`clerk_john_smith`) - Homeowner with lawn/garden equipment
  - **Jane Doe** (`clerk_jane_doe`) - Hobbyist with workshop tools
  - **Bob Builder** (`clerk_bob_builder`) - Contractor with professional equipment

- **5-10 Assets per user** with varied data:
  - Mix of brands (DeWalt, Craftsman, Honda, Makita, Milwaukee, etc.)
  - Different statuses (available, maintenance, loaned)
  - Various cost ranges ($10 to $5000)
  - Mix of favorited and non-favorited assets
  - Realistic purchase dates and locations

- **8-15 Maintenance records per asset** with variety:
  - Some overdue (due date in past)
  - Some due soon (within 7 days)
  - Some due later (within 30 days)
  - Some completed
  - Realistic maintenance tasks and required tools

### Using Demo Users

To test API endpoints with demo users, use their Clerk IDs in the `X-User-Id` header:

```bash
# Example: Get assets for John Smith
curl -H "X-Api-Key: <your-api-key>" -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT" http://localhost:8080/api/assets

# Example: Get assets for Jane Doe
curl -H "X-Api-Key: <your-api-key>" -H "X-User-Id: user_33UeKv6eNbmLb2HClHd1PN51AZ5" http://localhost:8080/api/assets

# Example: Get assets for Bob Builder
curl -H "X-Api-Key: <your-api-key>" -H "X-User-Id: user_33UeOCZ7LGxjHJ8dkwnAIozslO0" http://localhost:8080/api/assets
```

**Demo User Clerk IDs:**
- John Smith (Homeowner): `user_33UeIDzYloCoZABaaCR1WPmV7MT`
- Jane Doe (Hobbyist): `user_33UeKv6eNbmLb2HClHd1PN51AZ5`
- Bob Builder (Contractor): `user_33UeOCZ7LGxjHJ8dkwnAIozslO0`

### Important Notes

- **Seeding is idempotent**: Running the seed command multiple times will not create duplicates. Existing demo users are removed and recreated.
- **⚠️ Use with caution**: Seeding commands work in any environment, so be careful when running in production.

## 🌐 Endpoints

Once the application is running, you can access the following endpoints:

- **API Health Check**: `http://localhost:8080/join`
- **Swagger UI**: `http://localhost:8080/swagger`
- **Mongo Express**: `http://localhost:8081` (login with `admin` / `admin`)

## ☁️ Deployment

This project is configured for continuous deployment to **Google Cloud Run**. Any push to the `main` branch will trigger a new build and deployment via **Google Cloud Build**.
