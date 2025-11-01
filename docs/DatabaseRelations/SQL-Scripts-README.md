# ?? Skrypty weryfikacji relacji w bazie danych

## ?? Dost�pne skrypty

### 1. **Quick-Relationship-Check.sql** ? SZYBKI
**Przeznaczenie**: Szybkie sprawdzenie statusu wszystkich relacji  
**Czas wykonania**: ~2-3 sekundy  
**U�yj gdy**: Chcesz szybko sprawdzi� co jest zaimplementowane

**Sprawdza:**
- ? Czy wszystkie 6 Foreign Keys istniej�
- ? Delete behavior ka�dej relacji
- ? Procentowy post�p implementacji
- ? Lista brakuj�cych relacji

**Jak uruchomi�:**
```sql
-- W SQL Server Management Studio (SSMS):
-- 1. File ? Open ? File...
-- 2. Wybierz: Quick-Relationship-Check.sql
-- 3. Naci�nij F5 lub Execute

-- W Azure Data Studio:
-- 1. File ? Open File...
-- 2. Wybierz: Quick-Relationship-Check.sql
-- 3. Naci�nij F5 lub Run
```

**Przyk�adowy output:**
```
?? QUICK RELATIONSHIP CHECK
==========================

Relation     Status  On Delete  FK Column            References
1??  UserLoginHistory          ?      CASCADE    UserLoginHistories.UserId    AspNetUsers.Id
2??  UserSession            ?      CASCADE    UserSessions.UserId          AspNetUsers.Id
...

?? Summary:
-----------
Implemented: 6/6 Foreign Keys
Progress: 100%

? All relations implemented!
```

---

### 2. **Comprehensive-Relationship-Check.sql** ?? SZCZEGӣOWY
**Przeznaczenie**: Kompleksowa weryfikacja wszystkich relacji z testami  
**Czas wykonania**: ~10-15 sekund  
**U�yj gdy**: Chcesz szczeg�owo przetestowa� integralno�� bazy

**Sprawdza:**
1. ? **Wszystkie Foreign Keys** - lista z szczeg�ami
2. ? **Szczeg�owa weryfikacja** - ka�da relacja osobno
3. ? **Testy integralno�ci** - pr�ba dodania nieprawid�owych rekord�w
4. ? **Testy Cascade Delete** - weryfikacja automatycznego usuwania
5. ? **Statystyki** - liczba u�ytkownik�w i powi�zanych rekord�w
6. ? **Przyk�adowe dane** - rzeczywiste dane z JOIN
7. ? **Podsumowanie** - post�p i rekomendacje

**Jak uruchomi�:**
```sql
-- W SSMS lub Azure Data Studio:
-- 1. Otw�rz plik: Comprehensive-Relationship-Check.sql
-- 2. Naci�nij F5
-- 3. Przeczytaj wyniki w Messages i Results
```

**Przyk�adowy output:**
```
================================================
SPRAWDZANIE WSZYSTKICH RELACJI W AIRLINEMANAGER
================================================

CZʌ� 1: WSZYSTKIE FOREIGN KEYS W BAZIE
[Tabela z wszystkimi FK]

CZʌ� 2: SZCZEGӣOWA WERYFIKACJA RELACJI
--- RELACJA 1: ApplicationUser ? UserLoginHistory ---
? FK_UserLoginHistories_AspNetUsers_UserId: EXISTS
[Szczeg�y relacji]

CZʌ� 3: TESTY INTEGRALNO�CI REFERENCYJNEJ
--- TEST 1: UserLoginHistory Foreign Key Constraint ---
? PASSED: FK constraint works correctly

CZʌ� 4: TESTY CASCADE DELETE
Creating test user and related records...
? UserLoginHistory: Cascade Delete WORKS
? UserSession: Cascade Delete WORKS

CZʌ� 5: STATYSTYKI BAZY DANYCH
[Liczba u�ytkownik�w, sesji, log�w]

CZʌ� 6: PRZYK�ADOWE DANE Z RELACJAMI
[Rzeczywiste rekordy z JOIN]

CZʌ� 7: PODSUMOWANIE
Implementation Progress: 100%
1. UserLoginHistory: ? IMPLEMENTED
2. UserSession: ? IMPLEMENTED
...
```

---

## ?? Por�wnanie skrypt�w

| Cecha | Quick Check ? | Comprehensive Check ?? |
|-------|---------------|------------------------|
| **Czas wykonania** | 2-3 sek | 10-15 sek |
| **Sprawdza FK** | ? | ? |
| **Sprawdza Delete Behavior** | ? | ? |
| **Testy integralno�ci** | ? | ? |
| **Testy Cascade Delete** | ? | ? |
| **Statystyki** | ? | ? |
| **Przyk�adowe dane** | ? | ? |
| **Rekomendacje** | ?? Podstawowe | ? Szczeg�owe |
| **U�yj do** | Szybki check | Pe�na weryfikacja |

---

## ?? Kiedy u�y� kt�rego skryptu?

### Quick Check ? - u�yj gdy:
- ? Chcesz szybko sprawdzi� status implementacji
- ? Sprawdzasz czy migracje zosta�y zastosowane
- ? Weryfikujesz po deployment
- ? Robisz daily check
- ? Potrzebujesz szybkiej informacji

### Comprehensive Check ?? - u�yj gdy:
- ? Implementujesz nowe relacje
- ? Debugujesz problemy z FK
- ? Testujesz Cascade Delete
- ? Przygotowujesz si� do production deployment
- ? Chcesz zobaczy� przyk�adowe dane
- ? Potrzebujesz pe�nego raportu

---

## ?? Szybki start

### Krok 1: Otw�rz SSMS lub Azure Data Studio
```
1. Po��cz si� z baz�: AirlineManager-Dev
2. File ? Open ? File...
3. Wybierz odpowiedni skrypt
```

### Krok 2: Uruchom skrypt
```
Naci�nij F5 lub kliknij Execute
```

### Krok 3: Przeanalizuj wyniki
```
- Quick Check: Sprawd� tabel� wynik�w
- Comprehensive: Przeczytaj ca�y output w Messages
```

---

## ?? Rozwi�zywanie problem�w

### Problem: "Invalid object name 'AirlineManager-Dev'"
**Rozwi�zanie:**
```sql
-- Na pocz�tku skryptu zmie� nazw� bazy:
USE [Twoja-Nazwa-Bazy];
```

### Problem: "Permission denied"
**Rozwi�zanie:**
- Upewnij si� �e masz uprawnienia SELECT na tabelach systemowych
- Zaloguj si� jako u�ytkownik z wy�szymi uprawnieniami

### Problem: "FK nie pokazuje si� na diagramie mimo �e istnieje w bazie"
**Rozwi�zanie:**
1. Uruchom Quick Check - je�li FK istnieje w wynikach, problem jest w narz�dziu
2. Od�wie� diagram (F5)
3. Utw�rz nowy diagram od zera
4. U�yj innego narz�dzia (Azure Data Studio, DbSchema)

---

## ?? Interpretacja wynik�w

### ? Pozytywne znaki
```
? IMPLEMENTED - relacja istnieje i dzia�a
? PASSED - test przeszed� pomy�lnie
? CORRECT - konfiguracja jest prawid�owa
Progress: 100% - wszystko zaimplementowane
```

### ? Negatywne znaki
```
? MISSING - relacja nie istnieje
? FAILED - test nie przeszed�
? INCORRECT - b��dna konfiguracja
```

### ?? Ostrze�enia
```
?? PARTIAL - cz�ciowa implementacja
?? CHECK NEEDED - wymaga sprawdzenia
?? No test data - brak danych do testu
```

---

## ?? Co sprawdzaj� testy?

### Test 1: Foreign Key Constraint
**Cel**: Sprawdzenie czy FK blokuje nieprawid�owe dane  
**Metoda**: Pr�ba dodania rekordu z nieistniej�cym UserId  
**Oczekiwany wynik**: B��d FK constraint violation

### Test 2: Cascade Delete
**Cel**: Sprawdzenie czy usuni�cie parent usuwa child records  
**Metoda**: 
1. Tworzenie testowego u�ytkownika
2. Dodanie powi�zanych rekord�w
3. Usuni�cie u�ytkownika
4. Sprawdzenie czy powi�zane rekordy zosta�y usuni�te  
**Oczekiwany wynik**: Wszystkie powi�zane rekordy usuni�te

---

## ?? Dobre praktyki

### ? DO:
- Uruchamiaj Quick Check codziennie
- Uruchamiaj Comprehensive Check przed deploymentem
- Zapisuj output do pliku dla dokumentacji
- Uruchamiaj testy po ka�dej migracji
- Sprawdzaj wyniki przed kodem u�ywaj�cym relacji

### ? DON'T:
- Nie modyfikuj skrypt�w bez zrozumienia konsekwencji
- Nie uruchamiaj na produkcji bez testu
- Nie ignoruj ostrze�e�
- Nie zak�adaj �e diagram jest prawd� (sprawd� w bazie!)

---

## ?? Wsparcie

### Pytania?
1. Sprawd� dokumentacj� relacji w `docs/DatabaseRelations/`
2. Zobacz przyk�ady u�ycia w kodzie
3. Uruchom Comprehensive Check i przeanalizuj wyniki

### B��dy?
1. Sprawd� czy nazwa bazy jest poprawna w skrypcie
2. Zweryfikuj uprawnienia u�ytkownika
3. Uruchom Quick Check aby zobaczy� co jest zaimplementowane

---

## ?? Przyk�adowe scenariusze u�ycia

### Scenariusz 1: Sprawdzenie po migracji
```sql
-- 1. Uruchom Quick Check
-- 2. Sprawd� czy nowa relacja jest na li�cie
-- 3. Je�li nie ma, sprawd� czy migracja zosta�a zastosowana:
dotnet ef migrations list
```

### Scenariusz 2: Debug problemu z relacj�
```sql
-- 1. Uruchom Comprehensive Check
-- 2. Sprawd� sekcj� "TESTY INTEGRALNO�CI"
-- 3. Je�li test FAILED, FK nie istnieje lub jest nieprawid�owy
-- 4. Sprawd� migracje i ApplicationDbContext
```

### Scenariusz 3: Weryfikacja przed production
```bash
# 1. Uruchom Comprehensive Check
# 2. Zapisz wyniki do pliku
# 3. Sprawd� czy wszystkie testy PASSED
# 4. Zweryfikuj Cascade Delete behavior
# 5. Przejrzyj statystyki i przyk�adowe dane
```

---

**Ostatnia aktualizacja**: 2024-11-01  
**Wersja skrypt�w**: 2.0  
**Wspierane relacje**: 5 (UserLoginHistory, UserSession, UserAuditLog�2, IdentityRole)
