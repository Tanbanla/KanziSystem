// InputQuoteDetail.js - JavaScript cho màn hình chi tiết nhập báo giá
(function() {
    'use strict';

    // State management
    let quoteState = {
        currentMaDon: null,
        items: [],
        exchangeRate: 24500,
        totalUSD: 0,
        totalVND: 0,
        isDirty: false
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
            pageMaDon: document.getElementById('pageMaDon'),
            pageKhoi: document.getElementById('pageKhoi'),
            pageMpbyc: document.getElementById('pageMpbyc'),
            pageTenphongban: document.getElementById('pageTenphongban'),
            pageNyc: document.getElementById('pageNyc'),
            pageThmm: document.getElementById('pageThmm'),
            pageUrgentBadge: document.getElementById('pageUrgentBadge'),
            pageSupplierSelect: document.getElementById('pageSupplierSelect'),
            pageQuoteDate: document.getElementById('pageQuoteDate'),
            pageValidUntil: document.getElementById('pageValidUntil'),
            pageDetailBody: document.getElementById('pageDetailBody'),
            quoteInputBody: document.getElementById('quoteInputBody'),
            pageIdRequest: document.getElementById('pageIdRequest'),
            pageStep: document.getElementById('pageStep'),
            pageRegency: document.getElementById('pageRegency')
        };
    }

    // Event Listeners
    function initializeEventListeners() {
        // Button events
        document.getElementById('pageSave')?.addEventListener('click', saveQuote);
        document.getElementById('pageSendMail')?.addEventListener('click', pageSendMail);
        
        // Load initial data
        loadDetailData();
    }

    // Load detail data for the current request
    function loadDetailData() {
        const listRq = window.inputQuoteDetailData?.listRequest;
        if (!listRq) return;

        // Load request details
        renderDetailTable(listRq);
        // Load detail items
        callApi('/Quote/SearchInputQuote', {
            idRequestQuote: 0,
            maDon: window.inputQuoteDetailData?.maDon,
            maVatTu: '',
            maNcc: '',
            section: '',
            dayMM: null,
            pageSize: 1000,
            pageIndex: 0
        })
        .then(data => {
            if (data && Array.isArray(data)) {
                renderQuoteInputTable(data);
            }
        })
        .catch(err => showAlert('danger', 'Lỗi tải chi tiết: ' + err));
    }
    // Render detail table
    function renderDetailTable(data) {
        const tbody = elements.pageDetailBody;
        tbody.innerHTML = '';
        
        data.forEach((item, index) => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td class="text-center">${index + 1}</td>
                <td class="text-center">${item.CHR_MaHangNCC || ''}</td>
                <td class="text-center">${item.CHR_MaHangNoiBo || ''}</td>
                <td class="text-center">${item.NVCHR_ChungLoai || ''}</td>
                <td>${item.NVCHR_NameVN || ''}</td>
                <td>${item.CHR_NameEN || ''}</td>
                <td class="text-center">${item.INT_SoLuong || 0}</td>
                <td class="text-center">${item.NVCHR_DonVi || ''}</td>
                <td>${item.NVCHR_HinhDang || ''}</td>
                <td>${item.NVCHR_ChatLieu || ''}</td>
                <td>${item.NVCHR_ThanhPhan || ''}</td>
                <td>${item.NVCHR_KichThuoc || ''}</td>
                <td>${item.NVCHR_DongMay || ''}</td>
                <td>${item.NVCHR_TinhNang || ''}</td>
                <td>${item.NVCHR_File || ''}</td>
                <td>${item.NVCHR_TenNCC || ''}</td>
                <td class="text-center">${formatDate(item.DTM_KyHan)}</td>
                <td class="text-center">${item.CHR_Gap === 'true' ? 'Yes' : 'No'}</td>
                <td class="text-center">${item.BIT_LayBaoGia ? 'Yes' : 'No'}</td>
                <td>${item.NVCHR_LyDo || ''}</td>
            `;
            tbody.appendChild(row);
        });
    }

    // Render quote input table
    function renderQuoteInputTable(data) {
        const tbody = elements.quoteInputBody;
        tbody.innerHTML = '';
        
        data.forEach((item, index) => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td class="text-center">${index + 1}</td>
                <td>${item.CHR_MaHangNoiBo || ''}</td>
                <td><input type="text" class="form-control form-control-sm" value="${item.CHR_MaHangNCC || ''}" placeholder="Mã hàng NCC"></td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_TenHangHQ || ''}"></td>
                <td><input type="number" class="form-control form-control-sm" value="${item.INT_SoLuong || 0}"></td>
                <td><input type="number" class="form-control form-control-sm price-usd" value="${item.FL_USD || 0}" step="0.01"></td>
                <td><input type="number" class="form-control form-control-sm price-vnd" value="${item.FL_VND || 0}" readonly></td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_MOQ || ''}" placeholder="MOQ"></td>
                <td><input type="text" class="form-control form-control-sm" value="${item.DTM_LeadTime || ''}" placeholder="Lead Time"></td>
                <td><input type="date" class="form-control form-control-sm" value="${item.DTM_ShipTime ? new Date(item.DTM_ShipTime).toISOString().split('T')[0] : ''}"></td>
                <td>
                    <select class="form-control form-control-sm">
                        <option value="">--</option>
                        <option value="OK" ${item.VCHR_Rohs === 'OK' ? 'selected' : ''}>OK</option>
                        <option value="NG" ${item.VCHR_Rohs === 'NG' ? 'selected' : ''}>NG</option>
                        <option value="No Need" ${item.VCHR_Rohs === 'No Need' ? 'selected' : ''}>No Need</option>
                    </select>
                </td>
                <td>
                    <select class="form-control form-control-sm">
                        <option value="">--</option>
                        <option value="CO" ${item.VCHR_COCQ === 'CO' ? 'selected' : ''}>CO</option>
                        <option value="CQ" ${item.VCHR_COCQ === 'CQ' ? 'selected' : ''}>CQ</option>
                        <option value="CO&CQ" ${item.VCHR_COCQ === 'CO&CQ' ? 'selected' : ''}>CO&CQ</option>
                    </select>
                </td>
                <td>
                    <select class="form-control form-control-sm">
                        <option value="">--</option>
                        <option value="OK" ${item.VCHR_MSDS === 'OK' ? 'selected' : ''}>OK</option>
                        <option value="NG" ${item.VCHR_MSDS === 'NG' ? 'selected' : ''}>NG</option>
                        <option value="No Need" ${item.VCHR_MSDS === 'No Need' ? 'selected' : ''}>No Need</option>
                    </select>
                </td>
                <td>
                    <select class="form-control form-control-sm">
                        <option value="">--</option>
                        <option value="OK" ${item.VCHR_AnToan === 'OK' ? 'selected' : ''}>OK</option>
                        <option value="NG" ${item.VCHR_AnToan === 'NG' ? 'selected' : ''}>NG</option>
                        <option value="No Need" ${item.VCHR_AnToan === 'No Need' ? 'selected' : ''}>No Need</option>
                    </select>
                </td>
                <td>
                    <select class="form-control form-control-sm">
                        <option value="">--</option>
                        <option value="OK" ${item.VCHR_CamKet === 'OK' ? 'selected' : ''}>OK</option>
                        <option value="NG" ${item.VCHR_CamKet === 'NG' ? 'selected' : ''}>NG</option>
                        <option value="No Need" ${item.VCHR_CamKet === 'No Need' ? 'selected' : ''}>No Need</option>
                    </select>
                </td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_DeliveryTerm || ''}" placeholder="Phương thức giao"></td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_PaymentTerm || ''}" placeholder="Điều kiện"></td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_File || ''}" placeholder="Link file"></td>
            `;
            tbody.appendChild(row);
            
            // Attach event listeners for price calculation
            const usdInput = row.querySelector('.price-usd');
            const vndInput = row.querySelector('.price-vnd');
            usdInput.addEventListener('input', () => {
                vndInput.value = (parseFloat(usdInput.value) || 0) * quoteState.exchangeRate;
            });
        });
    }

    // Save quote
    function saveQuote() {
        const supplier = elements.pageSupplierSelect.value;
        if (!supplier) {
            showAlert('warning', 'Vui lòng chọn nhà cung cấp');
            return;
        }
        
        const items = collectQuoteItems();
        // Call API to save
        callApi('/Quote/InsertInputQuote', items)
            .then(data => {
                showAlert('success', 'Đã lưu báo giá thành công!');
            })
            .catch(err => showAlert('danger', 'Lỗi lưu: ' + err));
    }

    // Submit quote
    function pageSendMail() {
        
    }

    // Collect quote items from table
    function collectQuoteItems() {
        const rows = elements.quoteInputBody.querySelectorAll('tr');
        const items = [];
        
        rows.forEach(row => {
            const inputs = row.querySelectorAll('input, select');
            items.push({
                CHR_MaHangNoiBo: row.cells[1].textContent || '',
                CHR_MaHangNCC: inputs[0]?.value || '',
                NVCHR_TenHangHQ: inputs[1]?.value || '',
                INT_SoLuong: parseFloat(inputs[2]?.value) || 0,
                FL_USD: parseFloat(inputs[3]?.value) || 0,
                FL_VND: parseFloat(inputs[4]?.value) || 0,
                NVCHR_MOQ: inputs[5]?.value || '',
                DTM_LeadTime: inputs[6]?.value || '',
                DTM_ShipTime: inputs[7]?.value || null,
                VCHR_Rohs: inputs[8]?.value || '',
                VCHR_COCQ: inputs[9]?.value || '',
                VCHR_MSDS: inputs[10]?.value || '',
                VCHR_AnToan: inputs[11]?.value || '',
                VCHR_CamKet: inputs[12]?.value || '',
                NVCHR_DeliveryTerm: inputs[13]?.value || '',
                NVCHR_PaymentTerm: inputs[14]?.value || '',
                NVCHR_File: inputs[15]?.value || '',
                CHR_MaNCC: elements.pageSupplierSelect.value,
                DTM_QuoteDate: elements.pageQuoteDate.value,
                DTM_ValidUntil: elements.pageValidUntil.value,
                ID_RequestQuote: window.inputQuoteDetailData?.currentRequestId || 0
            });
        });
        
        return items;
    }

    // Go back to previous page
    function goBack() {
        window.history.back();
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
        alertDiv.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
        document.body.appendChild(alertDiv);
        setTimeout(() => alertDiv.remove(), 5000);
    }

    // Tìm kiếm - support both jQuery and plain DOM
    function buildSearchableDropdown(container) {
        // Similar to InputQuote.js
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
        
        console.log('InputQuoteDetail module initialized');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        setTimeout(init, 100);
    }
})();