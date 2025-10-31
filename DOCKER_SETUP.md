# Docker Setup dla AirlineManager

## ?? Uruchamianie Aplikacji w Docker

### Wymagania
- Docker Desktop zainstalowany
- Co najmniej 4GB RAM dla kontenerów
- Port 5000 (aplikacja) i 1433 (SQL Server) dostêpne

---

## ?? Quick Start

### 1. **Build i uruchomienie wszystkiego**
```bash
docker-compose up --build
```

### 2. **Dostêp do aplikacji**
- Aplikacja: http://localhost:5000
- SQL Server: localhost:1433

### 3. **Zatrzymanie**
```bash
docker-compose down
```

### 4. **Zatrzymanie z usuniêciem danych**
```bash
docker-compose down -v
```

---

## ?? Komponenty

### **1. SQL Server Container**
- Image: `mcr.microsoft.com/mssql/server:2022-latest`
- Port: `1433`
- User: `sa`
- Password: `YourStrong@Passw0rd` ?? **ZMIEÑ W PRODUKCJI!**
- Database: `AirlineManager` (tworzona automatycznie przez migracje)
- Healthcheck: Sprawdza czy SQL Server jest gotowy

### **2. Web Application Container**
- Build z lokalnego Dockerfile
- Port: `5000` ? `8080` (wewnêtrzny)
- Environment: `Docker`
- Connection String: Ustawiony przez zmienn¹ œrodowiskow¹
- Depends on: SQL Server (czeka a¿ bêdzie healthy)

---

## ?? Konfiguracja

### **appsettings.Docker.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=AirlineManager;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Uwaga:** `Server=sqlserver` - to nazwa serwisu w docker-compose!

### **Zmienne Œrodowiskowe**
W `docker-compose.yml` mo¿esz zmieniæ:
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Docker
  - ConnectionStrings__DefaultConnection=...
```

---

## ??? Komendy Docker

### **Build tylko obrazu**
```bash
docker-compose build
```

### **Uruchomienie w tle**
```bash
docker-compose up -d
```

### **Sprawdzenie statusu**
```bash
docker-compose ps
```

### **Logi aplikacji**
```bash
docker-compose logs webapp
```

### **Logi SQL Server**
```bash
docker-compose logs sqlserver
```

### **Logi na ¿ywo**
```bash
docker-compose logs -f webapp
```

### **Restart aplikacji**
```bash
docker-compose restart webapp
```

### **Wejœcie do kontenera aplikacji**
```bash
docker exec -it airlinemanager-webapp bash
```

### **Wejœcie do SQL Server**
```bash
docker exec -it airlinemanager-sqlserver bash
```

---

## ??? SQL Server Management

### **Po³¹czenie z SQL Server Management Studio (SSMS)**
```
Server: localhost,1433
Login: sa
Password: YourStrong@Passw0rd
```

### **Po³¹czenie z Azure Data Studio**
```
Connection type: Microsoft SQL Server
Server: localhost,1433
Authentication type: SQL Login
User name: sa
Password: YourStrong@Passw0rd
```

### **Sqlcmd w kontenerze**
```bash
docker exec -it airlinemanager-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -C
```

---

## ?? Diagnostyka

### **Problem: Aplikacja nie startuje**

#### **1. SprawdŸ logi**
```bash
docker-compose logs webapp
```

#### **2. SprawdŸ czy SQL Server jest gotowy**
```bash
docker-compose logs sqlserver | grep "SQL Server is now ready"
```

#### **3. SprawdŸ healthcheck SQL Server**
```bash
docker-compose ps
```
Status powinien byæ `healthy` dla sqlserver.

#### **4. SprawdŸ czy migracje siê wykona³y**
```bash
docker-compose logs webapp | grep "Database migrations"
```

---

### **Problem: Connection timeout**

#### **Rozwi¹zanie 1: Zwiêksz czas oczekiwania**
W `docker-compose.yml`:
```yaml
healthcheck:
  start_period: 30s  # zwiêksz z 10s
  retries: 15        # zwiêksz z 10
```

#### **Rozwi¹zanie 2: Rêczne uruchomienie**
```bash
# 1. Start tylko SQL Server
docker-compose up -d sqlserver

# 2. Poczekaj 30 sekund

# 3. Start aplikacji
docker-compose up webapp
```

---

### **Problem: Port ju¿ zajêty**

#### **Zmiana portu aplikacji**
W `docker-compose.yml`:
```yaml
ports:
  - "5001:8080"  # zmieñ 5000 na 5001
```

#### **Zmiana portu SQL Server**
```yaml
ports:
  - "1434:1433"  # zmieñ 1433 na 1434
```

---

### **Problem: Brak danych po restarcie**

Dane SQL Server s¹ przechowywane w volume:
```bash
# Lista volumes
docker volume ls

# Inspekcja volume
docker volume inspect airlinemanager_sqlserver_data
```

Aby **usun¹æ dane**:
```bash
docker-compose down -v
```

---

## ?? Bezpieczeñstwo

### **?? WA¯NE: Produkcja**

#### **1. Zmieñ has³o SQL Server**
```yaml
environment:
  - SA_PASSWORD=TwojeSecureHaslo123!@#
```

#### **2. U¿yj Docker Secrets**
```yaml
secrets:
  db_password:
    file: ./secrets/db_password.txt
```

#### **3. U¿yj .env file**
```bash
# .env
SA_PASSWORD=TwojeSecureHaslo123!@#
```

```yaml
environment:
  - SA_PASSWORD=${SA_PASSWORD}
```

#### **4. Dodaj .env do .gitignore**
```bash
echo ".env" >> .gitignore
```

---

## ?? Volumes

### **SQL Server Data**
- Volume: `sqlserver_data`
- Mount: `/var/opt/mssql`
- Przechowuje: Bazy danych, logi, backupy

### **Backup bazy danych**
```bash
docker exec airlinemanager-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -C \
  -Q "BACKUP DATABASE AirlineManager TO DISK='/var/opt/mssql/backup/AirlineManager.bak'"
```

### **Restore bazy danych**
```bash
docker exec airlinemanager-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -C \
  -Q "RESTORE DATABASE AirlineManager FROM DISK='/var/opt/mssql/backup/AirlineManager.bak'"
```

---

## ?? Networking

### **Network: airlinemanager-network**
- Type: bridge
- Services: webapp, sqlserver

### **Komunikacja miêdzy kontenerami**
- Webapp ? SQL Server: `sqlserver:1433`
- Host ? Webapp: `localhost:5000`
- Host ? SQL Server: `localhost:1433`

---

## ?? Rebuild po zmianach

### **Po zmianie kodu**
```bash
docker-compose up --build webapp
```

### **Pe³ny rebuild**
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up
```

---

## ?? Monitoring

### **Docker Desktop Dashboard**
- Otwórz Docker Desktop
- Containers ? airlinemanager-webapp
- Zobacz logi, stats, inspect

### **Zu¿ycie zasobów**
```bash
docker stats
```

### **Procesy w kontenerze**
```bash
docker top airlinemanager-webapp
```

---

## ?? Testowanie

### **1. SprawdŸ czy aplikacja odpowiada**
```bash
curl http://localhost:5000
```

### **2. SprawdŸ czy SQL Server dzia³a**
```bash
docker exec airlinemanager-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -C -Q "SELECT @@VERSION"
```

### **3. SprawdŸ czy baza zosta³a utworzona**
```bash
docker exec airlinemanager-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -C \
  -Q "SELECT name FROM sys.databases WHERE name='AirlineManager'"
```

---

## ?? Troubleshooting Checklist

- [ ] Docker Desktop jest uruchomiony
- [ ] Porty 5000 i 1433 s¹ wolne
- [ ] SQL Server container ma status `healthy`
- [ ] Webapp container jest `running`
- [ ] Brak b³êdów w logach: `docker-compose logs`
- [ ] Migracje siê wykona³y pomyœlnie
- [ ] Connection string jest poprawny
- [ ] Has³o SQL Server jest poprawne

---

## ?? Quick Commands Reference

```bash
# Start wszystkiego
docker-compose up -d

# Rebuild i start
docker-compose up --build

# Stop wszystkiego
docker-compose down

# Stop + usuñ dane
docker-compose down -v

# Logi
docker-compose logs -f

# Status
docker-compose ps

# Restart
docker-compose restart

# Build bez cache
docker-compose build --no-cache
```

---

## ?? Dalsze Kroki

### **Development**
1. U¿yj `docker-compose.override.yml` dla lokalnych customizacji
2. Dodaj volume dla hot reload:
```yaml
volumes:
  - ./AirlineManager:/app
```

### **Production**
1. U¿yj Docker Secrets dla hase³
2. Skonfiguruj reverse proxy (nginx/traefik)
3. Dodaj SSL/TLS
4. U¿yj production-ready SQL Server image
5. Skonfiguruj monitoring (Prometheus/Grafana)
6. Dodaj backupy automatyczne

---

**Autor**: GitHub Copilot  
**Data**: 2024  
**Wersja**: 1.0
