
// Supplier management page script
// Requires Bootstrap JS for modals and a backend providing the specified endpoints.

(function () {
    // Ensure DOM is ready
    document.addEventListener('DOMContentLoaded', init);

    function init() {
        // Utilities
        const api = {
            supplierSearch: '/Master/Suppliers/Search',
            supplierGet: (id) => `/Master/Suppliers/${id}`,
            supplierCreate: '/Master/Suppliers',
            supplierUpdate: (id) => `/Master/Suppliers/${id}`,
            supplierDelete: (id) => `/Master/Suppliers/${id}`,
            supplierExport: '/Master/Suppliers/ExportExcel',
            supplierImport: '/Master/Suppliers/ImportExcel',
            itemsList: (ma) => `/Master/Suppliers/${ma}/Items`,
            itemCreate: (ma) => `/Master/Suppliers/${ma}/Items`,
            itemUpdate: (ma, id) => `/Master/Suppliers/${ma}/Items/${id}`,
            itemDelete: (ma, id) => `/Master/Suppliers/${ma}/Items/${id}`
        };

        const tableBody = document.querySelector('#suppliersTable tbody');
        const itemsBody = document.querySelector('#itemsTable tbody');

        // Search
        document.getElementById('btnSearch')?.addEventListener('click', loadSuppliers);
        document.getElementById('btnReset')?.addEventListener('click', () => {
            document.getElementById('searchMa').value = '';
            document.getElementById('searchTen').value = '';
            loadSuppliers();
        });

        async function loadSuppliers() {
            const ma = document.getElementById('searchMa').value.trim();
            const ten = document.getElementById('searchTen').value.trim();
            const qs = new URLSearchParams({ ma, ten });
            const res = await fetch(`${api.supplierSearch}?${qs}`, { method: 'GET' });
            if (!res.ok) { tableBody.innerHTML = '<tr><td colspan="8">Không tải được dữ liệu</td></tr>'; return; }
            const data = await res.json();
            renderSuppliers(data);
        }

        function renderSuppliers(rows) {
            tableBody.innerHTML = '';
            if (!rows || rows.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="8" class="text-center">Không có dữ liệu</td></tr>';
                return;
            }
            rows.forEach(r => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${r.ncc_Id ?? ''}</td>
                    <td>${r.ma ?? ''}</td>
                    <td>${r.ten ?? ''}</td>
                    <td>${r.diachi ?? ''}</td>
                    <td>${r.sodienthoai ?? ''}</td>
                    <td>${r.khuvuc ?? ''}</td>
                    <td>${r.nhom ?? ''}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-primary me-1" data-action="edit">Sửa</button>
                        <button class="btn btn-sm btn-danger me-1" data-action="delete">Xóa</button>
                        <button class="btn btn-sm btn-outline-secondary" data-action="detail">Chi tiết</button>
                    </td>
                `;
                tr.querySelector('[data-action="edit"]').addEventListener('click', () => openSupplierModal(r));
                tr.querySelector('[data-action="delete"]').addEventListener('click', () => deleteSupplier(r));
                tr.querySelector('[data-action="detail"]').addEventListener('click', () => showDetails(r));
                tableBody.appendChild(tr);
            });
        }

        // Add supplier
        document.getElementById('btnAddSupplier')?.addEventListener('click', () => openSupplierModal({}));
        async function deleteSupplier(r) {
            if (!confirm('Xóa nhà cung cấp?')) return;
            const res = await fetch(api.supplierDelete(r.ncc_Id), { method: 'DELETE' });
            if (res.ok) loadSuppliers();
        }

        // Supplier Modal
        const supplierModal = new bootstrap.Modal(document.getElementById('supplierModal'));
        document.getElementById('btnSaveSupplier')?.addEventListener('click', saveSupplier);
        function openSupplierModal(r) {
            document.getElementById('nccId').value = r.ncc_Id ?? '';
            document.getElementById('ma').value = r.ma ?? '';
            document.getElementById('ten').value = r.ten ?? '';
            document.getElementById('diachi').value = r.diachi ?? '';
            document.getElementById('sodienthoai').value = r.sodienthoai ?? '';
            document.getElementById('fax').value = r.fax ?? '';
            document.getElementById('khuvuc').value = r.khuvuc ?? '';
            document.getElementById('nhom').value = r.nhom ?? '';
            document.getElementById('masothue').value = r.masothue ?? '';
            document.getElementById('nhanvienkinhdoand').value = r.nhanvienkinhdoand ?? '';
            document.getElementById('nhanvienketoan').value = r.nhanvienketoan ?? '';
            document.getElementById('ghichu').value = r.ghichu ?? '';
            document.getElementById('hinhthucmotk').value = r.hinhthucmotk ?? '';
            document.getElementById('dieukienthanhtoan').value = r.dieukienthanhtoan ?? '';
            supplierModal.show();
        }
        async function saveSupplier() {
            const payload = {
                ncc_Id: +(document.getElementById('nccId').value || 0),
                ma: document.getElementById('ma').value.trim(),
                ten: document.getElementById('ten').value.trim(),
                diachi: document.getElementById('diachi').value.trim(),
                sodienthoai: document.getElementById('sodienthoai').value.trim(),
                fax: document.getElementById('fax').value.trim(),
                khuvuc: document.getElementById('khuvuc').value.trim(),
                nhom: document.getElementById('nhom').value.trim(),
                masothue: document.getElementById('masothue').value.trim(),
                nhanvienkinhdoand: document.getElementById('nhanvienkinhdoand').value.trim(),
                nhanvienketoan: document.getElementById('nhanvienketoan').value.trim(),
                ghichu: document.getElementById('ghichu').value.trim(),
                hinhthucmotk: document.getElementById('hinhthucmotk').value.trim(),
                dieukienthanhtoan: document.getElementById('dieukienthanhtoan').value.trim()
            };
            const isEdit = !!payload.ncc_Id;
            const res = await fetch(isEdit ? api.supplierUpdate(payload.ncc_Id) : api.supplierCreate, {
                method: isEdit ? 'PUT' : 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (res.ok) { supplierModal.hide(); loadSuppliers(); }
        }

        // Detail & items
        const itemModal = new bootstrap.Modal(document.getElementById('itemModal'));
        let currentSupplierMa = null;
        function showDetails(r) {
            currentSupplierMa = r.ma;
            document.getElementById('detailMa').value = r.ma ?? '';
            document.getElementById('detailTen').value = r.ten ?? '';
            document.getElementById('detailSdt').value = r.sodienthoai ?? '';
            document.getElementById('detailKhuvuc').value = r.khuvuc ?? '';
            loadItems(r.ma);
        }
        async function loadItems(ma) {
            const res = await fetch(api.itemsList(ma), { method: 'GET' });
            if (!res.ok) { itemsBody.innerHTML = '<tr><td colspan="7">Không tải được dữ liệu</td></tr>'; return; }
            const data = await res.json();
            renderItems(data);
        }
        function renderItems(rows) {
            itemsBody.innerHTML = '';
            if (!rows || rows.length === 0) { itemsBody.innerHTML = '<tr><td colspan="7" class="text-center">Không có dữ liệu</td></tr>'; return; }
            rows.forEach(r => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${r.id ?? ''}</td>
                    <td>${r.chr_MaHang ?? ''}</td>
                    <td>${r.chr_MaNCC ?? ''}</td>
                    <td>${r.nvchar_TenNCC ?? ''}</td>
                    <td>${r.nvchr_CodeByNCC ?? ''}</td>
                    <td>${r.nvchr_MakeIn ?? ''}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-primary me-1" data-action="edit">Sửa</button>
                        <button class="btn btn-sm btn-danger" data-action="delete">Xóa</button>
                    </td>`;
                tr.querySelector('[data-action="edit"]').addEventListener('click', () => openItemModal(r));
                tr.querySelector('[data-action="delete"]').addEventListener('click', () => deleteItem(r));
                itemsBody.appendChild(tr);
            });
        }
        document.getElementById('btnAddItem')?.addEventListener('click', () => openItemModal({ chr_MaNCC: currentSupplierMa }));
        function openItemModal(r) {
            document.getElementById('itemId').value = r.id ?? '';
            document.getElementById('CHR_MaHang').value = r.chr_MaHang ?? '';
            document.getElementById('CHR_MaNCC').value = r.chr_MaNCC ?? currentSupplierMa ?? '';
            document.getElementById('NVCHAR_TenNCC').value = r.nvchar_TenNCC ?? '';
            document.getElementById('NVCHR_CodeByNCC').value = r.nvchr_CodeByNCC ?? '';
            document.getElementById('NVCHR_MakeIn').value = r.nvchr_MakeIn ?? '';
            itemModal.show();
        }
        document.getElementById('btnSaveItem')?.addEventListener('click', saveItem);
        async function saveItem() {
            const payload = {
                id: +(document.getElementById('itemId').value || 0),
                chr_MaHang: document.getElementById('CHR_MaHang').value.trim(),
                chr_MaNCC: document.getElementById('CHR_MaNCC').value.trim(),
                nvchar_TenNCC: document.getElementById('NVCHAR_TenNCC').value.trim(),
                nvchr_CodeByNCC: document.getElementById('NVCHR_CodeByNCC').value.trim(),
                nvchr_MakeIn: document.getElementById('NVCHR_MakeIn').value.trim()
            };
            const isEdit = !!payload.id;
            const res = await fetch(isEdit ? api.itemUpdate(currentSupplierMa, payload.id) : api.itemCreate(currentSupplierMa), {
                method: isEdit ? 'PUT' : 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (res.ok) { itemModal.hide(); loadItems(currentSupplierMa); }
        }
        async function deleteItem(r) {
            if (!confirm('Xóa mặt hàng?')) return;
            const res = await fetch(api.itemDelete(currentSupplierMa, r.id), { method: 'DELETE' });
            if (res.ok) loadItems(currentSupplierMa);
        }

        // Import/Export Excel
        document.getElementById('btnExportExcel')?.addEventListener('click', async () => {
            const res = await fetch(api.supplierExport, { method: 'GET' });
            if (!res.ok) return;
            const blob = await res.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url; a.download = 'Suppliers.xlsx'; a.click();
            window.URL.revokeObjectURL(url);
        });
        document.getElementById('btnImportExcel')?.addEventListener('click', () => document.getElementById('excelFileInput').click());
        document.getElementById('excelFileInput')?.addEventListener('change', async (e) => {
            const file = e.target.files[0]; if (!file) return;
            const fd = new FormData(); fd.append('file', file);
            const res = await fetch(api.supplierImport, { method: 'POST', body: fd });
            if (res.ok) loadSuppliers();
            e.target.value = '';
        });

        // Initial load
        loadSuppliers();
    }
})();
