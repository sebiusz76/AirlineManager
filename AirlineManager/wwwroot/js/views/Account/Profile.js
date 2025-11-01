/* ============================================
   ACCOUNT/PROFILE VIEW SCRIPTS
   Profile management, 2FA, and security features
 ============================================ */

(function () {
    'use strict';

    var recoveryCodesModal;
    var currentRecoveryCodes = [];

  /**
     * Get anti-forgery token for AJAX requests
     * @returns {string} CSRF token value
   */
    function getRequestVerificationToken() {
        var el = document.querySelector('#antiforgeryForm input[name="__RequestVerificationToken"]');
     return el ? el.value : '';
    }

    /**
     * Render QR code for 2FA setup
     * @param {string} uri - Authenticator URI
  */
    function renderQr(uri) {
        var container = document.getElementById('tfQr');
        if (!container) {
            console.error('QR container not found');
  return;
  }
        
        container.innerHTML = '';
        
        // Try to use QRCode library if available
        if (typeof QRCode !== 'undefined') {
     try {
           new QRCode(container, { 
      text: uri, 
width: 200, 
         height: 200,
 correctLevel: QRCode.CorrectLevel.M
  });
             return;
      } catch (err) {
   console.error('QRCode library error:', err);
  }
    }
        
     // Fallback: Use Google Charts API to generate QR code
    var qrSize = 200;
        var encodedUri = encodeURIComponent(uri);
        var qrImageUrl = 'https://chart.googleapis.com/chart?cht=qr&chs=' + qrSize + 'x' + qrSize + '&chl=' + encodedUri + '&choe=UTF-8';
        
        var img = document.createElement('img');
        img.src = qrImageUrl;
      img.alt = 'QR Code for 2FA setup';
     img.className = 'img-fluid';
        img.style.maxWidth = qrSize + 'px';
        img.style.display = 'block';
        img.style.margin = '0 auto';
    
        // Add error handler
        img.onerror = function() {
      container.innerHTML = '<div class="alert alert-warning"><i class="bi bi-exclamation-triangle me-2"></i>Unable to generate QR code. Please use the manual key below.</div>';
        };
        
      container.appendChild(img);
    }

    /**
     * Show toast notification
     * @param {string} message - Message to display
  * @param {string} type - Toast type (success, danger, info)
*/
    function showToast(message, type) {
        var icon = type === 'success' ? 'success' : type === 'danger' ? 'error' : 'info';
    Swal.fire({
   toast: true,
    position: 'top-end',
      icon: icon,
       title: message,
     showConfirmButton: false,
        timer: 3000,
            timerProgressBar: true
        });
    }

    /**
   * Show modal with recovery codes
     * @param {Array<string>} codes - Recovery codes array
     */
    function showRecoveryCodesModal(codes) {
 currentRecoveryCodes = codes || [];
 var pre = document.getElementById('modalRecoveryCodes');
      pre.innerText = currentRecoveryCodes.join('\n');

        if (!recoveryCodesModal) {
  recoveryCodesModal = new bootstrap.Modal(document.getElementById('recoveryCodesModal'));
            
     // Add event listener to reload page when modal is closed
       var modalElement = document.getElementById('recoveryCodesModal');
 modalElement.addEventListener('hidden.bs.modal', function () {
     // Reload page to show updated 2FA status
                location.reload();
  }, { once: true }); // Use once:true so the listener is removed after first execution
        }
        recoveryCodesModal.show();
    }

    /**
     * Copy recovery codes to clipboard
     */
    function copyRecoveryCodesToClipboard() {
        var text = currentRecoveryCodes.join('\n');
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(function () {
                showToast('Recovery codes copied to clipboard.', 'success');
            }).catch(function (err) {
       showToast('Failed to copy: ' + err.message, 'danger');
            });
        } else {
  // Fallback for older browsers
            var textarea = document.createElement('textarea');
            textarea.value = text;
     textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
       document.body.appendChild(textarea);
            textarea.select();
            try {
            document.execCommand('copy');
           showToast('Recovery codes copied to clipboard.', 'success');
          } catch (err) {
         showToast('Failed to copy.', 'danger');
    }
          document.body.removeChild(textarea);
        }
    }

    /**
   * Download recovery codes as text file
     */
    function downloadRecoveryCodes() {
   var text = currentRecoveryCodes.join('\n');
        var blob = new Blob([text], { type: 'text/plain' });
    var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = 'recovery-codes-' + new Date().toISOString().split('T')[0] + '.txt';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        showToast('Recovery codes downloaded.', 'success');
    }

    /**
     * Copy text to clipboard
     * @param {string} text - Text to copy
     */
    function copyToClipboard(text) {
  if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(function () {
          showToast('Copied to clipboard!', 'success');
  }).catch(function (err) {
            showToast('Failed to copy: ' + err.message, 'danger');
   });
        } else {
            // Fallback
  var textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
     document.body.appendChild(textarea);
            textarea.select();
        try {
           document.execCommand('copy');
        showToast('Copied to clipboard!', 'success');
        } catch (err) {
         showToast('Failed to copy.', 'danger');
         }
         document.body.removeChild(textarea);
        }
    }

    /**
     * Remove user avatar (placeholder functionality)
     */
    window.removeAvatar = function () {
        Swal.fire({
    title: 'Remove Avatar?',
          text: 'This will remove your current profile picture.',
   icon: 'warning',
 showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, remove it',
        cancelButtonText: 'Cancel'
        }).then((result) => {
      if (result.isConfirmed) {
           // TODO: Implement avatar removal
          Swal.fire('Not Implemented', 'Avatar removal functionality will be added soon.', 'info');
            }
        });
    };

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        // Recovery codes modal buttons
        document.getElementById('btnCopyRecoveryCodes')?.addEventListener('click', copyRecoveryCodesToClipboard);
     document.getElementById('btnDownloadRecoveryCodes')?.addEventListener('click', downloadRecoveryCodes);

        // Configure 2FA button
        var tfConfigureBtn = document.getElementById('tfConfigure');
        if (tfConfigureBtn) {
   tfConfigureBtn.addEventListener('click', function (e) {
           e.preventDefault();
 
           // Show loading state
    tfConfigureBtn.disabled = true;
  tfConfigureBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Generating...';
                
    fetch('/Account/Generate2faAjax', {
        method: 'POST',
        credentials: 'same-origin',
         headers: { 'RequestVerificationToken': getRequestVerificationToken() }
   }).then(function (r) {
   if (!r.ok) return r.text().then(t => { throw new Error(t || ('Status ' + r.status)); });
  return r.json();
          })
                .then(function (res) {
      // Reset button state
     tfConfigureBtn.disabled = false;
                 tfConfigureBtn.innerHTML = '<i class="bi bi-shield-plus me-2"></i>Configure Authenticator';
             
           if (res.success) {
      var tfArea = document.getElementById('tfArea');
     var tfKey = document.getElementById('tfKey');
     var tfFeedback = document.getElementById('tfFeedback');
       
        if (!tfArea || !tfKey) {
          console.error('Required elements not found');
         showToast('Error: Page elements missing', 'danger');
      return;
             }
       
     tfArea.style.display = 'block';
       
         // Render QR code
          renderQr(res.authenticatorUri);
     
           // Display formatted key
       var formattedKey = (res.sharedKey || '').replace(/(.{4})/g, '$1 ').trim();
                 tfKey.innerHTML = '';
       
          // Create key display with copy button
        var keyWrapper = document.createElement('div');
  keyWrapper.className = 'd-flex align-items-center justify-content-between bg-light p-3 rounded';
      keyWrapper.style.fontFamily = 'monospace';
    keyWrapper.style.fontSize = '1.1rem';
   
   var keyText = document.createElement('span');
 keyText.textContent = formattedKey;
          keyText.style.flex = '1';
     keyText.style.userSelect = 'all';
        
    var copyBtn = document.createElement('button');
        copyBtn.className = 'btn btn-sm btn-outline-primary ms-2';
       copyBtn.innerHTML = '<i class="bi bi-clipboard"></i>';
      copyBtn.title = 'Copy to clipboard';
         copyBtn.onclick = function() {
             copyToClipboard(res.sharedKey);
         };
             
                 keyWrapper.appendChild(keyText);
            keyWrapper.appendChild(copyBtn);
             tfKey.appendChild(keyWrapper);
            
  // Add help text
      var helpText = document.createElement('small');
             helpText.className = 'text-muted d-block mt-2';
      helpText.innerHTML = '<i class="bi bi-info-circle me-1"></i>Enter this key in your authenticator app if you can\'t scan the QR code.';
           tfKey.appendChild(helpText);
      
       if (tfFeedback) {
          tfFeedback.style.display = 'none';
               }
   
       // Focus on verification code input
            var tfCode = document.getElementById('tfCode');
                 if (tfCode) {
    setTimeout(function() {
    tfCode.focus();
          }, 500);
       }
          } else {
                var tfFeedback = document.getElementById('tfFeedback');
    if (tfFeedback) {
         tfFeedback.innerText = res.message || 'Failed to generate key.';
                tfFeedback.style.display = 'block';
   }
       showToast(res.message || 'Failed to generate 2FA key', 'danger');
      }
         }).catch(function (err) {
         // Reset button state
 tfConfigureBtn.disabled = false;
          tfConfigureBtn.innerHTML = '<i class="bi bi-shield-plus me-2"></i>Configure Authenticator';
    
         var tfFeedback = document.getElementById('tfFeedback');
          if (tfFeedback) {
    tfFeedback.innerText = err.message || 'Request failed.';
                tfFeedback.style.display = 'block';
        }
              showToast('Failed to generate 2FA: ' + (err.message || 'Network error'), 'danger');
      console.error('2FA generation error:', err);
                });
     });
        }

        // Enable 2FA button
        var tfEnableBtn = document.getElementById('tfEnable');
        if (tfEnableBtn) {
   tfEnableBtn.addEventListener('click', function (e) {
     e.preventDefault();
                var code = document.getElementById('tfCode').value.trim();
       var tfFeedback = document.getElementById('tfFeedback');
     
 if (!code) {
    if (tfFeedback) {
          tfFeedback.innerText = 'Please enter the verification code from your authenticator app.';
               tfFeedback.style.display = 'block';
    }
          showToast('Verification code required', 'warning');
      return;
          }
     
     // Validate code format (6 digits)
       if (!/^\d{6}$/.test(code)) {
                    if (tfFeedback) {
              tfFeedback.innerText = 'Verification code must be 6 digits.';
              tfFeedback.style.display = 'block';
               }
           showToast('Invalid code format', 'warning');
return;
    }
     
      // Show loading state
                tfEnableBtn.disabled = true;
      tfEnableBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Verifying...';

     fetch('/Account/EnableSecondFactorAjax', {
                    method: 'POST',
   credentials: 'same-origin',
          headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getRequestVerificationToken() },
            body: JSON.stringify({ VerificationCode: code })
          }).then(function (r) {
          if (!r.ok) return r.text().then(t => { throw new Error(t || ('Status ' + r.status)); });
      return r.json();
              })
                .then(function (res) {
              // Reset button state
         tfEnableBtn.disabled = false;
       tfEnableBtn.innerHTML = '<i class="bi bi-check-circle me-2"></i>Enable';
            
    if (res.success) {
    if (res.recovery) {
        showRecoveryCodesModal(res.recovery);
     }
showToast('Two-factor authentication enabled successfully!', 'success');
  // Note: Page will NOT auto-reload so user can save recovery codes from modal
          // User must manually close modal or navigate away
  } else {
  if (tfFeedback) {
       tfFeedback.innerText = res.message || 'Invalid verification code. Please try again.';
   tfFeedback.style.display = 'block';
 }
   showToast(res.message || 'Failed to enable 2FA', 'danger');
        }
   }).catch(function (err) {
      // Reset button state
tfEnableBtn.disabled = false;
               tfEnableBtn.innerHTML = '<i class="bi bi-check-circle me-2"></i>Enable';
         
    if (tfFeedback) {
            tfFeedback.innerText = err.message || 'Request failed.';
         tfFeedback.style.display = 'block';
     }
              showToast('Failed to enable 2FA: ' + (err.message || 'Network error'), 'danger');
 console.error('2FA enable error:', err);
 });
  });
     }

        // Disable 2FA button
    var tfDisableBtn = document.getElementById('tfDisable');
        if (tfDisableBtn) {
       tfDisableBtn.addEventListener('click', function (e) {
         e.preventDefault();
             Swal.fire({
        title: 'Disable Two-Factor Authentication?',
      text: 'You will need to reconfigure it to use authenticator apps again.',
   icon: 'warning',
              showCancelButton: true,
   confirmButtonColor: '#d33',
   cancelButtonColor: '#3085d6',
   confirmButtonText: 'Yes, disable it',
     cancelButtonText: 'Cancel'
         }).then(function (result) {
       if (result.isConfirmed) {
          var form = document.createElement('form');
      form.method = 'post';
            form.action = '/Account/DisableTwoFactorSelf';
                var token = getRequestVerificationToken();
 if (token) {
     var inp = document.createElement('input');
          inp.type = 'hidden';
         inp.name = '__RequestVerificationToken';
   inp.value = token;
         form.appendChild(inp);
        }
             document.body.appendChild(form);
     form.submit();
       }
        });
 });
        }

    // Reset recovery codes button
  var tfResetRecoveryBtn = document.getElementById('tfResetRecovery');
    if (tfResetRecoveryBtn) {
            tfResetRecoveryBtn.addEventListener('click', function (e) {
                e.preventDefault();
                Swal.fire({
          title: 'Reset Recovery Codes?',
     text: 'This will invalidate your existing recovery codes and generate new ones.',
          icon: 'warning',
   showCancelButton: true,
          confirmButtonColor: '#f0ad4e',
     cancelButtonColor: '#6c757d',
               confirmButtonText: 'Yes, reset them',
  cancelButtonText: 'Cancel'
      }).then(function (result) {
        if (result.isConfirmed) {
     fetch('/Account/ResetRecoveryCodesAjax', {
      method: 'POST',
        credentials: 'same-origin',
      headers: { 'RequestVerificationToken': getRequestVerificationToken() }
          }).then(function (r) {
    if (!r.ok) return r.text().then(t => { throw new Error(t || ('Status ' + r.status)); });
       return r.json();
      })
          .then(function (res) {
            if (res.success && res.recovery) {
 showRecoveryCodesModal(res.recovery);
         showToast('Recovery codes have been reset successfully!', 'success');
              } else {
             showToast(res.message || 'Failed to reset recovery codes.', 'danger');
    }
   }).catch(function (err) {
           showToast(err.message || 'Request failed.', 'danger');
  });
          }
       });
  });
 }

        // Activate tab based on TempData or URL hash
   (function activateTab() {
        var activeTabEl = document.querySelector('[data-active-tab]');
          var active = activeTabEl ? activeTabEl.getAttribute('data-active-tab') : '';
         if (!active) {
      active = window.location.hash.replace('#', '');
     }
            if (active) {
        var triggerEl = document.querySelector('#' + active + '-tab');
            if (triggerEl) {
              var tab = new bootstrap.Tab(triggerEl);
              tab.show();
     var target = document.querySelector(triggerEl.getAttribute('data-bs-target'));
     if (target) {
    var input = target.querySelector('input');
       if (input) input.focus();
     }
   }
            }
  })();
    });
})();