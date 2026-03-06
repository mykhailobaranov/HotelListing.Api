# HotelListing API

A comprehensive RESTful API for managing hotels and countries.

*Note: This is an educational project developed to deeply understand and practice advanced ASP.NET Core Web API concepts, architectural patterns, and industry best practices.*

## 🚀 Technologies & Tools

- **Framework:** .NET 9 (ASP.NET Core Web API)
- **Database & ORM:** MS SQL Server, Entity Framework Core (Code First)
- **Authentication:** JWT (JSON Web Tokens), ASP.NET Core Identity
- **Mapping:** AutoMapper
- **Logging:** Serilog
- **API Documentation:** Swagger UI / OpenAPI (with XML comments and JWT support)
- **Versioning:** Asp.Versioning.Mvc



## 🧠 Key Concepts & Features Implemented

During the development of this API, I've transitioned from basic CRUD to a production-ready architecture. Here are the key patterns and features implemented:

### 🏗️ Architecture & Design Patterns
- **Repository Pattern:** Generic and specific repositories to abstract database operations.
- **Service Layer:** Extracted business logic into a dedicated layer (Controller -> Service -> Repository).
- **DTO Pattern:** Strict separation of Domain Entities and Data Transfer Objects to prevent over-posting and hide internal database structures.
- **Result Pattern:** Elegant handling of success/failure states across layers without throwing unnecessary exceptions (`Result.Success()`, `Result.NotFound()`).

### 🔐 Security & Identity
- **JWT Authentication:** Secure endpoints using Bearer tokens.
- **Role-Based Authorization:** Strict access control (e.g., `[Authorize(Roles = "Admin")]` for mutating data, while reads are open/restricted to users).

### ⚡ Performance & Reliability
- **Pagination & Filtering:** Efficiently querying large datasets using query parameters (`?PageNumber=1&PageSize=10&Search=Ukraine`).
- **Output Caching:** Reduced database load by caching responses for frequently requested data (countries list).
- **Rate Limiting:** Protected endpoints from abuse/DDoS using Fixed Window Rate Limiting.

### 📖 API Design & Developer Experience
- **API Versioning:** URL-segment versioning (`api/v1/...` and `api/v2/...`) with deprecation handling for smooth client transitions.
- **Advanced Swagger UI:** Fully documented endpoints with XML summaries, explicit HTTP response types (`[ProducesResponseType]`), and integrated JWT authorization locks.
- **Global Exception Handling:** Centralized middleware to catch unhandled errors and return standardized JSON error responses (Problem Details) without exposing stack traces.
- **Partial Updates:** Supported `HTTP PATCH` using JSON Patch documents.

## 🌍 Live API Documentation (Swagger)
The API was successfully deployed to a **Microsoft Azure App Service**. 

👉 **[Test the Live API Here (Swagger UI)](https://hotellistingapi-gsbhaddug8g0dcet.polandcentral-01.azurewebsites.net/swagger/index.html)**

> **Note:** The API is hosted on a free Azure F1 tier. If it hasn't been accessed recently, the first request may take 15-20 seconds to wake up the server. Additionally, this link is part of a 30-day trial and may become inactive in the future. See the screenshots below for proof of deployment.

Below is a snapshot of the live Swagger UI in production:
<img width="1665" height="1048" alt="image" src="https://github.com/user-attachments/assets/8a6cd42f-7d01-4086-a29f-b2e4d12d086c" />


## 🚀 CI/CD Pipeline & Deployment
This project implements a fully automated CI/CD pipeline using **GitHub Actions**. 
Every push to the `master` branch triggers a workflow that automatically builds the .NET 9.0 application and deploys it to Azure.

<img width="801" height="234" alt="image" src="https://github.com/user-attachments/assets/6d76f44c-9ef2-4ed4-b333-21d1dd34b4d7" />


## 🛠️ How to Run (Locally)

1. Clone the repository.
2. Update the `appsettings.json` with your SQL Server connection string and JWT Secret Key.
3. Open the Package Manager Console and run `Update-Database` to apply EF Core migrations and seed initial data.
4. Run the application. Swagger UI will open automatically at `/swagger`.
