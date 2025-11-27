## Tech.Interviewer

![CodeRabbit Pull Request Reviews](https://img.shields.io/coderabbit/prs/github/Techinterview-space/web-api?utm_source=oss&utm_medium=github&utm_campaign=Techinterview-space%2Fweb-api&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)

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

## RSS Feeds

The API provides RSS 2.0 feeds for staying updated with the latest content:

### Company Reviews RSS Feed

Get the latest approved company reviews in RSS format:

```
GET /api/companies/reviews/recent.rss
```

**Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Number of items per page (default: 50, max: 100)

**Example:**
```
https://api.techinterview.space/companies/reviews/recent.rss?pageSize=20
```

The RSS feed includes:
- Rich descriptions with detailed rating breakdowns
- Company information and direct links
- Proper RSS 2.0 formatting for compatibility with all feed readers
- HTML-formatted content with emojis and structured data
