# Microservice Template

## Prequisites

- .NET Core SDK 5.0

## CLI_Template Usage

Clone the repository and navigate to the CLI_Template directory. Open a command line and run `dotnet new -i .\` to install the template.

1. Create a new folder for your microservice
2. Open a command line in the folder and run `dotnet new Microservice -n <SolutionName>` where `<SolutionName>` is the name of the service you want to create. See below for a list of arguments
3. The solution will now be created and its NuGet packages restored

| Argument      | Description                                  |
| :------------ | :------------------------------------------- |
| --name (-n)   | Names the output solution and projects.      |
| --dapper (-d) | Includes Dapper and FluentMigrator packages. |
| --efcore (-e) | Includes EFCore and SqlServer for EFCore.    |

4. There are four projects in the solution:

| Project | Description                                                                |
| :------ | :------------------------------------------------------------------------- |
| Domain  | Contains the Business components (domain objects and services, validation) |
| DAL     | Contains Data Access components like http and database clients             |
| Api     | Contains API controllers, filters and other components related             |
| Host    | Host application including appsettings and middlewares                     |
