#!/bin/bash

echo "?? Starting AirlineManager with Docker Compose..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "? Docker is not running. Please start Docker Desktop."
    exit 1
fi

echo "? Docker is running"
echo ""

# Check if .env file exists
if [ ! -f .env ]; then
    echo "??  .env file not found. Creating from .env.example..."
    cp .env.example .env
    echo "? .env file created. Please review and update if needed."
  echo ""
fi

# Stop any existing containers
echo "?? Stopping existing containers..."
docker-compose down

echo ""
echo "???  Building and starting containers..."
docker-compose up --build -d

echo ""
echo "? Waiting for SQL Server to be healthy..."
timeout=60
elapsed=0
while [ $elapsed -lt $timeout ]; do
    health=$(docker inspect --format='{{.State.Health.Status}}' airlinemanager-sqlserver 2>/dev/null)
    if [ "$health" = "healthy" ]; then
        echo "? SQL Server is healthy!"
        break
 fi
    echo "   Waiting... ($elapsed seconds)"
    sleep 5
    elapsed=$((elapsed + 5))
done

if [ "$health" != "healthy" ]; then
    echo "? SQL Server did not become healthy in time"
    echo "   Check logs with: docker-compose logs sqlserver"
    exit 1
fi

echo ""
echo "? Waiting for application to start..."
sleep 10

echo ""
echo "? Application is starting!"
echo ""
echo "?? Access points:"
echo " - Application: http://localhost:5000"
echo "   - SQL Server: localhost:1433"
echo "   - User: sa"
echo "   - Password: YourStrong@Passw0rd"
echo ""
echo "?? Useful commands:"
echo "   - View logs: docker-compose logs -f webapp"
echo "   - Stop: docker-compose down"
echo "   - Restart: docker-compose restart webapp"
echo ""
echo "?? Done! Opening browser..."
sleep 2

# Try to open browser (works on most systems)
if command -v xdg-open > /dev/null; then
    xdg-open http://localhost:5000
elif command -v open > /dev/null; then
    open http://localhost:5000
elif command -v start > /dev/null; then
    start http://localhost:5000
fi
