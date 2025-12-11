import { $, apiFetch, renderStatus, getQueryParam } from '/scripts/common.js';

(async function initMovieView() {
    const id = getQueryParam('id');
    const statusEl = $('#status');
    if (!id) return renderStatus(statusEl, 'err', 'Missing ?id in URL.');

    try {
        const m = await apiFetch(`/movies/${encodeURIComponent(id)}`);
        $('#movie-id').textContent = m.id;
        $('#movie-title').textContent = m.title;
        $('#movie-year').textContent = m.year;
        $('#movie-desc').textContent = m.description || '—';
        $('#edit-link').href = `/movies/edit.html?id=${encodeURIComponent(m.id)}`;
        renderStatus(statusEl, 'ok', 'Movie loaded successfully.');
    } catch (err) {
        renderStatus(statusEl, 'err', `Failed to load movie ${id}: ${err.message}`);
    }
})();
