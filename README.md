# KickFive

Een ASP.NET Core MVC-applicatie voor het reserveren van sportvelden ("bookings"), gebouwd met .NET 9, Entity Framework Core, ASP.NET Identity en een publiek bereikbare Azure SQL-database. Het project voorziet een meertalige webinterface (NL/EN/FR) en een REST API voor gebruik door een mobiele (MAUI) applicatie.

## Inhoudstafel

- [Overzicht](#overzicht)
- [Architectuur](#architectuur)
- [Technologieën](#technologieën)
- [Projectstructuur](#projectstructuur)
- [Databankstructuur](#databankstructuur)
- [Identity Framework](#identity-framework)
- [Meertaligheid](#meertaligheid)
- [Filtering en sortering](#filtering-en-sortering)
- [API](#api)
- [Middleware](#middleware)
- [Seeding](#seeding)
- [Installatie en configuratie](#installatie-en-configuratie)
- [Gebruikte AI-tools en bronnen](#gebruikte-ai-tools-en-bronnen)

---

## Overzicht

KickFive laat gebruikers sportvelden reserveren voor een bepaald tijdslot. De applicatie ondersteunt:

- Registratie, login en e-mailverificatie via ASP.NET Identity
- Rolgebaseerde toegang (Admin / User)
- Beheer van velden (`Field`), reserveringen (`Booking`) en gebruikers (`User`)
- Filtering en sortering op overzichtspagina's (bv. reserveringen op status/veld, gebruikers op naam/e-mail)
- Een REST API voor externe consumptie (bv. door een MAUI-mobiele app)
- Meertalige interface (Nederlands, Engels, Frans)
- Een publiek bereikbare Azure SQL-database

## Architectuur

Het project bestaat uit twee onderdelen binnen dezelfde Visual Studio Solution:

| Project | Verantwoordelijkheid |
|---|---|
| **KickFive** (ASP.NET Core MVC) | Controllers, Razor Views, API-endpoints, Identity-configuratie, middleware |
| **KickFive.Data** (Class Library) | Modelklassen, `DbContext`, database-seeding |

Deze scheiding zorgt ervoor dat de model- en databanklaag herbruikbaar en onafhankelijk testbaar is van de presentatielaag.

## Technologieën

- **.NET 9 / ASP.NET Core MVC**
- **Entity Framework Core** (Code First, migraties)
- **ASP.NET Core Identity** (met een eigen `User`-klasse)
- **Azure SQL Database** (publiek bereikbaar, SQL-authenticatie)
- **Bootstrap** voor de frontend-styling
- **AJAX** voor asynchrone interacties zonder volledige pagina-herlaad
- **SMTP (via Mailtrap)** voor e-mailverificatie

### NuGet-packages

| Package | Doel |
|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Koppelt ASP.NET Core Identity aan EF Core voor gebruikers-, rol- en tokenopslag in de databank |
| `Microsoft.AspNetCore.Identity.UI` | Levert de standaard Identity Razor-pagina's (Register, Login, ...) die gescaffold en aangepast zijn |
| `Microsoft.EntityFrameworkCore.Design` | Design-time componenten, nodig voor het genereren van migraties |
| `Microsoft.EntityFrameworkCore.SqlServer` | EF Core-provider voor SQL Server / Azure SQL Database |
| `Microsoft.EntityFrameworkCore.Tools` | CLI/Package Manager Console-commando's zoals `Add-Migration` en `Update-Database` |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | Ondersteunt het scaffolden van controllers, views en Identity-pagina's |
| `Azure.Identity` / `Azure.Core` | Ondersteuning voor Azure-authenticatiemechanismen (transitieve afhankelijkheid via de Azure SQL-integratie) |
| `Humanizer` | Leesbaar formatteren van strings, datums en getallen (transitieve afhankelijkheid) |


## Databankstructuur

De databank bestaat uit de volgende gerelateerde tabellen (`KickFive.Data/Models`):

- **Field** — de sportvelden die gereserveerd kunnen worden (`Id`, `Name`)
- **Booking** — een reservering (`Id`, `StartDateTime`, `EndDateTime`, `Status`, `Price`, `FieldId`, `UserId`)
- **User** — uitgebreide Identity-gebruiker (`Id`, `FirstName`, `LastName`, `Email`, `PhoneNumber`, ...)
- **Review** — beoordelingen gekoppeld aan velden en/of gebruikers

`Booking` heeft een foreign key-relatie met zowel `Field` als `User`; `Review` is op zijn beurt gekoppeld aan `User` (en/of `Field`), wat de vereiste van minstens drie onderling gerelateerde tabellen invult.

De `Price`-kolom is expliciet geconfigureerd met `HasPrecision(10, 2)` in `OnModelCreating` om afronding/truncatie te vermijden.

De databank draait op een **publiek bereikbare Azure SQL Database** (`kickfive-server.database.windows.net`), met SQL-authenticatie en een firewallregel die enkel toegestane IP-adressen toelaat.

## Identity Framework

- Een eigen `User`-klasse breidt `IdentityUser` uit met extra eigenschappen (`FirstName`, `LastName`, ...).
- De **Register**-pagina is gescaffold vanuit de standaard Identity UI, zodat extra velden en logica (zoals automatische roltoekenning) toegevoegd konden worden.
- Bij registratie wordt automatisch de rol **"User"** toegekend via `_userManager.AddToRoleAsync(user, "User")`.
- Er zijn **3 actieve rollen** voorzien: `Admin`, `User`, en een derde rol naargelang de functionele noden van het project.
- **E-mailverificatie** is verplicht (`RequireConfirmedAccount = true`) vooraleer een gebruiker toegang krijgt, via een zelfgeschreven `SmtpEmailSender` (implementatie van `IEmailSender`) die e-mails verstuurt via een SMTP-sandboxomgeving (Mailtrap). De toegangscode/credentials worden **niet** hardcoded in de broncode, maar veilig bewaard via `dotnet user-secrets`.
- Autorisatie wordt toegepast op controllers en actiemethoden via `[Authorize]`/`[Authorize(Roles = "Admin")]`, en de menustructuur past zich aan op basis van de rol van de ingelogde gebruiker.
- Gebruikersbeheer (via de `UsersController`) laat toe rollen toe te kennen en gebruikers te (de)blokkeren (`LockoutEnabled`/`LockoutEnd`).

## Meertaligheid

De applicatie ondersteunt **drie talen**: Nederlands, Engels en Frans, via `AddViewLocalization` en `.resx`-resourcebestanden in de map `Resources/`. Dit is geïntegreerd tot in het Identity Framework, zodat ook registratie-, login- en foutmeldingen vertaald worden. De taalkeuze wordt beheerd via `RequestLocalizationOptions` in `Program.cs`, en de `LanguageController` verzorgt het wisselen van taal vanuit de gebruikersinterface (bv. via een taalselector in `_Layout.cshtml`).

## Filtering en sortering

De overzichtspagina's (Index-views) van **Bookings** en **Users** bevatten filter- en sorteervelden:

- **Bookings**: filtering op status en veld, sortering op startdatum, prijs en status (met asc/desc-toggle via querystring-parameters).
- **Users**: zoeken op naam/e-mail, sortering op achternaam en e-mailadres.

De implementatie gebruikt `IQueryable<T>` zodat filter- en sorteercriteria samen worden vertaald naar één efficiënte SQL-query, pas uitgevoerd bij de finale `.ToListAsync()`.

## API

Het project voorziet RESTful API-controllers voor alle modellen (Booking, Field, User), bedoeld voor consumptie door een mobiele (MAUI) applicatie. De API-endpoints volgen dezelfde autorisatieregels als de webpagina's, inclusief login- en registratiefunctionaliteit.

## Middleware

Er is eigen middleware voorzien om specifieke logica (zoals cookiebeheer/verwerking) centraal af te handelen binnen de request-pipeline, naast de standaard ASP.NET Core middleware voor authenticatie, autorisatie en lokalisatie.

## Seeding

Bij het opstarten van de applicatie wordt de database automatisch (gedeeltelijk) geseed via `DbSeeder.SeedAsync()`, opgeroepen in `Program.cs`:

1. **Fields** worden aangemaakt indien de tabel leeg is.
2. **Users** worden aangemaakt via `UserManager<User>.CreateAsync()` (niet rechtstreeks via `DbContext`, zodat wachtwoorden correct gehasht worden), inclusief automatische roltoekenning.
3. **Bookings** worden pas aangemaakt nadat gecontroleerd is dat er voldoende gebruikers en velden bestaan, om foreign key-fouten te vermijden. De seeder is bestand tegen gedeeltelijk gevulde databanken (bv. na een eerdere onderbroken run).

## Installatie en configuratie

### Vereisten

Zorg dat het volgende geïnstalleerd is voordat je het project opzet:

- **.NET 9 SDK** — [download hier](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio 2022** (17.14 of hoger), met de workload **"ASP.NET en webontwikkeling"**
- **SQL Server Management Studio (SSMS)** of de SQL-extensie in VS Code (optioneel, handig om de databank te inspecteren)
- Een **Azure-account** (bv. Azure for Students) als je een eigen Azure SQL Database wil opzetten, of toegang tot de bestaande connection string van het project
- **Git**

### Stap 1 — Clone de repository

```bash
git clone https://github.com/<jouw-gebruikersnaam>/KickFive.git
cd KickFive
```

### Stap 2 — Herstel de NuGet-packages

```bash
dotnet restore
```

### Stap 3 — Installeer de EF Core CLI-tool (indien nog niet aanwezig)

```bash
dotnet tool install --global dotnet-ef
```

Als de tool al geïnstalleerd is maar verouderd:

```bash
dotnet tool update --global dotnet-ef
```

### Stap 4 — Configureer de connection string (User Secrets)

Gebruik **nooit** de echte connection string of wachtwoorden rechtstreeks in `appsettings.json` als het project publiek op GitHub staat. Gebruik in plaats daarvan .NET User Secrets, per project (moet uitgevoerd worden in de map van het **KickFive**-webproject, niet de Class Library):

```bash
cd KickFive
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:KickFiveContext" "Server=tcp:kickfive-server.database.windows.net,1433;Initial Catalog=KickFiveDb;User ID=<username>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

Vervang `<username>` en `<password>` door de echte SQL-server admin login (vraag deze op bij de projectverantwoordelijke, of maak je eigen Azure SQL Database aan — zie onderstaande sectie).

### Stap 5 — Configureer de SMTP-instellingen (e-mailverificatie)

```bash
dotnet user-secrets set "Email:SmtpHost" "sandbox.smtp.mailtrap.io"
dotnet user-secrets set "Email:SmtpPort" "2525"
dotnet user-secrets set "Email:SmtpUser" "<mailtrap-username>"
dotnet user-secrets set "Email:SmtpPass" "<mailtrap-password>"
```

Deze waarden vind je terug in je eigen Mailtrap-inbox onder de tab **"SMTP Settings"**, of via de projectverantwoordelijke.

### Stap 6 — Configureer de admin-gebruiker (optioneel, voor eerste opstart)

```bash
dotnet user-secrets set "AdminUser:Email" "admin@kickfive.com"
dotnet user-secrets set "AdminUser:Password" "Password123!"
```

Deze gebruiker wordt automatisch aangemaakt bij de eerste opstart, met de rol **Admin**.

### Stap 7 — Controleer je user secrets

```bash
dotnet user-secrets list
```

Je zou minstens deze keys moeten zien: `ConnectionStrings:KickFiveContext`, `Email:SmtpHost`, `Email:SmtpPort`, `Email:SmtpUser`, `Email:SmtpPass`, `AdminUser:Email`, `AdminUser:Password`.

### Stap 8 — Voer de EF Core-migraties uit

Vanuit de map van het **KickFive**-webproject:

```bash
dotnet ef database update
```

Of via de **Package Manager Console** in Visual Studio (zorg dat het KickFive-project als "Default project" ingesteld staat):

```powershell
Update-Database
```

Dit maakt alle tabellen aan in de (Azure) SQL-database op basis van de bestaande migraties in `Migrations/`.

### Stap 9 — Start de applicatie

```bash
dotnet run
```

Of druk op **F5** in Visual Studio. Bij de eerste opstart:
- worden de databanktabellen aangemaakt (indien nog niet gebeurd),
- worden de rollen `Admin` en `User` aangemaakt,
- wordt de admin-gebruiker aangemaakt (indien geconfigureerd in Stap 6),
- wordt de database geseed met testdata (`Field`, `User`, `Booking`) via `DbSeeder.SeedAsync()`.

De applicatie is nu bereikbaar op `https://localhost:7203` (of de poort die in de console getoond wordt).

## Gebruikte AI-tools en bronnen
-Ai gebruikt voor debuggen, hulp bij ajax implementatie, vertaling voor .resx files en Readme
-.Net Doc: https://learn.microsoft.com/en-us/dotnet/
- https://youtu.be/UY0AAnOhep4?si=06SWHINZqwUjJyqp
- https://youtu.be/P0i0aMPNa0I?si=61mbOCxxYQwheel8

