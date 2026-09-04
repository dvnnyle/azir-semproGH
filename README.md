<p align="center">
  <img src="wwwroot/assets/readme_assets/top-Banner.png" width="100%">
</p>

# Krisis - semesterprosjekt BAC-IT - UIA


## Medlemmer [G14]
- Danny Nguyen Le


## 1. For å starte applikasjonen: (kan være litt feil / outdated)
For å gå inn i prosjektmappen: `cd azir-sempro`

Bygg og start webapplikasjonen og MySQL-databasen med Docker Compose:

```bash
docker compose up --build
```

Åpne `http://localhost:8080`

Docker Compose oppretter databasen `krisisdb` og `Users`-tabellen automatisk fra `sql/users.sql`.

Stopp containerne med:

```bash
docker compose down
```

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

