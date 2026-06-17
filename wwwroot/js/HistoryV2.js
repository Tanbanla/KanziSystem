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
    const supplierSelect = document.getElementById('editNhaCungCap');
    const hiddenTenNCC = document.getElementById('editTenNCC');
    const btnExportManaHistory = document.getElementById('btnExportManaHistory');
    let currentPage = 1;
    let pageSize = Number(pageSizeSelect?.value) || 50;
    let currentGroups = [];
    let totalCountServer = 0;
    let serverPaged = false;
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
    function buildSearchableDropdown(container) {
        // Accept either a jQuery object or a DOM node
        const root = (window.jQuery && container && container.jquery) ? container[0] : container || document;

        const selects = root.querySelectorAll ? root.querySelectorAll('select.searchable-select') : [];
        selects.forEach(function (select) {
            if (select.dataset.searchDropdown === 'true') return;

            const options = Array.from(select.options).map(function (opt) {
                return { value: opt.value, text: opt.textContent || opt.innerText || '', selected: opt.selected };
            });

            // Build UI elements
            const wrapper = document.createElement('div'); wrapper.className = 'ms-container';
            const btn = document.createElement('div'); btn.className = 'ms-btn';
            btn.innerHTML = '<span class="ms-values"></span><span class="ms-placeholder"></span><span class="ms-caret">▾</span>';
            const dropdown = document.createElement('div'); dropdown.className = 'ms-dropdown';
            const search = document.createElement('div'); search.className = 'ms-search';
            const T = window.i18nHistoryQuote || {};
            search.innerHTML = '<input type="text" placeholder="' + (T.SearchEllipsis || 'Tìm...') + '" />';
            const list = document.createElement('div'); list.className = 'ms-list';

            function renderList(query) {
                const q = (query || '').toLowerCase();
                list.innerHTML = '';
                let hasItems = false;
                options.forEach(function (opt) {
                    if (!q || opt.text.toLowerCase().includes(q)) {
                        const item = document.createElement('div');
                        item.className = 'ms-item';
                        item.dataset.value = opt.value;
                        item.textContent = opt.text;
                        if (select.value === opt.value || opt.selected) item.classList.add('selected');
                        list.appendChild(item);
                        hasItems = true;
                    }
                });
                if (!hasItems) {
                    const empty = document.createElement('div'); empty.className = 'ms-empty'; empty.textContent = (T.NoResults || 'Không có kết quả');
                    list.appendChild(empty);
                }
            }

            function getRowsForPage(rows, pageIndex) {
                if (!Array.isArray(rows)) return [];
                if (serverPaged) return rows;
                const start = Math.max(0, (pageIndex - 1) * pageSize);
                return rows.slice(start, start + pageSize);
            }

            function updateButtonText() {
                const val = select.value;
                const found = options.find(o => o.value === val);
                const valuesEl = btn.querySelector('.ms-values');
                const placeholderEl = btn.querySelector('.ms-placeholder');
                if (found && found.text) {
                    valuesEl.textContent = found.text;
                    placeholderEl.textContent = '';
                } else {
                    valuesEl.textContent = '';
                    placeholderEl.textContent = (T.SelectPlaceholder || '-- Chọn --');
                }
                // reflect selected state in list items
                list.querySelectorAll('.ms-item').forEach(function (it) {
                    if (it.dataset.value === val) it.classList.add('selected'); else it.classList.remove('selected');
                });
            }

            updateButtonText();
            renderList('');

            dropdown.appendChild(search);
            dropdown.appendChild(list);
            // insert after select
            select.style.display = 'none';
            select.parentNode.insertBefore(wrapper, select.nextSibling);
            wrapper.appendChild(btn);
            wrapper.appendChild(dropdown);

            // store reference for reattaching
            dropdown._wrapper = wrapper;
            dropdown._detached = false;

            // Events
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                // close other dropdowns
                document.querySelectorAll('.ms-dropdown.open').forEach(function (d) {
                    if (d !== dropdown) {
                        d.classList.remove('open');
                        if (d._detached) {
                            d._wrapper.appendChild(d);
                            d.style.position = '';
                            d.style.top = '';
                            d.style.left = '';
                            d.style.width = '';
                            d.style.zIndex = '';
                            d._detached = false;
                        }
                    }
                });

                if (dropdown.classList.contains('open')) {
                    dropdown.classList.remove('open');
                    if (dropdown._detached) {
                        dropdown._wrapper.appendChild(dropdown);
                        dropdown.style.position = '';
                        dropdown.style.top = '';
                        dropdown.style.left = '';
                        dropdown.style.width = '';
                        dropdown.style.zIndex = '';
                        dropdown._detached = false;
                    }
                } else {
                    // attach to body to avoid clipping
                    const rect = btn.getBoundingClientRect();
                    const top = rect.top + window.scrollY + btn.offsetHeight;
                    const left = rect.left + window.scrollX;
                    document.body.appendChild(dropdown);
                    dropdown.style.position = 'absolute';
                    dropdown.style.top = top + 'px';
                    dropdown.style.left = left + 'px';
                    dropdown.style.width = btn.offsetWidth + 'px';
                    dropdown.style.zIndex = 3000;
                    dropdown.classList.add('open');
                    dropdown._detached = true;
                    const inp = search.querySelector('input');
                    if (inp) { inp.value = ''; inp.focus(); }
                    renderList('');
                }
            });

            // clicking outside should close and reattach any open dropdowns
            document.addEventListener('click', function () {
                document.querySelectorAll('.ms-dropdown').forEach(function (d) {
                    if (d.classList.contains('open')) {
                        d.classList.remove('open');
                        if (d._detached) {
                            d._wrapper.appendChild(d);
                            d.style.position = '';
                            d.style.top = '';
                            d.style.left = '';
                            d.style.width = '';
                            d.style.zIndex = '';
                            d._detached = false;
                        }
                    }
                });
            });

            dropdown.addEventListener('click', function (e) { e.stopPropagation(); });

            list.addEventListener('click', function (ev) {
                const it = ev.target.closest('.ms-item');
                if (!it) return;
                const value = it.dataset.value;
                select.value = value;
                // dispatch change events
                try { select.dispatchEvent(new Event('change', { bubbles: true })); } catch (ex) { }
                updateButtonText();
                dropdown.classList.remove('open');
                if (dropdown._detached) {
                    dropdown._wrapper.appendChild(dropdown);
                    dropdown.style.position = '';
                    dropdown.style.top = '';
                    dropdown.style.left = '';
                    dropdown.style.width = '';
                    dropdown.style.zIndex = '';
                    dropdown._detached = false;
                }
            });

            const inp = search.querySelector('input');
            if (inp) {
                inp.addEventListener('input', function () { renderList(this.value); });
            }

            // Mark enhanced
            select.dataset.searchDropdown = 'true';
            // When value changes programmatically, update UI text
            try { select.addEventListener('change', updateButtonText); } catch { }
        });
    }
    if (window.jQuery) buildSearchableDropdown($(document)); else buildSearchableDropdown(document);
    document.addEventListener('DOMContentLoaded', function () { buildSearchableDropdown(document); });

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

                // If import does not require selecting approver, show success
                if (!importResult?.isReturn) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Cập nhật người phê duyệt thành công'), type: 'success' });
                    applyFilters(1);
                    return;
                }

                // If import indicates a return flow, prompt user to select approver/section
                const step = 2;
                const section = importResult?.sectionCode || '';

                // Require user to select an approver when isReturn === true
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
            date: document.getElementById('dateTo')?.value || null,
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
            return { rows: data, totalCount: data.length, serverPaged: false };
        }

        const rows = getValue(data, ['data', 'Data'], []);
        const totalCount = Number(getValue(data, ['totalCount', 'TotalCount'], Array.isArray(rows) ? rows.length : 0)) || 0;
        return {
            rows: Array.isArray(rows) ? rows : [],
            totalCount,
            serverPaged: true
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

    function getRowsForPage(rows, pageIndex) {
        if (!Array.isArray(rows)) return [];
        if (serverPaged) return rows;

        return rows;
    }

    function approvalCell(name, time) {
        const text = String(name || '').trim();
        if (!text) return '<td></td>';
        const dt = formatDateTime(time);
        return `<td style="background:#cfe3c6;">${escapeHtml(text)}${dt ? `<div class="small text-muted">${escapeHtml(dt)}</div>` : ''}</td>`;
    }

    function renderTable(rows) {
        if (!tblBody) return;

        if (!rows || rows.length === 0) {
            const colSpan = document.querySelectorAll('.approval-table thead tr:last-child th')?.length + 14 || 25;
            tblBody.innerHTML = `<tr><td colspan="${colSpan}" class="text-center text-muted py-3">${escapeHtml(window.i18nHistoryQuote?.MsgNoDataToEdit || 'Không có dữ liệu')}</td></tr>`;
            return;
        }

        const startNo = (currentPage - 1) * pageSize + 1;
        const html = new Array(rows.length);

        for (let i = 0; i < rows.length; i++) {
            const row = rows[i] || {};
            var stepName = window.i18nHistoryQuote?.CHR_StepName;
            const deadline = getValue(row, ['DTM_KyHan']);

            html[i] = `
                <tr>
                    <td>${startNo + i}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaDon']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaHangNoiBo']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaHangNCC']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_NameEN']))}</td>
                    <td>${escapeHtml(getValue(row, ['NCC_1']))}</td>
                    <td>${escapeHtml(getValue(row, ['NCC_2']))}</td>
                    <td>${escapeHtml(getValue(row, ['NCC_3']))}</td>
                    <td>${escapeHtml(getValue(row, ['NCC_4']))}</td>
                    <td>${escapeHtml(getValue(row, ['NCC_5']))}</td>
                    <td style="${isOverdue(deadline) ? 'background:red;color:#fff;' : ''}">${escapeHtml(formatDate(deadline))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_CreateBy']))}</td>
                    ${approvalCell(getValue(row, ['QLSC_Approve']), getValue(row, ['QLSC_Time']))}
                    ${approvalCell(getValue(row, ['QLTC_Approve']), getValue(row, ['QLTC_Time']))}
                    ${approvalCell(getValue(row, ['PIC_Approve']), getValue(row, ['PIC_Time']))}
                    ${approvalCell(getValue(row, ['QLSC1_Approve']), getValue(row, ['QLSC1_Time']))}
                    ${approvalCell(getValue(row, ['PIC_PickNCC']), getValue(row, ['PIC_PickNCC_Time']))}
                    ${approvalCell(getValue(row, ['QLSC_PickNCC']), getValue(row, ['QLSC_PickNCC_Time']))}
                    ${approvalCell(getValue(row, ['QLTC_PickNCC']), getValue(row, ['QLTC_PickNCC_Time']))}
                    ${approvalCell(getValue(row, ['DEFT_PickNCC']), getValue(row, ['DEFT_PickNCC_Time']))}
                    <td>${escapeHtml(getValue(row, ['NCC_DuocChon']))}</td>
                    <td>${escapeHtml(getValue(row, ['NVCHR_ReasonPick']))}</td>
                    <td>${escapeHtml(getValue(row, ['NVCHR_File']))}</td>
                    <td>${escapeHtml(getValue(row, [stepName]))}</td>
                    <td>
                        <div class="action-buttons" role="group" aria-label="${escapeHtml(window.i18nHistoryQuote?.Actions || 'Actions')}">
                            <button type="button" class="btn btn-outline-info btn-view-history" title="${escapeHtml(window.i18nHistoryQuote?.ViewHistoryTooltip || 'View history')}" data-madon="${escapeHtml(getValue(row, ['CHR_MaDon']))}"><i class="fas fa-history"></i></button>
                            <button type="button" class="btn btn-outline-warning btn-return-history" title="${escapeHtml(window.i18nHistoryQuote?.ReturnTooltip || 'Return')}" data-madon="${escapeHtml(getValue(row, ['CHR_MaDon']))}"><i class="fas fa-undo"></i></button>
                            <button type="button" class="btn btn-outline-danger btn-delete-history" title="${escapeHtml(window.i18nHistoryQuote?.DeleteTooltip || 'Delete')}" data-madon="${escapeHtml(getValue(row, ['CHR_MaDon']))}"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>
                </tr>`;
        }

        tblBody.innerHTML = html.join('');
    }

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

    function renderSummaryProcessingStatus(result) {
        const row = Array.isArray(result) ? (result[0] || {}) : (result || {});
        document.getElementById('statCompletedOrders').textContent = getValue(row, ['SoDonHoanThanh', 'soDonHoanThanh'], 0);
        document.getElementById('statProcessingOrders').textContent = getValue(row, ['SoDonDangXuLy', 'soDonDangXuLy'], 0);
        document.getElementById('statUnprocessedOrders').textContent = getValue(row, ['SoDonChuaXuLy', 'soDonChuaXuLy'], 0);
    }

    function renderSummaryWaitingSupplier(result) {
        const row = Array.isArray(result) ? (result[0] || {}) : (result || {});
        document.getElementById('statWaitingSupplier').textContent = getValue(row, ['TongSoHang_DangCho', 'tongSoHang_DangCho'], 0);
        document.getElementById('statSelectedSupplier').textContent = getValue(row, ['TongSoHang_DaChon', 'tongSoHang_DaChon'], 0);
        document.getElementById('statBothStatusSupplier').textContent = getValue(row, ['TongSoHang_HaiTrangThai', 'tongSoHang_HaiTrangThai'], 0);
        document.getElementById('statTotalSupplier').textContent = getValue(row, ['TongSoHang_TatCa', 'tongSoHang_TatCa'], 0);
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
            serverPaged = parsed.serverPaged;

            renderTable(getRowsForPage(currentGroups, currentPage));
            renderPagination(currentPage, totalCountServer);
            renderSummaryCountQuotation(countQuotationResult);
            renderSummaryCountStatus(countStatusResult);
            renderSummaryProcessingStatus(processingStatusResult);
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

    // Initial load
    document.addEventListener('DOMContentLoaded', function () {
        applyFilters(1);
    });
})();
