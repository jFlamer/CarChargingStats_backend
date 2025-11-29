# 🔋 SmartCharge Backend - Energy Optimization API

REST API built with **.NET 8** for the **Codibly IT Academy 2025** recruitment task. The backend provides energy mix data and calculates optimal time windows for charging electric vehicles to minimize the carbon footprint.

## 🚀 Key Features

1.  **Energy Mix Analysis (`GET /api/EnergyMix/daily-mix`)**
    * Fetches data for 3 days (Today, Tomorrow, The Day After).
    * Supports two data sources:
        * 🇬🇧 **UK:** [Carbon Intensity API](https://carbon-intensity.github.io/api-definitions/).
        * 🇵🇱 **PL:** CSV parsing from **Polish Power Grid (PSE)**.
    * **Smart Time Zones:** Automatic conversion from UTC to local time (GMT/BST for UK, CET/CEST for PL) to correctly group data into daily intervals.

2.  **Charging Optimization Algorithm (`POST /api/EnergyMix/optimal-charging`)**
    * Calculates the best time window (1-6h) with the highest share of RES (Renewable Energy Sources).
    * Implements the **Sliding Window** algorithm.
    * **Time Travel Protection:** The algorithm filters out historical data, suggesting only future time slots.

## 🛠️ Tech Stack

* **Platform:** .NET 8 (C#)
* **Architecture:** Layered (Controllers -> Services -> Models)
* **Testing:** xUnit + Moq (Unit Testing)
* **Documentation:** Swagger / OpenAPI
* **Containerization:** Docker
* **Serialization:** System.Text.Json

## ⚙️ Local Setup & Running

### Prerequisites
* .NET 8 SDK
* (Optional) Docker

### Method 1: .NET CLI
1.  Clone the repository:
    ```bash
    git clone <your-repo-url>
    cd CarChargingStats_backend
    ```
2.  Run the application:
    ```bash
    dotnet run --project CodiblyBackend
    ```
3.  Open Swagger UI in your browser:
    `http://localhost:8080/swagger`

### Method 2: Docker
1.  Build the image:
    ```bash
    docker build -t codibly-backend .
    ```
2.  Run the container:
    ```bash
    docker run -p 8080:8080 codibly-backend
    ```

## 🧪 Unit Testing

The project maintains high code coverage with unit tests verifying business logic, error handling, and algorithms.

To run tests:
```bash
dotnet test
```

To access a Code Coverage report:
```bash
CodiblyBackend.Tests/
├── coveragereport/ 
    ├── index.html
```
What is tested?

- Correct calculation of clean energy percentages.

- The "Sliding Window" algorithm (including edge cases like midnight crossing).

- Input data validation.

- Handling of external API errors.

## 📂 Project Structure
```plaintext
CodiblyBackend/
├── Controllers/       # API Endpoints (EnergyMixController)
├── Services/          # Business logic & Integration (UK API, PL PSE Parser)
├── Models/            # Data Models (DTOs)
├── Program.cs         # DI Configuration & Pipeline
└── Dockerfile         # Docker Image Configuration (Multi-stage build)

CodiblyBackend.Tests/  # Test Project (xUnit)
```

## 🌍 API Endpoints
1. Get Energy Mix
`GET /api/EnergyMix/daily-mix`

Response Example:

```JSON
[
  {
    "date": "2025-11-27T00:00:00",
    "averageCEPercentage": 45.5,
    "fuelMix": [ ... ]
  }
]
```
2. Calculate Charging Window
`POST /api/EnergyMix/optimal-charging`
Body:

```JSON
{
  "Hours": 3
}
Response Example:
```

```
JSON
{
  "startTime": "2025-11-27T14:30:00Z",
  "endTime": "2025-11-27T17:30:00Z",
  "averageCleanEnergy": 82.1
}
```
---
_Created for Codibly IT Academy Recruitment Task 2025._