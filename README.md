# StarWarsAPI
Aplicación backend desarrollada en .NET 8 con SQL Server.

# Caracteristicas
- API REST con ASP.NET Core 8
- Autenticación JWT para endpoints protegidos
- Entity Framework Core 8 para acceso a datos en SQL Server
- Documentación de API con Swagger / OpenAPI
- Pruebas unitarias con xUnit + Moq

# Instrucciones para ejecutar el proyecto
- Clonar el proyecto
- Para facilitar la ejecucion, el proyecto cuenta con las variables de entorno hardcodeadas en el archivo appsettings.json
- Sera necesario contar con un connection string para una base de datos sql server
- Se puede tener una imagen de la db en docker ejecutando el comando:
- `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass12345" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest`
- Ejecutar `cd .\API\` desde la raiz del proyecto clonado
- Luego ejecutar `dotnet run`
- A continuacion, se desplegara swagger con los endpoins documentados
- Swagger se encontrara disponible en la url `http://localhost:8080/swagger/index.html`
