// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('table:not(.calendar-table):not(.permission-matrix):not([data-no-pagination])').forEach(table => {
        const body = table.tBodies[0];
        if (!body || body.rows.length <= 10 || table.closest('.calendar-grid, .calendar-week')) return;

        const rows = Array.from(body.rows);
        const wrapper = table.closest('.table-responsive') ?? table.parentElement;
        const controls = document.createElement('div');
        controls.className = 'table-pagination';
        controls.setAttribute('aria-label', 'Table pagination');
        controls.innerHTML = `
            <div class="pagination-summary" aria-live="polite"></div>
            <div class="pagination-actions">
                <label class="pagination-size-label">Rows
                    <select class="form-select form-select-sm pagination-size" aria-label="Rows per page">
                        <option value="10">10</option><option value="25">25</option><option value="50">50</option>
                    </select>
                </label>
                <button type="button" class="btn btn-outline-secondary btn-sm pagination-previous">Previous</button>
                <span class="pagination-current" aria-live="polite"></span>
                <button type="button" class="btn btn-outline-secondary btn-sm pagination-next">Next</button>
            </div>`;
        wrapper.insertAdjacentElement('afterend', controls);

        let page = 1;
        let pageSize = 10;
        const summary = controls.querySelector('.pagination-summary');
        const size = controls.querySelector('.pagination-size');
        const previous = controls.querySelector('.pagination-previous');
        const next = controls.querySelector('.pagination-next');
        const current = controls.querySelector('.pagination-current');
        const render = () => {
            const pages = Math.max(1, Math.ceil(rows.length / pageSize));
            page = Math.min(page, pages);
            rows.forEach((row, index) => { row.hidden = index < (page - 1) * pageSize || index >= page * pageSize; });
            const start = (page - 1) * pageSize + 1;
            const end = Math.min(page * pageSize, rows.length);
            summary.textContent = `Showing ${start}–${end} of ${rows.length}`;
            current.textContent = `Page ${page} of ${pages}`;
            previous.disabled = page === 1;
            next.disabled = page === pages;
        };
        size.addEventListener('change', () => { pageSize = Number(size.value); page = 1; render(); });
        previous.addEventListener('click', () => { page--; render(); });
        next.addEventListener('click', () => { page++; render(); });
        render();
    });

    const toggle = document.querySelector('.workspace-menu-toggle');
    const navigation = document.getElementById('workspace-nav');
    if (!toggle || !navigation) return;
    const close = () => {
        navigation.classList.remove('is-open');
        toggle.setAttribute('aria-expanded', 'false');
    };
    toggle.addEventListener('click', () => {
        const open = navigation.classList.toggle('is-open');
        toggle.setAttribute('aria-expanded', String(open));
    });
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && navigation.classList.contains('is-open')) {
            close();
            toggle.focus();
        }
    });
    document.addEventListener('click', event => {
        if (!navigation.contains(event.target) && !toggle.contains(event.target)) close();
    });
    navigation.querySelectorAll('a.active').forEach(link => link.setAttribute('aria-current', 'page'));
});
