dotnet ef migrations add InitialCreate --project src/API/Infrastructure --startup-project src/API/Presentation
dotnet ef database update --project src/Api/Infrastructure