/* ============================================
   ADMIN/APPLOGS/INDEX VIEW SCRIPTS
   Log management functionality
 ============================================ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var btnDeleteOld = document.getElementById('btnDeleteOld');
        if (btnDeleteOld) {
            btnDeleteOld.addEventListener('click', function () {
                Swal.fire({
                    title: 'Delete Old Logs?',
                    text: 'This will permanently delete all log entries older than 30 days.',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#f0ad4e',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Yes, delete them',
                    cancelButtonText: 'Cancel'
                }).then(function (result) {
                    if (result.isConfirmed) {
                        document.getElementById('deleteOldForm').submit();
                    }
                });
            });
        }

        var btnDeleteAll = document.getElementById('btnDeleteAll');
        if (btnDeleteAll) {
            btnDeleteAll.addEventListener('click', function () {
                Swal.fire({
                    title: 'Delete ALL Logs?',
                    html: '<strong>WARNING:</strong> This will permanently delete ALL log entries in the database.<br>This action cannot be undone.',
                    icon: 'error',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Yes, delete all',
                    cancelButtonText: 'Cancel'
                }).then(function (result) {
                    if (result.isConfirmed) {
                        document.getElementById('deleteAllForm').submit();
                    }
                });
            });
        }
    });
})();