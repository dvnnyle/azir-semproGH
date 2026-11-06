<p align="center">
  <img src="wwwroot/assets/readme_assets/top-Banner.png" width="100%">
</p>

# Krisis - semesterprosjekt BAC-IT - UiA

Krisis er en ressurs- og behovsportal for kriseberedskap. Offentlige aktører kan melde behov, og privatpersoner, bedrifter og frivillige kan tilby ressurser. Kriseledelsen får oversikt på kart og kan koble behov med riktig ressurs.

## Medlemmer [G14]

<!-- Hvert medlem legger inn navnet sitt her. -->
- Danny Nguyen Le

## Kjør med Docker

Du trenger bare [Docker Desktop](https://www.docker.com/products/docker-desktop/). Start det før du kjører kommandoene under.

**1. Hent koden**

```bash
git clone https://github.com/dvnnyle/azir-semproGH.git
cd azir-semproGH
```

**2. Velg passord og start**

Bytt ut passordene med dine egne. De skal ikke legges i Git.

Windows (PowerShell):

```powershell
$env:MYSQL_ROOT_PASSWORD = 'ditt-root-passord'
$env:DB_APP_PASSWORD = 'ditt-app-passord'
docker compose up -d --build
```

Mac og Linux:

```bash
export MYSQL_ROOT_PASSWORD='ditt-root-passord'
export DB_APP_PASSWORD='ditt-app-passord'
docker compose up -d --build
```

**3. Åpne appen**

Gå til `http://localhost:8080`. Hvis siden ikke svarer første gang, vent 30 sekunder og kjør `docker compose up -d app`.

**Dette er greit å vite**

- Sett de samme passordene på nytt i hver nye terminal før du bruker `docker compose`. Databasen beholder passordene fra første oppstart.
- Etter `git pull` kjører du `docker compose up -d --build`.
- Stopp appen med `docker compose down`.
- `docker compose down -v` sletter databasen for godt.
- Tabellene opprettes fra `Data/sql/` første gang databasen startes. Endringer i disse filene gjelder derfor ikke en database som allerede finnes. Se [Drift](Documentation/Documentation.md#drift).

**Har du en database fra før?** Da må du legge til kommunefeltet én gang, ellers feiler sidene med `Unknown column`. Bruk root-passordet du valgte første gang:

```bash
docker exec krisis-db mariadb -uroot -pditt-root-passord krisisdb -e "ALTER TABLE Submissions ADD COLUMN IF NOT EXISTS Kommune VARCHAR(100) NULL AFTER Adresse"
```

## Testbrukere

Disse opprettes automatisk når appen kjører i Development, som er standard med `docker-compose.yml`. De er bare for lokal testing.

| Rolle | E-post | Passord |
|---|---|---|
| Admin | `admin@krisis.no` | `!Krisis123` |
| Heimevernet | `bruker@heimevernet.no` | `!Heimervernet123` |

Begge kommer til portalen etter innlogging. Andre kan registrere seg selv på `/register`.

## Lokal utvikling

Med dette kjører appen utenfor Docker og oppdaterer seg når du lagrer, mens databasen fortsatt kjører i Docker. Du trenger også [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

Windows (PowerShell):

```powershell
$env:MYSQL_ROOT_PASSWORD = 'ditt-root-passord'
$env:DB_APP_PASSWORD = 'ditt-app-passord'
docker compose up -d db
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3307;Database=krisisdb;User=krisisapp;Password=$env:DB_APP_PASSWORD;"
dotnet watch run --launch-profile http
```

Mac og Linux:

```bash
export MYSQL_ROOT_PASSWORD='ditt-root-passord'
export DB_APP_PASSWORD='ditt-app-passord'
docker compose up -d db
export ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3307;Database=krisisdb;User=krisisapp;Password=$DB_APP_PASSWORD;"
dotnet watch run --launch-profile http
```

Appen åpnes på `http://localhost:5253`. Bruk `127.0.0.1`, ikke `localhost`, i tilkoblingen. På Windows kan `localhost` gjøre hver side rundt to sekunder tregere.

## Tester

Stopp `dotnet watch` først, fordi den låser byggfilene. Kjør så:

```bash
dotnet test azir-sempro.Tests
```

## Dokumentasjon

- [Drift, systemarkitektur, sikkerhet og testing](Documentation/Documentation.md)
- [Git-flyt for gruppa](howToRun.md)

## Bruk av KI

Vi har brukt GitHub Copilot, Claude og ChatGPT som hjelp i utviklingen. Hvert medlem fyller inn sin egen bruk i tabellen og legger til egne prompts.

| Fase | Verktøy | Bruk |
|---|---|---|
| Koding og feilsøking | Claude Code | Portal, kobling av behov og ressurser, kart, feilretting og tester |
| Koding | GitHub Copilot | Registrering, profilside, admin-oversikt, kartskjema, sikkerhetstiltak og validering av inndata |
| Design og tekst | Claude Code | Kontrast, skriftstørrelser, layout og tekst på nettsiden |
| Dokumentasjon | Claude Code | Utkast til denne README-filen |

**Eksempler på prompts (språkvasket)**

- "Hvorfor tar det så lang tid å navigere mellom sidene? Finn årsaken."
- "Filteret ser ikke ut til å virke. Undersøk hvorfor og rett feilen."
- "Lag en innstillingsside og legg den til i appen."
- "Skrift og lav kontrast er vanskelig å lese. Tenk deg et område med mye røyk: hvordan skal brukeren lese og bruke systemet?"

**Dette lærte vi**

For å kunne spørre må du vite hva du skal lage og få til. KI er i dag et suverent verktøy som kan få til mye, men du må bevisst vite hva du skal bruke KI til for å få det til. Det krever gode ferdigheter fra medlemmene. Du må kunne feilsøke og finne problemer selv, samt forstå koden du får tilbake, og ikke la KI gjøre hele jobben. Da vi er en gruppe på 6 med ulike ferdigheter når det kommer til KI, har vi også brukt det på ulike måter som passer hver enkelt.

Forslag fra KI må alltid testes i nettleseren, ettersom de ikke alltid fungerer. KI kjenner heller ikke oppdraget og brukerne like godt som oss. Flere av endringene kom fra tilbakemeldinger vi fikk, for eksempel om tekst som ikke passet budskapet og om liten skrift og lav kontrast.

**Forrutsettninger med KI**

Noen medlemmer kan oppleve at de går tom for credits og må vente på cooldown. I tillegg kan ikke KI alltid løse det du spør om, og svarene kan se riktige ut selv om de er feil. KI kan også gjøre mange endringer på kort tid, så det blir mer kode å gå gjennom. Handlinger som kan slette data må vi godkjenne selv. Passord og personopplysninger skal heller ikke legges inn i prompts, ettersom teksten sendes til en ekstern tjeneste.

## Lenker

- [GitHub](https://github.com/dvnnyle/azir-semproGH)
- [Trello](https://trello.com/b/9TofOcKn/azir-semproTL)
- [Figma](https://www.figma.com/design/43vJYUOC0oBQ4l5ZFmEe3o/azir-semproFM?t=pe3a3mwMyu3gA6o4-1)

<p align="center">
  <img src="wwwroot/assets/readme_assets/bottom-banner.png" width="100%">
</p>
