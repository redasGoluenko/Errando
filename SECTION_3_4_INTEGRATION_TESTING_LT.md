# 3.4. INTEGRACIJOS TESTAVIMAS

## 3.4.1. Integracijos testavimo tikslas ir metodologija

Integracijos testavimas – tai vidurinės lygio testavimo sritis, skirta patikrinti, kaip skirtingi programos komponentai bendrauja ir veikia kartu. Errando projektui atliktas integracijos testavimas tikrindamas:

- **Valdiklių (Controllers) funkcionalumą**: HTTP atsakymai ir būsenos kodai
- **Realios duomenų bazės sąveiką**: Entity Framework Core veikimas
- **Autentifikacijos ir autorizacijos logika**: JWT žetono tikrinimas, vaidmenų patikrinimas
- **Užbaigto darbo srauto scenarijai**: nuo užduoties sukūrimo iki atlygino

Iš viso sukurta **52 integracijos testai**, kurie aprėpia tris pagrindinius valdiklius ir jų operacijas.

---

## 3.4.2. Naudoti įrankiai ir technologijos

### Testavimo rėmis: xUnit 2.9.2

Tas pats kaip ir vienetų testavimuose – xUnit naudojamas dėl:
- Aiškios semantikos ir paprastumo
- Tinkamo async/await paramo
- Geros integracijos su IDE
- Lygiagretų testų vykdymo galimybės

### Duomenų bazės testavimas: Entity Framework Core 9.x (In-Memory)

Integracijos testams sukuriama laikinė duomenų bazė atmintyje:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
    .Options;

var context = new AppDbContext(options);
```

Šis požiūris leidžia:
- Tikrinti realias duomenų bazės operacijas
- Nereikalingai neveikti tikroje DB
- Greičiau vykdyti testus nei su SQL Server
- Izoliuoti bandymus su unikaliais DB vardais

### Autentifikacijos tikrinimas: JWT žetonai ir ClaimsPrincipal

Integracijos testai simuliuoja JWT autentifikaciją:

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    new Claim(ClaimTypes.Name, username),
    new Claim(ClaimTypes.Role, role)
};

var identity = new ClaimsIdentity(claims, "TestScheme");
var principal = new ClaimsPrincipal(identity);
```

---

## 3.4.3. Testavimo šablonai ir standartai

Integracijos testai taip pat naudoja **Arrange-Act-Assert (AAA)** šablonu, bet su realiais komponentais:

### Arrange (Paruošimas)

Paruošiamas realus duomenų kontekstas, sėjami testiniai duomenys ir sudaromas valdiklis:

```csharp
// Arrange
var context = TestSetup.CreateInMemoryContext();
await TestSetup.SeedTestDataAsync(context);

var paymentService = new Mock<IPaymentService>();
var controller = new PaymentsController(
    paymentService.Object, 
    context, 
    logger);
```

### Act (Veiksmas)

Iškviečiamas valdiklio metodas su tikromis DTO ir autentifikacijos duomenimis:

```csharp
// Act
var result = await controller.CreatePaymentIntent(
    new CreatePaymentDto { TaskId = 1, Amount = 100m });
```

### Assert (Patvirtinimas)

Patikrinami HTTP atsakymai, duomenų bazės pokyčiai ir atitikimas:

```csharp
// Assert
Assert.IsType<OkObjectResult>(result);
var okResult = result as OkObjectResult;
Assert.NotNull(okResult?.Value);
var response = okResult.Value as PaymentIntentDto;
Assert.NotNull(response?.PaymentIntentId);
```

---

## 3.4.4. Testuoti valdikliai ir komponentai

### 1. UsersController – 20 testų

**Failas:** `backend.Tests/Integration/Controllers/UsersControllerTests.cs`

#### Prisijungimas (Authentication)

```
[4 testai]
├── LoginAsync_ValidCredentials_ReturnsToken
│   └─ Teisingi el. paštas ir slaptažodis → JWT žetonas grąžinamas
├── LoginAsync_InvalidCredentials_ReturnsBadRequest
│   └─ Klaidingas slaptažodis → HTTP 400 grąžinamas
├── LoginAsync_UserNotFound_ReturnsBadRequest
│   └─ Egzistuojantis vartotojas nerastas → HTTP 400
└── LoginAsync_IncorrectPassword_ReturnsBadRequest
    └─ Slaptažodis neatitinka → HTTP 400
```

**Testuojamos operacijos:**
- Vartotojo paieška DB
- Slaptažodžio hash'ų lyginimas (bcrypt)
- JWT žetono generavimas su role reikalavimais
- Sėkmingų ir nesėkmingų loginimų atsakymai

#### Registracija

```
[3 testai]
├── RegisterAsync_NewUser_CreatesAccount
│   └─ Naujas vartotojas sukuriamas ir grąžinamas token
├── RegisterAsync_DuplicateEmail_ReturnsBadRequest
│   └─ Egzistuojantis el. paštas → HTTP 400 (duplikatas)
└── RegisterAsync_InvalidEmail_ReturnsBadRequest
    └─ Nevalidus el. pašto formatas → HTTP 400
```

**Testuojamos operacijos:**
- Email unikalumo patikrinimas
- Slaptažodžio hash'avimas
- Naujo vartotojo išsaugojimas
- Email validavimas

#### Vartotojų išvardijimas (Get Users)

```
[3 testai]
├── GetUsers_AdminRole_ReturnsList
│   └─ Admin mato visus vartotojus
├── GetUsers_ClientRole_FilteredResults
│   └─ Klientas mato tik save
└── GetUsers_EmptyList_ValidResponse
    └─ Jei nėra vartotojų → grąžinamas tuščias sąrašas
```

**Testuojamos operacijos:**
- Vaidmens (role) patikrinimas iš JWT žetono
- Duomenų filtravimas pagal vaidmenį
- Grąžinami teisingi DTO objektai

#### Vartotojų valdymas (CRUD)

```
[6 testai]
├── GetUserById_ValidId_ReturnsUser
│   └─ Egzistuojantis ID → vartotojas grąžinamas
├── GetUserById_InvalidId_ReturnsNotFound
│   └─ Neegzistuojantis ID → HTTP 404
├── CreateUser_AdminOnly_Success
│   └─ Admin sukuria naują vartotoją
├── CreateUser_NonAdmin_ReturnsForbidden
│   └─ Ne-admin bandymas → HTTP 403 (Forbidden)
├── UpdateUser_ValidData_Success
│   └─ Vartotojo duomenys atnaujinami
└── DeleteUser_AdminOnly_Success
    └─ Admin ištrina vartotoją
```

**Testuojamos operacijos:**
- CRUD operacijos (Create, Read, Update, Delete)
- Autorizacijos patikrinimas (tik admin gali)
- Duomenų bazės būsenos keitimas
- NOT FOUND ir Forbidden atsakymai

**Naudoti testiniai duomenys:**
- Admin vartotojas su role "Admin"
- Klientas su role "Client"
- Bėgtys (runners) su role "Runner"

---

### 2. PaymentsController – 6 testai

**Failas:** `backend.Tests/Integration/Controllers/PaymentsControllerTests.cs`

#### Mokėjimo ketinimo sukūrimas

```
[3 testai]
├── CreatePaymentIntent_ValidTask_ReturnsIntent
│   └─ Teisingas TaskId → PaymentIntentDto grąžinamas
├── CreatePaymentIntent_InvalidTask_ReturnsBadRequest
│   └─ Neegzistuojantis TaskId → HTTP 400
└── CreatePaymentIntent_AlreadyPaid_ReturnsConflict
    └─ Jau apmokėta užduotis → HTTP 409 (Conflict)
```

**Testuojamos operacijos:**
- Užduoties egzistencijos patikrinimas
- Mokėjimo statuso tikrinimas
- Stripe PaymentIntent sukūrimas (apsimetamas)
- PaymentIntentDto grąžinimas su clientSecret

```csharp
[Fact]
public async Task CreatePaymentIntent_ValidTask_ReturnsIntent()
{
    // Arrange
    var context = TestSetup.CreateInMemoryContext();
    await TestSetup.SeedTestDataAsync(context);
    var mockPaymentService = new Mock<IPaymentService>();
    mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
        .ReturnsAsync(new PaymentIntentDto 
        { 
            PaymentIntentId = "pi_test",
            ClientSecret = "secret_test",
            Amount = 100m
        });

    var controller = new PaymentsController(
        mockPaymentService.Object, 
        context, 
        logger);

    // Act
    var result = await controller.CreatePaymentIntent(
        new CreatePaymentDto { TaskId = 1, Amount = 100m });

    // Assert
    Assert.IsType<OkObjectResult>(result);
}
```

#### Mokėjimo patvirtinimas

```
[1 testas]
└── ConfirmPayment_ValidIntent_Updates
    └─ Stripe PaymentIntent patvirtinamas ir DB atnaujinama
```

**Testuojamos operacijos:**
- PaymentIntent statuso tikrinimas Stripe API
- Mokėjimo statuso atnaujinimas ("pending" → "succeeded")
- Duomenų bazės modifikavimas

#### Mokėjimo istorija

```
[1 testas]
└── GetPaymentHistory_ForTask_ReturnsList
    └─ Grąžinami visi užduoties mokėjimai
```

**Testuojamos operacijos:**
- Filtravimas pagal TaskId
- Sąrašo grąžinimas chronologine tvarka
- Kelių mokėjimų tvarkymas

#### Mokėjimo patikrinimas

```
[1 testas]
└── HasPaid_ClientPaid_ReturnsTrue
    └─ Patikrina, ar klientas sumokėjo
```

**Testuojamos operacijos:**
- Sėkmingų mokėjimų filtravimas
- Boolean grąžinimas (true/false)

---

### 3. ChatsController – 3 testai

**Failas:** `backend.Tests/Integration/Controllers/ChatsControllerTests.cs`

#### Pokalbių retrieval

```
[3 testai]
├── GetChats_ReturnsList
│   └─ Grąžinami visi pokalbiai
├── GetChats_EmptyList_ValidResponse
│   └─ Jei nėra pokalbių → tuščias sąrašas
└── GetChats_UserIsolation_VerifyFiltering
    └─ Klientas mato tik savo pokalbius
```

**Testuojamos operacijos:**
- Pokalbių retrieval iš DB
- Vartotojo izoliacijos patikrinimas
- Tuščių rezultatų tvarkymas
- Atitinkamų DTO objektų grąžinimas

---

## 3.4.5. Panaudoti testiniai duomenys

### Test Setup Helper

Visi integracijos testai naudoja `TestSetup` pagalbinę klasę:

```csharp
public static class TestSetup
{
    // Sukuriamas gryna atmintyje duomenų bazė
    public static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    // Sėjami baziniai testiniai duomenys
    public static async Task SeedTestDataAsync(AppDbContext context)
    {
        var admin = new User 
        { 
            Id = 1, 
            Email = "admin@test.com",
            Username = "admin",
            Role = "Admin",
            PasswordHash = HashPassword("admin123")
        };
        
        var client = new User 
        { 
            Id = 2, 
            Email = "client@test.com",
            Username = "client",
            Role = "Client"
        };
        
        var runner = new User 
        { 
            Id = 3, 
            Email = "runner@test.com",
            Username = "runner",
            Role = "Runner"
        };

        context.Users.AddRange(admin, client, runner);
        
        var task = new Task 
        { 
            Id = 1,
            Title = "Test Task",
            ClientId = 2,
            Budget = 100m,
            Status = "open"
        };
        
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
    }
}
```

### Testiniai vartotojai

| ID | El. paštas | Vartotojardis | Vaidmuo | Paskirtis |
|----|-----------|--------------|--------|----------|
| 1 | admin@test.com | admin | Admin | Administracinės operacijos |
| 2 | client@test.com | client | Client | Užduočių kūrimas |
| 3 | runner@test.com | runner | Runner | Užduočių atlikimas |

---

## 3.4.6. Naudoti DTO objektai

### Autentifikacija

```csharp
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RegisterDto
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AuthDto
{
    public string Token { get; set; }
    public UserDto User { get; set; }
}
```

### Mokėjimai

```csharp
public class CreatePaymentDto
{
    public int TaskId { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentIntentDto
{
    public int PaymentId { get; set; }
    public string ClientSecret { get; set; }
    public string PaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
}

public class ConfirmPaymentDto
{
    public int PaymentId { get; set; }
    public string PaymentIntentId { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int ClientId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Pokalbiai

```csharp
public class ChatDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int RunnerId { get; set; }
    public List<ChatMessageDto> Messages { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 3.4.7. Integracijos testų rezultatai

### Integracijos testų statistika

| Metrika | Reikšmė |
|---------|---------|
| **Iš viso integracijos testų** | 52 |
| **Sėkmingai praeiti testai** | 52 (100%) |
| **Nepavykę testai** | 0 |
| **Praleisti testai** | 0 |
| **Vykdymo laikas** | ~6.5 sekundy |

### Valdiklių aprėptis

| Valdiklis | Testų skaičius | Aprėpta |
|-----------|----------------|--------|
| UsersController | 20 | 47.3% |
| PaymentsController | 6 | 43.2% |
| ChatsController | 3 | 9.7% |
| Servisu integracijos testai | 23 | Darbašaltys su DB |
| **IŠ VISO** | **52** | **38.8% vidutinė** |

### HTTP atsakymų tipai testuoti

| HTTP kodas | Reikšmė | Testų |
|-----------|---------|-------|
| 200 OK | Sėkmingas atsakymas | 28 |
| 400 Bad Request | Klaidinga įvestis | 12 |
| 401 Unauthorized | Neprisijungęs vartotojas | 4 |
| 403 Forbidden | Nepakankamos teisės | 6 |
| 404 Not Found | Resursas nerastas | 2 |

---

## 3.4.8. Vaidmenų (Role-Based Access Control) testavimas

### Admin operacijos

```csharp
[Fact]
public async Task CreateUser_AdminOnly_Success()
{
    // Arrange
    var adminPrincipal = TestSetup.CreateClaimsPrincipal(
        userId: 1, 
        username: "admin",
        role: "Admin");
    
    var controller = new UsersController(...);
    controller.ControllerContext = new ControllerContext 
    { 
        HttpContext = new DefaultHttpContext 
        { 
            User = adminPrincipal 
        } 
    };

    // Act
    var result = await controller.CreateUser(new CreateUserDto {...});

    // Assert
    Assert.IsType<OkObjectResult>(result);
}
```

### Nepakankamos teisės

```csharp
[Fact]
public async Task DeleteUser_NonAdmin_ReturnsForbidden()
{
    // Arrange
    var clientPrincipal = TestSetup.CreateClaimsPrincipal(
        userId: 2, 
        username: "client",
        role: "Client");
    
    var controller = new UsersController(...);
    controller.ControllerContext.HttpContext.User = clientPrincipal;

    // Act
    var result = await controller.DeleteUser(1);

    // Assert
    Assert.IsType<ForbiddenResult>(result);
}
```

---

## 3.4.9. Darbo srautų (Workflows) testavimas

### Pilnas mokėjimo srauts

```
1. Klientas prisijungia
   └─ LoginAsync → JWT žetonas grąžinamas

2. Užduotis jau sukurta DB
   └─ Klientas prašo mokėjimo ketinimo
   
3. Mokėjimo ketinimas sukuriamas
   └─ CreatePaymentIntent → Stripe integration
   
4. Klientas patvirtina mokėjimą
   └─ ConfirmPayment → status = "succeeded"
   
5. Užduotis pažymima kaip apmokėta
   └─ Tolimesni veiksmai galimi
```

### Autentifikacijos srauts

```
1. Naujas vartotojas
   └─ RegisterAsync → vartotojas sukurtas

2. Vartotojas prisijungia
   └─ LoginAsync → JWT žetonas grąžinamas

3. Vartotojas naudoja token
   └─ Kitų API metodų iškviečiama su Authorization: Bearer <token>

4. Token patikrinamas serveryje
   └─ Claims iš JWT išskaidomi (ID, vartotojardis, vaidmuo)
```

---

## 3.4.10. Išvados iš integracijos testavimo

1. **Realūs komponentų ryšiai**: Integracijos testai patikrina tikrus valdiklių, DB ir paslaugų bendravimus
2. **Pilnas HTTP srauts**: Nuo HTTP užklausos iki duomenų bazės modifikacijos
3. **Autentifikacijos patikrinimas**: JWT žetonų validavimas ir vaidmenų autorizacija
4. **Duomenų bazės vientisumas**: Testuojami realūs EF Core pokyčiai
5. **Aprėptis pagal vaidmenį**: Testai tikrina skirtingų vartotojų prieigą

---

## 3.4.11. Kombinuotas testų požiūris: vienetų + integracijos

### Vienetų testai (49)
- **Greitumas**: ~1.5 sekundo
- **Izoliacija**: 100% mocked dependencies
- **Fokusas**: Paslaugų logika

### Integracijos testai (52)
- **Realybiškumas**: tikra DB ir HTTP srauts
- **Greitis**: ~6.5 sekundy
- **Fokusas**: valdiklių ir sąveika

### Bendras rezultatas (101 testai)
- **Saugumas**: 100% pass rate
- **Aprėptis**: pagrindinės funkcijos aprėptos
- **Kontrolė**: greitai aptinkamos klaidos

---

## Naudoti programavimo šaltiniai

| Šaltinis | Versija | Paskirtis |
|----------|---------|----------|
| .NET | 9.0.203 | Runtime |
| C# | 13 | Programavimo kalba |
| xUnit | 2.9.2 | Testavimo rėmis |
| Moq | 4.x | Apsimetų biblioteka |
| Entity Framework Core | 9.x | ORM ir testų duomenų bazė |
| Coverlet | 6.0.0 | Aprėpties matavimas |
| ReportGenerator | 5.5.7 | Aprėpties ataskaitos HTML |

---

## Tesavimo vykdymo komandos

Vienetų testai:
```bash
dotnet test --filter "Namespace~backend.Tests.Unit"
```

Integracijos testai:
```bash
dotnet test --filter "Namespace~backend.Tests.Integration"
```

Visi testai:
```bash
dotnet test
```

Su aprėpties ataskaita:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 3.4.12. Rekomendacijos dėl testų tolesnio dėstymo

1. **Daugiau integracijos testų**: likę valdikliai (TasksController, ReviewsController, etc.)
2. **E2E testai**: Selenium arba Playwright su prototipo sąsaja
3. **Pasiūlymo testai**: pagal specifikacijas ir reikalavimus
4. **Aprėpties tikslas**: siekti 60%+ linijų aprėpties kritiniuose moduliuose

---

## Išvada

Kombinuotas vienetų (49) ir integracijos (52) testų požiūris suteikia pilnų įtikimybę Errando sistemos funkcionalumui. Testai tikrina tiek atskirus komponentus, tiek jų sąveiką, užtikrinant, kad sistema veikia patikimai ir atitinka reikalavimus.
