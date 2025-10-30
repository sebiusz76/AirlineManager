/* ============================================
   SHARED/_LAYOUT VIEW SCRIPTS
   Main layout JavaScript functionality
 ============================================ */

(function () {
    'use strict';

    // Initialize on DOM ready
    $(function () {
        // Initialize Bootstrap tooltips
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });

        // Show toast if TempData contains notification
        const toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true
        });

        var toastType = document.querySelector('[data-toast-type]')?.getAttribute('data-toast-type') || '';
        var toastMessage = document.querySelector('[data-toast-message]')?.getAttribute('data-toast-message') || '';

        if (toastMessage) {
            // map server-side toast types to Swal icons
            var icon = 'info';
            switch ((toastType || '').toLowerCase()) {
                case 'success':
                    icon = 'success';
                    break;
                case 'error':
                    icon = 'error';
                    break;
                case 'warning':
                    icon = 'warning';
                    break;
                case 'info':
                    icon = 'info';
                    break;
            }

            toast.fire({
                icon: icon,
                title: toastMessage
            });
        }

        // Attach delete confirmation handler
        $(document).on('submit', 'form.delete-form', function (e) {
            var form = this;
            e.preventDefault();
            var email = $(form).find('.js-delete-user').data('email');

            Swal.fire({
                title: 'Are you sure? ',
                text: `Delete user ${email}? This action cannot be undone.`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });

        // Unsaved changes handling for edit forms
        var isDirty = false;
        $(document).on('change', 'form.edit-form :input', function () {
            isDirty = true;
        });

        // Intercept navigation away from edit page (links, back button)
        $(document).on('click', 'a', function (e) {
            var href = $(this).attr('href');
            if (isDirty && href && !href.startsWith('#') && !$(this).attr('target')) {
                e.preventDefault();
                var link = this;
                Swal.fire({
                    title: 'You have unsaved changes',
                    text: 'Are you sure you want to leave without saving?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Leave',
                    cancelButtonText: 'Stay'
                }).then(function (result) {
                    if (result.isConfirmed) {
                        window.location = href;
                    }
                });
            }
        });

        // When edit form is submitted, reset dirty flag
        $(document).on('submit', 'form.edit-form', function () {
            isDirty = false;
        });
    });
})();