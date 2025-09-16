# HippoExchange


# HippoExchange Setup Guide

## ✅ Option A — VS Code (Dev Container, recommended)

### Prereqs
- Install Docker Desktop  
- Install VS Code + the Dev Containers extension  
- git installed

### Steps
1. Clone & open:
   ```bash
   git clone https://github.com/DaltonSloan/theHippoExchange.git
   cd theHippoExchange
   code .
   
2. Reopen in container: when VS Code prompts **“Reopen in Container”**, click it.

3. Once it attaches, in the integrated terminal:
   ```bash
   cd /workspace/src/HippoExchange.Api
   dotnet restore
   dotnet run   # or: dotnet watch run
   
4. Hit the app:

   - API (HTTP): http://localhost:8080/health  
   - Swagger: http://localhost:8080/swagger  
   - Mongo Express: http://localhost:8081 (login: `admin` / `admin`)
