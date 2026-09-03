# Rask test

## Lokal dev
- dotnet watch run
- http://localhost:5253

## Docker
- docker build -t azir-sempro .
- docker run -p 8080:8080 azir-sempro
- http://localhost:8080

Docker garanterer samme miljø for alle (same OS, same .NET, same setup).

STEP by STEP - D

1. Hent koden
   - git clone https://github.com/dvnnyle/azir-semproGH
   - cd azir-semproGH

2. Sjekk at det funker i Docker - trenger IKKE .NET SDK installert, Docker ordner alt selv inni containeren
   - docker build -t azir-sempro .
   - docker run -p 8080:8080 azir-sempro
   - http://localhost:8080
   - Stopp containeren (Ctrl+C) når du har bekreftet at det funker

3. Per nå sjekk at du har
    - .NET 10 SDK lokalt 

4. Kjør appen lokalt for å utvikle
   - dotnet watch run
   - http://localhost:5253
   - La denne stå på mens du jobber - den oppdaterer seg selv når du lagrer
   - http://10.0.0.8:5253 - IP NETWORK

5. Lag din egen branch før du endrer noe
   - git checkout -b <ditt-navn>
   - Bytt ut <ditt-navn> med ditt eget navn, f.eks:
     - git checkout -b danny
   - IKKE jobb direkte i main eller master - lag alltid din egen branch med ditt navn

6. Send endringene til gitHub
   - git add <filer> || eller git add . for alle
   - git commit -m "melding her av endringer"
   - git push origin <ditt-navn>
   - Åpne en Pull Request på GitHub nettside inn til main (ikke push rett til main)