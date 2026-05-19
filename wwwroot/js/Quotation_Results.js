document.addEventListener('DOMContentLoaded', function () {
    // Global UI state preserved across tab switches
    // store opened supplier groups (key: "MaDon-MaHang") and additional-columns visibility
    window._quotationResultsState = window._quotationResultsState || { openGroups: {}, showAdditionalColumns: true };

    // Pagination state for Request List tab
    const requestListState = {
        pageIndex: 1,
        pageSize: 10,
        returnedCount: 0,
        totalCount: 0,
        lastPage: false
    };

    // Pagination state for Supplier tab
    const supplierState = {
        pageIndex: 1,
        pageSize: 20,
        returnedCount: 0,
        totalCount: 0,
        lastPage: false
    };

    // Khai báo biến toàn cục cho file
    const quotationApp = {
        init: function () {
            this.bindEvents();
            this.closeEdit();
            // Khởi tạo dropdown có tìm kiếm cho các select có class 'searchable-select'
            buildSearchableDropdown(document);
            console.log('Quotation Results initialized');
            // Initialize tabs
            this.initTabs();
            // Load initial data for first tab
            this.loadRequestList();
            // Initialize toggle for supplier additional columns
            const toggleBtn = document.getElementById('toggleAdditionalColumns');
            if (toggleBtn) toggleBtn.addEventListener('click', this.toggleAdditionalColumns.bind(this));
            // Apply persisted UI state (in case some thing was toggled earlier)
            this.applyAdditionalColumnsVisibility();
            // Initialize pagination event listeners
            this.initPaginationEvents();
        },
        initTabs: function () {
            const tabs = document.querySelectorAll('#quotationResultsTabs .nav-link');
            tabs.forEach(tab => {
                tab.addEventListener('click', (e) => {
                    e.preventDefault();
                    const target = tab.getAttribute('data-bs-target');
                    this.switchTab(target);
                });
            });
        },
        switchTab: function (target) {
            // Hide all tab panes
            document.querySelectorAll('.tab-pane').forEach(pane => pane.classList.remove('show', 'active'));
            // Show target tab pane
            const targetPane = document.querySelector(target);
            if (targetPane) targetPane.classList.add('show', 'active');
            // Update tab links
            document.querySelectorAll('#quotationResultsTabs .nav-link').forEach(link => link.classList.remove('active'));
            document.querySelector(`[data-bs-target="${target}"]`).classList.add('active');
            // Load data based on tab
            if (target === '#request-list') {
                this.loadRequestList();
                // reapply open-groups and column visibility after data is (re)rendered
                // searchItems will call reapply after it finishes rendering
            } else if (target === '#supplier-input') {
                this.loadSupplierInput();
                // ensure columns visibility applied for supplier table
                this.applyAdditionalColumnsVisibility();
            }
        },
        initPaginationEvents: function () {
            // Request list page size change
            const pageSize = document.getElementById('pageSizeSelect');
            if (pageSize) {
                pageSize.addEventListener('change', () => {
                    requestListState.pageSize = parseInt(pageSize.value) || 10;
                    requestListState.pageIndex = 1;
                    this.searchItems();
                });
            }

            // Supplier page size change
            const supplierPageSize = document.getElementById('supplierPageSizeSelect');
            if (supplierPageSize) {
                supplierPageSize.value = '20'; // Set default to 50
                supplierPageSize.addEventListener('change', () => {
                    supplierState.pageSize = parseInt(supplierPageSize.value) || 50;
                    supplierState.pageIndex = 1;
                    this.loadSupplierData();
                });
            }
        },

        loadRequestList: function () {
            // Load data for request list tab
            // This would call the API to get the summarized request data
            // For now, populate with existing logic or placeholder
            this.searchItems(); // Reuse existing search logic
        },
        loadSupplierInput: function () {
            // Load data for supplier input tab
            // Similar to InputQuote supplier search
            this.loadSupplierData();
        },
        loadSupplierData: function () {
            const payload = {
                MaDon: document.getElementById('supplierSearchMaDon')?.value || '',
                MaNcc: document.getElementById('supplierSearchMaNcc')?.value || '',
                MaVatTu: document.getElementById('supplierSearchMaVatTu')?.value || '',
                Section: document.getElementById('supplierSearchSection')?.value || '',
                Status: document.getElementById('searchStatusTab2')?.value || '',
                PageIndex: supplierState.pageIndex,
                PageSize: supplierState.pageSize,
            };
            const T = window.i18nQuotationResults || {};
            //SearchInputQuote
            fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/SearchSupplierQuoteBody', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(res => res.json())
                .then(data => {
                    const items = Array.isArray(data.data.data) ? data.data.data : [];
                    const total = typeof data.data.totalCount === 'number' ? data.data.totalCount : items.length;
                    supplierState.returnedCount = items.length;
                    supplierState.totalCount = total;
                    supplierState.lastPage = (supplierState.pageIndex * supplierState.pageSize) >= total;

                    this.renderSupplierTable(items);

                    // Update summary
                    const summaryText = document.getElementById('supplierSummaryText');
                    if (summaryText) summaryText.textContent = `${T.Sum || 'Tổng số'}: ${total || 0}`;

                    // Render pagination
                    this.renderSupplierPaginationControls();

                    // after render, reapply additional columns visibility
                    this.applyAdditionalColumnsVisibility();
                })
                .catch(err => console.error('Load supplier data failed', err));
        },
        renderSupplierTable: function (data) {
            const tbody = document.getElementById('supplierQuoteBody');
            if (!tbody) return;

            const table = document.getElementById('supplierQuoteTable');
            if (table) {
                table.style.fontSize = '10px';
                table.style.lineHeight = '1.2';
            }

            const getMismatchStyle = (isMatch) => isMatch === false ? 'color: red; background-color: #ffcccc;' : '';

            const rowsHtml = data.map((d, index) => {

                const vnd = (d.FL_VND != null && !isNaN(Number(d.FL_VND))) ? Number(d.FL_VND) : 0;
                const usd = (d.FL_USD != null && !isNaN(Number(d.FL_USD))) ? Number(d.FL_USD) : 0;
                const sl = (d.soluong != null && !isNaN(Number(d.soluong))) ? Number(d.soluong) : 0;
                let totalCell = '';
                if (vnd && vnd !== 0) {
                    try { totalCell = Number(vnd * sl).toLocaleString(); } catch { totalCell = vnd * sl; }
                    totalCell = totalCell + ' VND';
                } else if (usd && usd !== 0) {
                    try { totalCell = Number(usd *sl).toLocaleString(); } catch { totalCell = usd * sl; }
                    totalCell = totalCell + ' USD';
                }

                return `
                <tr class="text-center" data-madon="${d.CHR_MaDon || ''}" data-mahang="${d.CHR_MaHangNoiBo || ''}" data-id="${d.ID || ''}" style="text-align: center;">
                    <td style="padding: 2px 4px; text-align: center;">${index + 1}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.CHR_MaDon || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${this.MappingStatusSupplier(d.status || '')}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.CHR_MaThietBi || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.CHR_MaHangNoiBo || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.CHR_MaHangNCC || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.CHR_Phanloai || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_ChungLoai || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_NameVN || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.CHR_NameEN || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.INT_SoLuong || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_DonVi || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_HinhDang || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_ChatLieu || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_ThanhPhan || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_KichThuoc || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_DongMay || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_TinhNang || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_Rohs || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_COCQ || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_MSDS || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_AnToan || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_FileThietKe || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.CHR_MaNCC || ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.DTM_KyHan ? new Date(d.DTM_KyHan).toLocaleDateString() : ''}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.CHR_Gap === 'true' || d.CHR_Gap === true ? 'O' : 'X'}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.BIT_LayBaoGia === true ? 'O' : 'X'}</td>
                    <td class="additional-column" style="padding: 2px 4px; text-align: center;">${d.NVCHR_LyDo || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.CHR_MaNCC || ''}</td>
                    <td class="text-start" style="padding: 2px 4px; text-align: left;">${d.NVCHR_NameNCC || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;  ${getMismatchStyle(d.IsMatch_MaHangNCC)}">${d.CodeEquipmentNCC || ''}</td>
                    <td class="text-start" style="padding: 2px 4px; text-align: left; ${getMismatchStyle(d.IsMatch_NameVN)}">${d.NVCHR_TenHangHQ || ''}</td>
                    <td class="text-start" style="padding: 2px 4px; text-align: left;">${d.NameENByNCC || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_SoLuong)}">${d.soluong || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_DonVi)}">${d.donvi || ''}</td>
                    <td class="text-end" style="padding: 2px 4px; text-align: right;">${d.FL_USD != null ? Number(d.FL_USD).toLocaleString() : ''}</td>
                    <td class="text-end" style="padding: 2px 4px; text-align: right;">${d.FL_VND != null ? Number(d.FL_VND).toLocaleString() : ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.NVCHR_MOQ || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.DTM_LeadTime || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_Ngay)}">${d.DTM_ShipTime ? new Date(d.DTM_ShipTime).toLocaleDateString() : ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_Rohs)}">${d.VCHR_Rohs || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_COCQ)}">${d.VCHR_COCQ || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_MSDS)}">${d.VCHR_MSDS || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_AnToan)}">${d.VCHR_AnToan || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatchCamKet)}">${d.VCHR_CamKet || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.NVCHR_DeliveryTerm || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.NVCHR_PaymentTerm || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.NVCHR_File || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.DTM_EffectiveDate ? new Date(d.DTM_EffectiveDate).toLocaleDateString() : ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.DTM_ExpiryDate ? new Date(d.DTM_ExpiryDate).toLocaleDateString() : ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${totalCell || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">
                        <select class="form-control form-control-sm supplier-choice" data-madon="${d.CHR_MaDon || ''}" data-mahang="${d.CHR_MaHangNoiBo || ''}" data-id="${d.ID || ''}">
                            <option value="" ${(!d.BIT_Select && d.BIT_Select !== false) ? 'selected' : ''}></option>
                            <option value="true" ${d.BIT_Select === true ? 'selected' : ''}>O</option>
                            <option value="false" ${d.BIT_Select === false ? 'selected' : ''}>X</option>
                        </select>
                    </td>
                    <td><input type="text" class="form-control form-control-sm reason-input" value="${d.NVCHR_ReasonPick || ''}"></td>
                    <td><input type="text" class="form-control form-control-sm reason-input" value="${d.NVCHR_Note || ''}"></td>
                </tr>
            `;
            }).join('');

            tbody.innerHTML = rowsHtml;
            // reapply columns visibility for newly rendered rows
            this.applyAdditionalColumnsVisibility();
        },
        // mapping status supplier tab
        MappingStatusSupplier: function (codeStatus) {
            const T = window.i18nQuotationResults || {};
            switch (codeStatus) {
                case 'WAIT_PICK_NCC': return T.WaitPickSupplier || 'Chờ chọn nhà cung cấp';
                case 'PICKED': return T.SupplierSelected || 'Đã chọn nhà cung cấp';
                case 'WAIT_CONFIRM_NAME': return T.WaitConfirmName || 'Chờ xác nhận tên';
                case 'CONFIRMED': return T.WaitApproval || 'Chờ phê duyệt';
                case 'WAIT_NCC': return T.WaitSupplierQuote || 'Chờ báo giá nhà cung cấp';
                default: return '';
            }
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
            // Reset (button id in view is 'btnClear')
            const btnClear = document.getElementById('btnClear');
            if (btnClear) btnClear.addEventListener('click', this.resetFilters.bind(this));

            // Export File excel
            const btnExportFileTab1 = document.getElementById('btnExcelExport');
            if (btnExportFileTab1) btnExportFileTab1.addEventListener('click', this.ExportFileTab1.bind(this));

            // Reset supplier search filters
            const btnResetSupplierSearch = document.getElementById('btnClearnTAB2');
            if (btnResetSupplierSearch) btnResetSupplierSearch.addEventListener('click', this.resetFiltersTab2.bind(this));

            // Xác nhận lựa chọn
            const btnConfirmTop = document.getElementById('btnConfirmTop');
            const btnConfirmBottom = document.getElementById('btnConfirmBottom');
            if (btnConfirmTop) btnConfirmTop.addEventListener('click', this.confirmSelection.bind(this));
            if (btnConfirmBottom) btnConfirmBottom.addEventListener('click', this.confirmSelection.bind(this));

            // Save tab2 selections - open approver selection first
            const btnSaveTab2 = document.getElementById('SaveTab2');
            if (btnSaveTab2) btnSaveTab2.addEventListener('click', this.openApproverSelectionAndSave.bind(this));

            // Hủy
            const btnCancel = document.getElementById('btnCancel');
            if (btnCancel) {
                btnCancel.addEventListener('click', this.cancelSelection.bind(this));
            }
            // nhap excel
            const btnUploadExcel = document.getElementById('btnUploadExcel');
            if (btnUploadExcel) {
                btnUploadExcel.addEventListener('click', this.ImportExcelApproval.bind(this));
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

            // Supplier search
            const supplierSearchBtn = document.getElementById('supplierSearchBtn');
            if (supplierSearchBtn) {
                supplierSearchBtn.addEventListener('click', this.loadSupplierData.bind(this));
            }
            // Dowload file templeate tab 2
            const btnDownloadTem = document.getElementById('btnDownloadTem');
            if (btnDownloadTem) {
                btnDownloadTem.addEventListener('click', this.ExportExcelTepleate.bind(this));
            }
            // Input file Excel to system
            const btnImportSupplier = document.getElementById('supplierImportExcelBtn');
            if (btnImportSupplier) {
                btnImportSupplier.addEventListener('click', this.ImportSupplier.bind(this));
            }
            // Delegate: enforce only one supplier per maDon AND handle supplier-choice selects
            document.addEventListener('change', (e) => {
                // Handle checkbox selection inside expanded supplier-group (old style)
                const cb = e.target.closest('.supplier-select');
                if (cb) {
                    if (!cb.checked) return;
                    // Find current supplier group and derive maDon from groupId pattern: CHR_MaDon-CHR_MaHangNoiBo
                    const row = cb.closest('tr');
                    const groupContainer = row?.closest('.supplier-group');
                    const groupId = groupContainer?.id?.replace('sup-rows-', '') || '';
                    const maDon = groupId.split('-')[0] || '';
                    const maHang = groupId.split('-')[1] || '';
                    if (!maDon) return;
                    // Uncheck other supplier-select in all groups matching same maDon, except current checkbox
                    document.querySelectorAll('.supplier-group[id^="sup-rows-' + maDon + '-"] .supplier-select').forEach(other => {
                        if (other !== cb) other.checked = false;
                    });

                    // Also sync main supplier table selects: set matching supplier select to true, others in same group to false
                    const supplierId = cb.getAttribute('data-id') || cb.value || '';
                    if (supplierId) {
                        const selMatch = document.querySelector(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"][data-id="${supplierId}"]`);
                        if (selMatch) {
                            // find reason input in expanded row
                            const reasonEl = row.querySelector('.reason-input');
                            const reason = reasonEl?.value?.trim() || '';
                            if (!reason) {
                                // prompt for reason
                                showPrompt({ title: (window.i18nQuotationResults && window.i18nQuotationResults.Reason) || 'Lý do', message: (window.i18nQuotationResults && window.i18nQuotationResults.PromptEnterReason) || 'Vui lòng nhập lý do chọn nhà cung cấp', placeholder: '' })
                                    .then(r => {
                                        if (!r) {
                                            cb.checked = false;
                                            try { reasonEl && reasonEl.focus(); } catch { }
                                            return;
                                        }
                                        try { reasonEl.value = r; } catch { }
                                        selMatch.value = 'true';
                                        document.querySelectorAll(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"]`).forEach(s => { if (s !== selMatch) s.value = 'false'; });
                                    });
                                return;
                            }
                            selMatch.value = 'true';
                            // set other selects in same group to false
                            document.querySelectorAll(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"]`).forEach(s => { if (s !== selMatch) s.value = 'false'; });
                        }
                    }
                    return;
                }

                // Handle select change in supplierQuoteTable rows
                const sel = e.target.closest('.supplier-choice');
                if (sel) {
                    const val = sel.value;
                    const row = sel.closest('tr');
                    const maDon = row?.getAttribute('data-madon') || '';
                    const maHang = row?.getAttribute('data-mahang') || '';
                    const reasonEl = row?.querySelector('.reason-input');
                    if (val === 'true') {
                        const reason = reasonEl?.value?.trim() || '';
                        if (!reason) {
                            showPrompt({ title: (window.i18nQuotationResults && window.i18nQuotationResults.Reason) || 'Lý do', message: (window.i18nQuotationResults && window.i18nQuotationResults.PromptEnterReason) || 'Vui lòng nhập lý do chọn nhà cung cấp', placeholder: '' })
                                .then(r => {
                                    if (!r) {
                                        try { sel.value = ''; } catch { }
                                        try { reasonEl && reasonEl.focus(); } catch { }
                                        return;
                                    }
                                    try { reasonEl.value = r; } catch { }
                                    document.querySelectorAll(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"]`).forEach(s => {
                                        if (s !== sel) s.value = 'false';
                                    });
                                });
                            return;
                        }
                        // set other suppliers in same maDon+maHang to false
                        document.querySelectorAll(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"]`).forEach(s => {
                            if (s !== sel) s.value = 'false';
                        });
                    }
                }
            });
        },
        // Save selections from supplier table to server using SavePickSupplier endpoint
        saveTab2: async function () {
            const T = window.i18nQuotationResults || {};
            const btn = document.getElementById('SaveTab2');
            try {
                if (btn) btn.disabled = true;
                const rows = Array.from(document.querySelectorAll('#supplierQuoteBody tr'));
                const payload = [];
                rows.forEach(row => {
                    const sel = row.querySelector('select.supplier-choice');
                    if (!sel) return;
                    // include only rows where selection is explicitly set (true/false)
                    const val = sel.value;
                    if (val === '') return;
                    const idAttr = row.getAttribute('data-id') || sel.getAttribute('data-id') || '';
                    const id = idAttr !== '' && !isNaN(Number(idAttr)) ? Number(idAttr) : idAttr;
                    const reason = (row.querySelector('.reason-input')?.value || '').toString();
                    const maDon = row.getAttribute('data-madon') || '';
                    const maHang = row.getAttribute('data-mahang') || '';
                    payload.push({ ID: id, BIT_Select: (val === 'true'), NVCHR_ReasonPick: reason, CHR_MaDon: maDon, CHR_MaHangNoiBo: maHang });
                });
                const approverNext = window.__selectedNextApprover || '';
                if (!payload.length) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgWarnSelectOne || 'Vui lòng chọn ít nhất một nhà cung cấp.'), type: 'info' });
                    return;
                }
                var payloadWithApprover = { UserApproverNext: approverNext, listPick: payload };
                showLoading((T && T.LoadingData) ? T.LoadingData : 'Đang lưu...');
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/SavePickSupplier', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payloadWithApprover)
                });
                hideLoading();
                if (!res.ok) {
                    const txt = await res.text().catch(() => 'Lỗi server');
                    showDialog({ title: T.Notification || 'Thông báo', message: txt || (T.MsgSaveError || 'Lưu thất bại'), type: 'error' });
                    return;
                }
                const data = await res.json().catch(() => null);
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgSaveSuccess || 'Lưu thành công'), type: 'success' });
                // refresh supplier data to reflect saved selections
                this.loadSupplierData();
                // reset selected approver after save
                try { window.__selectedNextApprover = null; } catch { }
            } catch (err) {
                hideLoading();
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (err && err.message) ? err.message : (T.MsgSaveError || 'Lưu thất bại'), type: 'error' });
            } finally {
                if (btn) btn.disabled = false;
            }
        },

        // Open approver selection modal, fetch approvers and on confirm call saveTab2 with approver
        openApproverSelector: function (stepNumber, sectionCode) {
            // follow the same pattern used in Approval_Quote.js: return a Promise resolving to selected approver object or null
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
                    placeholderOpt.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.SelectPlaceholder) || '-- Chọn --';
                    sel.appendChild(placeholderOpt);

                    // fetch approvers from Quote controller
                    const body = { Step: stepNumber, SectionCost: sectionCode };
                    let list = [];
                    try {
            const resp = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/GetListApprovel', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                            body: JSON.stringify(body)
                        });
                        if (resp.ok) {
                            const data = await resp.json();
                            // controller returns data (could be array or wrapper)
                            list = Array.isArray(data) ? data : (data && data.data ? data.data : []);
                        }
                    } catch (e) { console.warn('Failed to load approvers', e); }

                    if (!list || !list.length) {
                        const emptyOpt = document.createElement('option');
                        emptyOpt.value = '';
                        emptyOpt.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.NoResults) || 'Không có kết quả';
                        sel.appendChild(emptyOpt);
                    } else {
                        list.forEach(item => {
                            const o = document.createElement('option');
                            // normalize likely server keys
                            const adid = item.chR_UserAdid || item.CHR_UserAdid || item.ADID || item.Id || item.id || '';
                            const name = item.nvchR_UserName || item.NVCHR_UserName || item.Name || item.FullName || item.nvchR_FullName || '';
                            o.value = adid || '';
                            o.textContent = (name ? (name + (adid ? (' (' + adid + ')') : '')) : (adid || ''));
                            try { o.dataset.raw = JSON.stringify(item); } catch { }
                            sel.appendChild(o);
                        });
                    }

                    // ensure modal attached to body
                    try { if (modal.parentElement !== document.body) document.body.appendChild(modal); } catch (e) { }
                    // show modal
                    try {
                        if (window.bootstrap && bootstrap.Modal) {
                            const bsModal = new bootstrap.Modal(modal, { backdrop: 'static' });
                            modal._bsModal = bsModal;
                            bsModal.show();
                            setTimeout(() => {
                                try { const createdBackdrop = document.querySelector('.modal-backdrop'); if (createdBackdrop) createdBackdrop.style.zIndex = '10550'; modal.style.zIndex = '10600'; } catch (e) { }
                            }, 10);
                        } else {
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

                    const confirmBtn = document.getElementById('confirmSelectApprover');
                    function cleanup() {
                        try { if (modal._bsModal) modal._bsModal.hide(); else { modal.style.display = 'none'; modal.classList.remove('show'); } } catch (e) { try { modal.style.display = 'none'; modal.classList.remove('show'); } catch { } }
                        try { if (modal._backdrop) { document.body.removeChild(modal._backdrop); delete modal._backdrop; } } catch (e) { }
                        try { confirmBtn.removeEventListener('click', onConfirm); } catch (e) { }
                        try { modal.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.removeEventListener('click', onCancel)); } catch (e) { }
                        if (notice) notice.style.display = 'none';
                        try { modal.style.zIndex = ''; } catch (e) { }
                    }
                    function onConfirm(e) {
                        e && e.preventDefault();
                        const value = sel.value;
                        if (!value) {
                            if (notice) notice.style.display = '';
                            return;
                        }
                        const raw = sel.selectedOptions && sel.selectedOptions[0] && sel.selectedOptions[0].dataset.raw;
                        let obj = null;
                        try { obj = raw ? JSON.parse(raw) : { CHR_UserAdid: value, NVCHR_UserName: sel.selectedOptions[0].textContent }; } catch { obj = { CHR_UserAdid: value, NVCHR_UserName: sel.selectedOptions[0].textContent }; }
                        cleanup();
                        resolve(obj);
                    }
                    function onCancel() { cleanup(); resolve(null); }
                    if (confirmBtn) confirmBtn.addEventListener('click', onConfirm);
                    try { modal.querySelectorAll('[data-bs-dismiss="modal"]').forEach(b => b.addEventListener('click', onCancel)); } catch (e) { }
                } catch (err) { reject(err); }
            });
        },

        openApproverSelectionAndSave: async function () {
            try {
                const step = 9;
                const section = document.getElementById('supplierSearchSection')?.value || document.getElementById('searchPhongBan')?.value || '';
                const selected = await this.openApproverSelector(step, section);
                if (!selected) return; // cancelled
                // normalize selected id field
                const approverId = selected.CHR_UserAdid ?? selected.chR_UserAdid ?? selected.CHR_Adid ?? selected.chR_Adid ?? selected.ADID ?? selected.Id ?? selected.id ?? selected.value ?? ''; // robust fallback
                // fallback to value if object has none
                const finalId = approverId || (selected.value || selected.Value || '');
                if (!finalId) {
                    // still save without approver
                    await this.saveTab2();
                    return;
                }
                window.__selectedNextApprover = finalId;
                await this.saveTab2();
            } catch (err) {
                console.error('openApproverSelectionAndSave error', err);
                showDialog({ message: 'Lỗi khi lấy danh sách người phê duyệt', type: 'error' });
            }
        },

        toggleAdditionalColumns: function () {
            const T = window.i18nQuotationResults || {};
            const table = document.getElementById('supplierQuoteTable');
            const btn = document.getElementById('toggleAdditionalColumns');
            if (!table || !btn) return;
            const showing = table.classList.toggle('show-additional');
            try {
                // update button text to reflect state
                const T = window.i18nQuotationResults || {};
                btn.textContent = showing ? (T.HideDetails || 'Ẩn chi tiết') : (T.ShowDetails || 'Hiện chi tiết');
            } catch { }
            const toggleBtn = document.getElementById('toggleAdditionalColumns');
            if (!toggleBtn) return;
            const isHidden = toggleBtn.textContent.includes(T.HideDetails||'Ẩn');

            // Persist state
            window._quotationResultsState.showAdditionalColumns = !isHidden;

            // Ẩn các th và td có class 'additional-column'
            const columns = document.querySelectorAll('#supplierQuoteTable th.additional-column, #supplierQuoteTable td.additional-column');
            columns.forEach(col => {
                if (isHidden) {
                    col.style.display = 'none';
                } else {
                    col.style.display = '';
                }
            });

            // Ẩn th DescriptionGroup (colspan=8)
            const descGroupTh = document.querySelector('#supplierQuoteTable th[colspan="8"].additional-column');
            if (descGroupTh) {
                descGroupTh.style.display = isHidden ? 'none' : '';
            }

            // Ẩn th BIVN Input (colspan=24)
            const bivnInputTh = document.querySelector('#supplierQuoteTable th[colspan="24"]');
            if (bivnInputTh) {
                bivnInputTh.style.display = isHidden ? 'none' : '';
            }

            // Ẩn th Vendor Input (colspan=21)
            //const vendorInputTh = document.querySelector('#supplierQuoteTable th[colspan="21"]');
            //if (vendorInputTh) {
            //    vendorInputTh.style.display = isHidden ? 'none' : '';
            //}

            toggleBtn.textContent = isHidden ? (T.ShowDetails || 'Hiện chi tiết') : (T.HideDetails || 'Ẩn chi tiết');
        },

        applyAdditionalColumnsVisibility: function () {
            // apply persisted visibility state to supplier table columns
            try {
                const state = window._quotationResultsState || { showAdditionalColumns: true };
                const shouldShow = !!state.showAdditionalColumns;
                const T = window.i18nQuotationResults || {};
                const toggleBtn = document.getElementById('toggleAdditionalColumns');
                if (toggleBtn) toggleBtn.textContent = shouldShow ? (T.HideDetails || 'Ẩn chi tiết') : (T.ShowDetails || 'Hiện chi tiết');
                const columns = document.querySelectorAll('#supplierQuoteTable th.additional-column, #supplierQuoteTable td.additional-column');
                columns.forEach(col => { col.style.display = shouldShow ? '' : 'none'; });
                // DescriptionGroup and BIVN Input header
                const descGroupTh = document.querySelector('#supplierQuoteTable th[colspan="8"].additional-column');
                if (descGroupTh) descGroupTh.style.display = shouldShow ? '' : 'none';
                const bivnInputTh = document.querySelector('#supplierQuoteTable th[colspan="19"]');
                if (bivnInputTh) bivnInputTh.style.display = shouldShow ? '' : 'none';
            } catch (e) { /* ignore */ }
        },
        openEditRequestModal: async function (id) {
            try {
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/SearchID', {
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
                    return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}`;
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
        // function xuat file
        ExportExcelTepleate: async function () {
            const payload = {
                MaDon: document.getElementById('supplierSearchMaDon')?.value || '',
                MaNcc: document.getElementById('supplierSearchMaNcc')?.value || '',
                MaVatTu: document.getElementById('supplierSearchMaVatTu')?.value || '',
                Section: document.getElementById('supplierSearchSection')?.value || '',
                Status: document.getElementById('searchStatusTab2')?.value || '',
                PageIndex: supplierState.pageIndex,
                PageSize: supplierState.pageSize,
            };
                try {
                const T = window.i18nQuotationResults || {};
                showLoading(T.LoadingData || 'Đang xử lý...');
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/ExportFileExcelQuotationResult', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (!res.ok) {
                    const msg = await res.text().catch(() => 'Lỗi không xác định');
                    throw new Error(msg || 'Xuất file thất bại');
                }
                const blob = await res.blob();
                let fileName = 'ResultQuotation.xlsx';
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
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (err && err.message) ? err.message : (T.MsgExportError || 'Không thể xuất file'), type: 'error' });
            } finally {
                hideLoading();
            }
        },
        // nhap file excel
        ImportExcelApproval: async function () {
            // Tạo input file ẩn
            const fileInput = document.createElement('input');
            fileInput.type = 'file';
            fileInput.accept = '.xlsx, .xls';
            fileInput.style.display = 'none';
            document.body.appendChild(fileInput);

            fileInput.addEventListener('change', function () {
                const file = fileInput.files[0];
                if (!file) return;
                const T = window.i18nQuotationResults || {};
                // Kiểm tra loại file
                const allowedTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
                if (!allowedTypes.includes(file.type)) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.InvalidFileType || 'Không thể xuất file'), type: 'error' });
                    document.body.removeChild(fileInput);
                    return;
                }

                // Tạo FormData
                const formData = new FormData();
                formData.append('file', file);
                // Gửi request
                try { showLoading((window.i18nQuotationResults && window.i18nQuotationResults.LoadingData) || 'Đang xử lý...'); } catch { }
            fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/ImportApprovalQuotianExcel', {
                    method: 'POST',
                    body: formData
                })
                    .then(response => {
                        if (!response.ok) {
                            return response.text().then(text => { throw new Error(text || 'Lỗi server'); });
                        }

                        const contentType = response.headers.get('content-type');
                        if (contentType && contentType.includes('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')) {
                            // Trả về file lỗi
                            return response.blob().then(blob => {
                                const url = window.URL.createObjectURL(blob);
                                const a = document.createElement('a');
                                a.href = url;
                                a.download = `ImportErrors_${new Date().toISOString().slice(0, 19).replace(/:/g, '')}.xlsx`;
                                document.body.appendChild(a);
                                a.click();
                                document.body.removeChild(a);
                                window.URL.revokeObjectURL(url);
                                showDialog({ title: T.Notification || 'Thông báo', message: (T.FileHasErrorsDownloaded || 'File có lỗi. Đã tải xuống file lỗi để kiểm tra.'), type: 'warning' });
                            });
                        } else {
                            // Thành công
                            return response.json().then(data => {
                                showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Nhập file thành công'), type: 'success' });
                            });
                        }
                    })
                    .catch(error => {
                        const T = window.i18nQuotationResults || {};
                        showDialog({ title: T.Notification || 'Thông báo', message: (error && error.message) ? error.message : (T.ErrorPrefix || 'Không thể xuất file'), type: 'error' });
                    })
                    .finally(() => {
                        try { hideLoading(); } catch { }
                        document.body.removeChild(fileInput);
                    });
            });
            this.loadSupplierData(); // refresh data before opening file dialog
            try {
                // open native file dialog
                fileInput.click();
            } catch (e) {
                console.error('Could not open file dialog', e);
            }
        },
        // function Import tab 2
        ImportSupplier: async function () {
            // Tạo input file ẩn
            const fileInput = document.createElement('input');
            fileInput.type = 'file';
            fileInput.accept = '.xlsx, .xls';
            fileInput.style.display = 'none';
            document.body.appendChild(fileInput);
            // chọn người phê duyệt
            const selectedApproverTab2 = await this.openApproverSelector(9, "");
            // Nếu không chọn người phê duyệt, thoát
            if (!selectedApproverTab2) {
                return;
            }
            fileInput.addEventListener('change', function () {
                const file = fileInput.files[0];
                if (!file) return;
                const T = window.i18nQuotationResults || {};
                // Kiểm tra loại file
                const allowedTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
                if (!allowedTypes.includes(file.type)) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.InvalidFileType || 'Không thể xuất file'), type: 'error' });
                    document.body.removeChild(fileInput);
                    return;
                }

                // Tạo FormData
                const formData = new FormData();
                formData.append('file', file);
                const fd = new FormData();
                fd.append('fileSend', file);
                fd.append('userNextApproval', selectedApproverTab2?.chR_UserAdid || '');
                // Gửi request
                try { showLoading((window.i18nQuotationResults && window.i18nQuotationResults.LoadingData) || 'Đang xử lý...'); } catch { }
            fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/ImportQuotianExcel', {
                    method: 'POST',
                    body: fd
                })
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => { throw new Error(text || 'Lỗi server'); });
                    }

                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')) {
                        // Trả về file lỗi
                        return response.blob().then(blob => {
                            const url = window.URL.createObjectURL(blob);
                            const a = document.createElement('a');
                            a.href = url;
                            a.download = `ImportErrors_${new Date().toISOString().slice(0, 19).replace(/:/g, '')}.xlsx`;
                            document.body.appendChild(a);
                            a.click();
                            document.body.removeChild(a);
                            window.URL.revokeObjectURL(url);
                            showDialog({ title: T.Notification || 'Thông báo', message: (T.FileHasErrorsDownloaded || 'File có lỗi. Đã tải xuống file lỗi để kiểm tra.'), type: 'warning' });
                        });
                    } else {
                        // Thành công
                        return response.json().then(data => {
                            showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Nhập file thành công'), type: 'success' });
                        });
                    }
                })
                .catch(error => {
                    const T = window.i18nQuotationResults || {};
                    showDialog({ title: T.Notification || 'Thông báo', message: (error && error.message) ? error.message : (T.ErrorPrefix || 'Không thể xuất file'), type: 'error' });
                })
                .finally(() => {
                    try { hideLoading(); } catch { }
                    document.body.removeChild(fileInput);
                });
            });
            this.loadSupplierData(); // refresh data before opening file dialog
            try {
                // open native file dialog
                fileInput.click();
            } catch (e) {
                console.error('Could not open file dialog', e);
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
                        pageSize: 30,
                        pageIndex: 0
                    };
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/SearchInputQuote', {
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
                    const rowsHtml = (data.data || []).map(d => {
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
            // persist open/closed state for this group so switching tabs doesn't lose it
            try {
                const groupId = (button.getAttribute('data-madon') || '') + '-' + (button.getAttribute('data-mahang') || '');
                window._quotationResultsState = window._quotationResultsState || { openGroups: {}, showAdditionalColumns: true };
                window._quotationResultsState.openGroups[groupId] = willOpen;
            } catch { }
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
            // Use select IDs from the view: searchMaDon, searchMaterial, searchPhongBan, searchStatus
            const maDon = document.getElementById('searchMaDon')?.value || '';
            const maHang = document.getElementById('searchMaterial')?.value || '';
            const section = document.getElementById('searchPhongBan')?.value || '';
            const status = document.getElementById('searchStatus')?.value || '';
            const payload = {
                maDon: maDon,
                maHang: maHang,
                section: section,
                status: status,
                pageIndex: requestListState.pageIndex,
                pageSize: requestListState.pageSize
            };
            try {
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/GetThongTinBaoGiaGomNhom', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (!res.ok) {
                    console.error('Search failed');
                    return;
                }
                const result = await res.json();
                const items = Array.isArray(result.data.data) ? result.data.data : [];
                const totalCount = result.data.totalCount || 0;
                requestListState.returnedCount = items.length;
                requestListState.totalCount = totalCount;
                requestListState.lastPage = (requestListState.pageIndex * requestListState.pageSize) >= totalCount;
                const tbody = document.getElementById('quotationResultsTableBody');
                if (!tbody) return;
                const rowsHtml = (items || []).map(i => {
                    const ngay = i.DTM_NgayMuonNhan ? new Date(i.DTM_NgayMuonNhan).toLocaleDateString('vi-VN') : '';
                    const ngayAttr = i.DTM_NgayMuonNhan ? new Date(i.DTM_NgayMuonNhan).toISOString().slice(0, 10) : '';
                    const statusClass = (function (s) {
                        if (!s && s !== 0) return 'bg-secondary';
                        const str = String(s).trim();
                        const code = str.toUpperCase();
                        // Map known status codes
                        if (code === 'WAITING_NCC') return 'bg-warning text-dark';
                        if (code === 'WAITING_PICK_NCC') return 'bg-success';
                        if (code === 'WAITING_APPROVER') return 'bg-primary text-white';
                        if (code === 'NO') return 'bg-secondary';

                        return 'bg-secondary';
                    })(i.Status);
                    const mapNameStatus = this.MappingStatusApproverSupplier(i.Status, i.ID_StepBaoGia);
                    const groupId = `${i.CHR_MaDon}-${i.CHR_MaHangNoiBo}`;
                    return `
                        <tr class="item-row" data-status="${i.Status || ''}" data-id="${i.CHR_MaDon || ''}">
                            <td class="text-center align-middle"><input type="checkbox" class="row-select" data-madon-select=${i.CHR_MaDon} /></td>
                            <td class="detail-cell text-center">
                                <button type="button" class="btn btn-sm btn-outline-primary toggle-sup" data-target="#sup-rows-${groupId}" data-madon="${i.CHR_MaDon || ''}" data-mahang="${i.CHR_MaHangNoiBo || ''}" data-ngay="${ngayAttr}" aria-expanded="false" title="Xem chi tiết">
                                    <i class="fas fa-info-circle"></i>
                                </button>
                            </td>
                            <td class="text-start"><span class="ms-1 fw-semibold">${i.CHR_MaDon || ''}</span></td>
                            <td class="text-start">${i.CHR_SectionName || ''}</td>
                            <td class="text-center">${i.CHR_CreateBy || ''}</td>
                            <td>${i.suppliesList || ''}</td>
                            <td>${i.categoryList || ''}</td>
                            <td class="text-center">${i.DTM_KyHan ? new Date(i.DTM_KyHan).toLocaleDateString('vi-VN') : ''}</td>
                            <td class="text-center">${i.DTM_NgayMuonNhan ? new Date(i.DTM_NgayMuonNhan).toLocaleDateString('vi-VN') : ''}</td>
                            <td class="text-center"><span class="badge status-badge ${statusClass}">${mapNameStatus}</span></td>
                        </tr>
                     `;
                }).join('');
                tbody.innerHTML = rowsHtml;
                const T = window.i18nQuotationResults || {};
                // Update summary
                const summaryText = document.getElementById('summaryText');
                if (summaryText) {
                    const startOne = requestListState.returnedCount === 0 ? 0 : ((requestListState.pageIndex - 1) * requestListState.pageSize + 1);
                    const endOne = requestListState.returnedCount === 0 ? 0 : ((requestListState.pageIndex - 1) * requestListState.pageSize + requestListState.returnedCount);
                    summaryText.textContent = `${T.Sum || 'Tổng số'}: ${startOne}-${endOne} / ${requestListState.totalCount}`;
                }

                // Xử lý select all checkbox
                const selectAllCheckbox = document.getElementById('selectAll');
                if (selectAllCheckbox) {
                    // Xóa event cũ nếu có
                    selectAllCheckbox.removeEventListener('change', this.handleSelectAll);
                    // Thêm event mới
                    selectAllCheckbox.addEventListener('change', this.handleSelectAll.bind(this));
                }

                // Thêm event cho từng checkbox row-select
                document.querySelectorAll('#quotationResultsTableBody .row-select').forEach(checkbox => {
                    checkbox.removeEventListener('change', this.handleRowCheckboxChange);
                    checkbox.addEventListener('change', this.handleRowCheckboxChange.bind(this));
                });


                // Render pagination
                this.renderRequestListPaginationControls();

                // Reapply persisted open-groups state so previously expanded supplier groups remain expanded
                try {
                    const state = window._quotationResultsState || { openGroups: {} };
                    Object.keys(state.openGroups || {}).forEach(gid => {
                        try {
                            const row = document.getElementById('sup-rows' + gid);
                            const btn = document.querySelector(`.toggle-sup[data-madon="${gid.split('-')[0]}"][data-mahang="${gid.split('-')[1]}"]`);
                            if (state.openGroups[gid]) {
                                if (row) row.classList.remove('d-none');
                                if (btn) {
                                    btn.setAttribute('aria-expanded', 'true');
                                    const icon = btn.querySelector('i');
                                    if (icon) { icon.classList.remove('fa-chevron-down'); icon.classList.add('fa-chevron-up'); }
                                }
                            } else {
                                if (row) row.classList.add('d-none');
                                if (btn) {
                                    btn.setAttribute('aria-expanded', 'false');
                                    const icon = btn.querySelector('i');
                                    if (icon) { icon.classList.remove('fa-chevron-up'); icon.classList.add('fa-chevron-down'); }
                                }
                            }
                        } catch (e) { }
                    });
                } catch (e) { }
                // Also reapply additional columns visibility
                this.applyAdditionalColumnsVisibility();
                // Wire up click on row to open approval modal (avoid clicks on toggle button / checkbox)
                document.querySelectorAll('#quotationResultsTableBody .item-row').forEach(row => {
                    const detailBtn = row.querySelector('.toggle-sup');
                    if (detailBtn) {
                        detailBtn.removeEventListener('click', this.handleDetailClick);
                        // Gắn event mới với bind this
                        detailBtn.addEventListener('click', (ev) => {
                            ev.stopPropagation(); 
                            const maDon = row.getAttribute('data-id');
                            if (maDon) {
                                quotationApp.openApprovalModal(maDon);
                            }
                        });
                    }
                });
            } catch (err) {
                console.error('Error calling GetThongTinBaoGiaGomNhom', err);
            }
        },

        // Xử lý khi click vào checkbox select all
        handleSelectAll: function (event) {
            const isChecked = event.target.checked;
            const allRowCheckboxes = document.querySelectorAll('#quotationResultsTableBody .row-select');

            allRowCheckboxes.forEach(checkbox => {
                checkbox.checked = isChecked;
            });

            // Cập nhật trạng thái select all (nếu cần)
            this.updateSelectAllState();

            // Gọi callback khi có sự thay đổi (nếu cần)
            this.onSelectionChange();
        },

        // Xử lý khi click vào từng checkbox row
        handleRowCheckboxChange: function (event) {
            // Cập nhật trạng thái của select all checkbox
            this.updateSelectAllState();

            // Gọi callback khi có sự thay đổi
            this.onSelectionChange();
        },

        // Cập nhật trạng thái của checkbox select all
        updateSelectAllState: function () {
            const selectAllCheckbox = document.getElementById('selectAll');
            if (!selectAllCheckbox) return;

            const allRowCheckboxes = document.querySelectorAll('#quotationResultsTableBody .row-select');
            const checkedCheckboxes = document.querySelectorAll('#quotationResultsTableBody .row-select:checked');

            if (allRowCheckboxes.length === 0) {
                selectAllCheckbox.checked = false;
                selectAllCheckbox.indeterminate = false;
            } else if (checkedCheckboxes.length === 0) {
                selectAllCheckbox.checked = false;
                selectAllCheckbox.indeterminate = false;
            } else if (checkedCheckboxes.length === allRowCheckboxes.length) {
                selectAllCheckbox.checked = true;
                selectAllCheckbox.indeterminate = false;
            } else {
                selectAllCheckbox.checked = false;
                selectAllCheckbox.indeterminate = true; // Trạng thái chưa chọn hết
            }
        },

        // Hàm callback khi có thay đổi selection (tùy chọn)
        onSelectionChange: function () {
            const selectedMaDons = this.getSelectedMaDon();

            // Cập nhật UI hiển thị số lượng đã chọn
            const selectedCountSpan = document.getElementById('selectedCount');
            if (selectedCountSpan) {
                selectedCountSpan.textContent = selectedMaDons.length;
            }

            // Enable/disable button xuất dữ liệu dựa trên số lượng chọn
            const exportBtn = document.getElementById('exportBtn');
            if (exportBtn) {
                exportBtn.disabled = selectedMaDons.length === 0;
            }
        },

        // Lấy danh sách mã đơn đã chọn
        getSelectedMaDon: function () {
            return Array.from(document.querySelectorAll('#quotationResultsTableBody .row-select:checked'))
                .map(cb => cb.getAttribute('data-madon-select'))
                .filter(maDon => maDon);
        },
        // mapping status Approver supplier tab
        MappingStatusApproverSupplier: function (codeStatus, step) {
            const T = window.i18nQuotationResults || {};
            switch (codeStatus) {
                case 'WAITING_NCC': return T.WaitPickApSupplier || 'Chờ báo gía nhà cung cấp';
                case 'WAITING_PICK_NCC': return T.SupplierApSelected || 'Chờ chọn nhà cung cấp';
                case 'WAITING_APPROVER':
                    if (step === 9) {
                        return T.ChiefApproval;
                    } else if (step === 10) {
                        return T.SectionApproval;
                    } else {
                        return T.DeptApproval;
                    }
                    //return T.WaitApConfirmName || 'Chờ phê duyệt';
                case 'NO': return T.undefined || 'Không xác định';
                default: return '';
            }
        },

        // Export File Excel tab 1
        ExportFileTab1: async function () {
            const selectedMaDons = [];

            // Lấy tất cả checkbox có class 'row-select' và đã được checked
            const checkboxes = document.querySelectorAll('#quotationResultsTableBody .row-select:checked');

            checkboxes.forEach(checkbox => {
                const maDon = checkbox.getAttribute('data-madon-select');
                if (maDon) {
                    selectedMaDons.push(maDon);
                }
            });
            if (selectedMaDons.length === 0) {
                const T = window.i18nQuotationResults || {};
                showDialog({
                    title: T.Notification || 'Thông báo',
                    message: T.MsgNoRowSelected || 'Vui lòng chọn ít nhất một dòng để xuất file',
                    type: 'warning'
                });
                return;
            }
            try {
                const T = window.i18nQuotationResults || {};
                showLoading(T.LoadingData || 'Đang xử lý...');
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/ExportFileExcelApproverResult', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(selectedMaDons)
                });
                if (!res.ok) {
                    const msg = await res.text().catch(() => 'Lỗi không xác định');
                    throw new Error(msg || 'Xuất file thất bại');
                }
                const blob = await res.blob();
                let fileName = 'ResultQuotationApprover.xlsx';
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
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (err && err.message) ? err.message : (T.MsgExportError || 'Không thể xuất file'), type: 'error' });
            } finally {
                hideLoading();
            }
        },
        // Open approval modal by request id and populate fields
        openApprovalModal: async function (maDon) {
            if (!maDon) return;
            try {
                const T = window.i18nQuotationResults || {};
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/GetSupplierApprovalInfor', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(maDon)
                });
                if (!res.ok) {
                    console.error('Load request detail failed');
                    showDialog({ message: T.RequestFailed || 'Không tải được dữ liệu chi tiết yêu cầu.' });
                    return;
                }
                const data = await res.json();

                // Helper to safely get possible property names (case-insensitive)
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

                // Determine master record to populate modal header/footer (use first detail if API returned array)
                let master = data;
                if (Array.isArray(data) && data.length > 0) {
                    master = data[0];
                }

                // populate modal header/footer fields using master record
                try { document.getElementById('madonhang').textContent = getVal(master, 'CHR_MaDon', 'chR_MaDon', 'ID') || maDon; } catch { }
                try { document.getElementById('khoi').textContent = getVal(master, 'CHR_SectionName', 'chR_SectionName') || ''; } catch { }
                try { document.getElementById('mpb_yc').textContent = getVal(master, 'CHR_SectionCode', 'chR_SectionCode', 'chR_CostCenter') || ''; } catch { }
                try { document.getElementById('tenphongban').textContent = getVal(master, 'CHR_SectionName', 'chR_SectionName') || ''; } catch { }
                try { document.getElementById('nyc').textContent = formatDate(getVal(master, 'DTM_NgayMuonNhan', 'dtM_NgayMuonNhan', 'DTM_CreateDate')); } catch { }
                try { document.getElementById('thmm').textContent = formatDate(getVal(master, 'DTM_KyHan', 'dtM_KyHan', 'DTM_Deadline')); } catch { }
                try { document.getElementById('requester').textContent = getVal(master, 'CHR_CreateBy', 'chR_CreateBy') || '-'; } catch { }
                try { document.getElementById('id_request').textContent = getVal(master, 'ID', 'iD', 'CHR_MaDon') || maDon; } catch { }
                try { document.getElementById('step').textContent = getVal(master, 'ID_StepBaoGia', 'iD_StepBaoGia') || ''; } catch { }

                // urgent badge
                try {
                    const ub = document.getElementById('urgent-badge');
                    const gap = getVal(data, 'CHR_Gap', 'chR_Gap');
                    const isUrgent = gap === true || String(gap).toLowerCase() === 'true' || String(gap) === '1' || String(gap).toLowerCase() === 'o';
                    if (ub) ub.style.display = isUrgent ? '' : 'none';
                } catch { }

                // Prepare details array: controller might return object with .Detail or an array
                let details = [];
                if (Array.isArray(data)) {
                    details = data;
                } else if (data && Array.isArray(data.Detail)) {
                    details = data.Detail;
                } else if (data && Array.isArray(data.data)) {
                    details = data.data;
                } else if (data && typeof data === 'object') {
                    if (Array.isArray(data.DetailList)) details = data.DetailList;
                    else details = [data];
                }

                const tbody = document.getElementById('detailModalBody');
                if (!tbody) return;
                tbody.innerHTML = '';
                const frag = document.createDocumentFragment();

                const mismatchStyle = (v) => {
                    if (v === false || v === 0 || v === '0' || String(v).toLowerCase() === 'false') {
                        return 'color: #a00; background-color: #ffecec;';
                    }
                    return '';
                };

                const isBitSelectFalse = (d) => {
                    const bitSelect = getVal(d, 'BIT_Select', 'bit_Select');
                    return bitSelect === false || bitSelect === 0 || String(bitSelect).toLowerCase() === 'false';
                };

                // LƯU TRỮ DỮ LIỆU LỰA CHỌN CỦA TỪNG DÒNG
                const rowSelections = []; // Mỗi phần tử: { isApproved: bool, rejectReason: string, id: int }

                details.forEach((d, idx) => {
                    const tr = document.createElement('tr');
                    tr.className = 'text-center';
                    tr.setAttribute('data-row-index', idx);

                    if (isBitSelectFalse(d)) {
                        tr.style.backgroundColor = '#f5f5f5';
                    }

                    const addTd = (txt, cls, style) => {
                        const td = document.createElement('td');
                        td.textContent = txt == null ? '' : String(txt);
                        if (cls) td.className = cls;
                        if (style) td.style.cssText = style;
                        return td;
                    };

                    tr.appendChild(addTd(idx + 1));
                    //tr.appendChild(addTd(getVal(d, 'iD', 'ID', 'id')));
                    tr.appendChild(addTd(getVal(d, 'chR_MaHangNoiBo', 'CHR_MaHangNoiBo')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_ChungLoai', 'NVCHR_ChungLoai')));
                    //tr.appendChild(addTd(getVal(d, 'chR_Phanloai', 'CHR_Phanloai')));
                    //tr.appendChild(addTd(getVal(d, 'chR_MaHangNCC', 'CHR_MaHangNCC')));

                    const nameVN = getVal(d, 'nvchR_NameVN', 'NVCHR_NameVN') || '';
                    const nameEN = getVal(d, 'nvchR_NameEN', 'NVCHR_NameEN') || '';
                    //tr.appendChild(addTd((nameVN + (nameEN ? ' / ' + nameEN : '')).trim(), 'text-start'));

                    //tr.appendChild(addTd(getVal(d, 'inT_SoLuong', 'INT_SoLuong') || '', 'text-center'));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_DonVi', 'NVCHR_DonVi') || '', 'text-center'));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_HinhDang', 'NVCHR_HinhDang')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_ChatLieu', 'NVCHR_ChatLieu')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_ThanhPhan', 'NVCHR_ThanhPhan')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_KichThuoc', 'NVCHR_KichThuoc')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_DongMay', 'NVCHR_DongMay')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_TinhNang', 'NVCHR_TinhNang')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_FileThietKe', 'NVCHR_FileThietKe')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_NhaSanXuat', 'NVCHR_NhaSanXuat')));
                    //tr.appendChild(addTd(getVal(d, 'chR_MaNCC', 'CHR_MaNCC')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_TenNCC', 'NVCHR_TenNCC')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_Rohs', 'NVCHR_Rohs')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_COCQ', 'NVCHR_COCQ')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_MSDS', 'NVCHR_MSDS')));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_AnToan', 'NVCHR_AnToan')));
                    //tr.appendChild(addTd(formatDate(getVal(d, 'dtM_KyHan', 'DTM_KyHan')), 'text-center'));

                    const gap = getVal(d, 'chR_Gap', 'CHR_Gap');
                    const gapLabel = gap != null && gap !== '' ? (String(gap).toLowerCase() === 'true' || String(gap) === '1' ? 'O' : 'X') : '';
                    //tr.appendChild(addTd(gapLabel, 'text-center'));

                    const lay = getVal(d, 'biT_LayBaoGia', 'BIT_LayBaoGia');
                    const layLabel = lay != null && lay !== '' ? (String(lay).toLowerCase() === 'true' || String(lay) === '1' ? 'O' : 'X') : '';
                    //tr.appendChild(addTd(layLabel, 'text-center'));
                    //tr.appendChild(addTd(getVal(d, 'nvchR_LyDo', 'NVCHR_LyDo')));

                    const fmtNum = (price, qty) => {
                        try {
                            const numPrice = parseFloat(price);
                            const numQty = parseFloat(qty);
                            if (isNaN(numPrice)) return '';
                            const total = isNaN(numQty) ? numPrice : numPrice * numQty;
                            return total.toLocaleString();
                        } catch {
                            return '';
                        }
                    };

                    tr.appendChild(addTd(getVal(d, 'CHR_MaNCC', 'chR_MaNCC')));
                    tr.appendChild(addTd(getVal(d, 'NVCHR_NameNCC', 'nvchR_NameNCC'), 'text-start'));
                    tr.appendChild(addTd(getVal(d, 'CHR_MaHangNCC', 'chR_MaHangNCC'), null, mismatchStyle(getVal(d, 'IsMatch_MaHangNCC', 'IsMatch_MaHangNCC'))));
                    //tr.appendChild(addTd(getVal(d, 'NVCHR_TenHangHQ', 'nvchR_TenHangHQ'), 'text-start', mismatchStyle(getVal(d, 'IsMatch_NameVN', 'IsMatch_NameVN'))));
                    //tr.appendChild(addTd(getVal(d, 'NameENByNCC', 'nameENByNCC'), null, mismatchStyle(getVal(d, 'IsMatch_NameEN', 'IsMatch_NameEN'))));
                    tr.appendChild(addTd(getVal(d, 'soluong', 'INT_SoLuong', 'soluong') || '', 'text-center', mismatchStyle(getVal(d, 'IsMatch_SoLuong', 'IsMatch_SoLuong'))));
                    tr.appendChild(addTd(getVal(d, 'donvi', 'NVCHR_DonVi') || '', 'text-center', mismatchStyle(getVal(d, 'IsMatch_DonVi', 'IsMatch_DonVi'))));
                    tr.appendChild(addTd(getVal(d, 'FL_USD', 'fl_usd')));
                    tr.appendChild(addTd(getVal(d, 'FL_VND', 'fl_vnd')));
                    //tr.appendChild(addTd(getVal(d, 'NVCHR_MOQ', 'nvchr_MOQ')));
                    //tr.appendChild(addTd(getVal(d, 'DTM_LeadTime', 'dtm_LeadTime')));
                    //tr.appendChild(addTd(formatDate(getVal(d, 'DTM_ShipTime', 'dtm_ShipTime')), null, mismatchStyle(getVal(d, 'IsMatch_Ngay', 'IsMatch_Ngay'))));
                    //tr.appendChild(addTd(getVal(d, 'VCHR_Rohs', 'vchr_Rohs'), null, mismatchStyle(getVal(d, 'IsMatch_Rohs', 'IsMatch_Rohs'))));
                    //tr.appendChild(addTd(getVal(d, 'VCHR_COCQ', 'vchr_COCQ'), null, mismatchStyle(getVal(d, 'IsMatch_COCQ', 'IsMatch_COCQ'))));
                    //tr.appendChild(addTd(getVal(d, 'VCHR_MSDS', 'vchr_MSDS'), null, mismatchStyle(getVal(d, 'IsMatch_MSDS', 'IsMatch_MSDS'))));
                    //tr.appendChild(addTd(getVal(d, 'VCHR_AnToan', 'vchr_AnToan'), null, mismatchStyle(getVal(d, 'IsMatch_AnToan', 'IsMatch_AnToan'))));
                    //tr.appendChild(addTd(getVal(d, 'VCHR_CamKet', 'vchr_CamKet'), null, mismatchStyle(getVal(d, 'IsMatchCamKet', 'IsMatchCamKet'))));
                    //tr.appendChild(addTd(getVal(d, 'NVCHR_DeliveryTerm', 'nvchr_DeliveryTerm')));
                    //tr.appendChild(addTd(getVal(d, 'NVCHR_PaymentTerm', 'nvchr_PaymentTerm')));
                    tr.appendChild(addTd(getVal(d, 'NVCHR_File', 'nvchr_File')));
                    tr.appendChild(addTd(formatDate(getVal(d, 'DTM_EffectiveDate', 'dtm_EffectiveDate'))));
                    tr.appendChild(addTd(formatDate(getVal(d, 'DTM_ExpiryDate', 'dtm_ExpiryDate'))));

                    const totalSys = (getVal(d, 'FL_VND')) ? (fmtNum(getVal(d, 'FL_VND'), getVal(d, 'soluong')) + ' VND') : (getVal(d, 'FL_USD') ? (fmtNum(getVal(d, 'FL_USD'), getVal(d, 'soluong')) + ' USD') : '');
                    tr.appendChild(addTd(totalSys, 'text-center'));

                    const pick = getVal(d, 'BIT_Select', 'bit_Select');
                    const pickLabel = pick === true || String(pick).toLowerCase() === 'true' ? 'O' : (pick === false || String(pick).toLowerCase() === 'false' ? 'X' : '');
                    tr.appendChild(addTd(pickLabel, 'text-center'));
                    tr.appendChild(addTd(getVal(d, 'NVCHR_ReasonPick', 'nvchr_ReasonPick') || getVal(d, 'NVCHR_LyDo', 'nvchr_LyDo')));
                    tr.appendChild(addTd(getVal(d, 'NVCHR_Note', 'nvchr_Note')));
                    // Lấy ID của dòng (quan trọng để gửi lên server)
                    const itemId = getVal(d, 'ID', 'iD');

                    // THÊM 2 CỘT MỚI: ĐỒNG Ý (CHECKBOX) VÀ LÝ DO TỪ CHỐI 
                    // Cột Đồng ý (checkbox)
                    const tdApprove = document.createElement('td');
                    tdApprove.className = 'text-center align-middle';
                    tdApprove.style.verticalAlign = 'middle';
                    const checkbox = document.createElement('input');
                    checkbox.type = 'checkbox';
                    checkbox.className = 'form-check-input approval-checkbox';
                    checkbox.style.margin = '0';  // Reset margin
                    checkbox.style.marginLeft = '-6px';
                    checkbox.style.marginTop = '-4px';
                    checkbox.setAttribute('data-row-index', idx);
                    // Mặc định tích chọn (đồng ý)
                    checkbox.checked = true;
                    tdApprove.appendChild(checkbox);
                    tr.appendChild(tdApprove);

                    // Cột Lý do từ chối (input text)
                    const tdReason = document.createElement('td');
                    tdReason.className = 'text-center';
                    tdReason.style.verticalAlign = 'middle';

                    const reasonInput = document.createElement('input');
                    reasonInput.type = 'text';
                    reasonInput.className = 'form-control form-control-sm reject-reason-input';
                    reasonInput.style.width = '100%';
                    reasonInput.style.minWidth = '150px';
                    reasonInput.placeholder = T.InputReasonTitle +'...';
                    reasonInput.disabled = true;
                    reasonInput.setAttribute('data-row-index', idx);
                    tdReason.appendChild(reasonInput);
                    tr.appendChild(tdReason);

                    // Lưu tham chiếu vào mảng rowSelections
                    rowSelections[idx] = {
                        id: itemId,
                        isApproved: true,
                        rejectReason: '',
                        checkbox: checkbox,
                        reasonInput: reasonInput
                    };

                    // Xử lý sự kiện khi checkbox thay đổi
                    checkbox.addEventListener('change', function (e) {
                        const rowIdx = parseInt(this.getAttribute('data-row-index'));
                        const isChecked = this.checked;
                        rowSelections[rowIdx].isApproved = isChecked;
                        rowSelections[rowIdx].rejectReason = '';
                        const input = rowSelections[rowIdx].reasonInput;
                        input.disabled = isChecked;
                        if (isChecked) {
                            input.value = '';
                            input.classList.remove('is-invalid');
                        }
                    });

                    // Xử lý sự kiện khi nhập lý do từ chối
                    reasonInput.addEventListener('input', function (e) {
                        const rowIdx = parseInt(this.getAttribute('data-row-index'));
                        rowSelections[rowIdx].rejectReason = this.value;
                        if (this.value.trim() !== '') {
                            this.classList.remove('is-invalid');
                        }
                    });

                    frag.appendChild(tr);
                });

                tbody.appendChild(frag);

                // HÀM KIỂM TRA TÍNH HỢP LỆ TRƯỚC KHI GỬI 
                const validateSelections = () => {
                    let isValid = true;
                    // Reset validation styles
                    for (let i = 0; i < rowSelections.length; i++) {
                        const sel = rowSelections[i];
                        if (sel.reasonInput) {
                            sel.reasonInput.classList.remove('is-invalid');
                        }
                    }
                    // Kiểm tra từng dòng: nếu không đồng ý (checked = false) mà lý do trống
                    for (let i = 0; i < rowSelections.length; i++) {
                        const sel = rowSelections[i];
                        if (!sel.isApproved && (!sel.rejectReason || sel.rejectReason.trim() === '')) {
                            isValid = false;
                            if (sel.reasonInput) {
                                sel.reasonInput.classList.add('is-invalid');
                                sel.reasonInput.focus();
                            }
                            showDialog({ message: (T.MSNGInputReason || 'Dòng thứ {0}: Vui lòng nhập lý do từ chối.').replace('{0}', i + 1), type: 'warning' });
                            break;
                        }
                    } 
                    return isValid;
                };

                // HÀM LẤY DỮ LIỆU PHÊ DUYỆT CHI TIẾT THEO ĐÚNG FORMAT ApproverDTO
                const getApprovalDetails = () => {
                    const approverList = [];
                    for (let i = 0; i < rowSelections.length; i++) {
                        const sel = rowSelections[i];
                        approverList.push({
                            Id: sel.id,
                            IsApproved: sel.isApproved,
                            Reason: sel.isApproved ? '' : (sel.rejectReason || '')
                        });
                    }
                    return approverList;
                };

                // Xử lý nút Confirm duy nhất
                const btnConfirm = document.getElementById('modalConfirm');
                if (btnConfirm) {
                    // Clone và thay thế để xóa hết các event cũ
                    const newBtnConfirm = btnConfirm.cloneNode(true);
                    btnConfirm.parentNode.replaceChild(newBtnConfirm, btnConfirm);

                    newBtnConfirm.onclick = async () => {
                        // Kiểm tra tính hợp lệ trước khi gửi
                        if (!validateSelections()) {
                            return;
                        }

                        // Lấy danh sách phê duyệt
                        const listConfirm = getApprovalDetails();

                        // Kiểm tra nếu không có dữ liệu
                        if (!listConfirm || listConfirm.length === 0) {
                            showDialog({ message: T.NoData||'Không có dữ liệu phê duyệt.', type: 'warning' });
                            return;
                        }

                        // Kiểm tra xem step hiện tại có cần chọn người phê duyệt tiếp theo không
                        const nextStep = master.ID_StepBaoGia;
                        let userApproverNext = '';

                        // Nếu step = 9 (QLSC) thì cần chọn người phê duyệt tiếp theo (QLTC)
                        if (nextStep === 9) {
                            const selectedApprover = await this.openApproverSelector(10, "");
                            if (!selectedApprover) {
                                return; // Người dùng đã hủy chọn
                            }
                            userApproverNext = selectedApprover.chR_UserAdid;
                        }

                        try {
                            newBtnConfirm.disabled = true;
                            showDialog({ message: T.LoadingData || 'Đang xử lý...', type: 'info', autoClose: false });

                            // Tạo payload theo đúng model ConfirmApproverModel
                            const payload = {
                                listCofirm: listConfirm,
                                UserApproverNext: userApproverNext
                            };

            const response = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/ConfirmApprover', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify(payload)
                            });

                            if (!response.ok) {
                                const errorText = await response.text().catch(() => null);
                                showDialog({ message: errorText || 'Xác nhận phê duyệt thất bại', type: 'error' });
                                return;
                            }

                            const result = await response.json().catch(() => null);
                            const hasRejected = listConfirm.some(item => item.IsApproved === false);

                            showDialog({
                                message: hasRejected ? T.Confirmation || 'Xác nhận phê duyệt thành công (có dòng bị từ chối)' : T.MsgSusscesAprover || 'Xác nhận phê duyệt thành công',
                                type: 'success'
                            });

                            // Reload dữ liệu và đóng modal
                            if (typeof quotationApp !== 'undefined' && quotationApp.searchItems) {
                                quotationApp.searchItems();
                            }

                            // Đóng modal (gọi hàm hideEditModal nếu có)
                            if (typeof hideEditModal === 'function') {
                                hideEditModal();
                            } else {
                                // Tìm và đóng modal theo cách thông thường
                                const modal = document.getElementById('editModal');
                                if (modal && typeof bootstrap !== 'undefined') {
                                    const bsModal = bootstrap.Modal.getInstance(modal);
                                    if (bsModal) bsModal.hide();
                                }
                            }

                        } catch (error) {
                            console.error('Confirm error:', error);
                            showDialog({ message: 'Error Approval: '+error, type: 'error' });
                        } finally {
                            newBtnConfirm.disabled = false;
                        }
                    };
                } else {
                    console.warn('Button modalConfirm not found');
                }

                // show modal
                showModal();
            } catch (err) {
                console.error('Error loading request detail', err);
                showDialog({ message: 'Đã xảy ra lỗi khi tải dữ liệu.' });
            }
        },
        renderRequestListPaginationControls: function () {
            const container = document.getElementById('paginationControls');
            if (!container) return;
            container.innerHTML = '';

            const totalPages = requestListState.totalCount ? Math.ceil(requestListState.totalCount / requestListState.pageSize) : 1;

            // Previous button
            const prevBtn = document.createElement('button');
            prevBtn.type = 'button';
            prevBtn.className = 'btn btn-sm btn-outline-secondary';
            prevBtn.textContent = '‹';
            prevBtn.disabled = requestListState.pageIndex <= 1;
            prevBtn.addEventListener('click', () => {
                if (requestListState.pageIndex > 1) {
                    requestListState.pageIndex--;
                    this.searchItems();
                }
            });
            container.appendChild(prevBtn);

            // Render a small range of page buttons around current page
            const range = 2;
            const start = Math.max(1, Math.min(requestListState.pageIndex - range, Math.max(1, totalPages - (range * 2))));
            const pages = [];
            for (let i = start; i <= Math.min(totalPages, start + (range * 2)); i++) {
                pages.push(i);
            }

            pages.forEach(p => {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'btn btn-sm ' + (p === requestListState.pageIndex ? 'btn-primary' : 'btn-outline-secondary');
                btn.textContent = p;
                if (p > totalPages) btn.disabled = true;
                btn.addEventListener('click', () => {
                    if (p !== requestListState.pageIndex) {
                        requestListState.pageIndex = p;
                        this.searchItems();
                    }
                });
                container.appendChild(btn);
            });

            // Next button
            const nextBtn = document.createElement('button');
            nextBtn.type = 'button';
            nextBtn.className = 'btn btn-sm btn-outline-secondary';
            nextBtn.textContent = '›';
            nextBtn.disabled = requestListState.pageIndex >= totalPages || requestListState.returnedCount === 0;
            nextBtn.addEventListener('click', () => {
                if (!nextBtn.disabled) {
                    requestListState.pageIndex++;
                    this.searchItems();
                }
            });
            container.appendChild(nextBtn);

            // Update paging info
            const pagingInfo = document.getElementById('pagingInfo');
            if (pagingInfo) {
                const startOne = requestListState.returnedCount === 0 ? 0 : ((requestListState.pageIndex - 1) * requestListState.pageSize + 1);
                const endOne = requestListState.returnedCount === 0 ? 0 : ((requestListState.pageIndex - 1) * requestListState.pageSize + requestListState.returnedCount);
                pagingInfo.textContent = `${startOne}-${endOne} / ${requestListState.totalCount}`;
            }
        },

        renderSupplierPaginationControls: function () {
            const container = document.getElementById('supplierPaginationControls');
            if (!container) return;
            container.innerHTML = '';

            const totalPages = supplierState.totalCount ? Math.ceil(supplierState.totalCount / supplierState.pageSize) : 1;

            // Previous button
            const prevBtn = document.createElement('button');
            prevBtn.type = 'button';
            prevBtn.className = 'btn btn-sm btn-outline-secondary';
            prevBtn.textContent = '‹';
            prevBtn.disabled = supplierState.pageIndex <= 1;
            prevBtn.addEventListener('click', () => {
                if (supplierState.pageIndex > 1) {
                    supplierState.pageIndex--;
                    this.loadSupplierData();
                }
            });
            container.appendChild(prevBtn);

            // Render a small range of page buttons around current page
            const range = 2;
            const start = Math.max(1, Math.min(supplierState.pageIndex - range, Math.max(1, totalPages - (range * 2))));
            const pages = [];
            for (let i = start; i <= Math.min(totalPages, start + (range * 2)); i++) {
                pages.push(i);
            }

            pages.forEach(p => {

                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'btn btn-sm ' + (p === supplierState.pageIndex ? 'btn-primary' : 'btn-outline-secondary');
                btn.textContent = p;
                if (p > totalPages) btn.disabled = true;
                btn.addEventListener('click', () => {
                    if (p !== supplierState.pageIndex) {
                        supplierState.pageIndex = p;
                        this.loadSupplierData();
                    }
                });
                container.appendChild(btn);
            });

            // Next button
            const nextBtn = document.createElement('button');
            nextBtn.type = 'button';
            nextBtn.className = 'btn btn-sm btn-outline-secondary';
            nextBtn.textContent = '›';
            nextBtn.disabled = supplierState.pageIndex >= totalPages || supplierState.returnedCount === 0;
            nextBtn.addEventListener('click', () => {
                if (!nextBtn.disabled) {
                    supplierState.pageIndex++;
                    this.loadSupplierData();
                }
            });
            container.appendChild(nextBtn);

            // Update paging info
            const pagingInfo = document.getElementById('supplierPagingInfo');
            if (pagingInfo) {
                const startOne = supplierState.returnedCount === 0 ? 0 : ((supplierState.pageIndex - 1) * supplierState.pageSize + 1);
                const endOne = supplierState.returnedCount === 0 ? 0 : ((supplierState.pageIndex - 1) * supplierState.pageSize + supplierState.returnedCount);
                pagingInfo.textContent = `${startOne}-${endOne} / ${supplierState.totalCount}`;
            }
        },

        resetFilters: function () {
            // Reset select filters (set value and dispatch change so enhanced dropdown UI updates)
            const selIds = ['searchMaDon', 'searchPhongBan', 'searchMaterial'];
            selIds.forEach(id => {
                const el = document.getElementById(id);
                if (el) {
                    try {
                        el.value = '';
                        el.selectedIndex = 0;
                        el.dispatchEvent(new Event('change', { bubbles: true }));
                    } catch (e) { /* ignore */ }
                }
            });
            const statusEl = document.getElementById('searchStatus');
            if (statusEl) {
                statusEl.value = '1';
                try { statusEl.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
            }

            // Ensure underlying selects are updated
            ['searchMaDon', 'searchPhongBan', 'searchMaterial', 'searchStatus'].forEach(id => {
                const el = document.getElementById(id);
                if (el) {
                    try { el.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
                }
            });

            // Reset pagination
            requestListState.pageIndex = 1;

            // Show all items
            document.querySelectorAll('.item-row').forEach(item => { item.style.display = ''; });

            // Reload data
            this.searchItems();
        },
        resetFiltersTab2: function () {
            // Reset select filters (set value and dispatch change so enhanced dropdown UI updates)
            const selIds = ['supplierSearchSection', 'supplierSearchMaVatTu', 'supplierSearchMaNcc', 'supplierSearchMaDon'];
            selIds.forEach(id => {
                const el = document.getElementById(id);
                if (el) {
                    try {
                        el.value = '';
                        el.selectedIndex = 0;
                        el.dispatchEvent(new Event('change', { bubbles: true }));
                    } catch (e) { /* ignore */ }
                }
            });
            const statusEl = document.getElementById('searchStatusTab2');
            if (statusEl) {
                statusEl.value = 'WAIT_PICK_NCC';
                try { statusEl.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
            }

            // Ensure underlying selects are updated
            ['supplierSearchSection', 'supplierSearchMaVatTu', 'supplierSearchMaNcc', 'supplierSearchMaDon', 'searchStatusTab2'].forEach(id => {
                const el = document.getElementById(id);
                if (el) {
                    try { el.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
                }
            });

            // Reset pagination
            supplierState.pageIndex = 1;

            // Show all items
            document.querySelectorAll('.item-row').forEach(item => { item.style.display = ''; });

            // Reload data
            this.loadSupplierData();
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
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgWarnSelectOne || 'Vui lòng chọn ít nhất một nhà cung cấp.'), type: 'info' });
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
            const res = await fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/ChonNhaCungCapBaoGia', {
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
            fetch((window.apiBaseUrl || '') + '/QuoteQuotationResults/ExportSelection', {
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
                    a.download = `SelectionQuote_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')}.xlsx`;
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
// Prompt dialog that returns a Promise resolving to the entered text, or null if cancelled
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
function ToDateTimeLocal(date) {
    if (!date) return null;
    // Nếu đã là ISO (yyyy-MM-ddTHH:mm:ss) hoặc yyyy-MM-dd, giữ nguyên
    if (/^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2})?$/.test(date)) {
        if (date.length === 10) return date + 'T00:00:00';
        return date;
    }
    // nếu là dd/MM/yyyy hh:mm:ss AM/PM
    const match = date.match(/(\d{2})\/(\d{2})\/(\d{4}) (\d{1,2}):(\d{2}):(\d{2}) (AM|PM)/);
    if (match) {
        let [_, d, m, y, h, min, s, ap] = match;
        h = parseInt(h, 10);
        if (ap === 'PM' && h < 12) h += 12;
        if (ap === 'AM' && h === 12) h = 0;
        const pad = n => n.toString().padStart(2, '0');
        return `${y}-${pad(m)}-${pad(d)}T${pad(h)}:${pad(min)}:${pad(s)}`;
    }
    // nếu là dd/MM/yyyy
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
