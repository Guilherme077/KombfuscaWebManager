(() => {
    const button = document.getElementById('copy-profile-link');
    if (!button) return;

    const status = document.getElementById('profile-copy-status');
    const fallback = document.getElementById('profile-copy-fallback');
    const input = document.getElementById('profile-share-url');
    // Use the browser's public origin and the generated route, including any application base path.
    const profileUrl = new URL(button.dataset.profilePath, window.location.href).href;
    input.value = profileUrl;

    button.addEventListener('click', async () => {
        button.disabled = true;
        status.textContent = '';
        try {
            await navigator.clipboard.writeText(profileUrl);
            fallback.hidden = true;
            status.textContent = 'Link copiado!';
        } catch {
            // Clipboard access may be unavailable or denied; keep the URL available for manual copying.
            fallback.hidden = false;
            input.focus();
            input.select();
            status.textContent = 'Selecione e copie o link abaixo.';
        } finally {
            button.disabled = false;
        }
    });
})();
