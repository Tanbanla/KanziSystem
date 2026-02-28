// InputQuote.js - JavaScript cho màn hình nhập thông tin báo giá
(function() {
    'use strict';

    // State management
    let quoteState = {
        pageIndex: 1, // 1-based index for server API
        pageSize: 10,
        returnedCount: 0,
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
        callApi('/Quote/SearchInputQuoteBySoDon', body)
            .then(data => {
                // data is expected to be an array for the requested page
                if (!data) return;
                const items = Array.isArray(data) ? data : [];
                quoteState.returnedCount = items.length;
                quoteState.lastPage = items.length < quoteState.pageSize;
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
                <td class="text-center">${formatDate(item.DTM_CreateDate)}</td>
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

        // summary shows returned items count for current query
        elements.summaryText.textContent = window.i18nInputQuote.SummaryFormat.replace('{0}', quoteState.returnedCount);
        renderPaginationControls();
    }

    function renderPaginationControls() {
        const container = elements.paginationControls;
        if (!container) return;
        container.innerHTML = '';

        const createButton = (text, cls, disabled, handler) => {
            const b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-sm ' + cls;
            b.textContent = text;
            if (disabled) b.disabled = true;
            if (handler) b.addEventListener('click', handler);
            return b;
        };
        // Prev
        container.appendChild(createButton('‹', 'btn-outline-secondary', quoteState.pageIndex <= 1, () => { goToPage(quoteState.pageIndex - 1); }));

        // current page indicator
        const pageIndicator = document.createElement('span');
        pageIndicator.className = 'btn btn-sm btn-outline-secondary disabled';
        pageIndicator.textContent = `${quoteState.pageIndex}`;
        container.appendChild(pageIndicator);

        // Next
        container.appendChild(createButton('›', 'btn-outline-secondary', quoteState.lastPage || quoteState.returnedCount === 0, () => { goToPage(quoteState.pageIndex + 1); }));

        // paging info (start-end)
        if (elements.pagingInfo) {
            const startOne = quoteState.returnedCount === 0 ? 0 : ((quoteState.pageIndex - 1) * quoteState.pageSize + 1);
            const endOne = quoteState.returnedCount === 0 ? 0 : ((quoteState.pageIndex - 1) * quoteState.pageSize + quoteState.returnedCount);
            elements.pagingInfo.textContent = `${startOne}-${endOne}`;
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
        window.location.href = `/Quote/InputQuoteDetail?maDon=${item.CHR_MaDon}`;
    }
    // Download sample Excel file
    function exportSampleExcel() {
        const url = '/template/ExportSampleExcel.xlsx';
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
    function formatDate(dateStr) {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toLocaleDateString('vi-VN');
    }

    function showAlert(type, message) {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        alertDiv.innerHTML = `${message}`;
        document.body.appendChild(alertDiv);
        setTimeout(() => alertDiv.remove(), 5000);
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
        
        console.log('InputQuote module initialized');
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
})();
// - màn hình xử lý trường hợp k điền

// - phê duyệt thêm k lấy báo giá 

// - chỉnh lại giao diện theo đơn
// - mail sửa lại
// - lead time là số ngày, cam kết bắt buộc phải điền, sửa tiêu đề cột điều điện 
// - file đính kèm cần lưu (bắt buộc)
// - điều kiện đơn giá k âm
// - tổng hợp theo số đơn yêu cầu, lưu lịch sử báo giá.
// - màn hình lịch sử xác nhận lại thông tin
// - màn hình xác nhận tên sửa lại hiển thông tin để xác nhận tên, tổng hợp theo mã tên mới, thêm chức năng nhập xuất bằng excel
// - xử lý excel màn hình nhập báo giá, xử lý loading và updata file đã xử lý