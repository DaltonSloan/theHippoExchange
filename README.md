# The HippoExchange

This is a simple API for managing user profiles.

## 🚀 Getting Started

### Prerequisites

  * **Docker Desktop**: Make sure you have Docker Desktop installed and running.
  * **VS Code**: You'll need Visual Studio Code with the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers).
  * **Git**: You'll need Git installed to clone the repository.

### Local Development

This project is set up to run in a containerized environment for easy development.

1.  **Clone the repository**:

    ```bash
    git clone https://github.com/DaltonSloan/theHippoExchange.git
    cd theHippoExchange
    ```

2.  **Open in VS Code**:

    ```bash
    code .
    ```

3.  **Reopen in Container**: When prompted, click on **"Reopen in Container"**. This will build the Docker container defined in the `.devcontainer/devcontainer.json` and `docker-compose.dev.yml` files.

4. **Create a .env File**: Once the container is up and running, create a new file in the base directory called ".env" without quotes. Within the file, type the following and fill in with the parameters in the Back End Discord channel #database:
    ```bash
    MONGO_USERNAME=
    MONGO_PASSWORD=
    MONGO_DB_NAME=
    MONGO_INITDB_DATABASE=
    ```

5.  **Run the application**: Once the container is up and running, open the integrated terminal in VS Code and run the following commands:

    ```bash
    cd /workspace/src/HippoExchange.Api
    dotnet run
    ```

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
curl -X POST http://localhost:8080/api/admin/seed

# Check status
curl http://localhost:8080/api/admin/seed/status

# Purge demo data
curl -X DELETE http://localhost:8080/api/admin/seed
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

> **Authentication update:** The API now requires a valid Clerk session token in the `Authorization` header. The legacy `X-User-Id` header is no longer accepted.

To exercise endpoints directly (for example with `curl` or Postman):

1. Sign in to the frontend or Clerk dashboard with one of the demo users listed below.
2. Retrieve a session token (`__session` cookie value or via the [Clerk CLI](https://clerk.com/docs/reference/clerk-cli/sessions#get-session-tokens)).
3. Include the token in the `Authorization` header:

```bash
curl -H "Authorization: Bearer <CLERK_SESSION_TOKEN>" http://localhost:8080/assets
```

**Demo Clerk IDs (for reference):**
- John Smith (Homeowner): `user_33UeIDzYloCoZABaaCR1WPmV7MT`
- Jane Doe (Hobbyist): `user_33UeKv6eNbmLb2HClHd1PN51AZ5`
- Bob Builder (Contractor): `user_33UeOCZ7LGxjHJ8dkwnAIozslO0`

### Important Notes

- **Seeding is idempotent**: Running the seed command multiple times will not create duplicates. Existing demo users are removed and recreated.
- **⚠️ Use with caution**: Seeding commands work in any environment, so be careful when running in production.

## 🌐 Endpoints

Once the application is running, you can access the following endpoints:

  * **API Health Check**: `http://localhost:8080/join`
  * **Swagger UI**: `http://localhost:8080/swagger`
  * **Mongo Express**: `http://localhost:8081` (login with `admin` / `admin`)

## ☁️ Deployment

This project is configured for continuous deployment to **Google Cloud Run**. Any push to the `main` branch will trigger a new build and deployment via **Google Cloud Build**.

## 📚 Code Documentation

Core services are now annotated with XML documentation comments so consumers can see purpose, parameters, and expected behaviour directly in IntelliSense. If you want to generate reference docs, enable the documentation file switch:

```bash
dotnet build /p:GenerateDocumentationFile=true
```

Please follow the same concise style when adding new public methods—cover what the operation does, any important side effects, and meaningful parameter details rather than repeating method names.

## 🐳 Run Frontend + Backend Together (Docker)

A root-level compose file lets you spin up MongoDB, the ASP.NET Core API, and the Vite client in one command:

```bash
# from the repository root
docker compose -f theHippoExchange/docker-compose.local.yml up --build
```

Services exposed locally:

- `frontend` → <http://localhost:5173>
- `api` → <http://localhost:8080>
- `mongo-express` → <http://localhost:8082>

Optional environment overrides before launching (so Clerk/Cloudinary work end-to-end):

```bash
export VITE_CLERK_PUBLISHABLE_KEY=pk_test_xxx
export VITE_CLERK_JWT_TEMPLATE=mobile
export CLOUDINARY_URL=cloudinary://<key>:<secret>@<cloud>
```

Mongo uses the baked-in `devuser` / `devpass` credentials and stores data inside the container; remove the container to reset its data volume.
