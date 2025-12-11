import { $, apiFetch, renderStatus, clearChildren, getQueryParam } from '/scripts/common.js';

(async function initMoviesIndex() {
    const page = Math.max(1, Number(getQueryParam('page') || localStorage.getItem('page') || '1'));
    const size = Math.min(100, Math.max(1, Number(getQueryParam('size') || localStorage.getItem('size') || '9')));

    localStorage.setItem('page', page);
    localStorage.setItem('size', size);

    const listEl = $('#movie-list');
    const statusEl = $('#status');
    const tpl = $('#movie-card');

    try {
        const payload = await apiFetch(`/movies?page=${page}&size=${size}`);
        const items = Array.isArray(payload) ? payload : (payload.data || []);
        clearChildren(listEl);

        if (items.length === 0) {
            renderStatus(statusEl, 'warn', 'No movies found for this page.');
        } else {
            renderStatus(statusEl, '', '');
        }

        for (const m of items) {
            const frag = tpl.content.cloneNode(true);
            const root = frag.querySelector('.card');

            root.querySelector('.title').textContent = m.title ?? '--';
            root.querySelector('.year').textContent = String(m.year ?? '--');
            root.querySelector('.btn-view').href = `/movies/view.html?id=${encodeURIComponent(m.id)}`;
            root.querySelector('.btn-edit').href = `/movies/edit.html?id=${encodeURIComponent(m.id)}`;
            root.querySelector('.btn-delete').dataset.id = m.id;

            listEl.appendChild(frag);
        }

        listEl.addEventListener('click', async (ev) => {
            const btn = ev.target.closest('button.btn-delete[data-id]');
            if (!btn) return;
            const id = btn.dataset.id;
            if (!confirm('Delete this movie? This cannot be undone.')) return;
            try {
                await apiFetch(`/movies/${encodeURIComponent(id)}`, {
                    method: 'DELETE',
                });
                renderStatus(statusEl, 'ok', `Movie ${id} deleted.`);
                setTimeout(() => location.reload(), 2000);
            } catch (err) {
                renderStatus(statusEl, 'err', `Delete failed: ${err.message}`);
            }
        });

        const sizeSelect = document.getElementById('page-size');
        const pageSizes = [3, 6, 9, 12, 15];
        for (const s of pageSizes) {
            const opt = document.createElement("option");
            opt.value = s;
            opt.textContent = String(s);
            opt.selected = (size == s);
            sizeSelect.appendChild(opt);
        }

        sizeSelect.addEventListener('change', () => {
            const params = new URLSearchParams(window.location.search);
            params.set('page', 1);
            params.set('size', sizeSelect.value);
            localStorage.setItem('page', 1);
            localStorage.setItem('size', sizeSelect.value);
            const newUrl = `${window.location.pathname}?${params.toString()}`;
            window.location.href = newUrl;
        });

        // Pagination
        $('#page-num').textContent = `Page ${page}`;

        const firstPage = page <= 1;
        const lastPage = page >= payload.meta.totalPages;

        const firstBtn = $('#first');
        const prevBtn = $('#prev');
        const nextBtn = $('#next');
        const lastBtn = $('#last');

        firstBtn.href = `?page=1&size=${size}`;
        prevBtn.href = `?page=${page - 1}&size=${size}`;
        nextBtn.href = `?page=${page + 1}&size=${size}`;
        lastBtn.href = `?page=${payload.meta.totalPages}&size=${size}`;

        firstBtn.classList.toggle('disabled', firstPage);
        prevBtn.classList.toggle('disabled', firstPage);
        nextBtn.classList.toggle('disabled', lastPage);
        lastBtn.classList.toggle('disabled', lastPage);

        firstBtn.setAttribute("onclick", `return ${!firstPage};`);
        prevBtn.setAttribute("onclick", `return ${!firstPage};`);
        nextBtn.setAttribute("onclick", `return ${!lastPage};`);
        lastBtn.setAttribute("onclick", `return ${!lastPage};`);

    } catch (err) {
        renderStatus(statusEl, 'err', `Failed to fetch movies: ${err.message}`);
    }
})();
