(function(){
    // Simple AJAX navigation for admin sidebar links
    const mainSelector = '#main-content';
    const sidebarSelector = '.sidebar';

    function setActiveLink(path) {
        document.querySelectorAll(sidebarSelector + ' .sidebar-item').forEach(a=>{
            try{
                a.classList.remove('active');
                const aPath = a.getAttribute('data-path') || a.getAttribute('href');
                if(!aPath) return;
                // compare ignoring trailing slash
                const normalize = p => p.replace(/\/$/, '');
                if(normalize(aPath) === normalize(path)) {
                    a.classList.add('active');
                }
            }catch(e){/*ignore*/}
        });
    }

    async function fetchAndReplace(url, addToHistory = true) {
        try {
            const res = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) {
                window.location.href = url; // fallback
                return;
            }
            const text = await res.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(text, 'text/html');
            const newContent = doc.querySelector(mainSelector) || doc.querySelector('.content-area') || doc.body;
            const target = document.querySelector(mainSelector) || document.querySelector('.content-area');
            if (newContent && target) {
                target.innerHTML = newContent.innerHTML;
                // update title
                const newTitle = doc.querySelector('title');
                if (newTitle) document.title = newTitle.innerText;
                // update active link
                setActiveLink(new URL(url, location.origin).pathname);
                // re-run any scripts in returned content (simple approach)
                const scripts = Array.from(newContent.querySelectorAll('script'));
                scripts.forEach(s => {
                    const script = document.createElement('script');
                    if (s.src) script.src = s.src;
                    script.text = s.textContent;
                    document.body.appendChild(script);
                });
                if (addToHistory) history.pushState({ url }, '', url);
            } else {
                window.location.href = url; // fallback
            }
        } catch (err) {
            console.error('Navigation error', err);
            window.location.href = url;
        }
    }

    // Intercept clicks on sidebar links
    document.addEventListener('click', function(e){
        const link = e.target.closest(sidebarSelector + ' a.sidebar-item');
        if (!link) return;
        const href = link.getAttribute('href');
        // Only handle same-origin GET links
        if (!href || href.startsWith('http') && !href.startsWith(location.origin)) return;
        e.preventDefault();
        fetchAndReplace(href, true);
    });

    // Handle navigation from action buttons inside content area that link to admin pages
    document.addEventListener('click', function(e){
        const link = e.target.closest('a');
        if (!link) return;
        const href = link.getAttribute('href');
        if (!href) return;
        // if link is inside main content and targets admin path, intercept
        if (link.closest('#main-content') || link.closest('.content-area')) {
            if (href.startsWith('/Admin')) {
                // allow opening external targets or anchors
                if (link.target && link.target !== '_self') return;
                e.preventDefault();
                fetchAndReplace(href, true);
            }
        }
    });

    // Handle back/forward
    window.addEventListener('popstate', function(e){
        const url = location.pathname + location.search;
        fetchAndReplace(url, false);
    });

    // Initial active link
    document.addEventListener('DOMContentLoaded', function(){
        setActiveLink(location.pathname);
    });
})();
