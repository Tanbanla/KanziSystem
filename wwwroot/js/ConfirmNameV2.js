(function () {
    const root = document.getElementById('confirm-name');
    if (!root) return;

    const role = root.getAttribute('data-role') || 'User';

    const els = {
        tenHang: document.getElementById('tenHang'),
        soDon: document.getElementById('soDon'),
        trangThai: document.getElementById('trangThai'),
        vitri: document.getElementById('searchPhongBan'),
        createdByFilter: document.getElementById('createdByFilter'),
        fromDateFilter: document.getElementById('fromDateFilter'),
        toDateFilter: document.getElementById('toDateFilter'),
        quickSearch: document.getElementById('quickSearch'),

        btnSearch: document.getElementById('btnSearch'),
        btnReset: document.getElementById('btnReset'),
        btnImportExcel: document.getElementById('btnImportExcel'),
        btnExportTemplate: document.getElementById('btnExportTemplate'),
        btnExportTable: document.getElementById('btnExportTable'),
        itemsExcelFileInput: document.getElementById('itemsExcelFileInput'),

        btnSaveSelected: document.getElementById('btnSaveSelected'),
        btnRejectShipSelected: document.getElementById('btnRejectShipSelected'),
        chkSelectAll: document.getElementById('chkSelectAll'),
        headerCheckAll: document.getElementById('_row_check_all'),
        selectedCount: document.getElementById('selectedCount'),

        resultCount: document.getElementById('resultCount'),
        pageInfo: document.getElementById('pageInfo'),
        prev: document.getElementById('prevPage'),
        next: document.getElementById('nextPage'),
        pageSizeSelect: document.getElementById('pageSizeSelect'),

        tabPending: document.getElementById('tabPending'),
        tabConfirmed: document.getElementById('tabConfirmed'),
        pendingTabPane: document.getElementById('pendingTabPane'),
        confirmedTabPane: document.getElementById('confirmedTabPane'),
        pendingActionBar: document.getElementById('pendingActionBar'),

        tbody: document.getElementById('confirmTableBody'),
        confirmedCardList: document.getElementById('confirmedCardList'),

        kpiConfirming: document.getElementById('kpiConfirming'),
        kpiConfirmed: document.getElementById('kpiConfirmed'),
        kpiRejected: document.getElementById('kpiRejected'),
        kpiTotal: document.getElementById('kpiTotal'),

        drawer: document.getElementById('confirmDetailDrawer'),
        drawerOverlay: document.getElementById('confirmDrawerOverlay'),
        btnCloseDrawer: document.getElementById('btnCloseDrawer'),
        drawerHistoryTimeline: document.getElementById('drawerHistoryTimeline')
    };

    const state = {
        activeTab: 'pending',
        pageIndex: 1,
        pageSize: 20,
        total: 0,
        listData: [],
        selectedIds: new Set(),
        serverPaging: false
    };

    const T = window.i18nConfirmName || {};

    function canEditTenHQ() { return role === 'UserShip' || role === 'UserPUR'; }
    function canReject() { return role === 'UserShip'; }

    function getDisplayStatus(r) {
        if (role === 'UserShip') return r.CHR_StatusShip || r.CHR_Status;
        if (role === 'UserPUR') return r.CHR_Status;
        return r.CHR_StatusACC || r.CHR_Status;
    }

    function statusBadge(status) {
        const txt = (status || '').toString();
        const v = txt.toLowerCase();
        if (v === 'confirmed') return `<span class="status-badge status-confirmed">${T.StatusConfirmed || 'Confirmed'}</span>`;
        if (v === 'confirming') return `<span class="status-badge status-confirming">${T.StatusConfirming || 'Confirming'}</span>`;
        if (v === 'rejected') return `<span class="status-badge status-rejected">${T.StatusRejected || 'Rejected'}</span>`;
        return `<span class="status-badge status-unknown">${txt || '-'}</span>`;
    }

    function formatDate(value) {
        if (!value) return '';
        const d = new Date(value);
        if (isNaN(d.getTime())) return '';
        const p = n => n.toString().padStart(2, '0');
        return `${p(d.getDate())}/${p(d.getMonth() + 1)}/${d.getFullYear()} ${p(d.getHours())}:${p(d.getMinutes())}`;
    }

    function getFieldValue(el) { return ((el && el.value) || '').toString().trim(); }

    function parseDateOnly(value) {
        if (!value) return null;
        const dt = new Date(value + 'T00:00:00');
        return isNaN(dt.getTime()) ? null : dt;
    }

    function getStatusForSearch() {
        const selectStatus = getFieldValue(els.trangThai);
        if (selectStatus) return selectStatus;
        return state.activeTab === 'confirmed' ? 'Confirmed' : 'Confirming';
    }

    function updateTabUi() {
        const pending = state.activeTab === 'pending';
        els.tabPending?.classList.toggle('active', pending);
        els.tabConfirmed?.classList.toggle('active', !pending);
        els.pendingTabPane?.classList.toggle('d-none', !pending);
        els.confirmedTabPane?.classList.toggle('d-none', pending);
        els.pendingActionBar?.classList.toggle('d-none', !pending);
    }

    function applyClientFilters(data) {
        let rows = Array.isArray(data) ? data.slice() : [];

        const creator = getFieldValue(els.createdByFilter).toLowerCase();
        const quick = getFieldValue(els.quickSearch).toLowerCase();
        const fromDate = parseDateOnly(getFieldValue(els.fromDateFilter));
        const toDate = parseDateOnly(getFieldValue(els.toDateFilter));

        if (creator) {
            rows = rows.filter(r => ((r.VCHR_CreateBy || '').toLowerCase().includes(creator)));
        }

        if (fromDate || toDate) {
            rows = rows.filter(r => {
                const dt = r.DTM_CreateDate ? new Date(r.DTM_CreateDate) : null;
                if (!dt || isNaN(dt.getTime())) return false;
                const day = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());
                if (fromDate && day < fromDate) return false;
                if (toDate && day > toDate) return false;
                return true;
            });
        }

        if (quick) {
            rows = rows.filter(r => {
                const stack = [
                    r.NVCHR_Note,
                    r.VCHR_MaHangNoiBo,
                    r.VCHR_TenHaiQuan,
                    r.VCHR_TenRecomment,
                    r.CHR_NameEN
                ].map(x => (x || '').toString().toLowerCase()).join(' | ');
                return stack.includes(quick);
            });
        }

        return rows;
    }

    function updateSelectedCount() {
        if (els.selectedCount) els.selectedCount.textContent = String(state.selectedIds.size);
    }

    function syncSelectAll() {
        const checks = Array.from(els.tbody?.querySelectorAll('.row-select') || []);
        const allChecked = checks.length > 0 && checks.every(c => c.checked);
        if (els.chkSelectAll) els.chkSelectAll.checked = allChecked;
        if (els.headerCheckAll) els.headerCheckAll.checked = allChecked;
    }

    function getCurrentPageRows() {
        const rows = applyClientFilters(state.listData);
        if (state.serverPaging) {
            const totalPages = Math.max(1, Math.ceil(state.total / state.pageSize));
            if (state.pageIndex > totalPages) state.pageIndex = totalPages;
            return { rows, totalPages };
        }

        state.total = rows.length;
        const totalPages = Math.max(1, Math.ceil(rows.length / state.pageSize));
        if (state.pageIndex > totalPages) state.pageIndex = totalPages;
        const from = (state.pageIndex - 1) * state.pageSize;
        const to = from + state.pageSize;
        return { rows: rows.slice(from, to), totalPages };
    }

    function renderPendingTable() {
        const { rows, totalPages } = getCurrentPageRows();
        els.resultCount.textContent = `${T.Total || 'Tổng'}: ${state.total}`;
        els.pageInfo.textContent = `${state.pageIndex}/${totalPages}`;

        if (!rows.length) {
            els.tbody.innerHTML = `<tr><td colspan="11" class="text-center text-muted py-4">${T.NoData || 'Không có dữ liệu'}</td></tr>`;
            updateSelectedCount();
            syncSelectAll();
            return;
        }

        els.tbody.innerHTML = rows.map(r => {
            const checked = state.selectedIds.has(r.ID) ? 'checked' : '';
            const actions = [
                `<button class="btn btn-sm btn-outline-primary js-save" data-id="${r.ID}"><i class="fas fa-save"></i></button>`,
                canReject() ? `<button class="btn btn-sm btn-outline-danger js-reject" data-id="${r.ID}"><i class="fas fa-times"></i></button>` : '',
                `<button class="btn btn-sm btn-outline-secondary js-detail" data-id="${r.ID}"><i class="fas fa-eye"></i></button>`
            ].filter(Boolean).join(' ');

            const tenHQCell = canEditTenHQ()
                ? `<input class="form-control form-control-sm js-tenhq" data-id="${r.ID}" value="${escapeHtml(r.VCHR_TenHaiQuan || '')}" />`
                : escapeHtml(r.VCHR_TenHaiQuan || '');

            return `
                <tr>
                    <td><input type="checkbox" class="row-select" data-id="${r.ID}" ${checked} /></td>
                    <td>${escapeHtml(r.CHR_MaDon || '')}</td>
                    <td>${escapeHtml(r.CHR_MaHangNoiBo || '')}</td>
                    <td class="text-truncate" style="max-width:240px;" title="${escapeHtml(r.VCHR_TenRecomment || '')}">${escapeHtml(r.VCHR_TenRecomment || '')}</td>
                    <td class="text-truncate" style="max-width:220px;" title="${escapeHtml(r.VCHR_TenHaiQuan || '')}">${tenHQCell}</td>
                    <td class="text-truncate" style="max-width:200px;" title="${escapeHtml(r.CHR_NameEN || '')}">${escapeHtml(r.CHR_NameEN || '')}</td>
                    <td>${escapeHtml(r.VCHR_CreateBy || '')}</td>
                    <td>${formatDate(r.DTM_CreateDate)}</td>
                    <td>${escapeHtml(r.VCHR_UpdateBy || r.VCHR_UserPUR || r.VCHR_UserShip || '-')}</td>
                    <td>${statusBadge(getDisplayStatus(r))}</td>
                    <td class="text-center"><div class="d-flex gap-1 justify-content-center">${actions}</div></td>
                </tr>`;
        }).join('');

        wirePendingEvents();
        updateSelectedCount();
        syncSelectAll();
    }

    function renderConfirmedCards() {
        const { rows, totalPages } = getCurrentPageRows();
        els.resultCount.textContent = `${T.Total || 'Tổng'}: ${state.total}`;
        els.pageInfo.textContent = `${state.pageIndex}/${totalPages}`;

        if (!rows.length) {
            els.confirmedCardList.innerHTML = `<div class="text-center text-muted py-4">${T.NoData || 'Không có dữ liệu'}</div>`;
            return;
        }

        els.confirmedCardList.innerHTML = rows.map(r => `
            <article class="confirm-card" data-id="${r.ID}">
                <div class="confirm-card-main js-card-detail" data-id="${r.ID}">
                    <div class="confirm-card-row"><span>Mã đơn:</span><b>${escapeHtml(r.CHR_MaDon || '-')}</b></div>
                    <div class="confirm-card-row"><span>Mã vật tư:</span><b>${escapeHtml(r.CHR_MaHangNoiBo || '-')}</b></div>
                    <div class="confirm-card-row"><span>Tên xác nhận:</span><b>${escapeHtml(r.VCHR_TenHaiQuan || r.VCHR_TenRecomment || '-')}</b></div>
                    <div class="confirm-card-meta">
                        ${statusBadge(getDisplayStatus(r))}
                        <span>Người xác nhận: <b>${escapeHtml( r.VCHR_UserShip || '-')}</b></span>
                        <span>Ngày xác nhận: <b>${formatDate(r.DTM_UserShip || r.DTM_UpdateDate)}</b></span>
                    </div>
                </div>
                <div class="confirm-card-side">
                    <div class="small text-muted">Cập nhật cuối</div>
                    <div><b>${escapeHtml(r.VCHR_UpdateBy || '-')}</b></div>
                    <div class="small">${formatDate(r.DTM_UpdateDate)}</div>
                    <div class="mt-2 d-flex gap-2 justify-content-end">
                        <button class="btn btn-sm btn-outline-primary js-detail" data-id="${r.ID}">Chi tiết</button>
                        <button class="btn btn-sm btn-outline-secondary js-history" data-id="${r.ID}">Lịch sử</button>
                    </div>
                </div>
            </article>`).join('');

        els.confirmedCardList.querySelectorAll('.js-card-detail,.js-detail').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const id = parseInt(e.currentTarget.getAttribute('data-id'));
                const item = state.listData.find(x => x.ID === id);
                openDrawer(item, false);
            });
        });

        els.confirmedCardList.querySelectorAll('.js-history').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const id = parseInt(e.currentTarget.getAttribute('data-id'));
                const item = state.listData.find(x => x.ID === id);
                openDrawer(item, true);
            });
        });
    }

    function wirePendingEvents() {
        els.tbody.querySelectorAll('.row-select').forEach(ch => {
            ch.addEventListener('change', () => {
                const id = parseInt(ch.getAttribute('data-id'));
                if (ch.checked) state.selectedIds.add(id); else state.selectedIds.delete(id);
                updateSelectedCount();
                syncSelectAll();
            });
        });

        els.tbody.querySelectorAll('.js-save').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = parseInt(btn.getAttribute('data-id'));
                const ten = els.tbody.querySelector(`.js-tenhq[data-id="${id}"]`);
                const ma = els.tbody.querySelector(`.js-manb[data-id="${id}"]`);
                const payload = {};
                if (ten) payload.tenHaiQuan = ten.value;
                if (ma) payload.maHangNoiBo = ma.value;
                if (Object.keys(payload).length) await saveInline(id, payload);
            });
        });

        els.tbody.querySelectorAll('.js-reject').forEach(btn => {
            btn.addEventListener('click', () => reject(parseInt(btn.getAttribute('data-id'))));
        });

        els.tbody.querySelectorAll('.js-detail').forEach(btn => {
            btn.addEventListener('click', () => {
                const id = parseInt(btn.getAttribute('data-id'));
                const item = state.listData.find(x => x.ID === id);
                openDrawer(item, false);
            });
        });
    }

    async function search(fetchKpi = true) {
        const body = {
            tenHang: getFieldValue(els.tenHang),
            soDon: getFieldValue(els.soDon),
            trangThai: getStatusForSearch(),
            section: getFieldValue(els.vitri),
            pageIndex: state.pageIndex,
            pageSize: state.pageSize
        };

        try {
            showLoading(T.LoadingMessage || 'Đang xử lý...');
            const res = await fetch((window.apiBaseUrl || '') + '/Material/SearchConfirmName', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (!res.ok) throw new Error(T.MsgSearchFailed || 'Search failed');

            const json = await res.json();
            const payload = json.data || {};
            state.listData = payload.data || [];

            const totalCount = Number(payload.totalCount);
            if (Number.isFinite(totalCount) && totalCount >= 0) {
                state.total = totalCount;
                state.serverPaging = totalCount > state.listData.length || state.pageIndex > 1;
            } else {
                state.total = state.listData.length;
                state.serverPaging = false;
            }

            if (state.activeTab === 'pending') renderPendingTable();
            else renderConfirmedCards();

            if (fetchKpi) await loadKpi();
        } catch (e) {
            console.error(e);
            showDialog({ title: T.Error || 'Lỗi', message: e.message || (T.MsgSearchFailed || 'Search failed'), type: 'error' });
        } finally {
            hideLoading();
        }
    }

    async function reject(id) {
        const lyDo = await showReasonDialog(T.ReasonTitle || 'Nhập lý do', T.ReasonMessage || 'Vui lòng nhập lý do từ chối');
        if (lyDo === null) return;
        try {
            showLoading(T.Processing || 'Đang xử lý...');
            const res = await fetch((window.apiBaseUrl || '') + '/Material/RejectConfirmName', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id, lyDo })
            });
            if (!res.ok) throw new Error(T.MsgGenericError || 'Thao tác thất bại');
            await search(true);
        } catch (e) {
            showDialog({ title: T.Error || 'Lỗi', message: e.message || (T.MsgGenericError || 'Thao tác thất bại'), type: 'error' });
        } finally {
            hideLoading();
        }
    }

    async function rejectShipSelected() {
        const items = await collectSelected();
        if (!items.length) return showDialog({ message: 'Chưa chọn bản ghi nào' });
        const lyDo = await showReasonDialog(T.ReasonTitle || 'Nhập lý do', T.ReasonMessage || 'Vui lòng nhập lý do từ chối');
        if (lyDo === null) return;
        for (const i of items) i.lyDo = lyDo;

        try {
            showLoading(T.Processing || 'Đang xử lý...');
            const res = await fetch((window.apiBaseUrl || '') + '/Material/RejectShipSelectedConfirmName?role=' + encodeURIComponent(role), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(items)
            });
            if (!res.ok) throw new Error(await res.text());
            state.selectedIds.clear();
            await search(true);
        } catch (e) {
            showDialog({ title: T.Error || 'Lỗi', message: e.message || 'Từ chối thất bại', type: 'error' });
        } finally {
            hideLoading();
        }
    }

    async function loadKpi() {
        const reqBase = {
            tenHang: getFieldValue(els.tenHang),
            soDon: getFieldValue(els.soDon),
            section: getFieldValue(els.vitri),
            pageIndex: 1,
            pageSize: 1
        };

        try {
            const res = await fetch(
                (window.apiBaseUrl || '') + '/Material/CountConfirmNameByRole',
                {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(reqBase)
                }
            );

            if (!res.ok) {
                throw new Error(`HTTP ${res.status}`);
            }

            const json = await res.json();

            if (els.kpiConfirming)
                els.kpiConfirming.textContent = json.countConfirming ?? 0;

            if (els.kpiConfirmed)
                els.kpiConfirmed.textContent = json.countCofirmed ?? 0;

            if (els.kpiRejected)
                els.kpiRejected.textContent = json.countRejected ?? 0;

            if (els.kpiTotal)
                els.kpiTotal.textContent = json.sum ?? 0;

        } catch (e) {
            console.warn('KPI load failed', e);

            if (els.kpiConfirming) els.kpiConfirming.textContent = 0;
            if (els.kpiConfirmed) els.kpiConfirmed.textContent = 0;
            if (els.kpiRejected) els.kpiRejected.textContent = 0;
            if (els.kpiTotal) els.kpiTotal.textContent = 0;
        }
    }


    async function saveInline(id, payload) {
        try {
            showLoading(T.Processing || 'Đang xử lý...');
            const body = Object.assign({ id, role }, payload);
            const res = await fetch((window.apiBaseUrl || '') + '/Material/SaveConfirmName', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!res.ok) throw new Error(T.MsgSaveFailed || 'Lưu thất bại');
            showDialog({ title: T.Success || 'Thành công', message: T.BtnSave || 'Đã lưu', type: 'success' });
            await search(false);
        } catch (e) {
            showDialog({ title: T.Error || 'Lỗi', message: e.message || (T.MsgSaveFailed || 'Lưu thất bại'), type: 'error' });
        } finally {
            hideLoading();
        }
    }

    async function collectSelected() {
        const checks = Array.from(els.tbody.querySelectorAll('.row-select:checked'));
        return checks.map(c => {
            const id = parseInt(c.getAttribute('data-id'));
            const ten = els.tbody.querySelector(`.js-tenhq[data-id="${id}"]`);
            const ma = els.tbody.querySelector(`.js-manb[data-id="${id}"]`);
            return {
                id,
                tenHaiQuan: ten ? ten.value : undefined,
                maHangNoiBo: ma ? ma.value : undefined,
                lyDo: ''
            };
        });
    }

    async function saveSelected() {
        const items = await collectSelected();
        if (!items.length) return showDialog({ message: 'Chưa chọn bản ghi nào' });
        try {
            showLoading(T.Processing || 'Đang xử lý...');
            const res = await fetch((window.apiBaseUrl || '') + '/Material/SaveSelectedConfirmName?role=' + encodeURIComponent(role), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(items)
            });
            if (!res.ok) throw new Error(await res.text());
            state.selectedIds.clear();
            await search(false);
        } catch (e) {
            showDialog({ title: T.Error || 'Lỗi', message: e.message || 'Lưu thất bại', type: 'error' });
        } finally {
            hideLoading();
        }
    }

    function openDrawer(item, focusHistory) {
        if (!item) return;

        setText('dMaDon', item.NVCHR_Note || '-');
        setText('dMaVatTu', item.VCHR_MaHangNoiBo || '-');
        setText('dTenHaiQuan', item.VCHR_TenHaiQuan || '-');
        setText('dTenEN', item.CHR_NameEN || '-');
        setText('dTenDeXuat', item.VCHR_TenRecomment || '-');
        const statusEl = document.getElementById('dTrangThai');
        if (statusEl) statusEl.innerHTML = statusBadge(getDisplayStatus(item));

        setText('dCreateBy', item.VCHR_CreateBy || '-');
        setText('dCreateDate', formatDate(item.DTM_CreateDate) || '-');
        setText('dUpdateBy', item.VCHR_UpdateBy || '-');
        setText('dUpdateDate', formatDate(item.DTM_UpdateDate) || '-');
        setText('dUserPur', item.VCHR_UserPUR || '-');
        setText('dUserPurDate', formatDate(item.DTM_UserPUR) || '-');
        setText('dUserShip', item.VCHR_UserShip || '-');
        setText('dUserShipDate', formatDate(item.DTM_UserShip) || '-');
        setText('dNote', item.NVCHR_Note || '-');
        setText('dRejectReason', item.NVCHR_LyDo || '-');

        if (els.drawerHistoryTimeline) {
            els.drawerHistoryTimeline.innerHTML = '<div class="text-muted small">Đang tải lịch sử...</div>';
        }
        loadHistory(item.ID, focusHistory);

        els.drawer?.classList.add('show');
        els.drawer?.setAttribute('aria-hidden', 'false');
        els.drawerOverlay?.classList.add('show');
        els.drawerOverlay?.setAttribute('aria-hidden', 'false');
    }

    function closeDrawer() {
        els.drawer?.classList.remove('show');
        els.drawer?.setAttribute('aria-hidden', 'true');
        els.drawerOverlay?.classList.remove('show');
        els.drawerOverlay?.setAttribute('aria-hidden', 'true');
    }

    async function loadHistory(confirmId) {
        try {
            const res = await fetch((window.apiBaseUrl || '') + '/Material/GetConfirmNameHistory?confirmId=' + encodeURIComponent(confirmId));
            if (!res.ok) throw new Error('Lỗi tải lịch sử');
            const items = await res.json();

            if (!Array.isArray(items) || !items.length) {
                els.drawerHistoryTimeline.innerHTML = '<div class="text-muted small">Chưa có lịch sử thay đổi</div>';
                return;
            }

            els.drawerHistoryTimeline.innerHTML = items.map(h => {
                const type = (h.actionType || '').toLowerCase();
                const icon = type === 'confirm' ? 'fa-check-circle text-success' : type === 'reject' ? 'fa-times-circle text-danger' : 'fa-pen text-primary';
                return `
                    <div class="history-item">
                        <div class="history-icon"><i class="fas ${icon}"></i></div>
                        <div class="history-content">
                            <div class="history-meta">${formatDate(h.actionDate)} · <b>${escapeHtml(h.actionBy || '-')}</b></div>
                            <div class="history-value">Old: ${escapeHtml(h.oldValue || '-')}</div>
                            <div class="history-value">New: ${escapeHtml(h.newValue || '-')}</div>
                        </div>
                    </div>`;
            }).join('');
        } catch (e) {
            els.drawerHistoryTimeline.innerHTML = `<div class="text-danger small">${escapeHtml(e.message || 'Lỗi tải lịch sử')}</div>`;
        }
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function escapeHtml(s) {
        return (s || '').toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function showLoading(message) {
        const el = document.getElementById('globalLoading');
        if (!el) return;
        const msgEl = el.querySelector('.loader-msg');
        if (msgEl) msgEl.textContent = message || 'Đang xử lý...';
        el.style.display = 'flex';
        el.setAttribute('aria-hidden', 'false');
    }

    function hideLoading() {
        const el = document.getElementById('globalLoading');
        if (!el) return;
        el.style.display = 'none';
        el.setAttribute('aria-hidden', 'true');
    }

    function showConfirmDialog(title, message) {
        return new Promise((resolve) => {
            const el = document.getElementById('cmConfirmDialog');
            if (!el) return resolve(window.confirm(message || title || 'Confirm?'));
            el.querySelector('.cm-confirm-title').textContent = title || 'Xác nhận';
            el.querySelector('.cm-confirm-body').textContent = message || '';
            const btnCancel = el.querySelector('[data-cm-action="cancel"]');
            const btnOk = el.querySelector('[data-cm-action="ok"]');
            const close = (result) => {
                el.classList.remove('show');
                el.style.display = 'none';
                btnCancel?.removeEventListener('click', onCancel);
                btnOk?.removeEventListener('click', onOk);
                resolve(result);
            };
            const onCancel = () => close(false);
            const onOk = () => close(true);
            btnCancel?.addEventListener('click', onCancel);
            btnOk?.addEventListener('click', onOk);
            el.style.display = 'block';
            el.classList.add('show');
        });
    }

    function showReasonDialog(title, message) {
        return new Promise((resolve) => {
            const el = document.getElementById('cmReasonDialog');
            if (!el) return resolve(prompt(message || title || 'Nhập lý do') || '');
            el.querySelector('.cm-reason-title').textContent = title || 'Nhập lý do';
            el.querySelector('.cm-reason-body').textContent = message || '';
            const input = el.querySelector('#cmReasonInput');
            const btnCancel = el.querySelector('[data-cm-action="cancel"]');
            const btnOk = el.querySelector('[data-cm-action="ok"]');
            input.value = '';

            const close = (value) => {
                el.classList.remove('show');
                el.style.display = 'none';
                btnCancel?.removeEventListener('click', onCancel);
                btnOk?.removeEventListener('click', onOk);
                resolve(value);
            };
            const onCancel = () => close(null);
            const onOk = () => close((input.value || '').trim());

            btnCancel?.addEventListener('click', onCancel);
            btnOk?.addEventListener('click', onOk);
            el.style.display = 'block';
            el.classList.add('show');
            input.focus();
        });
    }

    function showDialog({ title, message, type } = {}) {
        const overlay = document.getElementById('cmDialogOverlay');
        const titleEl = document.getElementById('cmDialogTitle');
        const bodyEl = document.getElementById('cmDialogBody');
        const footerEl = document.getElementById('cmDialogFooter');
        if (!overlay || !titleEl || !bodyEl || !footerEl) {
            alert(message || title || 'Thông báo');
            return;
        }

        titleEl.textContent = title || 'Thông báo';
        bodyEl.innerHTML = `<div class="d-flex align-items-start gap-2"><i class="fas ${type === 'success' ? 'fa-check-circle text-success' : type === 'error' ? 'fa-exclamation-circle text-danger' : 'fa-info-circle text-primary'}"></i><div>${escapeHtml(message || '')}</div></div>`;
        footerEl.innerHTML = '';
        const btn = document.createElement('button');
        btn.className = 'cm-btn cm-btn-primary';
        btn.textContent = T.OK || 'OK';
        btn.onclick = () => {
            overlay.style.display = 'none';
            overlay.setAttribute('aria-hidden', 'true');
        };
        footerEl.appendChild(btn);

        overlay.style.display = 'flex';
        overlay.setAttribute('aria-hidden', 'false');
        const closeBtn = overlay.querySelector('[data-cm-action="close"]');
        const bg = overlay.querySelector('[data-cm-action="overlay"]');
        if (closeBtn) closeBtn.onclick = btn.onclick;
        if (bg) bg.onclick = btn.onclick;
    }

    async function exportTemplate() {
        const url = (window.apiBaseUrl || '') + '/template/TemplateCofirmName.xlsx';
        const a = document.createElement('a');
        a.href = url;
        a.download = 'TemplateCofirmName.xlsx';
        document.body.appendChild(a);
        a.click();
        a.remove();
    }

    async function exportTable() {
        try {
            showLoading(T.Processing || 'Đang xuất...');
            const body = {
                tenHang: getFieldValue(els.tenHang),
                soDon: getFieldValue(els.soDon),
                trangThai: getStatusForSearch(),
                section: getFieldValue(els.vitri),
                pageIndex: state.pageIndex,
                pageSize: state.pageSize
            };
            const res = await fetch((window.apiBaseUrl || '') + '/Material/ExportToExcel', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!res.ok) throw new Error(T.ExportError || 'Xuất file thất bại');
            const blob = await res.blob();
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'TableConfirmName.xlsx';
            document.body.appendChild(a);
            a.click();
            a.remove();
            URL.revokeObjectURL(url);
        } catch (e) {
            showDialog({ title: T.Error || 'Lỗi', message: e.message || (T.ExportError || 'Xuất file thất bại'), type: 'error' });
        } finally {
            hideLoading();
        }
    }

    async function importExcel(e) {
        const file = e.target.files?.[0];
        if (!file) return;
        try {
            showLoading(T.LoadingMessage || 'Đang xử lý...');
            const fd = new FormData();
            fd.append('file', file);
            const res = await fetch((window.apiBaseUrl || '') + '/Material/ImportFromExcel', { method: 'POST', body: fd });
            if (!res.ok) throw new Error(await res.text());
            showDialog({ title: T.Success || 'Thành công', message: T.ImportSuccess || 'Nhập thành công', type: 'success' });
            await search(true);
        } catch (e2) {
            showDialog({ title: T.Error || 'Lỗi', message: e2.message || 'Import lỗi', type: 'error' });
        } finally {
            hideLoading();
            e.target.value = '';
        }
    }

    function resetFilters() {
        if (els.tenHang) els.tenHang.value = '';
        if (els.soDon) els.soDon.value = '';
        if (els.vitri) els.vitri.value = '';
        if (els.createdByFilter) els.createdByFilter.value = '';
        if (els.fromDateFilter) els.fromDateFilter.value = '';
        if (els.toDateFilter) els.toDateFilter.value = '';
        if (els.quickSearch) els.quickSearch.value = '';
        if (els.trangThai) els.trangThai.value = '';

        state.pageIndex = 1;
        state.selectedIds.clear();
        search(true);
    }

    function bindEvents() {
        els.btnSearch?.addEventListener('click', () => {
            state.pageIndex = 1;
            search(true);
        });

        els.btnReset?.addEventListener('click', resetFilters);
        els.prev?.addEventListener('click', () => {
            if (state.pageIndex <= 1) return;
            state.pageIndex--;
            if (state.serverPaging) search(false);
            else if (state.activeTab === 'pending') renderPendingTable(); else renderConfirmedCards();
        });
        els.next?.addEventListener('click', () => {
            const totalPages = Math.max(1, Math.ceil(state.total / state.pageSize));
            if (state.pageIndex >= totalPages) return;
            state.pageIndex++;
            if (state.serverPaging) search(false);
            else if (state.activeTab === 'pending') renderPendingTable(); else renderConfirmedCards();
        });

        els.pageSizeSelect?.addEventListener('change', () => {
            state.pageSize = parseInt(els.pageSizeSelect.value) || 20;
            state.pageIndex = 1;
            if (state.serverPaging) search(false);
            else if (state.activeTab === 'pending') renderPendingTable(); else renderConfirmedCards();
        });

        els.tabPending?.addEventListener('click', () => {
            state.activeTab = 'pending';
            state.pageIndex = 1;
            updateTabUi();
            if (els.trangThai) els.trangThai.value = 'Confirming';
            search(true);
        });

        els.tabConfirmed?.addEventListener('click', () => {
            state.activeTab = 'confirmed';
            state.pageIndex = 1;
            updateTabUi();
            if (els.trangThai) els.trangThai.value = 'Confirmed';
            search(true);
        });

        document.querySelectorAll('[data-kpi-status]').forEach(btn => {
            btn.addEventListener('click', () => {
                const status = btn.getAttribute('data-kpi-status');
                if (!els.trangThai) return;
                els.trangThai.value = status;
                if (status === 'Confirmed') {
                    state.activeTab = 'confirmed';
                } else {
                    state.activeTab = 'pending';
                }
                state.pageIndex = 1;
                updateTabUi();
                search(true);
            });
        });

        els.chkSelectAll?.addEventListener('change', () => {
            const checked = !!els.chkSelectAll.checked;
            els.tbody.querySelectorAll('.row-select').forEach(c => {
                c.checked = checked;
                const id = parseInt(c.getAttribute('data-id'));
                if (checked) state.selectedIds.add(id); else state.selectedIds.delete(id);
            });
            if (els.headerCheckAll) els.headerCheckAll.checked = checked;
            updateSelectedCount();
        });

        els.headerCheckAll?.addEventListener('change', () => {
            const checked = !!els.headerCheckAll.checked;
            els.tbody.querySelectorAll('.row-select').forEach(c => {
                c.checked = checked;
                const id = parseInt(c.getAttribute('data-id'));
                if (checked) state.selectedIds.add(id); else state.selectedIds.delete(id);
            });
            if (els.chkSelectAll) els.chkSelectAll.checked = checked;
            updateSelectedCount();
        });

        els.btnSaveSelected?.addEventListener('click', saveSelected);
        els.btnRejectShipSelected?.addEventListener('click', rejectShipSelected);

        els.btnExportTemplate?.addEventListener('click', exportTemplate);
        els.btnExportTable?.addEventListener('click', exportTable);
        els.btnImportExcel?.addEventListener('click', () => els.itemsExcelFileInput?.click());
        els.itemsExcelFileInput?.addEventListener('change', importExcel);

        els.btnCloseDrawer?.addEventListener('click', closeDrawer);
        els.drawerOverlay?.addEventListener('click', closeDrawer);

        if (window.KanziSearchableDropdown && typeof window.KanziSearchableDropdown.init === 'function') {
            try { window.KanziSearchableDropdown.init(root); } catch { }
        }
    }

    if (els.pageSizeSelect && parseInt(els.pageSizeSelect.value)) {
        state.pageSize = parseInt(els.pageSizeSelect.value) || state.pageSize;
    }

    if (els.trangThai && !els.trangThai.value) {
        els.trangThai.value = 'Confirming';
    }

    bindEvents();
    updateTabUi();
    search(true);
})();
