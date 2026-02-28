# Project Management System V2

A full-stack project management application with a .NET Web API backend and a React (TypeScript) frontend.

---
## Database Setup (Docker)

1. **Run docker compose:**
   ```bash
   docker compose up -d
   ```

## Backend Setup (API)

1. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

2. **Database Migration:**
   Ensure your connection string in `src/API/Presentation/appsettings.json` is correct for your local SQL Server instance, then run:
   ```bash
   dotnet ef database update --project src/API/Infrastructure --startup-project src/API/Infrastructure
   ```

3. **Run the API:**
   ```bash
   dotnet run --project src/API/Presentation
   ```
   (Swagger at `/swagger`).

---

## Frontend Setup (React)

1. **Navigate to Directory:**
   ```bash
   cd src/frontend
   ```

2. **Install Dependencies:**
   ```bash
   npm install
   ```

3. **Run Development Server:**
   ```bash
   npm run dev
   ```

---

## Running Tests

### Backend Tests
Run the NUnit test suite located in the `ProjectTests/` directory:
```bash
dotnet test
```

---

## Project Structure

- `src/API/Domain`: Core entities and business logic.
- `src/API/Application`: Services, DTOs, and interfaces.
- `src/API/Infrastructure`: Data access (EF Core), Migrations, and Repositories.
- `src/API/Presentation`: Minimal API endpoints and configuration.
- `src/frontend`: React application with Vite.
- `tests/ProjectTests`: Tests for each layer.
