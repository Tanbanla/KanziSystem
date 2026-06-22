(function () {
    function initEnhancements(root) {
        try {
            buildSearchableDropdown(root || document);
        } catch (e) {

        }

    }
    // Open approver selector modal and return a promise resolving to the selected approver object
    function openApproverSelector(stepNumber, sectionCode) {
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
                placeholderOpt.textContent = (window.i18nApproval && window.i18nApproval.SelectPlaceholder) || '-- Chọn --';
                sel.appendChild(placeholderOpt);

                // fetch approvers
                const body = { Step: stepNumber, SectionCost: sectionCode };
                let list = [];
                try {
                    const resp = await fetch((window.apiBaseUrl || '') + '/ApprovalQuote/GetListApprovel', {
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
                    emptyOpt.textContent = (window.i18nApproval && window.i18nApproval.NoResults) || 'Không có kết quả';
                    sel.appendChild(emptyOpt);
                } else {
                    list.forEach(item => {
                        const o = document.createElement('option');
                        // normalize keys to accept server DTO naming
                        const adid = item.chR_UserAdid || '';
                        const name = item.nvchR_UserName || '';
                        o.value = adid || (item.chR_UserAdid);
                        o.textContent = (name ? (name + (adid ? (' (' + adid + ')') : '')) : (adid || ''));
                        o.dataset.raw = JSON.stringify(item);
                        sel.appendChild(o);
                    });
                }

                // ensure modal is attached to body so it escapes local stacking contexts
                try {
                    if (modal.parentElement !== document.body) document.body.appendChild(modal);
                } catch (e) { }
                // show modal (bootstrap 5 manual show) and ensure backdrop/z-index are high
                try {
                    if (window.bootstrap && bootstrap.Modal) {
                        const bsModal = new bootstrap.Modal(modal, { backdrop: 'static' });
                        modal._bsModal = bsModal;
                        bsModal.show();
                        // after bootstrap created backdrop, increase z-index to ensure on top
                        setTimeout(() => {
                            try {
                                const backdrops = document.querySelectorAll('.modal-backdrop');
                                const createdBackdrop = backdrops.length ? backdrops[backdrops.length - 1] : null;
                                if (createdBackdrop) {
                                    createdBackdrop.style.zIndex = '10550';
                                    modal._bsBackdrop = createdBackdrop;
                                }
                                modal.style.zIndex = '10600';
                            } catch (e) { }
                        }, 10);
                    } else {
                        // create a simple backdrop and set high z-index
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

                // handlers
                const confirmBtn = document.getElementById('confirmSelectApprover');
                function cleanup() {
                    // hide
                    try {
                        if (modal._bsModal) modal._bsModal.hide();
                        else {
                            modal.style.display = 'none';
                            modal.classList.remove('show');
                        }
                    } catch (e) { try { modal.style.display = 'none'; modal.classList.remove('show'); } catch {} }
                    // remove any custom backdrop we created
                    try {
                        if (modal._backdrop) { document.body.removeChild(modal._backdrop); delete modal._backdrop; }
                    } catch (e) { }
                    // ensure bootstrap backdrop for approver modal is removed
                    try {
                        if (modal._bsBackdrop && modal._bsBackdrop.parentElement) {
                            modal._bsBackdrop.parentElement.removeChild(modal._bsBackdrop);
                        }
                        delete modal._bsBackdrop;
                    } catch (e) { }
                    // cleanup listeners
                    confirmBtn.removeEventListener('click', onConfirm);
                    modal.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.removeEventListener('click', onCancel));
                    if (notice) notice.style.display = 'none';
                    // reset inline zIndex
                    try { modal.style.zIndex = ''; } catch (e) { }
                }
                function onConfirm(e) {
                    e && e.preventDefault();
                    const value = sel.value;
                    if (!value) {
                        if (notice) {
                            notice.style.display = '';
                        }
                        return;
                    }
                    const raw = sel.selectedOptions && sel.selectedOptions[0] && sel.selectedOptions[0].dataset.raw;
                    let obj = null;
                    try { obj = raw ? JSON.parse(raw) : { CHR_UserAdid: value, NVCHR_UserName: sel.selectedOptions[0].textContent }; } catch { obj = { CHR_UserAdid: value, NVCHR_UserName: sel.selectedOptions[0].textContent }; }
                    cleanup();
                    resolve(obj);
                }
                function onCancel() { cleanup(); resolve(null); }
                confirmBtn.addEventListener('click', onConfirm);
                modal.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.addEventListener('click', onCancel));
            } catch (err) { reject(err); }
        });
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
        state.orderedMaDons.forEach((maDon, index) => {
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
            const tdNo = document.createElement('td');
            tdNo.textContent = String(index + 1);
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

            // NOTE: append cells once (order: select, no, status, detail, maDon, ngayYC, tenPB, pic, ngayNhan, kyHan)
            // style some cells centered for better readability
            tdSelect.className = (tdSelect.className ? tdSelect.className + ' ' : '') + 'text-center align-middle';
            tdNo.className = (tdNo.className ? tdNo.className + ' ' : '') + 'text-center';
            tdStatus.className = (tdStatus.className ? tdStatus.className + ' ' : '') + 'text-center';
            tdDetail.className = (tdDetail.className ? tdDetail.className + ' ' : '') + 'text-center';
            tdNgayYC.className = (tdNgayYC.className ? tdNgayYC.className + ' ' : '') + 'text-center';
            tdNgayNhan.className = (tdNgayNhan.className ? tdNgayNhan.className + ' ' : '') + 'text-center';
            tdKyHan.className = (tdKyHan.className ? tdKyHan.className + ' ' : '') + 'text-center';

            tr.appendChild(tdSelect);
            tr.appendChild(tdNo);
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

            // append cells in order: select, no, status, detail, ...
            tr.appendChild(tdSelect);
            tr.appendChild(tdNo);
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
            function td(text) { const c = document.createElement('td'); c.textContent = text == null ? '' : String(text); return c; }
            // helper to read multiple possible property names
            const getVal = (obj, ...names) => {
                for (const n of names) {
                    if (!obj) continue;
                    // accept exact property or different casing
                    if (obj[n] !== undefined && obj[n] !== null) return obj[n];
                    const alt = Object.keys(obj).find(k => k.toLowerCase() === n.toLowerCase());
                    if (alt && obj[alt] !== undefined) return obj[alt];
                }
                return '';
            };

            // Build cells and apply centering to numeric/short columns
            const tdIndex = td(String(idx + 1)); tdIndex.className = 'text-center'; tr.appendChild(tdIndex);
            const tdMaDonCell = td(maDon); tr.appendChild(tdMaDonCell); // Mã đơn (SystemCode)
            tr.appendChild(td(getVal(it, 'chR_MaHangNoiBo', 'chR_MaHangNoiBo'))); // Mã vật tư nội bộ
            tr.appendChild(td(getVal(it, 'nvchR_ChungLoai', 'nvchR_ChungLoai'))); // Chủng loại
            const tdPhanLoai = td(getVal(it, 'chR_Phanloai', 'chR_Phanloai')); tdPhanLoai.className = 'text-center'; tr.appendChild(tdPhanLoai); // Phân loại (Classification)
            tr.appendChild(td(getVal(it, 'chR_MaHangNCC', 'chR_MaHangNCC'))); // Mã hàng NCC (SupplierItemCode)
            tr.appendChild(td(((getVal(it, 'nvchR_NameVN', 'nvchR_NameVN') || '') + (getVal(it, 'nvchR_NameEN', 'nvchR_NameEN') ? ' / ' + getVal(it, 'nvchR_NameEN', 'nvchR_NameEN') : '')).trim())); // Tên hàng (VN/EN)

            // Description group (8 cols)
            const tdQty = td(getVal(it, 'inT_SoLuong', 'inT_SoLuong')); tdQty.className = 'text-center'; tr.appendChild(tdQty); // Số lượng
            const tdDonVi = td(getVal(it, 'nvchR_DonVi', 'nvchR_DonVi')); tdDonVi.className = 'text-center'; tr.appendChild(tdDonVi); // Đơn vị
            tr.appendChild(td(getVal(it, 'nvchR_HinhDang', 'nvchR_HinhDang'))); // Hình dạng
            tr.appendChild(td(getVal(it, 'nvchR_ChatLieu', 'nvchR_ChatLieu'))); // Chất liệu
            tr.appendChild(td(getVal(it, 'nvchR_ThanhPhan', 'nvchR_ThanhPhan'))); // Thành phần
            tr.appendChild(td(getVal(it, 'nvchR_KichThuoc', 'nvchR_KichThuoc'))); // Kích thước
            tr.appendChild(td(getVal(it, 'nvchR_DongMay', 'nvchR_DongMay'))); // Dùng cho máy/vi trí
            tr.appendChild(td(getVal(it, 'nvchR_TinhNang', 'nvchR_TinhNang'))); // Tính năng/Purpose

            // Additional fields requested
            tr.appendChild(td(getVal(it, 'nvchR_FileThietKe', 'nvchR_FileThietKe', 'chr_FileThietKe'))); // File thiết kế
            tr.appendChild(td(getVal(it, 'nvchR_NhaSanXuat', 'nvchR_NhaSanXuat'))); // Nhà sản xuất / Maker
            tr.appendChild(td(getVal(it, 'chR_MaNCC', 'chR_MaNCC', 'MaNCC'))); // Mã nhà cung cấp / Vendor code
            tr.appendChild(td(getVal(it, 'nvchR_TenNCC', 'nvchR_TenNCC'))); // Tên nhà cung cấp / Vendor name
            tr.appendChild(td(getVal(it, 'nvchR_Rohs', 'nvchR_Rohs'))); // ROHS
            tr.appendChild(td(getVal(it, 'nvchR_COCQ', 'nvchR_COCQ'))); // COCQ
            tr.appendChild(td(getVal(it, 'nvchR_MSDS', 'nvchR_MSDS'))); // MSDS
            tr.appendChild(td(getVal(it, 'vchR_AnToan', 'vchR_AnToan'))); // vchR_AnToan

            // Supplier deadline, urgent, get quotation, reason
            const tdDeadline = td(formatDate(getVal(it, 'dtM_KyHan', 'dtM_KyHan'))); tdDeadline.className = 'text-center'; tr.appendChild(tdDeadline); // Kỳ hạn chọn NCC
            const gap = getVal(it, 'chR_Gap', 'chR_Gap');
            const gapLabel = gap != null && gap !== '' ? (String(gap).toLowerCase() === 'true' || String(gap) === '1' ? 'O' : 'X') : '';
            const tdGap = td(gapLabel); tdGap.className = 'text-center'; tr.appendChild(tdGap); // Khẩn

            tr.appendChild(td(getVal(it, 'nvchR_ReasonQuotation'))); // NVCHR_ReasonQuotation

            const layBaogia = getVal(it, 'biT_LayBaoGia', 'biT_LayBaoGia');
            const layLabel = layBaogia != null && layBaogia !== '' ? (String(layBaogia).toLowerCase() === 'true' || String(layBaogia) === '1' ? 'O' : 'X') : '';
            const tdLay = td(layLabel); tdLay.className = 'text-center'; tr.appendChild(tdLay); // Lấy báo giá
            tr.appendChild(td(getVal(it, 'nvchR_LyDo', 'nvchR_LyDo'))); // Lý do
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
        const url = (window.apiBaseUrl || '') + '/ApprovalQuote/SearchApprovalQuote';
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
                    //it.iD_StepBaoGia = (it.iD_StepBaoGia != null ? parseInt(it.iD_StepBaoGia) + 1 : 1);
                    //if (it.iD_StepBaoGia == 6) {
                    //    it.iD_Status = 'WAIT_SEND_MAIL';
                    //} else {
                    //    it.iD_Status = 'APPROVAL';
                    //}
                    payload.push(it);
                });
            });
            if (payload.length === 0) return;
            // Before sending approval, require selection of next approver
            // derive next step and section from first item
            const first = payload[0];
            const nextStep = (first && typeof first.iD_StepBaoGia === 'number') ? (first.iD_StepBaoGia + 1) : 3;
            const sectionCode = first.chR_SectionCode || '';
            // If next step is 6, do not prompt for approver — send immediately
            if (nextStep === 6 || nextStep === 4) {
                payload.forEach(p => {
                    p.iD_StepBaoGia = (p.iD_StepBaoGia != null ? parseInt(p.iD_StepBaoGia) + 1 : 3);
                    if (nextStep === 4) {
                        p.iD_Status = 'APPROVAL4';
                    } else {
                        p.iD_Status = 'WAIT_SEND_MAIL';
                    }
                    p.chR_UserApproval = '';
                });
                btnApprove.disabled = true;
                const btnReturnEl = document.getElementById('btnReturn'); if (btnReturnEl) btnReturnEl.disabled = true;
                fetch((window.apiBaseUrl || '') + '/ApprovalQuote/UpdateQuotationOK', {
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
            } else {
                openApproverSelector(nextStep, sectionCode).then(selected => {
                    if (!selected) return; // cancelled or none selected
                    // attach chosen approver adid to each payload item
                    payload.forEach(p => {
                        p.chR_UserApproval = selected.chR_UserAdid || '';
                        p.iD_StepBaoGia = (p.iD_StepBaoGia != null ? parseInt(p.iD_StepBaoGia) + 1 : 3);
                        //if (p.iD_StepBaoGia == 6) {
                        //    p.iD_Status = 'WAIT_SEND_MAIL';
                        //} else {
                        //    p.iD_Status = 'APPROVAL';
                        //}
                        p.iD_Status = 'APPROVAL' + p.iD_StepBaoGia;
                    });
                    btnApprove.disabled = true;
                    const btnReturnEl = document.getElementById('btnReturn'); if (btnReturnEl) btnReturnEl.disabled = true;
                    fetch((window.apiBaseUrl || '') + '/ApprovalQuote/UpdateQuotationOK', {
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
                }).catch(err => { console.error(err); });
            }
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
                fetch((window.apiBaseUrl || '') + '/ApprovalQuote/UpdateQuotationNG', {
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
        // Event click Export to Excel
        const btnExport = document.getElementById('btnExcelExport');
        if (btnExport) {
            btnExport.addEventListener('click', async function () {
                try {
                    const payload = [];
                    Array.from(state.selectedMaDons).forEach(maDon => {
                        const group = state.groupsByMaDon[maDon] || [];
                        group.forEach(it => {
                           // it.iD_StepBaoGia = (it.iD_StepBaoGia != null ? parseInt(it.iD_StepBaoGia) + 1 : 1);
                            payload.push(it);
                        });
                    });
                    //if (payload.length === 0) {
                    //    showToast('danger', "Vui lòng chọn đơn muốn xuất dữ liệu");
                    //    return;
                    //} 
                    const res = await fetch((window.apiBaseUrl || '') + '/ApprovalQuote/ExportToExcel', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });
                    if (!res.ok) {
                        const msg = await res.text().catch(() => 'Lỗi không xác định');
                        throw new Error(msg || 'Xuất file thất bại');
                    }
                    const blob = await res.blob();
                    let fileName = 'FileApproverQuote.xlsx';
                    const cd = res.headers.get('content-disposition');
                    if (cd) {
                        const m = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(cd);
                        if (m && m[1]) fileName = m[1].replace(/["']/g, '').trim();
                    }
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = fileName;
                    document.body.appendChild(a);
                    a.click();
                    a.remove();
                    window.URL.revokeObjectURL(url);
                    //hideAr();
                    showToast('success', T.MSGExportOK);
                } catch (e) {
                    showToast('danger', e);
                    console.error('Export to Excel error', e);
                }

            });
        }
        // Event click Import from Excel
        const btnImportExport = document.getElementById('btnImportExport');
        if (btnImportExport) {
            btnImportExport.addEventListener('click', async function () {
                const fileInput = document.createElement('input');
                fileInput.type = 'file';
                fileInput.accept = '.xlsx, .xls';
                fileInput.style.display = 'none';
                document.body.appendChild(fileInput);

                fileInput.addEventListener('change', async function () {
                    const file = fileInput.files[0];
                    if (!file) return;
                    const T = window.i18nApproval || {};

                    const allowedTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
                    if (!allowedTypes.includes(file.type)) {
                        showToast('error', T.InvalidFileType || 'Loại file không hợp lệ');
                        document.body.removeChild(fileInput);
                        return;
                    }

                    const formData = new FormData();
                    formData.append('fileSend', file);

                    try {
                        showToast('info', T.Importing || 'Đang nhập...');
                        const response = await fetch((window.apiBaseUrl || '') + '/ApprovalQuote/ImportExcel', {
                            method: 'POST',
                            body: formData
                        });

                        if (!response.ok) {
                            const errorText = await response.text();
                            throw new Error(errorText || (T.ImportError || 'Nhập file thất bại'));
                        }

                        const contentType = response.headers.get('content-type');
                        if (contentType && contentType.includes('application/json')) {
                            // Success response
                            const result = await response.json();
                            showToast('success', result.message || (T.ImportSuccess || 'Nhập file thành công'));
                            // Optionally refresh the data
                            searchAndRender();
                        } else {
                            // Error file response
                            const blob = await response.blob();
                            let fileName = 'ImportErrors.xlsx';
                            const cd = response.headers.get('content-disposition');
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
                            showToast('warning', T.ImportErrorsFound || 'Có lỗi trong file, vui lòng kiểm tra file tải xuống');
                        }
                    } catch (error) {
                        console.error('Import error', error);
                        showToast('error', error.message || (T.ImportFailed || 'Nhập file thất bại'));
                    } finally {
                        document.body.removeChild(fileInput);
                    }
                });

                fileInput.click();
            });
        }
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
                //it.iD_StepBaoGia = (it.iD_StepBaoGia != null ? parseInt(it.iD_StepBaoGia) + 1 : 1);
                //it.iD_Status = 'APPROVAL';
                payload.push(it);
            });
            // ask user to select next approver for this group
            const nextStep = (payload[0] && typeof payload[0].iD_StepBaoGia === 'number') ? (payload[0].iD_StepBaoGia + 1) : 3;
            const sectionCode = payload[0].chR_SectionCode || '';
            if (nextStep === 6 || nextStep === 4) {
                payload.forEach(p => {
                    p.iD_StepBaoGia = (p.iD_StepBaoGia != null ? parseInt(p.iD_StepBaoGia) + 1 : 3);
                    if (nextStep === 4) {
                        p.iD_Status = 'APPROVAL4';
                    } else {
                        p.iD_Status = 'WAIT_SEND_MAIL';
                    }
                    p.chR_UserApproval = '';
                });
                fetch((window.apiBaseUrl || '') + '/ApprovalQuote/UpdateQuotationOK', {
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
            } else {
                openApproverSelector(nextStep, sectionCode).then(selected => {
                    if (!selected) return;
                    payload.forEach(p => {
                        p.chR_UserApproval = selected.chR_UserAdid || '';
                        p.iD_StepBaoGia = (p.iD_StepBaoGia != null ? parseInt(p.iD_StepBaoGia) + 1 : 3);
                        //if (p.iD_StepBaoGia == 6) {
                        //    p.iD_Status = 'WAIT_SEND_MAIL';
                        //} else {
                        //    p.iD_Status = 'APPROVAL';
                        //}
                        p.iD_Status = 'APPROVAL' + p.iD_StepBaoGia;
                    });
                    fetch((window.apiBaseUrl || '') + '/ApprovalQuote/UpdateQuotationOK', {
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
                }).catch(err => { console.error(err); });
            }
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
                fetch((window.apiBaseUrl || '') + '/ApprovalQuote/UpdateQuotationNG', {
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
            const approverModalEl = document.getElementById('selectApproverModal');
            const isApproverModalOpen = approverModalEl && (approverModalEl.classList.contains('show') || approverModalEl.style.display === 'block');
            if (isApproverModalOpen) return;
            const dialog = modalEl.querySelector('.modal-dialog');
            if (dialog && !dialog.contains(e.target)) {
                hideDetailModal();
            }
        });
        // ESC to close
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                const approverModalEl = document.getElementById('selectApproverModal');
                const isApproverModalOpen = approverModalEl && (approverModalEl.classList.contains('show') || approverModalEl.style.display === 'block');
                if (isApproverModalOpen) return;
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
