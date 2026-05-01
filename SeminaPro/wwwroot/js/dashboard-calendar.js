// ========================================
// CALENDAR FUNCTIONALITY
// ========================================

document.addEventListener('DOMContentLoaded', function() {
    const calendarGrid = document.getElementById('calendarGrid');
    const monthYear = document.getElementById('monthYear');
    const prevMonth = document.getElementById('prevMonth');
    const nextMonth = document.getElementById('nextMonth');

    if (!calendarGrid || !monthYear) return;

    let currentDate = new Date();

    function renderCalendar() {
        const year = currentDate.getFullYear();
        const month = currentDate.getMonth();

        // Set month/year display
        const monthNames = [
            'Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin',
            'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'
        ];
        monthYear.textContent = `${monthNames[month]} ${year}`;

        // Clear calendar
        calendarGrid.innerHTML = '';

        // Add day headers
        const dayHeaders = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];
        dayHeaders.forEach(day => {
            const header = document.createElement('div');
            header.className = 'calendar-header-day';
            header.textContent = day;
            header.style.fontWeight = '700';
            header.style.textAlign = 'center';
            header.style.padding = '0.5rem 0';
            header.style.color = 'var(--primary)';
            calendarGrid.appendChild(header);
        });

        // Get first day of month
        const firstDay = new Date(year, month, 1).getDay();
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        // Add empty cells for days before month starts
        for (let i = 0; i < (firstDay === 0 ? 6 : firstDay - 1); i++) {
            const emptyDay = document.createElement('div');
            emptyDay.className = 'calendar-day';
            emptyDay.style.backgroundColor = 'transparent';
            emptyDay.style.cursor = 'default';
            calendarGrid.appendChild(emptyDay);
        }

        // Add days of month
        const today = new Date();
        for (let day = 1; day <= daysInMonth; day++) {
            const dayElement = document.createElement('div');
            dayElement.className = 'calendar-day';
            dayElement.textContent = day;

            const cellDate = new Date(year, month, day);
            if (cellDate.toDateString() === today.toDateString()) {
                dayElement.classList.add('today');
            }

            // Add click event
            dayElement.addEventListener('click', function() {
                document.querySelectorAll('.calendar-day.active').forEach(d => d.classList.remove('active'));
                dayElement.classList.add('active');
            });

            calendarGrid.appendChild(dayElement);
        }
    }

    // Event listeners for month navigation
    if (prevMonth) {
        prevMonth.addEventListener('click', function() {
            currentDate.setMonth(currentDate.getMonth() - 1);
            renderCalendar();
        });
    }

    if (nextMonth) {
        nextMonth.addEventListener('click', function() {
            currentDate.setMonth(currentDate.getMonth() + 1);
            renderCalendar();
        });
    }

    // Initial render
    renderCalendar();
});
