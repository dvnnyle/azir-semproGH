// Viser en bekreftelsesdialog før et skjema med data-confirm-delete sendes.
// Ligger i egen fil fordi Content-Security-Policy blokkerer innebygd JavaScript.
document.querySelectorAll("[data-confirm-delete]").forEach((form) => {
    form.addEventListener("submit", (event) => {
        const message = form.getAttribute("data-confirm-delete");
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });
});
