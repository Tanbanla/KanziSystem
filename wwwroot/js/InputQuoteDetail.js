// InputQuoteDetail.js - JavaScript cho màn hình chi tiết nhập báo giá
(function () {
    'use strict';

    // State management
    let quoteState = {
        currentMaDon: null,
        items: [],
        exchangeRate: 24500,
        totalUSD: 0,
        totalVND: 0,
        isDirty: false,
        requestData: [] // Store request data for comparison
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
            pageRegency: document.getElementById('pageRegency'),
            exchangeRateInput: document.getElementById('exchangeRateInput')
        };
    }

    // Event Listeners
    function initializeEventListeners() {
        // Button events
        document.getElementById('pageSave')?.addEventListener('click', saveQuote);
        document.getElementById('pageSendMail')?.addEventListener('click', SendMail);
        document.getElementById('pageSearch')?.addEventListener('click', SearchEvent);
        document.getElementById('btnInputExcel')?.addEventListener('click', btnInputExcel);

        // Exchange rate change
        elements.exchangeRateInput?.addEventListener('input', updateExchangeRate);

        // Set initial exchange rate
        updateExchangeRate();

        // Load initial data
        loadDetailData();
    }

    // Load detail data for the current request
    async function loadDetailData() {
        // Load request details
        await callApi((window.apiBaseUrl || '') + '/InputQuote/SearchBaoGia', {
            idRequestQuote: 0,
            maDon: window.inputQuoteDetailData?.maDon,
            MaHang: document.getElementById('pageInternalItemCodeSelect')?.value || '',
            MaNcc: document.getElementById('pageSupplierSelect')?.value || '',
            section: '',
            dayMM: null,
            pageSize: 1000,
            pageIndex: 0,
            ChungLoai: document.getElementById('pageCategorySelect')?.value || '',
        })
            .then(data => {
                if (data && Array.isArray(data.data)) {
                    quoteState.requestData = data.data; // Store for comparison
                    renderDetailTable(data.data);
                }
            })
            .catch(err => showAlert('danger', window.i18nInputQuoteDetail.ErrorLoadingDetails + err));

        // Load detail items
        callApi((window.apiBaseUrl || '') + '/InputQuote/SearchInputQuote', {
            idRequestQuote: 0,
            maDon: window.inputQuoteDetailData?.maDon,
            maVatTu: document.getElementById('pageInternalItemCodeSelect')?.value || '',
            maNcc: document.getElementById('pageSupplierSelect')?.value || '',
            section: '',
            dayMM: null,
            pageSize: 1000,
            pageIndex: 0
        })
            .then(data => {
                if (data.data && Array.isArray(data.data)) {
                    renderQuoteInputTable(data.data);
                }
            })
            .catch(err => showAlert('danger', window.i18nInputQuoteDetail.ErrorLoadingDetails + err));
    }
    // Render detail table
    function renderDetailTable(data) {
        const tbody = elements.pageDetailBody;
        tbody.innerHTML = '';

        data.forEach((item, index) => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td class="text-center">${index + 1}</td>
                <td class="text-center">${item.chR_MaHangNCC || ''}</td>
                <td class="text-center">${item.chR_MaHangNoiBo || ''}</td>
                <td class="text-center">${item.chR_Phanloai || ''}</td>
                <td class="text-center">${item.nvchR_ChungLoai || ''}</td>
                <td>${item.nvchR_NameVN || ''}</td>
                <td>${item.chR_NameEN || ''}</td>
                <td class="text-center">${item.inT_SoLuong || 0}</td>
                <td class="text-center">${item.nvchR_DonVi || ''}</td>
                <td>${item.nvchR_HinhDang || ''}</td>
                <td>${item.nvchR_ChatLieu || ''}</td>
                <td>${item.nvchR_ThanhPhan || ''}</td>
                <td>${item.nvchR_KichThuoc || ''}</td>
                <td>${item.nvchR_DongMay || ''}</td>
                <td>${item.nvchR_TinhNang || ''}</td>
                <td>${item.nvchR_File || ''}</td>
                <td>${item.nvchR_TenNCC || ''}</td>
                <td class="text-center">${formatDate(item.dtM_KyHan)}</td>
                <td class="text-center">${item.chR_Gap === 'true' ? 'Yes' : 'No'}</td>
                <td class="text-center">${item.biT_LayBaoGia ? 'Yes' : 'No'}</td>
                <td>${item.nvchR_LyDo || ''}</td>
                <td hidden >${item.id || ''}</td>
                <td hidden >${item.chR_MaNCC || ''}</td>
                <td hidden >${item.nvchR_TenNCC || ''}</td>
                <td hidden >${item.biT_IsTemplate || false}</td>
            `;
            tbody.appendChild(row);
        });
    }

    // Render quote input table
    function renderQuoteInputTable(data = []) {
        const tbody = elements.quoteInputBody;
        tbody.innerHTML = '';

        data.forEach((item, index) => {
            // Find matching request item
            const requestItem = quoteState.requestData.find(r => r.id === item.ID_RequestQuote);

            // Helper to check mismatch and add class
            const getClassIfMismatch = (value, requestValue, compareFunc = (a, b) => a !== b) => {
                if (!requestItem || requestValue === undefined || requestValue === null) return '';
                return compareFunc(value, requestValue) ? 'highlight-yellow' : '';
            };

            const row = document.createElement('tr');
            row.innerHTML = `
                <td hidden>${item.ID || 0}</td>
                <td class="text-center">${index + 1}</td>
                <td>${item.NVCHR_NameNCC || ''}</td>
                <td>${item.CHR_MaHangNoiBo || ''}</td>
                <td><input type="text" class="form-control form-control-sm" value="${item.CHR_MaHangNCC || ''}" placeholder="${window.i18nInputQuoteDetail.SupplierCode}"></td>
                <td><input type="text" class="form-control form-control-sm ${getClassIfMismatch(item.NVCHR_TenHangHQ, requestItem?.nvchR_NameVN)}" value="${item.NVCHR_TenHangHQ || ''}"></td>
                <td><input type="number" class="form-control form-control-sm ${getClassIfMismatch(item.INT_SoLuong, requestItem?.inT_SoLuong)}" value="${item.INT_SoLuong || 0}" step="1" min="0"></td>
                <td><input type="text" class="form-control form-control-sm ${getClassIfMismatch(item.NVCHR_DonVi, requestItem?.nvchR_DonVi)}" value="${item.NVCHR_DonVi || ''}"></td>
                <td><input type="number" class="form-control form-control-sm price-usd" value="${item.FL_USD || 0}" step="1" min="0"></td>
                <td><input type="number" class="form-control form-control-sm price-vnd" value="${item.FL_VND || 0}" readonly></td>
                <td><input type="number" class="form-control form-control-sm ${getClassIfMismatch(item.NVCHR_MOQ, item.INT_SoLuong, (moq, qty) => parseFloat(moq || 0) > parseFloat(qty || 0))}" value="${item.NVCHR_MOQ || ''}" step="1" min="0" placeholder="MOQ"></td>
                <td><input type="number" class="form-control form-control-sm" value="${item.DTM_LeadTime || ''}" step="1" min="0" placeholder="Lead Time"></td>
                <td><input type="date" class="form-control form-control-sm ${getClassIfMismatch(item.DTM_ShipTime, requestItem?.dtM_KyHan, (ship, kyhan) => ship && kyhan && new Date(ship) > new Date(kyhan))}" value="${item.DTM_ShipTime ? new Date(item.DTM_ShipTime).toISOString().split('T')[0] : ''}"></td>
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
                        <option value="OK" ${item.VCHR_COCQ === 'OK' ? 'selected' : ''}>OK</option>
                        <option value="NG" ${item.VCHR_COCQ === 'NG' ? 'selected' : ''}>NG</option>
                        <option value="No Need" ${item.VCHR_COCQ === 'No Need' ? 'selected' : ''}>No Need</option>
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
                        <option value="Đồng ý (accept)" ${item.VCHR_CamKet === 'Đồng ý (accept)' ? 'selected' : ''}>Đồng ý (accept)</option>
                        <option value="Không đồng ý (not accept)" ${item.VCHR_CamKet === 'Không đồng ý (not accept)' ? 'selected' : ''}>Không đồng ý (not accept)</option>
                    </select>
                </td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_DeliveryTerm || ''}" placeholder="${window.i18nInputQuoteDetail.DeliveryMethod}"></td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_PaymentTerm || ''}" placeholder="${window.i18nInputQuoteDetail.PaymentCondition}"></td>
                <td><input type="text" class="form-control form-control-sm" value="${item.NVCHR_File || ''}" placeholder="${window.i18nInputQuoteDetail.Attachment}"></td>
                <td><input type="date" class="form-control form-control-sm" value="${item.DTM_EffectiveDate ? new Date(item.DTM_EffectiveDate).toISOString().split('T')[0] : ''}"></td>
                <td><input type="date" class="form-control form-control-sm" value="${item.DTM_ExpiryDate ? new Date(item.DTM_ExpiryDate).toISOString().split('T')[0] : ''}"></td>
            `;
            tbody.appendChild(row);

            // Get select elements for mismatch highlighting
            const rohsSelect = row.cells[13].querySelector('select');
            const cocqSelect = row.cells[14].querySelector('select');
            const msdsSelect = row.cells[15].querySelector('select');
            const anToanSelect = row.cells[16].querySelector('select');
            const camKetSelect = row.cells[17].querySelector('select');

            // Function to update highlight for selects
            const updateSelectHighlight = (select, compareValue) => {
                select.classList.remove('highlight-yellow');
                if ((select.value === 'NG' || select.value === 'No Need') && select.value !== compareValue) {
                    select.classList.add('highlight-yellow');
                }
            };
            //  COCQ: nếu chọn CO hoặc CQ mà giá trị request không phải OK thì highlight
            const updateCocqHighlight = (select, compareValue) => {
                select.classList.remove('highlight-yellow');
                if ((select.value === 'NG' || select.value === 'No Need') && compareValue !== '') {
                    select.classList.add('highlight-yellow');
                }
            };
            // Cam kết: nếu chọn NG mà giá trị request không phải Đồng ý (accept) thì highlight
            const updateCamKetHighlight = (select) => {
                select.classList.remove('highlight-yellow');
                if (select.value !== 'Đồng ý (accept)') {
                    select.classList.add('highlight-yellow');
                }
            };
            // Initial highlight
            updateSelectHighlight(rohsSelect, requestItem?.nvchR_Rohs);
            updateCocqHighlight(cocqSelect, requestItem?.nvchR_Cocq);
            updateSelectHighlight(msdsSelect, requestItem?.nvchR_Msds);
            updateSelectHighlight(anToanSelect, requestItem?.nvchR_AnToan);
            updateCamKetHighlight(camKetSelect);

            // Attach event listeners for select changes
            rohsSelect.addEventListener('change', () => updateSelectHighlight(rohsSelect, requestItem?.nvchR_Rohs));
            cocqSelect.addEventListener('change', () => updateCocqHighlight(cocqSelect, requestItem?.nvchR_COCQ));
            msdsSelect.addEventListener('change', () => updateSelectHighlight(msdsSelect, requestItem?.nvchR_MSDS));
            anToanSelect.addEventListener('change', () => updateSelectHighlight(anToanSelect, requestItem?.nvchR_AnToan));
            camKetSelect.addEventListener('change', () => updateCamKetHighlight(camKetSelect));

            // Attach event listeners for price calculation
            const usdInput = row.querySelector('.price-usd');
            const vndInput = row.querySelector('.price-vnd');
            usdInput.addEventListener('input', () => {
                const val = parseFloat(usdInput.value) || 0;
                if (val <= 0) {
                    showAlert('warning', window.i18nInputQuoteDetail.PriceMustBeGreaterThanZero);
                    usdInput.value = 0;
                }
                vndInput.value = (parseFloat(usdInput.value) || 0) * quoteState.exchangeRate;
            });
            // Set initial VND value
            vndInput.value = (parseFloat(usdInput.value) || 0) * quoteState.exchangeRate;
        });
    }

    // Update exchange rate and recalculate VND prices
    function updateExchangeRate() {
        const newRate = parseFloat(elements.exchangeRateInput.value) || 24500;
        quoteState.exchangeRate = newRate;
        // Recalculate all VND prices
        const usdInputs = document.querySelectorAll('.price-usd');
        usdInputs.forEach(usd => {
            const vndInput = usd.closest('td').nextElementSibling.querySelector('.price-vnd');
            if (vndInput) {
                vndInput.value = (parseFloat(usd.value) || 0) * newRate;
            }
        });
    }

    // Save quote
    function saveQuote() {
        //const supplier = elements.pageSupplierSelect.value;
        //if (!supplier) {
        //    showAlert('warning', 'Vui lòng chọn nhà cung cấp');
        //    return;
        //}

        const items = collectQuoteItems();
        // Call API to save
        callApi((window.apiBaseUrl || '') + '/InputQuote/UpdateQuoteDetail', items)
            .then(data => {
                showAlert('success', window.i18nInputQuoteDetail.DataFilteredSuccessfully);
            })
            .catch(err => showAlert('danger', window.i18nInputQuoteDetail.SaveError + err));
    }

    // Send mail quote
    function SendMail() {
        //const supplier = elements.pageSupplierSelect.value;
        //if (!supplier) {
        //    showAlert('warning', 'Vui lòng chọn nhà cung cấp trước khi gửi mail');
        //    return;
        //}

        // Collect data from pageDetailBody
        const rows = elements.pageDetailBody.querySelectorAll('tr');
        if (rows.length === 0) {
            showAlert('warning', window.i18nInputQuoteDetail.NoDataToSendMail);
            return;
        }

        const items = [];
        rows.forEach(row => {
            const cells = row.querySelectorAll('td');
            if (cells.length < 20) return; // Skip if not enough cells
            //if (cells[24].textContent.trim() == 'true' || cells[24].textContent.trim() == '1')  return;
            const item = {
                CHR_MaHangNoiBo: cells[2].textContent.trim() || '',
                CHR_MaHangNCC: cells[1].textContent.trim() || '',
                NVCHR_TenHangHQ: cells[5].textContent.trim() || '',
                NVCHR_DonVi: cells[8].textContent.trim() || '',
                INT_SoLuong: parseInt(cells[7].textContent.trim()) || 0,
                FL_USD: 0,
                FL_VND: 0,
                NVCHR_MOQ: '',
                DTM_LeadTime: '',
                DTM_ShipTime: null,
                VCHR_Rohs: '',
                VCHR_COCQ: '',
                VCHR_MSDS: '',
                VCHR_AnToan: '',
                VCHR_CamKet: '',
                NVCHR_DeliveryTerm: '',
                NVCHR_PaymentTerm: '',
                NVCHR_File: '',
                CHR_CodeNCC: cells[22].textContent.trim() || '',
                DTM_CreateDate: new Date().toISOString().split('T')[0],
                CHR_CreateBy: window.inputQuoteDetailData?.user,
                ID_RequestQuote: parseInt(cells[21].textContent.trim()) || 0,
                NVCHR_NameNCC: cells[23].textContent.trim() || '',

                //                    parseInt(elements.pageIdRequest.textContent) || 0
            };
            items.push(item);
        });

        if (items.length === 0) {
            showAlert('warning', window.i18nInputQuoteDetail.NoValidDataToSendMail);
            return;
        }
        var body = {
            MaDon: window.inputQuoteDetailData?.maDon,
            baoGiaDetail: items
        };
        // Call API to insert
        callApi((window.apiBaseUrl || '') + '/InputQuote/InsertInputQuote', body)
            .then(data => {
                showAlert('success', window.i18nInputQuoteDetail.SendMailSuccess);
                // Reload the quote input table
                loadDetailData();
            })
            .catch(err => showAlert('danger', window.i18nInputQuoteDetail.SendMailError + err));
    }
    // Search event
    function SearchEvent() {
        // Load initial data
        loadDetailData();
    }
    // Button input excel
    async function btnInputExcel() {

    }
    // Collect quote items from table
    function collectQuoteItems() {
        const rows = elements.quoteInputBody.querySelectorAll('tr');
        const items = [];

        rows.forEach(row => {
            const inputs = row.querySelectorAll('input, select');
            items.push({
                ID: parseInt(row.cells[0].textContent) || 0,
                //CHR_MaHangNoiBo: row.cells[1].textContent || '',
                CHR_MaHangNCC: inputs[0]?.value || '',
                NVCHR_TenHangHQ: inputs[1]?.value || '',
                INT_SoLuong: parseFloat(inputs[2]?.value) || 0,
                NVCHR_DonVi: inputs[3]?.value || '',
                FL_USD: parseFloat(inputs[4]?.value) || 0,
                FL_VND: parseFloat(inputs[5]?.value) || 0,
                NVCHR_MOQ: inputs[6]?.value || '',
                DTM_LeadTime: inputs[7]?.value || '',
                DTM_ShipTime: inputs[8]?.value || null,
                VCHR_Rohs: inputs[9]?.value || '',
                VCHR_COCQ: inputs[10]?.value || '',
                VCHR_MSDS: inputs[11]?.value || '',
                VCHR_AnToan: inputs[12]?.value || '',
                VCHR_CamKet: inputs[13]?.value || '',
                NVCHR_DeliveryTerm: inputs[14]?.value || '',
                NVCHR_PaymentTerm: inputs[15]?.value || '',
                NVCHR_File: inputs[16]?.value || '',
                DTM_EffectiveDate: inputs[17]?.value || null,
                DTM_ExpiryDate: inputs[18]?.value || null,
                CHR_UpdateBy: window.inputQuoteDetailData?.user,
                //DTM_QuoteDate: elements.pageQuoteDate.value,
                //DTM_ValidUntil: elements.pageValidUntil.value,
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
        alertDiv.innerHTML = `${message}`;
        //<button type="button" class="btn-close" data-bs-dismiss="alert"></button>
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
            search.innerHTML = '<input type="text" placeholder="' + (window.i18nInputQuoteDetail.SearchPlaceholder || 'Tìm...') + '" />';
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
                    const empty = document.createElement('div'); empty.className = 'ms-empty'; empty.textContent = window.i18nInputQuoteDetail.NoResults;
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
                    placeholderEl.textContent = window.i18nInputQuoteDetail.SelectPlaceholder;
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
            document.addEventListener('DOMContentLoaded', function () {
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
