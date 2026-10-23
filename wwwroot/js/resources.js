(() => {
    const colorByPriority = {
        rod: '#ef4444',
        gul: '#f59e0b',
        gronn: '#10b981',
        bla: '#3b82f6'
    };
    const tileUrl = 'https://cache.kartverket.no/v1/wmts/1.0.0/topo/default/webmercator/{z}/{y}/{x}.png';
    const dialog = document.getElementById('resourceDetail');
    const detailMapElement = document.getElementById('detailMap');
    let detailMap;

    function readPoints(json) {
        try {
            const points = JSON.parse(json || '[]');
            if (!Array.isArray(points)) return [];
            return points
                .map(point => [Number(point.lat), Number(point.lng)])
                .filter(([lat, lng]) => Number.isFinite(lat) && Number.isFinite(lng)
                    && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180);
        } catch {
            return [];
        }
    }

    function drawMap(element, points, color) {
        const map = L.map(element, {
            attributionControl: false,
            boxZoom: false,
            doubleClickZoom: false,
            dragging: false,
            keyboard: false,
            scrollWheelZoom: false,
            tap: false,
            touchZoom: false,
            zoomControl: false
        });
        L.tileLayer(tileUrl, { maxZoom: 18, noWrap: true }).addTo(map);

        if (points.length >= 3) {
            const polygon = L.polygon(points, {
                color,
                fillColor: color,
                fillOpacity: 0.28,
                weight: 2
            }).addTo(map);
            map.fitBounds(polygon.getBounds(), { padding: [10, 10], maxZoom: 15 });
        } else if (points.length > 0) {
            points.forEach(point => L.circleMarker(point, {
                color: '#f8fafc',
                fillColor: color,
                fillOpacity: 1,
                radius: 6,
                weight: 2
            }).addTo(map));
            if (points.length === 1) {
                map.setView(points[0], 14);
            } else {
                map.fitBounds(L.latLngBounds(points), { padding: [10, 10], maxZoom: 15 });
            }
        } else {
            map.setView([58.1635601, 8.0031555], 5);
        }

        return map;
    }

    function colorFor(value) {
        return colorByPriority[value] || colorByPriority.gronn;
    }

    document.querySelectorAll('[data-map-preview]').forEach(element => {
        const points = readPoints(element.dataset.points);
        const map = drawMap(element, points, colorFor(element.dataset.color));
        element._leafletMap = map;
    });

    function showDetails(card) {
        const points = readPoints(card.dataset.points);
        const color = colorFor(card.dataset.color);

        document.getElementById('detailTitle').textContent = card.dataset.title || '';
        document.getElementById('detailCategory').textContent = card.dataset.category || 'Ikke oppgitt';
        document.getElementById('detailLocation').textContent = card.dataset.location || 'Sted ikke oppgitt';
        document.getElementById('detailSubmitted').textContent = card.dataset.submitted || '';
        document.getElementById('detailDescription').textContent = card.dataset.description || '';

        const type = document.getElementById('detailType');
        type.textContent = card.dataset.type || '';
        type.className = `resource-badge ${card.dataset.type === 'Etterspørres' ? 'resource-badge--need' : 'resource-badge--offer'}`;

        const priority = document.getElementById('detailPriority');
        priority.textContent = card.dataset.priority || '';
        priority.style.setProperty('--priority-color', color);

        document.getElementById('detailStatus').textContent = card.dataset.status || '';

        if (detailMap) {
            detailMap.remove();
        }
        detailMap = drawMap(detailMapElement, points, color);

        const mapLink = document.getElementById('detailMapLink');
        if (points.length > 0) {
            const [lat, lng] = points[0];
            mapLink.href = `https://www.openstreetmap.org/?mlat=${encodeURIComponent(lat)}&mlon=${encodeURIComponent(lng)}#map=15/${encodeURIComponent(lat)}/${encodeURIComponent(lng)}`;
            mapLink.hidden = false;
        } else {
            mapLink.hidden = true;
        }

        dialog.showModal();
        window.requestAnimationFrame(() => detailMap.invalidateSize());
    }

    document.querySelectorAll('[data-resource-open]').forEach(card => {
        card.addEventListener('click', () => showDetails(card));
    });

    document.querySelectorAll('[data-detail-close]').forEach(button => {
        button.addEventListener('click', () => dialog.close());
    });

    dialog.addEventListener('click', event => {
        if (event.target === dialog) dialog.close();
    });
})();
