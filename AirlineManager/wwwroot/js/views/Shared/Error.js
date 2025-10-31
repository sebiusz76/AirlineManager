/* ============================================
   SHARED/ERROR VIEW SCRIPTS
   Error page functionality
 ============================================ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        // Display current time
        const currentTimeElement = document.getElementById('currentTime');
        if (currentTimeElement) {
            currentTimeElement.textContent = new Date().toLocaleString();
        }

        // Add animation on load
        const errorCard = document.querySelector('.error-card');
        if (errorCard) {
            errorCard.classList.add('animate-in');
        }
    });
})();

// Search functionality
function searchSite() {
    const searchTerm = prompt('What are you looking for?');
    if (searchTerm) {
        // In a real application, you would have a search endpoint
        window.location.href = '/?search=' + encodeURIComponent(searchTerm);
    }
}

// Make searchSite available globally for onclick handler
window.searchSite = searchSite;