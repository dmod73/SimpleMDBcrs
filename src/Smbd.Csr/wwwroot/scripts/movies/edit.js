import { $, apiFetch, renderStatus, getQueryParam, captureMovieForm } from '/scripts/common.js';

(async function initMovieEdit() {
    const id = getQueryParam('id');
    const form = $('#movie-form');
    const statusEl = $('#status');

    // Disables form fields and do not allow editing if movie id is missing.
    if (!id) {
        renderStatus(statusEl, 'err', 'Missing ?id in URL.');
        form.querySelectorAll('input,textarea,button,select').forEach(
            el => el.disabled = true
        );
        return;
    }

    // Populates form with data from movie (id) fetched from the API server.
    try {
        const m = await apiFetch(`/movies/${encodeURIComponent(id)}`);
        form.title.value = m.title ?? '';
        form.year.value = m.year ?? '';
        form.description.value = m.description ?? '';
        renderStatus(statusEl, 'ok', 'Loaded movie. You can edit and save.');
    } catch (err) {
        renderStatus(statusEl, 'err', `Failed to load data: ${err.message}`);
        return;
    }

    // Invokes the given function whenever the form 'submit' event is triggered.
    form.addEventListener('submit', async (ev) => {
        ev.preventDefault();
        const payload = captureMovieForm(form);

        // Input validation and feedback goes here. For example:
        //
        // if(payload.year > new Date().getFullYear()) {
        //     renderStatus(statusEl, 'err', 'Movie year cannot be in the future.');
        //     return;
        // } else if (...) {
        //     ...
        // }

        try {
            const updated = await apiFetch(`/movies/${encodeURIComponent(id)}`, {
                method: 'PUT',
                body: JSON.stringify(payload),
            });
            renderStatus(
                statusEl,
                'ok',
                `Updated movie #${updated.id} "${updated.title}" (${updated.year}).`
            );
        } catch (err) {
            renderStatus(statusEl, 'err', `Update failed: ${err.message}`);
        }
    });
})();
