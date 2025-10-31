/* ============================================
   ADMIN/CONFIGURATION/EDIT VIEW SCRIPTS
   Theme configuration management and preview
 ============================================ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        const themeSelect = document.getElementById('Theme_Default');
        
        if (themeSelect) {
            // Update theme preview when selection changes
    themeSelect.addEventListener('change', function () {
         const selectedTheme = this.value;
      
  if (window.ThemeManager) {
   // Temporarily apply theme for preview
   window.ThemeManager.setTheme(selectedTheme, false); // No notification for preview
    }
     });

      // Add visual indicators to options (if not already present)
            const options = themeSelect.querySelectorAll('option');
  options.forEach(option => {
     const theme = option.value;
        let icon = '';
      
       switch (theme) {
     case 'auto':
  icon = '?? ';
          break;
   case 'light':
    case 'light-crisp':
   case 'light-warm':
   icon = theme === 'light' ? '?? ' : 
      theme === 'light-crisp' ? '?? ' : '?? ';
       break;
          case 'dark':
                    case 'dark-slate':
    case 'dark-midnight':
                 icon = theme === 'dark' ? '?? ' : 
        theme === 'dark-slate' ? '?? ' : '?? ';
    break;
           }
     
      // Only add icon if not already present
      if (icon && !option.textContent.startsWith(icon)) {
        option.textContent = icon + option.textContent;
      }
      });
        }
    });
})();
