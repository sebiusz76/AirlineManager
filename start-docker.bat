@echo off
echo ?? Starting AirlineManager with Docker Compose...
echo.

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ? Docker is not running. Please start Docker Desktop.
  pause
    exit /b 1
)

echo ? Docker is running
echo.

REM Check if .env file exists
if not exist .env (
    echo ??  .env file not found. Creating from .env.example...
    copy .env.example .env
    echo ? .env file created. Please review and update if needed.
    echo.
)

REM Stop any existing containers
echo ?? Stopping existing containers...
docker-compose down

echo.
echo ???  Building and starting containers...
docker-compose up --build -d

echo.
echo ? Waiting for SQL Server to be healthy...
set /a timeout=60
set /a elapsed=0

:wait_loop
if %elapsed% geq %timeout% goto timeout_error

timeout /t 5 /nobreak >nul
set /a elapsed+=5

docker inspect --format="{{.State.Health.Status}}" airlinemanager-sqlserver 2>nul | find "healthy" >nul
if errorlevel 1 (
    echo    Waiting... (%elapsed% seconds)
    goto wait_loop
)

echo ? SQL Server is healthy!
echo.

echo ? Waiting for application to start...
timeout /t 10 /nobreak >nul

echo.
echo ? Application is starting!
echo.
echo ?? Access points:
echo  - Application: http://localhost:5000
echo  - SQL Server: localhost:1433
echo    - User: sa
echo    - Password: YourStrong@Passw0rd
echo.
echo ?? Useful commands:
echo    - View logs: docker-compose logs -f webapp
echo    - Stop: docker-compose down
echo    - Restart: docker-compose restart webapp
echo.
echo ?? Done! Opening browser...

timeout /t 2 /nobreak >nul
start http://localhost:5000

pause
exit /b 0

:timeout_error
echo ? SQL Server did not become healthy in time
echo    Check logs with: docker-compose logs sqlserver
pause
exit /b 1
