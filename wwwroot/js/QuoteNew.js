(function () {
    'use strict';

    function getModalController(modalElement) {
        if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
            if (typeof window.bootstrap.Modal.getInstance === 'function') {
                return window.bootstrap.Modal.getInstance(modalElement) || new window.bootstrap.Modal(modalElement);
            }

            return new window.bootstrap.Modal(modalElement);
        }

        if (window.jQuery && typeof window.jQuery.fn.modal === 'function') {
            return {
                show: function () {
                    window.jQuery(modalElement).modal('show');
                }
            };
        }

        return null;
    }

    function initializeQuoteModal() {
        var addButton = document.getElementById('btnAdd');
        var modalElement = document.getElementById('addInforModal');
        var addForm = document.getElementById('addInforForm');

        if (!addButton || !modalElement) {
            return;
        }

        addButton.addEventListener('click', function () {
            if (addForm) {
                addForm.reset();
            }

            var modal = getModalController(modalElement);
            if (modal) {
                modal.show();
            }
        });

        modalElement.addEventListener('shown.bs.modal', function () {
            var firstField = modalElement.querySelector('select, input:not([type="file"]), textarea');
            if (firstField) {
                firstField.focus();
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initializeQuoteModal();
    });
})();
