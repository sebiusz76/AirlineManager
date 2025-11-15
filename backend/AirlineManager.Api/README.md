# Airline Manager API

API do zarz¹dzania liniami lotniczymi z systemem rejestracji i logowania u¿ytkowników.

## Architektura

Projekt wykorzystuje **Clean Architecture** z nastêpuj¹cymi warstwami:

- **AirlineManager.Domain** - Encje domenowe (ApplicationUser)
- **AirlineManager.Application** - Logika biznesowa, CQRS z MediatR, DTOs, Validators
- **AirlineManager.Infrastructure** - Implementacja bazy danych, Identity, JWT
- **AirlineManager.Api** - Web API, Controllers, Endpoints

## Technologie

- .NET 10.0
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- MediatR (CQRS pattern)
- FluentValidation
- Scalar (API Documentation)

## Konfiguracja

### 1. Konfiguracja Connection String

Aplikacja wymaga konfiguracji connection string do bazy danych SQL Server.

#### Opcja A: U¿ycie User Secrets (Rekomendowane dla Development)

```bash
cd AirlineManager.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=AirlineManagerDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

#### Opcja B: Plik appsettings.Development.json (ju¿ skonfigurowany dla LocalDB)

Domyœlnie projekt jest skonfigurowany do u¿ycia LocalDB:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AirlineManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 2. Konfiguracja JWT Settings

#### Opcja A: User Secrets (Rekomendowane dla Production)

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG"
dotnet user-secrets set "JwtSettings:Issuer" "AirlineManagerApi"
dotnet user-secrets set "JwtSettings:Audience" "AirlineManagerClient"
dotnet user-secrets set "JwtSettings:ExpiryMinutes" "60"
```

#### Opcja B: appsettings.Development.json (Development Only)

Ju¿ skonfigurowane w pliku `appsettings.Development.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForDevelopmentAtLeast32CharactersLong!",
    "Issuer": "AirlineManagerApi",
    "Audience": "AirlineManagerClient",
    "ExpiryMinutes": "60"
  }
}
```

?? **UWAGA**: Dla œrodowiska produkcyjnego ZAWSZE u¿ywaj User Secrets lub zmiennych œrodowiskowych!

### 3. Migracja bazy danych

```bash
cd AirlineManager.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../AirlineManager.Api
dotnet ef database update --startup-project ../AirlineManager.Api
```

### 4. Uruchomienie aplikacji

```bash
cd AirlineManager.Api
dotnet run
```

Aplikacja bêdzie dostêpna pod adresem: `https://localhost:5001` (lub innym portem wyœwietlonym w konsoli)

### 5. Dokumentacja API

Po uruchomieniu aplikacji dokumentacja API (Scalar) bêdzie dostêpna pod adresem:
- Swagger UI: `https://localhost:5001/openapi/v1.json`
- Scalar UI: `https://localhost:5001/scalar/v1`

## Endpointy API

### Auth Controller

#### POST /api/auth/register
Rejestracja nowego u¿ytkownika

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "Jan",
  "lastName": "Kowalski"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "firstName": "Jan",
  "lastName": "Kowalski"
}
```

#### POST /api/auth/login
Logowanie u¿ytkownika

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "firstName": "Jan",
  "lastName": "Kowalski"
}
```

## Wymagania has³a

- Minimum 6 znaków
- Przynajmniej jedna cyfra
- Przynajmniej jedna ma³a litera
- Przynajmniej jedna wielka litera
- Przynajmniej jeden znak specjalny

## Bezpieczeñstwo

- Has³a s¹ hashowane przez ASP.NET Core Identity
- JWT tokeny s¹ podpisane kluczem tajnym
- Sensitive data (connection strings, JWT secret) powinny byæ przechowywane w User Secrets lub zmiennych œrodowiskowych
- Pliki `appsettings.json` i `appsettings.Development.json` s¹ ignorowane przez Git
- U¿yj pliku `appsettings.json.template` jako szablon dla w³asnej konfiguracji

## Build

```bash
dotnet build AirlineManager.sln
```

## Struktura projektu

```
backend/
??? AirlineManager.Domain/
?   ??? Entities/
?       ??? ApplicationUser.cs
??? AirlineManager.Application/
?   ??? DTOs/
?   ?   ??? Auth/
?   ??? Features/
?   ?   ??? Auth/
?   ?       ??? Commands/
?   ?       ?   ??? Register/
?   ?       ?   ??? Login/
?   ??? Common/
?   ?   ??? Interfaces/
?   ?   ??? Models/
?   ??? DependencyInjection.cs
??? AirlineManager.Infrastructure/
?   ??? Data/
?   ?   ??? ApplicationDbContext.cs
?   ??? Authentication/
?   ?   ??? JwtTokenGenerator.cs
?   ??? DependencyInjection.cs
??? AirlineManager.Api/
    ??? Controllers/
    ?   ??? AuthController.cs
    ??? Program.cs
    ??? appsettings.json.template
```

## Licencja

MIT
