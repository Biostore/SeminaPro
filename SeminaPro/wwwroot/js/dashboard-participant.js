/* ========================================
   DASHBOARD PARTICIPANT - JavaScript
   ======================================== */

document.addEventListener('DOMContentLoaded', function () {
    // Active sidebar item based on current page
    const currentPath = window.location.pathname;
    document.querySelectorAll('.sidebar-item[data-path]').forEach(item => {
        const itemPath = item.getAttribute('data-path');
        if (currentPath.startsWith(itemPath)) {
            document.querySelectorAll('.sidebar-item').forEach(i => i.classList.remove('active'));
            item.classList.add('active');
        }
    });

    // Smooth scroll for anchor links
    document.querySelectorAll('.sidebar-item[onclick]').forEach(link => {
        link.addEventListener('click', function (e) {
            if (this.getAttribute('href') && this.getAttribute('href').startsWith('#')) {
                e.preventDefault();
                const targetId = this.getAttribute('href').substring(1);
                const targetElement = document.getElementById(targetId);
                if (targetElement) {
                    targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }
        });
    });

    // Stat cards click navigation
    document.querySelectorAll('.stat-card[data-href]').forEach(card => {
        card.addEventListener('click', function () {
            const href = this.getAttribute('data-href');
            if (href) {
                window.location.href = href;
            }
        });
    });

    // Table row hover effect
    document.querySelectorAll('table tbody tr').forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.transform = 'translateX(4px)';
        });
        row.addEventListener('mouseleave', function () {
            this.style.transform = 'translateX(0)';
        });
    });

    // Initialize tooltips if using Bootstrap
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// Scroll to section function
function scrollToSection(sectionId) {
    const element = document.getElementById(sectionId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}
