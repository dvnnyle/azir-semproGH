<p align="center">
  <img src="wwwroot/assets/readme_assets/top-Banner.png" width="100%">
</p>

# Krisis - semesterprosjekt BAC-IT - UIA


## Medlemmer [G14]
- Danny Nguyen Le


## 1. For å starte applikasjonen: (kan være litt feil / outdated)
For å gå inn i prosjektmappen: `cd azir-sempro`

Sett sterke, ulike passord lokalt (ikke i Git), og bygg/start webappen og databasen med Docker Compose:

```bash
$env:MYSQL_ROOT_PASSWORD = '<strong-root-password>'
$env:DB_APP_PASSWORD = '<different-strong-app-password>'
docker compose up --build
```

Åpne `http://localhost:8080` (lokal utvikling; porten bindes kun til localhost).

Docker Compose oppretter databasen `krisisdb` og tabellene automatisk fra SQL-filene i `Data/sql/`, inkludert ASP.NET Identity-tabellene fra `identity.sql`. Appen bruker en egen databasekonto med begrensede rettigheter; databaseporten er kun tilgjengelig fra localhost.

Stopp containerne med:

```bash
docker compose down
```

**Fikk du nye endringer med `git pull`?** Init-scriptene kjøres bare når databasevolumet opprettes første gang. Ta sikkerhetskopi og bruk den dokumenterte, additive SQL-endringen for eksisterende databaser. Ikke slett volumet bare for å oppdatere skjemaet; det sletter testdata.

Sett de samme miljøvariablene på nytt i hvert PowerShell-vindu før du starter Compose.

## 2. Lokal development

For å starte applikasjonen lokalt uten Docker:


```bash
cd azir-sempro
```
eller 
```bash
dotnet run --project azir-sempro
```

HOT
```bash
dotnet watch run
```

Applikasjonen vil være tilgjengelig på `http://localhost:5253`


# Links
- [Github](https://github.com/dvnnyle/azir-semproGH)
- [Trello](https://trello.com/b/9TofOcKn/azir-semproTL)
- [Figma](https://www.figma.com/design/43vJYUOC0oBQ4l5ZFmEe3o/azir-semproFM?t=pe3a3mwMyu3gA6o4-1)


<p align="center">
  <img src="wwwroot/assets/readme_assets/bottom-banner.png" width="100%">
</p>
