# API Aggregator Service Documentation

## Introduction
The **API Aggregator Service** consolidates data from multiple external APIs, providing a unified response for:
- Weather data using **OpenWeatherMap**.
- News articles using **NewsAPI**.
- GitHub repositories using the **GitHub API**.

The service utilizes **caching** to optimize performance and minimize redundant API calls. Additionally, a robust **fallback mechanism** ensures uninterrupted functionality even if an external API fails.

All endpoints are accessible via **Swagger UI** for easy testing and exploration.

---

## Endpoints Overview

### 1. `GET /api/weather`
- **Description:** Fetches weather data for a specified city.
- **Query Parameters:**
  - `city` (default: "Athens"): The name of the city.
- **Response Example:**
  ```json
  {
    "name": "Athens",
    "main": {
      "temp": 25.0,
      "feels_like": 23.5,
      "humidity": 60,
      "pressure": 1012
    }
  }
  ```
- **Fallback:**
  - Returns default weather data if the city is invalid or the API is unavailable.

### 2. `GET /api/news`
- **Description:** Fetches news articles based on a keyword.
- **Query Parameters:**
  - `keyword` (default: "general"): The keyword for news articles.
- **Response Example:**
  ```json
  [
    {
      "title": "Latest Technology News",
      "description": "An overview of the latest advancements in tech.",
      "url": "https://example.com/article"
    }
  ]
  ```
- **Fallback:**
  - Returns a static article if the API fails or no articles are found.

### 3. `GET /api/github`
- **Description:** Fetches GitHub repositories for a specified user.
- **Query Parameters:**
  - `username` (default: "Mixmavr"): The GitHub username.
- **Response Example:**
  ```json
  [
    {
      "name": "API-Aggregator",
      "description": "A service that aggregates multiple APIs.",
      "url": "https://github.com/Mixmavr/API-Aggregator"
    }
  ]
  ```
- **Fallback:**
  - Returns a static repository entry if the API fails or no repositories are found.

---

## Features

### 1. Caching
- **Purpose:** Reduces redundant API calls and improves response times.
- **Implementation:**
  - Cached responses are stored in-memory for a defined duration (e.g., 30 minutes for weather data).
  - Cache keys are generated dynamically based on query parameters.

### 2. Fallback Mechanisms
- **Purpose:** Ensures consistent responses even during API failures.
- **Implementation:**
  - Returns static data when external APIs are unavailable or provide invalid responses.

---

## Setup & Configuration

### Prerequisites
- .NET SDK 8.0
- API keys for:
  - OpenWeatherMap
  - NewsAPI
  - GitHub (Personal Access Token)

### Configuration
The `appsettings.json` file should include the following:
```json
{
  "OpenWeatherMap": {
    "BaseUrl": "https://api.openweathermap.org/data/2.5",
    "ApiKey": "<YOUR_API_KEY>"
  },
  "NewsAPI": {
    "BaseUrl": "https://newsapi.org/v2",
    "ApiKey": "<YOUR_API_KEY>"
  },
  "GitHubAPI": {
    "BaseUrl": "https://api.github.com",
    "AccessToken": "<YOUR_ACCESS_TOKEN>"
  }
}
```

---

## Usage

###  Swagger UI
- **Access:** Navigate to `/swagger` after running the service.
- **Features:**
  - Test endpoints interactively.
  - View input parameters and response schemas.


## Testing & Maintenance

### Unit Testing
- **Path:** `ApiAggregator.Test`
- **Tools:** xUnit for unit testing services and controllers.

### Logs
- **Purpose:** Tracks API failures and fallback activations.
- **Location:** Console logs (can be extended to file-based or cloud logging).

---

## Contributors
- Mixmavr
- Team API Aggregator

---

For more details, visit the [repository](https://github.com/Mixmavr/API-Aggregator).
