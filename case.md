# Kriseberedskap – ressurs- og behovsportal for Totalforsvaret


I denne oppgaven skal studentene utvikle en webbasert løsning for støtte til
kriseberedskap og samhandling i Totalforsvaret.
Løsningen skal bidra til å koble behov fra offentlige aktører med tilgjengelige ressurser
fra innbyggere, frivillige organisasjoner og næringsliv ved større hendelser eller kriser. I
utgangspunktet skal løsningen fungere som en selvstendig løsning/applikasjon som
kan anvendes av kriseledelsen (både sivilt og militært) for å få rask oversikt over
tilgjengelige ressurser som er relevante i forhold til aktuell situasjon.
Bakgrunnen for oppgaven er behovet for bedre digital samhandling mellom sivile og
offentlige aktører i situasjoner som naturhendelser, ekstremvær, strømbrudd, bortfall
av kommunikasjon, evakuering eller andre samfunnskriser.
Under slike hendelser kan det være behov for rask oversikt over tilgjengelige ressurser
som kjøretøy, maskiner, droneoperatører, aggregater, sambandsutstyr, personell eller
tilgang til lokale ressurser som sand, grus eller drivstoff.
Mot slutten av semesteret blir det en EXPO hvor studentene kan presentere løsningene
som er utviklet. De tre beste gruppene blir premiert med gavekort i Kantina. Løsningene
blir da vurdert ut fra funksjonalitet og kravoppnåelse, teknisk kvalitet og arkitektur,
brukervennlighet og design, samt innovasjon og samfunnsnytte. Studentene oppfordres
til å tenke kreativ og innovativt i oppgaveløsningen


Studentene skal utvikle en løsning hvor:
• offentlige aktører kan registrere behov for bistand,
• brukere kan registrere hvilke ressurser de kan tilby,
• informasjon visualiseres i kart ved hjelp av Leaflet,
• brukere autentiseres med sikker innlogging og autentisering,
• data lagres i MariaDB,
• løsningen kjøres lokalt ved hjelp av Docker Compose eller .NET Aspire.
Applikasjonen skal utvikles i ASP.NET Core og versjonshåndtereing i GitHub.
Oppgaven skal gi studentene erfaring med:
• moderne webutvikling,
• sikker autentisering,
• databaser,
• containerbasert utvikling,
• kart- og geodata,
• samarbeid i utviklingsteam,
• dokumentasjon og presentasjon av tekniske løsninger.
Intressenter
1. Offentlig aktør
• Stat
• Kommune
• Politi
• Brann
• Helse
• Sivilforsvaret
• Forsvaret/Heimevernet
Skal kunne registrere:
• Type behov
• Geografisk område
• Tidspunkt/frister
• Prioritet
• Kontaktpunkt
• Status
Eksempler på behov:
• Transport
• Droneobservasjon
• Strøm/aggregat
• Snørydding
• Sand/grus
• Maskiner
• Evakuering
• Samband
• Mannskap
• Lokaler
2. Ressursleverandør
• Privatperson
• Bedrift
• Bonde
• Entreprenør
• Frivillig organisasjon
• Droneoperatør
Skal kunne registrere:
• Type av ressurs
• Geografisk område
• Tidspunkt/frister
• Kontaktpunkt
• Status
Eksempler på hva de kan tilby:
• Drone og droneoperatør
• ATV
• Traktor
• Gravemaskin
• Hjullaster
• Skogsmaskin
• Snøskuter
• Lastebil
• Sand/grus
• Aggregat
• Lokaler
• Sambandsutstyr
• Båt
• Mannskap/frivillige
Funksjonaliteter
Løsningen bør ha:
• Brukeradministrasjon (registrering, innlogging med brukernavn og passord,
roller/tilganger)
• Skille mellom offentlige brukere og ressursleverandører
• Kun innloggede brukere får registrere/booke behov/ressurser
• Offentlige brukere kan se behov og tilgjengelige ressurser
• Ressursleverandører kan se egne registrerte ressurser
• Ressursleverandører kan slette registrerte resurser
• Matching mellom behov og ressurser
• Statusflyt: ny, vurderes, tildelt og løst
• Mulighet for å skrive ut resurslister for sin region
Kreative løsninger
Studentene oppfordres til å tenke kreativt, utforme ideer, og bruke teknologi på nye
måter. Sett gjerne av et eget kapittel i prosjektrapporten som setter søkelys på
alternative og nye måter å bruke teknologi på innenfor problemstillingene dere jobber
med.
Kartfunksjon med Leaflet
Kartet kan vise:
• Registrerte behov
• Tilgjengelige ressurser
• Ressurser nær et behov
• Fargekoder:
o Rød = akutt behov
o Gul = planlagt behov
o Grønn = tilgjengelig ressurs
o Blå = offentlig aktør
Tekniske krav
Studentene skal lage en løsning som minst har:
• ASP.NET Core webapplikasjon
• MariaDB-database
• Docker Compose eller .NET Aspire
• GitHub-repository
• Innlogging med tofaktorautentisering
• Kartvisning med Leaflet
• README med kjøreinstruksjon
For viderekomne
Mulige utvidelser
• E-post for å varsle om at det eventuelt trengs ressurser
• Mulighet til å velge flere karttjenester fra GEONORGE (for eksempel flybilder)
• Eksport til CSV/JSON
• Enkel risiko-/prioritetsmodell
• Audit-logg
• Avstandssøk og adressesøk i kart
• Kategorier for ressurstyper
• Vedlegg/bilder av ressurser