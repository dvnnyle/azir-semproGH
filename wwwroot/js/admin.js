// "Skriv ut" i oversikten laster listen med valgt filter og åpner så utskrift.
// Parameteren fjernes, slik at en ny lasting av siden ikke skriver ut igjen.
const sideUrl = new URL(window.location.href);
if (sideUrl.searchParams.has("skrivut")) {
    sideUrl.searchParams.delete("skrivut");
    window.history.replaceState(null, "", sideUrl);
    window.addEventListener("load", () => window.print());
}

document.querySelectorAll("[data-confirm-delete]").forEach((form) => {
    form.addEventListener("submit", (event) => {
        const message = form.getAttribute("data-confirm-delete");
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });
});
