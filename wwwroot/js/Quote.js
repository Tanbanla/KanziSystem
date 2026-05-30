
(() => {
    const api = {
        insertListBaoGia: (window.apiBaseUrl || '') + '/Quote/InsertDanhSachBaoGia',
        getMaterials: (keyword) => (window.apiBaseUrl || '') + `/Quote/GetMaterialsByNameOrCode?keyword=${encodeURIComponent(keyword || '')}`,
        searchMaterials: (window.apiBaseUrl || '') + '/Quote/GetSearchMaterial'
        , uploadQuoteExcel: (window.apiBaseUrl || '') + '/Quote/UploadQuoteExcel'//UploadQuoteExcelBackup
        , exportAutoRender: (window.apiBaseUrl || '') + '/Quote/ExportAutoRender'
        , getNCCByCategory: (window.apiBaseUrl || '') + '/Quote/GetNCCByCategory'
        , exportRenderOutSide: (window.apiBaseUrl || '') + '/Quote/ExportRenderOutSide'
        , exportTable: (window.apiBaseUrl || '') + '/Quote/ExportTable'
        , searchApprover: (window.apiBaseUrl || '') + '/Quote/GetListApprovel'
        ,downloadMasterMaterial: `${window.apiBaseUrl || ''}/Master/ExportExcelMasterMaterial`
        ,downloadMasterVendor: `${window.apiBaseUrl || ''}/Master/ExportExcelMasterVendor`
        ,checkNCC: (window.apiBaseUrl || '') + '/Quote/CheckNCC'
    };

    const qs = (sel, root = document) => root.querySelector(sel);
    const qsa = (sel, root = document) => Array.from(root.querySelectorAll(sel));

    let currentPage = 1;
    let rowsPerPage = 5;
    let filteredRows = [];
    // in-memory storage for large dataset to avoid rendering all rows at once
    let allQuoteItems = [];
    let filteredQuoteItems = [];
    let SetionNameFirst = '';

    function renumberRows() {
        qsa('#quoteTableBody tr').forEach((tr, idx) => {
            const noCell = tr.children[0];
            if (noCell) noCell.textContent = String(idx + 1);
        });
        assignRowIds();
    }
    
    // Kiểm tra NCC có cung cấp chủng loại hay không
    async function checkNccCategory(maNcc, category) {
        try {
            const body = { maNcc: maNcc, category: category };
            const res = await fetch(api.checkNCC, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!res.ok) return false;
            const data = await res.json();
            return data && data.success !== false;
        } catch (err) {
            console.warn('Lỗi gọi API CheckNCC:', err);
            return true;
        }
    }

    // Generate a single request code (CHR_MaDon) for the whole submission
    function generateMaDonRequest(section) {
        try {
            const now = new Date();
            const utc = now.getTime() + (now.getTimezoneOffset() * 60000);
            const nowVN = new Date(utc + (7 * 60 * 60000));
            const yyyy = nowVN.getFullYear();
            const MM = String(nowVN.getMonth() + 1).padStart(2, '0');
            const dd = String(nowVN.getDate()).padStart(2, '0');
            const sec = (section || '').toString().trim().replace(/[^a-zA-Z0-9_-]/g, '_') || 'GEN';
            return `RQ_${sec}_${yyyy}_${MM}_${dd}`;
        } catch (e) {
            console.warn('Error merging visible row into full dataset', e);
        }
    }
    // Determine if a row is completely empty (no user-entered text/number/date and no meaningful select)
    function isRowEmpty(tr) {
        if (!tr) return true;
        // Check inputs (text, number, date, textarea)
        const inputs = Array.from(tr.querySelectorAll('input, textarea'));
        for (const inp of inputs) {
            // ignore hidden, file inputs
            if (inp.type === 'hidden' || inp.type === 'file' || inp.type === 'checkbox' || inp.type === 'radio') continue;
            const v = (inp.value || '').toString().trim();
            if (v !== '') return false;
        }

        // Check selects: ignore selects that have default values like 'true','false','No Need'
        const selects = Array.from(tr.querySelectorAll('select'));
        const ignoreVals = new Set(['', 'true', 'false', 'No Need']);
        for (const sel of selects) {
            const v = (sel.value || '').toString().trim();
            if (!ignoreVals.has(v)) return false;
        }

        return true;
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

    function updateSearchableSelectDisplay(sel) {
        if (!sel) return;
        try {
            const wrapper = sel.nextElementSibling;
            if (!wrapper || !wrapper.classList || !wrapper.classList.contains('ms-container')) return;
            const btn = wrapper.querySelector('.ms-btn');
            const values = wrapper.querySelector('.ms-values');
            const placeholder = wrapper.querySelector('.ms-placeholder');
            const opt = Array.from(sel.options).find(o => o.value === sel.value);
            if (opt && opt.text) {
                if (values) values.textContent = opt.text;
                if (placeholder) placeholder.textContent = '';
            } else {
                if (values) values.textContent = '';
                if (placeholder) placeholder.textContent = '-- Chọn --';
            }
        } catch (e) { /* ignore */ }
    }

    function assignRowIds() {
        // Ensure each row's fields have unique ids matching the pattern used in the server view
        qsa('#quoteTableBody tr').forEach((tr, idx) => {
            const i = idx + 1;
            const setId = (sel, idBase) => {
                const el = tr.querySelector(sel);
                if (el) el.id = `${idBase}_${i}`;
            };
            setId('.tenPhongBanTb', 'tenPhongBanTb');
            setId('.chungLoaiTb', 'chungLoai');
            setId('.tenPhanLoaiTb', 'tenPhanLoaiTb');
            setId('.maHangNoiBo', 'maHangNoiBo');
            setId('maThietBi', 'maThietBi');
            setId('maHangNCC', 'maHangNCC');
            setId('tenHangVN', 'tenHangVN');
            setId('tenHangEN', 'tenHangEN');
            setId('soLuong', 'soLuong');
            setId('donVi', 'donVi');
            setId('hinhDang', 'hinhDang');
            setId('chatLieu', 'chatLieu');
            setId('thanhPhan', 'thanhPhan');
            setId('kichThuoc', 'kichThuoc');
            setId('viTriSuDung', 'viTriSuDung');
            setId('tinhNang', 'tinhNang');
            setId('.rohsTb', 'rohsTb');
            setId('.CoCqTb', 'CoCqTb');
            setId('msds', 'msds');
            setId('tieuChuanAnToan', 'tieuChuanAnToan');
            setId('fileThietKe', 'fileThietKe');
            setId('nsx', 'nsx');
            setId('.nhaCungCapTb', 'nhaCungCapTb');
            setId('.laybaogiaTb', 'laybaogiaTb');
            setId('input[placeholder*="Lý do"]', 'lyDo');
            setId('.gapTb', 'gapTb');
            setId('nguoiYeuCauRow', 'nguoiYeuCauRow');

            // Set ID cho các date inputs
            const dateInputs = qsa('input[type="date"]', tr);
            if (dateInputs.length >= 1) dateInputs[0].id = `ngayMuonNhan_${i}`;
            if (dateInputs.length >= 2) dateInputs[1].id = `kyHanChonNCC_${i}`;
        });
    }

    function applyFiltersAndPagination() {
        const tbody = qs('#quoteTableBody');
        const allRows = Array.from(tbody.querySelectorAll('tr'));
        const filters = Array.from(document.querySelectorAll('.filter-input')).map(inp => inp.value.toLowerCase().trim());
        const T = window.i18nQuote || {};

        function getCellText(td) {
            const select = td.querySelector('select');
            if (select) {
                const opt = select.options[select.selectedIndex];
                return opt ? opt.text : '';
            }
            const input = td.querySelector('input');
            if (input && input.type !== 'date') {
                return input.value || '';
            }
            return td.textContent.trim();
        }

        // If we have items stored in memory (large dataset), filter in-memory to avoid scanning all DOM rows
        if (Array.isArray(allQuoteItems) && allQuoteItems.length > 0) {
            filteredQuoteItems = allQuoteItems.filter(dto => {
                // combine common searchable fields into one string for simple contains checks
                const combined = [
                    dto.chR_MaHangNoiBo, dto.chR_MaHangNCC, dto.nvchR_NameVN, dto.chR_NameEN,
                    dto.nvchR_DonVi, dto.chR_MaNCC, dto.nvchR_TenNCC, dto.nvchR_ChungLoai, dto.chR_Phanloai
                ].map(v => (v || '').toString().toLowerCase()).join(' ');
                return filters.every(filter => !filter || combined.includes(filter));
            });
            // render only visible page from filteredQuoteItems
            renderQuotePage(tbody, filteredQuoteItems);
            return;
        }

        filteredRows = allRows.filter(tr => {
            const tds = Array.from(tr.querySelectorAll('td'));
            return filters.every((filter, idx) => {
                if (!filter) return true;
                const td = tds[idx];
                if (!td) return true;
                const text = getCellText(td);
                return text.toLowerCase().includes(filter);
            });
        });

        const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
        if (currentPage > totalPages) currentPage = totalPages || 1;
        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;
        const visibleRows = filteredRows.slice(start, end);

        allRows.forEach(tr => tr.style.display = 'none');
        visibleRows.forEach(tr => tr.style.display = '');

        const pagination = qs('#paginationControls');
        const prev = qs('#prevPage');
        const next = qs('#nextPage');
        if (totalPages > 1) {
            pagination.style.display = '';
            prev.classList.toggle('disabled', currentPage === 1);
            next.classList.toggle('disabled', currentPage === totalPages);
        } else {
            pagination.style.display = 'none';
        }

        // Update pagination info
        const startEntry = (currentPage - 1) * rowsPerPage + 1;
        const endEntry = Math.min(currentPage * rowsPerPage, filteredRows.length);
        const totalEntries = filteredRows.length;
        const pageInfoText = `${T.Showing} ${startEntry} ~ ${endEntry} ${T.Of} ${totalEntries}`;
        qs('#pageInfo').textContent = pageInfoText;
        const pageNumberText = `${currentPage}/${totalPages}`;
        qs('#pageNumberInfo').textContent = pageNumberText;
        qs('#paginationInfo').style.display = totalPages > 1 ? '' : 'none';

        visibleRows.forEach((tr, idx) => {
            const noCell = tr.children[0];
            if (noCell) noCell.textContent = String(start + idx + 1);
        });
    }

    function addRow() {
        const tbody = qs('#quoteTableBody');
        const lastRow = tbody.lastElementChild;
        const newRow = lastRow ? lastRow.cloneNode(true) : null;
        if (!newRow) return;
        // clear inputs/selects
        qsa('input', newRow).forEach((inp) => {
            inp.value = '';
            inp.classList.remove('is-invalid');
        });
        qsa('select', newRow).forEach((sel) => {
            if (sel.classList.contains('rohsTb')) {
                sel.value = 'No Need';
            } else if (sel.classList.contains('laybaogiaTb')) {
                sel.value = 'true';
            } else if (sel.classList.contains('gapTb')) {
                sel.value = 'false';
            } else {
                sel.value = '';
            }
            sel.classList.remove('is-invalid');
            // if previously enhanced as searchable, remove flag so we can re-initialize
            try { $(sel).data('search-dropdown', false); } catch { }
        });

        // Remove any old searchable dropdown wrappers copied by clone
        // and show original selects so they can be re-enhanced
        qsa('.ms-container', newRow).forEach(w => w.remove());
        qsa('select.searchable-select', newRow).forEach(s => { s.style.display = ''; });

        tbody.appendChild(newRow);

        // Re-initialize searchable dropdowns for the new row
        try { buildSearchableDropdown($(newRow)); } catch { }

        renumberRows();
        applyFiltersAndPagination();
    }

    function removeRow(btn) {
        const tbody = qs('#quoteTableBody');
        const rows = qsa('tr', tbody);
        const tr = btn.closest('tr');
        
        if (rows.length > 1 && tr) {
            if (Array.isArray(allQuoteItems) && allQuoteItems.length > 0) {
                const start = (currentPage - 1) * rowsPerPage;
                const rowIndex = Array.from(tbody.querySelectorAll('tr')).indexOf(tr);
                const globalIndex = start + rowIndex;
                
                if (globalIndex >= 0 && globalIndex < allQuoteItems.length) {
                    allQuoteItems.splice(globalIndex, 1);
                    filteredQuoteItems = allQuoteItems.slice();

                    renderQuotePage(tbody, filteredQuoteItems);
                    return;
                }
            }
            tr.remove();
            renumberRows();
            applyFiltersAndPagination();
        }
    }

    function resetForm() {
        const form = qs('#quoteForm');
        form.reset();
        const tbody = qs('#quoteTableBody');
        if (tbody) {
            while (tbody.children.length > 5) {
                tbody.removeChild(tbody.lastElementChild);
            }

            qsa('.ms-container', tbody).forEach((w) => w.remove());

            qsa('select.searchable-select', tbody).forEach((sel) => {
                try {
                    const next = sel.nextElementSibling;
                    if (next && next.classList && next.classList.contains('ms-container')) next.remove();
                } catch (e) { }

                sel.style.display = '';

                try { $(sel).data('search-dropdown', false); } catch (e) { }

                if (sel.classList.contains('rohsTb')) {
                    sel.value = 'No Need';
                } else if (sel.classList.contains('laybaogiaTb')) {
                    sel.value = 'true';
                } else if (sel.classList.contains('gapTb')) {
                    sel.value = 'false';
                } else {
                    if (sel.options && sel.options.length) sel.selectedIndex = 0;
                }
                sel.classList.remove('is-invalid');
            });

            qsa('tr', tbody).forEach((tr) => {
                qsa('input', tr).forEach((inp) => {
                    if (inp.type === 'checkbox' || inp.type === 'radio') inp.checked = false;
                    else inp.value = '';
                    inp.classList.remove('is-invalid');
                });
            });
            allQuoteItems = [];
            filteredQuoteItems = [];
        }

        qsa('input, select', form).forEach((el) => el.classList.remove('is-invalid'));


        try {
            $('.ms-dropdown.open').each(function () {
                const $d = $(this);
                $d.removeClass('open');
                if ($d.data('detached')) {
                    const $wrapper = $d.data('wrapper');
                    if ($wrapper && $wrapper.length) $d.appendTo($wrapper).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                }
            });
        } catch (ex) { /* ignore if jquery missing */ }

        try { buildSearchableDropdown($(document)); }
        catch (ex) { console.error('Error re-initializing searchable dropdowns:', ex); }

        renumberRows();
        applyFiltersAndPagination();
    }

    function validateRow(tr) {
        // required fields per row: department, internal code, VN name, EN name, qty, unit, supplier, laybaogia, desired date
        let ok = true;

        const validateField = (selector, isSelect = false) => {
            let element;

            if (isSelect) {
                element = tr.querySelector(selector);
            } else {
                const elements = qsa(selector, tr);
                if (elements.length > 0) {
                    element = elements[0];
                }
            }

            if (!element) return false;

            const val = element.value ? element.value.toString().trim() : '';
            const isValid = val !== '';

            if (!isValid) ok = false;

            element.classList.toggle('is-invalid', !isValid);

            if (element.classList && element.classList.contains('searchable-select')) {
                const $element = $(element);
                const $wrapper = $element.siblings('.ms-container');
                if ($wrapper.length) {
                    $wrapper.find('.ms-btn').toggleClass('is-invalid', !isValid);
                }
            }

            return isValid;
        };

        // Danh sách các trường bắt buộc (có dấu *)
        const requiredFields = [
            // Phòng ban (select)
            { selector: '.tenPhongBanTb', isSelect: true, name: 'Phòng ban' },

            // Chủng loại (select) - thêm điều kiện bắt buộc
            { selector: '.chungLoaiTb', isSelect: true, name: 'Chủng loại' },

            // Mã hàng nội bộ (select)
            //{ selector: '.maHangNoiBo', isSelect: true, name: 'Mã hàng nội bộ' },

            // Tên hàng VN (input)
            { selector: 'input[name^="tenHangVN_"]', isSelect: false, name: 'Tên hàng VN' },

            // Tên hàng EN (input)
            { selector: 'input[name^="tenHangEN_"]', isSelect: false, name: 'Tên hàng EN' },

            // Số lượng (input number)
            { selector: 'input[type="number"]', isSelect: false, name: 'Số lượng' },

            // Đơn vị (input)
            { selector: 'input[name^="donVi_"]', isSelect: false, name: 'Đơn vị' },

            // Nhà cung cấp (select)
            { selector: '.nhaCungCapTb', isSelect: true, name: 'Nhà cung cấp' },

            // Lấy báo giá (select)
            { selector: '.laybaogiaTb', isSelect: true, name: 'Lấy báo giá' }
        ];

        requiredFields.forEach(field => {
            validateField(field.selector, field.isSelect);
        });

        // Ngày muốn nhận hàng (required - có dấu *)
        const dateInputs = qsa('input[type="date"]', tr);
        if (dateInputs.length >= 1) {
            const ngayMuonNhan = dateInputs[0]; 
            const ngayMuonNhanValid = ngayMuonNhan.value && ngayMuonNhan.value.toString().trim() !== '';

            if (!ngayMuonNhanValid) ok = false;
            ngayMuonNhan.classList.toggle('is-invalid', !ngayMuonNhanValid);
        }

        try {
            const maHangNoiBoEl = tr.querySelector('.maHangNoiBo');
            const maHangNCCEl = tr.querySelector('input[id^="maHangNCC_"]') || tr.querySelector('input[placeholder*="mã hàng ncc"]');
            const hasInternal = maHangNoiBoEl && (maHangNoiBoEl.value || '').toString().trim() !== '';
            const hasNcc = maHangNCCEl && (maHangNCCEl.value || '').toString().trim() !== '';
            if (!hasInternal) {
                // internal not provided -> supplier code required
                if (!hasNcc) {
                    ok = false;
                    if (maHangNCCEl) maHangNCCEl.classList.add('is-invalid');
                } else {
                    if (maHangNCCEl) maHangNCCEl.classList.remove('is-invalid');
                }
            } else {
   
                if (maHangNCCEl) maHangNCCEl.classList.remove('is-invalid');
            }
        } catch (e) { }

        return ok;
    }
    // điền thông tin dữ liệu từ một hàng
    function collectRow(tr) {
        const getSel = (selector) => {
            const el = tr.querySelector(selector);
            return el ? (el.value || '').toString() : '';
        };
        // lay thong tin 
        const src = window.indexQuoteData || {};

        const getSelDisplay = (selector) => {
            const el = tr.querySelector(selector);
            if (!el) return '';
            try {
                const wrapper = el.nextElementSibling;
                if (wrapper && wrapper.classList && wrapper.classList.contains('ms-container')) {
                    const values = wrapper.querySelector('.ms-values');
                    if (values && values.textContent && values.textContent.trim() !== '') {
                        SetionNameFirst = values.textContent.trim();
                        return values.textContent.trim();
                    } 
                }
            } catch (e) { /* ignore */ }
            // fallback to option text
            try {
                const opt = el.options && el.options[el.selectedIndex];
                if (opt && opt.text) return opt.text;
            } catch (e) { }
            return el.value ? el.value.toString() : '';
        };

        const getInputBy = (selectors) => {
            for (const s of selectors) {
                const el = tr.querySelector(s);
                if (el) return (el.value || '').toString();
            }
            return '';
        };

        const dates = qsa('input[type="date"]', tr);

        // Hàm lấy ngày giờ hiện tại theo múi giờ +7 (Việt Nam)
        const getVietnamTime = () => {
            const now = new Date();
            // Lấy UTC time và cộng thêm 7 giờ
            const utc = now.getTime() + (now.getTimezoneOffset() * 60000);
            return new Date(utc + (7 * 60 * 60000)); // +7 giờ
        };

        // Hàm format date thành string theo định dạng yyyy-mm-dd
        const formatDateString = (date) => {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        // Hàm format datetime thành ISO string với múi giờ +7
        const toVietnamISOString = (date) => {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            const seconds = String(date.getSeconds()).padStart(2, '0');
            return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}+07:00`;
        };

        // Lấy ngày tạo theo múi giờ +7
        const createDateVN = getVietnamTime();

        const obj = {
            ID: 0,
            CHR_MaDon: '',
            CHR_MaThietBi: getInputBy(['input[id^="maThietBi_"]', 'input[placeholder*="Mã thiết bị"]']),
            CHR_Phanloai: getSel('.tenPhanLoaiTb'),
            CHR_MaHangNoiBo: getSel('.maHangNoiBo'),
            CHR_MaHangNCC: getInputBy(['input[id^="maHangNCC_"]', 'input[placeholder*="Mã hàng NCC"]']),
            NVCHR_NameVN: getInputBy(['input[id^="tenHangVN_"]', 'input[placeholder*="thủ tục hải quan"]']),
            CHR_NameEN: getInputBy(['input[id^="tenHangEN_"]', 'input[placeholder*="tên hàng"]']),
            INT_SoLuong: getInputBy(['input[type="number"]']),
            NVCHR_DonVi: getInputBy(['input[id^="donVi_"]', 'input[placeholder*="Đơn vị"]']),
            NVCHR_ChungLoai: getSel('.chungLoaiTb'),
            NVCHR_HinhDang: getInputBy(['input[id^="hinhDang_"]', 'input[placeholder*="Hình dáng"]']),
            NVCHR_ChatLieu: getInputBy(['input[id^="chatLieu_"]', 'input[placeholder*="Chất liệu"]']),
            NVCHR_ThanhPhan: getInputBy(['input[id^="thanhPhan_"]', 'input[placeholder*="Thành phần"]']),
            NVCHR_KichThuoc: getInputBy(['input[id^="kichThuoc_"]', 'input[placeholder*="Kích thước"]']),
            NVCHR_DongMay: getInputBy(['input[id^="viTriSuDung_"]', 'input[placeholder*="Dùng cho máy"]']),
            NVCHR_TinhNang: getInputBy(['input[id^="tinhNang_"]', 'input[placeholder*="Dùng để làm gì"]']),
            NVCHR_Rohs: getSel('.rohsTb'),
            NVCHR_COCQ: getSel('.CoCqTb'),
            NVCHR_MSDS: getInputBy(['input[id^="msds_"]', 'input[placeholder*="MSDS"]']),
            NVCHR_AnToan: getInputBy(['input[id^="tieuChuanAnToan_"]', 'input[placeholder*="an toàn"]']),
            NVCHR_FileThietKe: getInputBy(['input[id^="fileThietKe_"]', 'input[placeholder*="File thiết kế"]']),
            CHR_LinkFile: getInputBy(['input[id^="linkFile_"]', 'input[placeholder*="Link"]']),
            NVCHR_NhaSanXuat: getInputBy(['input[id^="nsx_"]', 'input[placeholder*="NSX"]']),
            CHR_MaNCC: getSel('.nhaCungCapTb'),
            NVCHR_TenNCC: getSelDisplay('.nhaCungCapTb'),
            BIT_LayBaoGia: getSel('.laybaogiaTb') === 'true' || getSel('.laybaogiaTb') === '1' ? true : false,
            NVCHR_LyDo: getInputBy(['input[id^="lyDo_"]', 'input[placeholder*="Lý do"]']),
            DTM_NgayMuonNhan: (tr.querySelector('input[id^="ngayMuonNhan_"]') || dates[0])?.value || null,
            DTM_KyHan: (tr.querySelector('input[id^="kyHanChonNCC_"]') || dates[1])?.value || null,
            CHR_Gap: getSel('.gapTb'),
            CHR_SectionCode: getSel('.tenPhongBanTb'),
            CHR_SectionName: getSelDisplay('.tenPhongBanTb'),
            NVCHR_UserRequest: getInputBy(['input[id^="nguoiYeuCauRow_"]', 'input[placeholder*="Người yêu cầu"]']) === '' ? src.user : getInputBy(['input[id^="nguoiYeuCauRow_"]', 'input[placeholder*="Người yêu cầu"]']),
            CHR_CreateBy: window.indexQuoteData?.user ?? '',
            // Sử dụng ISO string với múi giờ +7
            DTM_CreateDate: toVietnamISOString(createDateVN),
            // Người phê duyệt (lấy từ select trên form)
            CHR_UserApproval: (document.querySelector('#approverSelect') || {}).value || '',
            ID_StepBaoGia: 2,
            ID_Status: 'CREATE',
            INT_SoLanUpdate: 0,
            DTM_UpdateLater: null,
            DTM_Deadline: null,
            BIT_IsTemplate: false
        };

        // Normalize numeric and date fields so JSON deserializer can parse them
        if (obj.INT_SoLuong === '') {
            obj.INT_SoLuong = null;
        } else if (obj.INT_SoLuong != null) {
            // parse as number (allow decimals)
            const n = parseFloat(obj.INT_SoLuong);
            obj.INT_SoLuong = Number.isFinite(n) ? n : null;
        }

        // Xử lý ngày tháng từ input date (giả định người dùng chọn theo giờ VN)
        if (obj.DTM_NgayMuonNhan === '' || obj.DTM_NgayMuonNhan == null) {
            obj.DTM_NgayMuonNhan = null;
        } else {
            try {
                // Tạo ngày từ input và thiết lập theo giờ VN
                const dateParts = obj.DTM_NgayMuonNhan.split('-');
                if (dateParts.length === 3) {
                    const year = parseInt(dateParts[0]);
                    const month = parseInt(dateParts[1]) - 1;
                    const day = parseInt(dateParts[2]);
                    const dateVN = new Date(Date.UTC(year, month, day, 7, 0, 0)); // 7:00 AM giờ VN
                    obj.DTM_NgayMuonNhan = dateVN.toISOString();
                }
            } catch (e) {
                console.error('Error parsing DTM_NgayMuonNhan:', e);
                obj.DTM_NgayMuonNhan = null;
            }
        }

        if (obj.DTM_KyHan === '' || obj.DTM_KyHan == null) {
            obj.DTM_KyHan = null;
        } else {
            try {
                const dateParts = obj.DTM_KyHan.split('-');
                if (dateParts.length === 3) {
                    const year = parseInt(dateParts[0]);
                    const month = parseInt(dateParts[1]) - 1;
                    const day = parseInt(dateParts[2]);
                    const dateVN = new Date(Date.UTC(year, month, day, 7, 0, 0));
                    obj.DTM_KyHan = dateVN.toISOString();
                }
            } catch (e) {
                console.error('Error parsing DTM_KyHan:', e);
                obj.DTM_KyHan = null;
            }
        }

        return obj;
    }
    // check ly do tu choi
    function CheckLyDoTuChoi(tr) {
        const getVal = (selector) => {
            const el = tr.querySelector(selector);
            return el ? (el.value || '').toString() : '';
        };

        const getEl = (selector) => tr.querySelector(selector);

        // bắt buộc nhập lý do khi từ chối lấy báo giá
        const layBaoGiaVal = getVal('.laybaogiaTb');
        const lyDoEl = getEl('.lydoTb') || getEl('input[placeholder*="Lý do"]') || getEl('input[id^="lyDo_"]');

        if (layBaoGiaVal === 'false') {
            const lyDoVal = lyDoEl ? (lyDoEl.value || '').toString() : '';
            if (!lyDoVal) {
                if (lyDoEl) lyDoEl.classList.add('is-invalid');
                return false;
            } else {
                if (lyDoEl) lyDoEl.classList.remove('is-invalid');
            }
        } else {
            if (lyDoEl) lyDoEl.classList.remove('is-invalid');
        }
        return true;
    }
    async function submitForm() {

        let rowsValid = true;
        let rowsCheckReason = true;
        let payload = [];
        const approverVal = (qs('#approverSelect') || {}).value || '';
        if (!approverVal || approverVal.toString().trim() === '') {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: (T.SelectApprover || 'Vui lòng chọn người phê duyệt trước khi gửi'), type: 'error' });
            return;
        }

        const visibleRows = Array.from(qsa('#quoteTableBody tr'));
        visibleRows.forEach((tr) => {
            if (isRowEmpty(tr)) return;
            if (!validateRow(tr)) rowsValid = false;
            if (!CheckLyDoTuChoi(tr)) rowsCheckReason = false;
        });

        if (Array.isArray(allQuoteItems) && allQuoteItems.length > 0) {

            const start = (currentPage - 1) * rowsPerPage;
            visibleRows.forEach((tr, idx) => {
                try {
                    const globalIdx = start + idx;
                    const collected = collectRow(tr);
                    if (Array.isArray(filteredQuoteItems) && filteredQuoteItems.length > globalIdx && filteredQuoteItems[globalIdx]) {
                        Object.assign(filteredQuoteItems[globalIdx], collected);
                    }
                    if (Array.isArray(allQuoteItems) && allQuoteItems.length > globalIdx && allQuoteItems[globalIdx]) {
                        Object.assign(allQuoteItems[globalIdx], collected);
                    } else {
                        allQuoteItems.push(collected);
                    }
                } catch (e) { console.warn('Error merging visible row into full dataset', e); }
            });

            payload = allQuoteItems.slice();
        } else {
            visibleRows.forEach((tr) => {
                if (isRowEmpty(tr)) return;
                payload.push(collectRow(tr));
            });
        }

        try {
            let sectionForPayload = '';
            for (const it of payload) {
                const s = it && (it.CHR_SectionCode || it.chR_SectionCode || it.CHR_SectionName || it.chR_SectionName || it.sectionCode || it.sectionName) || '';
                if (s && s.toString().trim() !== '') {
                    sectionForPayload = s.toString().trim();
                    break;
                }
            }
            if (!sectionForPayload) {
                const firstSel = qs('#quoteTableBody tr .tenPhongBanTb');
                if (firstSel) sectionForPayload = (firstSel.value || '').toString().trim();
            }

            const maDon = generateMaDonRequest(sectionForPayload);
            payload.forEach(item => {
                try {
                    if (!item.CHR_UserApproval || item.CHR_UserApproval.toString().trim() === '') {
                        item.CHR_UserApproval = approverVal;
                    }
                    if (!item.CHR_MaDon || item.CHR_MaDon.toString().trim() === '') {
                        item.CHR_MaDon = maDon;
                    }
                    if (!item.CHR_SectionName || item.CHR_SectionName.toString().trim() === '' || item.CHR_SectionName ==='#N/A') {
                        item.CHR_SectionName = SetionNameFirst;
                    }
                    item.ID_StepBaoGia = 2; // set step duyệt báo giá
                } catch (e) { }
            });
        } catch (e) { console.warn('Error ensuring CHR_MaDon/CHR_UserApproval on payload', e); }

        // If no rows to submit, inform user and abort
        if (payload.length === 0) {
            const T = window.i18nQuote || {};
            showDialog({ title: T.ErrorTitle || 'Lỗi', message: T.MsgInvalidData || 'Không có dữ liệu để gửi', type: 'error' });
            return;
        }
        if (!rowsCheckReason) {
            const T = window.i18nQuote || {};
            showDialog({
                title: T.ErrorTitle || 'Lỗi', message: T.MsgEnterReasonReject || 'Vui lòng nhập lý do từ chối lấy báo giá', type: 'error'
            });
            return;
        }
        if (!rowsValid) {
            const T = window.i18nQuote || {};
            showDialog({
                title: T.ErrorTitle || 'Lỗi', message: T.MsgFillRequired || 'Vui lòng điền đầy đủ các trường bắt buộc(*)', type: 'error'
            });
            return;
        }
        try {
            showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xử lý...');
            const res = await fetch(api.insertListBaoGia, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
            });
            if (!res.ok) throw new Error(await res.text());
            const data = await res.json();
            const T = window.i18nQuote || {};
            showDialog({ title: T.SuccessTitle || 'Thành công', message: T.MsgSubmitSuccess || 'Gửi yêu cầu báo giá thành công', type: 'success' });
            resetForm();
        } catch (err) {
            const T = window.i18nQuote || {};
            showDialog({
                title: T.ErrorTitle || 'Lỗi', message: err.message, type: 'error'
            });
        }
        finally {
            hideLoading();
        }
    }
    async function autoAddRowByCategory(selectEl) {
        const tr = selectEl.closest('tr');
        const code = selectEl.value;
        // Fetch suppliers for this material and if >1 create rows per supplier
        try {
            const supRes = await fetch(api.getNCCByCategory, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(code)
            });
            if (!supRes.ok) throw new Error(await supRes.text());
            const suppliers = await supRes.json();
            if (Array.isArray(suppliers) && suppliers.length > 0) {
                // Helper to extract supplier code
                const getSupCode = (s) => s?.chR_MaNCC || (typeof s === 'string' ? s : undefined) || '';
                // If only one supplier, set current row's supplier
                if (suppliers.length === 1) {
                    const s = suppliers[0];
                    const supCode = getSupCode(s);
                    const supSel = tr.querySelector('.nhaCungCapTb');
                    if (supSel) {
                        supSel.value = supCode;
                        try { updateSearchableSelectDisplay(supSel); } catch (e) { }
                    }
                    // Fill mã hàng NCC and NSX for the single supplier into the current row
                    const codeByNccInputSingle = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('mã hàng ncc'));
                    if (codeByNccInputSingle && s.nvchR_CodeByNCC) codeByNccInputSingle.value = s.nvchR_CodeByNCC;
                    const nsxInputSingle = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('nsx'));
                    if (nsxInputSingle && s.nvchR_MakeIn) nsxInputSingle.value = s.nvchR_MakeIn;
                } else if (suppliers.length > 1) {
                    // Collect current row values to replicate
                    const values = {};
                    // copy inputs
                    qsa('input', tr).forEach((inp) => values[inp.name || inp.id || inp.placeholder || inp.type] = inp.value);
                    // copy selects
                    qsa('select', tr).forEach((sel) => values[sel.className || sel.name || sel.id] = sel.value);

                    // For first supplier, set current row
                    const s0 = suppliers[0];
                    const firstCode = getSupCode(s0);
                    const supSel0 = tr.querySelector('.nhaCungCapTb');
                    if (supSel0) {
                        supSel0.value = firstCode;
                        // update visible searchable UI for this existing row
                        try { updateSearchableSelectDisplay(supSel0); } catch (e) { }
                    }
                    // Also fill mã hàng NCC and NSX for the first supplier into the current row
                    const codeByNccInputFirst = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('mã hàng ncc'));
                    if (codeByNccInputFirst && s0.nvchR_CodeByNCC) codeByNccInputFirst.value = s0.nvchR_CodeByNCC;
                    const nsxInputFirst = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('nsx'));
                    if (nsxInputFirst && s0.nvchR_MakeIn) nsxInputFirst.value = s0.nvchR_MakeIn;

                    // Insert additional rows for remaining suppliers
                    let insertAfter = tr;
                    for (let i = 1; i < suppliers.length; i++) {
                        const s = suppliers[i];
                        const supCode = getSupCode(s);
                        // clone the row
                        const newRow = tr.cloneNode(true);
                        // clean any ms-container wrappers inside clone
                        qsa('.ms-container', newRow).forEach(w => w.remove());
                        // restore selects display
                        qsa('select.searchable-select', newRow).forEach(sv => sv.style.display = '');

                        // set values on inputs/selects in newRow
                        qsa('input', newRow).forEach((inp) => {
                            const key = inp.name || inp.id || inp.placeholder || inp.type;
                            if (values.hasOwnProperty(key)) inp.value = values[key];
                            inp.classList.remove('is-invalid');
                        });
                        qsa('select', newRow).forEach((sel) => {
                            const key = sel.className || sel.name || sel.id;
                            if (values.hasOwnProperty(key)) sel.value = values[key];
                            sel.classList.remove('is-invalid');
                        });
                        // Set supplier value for this clone and update its searchable display
                        const supSel = newRow.querySelector('.nhaCungCapTb');
                        if (supSel) {
                            supSel.value = supCode || '';
                            try { updateSearchableSelectDisplay(supSel); } catch (e) { }
                        }

                        // Fill ten hang ncc in the cloned row
                        const codeByNccInput = qsa('input', newRow).find((i) => (i.placeholder || '').toLowerCase().includes('mã hàng ncc'));
                        if (codeByNccInput && s.nvchR_CodeByNCC) codeByNccInput.value = s.nvchR_CodeByNCC;
                        // Fill san xuat in the cloned row
                        const nsxInput = qsa('input', newRow).find((i) => (i.placeholder || '').toLowerCase().includes('nsx'));
                        if (nsxInput && s.nvchR_SanXuat) nsxInput.value = s.nvchR_SanXuat;


                        // insert after last inserted
                        insertAfter.parentNode.insertBefore(newRow, insertAfter.nextSibling);
                        insertAfter = newRow;
                    }
                    try { buildSearchableDropdown($(document)); } catch (ex) { }
                    renumberRows();
                }
            }
        } catch (err) {
            console.warn('Không thể lấy NCC cho mã hàng:', err);
        }
    }
    async function autofillFromMaterialSelect(selectEl) {
        const tr = selectEl.closest('tr');
        const code = selectEl.value;
        if (!code) return;
        try {
            const supplierSel = tr ? tr.querySelector('.nhaCungCapTb') : null;
            const supVal = supplierSel ? (supplierSel.value || '').toString().trim() : '';
            if (supVal) return;
        } catch (e) { /* ignore */ }
        try {
            const res = await fetch(api.getMaterials(code));
            if (!res.ok) throw new Error(await res.text());
            const materials = await res.json();
            const material = Array.isArray(materials) ? materials.find((m) => m.material_Code === code) : null;
            if (!material) return;
            // Fill EN name
            const enInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('tên hàng en'));
            if (enInput && material.material_Name_EN) enInput.value = material.material_Name_EN;
            // Fill unit
            const unitInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('đơn vị'));
            if (unitInput && (material.unit || material.material_Unit)) unitInput.value = material.unit || material.material_Unit;
            // Fill tên mở thủ tục hải quan
            const vnInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('thủ tục hải quan'));
            //const tenMoThuTucValue = material.tenMoThuTuc || material.TenMoThuTuc || (typeof material.GetTenMoThuTuc === 'function' ? material.GetTenMoThuTuc() : null);
            //if (vnInput && tenMoThuTucValue) vnInput.value = tenMoThuTucValue;
            const tenMoThuTucValue = material.nameVI || "" ;
            if (vnInput && tenMoThuTucValue) vnInput.value = tenMoThuTucValue;
            // Fill hinh dang
            const shapeInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('hình dáng'));
            if (shapeInput && material.shape) shapeInput.value = material.shape;
            // Fill chất liệu
            const materialInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('chất liệu'));
            if (materialInput && material.material) materialInput.value = material.material;
            // Fill thành phần
            const componentInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('thành phần'));
            if (componentInput && material.composition) componentInput.value = material.composition;
            // Fill kích thước
            const sizeInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('kích thước'));
            if (sizeInput && material.dimension) sizeInput.value = material.dimension;
            // Fill dùng cho máy 
            const usageInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('dùng cho máy/thiết bị/vị trí'));
            if (usageInput && material.usedFor) usageInput.value = material.usedFor;
            // Fill để làm gì
            const functionInput = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('dùng để làm gì'));
            if (functionInput && material.purpose) functionInput.value = material.purpose;
            // Chủng loại hàng
            const categorySelect = tr.querySelector('.chungLoaiTb');
            if (categorySelect && material.category_VN) {
                try {
                    // nếu select đang rỗng, thử set bằng value hoặc bằng text (setSelectValueByText sẽ tìm theo value trước)
                    setSelectValueByText(categorySelect, material.category_VN);
                    // nếu select được enhance thành searchable, cập nhật hiển thị
                    updateSearchableSelectDisplay(categorySelect);
                    autoAddRowByCategory(categorySelect);
                } catch (e) {
                    console.warn('Error setting category select:', e);
                }
            }
            // Optionally set PHAN LOẠI 
            const categoryInput = tr.querySelector('.tenPhanLoaiTb');
            const loaiHangValue = material.loaiHang || material.LoaiHang || (typeof material.GetLoaiHang === 'function' ? material.GetLoaiHang() : null);
            if (categoryInput && loaiHangValue) categoryInput.value = loaiHangValue;

        } catch (err) {
            console.warn('Không thể tự động điền thông tin vật tư:', err);
            showDialog({
                title: 'Lỗi', message: err.message, type: 'error'
            });
        }
    }
    async function exportAutoRender() {
        // Hook to static Auto Render modal in Index.cshtml
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
        const backdrop = overlay ? overlay.querySelector('[data-ar-action="overlay"]') : null;
        const tabHasCodeBtn = document.getElementById('arTabHasCode');
        const tabNoCodeBtn = document.getElementById('arTabNoCode');
        const tabHasCodeBody = document.getElementById('arTabHasCodeBody');
        const tabNoCodeBody = document.getElementById('arTabNoCodeBody');
        if (!overlay || !sectionSel || !lst || !searchBox || !selectAll || !btnExport || !btnCancel || !btnClose || !errEl || !tabHasCodeBtn || !tabNoCodeBtn || !tabHasCodeBody || !tabNoCodeBody || !sectionSel2 || !sectionNameEl2 || !lst2 || !searchBox2 || !selectAll2) {
            const T = window.i18nQuote || {};
            alert(T.MsgCannotOpenAutoRender || 'Không thể mở hộp thoại Auto render');
            return;
        }

        // Helpers
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
            btnExport.disabled = !!busy;
            btnCancel.disabled = !!busy;
            btnClose.disabled = !!busy;
            const T = window.i18nQuote || {};
            btnExport.textContent = busy ? (T.Exporting || 'Đang xuất...') : (T.ExportExcel || 'Xuất Excel');
        };

        // Tabs state
        let currentTab = 'hasCode'; // 'hasCode' | 'noCode'
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
        tabHasCodeBtn.onclick = () => switchTab('hasCode');
        tabNoCodeBtn.onclick = () => switchTab('noCode');
        switchTab('hasCode');

        // Populate Section options
        sectionSel.innerHTML = '';
        const ph = document.createElement('option');
        ph.value = '';
        // ph.textContent = (window.i18nQuote && window.i18nQuote.SelectSection) || 'Chọn phòng ban';
        sectionSel.appendChild(ph);
        // For second tab
        sectionSel2.innerHTML = '';
        const ph2 = document.createElement('option');
        ph2.value = '';
        sectionSel2.appendChild(ph2);
        const srcDeptSel = qs('.tenPhongBanTb');
        if (srcDeptSel) {
            Array.from(srcDeptSel.options).forEach((o) => {
                if (!o || !o.text) return;
                const opt = document.createElement('option');
                opt.value = o.value || '';
                opt.textContent = o.text || '';
                sectionSel.appendChild(opt);
                const opt2 = document.createElement('option');
                opt2.value = o.value || '';
                opt2.textContent = o.text || '';
                sectionSel2.appendChild(opt2);
            });
        }
        const updateSectionName = () => {
            const txt = sectionSel.options[sectionSel.selectedIndex]?.text || '';
            const parts = txt.split(' - ');
            sectionNameEl.textContent = parts.length > 1 ? parts.slice(1).join(' - ') : '';
        };
        const updateSectionName2 = () => {
            const txt = sectionSel2.options[sectionSel2.selectedIndex]?.text || '';
            const parts = txt.split(' - ');
            sectionNameEl2.textContent = parts.length > 1 ? parts.slice(1).join(' - ') : '';
        };
        sectionSel.onchange = updateSectionName;
        updateSectionName();
        sectionSel2.onchange = updateSectionName2;
        updateSectionName2();

        // Populate Material list with lazy loading (infinite scroll) & server search
        lst.innerHTML = '';
        const materialState = {
            pageIndex: 1,
            pageSize: 200,
            loading: false,
            lastQuery: '',
            hasMore: true,
            items: []
        };

        const createItemEl = (it) => {
            const wrap = document.createElement('label');
            wrap.style.display = 'flex';
            wrap.style.minWidth = '350px';
            wrap.style.alignItems = 'center';
            wrap.style.gap = '8px';
            wrap.style.padding = '4px 2px';
            wrap.style.cursor = 'pointer';
            wrap.dataset.search = (it.code + ' ' + it.text).toLowerCase();
            const cb = document.createElement('input');
            cb.type = 'checkbox';
            cb.value = it.code;
            const span = document.createElement('span');
            span.textContent = it.text;
            wrap.appendChild(cb);
            wrap.appendChild(span);
            return wrap;
        };

        async function loadMaterialPage(query = '') {
            if (materialState.loading) return;
            // new query -> reset
            if ((query || '') !== (materialState.lastQuery || '')) {
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
                const pageItems = Array.isArray(data) ? data.map(o => ({ code: o.material_Code || '', text: (o.material_Code || '') + ' - ' + (o.material_Name_VN || '') })) : [];
                // append
                pageItems.forEach(it => {
                    if (!it.code) return;
                    materialState.items.push(it);
                    lst.appendChild(createItemEl(it));
                });
                // determine hasMore
                if (pageItems.length < materialState.pageSize) materialState.hasMore = false;
                else materialState.pageIndex++;
                materialState.lastQuery = query || '';
            } catch (err) {
                console.warn('Không thể tải danh sách vật tư (paged):', err);
            } finally {
                materialState.loading = false;
            }
        }

        // initial load first page
        await loadMaterialPage('');

        // Search filter with debounce
        let searchTimer = null;
        searchBox.oninput = () => {
            const q = (searchBox.value || '').toString();
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => loadMaterialPage(q), 300);
        };

        // infinite scroll: when near bottom load next page
        lst.addEventListener('scroll', () => {
            try {
                if (lst.scrollTop + lst.clientHeight >= lst.scrollHeight - 40) {
                    if (!materialState.loading && materialState.hasMore) {
                        loadMaterialPage(materialState.lastQuery);
                    }
                }
            } catch (e) { }
        });

        // Select All toggle on visible items
        selectAll.onchange = () => {
            const visibleItems = Array.from(lst.querySelectorAll('label')).filter(el => el.style.display !== 'none');
            visibleItems.forEach(el => {
                const cb = el.querySelector('input[type="checkbox"]');
                if (cb) cb.checked = selectAll.checked;
            });
        };

        // Populate Category list (from existing category select options)
        lst2.innerHTML = '';
        const srcCategorySel = qs('.chungLoaiTb');
        const catItems = [];
        if (srcCategorySel) {
            Array.from(srcCategorySel.options).forEach((o) => {
                const val = o.value || '';
                const text = o.text || '';
                if (!val) return;
                catItems.push({ code: val, text: text });
            });
        }
        const createCatEl = (it) => {
            const wrap = document.createElement('label');
            wrap.style.display = 'flex';
            wrap.style.minWidth = '350px';
            wrap.style.alignItems = 'center';
            wrap.style.gap = '8px';
            wrap.style.padding = '4px 2px';
            wrap.style.cursor = 'pointer';
            wrap.dataset.search = (it.code + ' ' + it.text).toLowerCase();
            const cb = document.createElement('input');
            cb.type = 'checkbox';
            cb.value = it.code;
            const span = document.createElement('span');
            span.textContent = it.text;
            wrap.appendChild(cb);
            wrap.appendChild(span);
            return wrap;
        };
        catItems.forEach(it => lst2.appendChild(createCatEl(it)));

        // Search filter for categories
        searchBox2.oninput = () => {
            const q = (searchBox2.value || '').toLowerCase();
            Array.from(lst2.children).forEach((el) => {
                const s = el.dataset.search || '';
                el.style.display = !q || s.includes(q) ? '' : 'none';
            });
        };

        // Select All toggle for categories
        selectAll2.onchange = () => {
            const visibleItems = Array.from(lst2.querySelectorAll('label')).filter(el => el.style.display !== 'none');
            visibleItems.forEach(el => {
                const cb = el.querySelector('input[type="checkbox"]');
                if (cb) cb.checked = selectAll2.checked;
            });
        };

        // Button handlers (override to avoid duplicate bindings)
        btnCancel.onclick = hideAr;
        btnClose.onclick = hideAr;
        if (backdrop) backdrop.onclick = hideAr;

        btnExport.onclick = async () => {
            setError('');
            try {
                setBusy(true);
                showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xuất...');
                let sectionCode = '';
                let sectionText = '';
                let sectionName = '';
                let selectedIds = [];
                let endpoint = '';

                if (currentTab === 'hasCode') {
                    sectionCode = sectionSel.value || '';
                    sectionText = sectionSel.options[sectionSel.selectedIndex]?.text || '';
                    sectionName = sectionText.split(' - ').slice(1).join(' - ');
                    selectedIds = Array.from(lst.querySelectorAll('input[type="checkbox"]:checked')).map(cb => cb.value);
                    endpoint = api.exportAutoRender;
                    if (!sectionCode) {
                        const T = window.i18nQuote || {};
                        setError(T.MsgSelectSectionRequired || 'Vui lòng chọn mã phòng ban');
                        return;
                    }
                    if (selectedIds.length === 0) {
                        const T = window.i18nQuote || {};
                        setError(T.MsgSelectAtLeastOneMaterial || 'Vui lòng chọn ít nhất một mã hàng nội bộ');
                        return;
                    }
                } else {
                    sectionCode = sectionSel2.value || '';
                    sectionText = sectionSel2.options[sectionSel2.selectedIndex]?.text || '';
                    sectionName = sectionText.split(' - ').slice(1).join(' - ');
                    selectedIds = Array.from(lst2.querySelectorAll('input[type="checkbox"]:checked')).map(cb => cb.value);
                    endpoint = api.exportRenderOutSide;
                    if (!sectionCode) {
                        const T = window.i18nQuote || {};
                        setError(T.MsgSelectSectionRequired || 'Vui lòng chọn mã phòng ban');
                        return;
                    }
                    if (selectedIds.length === 0) {
                        const T = window.i18nQuote || {};
                        setError(T.MsgSelectAtLeastOneCategory || 'Vui lòng chọn ít nhất một chủng loại hàng');
                        return;
                    }
                }

                const payload = { sectionCode, sectionName, selectedItemIds: selectedIds };
                const res = await fetch(endpoint, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (!res.ok) {
                    const msg = await res.text().catch(() => 'Lỗi không xác định');
                    throw new Error(msg || 'Xuất file thất bại');
                }
                const blob = await res.blob();
                let fileName = 'AutoRenderQuote.xlsx';
                const cd = res.headers.get('content-disposition');
                if (cd) {
                    const m = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(cd);
                    if (m && m[1]) fileName = m[1].replace(/["']/g, '').trim();
                }
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = fileName;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
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

        // Show modal
        showAr();
    }
    function wireEvents() {
        const container = qs('#quote-request');
        if (!container) return;
        // Track previous value for department selects to allow reverting on invalid change
        qs('#quoteTableBody')?.addEventListener('focusin', (e) => {
            try {
                const t = e.target;
                if (t && t.classList && t.classList.contains('tenPhongBanTb')) {
                    t.dataset.prev = t.value || '';
                }
            } catch (ex) { }
        }, true);

        // When department changes ensure all non-empty rows use the same department code
        qs('#quoteTableBody')?.addEventListener('change', async (e) => {
            try {
                const t = e.target;
                if (!t || !t.classList || !t.classList.contains('tenPhongBanTb')) return;
                const sel = t;
                const newVal = (sel.value || '').toString();
                const prevVal = sel.dataset.prev || '';

                // Collect distinct non-empty section codes after this change
                const rows = qsa('#quoteTableBody tr');
                const set = new Set();
                for (const r of rows) {
                    const s = (r.querySelector('.tenPhongBanTb') || {}).value || '';
                    if (s) set.add(s.toString());
                }

                if (set.size > 1) {
                    // revert change and show warning
                    sel.value = prevVal;
                    try { updateSearchableSelectDisplay(sel); } catch (e) { }
                    const T = window.i18nQuote || {};
                    showDialog({ title: T.ErrorTitle || 'Lỗi', message: 'Không được chọn 2 mã phòng khác nhau trong cùng 1 đơn', type: 'error' });
                    return;
                }

                // If a single non-empty section exists, load approvers for it
                const single = set.size === 1 ? Array.from(set)[0] : '';
                if (single) {
                    await loadApprovers(single);
                } else {
                    // clear approver list if no section selected
                    const approverSel = qs('#approverSelect');
                    if (approverSel) {
                        approverSel.innerHTML = '<option value="">' + ((window.i18nQuote && window.i18nQuote.SelectApprover) || '-- Select --') + '</option>';
                    }
                }
            } catch (ex) {
                console.warn('Error handling department change', ex);
            }
        });
        qs('#btnAddRow')?.addEventListener('click', addRow);
        qs('#btnReset')?.addEventListener('click', resetForm);
        qs('#btnCreate')?.addEventListener('click', submitForm);
        qs('#btnAuto')?.addEventListener('click', exportAutoRender);
        qs('#btnDownExcelTable')?.addEventListener('click', exportTable);
        qs('#btnClearFilters')?.addEventListener('click', () => {
            qsa('.filter-input').forEach(inp => inp.value = '');
            currentPage = 1;
            applyFiltersAndPagination();
        });
        // Rows per page selector (if present in DOM)
        const rowsPerPageSelect = qs('#rowsPerPageSelect');
        if (rowsPerPageSelect) {
            // initialize select value
            rowsPerPage = parseInt(rowsPerPageSelect.value) || rowsPerPage;
            rowsPerPageSelect.addEventListener('change', (e) => {
                const v = parseInt(e.target.value);
                if (Number.isFinite(v) && v > 0) {
                    rowsPerPage = v;
                    currentPage = 1;
                    applyFiltersAndPagination();
                }
            });
        }

        qsa('.btn-remove-row', container).forEach((btn) => {
            btn.addEventListener('click', (e) => removeRow(e.currentTarget));
        });

        async function exportTable() {
            try {
                showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xuất...');
                const rows = qsa('#quoteTableBody tr');

                const res = await fetch(api.exportTable, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(allQuoteItems)
                });
                if (!res.ok) {
                    const msg = await res.text().catch(() => 'Lỗi không xác định');
                    throw new Error(msg || 'Xuất file thất bại');
                }
                const blob = await res.blob();
                let fileName = 'TableQuote.xlsx';
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
                showDialog({ title: (window.i18nQuote && window.i18nQuote.ErrorTitle) || 'Lỗi', message: err.message || 'Không thể xuất file', type: 'error' });
            } finally {
                hideLoading();
            }
        }
        // Delegate for future rows
        qs('#quoteTableBody')?.addEventListener('click', (e) => {
            const t = e.target;
            if (t.closest('.btn-remove-row')) {
                removeRow(t.closest('.btn-remove-row'));
            }
        });

        // Upload Excel
        qs('#btnUploadExcel')?.addEventListener('click', () => qs('#excelUpload')?.click());
        qs('#excelUpload')?.addEventListener('change', async (e) => {
            const file = e.target.files?.[0];
            if (!file) return;
            try {
                showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xử lý...');
                const fd = new FormData();
                fd.append('file', file);
                const res = await fetch(api.uploadQuoteExcel, { method: 'POST', body: fd });
                if (!res.ok) throw new Error(await res.text());
                const items = await res.json();
                if (!Array.isArray(items)) {
                    throw new Error((window.i18nQuote && window.i18nQuote.MsgInvalidData) || 'Dữ liệu không hợp lệ');
                    return;
                }
                populateTableFromItems(items);
               // const T = window.i18nQuote || {};

            } catch (err) {
                const T = window.i18nQuote || {};
                showDialog({ title: T.ErrorTitle || 'Lỗi', message: err.message || T.MsgCannotReadFile || 'Không thể đọc file', type: 'error' });
            } finally {
                hideLoading();
                e.target.value = '';
            }
        });
        // Download Excel stub
        qs('#btnDownloadExcel')?.addEventListener('click', () => {
            try {
                showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xử lý...');
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
        // dowload mater data
        qs('#btnDownMaster')?.addEventListener('click', async () => {

            const T = window.i18nQuote || {};
            try {
                const endpoints = [
                    { url: api.downloadMasterVendor, defaultName: 'ExportMasterVendor.xlsx' },
                    { url: api.downloadMasterMaterial, defaultName: 'ExportMasterMaterial.xlsx' }
                ];

                for (const ep of endpoints) {
                    const res = await fetch(ep.url, { method: 'GET' });
                    if (!res.ok) {
                        let txt = await res.text().catch(() => res.statusText);
                        showLoading({ title: (T.ErrorTitle || 'Lỗi'), message: (T.ExportFailed || 'Xuất file thất bại') + ': ' + (txt || res.statusText), type: 'error' });
                        continue;
                    }
                    const blob = await res.blob();
                    // try to parse filename from content-disposition
                    let filename = ep.defaultName;
                    try {
                        const cd = res.headers.get('content-disposition');
                        if (cd) {
                            const m = cd.match(/filename\*?=(?:UTF-8''|\")?([^;\"']+)/i);
                            if (m && m[1]) filename = decodeURIComponent(m[1].replace(/\"/g, '').trim());
                        }
                    } catch { }

                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = filename;
                    document.body.appendChild(a);
                    a.click();
                    a.remove();
                    window.URL.revokeObjectURL(url);
                }

                showLoading({ title: (T.SuccessTitle || 'Thành công'), message: (T.ExportSuccess || 'Xuất file hoàn tất'), type: 'success' });
            } catch (err) {
                const T = window.i18nSupplierMana || {};
                showLoading({ title: (T.ErrorTitle || 'Lỗi'), message: (T.ExportFailed || 'Xuất file thất bại') + ': ' + (err && err.message ? err.message : err), type: 'error' });
            } finally {
                hideLoading();
            }
        });
        // Consolidated change handler (delegated) for table selects
        qs('#quoteTableBody')?.addEventListener('change', async (e) => {
            const t = e.target;
            if (!t || !t.classList) return;

            // Selecting an internal material code -> autofill fields from material service
            if (t.classList.contains('maHangNoiBo')) {
                try {
                    await autofillFromMaterialSelect(t);
                } catch (ex) { console.warn('autofillFromMaterialSelect error', ex); }
                return;
            }

        if (t.classList.contains('nhaCungCapTb')) {
            try {
                const tr = t.closest('tr');
                if (!tr) return;
                const maNcc = (t.value || '').toString();
                const category = (tr.querySelector('.chungLoaiTb') || {}).value || '';
                if (!maNcc || !category) return;
                try {
                    const isValid = await checkNccCategory(maNcc, category);
                    if (!isValid) {
                        t.value = '';
                        try { updateSearchableSelectDisplay(t); } catch (e) { }
                        const T = window.i18nQuote || {};
                        showDialog({ title: T.ErrorTitle || 'Lỗi', message: 'Nhà cung cấp này không cung cấp chủng loại hàng được chọn. Vui lòng chọn nhà cung cấp khác.', type: 'error' });
                    }
                } catch (err) {
                    console.warn('Lỗi kiểm tra NCC:', err);
                }
            } catch (e) { /* ignore */ }
            return;
        }

            // Category changed -> 1) try to auto-add rows for suppliers, 2) refresh material options for the same row
            if (t.classList.contains('chungLoaiTb')) {
                const tr = t.closest('tr');
                // 1) try auto add rows (may insert rows or set supplier on current row)
                try { await autoAddRowByCategory(t); } catch (ex) { console.warn('autoAddRowByCategory error', ex); }

                // 2) refresh maHangNoiBo options for this row only (avoid updating all rows)
                try {
                    const T18 = window.i18nQuote || {};
                    const nhomHang = (t.value || '').toString();
                    const body = { MaHang: '', Name: '', NhomHang: nhomHang, PageIndex: 0, PageSize: 0 };
                    const res = await fetch(api.searchMaterials, {
                        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)
                    });
                    if (!res.ok) throw new Error(await res.text());
                    const materials = await res.json();
                    if (!tr) return;
                    const sel = tr.querySelector('.maHangNoiBo');
                    if (!sel) return;

                    // remove any custom wrapper and rebuild only this select
                    try {
                        const next = sel.nextElementSibling;
                        if (next && next.classList && next.classList.contains('ms-container')) next.remove();
                        const prevValue = sel.value;
                        sel.style.display = '';
                        sel.innerHTML = '';
                        const optDefault = document.createElement('option');
                        optDefault.value = '';
                        optDefault.textContent = T18.SelectInternalMaterialCode || '';
                        sel.appendChild(optDefault);
                        if (Array.isArray(materials)) {
                            materials.forEach((m) => {
                                const code = m.material_Code || '';
                                const name = m.material_Name_VN || '';
                                if (!code) return;
                                const o = document.createElement('option');
                                o.value = code;
                                o.textContent = `${code} - ${name}`;
                                sel.appendChild(o);
                            });
                        }
                        // restore previous selection if still present
                        try {
                            if (prevValue && Array.from(sel.options).some(o => o.value === prevValue)) sel.value = prevValue;
                            else sel.selectedIndex = 0;
                        } catch (ex) { sel.selectedIndex = 0; }
                        try { $(sel).data('search-dropdown', false); } catch { }
                    } catch (err) {
                        console.warn('Error updating material select:', err);
                    }

                    try { buildSearchableDropdown($(sel)); } catch (ex) { }
                } catch (err) {
                    console.warn('Không thể tải danh sách vật tư:', err);
                }

                return;
            }
        });
        function updateTenThuTucHaiQuan(tr) {
            const classMaterial = tr.querySelector('.tenPhanLoaiTb')?.value || '';
            const categorySel = tr.querySelector('.chungLoaiTb');
            const categoryVN = categorySel ? (categorySel.options[categorySel.selectedIndex]?.text || '') : '';
            const shape = getInputValue(tr, 'hinhDang');
            const material = getInputValue(tr, 'chatLieu');
            const composition = getInputValue(tr, 'thanhPhan');
            const dimension = getInputValue(tr, 'kichThuoc');
            const usedFor = getInputValue(tr, 'viTriSuDung');
            const purpose = getInputValue(tr, 'tinhNang');
            let tenHangVN = "";
            switch (classMaterial) {
                case "NO LIST": tenHangVN ="Có hình dáng dạng "+ shape + " & " + usedFor + " & " + purpose;
                    break;
                case "A":
                case "E":
                case "I":
                    break;
                default:
                    tenHangVN = categoryVN + " có hình dáng dạng " + shape + " chất liệu " + material + " thành phần hóa chất " + composition + " có kích thước " + dimension + " dung để " + usedFor + " cho " + purpose;
                    break;
            };

            const vnInput = tr.querySelector('input[id^="tenHangVN_"]');
            if (vnInput) vnInput.value = tenHangVN;
        }

        function getInputValue(tr, className) {
            const el = tr.querySelector('.' + className);
            return el ? (el.value || '') : '';
        }
        // Update ten thu tuc hai quan when related fields change
        qs('#quoteTableBody')?.addEventListener('input', (e) => {
            const t = e.target;
            if (t.classList.contains('chungLoaiTb') || t.classList.contains('hinhDang') || t.classList.contains('chatLieu') || t.classList.contains('thanhPhan') || t.classList.contains('kichThuoc') || t.classList.contains('viTriSuDung') || t.classList.contains('tinhNang')) {
                updateTenThuTucHaiQuan(t.closest('tr'));
            }
        });
        qs('#quoteTableBody')?.addEventListener('change', (e) => {
            const t = e.target;
            if (t.classList.contains('chungLoaiTb')) {
                updateTenThuTucHaiQuan(t.closest('tr'));
            }
        });

        // Filter and pagination events
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('filter-input')) {
                currentPage = 1;
                applyFiltersAndPagination();
            }
        });
        qs('#prevPage')?.addEventListener('click', (e) => {
            e.preventDefault();
            if (currentPage > 1) {
                currentPage--;
                const tbody = qs('#quoteTableBody');
                // if we have in-memory items use renderQuotePage, otherwise fallback to DOM pagination
                if (Array.isArray(filteredQuoteItems) && filteredQuoteItems.length > 0) {
                    renderQuotePage(tbody, filteredQuoteItems);
                } else {
                    applyFiltersAndPagination();
                }
            }
        });
        qs('#nextPage')?.addEventListener('click', (e) => {
            e.preventDefault();
            const totalCount = (Array.isArray(filteredQuoteItems) && filteredQuoteItems.length > 0) ? filteredQuoteItems.length : filteredRows.length;
            const totalPages = Math.max(1, Math.ceil(totalCount / rowsPerPage));
            if (currentPage < totalPages) {
                currentPage++;
                const tbody = qs('#quoteTableBody');
                if (Array.isArray(filteredQuoteItems) && filteredQuoteItems.length > 0) {
                    renderQuotePage(tbody, filteredQuoteItems);
                } else {
                    applyFiltersAndPagination();
                }
            }
        });
    }
    // search Approver

    // Tìm kiếm 
    function buildSearchableDropdown($container) {
        let $targets;
        if (!$container) return;
        try {
            if (typeof $container.is === 'function' && $container.is('select')) {
                $targets = $container;
            } else {
                $targets = $container.find('select.searchable-select');
            }
        } catch (e) {
            // fallback
            $targets = $container.find('select.searchable-select');
        }

        $targets.each(function () {
            const $select = $(this);
            if ($select.data('search-dropdown') === true) return;

            // Cache DOM options as initial set
            const domOptions = $select.find('option').map(function () {
                return { value: this.value, text: $(this).text(), selected: this.selected };
            }).get();

            // Build UI
            const $wrapper = $('<div class="ms-container"></div>');
            const $btn = $('<div class="ms-btn"><span class="ms-values"></span><span class="ms-placeholder"></span><span class="ms-caret">▾</span></div>');
            const $dropdown = $('<div class="ms-dropdown"></div>');
            const $search = $('<div class="ms-search"><input type="text" placeholder="Tìm..." /></div>');
            const $list = $('<div class="ms-list" style="max-height:320px; overflow:auto"></div>');

            // Remote loading state (only used for material selects)
            const isRemoteMaterial = $select.hasClass('maHangNoiBo');
            const remoteState = {
                pageIndex: 1,
                pageSize: 200,
                loading: false,
                lastQuery: '',
                hasMore: true,
                options: domOptions.slice() // start with dom options if any
            };

            // Helper to get category context (NhomHang) from same row
            const getCategoryForRow = () => {
                try {
                    const tr = $select.closest('tr');
                    const cat = tr.find('select.chungLoaiTb').val() || '';
                    return cat;
                } catch (e) { return ''; }
            };

            async function loadRemote(query, append = false) {
                if (!isRemoteMaterial) return;
                // Debounce callers should prevent concurrent calls, but guard anyway
                // if this is a new query (not append) cancel any outstanding request
                if (!append && remoteState.controller) {
                    try { remoteState.controller.abort(); } catch (e) { }
                    remoteState.controller = null;
                }
                if (remoteState.loading && append) return; // prevent concurrent append loads
                // If new query, reset paging
                if (query !== remoteState.lastQuery) {
                    remoteState.pageIndex = 1;
                    remoteState.hasMore = true;
                }
                const page = remoteState.pageIndex;
                const pageSize = remoteState.pageSize;
                const body = { MaHang: query || '', Name: query || '', NhomHang: getCategoryForRow() || '', PageIndex: page, PageSize: pageSize };
                // show loading sentinel in list
                $list.find('.ms-loading').remove();
                $list.append('<div class="ms-loading">Loading...</div>');
                remoteState.loading = true;
                // create abort controller for this request
                try { remoteState.controller = new AbortController(); } catch (e) { remoteState.controller = null; }
                try {
                    // use configured api endpoint if available
                    const url = (typeof api !== 'undefined' && api.searchMaterials) ? api.searchMaterials : '/Quote/GetSearchMaterial';
                    const fetchOpts = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) };
                    if (remoteState.controller && remoteState.controller.signal) fetchOpts.signal = remoteState.controller.signal;
                    const res = await fetch(url, fetchOpts);
                    if (!res.ok) throw new Error(await res.text());
                    const data = await res.json();
                    const items = Array.isArray(data) ? data.map(m => ({ value: m.material_Code || '', text: ((m.material_Code || '') + ' - ' + (m.material_Name_VN || '')) })) : [];
                    if (!append) {
                        remoteState.options = items;
                    } else {
                        const existing = new Set(remoteState.options.map(o => o.value));
                        items.forEach(it => { if (it && it.value && !existing.has(it.value)) remoteState.options.push(it); });
                    }

                    // Ensure the underlying <select> contains these options so native lookups / other code work
                    try {
                        items.forEach(it => {
                            if (!it || !it.value) return;
                            if ($select.find('option[value="' + it.value.replace(/"/g, '\\"') + '"]').length === 0) {
                                const opt = document.createElement('option');
                                opt.value = it.value;
                                opt.text = it.text || it.value;
                                $select.append(opt);
                            }
                        });
                    } catch (e) { /* ignore DOM errors */ }

                    remoteState.hasMore = items.length === pageSize;
                    if (remoteState.hasMore) remoteState.pageIndex = page + 1;
                    remoteState.lastQuery = query;
                } catch (err) {
                    // ignore abort errors silently
                    if (err && err.name === 'AbortError') {
                        // aborted by newer request
                    } else {
                        console.warn('Error loading remote materials:', err);
                    }
                } finally {
                    remoteState.loading = false;
                    // clear controller for completed or aborted request
                    try { remoteState.controller = null; } catch (e) { }
                    $list.find('.ms-loading').remove();
                    renderList(remoteState.lastQuery);
                }
            }

            // Populate list (uses remoteState.options for remote selects, otherwise domOptions)
            function renderList(query) {
                const q = (query || '').toLowerCase();
                $list.empty();
                let hasItems = false;
                const source = isRemoteMaterial ? remoteState.options : domOptions;
                source.forEach(function (opt) {
                    if (!q || (opt.text || '').toLowerCase().includes(q)) {
                        const $item = $('<div class="ms-item"></div>').attr('data-value', opt.value).text(opt.text);
                        if ($select.val() === opt.value || opt.selected) {
                            $item.addClass('selected');
                        }
                        $list.append($item);
                        hasItems = true;
                    }
                });
                if (!hasItems) {
                    const T = window.i18nQuote || {};
                    $list.append('<div class="ms-empty">' + (T.NoResults || 'Không có kết quả') + '</div>');
                }
                // If remote and hasMore, show loading sentinel
                if (isRemoteMaterial && remoteState.hasMore) {
                    $list.append('<div class="ms-loading">Loading more...</div>');
                }
            }

            function updateButtonText() {
                const val = $select.val();
                const source = isRemoteMaterial ? remoteState.options : domOptions;
                const found = source.find(o => o.value === val);
                if (found && found.text) {
                    $btn.find('.ms-values').text(found.text);
                    $btn.find('.ms-placeholder').text('');
                } else {
                    const T = window.i18nQuote || {};
                    $btn.find('.ms-values').text('');
                    $btn.find('.ms-placeholder').text(T.SelectPlaceholder || '-- Chọn --');
                }
            }

            updateButtonText();
            renderList('');

            $dropdown.append($search).append($list);
            $select.after($wrapper);
            $wrapper.append($btn).append($dropdown);
            $select.hide();
            // store reference to wrapper for reattaching
            $dropdown.data('wrapper', $wrapper);

            // Events
            $btn.on('click', function (e) {
                e.stopPropagation();
                // close other dropdowns and reattach them
                $('.ms-dropdown').not($dropdown).each(function () {
                    const $other = $(this);
                    if ($other.hasClass('open')) {
                        $other.removeClass('open');
                        if ($other.data('detached')) {
                            const $wrapper = $other.data('wrapper');
                            if ($wrapper && $wrapper.length) $other.appendTo($wrapper).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                        }
                    }
                });

                if ($dropdown.hasClass('open')) {
                    // close
                    $dropdown.removeClass('open');
                    if ($dropdown.data('detached')) {
                        $dropdown.appendTo($dropdown.data('wrapper')).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                    }
                } else {
                    // open: detach and append to body so it's not clipped by table container
                    const btnRect = $btn[0].getBoundingClientRect();
                    const top = btnRect.top + window.scrollY + $btn.outerHeight();
                    const left = btnRect.left + window.scrollX;
                    $dropdown.appendTo('body').css({ position: 'absolute', top: top + 'px', left: left + 'px', width: $btn.outerWidth() + 'px', zIndex: 3000 }).addClass('open').data('detached', true);
                    $search.find('input').val('');
                    // if remote and no options loaded yet, load first page
                    if (isRemoteMaterial && (!remoteState.options || remoteState.options.length === 0)) {
                        loadRemote('');
                    } else {
                        renderList('');
                    }
                    $search.find('input').focus();
                }
            });

            // clicking outside should close and reattach any open dropdowns
            $(document).on('click.quoteDropdown', function () {
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

            $dropdown.on('click', function (e) { e.stopPropagation(); });

            $list.on('click', '.ms-item', function () {
                const value = $(this).attr('data-value');
                // ensure underlying select has this option (for native display and later code)
                try {
                    if ($select.find('option[value="' + value.replace(/"/g, '\\"') + '"]').length === 0) {
                        const txt = (remoteState && remoteState.options ? (remoteState.options.find(o => o.value === value) || {}).text : null) || $(this).text();
                        const opt = document.createElement('option');
                        opt.value = value;
                        opt.text = txt || value;
                        $select.append(opt);
                    }
                } catch (e) { }
                // set value via jQuery
                $select.val(value);
                // trigger both jQuery and native change so listeners attached via addEventListener are invoked
                try { $select.trigger('change'); } catch (e) { }
                try {
                    const sel = $select[0];
                    if (sel && typeof sel.dispatchEvent === 'function') {
                        sel.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                } catch (ex) { }
                updateButtonText();
                $dropdown.removeClass('open');
                if ($dropdown.data('detached')) {
                    $dropdown.appendTo($dropdown.data('wrapper')).css({ position: '', top: '', left: '', width: '', zIndex: '' }).data('detached', false);
                }
            });

            // scroll to load more for remote lists (throttled)
            if (isRemoteMaterial) {
                let listScrollTimer = null;
                $list.on('scroll', function () {
                    const el = this;
                    if (listScrollTimer) clearTimeout(listScrollTimer);
                    listScrollTimer = setTimeout(() => {
                        try {
                            if (el.scrollTop + el.clientHeight >= el.scrollHeight - 40) {
                                if (remoteState.hasMore && !remoteState.loading) {
                                    loadRemote(remoteState.lastQuery || '', true);
                                }
                            }
                        } catch (e) { }
                    }, 150);
                });
            }

            // debounce remote searches per-dropdown
            let searchTimerLocal = null;
            $search.find('input').on('input', function () {
                const q = ($(this).val() || '').toString();
                if (isRemoteMaterial) {
                    clearTimeout(searchTimerLocal);
                    searchTimerLocal = setTimeout(() => {
                        // new query should replace options
                        loadRemote(q, false);
                    }, 250);
                } else {
                    renderList(q.toString());
                }
            });

            // Mark enhanced
            $select.data('search-dropdown', true);
        });
    }

    // Initialize for existing selects
    buildSearchableDropdown($(document));

    // Load approvers for a given section code and populate the top approver select
    async function loadApprovers(sectionCode) {
        const sel = qs('#approverSelect');
        if (!sel) return;
        try {
            showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xử lý...');
            const body = { Step: 2, SectionCost: sectionCode || '' };
            const res = await fetch(api.searchApprover, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!res.ok) throw new Error(await res.text());
            const data = await res.json();
            // data expected to be an array of approver DTOs
            sel.innerHTML = '';
            const optDefault = document.createElement('option');
            optDefault.value = '';
            optDefault.textContent = (window.i18nQuote && window.i18nQuote.SelectApprover) || '-- Select Approver --';
            sel.appendChild(optDefault);
            if (Array.isArray(data) && data.length > 0) {
                data.forEach(a => {
                    try {
                        const o = document.createElement('option');
                        o.value = a?.chR_UserAdid || '';
                        o.textContent = (a?.nvchR_UserName ? a.nvchR_UserName + ' (' + (o.value || '') + ')' : (o.value || ''));
                        sel.appendChild(o);
                    } catch (ex) { }
                });
            }
        } catch (err) {
            console.warn('Không thể tải danh sách approver:', err);
        } finally {
            hideLoading();
        }
    }

    function setSelectValueByText(select, textOrValue) {
        if (!select) return;
        const val = textOrValue ?? '';
        // try match by value first
        let opt = Array.from(select.options).find(o => o.value === val);
        if (!opt) {
            // try match by text prefix before ' - '
            opt = Array.from(select.options).find(o => (o.text || '').toLowerCase() === (val || '').toLowerCase() || (o.text || '').toLowerCase().startsWith((val || '').toLowerCase()));
        }
        if (opt) {
            select.value = opt.value;
            updateSearchableSelectDisplay(select);
        }
    }

    function populateRowFromDto(tr, dto, rowIndex = 1) {
        // Lấy số hàng từ phần tử No hoặc từ tham số
        const noCell = tr.querySelector('td:first-child');
        const rowNumber = noCell ? noCell.textContent.trim() : rowIndex;
        
        // nút xóa cell
        const lastCell = tr.querySelector('td:last-child');
        if (lastCell && !lastCell.querySelector('.btn-remove-row')) {
            lastCell.innerHTML = '<button type="button" class="btn btn-sm btn-link text-danger px-0 btn-remove-row" title="Remove Row"><i class="fas fa-times"></i></button>';
        }

        // Helper function để set giá trị cho select theo ID
        const setSelectById = (idPattern, value) => {
            const element = tr.querySelector(`#${idPattern}_${rowNumber}`);
            if (element) {
                setSelectValueByText(element, value);
            }
        };

        // Helper function để set giá trị cho input theo ID
        const setInputById = (idPattern, value) => {
            const element = tr.querySelector(`#${idPattern}_${rowNumber}`);
            if (element) {
                element.value = value ?? '';
            }
        };

        // Điền dữ liệu theo ID pattern của từng field

        // Phòng ban
        setSelectById('tenPhongBanTb', dto.chR_SectionCode || dto.chR_SectionName);

        // Chủng loại
        setSelectById('chungLoai', dto.nvchR_ChungLoai);

        // Phân loại
        setSelectById('tenPhanLoaiTb', dto.chR_Phanloai);

        // Mã thiết bị
        setInputById('maThietBi', dto.chR_MaThietBi);

        // Mã hàng nội bộ (select)
        setSelectById('maHangNoiBo', dto.chR_MaHangNoiBo);

        // Mã hàng NCC
        setInputById('maHangNCC', dto.chR_MaHangNCC);

        // Tên hàng VN
        setInputById('tenHangVN', dto.nvchR_NameVN);

        // Tên hàng EN
        setInputById('tenHangEN', dto.chR_NameEN);

        // Số lượng
        setInputById('soLuong', dto.inT_SoLuong);

        // Đơn vị
        setInputById('donVi', dto.nvchR_DonVi);

        // Hình dáng
        setInputById('hinhDang', dto.nvchR_HinhDang);

        // Chất liệu
        setInputById('chatLieu', dto.nvchR_ChatLieu);

        // Thành phần
        setInputById('thanhPhan', dto.nvchR_ThanhPhan);

        // Kích thước
        setInputById('kichThuoc', dto.nvchR_KichThuoc);

        // Vị trí sử dụng
        setInputById('viTriSuDung', dto.nvchR_DongMay);

        // Tính năng
        setInputById('tinhNang', dto.nvchR_TinhNang);

        // ROHS
        setSelectById('rohsTb', dto.nvchR_Rohs);

        // CO/CQ
        setSelectById('CoCqTb', dto.nvchR_COCQ);

        // MSDS
        setInputById('msds', dto.nvchR_MSDS);

        // Tiêu chuẩn an toàn
        setInputById('tieuChuanAnToan', dto.nvchR_AnToan);

        // File thiết kế
        setInputById('fileThietKe', dto.nvchR_FileThietKe);
        // Link ảnh hoặc file thiết kế
        setInputById('linkFile', dto.chR_LinkFile || dto.CHR_LinkFile || dto.nvchR_LinkFile);

        // Nhà sản xuất
        setInputById('nsx', dto.nvchR_NhaSanXuat);

        // Nhà cung cấp (select)
        setSelectById('nhaCungCapTb', dto.chR_MaNCC || dto.nvchR_TenNCC);

        // Lấy báo giá
        const layBaoGiaValue = dto.biT_LayBaoGia === true ? 'true' : dto.biT_LayBaoGia === false ? 'false' : '';
        setSelectById('laybaogiaTb', layBaoGiaValue);

        // Lý do
        setInputById('lyDo', dto.nvchR_LyDo);

        // Helper function: normalize various date representations and return value for input[type="date"] (yyyy-MM-dd)
        const dateToInputValue = (d) => {
            if (!d) return '';
            try {
                // If it's already a Date
                let dt = (d instanceof Date) ? d : null;
                if (!dt) {
                    // Try native parsing (ISO, RFC)
                    dt = new Date(d);
                    if (isNaN(dt)) {
                        // Try dd/MM/yyyy or dd-MM-yyyy
                        const m = (d || '').toString().match(/^(\d{2})[\/\-](\d{2})[\/\-](\d{4})/);
                        if (m) {
                            const day = parseInt(m[1], 10);
                            const month = parseInt(m[2], 10) - 1;
                            const year = parseInt(m[3], 10);
                            dt = new Date(year, month, day);
                        }
                    }
                }
                if (!dt || isNaN(dt)) return '';
                const y = dt.getFullYear();
                const m = String(dt.getMonth() + 1).padStart(2, '0');
                const day = String(dt.getDate()).padStart(2, '0');
                return `${y}-${m}-${day}`;
            } catch (e) {
                return '';
            }
        };

        // Ngày muốn nhận
        setInputById('ngayMuonNhan', dateToInputValue( dto.dtM_NgayMuonNhan));

        // Kỳ hạn chọn NCC
        setInputById('kyHanChonNCC', dateToInputValue(dto.dtM_KyHan));

        // Gấp
        setSelectById('gapTb', dto.chR_Gap);

        // Người yêu cầu
        setInputById('nguoiYeuCauRow', dto.nvchR_UserRequest || (window.indexQuoteData && window.indexQuoteData.user) || '');
    }

    async function populateTableFromItems(items) {
        // store items in memory and render only the current page to avoid inserting all rows into DOM
        allQuoteItems = Array.isArray(items) ? items.slice() : [];
        filteredQuoteItems = allQuoteItems.slice();
        currentPage = 1;

        // Validate that all non-empty rows share the same section code
        try {
            const sections = new Set();
            allQuoteItems.forEach(it => {
                const s = (it && (it.CHR_SectionCode || it.chR_SectionCode || it.sectionCode)) || '';
                if (s && s.toString().trim() !== '') sections.add(s.toString().trim());
            });
            if (sections.size > 1) {
                const T = window.i18nQuote || {};
                showDialog({ title: T.ErrorTitle || 'Lỗi', message: 'Không được upload dữ liệu chứa nhiều mã phòng khác nhau trong cùng 1 đơn. Vui lòng kiểm tra file Excel.', type: 'error' });
                return;
            }

            // If a single section present, load approvers for it
            if (sections.size === 1) {
                const section = Array.from(sections)[0];
                await loadApprovers(section);
            }

            // Ensure any material codes from uploaded items exist as options in the maHangNoiBo selects
            const materialCodes = new Set();
            allQuoteItems.forEach(it => {
                const code = (it && (it.CHR_MaHangNoiBo || it.chR_MaHangNoiBo || it.maHangNoiBo)) || '';
                if (code && code.toString().trim() !== '') materialCodes.add(code.toString().trim());
            });
            if (materialCodes.size > 0) {
                const allSelects = qsa('.maHangNoiBo');
                allSelects.forEach(sel => {
                    materialCodes.forEach(code => {
                        try {
                            if (!Array.from(sel.options).some(o => (o.value || '') === code)) {
                                const o = document.createElement('option');
                                o.value = code;
                                o.text = code; // fallback text; searchable dropdown will show this text
                                sel.appendChild(o);
                            }
                        } catch (e) { /* ignore individual failures */ }
                    });
                    // mark for rebuild of searchable UI
                    try { $(sel).data('search-dropdown', false); } catch { }
                });
            }
            const T = window.i18nQuote || {};
            showDialog({ title: T.SuccessTitle || 'Thành công', message: (T.MsgLoadedRows || 'Đã tải {0} dòng từ Excel').replace('{0}', items.length), type: 'success' });
        } catch (ex) {
            console.warn('Error validating uploaded items:', ex);
        }

        const tbody = qs('#quoteTableBody');
        if (!tbody) return;
        renderQuotePage(tbody, filteredQuoteItems);
    }

    // Render only the visible page from the provided items list
    function renderQuotePage(tbody, sourceItems) {
        try {
            showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xử lý...');
            const existing = qs('#quoteTableBody tr');
            const baseRow = existing ? existing.cloneNode(true) : null;
            const template = document.createElement('tr');
            
            if (!baseRow) {
                console.warn('No existing row found to clone from, creating minimal template');
                const cellsNeeded = 33;
                for (let j = 0; j < cellsNeeded; j++) {
                    const td = document.createElement('td');
                    if (j === cellsNeeded - 1) {
                        td.className = 'text-center';
                        td.innerHTML = '<button type="button" class="btn btn-sm btn-link text-danger px-0 btn-remove-row" title="Remove Row"><i class="fas fa-times"></i></button>';
                    }
                    template.appendChild(td);
                }
            }

            const total = Array.isArray(sourceItems) ? sourceItems.length : 0;
            const totalPages = Math.max(1, Math.ceil(total / rowsPerPage));
            if (currentPage > totalPages) currentPage = totalPages;
            const start = (currentPage - 1) * rowsPerPage;
            const end = Math.min(start + rowsPerPage, total);

            // batch append
            tbody.innerHTML = '';
            const frag = document.createDocumentFragment();
            for (let i = start; i < end; i++) {
                const dto = sourceItems[i] || {};
                const row = baseRow ? baseRow.cloneNode(true) : template.cloneNode(true);
                // clean wrappers and reset
                qsa('.ms-container', row).forEach(w => w.remove());
                qsa('select.searchable-select', row).forEach(s => { s.style.display = ''; try { $(s).data('search-dropdown', false); } catch (e) { } });
                qsa('input', row).forEach(inp => { inp.value = ''; inp.classList.remove('is-invalid'); });
                qsa('select', row).forEach(sel => { sel.value = ''; sel.classList.remove('is-invalid'); });

                populateRowFromDto(row, dto, i + 1);
                frag.appendChild(row);
            }
            tbody.appendChild(frag);

            // initialize searchable only for visible rows
            try { buildSearchableDropdown($(tbody)); } catch (e) { }

            // update numbers and ids
            renumberRows();

            // update pagination UI
            const pagination = qs('#paginationControls');
            const prev = qs('#prevPage');
            const next = qs('#nextPage');
            if (totalPages > 1) {
                if (pagination) pagination.style.display = '';
                if (prev) prev.classList.toggle('disabled', currentPage === 1);
                if (next) next.classList.toggle('disabled', currentPage === totalPages);
            } else {
                if (pagination) pagination.style.display = 'none';
            }
            const T = window.i18nQuote || {};
            const startEntry = total === 0 ? 0 : (start + 1);
            const endEntry = end;
            if (qs('#pageInfo')) qs('#pageInfo').textContent = `${T.Showing || 'Showing'} ${startEntry} ~ ${endEntry} ${T.Of || 'Of'} ${total}`;
            if (qs('#pageNumberInfo')) qs('#pageNumberInfo').textContent = `${currentPage}/${totalPages}`;
            if (qs('#paginationInfo')) qs('#paginationInfo').style.display = totalPages > 1 ? '' : 'none';

            filteredRows = Array.from(tbody.querySelectorAll('tr'));
        } catch (e) {
            console.warn('renderQuotePage error', e);
        } finally {
            hideLoading();
        }
    }
    // show message dialog
    function getDialogEls() {
        const overlay = document.getElementById('cmDialogOverlay');
        const titleEl = document.getElementById('cmDialogTitle');
        const bodyEl = document.getElementById('cmDialogBody');
        const footerEl = document.getElementById('cmDialogFooter');
        return { overlay, titleEl, bodyEl, footerEl };
    }
    function showDialog({ title = 'Thông báo', message = '', type = 'info', buttons } = {}) {
        const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
        if (!overlay) return alert(message);
        const T = window.i18nQuote || {};
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
        okBtn.textContent = T.DialogOk || 'Đồng ý';
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

    document.addEventListener('DOMContentLoaded', () => {
        wireEvents();
        renumberRows();
        applyFiltersAndPagination();
    });
})();
