(function () {
    const tblBody = document.getElementById('historyGroupTableBody');
    const statusFilter = document.getElementById('statusFilter');
    const btnApply = document.getElementById('btnApplyFilters');
    const btnReset = document.getElementById('btnResetFilters');
    const paginationEl = document.getElementById('historyPagination');
    const paginationInfoEl = document.getElementById('historyPaginationInfo');
    const btnExportHistory = document.getElementById('btnExportHistory');
    const btnImportHistory = document.getElementById('btnImportHistory');
    const supplierSelect = document.getElementById('editNhaCungCap');
    const hiddenTenNCC = document.getElementById('editTenNCC');
    let currentPage = 1;
    const pageSize = 50;
    let currentGroups = [];
    let totalCountServer = 0;
    let serverPaged = false;
    const role = window.HistoryData.role || 'User';

    function updateHiddenValue() {
        const selectedOption = supplierSelect.options[supplierSelect.selectedIndex];
        if (selectedOption && selectedOption.value) {
            hiddenTenNCC.value = selectedOption.text;
        } else {
            hiddenTenNCC.value = '';
        }
    }

    // Show return reason modal (separate from delete modal)
    function showReturnReasonModal() {
        return new Promise((resolve) => {
            const modalEl = document.getElementById('returnReasonModal');
            const textarea = document.getElementById('returnReasonText');
            const notice = document.getElementById('returnReasonNotice');
            const confirmBtn = document.getElementById('confirmReturnWithReason');
            if (!modalEl || !textarea || !confirmBtn) return resolve(null);

            // reset
            textarea.value = '';
            if (notice) notice.style.display = 'none';

            // ensure modal in body
            try { if (modalEl.parentElement !== document.body) document.body.appendChild(modalEl); } catch (e) { }

            let bsModal = null;
            try {
                if (window.bootstrap && bootstrap.Modal) {
                    bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static' });
                    bsModal.show();
                } else {
                    modalEl.style.display = 'block';
                    modalEl.classList.add('show');
                    document.body.classList.add('modal-open');
                }
            } catch (e) {
                modalEl.style.display = 'block';
                modalEl.classList.add('show');
                document.body.classList.add('modal-open');
            }

            function cleanup() {
                try { if (bsModal) bsModal.hide(); else { modalEl.style.display = 'none'; modalEl.classList.remove('show'); document.body.classList.remove('modal-open'); } } catch (e) { modalEl.style.display = 'none'; modalEl.classList.remove('show'); document.body.classList.remove('modal-open'); }
                try { confirmBtn.removeEventListener('click', onConfirm); } catch (e) { }
                try { modalEl.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.removeEventListener('click', onCancel)); } catch (e) { }
                try { modalEl.removeEventListener('hidden.bs.modal', onHidden); } catch (e) { }
            }

            function onHidden() { cleanup(); resolve(null); }
            function onCancel(e) { e && e.preventDefault(); cleanup(); resolve(null); }
            function onConfirm(e) {
                e && e.preventDefault();
                const reason = (textarea.value || '').trim();
                if (!reason) {
                    if (notice) notice.style.display = '';
                    return;
                }
                cleanup();
                resolve(reason);
            }

            try { modalEl.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.addEventListener('click', onCancel)); } catch (e) { }
            try { if (bsModal) modalEl.addEventListener('hidden.bs.modal', onHidden); } catch (e) { }
            confirmBtn.addEventListener('click', onConfirm);
            // focus textarea
            try { textarea.focus(); } catch (e) { }
        });
    }
    function showConfirmDialog(title, html, confirmText, cancelText) {
        const overlay = document.getElementById('cmDialogOverlay');
        const body = document.getElementById('cmDialogBody');
        const footer = document.getElementById('cmDialogFooter');
        const titleEl = document.getElementById('cmDialogTitle');
        if (!overlay || !body || !footer || !titleEl) {
            // fallback
            return Promise.resolve(window.confirm(html || title || 'Confirm'));
        }
        const T = window.i18nHistoryQuote || {};
        titleEl.textContent = title || (T.Confirm || 'Xác nhận');
        body.innerHTML = html || '';
        footer.innerHTML = `
            <div class="d-flex gap-2" style="justify-content:flex-end;">
                <button type="button" class="cm-btn cm-btn-cancel">${escapeHtml(cancelText || (T.Cancel || 'Hủy'))}</button>
                <div style="margin-right: 4px;"></div>
                <button type="button" class="cm-btn cm-btn-confirm" style ="background-color: #2335b7; color: white">${escapeHtml(confirmText || (T.Confirm || 'Có'))}</button>
            </div>`;

        overlay.style.display = 'flex';
        overlay.setAttribute('aria-hidden', 'false');

        try {
            const dlg = overlay.querySelector('.cm-dialog');
            const focusable = dlg && dlg.querySelector('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
            if (focusable && typeof focusable.focus === 'function') focusable.focus();
        } catch { }

        return new Promise((resolve) => {
            function cleanup() {
                overlay.setAttribute('aria-hidden', 'true');
                overlay.style.display = 'none';
                overlay.removeEventListener('click', overlayClickHandler);
                cancelBtn.removeEventListener('click', onCancel);
                confirmBtn.removeEventListener('click', onConfirm);
            }
            function onConfirm(e) { e && e.preventDefault(); cleanup(); resolve(true); }
            function onCancel(e) { e && e.preventDefault(); cleanup(); resolve(false); }
            function overlayClickHandler(evt) {
                const target = evt.target.closest('[data-cm-action="overlay"], [data-cm-action="close"]');
                if (target) { onCancel(); }
            }

            const confirmBtn = footer.querySelector('.cm-btn-confirm');
            const cancelBtn = footer.querySelector('.cm-btn-cancel');
            overlay.addEventListener('click', overlayClickHandler);
            cancelBtn.addEventListener('click', onCancel);
            confirmBtn.addEventListener('click', onConfirm);
        });
    }

    function escapeHtml(s) {
        if (!s) return '';
        return String(s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": "&#39;" }[c]; });
    }
    // Gán sự kiện change cho select
    supplierSelect.addEventListener('change', updateHiddenValue);

    // Show delete reason 
    function showDeleteReasonModal() {
        return new Promise((resolve) => {
            const modalEl = document.getElementById('deleteReasonModal');
            const textarea = document.getElementById('deleteReasonText');
            const notice = document.getElementById('deleteReasonNotice');
            const confirmBtn = document.getElementById('confirmDeleteWithReason');
            if (!modalEl || !textarea || !confirmBtn) return resolve(null);

            // reset
            textarea.value = '';
            notice.style.display = 'none';

            // ensure modal in body
            try { if (modalEl.parentElement !== document.body) document.body.appendChild(modalEl); } catch (e) { }

            let bsModal = null;
            try {
                if (window.bootstrap && bootstrap.Modal) {
                    bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static' });
                    bsModal.show();
                } else {
                    modalEl.style.display = 'block';
                    modalEl.classList.add('show');
                    document.body.classList.add('modal-open');
                }
            } catch (e) {
                modalEl.style.display = 'block';
                modalEl.classList.add('show');
                document.body.classList.add('modal-open');
            }

            function cleanup() {
                try { if (bsModal) bsModal.hide(); else { modalEl.style.display = 'none'; modalEl.classList.remove('show'); document.body.classList.remove('modal-open'); } } catch (e) { modalEl.style.display = 'none'; modalEl.classList.remove('show'); document.body.classList.remove('modal-open'); }
                try { confirmBtn.removeEventListener('click', onConfirm); } catch (e) { }
                try { modalEl.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.removeEventListener('click', onCancel)); } catch (e) { }
                try { modalEl.removeEventListener('hidden.bs.modal', onHidden); } catch (e) { }
            }

            function onHidden() { cleanup(); resolve(null); }
            function onCancel(e) { e && e.preventDefault(); cleanup(); resolve(null); }
            function onConfirm(e) {
                e && e.preventDefault();
                const reason = (textarea.value || '').trim();
                if (!reason) {
                    if (notice) notice.style.display = '';
                    return;
                }
                cleanup();
                resolve(reason);
            }

            try { modalEl.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.addEventListener('click', onCancel)); } catch (e) { }
            try { if (bsModal) modalEl.addEventListener('hidden.bs.modal', onHidden); } catch (e) { }
            confirmBtn.addEventListener('click', onConfirm);
            // focus textarea
            try { textarea.focus(); } catch (e) { }
        });
    }

    function applyFilters(page = 1) {
        const maDon = (document.getElementById('searchMaDon').value || '').trim();
        const phongBan = (document.getElementById('searchPhongBan').value || '').trim();
        const nguoiTao = (document.getElementById('searchNguoiTao').value || '').trim();
        const maVatTu = (document.getElementById('searchMaVatTu').value || '').trim();
        const nhaCungCap = (document.getElementById('searchNhaCungCap').value || '').trim();
        const status = statusFilter.value;
        const from = document.getElementById('dateFrom').value;
        const to = document.getElementById('dateTo').value;
        // build payload for SearchBaoGia
        const payload = {
            MaDon: maDon,
            MaNcc: nhaCungCap,
            Section: phongBan,
            NguoiYeuCau: nguoiTao,
            MaHang: maVatTu,
            TrangThai: status,
            Step: null,
            PageIndex: page,
            PageSize: pageSize,
            from: from || null,
            to: to || null,
            Chungloai: ''
        };
        fetch((window.apiBaseUrl || '') + '/History/SearchHistoryBaoGia', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(async r => {
                const T = window.i18nHistoryQuote || {};
                if (!r.ok) {
                    const errorText = await r.text().catch(() => '');
                    throw new Error(errorText || (T.MsgSearchFailed || 'Search failed'));
                }
                return r.json();
            })
            .then(data => {
                const wrapper = data?.data ?? data?.Data ?? data;
                let itemsArray = [];
                let totalFromServer = 0;
                let foundServerPaging = false;

                if (Array.isArray(wrapper)) {
                    itemsArray = wrapper;
                } else if (wrapper && Array.isArray(wrapper.data)) {
                    itemsArray = wrapper.data;
                    if (typeof wrapper.totalCount === 'number') { totalFromServer = wrapper.totalCount; foundServerPaging = true; }
                    else if (typeof wrapper.total === 'number') { totalFromServer = wrapper.total; foundServerPaging = true; }
                } else if (wrapper && Array.isArray(wrapper.Data)) {
                    itemsArray = wrapper.Data;
                    if (typeof wrapper.TotalCount === 'number') { totalFromServer = wrapper.TotalCount; foundServerPaging = true; }
                } else if (data && Array.isArray(data.data)) {
                    itemsArray = data.data;
                } else {
                    // fallback: try to find array inside returned object
                    itemsArray = [];
                }

                serverPaged = !!foundServerPaging;
                totalCountServer = serverPaged ? totalFromServer : 0;

                currentGroups = groupByMaDon(itemsArray);
                currentPage = page;
                renderGroups();
            })
            .catch(err => {
                console.error(err);
                renderEmpty();
            });
    }

    btnApply?.addEventListener('click', () => applyFilters(1));
    btnReset?.addEventListener('click', () => {
        document.getElementById('searchMaDon').value = '';
        document.getElementById('searchPhongBan').value = '';
        document.getElementById('searchNguoiTao').value = '';
        document.getElementById('searchMaVatTu').value = '';
        document.getElementById('searchNhaCungCap').value = '';
        statusFilter.value = '';
        document.getElementById('dateFrom').value = '';
        document.getElementById('dateTo').value = '';
        applyFilters(1);
    });
    btnExportHistory?.addEventListener('click', async () => {
        const maDon = (document.getElementById('searchMaDon').value || '').trim();
        const phongBan = (document.getElementById('searchPhongBan').value || '').trim();
        const nguoiTao = (document.getElementById('searchNguoiTao').value || '').trim();
        const maVatTu = (document.getElementById('searchMaVatTu').value || '').trim();
        const nhaCungCap = (document.getElementById('searchNhaCungCap').value || '').trim();
        const status = statusFilter.value;
        const from = document.getElementById('dateFrom').value;
        const to = document.getElementById('dateTo').value;
        // build payload for ExportHistory
        const payload = {
            MaDon: maDon,
            MaNcc: nhaCungCap,
            Section: phongBan,
            NguoiYeuCau: nguoiTao,
            MaHang: maVatTu,
            TrangThai: status,
            Step: null,
            PageIndex: 1,
            PageSize: 100,
            from: from || null,
            to: to || null,
            Chungloai: ''
        };
        // ExportHistory
        const T = window.i18nHistoryQuote || {};
        try {
            showLoading(T.Exporting || 'Đang xuất...');
            const res = await fetch((window.apiBaseUrl || '') + '/History/ExportManagerHistory', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) {
                const msg = await res.text().catch(() => T.ExportError || 'Xuất file thất bại');
                throw new Error(msg);
            }
            const blob = await res.blob();
            let fileName = 'HistoryQuote.xlsx';
            const cd = res.headers.get('content-disposition');
            if (cd) {
                const m = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(cd);
                if (m && m[1]) fileName = m[1].replace(/['"]/g, '').trim();
            }
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (err) {
            console.error('Error exporting history', err);
            showDialog(T.Notification || 'Thông báo', `<div class="text-danger">${err.message}</div>`);
        } finally {
            hideLoading();
        }
    });
    btnImportHistory?.addEventListener('click', async () => {
        const fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.accept = '.xlsx, .xls';
        fileInput.style.display = 'none';
        document.body.appendChild(fileInput);

        fileInput.addEventListener('change', async function () {
            const file = fileInput.files[0];
            if (!file) return;
            const T = window.i18nHistoryQuote || {};

            const allowedTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
            if (!allowedTypes.includes(file.type)) {
                showDialog({ title: T.Notification || 'Thông báo', message: (T.InvalidFileType || 'Loại file không hợp lệ'), type: 'error' });
                document.body.removeChild(fileInput);
                return;
            }

            const formData = new FormData();
            formData.append('file', file);

            try {
                showLoading((window.i18nHistoryQuote && window.i18nHistoryQuote.LoadingData) || 'Đang xử lý...');
            } catch (e) { }

            try {
                const response = await fetch((window.apiBaseUrl || '') + '/History/ImportFileExcelEditHistory', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    const errorText = await response.text();
                    throw new Error(errorText || 'Lỗi server');
                }

                const importResult = await response.json();

                // Import thành công, hiển thị dialog và CHỜ người dùng chọn
                const step = 2;
                const section = importResult?.sectionCode || '';
                if (!importResult?.isReturn) {
                    showDialog({
                        title: T.Notification || 'Thông báo',
                        message: (T.DataUpdatedSuccessfully || 'Cập nhật người phê duyệt thành công'),
                        type: 'success'
                    });
                    return;
                }

                const selected = await openApproverSelector(step, section);

                if (!selected) {
                    showDialog({
                        title: T.Notification || 'Thông báo',
                        message: (T.ImportSuccessButNoApprover || 'Nhập file thành công, nhưng chưa chọn người phê duyệt'),
                        type: 'warning'
                    });
                    return;
                }

                const approverId = selected.CHR_UserAdid ?? selected.chR_UserAdid ?? selected.CHR_Adid ?? selected.chR_Adid ?? selected.ADID ?? selected.Id ?? selected.id ?? selected.value ?? '';
                const finalId = approverId || (selected.value || selected.Value || '');

                if (!finalId) {
                    showDialog({
                        title: T.Notification || 'Thông báo',
                        message: (T.InvalidApprover || 'Người phê duyệt không hợp lệ'),
                        type: 'error'
                    });
                    return;
                }

                var payload = {
                    listUpdate: importResult?.listUpdate,
                    sectionCode: finalId
                };

                // Gọi API lưu người phê duyệt
                const updateResponse = await fetch((window.apiBaseUrl || '') + '/History/UpdateUserApprovalHistory', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(payload)
                });

                if (!updateResponse.ok) {
                    const errorText = await updateResponse.text();
                    throw new Error(errorText || 'Lỗi server khi cập nhật người phê duyệt');
                }

                const updateResult = await updateResponse.json();

                showDialog({
                    title: T.Notification || 'Thông báo',
                    message: (T.DataUpdatedSuccessfully || 'Cập nhật người phê duyệt thành công'),
                    type: 'success'
                });

            } catch (error) {
                const T = window.i18nHistoryQuote || {};
                showDialog({
                    title: T.Notification || 'Thông báo',
                    message: (error && error.message) ? error.message : (T.ErrorPrefix || 'Không thể import file'),
                    type: 'error'
                });
            } finally {
                try { hideLoading(); } catch (e) { }
                document.body.removeChild(fileInput);
            }
        });


        fileInput.click();
    });
    tblBody?.addEventListener('click', async (e) => {
        const t = e.target.closest('button');
        if (!t) return;
        // handle item-level history buttons inside detail rows (delegated)
        if (t.dataset.action === 'view-history') {
            const id = Number(t.dataset.id);
            if (!id) return;
            fetch((window.apiBaseUrl || '') + '/History/GetHistoryDataByID', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(id)
            })
                .then(r => { const T = window.i18nHistoryQuote || {}; if (!r.ok) throw new Error(T.MsgLoadHistoryFailed || 'Load history failed'); return r.json(); })
                .then(data => { const T = window.i18nHistoryQuote || {}; showDialog(T.PageTitleHistory || 'Lịch sử đơn', buildHistoryHtml(data)); })
                .catch(err => { console.error(err); const T = window.i18nHistoryQuote || {}; showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (T.MsgLoadHistoryFailed || 'Không tải được lịch sử.') + '</div>'); });
            return;
        }
        const row = e.target.closest('tr');
        // eidt item-level
        if (t.dataset.action === 'edit-history') {
            const id = Number(t.dataset.id);
            if (id) openEditModal(id);
        }
        // delete single history item
        if (t.dataset.action === 'delete-history') {
            const id = Number(t.dataset.id);
            const T = window.i18nHistoryQuote || {};
            if (!id) return;

            const reason = await showDeleteReasonModal();
            if (!reason) return;

            try {
                showLoading(T.Deleting || 'Đang xóa...');
                const payloadId = { id: id, reason: reason };
                const res = await fetch((window.apiBaseUrl || '') + '/History/DeleteDanhSachBaoGiaByID', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payloadId)
                });
                const text = await res.text();
                if (!res.ok) throw new Error(text || (T.DeleteFailed || 'Xóa thất bại'));
                showDialog(T.Notification || 'Thông báo', '<div class="text-success">' + (T.DeleteSuccess || 'Đã xóa thành công.') + '</div>');
                applyFilters();
            } catch (err) {
                console.error('Delete error', err);
                showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (err && err.message ? err.message : (T.DeleteFailed || 'Xóa thất bại')) + '</div>');
            } finally {
                hideLoading();
            }
            return;
        }
        if (t.classList.contains('btn-toggle-group')) {
            const groupId = row?.dataset.groupId;
            const detailRow = tblBody.querySelector(`tr.group-detail[data-group-id="${groupId}"]`);
            if (!detailRow) return;
            const icon = t.querySelector('i');
            const hidden = detailRow.hasAttribute('hidden');
            if (hidden) {
                detailRow.removeAttribute('hidden');
                icon?.classList.remove('fa-plus-square');
                icon?.classList.add('fa-minus-square');
                const group = currentGroups.find(g => g.groupId === groupId);
                (async function () {
                    try {
                        showLoading();
                        const maDonValue = group?.code || groupId;
                        const res = await fetch((window.apiBaseUrl || '') + '/History/GetByMaBaoGia', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(maDonValue)
                        });
                        if (!res.ok) {
                            // fallback to client-side items
                            throw new Error('Failed to load details');
                        }
                        const data = await res.json();
                        const items = Array.isArray(data) ? data : (data?.data || data?.Data || []);
                        if (items && items.length) {
                            renderChildOrders(detailRow, items);
                        } else if (group) {
                            renderChildOrders(detailRow, group.items);
                        }
                    } catch (e) {
                        console.warn('GetByMaBaoGiaAsync failed, using cached items', e);
                        if (group) renderChildOrders(detailRow, group.items);
                    } finally {
                        try { hideLoading(); } catch (e) { }
                    }
                })();
            } else {
                detailRow.setAttribute('hidden', '');
                icon?.classList.remove('fa-minus-square');
                icon?.classList.add('fa-plus-square');
            }
        }
        // Trả lại đơn 
        if (t.classList.contains('btn-view-return')) {
            const madon = row?.dataset.groupId;
            const T = window.i18nHistoryQuote || {};
            if (!madon) {
                showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (T.MsgSelectGroupFailed || 'Vui lòng chọn mã đơn!') + '</div>');
                return;
            }

            // Ask user to provide a reason for returning the quotation
            const reason = await showReturnReasonModal();
            if (!reason) return; // user cancelled or did not provide reason

            try {
                showLoading(T.Exporting || 'Đang xử lý...');

                const payload = { maDon: madon, reason: reason };
                const res = await fetch((window.apiBaseUrl || '') + '/History/ReturnQuotation', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                const text = await res.text();
                if (!res.ok) throw new Error(text || (T.ReturnFailed || 'Trả lại thất bại!'));
                showDialog(T.Notification || 'Thông báo', '<div class="text-success">' + (T.ReturnSuccess || 'Trả về thành công!') + '</div>');
                applyFilters();
            } catch (err) {
                showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (err && err.message ? err.message : (T.ReturnFailed || 'Trả lại thất bại!')) + '</div>');
            } finally {
                hideLoading();
            }
            return;
        }
        if (t.classList.contains('btn-view-history')) {
            const groupId = row?.dataset.groupId;
            const group = currentGroups.find(g => g.groupId === groupId);
            const soDon = group?.code || groupId;
            if (!soDon) return;
            // View history for the whole group (by SoDon)
            fetch((window.apiBaseUrl || '') + '/History/GetHistoryDataBySoDon', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(soDon)
            })
                .then(r => {
                    const T = window.i18nHistoryQuote || {};
                    if (!r.ok) throw new Error(T.MsgLoadHistoryFailed || 'Load history failed');
                    return r.json();
                })
                .then(data => {
                    const T = window.i18nHistoryQuote || {};
                    showDialog(T.PageTitleHistory || 'Lịch sử đơn', buildHistoryHtml(data));
                })
                .catch(err => {
                    console.error(err);
                    const T = window.i18nHistoryQuote || {};
                    showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (T.MsgLoadHistoryFailed || 'Không tải được lịch sử.') + '</div>');
                });
        }
        if (t.classList.contains('btn-view-approvals')) {
            const groupId = row?.dataset.groupId;
            document.dispatchEvent(new CustomEvent('quote-history:viewApprovals', { detail: { groupId } }));
        }
        // delete whole group (by MaDon)
        if (t.classList.contains('btn-delete-group')) {
            const groupId = row?.dataset.groupId;
            const T = window.i18nHistoryQuote || {};
            if (!groupId) return;
            const reason = await showDeleteReasonModal();
            if (!reason) return;

            try {
                showLoading(T.Deleting || 'Đang xóa...');

                const payloadGroup = { maDon: groupId, reason: reason };
                const resGroup = await fetch((window.apiBaseUrl || '') + '/History/DeleteDanhSachBaoGiaByMaDon', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payloadGroup)
                });
                const textGroup = await resGroup.text();
                if (!resGroup.ok) throw new Error(textGroup || (T.DeleteFailed || 'Xóa thất bại'));

                showDialog(T.Notification || 'Thông báo', '<div class="text-success">' + (T.DeleteSuccess || 'Đã xóa thành công.') + '</div>');
                applyFilters();
            } catch (err) {
                console.error('Delete group error', err);
                showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (err && err.message ? err.message : (T.DeleteFailed || 'Xóa thất bại')) + '</div>');
            } finally {
                hideLoading();
            }
            return;
        }
    });

    // Open approver selection modal, fetch approvers and on confirm call saveTab2 with approver
    function openApproverSelector(stepNumber, sectionCode) {
        // follow the same pattern used in Approval_Quote.js: return a Promise resolving to selected approver object or null
        return new Promise(async (resolve, reject) => {
            try {
                const modal = document.getElementById('selectApproverModal');
                const sel = document.getElementById('selectNextApprover');
                const notice = document.getElementById('selectApproverNotice');
                if (!modal || !sel) return resolve(null);
                // clear
                sel.innerHTML = '';
                const placeholderOpt = document.createElement('option');
                placeholderOpt.value = '';
                placeholderOpt.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.SelectPlaceholder) || '-- Chọn --';
                sel.appendChild(placeholderOpt);

                // fetch approvers from Quote controller
                const body = { Step: stepNumber, SectionCost: sectionCode };
                let list = [];
                try {
                    const resp = await fetch((window.apiBaseUrl || '') + '/History/GetListApprovel', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                        body: JSON.stringify(body)
                    });
                    if (resp.ok) {
                        const data = await resp.json();
                        // controller returns data (could be array or wrapper)
                        list = Array.isArray(data) ? data : (data && data.data ? data.data : []);
                    }
                } catch (e) { console.warn('Failed to load approvers', e); }

                if (!list || !list.length) {
                    const emptyOpt = document.createElement('option');
                    emptyOpt.value = '';
                    emptyOpt.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.NoResults) || 'Không có kết quả';
                    sel.appendChild(emptyOpt);
                } else {
                    list.forEach(item => {
                        const o = document.createElement('option');
                        // normalize likely server keys
                        const adid = item.chR_UserAdid || item.CHR_UserAdid || item.ADID || item.Id || item.id || '';
                        const name = item.nvchR_UserName || item.NVCHR_UserName || item.Name || item.FullName || item.nvchR_FullName || '';
                        o.value = adid || '';
                        o.textContent = (name ? (name + (adid ? (' (' + adid + ')') : '')) : (adid || ''));
                        try { o.dataset.raw = JSON.stringify(item); } catch { }
                        sel.appendChild(o);
                    });
                }

                // ensure modal attached to body
                try { if (modal.parentElement !== document.body) document.body.appendChild(modal); } catch (e) { }
                // show modal
                try {
                    if (window.bootstrap && bootstrap.Modal) {
                        const bsModal = new bootstrap.Modal(modal, { backdrop: 'static' });
                        modal._bsModal = bsModal;
                        bsModal.show();
                        setTimeout(() => {
                            try { const createdBackdrop = document.querySelector('.modal-backdrop'); if (createdBackdrop) createdBackdrop.style.zIndex = '10550'; modal.style.zIndex = '10600'; } catch (e) { }
                        }, 10);
                    } else {
                        const backdrop = document.createElement('div');
                        backdrop.className = 'modal-backdrop show custom-modal-backdrop';
                        backdrop.style.zIndex = '10550';
                        document.body.appendChild(backdrop);
                        modal._backdrop = backdrop;
                        modal.style.zIndex = '10600';
                        modal.style.display = 'block';
                        modal.classList.add('show');
                    }
                } catch (e) { modal.style.display = 'block'; modal.classList.add('show'); }

                const confirmBtn = document.getElementById('confirmSelectApprover');
                function cleanup() {
                    try { if (modal._bsModal) modal._bsModal.hide(); else { modal.style.display = 'none'; modal.classList.remove('show'); } } catch (e) { try { modal.style.display = 'none'; modal.classList.remove('show'); } catch { } }
                    try { if (modal._backdrop) { document.body.removeChild(modal._backdrop); delete modal._backdrop; } } catch (e) { }
                    try { confirmBtn.removeEventListener('click', onConfirm); } catch (e) { }
                    try { modal.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.removeEventListener('click', onCancel)); } catch (e) { }
                    if (notice) notice.style.display = 'none';
                    try { modal.style.zIndex = ''; } catch (e) { }
                }
                function onConfirm(e) {
                    e && e.preventDefault();
                    const value = sel.value;
                    if (!value) {
                        if (notice) notice.style.display = '';
                        return;
                    }
                    const raw = sel.selectedOptions && sel.selectedOptions[0] && sel.selectedOptions[0].dataset.raw;
                    let obj = null;
                    try { obj = raw ? JSON.parse(raw) : { CHR_UserAdid: value, NVCHR_UserName: sel.selectedOptions[0].textContent }; } catch { obj = { CHR_UserAdid: value, NVCHR_UserName: sel.selectedOptions[0].textContent }; }
                    cleanup();
                    resolve(obj);
                }
                function onCancel() { cleanup(); resolve(null); }
                if (confirmBtn) confirmBtn.addEventListener('click', onConfirm);
                try { modal.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.addEventListener('click', onCancel)); } catch (e) { }
            } catch (err) { reject(err); }
        });
    }

    function initEnhancements(root) {
        try {
            if (window.KanziSearchableDropdown && typeof window.KanziSearchableDropdown.init === 'function') {
                window.KanziSearchableDropdown.init(root || document);
            } else {
                buildSearchableDropdown(root || document);
            }
        } catch (e) {

        }

    }
    document.addEventListener('DOMContentLoaded', function () { initEnhancements(); });
    function groupByMaDon(rows) {
        if (!rows || !rows.length) return [];

        return rows.map(row => ({
            groupId: row.CHR_MaDon,
            code: row.CHR_MaDon,
            requester: row.CHR_CreateBy,
            created: toDateString(row.DTM_CreateDate),
            status: row.ID_Status,
            count: row.TongSoDon,               // tổng số đơn trong nhóm
            suppliersSent: row.SupperlierSened, // số nhà cung cấp đã gửi báo giá
            suppliersTotal: row.SupperlierSum,  // tổng số nhà cung cấp
            suppliers: row.Suppliers,       
        }));
    }
    function toDateString(dt) {
        try {
            if (!dt) return '';
            const d = new Date(dt);
            if (isNaN(d.getTime())) return '';
            return d.toLocaleDateString();
        } catch { return ''; }
    }

    function renderGroups() {
        if (!tblBody) return;
        tblBody.innerHTML = '';
        const T = window.i18nHistoryQuote || {};

        let total, totalPages, pageGroups, start;
        if (serverPaged) {
            total = totalCountServer || 0;
            totalPages = Math.max(1, Math.ceil(total / pageSize));
            if (currentPage > totalPages) currentPage = totalPages;
            start = (currentPage - 1) * pageSize;
            pageGroups = currentGroups;
        } else {
            total = currentGroups.length;
            totalPages = Math.max(1, Math.ceil(total / pageSize));
            if (currentPage > totalPages) currentPage = totalPages;
            start = (currentPage - 1) * pageSize;
            pageGroups = currentGroups.slice(start, start + pageSize);
        }

        pageGroups.forEach((g, idx) => {
            // Try cloning existing template row; if not present, build from scratch
            const templateRow = document.querySelector('#historyGroupTableBody tr.group-row');
            let tmpl;
            if (templateRow) {
                tmpl = templateRow.cloneNode(true);
                tmpl.removeAttribute('hidden');
            } else {
                tmpl = document.createElement('tr');
                tmpl.className = 'group-row';
                // show return button only for specific role (client-side)
                const returnBtnHtml = (role === 'UserPUR')
                    ? '<button type="button" class="btn btn-outline-secondary btn-view-return" title="Trả lại"><i class="fas ion-arrow-return-left"></i></button>'
                    : '';
                tmpl.innerHTML = `
                    <td class="text-center"><button type="button" class="btn btn-sm btn-link text-primary px-0 btn-toggle-group" title="Mở rộng"><i class="fas fa-plus-square"></i></button></td>
                    <td class="fw-semibold group-code"></td>
                    <td class="group-requester"></td>
                    <td class="group-created"></td>
                    <td class="group-status"></td>
                    <td class="group-count text-end"></td>
                    <td class="group-suppliers"></td>
                    <td class="text-center">
                        <div class="btn-group btn-group-sm" role="group">
                            <button type="button" class="btn btn-outline-info btn-view-history" title="Xem lịch sử"><i class="fas fa-history"></i></button>
                            ${returnBtnHtml}
                            <button type="button" class="btn btn-outline-danger btn-delete-group" title="Xóa đơn"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>
                `;
            }
            // <button type="button" class="btn btn-outline-primary btn-edit-uncompleted" title="S?a don tr? l?i"><i class="fas fa-edit"></i></button>
            tmpl.dataset.groupId = g.groupId;
            const cells = tmpl.querySelectorAll('td');
            const codeEl = tmpl.querySelector('.group-code'); if (codeEl) codeEl.textContent = g.code;
            const reqEl = tmpl.querySelector('.group-requester'); if (reqEl) reqEl.textContent = g.requester;
            const createdEl = tmpl.querySelector('.group-created'); if (createdEl) createdEl.textContent = g.created;
            const statusEl = tmpl.querySelector('.group-status'); if (statusEl) {
                if (g.status === 'WAIT_NCC' || g.status === 'WAIT_PICK_NCC') {
                    statusEl.textContent = StatusText(g.status) + ` ( ${T.Confirm} ${g.suppliersSent} / ${g.suppliersTotal} ${T.SenedSupperlier})`;
                } else {
                    statusEl.textContent = StatusText(g.status)
                }
            }
            const countEl = tmpl.querySelector('.group-count'); if (countEl) countEl.textContent = g.count;
            const suppEl = tmpl.querySelector('.group-suppliers'); if (suppEl) suppEl.textContent = g.suppliers;
            // index
            const idxCell = cells[0];
            if (idxCell) idxCell.setAttribute('data-idx', String(start + idx + 1));
            tblBody.appendChild(tmpl);

            // detail row
            const templateDetail = document.querySelector('#historyGroupTableBody tr.group-detail');
            let detailTmpl;
            if (templateDetail) {
                detailTmpl = templateDetail.cloneNode(true);
                detailTmpl.removeAttribute('hidden');
            } else {
                detailTmpl = document.createElement('tr');
                detailTmpl.className = 'group-detail';
                detailTmpl.innerHTML = `
                    <td colspan="8" class="p-0">
                        <div class="p-2">
                            <div class="table-responsive">
                                <table class="table table-sm table-bordered mb-0">
                                    <thead class="table-light">
                                        <tr class="text-center align-middle">
                                            <th style="width: 40px">No</th>
                                            <th style="min-width: 160px">${T.OrderCode}</th>
                                            <th style="min-width: 180px">${T.Department}</th>
                                            <th style="min-width: 200px">${T.Material}</th>
                                            <th style="min-width: 160px">${T.Supplier}</th>
                                            <th style="min-width: 140px">${T.Quantity}</th>
                                            <th style="min-width: 140px">${T.Status}</th>
                                            <th style="min-width: 160px">${T.UpdatedDate}</th>
                                            <th style="min-width: 200px">${T.Actions}</th>
                                        </tr>
                                    </thead>
                                    <tbody class="group-detail-body"></tbody>
                                </table>
                            </div>
                        </div>
                    </td>
                `;
            }
            detailTmpl.dataset.groupId = g.groupId;
            detailTmpl.setAttribute('hidden', '');
            tblBody.appendChild(detailTmpl);
        });

        // update pagination UI
        if (paginationInfoEl) {
            const showingFrom = total === 0 ? 0 : start + 1;
            const showingTo = Math.min(total, start + pageSize);
            const T = window.i18nHistoryQuote || {};
            const tpl = (T.PaginationInfo || 'Hiển thị {0} - {1} / {2} nhóm');
            paginationInfoEl.textContent = tpl.replace('{0}', showingFrom).replace('{1}', showingTo).replace('{2}', total);
        }
        renderPagination(totalPages);
    }

    function renderEmpty() {
        if (!tblBody) return;
        tblBody.innerHTML = '';
        renderPagination(1);
        if (paginationInfoEl) {
            const T = window.i18nHistoryQuote || {};
            const tpl = (T.PaginationInfo || 'Hiển thị {0} - {1} / {2} nhóm');
            paginationInfoEl.textContent = tpl.replace('{0}', 0).replace('{1}', 0).replace('{2}', 0);
        }
    }

    function renderPagination(totalPages) {
        if (!paginationEl) return;
        const ul = paginationEl;
        ul.innerHTML = '';
        const prev = document.createElement('li'); prev.className = 'page-item' + (currentPage === 1 ? ' disabled' : '');
        const prevBtn = document.createElement('button'); prevBtn.className = 'page-link'; prevBtn.textContent = '«'; prevBtn.dataset.page = 'prev';
        prev.appendChild(prevBtn); ul.appendChild(prev);
        for (let i = 1; i <= totalPages; i++) {
            const li = document.createElement('li'); li.className = 'page-item' + (i === currentPage ? ' active' : '');
            const btn = document.createElement('button'); btn.className = 'page-link'; btn.textContent = String(i); btn.dataset.page = String(i);
            li.appendChild(btn); ul.appendChild(li);
        }
        const next = document.createElement('li'); next.className = 'page-item' + (currentPage === totalPages ? ' disabled' : '');
        const nextBtn = document.createElement('button'); nextBtn.className = 'page-link'; nextBtn.textContent = '»'; nextBtn.dataset.page = 'next';
        next.appendChild(nextBtn); ul.appendChild(next);
    }

    paginationEl?.addEventListener('click', function (e) {
        const btn = e.target.closest('button');
        if (!btn) return;
        const val = btn.dataset.page;
        const totalPages = serverPaged ? Math.max(1, Math.ceil((totalCountServer || 0) / pageSize)) : Math.max(1, Math.ceil(currentGroups.length / pageSize));
        if (val === 'prev' && currentPage > 1) currentPage--;
        else if (val === 'next' && currentPage < totalPages) currentPage++;
        else if (!isNaN(Number(val))) currentPage = Number(val);

        if (serverPaged) applyFilters(currentPage); else renderGroups();
    });

    function renderChildOrders(detailRow, items) {
        const body = detailRow.querySelector('.group-detail-body');
        if (!body) return;
        body.innerHTML = '';
        items.forEach((it, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="text-center">${idx + 1}</td>
                <td>${it.chR_MaDon || ''}</td>
                 <td>${it.chR_SectionName || ''}</td>
                <td>${(it.chR_MaHangNoiBo || '')} ${(it.nvchR_NameVN ? ('- ' + it.nvchR_NameVN) : '')}</td>
                <td>${(it.chR_MaNCC || '')}</td>
                <td class="text-end">${it.inT_SoLuong ?? ''}</td>
                <td>${StatusText(it.iD_Status) || ''}</td>
                <td>${toDateString(it.dtM_CreateDate) || ''}</td>
                <td class="text-center">
                    <div class="btn-group btn-group-sm" role="group">
                        <button type="button" class="btn btn-outline-info" data-action="view-history" data-id="${it.id}"><i class="fas fa-history"></i></button>
                        <button type="button" class="btn btn-outline-primary" data-action="edit-history" data-id="${it.id}"><i class="fas fa-edit"></i></button>
                        <button type="button" class="btn btn-outline-danger" data-action="delete-history" data-id="${it.id}"><i class="fas fa-trash"></i></button>
                    </div>
                </td>
            `;
            body.appendChild(tr);
        });
    }

    // Open edit modal using latest history CHR_NewData
    function openEditModal(requestId) {
        fetch((window.apiBaseUrl || '') + '/History/SearchID', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(requestId))
        })
            .then(r => { const T = window.i18nHistoryQuote || {}; if (!r.ok) throw new Error(T.MsgLoadHistoryFailed || 'Load history failed'); return r.json(); })
            .then(result => {
                const data = result?.data || result || [];
                if (!data) {
                    const T = window.i18nHistoryQuote || {};
                    showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (T.MsgNoDataToEdit || 'Không có dữ liệu để chỉnh sửa.') + '</div>');
                    return;
                }
                fillEditFormFromDto(data);
                showEditModal();
            })
            .catch(err => {
                console.error(err);
                const T = window.i18nHistoryQuote || {};
                showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (T.MsgNotFoundData || 'Không tìm thấy dữ liệu.') + '</div>');
            });
    }

    function fillEditFormFromDto(dto) {
        // universal setter for inputs and selects; for selects ensure option exists
        function setControlValue(id, val, textForOption) {
            const el = document.getElementById(id);
            if (!el) return;
            try {
                if (el.tagName === 'SELECT') {
                    const strVal = val == null ? '' : String(val);
                    const exists = Array.from(el.options).some(o => String(o.value) === strVal);
                    if (!exists && strVal !== '') {
                        const opt = document.createElement('option');
                        opt.value = strVal;
                        opt.text = textForOption ?? strVal;
                        el.appendChild(opt);
                    }
                    el.value = strVal;
                    try { el.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
                } else {
                    el.value = val ?? '';
                }
            } catch (e) { try { el.value = val ?? ''; } catch { } }
        }

        const toDateInput = (d) => { try { if (!d) return ''; const dt = new Date(d); return dt.toISOString().slice(0, 10); } catch { return ''; } };

        document.getElementById('editRequestId')?.setAttribute('value', dto.ID ?? dto.id ?? '');
        setControlValue('editMaDon', dto.chR_MaDon || dto.CHR_MaDon || '');
        setControlValue('editRequester', dto.chR_CreateBy || dto.CHR_CreateBy || '');
        // Section uses code as option value
        setControlValue('editSectionCode', dto.chR_SectionCode || dto.CHR_SectionCode || '');
        setControlValue('editSectionName', dto.chR_SectionName || dto.CHR_SectionName || '');

        setControlValue('editChungLoai', dto.nvchR_ChungLoai || dto.NVCHR_ChungLoai || '');
        setControlValue('editPhanLoai', dto.chR_Phanloai || dto.CHR_Phanloai || '');
        setControlValue('editMaThietBi', dto.chR_MaThietBi || dto.CHR_MaThietBi || '');
        setControlValue('editMaHangNoiBo', dto.chR_MaHangNoiBo || dto.CHR_MaHangNoiBo || '');
        setControlValue('editMaHangNCC', dto.chR_MaHangNCC || dto.CHR_MaHangNCC || '');
        setControlValue('editTenHangVN', dto.nvchR_NameVN || dto.NVCHR_NameVN || '');
        setControlValue('editTenHangEN', dto.chR_NameEN || dto.CHR_NameEN || '');
        setControlValue('editSoLuong', dto.inT_SoLuong ?? '');
        setControlValue('editDonVi', dto.nvchR_DonVi || dto.NVCHR_DonVi || '');
        setControlValue('editHinhDang', dto.nvchR_HinhDang || dto.NVCHR_HinhDang || '');
        setControlValue('editChatLieu', dto.nvchR_ChatLieu || dto.NVCHR_ChatLieu || '');
        setControlValue('editThanhPhan', dto.nvchR_ThanhPhan || dto.NVCHR_ThanhPhan || '');
        setControlValue('editKichThuoc', dto.nvchR_KichThuoc || dto.NVCHR_KichThuoc || '');
        setControlValue('editDongMay', dto.nvchR_DongMay || dto.NVCHR_DongMay || '');
        setControlValue('editTinhNang', dto.nvchR_TinhNang || dto.NVCHR_TinhNang || '');
        setControlValue('editRohs', dto.nvchR_Rohs || dto.NVCHR_Rohs || '');
        setControlValue('editCOCQ', dto.nvchR_COCQ || dto.NVCHR_COCQ || '');
        setControlValue('editMSDS', dto.nvchR_MSDS || dto.NVCHR_MSDS || '');
        setControlValue('editAnToan', dto.nvchR_AnToan || dto.NVCHR_AnToan || '');
        setControlValue('editFileThietKe', dto.nvchR_FileThietKe || dto.NVCHR_FileThietKe || '');
        setControlValue('editNhaSanXuat', dto.nvchR_NhaSanXuat || dto.NVCHR_NhaSanXuat || '');
        setControlValue('editNhaCungCap', dto.chR_MaNCC || dto.CHR_MaNCC || '');
        setControlValue('editTenNCC', dto.nvchR_TenNCC || dto.NVCHR_TenNCC || '');
        setControlValue('editStatus', dto.iD_Status || dto.ID_Status || '');
        setControlValue('editStep', dto.iD_StepBaoGia || dto.ID_StepBaoGia || '');
        setControlValue('editSoLanUpdate', dto.inT_SoLanUpdate ?? '');

        setControlValue('editLayBaoGia', (dto.biT_LayBaoGia) ? 'true' : 'false');
        setControlValue('editLyDo', dto.nvchR_LyDo || dto.NVCHR_LyDo || '');
        setControlValue('editNgayMuonNhan', toDateInput(dto.dtM_NgayMuonNhan));
        setControlValue('editKyHan', toDateInput(dto.dtM_KyHan));
        setControlValue('editGap', (dto.chR_Gap) ?? 'false');
        setControlValue('editDaycreate', toDateInput(dto.dtM_CreateDate) || '');
        setControlValue('editUpdateLater', toDateInput(dto.dtM_UpdateLater) || '');
        setControlValue('editDeadline', toDateInput(dto.dtM_Deadline) || '');
        setControlValue('editIsTemplate', (dto.biT_IsTemplate === true) ? 'true' : (dto.biT_IsTemplate === false) ? 'false' : '');

        // enhance selects if needed
        try { if (window.jQuery) buildSearchableDropdown($(document)); else buildSearchableDropdown(document); } catch { }
    }

    function showEditModal() {
        const modalEl = document.getElementById('editHistoryModal');
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

    function hideEditModal() {
        const modalEl = document.getElementById('editHistoryModal');
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
        hideEditModal();
    });
    document.getElementById('btnCloseEdit_2')?.addEventListener('click', function () {
        hideEditModal();
    });
    // Save handler: submit a single DTO update
    document.getElementById('btnSaveHistoryEdit')?.addEventListener('click', function () {
        const dto = collectEditFormDto();
        if (!dto) return;
        // UpdateBaoGiaById expects a list
        fetch((window.apiBaseUrl || '') + '/History/UpdateBaoGiaById', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        })
            .then(async r => {
                const txt = await r.text();
                const T = window.i18nHistoryQuote || {};
                if (!r.ok) throw new Error(txt || (T.MsgSaveFailed || 'Lưu thất bại'));
                hideEditModal();
                const T2 = window.i18nHistoryQuote || {};
                showDialog(T2.Notification || 'Thông báo', '<div class="text-success">' + (T2.MsgSaveSuccess || 'Đã lưu thành công.') + '</div>');
                applyFilters();
            })
            .catch(err => {
                hideEditModal();
                console.error(err);
                const T = window.i18nHistoryQuote || {};
                showDialog(T.Notification || 'Thông báo', `<div class="text-danger">${err.message}</div>`);

            });
    });

    function collectEditFormDto() {
        const gv = id => document.getElementById(id)?.value || '';
        const toIso = d => { if (!d) return null; try { const parts = d.split('-'); return new Date(Date.UTC(+parts[0], +parts[1] - 1, +parts[2], 7, 0, 0)).toISOString(); } catch { return null; } };
        return {
            ID: Number(document.getElementById('editRequestId')?.getAttribute('value') || 0),
            CHR_MaDon: gv('editMaDon') || null,
            CHR_SectionCode: gv('editSectionCode') || null,
            CHR_SectionName: gv('editSectionName') || null,
            CHR_Phanloai: gv('editPhanLoai') || null,
            CHR_MaThietBi: gv('editMaThietBi') || null,
            CHR_MaHangNoiBo: gv('editMaHangNoiBo') || null,
            CHR_MaHangNCC: gv('editMaHangNCC') || null,
            NVCHR_NameVN: gv('editTenHangVN') || null,
            CHR_NameEN: gv('editTenHangEN') || null,
            INT_SoLuong: gv('editSoLuong') ? parseFloat(gv('editSoLuong')) : null,
            NVCHR_DonVi: gv('editDonVi') || null,
            NVCHR_ChungLoai: gv('editChungLoai') || null,
            NVCHR_HinhDang: gv('editHinhDang') || null,
            NVCHR_ChatLieu: gv('editChatLieu') || null,
            NVCHR_ThanhPhan: gv('editThanhPhan') || null,
            NVCHR_KichThuoc: gv('editKichThuoc') || null,
            NVCHR_DongMay: gv('editDongMay') || null,
            NVCHR_TinhNang: gv('editTinhNang') || null,
            NVCHR_Rohs: gv('editRohs') || null,
            NVCHR_COCQ: gv('editCOCQ') || null,
            NVCHR_MSDS: gv('editMSDS') || null,
            NVCHR_AnToan: gv('editAnToan') || null,
            NVCHR_FileThietKe: gv('editFileThietKe') || null,
            NVCHR_NhaSanXuat: gv('editNhaSanXuat') || null,
            CHR_MaNCC: gv('editNhaCungCap') || null,
            NVCHR_TenNCC: gv('editTenNCC') || null,
            BIT_LayBaoGia: (gv('editLayBaoGia') === 'true'),
            NVCHR_LyDo: gv('editLyDo') || null,
            DTM_NgayMuonNhan: toIso(gv('editNgayMuonNhan')),
            DTM_KyHan: toIso(gv('editKyHan')),
            CHR_Gap: gv('editGap') || null,
            CHR_CreateBy: gv('editRequester') || '',
            DTM_CreateDate: toIso(gv('editDaycreate')),
            ID_Status: gv('editStatus'),
            ID_StepBaoGia: gv('editStep'),
            INT_SoLanUpdate: gv('editSoLanUpdate') ? parseInt(gv('editSoLanUpdate')) + 1 : 1,
            DTM_UpdateLater: toIso(gv('editUpdateLater')),
            DTM_Deadline: toIso(gv('editDeadline')),
            BIT_IsTemplate: gv('editIsTemplate') ? (gv('editIsTemplate') === 'true') : null
        };
    }

    function showDialog(title, html) {
        const overlay = document.getElementById('cmDialogOverlay');
        const body = document.getElementById('cmDialogBody');
        const footer = document.getElementById('cmDialogFooter');
        const titleEl = document.getElementById('cmDialogTitle');

        if (!overlay || !body || !footer || !titleEl) {
            if (typeof title === 'object' && title !== null) {
                alert((title.title || 'Thông báo') + ': ' + (title.message || ''));
            } else {
                alert((title || 'Thông báo') + ': ' + (html || ''));
            }
            return;
        }

        const T = window.i18nHistoryQuote || {};

        // Handle both object parameter and separate title/html parameters
        let dialogTitle, dialogContent, dialogType;
        if (typeof title === 'object' && title !== null) {
            dialogTitle = title.title || (T.Notification || 'Thông báo');
            dialogContent = title.message || '';
            dialogType = title.type || '';
        } else {
            dialogTitle = title || (T.Notification || 'Thông báo');
            dialogContent = html || '';
            dialogType = '';
        }

        titleEl.textContent = dialogTitle;
        body.innerHTML = dialogContent;

        body.className = 'cm-dialog-body';
        if (dialogType === 'error') {
            body.className += ' text-danger';
        } else if (dialogType === 'success') {
            body.className += ' text-success';
        } else if (dialogType === 'warning') {
            body.className += ' text-warning';
        }

        footer.innerHTML = '<button type="button" class="cm-btn" data-cm-action="close">' + (T.Close || 'Đóng') + '</button>';

        // show overlay (CSS default is display:none)
        overlay.style.display = 'flex';
        overlay.setAttribute('aria-hidden', 'false');

        // Focus first focusable in dialog for accessibility
        try {
            const dlg = overlay.querySelector('.cm-dialog');
            const focusable = dlg && dlg.querySelector('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
            if (focusable && typeof focusable.focus === 'function') focusable.focus();
        } catch { }

        const doClose = () => {
            // If focus is within overlay, blur and move focus outside before hiding to avoid aria-hidden ancestor warnings
            try {
                const active = document.activeElement;
                if (active && overlay.contains(active)) {
                    if (typeof active.blur === 'function') active.blur();
                    const fallbackFocus = document.getElementById('btnApplyFilters') || document.body;
                    if (fallbackFocus && typeof fallbackFocus.focus === 'function') fallbackFocus.focus();
                }
            } catch { }
            overlay.setAttribute('aria-hidden', 'true');
            overlay.style.display = 'none';
        };

        if (overlay._closeHandler) overlay.removeEventListener('click', overlay._closeHandler);
        overlay._closeHandler = function (evt) {
            const target = evt.target.closest('[data-cm-action="close"], [data-cm-action="overlay"]');
            if (target) doClose();
        };
        overlay.addEventListener('click', overlay._closeHandler);
    }
    // lấy thông tin status hiển thị
    function StatusText(statusId) {
        var listStatus = window.HistoryData.status
        const status = listStatus.find(s => s.VCHR_CodeStatus === statusId);
        return status ? status.DisplayName : statusId;
    }
    function buildHistoryHtml(result) {
        const data = result?.data || result || [];
        const T = window.i18nHistoryQuote || {};
        if (!Array.isArray(data) || data.length === 0) return '<div>' + (T.MsgNoHistory || 'Không có lịch sử.') + '</div>';
        const rows = data.map((h, i) => {
            const dateStr = toDateString(h.chR_Updatedate);
            const action = StatusText(h.chR_ActionType) || '';
            const by = (h.chR_UpdateBy || '') + (h.nvchR_UpdateName ? (' - ' + h.nvchR_UpdateName) : '');
            const reason = h.nvchR_LyDo || '';
            const ID_RequestQuote = h.iD_RequestQuote;
            return `<tr>
                <td class="text-center">${i + 1}</td>
                <td>${dateStr}</td>
                <td>${ID_RequestQuote || ''}</td>
                <td>${action}</td>
                <td>${by}</td>
                <td>${reason}</td>
            </tr>`;
        }).join('');
        return `
            <div class="table-responsive">
                <table class="table table-sm table-bordered">
                    <thead class="table-light"><tr>
                        <th style="width:60px">#</th>
                        <th>${T.HistoryTime || 'Thời gian'}</th>
                        <th>${T.RequestNo || 'Số Request'}</th>
                        <th>${T.Action || 'Hành động'}</th>
                        <th>${T.UpdatedBy || 'Người cập nhật'}</th>
                        <th>${T.Reason || 'Lý do'}</th>
                    </tr></thead>
                    <tbody>${rows}</tbody>
                </table>
            </div>
        `;
    }
    // Loading overlay helpers
    function showLoading(message) {
        try {
            const el = document.getElementById('globalLoading');
            if (!el) return;
            const msgEl = el.querySelector('.loader-msg');
            if (msgEl && message) msgEl.textContent = message;
            el.style.display = 'flex';
            el.setAttribute('aria-hidden', 'false');
        } catch (e) { }
    }
    function hideLoading() {
        try {
            const el = document.getElementById('globalLoading');
            if (!el) return;
            el.style.display = 'none';
            el.setAttribute('aria-hidden', 'true');
            const msgEl = el.querySelector('.loader-msg');
            if (msgEl) msgEl.textContent = 'Đang xử lý...';
        } catch (e) { }
    }
    // Initial load
    document.addEventListener('DOMContentLoaded', function () {
        applyFilters(1);
    });
})();
