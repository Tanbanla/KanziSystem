// InputQuote.js - JavaScript cho màn hình nhập thông tin báo giá
(function() {
    'use strict';

    // State management
    let quoteState = {
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
            quoteRequestCode: document.getElementById('quoteRequestCode'),
            supplierSelect: document.getElementById('supplierSelect'),
            quoteDate: document.getElementById('quoteDate'),
            validUntil: document.getElementById('validUntil'),
            quoteNote: document.getElementById('quoteNote'),
            exchangeRate: document.getElementById('exchangeRate'),
            paymentTerms: document.getElementById('paymentTerms'),
            termsConditions: document.getElementById('termsConditions'),
            quoteItemsBody: document.getElementById('quoteItemsBody'),
            totalUSD: document.getElementById('totalUSD'),
            totalVND: document.getElementById('totalVND'),
            itemCount: document.getElementById('itemCount'),
            addProductModal: null, // Not needed for jQuery modal
            availableProductsBody: document.getElementById('availableProductsBody')
        };
        
        // No need to initialize Bootstrap modal, using jQuery instead
    }

    // Event Listeners
    function initializeEventListeners() {
        // Button events
        const btnAdd = document.getElementById('btnAddProduct'); if (btnAdd) btnAdd.addEventListener('click', showAddProductModal);
        const btnConfirm = document.getElementById('btnConfirmAddProducts'); if (btnConfirm) btnConfirm.addEventListener('click', addSelectedProducts);
        const btnImport = document.getElementById('btnImportExcel'); if (btnImport) btnImport.addEventListener('click', importFromExcel);
        const btnSave = document.getElementById('btnSaveQuote'); if (btnSave) btnSave.addEventListener('click', saveQuote);
        const btnSaveDraftEl = document.getElementById('btnSaveDraft'); if (btnSaveDraftEl) btnSaveDraftEl.addEventListener('click', saveDraft);
        const btnSubmit = document.getElementById('btnSubmitQuote'); if (btnSubmit) btnSubmit.addEventListener('click', submitQuote);
        const btnCancelEl = document.getElementById('btnCancel'); if (btnCancelEl) btnCancelEl.addEventListener('click', cancelQuote);
        const btnReset = document.getElementById('btnReset'); if (btnReset) btnReset.addEventListener('click', resetFilters);
        const btnApplyFilters = document.getElementById('btnApplyFilters'); if (btnApplyFilters) btnApplyFilters.addEventListener('click', searchInputQuote);
        const btnDialogSearch = document.getElementById('btnDialogSearch'); if (btnDialogSearch) btnDialogSearch.addEventListener('click', searchQuoteRequestDialog);
        // Close product panel
        const closePanelBtn = document.getElementById('btnCloseProductPanel');
        if (closePanelBtn) closePanelBtn.addEventListener('click', function() {
            setProductPanelVisible(false);
        });
        // Close when clicking overlay
        const overlayEl = document.getElementById('productPanelOverlay');
        if (overlayEl) overlayEl.addEventListener('click', function() {
            setProductPanelVisible(false);
        });
    }
    // Reset filters and clear current quote items
    function resetFilters() {
        // Clear selects and inputs used as filters
        const fields = ['quoteRequestCode', 'supplierSelect', 'searchPhongBan', 'searchMaVatTu', 'modalRqCode', 'modalInternalCode', 'modalSection', 'modalSupplier', 'searchMaVatTu'];
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

        // Clear quote items table and state
        if (elements.quoteItemsBody) elements.quoteItemsBody.innerHTML = '';
        quoteState.items = [];
        quoteState.totalUSD = 0;
        quoteState.totalVND = 0;
        quoteState.isDirty = false;

        // Update totals and item count UI if present
        if (elements.totalUSD) elements.totalUSD.textContent = '$0.00';
        if (elements.totalVND) elements.totalVND.textContent = '0 ₫';
        if (elements.itemCount) elements.itemCount.textContent = '0';

        showAlert('success', 'Đã đặt lại bộ lọc và xóa các mục hiện tại');
    }
    // Show add product modal
    function showAddProductModal() {
        const panel = document.getElementById('productPanel');
        if (panel) {
            setProductPanelVisible(true);
        } else {
            $('#addProductModal').modal('show');
        }
    }

    // Safely show/hide the side panel
    function setProductPanelVisible(show) {
        const panel = document.getElementById('productPanel');
        const overlay = document.getElementById('productPanelOverlay');
        if (!panel) return;
        if (show) {
            try { panel.removeAttribute('inert'); } catch (e) { }
            try { panel.setAttribute('aria-hidden', 'false'); } catch (e) { }
            if (overlay) try { overlay.setAttribute('aria-hidden', 'false'); } catch (e) { }
            document.body.classList.add('no-scroll-panel');
            // mark existing products so user cannot add duplicates
            try { markExistingProductsInDialog(); } catch (e) { }
            // focus first focusable element inside panel
            try {
                const focusable = panel.querySelector('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
                if (focusable && typeof focusable.focus === 'function') focusable.focus();
            } catch (e) { }
        } else {
            // if focus is inside panel, move it out before hiding
            try {
                const active = document.activeElement;
                if (panel.contains(active)) {
                    const fallback = document.getElementById('btnAddProduct') || document.getElementById('btnSaveQuote') || document.body;
                    if (fallback && typeof fallback.focus === 'function') try { fallback.focus(); } catch (e) { }
                }
            } catch (e) { }

            try { panel.setAttribute('inert', ''); } catch (e) { }
            try { panel.setAttribute('aria-hidden', 'true'); } catch (e) { }
            if (overlay) try { overlay.setAttribute('aria-hidden', 'true'); } catch (e) { }
            document.body.classList.remove('no-scroll-panel');
        }
    }


    // Add selected products to quote
    function addSelectedProducts() {
        var ks = false;
        const selectedCheckboxes = document.querySelectorAll('#availableProductsBody .product-select:checked');
        
        if (selectedCheckboxes.length === 0) {
            showAlert('warning', 'Vui lòng chọn ít nhất một sản phẩm!');
            return;
        }

        // Support data attributes from server-rendered rows or JSON in data-product
        selectedCheckboxes.forEach(checkbox => {
            let productData = null;
            try {
                if (checkbox.dataset.product) {
                    productData = JSON.parse(checkbox.dataset.product);
                } else {
                    productData = {
                        id: checkbox.value,
                        rqCode: checkbox.dataset.rq || checkbox.getAttribute('data-rq'),
                        internalCode: checkbox.dataset.internal || checkbox.getAttribute('data-internal'),
                        name: checkbox.dataset.name || checkbox.getAttribute('data-name'),
                        quantity: parseFloat(checkbox.dataset.qty || checkbox.getAttribute('data-qty')) || 0,
                        unit: checkbox.dataset.unit || checkbox.getAttribute('data-unit') || 'PCS',
                        superlier: checkbox.getAttribute('data-ncc'),
                        // original requirements from request (if any)
                        reqRohs: checkbox.dataset.rohs || checkbox.getAttribute('data-rohs') || '',
                        reqMsds: checkbox.dataset.msds || checkbox.getAttribute('data-msds') || '',
                        reqAnToan: checkbox.dataset.antoan || checkbox.getAttribute('data-antoan') || '',
                        reqCoCq: checkbox.dataset.cocq || checkbox.getAttribute('data-cocq') || ''
                    };
                }
            } catch (e) {
                // fallback minimal object
                productData = { id: checkbox.value, rqCode: '', internalCode: '', name: checkbox.closest('tr').cells[3].textContent.trim(), quantity: 0, unit: 'PCS' };
            }
            if (isDuplicateProduct(productData.internalCode, productData.superlier)) {
                ks = true;
            } else {
                addProductToQuote(productData);
            }
            // uncheck so user can add multiple times later if needed
            checkbox.checked = false;
        });

        // Hide side panel
        const panel = document.getElementById('productPanel');
        if (panel) {
            setProductPanelVisible(false);
        }
        if (!ks) {
            showAlert('success', `Đã thêm ${selectedCheckboxes.length} sản phẩm vào báo giá!`);
        } else {
            showAlert('danger', 'Sản phẩm đã tồn tại trong báo giá!');
        }
    }

    // Add product to quote table
    function addProductToQuote(product) {
        const rowIndex = quoteState.items.length + 1;
        const newRow = createProductRow(product, rowIndex);
        
        elements.quoteItemsBody.appendChild(newRow);
        quoteState.items.push(product);
        
        // Add animation class
        newRow.classList.add('new-row');
        setTimeout(() => newRow.classList.remove('new-row'), 500);
        
        attachRowEventListeners(newRow, rowIndex);
    }
    // check xem mã LK , tương ứng với mã NCC đã tồn tại chưa
    function isDuplicateProduct(internalCode, supplierCode) {
        return quoteState.items.some(item => item.internalCode === internalCode && item.superlier === supplierCode);
    }
    // Create product row HTML
    function createProductRow(product, index) {
        const row = document.createElement('tr');
        row.className = 'quote-item-row';
        // Ensure date value is formatted as yyyy-MM-dd for input[type="date"]
        const deliveryDateVal = formatDateForInput(product.deliveryDate);
        row.innerHTML = `
            <td class="text-center" hidden>${product.id}</td>
            <td class="text-center">${index}</td>
            <td><input type="text" class="form-control form-control-sm" value="${product.internalCode}" readonly></td>
            <td><input type="text" class="form-control form-control-sm" value="${product.superlier}" readonly></td>
            <td><input type="text" class="form-control form-control-sm supplier-code" value="${product.supplierCode || ''}" placeholder="Mã NCC..."></td>
            <td><input type="text" class="form-control form-control-sm" value="${product.name || product.productName || ''}" readonly></td>
            <td><input type="number" class="form-control form-control-sm text-right" value="${product.quantity || 0}" readonly></td>
            <td><input type="text" class="form-control form-control-sm text-center" value="${product.unit || 'PCS'}" readonly></td>
            <td><input type="number" class="form-control form-control-sm text-right price-usd" value="${product.priceUSD || 0}" placeholder="0"></td>
            <td><input type="number" class="form-control form-control-sm text-right price-vnd" value="${product.priceVND || 0}" placeholder="0"></td>
            <td><input type="text" class="form-control form-control-sm text-right moq" value="${product.moq || ''}" placeholder="MOQ"></td>
            <td><input type="text" class="form-control form-control-sm text-center packing" value="${product.packing || ''}" placeholder="Quy cách đóng gói"></td>
            <td><input type="time" class="form-control form-control-sm text-center lead-time" value="${product.leadTime || ''}" placeholder="Lead time"></td>
            <td><input type="date" class="form-control form-control-sm delivery-date" value="${deliveryDateVal}"></td>
            <td>
                <select class="form-control form-control-sm rohs">
                    <option value="">--</option>
                    <option value="OK">OK</option>
                    <option value="NG">NG</option>
                    <option value="No Need">No Need</option>
                </select>
            </td>
            <td>
                <select class="form-control form-control-sm co-cq">
                    <option value="">--</option>
                    <option value="CO">CO</option>
                    <option value="CQ">CQ</option>
                    <option value="CQ">CO&CQ</option>
                </select>
            </td>
            <td>
                <select class="form-control form-control-sm msds">
                    <option value="">--</option>
                    <option value="OK">OK</option>
                    <option value="NG">NG</option>
                    <option value="No Need">No Need</option>
                </select>
            </td>
            <td>
                <select class="form-control form-control-sm antoan">
                    <option value="">--</option>
                    <option value="OK">OK</option>
                    <option value="NG">NG</option>
                    <option value="No Need">No Need</option>
                </select>
            </td>
            <td>
                <select class="form-control form-control-sm camket">
                    <option value="">--</option>
                    <option value="OK">OK</option>
                    <option value="NG">NG</option>
                    <option value="No Need">No Need</option>
                </select>
            </td>
            <td><input type="text" class="form-control form-control-sm giaohang" value="${product.giaoHang || ''}" placeholder="Phương thức..."></td>
            <td><input type="text" class="form-control form-control-sm PayIf" value="${product.payIf || ''}" placeholder="Điều kiện..."></td>
            <td><input type="text" class="form-control form-control-sm file" value="${product.file || ''}" placeholder="Link file..."></td>
            <td class="text-center">
                <button type="button" class="btn btn-sm btn-outline-danger delete-row">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;
        // store requirement attrs on row for comparison
        if (product) {
            if (product.reqRohs) row.dataset.reqRohs = product.reqRohs;
            if (product.reqMsds) row.dataset.reqMsds = product.reqMsds;
            if (product.reqAnToan) row.dataset.reqAntoan = product.reqAnToan;
            if (product.reqCoCq) row.dataset.reqCocq = product.reqCoCq;
            // pre-select requirement fields if values provided
            try {
                const rohsSel = row.querySelector('.rohs');
                if (rohsSel && product.rohs) rohsSel.value = product.rohs;
                const cocqSel = row.querySelector('.co-cq');
                if (cocqSel && product.cocq) cocqSel.value = product.cocq;
                const msdsSel = row.querySelector('.msds');
                if (msdsSel && product.msds) msdsSel.value = product.msds;
                const antoanSel = row.querySelector('.antoan');
                if (antoanSel && product.antoan) antoanSel.value = product.antoan;
                const camketSel = row.querySelector('.camket');
                if (camketSel && product.camket) camketSel.value = product.camket;
            } catch (e) { }
        }
        return row;
    }

    // Format various date inputs to yyyy-MM-dd for input[type="date"]
    function formatDateForInput(val) {
        if (!val) return '';
        try {
            // If already yyyy-MM-dd
            if (typeof val === 'string') {
                // Accept ISO string like 2026-01-31T00:00:00
                const isoMatch = val.match(/^\d{4}-\d{2}-\d{2}/);
                if (isoMatch) return isoMatch[0];
                const d = new Date(val);
                if (!isNaN(d.getTime())) return d.toISOString().slice(0, 10);
                return '';
            }
            if (val instanceof Date) {
                if (!isNaN(val.getTime())) return val.toISOString().slice(0, 10);
                return '';
            }
            // Fallback: try to construct Date
            const d = new Date(val);
            if (!isNaN(d.getTime())) return d.toISOString().slice(0, 10);
        } catch (e) { }
        return '';
    }

    // Attach event listeners to new row
    function attachRowEventListeners(row, index) {

        // Delete row event
        const deleteBtn = row.querySelector('.delete-row');
        deleteBtn.addEventListener('click', function() {
            if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) {
                row.remove();
                quoteState.items.splice(index - 1, 1);
                updateRowNumbers();
                quoteState.isDirty = true;
            }
        });

        // Other input events for marking dirty state
        const inputs = row.querySelectorAll('input, select');
        inputs.forEach(input => {
            input.addEventListener('change', () => {
                quoteState.isDirty = true;
                applyRowHighlighting(row);
            });
        });

        // initial highlighting
        applyRowHighlighting(row);
    }

    // Update row numbers after deletion
    function updateRowNumbers() {
        const rows = elements.quoteItemsBody.querySelectorAll('.quote-item-row');
        rows.forEach((row, index) => {
            if (row.firstElementChild) {
                row.firstElementChild.textContent = index + 1;
            }
        });
    }

    // Apply yellow highlighting based on rules
    function applyRowHighlighting(row) {
        // helpers
        function highlight(el, flag) {
            if (!el) return;
            if (flag) {
                el.style.backgroundColor = '#fff3cd'; // bootstrap warning background-like
            } else {
                el.style.backgroundColor = '';
            }
        }

        const cells = row.querySelectorAll('td');
        // indexes based on createProductRow structure
        const nameInput = row.querySelector('td:nth-child(6) input');
        const qtyInput = row.querySelector('td:nth-child(7) input');
        const unitInput = row.querySelector('td:nth-child(8) input');
        const moqInput = row.querySelector('.moq');
        const deliveryDateInput = row.querySelector('.delivery-date');
        const rohsSel = row.querySelector('.rohs');
        const msdsSel = row.querySelector('.msds');
        const antoanSel = row.querySelector('.antoan');
        const cocqSel = row.querySelector('.co-cq');

        // MOQ > quantity
        const qtyVal = parseFloat(qtyInput && qtyInput.value) || 0;
        const moqVal = parseFloat(moqInput && moqInput.value) || 0;
        highlight(moqInput, moqVal > qtyVal && moqVal > 0);

        // Delivery date later than requested validUntil on page (if exists)
        try {
            const validUntilEl = document.getElementById('validUntil');
            const reqDate = validUntilEl && validUntilEl.value ? new Date(validUntilEl.value) : null;
            const delDate = deliveryDateInput && deliveryDateInput.value ? new Date(deliveryDateInput.value) : null;
            const late = reqDate && delDate && delDate > reqDate;
            highlight(deliveryDateInput, !!late);
        } catch (e) { }

        // Rohs/MSDS/AnToan/COCQ: if original request has requirement data and supplier selects NG or No Need
        const reqRohs = row.dataset.reqRohs || '';
        const reqMsds = row.dataset.reqMsds || '';
        const reqAntoan = row.dataset.reqAntoan || '';
        const reqCocq = row.dataset.reqCocq || '';

        const isBad = v => (v === 'NG' || v === 'No Need');
        highlight(rohsSel, !!reqRohs && isBad(rohsSel && rohsSel.value));
        highlight(msdsSel, !!reqMsds && isBad(msdsSel && msdsSel.value));
        highlight(antoanSel, !!reqAntoan && isBad(antoanSel && antoanSel.value));
        highlight(cocqSel, !!reqCocq && isBad(cocqSel && cocqSel.value));

        // Name, quantity, unit differences are not editable vs request here, but keep highlighting if empty/mismatch detectable
        highlight(nameInput, !nameInput || !nameInput.value);
        highlight(qtyInput, qtyVal <= 0);
        highlight(unitInput, !unitInput || !unitInput.value);
    }
    // Import from Excel
    function importFromExcel() {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.xlsx,.xls';
        input.onchange = function(e) {
            const file = e.target.files[0];
            if (file) {
                showAlert('info', 'Đang xử lý file Excel...');
                const formData = new FormData();
                formData.append('file', file);
                fetch('/Quote/UploadQuoteExcel', {
                    method: 'POST',
                    body: formData
                }).then(r => {
                    if (!r.ok) throw new Error('Upload thất bại');
                    return r.json();
                }).then(items => {
                    // items is a list of BaoGia_Request_of_QuotationDTO; add minimal fields to rows
                    (items || []).forEach(it => {
                        addProductToQuote({
                            internalCode: it.CHR_MaHangNoiBo || '',
                            name: it.NVCHR_NameVN || it.CHR_NameEN || '',
                            quantity: it.INT_SoLuong || 0,
                            unit: it.NVCHR_DonVi || 'PCS'
                        });
                    });
                    showAlert('success', 'Đã import thành công từ file Excel!');
                }).catch(err => {
                    showAlert('danger', 'Lỗi import: ' + (err && err.message ? err.message : err));
                });
            }
        };
        input.click();
    }

    // Save quote
    function saveQuote() {
        //if (!validateForm()) {
        //    return;
        //}
        const user = window.inputQuoteData.user || 'user web';
        const quoteData = collectFormData();
        
        // Add loading state
        const btnSave = document.getElementById('btnSaveQuote');
        btnSave.classList.add('loading');
        btnSave.disabled = true;

        // Map to DTO list and call backend
        const dtoList = (quoteData.items || []).map(it => ({
            ID: 0,
            ID_RequestQuote: parseInt(it.id || 0),
            CHR_CodeNCC: it.supplier || '',
            NVCHR_NameNCC: it.supplier,
            CHR_MaHangNCC: it.supplierCode || '',
            NVCHR_TenHangHQ: it.productName || '',
            FL_USD: it.priceUSD || 0,
            FL_VND: it.priceVND || 0,
            DTM_EndDate: quoteData.validUntil || null,
            NVCHR_MOQ: it.moq+'' || '0',
            DTM_LeadTime: it.leadTime || '',
            DTM_ShipTime: it.deliveryDate || '',
            NVCHR_Packing: it.packing || '',
            BIT_Commit: it.camket === 'true' || it.camket === true || it.camket === '1' ? true : false,
            NVCHR_Note: '',
            NVCHR_File: it.file || '',
            DTM_CreateDate: getVietnamTimeISO(),
            CHR_CreateBy: user,
            DTM_UpdateDate: null,
            CHR_UpdateBy:'',
            FL_Sum: (it.quantity || 0) * (it.priceVND || 0),
            BIT_Select: null,
            NVCHR_ReasonPick: '',
            CHR_Status: 'Draft',
            INT_NumberEdit: 0,
            NVCHR_dataOld: '',
            NVCHR_dataNew: '',
            FL_ExchangeRate: quoteData.exchangeRate || 0,
            FL_TaxRate: 0,
            FL_TaxAmount: 0,
            FL_TotalAfterTax: 0,
            NVCHR_PaymentTerm: it.payIf || '',
            NVCHR_Warranty: '',
            NVCHR_DeliveryTerm: it.giaoHang || '',
            VCHR_Rohs: it.rohs || '',
            VCHR_COCQ: it.cocq || '',
            VCHR_MSDS: it.msds || '',
            VCHR_AnToan: it.antoan || '',
            VCHR_CamKet: it.camket || ''
        }));

        callApi('/Quote/InsertInputQuote', dtoList)
            .then(() => {
                showAlert('success', 'Đã lưu báo giá thành công!');
                quoteState.isDirty = false;
                resetFilters();
            })
            .catch(err => {
                showAlert('danger', 'Lưu thất bại: ' + err);
            })
            .finally(() => {
                btnSave.classList.remove('loading');
                btnSave.disabled = false;
            });
    }
    function getVietnamTimeISO() {
        const now = new Date();
        // Lấy UTC và cộng thêm 7 giờ
        const utc = now.getTime() + (now.getTimezoneOffset() * 60000);
        const vietnamTime = new Date(utc + (7 * 60 * 60000));
        return vietnamTime.toISOString();
    }
    // Save as draft
    function saveDraft() {
        const quoteData = collectFormData();
        quoteData.status = 'draft';
        
        console.log('Saving draft:', quoteData);
        showAlert('success', 'Đã lưu nháp thành công!');
        quoteState.isDirty = false;
    }

    // Submit quote
    function submitQuote() {
        //if (!validateForm()) {
        //    return;
        //}

        if (confirm('Bạn có chắc chắn muốn gửi báo giá này? Sau khi gửi sẽ không thể chỉnh sửa.')) {
            const quoteData = collectFormData();
            quoteData.status = 'submitted';
            
            console.log('Submitting quote:', quoteData);
            showAlert('success', 'Đã gửi báo giá thành công!');
            quoteState.isDirty = false;
            
            // Redirect or reload page
            setTimeout(() => {
                window.location.href = '/Quote/Quotation_Results';
            }, 2000);
        }
    }

    // Cancel quote
    function cancelQuote() {
        if (quoteState.isDirty) {
            if (confirm('Bạn có thay đổi chưa được lưu. Bạn có chắc chắn muốn hủy?')) {
                window.location.href = '/Quote/Quote';
            }
        } else {
            window.location.href = '/Quote/Quote';
        }
    }

    // Validate form
    function validateForm() {
        let isValid = true;
        const errors = [];

        // Check required fields
        if (!elements.quoteRequestCode || !elements.quoteRequestCode.value || !elements.quoteRequestCode.value.trim()) {
            if (elements.quoteRequestCode) markFieldInvalid(elements.quoteRequestCode);
            errors.push('Mã yêu cầu báo giá là bắt buộc');
            isValid = false;
        } else {
            markFieldValid(elements.quoteRequestCode);
        }

        if (!elements.supplierSelect || !elements.supplierSelect.value) {
            if (elements.supplierSelect) markFieldInvalid(elements.supplierSelect);
            errors.push('Nhà cung cấp là bắt buộc');
            isValid = false;
        } else {
            markFieldValid(elements.supplierSelect);
        }

        // Check if at least one product with price
        const priceInputs = document.querySelectorAll('.price-usd');
        const hasValidProduct = Array.from(priceInputs).some(input => parseFloat(input.value) > 0);
        
        if (!hasValidProduct) {
            errors.push('Vui lòng nhập giá cho ít nhất một sản phẩm');
            isValid = false;
        }

        if (errors.length > 0) {
            showAlert('danger', 'Vui lòng kiểm tra:\n' + errors.join('\n'));
        }

        return isValid;
    }

    // Mark field as invalid
    function markFieldInvalid(field) {
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');
    }

    // Mark field as valid
    function markFieldValid(field) {
        field.classList.add('is-valid');
        field.classList.remove('is-invalid');
    }

    // Collect form data
    function collectFormData() {
        const items = [];
        const rows = elements.quoteItemsBody ? elements.quoteItemsBody.querySelectorAll('.quote-item-row') : [];

        rows.forEach(row => {
            // Bỏ qua dòng đầu tiên nếu không có dữ liệu (dòng mẫu trống)
            const inputs = row.querySelectorAll('input, select');
            const idCell = row.querySelector('td:first-child');
            // Kiểm tra nếu tất cả các input/select đều rỗng thì bỏ qua dòng này
            const isEmptyRow = Array.from(inputs).every(input => !input.value);
            if (isEmptyRow) return;
            items.push({
                id: idCell ? (idCell.textContent || '').trim() : '',                         // Cột 1 (hidden ID)
                internalCode: inputs[0]?.value || '',                                         // Cột 3
                supplier: inputs[1]?.value || '',                                             // Cột 4 (Mã nhà cung cấp)
                supplierCode: inputs[2]?.value || '',                                         // Cột 5 (Mã hàng NCC)
                productName: inputs[3]?.value || '',                                          // Cột 6 (Tên dùng mở thủ tục)
                quantity: parseFloat(inputs[4]?.value) || 0,                                  // Cột 7 (Số lượng)
                unit: inputs[5]?.value || '',                                                 // Cột 8 (Đơn vị)
                priceUSD: parseFloat(inputs[6]?.value) || 0,                                  // Cột 9 (Đơn giá USD)
                priceVND: parseFloat(inputs[7]?.value) || 0,                                  // Cột 10 (Đơn giá VND)
                moq: parseInt(inputs[8]?.value) || 0,                                         // Cột 11 (MOQ)
                packing: inputs[9]?.value || '',                                              // Cột 12 (Quy cách đóng gói)
                leadTime: inputs[10]?.value || '',                                            // Cột 13 (Lead time)
                deliveryDate: inputs[11]?.value || '',                                        // Cột 14 (Ngày giao)
                rohs: inputs[12]?.value || '',                                                // Cột 15 (Rohs)
                coCq: inputs[13]?.value || '',                                                // Cột 16 (CO/CQ)
                msds: inputs[14]?.value || '',                                                // Cột 17 (MSDS)
                antoan: inputs[15]?.value || '',                                              // Cột 18 (Tiêu chuẩn an toàn)
                camket: inputs[16]?.value || '',                                              // Cột 19 (Cam kết đúng yêu cầu)
                giaoHang: inputs[17]?.value || '',                                            // Cột 20 (Phương thức giao)
                payIf: inputs[18]?.value || '',                                               // Cột 21 (Điều kiện)
                file: inputs[19]?.value || ''                                                 // Cột 22 (File đính kèm)
            });
        });

        return {
            quoteRequestCode: elements.quoteRequestCode?.value || '',
            supplier: elements.supplierSelect?.value || '',
            quoteDate: elements.quoteDate?.value || '',
            validUntil: elements.validUntil?.value || '',
            notes: elements.quoteNote?.value || '',
            exchangeRate: quoteState.exchangeRate,
            paymentTerms: elements.paymentTerms?.value || '',
            termsConditions: elements.termsConditions?.value || '',
            items: items,
            totalUSD: quoteState.totalUSD,
            totalVND: quoteState.totalVND
        };
    }

    // Show alert message - Compatible với cả Bootstrap 4 và 5
    function showAlert(type, message) {
        // Create alert element
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        
        alertDiv.innerHTML = `
            ${message.replace(/\n/g, '<br>')}
        `;
        document.body.appendChild(alertDiv);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (alertDiv.parentNode) {
                alertDiv.remove();
            }
        }, 5000);
    }

    // Initialize
    function init() {
        // Wait for DOM and ensure all scripts are loaded
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(init, 100); // Small delay to ensure all scripts are loaded
                return;
            });
        }
        
        // Initialize elements first
        initializeElements();

        // Then initialize event listeners
        initializeEventListeners();
        
        // Set default delivery date (7 days from now)
        const defaultDeliveryDate = new Date();
        defaultDeliveryDate.setDate(defaultDeliveryDate.getDate() + 7);
        
        
        console.log('InputQuote module initialized');
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        // If DOM is already loaded, wait a bit for all scripts to load
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

    // Search Input Quote via backend
    function searchInputQuote() {
        const body = {
            idRequestQuote: 0,
            maDon: document.getElementById('quoteRequestCode')?.value || '',
            maVatTu: document.getElementById('searchMaVatTu')?.value || '',
            maNcc: document.getElementById('supplierSelect')?.value || '',
            section: document.getElementById('searchPhongBan')?.value || '',
            dayMM: null,
            pageSize: 100,
            pageIndex: 0
        };
        callApi('/Quote/SearchInputQuote', body)
            .then(data => {
                // Clear current rows
                elements.quoteItemsBody.innerHTML = '';
                quoteState.items = [];
                // data is list of BaoGia_Detail_of_QuotationDTO
                (data || []).forEach(d => {
                    addProductToQuote({
                        id: d.ID,                         // Cột 1 (hidden ID)
                        internalCode: d.CHR_MaHangNoiBo,                   // Cột 3
                        superlier: d.CHR_CodeNCC || '',                                             // Cột 4 (Mã nhà cung cấp)
                        supplierCode: d.CHR_MaHangNCC || '',                                         // Cột 5 (Mã hàng NCC)
                        productName: d.NVCHR_TenHangHQ || '',                                          // Cột 6 (Tên dùng mở thủ tục)
                        quantity: d.INT_SoLuong || 0,                                  // Cột 7 (Số lượng)
                        unit: d.NVCHR_DonVi || '',                                                 // Cột 8 (Đơn vị)
                        priceUSD: parseFloat(d.FL_USD) || 0,                                  // Cột 9 (Đơn giá USD)
                        priceVND: parseFloat(d.FL_VND) || 0,                                  // Cột 10 (Đơn giá VND)
                        moq: parseInt(d.NVCHR_MOQ) || 0,                                         // Cột 11 (MOQ)
                        packing: d.NVCHR_Packing || '',                                              // Cột 12 (Quy cách đóng gói)
                        leadTime: d.DTM_LeadTime || '',                                            // Cột 13 (Lead time)
                        deliveryDate: d.DTM_ShipTime || '',                                        // Cột 14 (Ngày giao)
                        rohs: d.VCHR_Rohs || '',                                                // Cột 15 (Rohs)
                        coCq: d.VCHR_COCQ || '',                                                // Cột 16 (CO/CQ)
                        msds: d.VCHR_MSDS || '',                                                // Cột 17 (MSDS)
                        antoan: d.VCHR_AnToan || '',                                              // Cột 18 (Tiêu chuẩn an toàn)
                        camket: d.VCHR_CamKet || '',                                              // Cột 19 (Cam kết đúng yêu cầu)
                        giaoHang: d.NVCHR_DeliveryTerm || '',                                            // Cột 20 (Phương thức giao)
                        payIf: d.NVCHR_PaymentTerm || '',                                               // Cột 21 (Điều kiện)
                        file: d.NVCHR_File || ''                                                 // Cột 22 (File đính kèm)
                    });
                });
                showAlert('success', 'Đã lọc dữ liệu báo giá');
            })
            .catch(err => showAlert('danger', 'Lọc thất bại: ' + err));
    }
    // Render lại bảng sản phẩm trong panel chọn sản phẩm
    function renderAvailableProductsDialog(products) {
        const tbody = elements.availableProductsBody || document.getElementById('availableProductsBody');
        tbody.innerHTML = (products || []).map(i => `
            <tr>
                <td class="text-center">
                    <input type="checkbox"
                        class="form-check-input product-select"
                        value="${i.iD}"
                        data-rq="${i.chR_MaDon || ''}"
                        data-internal="${i.chR_MaHangNoiBo || ''}"
                        data-name="${i.nvchR_NameVN ? i.nvchR_NameVN : (i.chR_NameEN || '')}"
                        data-section="${i.chR_SectionName || ''}"
                        data-qty="${i.inT_SoLuong || 0}"
                        data-unit="${i.nvchR_DonVi || ''}"
                        data-ncc="${i.chR_MaNCC || ''}"
                        data-rohs="${i.nvchR_Rohs || ''}"
                        data-antoan="${i.nvchR_AnToan || ''}"
                        data-cocq="${i.nvchR_COCQ || ''}"
                        data-msds="${i.nvchR_MSDS || ''}" />
                </td>
                <td>${i.chR_MaDon || ''}</td>
                <td>${i.chR_MaHangNoiBo || ''}</td>
                <td>${i.nvchR_NameVN ? i.nvchR_NameVN : (i.chR_NameEN || '')}</td>
                <td>${i.chR_SectionName || ''}</td>
                <td class="text-end">${i.inT_SoLuong || 0}</td>
                <td>${i.nvchR_DonVi || ''}</td>
                <td>${i.chR_MaNCC || ''}</td>
            </tr>
        `).join('');
    }

    // search dialog quote request
    function searchQuoteRequestDialog() {
        const body = {
            maDon: document.getElementById('modalRqCode')?.value || '',
            maNcc: document.getElementById('modalSupplier')?.value || '',
            section: document.getElementById('modalSection')?.value || '',
            nguoiYeuCau: '',
            maHang: document.getElementById('modalInternalCode')?.value || '',
            trangThai: null,
            step: 6,
            pageSize: 100,
            pageIndex: 0,
            date: null
        };
        callApi('/Quote/SearchBaoGia', body)
            .then(data => {
                renderAvailableProductsDialog(data || []);
                showAlert('success', 'Đã lọc dữ liệu báo giá');
            })
            .catch(err => showAlert('danger', 'Lọc thất bại: ' + err));
    }
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