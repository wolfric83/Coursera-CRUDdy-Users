# Activity 1: Copilot Notes

## 1. Purpose of the User Management API
The User Management API is intended to support basic user record operations for an internal business application. It allows the application to create, read, update, and delete user data through HTTP endpoints.

## 2. Project Structure
The project is organized as a standard ASP.NET Core Web API with the following main parts:

- Program.cs: Configures the web app, registers controllers, enables OpenAPI in development, and sets up the EF Core InMemory database.
- Controllers/: Contains API controllers.
- Controllers/UsersController.cs: Implements the CRUD endpoints for users.
- Data/: Stores data access classes.
- Data/UserDbContext.cs: Defines the EF Core database context.
- Models/: Stores domain models.
- Models/User.cs: Defines the User entity used by the API.
- appsettings.json: Stores application configuration settings.
- UserManagementAPI.csproj: Defines the project and package references.

## 3. CRUD Endpoints Created
The API includes the following endpoints:

- GET /api/users: Returns all users.
- GET /api/users/{id}: Returns a specific user by ID.
- POST /api/users: Creates a new user.
- PUT /api/users/{id}: Updates an existing user.
- DELETE /api/users/{id}: Deletes a user by ID.

## 4. How Copilot Assisted with Scaffolding and Code Generation
Copilot assisted by helping to:

- Scaffold the initial ASP.NET Core Web API structure.
- Generate the basic controller class and CRUD action methods.
- Create a simple EF Core context and a starter user model.
- Suggest a straightforward implementation using async EF Core methods.
- Help structure the project folders for a beginner-friendly API layout.

## 5. Parts of the Generated Code That Required Human Review
Although Copilot accelerated development, some parts needed careful review:

- The update logic in the PUT endpoint needed to be checked to ensure it updates the existing database record correctly rather than relying on a fragile entity update approach.
- The route and status code behavior needed to be reviewed to confirm the API returned the expected results for missing users and successful updates/deletes.
- The EF Core context required review to ensure the DbSet was properly exposed for the controller.
- The generated code was reviewed to keep the implementation simple, readable, and appropriate for a beginner-level API.

## 6. Testing Tools Used
The testing tools used for this activity included:

- VS Code REST Client / .http file testing via UserManagementAPI.http
- Bruno as the manual HTTP client for testing the API
- dotnet build for verifying the project compiles successfully

## 7. Test Results

| Test | Expected Result | Actual Result | Pass/Fail |
| --- | --- | --- | --- |
| Build project | Build succeeds without errors | Build succeeds without errors | Pass |
| GET /api/users | Returns a JSON array of users | Returns a JSON array of users | Pass |
| GET /api/users/{id} | Returns the user with the specified ID or 404 if not found | Returns the user with the specified ID or 404 if not found | Pass |
| POST /api/users | Creates a user and returns 201 Created | Creates a user and returns 201 Created | Pass |
| PUT /api/users/{id} | Updates the user and returns 204 No Content | Updates the user and returns 204 No Content | Pass |
| DELETE /api/users/{id} | Deletes the user and returns 204 No Content | Deletes the user and returns 204 No Content | Pass |
| GET /api/users/{id} after deletion | Returns 404 Not Found | Returns 404 Not Found | Pass |
