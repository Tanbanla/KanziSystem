document.addEventListener('DOMContentLoaded', function () {
    window._quotationResultsState = window._quotationResultsState || { openGroups: {}, showAdditionalColumns: true };

    const supplierPickReasonOptions = [
        'Cheaper price_Giá rẻ hơn',
        'Higher price_Giá cao hơn',
        'This category have 01 vendor only _Chủng loại chỉ có 1 NCC',
        'Only this vendor sent quotation_Chỉ NCC này gửi báo giá',
        'RQ section request QTN from 01 vendor only _ Phòng ban yêu cầu lấy giá từ 1 NCC',
        'EOL_ NCC dừng cấp mã hàng',
        'Vendors refused to quote_NCC từ chối báo giá',
        'Vendor did not send quotation on time_NCC không gửi báo giá đúng hạn',
        "Requesting section input Incorrect vendor's good code_Phòng ban điền sai mã hàng của NCC",
        'Vendors quoted for similar alternative product_NCC báo giá cho mã thay thế',
        'Higher price but MOQ suitable_ giá cao nhưng MOQ phù hợp',
        'Higher price but lead time suitable_ giá cao nhưng lead time phù hợp',
        'Vendor has been closed_NCC đã giải thể',
        'Cheaper price but MOQ unsuitable_ giá rẻ nhưng MOQ KHÔNG phù hợp',
        'Cheaper price but lead time unsuitable_ giá rẻ nhưng lead time DÀI'
    ];
    const supplierReasonDatalistId = 'supplierPickReasonOptionsList';

    function ensureReasonDatalist() {
        try {
            let listEl = document.getElementById(supplierReasonDatalistId);
            if (!listEl) {
                listEl = document.createElement('datalist');
                listEl.id = supplierReasonDatalistId;
                document.body.appendChild(listEl);
            }

            listEl.innerHTML = supplierPickReasonOptions.map(x => `<option value="${String(x).replace(/"/g, '&quot;')}"></option>`).join('');
        } catch { }
    }

    // Pagination state for Request List tab
    const requestListState = {
        pageIndex: 1,
        pageSize: 10,
        returnedCount: 0,
        totalCount: 0,
        lastPage: false
    };

    const masterQuoteState = {
        pageIndex: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
        lastPage: false
    };

    // Pagination state for Supplier tab
    const supplierState = {
        pageIndex: 1,
        pageSize: 20,
        returnedCount: 0,
        totalCount: 0,
        lastPage: false,
        latestSelectedPrices: {}
    };

    // hàm formart
    const escape = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
    const date = value => value ? new Date(value).toLocaleDateString('vi-VN') : '';
    const cell = value => `<td class ="text-center">${escape(value)}</td>`;

    const masterPriceGet = (row, name) => row?.[name] ?? row?.[name.charAt(0).toLowerCase() + name.slice(1)] ?? '';
    const masterPriceDateInput = value => {
        if (!value) return '';
        const parsed = new Date(value);
        return Number.isNaN(parsed.getTime()) ? '' : new Date(parsed.getTime() - parsed.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
    };
    const masterPriceHistoryCell = (row, name, type = 'text') => {
        const value = masterPriceGet(row, name);
        const displayValue =
            type === 'checkbox'
                ? (value ? 'Có' : 'Không')
                : (type === 'datetime-local'
                    ? date(value)
                    : value);

        const centerColumns = [
            'nvchR_PART_NAME_VN',
            'nvchR_PART_NAME_EN'
        ];

        const cssClass = !centerColumns.includes(name)
            ? 'text-center align-middle'
            : '';

        return `<td class="${cssClass}">${escape(displayValue)}</td>`;
    };
    const masterPriceExchangeRates = new Map([['USD', 1]]);
    const masterPriceExchangeRateRequests = new Map();

    async function getMasterPriceExchangeRate(currency) {
        const normalizedCurrency = String(currency || '').trim().toUpperCase();
        if (!normalizedCurrency) return null;
        if (masterPriceExchangeRates.has(normalizedCurrency)) return masterPriceExchangeRates.get(normalizedCurrency);
        if (masterPriceExchangeRateRequests.has(normalizedCurrency)) return masterPriceExchangeRateRequests.get(normalizedCurrency);

        const request = fetch(`${window.apiBaseUrl || ''}/InputQuotation/GetExchangeRate?currency=${encodeURIComponent(normalizedCurrency)}`)
            .then(async response => {
                if (!response.ok) throw new Error(await response.text() || `Không thể lấy tỷ giá ${normalizedCurrency}`);
                const result = await response.json();
                const rate = Number(result?.data ?? result);
                if (!Number.isFinite(rate) || rate <= 0) throw new Error(`Tỷ giá ${normalizedCurrency} không hợp lệ`);
                masterPriceExchangeRates.set(normalizedCurrency, rate);
                return rate;
            })
            .finally(() => masterPriceExchangeRateRequests.delete(normalizedCurrency));

        masterPriceExchangeRateRequests.set(normalizedCurrency, request);
        return request;
    }

    async function updateMasterPriceUsd() {
        const fields = document.getElementById('masterPriceEditFields');
        const priceInput = fields?.querySelector('[name="DEC_UNIT_PRICE"]');
        const currencyInput = fields?.querySelector('[name="CHR_CURRENCY"]');
        const usdInput = fields?.querySelector('[name="DEC_UNIT_PRICE_USD"]');
        if (!priceInput || !currencyInput || !usdInput) return;

        const currency = currencyInput.value.trim().toUpperCase();
        const price = Number(priceInput.value);
        if (!currency || !Number.isFinite(price) || price < 0) {
            usdInput.value = '';
            return;
        }

        const selectedCurrency = currency;
        try {
            const rate = await getMasterPriceExchangeRate(selectedCurrency);
            if (currencyInput.value.trim().toUpperCase() !== selectedCurrency) return;
            usdInput.value = String(Number((price / rate).toFixed(4)));
        } catch (error) {
            usdInput.value = '';
            showDialog({ title: 'Thông báo', message: error.message || 'Không thể lấy tỷ giá.', type: 'error' });
        }
    }

    const masterPriceOverlay = (kind, open) => {
        const overlay = document.getElementById(`masterPrice${kind === 'history' ? 'History' : 'Edit'}Overlay`);
        if (!overlay) return;
        overlay.classList.toggle('is-open', open);
        overlay.setAttribute('aria-hidden', String(!open));
        document.body.classList.toggle('master-price-modal-open', open);
        if (open) overlay.querySelector('button, input, select, textarea')?.focus();
    };

    async function loadMasterPriceHistory(code) {
        const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/GetPriceMasterHistory', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(code) });
        if (!response.ok) throw new Error(await response.text() || 'Không thể tải lịch sử master giá');
        const result = await response.json();
        return Array.isArray(result?.data) ? result.data : [];
    }

    async function exportMasterPriceHistory() {
        const code = masterPriceModalState.code;
        if (!code) return;
        const button = document.getElementById('masterPriceHistoryExport');
        if (button) button.disabled = true;
        showLoading('Đang tải lịch sử master giá...');
        try {
            const payload = {
                internalPartCode: code,
                vendorCode: document.getElementById('historyVendorCode')?.value.trim() || null,
                quotationRequestNo: document.getElementById('historyQuotationNo')?.value.trim() || null,
                from: document.getElementById('historyFrom')?.value || null,
                to: document.getElementById('historyTo')?.value || null
            };
            const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ExportPriceMasterHistory', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            if (!response.ok) throw new Error(await response.text() || 'Không thể xuất lịch sử master giá');
            const blob = await response.blob();
            const contentDisposition = response.headers.get('Content-Disposition') || '';
            const fileNameMatch = contentDisposition.match(/filename\*?=(?:UTF-8'')?([^;]+)/i);
            const fileName = fileNameMatch ? decodeURIComponent(fileNameMatch[1].replace(/^"|"$/g, '')) : `MasterGia_${code}.xlsx`;
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            link.remove();
            URL.revokeObjectURL(url);
        } catch (error) {
            showDialog({ title: 'Thông báo', message: error.message || 'Không thể xuất lịch sử master giá.', type: 'error' });
        } finally {
            if (button) button.disabled = false;
            hideLoading();
        }
    }

    function renderMasterPriceHistory(rows) {
        const head = document.getElementById('masterPriceHistoryHead');
        const body = document.getElementById('masterPriceHistoryBody');
        if (!head || !body) return;
        body.innerHTML = rows.length ? rows.map((row, index) => `<tr><td class="text-center">${index + 1}</td>
            ${masterPriceHistoryCell(row, 'id', 'number')}${masterPriceHistoryCell(row, 'dtM_UPLOAD', 'datetime-local')}${masterPriceHistoryCell(row, 'chR_UPLOAD_USERID')}
            ${masterPriceHistoryCell(row, 'chR_QUOTATION_REQUEST_NO')}${masterPriceHistoryCell(row, 'inT_QUOTATION_DETAIL', 'number')}${masterPriceHistoryCell(row, 'chR_EQUIPMENT_CODE')}
            ${masterPriceHistoryCell(row, 'chR_INTERNAL_PART_CODE')}${masterPriceHistoryCell(row, 'chR_VENDOR_PART_CODE')}${masterPriceHistoryCell(row, 'nvchR_PART_NAME_VN')}
            ${masterPriceHistoryCell(row, 'nvchR_PART_NAME_EN')}${masterPriceHistoryCell(row, 'nvchR_UNIT')}${masterPriceHistoryCell(row, 'nvchR_OTHER_REQUIREMENT')}
            ${masterPriceHistoryCell(row, 'nvchR_MAKER_ORIGIN')}${masterPriceHistoryCell(row, 'deC_QUANTITY', 'number')}${masterPriceHistoryCell(row, 'chR_VENDOR_CODE')}
            ${masterPriceHistoryCell(row, 'nvchR_VENDOR_NAME')}${masterPriceHistoryCell(row, 'deC_UNIT_PRICE', 'number')}${masterPriceHistoryCell(row, 'chR_CURRENCY')}
            ${masterPriceHistoryCell(row, 'deC_UNIT_PRICE_USD', 'number')}${masterPriceHistoryCell(row, 'inT_LEAD_TIME_DAY', 'number')}${masterPriceHistoryCell(row, 'deC_MOQ', 'number')}
            ${masterPriceHistoryCell(row, 'nvchR_REMARK')}${masterPriceHistoryCell(row, 'nvchR_DELIVERY_TERM')}${masterPriceHistoryCell(row, 'nvchR_PLACE')}
            ${masterPriceHistoryCell(row, 'nvchR_SHIPMENT_METHOD')}${masterPriceHistoryCell(row, 'deC_VAT_PERCENT', 'number')}${masterPriceHistoryCell(row, 'nvchR_PAYMENT_TERM')}
            ${masterPriceHistoryCell(row, 'dtM_PRICE_EFFECTIVE', 'datetime-local')}${masterPriceHistoryCell(row, 'dtM_PRICE_EXPIRATION', 'datetime-local')}
            ${masterPriceHistoryCell(row, 'biT_FIX_VENDOR', 'checkbox')}${masterPriceHistoryCell(row, 'nvchR_ADJUSTMENT_REASON')}
            ${masterPriceHistoryCell(row, 'nvchR_QTN_LINK')}${masterPriceHistoryCell(row, 'nvchR_QTN_EXCEL_LINK')}
            ${masterPriceHistoryCell(row, 'chR_CRT_USERID')}${masterPriceHistoryCell(row, 'dtM_CREATE', 'datetime-local')}${masterPriceHistoryCell(row, 'chR_UPD_USERID')}${masterPriceHistoryCell(row, 'dtM_UPDATE', 'datetime-local')}</tr>`).join('') : '<tr><td colspan="39" class="text-center text-muted py-4">Không có dữ liệu phù hợp</td></tr>';
    }

    function renderMasterPriceEdit(row, internalPartCode = '', includeAuditFields = true) {
        const container = document.getElementById('masterPriceEditFields');
        if (!container) return;
        container.classList.toggle('master-price-add-mode', !includeAuditFields);
        container.querySelectorAll('[data-master-price-audit]').forEach(field => field.hidden = !includeAuditFields);
        container.querySelectorAll('[name], input[type="file"]').forEach(input => {
            const value = masterPriceGet(row, input.name);
            if (input.type === 'checkbox') input.checked = Boolean(value);
            else if (input.type === 'datetime-local') input.value = masterPriceDateInput(value);
            else if (input.type === 'file') input.value = '';
            else input.value = value ?? '';
        });
        container.querySelectorAll('select').forEach(select => {
            select.dispatchEvent(new Event('change', { bubbles: true }));
        });
        const internalCodeInput = container.querySelector('[name="CHR_INTERNAL_PART_CODE"]');
        if (internalCodeInput && internalPartCode) internalCodeInput.value = internalPartCode;
        const vendorSelect = container.querySelector('[name="CHR_VENDOR_CODE"]');
        const vendorNameInput = container.querySelector('[name="NVCHR_VENDOR_NAME"]');
        if (vendorSelect && vendorNameInput) {
            vendorSelect.addEventListener('change', () => {
                const selected = vendorSelect.options[vendorSelect.selectedIndex];
                vendorNameInput.value = selected?.textContent.split(' - ').slice(1).join(' - ').trim() || '';
            });
        }
        initEnhancements(container);
        if (!container.dataset.masterPriceConversionBound) {
            container.querySelector('[name="DEC_UNIT_PRICE"]')?.addEventListener('input', updateMasterPriceUsd);
            container.querySelector('[name="CHR_CURRENCY"]')?.addEventListener('change', updateMasterPriceUsd);
            container.dataset.masterPriceConversionBound = 'true';
        }
        container.querySelectorAll('.master-price-file-input').forEach(fileInput => fileInput.addEventListener('change', () => {
            const linkInput = container.querySelector(`[name="${fileInput.dataset.linkField}"]`);
            if (linkInput && fileInput.files?.[0]) linkInput.value = fileInput.files[0].name;
        }));
        updateMasterPriceUsd();
    }

    async function loadMaterialInfoForMasterPrice(internalPartCode) {
        const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/GetMaterialInfo', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(internalPartCode)
        });
        if (!response.ok) throw new Error(await response.text() || 'Không thể tải thông tin linh kiện');

        const result = await response.json();
        const material = result?.data;
        if (!material) throw new Error('Không tìm thấy thông tin linh kiện');
        return material;
    }

    function fillMasterPriceFromMaterial(material) {
        const get = (...names) => names.map(name => material?.[name]).find(value => value !== undefined && value !== null) ?? '';
        const values = {
            CHR_INTERNAL_PART_CODE: get('material_Code', 'Material_Code'),
            NVCHR_PART_NAME_VN: get('material_Name_VN', 'Material_Name_VN', 'nameVI', 'NameVI'),
            NVCHR_PART_NAME_EN: get('material_Name_EN', 'Material_Name_EN'),
            NVCHR_UNIT: get('unit', 'Unit'),
            CHR_VENDOR_PART_CODE: get('Code_Suppiler', 'code_Suppiler')
            //NVCHR_OTHER_REQUIREMENT: get('tenMoThuTuc', 'TenMoThuTuc')
        };

        Object.entries(values).forEach(([name, value]) => {
            const input = document.querySelector(`#masterPriceEditFields [name="${name}"]`);
            if (input && value !== '') input.value = value;
        });
    }

    async function uploadMasterPriceFiles(dto) {
        const files = document.querySelectorAll('#masterPriceEditFields .master-price-file-input');
        for (const fileInput of files) {
            const file = fileInput.files?.[0];
            if (!file) continue;
            const formData = new FormData();
            formData.append('file', file);
            formData.append('kind', 'quotation');
            const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/UploadPriceMasterFile', { method: 'POST', body: formData });
            if (!response.ok) throw new Error(await response.text() || `Không thể tải file ${file.name}`);
            const result = await response.json();
            dto[fileInput.dataset.linkField] = result?.data || null;
        }
        return dto;
    }

    function readMasterPriceEdit() {
        const form = document.getElementById('masterPriceEditForm');
        const dto = {};
        form?.querySelectorAll('[name]').forEach(input => {
            if (input.type === 'file') return;
            if (input.type === 'checkbox') dto[input.name] = input.checked;
            else if (input.type === 'number') dto[input.name] = input.value === '' ? null : Number(input.value);
            else if (input.type === 'datetime-local') dto[input.name] = input.value ? new Date(input.value).toISOString() : null;
            else dto[input.name] = input.value || null;
        });
        if (masterPriceModalState.mode === 'add') {
            delete dto.ID;
            delete dto.DTM_UPLOAD;
            delete dto.DTM_CREATE;
        }
        return dto;
    }

    function validateMasterPriceEdit() {
        const fields = document.querySelectorAll('#masterPriceEditFields .master-price-field');
        const missing = [];
        let firstInvalid = null;

        fields.forEach(field => {
            const input = field.querySelector('[name]');
            const label = field.querySelector('label');
            const required = label?.querySelector('.text-danger');
            if (!input || !required) return;

            input.classList.remove('is-invalid');
            const fileInput = field.querySelector('.master-price-file-input');
            const linkedInput = fileInput?.dataset.linkField
                ? field.querySelector(`[name="${fileInput.dataset.linkField}"]`)
                : null;
            const hasValue = fileInput?.files?.length > 0 || Boolean((linkedInput || input).value?.trim());

            if (!hasValue) {
                input.classList.add('is-invalid');
                missing.push(label.textContent.replace('*', '').trim());
                firstInvalid ||= fileInput || input;
            }
        });

        if (!missing.length) return true;

        firstInvalid?.focus();
        showDialog({
            title: 'Thông báo',
            message: `Vui lòng nhập đầy đủ các trường bắt buộc: ${missing.join(', ')}.`,
            type: 'error'
        });
        return false;
    }

    const masterPriceModalState = { rows: [], code: '', mode: 'edit' };

    function getSupplierValidationWarnings(rows) {
        const groups = new Map();
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        rows.forEach(row => {
            const choice = row.querySelector('.supplier-choice')?.value || '';
            if (!choice) return;
            const groupKey = `${row.dataset.madon || ''}|${row.dataset.mahang || ''}`;
            if (!groups.has(groupKey)) groups.set(groupKey, []);
            groups.get(groupKey).push({ row, choice });
        });

        const warnings = [];
        groups.forEach(items => {
            const quoted = items.some(item => Number(item.row.dataset.priceUsd) > 0 || Number(item.row.dataset.priceVnd) > 0);
            const selected = items.filter(item => item.choice === 'true');
            if (quoted && selected.length === 0) {
                warnings.push({ row: items[0].row, reason: 'Có NCC báo giá nhưng toàn bộ NCC đang tick X, chưa chọn NCC nào.' });
            }
            selected.forEach(item => {
                const row = item.row;
                if (Number(row.dataset.priceUsd) <= 0 && Number(row.dataset.priceVnd) <= 0) {
                    warnings.push({ row, reason: 'Đã chọn NCC nhưng giá báo giá bằng 0.' });
                }
                const expiry = row.dataset.expiry ? new Date(row.dataset.expiry) : null;
                if (expiry && !Number.isNaN(expiry.getTime())) {
                    expiry.setHours(0, 0, 0, 0);
                    if (expiry < today) warnings.push({ row, reason: 'Đã chọn NCC nhưng báo giá đã hết hiệu lực.' });
                }
                if (!row.dataset.quoteLink?.trim()) {
                    warnings.push({ row, reason: 'Đã chọn NCC nhưng link báo giá đang để trống.' });
                }
            });
        });
        return warnings;
    }

    function exportSupplierWarnings(warnings) {
        const lines = ['STT,Số đơn,Mã hàng nội bộ,Mã NCC,Lỗi'];
        warnings.forEach((warning, index) => {
            const row = warning.row;
            const values = [index + 1, row.dataset.madon, row.dataset.mahang, row.dataset.vendor || '', warning.reason];
            lines.push(values.map(value => `"${String(value).replace(/"/g, '""')}"`).join(','));
        });
        const blob = new Blob(['\ufeff' + lines.join('\r\n')], { type: 'text/csv;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `SupplierValidationErrors_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')}.csv`;
        link.click();
        URL.revokeObjectURL(url);
    }

    function showSupplierValidationDialog(warnings) {
        return new Promise(resolve => {
            const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
            if (!overlay) { resolve('close'); return; }
            const T = window.i18nQuotationResults || {};
            titleEl.textContent = T.SupplierValidationWarningTitle || 'Cảnh báo lựa chọn nhà cung cấp';
            bodyEl.innerHTML = `<div class="text-danger mb-2">Phát hiện ${warnings.length} lỗi. Bạn có muốn tiếp tục gửi dữ liệu không?</div><ul>${warnings.map(x => `<li>${escape(x.row.dataset.madon)} / ${escape(x.row.dataset.mahang)}: ${escape(x.reason)}</li>`).join('')}</ul>`;
            footerEl.innerHTML = '';
            const addButton = (text, action, className) => {
                const button = document.createElement('button');
                button.type = 'button';
                button.className = className;
                button.textContent = text;
                button.onclick = () => { hideDialog(); resolve(action); };
                footerEl.appendChild(button);
            };
            addButton(T.SupplierValidationClose || 'Đóng', 'close', 'cm-btn cm-btn-outline');
            addButton(T.SupplierValidationExport || 'Xuất dữ liệu lỗi', 'export', 'cm-btn cm-btn-outline');
            addButton(T.SupplierValidationContinue || 'Tiếp tục', 'continue', 'cm-btn cm-btn-primary');
            overlay.setAttribute('aria-hidden', 'false');
            overlay.style.display = 'flex';
            attachDialogCloseHandlers();
            const close = () => { hideDialog(); resolve('close'); };
            overlay.querySelector('[data-cm-action="close"]').onclick = close;
            overlay.querySelector('[data-cm-action="overlay"]').onclick = close;
        });
    }
    // Khai báo biến toàn cục cho file
    const quotationApp = {
        init: function () {
            this.bindEvents();
            this.closeEdit();
            ensureReasonDatalist();
            // Khởi tạo dropdown có tìm kiếm cho các select có class 'searchable-select'
            initEnhancements();
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
            } else if (target === '#supplier-input') {
                this.loadSupplierInput();

                this.applyAdditionalColumnsVisibility();
            } else if (target === '#input-quotation') {
                this.loadMasterQuoteData();
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
                supplierPageSize.value = '20';
                supplierPageSize.addEventListener('change', () => {
                    supplierState.pageSize = parseInt(supplierPageSize.value) || 50;
                    supplierState.pageIndex = 1;
                    this.loadSupplierData();
                });
            }
        },

        loadRequestList: function () {
            this.searchItems();
        },
        loadSupplierInput: function () {
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
            fetch((window.apiBaseUrl || '') + '/QuoteResults/SearchSupplierQuoteBody', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(res => res.json())
                .then(async data => {
                    const items = Array.isArray(data.data.data) ? data.data.data : [];
                    const total = typeof data.data.totalCount === 'number' ? data.data.totalCount : items.length;
                    supplierState.returnedCount = items.length;
                    supplierState.totalCount = total;
                    supplierState.lastPage = (supplierState.pageIndex * supplierState.pageSize) >= total;

                    await this.loadLatestSelectedPrices(items);
                    this.renderSupplierTable(items);

                    // Update summary
                    const summaryText = document.getElementById('supplierSummaryText');
                    if (summaryText) summaryText.textContent = `${T.Sum || 'Tổng số'}: ${total || 0}`;

                    // Render pagination
                    this.renderSupplierPaginationControls();

                    // after render
                    this.applyAdditionalColumnsVisibility();
                })
                .catch(err => console.error('Load supplier data failed', err));
        },
        openMasterPriceModal: async function (code, kind) {
            const title = document.getElementById(kind === 'history' ? 'masterPriceHistorySubtitle' : 'masterPriceEditTitle');
            if (title && kind === 'history') title.textContent = `Mã hàng nội bộ: ${code}`;
            if (kind === 'add') {
                masterPriceModalState.code = code;
                masterPriceModalState.mode = 'add';
                const now = new Date().toISOString();
                const blank = { CHR_INTERNAL_PART_CODE: code, DTM_UPLOAD: now, DTM_CREATE: now, BIT_FIX_VENDOR: false, BIT_USE_LEAVE_RATE: false };
                document.getElementById('masterPriceEditTitle').textContent = 'Nhập master giá mới';
                document.getElementById('masterPriceEditSubtitle').textContent = `Mã hàng nội bộ: ${code}`;
                document.getElementById('masterPriceSaveButton').innerHTML = '<i class="fas fa-plus me-1"></i>Thêm master giá';
                renderMasterPriceEdit(blank, code, false);
                masterPriceOverlay('edit', true);
                try {
                    const material = await loadMaterialInfoForMasterPrice(code);
                    if (masterPriceModalState.mode === 'add' && masterPriceModalState.code === code) {
                        fillMasterPriceFromMaterial(material);
                    }
                } catch (error) {
                    showDialog({ title: 'Thông báo', message: error.message || 'Không thể tải thông tin linh kiện.', type: 'error' });
                }
                return;
            }
            masterPriceModalState.mode = 'edit';
            document.getElementById('masterPriceEditTitle').textContent = 'Chỉnh sửa master giá';
            document.getElementById('masterPriceEditSubtitle').textContent = 'Đang hiển thị bản ghi mới nhất';
            document.getElementById('masterPriceSaveButton').innerHTML = '<i class="fas fa-save me-1"></i>Lưu thay đổi';
            masterPriceOverlay(kind, true);
            if (kind === 'history') renderMasterPriceHistory([]);
            try {
                const rows = await loadMasterPriceHistory(code);
                masterPriceModalState.rows = rows;
                masterPriceModalState.code = code;
                if (kind === 'history') {
                    renderMasterPriceHistory(rows);
                } else if (rows.length) {
                    renderMasterPriceEdit(rows[0]);
                } else {
                    masterPriceOverlay('edit', false);
                    showDialog({ title: 'Thông báo', message: 'Không tìm thấy bản ghi master giá cho mã hàng này.', type: 'error' });
                }
            } catch (error) {
                masterPriceOverlay(kind, false);
                showDialog({ title: 'Thông báo', message: error.message || 'Không thể tải master giá.', type: 'error' });
            }
        },
        loadLatestSelectedPrices: async function (items) {
            const codes = [...new Set(items.map(item => item.CHR_MaHangNoiBo).filter(Boolean))];
            supplierState.latestSelectedPrices = {};
            if (!codes.length) return;

            try {
                const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/GetLatestSelectedPrices', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(codes)
                });
                if (!response.ok) throw new Error(await response.text() || 'Không thể lấy giá gần nhất');
                const result = await response.json();
                const prices = Array.isArray(result?.data) ? result.data : [];
                prices.forEach(price => {
                    if (price.MaHangNoiBo) supplierState.latestSelectedPrices[price.MaHangNoiBo] = price;
                });
            } catch (error) {
                console.error('Load latest selected prices failed', error);
            }
        },
        getMasterQuoteSearchPayload: function (includePaging = true) {
            const value = id => document.getElementById(id)?.value?.trim() || '';
            const payload = {
                MaDon: value('masterQuoteMaDon'),
                MaNcc: value('masterQuoteMaNcc'),
                MaThietBi: value('masterQuoteMaThietBi'),
                MaHangNoiBo: value('masterQuoteMaHangNoiBo'),
                MaHangNcc: value('masterQuoteMaHangNcc'),
                TrangThai: value('masterQuoteTrangThai'),
                ChungLoai: value('masterQuoteChungLoai'),
                NhomHang: value('masterQuoteNhomHang'),
                from: value('masterQuoteFrom') || null,
                to: value('masterQuoteTo') || null,
                PageIndex: includePaging ? masterQuoteState.pageIndex : 0,
                PageSize: includePaging ? masterQuoteState.pageSize : 0
            };
            return payload;
        },
        loadMasterQuoteData: async function () {
            const payload = this.getMasterQuoteSearchPayload();

            const tbody = document.getElementById('masterQuoteTableBody');
            if (tbody) tbody.innerHTML = '<tr><td colspan="27" class="text-center text-muted py-5"><i class="fas fa-spinner fa-spin me-2"></i>Đang tải dữ liệu...</td></tr>';

            try {
                const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/SearchMasterQuoteInfo', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (!response.ok) throw new Error(await response.text() || 'Không thể tải dữ liệu');
                const result = await response.json();
                const rows = Array.isArray(result?.data) ? result.data : [];
                if (!rows.length && masterQuoteState.pageIndex > 1) {
                    masterQuoteState.pageIndex--;
                    return this.loadMasterQuoteData();
                }
                const count = await this.getMasterQuoteCount(payload);
                masterQuoteState.totalCount = count;
                masterQuoteState.totalPages = Math.ceil(count / masterQuoteState.pageSize);
                masterQuoteState.lastPage = masterQuoteState.pageIndex >= masterQuoteState.totalPages;
                this.renderMasterQuoteTable(rows);
                const summary = document.getElementById('masterQuoteSummaryText');
                if (summary) summary.textContent = `Số dữ liệu: ${count}`;
                this.renderMasterQuotePaginationControls();
            } catch (error) {
                console.error('Load master quote data failed', error);
                if (tbody) tbody.innerHTML = '<tr><td colspan="27" class="text-center text-danger py-5"><i class="fas fa-exclamation-circle me-2"></i>Không thể tải dữ liệu</td></tr>';
            }
        },
        ExpTemplateExcel: async function () {
            const templateUrl = `${window.location.origin}/template/MasterGia.xlsx`;
            const templateFileName = 'MasterGia.xlsx';

            showLoading('Đang tải file mẫu master giá...');
            try {
                const response = await fetch(templateUrl);
                if (!response.ok) throw new Error('Không tìm thấy file mẫu MasterGia.xlsx');

                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = templateFileName;
                document.body.appendChild(link);
                link.click();
                link.remove();
                window.URL.revokeObjectURL(url);
            } catch (error) {
                const T = window.i18nQuotationResults || {};
                showDialog({
                    title: T.Notification || 'Thông báo',
                    message: error.message || 'Không thể tải file mẫu MasterGia.xlsx',
                    type: 'error'
                });
            } finally {
                hideLoading();
            }
        },
        exportMasterQuote: async function () {
            const button = document.getElementById('btnExportfile');
            if (button) button.disabled = true;
            showLoading('Đang xuất dữ liệu master giá...');

            try {
                const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ExportMasterQuote', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(this.getMasterQuoteSearchPayload(false))
                });
                if (!response.ok) throw new Error(await response.text() || 'Không thể xuất dữ liệu master giá.');

                const blob = await response.blob();
                const contentDisposition = response.headers.get('Content-Disposition') || '';
                const fileNameMatch = contentDisposition.match(/filename\*?=(?:UTF-8'')?([^;]+)/i);
                const fileName = fileNameMatch
                    ? decodeURIComponent(fileNameMatch[1].replace(/^"|"$/g, ''))
                    : 'MasterGia.xlsx';
                const url = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                link.remove();
                URL.revokeObjectURL(url);
            } catch (error) {
                showDialog({ title: 'Thông báo', message: error.message || 'Không thể xuất dữ liệu master giá.', type: 'error' });
            } finally {
                if (button) button.disabled = false;
                hideLoading();
            }
        },
        getMasterQuoteCount: async function (payload) {
            const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/CountMasterQuoteInfo', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!response.ok) throw new Error(await response.text() || 'Không thể đếm dữ liệu');
            const result = await response.json();
            return Number(result?.data || 0);
        },
        renderMasterQuotePaginationControls: function () {
            const container = document.getElementById('masterQuotePaginationControls');
            if (!container) return;
            container.innerHTML = '';

            const totalPages = masterQuoteState.totalPages || 1;
            const previous = document.createElement('button');
            previous.type = 'button';
            previous.className = 'btn btn-sm btn-outline-secondary';
            previous.textContent = '‹';
            previous.disabled = masterQuoteState.pageIndex <= 1;
            previous.addEventListener('click', () => {
                if (masterQuoteState.pageIndex > 1) {
                    masterQuoteState.pageIndex--;
                    this.loadMasterQuoteData();
                }
            });
            container.appendChild(previous);

            const range = 2;
            const start = Math.max(1, Math.min(masterQuoteState.pageIndex - range, Math.max(1, totalPages - (range * 2))));
            for (let page = start; page <= Math.min(totalPages, start + (range * 2)); page++) {
                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'btn btn-sm ' + (page === masterQuoteState.pageIndex ? 'btn-primary' : 'btn-outline-secondary');
                button.textContent = page;
                button.addEventListener('click', () => {
                    if (page !== masterQuoteState.pageIndex) {
                        masterQuoteState.pageIndex = page;
                        this.loadMasterQuoteData();
                    }
                });
                container.appendChild(button);
            }

            const next = document.createElement('button');
            next.type = 'button';
            next.className = 'btn btn-sm btn-outline-secondary';
            next.textContent = '›';
            next.disabled = masterQuoteState.pageIndex >= totalPages || masterQuoteState.totalCount === 0;
            next.addEventListener('click', () => {
                if (!next.disabled) {
                    masterQuoteState.pageIndex++;
                    this.loadMasterQuoteData();
                }
            });
            container.appendChild(next);

            const pagingInfo = document.getElementById('masterQuotePagingInfo');
            if (pagingInfo) {
                const startItem = masterQuoteState.totalCount === 0
                    ? 0
                    : ((masterQuoteState.pageIndex - 1) * masterQuoteState.pageSize + 1);
                const endItem = masterQuoteState.totalCount === 0
                    ? 0
                    : Math.min(masterQuoteState.pageIndex * masterQuoteState.pageSize, masterQuoteState.totalCount);
                pagingInfo.textContent = `${startItem}-${endItem} / ${masterQuoteState.totalCount}`;
            }
        },
        renderMasterQuoteTable: function (rows) {
            const tbody = document.getElementById('masterQuoteTableBody');
            if (!tbody) return;
            if (!rows.length) {
                tbody.innerHTML = '<tr><td colspan="27" class="text-center text-muted py-5"><i class="fas fa-table fa-2x mb-2 d-block"></i>Chưa có dữ liệu phù hợp</td></tr>';
                return;
            }

            const get = (row, ...names) => {
                for (const name of names) if (row[name] !== undefined && row[name] !== null) return row[name];
                return '';
            };
            const decimal4 = value => {
                if (value === null || value === undefined || value === '') return '';
                const number = Number(value);
                return Number.isFinite(number) ? Number(number.toFixed(4)).toString() : value;
            };
            let stt = 1;

            tbody.innerHTML = rows.map(row => `<tr>
                <td class ="text-center">${stt++}</td>
                <td>
                    <div class="action-buttons" role="group" aria-label="${escape('Actions')}">
                        <button
                            type="button"
                            class="btn btn-view-history"
                            title="${escape('View history')}"
                            data-bivncode="${escape(get(row, 'BIVNPartCode'))}">
                            <i class="fas fa-history"></i>
                        </button>

                        <button hidden
                            type="button"
                            class="btn btn-edit-history"
                            title="${escape('Edit')}"
                            data-bivncode="${escape(get(row, 'BIVNPartCode'))}">
                            <i class="fas fa-edit text-primary"></i>
                        </button>

                        <button
                            type="button"
                            class="btn btn-add-price"
                            title="${escape('Nhập master giá mới')}"
                            data-bivncode="${escape(get(row, 'BIVNPartCode'))}">
                            <i class="fas fa-plus text-success"></i>
                        </button>
                    </div>
                </td>
                ${cell(date(get(row, 'UploadDate')))}
                ${cell(get(row, 'PICUpload'))}
                ${cell(get(row, 'QuotationRequestNumber'))}
                ${cell(get(row, 'EquipmentCode'))}
                ${cell(get(row, 'VendorCode'))}
                ${cell(get(row, 'VendorName'))}
                ${cell(get(row, 'BIVNPartCode'))}
                ${cell(get(row, 'VendorGoodCode'))}
                ${cell(get(row, 'PartNameVN'))}
                ${cell(get(row, 'PartNameEN'))}
                ${cell(get(row, 'Quantity'))}
                ${cell(get(row, 'Unit'))}
                ${cell(get(row, 'OtherRequirement'))}
                ${cell(get(row, 'MakerOrigin'))}
                ${cell(decimal4(get(row, 'UnitPriceSupplier')))}
                ${cell(get(row, 'Currency'))}
                ${cell(decimal4(get(row, 'UnitPriceUSD')))}
                ${cell(get(row, 'LeadTime'))}
                ${cell(get(row, 'MOQ'))}
                ${cell(get(row, 'DeliveryTerm'))}
                ${cell(get(row, 'PaymentTerm'))}
                ${cell(date(get(row, 'PriceEffectiveDate')))}
                ${cell(date(get(row, 'ExpiryDate')))}
                ${cell(get(row, 'VendorCode'))}
                <td></td>
            </tr>`).join('');
        },
        renderSupplierTable: function (data) {
            const tbody = document.getElementById('supplierQuoteBody');
            if (!tbody) return;

            const table = document.getElementById('supplierQuoteTable');
            if (table) {
                table.style.fontSize = '10px';
                table.style.lineHeight = '1.2';
            }
            const ensureRefuseRowStyle = () => {
                const styleId = 'supplierRefuseRowStyle';
                if (document.getElementById(styleId)) return;
                const styleEl = document.createElement('style');
                styleEl.id = styleId;
                styleEl.textContent = '#supplierQuoteTable tbody tr.refuse-row > td { background-color: #f8d7da !important; color: #721c24 !important; }\n'
                    + '#supplierQuoteTable tbody tr.cheapest-row > td { background-color: #e4f5e5 !important; }\n'
                    + '#supplierQuoteTable .price-warning { color: #9a6700; background: #fff3cd; border: 1px solid #ffecb5; border-radius: 3px; padding: 2px 4px; margin-bottom: 3px; font-size: 10px; }';
                document.head.appendChild(styleEl);
            };


            const getMismatchStyle = (isMatch, step) => {
                try {
                    const s = Number(step || 0);
                    if (s > 6 && isMatch === false) return 'color: red; background-color: #ffcccc;';
                } catch { }
                return '';
            };

            const getPrice = item => {
                const vnd = Number(item.FL_VND);
                if (Number.isFinite(vnd) && vnd > 0) return { value: vnd, currency: 'VND' };
                const usd = Number(item.FL_USD);
                if (Number.isFinite(usd) && usd > 0) return { value: usd, currency: 'USD' };
                return null;
            };
            const priceByGroup = new Map();
            data.forEach(item => {
                const price = getPrice(item);
                if (!price) return;
                const key = `${item.CHR_MaDon || ''}|${item.CHR_MaHangNoiBo || ''}`;
                const current = priceByGroup.get(key);
                if (!current || (current.currency === price.currency && price.value < current.value)) {
                    priceByGroup.set(key, price);
                }
            });

            const rowsHtml = data.map((d, index) => {

                const vnd = (d.FL_VND != null && !isNaN(Number(d.FL_VND))) ? Number(d.FL_VND) : 0;
                const usd = (d.FL_USD != null && !isNaN(Number(d.FL_USD))) ? Number(d.FL_USD) : 0;
                const sl = (d.soluong != null && !isNaN(Number(d.soluong))) ? Number(d.soluong) : 0;
                let totalCell = '';
                if (vnd && vnd !== 0) {
                    try { totalCell = Number(vnd * sl).toLocaleString(); } catch { totalCell = vnd * sl; }
                    totalCell = totalCell + ' VND';
                } else if (usd && usd !== 0) {
                    try { totalCell = Number(usd * sl).toLocaleString(); } catch { totalCell = usd * sl; }
                    totalCell = totalCell + ' USD';
                }
                const checkRefuse = (d.CHR_Status === 'Refuse') ? true : false;
                const currentPrice = getPrice(d);
                const groupKey = `${d.CHR_MaDon || ''}|${d.CHR_MaHangNoiBo || ''}`;
                const cheapestPrice = priceByGroup.get(groupKey);
                const isCheapest = !!currentPrice && !!cheapestPrice
                    && currentPrice.currency === cheapestPrice.currency
                    && currentPrice.value === cheapestPrice.value;
                const latestPrice = supplierState.latestSelectedPrices[d.CHR_MaHangNoiBo || ''];
                const previousPrice = latestPrice ? getPrice(latestPrice) : null;
                const priceIncrease = !!currentPrice && !!previousPrice
                    && currentPrice.currency === previousPrice.currency
                    && previousPrice.value > 0
                    && currentPrice.value > previousPrice.value * 1.05;
                const priceWarning = priceIncrease
                    ? 'Giá cao hơn giá đã đặt gần nhất trên 5%, bắt buộc nhập lý do.'
                    : '';
                const canShowCustomsDeclaration = window.canShowCustomsDeclaration === true;
                const customsNotificationDisabled = d.BIT_Select !== true;
                return `
                <tr class="text-center ${checkRefuse ? 'refuse-row' : ''} ${isCheapest ? 'cheapest-row' : ''}" data-madon="${escape(d.CHR_MaDon || '')}" data-mahang="${escape(d.CHR_MaHangNoiBo || '')}" data-vendor="${escape(d.CHR_MaNCC || '')}" data-id="${escape(d.ID || '')}" data-price-usd="${usd}" data-price-vnd="${vnd}" data-expiry="${escape(d.DTM_ExpiryDate || '')}" data-quote-link="${escape(d.NVCHR_File || '')}" data-price-increase="${priceIncrease}" style="text-align: center;">
                    <td style="padding: 2px 4px; text-align: center;">${index + 1}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.CHR_MaDon || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;">${d.status || ''}</td>
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
                    <td class="text-start" style="padding: 2px 4px; text-align: left;">${d.ShortName || d.NVCHR_NameNCC || ''}</td>
                    <td style="padding: 2px 4px; text-align: center;  ${getMismatchStyle(d.IsMatch_MaHangNCC)}">${d.CodeEquipmentNCC || ''}</td>
                    <td class="text-start" style="padding: 2px 4px; text-align: left; ${getMismatchStyle(d.IsMatch_NameVN)}">${d.NVCHR_TenHangHQ || ''}</td>
                    <td class="text-start" style="padding: 2px 4px; text-align: left;">${d.NameENByNCC || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_SoLuong)}">${d.soluong || ''}</td>
                    <td style="padding: 2px 4px; text-align: center; ${getMismatchStyle(d.IsMatch_DonVi)}">${d.donvi || ''}</td>
                    <td class="text-end" style="padding: 2px 4px; text-align: right;">${checkRefuse ? 'Refuse' : (d.FL_USD != null ? Number(d.FL_USD).toLocaleString() : '')}</td>
                    <td class="text-end" style="padding: 2px 4px; text-align: right;">${checkRefuse ? 'Refuse' : (d.FL_VND != null ? Number(d.FL_VND).toLocaleString() : '')}</td>
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
                    <td class="supplier-selection-cell" style="padding: 4px; text-align: center;">
                        <select class="form-control form-control-sm supplier-choice" data-madon="${d.CHR_MaDon || ''}" data-mahang="${d.CHR_MaHangNoiBo || ''}" data-id="${d.ID || ''}" aria-label="Supplier selection">
                            <option value="" ${(!d.BIT_Select && d.BIT_Select !== false) ? 'selected' : ''}></option>
                            <option value="true" ${d.BIT_Select === true ? 'selected' : ''}>O</option>
                            <option value="false" ${d.BIT_Select === false ? 'selected' : ''}>X</option>
                        </select>
                    </td>
                    ${canShowCustomsDeclaration ? `<td class="customs-notification-cell">
                        <select class="form-control form-control-sm customs-notification-choice" ${customsNotificationDisabled ? 'disabled' : ''} aria-label="Thông báo hải quan">
                            <option value="NONEED" selected>No need</option>
                            <option value="NEED">Need</option>
                        </select>
                    </td>` : ''}
                    <td class="reason-cell">
                        ${priceWarning ? `<div class="price-warning" role="alert">${priceWarning}</div>` : ''}
                        <textarea class="form-control form-control-sm reason-input" rows="2" list="${supplierReasonDatalistId}" placeholder="Nhập lý do...">${d.NVCHR_ReasonPick || ''}</textarea>
                        <button type="button" class="reason-suggestion-trigger">
                            <i class="fas fa-lightbulb"></i> Chọn gợi ý lý do
                        </button>
                    </td>
                    <td class="reason-cell"><textarea class="form-control form-control-sm reason-input" rows="2" placeholder="Nhập lý do...">${d.NVCHR_Note || ''}</textarea></td>
                </tr>
            `;
            }).join('');

            tbody.innerHTML = rowsHtml;
            this.syncCustomsNotificationChoices();
            ensureRefuseRowStyle();
            this.applyAdditionalColumnsVisibility();
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
                const reasonSuggestionTrigger = e.target.closest('.reason-suggestion-trigger');
                if (reasonSuggestionTrigger) {
                    const row = reasonSuggestionTrigger.closest('tr');
                    const reasonInput = row?.querySelector('.reason-input');
                    if (reasonInput) {
                        showPrompt({
                            title: (window.i18nQuotationResults && window.i18nQuotationResults.Reason) || 'Lý do chọn nhà cung cấp',
                            message: 'Chọn một lý do có sẵn hoặc nhập lý do khác:',
                            defaultValue: reasonInput.value,
                            options: supplierPickReasonOptions,
                            allowCustom: true
                        }).then(value => {
                            if (value === null) return;
                            reasonInput.value = value;
                            reasonInput.dispatchEvent(new Event('input', { bubbles: true }));
                            reasonInput.focus();
                        });
                    }
                    return;
                }

                const btn = e.target.closest('.toggle-sup');
                if (btn) {
                    this.toggleSupplierDetails(btn);
                }

                const detailBtn = e.target.closest('button[data-action="detail-request"]');
                if (detailBtn) {
                    const id = detailBtn.getAttribute('data-id');
                    if (id) {
                        this.openEditRequestModal(parseInt(id, 10));
                    }
                }

                const historyBtn = e.target.closest('.btn-view-history');
                const editBtn = e.target.closest('.btn-edit-history');
                if (historyBtn || editBtn) {
                    const code = (historyBtn || editBtn).dataset.bivncode;
                    if (code) this.openMasterPriceModal(code, historyBtn ? 'history' : 'edit');
                }
                const addBtn = e.target.closest('.btn-add-price');
                if (addBtn?.dataset.bivncode) this.openMasterPriceModal(addBtn.dataset.bivncode, 'add');

                const closeButton = e.target.closest('[data-master-price-close]');
                if (closeButton) masterPriceOverlay(closeButton.dataset.masterPriceClose, false);
            });

            document.addEventListener('keydown', (event) => {
                if (event.key === 'Escape') {
                    masterPriceOverlay('history', false);
                    masterPriceOverlay('edit', false);
                }
            });

            const historyFilter = document.getElementById('masterPriceHistoryFilter');
            if (historyFilter) historyFilter.addEventListener('submit', event => {
                event.preventDefault();
                const vendor = document.getElementById('historyVendorCode')?.value.trim().toLowerCase() || '';
                const quotation = document.getElementById('historyQuotationNo')?.value.trim().toLowerCase() || '';
                const from = document.getElementById('historyFrom')?.value || '';
                const to = document.getElementById('historyTo')?.value || '';
                const filtered = masterPriceModalState.rows.filter(row => {
                    const rowDate = String(masterPriceGet(row, 'DTM_UPLOAD')).slice(0, 10);
                    return (!vendor || String(masterPriceGet(row, 'CHR_VENDOR_CODE')).toLowerCase().includes(vendor))
                        && (!quotation || String(masterPriceGet(row, 'CHR_QUOTATION_REQUEST_NO')).toLowerCase().includes(quotation))
                        && (!from || rowDate >= from) && (!to || rowDate <= to);
                });
                renderMasterPriceHistory(filtered);
            });
            if (historyFilter) historyFilter.addEventListener('reset', () => setTimeout(() => renderMasterPriceHistory(masterPriceModalState.rows), 0));
            const historyExport = document.getElementById('masterPriceHistoryExport');
            if (historyExport) historyExport.addEventListener('click', exportMasterPriceHistory);

            const editForm = document.getElementById('masterPriceEditForm');
            if (editForm) editForm.addEventListener('submit', async event => {
                event.preventDefault();
                if (!validateMasterPriceEdit()) return;
                const submit = editForm.querySelector('button[type="submit"]');
                if (submit) submit.disabled = true;
                showLoading(masterPriceModalState.mode === 'add' ? 'Đang thêm master giá...' : 'Đang lưu master giá...');
                try {
                    const isAdd = masterPriceModalState.mode === 'add';
                    const dto = await uploadMasterPriceFiles(readMasterPriceEdit());
                    const response = await fetch((window.apiBaseUrl || '') + `/QuoteResults/${isAdd ? 'AddPriceMaster' : 'UpdatePriceMaster'}`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(dto) });
                    if (!response.ok) throw new Error(await response.text() || 'Không thể lưu master giá');
                    masterPriceOverlay('edit', false);
                    this.loadMasterQuoteData();
                    showDialog({ title: 'Thông báo', message: isAdd ? 'Đã thêm master giá.' : 'Đã cập nhật master giá.', type: 'success' });
                } catch (error) {
                    showDialog({ title: 'Thông báo', message: error.message || 'Không thể lưu master giá.', type: 'error' });
                } finally {
                    if (submit) submit.disabled = false;
                    hideLoading();
                }
            });

            // Filter theo trạng thái
            document.querySelectorAll('.status-option').forEach(option => {
                option.addEventListener('click', this.filterByStatus.bind(this));
            });

            // Tìm kiếm
            const btnSearch = document.getElementById('btnSearch');
            if (btnSearch) {
                btnSearch.addEventListener('click',
                    () => {
                        requestListState.pageIndex = 0;
                        this.searchItems();
                    }
                );
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
                supplierSearchBtn.addEventListener('click',
                    () => {
                        supplierState.pageIndex = 1;
                        this.loadSupplierData();
                    }
                );
            }

            const masterQuoteForm = document.getElementById('masterQuoteFilterForm');
            if (masterQuoteForm) masterQuoteForm.addEventListener('submit', (event) => {
                event.preventDefault();
                masterQuoteState.pageIndex = 1;
                this.loadMasterQuoteData();
            });

            const masterQuoteClearBtn = document.getElementById('masterQuoteClearBtn');
            if (masterQuoteClearBtn) masterQuoteClearBtn.addEventListener('click', () => {
                masterQuoteState.pageIndex = 1;
                setTimeout(() => this.loadMasterQuoteData(), 0);
            });

            const masterQuoteExpTemBtn = document.getElementById('masterQuoteExpTemBtn');
            if (masterQuoteExpTemBtn) masterQuoteExpTemBtn.addEventListener('click', (event) => {
                event.preventDefault();
                this.ExpTemplateExcel();
            });

            const masterPriceImportButton = document.getElementById('btnImportfile');
            const masterPriceImportInput = document.getElementById('masterPriceImportFile');
            if (masterPriceImportButton && masterPriceImportInput) {
                masterPriceImportButton.addEventListener('click', () => masterPriceImportInput.click());
                masterPriceImportInput.addEventListener('change', async () => {
                    const file = masterPriceImportInput.files?.[0];
                    masterPriceImportInput.value = '';
                    if (!file) return;
                    if (!file.name.toLowerCase().endsWith('.xlsx')) {
                        showDialog({ title: 'Thông báo', message: 'Vui lòng chọn file Excel định dạng .xlsx.', type: 'error' });
                        return;
                    }

                    masterPriceImportButton.disabled = true;
                    showLoading('Đang import master giá...');
                    try {
                        const formData = new FormData();
                        formData.append('file', file);
                        const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ImportPriceMaster', { method: 'POST', body: formData });
                        if (!response.ok) {
                            const contentType = response.headers.get('Content-Type') || '';
                            if (contentType.includes('spreadsheetml.sheet')) {
                                const blob = await response.blob();
                                const url = URL.createObjectURL(blob);
                                const link = document.createElement('a');
                                link.href = url;
                                link.download = 'ImportMasterGia_Errors.xlsx';
                                document.body.appendChild(link);
                                link.click();
                                link.remove();
                                URL.revokeObjectURL(url);
                                showDialog({ title: 'Thông báo', message: 'File có dòng dữ liệu sai. File lỗi đã được tải xuống.', type: 'error' });
                            } else {
                                throw new Error(await response.text() || 'Không thể import master giá.');
                            }
                            return;
                        }
                        const result = await response.json();
                        await this.loadMasterQuoteData();
                        showDialog({ title: 'Thông báo', message: result?.message || 'Import master giá thành công.', type: 'success' });
                    } catch (error) {
                        showDialog({ title: 'Thông báo', message: error.message || 'Không thể import master giá.', type: 'error' });
                    } finally {
                        masterPriceImportButton.disabled = false;
                        hideLoading();
                    }
                });
            }

            const masterPriceExportButton = document.getElementById('btnExportfile');
            if (masterPriceExportButton) {
                masterPriceExportButton.addEventListener('click', this.exportMasterQuote.bind(this));
            }

            const masterQuotePageSize = document.getElementById('masterQuotePageSize');
            if (masterQuotePageSize) masterQuotePageSize.addEventListener('change', () => {
                masterQuoteState.pageSize = parseInt(masterQuotePageSize.value, 10) || 20;
                masterQuoteState.pageIndex = 1;
                this.loadMasterQuoteData();
            });

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

            document.addEventListener('change', (e) => {
                const cb = e.target.closest('.supplier-select');
                if (cb) {
                    if (!cb.checked) return;
                    const row = cb.closest('tr');
                    const groupContainer = row?.closest('.supplier-group');
                    const groupId = groupContainer?.id?.replace('sup-rows-', '') || '';
                    const maDon = groupId.split('-')[0] || '';
                    const maHang = groupId.split('-')[1] || '';
                    if (!maDon) return;
                    document.querySelectorAll('.supplier-group[id^="sup-rows-' + maDon + '-"] .supplier-select').forEach(other => {
                        if (other !== cb) other.checked = false;
                    });

                    const supplierId = cb.getAttribute('data-id') || cb.value || '';
                    if (supplierId) {
                        const selMatch = document.querySelector(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"][data-id="${supplierId}"]`);
                        if (selMatch) {
                            const reasonEl = row.querySelector('.reason-input');
                            const reason = reasonEl?.value?.trim() || '';
                            if (!reason) {
                                showPrompt({
                                    title: (window.i18nQuotationResults && window.i18nQuotationResults.Reason) || 'Lý do',
                                    message: (window.i18nQuotationResults && window.i18nQuotationResults.PromptEnterReason) || 'Vui lòng nhập lý do chọn nhà cung cấp',
                                    placeholder: '',
                                    allowCustom: true,
                                    options: supplierPickReasonOptions
                                })
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
                            document.querySelectorAll(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"]`).forEach(s => { if (s !== selMatch) s.value = 'false'; });
                        }
                    }
                    return;
                }
                const sel = e.target.closest('.supplier-choice');
                if (sel) {
                    const val = sel.value;
                    const row = sel.closest('tr');
                    const maDon = row?.getAttribute('data-madon') || '';
                    const maHang = row?.getAttribute('data-mahang') || '';
                    const reasonEl = row?.querySelector('.reason-input');
                    if (val === 'true') {
                        const reason = reasonEl?.value?.trim() || '';
                        const priceIncrease = row?.getAttribute('data-price-increase') === 'true';
                        if (!reason) {
                            showPrompt({
                                title: (window.i18nQuotationResults && window.i18nQuotationResults.Reason) || 'Lý do',
                                message: priceIncrease
                                    ? 'Giá cao hơn giá đã đặt gần nhất trên 5%. Vui lòng nhập lý do.'
                                    : ((window.i18nQuotationResults && window.i18nQuotationResults.PromptEnterReason) || 'Vui lòng nhập lý do chọn nhà cung cấp'),
                                placeholder: '',
                                allowCustom: true,
                                options: supplierPickReasonOptions
                            })
                                .then(r => {
                                    if (!r) {
                                        try { sel.value = ''; } catch { }
                                        try { reasonEl && reasonEl.focus(); } catch { }
                                        this.syncCustomsNotificationChoices();
                                        return;
                                    }
                                    try { reasonEl.value = r; } catch { }
                                    document.querySelectorAll(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"]`).forEach(s => {
                                        if (s !== sel) s.value = 'false';
                                    });
                                    this.syncCustomsNotificationChoices();
                                });
                            return;
                        }
                        document.querySelectorAll(`select.supplier-choice[data-madon="${maDon}"][data-mahang="${maHang}"]`).forEach(s => {
                            if (s !== sel) s.value = 'false';
                        });
                    }
                    this.syncCustomsNotificationChoices();
                }
            });
        },
        syncCustomsNotificationChoices: function () {
            document.querySelectorAll('#supplierQuoteBody tr').forEach(row => {
                const supplierChoice = row.querySelector('.supplier-choice');
                const customsChoice = row.querySelector('.customs-notification-choice');
                if (!supplierChoice || !customsChoice) return;

                const isSelectedSupplier = supplierChoice.value === 'true';
                customsChoice.disabled = !isSelectedSupplier;
                if (!isSelectedSupplier) customsChoice.value = 'NONEED';
            });
        },
        saveTab2: async function () {
            const T = window.i18nQuotationResults || {};
            const btn = document.getElementById('SaveTab2');
            try {
                if (btn) btn.disabled = true;
                const rows = Array.from(document.querySelectorAll('#supplierQuoteBody tr'));
                const payload = [];
                let missingPriceReason = false;
                rows.forEach(row => {
                    const sel = row.querySelector('select.supplier-choice');
                    if (!sel) return;
                    const val = sel.value;
                    if (val === '') return;
                    const idAttr = row.getAttribute('data-id') || sel.getAttribute('data-id') || '';
                    const id = idAttr !== '' && !isNaN(Number(idAttr)) ? Number(idAttr) : idAttr;
                    const reason = (row.querySelector('.reason-input')?.value || '').toString();
                    if (val === 'true' && row.getAttribute('data-price-increase') === 'true' && !reason.trim()) {
                        missingPriceReason = true;
                    }
                    const maDon = row.getAttribute('data-madon') || sel.getAttribute('data-madon') || '';
                    const maHang = row.getAttribute('data-mahang') || sel.getAttribute('data-mahang') || '';
                    payload.push({
                        ID: id,
                        BIT_Select: (val === 'true'),
                        CustomsDeclaration: row.querySelector('.customs-notification-choice')?.value || 'NONEED',
                        NVCHR_ReasonPick: reason,
                        NVCHR_NameNCC: row.getAttribute('data-vendor') || '',
                        CHR_MaHangNCC: maHang,
                        CHR_MaDon: maDon,
                        CHR_MaThietBi: row.querySelector('td:nth-child(4)')?.textContent?.trim() || '',
                        NVCHR_ChungLoai: row.querySelector('td:nth-child(8)')?.textContent?.trim() || '',
                        FL_USD: Number(row.getAttribute('data-price-usd') || 0) || null,
                        FL_VND: Number(row.getAttribute('data-price-vnd') || 0) || null,
                        DTM_ExpiryDate: row.getAttribute('data-expiry') || null,
                        NVCHR_File: row.getAttribute('data-quote-link') || null
                    });
                });
                if (missingPriceReason) {
                    showDialog({
                        title: T.Notification || 'Thông báo',
                        message: 'Các mặt hàng có giá cao hơn giá đã đặt gần nhất trên 5% bắt buộc phải nhập lý do.',
                        type: 'error'
                    });
                    return;
                }
                const warnings = getSupplierValidationWarnings(rows);
                if (warnings.length) {
                    const action = await showSupplierValidationDialog(warnings);
                    if (action === 'export') exportSupplierWarnings(warnings);
                    if (action !== 'continue') return;
                }
                const approverNext = window.__selectedNextApprover || '';
                if (!payload.length) {
                    showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgWarnSelectOne || 'Vui lòng chọn ít nhất một nhà cung cấp.'), type: 'info' });
                    return;
                }
                var payloadWithApprover = { UserApproverNext: approverNext, listPick: payload };
                showLoading((T && T.LoadingData) ? T.LoadingData : 'Đang lưu...');
                const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/SavePickSupplier', {
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

        openApproverSelector: function (stepNumber, sectionCode) {
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

                    const body = { Step: stepNumber, SectionCost: sectionCode };
                    let list = [];
                    try {
                        const resp = await fetch((window.apiBaseUrl || '') + '/QuoteResults/GetListApprovel', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                            body: JSON.stringify(body)
                        });
                        if (resp.ok) {
                            const data = await resp.json();
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
                const approverId = selected.CHR_UserAdid ?? selected.chR_UserAdid ?? selected.CHR_Adid ?? selected.chR_Adid ?? selected.ADID ?? selected.Id ?? selected.id ?? selected.value ?? '';
                const finalId = approverId || (selected.value || selected.Value || '');
                if (!finalId) {
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
            const isHidden = toggleBtn.textContent.includes(T.HideDetails || 'Ẩn');

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
                const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/SearchID', {
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
                const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ExportFileExcelQuotationResult', {
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
                fetch((window.apiBaseUrl || '') + '/QuoteResults/ImportApprovalQuotianExcel', {
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
                                try { hideLoading(); } catch { }
                                showDialog({ title: T.Notification || 'Thông báo', message: (T.FileHasErrorsDownloaded || 'File có lỗi. Đã tải xuống file lỗi để kiểm tra.'), type: 'warning' });
                            });
                        } else {
                            // Thành công
                            return response.json().then(async data => {
                                try {
                                    if (data && data.requiresSelection) {
                                        try { hideLoading(); } catch { }
                                        const selected = await quotationApp.openApproverSelector(data.Step || 10, '');
                                        if (!selected) {
                                            showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgCancelled || 'Đã hủy lựa chọn người phê duyệt'), type: 'info' });
                                            return;
                                        }
                                        const approverId = selected.CHR_UserAdid || selected.chR_UserAdid || selected.ADID || selected.Id || selected.id || selected.value || '';
                                        const payload = { SelectedApprover: approverId, Items: data.items };
                                        try { showLoading((window.i18nQuotationResults && window.i18nQuotationResults.LoadingData) || 'Đang xử lý...'); } catch { }
                                        const res2 = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ConfirmImportedApprovals', {
                                            method: 'POST',
                                            headers: { 'Content-Type': 'application/json' },
                                            body: JSON.stringify(payload)
                                        });
                                        try { hideLoading(); } catch { }
                                        if (!res2.ok) {
                                            const txt = await res2.text().catch(() => 'Lỗi server');
                                            showDialog({ title: T.Notification || 'Thông báo', message: txt || (T.MsgSaveError || 'Lỗi khi xử lý phê duyệt'), type: 'error' });
                                            return;
                                        }
                                        showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Nhập file thành công'), type: 'success' });
                                        try { quotationApp.reloadTables(); } catch (e) { }
                                        return;
                                    }
                                } catch (err) {
                                    console.error('Post-import approver flow failed', err);
                                }
                                showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Nhập file thành công'), type: 'success' });
                                try { quotationApp.reloadTables(); } catch (e) { }
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
            try {
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
                fetch((window.apiBaseUrl || '') + '/QuoteResults/ImportQuotianExcel', {
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
                            return response.json().then(async data => {
                                if (data && data.requiresWarning) {
                                    const warnings = (data.warnings || []).map(item => ({
                                        row: { dataset: { madon: item.maDon || '', mahang: item.maHangNoiBo || '', vendor: item.vendorCode || '' } },
                                        reason: item.reason || ''
                                    }));
                                    try { hideLoading(); } catch { }
                                    const action = await showSupplierValidationDialog(warnings);
                                    if (action === 'export') exportSupplierWarnings(warnings);
                                    if (action !== 'continue') return;
                                    try { showLoading((window.i18nQuotationResults && window.i18nQuotationResults.LoadingData) || 'Đang xử lý...'); } catch { }
                                    const confirmResponse = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ConfirmImportedSupplier', {
                                        method: 'POST',
                                        headers: { 'Content-Type': 'application/json' },
                                        body: JSON.stringify({ items: data.items || [], userNextApproval: data.userNextApproval || '' })
                                    });
                                    if (!confirmResponse.ok) {
                                        throw new Error(await confirmResponse.text() || 'Lỗi khi lưu dữ liệu import');
                                    }
                                    showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Nhập file thành công'), type: 'success' });
                                    try { quotationApp.reloadTables(); } catch (e) { }
                                    return;
                                }
                                showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Nhập file thành công'), type: 'success' });
                                try { quotationApp.reloadTables(); } catch (e) { }
                            });
                        }
                    })
                    .catch(error => {
                        const T = window.i18nQuotationResults || {};
                        try { hideLoading(); } catch { }
                        showDialog({ title: T.Notification || 'Thông báo', message: (error && error.message) ? error.message : (T.ErrorPrefix || 'Không thể xuất file'), type: 'error' });
                    })
                    .finally(() => {
                        try { hideLoading(); } catch { }
                        document.body.removeChild(fileInput);
                    });
            });
            this.loadSupplierData();
            try {
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

            const icon = button.querySelector('i');
            if (icon) {
                icon.classList.toggle('fa-chevron-down');
                icon.classList.toggle('fa-chevron-up');
            }

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
                    const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/SearchInputQuote', {
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
                                    <td><input type="text" class="form-control reason-input" list="${supplierReasonDatalistId}" /></td>    
                                </tr>`;
                    }).join('');
                    bodyEl.innerHTML = rowsHtml;
                    bodyEl.dataset.loaded = 'true';
                } catch (err) {
                    console.error('Load supplier details failed', err);
                }
            }
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
                const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/GetThongTinBaoGiaGomNhom', {
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
                            <td class="text-center">${i.DTM_KyHan ? new Date(i.DTM_KyHan).toLocaleDateString('vi-VN') : ''}</td>
                            <td class="text-center">${i.DTM_NgayMuonNhan ? new Date(i.DTM_NgayMuonNhan).toLocaleDateString('vi-VN') : ''}</td>
                            <td class="text-center"><span class="badge status-badge ${statusClass}">${mapNameStatus}</span></td>
                        </tr>
                     `;
                    //                            <td>${i.categoryList || ''}</td>
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

                this.applyAdditionalColumnsVisibility();
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
            // If user checked "selectAll" we should send an empty list to indicate export all
            const selectAllCheckbox = document.getElementById('selectAll');
            const isSelectAll = !!(selectAllCheckbox && selectAllCheckbox.checked);

            const selectedMaDons = [];

            if (!isSelectAll) {
                // Lấy tất cả checkbox có class 'row-select' và đã được checked
                const checkboxes = document.querySelectorAll('#quotationResultsTableBody .row-select:checked');
                checkboxes.forEach(checkbox => {
                    const maDon = checkbox.getAttribute('data-madon-select');
                    if (maDon) selectedMaDons.push(maDon);
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
            }
            try {
                const T = window.i18nQuotationResults || {};
                showLoading(T.LoadingData || 'Đang xử lý...');
                const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ExportFileExcelApproverResult', {
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
        openApprovalModal: async function (maDon) {
            if (!maDon) return;
            try {
                const T = window.i18nQuotationResults || {};
                const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/GetSupplierApprovalInfor', {
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

                let master = data;
                if (Array.isArray(data) && data.length > 0) {
                    master = data[0];
                }

                try { document.getElementById('madonhang').textContent = getVal(master, 'CHR_MaDon', 'chR_MaDon', 'ID') || maDon; } catch { }
                try { document.getElementById('khoi').textContent = getVal(master, 'CHR_SectionName', 'chR_SectionName') || ''; } catch { }
                try { document.getElementById('mpb_yc').textContent = getVal(master, 'CHR_SectionCode', 'chR_SectionCode', 'chR_CostCenter') || ''; } catch { }
                try { document.getElementById('tenphongban').textContent = getVal(master, 'CHR_SectionName', 'chR_SectionName') || ''; } catch { }
                try { document.getElementById('nyc').textContent = formatDate(getVal(master, 'DTM_NgayMuonNhan', 'dtM_NgayMuonNhan', 'DTM_CreateDate')); } catch { }
                try { document.getElementById('thmm').textContent = formatDate(getVal(master, 'DTM_KyHan', 'dtM_KyHan', 'DTM_Deadline')); } catch { }
                try { document.getElementById('requester').textContent = getVal(master, 'CHR_CreateBy', 'chR_CreateBy') || '-'; } catch { }
                try { document.getElementById('id_request').textContent = getVal(master, 'ID', 'iD', 'CHR_MaDon') || maDon; } catch { }
                try { document.getElementById('step').textContent = getVal(master, 'ID_StepBaoGia', 'iD_StepBaoGia') || ''; } catch { }

                try {
                    const ub = document.getElementById('urgent-badge');
                    const gap = getVal(data, 'CHR_Gap', 'chR_Gap');
                    const isUrgent = gap === true || String(gap).toLowerCase() === 'true' || String(gap) === '1' || String(gap).toLowerCase() === 'o';
                    if (ub) ub.style.display = isUrgent ? '' : 'none';
                } catch { }

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
                    tr.appendChild(addTd(getVal(d, 'ShortName', 'shortName'), 'text-start'));
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
                    reasonInput.placeholder = T.InputReasonTitle + '...';
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
                            showDialog({ message: T.NoData || 'Không có dữ liệu phê duyệt.', type: 'warning' });
                            return;
                        }

                        // Kiểm tra xem step hiện tại có cần chọn người phê duyệt tiếp theo không
                        const nextStep = Number(getVal(master, 'ID_StepBaoGia', 'iD_StepBaoGia'));
                        let userApproverNext = '';

                        const flowCode = String(getVal(master, 'FlowCode', 'flowCode') || '').trim().toUpperCase();
                        const selectedTotalVnd = details
                            .filter(detail => getVal(detail, 'BIT_Select', 'bit_Select') === true)
                            .reduce((total, detail) => {
                                const totalAfterTax = Number(getVal(detail, 'FL_TotalAfterTax', 'fl_TotalAfterTax'));
                                const totalVnd = Number(getVal(detail, 'FL_Sum', 'fl_Sum'));
                                const unitPriceVnd = Number(getVal(detail, 'FL_VND', 'fl_vnd'));
                                const quantity = Number(getVal(detail, 'INT_SoLuong', 'inT_SoLuong', 'soluong')) || 0;
                                const value = Number.isFinite(totalAfterTax) && totalAfterTax > 0
                                    ? totalAfterTax
                                    : (Number.isFinite(totalVnd) && totalVnd > 0 ? totalVnd : unitPriceVnd * quantity);
                                return total + (Number.isFinite(value) ? value : 0);
                            }, 0);
                        const gaNeedsQltc = flowCode === 'GA' && selectedTotalVnd >= 100000000;

                        // PUR always continues to QLTC at step 9. GA only does so at 100 million VND or above.
                        if (nextStep === 9 && (flowCode !== 'GA' || gaNeedsQltc)) {
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

                            const response = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ConfirmApprover', {
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
                            showDialog({ message: 'Error Approval: ' + error, type: 'error' });
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
        reloadTables: function () {
            try { this.loadSupplierData(); } catch (e) { /* ignore */ }
            try { this.searchItems(); } catch (e) { /* ignore */ }
        },
        confirmSelection: async function () {
            const selections = this.getSelections();
            if (!selections.length) {
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgWarnSelectOne || 'Vui lòng chọn ít nhất một nhà cung cấp.'), type: 'info' });
                return;
            }

            const missingReasons = selections.filter(x => (!x.NVCHR_ReasonPick || x.NVCHR_ReasonPick.trim() === '')).length;
            if (missingReasons > 0) {
                const T = window.i18nQuotationResults || {};
                showDialog({ title: T.Notification || 'Thông báo', message: (T.MsgMissingReasons || 'Có {0} lựa chọn chưa nhập lý do.').replace('{0}', missingReasons), type: 'info' });
                return;
            }
            try {
                const res = await fetch((window.apiBaseUrl || '') + '/QuoteResults/ChonNhaCungCapBaoGia', {
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
                try { this.loadSupplierData(); } catch (e) { }

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
            fetch((window.apiBaseUrl || '') + '/QuoteResults/ExportSelection', {
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
            modalEl.style.display = 'block';
            modalEl.classList.add('show');
            modalEl.setAttribute('aria-hidden', 'false');
            document.body.classList.add('modal-open');
        }
    } catch {
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
function showPrompt({ title = (window.i18nQuotationResults && window.i18nQuotationResults.Notification) || 'Thông báo', message = '', placeholder = '', defaultValue = '', options = null, allowCustom = false } = {}) {
    return new Promise((resolve) => {
        const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
        if (!overlay) {
            if (Array.isArray(options) && options.length) {
                const listText = options.map((x, i) => `${i + 1}. ${x}`).join('\n');
                const val = window.prompt(`${message || title}\n\n${listText}`, defaultValue || '');
                resolve(val === null ? null : (val || '').toString());
                return;
            }
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
        const useOptions = Array.isArray(options) && options.length > 0;
        let inp;
        if (useOptions) {
            if (allowCustom) {
                inp = document.createElement('input');
                inp.type = 'text';
                inp.className = 'form-control';
                inp.placeholder = placeholder || 'Nhập hoặc chọn lý do';
                inp.value = defaultValue || '';
                inp.setAttribute('aria-label', 'Lý do chọn nhà cung cấp');
                container.appendChild(inp);

                const optionsPanel = document.createElement('div');
                optionsPanel.className = 'cm-prompt-reason-options';
                optionsPanel.setAttribute('role', 'listbox');
                const optionButtons = options.map(opt => {
                    const button = document.createElement('button');
                    const parts = String(opt).split('_');
                    const english = parts.shift() || '';
                    const vietnamese = parts.join('_');
                    button.type = 'button';
                    button.className = 'cm-prompt-reason-option';
                    button.setAttribute('role', 'option');
                    button.dataset.value = opt;
                    const englishEl = document.createElement('span');
                    englishEl.className = 'cm-prompt-reason-en';
                    englishEl.textContent = english;
                    button.appendChild(englishEl);
                    if (vietnamese) {
                        const vietnameseEl = document.createElement('span');
                        vietnameseEl.className = 'cm-prompt-reason-vi';
                        vietnameseEl.textContent = vietnamese;
                        button.appendChild(vietnameseEl);
                    }
                    button.addEventListener('click', () => {
                        inp.value = opt;
                        optionButtons.forEach(item => item.classList.toggle('is-selected', item === button));
                        inp.focus();
                    });
                    optionsPanel.appendChild(button);
                    return button;
                });
                inp.addEventListener('input', () => {
                    const query = inp.value.trim().toLowerCase();
                    optionButtons.forEach(button => {
                        button.hidden = query && !button.dataset.value.toLowerCase().includes(query);
                    });
                });
                container.appendChild(optionsPanel);
            } else {
                inp = document.createElement('select');
                inp.className = 'form-select';
                const placeholderOpt = document.createElement('option');
                placeholderOpt.value = '';
                placeholderOpt.textContent = placeholder || ('');
                inp.appendChild(placeholderOpt);
                options.forEach(opt => {
                    const optionEl = document.createElement('option');
                    optionEl.value = opt;
                    optionEl.textContent = opt;
                    inp.appendChild(optionEl);
                });
                if (defaultValue) inp.value = defaultValue;
            }
        } else {
            inp = document.createElement('input');
            inp.type = 'text';
            inp.className = 'form-control';
            inp.placeholder = placeholder || '';
            inp.value = defaultValue || '';
        }
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
        setTimeout(() => {
            try {
                inp.focus();
                if (!useOptions && typeof inp.select === 'function') inp.select();
            } catch { }
        }, 50);
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
function initEnhancements(root) {
    try {
        if (window.KanziSearchableDropdown && typeof window.KanziSearchableDropdown.init === 'function') {
            window.KanziSearchableDropdown.init(root || document);
        } else {
            buildSearchableDropdown(root || document);
        }
    } catch (e) {

    }

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
