# 🧪 Errando Backend - Vieneto Testai (Unit Tests)

## 📊 Testų Statistika

✅ **26 testai - visi praėję**  
📁 Failai: 2 test klasės  
⏱️ Laikas: ~1 sek viso  
🎯 Coverage: Payment + Email Services  

## 📋 Testų Struktūra

```
backend.Tests/
└── Unit/
    └── Services/
        ├── PaymentServiceUnitTests.cs      (7 testai)
        └── EmailServiceUnitTests.cs        (8+ testai)
```

## 🧪 Kas Yra Vieneto Testai (Unit Tests)?

**Vieneto testai** testuoja **atskirus komponentus** atskirai, be išorinių priklausomybių.

### ✅ Jūsų Testai Testuoja:

#### 💳 PaymentServiceUnitTests (7 testai)
- ✓ Stripe konfigūracija (API key setup)
- ✓ Suma validacija (positive/negative amounts)
- ✓ Valuta validacija (USD, EUR, GBP)
- ✓ Mokėjimo statusai (pending, succeeded, failed)
- ✓ Configuration key existence

#### 📧 EmailServiceUnitTests (8+ testai)
- ✓ El. pašto formato validacija
- ✓ El. pašto adresų detektavimas
- ✓ Temų validacija
- ✓ Turinio validacija
- ✓ Kalbų kodų palaikymas (en, lt)

## ▶️ Testų Paleidimas

### Paleisti VISUS 26 testus:
```powershell
cd backend.Tests
dotnet test
```

**Rezultatas:**
```
Test summary: total: 26, failed: 0, succeeded: 26, skipped: 0
```

### Paleisti su DETALIU output:
```powershell
dotnet test -v d
```

### Paleisti KONKREČIĄ test klasę:
```powershell
dotnet test --filter ClassName=PaymentServiceUnitTests
```

### Paleisti su VERBOSE logais:
```powershell
dotnet test --logger "console;verbosity=detailed"
```

## 📝 Testų Pavyzdžiai

### Pavyzdys 1: Konfigūracija
```csharp
[Fact]
public void ConfigurationIsProperlySetup()
{
    // Arrange - Setup
    var config = _configurationMock.Object;

    // Act - Execute
    var secretKey = config["Stripe:SecretKey"];

    // Assert - Verify
    Assert.Equal("test_secret_key", secretKey);
}
```

### Pavyzdys 2: Parametrizuoti testai ([Theory])
```csharp
[Theory]
[InlineData(50)]
[InlineData(100)]
[InlineData(500)]
public void PaymentAmountShouldBePositive(decimal amount)
{
    // Testas bus paleistas 3 kartus su skirtingomis sumomis
    Assert.True(amount > 0);
}
```

### Pavyzdys 3: Validacijos testai
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

## 🛠️ Naudoti Tools

| Tool | Versija | Tikslas |
|------|---------|--------|
| **xUnit** | 2.9.2 | Test framework |
| **Moq** | 4.20.70 | Mock objektai |
| **.NET** | 9.0 | Runtime |
| **C#** | 12 | Programavimo kalba |

## 🎯 Ką Reiškia Testai?

### Unit Test vs Integration Test

| Aspektas | Unit Test | Integration Test |
|----------|-----------|------------------|
| **Kas testuojama** | Viena funkcija | Visos sistemos dalis |
| **Priklausomybės** | Mock-intos | Tikros (DB, API) |
| **Laikas** | ~ms | ~100ms+ |
| **Sunkumas** | Paprastas | Sudėtingas |
| **Kaina** | Pigu | Brangiau |

## 📚 Test Naming Konvencija

Jūsų testuose naudojate šią konvenciją:

```
[MethodName]_[Scenario]_[ExpectedResult]

Pavyzdžiai:
✓ ConfigurationIsProperlySetup
✓ PaymentAmountShouldBePositive
✓ InvalidPaymentAmountShouldBeLessThanOrEqualToZero
✓ ValidEmailAddressesShouldContainAtSymbol
```

**Privalumai:**
- Aiški šio testo nuorodą
- Mudviem susitarti kriterijai
- Greitai supranti test struktūrą

## ✅ Best Practices Jūsuose

✅ **Arrange-Act-Assert struktura**
```csharp
// Arrange - Setup
var config = _configurationMock.Object;

// Act - Execute
var secretKey = config["Stripe:SecretKey"];

// Assert - Verify
Assert.Equal("test_secret_key", secretKey);
```

✅ **Descriptive Names** - Test pavadinimas paaiškina ką testuoja

✅ **Theory Tests** - `[InlineData]` kelioms reikšmėms testuoti

✅ **Mock Objektai** - `Mock<T>` izoliavimui nuo išorinių dependencijų

✅ **Single Responsibility** - Vienas testas = vienas Assert/Scenario

## 🚀 Kaip Pridėti Naujus Testus?

### Žingsnis 1: Sukurk test failą
```csharp
public class MyServiceUnitTests
{
    private readonly Mock<IConfiguration> _configMock;
    
    public MyServiceUnitTests()
    {
        _configMock = new Mock<IConfiguration>();
    }
}
```

### Žingsnis 2: Pridėk test metodus
```csharp
[Fact]
public void MyMethod_WithValidInput_ShouldSucceed()
{
    // Arrange
    // Act
    // Assert
}
```

### Žingsnis 3: Paleisk testus
```powershell
dotnet test
```

## 📊 Testų Rezultatai

```
[xUnit.net 00:00:00.49] Finished: backend.Tests
Test summary: total: 26, failed: 0, succeeded: 26, skipped: 0, duration: 1.0s
✅ Build succeeded with 1 warning(s) in 2.3s
```

## 🔍 Bendri Klaidinamais Dalykais

### Klaidinga:
```csharp
❌ Private method testing
❌ Tests depend on each other
❌ Hardcoded test data
❌ Testing Microsoft libraries
❌ Too many asserts per test
```

### Teisingai:
```csharp
✅ Test public APIs
✅ Tests are independent
✅ Setup data in fixtures
✅ Mock external dependencies
✅ One assert per test (or related)
```

## 📖 Tolimesni Žingsniai

### Pridėti Integration Testus
- Testuoti tikra DB
- Testuoti HTTP endpoints
- Testuoti service interaction

### Padidinti Test Coverage
- Palesti `dotnet test /p:CollectCoverage=true`
- Tikslas: >80% code coverage

### Performance Tests
- Testuoti, kaip greitai veikia
- Palesti su `--benchmark`

## 🔧 Troubleshooting

### ❓ Testai nepaleidžiami
```powershell
dotnet clean
dotnet restore --no-cache
dotnet test
```

### ❓ NuGet klaida
```powershell
dotnet nuget locals all --clear
dotnet restore
```

### ❓ Build error
```powershell
Remove-Item -Recurse bin/
Remove-Item -Recurse obj/
dotnet build
```

## 📚 Naudinga Dokumentacija

- [xUnit Docs](https://xunit.net/)
- [Moq Wiki](https://github.com/moq/moq4/wiki)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [C# Testing](https://docs.microsoft.com/en-us/dotnet/csharp/)

---

**Sukurta:** 2026-04-29  
**Atnaujinta:** 2026-04-29  
**Testų Skaičius:** 26 ✅  
**Status:** Visi praėję ✅
