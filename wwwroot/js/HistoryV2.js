(function () {
    const tblBody = document.getElementById('historyGroupTableBody') || document.querySelector('.approval-table tbody');
    const statusFilter = document.getElementById('statusFilter');
    const btnApply = document.getElementById('btnApplyFilters');
    const btnReset = document.getElementById('btnResetFilters');
    const paginationEl = document.getElementById('historyPagination');
    const paginationInfoEl = document.getElementById('historyPaginationInfo');
    const pageSizeSelect = document.getElementById('historyPageSize');
    const btnExportHistory = document.getElementById('btnExportHistory');
    const btnImportHistory = document.getElementById('btnImportHistory');
    const btnExportManaHistory = document.getElementById('btnExportManaHistory');
    let currentPage = 1;
    let pageSize = Number(pageSizeSelect?.value) || 50;
    let currentGroups = [];
    let editOrderRows = [];
    let totalCountServer = 0;
    const role = window.HistoryData.role || 'User';

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

    // Tìm kiếm - support both jQuery and plain DOM
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

    btnExportManaHistory?.addEventListener('click', handleHistoryExportClick);
    async function handleHistoryExportClick(ev) {
        ev && ev.preventDefault();

        if ((btnExportManaHistory && btnExportManaHistory.disabled) || (btnExportHistory && btnExportHistory.disabled)) return;

        if (btnExportManaHistory) btnExportManaHistory.disabled = true;
        if (btnExportHistory) btnExportHistory.disabled = true;

        showLoading(window.i18nHistoryQuote?.Exporting || 'Đang xuất...');
        try {
            const payload = buildSearchPayload(1);
            const res = await fetch(apiUrl('/History/ExportHistoryExcel'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                const txt = await res.text().catch(() => null);
                throw new Error(txt || (window.i18nHistoryQuote?.MsgCannotExport || 'Không thể xuất file'));
            }

            const blob = await res.blob();
            let fileName = `HistoryQuote_${new Date().toISOString().replace(/[:.]/g, '')}.xlsx`;
            try {
                const cd = res.headers.get('content-disposition') || res.headers.get('Content-Disposition');
                if (cd) {
                    const m = /filename[^;=\\n]*=((['"]).*?\\2|[^;\\n]*)/.exec(cd);
                    if (m && m[1]) fileName = m[1].replace(/['"]/g, '').trim();
                }
            } catch (e) { }

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (err) {
            showDialog({ title: window.i18nHistoryQuote?.Notification || 'Thông báo', message: err?.message || String(err), type: 'error' });
        } finally {
            hideLoading();
            if (btnExportManaHistory) btnExportManaHistory.disabled = false;
            if (btnExportHistory) btnExportHistory.disabled = false;
        }
    }

    btnExportHistory?.addEventListener('click', handleHistoryExportManaClick);
    async function handleHistoryExportManaClick(ev) {
        ev && ev.preventDefault();

        if ((btnExportManaHistory && btnExportManaHistory.disabled) || (btnExportHistory && btnExportHistory.disabled)) return;

        if (btnExportManaHistory) btnExportManaHistory.disabled = true;
        if (btnExportHistory) btnExportHistory.disabled = true;

        showLoading(window.i18nHistoryQuote?.Exporting || 'Đang xuất...');
        try {
            const payload = buildSearchPayload(1);
            const res = await fetch(apiUrl('/History/ExportManagerHistoryIndex'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                const txt = await res.text().catch(() => null);
                throw new Error(txt || (window.i18nHistoryQuote?.MsgCannotExport || 'Không thể xuất file'));
            }

            const blob = await res.blob();
            let fileName = `HistoryQuote_${new Date().toISOString().replace(/[:.]/g, '')}.xlsx`;
            try {
                const cd = res.headers.get('content-disposition') || res.headers.get('Content-Disposition');
                if (cd) {
                    const m = /filename[^;=\\n]*=((['"]).*?\\2|[^;\\n]*)/.exec(cd);
                    if (m && m[1]) fileName = m[1].replace(/['"]/g, '').trim();
                }
            } catch (e) { }

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (err) {
            showDialog({ title: window.i18nHistoryQuote?.Notification || 'Thông báo', message: err?.message || String(err), type: 'error' });
        } finally {
            hideLoading();
            if (btnExportManaHistory) btnExportManaHistory.disabled = false;
            if (btnExportHistory) btnExportHistory.disabled = false;
        }
    }
    // Import history file
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

            try { showLoading((window.i18nHistoryQuote && window.i18nHistoryQuote.LoadingData) || 'Đang xử lý...'); } catch (e) { }

            try {
                const response = await fetch(apiUrl('/History/ImportFileExcelEditHistory'), {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    const errorText = await response.text().catch(() => null);
                    throw new Error(errorText || (T.ErrorPrefix || 'Lỗi server'));
                }

                const importResult = await response.json().catch(() => null);

                // 
                if (!importResult?.isReturn) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Cập nhật người phê duyệt thành công'), type: 'success' });
                    applyFilters(1);
                    return;
                }

                // 
                const step = 2;
                const section = importResult?.sectionCode || '';

                // 
                let selected = await openApproverSelector(step, section);
                while (!selected) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.MustSelectApprover || 'Bạn phải chọn người phê duyệt trước khi thoát'), type: 'warning' });
                    selected = await openApproverSelector(step, section);
                }

                const approverId = selected.CHR_UserAdid ?? selected.chR_UserAdid ?? selected.CHR_Adid ?? selected.chR_Adid ?? selected.ADID ?? selected.Id ?? selected.id ?? selected.value ?? '';
                const finalId = approverId || (selected.value || selected.Value || '');

                if (!finalId) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.InvalidApprover || 'Người phê duyệt không hợp lệ'), type: 'error' });
                    return;
                }

                const payload = {
                    listUpdate: importResult?.listUpdate,
                    sectionCode: finalId
                };

                const updateResponse = await fetch(apiUrl('/History/UpdateUserApprovalHistory'), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (!updateResponse.ok) {
                    const text = await updateResponse.text().catch(() => null);
                    throw new Error(text || (T.ErrorPrefix || 'Lỗi server khi cập nhật người phê duyệt'));
                }

                const updateResult = await updateResponse.json().catch(() => null);

                showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Cập nhật người phê duyệt thành công'), type: 'success' });
                applyFilters(1);

            } catch (error) {
                const T = window.i18nHistoryQuote || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (error && error.message) ? error.message : (T.ErrorPrefix || 'Không thể import file'), type: 'error' });
            } finally {
                try { hideLoading(); } catch (e) { }
                try { document.body.removeChild(fileInput); } catch (e) { }
            }
        });

        fileInput.click();
    });

    // Open approver selection modal (copied/adapted from HistoryQuote.js)
    function openApproverSelector(stepNumber, sectionCode) {
        return new Promise(async (resolve, reject) => {
            try {
                const modal = document.getElementById('selectApproverModal');
                const sel = document.getElementById('selectNextApprover');
                const notice = document.getElementById('selectApproverNotice');
                if (!modal || !sel) return resolve(null);
                sel.innerHTML = '';
                const placeholderOpt = document.createElement('option');
                placeholderOpt.value = '';
                placeholderOpt.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.SelectPlaceholder) || '-- Chọn --';
                sel.appendChild(placeholderOpt);

                const body = { Step: stepNumber, SectionCost: sectionCode };
                let list = [];
                try {
                    const resp = await fetch(apiUrl('/History/GetListApprovel'), {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                        body: JSON.stringify(body)
                    });
                    if (resp.ok) {
                        const data = await resp.json();
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
                        const adid = item.chR_UserAdid || item.CHR_UserAdid || item.ADID || item.Id || item.id || '';
                        const name = item.nvchR_UserName || item.NVCHR_UserName || item.Name || item.FullName || item.nvchR_FullName || '';
                        o.value = adid || '';
                        o.textContent = (name ? (name + (adid ? (' (' + adid + ')') : '')) : (adid || ''));
                        try { o.dataset.raw = JSON.stringify(item); } catch { }
                        sel.appendChild(o);
                    });
                }

                try { if (modal.parentElement !== document.body) document.body.appendChild(modal); } catch (e) { }
                try {
                    if (window.bootstrap && bootstrap.Modal) {
                        const bsModal = new bootstrap.Modal(modal, { backdrop: 'static' });
                        modal._bsModal = bsModal;
                        bsModal.show();
                        setTimeout(() => { try { const createdBackdrop = document.querySelector('.modal-backdrop'); if (createdBackdrop) createdBackdrop.style.zIndex = '10550'; modal.style.zIndex = '10600'; } catch (e) { } }, 10);
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
            if (msgEl) msgEl.textContent = window.i18nHistoryQuote?.LoadingData || 'Processing...';
        } catch (e) { }
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

    const state = {
        requestController: null
    };

    function apiUrl(path) {
        const base = (window.apiBaseUrl || '').trim().replace(/\/$/, '');
        if (!base) return path;
        return `${base}${path.startsWith('/') ? '' : '/'}${path}`;
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function getValue(obj, keys, fallback = '') {
        if (!obj) return fallback;
        for (const key of keys) {
            if (obj[key] !== undefined && obj[key] !== null) return obj[key];
            const lower = Object.keys(obj).find(k => k.toLowerCase() === key.toLowerCase());
            if (lower && obj[lower] !== undefined && obj[lower] !== null) return obj[lower];
        }
        return fallback;
    }

    function formatDate(value) {
        if (!value) return '';
        const d = new Date(value);
        if (Number.isNaN(d.getTime())) return '';
        const dd = String(d.getDate()).padStart(2, '0');
        const mm = String(d.getMonth() + 1).padStart(2, '0');
        const yyyy = d.getFullYear();
        return `${dd}/${mm}/${yyyy}`;
    }

    function formatDateTime(value) {
        if (!value) return '';
        const d = new Date(value);
        if (Number.isNaN(d.getTime())) return '';
        const dd = String(d.getDate()).padStart(2, '0');
        const mm = String(d.getMonth() + 1).padStart(2, '0');
        const yyyy = d.getFullYear();
        const hh = String(d.getHours()).padStart(2, '0');
        const mi = String(d.getMinutes()).padStart(2, '0');
        return `${dd}/${mm}/${yyyy} ${hh}:${mi}`;
    }

    function isOverdue(dateValue) {
        if (!dateValue) return false;
        const d = new Date(dateValue);
        if (Number.isNaN(d.getTime())) return false;
        const now = new Date();
        now.setHours(0, 0, 0, 0);
        d.setHours(0, 0, 0, 0);
        return d < now;
    }

    function buildSearchPayload(pageIndex = 1) {
        return {
            maDon: document.getElementById('searchMaDon')?.value?.trim() || null,
            maNcc: document.getElementById('searchNhaCungCap')?.value?.trim() || null,
            section: document.getElementById('searchPhongBan')?.value?.trim() || null,
            nguoiYeuCau: document.getElementById('searchNguoiTao')?.value?.trim() || null,
            maHang: document.getElementById('searchMaVatTu')?.value?.trim() || null,
            trangThai: document.getElementById('statusFilter')?.value?.trim() || null,
            pageIndex,
            pageSize,
            to: document.getElementById('dateTo')?.value || null,
            from: document.getElementById('dateFrom')?.value || null,
            chungLoai: null
        };
    }

    async function postJson(url, payload, signal) {
        const response = await fetch(apiUrl(url), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload),
            signal
        });

        const json = await response.json().catch(() => null);
        if (!response.ok) {
            const message = typeof json === 'string'
                ? json
                : getValue(json, ['message', 'Message'], 'Có lỗi xảy ra');
            throw new Error(message);
        }

        return json;
    }

    function normalizeListResponse(result) {
        const root = result || {};
        const data = getValue(root, ['data', 'Data'], []);
        if (Array.isArray(data)) {
            return { rows: data, totalCount: data.length };
        }

        const rows = getValue(data, ['data', 'Data'], []);
        const totalCount = Number(getValue(data, ['totalCount', 'TotalCount'], Array.isArray(rows) ? rows.length : 0)) || 0;
        return {
            rows: Array.isArray(rows) ? rows : [],
            totalCount
        };
    }

    function renderPagination(pageIndex, totalCount) {
        if (!paginationEl) return;

        const totalPages = Math.max(1, Math.ceil((totalCount || 0) / pageSize));
        currentPage = Math.min(Math.max(1, pageIndex), totalPages);

        const items = [];
        items.push(`<li class="page-item ${currentPage <= 1 ? 'disabled' : ''}"><button class="page-link" data-page="prev">«</button></li>`);

        const maxButtons = 5;
        const start = Math.max(1, currentPage - Math.floor(maxButtons / 2));
        const end = Math.min(totalPages, start + maxButtons - 1);
        const adjustedStart = Math.max(1, end - maxButtons + 1);

        for (let p = adjustedStart; p <= end; p++) {
            items.push(`<li class="page-item ${p === currentPage ? 'active' : ''}"><button class="page-link" data-page="${p}">${p}</button></li>`);
        }

        items.push(`<li class="page-item ${currentPage >= totalPages ? 'disabled' : ''}"><button class="page-link" data-page="next">»</button></li>`);
        paginationEl.innerHTML = items.join('');

        if (paginationInfoEl) {
            const total = totalCount || 0;
            const from = total === 0 ? 0 : (currentPage - 1) * pageSize + 1;
            const to = Math.min(currentPage * pageSize, total);
            const template = window.i18nHistoryQuote?.PaginationInfo || 'Hiển thị {0} - {1} / {2}';
            paginationInfoEl.textContent = template
                .replace('{0}', from)
                .replace('{1}', to)
                .replace('{2}', total);
        }

    }

    function navigateToPage(targetPage) {
        const totalPages = Math.max(1, Math.ceil((totalCountServer || 0) / pageSize));
        const safeTarget = Math.min(Math.max(1, Number(targetPage) || 1), totalPages);
        if (safeTarget === currentPage) return;

        applyFilters(safeTarget);
    }

    function getRowsForPage(rows) {
        if (!Array.isArray(rows)) return [];
        return rows;
    }

    function mapActionText(actionType) {
        const code = String(actionType || '').trim();
        if (!code) return '';
        const statuses = window.HistoryData?.status;
        if (!Array.isArray(statuses)) return code;
        const found = statuses.find(s => String(s?.VCHR_CodeStatus || '').trim() === code);
        return found?.DisplayName || code;
    }

    function buildHistoryHtml(result) {
        const data = Array.isArray(result) ? result : (result?.data || result?.Data || []);
        const T = window.i18nHistoryQuote || {};
        if (!Array.isArray(data) || data.length === 0) {
            return `<div>${escapeHtml(T.MsgNoHistory || 'Không có lịch sử.')}</div>`;
        }

        const rows = data.map((h, index) => {
            const dateText = formatDateTime(getValue(h, ['CHR_Updatedate', 'chR_Updatedate']));
            const requestId = getValue(h, ['ID_RequestQuote', 'iD_RequestQuote']);
            const action = mapActionText(getValue(h, ['CHR_ActionType', 'chR_ActionType']));
            const updateBy = getValue(h, ['CHR_UpdateBy', 'chR_UpdateBy']);
            const updateName = getValue(h, ['NVCHR_UpdateName', 'nvchR_UpdateName']);
            const reason = getValue(h, ['NVCHR_LyDo', 'nvchR_LyDo']);

            return `<tr>
                <td class="text-center">${index + 1}</td>
                <td>${escapeHtml(dateText)}</td>
                <td>${escapeHtml(requestId)}</td>
                <td>${escapeHtml(action)}</td>
                <td>${escapeHtml(updateBy)}${updateName ? ` - ${escapeHtml(updateName)}` : ''}</td>
                <td>${escapeHtml(reason)}</td>
            </tr>`;
        }).join('');

        return `
            <div class="table-responsive">
                <table class="table table-sm table-bordered">
                    <thead class="table-light"><tr>
                        <th style="width:60px">#</th>
                        <th>${escapeHtml(T.HistoryTime || 'Thời gian')}</th>
                        <th>${escapeHtml(T.RequestNo || 'Số Request')}</th>
                        <th>${escapeHtml(T.Action || 'Hành động')}</th>
                        <th>${escapeHtml(T.UpdatedBy || 'Người cập nhật')}</th>
                        <th>${escapeHtml(T.Reason || 'Lý do')}</th>
                    </tr></thead>
                    <tbody>${rows}</tbody>
                </table>
            </div>`;
    }

    function showReasonModal({ modalId, textareaId, noticeId, confirmButtonId }) {
        return new Promise((resolve) => {
            const modalEl = document.getElementById(modalId);
            const textarea = document.getElementById(textareaId);
            const notice = document.getElementById(noticeId);
            const confirmBtn = document.getElementById(confirmButtonId);
            if (!modalEl || !textarea || !confirmBtn) {
                resolve(null);
                return;
            }

            textarea.value = '';
            if (notice) notice.style.display = 'none';

            try {
                if (modalEl.parentElement !== document.body) {
                    document.body.appendChild(modalEl);
                }
            } catch (e) { }

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

            const cleanUp = () => {
                try {
                    if (bsModal) bsModal.hide();
                    else {
                        modalEl.style.display = 'none';
                        modalEl.classList.remove('show');
                        document.body.classList.remove('modal-open');
                    }
                } catch (e) {
                    modalEl.style.display = 'none';
                    modalEl.classList.remove('show');
                    document.body.classList.remove('modal-open');
                }
                try { confirmBtn.removeEventListener('click', onConfirm); } catch (e) { }
                try { modalEl.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.removeEventListener('click', onCancel)); } catch (e) { }
                try { modalEl.removeEventListener('hidden.bs.modal', onHidden); } catch (e) { }
            };

            const onHidden = () => {
                cleanUp();
                resolve(null);
            };

            const onCancel = (e) => {
                e?.preventDefault();
                cleanUp();
                resolve(null);
            };

            const onConfirm = (e) => {
                e?.preventDefault();
                const reason = (textarea.value || '').trim();
                if (!reason) {
                    if (notice) notice.style.display = '';
                    return;
                }
                cleanUp();
                resolve(reason);
            };

            try { modalEl.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.addEventListener('click', onCancel)); } catch (e) { }
            try { if (bsModal) modalEl.addEventListener('hidden.bs.modal', onHidden); } catch (e) { }
            confirmBtn.addEventListener('click', onConfirm);
            try { textarea.focus(); } catch (e) { }
        });
    }

    async function handleViewHistory(button) {
        const T = window.i18nHistoryQuote || {};
        const maDon = button.getAttribute('data-madon') || '';
        const maHang = button.getAttribute('data-mahang') || '';
        const maHangNcc = button.getAttribute('data-mahangncc') || '';
        if (!maDon) {
            showDialog({ title: T.Notification || 'Thông báo', message: T.MsgSelectGroupFailed || 'Vui lòng chọn mã đơn!', type: 'warning' });
            return;
        }

        try {
            showLoading(T.LoadingData || 'Đang tải...');
            const histories = await postJson('/History/GetHistoryApprover', {
                maDon,
                maHang,
                maHangNCC: maHangNcc
            });
            showDialog(T.PageTitleHistory || 'Lịch sử đơn', buildHistoryHtml(histories));
        } catch (error) {
            showDialog({
                title: T.Notification || 'Thông báo',
                message: error?.message || T.MsgLoadHistoryFailed || 'Không tải được lịch sử.',
                type: 'error'
            });
        } finally {
            hideLoading();
        }
    }

    async function handleDeleteHistory(button) {
        const T = window.i18nHistoryQuote || {};
        const maDon = button.getAttribute('data-madon') || '';
        if (!maDon) return;

        const reason = await showReasonModal({
            modalId: 'deleteReasonModal',
            textareaId: 'deleteReasonText',
            noticeId: 'deleteReasonNotice',
            confirmButtonId: 'confirmDeleteWithReason'
        });
        if (!reason) return;

        try {
            showLoading(T.Deleting || 'Đang xóa...');
            const response = await fetch(apiUrl('/History/DeleteDanhSachBaoGiaByMaDon'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ maDon, reason })
            });

            const text = await response.text().catch(() => null);
            if (!response.ok) {
                throw new Error(text || (T.DeleteFailed || 'Xóa thất bại'));
            }

            showDialog({ title: T.Notification || 'Thông báo', message: T.DeleteSuccess || 'Đã xóa thành công.', type: 'success' });
            applyFilters(currentPage);
        } catch (error) {
            showDialog({
                title: T.Notification || 'Thông báo',
                message: error?.message || T.DeleteFailed || 'Xóa thất bại',
                type: 'error'
            });
        } finally {
            hideLoading();
        }
    }

    async function handleReturnHistory(button) {
        const T = window.i18nHistoryQuote || {};
        const maDon = button.getAttribute('data-madon') || '';
        if (!maDon) return;

        const reason = await showReasonModal({
            modalId: 'returnReasonModal',
            textareaId: 'returnReasonText',
            noticeId: 'returnReasonNotice',
            confirmButtonId: 'confirmReturnWithReason'
        });
        if (!reason) return;

        try {
            showLoading(T.Exporting || 'Đang xử lý...');
            const response = await fetch(apiUrl('/History/ReturnQuotation'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ maDon, reason })
            });

            const text = await response.text().catch(() => null);
            if (!response.ok) {
                throw new Error(text || (T.ReturnFailed || 'Trả lại thất bại!'));
            }

            showDialog({ title: T.Notification || 'Thông báo', message: T.ReturnSuccess || 'Trả về thành công!', type: 'success' });
            applyFilters(currentPage);
        } catch (error) {
            showDialog({
                title: T.Notification || 'Thông báo',
                message: error?.message || T.ReturnFailed || 'Trả lại thất bại!',
                type: 'error'
            });
        } finally {
            hideLoading();
        }
    }

    function approvalCell(name, time, userNext, cellStep, currentStep) {
        const text = String(name || '').trim();

        if (cellStep < currentStep) {
            if (text) {
                const dt = formatDateTime(time);
                return `<td style="background:#cfe3c6;">
                ${escapeHtml(text)}
                ${dt ? `<div class="small text-muted">${escapeHtml(dt)}</div>` : ''}
                    </td>`;
            }
        } else if (cellStep === currentStep) {
            return `<td>${escapeHtml(userNext)}</td>`;
        }
        return `<td></td>`;
    }
    function costCell(flUsed, cellStep) {
        if (cellStep > 11 && flUsed != null && flUsed !== '') {
            const value = parseFloat(flUsed);
            return `<td>${value.toFixed(4).replace(/\.?0+$/, '')} USD</td>`;
        }
        return `<td></td>`;
    }
    function StatusCell(statusName, step, isAllRefuse, CHR_Status, CHR_StatusACC, CHR_StatusShip) {
        if (step === 12) {
            if (CHR_Status === 'Confirming') {
              return ` <td>${escapeHtml(window.i18nHistoryQuote?.StatusPUR || 'Chờ xác nhận tên của PUR')}</td>`
            } else if (CHR_StatusACC === 'Confirming') {
                return ` <td>${escapeHtml(window.i18nHistoryQuote?.StatusACC || 'Chờ xác nhận tên của phòng ban')}</td>`
            } else if (CHR_StatusShip === 'Confirming') {
                return  ` <td>${escapeHtml(window.i18nHistoryQuote?.StatusShip || 'Chờ xác nhận tên của Ship')}</td>`
            } else {
                return  ` <td>${escapeHtml(statusName)}</td>`
            }

        } else {
          return  isAllRefuse ? ` <td>${escapeHtml(window.i18nHistoryQuote?.AllRefuse || 'Toàn bộ các NCC đã từ chối báo giá')}</td>`
              : `<td>${escapeHtml(statusName)}</td>`
        }

    }
    function supplierCell(value, bitValue, status, step, isAllRefuse, selectedSupplier, quoteLink) {
        const raw = String(bitValue ?? '').trim().toLowerCase();
        const isRefuse = String(status ?? '').trim().toLowerCase() === 'refuse';
        const isSelected = bitValue === 1 || bitValue === true || raw === '1' || raw === 'true';

        const supplierName = String(value ?? '').trim();
        const selectedName = String(selectedSupplier ?? '').trim();

        const stepByRole = role === 'UserPUR' ? 8 : 12;

        // Vẫn tô nền xanh khi step > stepByRole
        const isPickedSupplier =
            step > stepByRole &&
            supplierName &&
            selectedName &&
            supplierName.toLowerCase() === selectedName.toLowerCase();

        // Chỉ cho download khi step > stepByRole
        const canDownloadQuote =
            step > stepByRole &&
            supplierName &&
            String(quoteLink ?? '').trim();

        let bgColor = '#ffffff';
        let textColor = '';

        if (step < 5) {
            bgColor = '#ffffff';
        }
        else if (isAllRefuse) {
            bgColor = '#e74c3c';
            textColor = '#ffffff';
        }
        else if (isSelected && !isRefuse) {
            bgColor = '#cfe3c6';
        }
        else if (isRefuse) {
            bgColor = '#f1c232';
        }

        // Tô nền supplier được chọn khi step > 10
        if (isPickedSupplier) {
            bgColor = '#cfe2ff';

            // Chỉ đổi màu chữ thành link khi step > stepByRole
            if (step > stepByRole) {
                textColor = '#0d6efd';
            }
        }

        const cellContent = canDownloadQuote
            ? `<button type="button"
                   class="btn btn-link p-0 btn-download"
                   data-file="${escapeHtml(quoteLink)}"
                   title="Download quote"
                   style="color:#0d6efd;text-decoration:underline;white-space:normal;word-break:break-all;overflow-wrap:anywhere;display:block;width:100%;text-align:inherit;line-height:1.2;">
                ${escapeHtml(value)}
           </button>`
            : escapeHtml(value);

        return `<td style="background:${bgColor};${textColor ? `color:${textColor};font-weight:600;` : ''}">
                ${cellContent}
            </td>`;
    }
    function renderTable(rows) {
        if (!tblBody) return;

        if (!rows || rows.length === 0) {
            const colSpan = document.querySelectorAll('.approval-table thead tr:last-child th')?.length + 17 || 28;
            tblBody.innerHTML = `<tr><td colspan="${colSpan}" class="text-center text-muted py-3">${escapeHtml(window.i18nHistoryQuote?.MsgNoDataToEdit || 'Không có dữ liệu')}</td></tr>`;
            return;
        }

        const startNo = (currentPage - 1) * pageSize + 1;
        const html = new Array(rows.length);

        for (let i = 0; i < rows.length; i++) {
            const row = rows[i] || {};
            const stepName = window.i18nHistoryQuote?.CHR_StepName || 'CHR_StepName';
            const deadline = getValue(row, ['DTM_KyHan']);
            const overdue = isOverdue(deadline);
            const maDon = getValue(row, ['CHR_MaDon']);
            const maHang = getValue(row, ['CHR_MaHangNoiBo']);
            const maHangNcc = getValue(row, ['CHR_MaHangNCC']);
            const editAction = `
                <button type="button"
                        class="btn btn-edit-history bg-transparent border-0 shadow-none p-0"
                        title="Edit"
                        data-mahangncc="${escapeHtml(getValue(row, ['CHR_MaHangNCC']))}"
                        data-mahangnb="${escapeHtml(getValue(row, ['CHR_MaHangNoiBo']))}"
                        data-name="${escapeHtml(getValue(row, ['CHR_NameEN']))}"
                        data-madon="${escapeHtml(maDon)}">
                    <i class="fas fa-edit text-primary"></i>
                </button>`;
            const countStatus = [
                row.Status_1,
                row.Status_2,
                row.Status_3,
                row.Status_4,
                row.Status_5
            ].filter(s => String(s ?? '').trim().toLowerCase() === 'refuse').length;
            const countNCC = [
                row.NCC_1,
                row.NCC_2,
                row.NCC_3,
                row.NCC_4,
                row.NCC_5
            ].filter(s => String(s ?? '').trim() !== '').length;

            const isAllRefuse = countStatus === countNCC;
            const step = Number(getValue(row, ['Step', 'step'], 0)) || 0;
            const selectedSupplier = getValue(row, ['NCC_DuocChon', 'ncc_DuocChon', 'NCCDuocChon']);
            const link1 = getValue(row, ['nLink_1', 'Link_1', 'link_1']);
            const link2 = getValue(row, ['Link_2', 'link_2']);
            const link3 = getValue(row, ['Link_3', 'link_3']);
            const link4 = getValue(row, ['Link_4', 'link_4']);
            const link5 = getValue(row, ['Link_5', 'link_5']);
            const returnAction = role === 'UserPUR'
                ? `<button type="button" class="btn btn-outline-warning btn-return-history" title="${escapeHtml(window.i18nHistoryQuote?.ReturnTooltip || 'Return')}" data-madon="${escapeHtml(maDon)}"><i class="fas fa-undo"></i></button>`
                : '';
            const StatusRow = StatusCell(getValue(row, [stepName]), step, isAllRefuse, getValue(row, ['CHR_Status']), getValue(row, ['CHR_StatusACC']), getValue(row, ['CHR_StatusShip']));
            html[i] = `
                <tr>
                    <td>${startNo + i}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaDon']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaHangNoiBo']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaHangNCC']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_NameEN']))}</td>
                    <td style="max-width: 120px;" >${escapeHtml(getValue(row, ['NVCHR_ChungLoai']))}</td>
                    ${supplierCell(getValue(row, ['NCC_1']), getValue(row, ['BitNCC_1', 'bitNCC_1']), getValue(row, ['Status_1', 'status_1']), step, isAllRefuse, selectedSupplier, link1)}
                    ${supplierCell(getValue(row, ['NCC_2']), getValue(row, ['BitNCC_2', 'bitNCC_2']), getValue(row, ['Status_2', 'status_2']), step, isAllRefuse, selectedSupplier, link2)}
                    ${supplierCell(getValue(row, ['NCC_3']), getValue(row, ['BitNCC_3', 'bitNCC_3']), getValue(row, ['Status_3', 'status_3']), step, isAllRefuse, selectedSupplier, link3)}
                    ${supplierCell(getValue(row, ['NCC_4']), getValue(row, ['BitNCC_4', 'bitNCC_4']), getValue(row, ['Status_4', 'status_4']), step, isAllRefuse, selectedSupplier, link4)}
                    ${supplierCell(getValue(row, ['NCC_5']), getValue(row, ['BitNCC_5', 'bitNCC_5']), getValue(row, ['Status_5', 'status_5']), step, isAllRefuse, selectedSupplier, link5)}
                    <td>${escapeHtml(getValue(row, ['NVCHR_ReasonPick']))}</td>
                    ${costCell(getValue(row, ['FL_USD']), step)}
                    <td style="${overdue ? 'background:red;color:#fff;' : ''}">${escapeHtml(formatDate(deadline))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_CreateBy']))}</td>
                    ${approvalCell(getValue(row, ['QLSC_Approve']), getValue(row, ['QLSC_Time']), getValue(row, ['UserNext']), 2, step)}
                    ${approvalCell(getValue(row, ['QLTC_Approve']), getValue(row, ['QLTC_Time']), getValue(row, ['UserNext']), 3, step)}
                    ${approvalCell(getValue(row, ['PIC_Approve']), getValue(row, ['PIC_Time']), getValue(row, ['UserNext']), 4, step)}
                    ${approvalCell(getValue(row, ['QLSC1_Approve']), getValue(row, ['QLSC1_Time']), getValue(row, ['UserNext']), 5, step)}
                    ${approvalCell(getValue(row, ['PIC_PickNCC']), getValue(row, ['PIC_PickNCC_Time']), getValue(row, ['UserNext']), 7, step)}
                    ${approvalCell(getValue(row, ['QLSC_PickNCC']), getValue(row, ['QLSC_PickNCC_Time']), getValue(row, ['UserNext']), 9, step)}
                    ${approvalCell(getValue(row, ['QLTC_PickNCC']), getValue(row, ['QLTC_PickNCC_Time']), getValue(row, ['UserNext']), 10, step)}
                    ${approvalCell(getValue(row, ['DEFT_PickNCC']), getValue(row, ['DEFT_PickNCC_Time']), getValue(row, ['UserNext']), 11, step)}
                    ${StatusRow}
                    <td>
                        <div class="action-buttons" role="group" aria-label="${escapeHtml(window.i18nHistoryQuote?.Actions || 'Actions')}">
                            ${editAction}
                            <button type="button" class="btn btn-outline-info btn-view-history" title="${escapeHtml(window.i18nHistoryQuote?.ViewHistoryTooltip || 'View history')}" data-madon="${escapeHtml(maDon)}" data-mahang="${escapeHtml(maHang)}" data-mahangncc="${escapeHtml(maHangNcc)}"><i class="fas fa-history"></i></button>
                            ${returnAction}
                            <button type="button" class="btn btn-outline-danger btn-delete-history" title="${escapeHtml(window.i18nHistoryQuote?.DeleteTooltip || 'Delete')}" data-madon="${escapeHtml(maDon)}"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>
                </tr>`;
        }

        tblBody.innerHTML = html.join('');
    }
    // tải file xuống
    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.btn-download');
        if (!btn) return;

        const file = btn.dataset.file;
        if (!file) {
            alert("Không có file");
            return;
        }

        try {
            const response = await fetch(apiUrl('/History/DownloadQuoteFile'), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(file)
            });

            if (!response.ok) {
                const errText = await response.text();
                throw new Error(errText || "Download thất bại");
            }

            const blob = await response.blob();

            // tạo link download
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;

            // lấy tên file từ path
            a.download = file.split('/').pop() || 'download';
            document.body.appendChild(a);
            a.click();

            a.remove();
            window.URL.revokeObjectURL(url);

        } catch (err) {
            console.error(err);
            alert("Chi tiết: " + err.message);
        }
    });
    function renderSummaryCountQuotation(result) {
        const row = Array.isArray(result) ? (result[0] || {}) : (result || {});
        document.getElementById('statDueSoon').textContent = getValue(row, ['DenHanLuaChon', 'denHanLuaChon'], 0);
        document.getElementById('statOneDayLeft').textContent = getValue(row, ['ConMotNgayHetHan', 'conMotNgayHetHan'], 0);
        document.getElementById('statRemaining').textContent = getValue(row, ['ConLai', 'conLai'], 0);
        document.getElementById('statOverdue').textContent = getValue(row, ['QuaHan', 'quaHan'], 0);
    }

    function renderSummaryCountStatus(result) {
        const row = Array.isArray(result) ? (result[0] || {}) : (result || {});
        document.getElementById('statWaitDeptPic').textContent = getValue(row, ['PICSection', 'picSection'], 0);
        document.getElementById('statWaitDeptQlsc').textContent = getValue(row, ['QLSCSection', 'qlsCSection'], 0);
        document.getElementById('statWaitDeptQltc').textContent = getValue(row, ['QLTCSection', 'qltCSection'], 0);
        document.getElementById('statWaitOrderPic').textContent = getValue(row, ['PICPur', 'picPur'], 0);
        document.getElementById('statWaitOrderQlsc').textContent = getValue(row, ['QLSCPur', 'qlsCPur'], 0);
    }


    function renderSummaryWaitingSupplier(result) {
        const row = Array.isArray(result) ? (result[0] || {}) : (result || {});
        document.getElementById('statWaitingSupplier').textContent = getValue(row, ['IsNeed'], 0);
        document.getElementById('statSelectedSupplier').textContent = getValue(row, ['IsNeedPick'], 0);
        document.getElementById('statBothStatusSupplier').textContent = getValue(row, ['IsPicked'], 0);
        document.getElementById('statTotalSupplier').textContent = getValue(row, ['IsPicking'], 0);
    }

    async function applyFilters(pageIndex = 1) {
        if (state.requestController) state.requestController.abort();
        state.requestController = new AbortController();

        const payload = buildSearchPayload(pageIndex);
        currentPage = pageIndex;

        try {
            showLoading(window.i18nHistoryQuote?.LoadingData || 'Processing...');

            const [historyResult, countQuotationResult, countStatusResult, processingStatusResult, waitingSupplierResult] = await Promise.all([
                postJson('/History/SearchHistory', payload, state.requestController.signal),
                postJson('/History/GetCountQuotation', payload, state.requestController.signal),
                postJson('/History/GetCountStatus', payload, state.requestController.signal),
                postJson('/History/GetProcessingStatus', payload, state.requestController.signal),
                postJson('/History/GetHistoryTab1', payload, state.requestController.signal)
            ]);

            const parsed = normalizeListResponse(historyResult);
            currentGroups = parsed.rows;
            totalCountServer = historyResult.totalCount;

            renderTable(getRowsForPage(currentGroups));
            renderPagination(currentPage, totalCountServer);
            renderSummaryCountQuotation(countQuotationResult);
            renderSummaryCountStatus(countStatusResult);
            //renderSummaryProcessingStatus(processingStatusResult);
            renderSummaryWaitingSupplier(waitingSupplierResult);
        } catch (error) {
            if (error?.name === 'AbortError') return;
            showDialog({
                title: window.i18nHistoryQuote?.Notification || 'Thông báo',
                message: error?.message || window.i18nHistoryQuote?.MsgSearchFailed || 'Lỗi tìm kiếm dữ liệu',
                type: 'error'
            });
        } finally {
            hideLoading();
        }
    }

    btnApply?.addEventListener('click', () => applyFilters(1));

    document.getElementById('historyFilterForm')?.addEventListener('submit', function (e) {
        e.preventDefault();
        applyFilters(1);
    });

    paginationEl?.addEventListener('click', function (e) {
        const btn = e.target.closest && e.target.closest('button[data-page]');
        if (!btn) return;
        e.preventDefault();

        const pageAttr = btn.getAttribute('data-page');

        const totalPages = Math.max(1, Math.ceil((totalCountServer || 0) / pageSize));
        let targetPage = 1;

        if (pageAttr === 'prev') {
            targetPage = Math.max(1, currentPage - 1);
        } else if (pageAttr === 'next') {
            targetPage = Math.min(totalPages, currentPage + 1);
        } else {
            const n = Number(pageAttr);
            targetPage = Number.isInteger(n) && n > 0 ? Math.min(Math.max(1, n), totalPages) : currentPage;
        }

        navigateToPage(targetPage);

    });

    pageSizeSelect?.addEventListener('change', function () {
        const nextSize = Number(this.value);
        if (!Number.isInteger(nextSize) || nextSize <= 0) return;
        pageSize = nextSize;
        applyFilters(1);
    });

    tblBody?.addEventListener('click', async function (event) {
        const button = event.target.closest('button');
        if (!button) return;

        if (button.classList.contains('btn-view-history')) {
            await handleViewHistory(button);
            return;
        }

        if (button.classList.contains('btn-return-history')) {
            await handleReturnHistory(button);
            return;
        }

        if (button.classList.contains('btn-delete-history')) {
            await handleDeleteHistory(button);
        }

        if (button.classList.contains('btn-edit-history')) {
            await openEditModal(button);
            return;
        }
    });
    async function openEditModal(button) {
        const payload = {
            MaDon: button.dataset.madon || '',
            MaHangNCC: '',
            MaHang: button.dataset.mahangnb || '',
            NameEn: button.dataset.name || ''
        };

        const preferredVendorCode = button.dataset.mahangncc || '';

        try {
            showLoading(window.i18nHistoryQuote?.LoadingData || 'Processing...');
            const response = await fetch((window.apiBaseUrl || '') + '/History/SearchOrderInfo', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            const T = window.i18nHistoryQuote || {};
            if (!response.ok) {
                throw new Error(T.MsgLoadHistoryFailed || 'Load history failed');
            }

            const result = await response.json();
            const data = Array.isArray(result)
                ? result
                : (Array.isArray(result?.data) ? result.data : (Array.isArray(result?.Data) ? result.Data : []));

            if (!Array.isArray(data) || data.length === 0) {
                showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + (T.MsgNoDataToEdit || 'Không có dữ liệu để chỉnh sửa.') + '</div>');
                return;
            }

            editOrderRows = data;
            initEditSupplierSelector(preferredVendorCode);
            showEditModal();
        } catch (err) {
            console.error(err);
            const T = window.i18nHistoryQuote || {};
            showDialog(T.Notification || 'Thông báo', '<div class="text-danger">' + ((err && err.message) || T.MsgNotFoundData || 'Không tìm thấy dữ liệu.') + '</div>');
        } finally {
            hideLoading();
        }
    }

    function initEditSupplierSelector(preferredVendorCode) {
        const supplierSelect = document.getElementById('editSupplierSelect');
        const supplierCount = document.getElementById('editSupplierCount');
        if (!supplierSelect) {
            fillEditFormFromDto(editOrderRows[0]);
            return;
        }

        supplierSelect.innerHTML = '';
        editOrderRows.forEach((row, index) => {
            const option = document.createElement('option');
            const vendorCode = getValue(row, ['CHR_MaNCC', 'chR_MaNCC'], '');
            const vendorName = getValue(row, ['NVCHR_TenNCC', 'nvchR_TenNCC'], '');
            const vendorItemCode = getValue(row, ['CHR_MaHangNCC', 'chR_MaHangNCC'], '');

            option.value = String(index);
            option.textContent = `${vendorCode || '-'}${vendorName ? ` - ${vendorName}` : ''}${vendorItemCode ? ` (${vendorItemCode})` : ''}`;
            supplierSelect.appendChild(option);
        });

        if (supplierCount) {
            supplierCount.textContent = `${editOrderRows.length}`;
        }

        let selectedIndex = 0;
        if (preferredVendorCode) {
            const foundIndex = editOrderRows.findIndex(x =>
                String(getValue(x, ['CHR_MaHangNCC', 'chR_MaHangNCC'], '')).trim().toLowerCase() === preferredVendorCode.trim().toLowerCase()
            );
            if (foundIndex >= 0) selectedIndex = foundIndex;
        }

        supplierSelect.value = String(selectedIndex);
        fillEditFormFromDto(editOrderRows[selectedIndex]);

        if (!supplierSelect.dataset.boundChange) {
            supplierSelect.addEventListener('change', function () {
                const idx = Number(this.value);
                if (Number.isInteger(idx) && idx >= 0 && idx < editOrderRows.length) {
                    fillEditFormFromDto(editOrderRows[idx]);
                }
            });
            supplierSelect.dataset.boundChange = '1';
        }
    }

    function fillEditFormFromDto(dto) {
        if (!dto) return;

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

        const read = (keys, fallback = '') => getValue(dto, keys, fallback);
        const toDateInput = (d) => { try { if (!d) return ''; const dt = new Date(d); return dt.toISOString().slice(0, 10); } catch { return ''; } };

        const requestId = read(['ID', 'id'], '');
        const requestIdEl = document.getElementById('editRequestId');
        if (requestIdEl) requestIdEl.value = requestId;

        setControlValue('editMaDon', read(['CHR_MaDon', 'chR_MaDon'], ''));
        setControlValue('editRequester', read(['CHR_CreateBy', 'chR_CreateBy'], ''));
        setControlValue('editSectionCode', read(['CHR_SectionCode', 'chR_SectionCode'], ''));
        setControlValue('editSectionName', read(['CHR_SectionName', 'chR_SectionName'], ''));

        const supplierCode = read(['CHR_MaNCC', 'chR_MaNCC'], '');
        const supplierName = read(['NVCHR_TenNCC', 'nvchR_TenNCC'], '');
        setControlValue('editNhaCungCap', supplierCode);
        setControlValue('editTenNCC', supplierName);
        setControlValue('editSupplierCodeDisplay', supplierCode);
        setControlValue('editSupplierNameDisplay', supplierName);

        setControlValue('editChungLoai', read(['NVCHR_ChungLoai', 'nvchR_ChungLoai'], ''));
        setControlValue('editPhanLoai', read(['CHR_Phanloai', 'chR_Phanloai'], ''));
        setControlValue('editMaThietBi', read(['CHR_MaThietBi', 'chR_MaThietBi'], ''));
        setControlValue('editMaHangNoiBo', read(['CHR_MaHangNoiBo', 'chR_MaHangNoiBo'], ''));
        setControlValue('editMaHangNCC', read(['CHR_MaHangNCC', 'chR_MaHangNCC'], ''));
        setControlValue('editTenHangVN', read(['NVCHR_NameVN', 'nvchR_NameVN'], ''));
        setControlValue('editTenHangEN', read(['CHR_NameEN', 'chR_NameEN'], ''));
        setControlValue('editSoLuong', read(['INT_SoLuong', 'inT_SoLuong'], ''));
        setControlValue('editDonVi', read(['NVCHR_DonVi', 'nvchR_DonVi'], ''));
        setControlValue('editHinhDang', read(['NVCHR_HinhDang', 'nvchR_HinhDang'], ''));
        setControlValue('editChatLieu', read(['NVCHR_ChatLieu', 'nvchR_ChatLieu'], ''));
        setControlValue('editThanhPhan', read(['NVCHR_ThanhPhan', 'nvchR_ThanhPhan'], ''));
        setControlValue('editKichThuoc', read(['NVCHR_KichThuoc', 'nvchR_KichThuoc'], ''));
        setControlValue('editDongMay', read(['NVCHR_DongMay', 'nvchR_DongMay'], ''));
        setControlValue('editTinhNang', read(['NVCHR_TinhNang', 'nvchR_TinhNang'], ''));
        setControlValue('editRohs', read(['NVCHR_Rohs', 'nvchR_Rohs'], ''));
        setControlValue('editCOCQ', read(['NVCHR_COCQ', 'nvchR_COCQ'], ''));
        setControlValue('editMSDS', read(['NVCHR_MSDS', 'nvchR_MSDS'], ''));
        setControlValue('editAnToan', read(['NVCHR_AnToan', 'nvchR_AnToan'], ''));
        setControlValue('editFileThietKe', read(['NVCHR_FileThietKe', 'nvchR_FileThietKe'], ''));
        setControlValue('editNhaSanXuat', read(['NVCHR_NhaSanXuat', 'nvchR_NhaSanXuat'], ''));
        setControlValue('editStatus', read(['ID_Status', 'iD_Status'], ''));
        setControlValue('editStep', read(['ID_StepBaoGia', 'iD_StepBaoGia'], ''));
        setControlValue('editSoLanUpdate', read(['INT_SoLanUpdate', 'inT_SoLanUpdate'], ''));

        const layBaoGia = read(['BIT_LayBaoGia', 'biT_LayBaoGia'], false);
        setControlValue('editLayBaoGia', (layBaoGia === true || String(layBaoGia).toLowerCase() === 'true') ? 'true' : 'false');
        setControlValue('editLyDo', read(['NVCHR_LyDo', 'nvchR_LyDo'], ''));
        setControlValue('editNgayMuonNhan', toDateInput(read(['DTM_NgayMuonNhan', 'dtM_NgayMuonNhan'], null)));
        setControlValue('editKyHan', toDateInput(read(['DTM_KyHan', 'dtM_KyHan'], null)));

        const urgent = read(['CHR_Gap', 'chR_Gap'], false);
        setControlValue('editGap', (urgent === true || String(urgent).toLowerCase() === 'true') ? 'true' : 'false');

        setControlValue('editDaycreate', toDateInput(read(['DTM_CreateDate', 'dtM_CreateDate'], null)) || '');
        setControlValue('editUpdateLater', toDateInput(read(['DTM_UpdateLater', 'dtM_UpdateLater'], null)) || '');
        setControlValue('editDeadline', toDateInput(read(['DTM_Deadline', 'dtM_Deadline'], null)) || '');

        const isTemplate = read(['BIT_IsTemplate', 'biT_IsTemplate'], '');
        setControlValue('editIsTemplate', isTemplate === '' ? '' : ((isTemplate === true || String(isTemplate).toLowerCase() === 'true') ? 'true' : 'false'));

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

    function collectEditFormDto() {
        const gv = id => document.getElementById(id)?.value || '';
        const toIso = d => {
            if (!d) return null;
            try {
                const parts = d.split('-');
                return new Date(Date.UTC(+parts[0], +parts[1] - 1, +parts[2], 7, 0, 0)).toISOString();
            } catch {
                return null;
            }
        };

        return {
            ID: Number(gv('editRequestId') || 0),
            CHR_MaDon: gv('editMaDon') || '',
            CHR_SectionCode: gv('editSectionCode') || '',
            CHR_SectionName: gv('editSectionName') || '',
            CHR_Phanloai: gv('editPhanLoai') || '',
            CHR_MaThietBi: gv('editMaThietBi') || '',
            CHR_MaHangNoiBo: gv('editMaHangNoiBo') || '',
            CHR_MaHangNCC: gv('editMaHangNCC') || '',
            NVCHR_NameVN: gv('editTenHangVN') || '',
            CHR_NameEN: gv('editTenHangEN') || '',
            INT_SoLuong: gv('editSoLuong') ? parseFloat(gv('editSoLuong')) : -1,
            NVCHR_DonVi: gv('editDonVi') || '',
            NVCHR_ChungLoai: gv('editChungLoai') || '',
            NVCHR_HinhDang: gv('editHinhDang') || '',
            NVCHR_ChatLieu: gv('editChatLieu') || '',
            NVCHR_ThanhPhan: gv('editThanhPhan') || '',
            NVCHR_KichThuoc: gv('editKichThuoc') || '',
            NVCHR_DongMay: gv('editDongMay') || '',
            NVCHR_TinhNang: gv('editTinhNang') || '',
            NVCHR_Rohs: gv('editRohs') || '',
            NVCHR_COCQ: gv('editCOCQ') || '',
            NVCHR_MSDS: gv('editMSDS') || '',
            NVCHR_AnToan: gv('editAnToan') || '',
            NVCHR_FileThietKe: gv('editFileThietKe') || '',
            NVCHR_NhaSanXuat: gv('editNhaSanXuat') || '',
            CHR_MaNCC: gv('editNhaCungCap') || '',
            NVCHR_TenNCC: gv('editTenNCC') || '',
            BIT_LayBaoGia: gv('editLayBaoGia') === 'true',
            NVCHR_LyDo: gv('editLyDo') || '',
            DTM_NgayMuonNhan: toIso(gv('editNgayMuonNhan')),
            DTM_KyHan: toIso(gv('editKyHan')),
            CHR_Gap: gv('editGap') || '',
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

    document.getElementById('btnSaveHistoryEdit')?.addEventListener('click', async function () {
        const T = window.i18nHistoryQuote || {};
        const saveBtn = this;
        const dto = collectEditFormDto();

        if (!dto || !dto.ID) {
            showDialog({
                title: T.Notification || 'Thông báo',
                message: T.MsgNoDataToEdit || 'Không có dữ liệu để chỉnh sửa.',
                type: 'warning'
            });
            return;
        }

        try {
            saveBtn.disabled = true;
            showLoading(T.LoadingData || 'Đang xử lý...');

            const response = await fetch(apiUrl('/History/UpdateBaoGiaById'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            });

            const text = await response.text().catch(() => '');
            if (!response.ok) {
                hideEditModal();
                throw new Error(text || (T.MsgSaveFailed || 'Lưu thất bại'));
            }

            hideEditModal();
            showDialog({
                title: T.Notification || 'Thông báo',
                message: T.MsgSaveSuccess || 'Đã lưu thành công.',
                type: 'success'
            });
            applyFilters(currentPage || 1);
        } catch (err) {
            hideEditModal();
            console.error(err);
            showDialog({
                title: T.Notification || 'Thông báo',
                message: err?.message || (T.MsgSaveFailed || 'Lưu thất bại'),
                type: 'error'
            });
        } finally {
            saveBtn.disabled = false;
            hideLoading();
        }
    });

    // Initial load
    document.addEventListener('DOMContentLoaded', function () {
        applyFilters(1);
    });
})();
