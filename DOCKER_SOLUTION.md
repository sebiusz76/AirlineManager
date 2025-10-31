# ?? Docker Setup - Podsumowanie Rozwi�zania

## ? Status: ROZWI�ZANE

Aplikacja AirlineManager zosta�a pomy�lnie skonfigurowana do dzia�ania w kontenerze Docker z SQL Server.

---

## ?? Zidentyfikowane Problemy

### **1. LocalDB nie dzia�a w kontenerze**
```
? Server=(localdb)\\mssqllocaldb
```
- LocalDB to funkcja Windows, kt�ra nie jest dost�pna w kontenerach Linux
- Kontener Docker u�ywa Linux-based images

### **2. Problematyczny USER w Dockerfile**
```dockerfile
? USER $APP_UID  # Zmienna nie ustawiona
```

### **3. Brak konfiguracji dla �rodowiska Docker**
- Tylko `appsettings.json` z LocalDB
- Brak `appsettings.Docker.json`

---

## ? Rozwi�zanie

### **Utworzone Pliki**

#### **1. docker-compose.yml**
```yaml
? SQL Server 2022 container
? Web application container
? Network po��czenie mi�dzy kontenerami
? Healthcheck dla SQL Server
? Volumes dla persistence danych
```

**Funkcje:**
- SQL Server z automatic healthcheck
- Webapp czeka a� SQL Server b�dzie ready
- Data persistence w Docker volumes
- Isolated network

#### **2. appsettings.Docker.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=AirlineManager;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Kluczowe zmiany:**
- `Server=sqlserver` - nazwa service z docker-compose
- `User Id=sa` - SQL Server authentication zamiast Trusted_Connection
- `TrustServerCertificate=True` - dla development

#### **3. Dockerfile (naprawiony)**
```dockerfile
? Usuni�to problematyczny USER $APP_UID
? Dodano ENV ASPNETCORE_ENVIRONMENT=Docker
? Fixed multi-stage build
```

#### **4. .env.example**
```env
SA_PASSWORD=YourStrong@Passw0rd
ASPNETCORE_ENVIRONMENT=Docker
```

#### **5. start-docker.bat / .sh**
```bash
? Sprawdza czy Docker dzia�a
? Tworzy .env je�li nie istnieje
? Czeka na SQL Server healthcheck
? Otwiera browser automatycznie
```

#### **6. DOCKER_SETUP.md**
```markdown
? Pe�na dokumentacja
? Troubleshooting guide
? Quick commands reference
? Security best practices
```

---

## ?? Jak Uruchomi�

### **Metoda 1: Skrypt (Zalecane)**

**Windows:**
```powershell
.\start-docker.bat
```

**Linux/Mac:**
```bash
chmod +x start-docker.sh
./start-docker.sh
```

### **Metoda 2: R�cznie**

```bash
# 1. Build i start
docker-compose up --build

# 2. Aplikacja dost�pna
http://localhost:5000
```

---

## ?? Architektura

```
???????????????????????????????????????????????
?   Host Machine (Windows)            ?
???????????????????????????????????????????????
?        ?
?  ????????????????    ????????????????    ?
?  ?   Browser    ?      ? SSMS / Azure ?    ?
?  ?  localhost   ?    ? Data Studio  ?    ?
?  ?   :5000    ?      ?  :1433       ?    ?
?  ????????????????      ????????????????    ?
?         ?       ? ?
?       ?         ?          ?
?  ?????????????????????????????????????????  ?
?  ?     Docker Network      ?  ?
?  ?    (airlinemanager-network)          ?  ?
?  ? ??
?  ?  ??????????????????  ??????????????? ?  ?
?  ?  ?Web App      ?  ? SQL Server  ? ?  ?
?  ?  ?   Container    ?  ?  Container  ? ?  ?
?  ?  ??????????????????  ??????????????? ?  ?
?  ?  ? .NET 9         ?? ? SQL 2022    ? ?  ?
?  ?  ? Port: 8080     ?  ? Port: 1433  ? ?  ?
?  ?  ? ENV: Docker    ?  ? SA Password ? ?  ?
?  ?  ??????????????????  ??????????????? ?  ?
?  ?            ?       ?  ?
?  ?                  ?????????????????? ?  ?
?  ?         ?  Docker Volume ? ?  ?
?  ?    ?  (sqlserver_   ? ?  ?
?  ?                    ?    data)       ? ?  ?
?  ?    ?????????????????? ?  ?
?  ??????????????????????????????????????????
?            ?
???????????????????????????????????????????????
```

---

## ?? Konfiguracja

### **SQL Server**
```yaml
Image: mcr.microsoft.com/mssql/server:2022-latest
Port: 1433
User: sa
Password: YourStrong@Passw0rd ?? ZMIE� W PRODUKCJI
Database: AirlineManager (auto-created)
Volume: sqlserver_data
```

### **Web Application**
```yaml
Build: z lokalnego Dockerfile
Port: 5000 ? 8080 (internal)
Environment: Docker
Connection String: Via environment variable
Depends on: SQL Server (waits for healthy)
```

### **Network**
```yaml
Type: bridge
Name: airlinemanager-network
Services: webapp, sqlserver
```

---

## ?? Dane Logowania

### **Aplikacja**
```
URL: http://localhost:5000
Default SuperAdmin:
  Email: admin@example.com
  Password: Admin123!
```

### **SQL Server**
```
Server: localhost,1433
Authentication: SQL Server Authentication
Username: sa
Password: YourStrong@Passw0rd
Database: AirlineManager
```

---

## ?? Quick Commands

```bash
# Start wszystkiego
docker-compose up -d

# Start z rebuild
docker-compose up --build

# Stop wszystkiego
docker-compose down

# Stop + usu� dane
docker-compose down -v

# Logi aplikacji
docker-compose logs -f webapp

# Logi SQL Server
docker-compose logs -f sqlserver

# Status kontener�w
docker-compose ps

# Restart aplikacji
docker-compose restart webapp
```

---

## ?? Diagnostyka

### **Problem: Aplikacja nie startuje**

#### **1. Sprawd� logi**
```bash
docker-compose logs webapp
```

#### **2. Sprawd� SQL Server**
```bash
docker-compose logs sqlserver | grep "SQL Server is now ready"
```

#### **3. Sprawd� healthcheck**
```bash
docker-compose ps
# Status powinien by� "healthy" dla sqlserver
```

#### **4. Sprawd� connection string**
```bash
docker exec airlinemanager-webapp env | grep ConnectionStrings
```

### **Problem: Connection timeout**

#### **Rozwi�zanie 1: Zwi�ksz timeout**
W `docker-compose.yml`:
```yaml
healthcheck:
  start_period: 30s  # zwi�ksz z 10s
  retries: 15 # zwi�ksz z 10
```

#### **Rozwi�zanie 2: R�czny start**
```bash
# 1. Start tylko SQL Server
docker-compose up -d sqlserver

# 2. Poczekaj 30 sekund
sleep 30

# 3. Start aplikacji
docker-compose up webapp
```

### **Problem: Port ju� zaj�ty**

#### **Zmie� port aplikacji**
W `docker-compose.yml`:
```yaml
ports:
  - "5001:8080"  # zmie� 5000 na 5001
```

#### **Zmie� port SQL Server**
```yaml
ports:
  - "1434:1433"  # zmie� 1433 na 1434
```

---

## ?? Bezpiecze�stwo

### **?? WA�NE DLA PRODUKCJI**

#### **1. Zmie� has�o SQL Server**
```bash
# W .env
SA_PASSWORD=TwojeSecureHas�o123!@#
```

#### **2. U�yj Docker Secrets**
```yaml
secrets:
  db_password:
    file: ./secrets/db_password.txt

services:
  sqlserver:
    environment:
      - SA_PASSWORD_FILE=/run/secrets/db_password
```

#### **3. Dodaj .env do .gitignore**
```bash
echo ".env" >> .gitignore
git add .gitignore
git commit -m "Ignore .env file"
```

#### **4. U�ywaj HTTPS**
- Skonfiguruj reverse proxy (nginx/traefik)
- Dodaj certyfikaty SSL/TLS
- Wymuszaj HTTPS

---

## ?? Data Persistence

### **SQL Server Data**
```
Volume: sqlserver_data
Mount: /var/opt/mssql
Przechowuje: Databases, logs, backups
```

### **Backup**
```bash
# Backup database
docker exec airlinemanager-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -C \
-Q "BACKUP DATABASE AirlineManager TO DISK='/var/opt/mssql/backup/AirlineManager.bak'"

# Copy backup to host
docker cp airlinemanager-sqlserver:/var/opt/mssql/backup/AirlineManager.bak ./
```

### **Restore**
```bash
# Copy backup to container
docker cp ./AirlineManager.bak airlinemanager-sqlserver:/var/opt/mssql/backup/

# Restore database
docker exec airlinemanager-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -C \
  -Q "RESTORE DATABASE AirlineManager FROM DISK='/var/opt/mssql/backup/AirlineManager.bak'"
```

---

## ? Co Dzia�a

- ? **Build successful** - aplikacja buduje si� bez b��d�w
- ? **SQL Server** - dzia�a w kontenerze z healthcheck
- ? **Web App** - dzia�a w kontenerze z .NET 9
- ? **Connection** - aplikacja ��czy si� z SQL Server
- ? **Migrations** - automatyczne przy starcie
- ? **Data Persistence** - dane zachowane w volumes
- ? **Network** - isolated network dla security
- ? **Configuration** - environment-specific config
- ? **Healthcheck** - webapp czeka na SQL Server
- ? **Logging** - Serilog dzia�a w kontenerze

---

## ?? Nast�pne Kroki

### **Development**
1. ? Dodaj volume mount dla hot reload
2. ? Skonfiguruj docker-compose.override.yml
3. ? Dodaj debugger support

### **Production**
1. ? U�yj production SQL Server image
2. ? Skonfiguruj reverse proxy (nginx/traefik)
3. ? Dodaj SSL/TLS certificates
4. ? Implementuj Docker Secrets
5. ? Skonfiguruj monitoring (Prometheus/Grafana)
6. ? Dodaj automated backups
7. ? Implementuj load balancing
8. ? Skonfiguruj CI/CD pipeline

---

## ?? Dokumentacja

### **Utworzone Pliki**
```
??? docker-compose.yml        # Orchestration
??? Dockerfile (updated)        # Build instructions
??? appsettings.Docker.json# Docker configuration
??? .env.example                # Environment variables template
??? start-docker.bat    # Windows startup script
??? start-docker.sh           # Linux/Mac startup script
??? DOCKER_SETUP.md      # Pe�na dokumentacja (11 stron)
```

### **Linki**
- [Docker Setup Guide](DOCKER_SETUP.md) - Pe�na dokumentacja
- [Docker Compose Docs](https://docs.docker.com/compose/)
- [SQL Server Docker](https://hub.docker.com/_/microsoft-mssql-server)
- [.NET Docker](https://hub.docker.com/_/microsoft-dotnet-aspnet/)

---

## ?? Podsumowanie

### **Co By�o:**
? LocalDB w connection string  
? Brak Docker configuration  
? Problematyczny Dockerfile  
? Aplikacja nie dzia�a�a w kontenerze  

### **Co Jest Teraz:**
? SQL Server 2022 w kontenerze  
? .NET 9 app w kontenerze  
? Docker Compose orchestration  
? Healthchecks i dependencies  
? Data persistence w volumes  
? Environment-specific config  
? Startup scripts (Windows + Linux)  
? Pe�na dokumentacja  
? **APLIKACJA DZIA�A W DOCKER!** ??  

---

**To start the application:**
```bash
.\start-docker.bat         # Windows
# OR
./start-docker.sh          # Linux/Mac
# OR
docker-compose up --build  # Manual
```

**Then open:** http://localhost:5000

---

**Autor**: GitHub Copilot  
**Data**: 2024  
**Status**: ? READY FOR USE
