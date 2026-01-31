(function () {
    const tblBody = document.getElementById('historyGroupTableBody');
    const statusFilter = document.getElementById('statusFilter');
    const btnApply = document.getElementById('btnApplyFilters');
    const btnReset = document.getElementById('btnResetFilters');
    const paginationEl = document.getElementById('historyPagination');
    const paginationInfoEl = document.getElementById('historyPaginationInfo');

    let currentPage = 1;
    const pageSize = 20;
    let currentGroups = [];

    function applyFilters() {
        const maDon = (document.getElementById('searchMaDon').value || '').trim();
        const phongBan = (document.getElementById('searchPhongBan').value || '').trim();
        const nguoiTao = (document.getElementById('searchNguoiTao').value || '').trim();
        const maVatTu = (document.getElementById('searchMaVatTu').value || '').trim();
        const nhaCungCap = (document.getElementById('searchNhaCungCap').value || '').trim();
        const status = statusFilter.value;
        const from = document.getElementById('dateFrom').value;
        const to = document.getElementById('dateTo').value;
        // build payload for SearchBaoGia
        const payload = {
            MaDon: maDon,
            MaNcc: nhaCungCap,
            Section: phongBan,
            NguoiYeuCau: nguoiTao,
            MaHang: maVatTu,
            TrangThai: status,
            Step: null,
            PageIndex: 1,
            PageSize: 10000,
            Date: (from && to) ? { From: from, To: to } : null
        };
        fetch('/Quote/SearchBaoGia', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(r => {
                if (!r.ok) throw new Error('Search failed');
                return r.json();
            })
            .then(data => {
                // data is list of BaoGia_Request_of_QuotationDTO
                currentGroups = groupByMaDon(Array.isArray(data) ? data : (data?.Data || []));
                currentPage = 1;
                renderGroups();
            })
            .catch(err => {
                console.error(err);
                renderEmpty();
            });
    }

    btnApply?.addEventListener('click', applyFilters);
    btnReset?.addEventListener('click', () => {
        document.getElementById('searchMaDon').value = '';
        document.getElementById('searchPhongBan').value = '';
        document.getElementById('searchNguoiTao').value = '';
        document.getElementById('searchMaVatTu').value = '';
        document.getElementById('searchNhaCungCap').value = '';
        statusFilter.value = '';
        document.getElementById('dateFrom').value = '';
        document.getElementById('dateTo').value = '';
        applyFilters();
    });

    tblBody?.addEventListener('click', (e) => {
        const t = e.target.closest('button');
        if (!t) return;
        // handle item-level history buttons inside detail rows (delegated)
        if (t.dataset.action === 'view-history') {
            const id = Number(t.dataset.id);
            if (!id) return;
            fetch('/Quote/GetHistoryDataByID', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(id)
            })
                .then(r => { if (!r.ok) throw new Error('Load history failed'); return r.json(); })
                .then(data => { showDialog('Lịch sử đơn', buildHistoryHtml(data)); })
                .catch(err => { console.error(err); showDialog('Thông báo', '<div class="text-danger">Không tải được lịch sử.</div>'); });
            return;
        }
        const row = e.target.closest('tr');
        // eidt item-level
        if (t.dataset.action === 'edit-history') {
            const id = Number(t.dataset.id);
            if (id) openEditModal(id);
        }
        if (t.classList.contains('btn-toggle-group')) {
            const groupId = row?.dataset.groupId;
            const detailRow = tblBody.querySelector(`tr.group-detail[data-group-id="${groupId}"]`);
            if (!detailRow) return;
            const icon = t.querySelector('i');
            const hidden = detailRow.hasAttribute('hidden');
            if (hidden) {
                detailRow.removeAttribute('hidden');
                icon?.classList.remove('fa-plus-square');
                icon?.classList.add('fa-minus-square');
                // render child orders for the group
                const group = currentGroups.find(g => g.groupId === groupId);
                if (group) renderChildOrders(detailRow, group.items);
            } else {
                detailRow.setAttribute('hidden', '');
                icon?.classList.remove('fa-minus-square');
                icon?.classList.add('fa-plus-square');
            }
        }
        //if (t.classList.contains('btn-edit-uncompleted')) {
        //    const groupId = row?.dataset.groupId;
        //    const group = currentGroups.find(g => g.groupId === groupId);
        //    const firstId = group?.items?.[0]?.id;
        //    if (groupId) openEditModal(groupId);
        //    return;
        //}
        if (t.classList.contains('btn-view-history')) {
            const groupId = row?.dataset.groupId;
            const group = currentGroups.find(g => g.groupId === groupId);
            const soDon = group?.code || groupId;
            if (!soDon) return;
            // View history for the whole group (by SoDon)
            fetch('/Quote/GetHistoryDataBySoDon', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(soDon)
            })
                .then(r => {
                    if (!r.ok) throw new Error('Load history failed');
                    return r.json();
                })
                .then(data => {
                    showDialog('Lịch sử đơn', buildHistoryHtml(data));
                })
                .catch(err => {
                    console.error(err);
                    showDialog('Thông báo', '<div class="text-danger">Không tải được lịch sử.</div>');
                });
        }
        if (t.classList.contains('btn-view-approvals')) {
            const groupId = row?.dataset.groupId;
            document.dispatchEvent(new CustomEvent('quote-history:viewApprovals', { detail: { groupId } }));
        }
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
                    const empty = document.createElement('div'); empty.className = 'ms-empty'; empty.textContent = 'Không có kết quả';
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
                    placeholderEl.textContent = '-- Chọn --';
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
                try { select.dispatchEvent(new Event('change', { bubbles: true })); } catch (ex) {}
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
            try { select.addEventListener('change', updateButtonText); } catch {}
        });
    }

    // Initialize for existing selects. If jQuery present, pass $(document) for compatibility.
    if (window.jQuery) buildSearchableDropdown($(document)); else buildSearchableDropdown(document);
    // Also run on DOMContentLoaded to ensure selects added later are enhanced
    document.addEventListener('DOMContentLoaded', function () { buildSearchableDropdown(document); });

    // Helper: group items by CHR_MaDon
    function groupByMaDon(items) {
        function getVal(obj, ...keys) {
            for (const k of keys) {
                if (!obj) continue;
                if (Object.prototype.hasOwnProperty.call(obj, k)) return obj[k];
                const lower = k.charAt(0).toLowerCase() + k.slice(1);
                if (Object.prototype.hasOwnProperty.call(obj, lower)) return obj[lower];
                const upper = k.charAt(0).toUpperCase() + k.slice(1);
                if (Object.prototype.hasOwnProperty.call(obj, upper)) return obj[upper];
            }
            return undefined;
        }
        const map = new Map();
        (items || []).forEach(it => {
            const rawKey = getVal(it,'chR_MaDon') || '';
            const key = String(rawKey).trim();
            if (!key) return;
            if (!map.has(key)) map.set(key, []);
            map.get(key).push(it);
        });
        const groups = Array.from(map.entries()).map(([key, arr]) => {
            const first = arr[0] || {};
            const suppliers = Array.from(new Set(arr.map(x => {
                const ma = getVal(x,'chR_MaNCC') || '';
                return (ma || '');
            }))).filter(Boolean).join(', ');
            return {
                groupId: key,
                code: key,
                requester: getVal(first,'chR_CreateBy') || '',
                created: toDateString(getVal(first,'dtM_CreateDate')),
                status: getVal(first,'iD_Status') || '',
                count: arr.length,
                suppliers,
                items: arr
            };
        });
        return groups;
    }

    function toDateString(dt) {
        try {
            if (!dt) return '';
            const d = new Date(dt);
            if (isNaN(d.getTime())) return '';
            return d.toLocaleDateString();
        } catch { return ''; }
    }

    function renderGroups() {
        if (!tblBody) return;
        tblBody.innerHTML = '';
        const total = currentGroups.length;
        const totalPages = Math.max(1, Math.ceil(total / pageSize));
        if (currentPage > totalPages) currentPage = totalPages;
        const start = (currentPage - 1) * pageSize;
        const pageGroups = currentGroups.slice(start, start + pageSize);

        pageGroups.forEach((g, idx) => {
            // Try cloning existing template row; if not present, build from scratch
            const templateRow = document.querySelector('#historyGroupTableBody tr.group-row');
            let tmpl;
            if (templateRow) {
                tmpl = templateRow.cloneNode(true);
                tmpl.removeAttribute('hidden');
            } else {
                tmpl = document.createElement('tr');
                tmpl.className = 'group-row';
                tmpl.innerHTML = `
                    <td class="text-center"><button type="button" class="btn btn-sm btn-link text-primary px-0 btn-toggle-group" title="Mở rộng"><i class="fas fa-plus-square"></i></button></td>
                    <td class="fw-semibold group-code"></td>
                    <td class="group-requester"></td>
                    <td class="group-created"></td>
                    <td class="group-status"></td>
                    <td class="group-count text-end"></td>
                    <td class="group-suppliers"></td>
                    <td class="text-center">
                        <div class="btn-group btn-group-sm" role="group">
                            <button type="button" class="btn btn-outline-info btn-view-history" title="Xem lịch sử"><i class="fas fa-history"></i></button>
                            <button type="button" class="btn btn-outline-secondary btn-view-approvals" title="Phê duyệt"><i class="fas fa-check-double"></i></button>
                        </div>
                    </td>
                `;
            }
           // <button type="button" class="btn btn-outline-primary btn-edit-uncompleted" title="S?a don tr? l?i"><i class="fas fa-edit"></i></button>
            tmpl.dataset.groupId = g.groupId;
            const cells = tmpl.querySelectorAll('td');
            const codeEl = tmpl.querySelector('.group-code'); if (codeEl) codeEl.textContent = g.code;
            const reqEl = tmpl.querySelector('.group-requester'); if (reqEl) reqEl.textContent = g.requester;
            const createdEl = tmpl.querySelector('.group-created'); if (createdEl) createdEl.textContent = g.created;
            const statusEl = tmpl.querySelector('.group-status'); if (statusEl) statusEl.textContent = StatusText(g.status);
            const countEl = tmpl.querySelector('.group-count'); if (countEl) countEl.textContent = g.count;
            const suppEl = tmpl.querySelector('.group-suppliers'); if (suppEl) suppEl.textContent = g.suppliers;
            // index
            const idxCell = cells[0];
            if (idxCell) idxCell.setAttribute('data-idx', String(start + idx + 1));
            tblBody.appendChild(tmpl);

            // detail row
            const templateDetail = document.querySelector('#historyGroupTableBody tr.group-detail');
            let detailTmpl;
            if (templateDetail) {
                detailTmpl = templateDetail.cloneNode(true);
                detailTmpl.removeAttribute('hidden');
            } else {
                detailTmpl = document.createElement('tr');
                detailTmpl.className = 'group-detail';
                detailTmpl.innerHTML = `
                    <td colspan="8" class="p-0">
                        <div class="p-2">
                            <div class="table-responsive">
                                <table class="table table-sm table-bordered mb-0">
                                    <thead class="table-light">
                                        <tr class="text-center align-middle">
                                            <th style="width: 40px">No</th>
                                            <th style="min-width: 160px">Mã đơn</th>
                                            <th style="min-width: 180px">Phòng ban</th>
                                            <th style="min-width: 200px">Vật tư</th>
                                            <th style="min-width: 160px">NCC</th>
                                            <th style="min-width: 140px">Số lượng</th>
                                            <th style="min-width: 140px">Trạng thái</th>
                                            <th style="min-width: 160px">Ngày cập nhật</th>
                                            <th style="min-width: 200px">Thao tác</th>
                                        </tr>
                                    </thead>
                                    <tbody class="group-detail-body"></tbody>
                                </table>
                            </div>
                        </div>
                    </td>
                `;
            }
            detailTmpl.dataset.groupId = g.groupId;
            detailTmpl.setAttribute('hidden', '');
            tblBody.appendChild(detailTmpl);
        });

        // update pagination UI
        if (paginationInfoEl) {
            const showingFrom = total === 0 ? 0 : start + 1;
            const showingTo = Math.min(total, start + pageSize);
            paginationInfoEl.textContent = `Hiển thị ${showingFrom} - ${showingTo} / ${total} nhóm`;
        }
        renderPagination(totalPages);
    }

    function renderEmpty() {
        if (!tblBody) return;
        tblBody.innerHTML = '';
        renderPagination(1);
        if (paginationInfoEl) paginationInfoEl.textContent = 'Hiển thị 0 - 0 / 0 nhóm';
    }

    function renderPagination(totalPages) {
        if (!paginationEl) return;
        const ul = paginationEl;
        ul.innerHTML = '';
        const prev = document.createElement('li'); prev.className = 'page-item' + (currentPage === 1 ? ' disabled' : '');
        const prevBtn = document.createElement('button'); prevBtn.className = 'page-link'; prevBtn.textContent = '«'; prevBtn.dataset.page = 'prev';
        prev.appendChild(prevBtn); ul.appendChild(prev);
        for (let i = 1; i <= totalPages; i++) {
            const li = document.createElement('li'); li.className = 'page-item' + (i === currentPage ? ' active' : '');
            const btn = document.createElement('button'); btn.className = 'page-link'; btn.textContent = String(i); btn.dataset.page = String(i);
            li.appendChild(btn); ul.appendChild(li);
        }
        const next = document.createElement('li'); next.className = 'page-item' + (currentPage === totalPages ? ' disabled' : '');
        const nextBtn = document.createElement('button'); nextBtn.className = 'page-link'; nextBtn.textContent = '»'; nextBtn.dataset.page = 'next';
        next.appendChild(nextBtn); ul.appendChild(next);
    }

    paginationEl?.addEventListener('click', function (e) {
        const btn = e.target.closest('button');
        if (!btn) return;
        const val = btn.dataset.page;
        const totalPages = Math.max(1, Math.ceil(currentGroups.length / pageSize));
        if (val === 'prev' && currentPage > 1) currentPage--;
        else if (val === 'next' && currentPage < totalPages) currentPage++;
        else if (!isNaN(Number(val))) currentPage = Number(val);
        renderGroups();
    });

    function renderChildOrders(detailRow, items) {
        const body = detailRow.querySelector('.group-detail-body');
        if (!body) return;
        body.innerHTML = '';
        items.forEach((it, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="text-center">${idx + 1}</td>
                <td>${it.chR_MaDon || ''}</td>
                 <td>${it.chR_SectionName || ''}</td>
                <td>${(it.chR_MaHangNoiBo || '')} ${(it.nvchR_NameVN ? ('- ' + it.nvchR_NameVN) : '')}</td>
                <td>${(it.chR_MaNCC || '')}</td>
                <td class="text-end">${it.inT_SoLuong ?? ''}</td>
                <td>${StatusText(it.iD_Status) || ''}</td>
                <td>${toDateString(it.dtM_CreateDate) || ''}</td>
                <td class="text-center">
                    <div class="btn-group btn-group-sm" role="group">
                        <button type="button" class="btn btn-outline-info" data-action="view-history" data-id="${it.id}"><i class="fas fa-history"></i></button>
                        <button type="button" class="btn btn-outline-primary" data-action="edit-history" data-id="${it.id}"><i class="fas fa-edit"></i></button>
                    </div>
                </td>
            `;
            body.appendChild(tr);
        });
    }

    // Open edit modal using latest history CHR_NewData
    function openEditModal(requestId) {
        fetch('/Quote/SearchID', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(requestId))
        })
        .then(r => { if (!r.ok) throw new Error('Load history failed'); return r.json(); })
        .then(result => {
            const data = result?.data || result || [];
            if (!data){
                showDialog('Thông báo', '<div class="text-danger">Không có dữ liệu để chỉnh sửa.</div>');
                return;
            }
            fillEditFormFromDto(data);
            showEditModal();
        })
        .catch(err => {
            console.error(err);
            showDialog('Thông báo', '<div class="text-danger">Không tìm thấy dữ liệu.</div>');
        });
    }

    function fillEditFormFromDto(dto) {
        const setVal = (id, val) => { const el = document.getElementById(id); if (el) el.value = val ?? ''; };
        const setSelect = (id, val) => { const el = document.getElementById(id); if (el) el.value = val ?? ''; };
        document.getElementById('editRequestId')?.setAttribute('value', dto.ID ?? dto.id ?? '');
        document.getElementById('editMaDon') && (document.getElementById('editMaDon').value =  dto.chR_MaDon || '');
        document.getElementById('editRequester') && (document.getElementById('editRequester').value =  dto.chR_CreateBy || '');
        setVal('editSectionName', dto.chR_SectionName || '');
        setVal('editSectionCode', dto.chR_SectionCode || '');

        setSelect('editChungLoai',  dto.nvchR_ChungLoai || '');
        try { document.getElementById('editChungLoai')?.dispatchEvent(new Event('change', { bubbles: true })); } catch {}
        setSelect('editPhanLoai', dto.chR_Phanloai || '');
        try { document.getElementById('editPhanLoai')?.dispatchEvent(new Event('change', { bubbles: true })); } catch {}
        setVal('editMaThietBi',  dto.chR_MaThietBi || '');
        setSelect('editMaHangNoiBo',  dto.chR_MaHangNoiBo || '');
        try { document.getElementById('editMaHangNoiBo')?.dispatchEvent(new Event('change', { bubbles: true })); } catch {}
        setVal('editMaHangNCC',  dto.chR_MaHangNCC || '');
        setVal('editTenHangVN',  dto.nvchR_NameVN || '');
        setVal('editTenHangEN',  dto.chR_NameEN || '');
        setVal('editSoLuong',  dto.inT_SoLuong ?? '');
        setVal('editDonVi',  dto.nvchR_DonVi || '');
        setVal('editHinhDang',  dto.nvchR_HinhDang || '');
        setVal('editChatLieu',  dto.nvchR_ChatLieu || '');
        setVal('editThanhPhan',  dto.nvchR_ThanhPhan || '');
        setVal('editKichThuoc',  dto.nvchR_KichThuoc || '');
        setVal('editDongMay',  dto.nvchR_DongMay || '');
        setVal('editTinhNang',  dto.nvchR_TinhNang || '');
        setSelect('editRohs',  dto.nvchR_Rohs || '');
        setSelect('editCOCQ',  dto.nvchR_COCQ || '');
        setVal('editMSDS',  dto.nvchR_MSDS || '');
        setVal('editAnToan',dto.nvchR_AnToan || '');
        setVal('editFileThietKe', dto.nvchR_FileThietKe || '');
        setVal('editNhaSanXuat',  dto.nvchR_NhaSanXuat || '');
        setSelect('editNhaCungCap', dto.chR_MaNCC || '');
        setVal('editTenNCC', dto.nvchR_TenNCC || '');
        setVal('editStatus', dto.iD_Status || '');
        setVal('editStep', dto.iD_StepBaoGia || '');
        setVal('editSoLanUpdate', dto.inT_SoLanUpdate ?? '');

        try { document.getElementById('editNhaCungCap')?.dispatchEvent(new Event('change', { bubbles: true })); } catch {}
        setSelect('editLayBaoGia', (dto.biT_LayBaoGia) ? 'true' : 'false');
        try { document.getElementById('editLayBaoGia')?.dispatchEvent(new Event('change', { bubbles: true })); } catch {}
        setVal('editLyDo', dto.nvchR_LyDo || '');
        const toDateInput = (d) => { try { if (!d) return ''; const dt = new Date(d); return dt.toISOString().slice(0,10);} catch { return ''; } };
        setVal('editNgayMuonNhan', toDateInput(dto.dtM_NgayMuonNhan));
        setVal('editKyHan', toDateInput(dto.dtM_KyHan));
        setSelect('editGap', (dto.chR_Gap) ?? 'false');
        setVal('editDaycreate', toDateInput(dto.dtM_CreateDate) || '');
        setVal('editUpdateLater', toDateInput(dto.dtM_UpdateLater) || '');
        setVal('editDeadline', toDateInput(dto.dtM_Deadline) || '');
        setSelect('editIsTemplate', (dto.biT_IsTemplate === true) ? 'true' : (dto.biT_IsTemplate === false) ? 'false' : '');
        setVal('editSectionName', dto.chR_SectionName || '');
        try { document.getElementById('editGap')?.dispatchEvent(new Event('change', { bubbles: true })); } catch {}
        // enhance selects if needed
        try { if (window.jQuery) buildSearchableDropdown($(document)); else buildSearchableDropdown(document); } catch {}
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
        // Accessibility: if focus is inside modal, blur and move focus before hiding (to avoid aria-hidden ancestor with focused descendant)
        try {
            const active = document.activeElement;
            if (active && modalEl.contains(active)) {
                if (typeof active.blur === 'function') active.blur();
                const fallbackFocus = document.getElementById('btnApplyFilters') || document.body;
                if (fallbackFocus && typeof fallbackFocus.focus === 'function') fallbackFocus.focus();
            }
        } catch {}
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
        } catch {}
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
    // Save handler: submit a single DTO update
    document.getElementById('btnSaveHistoryEdit')?.addEventListener('click', function () {
        const dto = collectEditFormDto();
        if (!dto) return;
        // UpdateBaoGiaById expects a list
        fetch('/Quote/UpdateBaoGiaById', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        })
        .then(async r => {
            const txt = await r.text();
            if (!r.ok) throw new Error(txt || 'Lưu thất bại');
            hideEditModal();
            showDialog('Thành công', '<div class="text-success">Đã lưu thành công.</div>');
            applyFilters();
        })
        .catch(err => {
            console.error(err);
            showDialog('Thông báo', `<div class="text-danger">${err.message}</div>`);
        });
    });

    function collectEditFormDto() {
        const gv = id => document.getElementById(id)?.value || '';
        const toIso = d => { if (!d) return null; try { const parts = d.split('-'); return new Date(Date.UTC(+parts[0], +parts[1]-1, +parts[2], 7, 0, 0)).toISOString(); } catch { return null; } };
        return {
            ID: Number(document.getElementById('editRequestId')?.getAttribute('value') || 0),
            CHR_MaDon: gv('editMaDon') || null,
            CHR_SectionCode: gv('editSectionCode') || null,
            CHR_SectionName: gv('editSectionName') || null,
            CHR_Phanloai: gv('editPhanLoai') || null,
            CHR_MaThietBi: gv('editMaThietBi') || null,
            CHR_MaHangNoiBo: gv('editMaHangNoiBo') || null,
            CHR_MaHangNCC: gv('editMaHangNCC') || null,
            NVCHR_NameVN: gv('editTenHangVN') || null,
            CHR_NameEN: gv('editTenHangEN') || null,
            INT_SoLuong: gv('editSoLuong') ? parseFloat(gv('editSoLuong')) : null,
            NVCHR_DonVi: gv('editDonVi') || null,
            NVCHR_ChungLoai: gv('editChungLoai') || null,
            NVCHR_HinhDang: gv('editHinhDang') || null,
            NVCHR_ChatLieu: gv('editChatLieu') || null,
            NVCHR_ThanhPhan: gv('editThanhPhan') || null,
            NVCHR_KichThuoc: gv('editKichThuoc') || null,
            NVCHR_DongMay: gv('editDongMay') || null,
            NVCHR_TinhNang: gv('editTinhNang') || null,
            NVCHR_Rohs: gv('editRohs') || null,
            NVCHR_COCQ: gv('editCOCQ') || null,
            NVCHR_MSDS: gv('editMSDS') || null,
            NVCHR_AnToan: gv('editAnToan') || null,
            NVCHR_FileThietKe: gv('editFileThietKe') || null,
            NVCHR_NhaSanXuat: gv('editNhaSanXuat') || null,
            CHR_MaNCC: gv('editNhaCungCap') || null,
            NVCHR_TenNCC: gv('editTenNCC') || null,
            BIT_LayBaoGia: (gv('editLayBaoGia') === 'true'),
            NVCHR_LyDo: gv('editLyDo') || null,
            DTM_NgayMuonNhan: toIso(gv('editNgayMuonNhan')),
            DTM_KyHan: toIso(gv('editKyHan')),
            CHR_Gap: gv('editGap') || null,
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

    function showDialog(title, html) {
        const overlay = document.getElementById('cmDialogOverlay');
        const body = document.getElementById('cmDialogBody');
        const footer = document.getElementById('cmDialogFooter');
        const titleEl = document.getElementById('cmDialogTitle');
        if (!overlay || !body || !footer || !titleEl) return;
        titleEl.textContent = title || 'Thông báo';
        body.innerHTML = html || '';
        footer.innerHTML = '<button type="button" class="cm-btn" data-cm-action="close">Đóng</button>';
        // show overlay (CSS default is display:none)
        overlay.style.display = 'flex';
        overlay.setAttribute('aria-hidden', 'false');

        // Focus first focusable in dialog for accessibility
        try {
            const dlg = overlay.querySelector('.cm-dialog');
            const focusable = dlg && dlg.querySelector('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
            if (focusable && typeof focusable.focus === 'function') focusable.focus();
        } catch {}

        const doClose = () => {
            // If focus is within overlay, blur and move focus outside before hiding to avoid aria-hidden ancestor warnings
            try {
                const active = document.activeElement;
                if (active && overlay.contains(active)) {
                    if (typeof active.blur === 'function') active.blur();
                    const fallbackFocus = document.getElementById('btnApplyFilters') || document.body;
                    if (fallbackFocus && typeof fallbackFocus.focus === 'function') fallbackFocus.focus();
                }
            } catch {}
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
    // lấy thông tin status hiển thị
    function StatusText(statusId) {
        var listStatus = window.HistoryData.status
        const status = listStatus.find(s => s.VCHR_CodeStatus === statusId);
        return status ? status.NVCHR_TenStatus : statusId;
    }
    function buildHistoryHtml(result) {
        const data = result?.data || result || [];
        if (!Array.isArray(data) || data.length === 0) return '<div>Không có lịch sử.</div>';
        const rows = data.map((h, i) => {
            const dateStr = toDateString(h.chR_Updatedate);
            const action = StatusText(h.chR_ActionType) || '';
            const by = (h.chR_UpdateBy || '') + (h.nvchR_UpdateName ? (' - ' + h.nvchR_UpdateName) : '');
            const reason = h.nvchR_LyDo || '';
            const ID_RequestQuote = h.iD_RequestQuote;
            return `<tr>
                <td class="text-center">${i + 1}</td>
                <td>${dateStr}</td>
                <td>${ID_RequestQuote || ''}</td>
                <td>${action}</td>
                <td>${by}</td>
                <td>${reason}</td>
            </tr>`;
        }).join('');
        return `
            <div class="table-responsive">
                <table class="table table-sm table-bordered">
                    <thead class="table-light"><tr>
                        <th style="width:60px">#</th>
                        <th>Thời gian</th>
                        <th>Số Request</th>
                        <th>Hành động</th>
                        <th>Người cập nhật</th>
                        <th>Lý do</th>
                    </tr></thead>
                    <tbody>${rows}</tbody>
                </table>
            </div>
        `;
    }

    // Initial load
    document.addEventListener('DOMContentLoaded', function () {
        applyFilters();
    });
})();