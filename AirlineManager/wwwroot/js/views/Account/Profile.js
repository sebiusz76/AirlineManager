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
        container.innerHTML = '';
        new QRCode(container, { text: uri, width: 200, height: 200 });
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
                fetch('/Account/Generate2faAjax', {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: { 'RequestVerificationToken': getRequestVerificationToken() }
                }).then(function (r) {
                    if (!r.ok) return r.text().then(t => { throw new Error(t || ('Status ' + r.status)); });
                    return r.json();
                })
                    .then(function (res) {
                        if (res.success) {
                            document.getElementById('tfArea').style.display = 'block';
                            renderQr(res.authenticatorUri);
                            document.getElementById('tfKey').innerText = (res.sharedKey || '').replace(/(.{4})/g, '$1 ').trim();
                            document.getElementById('tfFeedback').style.display = 'none';
                        } else {
                            document.getElementById('tfFeedback').innerText = res.message || 'Failed to generate key.';
                            document.getElementById('tfFeedback').style.display = 'block';
                        }
                    }).catch(function (err) {
                        document.getElementById('tfFeedback').innerText = err.message || 'Request failed.';
                        document.getElementById('tfFeedback').style.display = 'block';
                    });
            });
        }

        // Enable 2FA button
        var tfEnableBtn = document.getElementById('tfEnable');
        if (tfEnableBtn) {
            tfEnableBtn.addEventListener('click', function (e) {
                e.preventDefault();
                var code = document.getElementById('tfCode').value.trim();
                if (!code) {
                    document.getElementById('tfFeedback').innerText = 'Enter verification code.';
                    document.getElementById('tfFeedback').style.display = 'block';
                    return;
                }
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
                        if (res.success) {
                            if (res.recovery) {
                                showRecoveryCodesModal(res.recovery);
                                showToast('Two-factor authentication enabled successfully!', 'success');
                            }
                            setTimeout(function () { location.reload(); }, 3000);
                        } else {
                            document.getElementById('tfFeedback').innerText = res.message || 'Failed to enable.';
                            document.getElementById('tfFeedback').style.display = 'block';
                        }
                    }).catch(function (err) {
                        document.getElementById('tfFeedback').innerText = err.message || 'Request failed.';
                        document.getElementById('tfFeedback').style.display = 'block';
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