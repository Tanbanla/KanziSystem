(function () {
    const tblBody = document.getElementById('historyGroupTableBody') || document.querySelector('.approval-table tbody');
    const statusFilter = document.getElementById('statusFilter');
    const btnApply = document.getElementById('btnApplyFilters');
    const btnReset = document.getElementById('btnResetFilters');
    const paginationEl = document.getElementById('historyPagination');
    const paginationInfoEl = document.getElementById('historyPaginationInfo');
    const pageSizeSelect = document.getElementById('historyPageSize');
    const pageIndexInput = document.getElementById('historyPageIndex');
    const btnGoPage = document.getElementById('historyGoPage');
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
    const statusMap = new Map((window.HistoryData?.status || []).map(s => [
        s?.VCHR_CodeStatus,
        s?.DisplayName || s?.NVCHR_TenStatus || s?.CHR_TenStatusEN || s?.CHR_TenStatusJP || ''
    ]));

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

        if (pageIndexInput) {
            pageIndexInput.min = '1';
            pageIndexInput.max = String(totalPages);
            pageIndexInput.value = String(currentPage);
        }
    }

    function navigateToPage(targetPage) {
        const totalPages = Math.max(1, Math.ceil((totalCountServer || 0) / pageSize));
        const safeTarget = Math.min(Math.max(1, Number(targetPage) || 1), totalPages);
        if (safeTarget === currentPage) return;

        if (serverPaged) {
            applyFilters(safeTarget);
        } else {
            currentPage = safeTarget;
            renderTable(getRowsForPage(currentGroups, currentPage));
            renderPagination(currentPage, totalCountServer);
        }
    }

    function getRowsForPage(rows, pageIndex) {
        if (!Array.isArray(rows)) return [];
        if (serverPaged) return rows;

        const safePage = Math.max(1, Number(pageIndex) || 1);
        const start = (safePage - 1) * pageSize;
        return rows.slice(start, start + pageSize);
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
            const statusCode = getValue(row, ['ID_Status', 'id_Status', 'status'], '');
            const statusText = statusMap.get(statusCode) || statusCode || '';
            const deadline = getValue(row, ['DTM_KyHan', 'dtm_KyHan']);

            html[i] = `
                <tr>
                    <td>${startNo + i}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaDon', 'chr_MaDon']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaHangNoiBo', 'chr_MaHangNoiBo']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_MaHangNCC', 'chr_MaHangNCC']))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_NameEN', 'chr_NameEN']))}</td>
                    <td>${escapeHtml(getValue(row, ['Vender1', 'vender1']))}</td>
                    <td>${escapeHtml(getValue(row, ['Vender2', 'vender2']))}</td>
                    <td>${escapeHtml(getValue(row, ['Vender3', 'vender3']))}</td>
                    <td>${escapeHtml(getValue(row, ['Vender4', 'vender4']))}</td>
                    <td>${escapeHtml(getValue(row, ['Vender5', 'vender5']))}</td>
                    <td style="${isOverdue(deadline) ? 'background:red;color:#fff;' : ''}">${escapeHtml(formatDate(deadline))}</td>
                    <td>${escapeHtml(getValue(row, ['CHR_CreateBy', 'chr_CreateBy']))}</td>
                    ${approvalCell(getValue(row, ['QLSC_Approve', 'qlsC_Approve']), getValue(row, ['QLSC_Time', 'qlsC_Time']))}
                    ${approvalCell(getValue(row, ['QLTC_Approve', 'qltC_Approve']), getValue(row, ['QLTC_Time', 'qltC_Time']))}
                    ${approvalCell(getValue(row, ['PIC_Approve', 'piC_Approve']), getValue(row, ['PIC_Time', 'piC_Time']))}
                    ${approvalCell(getValue(row, ['QLSC1_Approve', 'qlsC1_Approve']), getValue(row, ['QLSC1_Time', 'qlsC1_Time']))}
                    ${approvalCell(getValue(row, ['PIC_PickNCC', 'piC_PickNCC']), getValue(row, ['PIC_PickNCC_Time', 'piC_PickNCC_Time']))}
                    ${approvalCell(getValue(row, ['QLSC_PickNCC', 'qlsC_PickNCC']), getValue(row, ['QLSC_PickNCC_Time', 'qlsC_PickNCC_Time']))}
                    ${approvalCell(getValue(row, ['QLTC_PickNCC', 'qltC_PickNCC']), getValue(row, ['QLTC_PickNCC_Time', 'qltC_PickNCC_Time']))}
                    ${approvalCell(getValue(row, ['DEFT_PickNCC', 'defT_PickNCC']), getValue(row, ['DEFT_PickNCC_Time', 'defT_PickNCC_Time']))}
                    <td>${escapeHtml(getValue(row, ['PickVendor', 'pickVendor']))}</td>
                    <td>${escapeHtml(getValue(row, ['PickReason', 'pickReason']))}</td>
                    <td>${escapeHtml(getValue(row, ['PickLink', 'pickLink']))}</td>
                    <td>${escapeHtml(statusText)}</td>
                    <td>
                        <div class="action-buttons" role="group" aria-label="Actions">
                            <button type="button" class="btn btn-outline-info btn-view-history" title="Xem lịch sử" data-madon="${escapeHtml(getValue(row, ['CHR_MaDon', 'chr_MaDon']))}"><i class="fas fa-history"></i></button>
                            <button type="button" class="btn btn-outline-warning btn-return-history" title="Trả lại" data-madon="${escapeHtml(getValue(row, ['CHR_MaDon', 'chr_MaDon']))}"><i class="fas fa-undo"></i></button>
                            <button type="button" class="btn btn-outline-danger btn-delete-history" title="Xóa" data-madon="${escapeHtml(getValue(row, ['CHR_MaDon', 'chr_MaDon']))}"><i class="fas fa-trash"></i></button>
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
        document.getElementById('statCompletedOrders').textContent = getValue(row, ['SoDonHoanThanh', 'soDonHoanThanh'], 0);
        document.getElementById('statProcessingOrders').textContent = getValue(row, ['SoDonDangXuLy', 'soDonDangXuLy'], 0);
        document.getElementById('statUnprocessedOrders').textContent = getValue(row, ['SoDonChuaXuLy', 'soDonChuaXuLy'], 0);
    }

    async function applyFilters(pageIndex = 1) {
        if (state.requestController) state.requestController.abort();
        state.requestController = new AbortController();

        const payload = buildSearchPayload(pageIndex);
        currentPage = pageIndex;

        try {
            showLoading(window.i18nHistoryQuote?.Filter || 'Đang lọc dữ liệu...');

            const [historyResult, countQuotationResult, countStatusResult] = await Promise.all([
                postJson('/History/SearchHistory', payload, state.requestController.signal),
                postJson('/History/GetCountQuotation', payload, state.requestController.signal),
                postJson('/History/GetCountStatus', payload, state.requestController.signal)
            ]);

            const parsed = normalizeListResponse(historyResult);
            currentGroups = parsed.rows;
            totalCountServer = historyResult.totalCount;
            serverPaged = parsed.serverPaged;

            renderTable(getRowsForPage(currentGroups, currentPage));
            renderPagination(currentPage, totalCountServer);
            renderSummaryCountQuotation(countQuotationResult);
            renderSummaryCountStatus(countStatusResult);
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
        const btn = e.target.closest('button[data-page]');
        if (!btn || btn.parentElement.classList.contains('disabled')) return;

        const totalPages = Math.max(1, Math.ceil((totalCountServer || 0) / pageSize));

        const page = btn.dataset.page;
        if (page === 'prev') {
            if (currentPage > 1) {
                navigateToPage(currentPage - 1);
            }
            return;
        }

        if (page === 'next') {
            if (currentPage < totalPages) {
                navigateToPage(currentPage + 1);
            }
            return;
        }

        const targetPage = Number(page);
        if (Number.isInteger(targetPage) && targetPage > 0 && targetPage !== currentPage) {
            navigateToPage(targetPage);
        }
    });

    pageSizeSelect?.addEventListener('change', function () {
        const nextSize = Number(this.value);
        if (!Number.isInteger(nextSize) || nextSize <= 0) return;
        pageSize = nextSize;
        applyFilters(1);
    });

    btnGoPage?.addEventListener('click', function () {
        const target = Number(pageIndexInput?.value);
        if (!Number.isInteger(target) || target <= 0) return;
        navigateToPage(target);
    });

    pageIndexInput?.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        e.preventDefault();
        const target = Number(pageIndexInput?.value);
        if (!Number.isInteger(target) || target <= 0) return;
        navigateToPage(target);
    });

    // Initial load
    document.addEventListener('DOMContentLoaded', function () {
        applyFilters(1);
    });
})();
