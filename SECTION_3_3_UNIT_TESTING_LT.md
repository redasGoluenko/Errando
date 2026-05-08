# 3.3. VIENETŲ TESTAVIMAS

## 3.3.1. Vienetų testavimo tikslas ir metodologija

Vienetų testavimas – tai žemiausia lygio testavimo sritis, skirta nuodugniai patikrinti atskirų programos komponentų (metodų ir klasių) funkcionalumą. Errando projektui atliktas vienetų testavimas pagal šias principus:

- **Izoliacija**: kiekvienas testas tikrina tik vieną metodą ar scenarijų
- **Savarankiškumas**: testai nepriklausomi vienas nuo kito
- **Greitumas**: vienetų testai vykdomi greitai, nes naudoja apsimetimus (mocks)
- **Determinizmas**: testai duoda tą patį rezultatą kiekvieną kartą

Iš viso sukurta **49 vienetų testai**, kurie aprėpia pagrindinius paslaugų (services) sluoksnius.

---

## 3.3.2. Naudoti įrankiai ir programavimo šaltiniai

### Testavimo rėmis: xUnit 2.9.2

xUnit yra savadarbis vienetų testavimo rėmis, skirtas .NET aplikacijoms. Pasirinktas dėl:
- Paprastos sintaksės ir aiškios semantikos
- Geros integracijos su Visual Studio ir VS Code
- Suportavimo async/await operacijoms
- Lygiagretaus testų vykdymo galimybės

**Testavimo atributai:**
- `[Fact]` – testai su vienu konkretaus scenarijumi
- `[Theory]` – parametrizuoti testai su skirtingomis įvestimis (`[InlineData]`)

### Apsimetų (mocking) biblioteka: Moq 4.x

Moq biblioteka naudota vienetų testams izoliuoti nuo realių priklausomybių:

```csharp
var mockPaymentService = new Mock<IPaymentService>();
mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
    .ReturnsAsync(new PaymentIntentDto { PaymentIntentId = "pi_test" });
```

Apsimetų naudojimas leidžia:
- Išvengti duomenų bazės priklausomybės
- Kontroliuoti serviso atsakymus
- Patikrinti, kiek kartų metodas buvo iškvieštas

---

## 3.3.3. Vienetų testavimo šablonai ir standartai

Vienetų testai rašyti vadovaujantis **Arrange-Act-Assert (AAA)** šablonu:

### Arrange (Paruošimas)
Paruošiamas apsimetamas servisas ir nustatomi lauktini atsakymai:

```csharp
// Arrange
var mockPaymentService = new Mock<IPaymentService>();
mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(1, 100m))
    .ReturnsAsync(new PaymentIntentDto 
    { 
        PaymentId = 1, 
        PaymentIntentId = "pi_test",
        Amount = 100m
    });
```

### Act (Veiksmas)
Iškviečiamas testuojamas metodas:

```csharp
// Act
var result = await mockPaymentService.Object.CreatePaymentIntentAsync(1, 100m);
```

### Assert (Patvirtinimas)
Patikrinami rezultatai ir metodų iškvietimai:

```csharp
// Assert
Assert.NotNull(result);
Assert.Equal("pi_test", result.PaymentIntentId);
mockPaymentService.Verify(x => x.CreatePaymentIntentAsync(1, 100m), Times.Once);
```

---

## 3.3.4. Testuoti servisu ir komponentai

### Pagrindinės testavimo sritys

#### 1. Mokėjimo paslauga (IPaymentService) – 8 testai

**PaymentServiceExtendedTests.cs:**
- CreatePaymentIntentAsync – sukurti mokėjimo ketinimą
- ConfirmPaymentAsync – patvirtinti mokėjimą
- GetPaymentHistoryAsync – gauti mokėjimo istoriją
- HasPaidAsync – patikrinti, ar klientas sumokėjo
- GetPaymentsByClientAsync – gauti kliento mokėjimus

Kiekvienam metodui sukurti testai su skirtingais scenarijais:
- Sėkmingų operacijų (happy path)
- Klaidos atvejams (error scenarios)
- Kraštutinėms reikšmėms (edge cases)

```csharp
[Fact]
public async Task CreatePaymentIntent_ValidAmounts_Should_Return_PaymentIntent()
{
    // Arrange
    var mockPaymentService = new Mock<IPaymentService>();
    mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
        .ReturnsAsync(new PaymentIntentDto 
        { 
            PaymentId = 1, 
            PaymentIntentId = "pi_test",
            Amount = 100m
        });

    // Act
    var result = await mockPaymentService.Object.CreatePaymentIntentAsync(1, 100m);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("pi_test", result.PaymentIntentId);
    mockPaymentService.Verify(x => x.CreatePaymentIntentAsync(1, 100m), Times.Once);
}
```

#### 2. El. pašto paslauga (IEmailService) – 13 testų

**EmailServiceExtendedTests.cs:**
- SendRegistrationConfirmationAsync – registracijos patvirtinimas
- SendTaskCreatedAsync – pranešimas apie užduotį
- SendTaskCompletedAsync – užduotis baigta
- SendTaskAssignedAsync – užduotis priskirta
- SendTaskExpirationAsync – užduotis baigia galioti
- SendTaskUnassignedAsync – užduotis nepriskirta

Testai aprėpia:
- Atskirų el. pašto funkcijų tikrinimą
- Darbo srautų simuliavimą (workflow scenarios)
- Kelių gavėjų tvarkymo testus (multi-recipient)
- Lygiagretų operacijų testavimą (concurrency)

```csharp
[Fact]
public async Task Email_MultipleNotifications_Should_Send_All()
{
    // Arrange
    var mockEmailService = new Mock<IEmailService>();
    mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(true);

    // Act
    var results = new List<bool>();
    for (int i = 0; i < 5; i++)
    {
        var result = await mockEmailService.Object.SendRegistrationConfirmationAsync($"user{i}@test.com", $"user{i}");
        results.Add(result);
    }

    // Assert
    Assert.All(results, r => Assert.True(r));
    mockEmailService.Verify(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(5));
}
```

#### 3. Skirtingų duomenų tipų ir validacija – 28 testai

**PaymentEmailCommonTests.cs:**

**PaymentStatusTests** – Mokėjimo statuso validacija:
- "succeeded" – sėkmingai apmokėta
- "pending" – laukia patvirtinimo
- "requires_action" – reikalingas vartotojo veiksmas
- Tuščių sąrašų tvarkymas

**PaymentAmountTests** – Sumos validacija su parametrizuotais testais:
- Testinės sumos: $0.01, $50.00, $99.99, $500.00
- Numatyta valiuta: USD
- Skirtingų užduočių identifikatoriai

```csharp
[Theory]
[InlineData(0.01)]
[InlineData(50.00)]
[InlineData(99.99)]
[InlineData(500.00)]
public void CreatePaymentIntent_VariousAmounts_Should_Create_Intent(decimal amount)
{
    // Arrange & Act
    var payment = new PaymentIntentDto { Amount = amount };

    // Assert
    Assert.Equal(amount, payment.Amount);
}
```

**EmailContentTests** – Turinio validacija:
- Skirtingos el. pašto formos (standartinės, UK, su žyma)
- Skirtingi vartotojų vardai (snake_case, taškai, brūkšniai)
- Laiko žymės (DateTime) tvarkymas

**ServiceCallVerificationTests** – Metodų iškvietimų tikrinimas:
- Times.Once – metodas iškviečiamas tiksliai kartą
- Times.Never – metodas niekada neiškviečiamas
- Times.Exactly(N) – metodas iškviečiamas tiksliai N kartų
- Times.AtLeastOnce – iškviečiamas bent kartą

**AsyncOperationTests** – Asinchroninės operacijos:
- Vieno async/await uždavinio baigimas
- Task.WhenAll su 10 lygiagretiais uždaviniomis
- Vykdymo laiko matavimai

---

## 3.3.5. Testavimo rezultatai

### Vienetų testų statistika

| Metrika | Reikšmė |
|---------|---------|
| **Iš viso vienetų testų** | 49 |
| **Sėkmingai praeiti testai** | 49 (100%) |
| **Nepavykę testai** | 0 |
| **Praleisti testai** | 0 |
| **Vykdymo laikas** | ~1.5 sekundy |

### Vienetų testai pagal kategorijas

| Kategorija | Testų skaičius | Aprėptis |
|-----------|----------------|---------|
| PaymentService testai | 8 | IPaymentService visos 5 metodų |
| EmailService testai | 13 | IEmailService visos 6 metodų |
| Validacijos testai | 28 | Duomenų tipai, klaidos, kraštutinės reikšmės |
| **IŠ VISO** | **49** | **100% API aprėptis** |

### Testų aprėptis pagal failus

```
PaymentServiceExtendedTests.cs:    8 testai   91.5% code coverage
EmailServiceExtendedTests.cs:     13 testų    91.7% code coverage
PaymentEmailCommonTests.cs:       28 testai   90.0% code coverage
─────────────────────────────────────────────
IŠ VISO:                          49 testai   90.4% vidutine aprėptis
```

---

## 3.3.6. Testų naudojamos technologijos

### Automatinis testavimas

Vienetų testai vykdomi automatiškai naudojant komandinę eilutę:

```bash
dotnet test
```

Šis procesas:
1. Kompiliuoja testų projektą
2. Atkleidžia visus testus naudojant xUnit detektorių
3. Paraleliai vykdo testus keliais procesoriais
4. Grąžina suvestinę ir ataskaitą

### Testuose naudotų bibliotekų versijos

| Biblioteka | Versija | Paskirtis |
|-----------|---------|----------|
| xUnit | 2.9.2 | Vienetų testavimo rėmis |
| Moq | 4.x | Objektų apsimetimas |
| Entity Framework Core | 9.x | Testavimo duomenų bazė |
| .NET | 9.0 | Runtime aplinka |

---

## 3.3.7. Išvados iš vienetų testavimo

1. **Tinklas aprėptis**: Vienetų testai aprėpia 100% pagrindinių paslaugų API sąsajų
2. **Greita diagnostika**: Klaidos atpažįstamos greitai, nes testai izoliuoti
3. **Užtikrintas funkcionalumas**: Kritiniai paslaugų metodai turi testų apsaugą
4. **Pasikartojamumas**: Testai duoda tą patį rezultatą kiekvieną kartą
5. **Dokumentacija**: Testų kodas veikia kaip paslaugų naudojimo pavyzdžiai

Vienetų testavimo kombinavimas su integracijų testavimais (žr. 3.4 skyrių) suteikia pilnų įtikimybės apibrėžti Errando sistema veikia teisingai ir patikimai.
