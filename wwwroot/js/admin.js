document.querySelectorAll("[data-confirm-delete]").forEach((form) => {
    form.addEventListener("submit", (event) => {
        const message = form.getAttribute("data-confirm-delete");
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });
});
