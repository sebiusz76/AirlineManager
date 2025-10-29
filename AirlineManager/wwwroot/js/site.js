// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Cookie Consent Management
(function () {
    'use strict';

    const COOKIE_CONSENT_KEY = 'cookieConsent';
    const COOKIE_CONSENT_DATE_KEY = 'cookieConsentDate';
    const CONSENT_EXPIRY_DAYS = 365;

    // Cookie consent object
    const CookieConsent = {
        init: function () {
            this.checkConsent();
            this.attachEventListeners();
        },

        checkConsent: function () {
            const consent = this.getConsent();
            const consentDate = localStorage.getItem(COOKIE_CONSENT_DATE_KEY);

            // Check if consent exists and is not expired
            if (consent === null || this.isConsentExpired(consentDate)) {
                this.showBanner();
            } else {
                this.hideBanner();
                // Apply consent settings
                this.applyConsent(consent);
            }
        },

        isConsentExpired: function (consentDate) {
            if (!consentDate) return true;

            const consentTime = new Date(consentDate).getTime();
            const currentTime = new Date().getTime();
            const daysDiff = (currentTime - consentTime) / (1000 * 60 * 60 * 24);

            return daysDiff > CONSENT_EXPIRY_DAYS;
        },

        showBanner: function () {
            const banner = document.getElementById('cookieConsent');
            if (banner) {
                banner.classList.add('show');
            }
        },

        hideBanner: function () {
            const banner = document.getElementById('cookieConsent');
            if (banner) {
                banner.classList.remove('show');
            }
        },

        acceptAll: function () {
            const consent = {
                necessary: true,
                analytics: true,
                marketing: true
            };
            this.saveConsent(consent);
            this.applyConsent(consent);
            this.hideBanner();
            this.showToast('Cookie preferences saved', 'success');
        },

        declineAll: function () {
            const consent = {
                necessary: true, // Necessary cookies cannot be declined
                analytics: false,
                marketing: false
            };
            this.saveConsent(consent);
            this.applyConsent(consent);
            this.hideBanner();
            this.showToast('Only necessary cookies will be used', 'info');
        },

        saveConsent: function (consent) {
            localStorage.setItem(COOKIE_CONSENT_KEY, JSON.stringify(consent));
            localStorage.setItem(COOKIE_CONSENT_DATE_KEY, new Date().toISOString());
        },

        getConsent: function () {
            const consent = localStorage.getItem(COOKIE_CONSENT_KEY);
            return consent ? JSON.parse(consent) : null;
        },

        applyConsent: function (consent) {
            // Here you can add logic to enable/disable tracking scripts
            // based on user consent

            if (consent.analytics) {
                // Enable Google Analytics or other analytics tools
                console.log('Analytics cookies enabled');
                // Example: this.enableGoogleAnalytics();
            } else {
                console.log('Analytics cookies disabled');
            }

            if (consent.marketing) {
                // Enable marketing/advertising cookies
                console.log('Marketing cookies enabled');
            } else {
                console.log('Marketing cookies disabled');
            }
        },

        attachEventListeners: function () {
            const acceptBtn = document.getElementById('acceptCookies');
            const declineBtn = document.getElementById('declineCookies');

            if (acceptBtn) {
                acceptBtn.addEventListener('click', () => this.acceptAll());
            }

            if (declineBtn) {
                declineBtn.addEventListener('click', () => this.declineAll());
            }
        },

        showToast: function (message, type) {
            // If SweetAlert2 is available, use it
            if (typeof Swal !== 'undefined') {
                const toast = Swal.mixin({
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: true
                });

                toast.fire({
                    icon: type || 'info',
                    title: message
                });
            } else {
                // Fallback to console
                console.log(`${type}: ${message}`);
            }
        },

        // Method to revoke consent (can be called from Privacy page)
        revokeConsent: function () {
            localStorage.removeItem(COOKIE_CONSENT_KEY);
            localStorage.removeItem(COOKIE_CONSENT_DATE_KEY);
            this.showBanner();
            this.showToast('Cookie consent revoked. Please set your preferences again.', 'info');
        }
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            CookieConsent.init();
        });
    } else {
        CookieConsent.init();
    }

    // Expose revokeConsent method globally for use in Privacy page
    window.CookieConsent = {
        revoke: function () {
            CookieConsent.revokeConsent();
        },
        getConsent: function () {
            return CookieConsent.getConsent();
        }
    };
})();