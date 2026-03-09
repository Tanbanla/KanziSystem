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
        pageInfo: document.getElementById('pageInfo'),
        btnImportExcel: document.getElementById('btnImportExcel'),
        btnExportTemplate: document.getElementById('btnExportTemplate'),
        btnExportTable: document.getElementById('btnExportTable'),
        itemsExcelFileInput: document.getElementById('itemsExcelFileInput')
    };

    let state = { pageIndex: 1, pageSize: 20, total: 0, listData: [] };

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
            { key: 'select', label: T.Select || 'Chọn', editable: false },
            { key: 'ID_RequestQuote', label: T.RequestQuoteNumber || 'ID Yêu cầu báo giá', editable: false },
            { key: 'CHR_SectionName', label: T.Department || 'Phòng ban', editable: false },
            { key: 'CHR_Phanloai', label: T.EquipmentClassification || 'Phân loại thiết bị', editable: false },
            { key: 'CHR_MaThietBi', label: T.EquipmentCode || 'Mã thiết bị', editable: false },
            { key: 'CHR_MaHangNCC', label: T.SupplierItemCode || 'Mã hàng của NCC', editable: false },
            { key: 'VCHR_TenRecomment', label: T.VietnameseItemNameDraft || 'Tên hàng VN dùng để mở thủ tục hải quan (dự thảo)(*)', editable: false },
            { key: 'CHR_NameEN', label: T.EnglishItemName || 'Tên hàng tiếng anh(*)', editable: false },
            { key: 'INT_SoLuong', label: T.Quantity || 'Số lượng(*)', editable: false },
            { key: 'NVCHR_DonVi', label: T.Unit || 'Đơn vị(*)', editable: false },
            { key: 'NVCHR_ChungLoai', label: T.ItemCategory || 'Chủng loại hàng', editable: false },
            { key: 'NVCHR_HinhDang', label: T.Shape || 'Hình dáng', editable: false },
            { key: 'NVCHR_ChatLieu', label: T.Material || 'Chất liệu', editable: false },
            { key: 'NVCHR_ThanhPhan', label: T.Composition || 'Thành phần, hàm lượng (đối với hóa chất)', editable: false },
            { key: 'NVCHR_KichThuoc', label: T.Dimensions || 'Kích thước(mm) (dài/rộng/cao)', editable: false },
            { key: 'NVCHR_DongMay', label: T.UsedForMachine || 'Dùng cho máy/thiết bị/vị trí nào', editable: false },
            { key: 'NVCHR_TinhNang', label: T.Feature || 'Dùng để làm gì (tính năng)', editable: false },
            { key: 'tenHQ', label: T.ConfirmName || 'Xác nhận tên', editable: canEditTenHQ() },
            { key: 'maNB', label: T.ConfirmCode || 'Xác nhận mã', editable: canEditMaNB() },
            { key: 'status', label: T.Status || 'Trạng thái', editable: false },
            { key: 'DTM_CreateDate', label: T.CreatedDate || 'Ngày tạo', editable: false },
            { key: 'handler', label: T.Handler || 'Người xử lý', editable: false },
            { key: 'note', label: T.Note || 'Ghi chú', editable: false }
        ];

        // Đánh dấu 7 hàng cuối (giữ màu xanh nhưng không cố định)
        fields.slice(-7).forEach(f => f.isSpecial = true);

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
        //const selectAllCheckbox = document.createElement('input');
        //selectAllCheckbox.type = 'checkbox';
        //selectAllCheckbox.className = 'js-select-all';
        //const selectTh = document.createElement('th');
        //selectTh.style.minWidth = '50px';
        //selectTh.style.textAlign = 'center';
        //selectTh.style.backgroundColor = '#2335B7';
        //selectTh.style.color = '#FFFF';
        //selectTh.style.position = 'sticky';
        //selectTh.style.left = '0';
        //selectTh.style.zIndex = '21';
        //selectTh.style.boxShadow = '2px 0 4px rgba(0,0,0,0.1)';
        // selectTh.appendChild(selectAllCheckbox);
        //headerRow.appendChild(selectTh);
        headerRow.innerHTML += `<th style="min-width: 250px; background-color: #2335B7; color: #FFFF; position: sticky; z-index: 21; box-shadow: 2px 0 4px rgba(0,0,0,0.1);">${T.Datafield}</th>` +
            data.map((r, i) => `<th style="min-width: 200px; text-align: center; background-color: #e0e0e0;">${T.Record} ${(state.pageIndex - 1) * state.pageSize + i + 1}</th>`).join('');
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
            // Không thêm class fixed-row nữa

            const th = document.createElement('th');
            th.innerHTML = field.label;
            th.style.fontWeight = 'bold';
            th.style.backgroundColor = '#2335B7';
            th.style.color = '#FFFF';
            th.style.position = 'sticky';
            th.style.left = '0';
            th.style.zIndex = '15';
            th.style.minWidth = '250px';
            th.style.maxWidth = '250px';
            th.style.boxShadow = '2px 0 4px rgba(0,0,0,0.1)';
            th.style.padding = '8px 12px';
            // Giữ màu xanh cho 7 hàng cuối
            if (field.isSpecial) {
                th.style.backgroundColor = '#28a745';
                th.style.color = '#ffffff';
            }
            tr.appendChild(th);

            data.forEach(r => {
                const td = document.createElement('td');
                td.style.padding = '8px 12px';
                td.style.verticalAlign = 'middle';

                if (field.key === 'select') {
                    td.innerHTML = `<input type="checkbox" class="js-select-row" data-id="${r.ID}" />`;
                    td.style.textAlign = 'center';
                } else if (field.key === 'status') {
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
                } else if (field.key === 'DTM_CreateDate') {
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

        // Gán sự kiện cho select all
        //selectAllCheckbox.addEventListener('change', () => {
        //    const checkboxes = els.tbody.querySelectorAll('.js-select-row');
        //    checkboxes.forEach(cb => cb.checked = selectAllCheckbox.checked);
        //});

        // Bỏ event listener cho input change và nút actions, vì bây giờ lưu hàng loạt
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
        const T = window.i18nConfirmName || {};
        els.resultCount.textContent = `${T.Total || 'Tổng'}: ${state.total}`;
        const totalPages = Math.max(1, Math.ceil(state.total / state.pageSize));
        els.pageInfo.textContent = `${state.pageIndex}/${totalPages}`;
        var a = data.data.data || [];
        state.listData = a;
        renderRows(a);
    }

    async function saveInline(id, payload) {
        const body = Object.assign({ id, role }, payload);
        const res = await fetch('/Material/SaveConfirmName', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        if (!res.ok) { const T = window.i18nConfirmName || {}; showDialog({ title: T.Error || 'Lỗi', message: err.message || 'Không thể Save', type: 'error' }); }
    }
    async function saveSeclections(listSelect) {
        const res = await fetch('/Material/SaveSelectedConfirmName', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(listSelect) });
        if (!res.ok) { const T = window.i18nConfirmName || {}; showDialog({ title: T.Error || 'Lỗi', message: res.message || 'Không thể Save', type: 'error' }); }
    }
    async function approve(id, skipConfirm = false) {
        const T = window.i18nConfirmName || {};
        if (!skipConfirm) {
            const ok = await showConfirmDialog(T.ConfirmApproveTitle || 'Xác nhận đồng ý?', T.ConfirmApproveMessage || 'Bạn có chắc chắn muốn phê duyệt yêu cầu này?');
            if (!ok) return;
        }
        const res = await fetch('/Material/ApproveConfirmName', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id }) });
        if (res.ok) { search(); } else { showDialog({ title: T.Error || 'Lỗi', message: T.MsgGenericError || 'Thao tác thất bại', type: 'error' }); }
    }

    async function reject(id, lyDo) {
        const T = window.i18nConfirmName || {};
        if (!lyDo) {
            lyDo = await showReasonDialog(T.ReasonTitle || 'Nhập lý do từ chối', T.ReasonMessage || 'Vui lòng nhập lý do từ chối xử lý yêu cầu này:');
            if (lyDo === null) return;
        }
        const res = await fetch('/Material/RejectConfirmName', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id, lyDo }) });
        if (res.ok) { search(); } else { showDialog({ title: T.Error || 'Lỗi', message: T.MsgGenericError || 'Thao tác thất bại', type: 'error' }); }
    }

    els.btnSearch.addEventListener('click', () => { state.pageIndex = 1; search(); });

    // các button excel
    els.btnExportTemplate.addEventListener('click', () => { exportTemplate(); });
    els.btnExportTable.addEventListener('click', () => { exportTable(); });
    els.btnImportExcel.addEventListener('click', () => itemsExcelFileInput?.click());
    els.itemsExcelFileInput.addEventListener('change', async (e) => { importExcel(e); });

    // cac ham excel
    async function exportTemplate() {
        try {
            const url = '/template/TemplateCofirmName.xlsx';
            const a = document.createElement('a');
            a.href = url;
            a.download = 'TemplateCofirmName.xlsx';
            document.body.appendChild(a);
            a.click();
            a.remove();
        } catch (err) {
            console.error('Error downloading template', err);
        }
    }
    async function importExcel(e) {
        const file = e.target.files?.[0];
        if (!file) return;
        const T = window.i18nConfirmName || {};
        try {
            showLoading(T.LoadingMessage || 'Đang xử lý...');
            const fd = new FormData();
            fd.append('file', file);
            const res = await fetch('/Material/ImportFromExcel', { method: 'POST', body: fd });
            if (!res.ok) throw new Error(await res.text());
            const items = await res.json();
            if (!Array.isArray(items)) throw new Error('Dữ liệu không hợp lệ');
            showDialog({ title: T.Success || 'Thành công', message: T.ImportSuccess || 'Nhập bằng file thành công', type: 'success' });
            search();
        } catch (err) {
            showDialog({ title: T.Error || 'Lỗi', message: err.message || 'Không thể đọc file', type: 'error' });
        } finally {
            hideLoading();
            e.target.value = '';
        }
    }
    async function exportTable() {
        const T = window.i18nConfirmName || {};
        try {
            showLoading(T.Processing || 'Đang xuất...');

            const res = await fetch('/Material/ExportToExcel', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(state.listData)
            });
            if (!res.ok) {
                const msg = await res.text().catch(() => 'Lỗi không xác định');
                throw new Error(msg || (T.ExportError || 'Xuất file thất bại'));
            }
            const blob = await res.blob();
            let fileName = 'TableConfirmName.xlsx';
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
            console.error('Error export table', err);
        } finally {
            hideLoading();
        }
    }

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

    // Bulk save button
    const btnBulkSave = document.getElementById('btnBulkSave');
    if (btnBulkSave) {
        btnBulkSave.addEventListener('click', async () => {
            const selectedRows = Array.from(els.tbody.querySelectorAll('.js-select-row:checked'));
            if (selectedRows.length === 0) {
                const T = window.i18nConfirmName || {};
                showDialog({ title: T.Warning || 'Cảnh báo', message: T.NoSelection || 'Vui lòng chọn ít nhất một hàng', type: 'info' });
                return;
            }
            const T = window.i18nConfirmName || {};
            showLoading(T.Processing || 'Đang xử lý...');
            try {
                const dataToSave = [];
                for (const cb of selectedRows) {
                    const id = parseInt(cb.getAttribute('data-id'));
                    const TenHaiQuan = els.tbody.querySelector(`.js-tenhq[data-id="${id}"]`);
                    const MaHangNoiBo = els.tbody.querySelector(`.js-manb[data-id="${id}"]`);
                    var item = { id };
                    if (TenHaiQuan) item.tenHaiQuan = TenHaiQuan.value;
                    if (MaHangNoiBo) item.maHangNoiBo = MaHangNoiBo.value;
                    dataToSave.push(item);
                }
                if (Object.keys(dataToSave).length > 0) {
                    await saveSeclections(dataToSave);
                }
                search(); // Refresh data
            } catch (err) {
                console.error('Bulk save failed', err);
                showDialog({ title: T.Error || 'Lỗi', message: T.MsgSaveFailed || 'Lưu thất bại', type: 'error' });
            } finally {
                hideLoading();
            }
        });
    }

    // Bulk approve button
    const btnBulkApprove = document.getElementById('btnBulkApprove');
    if (btnBulkApprove) {
        btnBulkApprove.addEventListener('click', async () => {
            if (!canApprove()) return;
            const selectedRows = Array.from(els.tbody.querySelectorAll('.js-select-row:checked'));
            if (selectedRows.length === 0) {
                const T = window.i18nConfirmName || {};
                showDialog({ title: T.Warning || 'Cảnh báo', message: T.NoSelection || 'Vui lòng chọn ít nhất một hàng', type: 'info' });
                return;
            }
            const T = window.i18nConfirmName || {};
            const action = await showActionDialog(T.BulkApproveTitle || 'Chọn hành động phê duyệt', T.BulkApproveMessage || 'Chọn Đồng ý hoặc Từ chối cho các hàng đã chọn:');
            if (!action) return;
            showLoading(T.Processing || 'Đang xử lý...');
            try {
                for (const cb of selectedRows) {
                    const id = parseInt(cb.getAttribute('data-id'));
                    if (action === 'approve') {
                        await approve(id, true);
                    } else if (action === 'reject') {
                        const lyDo = await showReasonDialog(T.ReasonTitle || 'Nhập lý do từ chối', T.ReasonMessage || 'Vui lòng nhập lý do từ chối xử lý yêu cầu này:');
                        if (lyDo === null) continue;
                        await reject(id, lyDo);
                    }
                }
                search(); // Refresh data
            } catch (err) {
                console.error('Bulk approve failed', err);
                showDialog({ title: T.Error || 'Lỗi', message: T.MsgGenericError || 'Thao tác thất bại', type: 'error' });
            } finally {
                hideLoading();
            }
        });
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
    // show message dialog
    function getDialogEls() {
        const overlay = document.getElementById('cmDialogOverlay');
        const titleEl = document.getElementById('cmDialogTitle');
        const bodyEl = document.getElementById('cmDialogBody');
        const footerEl = document.getElementById('cmDialogFooter');
        return { overlay, titleEl, bodyEl, footerEl };
    }
    function showDialog({ title = (window.i18nConfirmName && window.i18nConfirmName.Notification) || 'Thông báo', message = '', type = 'info', buttons } = {}) {
        const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
        if (!overlay) return alert(message);

        // Ensure overlay is attached to body so fixed positioning is not clipped by parent containers
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
        const T = window.i18nConfirmName || {};
        okBtn.textContent = (buttons && buttons.okText) || (T.OK || 'Đồng ý');
        okBtn.addEventListener('click', () => hideDialog());
        footerEl.appendChild(okBtn);

        overlay.setAttribute('aria-hidden', 'false');
        overlay.style.display = 'flex';
        attachDialogCloseHandlers();
    }
    function hideDialog() {
        const { overlay } = getDialogEls();
        if (overlay) {
            overlay.style.display = 'none';
            overlay.setAttribute('aria-hidden', 'true');
        }
    }

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

function showActionDialog(title, message) {
    return new Promise((resolve) => {
        const el = document.getElementById('cmActionDialog');
        if (!el) { resolve(null); return; }
        const T = window.i18nConfirmName || {};
        el.querySelector('.cm-action-title').textContent = title || (T.SelectAction || 'Chọn hành động');
        el.querySelector('.cm-action-body').textContent = message || '';
        const btnApprove = el.querySelector('[data-cm-action="approve"]');
        const btnReject = el.querySelector('[data-cm-action="reject"]');
        const btnCancel = el.querySelector('[data-cm-action="cancel"]');
        const close = () => { el.setAttribute('aria-hidden', 'true'); el.classList.remove('show'); el.style.display = 'none'; document.body.classList.remove('modal-open'); cleanup(); };
        const open = () => { el.style.display = 'block'; el.style.zIndex = '3000'; el.setAttribute('aria-hidden', 'false'); el.classList.add('show'); document.body.classList.add('modal-open'); };
        const onApprove = () => { close(); resolve('approve'); };
        const onReject = () => { close(); resolve('reject'); };
        const onCancel = () => { close(); resolve(null); };
        const cleanup = () => {
            btnApprove && btnApprove.removeEventListener('click', onApprove);
            btnReject && btnReject.removeEventListener('click', onReject);
            btnCancel && btnCancel.removeEventListener('click', onCancel);
        };
        btnApprove && btnApprove.addEventListener('click', onApprove);
        btnReject && btnReject.addEventListener('click', onReject);
        btnCancel && btnCancel.addEventListener('click', onCancel);
        open();
    });
}
