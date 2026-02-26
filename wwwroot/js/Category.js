// Category.js
document.addEventListener('DOMContentLoaded', function () {
    let categories = [];
    let filteredCategories = [];

    // Load categories
    async function loadCategories(searchTerm = '') {
        try {
            const response = await fetch('/Master/SearchCategoryByName', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(searchTerm)
            });
            const result = await response.json();
            if (result.success) {
                categories = result.data;
                filteredCategories = categories;
                renderTable();
            } else {
                showDialog(window.i18nCategory.ErrorTitle, result.message || window.i18nCategory.LoadFailed);
            }
        } catch (error) {
            showDialog(window.i18nCategory.ErrorTitle, window.i18nCategory.ErrorConnection);
        }
    }

    // Render table
    function renderTable() {
        const tbody = document.querySelector('#categoriesTable tbody');
        tbody.innerHTML = '';
        if (filteredCategories.length === 0) {
            tbody.innerHTML = `<tr><td colspan="3" class="text-center">${window.i18nCategory.NoData}</td></tr>`;
            return;
        }
        filteredCategories.forEach(cat => {
            const row = `
                <tr>
                    <td class="text-center">${cat.id}</td>
                    <td>${cat.nvchR_Category}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-outline-danger" onclick="deleteCategory(${cat.id})">
                            <i class="fas fa-trash"></i> ${window.i18nCategory.Delete}
                        </button>
                    </td>
                </tr>
            `;
            tbody.innerHTML += row;
        });
    }

    // Search
    document.getElementById('btnSearch').addEventListener('click', function () {
        const searchTerm = document.getElementById('searchName').value.trim();
        filteredCategories = categories.filter(cat => cat.nvchR_Category.toLowerCase().includes(searchTerm.toLowerCase()));
        renderTable();
    });

    // Reset
    document.getElementById('btnReset').addEventListener('click', function () {
        document.getElementById('searchName').value = '';
        filteredCategories = categories;
        renderTable();
    });

    // Add category
    document.getElementById('btnAddCategory').addEventListener('click', function () {
        document.getElementById('categoryName').value = '';
        document.getElementById('categoryModal').style.display = 'flex';
    });

    // Save category
    document.getElementById('btnSaveCategory').addEventListener('click', async function () {
        const name = document.getElementById('categoryName').value.trim();
        if (!name) {
            showDialog(window.i18nCategory.ErrorTitle, 'Tên ch?ng lo?i không ???c ?? tr?ng');
            return;
        }
        try {
            const response = await fetch('/Master/AddCategory', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ NVCHR_Category: name })
            });
            const result = await response.json();
            if (result.success) {
                showDialog(window.i18nCategory.SuccessTitle, window.i18nCategory.SaveSuccessMessage);
                document.getElementById('categoryModal').style.display = 'none';
                loadCategories();
            } else {
                showDialog(window.i18nCategory.ErrorTitle, result.message);
            }
        } catch (error) {
            showDialog(window.i18nCategory.ErrorTitle, window.i18nCategory.ErrorConnection);
        }
    });

    // Delete category
    window.deleteCategory = async function (id) {
        showConfirm(window.i18nCategory.ConfirmDeleteTitle, window.i18nCategory.ConfirmDeleteMessage, async () => {
            try {
                const response = await fetch('/Master/DeleteCategory', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(id)
                });
                const result = await response.json();
                if (result.success) {
                    showDialog(window.i18nCategory.DeleteSuccessTitle, window.i18nCategory.DeleteSuccessMessage);
                    loadCategories();
                } else {
                    showDialog(window.i18nCategory.ErrorTitle, result.message);
                }
            } catch (error) {
                showDialog(window.i18nCategory.ErrorTitle, window.i18nCategory.ErrorConnection);
            }
        });
    };

    // Import Excel
    document.getElementById('btnImportExcel').addEventListener('click', function () {
        document.getElementById('excelFileInput').click();
    });

    document.getElementById('excelFileInput').addEventListener('change', async function (e) {
        const file = e.target.files[0];
        if (!file) return;
        const formData = new FormData();
        formData.append('importRequest', file);
        try {
            const response = await fetch('/Master/ImportCategory', {
                method: 'POST',
                body: formData
            });
            const result = await response.json();
            if (response.ok) {
                showDialog(window.i18nCategory.SuccessTitle, 'Import thành công');
                loadCategories();
            } else {
                showDialog(window.i18nCategory.ErrorTitle, result.title || 'Import th?t b?i');
            }
        } catch (error) {
            showDialog(window.i18nCategory.ErrorTitle, window.i18nCategory.ErrorConnection);
        }
        e.target.value = '';
    });

    // Modal close
    document.getElementById('btnCloseEdit').addEventListener('click', function () {
        document.getElementById('categoryModal').style.display = 'none';
    });
    document.getElementById('btnCloseModal').addEventListener('click', function () {
        document.getElementById('categoryModal').style.display = 'none';
    });

    // Dialog functions (simplified)
    function showDialog(title, message) {
        document.getElementById('cmDialogTitle').textContent = title;
        document.getElementById('cmDialogBody').textContent = message;
        document.getElementById('cmDialogFooter').innerHTML = `<button class="cm-btn cm-btn-primary" data-cm-action="close">${window.i18nCategory.OK}</button>`;
        document.getElementById('cmDialogOverlay').style.display = 'flex';
    }

    function showConfirm(title, message, onConfirm) {
        document.getElementById('cmConfirmTitle').textContent = title;
        document.getElementById('cmConfirmDialog').querySelector('.cm-confirm-body').textContent = message;
        document.getElementById('cmConfirmDialog').style.display = 'flex';
        const okBtn = document.querySelector('#cmConfirmDialog [data-cm-action="ok"]');
        okBtn.onclick = () => {
            document.getElementById('cmConfirmDialog').style.display = 'none';
            onConfirm();
        };
    }

    // Close dialogs
    document.querySelectorAll('[data-cm-action="close"], [data-cm-action="overlay"], [data-cm-action="cancel"]').forEach(el => {
        el.addEventListener('click', function () {
            this.closest('.cm-dialog-overlay').style.display = 'none';
        });
    });

    // Initial load
    loadCategories();
});