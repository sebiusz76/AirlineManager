(function () {
    function loadScript(url, cb) {
        var s = document.createElement('script');
        s.src = url;
        s.async = true;
        s.onload = cb;
        s.onerror = function () { console.error('Failed to load', url); };
        document.head.appendChild(s);
    }

    var localPath = '~/lib/sweetalert2/sweetalert2.all.min.js';
    // try to load local file, if not available, fall back to CDN
    fetch(localPath, { method: 'HEAD' }).then(function (res) {
        if (res.ok) {
            loadScript(localPath);
        } else {
            loadScript('https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js');
        }
    }).catch(function () {
        loadScript('https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js');
    });
})();