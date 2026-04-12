// JS for Quote page: handle buttons, validations, row operations, autofill from material selection, and API calls
(() => {
    // ==================== CONSTANTS & CONFIGURATION ====================
    const CONFIG = {
        ROWS_PER_PAGE: 5,
        PAGE_SIZE: 200,
        DEBOUNCE_DELAY: 300,
        SCROLL_THROTTLE: 150,
        SEARCH_DELAY: 250,
        VIETNAM_TZ_OFFSET: 7,
    };

    const SELECTORS = {
        TABLE_BODY: '#quoteTableBody',
        FILTER_INPUT: '.filter-input',
        BTN_ADD_ROW: '#btnAddRow',
        BTN_RESET: '#btnReset',
        BTN_CREATE: '#btnCreate',
        BTN_AUTO: '#btnAuto',
        BTN_DOWN_EXCEL: '#btnDownExcelTable',
        BTN_CLEAR_FILTERS: '#btnClearFilters',
        BTN_UPLOAD_EXCEL: '#btnUploadExcel',
        BTN_DOWNLOAD_EXCEL: '#btnDownloadExcel',
        ROWS_PER_PAGE_SELECT: '#rowsPerPageSelect',
        PAGINATION_CONTROLS: '#paginationControls',
        PREV_PAGE: '#prevPage',
        NEXT_PAGE: '#nextPage',
        PAGE_INFO: '#pageInfo',
        PAGE_NUMBER_INFO: '#pageNumberInfo',
        PAGINATION_INFO: '#paginationInfo',
        APPROVER_SELECT: '#approverSelect',
        QUOTE_FORM: '#quoteForm',
        EXCEL_UPLOAD: '#excelUpload',
    };

    const api = {
        insertListBaoGia: '/Quote/InsertDanhSachBaoGia',
        getMaterials: (keyword) => `/Quote/GetMaterialsByNameOrCode?keyword=${encodeURIComponent(keyword || '')}`,
        searchMaterials: '/Quote/GetSearchMaterial',
        getSuppliersByMaHang: '/Quote/GetNhaCungCapByMaHang',
        uploadQuoteExcel: '/Quote/UploadQuoteExcel',
        exportAutoRender: '/Quote/ExportAutoRender',
        getNCCByCategory: '/Quote/GetNCCByCategory',
        exportRenderOutSide: '/Quote/ExportRenderOutSide',
        exportTable: '/Quote/ExportTable',
        searchApprover: '/Quote/GetListApprovel'
    };

    // Add base URL if exists
    if (window.apiBaseUrl) {
        Object.keys(api).forEach(key => {
            if (typeof api[key] === 'string') api[key] = window.apiBaseUrl + api[key];
            else if (typeof api[key] === 'function') {
                const original = api[key];
                api[key] = (...args) => window.apiBaseUrl + original(...args);
            }
        });
    }

    // ==================== STATE MANAGEMENT ====================
    let state = {
        currentPage: 1,
        rowsPerPage: CONFIG.ROWS_PER_PAGE,
        filteredRows: [],
        allQuoteItems: [],
        filteredQuoteItems: [],
        sectionNameFirst: '',
    };

    // ==================== UTILITY FUNCTIONS ====================
    const qs = (sel, root = document) => root.querySelector(sel);
    const qsa = (sel, root = document) => Array.from(root.querySelectorAll(sel));

    const getVietnamTime = () => {
        const now = new Date();
        const utc = now.getTime() + (now.getTimezoneOffset() * 60000);
        return new Date(utc + (CONFIG.VIETNAM_TZ_OFFSET * 60 * 60000));
    };

    const toVietnamISOString = (date) => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}+07:00`;
    };

    const formatDateForInput = (date) => {
        if (!date) return '';
        try {
            let dt = date instanceof Date ? date : new Date(date);
            if (isNaN(dt)) {
                const match = String(date).match(/^(\d{2})[\/\-](\d{2})[\/\-](\d{4})/);
                if (match) dt = new Date(parseInt(match[3]), parseInt(match[2]) - 1, parseInt(match[1]));
            }
            if (isNaN(dt)) return '';
            return `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}-${String(dt.getDate()).padStart(2, '0')}`;
        } catch {
            return '';
        }
    };

    const generateMaDonRequest = (section) => {
        try {
            const nowVN = getVietnamTime();
            const yyyy = nowVN.getFullYear();
            const MM = String(nowVN.getMonth() + 1).padStart(2, '0');
            const dd = String(nowVN.getDate()).padStart(2, '0');
            const sec = (section || '').toString().trim().replace(/[^a-zA-Z0-9_-]/g, '_') || 'GEN';
            return `RQ_${sec}_${yyyy}_${MM}_${dd}`;
        } catch {
            return `RQ_GEN_${Date.now()}`;
        }
    };

    // ==================== LOADING & DIALOG ====================
    const showLoading = (message) => {
        const el = document.getElementById('globalLoading');
        if (!el) return;
        const msgEl = el.querySelector('.loader-msg');
        if (msgEl && message) msgEl.textContent = message;
        el.style.display = 'flex';
        el.setAttribute('aria-hidden', 'false');
    };

    const hideLoading = () => {
        const el = document.getElementById('globalLoading');
        if (!el) return;
        el.style.display = 'none';
        el.setAttribute('aria-hidden', 'true');
        const msgEl = el.querySelector('.loader-msg');
        if (msgEl) msgEl.textContent = 'Đang xử lý...';
    };

    const getDialogEls = () => ({
        overlay: document.getElementById('cmDialogOverlay'),
        titleEl: document.getElementById('cmDialogTitle'),
        bodyEl: document.getElementById('cmDialogBody'),
        footerEl: document.getElementById('cmDialogFooter')
    });

    const hideDialog = () => {
        const { overlay } = getDialogEls();
        if (overlay) {
            overlay.style.display = 'none';
            overlay.setAttribute('aria-hidden', 'true');
        }
    };

    const showDialog = ({ title = 'Thông báo', message = '', type = 'info' }) => {
        const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
        if (!overlay) return alert(message);

        if (overlay.parentElement !== document.body) document.body.appendChild(overlay);

        const T = window.i18nQuote || {};
        titleEl.textContent = title;

        const iconMap = {
            success: 'fa-check-circle text-success',
            error: 'fa-exclamation-circle text-danger',
            info: 'fa-info-circle text-primary'
        };

        bodyEl.innerHTML = `<div class="d-flex align-items-start gap-2">
            <i class="fas ${iconMap[type] || iconMap.info}"></i>
            <div>${message}</div>
        </div>`;

        footerEl.innerHTML = '';
        const okBtn = document.createElement('button');
        okBtn.className = 'cm-btn cm-btn-primary';
        okBtn.textContent = T.DialogOk || 'Đồng ý';
        okBtn.addEventListener('click', hideDialog);
        footerEl.appendChild(okBtn);

        overlay.style.display = 'flex';
        overlay.setAttribute('aria-hidden', 'false');
    };

    // ==================== TABLE ROW OPERATIONS ====================
    const renumberRows = () => {
        qsa('#quoteTableBody tr').forEach((tr, idx) => {
            const noCell = tr.children[0];
            if (noCell) noCell.textContent = String(idx + 1);
        });
        assignRowIds();
    };

    const assignRowIds = () => {
        const fields = [
            'tenPhongBanTb', 'chungLoaiTb', 'tenPhanLoaiTb', 'maHangNoiBo', 'maThietBi',
            'maHangNCC', 'tenHangVN', 'tenHangEN', 'soLuong', 'donVi', 'hinhDang', 'chatLieu',
            'thanhPhan', 'kichThuoc', 'viTriSuDung', 'tinhNang', 'rohsTb', 'CoCqTb', 'msds',
            'tieuChuanAnToan', 'fileThietKe', 'nsx', 'nhaCungCapTb', 'laybaogiaTb', 'gapTb', 'nguoiYeuCauRow'
        ];

        qsa('#quoteTableBody tr').forEach((tr, idx) => {
            const rowNum = idx + 1;
            fields.forEach(field => {
                const el = tr.querySelector(`.${field}`);
                if (el) el.id = `${field}_${rowNum}`;
            });

            const dateInputs = qsa('input[type="date"]', tr);
            if (dateInputs[0]) dateInputs[0].id = `ngayMuonNhan_${rowNum}`;
            if (dateInputs[1]) dateInputs[1].id = `kyHanChonNCC_${rowNum}`;
        });
    };

    const isRowEmpty = (tr) => {
        if (!tr) return true;

        const inputs = qsa('input, textarea', tr);
        for (const inp of inputs) {
            if (!['hidden', 'file', 'checkbox', 'radio'].includes(inp.type) && inp.value?.trim()) return false;
        }

        const ignoreVals = new Set(['', 'true', 'false', 'No Need']);
        const selects = qsa('select', tr);
        for (const sel of selects) {
            if (!ignoreVals.has(sel.value?.trim())) return false;
        }

        return true;
    };

    const updateSearchableSelectDisplay = (sel) => {
        if (!sel?.nextElementSibling?.classList?.contains('ms-container')) return;
        const wrapper = sel.nextElementSibling;
        const values = wrapper.querySelector('.ms-values');
        const placeholder = wrapper.querySelector('.ms-placeholder');
        const opt = Array.from(sel.options).find(o => o.value === sel.value);

        if (opt?.text) {
            if (values) values.textContent = opt.text;
            if (placeholder) placeholder.textContent = '';
        } else {
            if (values) values.textContent = '';
            if (placeholder) placeholder.textContent = '-- Chọn --';
        }
    };

    const setSelectValueByText = (select, textOrValue) => {
        if (!select) return;
        const val = textOrValue ?? '';

        let opt = Array.from(select.options).find(o => o.value === val);
        if (!opt) {
            opt = Array.from(select.options).find(o =>
                (o.text || '').toLowerCase() === val.toLowerCase() ||
                (o.text || '').toLowerCase().startsWith(val.toLowerCase())
            );
        }

        if (opt) {
            select.value = opt.value;
            updateSearchableSelectDisplay(select);
        }
    };

    const addRow = () => {
        const tbody = qs(SELECTORS.TABLE_BODY);
        const lastRow = tbody.lastElementChild;
        if (!lastRow) return;

        const newRow = lastRow.cloneNode(true);

        // Reset inputs
        qsa('input', newRow).forEach(inp => {
            inp.value = '';
            inp.classList.remove('is-invalid');
        });

        // Reset selects
        qsa('select', newRow).forEach(sel => {
            if (sel.classList.contains('rohsTb')) sel.value = 'No Need';
            else if (sel.classList.contains('laybaogiaTb')) sel.value = 'true';
            else if (sel.classList.contains('gapTb')) sel.value = 'false';
            else sel.value = '';
            sel.classList.remove('is-invalid');
        });

        // Remove searchable wrappers
        qsa('.ms-container', newRow).forEach(w => w.remove());
        qsa('select.searchable-select', newRow).forEach(s => s.style.display = '');

        tbody.appendChild(newRow);
        try { buildSearchableDropdown($(newRow)); } catch { }

        renumberRows();
        // Sync in-memory state to include this new DOM row so pagination will account for it
        try { syncStateFromDOM(); } catch {}
        applyFiltersAndPagination();
    };

    const removeRow = (btn) => {
        const tbody = qs(SELECTORS.TABLE_BODY);
        const rows = qsa('tr', tbody);
        const tr = btn.closest('tr');
        if (rows.length > 1 && tr) {
            tr.remove();
            renumberRows();
            try { syncStateFromDOM(); } catch {}
            applyFiltersAndPagination();
        }
    };

    // Rebuild state.allQuoteItems from current DOM rows (used when rows are added/removed manually)
    const syncStateFromDOM = () => {
        const tbody = qs(SELECTORS.TABLE_BODY);
        if (!tbody) return;
        const rows = Array.from(tbody.querySelectorAll('tr'));
        const items = [];
        for (const tr of rows) {
            try {
                if (isRowEmpty(tr)) continue;
                const dto = collectRow(tr);
                items.push(dto);
            } catch { }
        }
        state.allQuoteItems = items;
        state.filteredQuoteItems = [...items];
        // reset to first page when structure changes
        state.currentPage = 1;
    };

    const resetForm = () => {
        const form = qs(SELECTORS.QUOTE_FORM);
        if (form) form.reset();

        const tbody = qs(SELECTORS.TABLE_BODY);
        if (!tbody) return;

        // Keep only 5 rows
        while (tbody.children.length > 5) tbody.removeChild(tbody.lastElementChild);

        // Reset selects and remove wrappers
        qsa('.ms-container', tbody).forEach(w => w.remove());
        qsa('select.searchable-select', tbody).forEach(sel => {
            try { if (window.jQuery) $(sel).removeData('search-dropdown'); } catch {}

            sel.style.display = '';
            if (sel.classList.contains('rohsTb')) sel.value = 'No Need';
            else if (sel.classList.contains('laybaogiaTb')) sel.value = 'true';
            else if (sel.classList.contains('gapTb')) sel.value = 'false';
            else if (sel.options?.length) sel.selectedIndex = 0;
            sel.classList.remove('is-invalid');
        });

        // Clear inputs
        qsa('tr', tbody).forEach(tr => {
            qsa('input', tr).forEach(inp => {
                if (['checkbox', 'radio'].includes(inp.type)) inp.checked = false;
                else inp.value = '';
                inp.classList.remove('is-invalid');
            });
        });

        state.allQuoteItems = [];
        state.filteredQuoteItems = [];

        // Rebuild searchable dropdowns for tbody only (faster and avoids skipping due to leftover flags)
        try { buildSearchableDropdown($(tbody)); } catch { try { buildSearchableDropdown($(document)); } catch {} }
        renumberRows();
        applyFiltersAndPagination();
    };

    // ==================== VALIDATION ====================
    const validateRow = (tr) => {
        let ok = true;

        const validateField = (selector, isSelect = false) => {
            const element = tr.querySelector(selector);
            if (!element) return true;

            const isValid = element.value?.trim() !== '';
            if (!isValid) ok = false;
            element.classList.toggle('is-invalid', !isValid);

            // Handle searchable select UI
            if (element.classList?.contains('searchable-select')) {
                const $wrapper = $(element).siblings('.ms-container');
                if ($wrapper.length) $wrapper.find('.ms-btn').toggleClass('is-invalid', !isValid);
            }

            return isValid;
        };

        const requiredFields = [
            { selector: '.tenPhongBanTb', isSelect: true },
            { selector: 'input[name^="tenHangVN_"]', isSelect: false },
            { selector: 'input[name^="tenHangEN_"]', isSelect: false },
            { selector: 'input[type="number"]', isSelect: false },
            { selector: 'input[name^="donVi_"]', isSelect: false },
            { selector: '.nhaCungCapTb', isSelect: true },
            { selector: '.laybaogiaTb', isSelect: true }
        ];

        requiredFields.forEach(field => validateField(field.selector, field.isSelect));

        // Validate ngay muon nhan
        const dateInputs = qsa('input[type="date"]', tr);
        if (dateInputs[0]) {
            const isValid = !!dateInputs[0].value?.trim();
            if (!isValid) ok = false;
            dateInputs[0].classList.toggle('is-invalid', !isValid);
        }

        // Validate internal vs supplier code
        const maHangNoiBoEl = tr.querySelector('.maHangNoiBo');
        const maHangNCCEl = tr.querySelector('input[id^="maHangNCC_"]') || tr.querySelector('input[placeholder*="mã hàng ncc"]');
        const hasInternal = maHangNoiBoEl?.value?.trim();
        const hasNcc = maHangNCCEl?.value?.trim();

        if (!hasInternal && !hasNcc) {
            ok = false;
            if (maHangNCCEl) maHangNCCEl.classList.add('is-invalid');
        } else if (maHangNCCEl) {
            maHangNCCEl.classList.remove('is-invalid');
        }

        return ok;
    };

    const checkLyDoTuChoi = (tr) => {
        const layBaoGiaVal = tr.querySelector('.laybaogiaTb')?.value;
        const lyDoEl = tr.querySelector('.lydoTb') || tr.querySelector('input[placeholder*="Lý do"]') || tr.querySelector('input[id^="lyDo_"]');

        if (layBaoGiaVal === 'false') {
            const lyDoVal = lyDoEl?.value?.trim();
            if (!lyDoVal) {
                if (lyDoEl) lyDoEl.classList.add('is-invalid');
                return false;
            }
            if (lyDoEl) lyDoEl.classList.remove('is-invalid');
        } else if (lyDoEl) {
            lyDoEl.classList.remove('is-invalid');
        }
        return true;
    };

    // ==================== DATA COLLECTION ====================
    const collectRow = (tr) => {
        const getSel = (selector) => tr.querySelector(selector)?.value || '';
        const getInput = (selectors) => {
            for (const s of selectors) {
                const el = tr.querySelector(s);
                if (el) return el.value || '';
            }
            return '';
        };

        const getSelDisplay = (selector) => {
            const el = tr.querySelector(selector);
            if (!el) return '';
            try {
                const wrapper = el.nextElementSibling;
                if (wrapper?.classList?.contains('ms-container')) {
                    const values = wrapper.querySelector('.ms-values');
                    if (values?.textContent?.trim()) {
                        state.sectionNameFirst = values.textContent.trim();
                        return values.textContent.trim();
                    }
                }
            } catch { }
            const opt = el.options?.[el.selectedIndex];
            return opt?.text || el.value || '';
        };

        const dates = qsa('input[type="date"]', tr);
        const createDateVN = getVietnamTime();
        const src = window.indexQuoteData || {};

        const obj = {
            ID: 0,
            CHR_MaDon: '',
            CHR_MaThietBi: getInput(['input[id^="maThietBi_"]', 'input[placeholder*="Mã thiết bị"]']),
            CHR_Phanloai: getSel('.tenPhanLoaiTb'),
            CHR_MaHangNoiBo: getSel('.maHangNoiBo'),
            CHR_MaHangNCC: getInput(['input[id^="maHangNCC_"]', 'input[placeholder*="Mã hàng NCC"]']),
            NVCHR_NameVN: getInput(['input[id^="tenHangVN_"]', 'input[placeholder*="thủ tục hải quan"]']),
            CHR_NameEN: getInput(['input[id^="tenHangEN_"]', 'input[placeholder*="tên hàng"]']),
            INT_SoLuong: getInput(['input[type="number"]']),
            NVCHR_DonVi: getInput(['input[id^="donVi_"]', 'input[placeholder*="Đơn vị"]']),
            NVCHR_ChungLoai: getSel('.chungLoaiTb'),
            NVCHR_HinhDang: getInput(['input[id^="hinhDang_"]', 'input[placeholder*="Hình dáng"]']),
            NVCHR_ChatLieu: getInput(['input[id^="chatLieu_"]', 'input[placeholder*="Chất liệu"]']),
            NVCHR_ThanhPhan: getInput(['input[id^="thanhPhan_"]', 'input[placeholder*="Thành phần"]']),
            NVCHR_KichThuoc: getInput(['input[id^="kichThuoc_"]', 'input[placeholder*="Kích thước"]']),
            NVCHR_DongMay: getInput(['input[id^="viTriSuDung_"]', 'input[placeholder*="Dùng cho máy"]']),
            NVCHR_TinhNang: getInput(['input[id^="tinhNang_"]', 'input[placeholder*="Dùng để làm gì"]']),
            NVCHR_Rohs: getSel('.rohsTb'),
            NVCHR_COCQ: getSel('.CoCqTb'),
            NVCHR_MSDS: getInput(['input[id^="msds_"]', 'input[placeholder*="MSDS"]']),
            NVCHR_AnToan: getInput(['input[id^="tieuChuanAnToan_"]', 'input[placeholder*="an toàn"]']),
            NVCHR_FileThietKe: getInput(['input[id^="fileThietKe_"]', 'input[placeholder*="File thiết kế"]']),
            NVCHR_NhaSanXuat: getInput(['input[id^="nsx_"]', 'input[placeholder*="NSX"]']),
            CHR_MaNCC: getSel('.nhaCungCapTb'),
            NVCHR_TenNCC: getSelDisplay('.nhaCungCapTb'),
            BIT_LayBaoGia: getSel('.laybaogiaTb') === 'true',
            NVCHR_LyDo: getInput(['input[id^="lyDo_"]', 'input[placeholder*="Lý do"]']),
            DTM_NgayMuonNhan: (tr.querySelector('input[id^="ngayMuonNhan_"]') || dates[0])?.value || null,
            DTM_KyHan: (tr.querySelector('input[id^="kyHanChonNCC_"]') || dates[1])?.value || null,
            CHR_Gap: getSel('.gapTb'),
            CHR_SectionCode: getSel('.tenPhongBanTb'),
            CHR_SectionName: getSelDisplay('.tenPhongBanTb'),
            NVCHR_UserRequest: getInput(['input[id^="nguoiYeuCauRow_"]', 'input[placeholder*="Người yêu cầu"]']) || src.user,
            CHR_CreateBy: src.user || '',
            DTM_CreateDate: toVietnamISOString(createDateVN),
            CHR_UserApproval: qs(SELECTORS.APPROVER_SELECT)?.value || '',
            ID_StepBaoGia: 2,
            ID_Status: 'CREATE',
            INT_SoLanUpdate: 0,
            DTM_UpdateLater: null,
            DTM_Deadline: null,
            BIT_IsTemplate: false
        };

        // Parse numeric fields
        if (obj.INT_SoLuong !== '') {
            const n = parseFloat(obj.INT_SoLuong);
            obj.INT_SoLuong = isFinite(n) ? n : null;
        } else {
            obj.INT_SoLuong = null;
        }

        // Parse dates
        [obj.DTM_NgayMuonNhan, obj.DTM_KyHan].forEach((date, idx) => {
            if (!date) return;
            const dateParts = date.split('-');
            if (dateParts.length === 3) {
                const dt = new Date(Date.UTC(parseInt(dateParts[0]), parseInt(dateParts[1]) - 1, parseInt(dateParts[2]), 7, 0, 0));
                if (idx === 0) obj.DTM_NgayMuonNhan = dt.toISOString();
                else obj.DTM_KyHan = dt.toISOString();
            }
        });

        return obj;
    };

    // ==================== SUBMIT FORM ====================
    const submitForm = async () => {
        let rowsValid = true;
        let rowsCheckReason = true;

        const approverVal = qs(SELECTORS.APPROVER_SELECT)?.value?.trim();
        if (!approverVal) {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: T.SelectApprover || 'Vui lòng chọn người phê duyệt trước khi gửi', type: 'error' });
            return;
        }

        const visibleRows = qsa('#quoteTableBody tr');
        visibleRows.forEach(tr => {
            if (isRowEmpty(tr)) return;
            if (!validateRow(tr)) rowsValid = false;
            if (!checkLyDoTuChoi(tr)) rowsCheckReason = false;
        });

        // Merge data from visible rows
        if (state.allQuoteItems.length > 0) {
            const start = (state.currentPage - 1) * state.rowsPerPage;
            visibleRows.forEach((tr, idx) => {
                const globalIdx = start + idx;
                const collected = collectRow(tr);
                if (state.filteredQuoteItems[globalIdx]) Object.assign(state.filteredQuoteItems[globalIdx], collected);
                if (state.allQuoteItems[globalIdx]) Object.assign(state.allQuoteItems[globalIdx], collected);
                else state.allQuoteItems.push(collected);
            });
        }

        let payload = state.allQuoteItems.length > 0 ? [...state.allQuoteItems] : [];
        if (payload.length === 0) {
            visibleRows.forEach(tr => {
                if (!isRowEmpty(tr)) payload.push(collectRow(tr));
            });
        }

        // Add missing fields to payload items
        let sectionForPayload = '';
        for (const item of payload) {
            const s = item.CHR_SectionCode || item.chR_SectionCode || item.CHR_SectionName || '';
            if (s?.trim()) {
                sectionForPayload = s.trim();
                break;
            }
        }

        const maDon = generateMaDonRequest(sectionForPayload);
        payload.forEach(item => {
            if (!item.CHR_UserApproval?.trim()) item.CHR_UserApproval = approverVal;
            if (!item.CHR_MaDon?.trim()) item.CHR_MaDon = maDon;
            if (!item.CHR_SectionName?.trim() || item.CHR_SectionName === '#N/A') item.CHR_SectionName = state.sectionNameFirst;
            item.ID_StepBaoGia = 2;
        });

        if (payload.length === 0) {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: T.MsgInvalidData || 'Không có dữ liệu để gửi', type: 'error' });
            return;
        }

        if (!rowsCheckReason) {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: T.MsgEnterReasonReject || 'Vui lòng nhập lý do từ chối lấy báo giá', type: 'error' });
            return;
        }

        if (!rowsValid) {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: T.MsgFillRequired || 'Vui lòng điền đầy đủ các trường bắt buộc(*)', type: 'error' });
            return;
        }

        try {
            showLoading((window.i18nQuote?.Exporting) || 'Đang xử lý...');
            const res = await fetch(api.insertListBaoGia, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
            });
            if (!res.ok) throw new Error(await res.text());

            const T = window.i18nQuote || {};
            showDialog({ title: T.SuccessTitle || 'Thành công', message: T.MsgSubmitSuccess || 'Gửi yêu cầu báo giá thành công', type: 'success' });
            resetForm();
        } catch (err) {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: err.message, type: 'error' });
        } finally {
            hideLoading();
        }
    };

    // ==================== AUTO FILL FUNCTIONS ====================
    const autofillFromMaterialSelect = async (selectEl) => {
        const tr = selectEl.closest('tr');
        const code = selectEl.value;
        if (!code) return;

        // Check if supplier already selected
        const supplierSel = tr?.querySelector('.nhaCungCapTb');
        if (supplierSel?.value?.trim()) return;

        try {
            const res = await fetch(api.getMaterials(code));
            if (!res.ok) throw new Error(await res.text());
            const materials = await res.json();
            const material = Array.isArray(materials) ? materials.find(m => m.material_Code === code) : null;
            if (!material) return;

            const fieldMappings = [
                { selector: 'tên hàng en', prop: 'material_Name_EN' },
                { selector: 'đơn vị', prop: 'unit' },
                { selector: 'thủ tục hải quan', prop: 'nameVI' },
                { selector: 'hình dáng', prop: 'shape' },
                { selector: 'chất liệu', prop: 'material' },
                { selector: 'thành phần', prop: 'composition' },
                { selector: 'kích thước', prop: 'dimension' },
                { selector: 'dùng cho máy', prop: 'usedFor' },
                { selector: 'dùng để làm gì', prop: 'purpose' }
            ];

            fieldMappings.forEach(({ selector, prop }) => {
                const input = qsa('input', tr).find(i => i.placeholder?.toLowerCase().includes(selector));
                if (input && material[prop]) input.value = material[prop];
            });

            // Set category
            const categorySelect = tr.querySelector('.chungLoaiTb');
            if (categorySelect && material.category_VN) {
                setSelectValueByText(categorySelect, material.category_VN);
                updateSearchableSelectDisplay(categorySelect);
                await autoAddRowByCategory(categorySelect);
            }

            // Set sub-category
            const categoryInput = tr.querySelector('.tenPhanLoaiTb');
            const loaiHangValue = material.loaiHang || material.LoaiHang;
            if (categoryInput && loaiHangValue) categoryInput.value = loaiHangValue;

        } catch (err) {
            console.warn('Không thể tự động điền thông tin vật tư:', err);
            showDialog({ title: 'Lỗi', message: err.message, type: 'error' });
        }
    };

    const autoAddRowByCategory = async (selectEl) => {
        const tr = selectEl.closest('tr');
        const code = selectEl.value;

        try {
            const supRes = await fetch(api.getNCCByCategory, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(code)
            });
            if (!supRes.ok) throw new Error(await supRes.text());

            const suppliers = await supRes.json();
            if (!Array.isArray(suppliers) || suppliers.length === 0) return;

            const getSupCode = (s) => s?.chR_MaNCC || (typeof s === 'string' ? s : '') || '';

            if (suppliers.length === 1) {
                const s = suppliers[0];
                const supSel = tr.querySelector('.nhaCungCapTb');
                if (supSel) {
                    supSel.value = getSupCode(s);
                    updateSearchableSelectDisplay(supSel);
                    try { buildSearchableDropdown($(tr)); } catch {}
                    renumberRows();
                    applyFiltersAndPagination();
                }

                const codeByNccInput = qsa('input', tr).find(i => i.placeholder?.toLowerCase().includes('mã hàng ncc'));
                if (codeByNccInput && s.nvchR_CodeByNCC) codeByNccInput.value = s.nvchR_CodeByNCC;

                const nsxInput = qsa('input', tr).find(i => i.placeholder?.toLowerCase().includes('nsx'));
                if (nsxInput && s.nvchR_MakeIn) nsxInput.value = s.nvchR_MakeIn;
            } else {
                // Store current row values
                const values = {};
                qsa('input', tr).forEach(inp => values[inp.name || inp.id || inp.placeholder || inp.type] = inp.value);
                qsa('select', tr).forEach(sel => values[sel.className || sel.name || sel.id] = sel.value);

                // Set first supplier
                const s0 = suppliers[0];
                const supSel0 = tr.querySelector('.nhaCungCapTb');
                if (supSel0) {
                    supSel0.value = getSupCode(s0);
                    updateSearchableSelectDisplay(supSel0);
                }

                const codeByNccFirst = qsa('input', tr).find(i => i.placeholder?.toLowerCase().includes('mã hàng ncc'));
                if (codeByNccFirst && s0.nvchR_CodeByNCC) codeByNccFirst.value = s0.nvchR_CodeByNCC;

                const nsxFirst = qsa('input', tr).find(i => i.placeholder?.toLowerCase().includes('nsx'));
                if (nsxFirst && s0.nvchR_MakeIn) nsxFirst.value = s0.nvchR_MakeIn;

                // Clone for remaining suppliers
                let insertAfter = tr;
                for (let i = 1; i < suppliers.length; i++) {
                    const s = suppliers[i];
                    const newRow = tr.cloneNode(true);

                    qsa('.ms-container', newRow).forEach(w => w.remove());
                    qsa('select.searchable-select', newRow).forEach(sv => sv.style.display = '');

                    qsa('input', newRow).forEach(inp => {
                        const key = inp.name || inp.id || inp.placeholder || inp.type;
                        if (values[key]) inp.value = values[key];
                        inp.classList.remove('is-invalid');
                    });

                    qsa('select', newRow).forEach(sel => {
                        const key = sel.className || sel.name || sel.id;
                        if (values[key]) sel.value = values[key];
                        sel.classList.remove('is-invalid');
                    });

                    const supSel = newRow.querySelector('.nhaCungCapTb');
                    if (supSel) {
                        supSel.value = getSupCode(s);
                        updateSearchableSelectDisplay(supSel);
                    }

                    const codeByNcc = qsa('input', newRow).find(i => i.placeholder?.toLowerCase().includes('mã hàng ncc'));
                    if (codeByNcc && s.nvchR_CodeByNCC) codeByNcc.value = s.nvchR_CodeByNCC;

                    const nsx = qsa('input', newRow).find(i => i.placeholder?.toLowerCase().includes('nsx'));
                    if (nsx && s.nvchR_MakeIn) nsx.value = s.nvchR_MakeIn;

                    insertAfter.parentNode.insertBefore(newRow, insertAfter.nextSibling);
                    insertAfter = newRow;
                }

                try { buildSearchableDropdown($(document)); } catch { }
                renumberRows();
                // After cloning rows, refresh pagination and filtering so new rows are considered
                applyFiltersAndPagination();
            }
        } catch (err) {
            console.warn('Không thể lấy NCC cho mã hàng:', err);
        }
    };

    // ==================== PAGINATION & FILTERS ====================
    const applyFiltersAndPagination = () => {
        const tbody = qs(SELECTORS.TABLE_BODY);
        const allRows = Array.from(tbody.querySelectorAll('tr'));
        const filters = qsa(SELECTORS.FILTER_INPUT).map(inp => inp.value.toLowerCase().trim());

        const getCellText = (td) => {
            const select = td.querySelector('select');
            if (select) {
                const opt = select.options[select.selectedIndex];
                return opt?.text || '';
            }
            const input = td.querySelector('input');
            if (input && input.type !== 'date') return input.value || '';
            return td.textContent.trim();
        };

        // Filter using in-memory dataset if available
        if (state.allQuoteItems.length > 0) {
            state.filteredQuoteItems = state.allQuoteItems.filter(dto => {
                const combined = [
                    dto.chR_MaHangNoiBo, dto.chR_MaHangNCC, dto.nvchR_NameVN, dto.chR_NameEN,
                    dto.nvchR_DonVi, dto.chR_MaNCC, dto.nvchR_TenNCC, dto.nvchR_ChungLoai, dto.chR_Phanloai
                ].map(v => (v || '').toLowerCase()).join(' ');
                return filters.every(filter => !filter || combined.includes(filter));
            });
            renderQuotePage(tbody, state.filteredQuoteItems);
            return;
        }

        // Fallback: filter DOM rows
        state.filteredRows = allRows.filter(tr => {
            const tds = Array.from(tr.querySelectorAll('td'));
            return filters.every((filter, idx) => {
                if (!filter) return true;
                const td = tds[idx];
                return td ? getCellText(td).toLowerCase().includes(filter) : true;
            });
        });

        const totalPages = Math.ceil(state.filteredRows.length / state.rowsPerPage);
        if (state.currentPage > totalPages) state.currentPage = totalPages || 1;

        const start = (state.currentPage - 1) * state.rowsPerPage;
        const end = start + state.rowsPerPage;
        const visibleRows = state.filteredRows.slice(start, end);

        allRows.forEach(tr => tr.style.display = 'none');
        visibleRows.forEach(tr => tr.style.display = '');

        updatePaginationUI(totalPages);
        visibleRows.forEach((tr, idx) => {
            const noCell = tr.children[0];
            if (noCell) noCell.textContent = String(start + idx + 1);
        });
    };

    const updatePaginationUI = (totalPages) => {
        const pagination = qs(SELECTORS.PAGINATION_CONTROLS);
        const prev = qs(SELECTORS.PREV_PAGE);
        const next = qs(SELECTORS.NEXT_PAGE);

        if (totalPages > 1) {
            if (pagination) pagination.style.display = '';
            if (prev) prev.classList.toggle('disabled', state.currentPage === 1);
            if (next) next.classList.toggle('disabled', state.currentPage === totalPages);
        } else if (pagination) {
            pagination.style.display = 'none';
        }

        const totalEntries = state.filteredRows.length;
        const startEntry = totalEntries === 0 ? 0 : (state.currentPage - 1) * state.rowsPerPage + 1;
        const endEntry = Math.min(state.currentPage * state.rowsPerPage, totalEntries);
        const T = window.i18nQuote || {};

        const pageInfo = qs(SELECTORS.PAGE_INFO);
        if (pageInfo) pageInfo.textContent = `${T.Showing || 'Showing'} ${startEntry} ~ ${endEntry} ${T.Of || 'Of'} ${totalEntries}`;

        const pageNumberInfo = qs(SELECTORS.PAGE_NUMBER_INFO);
        if (pageNumberInfo) pageNumberInfo.textContent = `${state.currentPage}/${totalPages}`;

        const paginationInfo = qs(SELECTORS.PAGINATION_INFO);
        if (paginationInfo) paginationInfo.style.display = totalPages > 1 ? '' : 'none';
    };

    const renderQuotePage = (tbody, sourceItems) => {
        try {
            showLoading((window.i18nQuote?.Exporting) || 'Đang xử lý...');
            const baseRow = qs('#quoteTableBody tr');
            if (!baseRow) return;

            const total = sourceItems.length;
            const totalPages = Math.max(1, Math.ceil(total / state.rowsPerPage));
            if (state.currentPage > totalPages) state.currentPage = totalPages;

            const start = (state.currentPage - 1) * state.rowsPerPage;
            const end = Math.min(start + state.rowsPerPage, total);

            tbody.innerHTML = '';
            const frag = document.createDocumentFragment();

            for (let i = start; i < end; i++) {
                const dto = sourceItems[i] || {};
                const row = baseRow.cloneNode(true);

                qsa('.ms-container', row).forEach(w => w.remove());
                qsa('select.searchable-select', row).forEach(s => {
                    s.style.display = '';
                    try { $(s).data('search-dropdown', false); } catch { }
                });
                qsa('input', row).forEach(inp => {
                    inp.value = '';
                    inp.classList.remove('is-invalid');
                });
                qsa('select', row).forEach(sel => {
                    sel.value = '';
                    sel.classList.remove('is-invalid');
                });

                populateRowFromDto(row, dto, i + 1);
                frag.appendChild(row);
            }

            tbody.appendChild(frag);
            try { buildSearchableDropdown($(tbody)); } catch { }
            renumberRows();
            updatePaginationUI(totalPages);

            state.filteredRows = Array.from(tbody.querySelectorAll('tr'));
        } catch (e) {
            console.warn('renderQuotePage error', e);
        } finally {
            hideLoading();
        }
    };

    const populateRowFromDto = (tr, dto, rowIndex = 1) => {
        const setSelectById = (idPattern, value) => {
            const element = tr.querySelector(`#${idPattern}_${rowIndex}`);
            if (element) setSelectValueByText(element, value);
        };

        const setInputById = (idPattern, value) => {
            const element = tr.querySelector(`#${idPattern}_${rowIndex}`);
            if (element) element.value = value ?? '';
        };

        setSelectById('tenPhongBanTb', dto.chR_SectionCode || dto.chR_SectionName);
        setSelectById('chungLoai', dto.nvchR_ChungLoai);
        setSelectById('tenPhanLoaiTb', dto.chR_Phanloai);
        setInputById('maThietBi', dto.chR_MaThietBi);
        setSelectById('maHangNoiBo', dto.chR_MaHangNoiBo);
        setInputById('maHangNCC', dto.chR_MaHangNCC);
        setInputById('tenHangVN', dto.nvchR_NameVN);
        setInputById('tenHangEN', dto.chR_NameEN);
        setInputById('soLuong', dto.inT_SoLuong);
        setInputById('donVi', dto.nvchR_DonVi);
        setInputById('hinhDang', dto.nvchR_HinhDang);
        setInputById('chatLieu', dto.nvchR_ChatLieu);
        setInputById('thanhPhan', dto.nvchR_ThanhPhan);
        setInputById('kichThuoc', dto.nvchR_KichThuoc);
        setInputById('viTriSuDung', dto.nvchR_DongMay);
        setInputById('tinhNang', dto.nvchR_TinhNang);
        setSelectById('rohsTb', dto.nvchR_Rohs);
        setSelectById('CoCqTb', dto.nvchR_COCQ);
        setInputById('msds', dto.nvchR_MSDS);
        setInputById('tieuChuanAnToan', dto.nvchR_AnToan);
        setInputById('fileThietKe', dto.nvchR_FileThietKe);
        setInputById('nsx', dto.nvchR_NhaSanXuat);
        setSelectById('nhaCungCapTb', dto.chR_MaNCC || dto.nvchR_TenNCC);

        const layBaoGiaValue = dto.biT_LayBaoGia === true ? 'true' : dto.biT_LayBaoGia === false ? 'false' : '';
        setSelectById('laybaogiaTb', layBaoGiaValue);

        setInputById('lyDo', dto.nvchR_LyDo);
        setInputById('ngayMuonNhan', formatDateForInput(dto.dtM_NgayMuonNhan));
        setInputById('kyHanChonNCC', formatDateForInput(dto.dtM_KyHan));
        setSelectById('gapTb', dto.chR_Gap);
        setInputById('nguoiYeuCauRow', dto.nvchR_UserRequest || window.indexQuoteData?.user || '');
    };

    const populateTableFromItems = async (items) => {
        state.allQuoteItems = [...items];
        state.filteredQuoteItems = [...items];
        state.currentPage = 1;

        // Validate sections
        const sections = new Set();
        state.allQuoteItems.forEach(it => {
            const s = it.CHR_SectionCode || it.chR_SectionCode || it.sectionCode;
            if (s?.trim()) sections.add(s.trim());
        });

        if (sections.size > 1) {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: 'Không được upload dữ liệu chứa nhiều mã phòng khác nhau trong cùng 1 đơn', type: 'error' });
            return;
        }

        if (sections.size === 1) await loadApprovers(Array.from(sections)[0]);

        // Ensure material codes exist as options
        const materialCodes = new Set();
        state.allQuoteItems.forEach(it => {
            const code = it.CHR_MaHangNoiBo || it.chR_MaHangNoiBo || it.maHangNoiBo;
            if (code?.trim()) materialCodes.add(code.trim());
        });

        if (materialCodes.size > 0) {
            qsa('.maHangNoiBo').forEach(sel => {
                materialCodes.forEach(code => {
                    if (!Array.from(sel.options).some(o => o.value === code)) {
                        const o = document.createElement('option');
                        o.value = code;
                        o.text = code;
                        sel.appendChild(o);
                    }
                });
                try { $(sel).data('search-dropdown', false); } catch { }
            });
        }

        const tbody = qs(SELECTORS.TABLE_BODY);
        if (tbody) renderQuotePage(tbody, state.filteredQuoteItems);
    };

    // ==================== EXPORT FUNCTIONS ====================
    const exportTable = async () => {
        try {
            showLoading((window.i18nQuote?.Exporting) || 'Đang xuất...');
            const res = await fetch(api.exportTable, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(state.allQuoteItems)
            });
            if (!res.ok) throw new Error(await res.text());

            const blob = await res.blob();
            let fileName = 'TableQuote.xlsx';
            const cd = res.headers.get('content-disposition');
            if (cd) {
                const match = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(cd);
                if (match?.[1]) fileName = match[1].replace(/["']/g, '').trim();
            }

            downloadBlob(blob, fileName);
        } catch (err) {
            showDialog({ title: (window.i18nQuote?.ErrorTitle) || 'Lỗi', message: err.message || 'Không thể xuất file', type: 'error' });
        } finally {
            hideLoading();
        }
    };

    const downloadBlob = (blob, fileName) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    };

    // ==================== AUTO RENDER MODAL ====================
    const exportAutoRender = () => {
        const overlay = document.getElementById('arModalOverlay');
        const sectionSel = document.getElementById('arSection');
        const sectionSel2 = document.getElementById('arSection2');
        const sectionNameEl = document.getElementById('arSectionName');
        const sectionNameEl2 = document.getElementById('arSectionName2');
        const lst = document.getElementById('arMaterialList');
        const searchBox = document.getElementById('arSearch');
        const selectAll = document.getElementById('arSelectAll');
        const lst2 = document.getElementById('arCategoryList');
        const searchBox2 = document.getElementById('arSearch2');
        const selectAll2 = document.getElementById('arSelectAll2');
        const btnExport = document.getElementById('arExportBtn');
        const btnCancel = document.getElementById('arCancelBtn');
        const btnClose = document.getElementById('arCloseBtn');
        const errEl = document.getElementById('arError');
        const backdrop = overlay?.querySelector('[data-ar-action="overlay"]');
        const tabHasCodeBtn = document.getElementById('arTabHasCode');
        const tabNoCodeBtn = document.getElementById('arTabNoCode');
        const tabHasCodeBody = document.getElementById('arTabHasCodeBody');
        const tabNoCodeBody = document.getElementById('arTabNoCodeBody');

        if (!overlay || !sectionSel || !lst || !searchBox || !selectAll || !btnExport || !btnCancel || !btnClose || !errEl ||
            !tabHasCodeBtn || !tabNoCodeBtn || !tabHasCodeBody || !tabNoCodeBody || !sectionSel2 || !sectionNameEl2 || !lst2 || !searchBox2 || !selectAll2) {
            const T = window.i18nQuote || {};
            alert(T.MsgCannotOpenAutoRender || 'Không thể mở hộp thoại Auto render');
            return;
        }

        let currentTab = 'hasCode';
        let materialState = {
            pageIndex: 1,
            pageSize: CONFIG.PAGE_SIZE,
            loading: false,
            lastQuery: '',
            hasMore: true,
            items: []
        };

        const hideAr = () => {
            overlay.style.display = 'none';
            overlay.setAttribute('aria-hidden', 'true');
        };

        const showAr = () => {
            overlay.style.display = 'flex';
            overlay.setAttribute('aria-hidden', 'false');
        };

        const setError = (msg) => {
            if (msg) {
                errEl.textContent = msg;
                errEl.style.display = '';
            } else {
                errEl.textContent = '';
                errEl.style.display = 'none';
            }
        };

        const setBusy = (busy) => {
            btnExport.disabled = busy;
            btnCancel.disabled = busy;
            btnClose.disabled = busy;
            const T = window.i18nQuote || {};
            btnExport.textContent = busy ? (T.Exporting || 'Đang xuất...') : (T.ExportExcel || 'Xuất Excel');
        };

        const switchTab = (tab) => {
            currentTab = tab;
            const activeClass = 'cm-btn cm-btn-primary';
            const inactiveClass = 'cm-btn cm-btn-outline';

            if (tab === 'hasCode') {
                tabHasCodeBody.style.display = '';
                tabNoCodeBody.style.display = 'none';
                tabHasCodeBtn.className = activeClass;
                tabNoCodeBtn.className = inactiveClass;
            } else {
                tabHasCodeBody.style.display = 'none';
                tabNoCodeBody.style.display = '';
                tabHasCodeBtn.className = inactiveClass;
                tabNoCodeBtn.className = activeClass;
            }
            setError('');
        };

        // Populate section options
        const populateSections = () => {
            const srcDeptSel = qs('.tenPhongBanTb');
            if (!srcDeptSel) return;

            [sectionSel, sectionSel2].forEach(sel => {
                sel.innerHTML = '';
                const defaultOpt = document.createElement('option');
                defaultOpt.value = '';
                defaultOpt.textContent = '';
                sel.appendChild(defaultOpt);

                Array.from(srcDeptSel.options).forEach(o => {
                    if (!o?.text) return;
                    const opt = document.createElement('option');
                    opt.value = o.value || '';
                    opt.textContent = o.text || '';
                    sel.appendChild(opt);
                });
            });
        };

        const updateSectionName = (sel, nameEl) => {
            const txt = sel.options[sel.selectedIndex]?.text || '';
            const parts = txt.split(' - ');
            nameEl.textContent = parts.length > 1 ? parts.slice(1).join(' - ') : '';
        };

        // Load materials for auto render
        const loadMaterialPage = async (query = '') => {
            if (materialState.loading) return;

            if (query !== materialState.lastQuery) {
                materialState.pageIndex = 1;
                materialState.hasMore = true;
                materialState.items = [];
                lst.innerHTML = '';
            }
            if (!materialState.hasMore) return;

            materialState.loading = true;
            const body = { MaHang: query, Name: query || '', NhomHang: '', PageIndex: materialState.pageIndex, PageSize: materialState.pageSize };

            try {
                const res = await fetch(api.searchMaterials, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
                if (!res.ok) throw new Error(await res.text());
                const data = await res.json();

                const pageItems = Array.isArray(data) ? data.map(o => ({ code: o.material_Code || '', text: `${o.material_Code || ''} - ${o.material_Name_VN || ''}` })) : [];

                pageItems.forEach(it => {
                    if (!it.code) return;
                    materialState.items.push(it);
                    const wrap = document.createElement('label');
                    wrap.style.cssText = 'display:flex;min-width:350px;align-items:center;gap:8px;padding:4px 2px;cursor:pointer';
                    wrap.dataset.search = (it.code + ' ' + it.text).toLowerCase();
                    wrap.innerHTML = `<input type="checkbox" value="${it.code}"><span>${it.text}</span>`;
                    lst.appendChild(wrap);
                });

                materialState.hasMore = pageItems.length === materialState.pageSize;
                if (materialState.hasMore) materialState.pageIndex++;
                materialState.lastQuery = query || '';
            } catch (err) {
                console.warn('Không thể tải danh sách vật tư:', err);
            } finally {
                materialState.loading = false;
            }
        };

        // Load categories
        const loadCategories = () => {
            const srcCategorySel = qs('.chungLoaiTb');
            if (!srcCategorySel) return;

            lst2.innerHTML = '';
            Array.from(srcCategorySel.options).forEach(o => {
                const val = o.value || '';
                const text = o.text || '';
                if (!val) return;

                const wrap = document.createElement('label');
                wrap.style.cssText = 'display:flex;min-width:350px;align-items:center;gap:8px;padding:4px 2px;cursor:pointer';
                wrap.dataset.search = (val + ' ' + text).toLowerCase();
                wrap.innerHTML = `<input type="checkbox" value="${val}"><span>${text}</span>`;
                lst2.appendChild(wrap);
            });
        };

        // Filter categories
        const filterCategories = (query) => {
            const q = query.toLowerCase();
            Array.from(lst2.children).forEach(el => {
                const s = el.dataset.search || '';
                el.style.display = !q || s.includes(q) ? '' : 'none';
            });
        };

        // Select all toggle
        const setupSelectAll = (list, selectAllCheckbox) => {
            selectAllCheckbox.onchange = () => {
                const visibleItems = Array.from(list.querySelectorAll('label')).filter(el => el.style.display !== 'none');
                visibleItems.forEach(el => {
                    const cb = el.querySelector('input[type="checkbox"]');
                    if (cb) cb.checked = selectAllCheckbox.checked;
                });
            };
        };

        // Export handler
        const handleExport = async () => {
            setError('');
            try {
                setBusy(true);
                showLoading((window.i18nQuote?.Exporting) || 'Đang xuất...');

                let sectionCode = '';
                let sectionName = '';
                let selectedIds = [];
                let endpoint = '';

                if (currentTab === 'hasCode') {
                    sectionCode = sectionSel.value || '';
                    const sectionText = sectionSel.options[sectionSel.selectedIndex]?.text || '';
                    sectionName = sectionText.split(' - ').slice(1).join(' - ');
                    selectedIds = Array.from(lst.querySelectorAll('input[type="checkbox"]:checked')).map(cb => cb.value);
                    endpoint = api.exportAutoRender;

                    if (!sectionCode) {
                        setError((window.i18nQuote || {}).MsgSelectSectionRequired || 'Vui lòng chọn mã phòng ban');
                        return;
                    }
                    if (selectedIds.length === 0) {
                        setError((window.i18nQuote || {}).MsgSelectAtLeastOneMaterial || 'Vui lòng chọn ít nhất một mã hàng nội bộ');
                        return;
                    }
                } else {
                    sectionCode = sectionSel2.value || '';
                    const sectionText = sectionSel2.options[sectionSel2.selectedIndex]?.text || '';
                    sectionName = sectionText.split(' - ').slice(1).join(' - ');
                    selectedIds = Array.from(lst2.querySelectorAll('input[type="checkbox"]:checked')).map(cb => cb.value);
                    endpoint = api.exportRenderOutSide;

                    if (!sectionCode) {
                        setError((window.i18nQuote || {}).MsgSelectSectionRequired || 'Vui lòng chọn mã phòng ban');
                        return;
                    }
                    if (selectedIds.length === 0) {
                        setError((window.i18nQuote || {}).MsgSelectAtLeastOneCategory || 'Vui lòng chọn ít nhất một chủng loại hàng');
                        return;
                    }
                }

                const res = await fetch(endpoint, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ sectionCode, sectionName, selectedItemIds: selectedIds })
                });

                if (!res.ok) throw new Error(await res.text());

                const blob = await res.blob();
                let fileName = 'AutoRenderQuote.xlsx';
                const cd = res.headers.get('content-disposition');
                if (cd) {
                    const match = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(cd);
                    if (match?.[1]) fileName = match[1].replace(/["']/g, '').trim();
                }

                downloadBlob(blob, fileName);
                hideAr();
                const T = window.i18nQuote || {};
                showDialog({ title: T.SuccessTitle || 'Thành công', message: T.MsgExportedExcel || 'Đã xuất file Excel tự động.', type: 'success' });
            } catch (e) {
                const T = window.i18nQuote || {};
                setError(e.message || T.MsgCannotExport || 'Không thể xuất file');
            } finally {
                hideLoading();
                setBusy(false);
            }
        };

        // Setup event listeners
        tabHasCodeBtn.onclick = () => switchTab('hasCode');
        tabNoCodeBtn.onclick = () => switchTab('noCode');
        btnCancel.onclick = hideAr;
        btnClose.onclick = hideAr;
        if (backdrop) backdrop.onclick = hideAr;
        btnExport.onclick = handleExport;

        populateSections();
        sectionSel.onchange = () => updateSectionName(sectionSel, sectionNameEl);
        sectionSel2.onchange = () => updateSectionName(sectionSel2, sectionNameEl2);
        updateSectionName(sectionSel, sectionNameEl);
        updateSectionName(sectionSel2, sectionNameEl2);

        // Setup infinite scroll for materials
        let searchTimer = null;
        searchBox.oninput = () => {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => loadMaterialPage(searchBox.value), CONFIG.SEARCH_DELAY);
        };

        lst.addEventListener('scroll', () => {
            if (lst.scrollTop + lst.clientHeight >= lst.scrollHeight - 40) {
                if (!materialState.loading && materialState.hasMore) {
                    loadMaterialPage(materialState.lastQuery);
                }
            }
        });

        setupSelectAll(lst, selectAll);
        loadCategories();
        setupSelectAll(lst2, selectAll2);
        searchBox2.oninput = () => filterCategories(searchBox2.value);

         loadMaterialPage('');
        switchTab('hasCode');
        showAr();
    };

    // ==================== APPROVER FUNCTIONS ====================
    const loadApprovers = async (sectionCode) => {
        const sel = qs(SELECTORS.APPROVER_SELECT);
        if (!sel) return;

        try {
            showLoading((window.i18nQuote?.Exporting) || 'Đang xử lý...');
            const res = await fetch(api.searchApprover, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Step: 2, SectionCost: sectionCode || '' })
            });
            if (!res.ok) throw new Error(await res.text());

            const data = await res.json();
            sel.innerHTML = `<option value="">${(window.i18nQuote?.SelectApprover) || '-- Select Approver --'}</option>`;

            if (Array.isArray(data)) {
                data.forEach(a => {
                    const userAdid = a?.chR_UserAdid || '';
                    const userName = a?.nvchR_UserName || '';
                    const opt = document.createElement('option');
                    opt.value = userAdid;
                    opt.textContent = userName ? `${userName} (${userAdid})` : userAdid;
                    sel.appendChild(opt);
                });
            }
        } catch (err) {
            console.warn('Không thể tải danh sách approver:', err);
        } finally {
            hideLoading();
        }
    };

    // ==================== SEARCHABLE DROPDOWN ====================
    const buildSearchableDropdown = ($container) => {
        if (!$container) return;

        let $targets;
        try {
            $targets = typeof $container.is === 'function' && $container.is('select') ? $container : $container.find('select.searchable-select');
        } catch {
            $targets = $container.find('select.searchable-select');
        }

        $targets.each(function () {
            const $select = $(this);
            if ($select.data('search-dropdown') === true) return;

            const domOptions = $select.find('option').map(function () {
                return { value: this.value, text: $(this).text(), selected: this.selected };
            }).get();

            const isRemoteMaterial = $select.hasClass('maHangNoiBo');
            const remoteState = {
                pageIndex: 1,
                pageSize: CONFIG.PAGE_SIZE,
                loading: false,
                lastQuery: '',
                hasMore: true,
                options: [...domOptions],
                controller: null
            };

            const getCategoryForRow = () => {
                try {
                    const tr = $select.closest('tr');
                    return tr.find('select.chungLoaiTb').val() || '';
                } catch {
                    return '';
                }
            };

            const loadRemote = async (query, append = false) => {
                if (!isRemoteMaterial) return;

                if (!append && remoteState.controller) {
                    try { remoteState.controller.abort(); } catch { }
                    remoteState.controller = null;
                }
                if (remoteState.loading && append) return;

                if (query !== remoteState.lastQuery) {
                    remoteState.pageIndex = 1;
                    remoteState.hasMore = true;
                }

                const page = remoteState.pageIndex;
                const pageSize = remoteState.pageSize;
                const body = { MaHang: query || '', Name: query || '', NhomHang: getCategoryForRow() || '', PageIndex: page, PageSize: pageSize };

                $list.find('.ms-loading').remove();
                $list.append('<div class="ms-loading">Loading...</div>');
                remoteState.loading = true;

                try {
                    remoteState.controller = new AbortController();
                    const res = await fetch(api.searchMaterials, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(body),
                        signal: remoteState.controller.signal
                    });
                    if (!res.ok) throw new Error(await res.text());

                    const data = await res.json();
                    const items = Array.isArray(data) ? data.map(m => ({ value: m.material_Code || '', text: `${m.material_Code || ''} - ${m.material_Name_VN || ''}` })) : [];

                    if (!append) {
                        remoteState.options = items;
                    } else {
                        const existing = new Set(remoteState.options.map(o => o.value));
                        items.forEach(it => { if (it?.value && !existing.has(it.value)) remoteState.options.push(it); });
                    }

                    // Update underlying select
                    items.forEach(it => {
                        if (!it?.value) return;
                        if ($select.find(`option[value="${it.value.replace(/"/g, '\\"')}"]`).length === 0) {
                            $select.append(`<option value="${it.value}">${it.text || it.value}</option>`);
                        }
                    });

                    remoteState.hasMore = items.length === pageSize;
                    if (remoteState.hasMore) remoteState.pageIndex = page + 1;
                    remoteState.lastQuery = query;
                } catch (err) {
                    if (err?.name !== 'AbortError') console.warn('Error loading remote materials:', err);
                } finally {
                    remoteState.loading = false;
                    remoteState.controller = null;
                    $list.find('.ms-loading').remove();
                    renderList(remoteState.lastQuery);
                }
            };

            const renderList = (query) => {
                const q = (query || '').toLowerCase();
                $list.empty();
                let hasItems = false;
                const source = isRemoteMaterial ? remoteState.options : domOptions;

                source.forEach(opt => {
                    if (!q || (opt.text || '').toLowerCase().includes(q)) {
                        const $item = $('<div class="ms-item"></div>').attr('data-value', opt.value).text(opt.text);
                        if ($select.val() === opt.value || opt.selected) $item.addClass('selected');
                        $list.append($item);
                        hasItems = true;
                    }
                });

                if (!hasItems) {
                    const T = window.i18nQuote || {};
                    $list.append(`<div class="ms-empty">${T.NoResults || 'Không có kết quả'}</div>`);
                }
                if (isRemoteMaterial && remoteState.hasMore) {
                    $list.append('<div class="ms-loading">Loading more...</div>');
                }
            };

            const updateButtonText = () => {
                const val = $select.val();
                const source = isRemoteMaterial ? remoteState.options : domOptions;
                const found = source.find(o => o.value === val);
                if (found?.text) {
                    $btn.find('.ms-values').text(found.text);
                    $btn.find('.ms-placeholder').text('');
                } else {
                    const T = window.i18nQuote || {};
                    $btn.find('.ms-values').text('');
                    $btn.find('.ms-placeholder').text(T.SelectPlaceholder || '-- Chọn --');
                }
            };

            // Build UI
            const $wrapper = $('<div class="ms-container"></div>');
            const $btn = $('<div class="ms-btn"><span class="ms-values"></span><span class="ms-placeholder"></span><span class="ms-caret">▾</span></div>');
            const $dropdown = $('<div class="ms-dropdown"></div>');
            const $search = $('<div class="ms-search"><input type="text" placeholder="Tìm..." /></div>');
            const $list = $('<div class="ms-list" style="max-height:320px; overflow:auto"></div>');

            updateButtonText();
            renderList('');

            $dropdown.append($search).append($list);
            $select.after($wrapper);
            $wrapper.append($btn).append($dropdown);
            $select.hide();
            $dropdown.data('wrapper', $wrapper);

            // Events
            $btn.on('click', function (e) {
                e.stopPropagation();

                $('.ms-dropdown').not($dropdown).each(function () {
                    const $other = $(this);
                    if ($other.hasClass('open')) {
                        $other.removeClass('open');
                        if ($other.data('detached')) {
                            $other.appendTo($other.data('wrapper')).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                        }
                    }
                });

                if ($dropdown.hasClass('open')) {
                    $dropdown.removeClass('open');
                    if ($dropdown.data('detached')) {
                        $dropdown.appendTo($dropdown.data('wrapper')).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                    }
                } else {
                    const btnRect = $btn[0].getBoundingClientRect();
                    $dropdown.appendTo('body').css({
                        position: 'absolute',
                        top: btnRect.top + window.scrollY + $btn.outerHeight() + 'px',
                        left: btnRect.left + window.scrollX + 'px',
                        width: $btn.outerWidth() + 'px',
                        zIndex: 3000
                    }).addClass('open').data('detached', true);
                    $search.find('input').val('');
                    if (isRemoteMaterial && (!remoteState.options.length)) loadRemote('');
                    else renderList('');
                    $search.find('input').focus();
                }
            });

            $(document).on('click.quoteDropdown', () => {
                $('.ms-dropdown').each(function () {
                    const $d = $(this);
                    if ($d.hasClass('open')) {
                        $d.removeClass('open');
                        if ($d.data('detached')) {
                            $d.appendTo($d.data('wrapper')).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                        }
                    }
                });
            });

            $dropdown.on('click', e => e.stopPropagation());

            $list.on('click', '.ms-item', function () {
                const value = $(this).attr('data-value');
                if ($select.find(`option[value="${value.replace(/"/g, '\\"')}"]`).length === 0) {
                    const txt = (remoteState.options.find(o => o.value === value) || {}).text || $(this).text();
                    $select.append(`<option value="${value}">${txt || value}</option>`);
                }
                $select.val(value);
                try { $select.trigger('change'); } catch { }
                try { $select[0]?.dispatchEvent(new Event('change', { bubbles: true })); } catch { }
                updateButtonText();
                $dropdown.removeClass('open');
                if ($dropdown.data('detached')) {
                    $dropdown.appendTo($dropdown.data('wrapper')).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                }
            });

            if (isRemoteMaterial) {
                let scrollTimer = null;
                $list.on('scroll', function () {
                    clearTimeout(scrollTimer);
                    scrollTimer = setTimeout(() => {
                        if (this.scrollTop + this.clientHeight >= this.scrollHeight - 40) {
                            if (remoteState.hasMore && !remoteState.loading) {
                                loadRemote(remoteState.lastQuery || '', true);
                            }
                        }
                    }, CONFIG.SCROLL_THROTTLE);
                });
            }

            let searchTimerLocal = null;
            $search.find('input').on('input', function () {
                const q = $(this).val() || '';
                if (isRemoteMaterial) {
                    clearTimeout(searchTimerLocal);
                    searchTimerLocal = setTimeout(() => loadRemote(q, false), CONFIG.SEARCH_DELAY);
                } else {
                    renderList(q);
                }
            });

            $select.data('search-dropdown', true);
        });
    };

    // ==================== TEN THU TUC HAI QUAN ====================
    const updateTenThuTucHaiQuan = (tr) => {
        const classMaterial = tr.querySelector('.tenPhanLoaiTb')?.value || '';
        const categorySel = tr.querySelector('.chungLoaiTb');
        const categoryVN = categorySel?.options[categorySel.selectedIndex]?.text || '';
        const shape = getInputValue(tr, 'hinhDang');
        const material = getInputValue(tr, 'chatLieu');
        const composition = getInputValue(tr, 'thanhPhan');
        const dimension = getInputValue(tr, 'kichThuoc');
        const usedFor = getInputValue(tr, 'viTriSuDung');
        const purpose = getInputValue(tr, 'tinhNang');

        let tenHangVN = '';
        if (classMaterial === 'NO LIST') {
            tenHangVN = `Có hình dáng dạng ${shape} & ${usedFor} & ${purpose}`;
        } else if (!['A', 'E', 'I'].includes(classMaterial)) {
            tenHangVN = `${categoryVN} có hình dáng dạng ${shape} chất liệu ${material} thành phần hóa chất ${composition} có kích thước ${dimension} dung để ${usedFor} cho ${purpose}`;
        }

        const vnInput = tr.querySelector('input[id^="tenHangVN_"]');
        if (vnInput) vnInput.value = tenHangVN;
    };

    const getInputValue = (tr, className) => tr.querySelector(`.${className}`)?.value || '';

    // ==================== EVENT HANDLERS ====================
    const wireEvents = () => {
        const container = qs('#quote-request');
        if (!container) return;

        // Department change handler
        qs(SELECTORS.TABLE_BODY)?.addEventListener('focusin', (e) => {
            const t = e.target;
            if (t?.classList?.contains('tenPhongBanTb')) t.dataset.prev = t.value || '';
        }, true);

        qs(SELECTORS.TABLE_BODY)?.addEventListener('change', async (e) => {
            const t = e.target;
            if (!t?.classList?.contains('tenPhongBanTb')) return;

            const newVal = t.value || '';
            const prevVal = t.dataset.prev || '';

            const rows = qsa('#quoteTableBody tr');
            const sections = new Set();
            for (const r of rows) {
                const s = r.querySelector('.tenPhongBanTb')?.value;
                if (s) sections.add(s);
            }

            if (sections.size > 1) {
                t.value = prevVal;
                updateSearchableSelectDisplay(t);
                const T = window.i18nQuote || {};
                showDialog({ title: T.ErrorTitle || 'Lỗi', message: 'Không được chọn 2 mã phòng khác nhau trong cùng 1 đơn', type: 'error' });
                return;
            }

            const single = sections.size === 1 ? Array.from(sections)[0] : '';
            if (single) await loadApprovers(single);
            else {
                const approverSel = qs(SELECTORS.APPROVER_SELECT);
                if (approverSel) approverSel.innerHTML = `<option value="">${(window.i18nQuote?.SelectApprover) || '-- Select --'}</option>`;
            }
        });

        // Button handlers
        qs(SELECTORS.BTN_ADD_ROW)?.addEventListener('click', addRow);
        qs(SELECTORS.BTN_RESET)?.addEventListener('click', resetForm);
        qs(SELECTORS.BTN_CREATE)?.addEventListener('click', submitForm);
        qs(SELECTORS.BTN_AUTO)?.addEventListener('click', exportAutoRender);
        qs(SELECTORS.BTN_DOWN_EXCEL)?.addEventListener('click', exportTable);
        qs(SELECTORS.BTN_CLEAR_FILTERS)?.addEventListener('click', () => {
            qsa(SELECTORS.FILTER_INPUT).forEach(inp => inp.value = '');
            state.currentPage = 1;
            applyFiltersAndPagination();
        });

        // Rows per page
        const rowsPerPageSelect = qs(SELECTORS.ROWS_PER_PAGE_SELECT);
        if (rowsPerPageSelect) {
            state.rowsPerPage = parseInt(rowsPerPageSelect.value) || CONFIG.ROWS_PER_PAGE;
            rowsPerPageSelect.addEventListener('change', (e) => {
                const v = parseInt(e.target.value);
                if (isFinite(v) && v > 0) {
                    state.rowsPerPage = v;
                    state.currentPage = 1;
                    applyFiltersAndPagination();
                }
            });
        }

        // Remove row buttons
        qsa('.btn-remove-row', container).forEach(btn => {
            btn.addEventListener('click', e => removeRow(e.currentTarget));
        });

        qs(SELECTORS.TABLE_BODY)?.addEventListener('click', (e) => {
            if (e.target.closest('.btn-remove-row')) removeRow(e.target.closest('.btn-remove-row'));
        });

        // Upload Excel
        qs(SELECTORS.BTN_UPLOAD_EXCEL)?.addEventListener('click', () => qs(SELECTORS.EXCEL_UPLOAD)?.click());
        qs(SELECTORS.EXCEL_UPLOAD)?.addEventListener('change', async (e) => {
            const file = e.target.files?.[0];
            if (!file) return;

            try {
                showLoading((window.i18nQuote?.Exporting) || 'Đang xử lý...');
                const fd = new FormData();
                fd.append('file', file);
                const res = await fetch(api.uploadQuoteExcel, { method: 'POST', body: fd });
                if (!res.ok) throw new Error(await res.text());

                const items = await res.json();
                if (!Array.isArray(items)) throw new Error((window.i18nQuote?.MsgInvalidData) || 'Dữ liệu không hợp lệ');

                await populateTableFromItems(items);
                const T = window.i18nQuote || {};
                showDialog({ title: T.SuccessTitle || 'Thành công', message: (T.MsgLoadedRows || 'Đã tải {0} dòng từ Excel').replace('{0}', items.length), type: 'success' });
            } catch (err) {
                const T = window.i18nQuote || {};
                showDialog({ title: T.ErrorTitle || 'Lỗi', message: err.message || T.MsgCannotReadFile || 'Không thể đọc file', type: 'error' });
            } finally {
                hideLoading();
                e.target.value = '';
            }
        });

        // Download template
        qs('#btnDownloadExcel')?.addEventListener('click', () => {
            try {
                showLoading((window.i18nQuote?.Exporting) || 'Đang xử lý...');
                const url = (window.apiBaseUrl || '') + '/template/TemPlateQuote.xlsx';
                const a = document.createElement('a');
                a.href = url;
                a.download = 'Mau_Quote.xlsx';
                document.body.appendChild(a);
                a.click();
                a.remove();
            } catch (err) {
                console.error('Error downloading template', err);
            } finally {
                hideLoading();
            }
        });

        // Table change handlers
        qs(SELECTORS.TABLE_BODY)?.addEventListener('change', async (e) => {
            const t = e.target;
            if (!t?.classList) return;

            if (t.classList.contains('maHangNoiBo')) {
                await autofillFromMaterialSelect(t);
                return;
            }

            if (t.classList.contains('chungLoaiTb')) {
                const tr = t.closest('tr');
                await autoAddRowByCategory(t);

                try {
                    const T18 = window.i18nQuote || {};
                    const nhomHang = t.value || '';
                    const res = await fetch(api.searchMaterials, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ MaHang: '', Name: '', NhomHang: nhomHang, PageIndex: 0, PageSize: 0 })
                    });
                    if (!res.ok) throw new Error(await res.text());

                    const materials = await res.json();
                    const sel = tr.querySelector('.maHangNoiBo');
                    if (sel) {
                        const next = sel.nextElementSibling;
                        if (next?.classList?.contains('ms-container')) next.remove();
                        const prevValue = sel.value;
                        sel.style.display = '';
                        sel.innerHTML = `<option value="">${T18.SelectInternalMaterialCode || ''}</option>`;

                        if (Array.isArray(materials)) {
                            materials.forEach(m => {
                                const code = m.material_Code || '';
                                if (code) {
                                    const opt = document.createElement('option');
                                    opt.value = code;
                                    opt.textContent = `${code} - ${m.material_Name_VN || ''}`;
                                    sel.appendChild(opt);
                                }
                            });
                        }

                        if (prevValue && Array.from(sel.options).some(o => o.value === prevValue)) sel.value = prevValue;
                        else sel.selectedIndex = 0;

                        try { $(sel).data('search-dropdown', false); } catch { }
                        buildSearchableDropdown($(sel));
                    }
                } catch (err) {
                    console.warn('Không thể tải danh sách vật tư:', err);
                }
                return;
            }
        });

        // Auto-generate ten thu tuc hai quan
        qs(SELECTORS.TABLE_BODY)?.addEventListener('input', (e) => {
            const t = e.target;
            const fields = ['chungLoaiTb', 'hinhDang', 'chatLieu', 'thanhPhan', 'kichThuoc', 'viTriSuDung', 'tinhNang'];
            if (fields.some(f => t.classList.contains(f))) {
                updateTenThuTucHaiQuan(t.closest('tr'));
            }
        });

        qs(SELECTORS.TABLE_BODY)?.addEventListener('change', (e) => {
            if (e.target.classList.contains('chungLoaiTb')) {
                updateTenThuTucHaiQuan(e.target.closest('tr'));
            }
        });

        // Filter and pagination
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('filter-input')) {
                state.currentPage = 1;
                applyFiltersAndPagination();
            }
        });

        qs(SELECTORS.PREV_PAGE)?.addEventListener('click', (e) => {
            e.preventDefault();
            if (state.currentPage > 1) {
                state.currentPage--;
                const tbody = qs(SELECTORS.TABLE_BODY);
                if (state.filteredQuoteItems.length) renderQuotePage(tbody, state.filteredQuoteItems);
                else applyFiltersAndPagination();
            }
        });

        qs(SELECTORS.NEXT_PAGE)?.addEventListener('click', (e) => {
            e.preventDefault();
            const totalCount = state.filteredQuoteItems.length || state.filteredRows.length;
            const totalPages = Math.max(1, Math.ceil(totalCount / state.rowsPerPage));
            if (state.currentPage < totalPages) {
                state.currentPage++;
                const tbody = qs(SELECTORS.TABLE_BODY);
                if (state.filteredQuoteItems.length) renderQuotePage(tbody, state.filteredQuoteItems);
                else applyFiltersAndPagination();
            }
        });
    };

    // ==================== INITIALIZATION ====================
    document.addEventListener('DOMContentLoaded', () => {
        buildSearchableDropdown($(document));
        wireEvents();
        renumberRows();
        applyFiltersAndPagination();
    });
})();
