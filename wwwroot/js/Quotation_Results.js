document.addEventListener('DOMContentLoaded', function () {
    // Khai báo biến toàn cục cho file
    const quotationApp = {
        init: function () {
            this.bindEvents();
            this.closeEdit();
            // Khởi tạo dropdown có tìm kiếm cho các select có class 'searchable-select'
            buildSearchableDropdown(document);
            console.log('Quotation Results initialized');
        },
        // đóng modal 
        closeEdit: function () {
            document.getElementById('btnCloseEdit_1')?.addEventListener('click', function () {
                hideEditModal();
            });
            document.getElementById('btnCloseEdit_2')?.addEventListener('click', function () {
                hideEditModal();
            });
        },
        bindEvents: function () {
            // Delegation: Toggle chi tiết nhà cung cấp và load dữ liệu khi mở
            document.addEventListener('click', (e) => {
                const btn = e.target.closest('.toggle-sup');
                if (btn) {
                    this.toggleSupplierDetails(btn);
                }

                // Open edit modal for request detail buttons inside supplier list
                const detailBtn = e.target.closest('button[data-action="detail-request"]');
                if (detailBtn) {
                    const id = detailBtn.getAttribute('data-id');
                    if (id) {
                        this.openEditRequestModal(parseInt(id, 10));
                    }
                }
            });

            // Filter theo trạng thái
            document.querySelectorAll('.status-option').forEach(option => {
                option.addEventListener('click', this.filterByStatus.bind(this));
            });

            // Tìm kiếm
            const btnSearch = document.getElementById('btnSearch');
            if (btnSearch) {
                btnSearch.addEventListener('click', this.searchItems.bind(this));
            }

            // Reset
            const btnReset = document.getElementById('btnReset');
            if (btnReset) {
                btnReset.addEventListener('click', this.resetFilters.bind(this));
            }

            // Xác nhận lựa chọn
            const btnConfirmTop = document.getElementById('btnConfirmTop');
            const btnConfirmBottom = document.getElementById('btnConfirmBottom');
            if (btnConfirmTop) btnConfirmTop.addEventListener('click', this.confirmSelection.bind(this));
            if (btnConfirmBottom) btnConfirmBottom.addEventListener('click', this.confirmSelection.bind(this));

            // Hủy
            const btnCancel = document.getElementById('btnCancel');
            if (btnCancel) {
                btnCancel.addEventListener('click', this.cancelSelection.bind(this));
            }

            // Xuất danh sách
            const btnExport = document.getElementById('btnExport');
            if (btnExport) {
                btnExport.addEventListener('click', this.exportList.bind(this));
            }

            // Chọn tất cả (nếu có)
            const selectAll = document.getElementById('selectAll');
            if (selectAll) {
                selectAll.addEventListener('change', this.toggleSelectAll.bind(this));
            }

            // Delegate: enforce only one supplier per maDon
            document.addEventListener('change', (e) => {
                const cb = e.target.closest('.supplier-select');
                if (!cb) return;
                if (!cb.checked) return;
                // Find current supplier group and derive maDon from groupId pattern: CHR_MaDon-CHR_MaHangNoiBo
                const row = cb.closest('tr');
                const groupContainer = row?.closest('.supplier-group');
                const groupId = groupContainer?.id?.replace('sup-rows-', '') || '';
                const maDon = groupId.split('-')[0] || '';
                if (!maDon) return;
                // Uncheck other supplier-select in all groups matching same maDon, except current checkbox
                document.querySelectorAll('.supplier-group[id^="sup-rows-' + maDon + '-"] .supplier-select').forEach(other => {
                    if (other !== cb) other.checked = false;
                });
            });
        },
        openEditRequestModal: async function (id) {
            try {
                const res = await fetch('/Quote/SearchID', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(id)
                });
                if (!res.ok) {
                    const err = await res.text();
                    console.error('Load request detail failed', err);
                    alert('Không tải được dữ liệu chi tiết yêu cầu.');
                    return;
                }
                const data = await res.json();
                // Fill modal fields safely
                const setVal = (id, val) => { const el = document.getElementById(id); if (el) el.value = val ?? ''; };
                setVal('editRequestId', data.id);
                setVal('editMaDon', data.chR_MaDon);
                setVal('editRequester', data.chR_CreateBy);
                setVal('editSectionName', data.chR_SectionName);
                setVal('editPhanLoai', data.chR_Phanloai);
                setVal('editChungLoai', data.nvchR_ChungLoai);
                setVal('editMaHangNoiBo', data.chR_MaHangNoiBo);
                setVal('editMaThietBi', data.chR_MaThietBi);
                setVal('editMaHangNCC', data.chR_MaHangNCC);
                setVal('editTenHangVN', data.nvchR_NameVN);
                setVal('editTenHangEN', data.chR_NameEN);
                setVal('editSoLuong', data.inT_SoLuong);
                setVal('editDonVi', data.nvchR_DonVi);
                setVal('editHinhDang', data.nvchR_HinhDang);
                setVal('editChatLieu', data.nvchR_ChatLieu);
                setVal('editThanhPhan', data.nvchR_ThanhPhan);
                setVal('editKichThuoc', data.nvchR_KichThuoc);
                setVal('editDongMay', data.nvchR_DongMay);
                setVal('editTinhNang', data.nvchR_TinhNang);
                setVal('editRohs', data.nvchR_Rohs);
                setVal('editCOCQ', data.nvchR_COCQ);
                setVal('editMSDS', data.nvchR_MSDS);
                setVal('editAnToan', data.nvchR_AnToan);
                setVal('editFileThietKe', data.nvchR_FileThietKe);
                setVal('editNhaSanXuat', data.nvchR_NhaSanXuat);
                setVal('editNhaCungCap', data.nvchR_TenNCC);
                setVal('editLyDo', data.nvchR_LyDo);
                // date fields (format to yyyy-MM-dd for input[type=date])
                const toDateInput = (d) => {
                    if (!d) return '';
                    const dt = new Date(d);
                    const pad = (n) => n.toString().padStart(2, '0');
                    return `${dt.getFullYear()}-${pad(dt.getMonth()+1)}-${pad(dt.getDate())}`;
                };
                setVal('editNgayMuonNhan', toDateInput(data.dtM_NgayMuonNhan));
                setVal('editKyHan', toDateInput(data.dtM_KyHan));
                setVal('editDaycreate', toDateInput(data.dtM_CreateDate));
                setVal('editUpdateLater', toDateInput(data.dtM_UpdateLater));
                setVal('editDeadline', toDateInput(data.dtM_Deadline));
                // hidden fields
                setVal('editSectionCode', data.chR_SectionCode);
                setVal('editIsTemplate', data.biT_IsTemplate);
                setVal('editStatus', data.iD_Status);
                setVal('editStep', data.iD_StepBaoGia);
                setVal('editSoLanUpdate', data.inT_SoLanUpdate);
                // selects
                const selLayBaoGia = document.getElementById('editLayBaoGia');
                if (selLayBaoGia) selLayBaoGia.value = (data.biT_LayBaoGia === true ? 'true' : 'false');
                const selGap = document.getElementById('editGap');
                if (selGap) selGap.value = (data.chR_Gap === 'true' || data.chR_Gap === true ? 'true' : 'false');

                // Modal open 
                showModal();
            } catch (err) {
                console.error('Error loading request detail', err);
                alert('Đã xảy ra lỗi khi tải dữ liệu.');
            }
        },

        toggleSupplierDetails: async function (button) {
            const targetSel = button.getAttribute('data-target');
            const targetRow = document.querySelector(targetSel);
            if (!targetRow) return;

            const willOpen = targetRow.classList.contains('d-none');
            targetRow.classList.toggle('d-none');
            button.setAttribute('aria-expanded', (!willOpen).toString());

            // Toggle icon if exists
            const icon = button.querySelector('i');
            if (icon) {
                icon.classList.toggle('fa-chevron-down');
                icon.classList.toggle('fa-chevron-up');
            }

            // Only load suppliers when opening and not loaded yet
            if (willOpen) {
                const madon = button.getAttribute('data-madon') || '';
                const mahang = button.getAttribute('data-mahang') || '';
                const bodyEl = document.querySelector(`#supplier-body-${madon}-${mahang}`);
                const ngay = button.getAttribute('data-ngay') || null;
                if (!bodyEl || bodyEl.dataset.loaded === 'true') return;
                try {
                    const payload = {
                        idRequestQuote: null,
                        maDon: madon,
                        maVatTu: mahang,
                        maNcc: null,
                        section: null,
                        dayMM: ToDateTimeLocal(ngay),
                        pageSize: 10,
                        pageIndex: 1
                    };
                    const res = await fetch('/Quote/SearchInputQuote', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });
                    if (!res.ok) {
                        const errText = await res.text();
                        console.error('Load supplier details failed', res, errText);
                        return;
                    }
                    const data = await res.json();
                    const rowsHtml = (data || []).map(d => {
                        const id = d.ID || 0;
                        const nameNCC = d.NVCHR_NameNCC || '';
                        const maHS = d.CHR_MaHangNCC || '';
                        const tenHQ = d.NVCHR_TenHangHQ || '';
                        const usd = d.FL_USD != null ? Number(d.FL_USD).toLocaleString() : '';
                        const vnd = d.FL_VND != null ? Number(d.FL_VND).toLocaleString() : '';
                        const moq = d.NVCHR_MOQ || '';
                        const lead = d.DTM_LeadTime || '';
                        const ngayGiao = d.DTM_ShipTime ? new Date(d.DTM_ShipTime).toLocaleDateString() : '';
                        const cocq = d.VCHR_COCQ || '';
                        const quyCach = d.NVCHR_Packing || '';
                        const rohs = d.VCHR_Rohs || '';
                        const msds = d.VCHR_MSDS || '';
                        const safe = d.VCHR_AnToan || '';
                        const camKet = d.VCHR_CamKet || '';
                        const phuongThuc = d.NVCHR_DeliveryTerm || '';
                        const dieuKien = d.NVCHR_PaymentTerm || '';
                        const file = d.NVCHR_File || '';
                        return `<tr class="small text-center">
                                     <td>
                                       <div class="btn-group btn-group-sm" role="group">
                                            <button type="button" class="btn btn-outline-primary" data-action="detail-request" data-id="${id}"><i class="fas fa-edit"></i> </button>
                                        </div>
                                    </td>
                                    <td class="text-start"><strong>${nameNCC}</strong></td>
                                    <td>${maHS}</td>
                                    <td class="text-start">${tenHQ}</td>
                                    <td class="text-end">${usd}</td>
                                    <td class="text-end">${vnd}</td>
                                    <td>${moq}</td>
                                    <td>${lead}</td>
                                    <td>${quyCach}</td>
                                    <td>${ngayGiao}</td>
                                    <td>${rohs}</td>
                                    <td>${cocq}</td>
                                    <td>${msds}</td>
                                    <td>${safe}</td>
                                    <td>${camKet}</td>
                                    <td>${phuongThuc}</td>
                                    <td>${dieuKien}</td>
                                    <td>${file}</td>
                                    <td><input class="form-check-input supplier-select" type="checkbox" value="${id}" data-id="${id}" /></td>
                                    <td><input type="text" class="form-control reason-input" /></td>    
                                </tr>`;
                    }).join('');
                    bodyEl.innerHTML = rowsHtml;
                    bodyEl.dataset.loaded = 'true';
                } catch (err) {
                    console.error('Load supplier details failed', err);
                }
            }
        },  

        filterByStatus: function (e) {
            const selectedOption = e.currentTarget;
            const status = selectedOption.getAttribute('data-value');

            // Update active class
            document.querySelectorAll('.status-option').forEach(opt => {
                opt.classList.remove('active');
            });
            selectedOption.classList.add('active');

            // Filter items
            const items = document.querySelectorAll('.item-row');
            items.forEach(item => {
                const itemStatus = item.getAttribute('data-status');
                item.style.display = (!status || itemStatus === status) ? '' : 'none';
            });
        },

        searchItems: async function () {
            const maDon = document.getElementById('filterRq')?.value || '';
            const maHang = document.getElementById('filterInternal')?.value || '';
            const section = document.getElementById('searchPhongBan')?.value || '';
            const activeStatus = document.querySelector('.status-option.active')?.getAttribute('data-value') || '';
            const payload = {
                maDon: maDon,
                maHang: maHang,
                section: section,
                pageIndex: 1,
                pageSize: 50
            };
            try {
                const res = await fetch('/Quote/GetThongTinBaoGiaGomNhom', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (!res.ok) {
                    console.error('Search failed');
                    return;
                }
                const data = await res.json();
                const tbody = document.getElementById('itemsBody');
                if (!tbody) return;
                const rowsHtml = (data || [])
                    .filter(r => !activeStatus || (r.Status === activeStatus))
                    .map(i => {
                        const ngay = i.DTM_NgayMuonNhan ? new Date(i.DTM_NgayMuonNhan).toLocaleDateString('vi-VN') : '';
                        const ngayAttr = i.DTM_NgayMuonNhan ? new Date(i.DTM_NgayMuonNhan).toISOString().slice(0, 10) : '';
                        const statusClass = (function (s) {
                            if (s === 'Chưa chọn NCC') return 'bg-success';
                            if (s === 'Đang chờ') return 'bg-warning text-dark';
                            return 'bg-secondary';
                        })(i.Status);
                        const groupId = `${i.CHR_MaDon}-${i.CHR_MaHangNoiBo}`;
                        return `
                        <tr class="item-row" data-status="${i.Status || ''}">
                            <td class="detail-cell text-center">
                                <button type="button" class="btn btn-sm btn-outline-primary toggle-sup" data-target="#sup-rows-${groupId}" data-madon="${i.CHR_MaDon || ''}" data-mahang="${i.CHR_MaHangNoiBo || ''}" data-ngay="${ngayAttr}" aria-expanded="false" title="Xem chi tiết">
                                    <span class="ms-1">Chi tiết</span>
                                </button>
                            </td>
                            <td class="text-start"><span class="ms-1 fw-semibold">${i.CHR_MaDon || ''}</span></td>
                            <td>${i.CHR_MaHangNoiBo || ''}</td>
                            <td class="text-start">${i.CHR_Phanloai || ''}</td>
                            <td class="text-end">${i.NVCHR_NameVN || ''}</td>
                            <td>${i.INT_SoLuong ?? ''}</td>
                            <td>${i.NVCHR_DonVi || ''}</td>
                            <td>${ngay}</td>
                            <td><span class="badge status-badge ${statusClass}">${i.Status || ''}</span></td>
                            <td><input type="checkbox" class="form-check-input item-select" /></td>
                        </tr>
                        <tr id="sup-rows-${groupId}" class="supplier-group d-none">
                            <td colspan="13" class="p-0">
                                <table class="table table-sm mb-0 supplier-table">
                                    <thead>
                                        <tr class="supplier-head text-center">
                                            <th>Nội dung</th>
                                            <th>Nhà cung cấp</th>
                                            <th>Mã HS</th>
                                            <th>Tên hàng (HQ)</th>
                                            <th>Đơn giá USD</th>
                                            <th>Đơn giá VND</th>
                                            <th>MOQ</th>
                                            <th>Lead time</th>
                                            <th>Quy cách đóng hàng</th>
                                            <th>Ngày giao</th>
                                            <th>Rohs</th>
                                            <th>CO/CQ</th>
                                            <th>MSDS kèm số CAS</th>
                                            <th>An toàn</th>
                                            <th>Cam kết đúng yêu cầu</th>
                                            <th>Phương thức giao</th>
                                            <th>Điều kiện</th>
                                            <th>File đính kèm</th>
                                            <th>Chọn</th>
                                            <th>Lý do</th>
                                        </tr>
                                    </thead>
                                    <tbody id="supplier-body-${groupId}"></tbody>
                                </table>
                            </td>
                        </tr>`;
                    }).join('');
                tbody.innerHTML = rowsHtml;
            } catch (err) {
                console.error('Error calling GetThongTinBaoGiaGomNhom', err);
            }
        },

        resetFilters: function () {
            // Reset input fields
            const rq = document.getElementById('filterRq');
            const internal = document.getElementById('filterInternal');
            if (rq) rq.value = '';
            if (internal) internal.value = '';

            // Reset status filter
            document.querySelectorAll('.status-option').forEach(opt => opt.classList.remove('active'));
            const allOpt = document.querySelector('.status-option[data-value=""]');
            if (allOpt) allOpt.classList.add('active');

            // Show all items
            document.querySelectorAll('.item-row').forEach(item => { item.style.display = ''; });
        },

        getSelections: function () {
            const result = [];
            document.querySelectorAll('.item-row').forEach(tr => {
                if (tr.style.display === 'none') return; // Chỉ xét các item đang hiển thị

                const btn = tr.querySelector('.toggle-sup');
                const maDon = btn?.getAttribute('data-madon') || '';
                const maHang = btn?.getAttribute('data-mahang') || '';
                const groupId = `${maDon}-${maHang}`;

                // Nếu chọn toàn bộ item, thêm một record tổng quát (không có ID NCC)
                const itemChecked = tr.querySelector('.item-select')?.checked === true;
                if (itemChecked) {
                    result.push({ ID: groupId, BIT_Select: true, NVCHR_ReasonPick: '' });
                }

                // Duyệt các NCC trong nhóm và lấy các lựa chọn + lý do
                const supplierGroup = document.getElementById(`sup-rows-${groupId}`);
                if (supplierGroup) {
                    supplierGroup.querySelectorAll('tbody tr').forEach(row => {
                        const cb = row.querySelector('.supplier-select');
                        if (!cb) return;
                        const isChecked = cb.checked === true;
                        const id = cb.getAttribute('data-id') || cb.value || '';
                        const reason = row.querySelector('.reason-input')?.value?.trim() || '';
                        //if (isChecked) {
                        result.push({ ID: id, BIT_Select: isChecked, NVCHR_ReasonPick: reason });
                        //}
                    });
                }
            });
            return result;
        },
        getSelectionsExcel: function () {
            const result = [];
            document.querySelectorAll('.item-row').forEach(tr => {
                if (tr.style.display === 'none') return; // Chỉ xét các item đang hiển thị

                const btn = tr.querySelector('.toggle-sup');
                const maDon = btn?.getAttribute('data-madon') || '';
                const maHang = btn?.getAttribute('data-mahang') || '';
                const groupId = `${maDon}-${maHang}`;

                // Nếu chọn toàn bộ item, thêm một record tổng quát (không có ID NCC)
                const itemChecked = tr.querySelector('.item-select')?.checked === true;
                if (itemChecked) {
                    result.push({ ID: "", MaDon: maDon });
                }

                // Duyệt các NCC trong nhóm và lấy các lựa chọn + lý do
                const supplierGroup = document.getElementById(`sup-rows-${groupId}`);
                if (supplierGroup) {
                    supplierGroup.querySelectorAll('tbody tr').forEach(row => {
                        const cb = row.querySelector('.supplier-select');
                        if (!cb) return;
                        const isChecked = cb.checked === true;
                        const id = cb.getAttribute('data-id') || cb.value || '';
                        if (isChecked) {
                            result.push({ ID: id, MaDon: "" });
                        }
                    });
                }
            });
            return result;
        },
        confirmSelection: async function () {
            const selections = this.getSelections();
            if (!selections.length) {
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgWarnSelectOne || 'Vui lòng chọn ít nhất một sản phẩm hoặc nhà cung cấp.'), type: 'info' });
                return;
            }

            // Warn if there are selections without reason (not mandatory)
            const missingReasons = selections.filter(x => (!x.NVCHR_ReasonPick || x.NVCHR_ReasonPick.trim() === '')).length;
            if (missingReasons > 0) {
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgMissingReasons || 'Có {0} lựa chọn chưa nhập lý do.').replace('{0}', missingReasons), type: 'info' });
                return;
            }
            try {
                const res = await fetch('/Quote/ChonNhaCungCapBaoGia', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(selections)
                });
                if (!res.ok) {
                    const T = window.i18nQuotationResults || {};
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgSaveError || 'lỗi {0}').replace('{0}', res.status), type: 'error' });
                    return;
                }
                const data = await res.json();
                const T = window.i18nQuotationResults || {};
                if (!data) return showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgSaveError || 'lỗi {0}').replace('{0}', ''), type: 'error' });
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgSaveSuccess || 'Gửi thành công'), type: 'success' });
                
            } catch (err) {
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgSaveError || 'lỗi {0}').replace('{0}', err), type: 'error' });
                return;
            }
            
        },

        cancelSelection: function () {
            const T = window.i18nQuotationResults || {};
            if (confirm(T.MsgCancelConfirm || 'Bạn có chắc muốn hủy bỏ tất cả lựa chọn?')) {
                document.querySelectorAll('.item-select, .supplier-select').forEach(c => { c.checked = false; });
            }
        },

        exportList: function () {
            const all = this.getSelectionsExcel();
            const selected = all;
            if (!selected.length) {
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgExportSelectOne || 'Vui lòng chọn ít nhất một nhà cung cấp hoặc sản phẩm để xuất.'), type: 'info' });
                return;
            }
            fetch('/Quote/ExportSelection', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(selected)
            })
                .then(async res => {
                    if (!res.ok) {
                        const txt = await res.text();
                        throw new Error(txt || 'Export failed');
                    }
                    return res.blob();
                })
                .then(blob => {
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `SelectionQuote_${new Date().toISOString().slice(0,19).replace(/[:T]/g, '-')}.xlsx`;
                    document.body.appendChild(a);
                    a.click();
                    a.remove();
                    window.URL.revokeObjectURL(url);
                })
                .catch(err => {
                    const T = window.i18nQuotationResults || {};
                    showDialog({ title: T.Notification || 'Thông báo', message: (err && err.message) ? err.message : (T.MsgExportError || 'Không thể xuất file'), type: 'error' });
                });
        },

        toggleSelectAll: function (e) {
            const isChecked = e.target.checked;
            document.querySelectorAll('.item-select').forEach(cb => { cb.checked = isChecked; });
        }
    };

    // Khởi tạo ứng dụng
    quotationApp.init();
});
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
    okBtn.textContent = (buttons && buttons.okText) || ((window.i18nQuotationResults && window.i18nQuotationResults.DialogOk) || 'Đồng ý');
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
function ToDateTimeLocal(date) {
    if (!date) return null;
    // Nếu đã là ISO (yyyy-MM-ddTHH:mm:ss) hoặc yyyy-MM-dd, giữ nguyên
    if (/^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2})?$/.test(date)) {
        if (date.length === 10) return date + 'T00:00:00';
        return date;
    }
    // Nếu là dd/MM/yyyy hh:mm:ss AM/PM
    const match = date.match(/(\d{2})\/(\d{2})\/(\d{4}) (\d{1,2}):(\d{2}):(\d{2}) (AM|PM)/);
    if (match) {
        let [_, d, m, y, h, min, s, ap] = match;
        h = parseInt(h, 10);
        if (ap === 'PM' && h < 12) h += 12;
        if (ap === 'AM' && h === 12) h = 0;
        const pad = n => n.toString().padStart(2, '0');
        return `${y}-${pad(m)}-${pad(d)}T${pad(h)}:${pad(min)}:${pad(s)}`;
    }
    // Nếu là dd/MM/yyyy
    const match2 = date.match(/(\d{2})\/(\d{2})\/(\d{4})/);
    if (match2) {
        let [_, d, m, y] = match2;
        const pad = n => n.toString().padStart(2, '0');
        return `${y}-${pad(m)}-${pad(d)}T00:00:00`;
    }
    return null;
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