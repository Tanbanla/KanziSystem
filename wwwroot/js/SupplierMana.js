// Supplier management page script
document.addEventListener('DOMContentLoaded', function () {
    const api = {
        supplierSearch: '/Master/SearchSupplier',
        supplierCreate: '/Master/AddSupplier',
        supplierUpdate: `/Master/UpdateSupplier`,
        supplierDelete: `/Master/DeleteSupplier`,
        supplierExport: '/Master/Suppliers/ExportExcel',
        supplierImport: '/Master/Suppliers/ImportExcel',
        // Supplier Category Items API (BaoGia_NCC_CategoryDTO)
        getSupplierDetail: (codeNcc) => `/Master/GetSupplierDetail?codeNcc=${encodeURIComponent(codeNcc)}`,
        addSupplierDetail: '/Master/AddSupplierDetail',
        deleteSupplierDetail: (id) => `/Master/DeleteSupplierDetail?req=${encodeURIComponent(id)}`
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
        if (!res.ok) {
            tableBody.innerHTML = '<tr><td colspan="8" class="text-center">Không tải được dữ liệu</td></tr>';
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
                <td class="text-center">
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

    function updatePagingControls(rows) {
        if (!pageInfo) return;
        pageInfo.textContent = `Trang ${currentPage}`;
        if (btnPrevPage) btnPrevPage.disabled = currentPage <= 1;
        if (btnNextPage) btnNextPage.disabled = lastPageReached || !rows || rows.length === 0;
    }

    document.getElementById('btnAddSupplier')?.addEventListener('click', () => openSupplierModal({}));
    async function deleteSupplier(r) {
        const ok = await showConfirmDialog('Xác nhận đồng ý?', 'Bạn có chắc chắn muốn xóa nhà cung cấp này?');
        if (!ok) return;
        const payload = { Id: parseInt(r.ncc_Id) };
        const res = await fetch(api.supplierDelete, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)});
        if (!res.ok) {
            showDialog({
                title: 'Lỗi', message: `Xảy ra lỗi ${res.message}`, type: 'error'
            });
            return;
        }
        showDialog({ title: 'Thành công', message: 'Gửi yêu cầu thành công', type: 'success' });
        loadSuppliers();
    }

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
        supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">Đang tải...</td></tr>';
        try {
            const res = await fetch(api.getSupplierDetail(codeNcc), { method: 'GET' });
            if (!res.ok) { supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">Không tải được dữ liệu</td></tr>'; return; }
            const data = await res.json();
            const list = data.data ?? [];
            renderSupplierItems(Array.isArray(list) ? list : []);
        } catch (e) {
            supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">Lỗi tải dữ liệu</td></tr>';
        }
    }

    function renderSupplierItems(items) {
        if (!supplierItemsTbody) return;
        supplierItemsTbody.innerHTML = '';
        if (!items || items.length === 0) {
            supplierItemsTbody.innerHTML = '<tr><td colspan="7" class="text-center">Không có dữ liệu</td></tr>';
            return;
        }
        items.forEach(it => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="text-center" hidden>${it.id ?? ''}</td>
                <td class="text-center">${it.nvchR_ChungLoai ?? ''}</td>
                <td>${it.nvchR_MakeIn ?? ''}</td>
                <td>${it.chR_Status ?? ''}</td>
                <td>${it.chR_PIC ?? ''}</td>
                <td>${it.chR_Mail ?? ''}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-danger" data-action="delete-item">Xóa</button>
                </td>
            `;
            const btnDel = tr.querySelector('[data-action="delete-item"]');
            btnDel?.addEventListener('click', async () => {
                const ok = await showConfirmDialog('Xác nhận đồng ý?', 'Bạn có chắc chắn muốn xóa loại hàng này?');
                if (!ok) return;
                try {
                    const res = await fetch(api.deleteSupplierDetail(it.id), { method: 'GET' });
                    if (!res.ok) { showDialog({ title: 'Lỗi', message: 'Xóa thất bại', type: 'error' }); return; }
                    showDialog({ title: 'Thành công', message: 'Xóa thành công', type: 'success' });
                    loadSupplierItems(currentItemSupplier.ma);
                } catch {
                    showDialog({ title: 'Lỗi', message: 'Xóa thất bại', type: 'error' });
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
            showDialog({ title: 'Thiếu dữ liệu', message: 'Vui lòng nhập Mã hàng và Mã NCC', type: 'error' });
            return;
        }
        try {
            const res = await fetch(api.addSupplierDetail, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            if (!res.ok) { showDialog({ title: 'Lỗi', message: 'Thêm thất bại', type: 'error' }); return; }
            showDialog({ title: 'Thành công', message: 'Thêm loại hàng thành công', type: 'success' });
            // reset simple fields except MaNCC
            document.getElementById('CHR_MaHang').value = '';
            document.getElementById('NVCHAR_TenNCC').value = '';
            document.getElementById('NVCHR_CodeByNCC').value = '';
            document.getElementById('NVCHR_MakeIn').value = '';
            await loadSupplierItems(currentItemSupplier.ma);
        } catch {
            showDialog({ title: 'Lỗi', message: 'Thêm thất bại', type: 'error' });
        }
    };
    btnSaveItem?.addEventListener('click', saveItemHandler);
    btnSaveItemHeader?.addEventListener('click', saveItemHandler);

    btnImportItemsExcel?.addEventListener('click', () => itemsExcelFileInput?.click());
    itemsExcelFileInput?.addEventListener('change', async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        // Try parse via SheetJS if available
        if (!window.XLSX) {
            showDialog({ title: 'Thiếu thư viện', message: 'Không có thư viện đọc Excel trên trình duyệt. Vui lòng nhập từng dòng hoặc liên hệ quản trị.', type: 'error' });
            e.target.value = '';
            return;
        }
        try {
            const data = await file.arrayBuffer();
            const wb = window.XLSX.read(data, { type: 'array' });
            const ws = wb.Sheets[wb.SheetNames[0]];
            const rows = window.XLSX.utils.sheet_to_json(ws, { defval: '' });
            // Expect columns: CHR_MaHang, NVCHR_CodeByNCC, NVCHR_MakeIn, NVCHAR_TenNCC(optional)
            let okCount = 0, failCount = 0;
            for (const row of rows) {
                const payload = {
                    CHR_MaHang: String(row.CHR_MaHang || '').trim(),
                    CHR_MaNCC: currentItemSupplier.ma,
                    NVCHAR_TenNCC: String(row.NVCHAR_TenNCC || currentItemSupplier.ten || '').trim(),
                    NVCHR_CodeByNCC: String(row.NVCHR_CodeByNCC || '').trim(),
                    NVCHR_MakeIn: String(row.NVCHR_MakeIn || '').trim()
                };
                if (!payload.CHR_MaHang || !payload.CHR_MaNCC) { failCount++; continue; }
                try {
                    const res = await fetch(api.addSupplierDetail, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
                    if (res.ok) okCount++; else failCount++;
                } catch { failCount++; }
            }
            showDialog({ title: 'Nhập Excel', message: `Thành công: ${okCount}, Thất bại: ${failCount}`, type: failCount === 0 ? 'success' : 'info' });
            await loadSupplierItems(currentItemSupplier.ma);
        } catch {
            showDialog({ title: 'Lỗi', message: 'Không đọc được file Excel', type: 'error' });
        }
        e.target.value = '';
    });

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
    function showDialog({ title = 'Thông báo', message = '', type = 'info', buttons } = {}) {
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
        okBtn.textContent = (buttons && buttons.okText) || 'Đồng ý';
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
            el.querySelector('.cm-confirm-title').textContent = title || 'Xác nhận';
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
