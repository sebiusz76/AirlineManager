/* ============================================
 THEME/VARIANTS VIEW SCRIPTS
   Theme switching functionality
 ============================================ */

(function () {
    'use strict';

    /**
     * Apply theme and show confirmation toast
     * @param {string} theme - Theme identifier
     */
    window.applyTheme = function (theme) {
        if (window.ThemeManager) {
            window.ThemeManager.setTheme(theme);

            // Show success toast
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'success',
                    title: 'Theme applied!',
                    text: `${theme} theme has been activated.`,
                    showConfirmButton: false,
                    timer: 2000,
                    timerProgressBar: true
                });
            }
        }
    };
})();