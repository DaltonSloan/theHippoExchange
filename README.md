The Hippo Exchange API

Welcome to The Hippo Exchange, a backend service designed to manage user profiles and other core functionalities for the platform.
Table of Contents

    Features

    Technology Stack

    Prerequisites

    Project Setup

    Running the Application

    API Endpoints

    Deployment

✨ Features

    User Profile Management: Create, update, and retrieve user profiles.

    Clerk Integration: Automatically create a user profile upon user creation in Clerk via webhooks.

    Containerized Development: Full development environment setup using Docker and VS Code Dev Containers for consistency and ease of use.

    CI/CD Pipeline: Automated deployment to Google Cloud Run configured via cloudbuild.yaml.

🛠️ Technology Stack

    Backend: .NET 8 Minimal API

    Database: MongoDB

    Containerization: Docker, Docker Compose

    CI/CD: Google Cloud Build, Google Cloud Run

    Authentication (In Progress): Clerk

✅ Prerequisites

Before you begin, ensure you have the following installed on your local machine:

    Git

    Docker Desktop

    Visual Studio Code

    VS Code Dev Containers Extension

⚙️ Project Setup

Follow these steps to get the project running.
1. Clone the Repository

First, clone the project from GitHub to your local machine.

git clone [https://github.com/DaltonSloan/theHippoExchange.git](https://github.com/DaltonSloan/theHippoExchange.git)
cd theHippoExchange

2. Create the Environment Configuration File

The application uses a .env file at the root of the project to manage database credentials and other secrets. This file is ignored by Git, so you must create it yourself.

Action: Create a new file named .env in the root directory of the project.
3. Configure Database Credentials

Copy and paste the following template into your newly created .env file. These variables are used by docker-compose.dev.yml to configure the MongoDB container and the API service.

# MongoDB Credentials
###These are used to create the root user for the MongoDB instance.
MONGO_USERNAME=devuser
MONGO_PASSWORD=devpass

# MongoDB Database Name
### This defines the name of the database to be used.
MONGO_DB_NAME=HypoExchange

# This variable is used by the mongo-init script to create the initial database.
### It should match MONGO_DB_NAME.
MONGO_INITDB_DATABASE=HypoExchange

How These Variables Work:

    MONGO_USERNAME & MONGO_PASSWORD: These are used by the mongodb service in docker-compose.dev.yml to set the MONGO_INITDB_ROOT_USERNAME and MONGO_INITDB_ROOT_PASSWORD. They create the initial administrative user for the database.

    MONGO_DB_NAME: The .NET API service uses this to know which database to connect to. The mongo-init/01-create-db.js script also uses this to create the initial profiles collection.

    Connection String Construction: The docker-compose.dev.yml file automatically constructs the MongoDB connection string for you and passes it to the API container as an environment variable (Mongo__ConnectionString). The final connection string looks like this:
    "mongodb://devuser:devpass@mongodb:27017/?authSource=admin"

Your setup is now complete! The next step is to run the application.
🚀 Running the Application

The recommended way to run the project is using the VS Code Dev Container, which ensures a consistent and isolated development environment.
Option A: VS Code Dev Container (Recommended)

    Open in VS Code:

    code .

    Reopen in Container: A notification will appear in the bottom-right corner of VS Code. Click the "Reopen in Container" button. VS Code will now build the Docker containers defined in the docker-compose files. This may take a few minutes on the first run.

    Start the Application: Once the container is built and VS Code is attached, it will automatically run dotnet restore. After it finishes, open a new terminal in VS Code (Terminal > New Terminal) and run the API:

    # Navigate to the API project directory
    cd src/HypoExchange.Api

    # Run the application with hot-reload
    dotnet watch run

Option B: Manual Docker Compose

If you prefer not to use a Dev Container, you can run the services directly using Docker Compose.

    Build and Run Services: From the root of the project, run:

    docker-compose -f docker-compose.dev.yml up --build

Accessing the Services

Once the application is running, you can access the following endpoints:

    API (HTTP): http://localhost:8082

    Swagger UI: http://localhost:8082/swagger (Explore and test API endpoints here)

    Mongo Express: http://localhost:8081 (A web-based admin panel for MongoDB. Login with admin / admin)

🗺️ API Endpoints

Authentication is handled temporarily via a custom header: X-User-Id. You can set this header in Swagger by clicking the "Authorize" button and entering any string as the user ID.

Method
	

Endpoint
	

Description

GET
	

/api/profile
	

Retrieves the profile for the current user.

POST
	

/api/profile
	

Creates or updates the profile for the current user.

POST
	

/api/webhooks/clerk
	

(Internal) Webhook for receiving events from Clerk.
☁️ Deployment

This project is configured for continuous deployment to Google Cloud Run using a cloudbuild.yaml file. On every push to the main branch, a GitHub Action triggers Google Cloud Build to build the production Docker image, push it to the Artifact Registry, and deploy it as a new revision to Cloud Run.
