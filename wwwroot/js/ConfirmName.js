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
            case 'confirming': return '<span class="status-badge status-confirming">' + (T.StatusConfirming || 'Đang xác nhận') + '</span';
            case 'rejected': return '<span class="status-badge status-rejected">' + (T.StatusRejected || 'Từ chối') + '</span>';
            default: return '<span class="status-badge status-draft">' + (T.StatusDraft || 'Mới') + '</span>';
        }
    }

    function canEditTenHQ() { return role === 'UserShip' || role === 'UserPUR'; }
    function canEditMaNB() { return role === 'UserAcc' || role === 'UserPUR'; }
    function canApprove() { return role === 'UserPUR'; }

    function renderRows(data) {
        const T = window.i18nConfirmName || {};
        const fields = [
            { key: 'ID_RequestQuote', label: 'Số đơn yêu cầu báo giá', editable: false },
            { key: 'CHR_SectionName', label: 'Phòng ban', editable: false },
            { key: 'CHR_Phanloai', label: 'Phân loại thiết bị', editable: false },
            { key: 'CHR_MaThietBi', label: 'Mã thiết bị', editable: false },
            { key: 'CHR_MaHangNCC', label: 'Mã hàng của NCC', editable: false },
            { key: 'VCHR_TenRecomment', label: 'Tên hàng VN dùng để mở thủ tục hải quan (dự thảo)(*)', editable: false },
            { key: 'CHR_NameEN', label: 'Tên hàng tiếng anh(*)', editable: false },
            { key: 'INT_SoLuong', label: 'Số lượng(*)', editable: false },
            { key: 'NVCHR_DonVi', label: 'Đơn vị(*)', editable: false },
            { key: 'NVCHR_ChungLoai', label: 'Chủng loại hàng', editable: false },
            { key: 'NVCHR_HinhDang', label: 'Hình dáng', editable: false },
            { key: 'NVCHR_ChatLieu', label: 'Chất liệu', editable: false },
            { key: 'NVCHR_ThanhPhan', label: 'Thành phần, hàm lượng (đối với hóa chất)', editable: false },
            { key: 'NVCHR_KichThuoc', label: 'Kích thước(mm) (dài/rộng/cao)', editable: false },
            { key: 'NVCHR_DongMay', label: 'Dùng cho máy/thiết bị/vị trí nào', editable: false },
            { key: 'NVCHR_TinhNang', label: 'Dùng để làm gì (tính năng)', editable: false },
            { key: 'tenHQ', label: 'Xác nhận tên', editable: canEditTenHQ() },
            { key: 'maNB', label: 'Xác nhận mã', editable: canEditMaNB() },
            { key: 'status', label: 'Trạng thái', editable: false },
            { key: 'dtM_CreateDate', label: 'Ngày tạo', editable: false },
            { key: 'handler', label: 'Người xử lý', editable: false },
            { key: 'note', label: 'Ghi chú', editable: false },
            { key: 'actions', label: 'Hành động', editable: false }
        ];

        // Đánh dấu 5 hàng cuối là cố định (sticky bottom)
        fields.slice(-7).forEach(f => f.isFixed = true);

        // Xóa thead cũ nếu có
        const existingThead = els.tbody.previousElementSibling;
        if (existingThead && existingThead.tagName === 'THEAD') {
            existingThead.remove();
        }

        if (!data || data.length === 0) {
            els.tbody.innerHTML = '<tr><td colspan="1" class="text-center text-muted">' + (T.NoData || 'Không có dữ liệu') + '</td></tr>';
            return;
        }

        // Tạo thead động
        const thead = document.createElement('thead');
        thead.className = 'table-light';
        thead.style.position = 'sticky';
        thead.style.top = '0';
        thead.style.zIndex = '20';
        thead.style.backgroundColor = 'white';
        thead.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
        const headerRow = document.createElement('tr');
        headerRow.innerHTML = '<th style="min-width: 250px; background-color: #2335B7; color: #FFFF; position: sticky; left: 0; z-index: 21; box-shadow: 2px 0 4px rgba(0,0,0,0.1);">Thuộc tính</th>' +
            data.map((r, i) => `<th style="min-width: 200px; text-align: center; background-color: #e0e0e0;">Bản ghi ${(state.pageIndex - 1) * state.pageSize + i + 1}</th>`).join('');
        thead.appendChild(headerRow);
        els.tbody.parentNode.insertBefore(thead, els.tbody);

        // Cấu hình bảng
        const table = els.tbody.closest('table');
        if (table) {
            table.style.overflowX = 'auto';
            table.style.width = '100%';
            table.style.borderCollapse = 'collapse';
            table.style.boxShadow = '0 0 8px rgba(0,0,0,0.05)';
        }

        // Render tbody
        els.tbody.innerHTML = '';
        fields.forEach(field => {
            const tr = document.createElement('tr');
            if (field.isFixed) tr.classList.add('fixed-row');

            const th = document.createElement('th');
            th.innerHTML = field.label;
            th.style.fontWeight = 'bold';
            th.style.backgroundColor = '#2335B7';
            th.style.color = '#FFFF';
            th.style.position = 'sticky';
            th.style.left = '0';
            th.style.zIndex = '15';
            th.style.minWidth = '250px';
            th.style.boxShadow = '2px 0 4px rgba(0,0,0,0.1)';
            th.style.padding = '8px 12px';
            // If this field is one of the last 7 (marked fixed), make header green
            if (field.isFixed) {
                th.style.backgroundColor = '#28a745';
                th.style.color = '#ffffff';
            }
            tr.appendChild(th);

            data.forEach(r => {
                const td = document.createElement('td');
                td.style.padding = '8px 12px';
                td.style.verticalAlign = 'middle';
                // highlight fixed rows with green background
                //if (field.isFixed) {
                //    td.style.backgroundColor = '#d4edda';
                //}

                if (field.key === 'status') {
                    td.innerHTML = statusBadge(r.CHR_Status);
                    td.style.textAlign = 'center';
                } else if (field.key === 'tenHQ') {
                    if (field.editable) {
                        td.innerHTML = `<input class="form-control form-control-sm js-tenhq" data-id="${r.ID}" value="${r.VCHR_TenHaiQuan || ''}" />`;
                    } else {
                        td.innerHTML = `<div class="cell-sm">${r.VCHR_TenHaiQuan || ''}</div>`;
                    }
                } else if (field.key === 'maNB') {
                    if (canEditMaNB()) {
                        td.innerHTML = `<input class="form-control form-control-sm js-manb" data-id="${r.ID}" value="${r.VCHR_MaHangNoiBo || ''}" />`;
                    } else {
                        td.innerHTML = `<div>${r.VCHR_MaHangNoiBo || ''}</div>`;
                    }
                } else if (field.key === 'actions') {
                    const actions = [
                        canApprove() ? `<button class="btn btn-sm btn-success js-approve" data-id="${r.ID}">${T.BtnApprove || 'Đồng ý'}</button>` : '',
                        canApprove() ? `<button class="btn btn-sm btn-outline-danger js-reject" data-id="${r.ID}">${T.BtnReject || 'Từ chối'}</button>` : ''
                    ].filter(Boolean).join(' ');
                    td.innerHTML = actions;
                    td.style.textAlign = 'center';
                } else if (field.key === 'dtM_CreateDate') {
                    td.textContent = formatDate(r[field.key]);
                    td.style.textAlign = 'center';
                } else if (field.key === 'handler') {
                    const handler = [
                        r.VCHR_UserShip && `Ship: ${r.VCHR_UserShip} (${formatDate(r.DTM_UserShip)})`,
                        r.VCHR_UserAcc && `Acc: ${r.VCHR_UserAcc} (${formatDate(r.DTM_UserAcc)})`,
                        r.VCHR_UserPUR && `PUR: ${r.VCHR_UserPUR} (${formatDate(r.DTM_UserPUR)})`
                    ].filter(Boolean).join('<br/>');
                    td.innerHTML = handler || ((T.CreatedByPrefix || 'Khởi tạo bởi ') + r.VCHR_CreateBy);
                    td.style.textAlign = 'center';
                } else if (field.key === 'note') {
                    td.innerHTML = `<div class="small text-muted">${r.NVCHR_Note || ''}</div><div class="text-danger small">${r.NVCHR_LyDo || ''}</div>`;
                } else {
                    td.textContent = r[field.key] || '';
                }
                tr.appendChild(td);
            });
            els.tbody.appendChild(tr);
        });

        // Gán sự kiện cho các input/button
        els.tbody.querySelectorAll('.js-tenhq').forEach(el => {
            el.addEventListener('change', () => saveInline(parseInt(el.getAttribute('data-id')), { tenHaiQuan: el.value }));
        });
        els.tbody.querySelectorAll('.js-manb').forEach(el => {
            el.addEventListener('change', () => saveInline(parseInt(el.getAttribute('data-id')), { maHangNoiBo: el.value }));
        });
        els.tbody.querySelectorAll('.js-approve').forEach(btn => btn.addEventListener('click', () => approve(parseInt(btn.getAttribute('data-id')))));
        els.tbody.querySelectorAll('.js-reject').forEach(btn => btn.addEventListener('click', () => reject(parseInt(btn.getAttribute('data-id')))));

        // Xử lý sticky bottom cho 5 hàng cố định (dùng chiều cao thực tế)
        const fixedTrs = els.tbody.querySelectorAll('.fixed-row');
        if (fixedTrs.length > 0) {
            // Áp dụng style chung trước khi đo
            fixedTrs.forEach(tr => {
                tr.style.position = 'sticky';
                tr.style.backgroundColor = '#f9f9f9';
                tr.style.zIndex = '16';
                tr.style.boxShadow = '0 -2px 4px rgba(0,0,0,0.1)';
                tr.style.borderTop = '1px solid #ddd';
                // Không fix height, để tự động theo nội dung
            });

            // Đo chiều cao thực tế
            const heights = Array.from(fixedTrs).map(tr => tr.offsetHeight);

            // Tính bottom cho từng hàng (hàng cuối bottom = 0, hàng trên bottom = tổng chiều cao các hàng bên dưới)
            fixedTrs.forEach((tr, i) => {
                let bottomOffset = 0;
                for (let j = i + 1; j < fixedTrs.length; j++) {
                    bottomOffset += heights[j];
                }
                tr.style.bottom = bottomOffset + 'px';
            });
        }
    }
    
    function formatDate(d) {
        if (window.cmMomentFormat) { return window.cmMomentFormat(d); }
        if (!d) return '';
        const dt = new Date(d);
        if (isNaN(dt.getTime())) return '';
        const pad = n => n.toString().padStart(2, '0');
        return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())} ${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
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
        state.total = data.data.totalCount || 0;
        state.pageIndex = data.pageIndex || 1;
        state.pageSize = data.pageSize || 20;
        const T = window.i18nConfirmName || {};
        els.resultCount.textContent = `${T.Total || 'Tổng'}: ${state.total}`;
        const totalPages = Math.max(1, Math.ceil(state.total / state.pageSize));
        els.pageInfo.textContent = `${state.pageIndex}/${totalPages}`;
        renderRows(data.data.data || []);
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
    //document.getElementById('btnCloseEdit_1').addEventListener('click', function () {
    //    hideEditModal();
    //});
    //document.getElementById('btnCloseEdit_2').addEventListener('click', function () {
    //    hideEditModal();
    //});
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
