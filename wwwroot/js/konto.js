// Felles for kontosidene (login, registrering, nytt passord).
// Egen fil fordi CSP blokkerer inline-script uten nonce.

// Øye-knapp på alle passordfelt: veksler mellom skjult og synlig tekst.
document.querySelectorAll('input.konto-input[type="password"]').forEach(input => {
    const felt = document.createElement('div');
    felt.className = 'passord-felt';
    input.parentNode.insertBefore(felt, input);
    felt.appendChild(input);

    const knapp = document.createElement('button');
    knapp.type = 'button';
    knapp.className = 'passord-vis';
    knapp.setAttribute('aria-label', 'Vis passord');
    knapp.setAttribute('aria-pressed', 'false');
    knapp.innerHTML = '<i class="fa-solid fa-eye" aria-hidden="true"></i>';
    felt.appendChild(knapp);

    knapp.addEventListener('click', () => {
        const vises = input.type === 'password';
        input.type = vises ? 'text' : 'password';
        knapp.setAttribute('aria-label', vises ? 'Skjul passord' : 'Vis passord');
        knapp.setAttribute('aria-pressed', String(vises));
        knapp.querySelector('i').className = vises ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye';
        input.focus();
    });
});

// Toast med feilmelding (f.eks. feil e-post/passord). Lukkes med krysset eller etter 6 sekunder.
const toast = document.getElementById('kontoToast');
if (toast) {
    const lukk = () => {
        toast.classList.add('konto-toast--skjul');
        toast.addEventListener('animationend', () => toast.remove(), { once: true });
    };
    toast.querySelector('.konto-toast__lukk').addEventListener('click', lukk);
    setTimeout(lukk, 6000);
}
