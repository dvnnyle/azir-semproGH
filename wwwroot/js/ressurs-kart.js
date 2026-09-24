/* Delt tegne-logikk for de read-only "ambient" kartene i skjema-flyten
   (Beskrivelse, Oppsummering, Kvittering). Hver side navigerer til en helt
   ny HTML-side, sa ingen Leaflet-instans overlever mellom stegene - det
   eneste som faktisk folger med er koordinat-JSON-en (PunkterJson). Denne
   funksjonen gjor det samme oppsettet (kart, polygon/nal, fitBounds) som
   ellers matte kopieres i hver .cshtml-fil. */
function tegnRessursOmradeKart(elementId, punkterJsonStr, options) {
    options = options || {};
    const map = L.map(elementId, Object.assign(
        { minZoom: 4, attributionControl: false, dragging: false, scrollWheelZoom: false },
        options.mapOptions
    ));
    L.tileLayer('https://cache.kartverket.no/v1/wmts/1.0.0/topo/default/webmercator/{z}/{y}/{x}.png', { maxZoom: 18, noWrap: true }).addTo(map);

    const nalIkon = L.divIcon({ html: '<div class="marker-pin"><i class="fa-solid fa-location-dot"></i></div>', className: '', iconSize: [30, 30], iconAnchor: [15, 30] });
    const punkter = JSON.parse(punkterJsonStr || '[]').map(p => [p.lat, p.lng]);
    const erOmrade = punkter.length >= 3;
    const bounds = L.latLngBounds(punkter);
    const senter = bounds.getCenter();

    const farge = options.farge || '#10b981';
    if (erOmrade) {
        L.polygon(punkter, { color: farge, fillColor: farge, fillOpacity: 0.35 }).addTo(map);
    } else {
        L.marker(senter, { icon: nalIkon }).addTo(map);
    }

    // maxZoom: 17 holder zoomen konsekvent med Draw-siden, som alltid tegner pa zoom 17
    const padding = options.padding || [40, 40];
    setTimeout(() => {
        map.invalidateSize();
        map.fitBounds(erOmrade ? bounds : bounds.pad(2), { padding, maxZoom: 17, animate: false });
    }, 50);

    return { map, punkter, erOmrade, bounds, senter };
}
