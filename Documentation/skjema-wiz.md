# Skjema-wizen, hvordan er den bygd opp

Kort forklart: det er 4 sider på rad (steg 1-3 + kvittering), og hver side er
sin egen route/side i FormController. Ikke noe SPA-greie med JS-state, bare
ordentlig Controller/View/Redirect sånn oppgaven vil ha det.

## Stegene

1. /Form/Draw - velg Type (ressurs/behov) + tegn område eller slipp en nål på
   kartet. Geoman gjør selve tegninga (ikke noe vi har skrevet selv), vi bare
   leser ut punktene når Fullfør tegning trykkes.
2. /Form/Beskrivelse - tittel, kategori, prioritet (farge), beskrivelse.
3. /Form/Oppsummering - les-modus, viser alt du har fylt ut før du sender.
4. /Form/Kvittering - takke-side etter innsending, viser status og sånt.

## Kartet og tegninga

Kartet er Leaflet, og selve tegneverktøyet (område/polygon eller nål) er
Leaflet-Geoman (geoman.io/docs/leaflet), ikke noe vi har skrevet fra bunnen
av selv. Begge er lastet lokalt fra wwwroot/lib i stedet for CDN. Geoman
lager et vanlig Leaflet-lag (L.Polygon eller L.Marker) når du er ferdig å
tegne, og vi leser bare ut koordinatene fra det laget (getLatLngs/getLatLng
- vanlig Leaflet-API, ikke noe Geoman-spesifikt) og lagrer dem som JSON i
PunkterJson. Se Draw.cshtml for selve oppsettet.

## Hvordan data flyter mellom sidene

Siden det ikke er noe SPA så må dataen på en måte følge med fra side til
side. Løsningen er TempData (session-greie i ASP.NET) - hver POST lagrer hele
FormViewModel-en som JSON i TempData, og neste side leser den ut igjen
(HentUtkast/LagreUtkast nederst i FormController.cs). Sånn slipper man å
sende alle feltene som skjulte inputs på hver eneste side - bare feltene som
faktisk trengs på den siden vises, resten følger bare med i bagasjen.

## Type vs Farge - to forskjellige ting

Dette er den nyeste greia, litt lett å blande sammen (jeg blanda det selv
første gang):

- Type = hva det er. ressurs (du tilbyr noe) eller behov (du trenger noe).
  Velges som knapper på steg 1, styrer bl.a. overskriften på resten av
  sidene.
- Farge = hvor viktig det er. gronn/gul/rod. Matcher kart-fargene fra
  oppgaveteksten (case.md). Velges på steg 2.

Så en Meld behov-innsending kan fortsatt være grønn (lav prioritet) eller
rød (akutt) - de to feltene er helt uavhengige av hverandre.

## Databasen

Når du trykker Bekreft og send inn på Oppsummering, går det til
FormController.Submit(), som lager en Submission (egen klasse, ikke samme
som FormViewModel - se Models/Submission.cs) og lagrer den ekte i
Submissions-tabellen via Entity Framework. Skjema for tabellen ligger i
Data/sql/submissions.sql.

## Ting som ikke er ferdig enda (ikke min jobb, men greit å vite)

- Ingen innlogging. UserId på hver submission er null inntil noen bygger
  ekte login/auth - se TODO i Submit().
- Status endres ikke noe sted enda. Alt starter som ny, men det finnes
  ingen admin-side som kan flytte den til vurderes/tildelt/løst.
- Kart-siden (/Map) viser ikke disse innsendingene noe sted likevel - det
  er egen greie, ikke bygd av meg.
