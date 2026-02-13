(function () {
    function initEnhancements(root) {
        try {
            buildSearchableDropdown(root || document);
        } catch (e) {

        }

    }
    // Show a custom input dialog.
    function showInputDialog(title, placeholder) {
        return new Promise((resolve, reject) => {
            try {
                const overlay = document.getElementById('cmDialogOverlay');
                if (!overlay) {
                    // fallback to prompt
                    const v = prompt(title + '\n' + (placeholder || ''));
                    if (v === null) return resolve({ action: 'cancel', value: null });
                    return resolve({ action: 'ok', value: v });
                }

                const dialog = overlay.querySelector('.cm-dialog');
                const titleEl = overlay.querySelector('#cmDialogTitle');
                const bodyEl = overlay.querySelector('#cmDialogBody');
                const footerEl = overlay.querySelector('#cmDialogFooter');
                // build content
                const T = window.i18nApproval || {};
                if (titleEl) titleEl.textContent = title || T.InputReasonTitle || 'Input';
                bodyEl.innerHTML = '';
                const input = document.createElement('textarea');
                input.style.width = '100%';
                input.style.minHeight = '100px';
                input.placeholder = placeholder || '';
                input.id = 'cmDialogInput';
                bodyEl.appendChild(input);

                // build footer buttons
                footerEl.innerHTML = '';
                const btnCancel = document.createElement('button');
                btnCancel.type = 'button';
                btnCancel.className = 'btn btn-outline-secondary';
                btnCancel.textContent = (T.DialogCancel || 'Hủy');
                btnCancel.dataset.cmAction = 'cancel';

                const btnOk = document.createElement('button');
                btnOk.type = 'button';
                btnOk.className = 'btn btn-primary ms-2';
                btnOk.textContent = (T.DialogConfirm || 'Xác nhận');
                btnOk.dataset.cmAction = 'ok';

                footerEl.appendChild(btnCancel);
                footerEl.appendChild(btnOk);

                // show overlay (use inline flex so it centers regardless of existing inline styles)
                overlay.setAttribute('aria-hidden', 'false');
                overlay.style.display = 'flex';
                // focus handling
                const prevActive = document.activeElement;
                input.focus();

                function cleanup() {
                    // hide
                    overlay.setAttribute('aria-hidden', 'true');
                    overlay.style.display = 'none';
                    // restore title/body/footer to default (optional)
                    // remove listeners
                    btnOk.removeEventListener('click', onOk);
                    btnCancel.removeEventListener('click', onCancel);
                    overlay.querySelectorAll('[data-cm-action="close"]').forEach(b => b.removeEventListener('click', onCancel));
                    const backdrop = overlay.querySelector('[data-cm-action="overlay"]');
                    if (backdrop) backdrop.removeEventListener('click', onCancel);
                    if (prevActive && typeof prevActive.focus === 'function') try { prevActive.focus(); } catch (e) { }
                }

                function onOk(e) {
                    e && e.preventDefault();
                    const val = input.value;
                    cleanup();
                    resolve({ action: 'ok', value: val });
                }
                function onCancel(e) {
                    e && e.preventDefault();
                    cleanup();
                    resolve({ action: 'cancel', value: null });
                }

                btnOk.addEventListener('click', onOk);
                btnCancel.addEventListener('click', onCancel);
                overlay.querySelectorAll('[data-cm-action="close"]').forEach(b => b.addEventListener('click', onCancel));
                const backdrop = overlay.querySelector('[data-cm-action="overlay"]');
                if (backdrop) backdrop.addEventListener('click', onCancel);

                // ESC to cancel
                function onKey(e) {
                    if (e.key === 'Escape') { onCancel(); }
                    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) { onOk(); }
                }
                document.addEventListener('keydown', onKey);
                // ensure cleanup also removes key listener
                const origCleanup = cleanup;
                cleanup = function () {
                    document.removeEventListener('keydown', onKey);
                    origCleanup();
                };
            } catch (err) {
                console.error('showInputDialog error', err);
                reject(err);
            }
        });
    }
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
            search.innerHTML = '<input type="text" placeholder="Tìm..." />';
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
                    const empty = document.createElement('div'); empty.className = 'ms-empty'; empty.textContent = (window.i18nApproval && window.i18nApproval.NoResults) || 'Không có kết quả';
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
                    placeholderEl.textContent = (window.i18nApproval && window.i18nApproval.SelectPlaceholder) || '-- Chọn --';
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

    initEnhancements(window.jQuery ? $(document) : document);
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { initEnhancements(document); });
    } else {
        // DOM already ready
        initEnhancements(document);
    }

    // Additionally on window load (assets/styles applied)
    window.addEventListener('load', function () { initEnhancements(document); });

    // Observe DOM for late-added selects and enhance them automatically
    if (window.MutationObserver) {
        const observer = new MutationObserver(function (mutations) {
            let needsEnhance = false;
            for (const m of mutations) {
                if (m.addedNodes && m.addedNodes.length) {
                    for (const n of m.addedNodes) {
                        if (!(n instanceof Element)) continue;
                        if (n.matches && n.matches('select.searchable-select') && n.dataset.searchDropdown !== 'true') {
                            needsEnhance = true;
                        }
                        const innerSelects = n.querySelectorAll ? n.querySelectorAll('select.searchable-select') : [];
                        for (const s of innerSelects) {
                            if (s.dataset.searchDropdown !== 'true') {
                                needsEnhance = true;
                            }
                        }
                    }
                }
            }
            if (needsEnhance) {
                initEnhancements(document);
            }
        });
        observer.observe(document.documentElement || document.body, { childList: true, subtree: true });
    }

    // -----------------------------
    // Approval Quote page behaviors
    // -----------------------------
    const state = {
        groupsByMaDon: {},
        orderedMaDons: [],
        selectedMaDons: new Set(),
        currentMaDonInModal: null
    };

    // lay ten trang thai de hien thi
    function getStepName(stepNumber) {
        var listSteps = window.ApprovalData.steps
        const step = listSteps.find(s => s.INT_StepNumber === stepNumber);
        var lang = window.ApprovalData.uiland;
        switch (lang) {
            case 'en': return step ? step.CHR_StepNameEN : stepNumber;
            case 'ja': return step ? step.CHR_StepNameJP : stepNumber;
            default: return step ? step.CHR_StepName : stepNumber;
        };
    }
    function getFilterValues() {
        const status = document.getElementById('tinhTrangPheDuyet')?.value || '';
        const section = document.getElementById('searchPhongBan')?.value || '';
        const soDon = document.getElementById('searchmaDonBaoGia')?.value || '';
        const maHang = document.getElementById('searchMaterial')?.value || '';
        return {
            StatusApprover: status,
            Section: section,
            SoDon: soDon,
            MaHang: maHang
        };
    }

    function updateSummaryAndButtons() {
        const summaryEl = document.getElementById('summaryText');
        if (summaryEl) {
            const T = window.i18nApproval || {};
            const fmt = T.SummaryFormat || '{0} bản ghi';
            summaryEl.textContent = fmt.replace('{0}', state.orderedMaDons.length);
        }
        const approveBtn = document.getElementById('btnApprove');
        const returnBtn = document.getElementById('btnReturn');
        const approveCountEl = document.getElementById('approveCount');
        const returnCountEl = document.getElementById('returnCount');
        const selectedCount = state.selectedMaDons.size;
        if (approveBtn) approveBtn.disabled = selectedCount === 0;
        if (returnBtn) returnBtn.disabled = selectedCount === 0;
        if (approveCountEl) approveCountEl.textContent = selectedCount;
        if (returnCountEl) returnCountEl.textContent = selectedCount;
    }

    function updateSelectAllState() {
        const selectAllEl = document.getElementById('selectAll');
        if (!selectAllEl) return;
        const total = state.orderedMaDons.length;
        const selected = state.selectedMaDons.size;
        selectAllEl.checked = total > 0 && selected === total;
        selectAllEl.indeterminate = selected > 0 && selected < total;
    }

    function clearSelection() {
        state.selectedMaDons.clear();
        document.querySelectorAll('#approvalTableBody tr').forEach(tr => tr.classList.remove('table-active'));
        updateSummaryAndButtons();
    }

    function renderTable() {
        const tbody = document.getElementById('approvalTableBody');
        if (!tbody) return;
        tbody.innerHTML = '';
        state.orderedMaDons.forEach(maDon => {
            const group = state.groupsByMaDon[maDon];
            if (!group || group.length === 0) return;
            const first = group[0];
            const tr = document.createElement('tr');
            tr.className = 'text-center';
            tr.dataset.maDon = maDon;
            if (state.selectedMaDons.has(maDon)) tr.classList.add('table-active');
            // selection checkbox cell
            const tdSelect = document.createElement('td');
            tdSelect.style.width = '40px';
            const chk = document.createElement('input');
            chk.type = 'checkbox';
            chk.className = 'row-select';
            chk.dataset.madon = maDon;
            chk.checked = state.selectedMaDons.has(maDon);
            chk.addEventListener('change', function (e) {
                e.stopPropagation();
                if (this.checked) {
                    state.selectedMaDons.add(maDon);
                    tr.classList.add('table-active');
                } else {
                    state.selectedMaDons.delete(maDon);
                    tr.classList.remove('table-active');
                }
                updateSummaryAndButtons();
                updateSelectAllState();
            });
            tdSelect.appendChild(chk);

            const tdStatus = document.createElement('td');
            tdStatus.textContent = getStepName(first.iD_StepBaoGia) || '';
            const tdDetail = document.createElement('td');
            const btn = document.createElement('button');
            btn.className = 'btn btn-outline-primary btn-sm';
            btn.innerHTML = '<i class="fas fa-info-circle"></i> ' + ((window.i18nApproval && window.i18nApproval.Detail) || 'Chi tiết');
            btn.addEventListener('click', function (ev) {
                ev.stopPropagation();
                showDetailModal(maDon);
            });
            tdDetail.appendChild(btn);
            const tdMaDon = document.createElement('td'); tdMaDon.textContent = maDon;
            const tdNgayYC = document.createElement('td'); tdNgayYC.textContent = formatDate(first.dtM_CreateDate);
            const tdTenPB = document.createElement('td'); tdTenPB.textContent = first.chR_SectionName || '';
            const tdPic = document.createElement('td'); tdPic.textContent = first.chR_CreateBy || '';
            const tdNgayNhan = document.createElement('td'); tdNgayNhan.textContent = formatDate(first.dtM_NgayMuonNhan);
            const tdKyHan = document.createElement('td'); tdKyHan.textContent = formatDate(first.dtM_Deadline || first.dtM_KyHan);

            tr.appendChild(tdStatus);
            tr.appendChild(tdDetail);
            tr.appendChild(tdMaDon);
            tr.appendChild(tdNgayYC);
            tr.appendChild(tdTenPB);
            tr.appendChild(tdPic);
            tr.appendChild(tdNgayNhan);
            tr.appendChild(tdKyHan);

            tr.addEventListener('click', function (ev) {
                // ignore clicks originating from the checkbox itself
                if (ev.target && ev.target.closest && ev.target.closest('input.row-select')) return;
                const key = this.dataset.maDon;
                if (state.selectedMaDons.has(key)) {
                    state.selectedMaDons.delete(key);
                    this.classList.remove('table-active');
                } else {
                    state.selectedMaDons.add(key);
                    this.classList.add('table-active');
                }
                // sync checkbox state
                const cb = this.querySelector('input.row-select');
                if (cb) cb.checked = state.selectedMaDons.has(key);
                updateSummaryAndButtons();
                updateSelectAllState();
            });

            // append cells in order: select, status, detail, ...
            tr.appendChild(tdSelect);
            tr.appendChild(tdStatus);
            tr.appendChild(tdDetail);
            tr.appendChild(tdMaDon);
            tr.appendChild(tdNgayYC);
            tr.appendChild(tdTenPB);
            tr.appendChild(tdPic);
            tr.appendChild(tdNgayNhan);
            tr.appendChild(tdKyHan);

            tbody.appendChild(tr);
        });
        updateSummaryAndButtons();
        updateSelectAllState();
    }

    function formatDate(val) {
        if (!val) return '';
        try {
            // Accept ISO strings or ticks
            const d = typeof val === 'string' ? new Date(val) : val;
            if (!isNaN(d.getTime())) {
                const dd = String(d.getDate()).padStart(2, '0');
                const mm = String(d.getMonth() + 1).padStart(2, '0');
                const yyyy = d.getFullYear();
                return `${dd}/${mm}/${yyyy}`;
            }
        } catch (_) { }
        return String(val);
    }

    function groupByMaDon(items) {
        const map = {};
        const order = [];
        items.forEach(it => {
            const key = it.chR_MaDon || '';
            if (!key) return;
            if (!map[key]) { map[key] = []; order.push(key); }
            map[key].push(it);
        });
        state.groupsByMaDon = map;
        state.orderedMaDons = order;
        state.selectedMaDons.clear();
    }

    function showDetailModal(maDon) {
        const group = state.groupsByMaDon[maDon] || [];
        state.currentMaDonInModal = maDon;
        // header info
        const madonEl = document.getElementById('madonhang');
        if (madonEl) {
            const T = window.i18nApproval || {};
            const fmt = T.DetailModalTitleFormat || '{0} ({1})';
            madonEl.textContent = fmt.replace('{0}', maDon).replace('{1}', group.length);
        }
        const first = group[0] || {};
        const setText = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val == null ? '' : String(val); };
        setText('khoi', first.chR_SectionName || '');
        setText('mbp', first.chR_SectionCode || '');
        setText('mpb_yc', first.chR_CostCenter || first.chR_SectionCode || '');
        setText('tenphongban', first.chR_SectionName || '');
        setText('chuyen', first.nvchR_DongMay || '');
        setText('nyc', formatDate(first.dtM_CreateDate));
        setText('thmm', formatDate(first.dtM_NgayMuonNhan));
        // urgent badge
        const urgent = group.some(it => {
            const v = it && it.chR_Gap;
            if (v == null) return false;
            const s = String(v).toLowerCase();
            return s === 'true' || s === '1' || s === 'o' || s === 'x' && false; // only true cases
        });
        const urgentBadge = document.getElementById('urgent-badge');
        if (urgentBadge) urgentBadge.style.display = urgent ? '' : 'none';

        // extra info (take the first line for summary)
        setText('rohs_requirement', first.nvchR_Rohs || '-');
        setText('cocq_requirement', first.nvchR_COCQ || '-');
        setText('msds_requirement', first.nvchR_MSDS || '-');
        setText('requester', first.chR_CreateBy || '-');

        // footer info
        setText('id_request', maDon || '');
        setText('step', getStepName(first.iD_StepBaoGia));
        setText('regency', first.chR_Quyen || '');

        // build table body per new layout
        const tbody = document.getElementById('detailModalBody');
        if (tbody) {
        tbody.innerHTML = '';
        const frag = document.createDocumentFragment();
        group.forEach((it, idx) => {
            const tr = document.createElement('tr');
            function td(text) { const c = document.createElement('td'); c.textContent = text || ''; return c; }
            tr.appendChild(td(String(idx + 1)));
                tr.appendChild(td(maDon)); // Mã đơn
                tr.appendChild(td(it.chR_MaHangNoiBo)); // Mã vật tư nội bộ
                tr.appendChild(td(it.nvchR_ChungLoai)); // Mã vật tư nội bộ
                tr.appendChild(td(((it.nvchR_NameVN || '') + (it.nvchR_NameEN ? ' / ' + it.nvchR_NameEN : '')).trim())); // Tên hàng (VN/EN)
                tr.appendChild(td(it.inT_SoLuong != null ? String(it.inT_SoLuong) : ''));// Số lượng
                tr.appendChild(td(it.nvchR_DonVi)); // Đơn vị
                tr.appendChild(td(it.nvchR_HinhDang)); // Hình dạng
                tr.appendChild(td(it.nvchR_ChatLieu)); // Vật liệu
                tr.appendChild(td(it.nvchR_ThanhPhan)); // Thành phần
                tr.appendChild(td(it.nvchR_KichThuoc)); // Kích thước
                tr.appendChild(td(it.nvchR_DongMay)); // dòng máy
                //tr.appendChild(td(it.nvchR_DongMay)); // Vị trí sử dụng
                tr.appendChild(td(it.nvchR_TinhNang)); // Mục đích/Tính năng
            const ncc = (it.chR_MaNCC ? it.chR_MaNCC : '') + (it.nvchR_TenNCC ? ` - ${it.nvchR_TenNCC}` : '');
                tr.appendChild(td(ncc.trim())); // NCC
                tr.appendChild(td(formatDate(it.dtM_KyHan))); // Ngày nhận mong muốn
                const gap = it.chR_Gap != null ? (String(it.chR_Gap).toLowerCase() === 'true' || String(it.chR_Gap) === '1' ? 'O' : 'X') : '';
                tr.appendChild(td(gap)); // Khẩn
            frag.appendChild(tr);
        });
        tbody.appendChild(frag);
        }

        const modalEl = document.getElementById('detailModal');
        if (modalEl) {
            // create backdrop
            const backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop show';
            backdrop.style.zIndex = '3999';
            document.body.appendChild(backdrop);
            modalEl._backdrop = backdrop;

            // simple show
            try { modalEl.setAttribute('aria-hidden', 'false'); } catch (e) { }
            try { modalEl.removeAttribute('inert'); } catch (e) { }
            modalEl.style.display = 'block';
            modalEl.classList.add('show');
            document.body.classList.add('modal-open');
            const closeBtn = modalEl.querySelector('.btn-close');
            if (closeBtn && typeof closeBtn.focus === 'function') {
                closeBtn.focus();
            }
        }
    }

    function hideDetailModal() {
        const modalEl = document.getElementById('detailModal');
        if (modalEl) {
            // remove backdrop
            if (modalEl._backdrop) {
                document.body.removeChild(modalEl._backdrop);
                delete modalEl._backdrop;
            }

            // move focus to a logical control outside modal before hiding
            const fallback = document.getElementById('selectAll') || document.getElementById('btnSearch') || document.body;
            try { if (fallback && typeof fallback.focus === 'function') fallback.focus(); } catch (e) { }

            modalEl.style.display = 'none';
            modalEl.classList.remove('show');
            try { modalEl.setAttribute('aria-hidden', 'true'); } catch (e) { }
            try { modalEl.setAttribute('inert', ''); } catch (e) { }
        }

        document.body.classList.remove('modal-open');
        state.currentMaDonInModal = null;
    }
    function ReturnCode(IDStep) {
        switch (IDStep) {
            case 2: return 'RETURN_QLSC'; // Trả về QLSC phong ban
            case 3: return 'RETURN_QLTC'; // Trả về QLTC phong ban
            case 4: return 'RETURN_PIC';    // Trả về PIC phong mua hang
            case 5: return 'RETURN_QLSC_1'; // Trả về QLSC phong ban mua hang
            default: return 'RETURN';
        }
    }

    async function searchAndRender() {
        clearSelection();
        const payload = getFilterValues();
        const url = '/ApprovalQuote/SearchApprovalQuote';
        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                body: JSON.stringify(payload)
            });
            const json = await res.json();
            if (json && json.success && Array.isArray(json.data)) {
                groupByMaDon(json.data);
                renderTable();
            } else {
                state.groupsByMaDon = {}; state.orderedMaDons = []; clearSelection();
                renderTable();
                console.warn('Search failed:', json && json.message);
            }
        } catch (e) {
            console.error('Search error', e);
        }
    }
    // Lightweight toast helper (fallback if global showToast is not provided)
    function showToast(type, message, timeout = 3000) {
        try {
            let container = document.getElementById('cmToastContainer');
            if (!container) {
                container = document.createElement('div');
                container.id = 'cmToastContainer';
                container.style.position = 'fixed';
                container.style.top = '1rem';
                container.style.right = '1rem';
                container.style.zIndex = '2000';
                container.style.display = 'flex';
                container.style.flexDirection = 'column';
                container.style.gap = '0.5rem';
                document.body.appendChild(container);
            }

            const toast = document.createElement('div');
            toast.className = 'cm-toast';
            toast.style.minWidth = '200px';
            toast.style.maxWidth = '320px';
            toast.style.padding = '0.75rem 1rem';
            toast.style.borderRadius = '0.375rem';
            toast.style.boxShadow = '0 2px 6px rgba(0,0,0,0.15)';
            toast.style.color = '#fff';
            toast.style.fontSize = '0.95rem';
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 200ms ease, transform 200ms ease';
            toast.style.transform = 'translateY(-6px)';

            if (type === 'success') {
                toast.style.background = '#198754';
            } else if (type === 'error' || type === 'danger') {
                toast.style.background = '#dc3545';
            } else if (type === 'warning') {
                toast.style.background = '#ffc107';
                toast.style.color = '#000';
            } else {
                toast.style.background = '#0d6efd';
            }

            toast.textContent = message || '';
            container.appendChild(toast);

            // force reflow then show
            void toast.offsetWidth;
            toast.style.opacity = '1';
            toast.style.transform = 'translateY(0)';

            const remove = () => {
                toast.style.opacity = '0';
                toast.style.transform = 'translateY(-6px)';
                setTimeout(() => { try { container.removeChild(toast); } catch (e) { } }, 220);
            };

            const tId = setTimeout(remove, timeout);
            // allow click to dismiss early
            toast.addEventListener('click', () => {
                clearTimeout(tId);
                remove();
            });
        } catch (err) {
            // fallback to alert if something goes wrong
            try { console.error(err); alert(message); } catch (e) { }
        }
    }
    function attachEvents() {
        const T = window.i18nApproval || {};
        const btnSearch = document.getElementById('btnSearch');
        if (btnSearch) btnSearch.addEventListener('click', searchAndRender);
        const btnClear = document.getElementById('btnClear');
        if (btnClear) btnClear.addEventListener('click', function () {
            ['tinhTrangPheDuyet', 'searchPhongBan', 'searchmaDonBaoGia', 'searchMaterial'].forEach(id => {
                const el = document.getElementById(id);
                if (el) {
                    el.value = '';
                    try { el.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
                }
            });
            state.groupsByMaDon = {}; state.orderedMaDons = []; clearSelection(); renderTable();
        });
        // xử lý phê duyệt OK
        const btnApprove = document.getElementById('btnApprove');
        if (btnApprove) btnApprove.addEventListener('click', function () {
            if (state.selectedMaDons.size === 0) return;
            const payload = [];
            Array.from(state.selectedMaDons).forEach(maDon => {
                const group = state.groupsByMaDon[maDon] || [];
                group.forEach(it => {
                    it.iD_StepBaoGia = (it.iD_StepBaoGia != null ? parseInt(it.iD_StepBaoGia) + 1 : 1);
                    it.iD_Status = 'APPROVAL';
                    payload.push(it);
                });
            });
            if (payload.length === 0) return;
            btnApprove.disabled = true;
            const btnReturnEl = document.getElementById('btnReturn'); if (btnReturnEl) btnReturnEl.disabled = true;
            fetch('/ApprovalQuote/UpdateQuotationOK', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                body: JSON.stringify(payload)
            }).then(res => res.json()).then(json => {
                if (json && json.success) {
                    showToast('success', T.MsgSusscesAprover);
                    searchAndRender();
                } else {
                    showToast('danger', T.MSGFailedApprover + (json && json.message ? json.message : 'Unknown'));
                }
            }).catch(err => {
                console.error('Approval error', err);
                showToast('danger', T.MSGErrorApprover);
            }).finally(() => {
                updateSummaryAndButtons();
            });
        });
        // Xử lý trả lại NG
        const btnReturn = document.getElementById('btnReturn');
        if (btnReturn) btnReturn.addEventListener('click', function () {
            if (state.selectedMaDons.size === 0) return;
            // show custom input dialog for reason
            showInputDialog(T.InputReasonTitle || 'Lý do trả lại', T.InputReasonPlaceholder || 'Nhập lý do trả lại...').then(result => {
                if (!result) return;
                if (result.action !== 'ok') return; // cancelled
                const reason = (result.value || '').trim();
                if (!reason) { showToast('warning', (T.ReasonRequired || 'Lý do không được để trống.')); return; }
                // collect payload and set return reason
                const payload = [];
                Array.from(state.selectedMaDons).forEach(maDon => {
                    const group = state.groupsByMaDon[maDon] || [];
                    group.forEach(it => {
                        it.nvchR_LyDo = reason;
                        it.iD_Status = ReturnCode(it.iD_StepBaoGia);
                        it.iD_StepBaoGia = 1;
                        payload.push(it);
                    });
                });
                if (payload.length === 0) return;
                btnReturn.disabled = true;
                const btnApproveEl = document.getElementById('btnApprove'); if (btnApproveEl) btnApproveEl.disabled = true;
                fetch('/ApprovalQuote/UpdateQuotationNG', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                    body: JSON.stringify(payload)
                }).then(res => res.json()).then(json => {
                    if (json && json.success) {
                        showToast('success', T.MSGReturnOK);
                        searchAndRender();
                    } else {
                        showToast('danger', T.MSGReturnFailed +': ' + (json && json.message ? json.message : 'Unknown'));
                    }
                }).catch(err => {
                    console.error('Return error', err);
                    showToast('danger', T.ReturnError);
                }).finally(() => {
                    updateSummaryAndButtons();
                });
            }).catch(() => {});
        });

        // select all checkbox
        const selectAllEl = document.getElementById('selectAll');
        if (selectAllEl) {
            selectAllEl.addEventListener('change', function () {
                const checked = this.checked;
                state.selectedMaDons.clear();
                if (checked) state.orderedMaDons.forEach(m => state.selectedMaDons.add(m));
                renderTable();
                updateSummaryAndButtons();
                updateSelectAllState();
            });
        }

        // modal footer actions
        const modalApprove = document.getElementById('modalApprove');
        if (modalApprove) modalApprove.addEventListener('click', function () {
            const maDon = state.currentMaDonInModal;
            if (!maDon) return;
            const group = state.groupsByMaDon[maDon] || [];
            if (!group.length) return;
            const payload = [];
            group.forEach(it => {
                it.iD_StepBaoGia = (it.iD_StepBaoGia != null ? parseInt(it.iD_StepBaoGia) + 1 : 1);
                it.iD_Status = 'APPROVAL';
                payload.push(it);
            });
            fetch('/ApprovalQuote/UpdateQuotationOK', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                body: JSON.stringify(payload)
            }).then(res => res.json()).then(json => {
                if (json && json.success) {
                    showToast('success', T.MsgSusscesAprover);
                    hideDetailModal();
                    searchAndRender();
                } else {
                    showToast('danger', T.MSGFailedApprover + (json && json.message ? json.message : 'Unknown'));
    }
            }).catch(err => {
                console.error('Approval error', err);
                showToast('danger', T.MSGErrorApprover);
            });
        });

        const modalReject = document.getElementById('modalReject');
        if (modalReject) modalReject.addEventListener('click', function () {
            hideEditModal('detailModal');
            const maDon = state.currentMaDonInModal;
            if (!maDon) return;
            const group = state.groupsByMaDon[maDon] || [];
            if (!group.length) return;
            showInputDialog(T.InputReasonTitle || 'Lý do trả lại', T.InputReasonPlaceholder || 'Nhập lý do trả lại...').then(result => {
                if (!result || result.action !== 'ok') return;
                const reason = (result.value || '').trim();
                if (!reason) { showToast('warning', (T.ReasonRequired || 'Lý do không được để trống.')); return; }
                const payload = [];
                group.forEach(it => {
                    it.nvchR_LyDo = reason;
                    it.iD_Status = ReturnCode(it.iD_StepBaoGia);
                    it.iD_StepBaoGia = 1;
                    payload.push(it);
                });
                fetch('/ApprovalQuote/UpdateQuotationNG', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                    body: JSON.stringify(payload)
                }).then(res => res.json()).then(json => {
                    if (json && json.success) {
                        showToast('success', T.MSGReturnOK);
                        hideDetailModal();
                        searchAndRender();
                    } else {
                        showToast('danger', T.MSGReturnFailed + ': ' + (json && json.message ? json.message : 'Unknown'));
                    }
                }).catch(err => {
                    console.error('Return error', err);
                    showToast('danger', T.ReturnError);
                });
            });
        });
    }
    function hideEditModal(modalName) {
        const modalEl = document.getElementById(modalName);
        if (modalEl._backdrop) {
            document.body.removeChild(modalEl._backdrop);
            delete modalEl._backdrop;
        }
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
    // Init: attach events and auto load data once ready
    function initPage() {
        attachEvents();
        searchAndRender();
        // Hook close button in modal header if exists
        function wireClose() {
            document.querySelectorAll('#detailModal .btn-close').forEach(btn => {
                btn.addEventListener('click', hideDetailModal);
            });
            document.querySelectorAll('#detailModal [data-bs-dismiss="modal"]').forEach(btn => {
                btn.addEventListener('click', hideDetailModal);
            });
        }
        wireClose();
        // also close when clicking outside dialog area
        document.addEventListener('click', function (e) {
            const modalEl = document.getElementById('detailModal');
            if (!modalEl || modalEl.style.display !== 'block') return;
            const dialog = modalEl.querySelector('.modal-dialog');
            if (dialog && !dialog.contains(e.target)) {
                hideDetailModal();
            }
        });
        // ESC to close
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                const modalEl = document.getElementById('detailModal');
                if (modalEl && modalEl.style.display === 'block') hideDetailModal();
            }
        });
    }
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPage);
    } else {
        initPage();
    }
})();