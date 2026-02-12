(function () {
    const root = document.getElementById('confirm-name');
    if (!root) return;
    const role = root.getAttribute('data-role') || 'UserPUR';

    const els = {
        tenHang: document.getElementById('tenHang'),
        soDon: document.getElementById('soDon'),
        trangThai: document.getElementById('trangThai'),
        vitri: document.getElementById('searchPhongBan'),
        btnSearch: document.getElementById('btnSearch'),
        tbody: document.getElementById('confirmTableBody'),
        resultCount: document.getElementById('resultCount'),
        prev: document.getElementById('prevPage'),
        next: document.getElementById('nextPage'),
        pageInfo: document.getElementById('pageInfo')
    };

    let state = { pageIndex: 1, pageSize: 20, total: 0 };

    function statusBadge(s) {
        const T = window.i18nConfirmName || {};
        switch ((s || '').toLowerCase()) {
            case 'confirmed': return '<span class="status-badge status-confirmed">' + (T.StatusConfirmed || 'Đã xác nhận') + '</span>';
            case 'confirming': return '<span class="status-badge status-confirming">' + (T.StatusConfirming || 'Đang xác nhận') + '</span>';
            case 'rejected': return '<span class="status-badge status-rejected">' + (T.StatusRejected || 'Từ chối') + '</span>';
            default: return '<span class="status-badge status-draft">' + (T.StatusDraft || 'Mới') + '</span>';
        }
    }

    function canEditTenHQ() { return role === 'UserShip' || role === 'UserPUR'; }
    function canEditMaNB() { return role === 'UserAcc' || role === 'UserPUR'; }
    function canApprove() { return role === 'UserPUR'; }

    function renderRows(data) {
        if (!data || data.length === 0) {
            const T = window.i18nConfirmName || {};
            els.tbody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">' + (T.NoData || 'Không có dữ liệu') + '</td></tr>';
            return;
        }
        els.tbody.innerHTML = data.map((r, i) => {
            const idx = (state.pageIndex - 1) * state.pageSize + i + 1;
            const tenHQ = canEditTenHQ() ? `<input class="form-control form-control-sm js-tenhq" data-id="${r.id}" value="${r.vchR_TenHaiQuan || ''}" />` : `<div class="cell-sm">${r.vchR_TenHaiQuan || ''}</div>`;
            const maNB = canEditMaNB() ? `<input class="form-control form-control-sm js-manb" data-id="${r.id}" value="${r.vchR_MaHangNoiBo || ''}" />` : `<div>${r.vchR_MaHangNoiBo || ''}</div>`;
            const T = window.i18nConfirmName || {};
            const actions = [
                canApprove() ? `<button class="btn btn-sm btn-success js-approve" data-id="${r.id}">${T.BtnApprove || 'Đồng ý'}</button>` : '',
                canApprove() ? `<button class="btn btn-sm btn-outline-danger js-reject" data-id="${r.id}">${T.BtnReject || 'Từ chối'}</button>` : ''
                //canEditTenHQ() ? `<button class="btn btn-sm btn-primary js-save" data-id="${r.id}">Lưu</button>` : '',
                //canEditMaNB() ? `<button class="btn btn-sm btn-primary js-save" data-id="${r.id}">Lưu</button>` : ''
            ].filter(Boolean).join(' ');
            const handler = [r.vchR_UserShip && `Ship: ${r.vchR_UserShip} (${formatDate(r.dtM_UserShip)})`, r.vchR_UserAcc && `Acc: ${r.vchR_UserAcc} (${formatDate(r.dtM_UserAcc)})`, r.vchR_UserPUR && `PUR: ${r.vchR_UserPUR} (${formatDate(r.dtM_UserPUR)})`].filter(Boolean).join('<br/>');
            return `<tr>
            <td class="text-center">${idx}</td>
            <td class="text-center">
                <button type="button" class="btn btn-outline-primary" data-action="detailRQ" data-id="${r.iD_RequestQuote}"><i class="fas fa-edit"></i></button>
            </td>
            <td class="text-center">${r.iD_RequestQuote || ''}</td>
            <td>${r.vchR_TenRecomment || ''}</td>
            <td>${tenHQ}</td>
            <td>${maNB}</td>
            <td class="text-center">${statusBadge(r.chR_Status)}</td>
            <td class="text-center">${formatDate(r.dtM_CreateDate)}</td>
            <td class="text-center">${handler || ((T.CreatedByPrefix || 'Khởi tạo bởi ') + r.vchR_CreateBy)}</td>
            <td><div class="small text-muted">${r.nvchR_Note || ''}</div><div class="text-danger small">${r.nvchR_LyDo || ''}</div></td>
            <td class="text-center">${actions}</td>
      </tr>`;
        }).join('');

        // attach events
        els.tbody.querySelectorAll('.js-tenhq').forEach(el => {
            el.addEventListener('change', () => saveInline(parseInt(el.getAttribute('data-id')), { tenHaiQuan: el.value }));
        });
        els.tbody.querySelectorAll('.js-manb').forEach(el => {
            el.addEventListener('change', () => saveInline(parseInt(el.getAttribute('data-id')), { maHangNoiBo: el.value }));
        });
        els.tbody.querySelectorAll('.js-approve').forEach(btn => btn.addEventListener('click', () => approve(parseInt(btn.getAttribute('data-id')))));
        els.tbody.querySelectorAll('.js-reject').forEach(btn => btn.addEventListener('click', () => reject(parseInt(btn.getAttribute('data-id')))));
        //els.tbody.querySelectorAll('.js-save').forEach(btn => btn.addEventListener('click', () => save(parseInt(btn.getAttribute('data-id')))));
        // detail buttons
        els.tbody.querySelectorAll('[data-action="detailRQ"]').forEach(btn => {
            btn.addEventListener('click', () => {
                const id = parseInt(btn.getAttribute('data-id'));
                if (!isNaN(id)) loadRequestDetail(id);
            });
        });
    }

    function formatDate(d) {
        if (window.cmMomentFormat) { return window.cmMomentFormat(d); }
        if (!d) return '';
        const dt = new Date(d);
        if (isNaN(dt.getTime())) return '';
        const pad = n => n.toString().padStart(2, '0');
        return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())} ${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
    }
    async function loadRequestDetail(id) {
        try {
            const res = await fetch('/Material/SearchID', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(id) });
            if (!res.ok) { alert('Không tải được chi tiết'); return; }
            const r = await res.json();
            // populate modal fields safely
            const setVal = (id, val) => { const el = document.getElementById(id); if (el) el.value = val ?? ''; };
            setVal('editRequestId', r.id ?? '');
            setVal('editMaDon', r.chR_MaDon ?? '');
            setVal('editRequester', r.vchR_CreateBy ?? '');
            setVal('editSectionName', r.chR_SectionName ?? '');
            setVal('editPhanLoai', r.chR_Phanloai ?? '');
            setVal('editChungLoai', r.nvchR_ChungLoai ?? '');
            setVal('editMaHangNoiBo', r.vchR_MaHangNoiBo ?? '');
            setVal('editMaThietBi', r.chR_MaThietBi ?? '');
            setVal('editMaHangNCC', r.chR_MaHangNCC ?? '');
            setVal('editTenHangVN', r.nvchR_NameVN ?? '');
            setVal('editTenHangEN', r.chR_NameEN ?? '');
            setVal('editSoLuong', r.inT_SoLuong ?? '');
            setVal('editDonVi', r.nvchR_DonVi ?? '');
            setVal('editHinhDang', r.nvchR_HinhDang ?? '');
            setVal('editChatLieu', r.nvchR_ChatLieu ?? '');
            setVal('editThanhPhan', r.nvchR_ThanhPhan ?? '');
            setVal('editKichThuoc', r.nvchR_KichThuoc ?? '');
            setVal('editDongMay', r.nvchR_DongMay ?? '');
            setVal('editTinhNang', r.nvchR_TinhNang ?? '');
            setVal('editRohs', r.nvchR_Rohs ?? '');
            setVal('editCOCQ', r.nvchR_COCQ ?? '');
            setVal('editMSDS', r.nvchR_MSDS ?? '');
            setVal('editAnToan', r.nvchR_AnToan ?? '');
            setVal('editFileThietKe', r.nvchR_FileThietKe ?? '');
            setVal('editNhaSanXuat', r.nvchR_NhaSanXuat ?? '');
            setVal('editNhaCungCap', r.nvchR_TenNCC ?? '');
            // selects
            const layBaoGiaEl = document.getElementById('editLayBaoGia'); if (layBaoGiaEl) layBaoGiaEl.value = String(r.biT_LayBaoGia ?? 'false');
            setVal('editLyDo', r.lyDo ?? r.nvchR_LyDo ?? '');
            setVal('editNgayMuonNhan', (r.dtM_NgayMuonNhan ?? '').toString().substring(0, 10));
            setVal('editKyHan', (r.dtM_KyHan ?? '').toString().substring(0, 10));
            const gapEl = document.getElementById('editGap'); if (gapEl) gapEl.value = String(r.chR_Gap ?? 'false');

            // show modal
            showModal();
        } catch (e) {
            console.error('Load detail error', e);
            alert('Có lỗi khi tải dữ liệu chi tiết');
        }
    }

    async function search() {
        const body = {
            tenHang: els.tenHang.value.trim(),
            soDon: els.soDon.value.trim(),
            trangThai: els.trangThai.value,
            Section: els.vitri.value,
            pageIndex: state.pageIndex,
            pageSize: state.pageSize
        };
        const res = await fetch('/Material/SearchConfirmName', {
            method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)
        });
        if (!res.ok) { const T = window.i18nConfirmName || {}; console.error(T.MsgSearchFailed || 'Search failed'); return; }
        const data = await res.json();
        state.total = data.total || 0;
        state.pageIndex = data.pageIndex || 1;
        state.pageSize = data.pageSize || 20;
        const T = window.i18nConfirmName || {};
        els.resultCount.textContent = `${T.Total || 'Tổng'}: ${state.total}`;
        const totalPages = Math.max(1, Math.ceil(state.total / state.pageSize));
        els.pageInfo.textContent = `${state.pageIndex}/${totalPages}`;
        renderRows(data.data || []);
    }

    async function saveInline(id, payload) {
        const body = Object.assign({ id, role }, payload);
        const res = await fetch('/Material/SaveConfirmName', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        if (!res.ok) { const T = window.i18nConfirmName || {}; alert(T.MsgSaveFailed || 'Lưu thất bại'); }
    }

    async function approve(id) {
        const T = window.i18nConfirmName || {};
        const ok = await showConfirmDialog(T.ConfirmApproveTitle || 'Xác nhận đồng ý?', T.ConfirmApproveMessage || 'Bạn có chắc chắn muốn phê duyệt yêu cầu này?');
        if (!ok) return;
        const res = await fetch('/Material/ApproveConfirmName', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id }) });
        if (res.ok) { search(); } else { alert(T.MsgGenericError || 'Thao tác thất bại'); }
    }

    async function reject(id) {
        const T = window.i18nConfirmName || {};
        const lyDo = await showReasonDialog(T.ReasonTitle || 'Nhập lý do từ chối', T.ReasonMessage || 'Vui lòng nhập lý do từ chối xử lý yêu cầu này:');
        if (lyDo === null) return;
        const res = await fetch('/Material/RejectConfirmName', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id, lyDo }) });
        if (res.ok) { search(); } else { alert(T.MsgGenericError || 'Thao tác thất bại'); }
    }

    els.btnSearch.addEventListener('click', () => { state.pageIndex = 1; search(); });
    document.getElementById('btnCloseEdit_1').addEventListener('click', function () {
        hideEditModal();
    });
    document.getElementById('btnCloseEdit_2').addEventListener('click', function () {
        hideEditModal();
    });
    // Reset button
    const resetBtn = document.getElementById('btnReset');
    if (resetBtn) {
        resetBtn.addEventListener('click', () => {
            const T = window.i18nConfirmName || {};
            // clear inputs
            els.tenHang.value = '';
            els.soDon.value = '';
            els.trangThai.value = '';
            // reset searchable select if present
            const sp = document.getElementById('searchPhongBan');
            if (sp) {
                sp.value = '';
                try { sp.dispatchEvent(new Event('change', { bubbles: true })); } catch { }
                // if enhanced UI exists update text
                const wrapper = sp.nextElementSibling;
                if (wrapper && wrapper.classList.contains('ms-container')) {
                    const valuesEl = wrapper.querySelector('.ms-values');
                    const placeholderEl = wrapper.querySelector('.ms-placeholder');
                    if (valuesEl && placeholderEl) { valuesEl.textContent = ''; placeholderEl.textContent = (T.SelectPlaceholder || '-- Chọn --'); }
                }
            }
            // reset paging
            state.pageIndex = 1;
            state.pageSize = 20;
            search();
        });
    }
    els.prev.addEventListener('click', () => { if (state.pageIndex > 1) { state.pageIndex--; search(); } });
    els.next.addEventListener('click', () => { const totalPages = Math.max(1, Math.ceil(state.total / state.pageSize)); if (state.pageIndex < totalPages) { state.pageIndex++; search(); } });
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
    // enhance searchable selects inside the page
    try {
        buildSearchableDropdown(document.getElementById('confirm-name'));
    } catch (e) { /* ignore if function not available */ }
    // Custom dialogs
    function showConfirmDialog(title, message) {
        return new Promise((resolve) => {
            const el = document.getElementById('cmConfirmDialog');
            if (!el) { resolve(false); return; }
            const T = window.i18nConfirmName || {};
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

    function showReasonDialog(title, message) {
        return new Promise((resolve) => {
            const el = document.getElementById('cmReasonDialog');
            if (!el) { resolve(null); return; }
            const T = window.i18nConfirmName || {};
            el.querySelector('.cm-reason-title').textContent = title || (T.EnterReason || 'Nhập lý do');
            el.querySelector('.cm-reason-body').textContent = message || '';
            const input = el.querySelector('#cmReasonInput');
            //const overlay = el.querySelector('.cm-dialog-backdrop');
            const btnCancel = el.querySelector('[data-cm-action="cancel"]');
            const btnOk = el.querySelector('[data-cm-action="ok"]');
            input.value = '';
            const close = () => { el.setAttribute('aria-hidden', 'true'); el.classList.remove('show'); el.style.display = 'none'; document.body.classList.remove('modal-open'); cleanup(); };
           const open = () => { el.style.display = 'block'; el.style.zIndex = '3000'; el.setAttribute('aria-hidden', 'false'); el.classList.add('show'); document.body.classList.add('modal-open'); input && input.focus(); };
            const onCancel = () => { close(); resolve(null); };
            const onOk = () => { const v = (input.value || '').trim(); close(); resolve(v); };
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
    // initial search
    search();
})();
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
        const T = window.i18nConfirmName || {};
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
