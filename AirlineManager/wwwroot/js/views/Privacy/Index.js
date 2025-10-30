/* ============================================
   PRIVACY/INDEX VIEW SCRIPTS
   Cookie consent management
 ============================================ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var revokeCookieConsentBtn = document.getElementById('revokeCookieConsent');

        if (revokeCookieConsentBtn) {
            revokeCookieConsentBtn.addEventListener('click', function () {
                if (typeof window.CookieConsent !== 'undefined') {
                    window.CookieConsent.revoke();
                }
            });
        }
    });
})();