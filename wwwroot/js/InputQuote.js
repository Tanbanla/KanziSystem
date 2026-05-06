// InputQuote.js - JavaScript cho màn hình nhập thông tin báo giá
(function() {
    'use strict';

    // State management
    let quoteState = {
        pageIndex: 1, // 1-based index for server API
        pageSize: 10,
        returnedCount: 0,
        totalCount: 0,
        lastPage: false
    };

    // DOM Elements
    let elements = {};
    function initEnhancements(root) {
        try {
            buildSearchableDropdown(root || document);
        } catch (e) {
    
        }

    }
    // Initialize elements after DOM and Bootstrap are loaded
    function initializeElements() {
        elements = {
            inputQuoteTableBody: document.getElementById('inputQuoteTableBody'),
            summaryText: document.getElementById('summaryText'),
            selectAll: document.getElementById('selectAll')
            ,
            pageSizeSelect: document.getElementById('pageSizeSelect'),
            paginationControls: document.getElementById('paginationControls'),
            pagingInfo: document.getElementById('pagingInfo')
        };
    }

    // Event Listeners
    function initializeEventListeners() {
        // Button events
        document.getElementById('btnSearch')?.addEventListener('click', loadQuoteList);
        document.getElementById('btnClear')?.addEventListener('click', clearFilters);
        document.getElementById('selectAll')?.addEventListener('change', toggleSelectAll);
        document.getElementById('btnSampleExcel')?.addEventListener('click', exportSampleExcel);
        elements.pageSizeSelect?.addEventListener('change', function () {
            const v = parseInt(this.value, 10) || 10;
            quoteState.pageSize = v;
            quoteState.pageIndex = 1;
            loadQuoteList();
        });
        
        // Load initial data
        loadQuoteList();
    }

    // Load list of quotation requests
    function loadQuoteList() {
        //document.getElementById('supplierSelect')?.value ||
        // Mock data for now, replace with API call
        const body = {
            maDon: document.getElementById('searchMaDon')?.value || '',
            section: document.getElementById('searchPhongBan')?.value || '',
            maHang: document.getElementById('searchMaterial')?.value || '',
            pageSize: quoteState.pageSize,
            pageIndex: quoteState.pageIndex
        };

        callApi((window.apiBaseUrl || '') + '/Quote/SearchInputQuoteBySoDon', body)
            .then(res => {
                if (!res) return;
                const items = Array.isArray(res.data) ? res.data : [];
                const total = typeof res.totalCount === 'number' ? res.totalCount : items.length;
                quoteState.returnedCount = items.length;
                quoteState.totalCount = total;
                quoteState.lastPage = (quoteState.pageIndex * quoteState.pageSize) >= total;
                renderQuoteList(items);
                showAlert('success', window.i18nInputQuote.DataFilteredSuccessfully);
            })
            .catch(err => showAlert('danger', window.i18nInputQuote.FilterFailed.replace('{0}', err)));
    }

    // Render the list of quotes
    function renderQuoteList(data) {
        const tbody = elements.inputQuoteTableBody;
        tbody.innerHTML = '';
        const items = Array.isArray(data) ? data : [];

        items.forEach((item, index) => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td class="text-center"><input type="checkbox" class="quote-select" value="${item.CHR_MaDon}" /></td>
                <td class="text-center">
                    <button class="btn btn-sm btn-outline-primary detail-btn" data-createby="${item.CHR_CreateBy}" data-madon="${item.CHR_MaDon}">
                        <i class="fas fa-eye"></i>
                    </button>
                </td>
                <td class="text-center">${item.CHR_MaDon}</td>
                <td class="text-center">${formatDateDs(item.DTM_CreateDate)}</td>
                <td class="text-center">${item.CHR_SectionName}</td>
                <td class="text-center">${item.CHR_CreateBy}</td>
                <td class="text-center">${item.SoLuongLinhKien}</td>
                <td>${item.DanhSachNCC}</td>
                <td class="text-center">${item.TrangThai}</td>
            `;
            tbody.appendChild(row);

            // Attach event listener for detail button
            const btn = row.querySelector('.detail-btn');
            if (btn) btn.addEventListener('click', () => openDetailPage(item));
        });

        // summary shows returned items count for current page and total
        const startOne = quoteState.returnedCount === 0 ? 0 : ((quoteState.pageIndex - 1) * quoteState.pageSize + 1);
        const endOne = quoteState.returnedCount === 0 ? 0 : ((quoteState.pageIndex - 1) * quoteState.pageSize + quoteState.returnedCount);
        elements.summaryText.textContent = (window.i18nInputQuote.SummaryFormat || '{0}').replace('{0}', `${startOne}-${endOne} / ${quoteState.totalCount}`);
        renderPaginationControls();
    }

    function renderPaginationControls() {
        const container = elements.paginationControls;
        if (!container) return;
        container.innerHTML = '';
        // Create prev button
        const prevBtn = document.createElement('button');
        prevBtn.type = 'button';
        prevBtn.className = 'btn btn-sm btn-outline-secondary';
        prevBtn.textContent = '‹';
        prevBtn.disabled = quoteState.pageIndex <= 1;
        prevBtn.addEventListener('click', () => { if (quoteState.pageIndex > 1) goToPage(quoteState.pageIndex - 1); });
        container.appendChild(prevBtn);

        // Render a small range of page buttons around current page
        const range = 2; // pages before/after
        const totalPages = quoteState.totalCount ? Math.ceil(quoteState.totalCount / quoteState.pageSize) : 1;
        const start = Math.max(1, Math.min(quoteState.pageIndex - range, Math.max(1, totalPages - (range * 2))));
        const pages = [];
        for (let i = start; i <= Math.min(totalPages, start + (range * 2)); i++) {
            pages.push(i);
        }

        pages.forEach(p => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'btn btn-sm ' + (p === quoteState.pageIndex ? 'btn-primary' : 'btn-outline-secondary');
            btn.textContent = p;
            // disable future pages if we know we're on last page and p > current
            if (p > totalPages) btn.disabled = true;
            btn.addEventListener('click', () => { if (p !== quoteState.pageIndex) goToPage(p); });
            container.appendChild(btn);
        });

        // Create next button
        const nextBtn = document.createElement('button');
        nextBtn.type = 'button';
        nextBtn.className = 'btn btn-sm btn-outline-secondary';
        nextBtn.textContent = '›';
        nextBtn.disabled = quoteState.pageIndex >= totalPages || quoteState.returnedCount === 0;
        nextBtn.addEventListener('click', () => { if (!nextBtn.disabled) goToPage(quoteState.pageIndex + 1); });
        container.appendChild(nextBtn);

        // paging info (start-end)
        if (elements.pagingInfo) {
            const startOne = quoteState.returnedCount === 0 ? 0 : ((quoteState.pageIndex - 1) * quoteState.pageSize + 1);
            const endOne = quoteState.returnedCount === 0 ? 0 : ((quoteState.pageIndex - 1) * quoteState.pageSize + quoteState.returnedCount);
            elements.pagingInfo.textContent = `${startOne}-${endOne} / ${quoteState.totalCount}`;
        }
    }

    function goToPage(index) {
        if (index < 1) index = 1;
        quoteState.pageIndex = index;
        loadQuoteList();
        // ensure selectAll is cleared
        if (elements.selectAll) elements.selectAll.checked = false;
    }

    // Open detail page for a specific quote
    function openDetailPage(item) {
        window.location.href = (window.apiBaseUrl || '') + `/Quote/InputQuoteDetail?maDon=${item.CHR_MaDon}`;
    }
    // Download sample Excel file
    function exportSampleExcel() {
        const url = (window.apiBaseUrl || '') + '/template/TmSendMailNew.xlsx';
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Sample_Export.xlsx';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }
    // Clear filters
    function clearFilters() {
        // Clear selects and inputs used as filters
        const fields = ['searchMaDon', 'searchPhongBan','searchMaterial'];
        fields.forEach(id => {
            const el = document.getElementById(id);
            if (el) {
                if (el.tagName === 'SELECT') el.selectedIndex = 0;
                else el.value = '';
                // remove validation classes
                el.classList.remove('is-invalid');
                el.classList.remove('is-valid');
                // ensure any enhanced/select widgets update their UI
                try { el.dispatchEvent(new Event('change', { bubbles: true })); } catch (e) { }
            }
        });
        loadQuoteList();     
        const T = window.i18nInputQuote || {};
        showAlert('success', T.MsgResetSuccess || 'Đã đặt lại bộ lọc và xóa các mục hiện tại');
    }

    // Toggle select all
    function toggleSelectAll() {
        const checkboxes = document.querySelectorAll('.quote-select');
        checkboxes.forEach(cb => cb.checked = elements.selectAll.checked);
    }

    // Utility functions
    function formatDateDs(dateStr) {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit'
        });
    }

    function showAlert(type, message) {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        alertDiv.innerHTML = `${message}`;
        document.body.appendChild(alertDiv);
        setTimeout(() => alertDiv.remove(), 5000);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        setTimeout(init, 100);
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
            const T = window.i18nInputQuote || {};
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
                    const empty = document.createElement('div'); empty.className = 'ms-empty'; empty.textContent = (window.i18nInputQuote && window.i18nInputQuote.NoResults) || 'Không có kết quả';
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
                    const T = window.i18nInputQuote || {};
                    placeholderEl.textContent = T.SelectPlaceholder || '-- Chọn --';
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

    // Generic API caller
    function callApi(url, body) {
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        }).then(r => {
            if (!r.ok) return r.text().then(t => { throw (t || ('HTTP ' + r.status)); });
            return r.json();
        });
    }
    // tab nhập theo nhà cung cấp
    // Supplier Quote Tab State
    let supplierState = {
        currentPage: 1,
        pageSize: 10,
        totalPages: 0,
        totalCount: 0,
        searchParams: {
            idRequestQuote: 0,
            maDon: '',
            maVatTu: '',
            maNcc: '',
            section: '',
            dayMM: null
        }
    };

    // Initialize supplier elements
    function initializeSupplierElements() {
        // Elements are already in the DOM
    }

    // Initialize supplier event listeners
    function initializeSupplierEventListeners() {
        // Tab change event
        // document.getElementById('supplier-input-tab')?.addEventListener('shown.bs.tab', function (e) {
        //     loadSupplierQuotes(supplierState.searchParams, supplierState.currentPage, supplierState.pageSize);
        // });

        // Search button
        document.getElementById('supplierSearchBtn')?.addEventListener('click', function() {
            supplierState.searchParams = {
                idRequestQuote: 0,
                maDon: (document.getElementById('supplierSearchMaDon')?.value || '').trim(),
                maVatTu: (document.getElementById('supplierSearchMaVatTu')?.value || '').trim(),
                maNcc: (document.getElementById('supplierSearchMaNcc')?.value || '').trim(),
                section: (document.getElementById('supplierSearchSection')?.value || '').trim(),
                dayMM: document.getElementById('supplierSearchDayMM')?.value || null
            };
            supplierState.currentPage = 1;
            loadSupplierQuotes(supplierState.searchParams, supplierState.currentPage, supplierState.pageSize);
        });

        // Import Excel button
        document.getElementById('supplierImportExcelBtn')?.addEventListener('click', function() {
            // Tạo input file ẩn
            const fileInput = document.createElement('input');
            fileInput.type = 'file';
            fileInput.accept = '.xlsx, .xls';
            fileInput.style.display = 'none';
            document.body.appendChild(fileInput);

            fileInput.addEventListener('change', function() {
                const file = fileInput.files[0];
                if (!file) return;

                // Kiểm tra loại file
                const allowedTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
                if (!allowedTypes.includes(file.type)) {
                    showAlert('danger', window.i18nInputQuote.InvalidFileType || 'Chỉ chấp nhận file Excel (.xlsx, .xls)');
                    document.body.removeChild(fileInput);
                    return;
                }

                // Tạo FormData
                const formData = new FormData();
                formData.append('file', file);

                // Gửi request
                fetch((window.apiBaseUrl || '') + '/Quote/ImportExcelInputQuote', {
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
                            showAlert('warning', window.i18nInputQuote.FileHasErrorsDownloaded || 'File có lỗi. Đã tải xuống file lỗi để kiểm tra.');
                        });
                    } else {
                        // Thành công
                        return response.json().then(data => {
                            showAlert('success', window.i18nInputQuote.DataUpdatedSuccessfully || 'Dữ liệu đã được cập nhật thành công');
                        });
                    }
                })
                .catch(error => {
                    showAlert('danger', (window.i18nInputQuote.ErrorPrefix || 'Lỗi: ') + error.message);
                })
                .finally(() => {
                    document.body.removeChild(fileInput);
                });
            });

            // Trigger click để mở file picker
            fileInput.click();
        });

        // Page size change
        document.getElementById('supplierPageSizeSelect')?.addEventListener('change', function() {
            supplierState.pageSize = parseInt(this.value) || 10;
            supplierState.currentPage = 1;
            loadSupplierQuotes(supplierState.searchParams, supplierState.currentPage, supplierState.pageSize);
        });
    }

    // Load supplier quotes
    async function loadSupplierQuotes(searchParams, pageIndex, pageSize) {
        const body = {
            ...searchParams,
            pageSize: pageSize,
            pageIndex: pageIndex
        };

        callApi((window.apiBaseUrl || '') + '/Quote/SearchInputQuote', body)
            .then(res => {
                if (!res) {
                    renderSupplierTable([]);
                    renderSupplierPagination(0, 1);
                    document.getElementById('supplierSummaryText').textContent = (window.i18nInputQuote.TotalPrefix || 'Tổng: ') + 0;
                    return;
                }
                const items = Array.isArray(res.data) ? res.data : [];
                const total = typeof res.totalCount === 'number' ? res.totalCount : items.length;
                supplierState.totalCount = total;
                supplierState.totalPages = total > 0 ? Math.ceil(total / pageSize) : 0;
                renderSupplierTable(items);
                renderSupplierPagination(supplierState.totalPages, pageIndex);
                document.getElementById('supplierSummaryText').textContent = (window.i18nInputQuote.TotalPrefix || 'Tổng: ') + total;
        })
            .catch(err => {
                showAlert('danger', (window.i18nInputQuote.SupplierDataLoadError || 'Lỗi tải dữ liệu nhà cung cấp: ') + err);
                renderSupplierTable([]);
                renderSupplierPagination(0, 1);
            });
    }

    // Render supplier table
    function renderSupplierTable(data) {
        const tbody = document.getElementById('supplierQuoteBody');
        tbody.innerHTML = '';

        if (!data || data.length === 0) {
            const row = document.createElement('tr');
            row.innerHTML = `<td colspan="24" class="text-center text-muted py-4">${window.i18nInputQuote.NoData || 'Không có dữ liệu'}</td>`;
            tbody.appendChild(row);
            return;
        }

        function isFalseFlag(v) {
            return v === false || v === 0;
        }

        data.forEach((item, index) => {
            const row = document.createElement('tr');

            // helper to create td with optional classes and text
            function td(text, className) {
                const c = document.createElement('td');
                if (className) c.className = className;
                c.textContent = text != null ? text : '';
                return c;
            }

            // 1 - STT
            row.appendChild(td(((supplierState.currentPage - 1) * supplierState.pageSize) + index + 1, 'text-center'));

            // 2 - Order number
            row.appendChild(td(item.CHR_MaDon || '', 'text-center'));

            // 3 - Supplier code
            row.appendChild(td(item.CHR_CodeNCC || ''));

            // 4 - Supplier name
            row.appendChild(td(item.NVCHR_NameNCC || ''));

            // 5 - Supplier item code -> IsMatch_MaHangNCC
            const maHangCell = td(item.CHR_MaHangNCC || '');
            if (isFalseFlag(item.IsMatch_MaHangNCC)) {
                maHangCell.classList.add('mismatch');
                maHangCell.style.color = 'red';
                maHangCell.style.border = '1px solid red';
            }
            row.appendChild(maHangCell);

            // 6 - Item name HQ -> IsMatch_NameVN
            const nameVNCell = td(item.NVCHR_TenHangHQ || '');
            if (isFalseFlag(item.IsMatch_NameVN)) {
                nameVNCell.classList.add('mismatch');
                nameVNCell.style.color = 'red';
                nameVNCell.style.border = '1px solid red';
            }
            row.appendChild(nameVNCell);

            // 7 - Quantity -> IsMatch_SoLuong
            const qtyCell = td(item.INT_SoLuong || '', 'text-center');
            if (isFalseFlag(item.IsMatch_SoLuong)) {
                qtyCell.classList.add('mismatch');
                qtyCell.style.color = 'red';
                qtyCell.style.border = '1px solid red';
            }
            row.appendChild(qtyCell);

            // 8 - Unit -> IsMatch_DonVi
            const unitCell = td(item.NVCHR_DonVi || '', 'text-center');
            if (isFalseFlag(item.IsMatch_DonVi)) {
                unitCell.classList.add('mismatch');
                unitCell.style.color = 'red';
                unitCell.style.border = '1px solid red';
            }
            row.appendChild(unitCell);

            // 9 - Price USD
            row.appendChild(td(item.FL_USD ? item.FL_USD.toFixed(2) : '', 'text-end'));

            // 10 - Price VND
            row.appendChild(td(item.FL_VND ? item.FL_VND.toFixed(2) : '', 'text-end'));

            // 11 - MOQ
            row.appendChild(td(item.NVCHR_MOQ || ''));

            // 12 - LeadTime
            row.appendChild(td(item.DTM_LeadTime || ''));

            // 13 - Delivery date -> IsMatch_Ngay
            const deliveryCell = td(item.DTM_ShipTime ? new Date(item.DTM_ShipTime).toLocaleDateString('vi-VN') : '');
            if (isFalseFlag(item.IsMatch_Ngay)) {
                deliveryCell.classList.add('mismatch');
                deliveryCell.style.color = 'red';
                deliveryCell.style.border = '1px solid red';
            }
            row.appendChild(deliveryCell);

            // 14 - Rohs -> IsMatch_Rohs
            const rohsCell = td(item.VCHR_Rohs || '');
            if (isFalseFlag(item.IsMatch_Rohs)) {
                rohsCell.classList.add('mismatch');
                rohsCell.style.color = 'red';
                rohsCell.style.border = '1px solid red';
            }
            row.appendChild(rohsCell);

            // 15 - COCQ -> IsMatch_COCQ
            const cocqCell = td(item.VCHR_COCQ || '');
            if (isFalseFlag(item.IsMatch_COCQ)) {
                cocqCell.classList.add('mismatch');
                cocqCell.style.color = 'red';
                cocqCell.style.border = '1px solid red';
            }
            row.appendChild(cocqCell);

            // 16 - MSDS -> IsMatch_MSDS
            const msdsCell = td(item.VCHR_MSDS || '');
            if (isFalseFlag(item.IsMatch_MSDS)) {
                msdsCell.classList.add('mismatch');
                msdsCell.style.color = 'red';
                msdsCell.style.border = '1px solid red';
            }
            row.appendChild(msdsCell);

            // 17 - Safety -> IsMatch_AnToan
            const safetyCell = td(item.VCHR_AnToan || '');
            if (isFalseFlag(item.IsMatch_AnToan)) {
                safetyCell.classList.add('mismatch');
                safetyCell.style.color = 'red';
                safetyCell.style.border = '1px solid red';
            }
            row.appendChild(safetyCell);

            // 18 - Commitment
            const commitCell = td(item.VCHR_CamKet || '');
            if (isFalseFlag(item.IsMatchCamKet)) {
                commitCell.classList.add('mismatch');
                commitCell.style.color = 'red';
                commitCell.style.border = '1px solid red';
            }
            row.appendChild(commitCell);

            // 19 - Delivery term
            row.appendChild(td(item.NVCHR_DeliveryTerm || ''));

            // 20 - Payment term
            row.appendChild(td(item.NVCHR_PaymentTerm || ''));

            // 21 - Attachment
            row.appendChild(td(item.NVCHR_File || ''));

            // 22 - DTM_EffectiveDate
            row.appendChild(td(formatDate(item.DTM_EffectiveDate) || ''));

            // 23 - DTM_ExpiryDate
            row.appendChild(td(formatDate(item.DTM_ExpiryDate) || ''));

            // 24 - Input time
            row.appendChild(td(formatDate(item.DTM_UpdateDate) || ''));

            tbody.appendChild(row);
        });
    }

    // Render supplier pagination
    function renderSupplierPagination(totalPages, currentPage) {
        const pagination = document.getElementById('supplierPaginationControls');
        pagination.innerHTML = '';

        if (totalPages <= 1) return;

        // Previous button
        const prevLi = document.createElement('button');
        prevLi.className = `btn btn-sm btn-outline-secondary ${currentPage <= 1 ? 'disabled' : ''}`;
        prevLi.innerHTML = `‹`;
        prevLi.addEventListener('click', function(e) {
            e.preventDefault();
            if (currentPage > 1) {
                supplierState.currentPage = currentPage - 1;
                loadSupplierQuotes(supplierState.searchParams, supplierState.currentPage, supplierState.pageSize);
            }
        });
        pagination.appendChild(prevLi);

        // Page numbers (1-based)
        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(totalPages, currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            const btn = document.createElement('button');
            btn.className = `btn btn-sm ${i === currentPage ? 'btn-primary' : 'btn-outline-secondary'}`;
            btn.textContent = i;
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                supplierState.currentPage = i;
                loadSupplierQuotes(supplierState.searchParams, supplierState.currentPage, supplierState.pageSize);
            });
            pagination.appendChild(btn);
        }

        // Next button
        const nextBtn = document.createElement('button');
        nextBtn.className = `btn btn-sm btn-outline-secondary ${currentPage >= totalPages ? 'disabled' : ''}`;
        nextBtn.innerHTML = `›`;
        nextBtn.addEventListener('click', function(e) {
            e.preventDefault();
            if (currentPage < totalPages) {
                supplierState.currentPage = currentPage + 1;
                loadSupplierQuotes(supplierState.searchParams, supplierState.currentPage, supplierState.pageSize);
            }
        });
        pagination.appendChild(nextBtn);

        // paging info
        const pagingInfo = document.getElementById('supplierPagingInfo');
        if (pagingInfo) {
            const start = (currentPage - 1) * supplierState.pageSize + 1;
            const end = Math.min(currentPage * supplierState.pageSize, supplierState.totalCount || totalPages * supplierState.pageSize);
            pagingInfo.textContent = `${start}-${end} / ${supplierState.totalCount || ''}`;
        }
    }
    function formatDate(d) {
        if (window.cmMomentFormat) { return window.cmMomentFormat(d); }
        if (!d) return '';
        const dt = new Date(d);
        if (isNaN(dt.getTime())) return '';
        const pad = n => n.toString().padStart(2, '0');
        return `${pad(dt.getDate())}/${pad(dt.getMonth() + 1)}/${dt.getFullYear()} - ${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
    }
    // Initialize
    function init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(init, 100);
                return;
            });
        }
        
        initializeElements();
        initializeEventListeners();
        initializeSupplierElements();
        initializeSupplierEventListeners();
        initializeTabSwitch();

        console.log('InputQuote module initialized');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        setTimeout(init, 100);
    }
    
    // Fallback tab switch handler in case Bootstrap's JS is not available
    function initializeTabSwitch() {
        const tabs = [
            { btnId: 'request-list-tab', paneId: 'request-list' },
            { btnId: 'supplier-input-tab', paneId: 'supplier-input' }
        ];

        function activate(tabBtn, paneId) {
            // nav links
            document.querySelectorAll('#inputQuoteTabs .nav-link').forEach(n => {
                n.classList.remove('active');
                n.setAttribute('aria-selected', 'false');
            });
            // panes
            document.querySelectorAll('#inputQuoteTabsContent .tab-pane').forEach(p => {
                p.classList.remove('show', 'active');
            });

            const btn = document.getElementById(tabBtn);
            const pane = document.getElementById(paneId);
            if (btn) {
                btn.classList.add('active');
                btn.setAttribute('aria-selected', 'true');
            }
            if (pane) {
                pane.classList.add('show', 'active');
            }
            // If supplier tab activated, load its data automatically
            if (paneId === 'supplier-input') {
                supplierState.searchParams = {
                    idRequestQuote: 0,
                    maDon: document.getElementById('supplierSearchMaDon')?.value || '',
                    maVatTu: document.getElementById('supplierSearchMaVatTu')?.value || '',
                    maNcc: document.getElementById('supplierSearchMaNcc')?.value || '',
                    section: document.getElementById('supplierSearchSection')?.value || '',
                    dayMM: document.getElementById('supplierSearchDayMM')?.value || null
                };
                supplierState.currentPage = 1;
                loadSupplierQuotes(supplierState.searchParams, supplierState.currentPage, supplierState.pageSize);
            }
        }

        tabs.forEach(t => {
            const el = document.getElementById(t.btnId);
            if (!el) return;
            // ensure click toggles tab even if bootstrap is missing
            el.addEventListener('click', function (e) {
                // let bootstrap handle if present (no preventDefault)
                // but also activate manually after tiny delay to avoid race
                setTimeout(() => activate(t.btnId, t.paneId), 0);
            });
        });
    }
})();
