if (typeof window.buildSearchableDropdown !== 'function') {

    // show dialog
    function getDialogEls() {
        const overlay = document.getElementById('cmDialogOverlay');
        const titleEl = document.getElementById('cmDialogTitle');
        const bodyEl = document.getElementById('cmDialogBody');
        const footerEl = document.getElementById('cmDialogFooter');
        return { overlay, titleEl, bodyEl, footerEl };
    }
    function showDialog({ title = (window.i18nQuotationResults && window.i18nQuotationResults.Notification) || 'Thông báo', message = '', type = 'info', buttons } = {}) {
        const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
        if (!overlay) return alert(message);

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
        okBtn.textContent = (buttons && buttons.okText) || ((window.i18nQuotationResults && window.i18nQuotationResults.DialogOk) || 'Đồng ý');
        okBtn.addEventListener('click', () => hideDialog());
        footerEl.appendChild(okBtn);

        overlay.setAttribute('aria-hidden', 'false');
        overlay.style.display = 'flex';
        attachDialogCloseHandlers();
    }
    function showPrompt({ title = (window.i18nQuotationResults && window.i18nQuotationResults.Notification) || 'Thông báo', message = '', placeholder = '', defaultValue = '' } = {}) {
        return new Promise((resolve) => {
            const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
            if (!overlay) {
                const val = window.prompt(message || title, defaultValue || '');
                resolve(val === null ? null : (val || '').toString());
                return;
            }
            try {
                if (overlay.parentElement !== document.body) document.body.appendChild(overlay);
            } catch (e) { }

            titleEl.textContent = title;
            bodyEl.innerHTML = '';
            const container = document.createElement('div');
            container.className = 'd-flex flex-column gap-2';
            if (message) {
                const msg = document.createElement('div');
                msg.innerHTML = message;
                container.appendChild(msg);
            }
            const inp = document.createElement('input');
            inp.type = 'text';
            inp.className = 'form-control';
            inp.placeholder = placeholder || '';
            inp.value = defaultValue || '';
            container.appendChild(inp);
            bodyEl.appendChild(container);

            footerEl.innerHTML = '';
            const btnCancel = document.createElement('button');
            btnCancel.className = 'cm-btn cm-btn-outline';
            btnCancel.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.Cancel) || 'Hủy';
            btnCancel.addEventListener('click', () => {
                hideDialog();
                resolve(null);
            });
            const btnOk = document.createElement('button');
            btnOk.className = 'cm-btn cm-btn-primary';
            btnOk.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.Confirm) || 'Đồng ý';
            btnOk.addEventListener('click', () => {
                const v = inp.value == null ? '' : inp.value.toString();
                hideDialog();
                resolve(v.trim());
            });
            footerEl.appendChild(btnCancel);
            footerEl.appendChild(btnOk);

            // wire up global pending resolver so overlay/close buttons can cancel the prompt
            window.__cmPendingResolve = function (v) { try { resolve(v === false ? null : v); } catch { } window.__cmPendingResolve = null; };

            overlay.setAttribute('aria-hidden', 'false');
            overlay.style.display = 'flex';
            attachDialogCloseHandlers();
            // focus
            setTimeout(() => { try { inp.focus(); inp.select(); } catch { } }, 50);
        });
    }
    function hideDialog() {
        const { overlay } = getDialogEls();
        if (overlay) {
            overlay.style.display = 'none';
            overlay.setAttribute('aria-hidden', 'true');
        }
    }
    // attach close handlers
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
    // Tìm kiếm
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
            const T = window.i18nQuotationResults || {};
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
                    const empty = document.createElement('div'); empty.className = 'ms-empty'; empty.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.NoResults) || 'Không có kết quả';
                    list.appendChild(empty);
                }
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
                    placeholderEl.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.SelectPlaceholder) || '-- Chọn --';
                }
            }

            // update when underlying select changes programmatically
            select.addEventListener('change', function () {
                updateButtonText();
            });

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
        });
    }

    // expose globally
    window.buildSearchableDropdown = buildSearchableDropdown;
}

// Auto-initialize when page is ready so pages that include this script
// don't need to call buildSearchableDropdown manually.
(function () {
    function run() {
        try { if (typeof window.buildSearchableDropdown === 'function') window.buildSearchableDropdown(document); } catch (e) { /* ignore errors */ }
    }
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', run);
    } else { run(); }
})();
function showModal() {
    const modalEl = document.getElementById('detailModal');
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
    const modalEl = document.getElementById('detailModal');
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
// Pagination and search logic for SelectQuoteSection page
(function () {
    const state = {
        pageIndex: 1,
        pageSize: 20,
        totalCount: 0
    };

    function updateSummary(total) {
        const el = document.getElementById('summaryText');
        if (el) el.textContent = window.i18nQuotationResults ? window.i18nQuotationResults.SummaryRecords.replace('{0}', total) : `Tổng số: ${total}`;
    }

    function renderTable(items) {
        const tbody = document.getElementById('sectionRequestBody');
        if (!tbody) return;
        // helper: map status code/text to badge class similar to Quotation_Results
        function mapStatusClass(code) {
            if (!code && code !== 0) return 'bg-secondary';
            const s = String(code).toUpperCase();
            if (s === 'WAITING_NCC' || s === 'WAIT_NCC') return 'bg-warning text-dark';
            if (s === 'WAITING_PICK_NCC' || s === 'PICKED') return 'bg-success';
            if (s === 'WAITING_APPROVER' || s === 'CONFIRMED') return 'bg-primary';
            return 'bg-secondary';
        }
        function mapStatusText(code) {
            const T = window.i18nQuotationResults || {};
            switch ((code || '').toString()) {
                case 'WAITING_NCC': return T.WaitPickApSupplier || 'Chờ báo gía nhà cung cấp';
                case 'WAITING_PICK_NCC': return T.SupplierApSelected || 'Chờ chọn nhà cung cấp';
                case 'WAITING_APPROVER': return T.WaitApConfirmName || 'Chờ phê duyệt';
                case 'PICKED': return T.SupplierSelected || 'Đã chọn nhà cung cấp';
                default: return code || '';
            }
        }
        // Simple grouped view by CHR_MaDon
        const groups = {};
        items.forEach(d => {
            const key = d.CHR_MaDon || '(No MaDon)';
            if (!groups[key]) groups[key] = [];
            groups[key].push(d);
        });
        // expose groups for detail rendering
        window._selectQuoteGroups = groups;
        tbody.innerHTML = '';
        Object.keys(groups).forEach(maDon => {
            const grp = groups[maDon];
            const tr = document.createElement('tr');
            tr.className = 'group-row';
            // compute aggregates
            const totalQty = grp.reduce((s, it) => s + (Number(it.INT_SoLuong) || 0), 0);
            const first = grp[0] || {};
            const orderCode = maDon;
            const section = first.CHR_SectionName || first.CHR_SectionCode || '';
            const material = first.CHR_MaHangNoiBo || '';
            const name = first.NVCHR_NameVN || '';
            const unit = first.NVCHR_DonVi || '';
            const supplier = first.NVCHR_NameNCC || first.NVCHR_NameNCC || '';
            const wantDate = first.DTM_NgayMuonNhan ? new Date(first.DTM_NgayMuonNhan).toLocaleDateString() : '';
            const status = first.status || first.CHR_TrangThai || '';
            const statusClass = mapStatusClass(status);
            const statusText = mapStatusText(status);

            tr.innerHTML = `
                <td class="text-center"><input type="checkbox" class="group-select" data-madon="${maDon}"></td>
                <td class="text-start">${orderCode}</td>
                <td class="text-start">${section}</td>
                <td class="text-start">${material}</td>
                <td class="text-start">${name} <small class="text-muted">(${grp.length})</small></td>
                <td class="text-end">${totalQty || ''}</td>
                <td class="text-start">${unit}</td>
                <td class="text-start">${supplier}</td>
                <td class="text-center">${wantDate}</td>
                <td class="text-center"><span class="status-badge ${statusClass}">${statusText}</span></td>
                <td class="text-center"><button type="button" class="btn btn-sm btn-outline-primary btn-view-detail" data-madon="${maDon}">Chi tiết</button></td>
            `;
            tbody.appendChild(tr);
        });

        // wire detail buttons and group selects
        tbody.querySelectorAll('.btn-view-detail').forEach(btn => {
            btn.onclick = function () { const md = this.dataset.madon; if (md) showGroupDetail(md); };
        });
        tbody.querySelectorAll('.group-select').forEach(cb => {
            cb.addEventListener('change', function () {
                window._selectedGroups = window._selectedGroups || new Set();
                const md = this.dataset.madon;
                if (this.checked) window._selectedGroups.add(md); else window._selectedGroups.delete(md);
                // update selectAll checkbox
                const all = document.querySelectorAll('#sectionRequestBody .group-select');
                const checked = Array.from(all).every(x => x.checked);
                const selectAll = document.getElementById('selectAll'); if (selectAll) selectAll.checked = checked;
            });
        });
    }

    function showGroupDetail(maDon) {
        const groups = window._selectQuoteGroups || {};
        const items = groups[maDon] || [];
        // populate modal header basic info from first item
        const first = items[0] || {};
        document.getElementById('madonhang').textContent = maDon;
        document.getElementById('mpb_yc').textContent = first.CHR_SectionCode || '';
        document.getElementById('tenphongban').textContent = first.CHR_SectionName || '';
        document.getElementById('nyc').textContent = first.DTM_NgayMuonNhan ? new Date(first.DTM_NgayMuonNhan).toLocaleDateString() : '';
        document.getElementById('thmm').textContent = first.DTM_KyHan ? new Date(first.DTM_KyHan).toLocaleDateString() : '';
        document.getElementById('requester').textContent = first.CHR_CreateBy || '-';
        document.getElementById('id_request').textContent = first.ID || maDon;
        document.getElementById('step').textContent = first.ID_StepBaoGia || '';
        document.getElementById('regency').textContent = first.ID_Status || '';

        // urgent badge
        const ub = document.getElementById('urgent-badge');
        const gap = first.CHR_Gap;
        const isUrgent = gap === true || String(gap).toLowerCase() === 'true' || String(gap) === '1' || String(gap).toLowerCase() === 'o';
        if (ub) ub.style.display = isUrgent ? '' : 'none';

        const body = document.getElementById('detailModalBody');
        if (!body) return;
        body.innerHTML = '';
        // render rows using DOM for safety
        const frag = document.createDocumentFragment();
        // helper to render mismatch styling for vendor comparison fields
        const mismatchStyle = (v) => {
            if (v === false || v === 0 || v === '0' || String(v).toLowerCase() === 'false') {
                return 'color: #a00; background-color: #ffecec;';
            }
            return '';
        };
        const getVal = (obj, ...names) => {
            if (!obj) return '';
            for (const n of names) {
                if (obj[n] !== undefined && obj[n] !== null) return obj[n];
                const alt = Object.keys(obj).find(k => k.toLowerCase() === (n || '').toLowerCase());
                if (alt && obj[alt] !== undefined && obj[alt] !== null) return obj[alt];
            }
            return '';
        };
        const formatDate = (val) => {
            if (!val) return '';
            try {
                const d = new Date(val);
                if (!isNaN(d.getTime())) return d.toLocaleDateString();
            } catch { }
            return String(val || '');
        };
        const fmtNum = v => { try { return v != null && v !== '' ? Number(v).toLocaleString() : ''; } catch { return v || ''; } };
        items.forEach((d, idx) => {
            const tr = document.createElement('tr');
            const pick = getVal(d, 'BIT_Select', 'bit_Select');
            tr.className = 'text-center' + (pick === false || String(pick).toLowerCase() === 'false' ? ' table-secondary' : '');

            const addTd = (txt, cls, style) => { const td = document.createElement('td'); td.textContent = txt == null ? '' : String(txt); if (cls) td.className = cls; if (style) td.style.cssText = style; return td; };

            tr.appendChild(addTd(idx + 1));
            tr.appendChild(addTd(getVal(d, 'chR_MaHangNoiBo', 'CHR_MaHangNoiBo', 'CHR_MaHangNoiBo')));
            tr.appendChild(addTd(getVal(d, 'chR_MaHangNoiBo', 'CHR_MaHangNoiBo')));
            tr.appendChild(addTd(getVal(d, 'nvchR_ChungLoai', 'NVCHR_ChungLoai')));
            tr.appendChild(addTd(getVal(d, 'chR_Phanloai', 'CHR_Phanloai')));
            tr.appendChild(addTd(getVal(d, 'chR_MaHangNCC', 'CHR_MaHangNCC')));
            // name VN/EN combined
            const nameVN = getVal(d, 'nvchR_NameVN', 'NVCHR_NameVN') || '';
            const nameEN = getVal(d, 'nvchR_NameEN', 'NVCHR_NameEN') || '';
            tr.appendChild(addTd((nameVN + (nameEN ? ' / ' + nameEN : '')).trim(), 'text-start'));

            tr.appendChild(addTd(getVal(d, 'inT_SoLuong', 'INT_SoLuong') || '', 'text-center'));
            tr.appendChild(addTd(getVal(d, 'nvchR_DonVi', 'NVCHR_DonVi') || '', 'text-center'));
            tr.appendChild(addTd(getVal(d, 'nvchR_HinhDang', 'NVCHR_HinhDang')));
            tr.appendChild(addTd(getVal(d, 'nvchR_ChatLieu', 'NVCHR_ChatLieu')));
            tr.appendChild(addTd(getVal(d, 'nvchR_ThanhPhan', 'NVCHR_ThanhPhan')));
            tr.appendChild(addTd(getVal(d, 'nvchR_KichThuoc', 'NVCHR_KichThuoc')));
            tr.appendChild(addTd(getVal(d, 'nvchR_DongMay', 'NVCHR_DongMay')));
            tr.appendChild(addTd(getVal(d, 'nvchR_TinhNang', 'NVCHR_TinhNang')));

            tr.appendChild(addTd(getVal(d, 'nvchR_FileThietKe', 'NVCHR_FileThietKe')));
            tr.appendChild(addTd(getVal(d, 'nvchR_NhaSanXuat', 'NVCHR_NhaSanXuat')));
            tr.appendChild(addTd(getVal(d, 'chR_MaNCC', 'CHR_MaNCC')));
            tr.appendChild(addTd(getVal(d, 'nvchR_TenNCC', 'NVCHR_TenNCC')));
            tr.appendChild(addTd(getVal(d, 'nvchR_Rohs', 'NVCHR_Rohs')));
            tr.appendChild(addTd(getVal(d, 'nvchR_COCQ', 'NVCHR_COCQ')));
            tr.appendChild(addTd(getVal(d, 'nvchR_MSDS', 'NVCHR_MSDS')));
            tr.appendChild(addTd(getVal(d, 'nvchR_AnToan', 'NVCHR_AnToan')));

            tr.appendChild(addTd(formatDate(getVal(d, 'dtM_KyHan', 'DTM_KyHan')), 'text-center'));
            const gap = getVal(d, 'chR_Gap', 'CHR_Gap');
            const gapLabel = gap != null && gap !== '' ? (String(gap).toLowerCase() === 'true' || String(gap) === '1' ? 'O' : 'X') : '';
            tr.appendChild(addTd(gapLabel, 'text-center'));
            const lay = getVal(d, 'biT_LayBaoGia', 'BIT_LayBaoGia');
            const layLabel = lay != null && lay !== '' ? (String(lay).toLowerCase() === 'true' || String(lay) === '1' ? 'O' : 'X') : '';
            tr.appendChild(addTd(layLabel, 'text-center'));
            tr.appendChild(addTd(getVal(d, 'nvchR_LyDo', 'NVCHR_LyDo')));

            // Vendor input columns (read-only)
            tr.appendChild(addTd(getVal(d, 'CHR_MaNCC', 'chR_MaNCC')));
            tr.appendChild(addTd(getVal(d, 'NVCHR_NameNCC', 'nvchR_NameNCC'), 'text-start'));
            tr.appendChild(addTd(getVal(d, 'CHR_MaHangNCC', 'chR_MaHangNCC'), null, mismatchStyle(getVal(d, 'IsMatch_MaHangNCC', 'IsMatch_MaHangNCC'))));
            tr.appendChild(addTd(getVal(d, 'NVCHR_TenHangHQ', 'nvchR_TenHangHQ'), 'text-start', mismatchStyle(getVal(d, 'IsMatch_NameVN', 'IsMatch_NameVN'))));
            tr.appendChild(addTd(getVal(d, 'NameENByNCC', 'nameENByNCC'), null, mismatchStyle(getVal(d, 'IsMatch_NameEN', 'IsMatch_NameEN'))));
            tr.appendChild(addTd(getVal(d, 'soluong', 'INT_SoLuong', 'soluong') || '', 'text-center', mismatchStyle(getVal(d, 'IsMatch_SoLuong', 'IsMatch_SoLuong'))));
            tr.appendChild(addTd(getVal(d, 'donvi', 'NVCHR_DonVi') || '', 'text-center', mismatchStyle(getVal(d, 'IsMatch_DonVi', 'IsMatch_DonVi'))));
            tr.appendChild(addTd(fmtNum(getVal(d, 'FL_USD', 'fl_usd')), 'text-end'));
            tr.appendChild(addTd(fmtNum(getVal(d, 'FL_VND', 'fl_vnd')), 'text-end'));
            tr.appendChild(addTd(getVal(d, 'NVCHR_MOQ', 'nvchr_MOQ')));
            tr.appendChild(addTd(getVal(d, 'DTM_LeadTime', 'dtm_LeadTime')));
            tr.appendChild(addTd(formatDate(getVal(d, 'DTM_ShipTime', 'dtm_ShipTime')), null, mismatchStyle(getVal(d, 'IsMatch_Ngay', 'IsMatch_Ngay'))));
            tr.appendChild(addTd(getVal(d, 'VCHR_Rohs', 'vchr_Rohs'), null, mismatchStyle(getVal(d, 'IsMatch_Rohs', 'IsMatch_Rohs'))));
            tr.appendChild(addTd(getVal(d, 'VCHR_COCQ', 'vchr_COCQ'), null, mismatchStyle(getVal(d, 'IsMatch_COCQ', 'IsMatch_COCQ'))));
            tr.appendChild(addTd(getVal(d, 'VCHR_MSDS', 'vchr_MSDS'), null, mismatchStyle(getVal(d, 'IsMatch_MSDS', 'IsMatch_MSDS'))));
            tr.appendChild(addTd(getVal(d, 'VCHR_AnToan', 'vchr_AnToan'), null, mismatchStyle(getVal(d, 'IsMatch_AnToan', 'IsMatch_AnToan'))));
            tr.appendChild(addTd(getVal(d, 'VCHR_CamKet', 'vchr_CamKet'), null, mismatchStyle(getVal(d, 'IsMatchCamKet', 'IsMatchCamKet'))));
            tr.appendChild(addTd(getVal(d, 'NVCHR_DeliveryTerm', 'nvchr_DeliveryTerm')));
            tr.appendChild(addTd(getVal(d, 'NVCHR_PaymentTerm', 'nvchr_PaymentTerm')));
            tr.appendChild(addTd(getVal(d, 'NVCHR_File', 'nvchr_File')));
            tr.appendChild(addTd(formatDate(getVal(d, 'DTM_EffectiveDate', 'dtm_EffectiveDate'))));
            tr.appendChild(addTd(formatDate(getVal(d, 'DTM_ExpiryDate', 'dtm_ExpiryDate'))));

            // System total (try VND then USD)
            const totalSys = (getVal(d, 'FL_VND', 'fl_vnd') || getVal(d, 'TotalVND')) ? (fmtNum(getVal(d, 'FL_VND', 'fl_vnd') || getVal(d, 'TotalVND')) + ' VND') : (getVal(d, 'FL_USD', 'fl_usd') ? (fmtNum(getVal(d, 'FL_USD', 'fl_vnd')) + ' USD') : '');
            tr.appendChild(addTd(totalSys, 'text-center'));

            // PIC selection and reason (display only)
            //const pick = getVal(d, 'BIT_Select', 'bit_Select');
            const pickLabel = pick === true || String(pick).toLowerCase() === 'true' ? 'O' : (pick === false || String(pick).toLowerCase() === 'false' ? 'X' : '');
            tr.appendChild(addTd(pickLabel, 'text-center'));
            tr.appendChild(addTd(getVal(d, 'NVCHR_ReasonPick', 'nvchr_ReasonPick') || getVal(d, 'NVCHR_LyDo', 'nvchr_LyDo')));
            tr.appendChild(addTd(getVal(d, 'NVCHR_Note', 'nvchr_Note')));
            // approval
            tr.appendChild(addTd(getVal(d, 'userQlsc')));
            tr.appendChild(addTd((getVal(d, 'lyDoQlsc') === null || getVal(d, 'lyDoQlsc') === "") ? "OK" : "NG"));
            tr.appendChild(addTd(getVal(d, 'lyDoQlsc')));

            tr.appendChild(addTd(getVal(d, 'userQltc')));
            tr.appendChild(addTd((getVal(d, 'lyDoQltc') === null || getVal(d, 'lyDoQltc') === "") ? "OK" : "NG"));
            tr.appendChild(addTd(getVal(d, 'lyDoQltc')));

            tr.appendChild(addTd(getVal(d, 'userDeft')));
            tr.appendChild(addTd((getVal(d, 'lyDoDeft') === null || getVal(d, 'lyDoDeft') === "") ? "OK" : "NG"));
            tr.appendChild(addTd(getVal(d, 'lyDoDeft')));

            frag.appendChild(tr);
        });
        body.appendChild(frag);

        // show modal
        showModal();
    }

    function renderPagination() {
        const container = document.getElementById('sectionPagination');
        const pageInfo = document.getElementById('sectionPageInfo');
        if (!container) return;
        container.innerHTML = '';
        const total = state.totalCount || 0;
        const last = Math.max(1, Math.ceil(total / state.pageSize));

        const createBtn = (text, disabled, cb) => {
            const b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-sm btn-outline-primary';
            b.textContent = text;
            if (disabled) b.disabled = true;
            b.addEventListener('click', cb);
            return b;
        };

        container.appendChild(createBtn('<<', state.pageIndex <= 1, () => { state.pageIndex = 1; doSearch(); }));
        container.appendChild(createBtn('<', state.pageIndex <= 1, () => { state.pageIndex = Math.max(1, state.pageIndex - 1); doSearch(); }));

        const info = document.createElement('span');
        info.className = 'btn btn-sm disabled';
        info.textContent = `Trang ${state.pageIndex} / ${last}`;
        container.appendChild(info);

        container.appendChild(createBtn('>', state.pageIndex >= last, () => { state.pageIndex = Math.min(last, state.pageIndex + 1); doSearch(); }));
        container.appendChild(createBtn('>>', state.pageIndex >= last, () => { state.pageIndex = last; doSearch(); }));

        if (pageInfo) pageInfo.textContent = `Hiển thị ${Math.min(state.pageSize, total - (state.pageIndex - 1) * state.pageSize)} / ${total}`;
    }

    async function doSearch() {
        try {
            showLoading();
            const payload = {
                MaDon: document.getElementById('searchMaDon')?.value || '',
                Section: document.getElementById('searchPhongBan')?.value || '',
                MaVatTu: document.getElementById('searchMaterial')?.value || '',
                MaNcc: document.getElementById('searchSupplier')?.value || '',
                PageIndex: state.pageIndex,
                PageSize: state.pageSize
            };
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteSelectSection/SearchQuoteSection', {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
            });
            if (!res.ok) {
                const txt = await res.text();
                throw new Error(txt || 'Server error');
            }
            const data = await res.json();
            const items = (data && data.data) ? (Array.isArray(data.data) ? data.data : (data.data.data || [])) : (Array.isArray(data) ? data : []);
            const total = (data && data.data && typeof data.data.totalCount === 'number') ? data.data.totalCount : (data && typeof data.totalCount === 'number' ? data.totalCount : items.length);
            state.totalCount = total;
            renderTable(items);
            renderPagination();
            updateSummary(total);
        } catch (e) {
            console.error('Search error', e);
            showDialog({ message: 'Lỗi tìm kiếm: ' + (e && e.message ? e.message : e), type: 'error' });
        } finally {
            hideLoading();
        }
    }

    // wire buttons and controls
    document.addEventListener('DOMContentLoaded', function () {
        // search button
        document.getElementById('btnSearch')?.addEventListener('click', function () { state.pageIndex = 1; doSearch(); });
        // clear
        document.getElementById('btnClear')?.addEventListener('click', function () {
            const form = document.getElementById('filterForm'); if (form) form.reset();
            // reset any enhanced selects
            document.querySelectorAll('select.searchable-select').forEach(s => { s.value = ''; try { s.dispatchEvent(new Event('change', { bubbles: true })); } catch { } });
            state.pageIndex = 1; doSearch();
        });
        // page size
        const ps = document.getElementById('sectionPageSizeSelect');
        if (ps) {
            ps.value = state.pageSize.toString();
            ps.addEventListener('change', function () { state.pageSize = parseInt(ps.value) || 20; state.pageIndex = 1; doSearch(); });
        }
        // selectAll groups
        const selectAll = document.getElementById('selectAll');
        if (selectAll) {
            selectAll.addEventListener('change', function () {
                const checks = Array.from(document.querySelectorAll('#sectionRequestBody .group-select'));
                const checked = !!selectAll.checked;
                window._selectedGroups = window._selectedGroups || new Set();
                if (checked) {
                    checks.forEach(c => {
                        c.checked = true;
                        const md = c.dataset ? c.dataset.madon : null;
                        if (md) window._selectedGroups.add(md);
                        try { c.dispatchEvent(new Event('input', { bubbles: true })); } catch (e) { }
                        try { c.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
                    });
                } else {
                    checks.forEach(c => {
                        c.checked = false;
                        const md = c.dataset ? c.dataset.madon : null;
                        if (md) window._selectedGroups.delete(md);
                        try { c.dispatchEvent(new Event('input', { bubbles: true })); } catch (e) { }
                        try { c.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
                    });
                }
            });
        }
        // export selected
        const btnExportSelected = document.getElementById('btnExportSelected');
        if (btnExportSelected) {
            btnExportSelected.addEventListener('click', function () {
                const selected = Array.from(window._selectedGroups || new Set());
                if (!selected.length) {
                    showDialog({ message: 'Vui lòng chọn ít nhất một nhóm để xuất.', type: 'info' });
                    return;
                }
                // call export API with selected MaDon
            fetch((window.apiBaseUrl || '') + '/QuoteSelectSection/ExportSelectedGroups', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(selected)
                })
                    .then(res => {
                        if (!res.ok) throw new Error('Export failed');
                        return res.blob();
                    })
                    .then(blob => {
                        const url = window.URL.createObjectURL(blob);
                        const a = document.createElement('a');
                        a.href = url;
                        a.download = `SelectedGroups_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.xlsx`;
                        document.body.appendChild(a);
                        a.click();
                        a.remove();
                        window.URL.revokeObjectURL(url);
                    })
                    .catch(err => {
                        showDialog({ message: 'Lỗi xuất file: ' + err.message, type: 'error' });
                    });
            });
        }
        // initial search
        doSearch();
    });
})();
