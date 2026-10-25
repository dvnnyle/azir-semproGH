(() => {
    const farger = { rod: '#ef4444', gul: '#f59e0b', gronn: '#10b981' };
    const fliser = 'https://cache.kartverket.no/v1/wmts/1.0.0/topo/default/webmercator/{z}/{y}/{x}.png';

    const kart = L.map('map', { zoomControl: false }).setView([58.1467, 7.9956], 11);
    L.control.zoom({ position: 'topright' }).addTo(kart);
    L.tileLayer(fliser, { maxZoom: 18 }).addTo(kart);

    const alleBounds = [];

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

        alleBounds.push(...punkter);
    });

    setTimeout(() => {
        kart.invalidateSize();
        if (alleBounds.length > 0) {
            kart.fitBounds(L.latLngBounds(alleBounds), { padding: [80, 80] });
        }
    }, 50);
})();