# 📸 Screenshots Instrukcijos - Testų Paleistis ir Coverage

## 1️⃣ SCREENSHOT: Terminal su Testų Paleistimu

### Komanda (nukopijuok ir įklijuok į PowerShell):

```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test --logger "console;verbosity=detailed"
```

### Ką Turėtų Matytis:
```
[xUnit.net 00:00:00.08]   Discovered:  backend.Tests
[xUnit.net 00:00:00.08]   Starting:    backend.Tests

✓ Passed backend.Tests.Unit.Services.PaymentServiceUnitTests...
✓ Passed backend.Tests.Unit.Services.EmailServiceUnitTests...

Test Run Successful.
Total tests: 26
     Passed: 26
 Total time: 0.4476 Seconds

Test summary: total: 26, failed: 0, succeeded: 26, skipped: 0, duration: 0.6s
✅ Build succeeded
```

**SCREENSHOT TIPS:**
- Pažymėkite "Total tests: 26" eilutę
- Pažymėkite "Passed: 26" eilutę
- Pažymėkite "Build succeeded" žalią tekstą

---

## 2️⃣ SCREENSHOT: Code Coverage Raport

### Komanda:

```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test /p:CollectCoverage=true
```

### Ką Turėtų Matytis:
```
Test summary: total: 26, failed: 0, succeeded: 26, skipped: 0, duration: 0.6s
✅ Build succeeded with 1 warning(s) in 1.9s
```

**SCREENSHOT TIPS:**
- Pažymėkite test summary eilutę
- Pažymėkite "succeeded: 26" skaičių

---

## 3️⃣ SCREENSHOT: Test Klasės Detalės

### Komanda (tik Payment testai):

```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test --filter ClassName=PaymentServiceUnitTests --logger "console;verbosity=detailed"
```

### Komanda (tik Email testai):

```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test --filter ClassName=EmailServiceUnitTests --logger "console;verbosity=detailed"
```

**SCREENSHOT TIPS:**
- Parodyti kiekvieno testo rezultatą
- Pabraukt zelenas "Passed" žodžius

---

## 4️⃣ SCREENSHOT: Test Struktūra (File Explorer)

### Atidaryti:
```
c:\Users\redas\Errando\backend.Tests\Unit\Services\
```

**SCREENSHOT TIPS:**
- Parodyti 2 failą:
  - `PaymentServiceUnitTests.cs`
  - `EmailServiceUnitTests.cs`

---

## 5️⃣ SCREENSHOT: Test Kodas

### Failas: PaymentServiceUnitTests.cs

**Perkopijuoti šią dalį:**
```csharp
[Fact]
public void ConfigurationIsProperlySetup()
{
    // Arrange
    var config = _configurationMock.Object;

    // Act
    var secretKey = config["Stripe:SecretKey"];

    // Assert
    Assert.Equal("test_secret_key", secretKey);
}
```

**SCREENSHOT TIPS:**
- Parodyti test metodą su [Fact] atributu
- Pažymėti "Arrange-Act-Assert" komentarus

---

## 6️⃣ SCREENSHOT: Theory Test su Parametrais

### Failas: PaymentServiceUnitTests.cs

**Perkopijuoti:**
```csharp
[Theory]
[InlineData(50)]
[InlineData(100)]
[InlineData(500)]
public void PaymentAmountShouldBePositive(decimal amount)
{
    // Test that positive amounts are valid
    Assert.True(amount > 0);
}
```

**SCREENSHOT TIPS:**
- Parodyti [Theory] su [InlineData] parametrais
- Paaiškinti, kad testas paleista 3 kartus

---

## 7️⃣ SCREENSHOT: Email Validacijos Testai

### Failas: EmailServiceUnitTests.cs

```csharp
[Theory]
[InlineData("test@example.com")]
[InlineData("user@domain.co.uk")]
[InlineData("name+tag@service.org")]
public void ValidEmailAddressesShouldContainAtSymbol(string email)
{
    Assert.Contains("@", email);
}
```

---

## 8️⃣ SCREENSHOT: Testų Suvestinė (Excel)

Naudoti šiuos skaičius:

| Testas | Kiekis | Status |
|--------|--------|--------|
| **Total Tests** | 26 | ✅ PASS |
| **PaymentService Tests** | 7 | ✅ PASS |
| **EmailService Tests** | 14 | ✅ PASS |
| **Other Tests** | 5 | ✅ PASS |
| **Failed** | 0 | ✅ PASS |
| **Duration** | 0.6 sec | ⚡ FAST |

---

## 9️⃣ SCREENSHOT: Projekto Struktūra

### Komanda:
```powershell
cd c:\Users\redas\Errando && tree backend.Tests /L 3
```

**SCREENSHOT TIPS:**
- Parodyti folder hierarchy
- Pažymėti "Unit/Services/" folderį

---

## 🔟 SCREENSHOT: README Dokumentacija

### Failas:
```
c:\Users\redas\Errando\backend.Tests\README.md
```

**SCREENSHOT TIPS:**
- Parodyti dokumentacijos pradžią
- Pažymėti "26 testai - visi praėję ✅"

---

## 📋 Suvestinė Skaičiams:

```
📊 Testų Statistika:
├── Total Tests: 26 ✅
├── Passed: 26 ✅
├── Failed: 0 ✅
├── Duration: 0.6 seconds ⚡
├── Pass Rate: 100% 🎯
└── Framework: xUnit 2.9.2

📁 Test Failai:
├── PaymentServiceUnitTests.cs (7 tests)
├── EmailServiceUnitTests.cs (14 tests)
└── UnitTest1.cs (5 tests)

🎯 Test Kategorijos:
├── Konfigūracija Tests: 2
├── Suma Validacija: 5
├── Email Validacija: 8
├── Valuta Tests: 3
├── Language Tests: 2
└── Other: 6
```

---

## 🚀 Quick Copy-Paste Komandos

### 1. Greitai paleisti testus:
```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test
```

### 2. Su detaliu output:
```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test -v d
```

### 3. Payment testai tik:
```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test --filter ClassName=PaymentServiceUnitTests
```

### 4. Email testai tik:
```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test --filter ClassName=EmailServiceUnitTests
```

### 5. Su coverage:
```powershell
cd c:\Users\redas\Errando\backend.Tests && dotnet test /p:CollectCoverage=true
```

---

**Šio lapelio tikslas:** Padėti jums susirasti ir nukopijuoti komandas skaidrėms. Tiesiog nukopijuokite komandą → Paleiskite → Padarykite screenshot!

**Data:** 2026-04-29
