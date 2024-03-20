## Tech.Interviewer

This repo is for Backend API for [techinterview.space](https://techinterview.space).

## Apps
1. [Frontend](https://techinterview.space)
2. [API](https://api.techinterview.space)

## Tech Stack

- .NET 8
- Angular 17

## Docs

- [Contribution Guidelines](./CONTRIBUTING.md)
- [Code of Conduct](./CODE_OF_CONDUCT.md)

## How to run .NET Web Api

### Prerequisites

- MacOs, Windows, or Linux (_MacOs is preferred. Because I think so_)
- Docker Desktop installed
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed
- Visual Studio Code, JetBrains Rider, or Visual Studio installed

1. Clone the repository
2. Open the solution in Visual Studio Code, JetBrains Rider, or Visual Studio.
3. Install dependencies by suggestion of the IDe or by running `dotnet restore` in the terminal in the `./src` folder of the project.
4. Run command `docker-compose up -d --build database.api elasticsearch localstack` in the terminal in the root folder of the project.
5. Launch the application in your IDE or by running `dotnet run` in the terminal in the `./src` folder of the project.

Expected: The application should be running on `https://localhost:5001`.
