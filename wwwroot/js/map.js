(() => {
    const farger = { rod: '#ef4444', gul: '#f59e0b', gronn: '#10b981' };
    const fliser = 'https://cache.kartverket.no/v1/wmts/1.0.0/topo/default/webmercator/{z}/{y}/{x}.png';

    const kart = L.map('map', { zoomControl: false }).setView([58.1467, 7.9956], 11);
    L.control.zoom({ position: 'topright' }).addTo(kart);
    L.tileLayer(fliser, { maxZoom: 18 }).addTo(kart);

    const alleBounds = [];
    const nodetatLag = [];

    document.querySelectorAll('.kart-listeelement').forEach(el => {
        const punkter = JSON.parse(el.dataset.points || '[]').map(p => [p.lat, p.lng]);
        if (punkter.length === 0) return;

        const farge = farger[el.dataset.color] || farger.gronn;
        const esc = t => { const d = document.createElement('div'); d.textContent = t; return d.innerHTML; };
        const ikon = el.dataset.erbehov === '1' ? 'fa-triangle-exclamation' : 'fa-hand-holding-heart';

        const popup = `
            <div class="kart-popup">
                <h3>${esc(el.dataset.title)}</h3>
                <p><i class="fa-solid ${ikon}"></i> ${esc(el.dataset.type)} - ${esc(el.dataset.category)}</p>
                <span class="kart-popup__status">${esc(el.dataset.status)}</span>
                <a href="/Resources?apne=${el.dataset.id}" class="kart-popup__knapp">Gå til innsending</a>
            </div>`;

        let lag;
        if (punkter.length >= 3) {
            lag = L.polygon(punkter, { color: farge, fillColor: farge, fillOpacity: 0.35 });
        } else {
            const nal = L.divIcon({
                html: `<i class="fa-solid fa-location-dot" style="color:${farge};font-size:30px;-webkit-text-stroke:2px #f1f5f9;paint-order:stroke fill"></i>`,
                className: '',
                iconSize: [30, 30],
                iconAnchor: [15, 30]
            });
            lag = L.marker(punkter[0], { icon: nal });
        }

        lag.bindPopup(popup).addTo(kart);
        el.addEventListener('click', () => lag.openPopup());
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
            kart.fitBounds(L.latLngBounds(alleBounds), { padding: [80, 80], maxZoom: 11 });
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
                    .bindPopup(`<div class="kart-popup"><h3>${s.navn}</h3><p>${s.adresse}</p></div>`)
                    .addTo(kart);
                nodetatLag.push(nodetatMerke);
                alleBounds.push([s.lat, s.lng]);
            });
        })
        .catch(() => {})
        .finally(() => setTimeout(visVisning, 50));

    const oppdaterFilter = () => {
        const vis = type => document.querySelector(`.kart-filter input[data-filter="${type}"]`)?.checked ?? true;
        const visRessurs = vis('ressurs');
        const visBehov = vis('behov');
        const visNodetat = vis('nodetat');

        document.querySelectorAll('.kart-listeelement').forEach(el => {
            const skalVises = el.dataset.erbehov === '1' ? visBehov : visRessurs;
            el.style.display = skalVises ? '' : 'none';
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
})();