# ?? Skrypty weryfikacji relacji w bazie danych

## ?? Dostêpne skrypty

### 1. **Quick-Relationship-Check.sql** ? SZYBKI
**Przeznaczenie**: Szybkie sprawdzenie statusu wszystkich relacji  
**Czas wykonania**: ~2-3 sekundy  
**U¿yj gdy**: Chcesz szybko sprawdziæ co jest zaimplementowane

**Sprawdza:**
- ? Czy wszystkie 6 Foreign Keys istniej¹
- ? Delete behavior ka¿dej relacji
- ? Procentowy postêp implementacji
- ? Lista brakuj¹cych relacji

**Jak uruchomiæ:**
```sql
-- W SQL Server Management Studio (SSMS):
-- 1. File ? Open ? File...
-- 2. Wybierz: Quick-Relationship-Check.sql
-- 3. Naciœnij F5 lub Execute

-- W Azure Data Studio:
-- 1. File ? Open File...
-- 2. Wybierz: Quick-Relationship-Check.sql
-- 3. Naciœnij F5 lub Run
```

**Przyk³adowy output:**
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

### 2. **Comprehensive-Relationship-Check.sql** ?? SZCZEGÓ£OWY
**Przeznaczenie**: Kompleksowa weryfikacja wszystkich relacji z testami  
**Czas wykonania**: ~10-15 sekund  
**U¿yj gdy**: Chcesz szczegó³owo przetestowaæ integralnoœæ bazy

**Sprawdza:**
1. ? **Wszystkie Foreign Keys** - lista z szczegó³ami
2. ? **Szczegó³owa weryfikacja** - ka¿da relacja osobno
3. ? **Testy integralnoœci** - próba dodania nieprawid³owych rekordów
4. ? **Testy Cascade Delete** - weryfikacja automatycznego usuwania
5. ? **Statystyki** - liczba u¿ytkowników i powi¹zanych rekordów
6. ? **Przyk³adowe dane** - rzeczywiste dane z JOIN
7. ? **Podsumowanie** - postêp i rekomendacje

**Jak uruchomiæ:**
```sql
-- W SSMS lub Azure Data Studio:
-- 1. Otwórz plik: Comprehensive-Relationship-Check.sql
-- 2. Naciœnij F5
-- 3. Przeczytaj wyniki w Messages i Results
```

**Przyk³adowy output:**
```
================================================
SPRAWDZANIE WSZYSTKICH RELACJI W AIRLINEMANAGER
================================================

CZÊŒÆ 1: WSZYSTKIE FOREIGN KEYS W BAZIE
[Tabela z wszystkimi FK]

CZÊŒÆ 2: SZCZEGÓ£OWA WERYFIKACJA RELACJI
--- RELACJA 1: ApplicationUser ? UserLoginHistory ---
? FK_UserLoginHistories_AspNetUsers_UserId: EXISTS
[Szczegó³y relacji]

CZÊŒÆ 3: TESTY INTEGRALNOŒCI REFERENCYJNEJ
--- TEST 1: UserLoginHistory Foreign Key Constraint ---
? PASSED: FK constraint works correctly

CZÊŒÆ 4: TESTY CASCADE DELETE
Creating test user and related records...
? UserLoginHistory: Cascade Delete WORKS
? UserSession: Cascade Delete WORKS

CZÊŒÆ 5: STATYSTYKI BAZY DANYCH
[Liczba u¿ytkowników, sesji, logów]

CZÊŒÆ 6: PRZYK£ADOWE DANE Z RELACJAMI
[Rzeczywiste rekordy z JOIN]

CZÊŒÆ 7: PODSUMOWANIE
Implementation Progress: 100%
1. UserLoginHistory: ? IMPLEMENTED
2. UserSession: ? IMPLEMENTED
...
```

---

## ?? Porównanie skryptów

| Cecha | Quick Check ? | Comprehensive Check ?? |
|-------|---------------|------------------------|
| **Czas wykonania** | 2-3 sek | 10-15 sek |
| **Sprawdza FK** | ? | ? |
| **Sprawdza Delete Behavior** | ? | ? |
| **Testy integralnoœci** | ? | ? |
| **Testy Cascade Delete** | ? | ? |
| **Statystyki** | ? | ? |
| **Przyk³adowe dane** | ? | ? |
| **Rekomendacje** | ?? Podstawowe | ? Szczegó³owe |
| **U¿yj do** | Szybki check | Pe³na weryfikacja |

---

## ?? Kiedy u¿yæ którego skryptu?

### Quick Check ? - u¿yj gdy:
- ? Chcesz szybko sprawdziæ status implementacji
- ? Sprawdzasz czy migracje zosta³y zastosowane
- ? Weryfikujesz po deployment
- ? Robisz daily check
- ? Potrzebujesz szybkiej informacji

### Comprehensive Check ?? - u¿yj gdy:
- ? Implementujesz nowe relacje
- ? Debugujesz problemy z FK
- ? Testujesz Cascade Delete
- ? Przygotowujesz siê do production deployment
- ? Chcesz zobaczyæ przyk³adowe dane
- ? Potrzebujesz pe³nego raportu

---

## ?? Szybki start

### Krok 1: Otwórz SSMS lub Azure Data Studio
```
1. Po³¹cz siê z baz¹: AirlineManager-Dev
2. File ? Open ? File...
3. Wybierz odpowiedni skrypt
```

### Krok 2: Uruchom skrypt
```
Naciœnij F5 lub kliknij Execute
```

### Krok 3: Przeanalizuj wyniki
```
- Quick Check: SprawdŸ tabelê wyników
- Comprehensive: Przeczytaj ca³y output w Messages
```

---

## ?? Rozwi¹zywanie problemów

### Problem: "Invalid object name 'AirlineManager-Dev'"
**Rozwi¹zanie:**
```sql
-- Na pocz¹tku skryptu zmieñ nazwê bazy:
USE [Twoja-Nazwa-Bazy];
```

### Problem: "Permission denied"
**Rozwi¹zanie:**
- Upewnij siê ¿e masz uprawnienia SELECT na tabelach systemowych
- Zaloguj siê jako u¿ytkownik z wy¿szymi uprawnieniami

### Problem: "FK nie pokazuje siê na diagramie mimo ¿e istnieje w bazie"
**Rozwi¹zanie:**
1. Uruchom Quick Check - jeœli FK istnieje w wynikach, problem jest w narzêdziu
2. Odœwie¿ diagram (F5)
3. Utwórz nowy diagram od zera
4. U¿yj innego narzêdzia (Azure Data Studio, DbSchema)

---

## ?? Interpretacja wyników

### ? Pozytywne znaki
```
? IMPLEMENTED - relacja istnieje i dzia³a
? PASSED - test przeszed³ pomyœlnie
? CORRECT - konfiguracja jest prawid³owa
Progress: 100% - wszystko zaimplementowane
```

### ? Negatywne znaki
```
? MISSING - relacja nie istnieje
? FAILED - test nie przeszed³
? INCORRECT - b³êdna konfiguracja
```

### ?? Ostrze¿enia
```
?? PARTIAL - czêœciowa implementacja
?? CHECK NEEDED - wymaga sprawdzenia
?? No test data - brak danych do testu
```

---

## ?? Co sprawdzaj¹ testy?

### Test 1: Foreign Key Constraint
**Cel**: Sprawdzenie czy FK blokuje nieprawid³owe dane  
**Metoda**: Próba dodania rekordu z nieistniej¹cym UserId  
**Oczekiwany wynik**: B³¹d FK constraint violation

### Test 2: Cascade Delete
**Cel**: Sprawdzenie czy usuniêcie parent usuwa child records  
**Metoda**: 
1. Tworzenie testowego u¿ytkownika
2. Dodanie powi¹zanych rekordów
3. Usuniêcie u¿ytkownika
4. Sprawdzenie czy powi¹zane rekordy zosta³y usuniête  
**Oczekiwany wynik**: Wszystkie powi¹zane rekordy usuniête

---

## ?? Dobre praktyki

### ? DO:
- Uruchamiaj Quick Check codziennie
- Uruchamiaj Comprehensive Check przed deploymentem
- Zapisuj output do pliku dla dokumentacji
- Uruchamiaj testy po ka¿dej migracji
- Sprawdzaj wyniki przed kodem u¿ywaj¹cym relacji

### ? DON'T:
- Nie modyfikuj skryptów bez zrozumienia konsekwencji
- Nie uruchamiaj na produkcji bez testu
- Nie ignoruj ostrze¿eñ
- Nie zak³adaj ¿e diagram jest prawd¹ (sprawdŸ w bazie!)

---

## ?? Wsparcie

### Pytania?
1. SprawdŸ dokumentacjê relacji w `docs/DatabaseRelations/`
2. Zobacz przyk³ady u¿ycia w kodzie
3. Uruchom Comprehensive Check i przeanalizuj wyniki

### B³êdy?
1. SprawdŸ czy nazwa bazy jest poprawna w skrypcie
2. Zweryfikuj uprawnienia u¿ytkownika
3. Uruchom Quick Check aby zobaczyæ co jest zaimplementowane

---

## ?? Przyk³adowe scenariusze u¿ycia

### Scenariusz 1: Sprawdzenie po migracji
```sql
-- 1. Uruchom Quick Check
-- 2. SprawdŸ czy nowa relacja jest na liœcie
-- 3. Jeœli nie ma, sprawdŸ czy migracja zosta³a zastosowana:
dotnet ef migrations list
```

### Scenariusz 2: Debug problemu z relacj¹
```sql
-- 1. Uruchom Comprehensive Check
-- 2. SprawdŸ sekcjê "TESTY INTEGRALNOŒCI"
-- 3. Jeœli test FAILED, FK nie istnieje lub jest nieprawid³owy
-- 4. SprawdŸ migracje i ApplicationDbContext
```

### Scenariusz 3: Weryfikacja przed production
```bash
# 1. Uruchom Comprehensive Check
# 2. Zapisz wyniki do pliku
# 3. SprawdŸ czy wszystkie testy PASSED
# 4. Zweryfikuj Cascade Delete behavior
# 5. Przejrzyj statystyki i przyk³adowe dane
```

---

**Ostatnia aktualizacja**: 2024-11-01  
**Wersja skryptów**: 2.0  
**Wspierane relacje**: 5 (UserLoginHistory, UserSession, UserAuditLog×2, IdentityRole)
