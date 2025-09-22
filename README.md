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

## 🌐 Endpoints

Once the application is running, you can access the following endpoints:

  * **API Health Check**: `http://localhost:8080/join`
  * **Swagger UI**: `http://localhost:8080/swagger`
  * **Mongo Express**: `http://localhost:8081` (login with `admin` / `admin`)

## ☁️ Deployment

This project is configured for continuous deployment to **Google Cloud Run**. Any push to the `main` branch will trigger a new build and deployment via **Google Cloud Build**.
