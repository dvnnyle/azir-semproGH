(() => {
    const farger = { rod: '#ef4444', gul: '#f59e0b', gronn: '#10b981' };
    const fliser = 'https://cache.kartverket.no/v1/wmts/1.0.0/topo/default/webmercator/{z}/{y}/{x}.png';

    const kart = L.map('map', { zoomControl: false }).setView([58.1467, 7.9956], 13.4);
    L.control.zoom({ position: 'topright' }).addTo(kart);
    L.tileLayer(fliser, { maxZoom: 18 }).addTo(kart);

    const alleBounds = [];
    const nodetatLag = [];

    // Kartet fyller hele vinduet, men toppmenyen og sidepanelet ligger oppå det.
    // På smale skjermer er panelet et bunnark (samme grense som i form.css).
    const skjultOmrade = () => {
        const panel = document.querySelector('.ressurs-split__form').getBoundingClientRect();
        const erBunnark = window.innerWidth <= 480;
        return {
            topp: document.querySelector('.nav-bar').getBoundingClientRect().bottom,
            venstre: erBunnark ? 0 : panel.right,
            bunn: erBunnark ? window.innerHeight - panel.top : 0
        };
    };

    const startOmrade = skjultOmrade();
    const popupValg = {
        autoPanPaddingTopLeft: [startOmrade.venstre + 10, startOmrade.topp + 10],
        autoPanPaddingBottomRight: [10, startOmrade.bunn + 10]
    };

    // Kartsenteret som gjør at punktet havner midt i den synlige delen av kartet.
    const senterForSynlig = (punkt, zoom, popupHoyde = 0) => {
        const { topp, venstre, bunn } = skjultOmrade();
        const forskyvning = L.point(venstre / 2, (topp - bunn + popupHoyde) / 2);
        return kart.unproject(kart.project(punkt, zoom).subtract(forskyvning), zoom);
    };

    const midtstillPopup = lag => {
        lag.openPopup();
        const popup = lag.getPopup();
        kart.panTo(senterForSynlig(popup.getLatLng(), kart.getZoom(), popup.getElement().offsetHeight));
    };

    const posisjonKnapp = L.DomUtil.create('button', 'kart-posisjon leaflet-bar');
    posisjonKnapp.type = 'button';
    posisjonKnapp.title = 'Bruk min posisjon';
    posisjonKnapp.setAttribute('aria-label', 'Bruk min posisjon');
    posisjonKnapp.innerHTML = '<i class="fa-solid fa-location-crosshairs" aria-hidden="true"></i>';
    L.DomEvent.disableClickPropagation(posisjonKnapp);

    const posisjonKontroll = L.control({ position: 'topright' });
    posisjonKontroll.onAdd = () => posisjonKnapp;
    posisjonKontroll.addTo(kart);

    let minPosisjon = null;

    // setView: false av samme grunn som i skjemaet: Leaflets egen zoom tilpasser seg
    // nøyaktigheten, og den kan være flere hundre meter uten ekte GPS.
    posisjonKnapp.addEventListener('click', () => {
        posisjonKnapp.disabled = true;
        kart.locate({ setView: false, maxZoom: 20, enableHighAccuracy: true });
    });

    kart.on('locationfound', e => {
        posisjonKnapp.disabled = false;
        kart.setView(senterForSynlig(e.latlng, 16), 16);
        if (minPosisjon) kart.removeLayer(minPosisjon);
        const prikk = L.divIcon({ html: '<span class="kart-deg"></span>', className: '', iconSize: [18, 18], iconAnchor: [9, 9] });
        minPosisjon = L.marker(e.latlng, { icon: prikk, zIndexOffset: 1000 })
            .bindTooltip('Du er her')
            .addTo(kart);
    });

    kart.on('locationerror', e => {
        posisjonKnapp.disabled = false;
        alert('Fikk ikke tilgang til posisjonen din. (kode ' + e.code + ': ' + e.message + ')');
    });

    document.querySelectorAll('.kart-listeelement').forEach(el => {
        const punkter = JSON.parse(el.dataset.points || '[]').map(p => [p.lat, p.lng]);
        if (punkter.length === 0) return;

        const farge = farger[el.dataset.color] || farger.gronn;
        const esc = t => { const d = document.createElement('div'); d.textContent = t; return d.innerHTML; };
        const ikon = el.dataset.erbehov === '1' ? 'fa-triangle-exclamation' : 'fa-hand-holding-heart';

        const prioritet = el.dataset.priority ? ` · ${esc(el.dataset.priority)} prioritet` : '';

        const popup = `
            <div class="kart-popup">
                <h3>${esc(el.dataset.title)}</h3>
                <p><i class="fa-solid ${ikon}"></i> ${esc(el.dataset.type)} - ${esc(el.dataset.category)}</p>
                <p><i class="fa-solid fa-location-dot"></i> ${esc(el.dataset.address)}</p>
                <p><i class="fa-solid fa-circle-info"></i> ${esc(el.dataset.status)}${prioritet}</p>
                <p><i class="fa-solid fa-user"></i> ${esc(el.dataset.publisher)}</p>
                <a href="/Resources?apne=${el.dataset.id}" class="kart-popup__knapp">Gå til innsending</a>
            </div>`;

        let lag;
        if (punkter.length >= 3) {
            lag = L.polygon(punkter, { color: farge, fillColor: farge, fillOpacity: 0.45, weight: 4, className: 'kart-omrade' });
        } else {
            const nal = L.divIcon({
                // Ulik startforsinkelse, så ikke alle nålene pulserer i takt.
                html: `<span class="kart-nal-puls" style="background:${farge};animation-delay:${(el.dataset.id % 8) * -0.25}s"></span><i class="fa-solid fa-location-dot" style="color:${farge};font-size:30px;-webkit-text-stroke:2px #f1f5f9;paint-order:stroke fill"></i>`,
                className: 'kart-nal',
                iconSize: [30, 30],
                iconAnchor: [15, 30]
            });
            lag = L.marker(punkter[0], { icon: nal });
        }

        lag.bindPopup(popup, popupValg).addTo(kart);
        el.addEventListener('click', () => midtstillPopup(lag));
        el._kartLag = lag;

        alleBounds.push(...punkter);
    });

    const nodetatIkoner = {
        sykehus: { ikon: 'fa-hospital', farge: '#16a34a' },
        ambulanse: { ikon: 'fa-truck-medical', farge: '#16a34a' },
        politi: { ikon: 'fa-building-shield', farge: '#1e3a8a' },
        brann: { ikon: 'fa-fire-extinguisher', farge: '#dc2626' }
    };

    const visVisning = () => {
        kart.invalidateSize();
        if (alleBounds.length > 0) {
            kart.fitBounds(L.latLngBounds(alleBounds), { padding: [80, 80], maxZoom: 13.4 });
        }
    };

    fetch('/data/nodetat-stasjoner.json')
        .then(r => r.json())
        .then(stasjoner => {
            stasjoner.forEach(s => {
                const { ikon, farge } = nodetatIkoner[s.type] || nodetatIkoner.brann;
                const merke = L.divIcon({
                    html: `<i class="fa-solid ${ikon}" style="color:${farge};font-size:22px;-webkit-text-stroke:1.5px #f1f5f9;paint-order:stroke fill"></i>`,
                    className: '',
                    iconSize: [22, 22],
                    iconAnchor: [11, 22]
                });
                const nodetatMerke = L.marker([s.lat, s.lng], { icon: merke })
                    .bindPopup(`<div class="kart-popup"><h3>${s.navn}</h3><p>${s.adresse}</p></div>`, popupValg)
                    .addTo(kart);
                nodetatLag.push(nodetatMerke);
            });
        })
        .catch(() => {})
        .finally(() => setTimeout(visVisning, 50));

    const PER_SIDE = 15;
    const lastFlere = document.getElementById('kartLastFlere');
    let antallVist = PER_SIDE;

    const vis = type => document.querySelector(`.kart-filter input[data-filter="${type}"]`)?.checked ?? true;

    // Bare listen begrenses. Alle nåler vises på kartet uansett.
    const oppdaterListe = () => {
        let treff = 0;
        document.querySelectorAll('.kart-listeelement').forEach(el => {
            const valgt = vis(el.dataset.erbehov === '1' ? 'behov' : 'ressurs');
            if (valgt) treff++;
            el.style.display = valgt && treff <= antallVist ? '' : 'none';
        });
        lastFlere.hidden = treff <= antallVist;
        lastFlere.textContent = `Last inn flere (${Math.max(treff - antallVist, 0)})`;
    };

    lastFlere.addEventListener('click', () => {
        antallVist += PER_SIDE;
        oppdaterListe();
    });

    const oppdaterFilter = () => {
        const visRessurs = vis('ressurs');
        const visBehov = vis('behov');
        const visNodetat = vis('nodetat');

        antallVist = PER_SIDE;
        oppdaterListe();

        document.querySelectorAll('.kart-listeelement').forEach(el => {
            const skalVises = el.dataset.erbehov === '1' ? visBehov : visRessurs;
            if (!el._kartLag) return;
            if (skalVises) {
                kart.addLayer(el._kartLag);
            } else {
                kart.removeLayer(el._kartLag);
            }
        });

        nodetatLag.forEach(lag => {
            if (visNodetat) {
                kart.addLayer(lag);
            } else {
                kart.removeLayer(lag);
            }
        });
    };

    document.querySelectorAll('.kart-filter input[data-filter]').forEach(input => {
        input.addEventListener('change', oppdaterFilter);
    });

    oppdaterListe();
})();