// Lastes i <head>, slik at valgene gjelder før siden tegnes første gang.
(() => {
    const NOKKEL = 'krisis-tilgjengelighet';
    const GYLDIGE = ['hoy-kontrast', 'storre-tekst', 'fokus', 'redusert-bevegelse', 'lettlest-skrift', 'understrek'];
    const html = document.documentElement;

    const les = () => {
        try {
            const lagret = JSON.parse(localStorage.getItem(NOKKEL));
            return Array.isArray(lagret) ? lagret.filter(valg => GYLDIGE.includes(valg)) : [];
        } catch {
            return [];
        }
    };

    const lagre = valgte => {
        try {
            localStorage.setItem(NOKKEL, JSON.stringify([...valgte]));
        } catch {
            // Uten lagring gjelder valget bare på denne siden.
        }
    };

    const valgte = new Set(les());
    valgte.forEach(valg => html.classList.add(`a11y-${valg}`));

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-a11y]').forEach(bryter => {
            const valg = bryter.dataset.a11y;
            bryter.checked = valgte.has(valg);
            bryter.addEventListener('change', () => {
                if (bryter.checked) {
                    valgte.add(valg);
                } else {
                    valgte.delete(valg);
                }
                html.classList.toggle(`a11y-${valg}`, bryter.checked);
                lagre(valgte);
            });
        });
    });
})();
