(() => {
    const days = Array.from(document.querySelectorAll('.shift-day'));
    const field = (day, name) => day.querySelector(`[data-timing="${name}"]`);
    const clock = minutes => `${String(Math.floor(minutes / 60) % 24).padStart(2, '0')}:${String(minutes % 60).padStart(2, '0')}`;
    function refresh(day) {
        const selected = day.querySelector('[data-day-type]:checked');
        const working = day.querySelector('[data-day-type="IsWorking"]').checked;
        day.querySelector('.shift-working').hidden = !working;
        day.querySelector('.shift-other').hidden = !day.querySelector('[data-day-type="IsOther"]').checked;
        let text = selected ? selected.parentElement.textContent.trim() : 'Not configured';
        const start = Number(field(day, 'StartTimeHours').value) * 60 + Number(field(day, 'StartTimeMinutes').value);
        const duration = Number(field(day, 'DurationHours').value) * 60 + Number(field(day, 'DurationMinutes').value);
        const configured = field(day, 'StartTimeHours').value !== '' || field(day, 'StartTimeMinutes').value !== '';
        const preview = working && configured && duration > 0
            ? `${clock(start)} – ${clock(start + duration)}${start + duration >= 1440 ? ' (next day)' : ''}` : '';
        day.querySelector('.shift-day-summary').textContent = text + (preview ? ` · ${preview}` : '');
        day.querySelector('.shift-hours-preview').textContent = preview ? `Start + shift duration: ${preview}. Breaks are configured separately below.` : 'Enter a start time and duration to preview the hours.';
    }
    days.forEach((day, index) => {
        day.querySelectorAll('[data-day-type]').forEach(input => input.addEventListener('change', () => {
            if (input.checked) day.querySelectorAll('[data-day-type]').forEach(other => { if (other !== input) other.checked = false; });
            refresh(day);
        }));
        day.querySelectorAll('[data-timing]').forEach(input => input.addEventListener('input', () => refresh(day)));
        day.querySelectorAll('.shift-options').forEach(options => {
            options.open = Array.from(options.querySelectorAll('[data-timing]')).some(input => Number(input.value) > 0) || !!options.querySelector('[data-flexible]:checked');
        });
        day.querySelector('.shift-copy')?.addEventListener('click', () => {
            const previous = days[index - 1];
            day.querySelectorAll('[data-timing]').forEach(input => {
                if (!input.dataset.timing.startsWith('Other')) input.value = field(previous, input.dataset.timing).value;
            });
            day.querySelector('[data-flexible]').checked = previous.querySelector('[data-flexible]').checked;
            day.querySelectorAll('.shift-options').forEach(options => { options.open = true; });
            day.querySelector('.shift-feedback').textContent = `Timings copied from ${previous.querySelector('.shift-day-name').textContent}. Review them before saving.`;
            refresh(day);
        });
        refresh(day);
    });
    const form = days[0]?.closest('form');
    form?.addEventListener('invalid', event => {
        let details = event.target.closest('details');
        while (details) { details.open = true; details = details.parentElement.closest('details'); }
    }, true);
    form?.addEventListener('submit', () => {
        // A partially entered hour/minute pair represents a duration with a zero component.
        form.querySelectorAll('.shift-time-pair').forEach(pair => {
            const inputs = Array.from(pair.querySelectorAll('input'));
            if (inputs.some(input => input.value !== '')) inputs.forEach(input => { if (input.value === '') input.value = '0'; });
        });
    });
})();
