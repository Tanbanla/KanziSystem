// Supplier management page script
document.addEventListener('DOMContentLoaded', function () {
    const api = {
        supplierSearch: (window.apiBaseUrl || '') + '/Master/SearchSupplier',
        supplierCreate: (window.apiBaseUrl || '') + '/Master/AddSupplier',
        supplierUpdate: (window.apiBaseUrl || '') + `/Master/UpdateSupplier`,
        supplierDelete: (window.apiBaseUrl || '') + `/Master/DeleteSupplier`,
        supplierExport: (window.apiBaseUrl || '') + '/Master/ExportExcel',
        supplierImport: (window.apiBaseUrl || '') + '/Master/ImportSupplierExcel',
        // Supplier Category Items API (BaoGia_NCC_CategoryDTO)
        getSupplierDetail: (codeNcc) => (window.apiBaseUrl || '') +`/Master/GetSupplierDetail?codeNcc=${encodeURIComponent(codeNcc)}`,
        addSupplierDetail: (window.apiBaseUrl || '') + '/Master/AddSupplierDetail',
        deleteSupplierDetail: (id) => (window.apiBaseUrl || '') +`/Master/DeleteSupplierDetail?req=${encodeURIComponent(id)}`,
        addListSupplierDetail: (window.apiBaseUrl || '') + '/Master/AddListSupplierDetail',
        ImportSupplierDetail: (window.apiBaseUrl || '') + '/Master/ImportSupplierDetail', //UpdateMaterialInfo
        ImportExcelMaterial: (window.apiBaseUrl || '') + '/Master/ImportExcelMaterial'
    };

    const tableBody = document.querySelector('#suppliersTable tbody');

    // pagination state
    let currentPage = 1;
    let pageSize = parseInt(document.getElementById('pageSizeSelect')?.value ?? '20');
    let lastPageReached = false;

    const btnPrevPage = document.getElementById('btnPrevPage');
    const btnNextPage = document.getElementById('btnNextPage');
    const pageInfo = document.getElementById('pageInfo');
    const pageSizeSelect = document.getElementById('pageSizeSelect');
    const downloadMaster = document.getElementById('btnExportMaster');
    const btnImportExcelMaterial = document.getElementById('btnImportExcelMaterial');
    const btnTemplateImportExcel = document.getElementById('btnTemplateImportExcel');


    if (btnTemplateImportExcel) {
        btnTemplateImportExcel.addEventListener('click', async () => {
            const T = window.i18nSupplierMana || {};
            try {
                const templates = [
                    { url: (window.apiBaseUrl || '') + '/template/TemplateImportMaterial.xlsx', filename: 'Mẫu file Master Material.xlsx' },
                    { url: (window.apiBaseUrl || '') + '/template/NccMaster.xlsx', filename: 'Mẫu file Master Vendor.xlsx' }
                ];

                for (const template of templates) {
                    const a = document.createElement('a');
                    a.href = template.url;
                    a.download = template.filename;
                    document.body.appendChild(a);
                    a.click();
                    a.remove();
                }

                showDialog({ title: (T.SuccessTitle || 'Thành công'), message: (T.ExportSuccess || 'Xuất file mẫu hoàn tất'), type: 'success' });
            } catch (err) {
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.ExportFailed || 'Xuất file thất bại') + ': ' + (err && err.message ? err.message : err), type: 'error' });
            }
        });
    }

    if (downloadMaster) {
        downloadMaster.addEventListener('click', async () => {
            const T = window.i18nSupplierMana || {};
            try {
                const base = window.apiBaseUrl || '';
                const endpoints = [
                    { url: base + '/Master/ExportExcelMasterVendor', defaultName: 'ExportMasterVendor.xlsx' },
                    { url: base + '/Master/ExportExcelMasterMaterial', defaultName: 'ExportMasterMaterial.xlsx' }
                ];

                for (const ep of endpoints) {
                    const res = await fetch(ep.url, { method: 'GET' });
                    if (!res.ok) {
                        let txt = await res.text().catch(() => res.statusText);
                        showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.ExportFailed || 'Xuất file thất bại') + ': ' + (txt || res.statusText), type: 'error' });
                        continue;
                    }
                    const blob = await res.blob();
                    // try to parse filename from content-disposition
                    let filename = ep.defaultName;
                    try {
                        const cd = res.headers.get('content-disposition');
                        if (cd) {
                            const m = cd.match(/filename\*?=(?:UTF-8''|\")?([^;\"']+)/i);
                            if (m && m[1]) filename = decodeURIComponent(m[1].replace(/\"/g, '').trim());
                        }
                    } catch { }

                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = filename;
                    document.body.appendChild(a);
                    a.click();
                    a.remove();
                    window.URL.revokeObjectURL(url);
                }

                showDialog({ title: (T.SuccessTitle || 'Thành công'), message: (T.ExportSuccess || 'Xuất file hoàn tất'), type: 'success' });
            } catch (err) {
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.ExportFailed || 'Xuất file thất bại') + ': ' + (err && err.message ? err.message : err), type: 'error' });
            }
        });
    }
    document.getElementById('btnSearch')?.addEventListener('click', () => { currentPage = 1; loadSuppliers(); });
    document.getElementById('btnReset')?.addEventListener('click', () => {
        document.getElementById('searchMa').value = '';
        document.getElementById('searchTen').value = '';
        currentPage = 1; loadSuppliers();
    });

    btnPrevPage?.addEventListener('click', () => {
        if (currentPage > 1) { currentPage--; loadSuppliers(); }
    });
    btnNextPage?.addEventListener('click', () => {
        if (!lastPageReached) { currentPage++; loadSuppliers(); }
    });
    pageSizeSelect?.addEventListener('change', (e) => {
        pageSize = parseInt(e.target.value || '20');
        currentPage = 1;
        loadSuppliers();
    });

    async function loadSuppliers() {
        const ma = document.getElementById('searchMa').value.trim();
        const ten = document.getElementById('searchTen').value.trim();
        const body = { CodeNcc: ma, NameNcc: ten, PageIndex: currentPage, PageSize: pageSize };
        const res = await fetch(api.supplierSearch, { method: 'POST', body: JSON.stringify(body), headers: { 'Content-Type': 'application/json' } });
        const T = window.i18nSupplierMana || {};
        if (!res.ok) {
            tableBody.innerHTML = '<tr><td colspan="8" class="text-center">' + (T.LoadFailed || 'Không tải được dữ liệu') + '</td></tr>';
            updatePagingControls([]);
            return;
        }
        const data = await res.json();
        const rows = data.data ?? data.Data ?? [];
        // if returned rows < pageSize then this is last page
        lastPageReached = rows.length < pageSize;
        renderSuppliers(rows);
        updatePagingControls(rows);
    }

    function renderSuppliers(rows) {
        tableBody.innerHTML = '';
        const T = window.i18nSupplierMana || {};
        if (!rows || rows.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="8" class="text-center">' + (T.NoData || 'Không có dữ liệu') + '</td></tr>';
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
                <td class="text-center">
                    <button class="btn btn-sm btn-primary me-1" data-action="edit">${T.Edit}</button>
                    <button class="btn btn-sm btn-danger me-1" data-action="delete">${T.Delete}</button>
                    <button class="btn btn-sm btn-outline-secondary" data-action="detail">${T.Detail}</button>
                </td>
            `;
            tr.querySelector('[data-action="edit"]').addEventListener('click', () => openSupplierModal(r));
            tr.querySelector('[data-action="delete"]').addEventListener('click', () => deleteSupplier(r));
            tr.querySelector('[data-action="detail"]').addEventListener('click', () => showDetails(r));
            tableBody.appendChild(tr);
        });
    }

    function updatePagingControls(rows) {
        if (!pageInfo) return;
        const T = window.i18nSupplierMana || {};
        pageInfo.textContent = (T.PageIndex || 'Trang {0}').replace('{0}', currentPage);
        if (btnPrevPage) btnPrevPage.disabled = currentPage <= 1;
        if (btnNextPage) btnNextPage.disabled = lastPageReached || !rows || rows.length === 0;
    }

    document.getElementById('btnAddSupplier')?.addEventListener('click', () => openSupplierModal({}));
    async function deleteSupplier(r) {
        const T = window.i18nSupplierMana || {};
        const ok = await showConfirmDialog(T.ConfirmDeleteTitle || 'Xác nhận đồng ý?', T.ConfirmDeleteMessage || 'Bạn có chắc chắn muốn xóa nhà cung cấp này?');
        if (!ok) return;
        const payload = { Id: parseInt(r.ncc_Id) };
        const res = await fetch(api.supplierDelete, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)});
        if (!res.ok) {
            const T = window.i18nSupplierMana || {};
            showDialog({
                title: (T.ErrorTitle || 'Lỗi'), message: `Xảy ra lỗi ${res.message}`, type: 'error'
            });
            return;
        }
        const T2 = window.i18nSupplierMana || {};
        showDialog({ title: (T2.SuccessTitle || 'Thành công'), message: (T2.SaveSuccessMessage || 'Gửi yêu cầu thành công'), type: 'success' });
        loadSuppliers();
    }
    // xuất file mẫu
    document.getElementById('btnExportTemplateExcel')?.addEventListener('click', async () => {
        try {
            const url = (window.apiBaseUrl || '') + '/template/TemplateNCC.xlsx';
            const a = document.createElement('a');
            a.href = url;
            a.download = 'Mau_ChungLoai_NCC.xlsx';
            document.body.appendChild(a);
            a.click();
            a.remove();
        } catch (err) {
            console.error('Error downloading template', err);
        }
    });
    // initialize modals without backdrop to avoid modal-backdrop show
    const supplierModal = new bootstrap.Modal(document.getElementById('supplierModal'), { backdrop: false });
    const itemModal = new bootstrap.Modal(document.getElementById('itemModal'), { backdrop: false });
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
        document.getElementById('thuTucMoHaiQuan').value = r.canphaixacnhanlamthutuchaiquan ?? '';
        showEditModal('supplierModal');
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
            dieukienthanhtoan: document.getElementById('dieukienthanhtoan').value.trim(),
            Canphaixacnhanlamthutuchaiquan: document.getElementById('thuTucMoHaiQuan').value.trim(),
        };
        const isEdit = !!payload.ncc_Id;
        const res = await fetch(isEdit ? api.supplierUpdate : api.supplierCreate, {
            method:  'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (!res.ok) {
            showDialog({
                title: 'Lỗi', message: `Xảy ra lỗi ${res.message}`, type: 'error'
            });
            return;
        }
        hideEditModal('supplierModal');
        showDialog({ title: 'Thành công', message: 'Gửi yêu cầu thành công', type: 'success' });
        loadSuppliers();
    }

    // Supplier items (BaoGia_NCC_CategoryDTO) handling
    let currentItemSupplier = { ma: '', ten: '' };
    const supplierItemsTbody = document.querySelector('#supplierItemsTable tbody');
    const btnSaveItem = document.getElementById('btnSaveItem');
    const btnSaveItemHeader = document.getElementById('btnSaveItemHeader');
    const btnImportItemsExcel = document.getElementById('btnImportItemsExcel');
    const itemsExcelFileInput = document.getElementById('itemsExcelFileInput');
    const btnImportExcelFirst = document.getElementById('btnImportExcelFirst');
    const itemsExcelFileInputFirst = document.getElementById('itemsExcelFileInputFirst');

    async function showDetails(r) {
        currentItemSupplier = { ma: r.ma || '', ten: r.ten || '' };
        const codeEl = document.getElementById('selectedNccCode');
        const nameEl = document.getElementById('selectedNccName');
        if (codeEl) codeEl.textContent = currentItemSupplier.ma;
        if (nameEl) nameEl.textContent = currentItemSupplier.ten;
        // preset form's MaNCC
        const maNccInput = document.getElementById('CHR_MaNCC');
        if (maNccInput) maNccInput.value = currentItemSupplier.ma;
        await loadSupplierItems(currentItemSupplier.ma);
        showEditModal('itemModal');
    }

    async function loadSupplierItems(codeNcc) {
        if (!supplierItemsTbody) return;
        const T = window.i18nSupplierMana || {};
        supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">' + (T.Loading || 'Đang tải...') + '</td></tr>';
        try {
            const res = await fetch(api.getSupplierDetail(codeNcc), { method: 'GET' });
            if (!res.ok) { supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">' + (T.LoadFailed || 'Không tải được dữ liệu') + '</td></tr>'; return; }
            const data = await res.json();
            const list = data.data ?? [];
            renderSupplierItems(Array.isArray(list) ? list : []);
        } catch (e) {
            supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">' + (T.LoadFailed || 'Lỗi tải dữ liệu') + '</td></tr>';
        }
    }

    function renderSupplierItems(items) {
        if (!supplierItemsTbody) return;
        supplierItemsTbody.innerHTML = '';
        const T = window.i18nSupplierMana || {};
        if (!items || items.length === 0) {
            supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">' + (T.NoData || 'Không có dữ liệu') + '</td></tr>';
            return;
        }
        items.forEach(it => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="text-center" hidden>${it.id ?? ''}</td>
                <td class="text-center">${it.nvchR_ChungLoai ?? ''}</td>
                <td>${it.nvchR_SanXuat ?? ''}</td>
                <td class="text-center">${it.chR_Status ?? ''}</td>
                <td>${it.chR_PIC ?? ''}</td>
                <td>${it.chR_Mail ?? ''}</td>
                <td class="text-center">
                    <button class="btn btn-sm btn-danger" data-action="delete-item">${T.Delete}</button>
                </td>
            `;
            const btnDel = tr.querySelector('[data-action="delete-item"]');
            btnDel?.addEventListener('click', async () => {
                const ok = await showConfirmDialog((T.ConfirmDeleteTitle || 'Xác nhận đồng ý?'), (T.ConfirmDeleteMessage || 'Bạn có chắc chắn muốn xóa loại hàng này?'));
                if (!ok) return;
                try {
                    const res = await fetch(api.deleteSupplierDetail(it.id), { method: 'GET' });
                    if (!res.ok) { showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.DeleteErrorMessage || 'Xóa thất bại'), type: 'error' }); return; }
                    showDialog({ title: (T.SuccessTitle || 'Thành công'), message: (T.DeleteSuccessMessage || 'Xóa thành công'), type: 'success' });
                    loadSupplierItems(currentItemSupplier.ma);
                } catch {
                    showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.DeleteErrorMessage || 'Xóa thất bại'), type: 'error' });
                }
            });
            supplierItemsTbody.appendChild(tr);
        });
    }

    const saveItemHandler = async () => {
        const payload = {
            CHR_MaHang: document.getElementById('CHR_MaHang').value.trim(),
            CHR_MaNCC: document.getElementById('CHR_MaNCC').value.trim() || currentItemSupplier.ma,
            NVCHAR_TenNCC: document.getElementById('NVCHAR_TenNCC').value.trim() || currentItemSupplier.ten,
            NVCHR_CodeByNCC: document.getElementById('NVCHR_CodeByNCC').value.trim(),
            NVCHR_MakeIn: document.getElementById('NVCHR_MakeIn').value.trim()
        };
        if (!payload.CHR_MaHang || !payload.CHR_MaNCC) {
            const T = window.i18nSupplierMana || {};
            showDialog({ title: (T.ErrorTitle || 'Thiếu dữ liệu'), message: (T.MissingItemFields || 'Vui lòng nhập Mã hàng và Mã NCC'), type: 'error' });
            return;
        }
        try {
            const res = await fetch(api.addSupplierDetail, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            const T = window.i18nSupplierMana || {};
            if (!res.ok) { showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.AddItemFailed || 'Thêm thất bại'), type: 'error' }); return; }
            showDialog({ title: (T.SuccessTitle || 'Thành công'), message: (T.AddItemSuccess || 'Thêm loại hàng thành công'), type: 'success' });
            // reset simple fields except MaNCC
            document.getElementById('CHR_MaHang').value = '';
            document.getElementById('NVCHAR_TenNCC').value = '';
            document.getElementById('NVCHR_CodeByNCC').value = '';
            document.getElementById('NVCHR_MakeIn').value = '';
            await loadSupplierItems(currentItemSupplier.ma);
        } catch {
            const T = window.i18nSupplierMana || {};
            showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.AddItemFailed || 'Thêm thất bại'), type: 'error' });
        }
    };
    btnSaveItem?.addEventListener('click', saveItemHandler);
    btnSaveItemHeader?.addEventListener('click', saveItemHandler);

    btnImportItemsExcel?.addEventListener('click', () => itemsExcelFileInput?.click());
    itemsExcelFileInput?.addEventListener('change', async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        try {
            // property names expected by InsertFileExcelSupplierRequestDTO (multipart/form-data)
            const fd = new FormData();
            fd.append('FileExcel', file);
            fd.append('maNCC', currentItemSupplier.ma || '');
            fd.append('tenNCC', currentItemSupplier.ten || '');
            const res = await fetch(api.addListSupplierDetail, { method: 'POST', body: fd });
            if (!res.ok) {
                let txt = await res.text();
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportFailed || 'Nhập thất bại') + ': ' + (txt || res.statusText), type: 'error' });
            } else {
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportSuccess || 'Nhập file thành công'), type: 'success' });
                await loadSupplierItems(currentItemSupplier.ma);
            }
        } catch (err) {
            const T = window.i18nSupplierMana || {};
            showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.CannotSendFile || 'Không thể gửi file') + ': ' + (err.message || err), type: 'error' });
        }
        e.target.value = '';
    });

    btnImportExcelFirst?.addEventListener('click', () => itemsExcelFileInputFirst?.click());
    itemsExcelFileInputFirst?.addEventListener('change', async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        try {
            // property names expected by InsertFileExcelSupplierRequestDTO (multipart/form-data)
            const fd = new FormData();
            fd.append('FileExcel', file);
            const res = await fetch(api.ImportSupplierDetail, { method: 'POST', body: fd });
            if (!res.ok) {
                let txt = await res.text();
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportFailed || 'Nhập thất bại') + ': ' + (txt || res.statusText), type: 'error' });
            } else {
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportSuccess || 'Nhập file thành công'), type: 'success' });
                await loadSupplierItems(currentItemSupplier.ma);
            }
        } catch (err) {
            const T = window.i18nSupplierMana || {};
            showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.CannotSendFile || 'Không thể gửi file') + ': ' + (err.message || err), type: 'error' });
        }
        e.target.value = '';
    });

    // Input material info from excel file
    btnImportExcelMaterial?.addEventListener('click', () => itemsExcelFileInputMaterial?.click());
    itemsExcelFileInputMaterial?.addEventListener('change', async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        try {
            // property names expected by InsertFileExcelSupplierRequestDTO (multipart/form-data)
            const fd = new FormData();
            fd.append('FileExcel', file);
            const res = await fetch(api.ImportExcelMaterial, { method: 'POST', body: fd });
            if (!res.ok) {
                let txt = await res.text();
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportFailed || 'Nhập thất bại') + ': ' + (txt || res.statusText), type: 'error' });
            } else {
                const T = window.i18nSupplierMana || {};
                showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportSuccess || 'Nhập file thành công'), type: 'success' });
                await loadSupplierItems(currentItemSupplier.ma);
            }
        } catch (err) {
            const T = window.i18nSupplierMana || {};
            showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.CannotSendFile || 'Không thể gửi file') + ': ' + (err.message || err), type: 'error' });
        }
        e.target.value = '';
    });

    document.getElementById('btnExportExcel')?.addEventListener('click', async () => {
        const ma = document.getElementById('searchMa').value.trim();
        const ten = document.getElementById('searchTen').value.trim();
        const body = { CodeNcc: ma, NameNcc: ten, PageIndex: currentPage, PageSize: pageSize };
        const res = await fetch(api.supplierExport, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)});
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
        const fd = new FormData(); fd.append('importRequest', file);
        const res = await fetch(api.supplierImport, { method: 'POST', body: fd });
        if (res.ok) loadSuppliers();
        e.target.value = '';
    });
    function showEditModal(modalName) {
        const modalEl = document.getElementById(modalName);
        if (!modalEl) return;
        try {
            const bs = window.bootstrap;
            if (bs && bs.Modal) {
                const m = bs.Modal.getOrCreateInstance(modalEl);
                m.show();
            } else {
                // Fallback: manually show modal
                modalEl.style.display = 'block';
                modalEl.classList.add('show');
                modalEl.setAttribute('aria-hidden', 'false');
                // prevent body scroll
                document.body.classList.add('modal-open');
            }
        } catch {
            // Fallback: manually show modal
            modalEl.style.display = 'block';
            modalEl.classList.add('show');
            modalEl.setAttribute('aria-hidden', 'false');
            document.body.classList.add('modal-open');
        }
    }
    function hideEditModal(modalName) {
        const modalEl = document.getElementById(modalName);
        if (!modalEl) return;
        // Accessibility: if focus is inside modal, blur and move focus before hiding (to avoid aria-hidden ancestor with focused descendant)
        try {
            const active = document.activeElement;
            if (active && modalEl.contains(active)) {
                if (typeof active.blur === 'function') active.blur();
                const fallbackFocus = document.getElementById('btnApplyFilters') || document.body;
                if (fallbackFocus && typeof fallbackFocus.focus === 'function') fallbackFocus.focus();
            }
        } catch { }
        modalEl.style.display = 'none';
        modalEl.classList.remove('show');
        modalEl.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('modal-open');
        // clean up inline sizing
        try {
            const dialog = modalEl.querySelector('.modal-dialog');
            if (dialog) {
                dialog.style.maxWidth = '';
                dialog.style.width = '';
                dialog.style.margin = '';
            }
        } catch { }
        const backdrop = document.querySelector('.custom-modal-backdrop');
        if (backdrop) backdrop.remove();
    }
    // đóng modal 
    document.getElementById('btnCloseEdit_1')?.addEventListener('click', function () {
        hideEditModal('supplierModal');
    });
    document.getElementById('btnCloseEdit_2')?.addEventListener('click', function () {
        hideEditModal('supplierModal');
    });
    document.getElementById('btnCloseItem_top')?.addEventListener('click', function () {
        hideEditModal('itemModal');
    });
    document.getElementById('btnCloseItem_bottom')?.addEventListener('click', function () {
        hideEditModal('itemModal');
    });
    loadSuppliers();
    // show message dialog
    function getDialogEls() {
        const overlay = document.getElementById('cmDialogOverlay');
        const titleEl = document.getElementById('cmDialogTitle');
        const bodyEl = document.getElementById('cmDialogBody');
        const footerEl = document.getElementById('cmDialogFooter');
        return { overlay, titleEl, bodyEl, footerEl };
    }
    function showDialog({ title = (window.i18nSupplierMana && window.i18nSupplierMana.Notification) || 'Thông báo', message = '', type = 'info', buttons } = {}) {
        const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
        if (!overlay) return alert(message);

        // Ensure overlay is attached to body so fixed positioning is not clipped by parent containers
        try {
            if (overlay.parentElement !== document.body) document.body.appendChild(overlay);
        } catch (e) { /* ignore */ }

        titleEl.textContent = title;
        bodyEl.innerHTML = `<div class="d-flex align-items-start gap-2">
            <i class="fas ${type === 'success' ? 'fa-check-circle text-success' : type === 'error' ? 'fa-exclamation-circle text-danger' : 'fa-info-circle text-primary'}"></i>
            <div>${message}</div>
        </div>`;
        footerEl.innerHTML = '';
        const okBtn = document.createElement('button');
        okBtn.className = 'cm-btn cm-btn-primary';
        const T = window.i18nSupplierMana || {};
        okBtn.textContent = (buttons && buttons.okText) || (T.OK || 'Đồng ý');
        okBtn.addEventListener('click', () => hideDialog());
        footerEl.appendChild(okBtn);

        overlay.setAttribute('aria-hidden', 'false');
        overlay.style.display = 'flex';
        attachDialogCloseHandlers();
    }
    function hideDialog() {
        const { overlay } = getDialogEls();
        if (overlay) {
            overlay.style.display = 'none';
            overlay.setAttribute('aria-hidden', 'true');
        }
    }

    function attachDialogCloseHandlers() {
        const { overlay, footerEl } = getDialogEls();
        const closeBtn = overlay.querySelector('[data-cm-action="close"]');
        if (closeBtn) {
            closeBtn.onclick = () => {
                // If a confirm dialog is waiting, resolve it as false
                if (typeof window.__cmPendingResolve === 'function') {
                    const r = window.__cmPendingResolve;
                    window.__cmPendingResolve = null;
                    r(false);
                }
                hideDialog();
            };
        }
        const overlayClick = overlay.querySelector('[data-cm-action="overlay"]');
        if (overlayClick) overlayClick.onclick = () => {
            if (typeof window.__cmPendingResolve === 'function') {
                const r = window.__cmPendingResolve;
                window.__cmPendingResolve = null;
                r(false);
            }
            hideDialog();
        };
    }
    // Custom dialogs
    function showConfirmDialog(title, message) {
        return new Promise((resolve) => {
            const el = document.getElementById('cmConfirmDialog');
            if (!el) { resolve(false); return; }
            const T = window.i18nSupplierMana || {};
            el.querySelector('.cm-confirm-title').textContent = title || (T.Confirm || 'Xác nhận');
            el.querySelector('.cm-confirm-body').textContent = message || '';
            //const overlay = el.querySelector('.cm-dialog-backdrop');
            const btnCancel = el.querySelector('[data-cm-action="cancel"]');
            const btnOk = el.querySelector('[data-cm-action="ok"]');
            const close = () => { el.setAttribute('aria-hidden', 'true'); el.classList.remove('show'); el.style.display = 'none'; document.body.classList.remove('modal-open'); cleanup(); };
            const open = () => { el.style.display = 'block'; el.style.zIndex = '3000'; el.setAttribute('aria-hidden', 'false'); el.classList.add('show'); document.body.classList.add('modal-open'); };
            const onCancel = () => { close(); resolve(false); };
            const onOk = () => { close(); resolve(true); };
            const cleanup = () => {
                //overlay && overlay.removeEventListener('click', onCancel);
                btnCancel && btnCancel.removeEventListener('click', onCancel);
                btnOk && btnOk.removeEventListener('click', onOk);
            };
            //overlay && overlay.addEventListener('click', onCancel);
            btnCancel && btnCancel.addEventListener('click', onCancel);
            btnOk && btnOk.addEventListener('click', onOk);
            open();
        });
    }
});
