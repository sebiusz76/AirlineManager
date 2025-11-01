// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ============================================
// THEME MANAGEMENT
// ============================================
(function () {
    'use strict';

    const ThemeManager = {
        // Available themes
        themes: {
            auto: { name: 'Auto (System)', icon: 'bi-circle-half', description: 'Follows your system preference' },
            light: { name: 'Light (Soft)', icon: 'bi-sun', variant: 'light', description: 'Warm & comfortable' },
            'light-crisp': { name: 'Light (Crisp)', icon: 'bi-sun-fill', variant: 'light-crisp', description: 'Professional & sharp' },
            'light-warm': { name: 'Light (Warm)', icon: 'bi-brightness-high', variant: 'light-warm', description: 'Cozy & inviting' },
            dark: { name: 'Dark (Soft)', icon: 'bi-moon', variant: 'dark', description: 'Warm & easy on eyes' },
            'dark-slate': { name: 'Dark (Slate)', icon: 'bi-moon-stars', variant: 'dark-slate', description: 'Professional & neutral' },
            'dark-midnight': { name: 'Dark (Midnight)', icon: 'bi-moon-fill', variant: 'dark-midnight', description: 'Modern tech style' }
        },

        init: function () {
            this.attachEventListeners();
            this.updateThemeUI();
        },

        attachEventListeners: function () {
            // Handle theme selection from dropdown buttons (if any)
            const themeOptions = document.querySelectorAll('.theme-option');
            themeOptions.forEach(option => {
                option.addEventListener('click', (e) => {
                    e.preventDefault();
                    const theme = option.getAttribute('data-theme');
                    this.setTheme(theme, true); // Show notification

                    // Close the dropdown after selection
                    const dropdownElement = option.closest('.dropdown-menu');
                    if (dropdownElement) {
                        const bsDropdown = bootstrap.Dropdown.getInstance(
                            document.querySelector('[data-bs-toggle="dropdown"]')
                        );
                        if (bsDropdown) {
                            bsDropdown.hide();
                        }
                    }
                });
            });

            // Handle theme selection from select dropdown
            const themeSelect = document.getElementById('themeSelect');
            if (themeSelect) {
                themeSelect.addEventListener('change', (e) => {
                    const theme = e.target.value;

                    // Add animation class
                    themeSelect.classList.add('theme-changing');
                    setTimeout(() => {
                        themeSelect.classList.remove('theme-changing');
                    }, 300);

                    this.setTheme(theme, true); // Show notification
                });
            }
        },

        setTheme: function (theme, showNotification = true) {
            const htmlElement = document.documentElement;

            // Add transitioning class to prevent flash
            htmlElement.setAttribute('data-bs-theme-transitioning', '');

            // Apply theme to HTML element
            if (theme === 'auto') {
                htmlElement.setAttribute('data-bs-theme', 'auto');
                // Apply system preference immediately
                const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
                if (systemPrefersDark) {
                    htmlElement.setAttribute('data-bs-theme', 'dark');
                } else {
                    htmlElement.setAttribute('data-bs-theme', 'light');
                }
            } else if (theme.startsWith('dark-') || theme.startsWith('light-')) {
                // Apply specific variant
                htmlElement.setAttribute('data-bs-theme', theme);
            } else {
                htmlElement.setAttribute('data-bs-theme', theme);
            }

            // Remove transitioning class after a brief delay
            setTimeout(() => {
                htmlElement.removeAttribute('data-bs-theme-transitioning');
            }, 50);

            // Update UI
            this.updateThemeUI();

            // Save to server if user is authenticated
            this.saveThemePreference(theme);

            // Show subtle notification only if requested and user initiated the change
            if (showNotification) {
                this.showThemeChangeNotification(theme);
            }
        },

        updateThemeUI: function () {
            const htmlElement = document.documentElement;
            let currentTheme = htmlElement.getAttribute('data-bs-theme') || 'auto';

            // Normalize theme variants for UI display
            const displayTheme = currentTheme;

            // Update dropdown button icon and label (if exists - for separate theme selector)
            const themeIcon = document.getElementById('themeIcon');
            const themeLabel = document.getElementById('themeLabel');

            if (themeIcon && this.themes[displayTheme]) {
                // Remove all theme icon classes
                const iconClasses = ['bi-sun', 'bi-moon', 'bi-moon-stars', 'bi-moon-fill', 'bi-circle-half'];
                iconClasses.forEach(cls => themeIcon.classList.remove(cls));

                // Add appropriate icon
                themeIcon.classList.add(this.themes[displayTheme].icon);

                if (themeLabel) {
                    themeLabel.textContent = this.themes[displayTheme].name;
                }
            }

            // Update current theme badge in user menu
            const themeBadge = document.getElementById('currentThemeBadge');
            if (themeBadge && this.themes[displayTheme]) {
                themeBadge.textContent = this.themes[displayTheme].name;

                // Change badge color based on theme
                themeBadge.className = 'badge';
                switch (displayTheme) {
                    case 'light':
                        themeBadge.classList.add('bg-warning', 'text-dark');
                        break;
                    case 'dark':
                    case 'dark-slate':
                    case 'dark-midnight':
                        themeBadge.classList.add('bg-dark', 'text-light');
                        break;
                    default:
                        themeBadge.classList.add('bg-primary');
                }
                themeBadge.style.fontSize = '0.65rem';
            }

            // Update theme select dropdown value
            const themeSelect = document.getElementById('themeSelect');
            if (themeSelect && themeSelect.value !== displayTheme) {
                // Check if option exists
                const optionExists = Array.from(themeSelect.options).some(opt => opt.value === displayTheme);
                if (optionExists) {
                    themeSelect.value = displayTheme;
                }
            }

            // Update theme description text
            const themeDescText = document.getElementById('themeDescriptionText');
            if (themeDescText && this.themes[displayTheme]) {
                themeDescText.textContent = this.themes[displayTheme].description;
            }

            // Update active state in dropdown buttons (if any)
            const themeOptions = document.querySelectorAll('.theme-option');
            themeOptions.forEach(option => {
                const optionTheme = option.getAttribute('data-theme');
                if (optionTheme === displayTheme) {
                    option.classList.add('active');
                } else {
                    option.classList.remove('active');
                }
            });

            // Update configuration select if present
            const configSelect = document.getElementById('Theme_Default');
            if (configSelect && configSelect.value !== displayTheme) {
                // Only update if it's different to avoid infinite loops
                const optionExists = Array.from(configSelect.options).some(opt => opt.value === displayTheme);
                if (optionExists) {
                    configSelect.value = displayTheme;
                }
            }
        },

        saveThemePreference: function (theme) {
            // Check if user is authenticated by looking for logout form
            const isAuthenticated = document.getElementById('logoutForm') !== null;

            if (!isAuthenticated) {
                console.log('User not authenticated, theme preference not saved to server');
                // Save to localStorage for guests
                localStorage.setItem('guestTheme', theme);
                return;
            }

            // Get anti-forgery token
            const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            const token = tokenElement ? tokenElement.value : '';

            fetch('/Theme/SetThemeAjax', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ theme: theme })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        console.log('Theme preference saved:', data.theme);
                    } else {
                        console.error('Failed to save theme preference:', data.message);
                    }
                })
                .catch(error => {
                    console.error('Error saving theme preference:', error);
                });
        },

        // Load guest theme preference
        loadGuestTheme: function () {
            const isAuthenticated = document.getElementById('logoutForm') !== null;
            if (!isAuthenticated) {
                const savedTheme = localStorage.getItem('guestTheme');
                if (savedTheme && this.themes[savedTheme]) {
                    this.setTheme(savedTheme, false); // Don't show notification on load
                }
            }
        },

        // Listen for system theme changes when in auto mode
        watchSystemTheme: function () {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            mediaQuery.addEventListener('change', (e) => {
                const htmlElement = document.documentElement;
                const currentTheme = htmlElement.getAttribute('data-bs-theme');

                if (currentTheme === 'auto' || !currentTheme) {
                    if (e.matches) {
                        htmlElement.setAttribute('data-bs-theme', 'dark');
                    } else {
                        htmlElement.setAttribute('data-bs-theme', 'light');
                    }
                    this.updateThemeUI();
                }
            });
        },

        // Show theme change notification
        showThemeChangeNotification: function (theme) {
            // Check if user is authenticated
            const isAuthenticated = document.getElementById('logoutForm') !== null;
            
            // Don't show notification for guest users
            if (!isAuthenticated) {
                console.log(`Theme changed to: ${theme} (guest user - no notification)`);
                return;
            }

            const themeName = this.themes[theme] ? this.themes[theme].name : theme;

            // Check if SweetAlert2 is available
            if (typeof Swal !== 'undefined') {
                const toast = Swal.mixin({
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 1500,
                    timerProgressBar: true,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer);
                        toast.addEventListener('mouseleave', Swal.resumeTimer);
                    }
                });

                let icon = 'success';
                let iconHtml = '';

                switch (theme) {
                    case 'light':
                    case 'light-crisp':
                    case 'light-warm':
                        iconHtml = '<i class="bi bi-sun-fill" style="font-size: 2rem; color: #fbbf24;"></i>';
                        break;
                    case 'dark':
                    case 'dark-slate':
                    case 'dark-midnight':
                        iconHtml = '<i class="bi bi-moon-fill" style="font-size: 2rem; color: #60a5fa;"></i>';
                        break;
                    default:
                        iconHtml = '<i class="bi bi-circle-half" style="font-size: 2rem; color: #a78bfa;"></i>';
                }

                toast.fire({
                    icon: icon,
                    iconHtml: iconHtml,
                    title: `Theme changed to ${themeName}`,
                    html: '<small>Your preference has been saved</small>'
                });
            } else {
                console.log(`Theme changed to: ${themeName}`);
            }
        }
    };

    // Initialize Theme Manager when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            ThemeManager.init();
            ThemeManager.loadGuestTheme();
            ThemeManager.watchSystemTheme();
        });
    } else {
        ThemeManager.init();
        ThemeManager.loadGuestTheme();
        ThemeManager.watchSystemTheme();
    }

    // Expose ThemeManager globally for debugging and configuration page
    window.ThemeManager = ThemeManager;
})();

// ============================================
// COOKIE CONSENT MANAGEMENT
// ============================================
(function () {
    'use strict';

    const COOKIE_CONSENT_KEY = 'cookieConsent';
    const COOKIE_CONSENT_DATE_KEY = 'cookieConsentDate';
    const CONSENT_EXPIRY_DAYS = 365;

    // Cookie consent object
    const CookieConsent = {
        init: function () {
            // Check if consent has already been given
            const consent = this.getConsent();
     
          if (!consent) {
                // Show the banner if no consent recorded
       this.showBanner();
  } else {
        // Check if consent has expired
   const consentDate = this.getConsentDate();
        if (consentDate) {
const expiryDate = new Date(consentDate);
        expiryDate.setDate(expiryDate.getDate() + CONSENT_EXPIRY_DAYS);
        
          if (new Date() > expiryDate) {
   // Consent expired, show banner again
         this.revokeConsent();
 this.showBanner();
             }
                }
            }

   // Attach event listeners
            this.attachEventListeners();
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
        
       // Add fade out animation
       banner.style.animation = 'slideDown 0.4s ease-out';
              setTimeout(() => {
                banner.style.display = 'none';
    }, 400);
            }
      },

    attachEventListeners: function () {
            const acceptButton = document.getElementById('acceptCookies');
            const declineButton = document.getElementById('declineCookies');

            if (acceptButton) {
      acceptButton.addEventListener('click', () => {
            this.acceptCookies();
    });
      }

  if (declineButton) {
    declineButton.addEventListener('click', () => {
    this.declineCookies();
    });
   }
        },

        acceptCookies: function () {
            this.setConsent('accepted');
        this.hideBanner();
      
            // Show subtle notification
      if (typeof Swal !== 'undefined') {
           const toast = Swal.mixin({
   toast: true,
      position: 'top-end',
   showConfirmButton: false,
  timer: 2000,
   timerProgressBar: true
              });

   toast.fire({
   icon: 'success',
        title: 'Cookie Preferences Saved',
         text: 'All cookies accepted'
       });
     }
         
     console.log('Cookies accepted - all tracking and analytics enabled');
        },

        declineCookies: function () {
  this.setConsent('necessary');
            this.hideBanner();
            
         // Show subtle notification
            if (typeof Swal !== 'undefined') {
        const toast = Swal.mixin({
     toast: true,
      position: 'top-end',
             showConfirmButton: false,
          timer: 2000,
               timerProgressBar: true
    });

     toast.fire({
   icon: 'info',
            title: 'Cookie Preferences Saved',
     text: 'Only necessary cookies enabled'
    });
            }
     
            console.log('Only necessary cookies accepted - tracking and analytics disabled');
        },

        setConsent: function (value) {
            localStorage.setItem(COOKIE_CONSENT_KEY, value);
            localStorage.setItem(COOKIE_CONSENT_DATE_KEY, new Date().toISOString());
      },

    getConsent: function () {
            return localStorage.getItem(COOKIE_CONSENT_KEY);
   },

        getConsentDate: function () {
       return localStorage.getItem(COOKIE_CONSENT_DATE_KEY);
        },

        revokeConsent: function () {
       localStorage.removeItem(COOKIE_CONSENT_KEY);
      localStorage.removeItem(COOKIE_CONSENT_DATE_KEY);
     
            // Show banner again
    this.showBanner();
      
          // Show notification
      if (typeof Swal !== 'undefined') {
    const toast = Swal.mixin({
        toast: true,
     position: 'top-end',
  showConfirmButton: false,
timer: 3000,
    timerProgressBar: true
                });

   toast.fire({
   icon: 'info',
 title: 'Cookie Consent Revoked',
           text: 'Please choose your cookie preferences again'
           });
      }
     
      console.log('Cookie consent revoked - please select preferences again');
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
        },
        init: function () {
       CookieConsent.init();
        }
    };
})();