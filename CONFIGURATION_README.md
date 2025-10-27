# Application Configuration System

System zarządzania konfiguracją aplikacji przechowywany w bazie danych.

## 🎯 Funkcjonalności

- ✅ Przechowywanie konfiguracji w bazie danych
- ✅ Szyfrowanie wrażliwych danych (hasła SMTP)
- ✅ Panel administracyjny do zarządzania konfiguracją
- ✅ Kategorie konfiguracji (SMTP, General, Security, itp.)
- ✅ Historia zmian (kto i kiedy zmodyfikował)
- ✅ Test połączenia SMTP
- ✅ API do odczytu konfiguracji z aplikacji

## 📋 Struktura tabeli `AppConfigurations`

| Kolumna | Typ | Opis |
|---------|-----|------|
| `Id` | int | Primary key |
| `Key` | string(200) | Unikalny klucz konfiguracji |
| `Value` | string | Wartość (zaszyfrowana jeśli `IsEncrypted=true`) |
| `Description` | string(500) | Opis opcji konfiguracyjnej |
| `Category` | string(50) | Kategoria (SMTP, General, Security) |
| `IsEncrypted` | bool | Czy wartość jest zaszyfrowana |
| `LastModified` | DateTime | Data ostatniej modyfikacji |
| `LastModifiedBy` | string(256) | Email użytkownika który zmodyfikował |

## 📧 Domyślna konfiguracja SMTP

System automatycznie tworzy następujące wpisy konfiguracyjne dla SMTP:

- `SMTP_Host` - Hostname serwera SMTP (np. smtp.gmail.com)
- `SMTP_Port` - Port serwera SMTP (587, 465, 25)
- `SMTP_Username` - Nazwa użytkownika do autentykacji
- `SMTP_Password` - Hasło (⚠️ **zaszyfrowane**)
- `SMTP_FromEmail` - Adres email nadawcy
- `SMTP_FromName` - Nazwa wyświetlana nadawcy
- `SMTP_EnableSSL` - Włącz SSL/TLS (true/false)

## 🔐 Bezpieczeństwo

### Szyfrowanie
- Hasła i wrażliwe dane są szyfrowane AES-256
- Klucz szyfrowania powinien być przechowywany w bezpiecznym miejscu:
  - **Development:** `appsettings.Development.json`
  - **Production:** Azure Key Vault, AWS Secrets Manager, Environment Variables

### Uprawnienia
- **Tylko SuperAdmin** ma dostęp do panelu konfiguracji
- Dostęp przez: `/Admin/Configuration`

## 💻 Użycie w kodzie

### 1. Dependency Injection

```csharp
public class MyService
{
    private readonly IConfigurationService _config;
    
    public MyService(IConfigurationService config)
    {
        _config = config;
    }
}
```

### 2. Odczyt pojedynczej wartości

```csharp
// String value
var smtpHost = await _config.GetValueAsync("SMTP_Host");

// Typed value (int, bool, etc.)
var smtpPort = await _config.GetValueAsync<int>("SMTP_Port");
var enableSsl = await _config.GetValueAsync<bool>("SMTP_EnableSSL");
```

### 3. Odczyt całej kategorii

```csharp
var smtpConfig = await _config.GetCategoryAsync("SMTP");

var host = smtpConfig["SMTP_Host"];
var port = int.Parse(smtpConfig["SMTP_Port"]);
var username = smtpConfig["SMTP_Username"];
var password = smtpConfig["SMTP_Password"]; // Automatycznie odszyfrowane
```

### 4. Zapis wartości

```csharp
await _config.SetValueAsync("SMTP_Host", "smtp.gmail.com", "admin@example.com");
```

## 📧 Przykład: Wysyłanie emaila z konfiguracją SMTP

```csharp
public class EmailService
{
  private readonly IConfigurationService _config;
    
    public EmailService(IConfigurationService config)
    {
        _config = config;
    }
  
    public async Task SendEmailAsync(string to, string subject, string body)
    {
      var smtpConfig = await _config.GetCategoryAsync("SMTP");
 
        var host = smtpConfig["SMTP_Host"];
   var port = int.Parse(smtpConfig["SMTP_Port"]);
        var username = smtpConfig["SMTP_Username"];
   var password = smtpConfig["SMTP_Password"];
     var fromEmail = smtpConfig["SMTP_FromEmail"];
        var fromName = smtpConfig["SMTP_FromName"];
        var enableSsl = bool.Parse(smtpConfig["SMTP_EnableSSL"]);
        
        using var client = new SmtpClient(host, port);
   client.EnableSsl = enableSsl;
        client.Credentials = new NetworkCredential(username, password);
   
      var message = new MailMessage
     {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
    Body = body,
     IsBodyHtml = true
        };
        message.To.Add(to);
        
    await client.SendMailAsync(message);
    }
}
```

## 🎨 Panel administracyjny

### Dostęp
`/Admin/Configuration`

### Funkcje
- **Filtrowanie po kategorii** - przycisk dla każdej kategorii
- **Edycja wartości** - kliknij "Edit" obok wpisu
- **Test SMTP** - przycisk "Test SMTP" w kategorii SMTP (wysyła email testowy)
- **Historia zmian** - wyświetla kto i kiedy zmodyfikował wartość

### Widok edycji
- **Pola zaszyfrowane** wyświetlają `••••••••`
- **Input type** automatycznie dopasowywany:
  - `password` dla zaszyfrowanych wartości
  - `number` dla portów/timeoutów
  - `select` dla wartości boolean
  - `textarea` dla długich tekstów

## 🔄 Dodawanie nowych kategorii konfiguracyjnych

### 1. Dodaj wpis do bazy danych

```csharp
// W migration lub przez SQL
builder.Entity<AppConfiguration>().HasData(
    new AppConfiguration
    {
    Id = 8,
        Key = "App_Name",
    Value = "AirlineManager",
        Description = "Application display name",
        Category = "General",
        IsEncrypted = false,
        LastModified = DateTime.UtcNow,
        LastModifiedBy = "System"
    }
);
```

### 2. Użyj w kodzie

```csharp
var appName = await _config.GetValueAsync("App_Name");
```

## ⚠️ Ważne uwagi

### Klucz szyfrowania
W produkcji **NIGDY** nie przechowuj klucza szyfrowania w kodzie. Użyj:

```csharp
// appsettings.json (Development)
{
  "Encryption": {
    "Key": "your-32-character-encryption-key-here!"
  }
}

// Program.cs
var encryptionKey = builder.Configuration["Encryption:Key"];
```

### Environment Variables (Production)
```bash
export ENCRYPTION_KEY="your-secure-key-here"
```

```csharp
var encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
```

### Azure Key Vault (Recommended for Production)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

var encryptionKey = builder.Configuration["EncryptionKey"];
```

## 🚀 Migration

Migration `AddAppConfiguration` automatycznie:
- Tworzy tabelę `AppConfigurations`
- Dodaje indeksy (unique na `Key`, na `Category`)
- Seeduje domyślną konfigurację SMTP

Uruchom migration:
```bash
dotnet ef database update
```

## 📝 TODO / Przyszłe rozszerzenia

- [ ] Walidacja wartości (regex, zakresy)
- [ ] Export/Import konfiguracji (JSON)
- [ ] Wersjonowanie zmian (audit log)
- [ ] Caching wartości konfiguracyjnych
- [ ] Grupowanie konfiguracji (np. SMTP_Primary, SMTP_Backup)
- [ ] UI dla dodawania nowych wpisów konfiguracyjnych
- [ ] Role-based access (niektóre kategorie tylko dla SuperAdmin)
