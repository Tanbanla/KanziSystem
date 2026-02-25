// JS for Quote page: handle buttons, validations, row operations, autofill from material selection, and API calls
(() => {
    const api = {
        insertListBaoGia: '/Quote/InsertDanhSachBaoGia',
        getMaterials: (keyword) => `/Quote/GetMaterialsByNameOrCode?keyword=${encodeURIComponent(keyword || '')}`,
        searchMaterials: '/Quote/GetSearchMaterial'
        , getSuppliersByMaHang: '/Quote/GetNhaCungCapByMaHang'
        , uploadQuoteExcel: '/Quote/UploadQuoteExcel'
        , exportAutoRender: '/Quote/ExportAutoRender'
        , getNCCByCategory: '/Quote/GetNCCByCategory'
        , exportRenderOutSide: '/Quote/ExportRenderOutSide'
        , exportTable: '/Quote/ExportTable'
    };

    const qs = (sel, root = document) => root.querySelector(sel);
    const qsa = (sel, root = document) => Array.from(root.querySelectorAll(sel));

    let currentPage = 1;
    const rowsPerPage = 10;
    let filteredRows = [];

    function renumberRows() {
        qsa('#quoteTableBody tr').forEach((tr, idx) => {
            const noCell = tr.children[0];
            if (noCell) noCell.textContent = String(idx + 1);
        });
        assignRowIds();
        applyFiltersAndPagination();
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
        const pageInfoText = `Showing ${startEntry} to ${endEntry} of ${totalEntries} entries`;
        qs('#pageInfo').textContent = pageInfoText;
        const pageNumberText = `Page ${currentPage} of ${totalPages}`;
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
    }

    function removeRow(btn) {
        const tbody = qs('#quoteTableBody');
        const rows = qsa('tr', tbody);
        const tr = btn.closest('tr');
        if (rows.length > 1 && tr) {
            tr.remove();
            renumberRows();
        }
    }

    function resetForm() {
        const form = qs('#quoteForm');
        form.reset();
        // keep 5 rows
        const tbody = qs('#quoteTableBody');
        if (tbody) {
            // remove extra rows until only 5 remain
            while (tbody.children.length > 5) {
                tbody.removeChild(tbody.lastElementChild);
            }

            // Remove any searchable dropdown wrappers inside the table and reset selects
            qsa('.ms-container', tbody).forEach((w) => w.remove());

            qsa('select.searchable-select', tbody).forEach((sel) => {
                // if wrapper was inserted as sibling after select, remove it
                try {
                    const next = sel.nextElementSibling;
                    if (next && next.classList && next.classList.contains('ms-container')) next.remove();
                } catch (e) { }

                // show original select
                sel.style.display = '';

                // reset stored enhanced flag so buildSearchableDropdown will re-run
                try { $(sel).data('search-dropdown', false); } catch (e) { }

                // set default value based on classes
                if (sel.classList.contains('rohsTb')) {
                    sel.value = 'No Need';
                } else if (sel.classList.contains('laybaogiaTb')) {
                    sel.value = 'true';
                } else if (sel.classList.contains('gapTb')) {
                    sel.value = 'false';
                } else {
                    // reset to first option if exists
                    if (sel.options && sel.options.length) sel.selectedIndex = 0;
                }
                sel.classList.remove('is-invalid');
            });

            // clear all inputs inside remaining rows
            qsa('tr', tbody).forEach((tr) => {
                qsa('input', tr).forEach((inp) => {
                    if (inp.type === 'checkbox' || inp.type === 'radio') inp.checked = false;
                    else inp.value = '';
                    inp.classList.remove('is-invalid');
                });
            });
        }

        // clear validation styles for form-level controls
        qsa('input, select', form).forEach((el) => el.classList.remove('is-invalid'));

        // close any open dropdowns and reattach them to their wrappers
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

        // re-init searchable dropdowns for remaining selects (use document to ensure all get processed)
        try { buildSearchableDropdown($(document)); }
        catch (ex) { console.error('Error re-initializing searchable dropdowns:', ex); }

        renumberRows();
    }

    // No header validation: the view does not include header fields

    function validateRow(tr) {
        // required fields per row: department, internal code, VN name, EN name, qty, unit, supplier, laybaogia, desired date
        let ok = true;

        // Helper function to get value and mark as invalid
        const validateField = (selector, isSelect = false) => {
            let element;

            if (isSelect) {
                element = tr.querySelector(selector);
            } else {
                // For inputs, we need to be more specific
                const elements = qsa(selector, tr);
                if (elements.length > 0) {
                    element = elements[0];
                }
            }

            if (!element) return false;

            const val = element.value ? element.value.toString().trim() : '';
            const isValid = val !== '';

            if (!isValid) ok = false;

            // Mark element as invalid
            element.classList.toggle('is-invalid', !isValid);

            // For searchable selects, also mark the custom dropdown UI
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

        // Validate all required fields
        requiredFields.forEach(field => {
            validateField(field.selector, field.isSelect);
        });

        // Ngày muốn nhận hàng (required - có dấu *)
        const dateInputs = qsa('input[type="date"]', tr);
        if (dateInputs.length >= 1) {
            const ngayMuonNhan = dateInputs[0]; // First date input is ngayMuonNhan
            const ngayMuonNhanValid = ngayMuonNhan.value && ngayMuonNhan.value.toString().trim() !== '';

            if (!ngayMuonNhanValid) ok = false;
            ngayMuonNhan.classList.toggle('is-invalid', !ngayMuonNhanValid);
        }

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
                    if (values && values.textContent && values.textContent.trim() !== '') return values.textContent.trim();
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

        // Tạo mã đơn tự động dựa trên tên phòng và ngày hiện tại (theo giờ VN)
        const generateMaDon = () => {
            const maPhongBan = getSel('.tenPhongBanTb');
            const nowVN = getVietnamTime();
            const year = nowVN.getFullYear();
            const month = String(nowVN.getMonth() + 1).padStart(2, '0');
            const day = String(nowVN.getDate()).padStart(2, '0');
            return `RQ_${maPhongBan}_${year}_${month}_${day}`;
        };

        // Lấy ngày tạo theo múi giờ +7
        const createDateVN = getVietnamTime();

        const obj = {
            ID: 0,
            CHR_MaDon: generateMaDon(),
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
            CHR_CreateBy: getInputBy(['input[id^="nguoiYeuCauRow_"]', 'input[placeholder*="Người yêu cầu"]']) === '' ? src.user : getInputBy(['input[id^="nguoiYeuCauRow_"]', 'input[placeholder*="Người yêu cầu"]']),
            // Sử dụng ISO string với múi giờ +7
            DTM_CreateDate: toVietnamISOString(createDateVN),
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
        const rows = qsa('#quoteTableBody tr');
        let rowsValid = true;
        let rowsCheckReason = true;
        const payload = [];
        rows.forEach((tr) => {
            if (!validateRow(tr)) rowsValid = false;
            if (!CheckLyDoTuChoi(tr)) rowsCheckReason = false;
            payload.push(collectRow(tr));
        });
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

                    // re-init searchable dropdowns and ids
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
            const tenMoThuTucValue = material.tenMoThuTuc || material.TenMoThuTuc || (typeof material.GetTenMoThuTuc === 'function' ? material.GetTenMoThuTuc() : null);
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
            if (categorySelect && material.category_VN) categorySelect.value = material.category_VN;
            // Optionally set PHAN LOẠI 
            const categoryInput = tr.querySelector('.tenPhanLoaiTb');
            const loaiHangValue = material.loaiHang || material.LoaiHang || (typeof material.GetLoaiHang === 'function' ? material.GetLoaiHang() : null);
            if (categoryInput && loaiHangValue) categoryInput.value = loaiHangValue;

            // Fetch suppliers for this material and if >1 create rows per supplier
            //try {
            //    const supRes = await fetch(api.getSuppliersByMaHang, {
            //        method: 'POST',
            //        headers: { 'Content-Type': 'application/json' },
            //        body: JSON.stringify(code)
            //    });
            //    if (!supRes.ok) throw new Error(await supRes.text());
            //    const suppliers = await supRes.json();
            //    if (Array.isArray(suppliers) && suppliers.length > 0) {
            //        // Helper to extract supplier code
            //        const getSupCode = (s) => s?.chR_MaNCC ||  (typeof s === 'string' ? s : undefined) || '';
            //        // If only one supplier, set current row's supplier
            //        if (suppliers.length === 1) {
            //            const s = suppliers[0];
            //            const supCode = getSupCode(s);
            //            const supSel = tr.querySelector('.nhaCungCapTb');
            //            if (supSel) {
            //                supSel.value = supCode;
            //                try { updateSearchableSelectDisplay(supSel); } catch (e) { }
            //            }
            //            // Fill mã hàng NCC and NSX for the single supplier into the current row
            //            const codeByNccInputSingle = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('mã hàng ncc'));
            //            if (codeByNccInputSingle && s.nvchR_CodeByNCC) codeByNccInputSingle.value = s.nvchR_CodeByNCC;
            //            const nsxInputSingle = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('nsx'));
            //            if (nsxInputSingle && s.nvchR_MakeIn) nsxInputSingle.value = s.nvchR_MakeIn;
            //        } else if (suppliers.length > 1) {
            //            // Collect current row values to replicate
            //            const values = {};
            //            // copy inputs
            //            qsa('input', tr).forEach((inp) => values[inp.name || inp.id || inp.placeholder || inp.type] = inp.value);
            //            // copy selects
            //            qsa('select', tr).forEach((sel) => values[sel.className || sel.name || sel.id] = sel.value);

            //            // For first supplier, set current row
            //            const s0 = suppliers[0];
            //            const firstCode = getSupCode(s0);
            //            const supSel0 = tr.querySelector('.nhaCungCapTb');
            //            if (supSel0) {
            //                supSel0.value = firstCode;
            //                // update visible searchable UI for this existing row
            //                try { updateSearchableSelectDisplay(supSel0); } catch (e) { }
            //            }
            //            // Also fill mã hàng NCC and NSX for the first supplier into the current row
            //            const codeByNccInputFirst = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('mã hàng ncc'));
            //            if (codeByNccInputFirst && s0.nvchR_CodeByNCC) codeByNccInputFirst.value = s0.nvchR_CodeByNCC;
            //            const nsxInputFirst = qsa('input', tr).find((i) => (i.placeholder || '').toLowerCase().includes('nsx'));
            //            if (nsxInputFirst && s0.nvchR_MakeIn) nsxInputFirst.value = s0.nvchR_MakeIn;

            //            // Insert additional rows for remaining suppliers
            //            let insertAfter = tr;
            //            for (let i = 1; i < suppliers.length; i++) {
            //                const s = suppliers[i];
            //                const supCode = getSupCode(s);
            //                // clone the row
            //                const newRow = tr.cloneNode(true);
            //                // clean any ms-container wrappers inside clone
            //                qsa('.ms-container', newRow).forEach(w => w.remove());
            //                // restore selects display
            //                qsa('select.searchable-select', newRow).forEach(sv => sv.style.display = '');

            //                // set values on inputs/selects in newRow
            //                qsa('input', newRow).forEach((inp) => {
            //                    const key = inp.name || inp.id || inp.placeholder || inp.type;
            //                    if (values.hasOwnProperty(key)) inp.value = values[key];
            //                    inp.classList.remove('is-invalid');
            //                });
            //                qsa('select', newRow).forEach((sel) => {
            //                    const key = sel.className || sel.name || sel.id;
            //                    if (values.hasOwnProperty(key)) sel.value = values[key];
            //                    sel.classList.remove('is-invalid');
            //                });
            //                // Set supplier value for this clone and update its searchable display
            //                const supSel = newRow.querySelector('.nhaCungCapTb');
            //                if (supSel) {
            //                    supSel.value = supCode || '';
            //                    try { updateSearchableSelectDisplay(supSel); } catch(e) { }
            //                }

            //                // Fill ten hang ncc in the cloned row
            //                const codeByNccInput = qsa('input', newRow).find((i) => (i.placeholder || '').toLowerCase().includes('mã hàng ncc'));
            //                if (codeByNccInput && s.nvchR_CodeByNCC) codeByNccInput.value = s.nvchR_CodeByNCC;
            //                // Fill san xuat in the cloned row
            //                const nsxInput = qsa('input', newRow).find((i) => (i.placeholder || '').toLowerCase().includes('nsx'));
            //                if (nsxInput && s.nvchR_MakeIn) nsxInput.value = s.nvchR_MakeIn;


            //                // insert after last inserted
            //                insertAfter.parentNode.insertBefore(newRow, insertAfter.nextSibling);
            //                insertAfter = newRow;
            //            }

            //            // re-init searchable dropdowns and ids
            //            try { buildSearchableDropdown($(document)); } catch (ex) { }
            //            renumberRows();
            //        }
            //    }
            //} catch (err) {
            //    console.warn('Không thể lấy NCC cho mã hàng:', err);
            //}
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

        // Populate Material list
        lst.innerHTML = '';
        //const srcMaterialSel = qs('.maHangNoiBo');
        const body = { MaHang: '', Name: '', NhomHang: '', PageIndex: 0, PageSize: 0 };
        const res = await fetch(api.searchMaterials, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        if (!res.ok) throw new Error(await res.text());
        const srcMaterialSel = await res.json();
        const items = [];
        if (srcMaterialSel) {
            srcMaterialSel.forEach((o) => {
                if (!o.material_Code) return;
                var nd = o.material_Code + ' - ' + o.material_Name_VN
                items.push({ code: o.material_Code, text: nd || o.material_Code });
            });
        }
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
        items.forEach(it => lst.appendChild(createItemEl(it)));

        // Search filter
        searchBox.oninput = () => {
            const q = (searchBox.value || '').toLowerCase();
            Array.from(lst.children).forEach((el) => {
                const s = el.dataset.search || '';
                el.style.display = !q || s.includes(q) ? '' : 'none';
            });
        };

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
                setBusy(false);
            }
        };

        // Show modal
        showAr();
    }
    function wireEvents() {
        const container = qs('#quote-request');
        if (!container) return;
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

        qsa('.btn-remove-row', container).forEach((btn) => {
            btn.addEventListener('click', (e) => removeRow(e.currentTarget));
        });

        async function exportTable() {
            try {
                showLoading((window.i18nQuote && window.i18nQuote.Exporting) || 'Đang xuất...');
                const rows = qsa('#quoteTableBody tr');
                const payload = [];
                rows.forEach(tr => {
                    // use existing collectRow to build DTO-like object
                    const obj = collectRow(tr);
                    payload.push(obj);
                });

                const res = await fetch(api.exportTable, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
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
                if (!Array.isArray(items)) throw new Error((window.i18nQuote && window.i18nQuote.MsgInvalidData) || 'Dữ liệu không hợp lệ');
                populateTableFromItems(items);
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
        // Download Excel stub
        qs('#btnDownloadExcel')?.addEventListener('click', () => {
            try {
                const url = '/template/TemPlateQuote.xlsx';
                const a = document.createElement('a');
                a.href = url;
                a.download = 'Mau_Quote.xlsx';
                document.body.appendChild(a);
                a.click();
                a.remove();
            } catch (err) {
                console.error('Error downloading template', err);
            }

        });
        // Dừng xử lý khi chọn loại hàng
        // Autofill when selecting internal material code
        qs('#quoteTableBody')?.addEventListener('change', (e) => {
            const t = e.target;
            if (t.classList && t.classList.contains('maHangNoiBo')) {
                autofillFromMaterialSelect(t);
            }
        });
        // lấy dữ liệu tự động khi thay đổi chủng loại
        qs('#quoteTableBody')?.addEventListener('change', (e) => {
            const t = e.target;
            if (t.classList && t.classList.contains('chungLoaiTb')) {
                autoAddRowByCategory(t);
            }
        })
        // When category changes, reload material list from server and update material selects
        qs('#quoteTableBody')?.addEventListener('change', async (e) => {
            const t = e.target;
            const T18 = window.i18nQuote || {};
            if (t.classList && t.classList.contains('chungLoaiTb')) {
                const nhomHang = (t.value || '').toString();
                try {
                    // call POST /Quote/GetSearchMaterial with JSON body
                    const body = { MaHang: '', Name: '', NhomHang: nhomHang, PageIndex: 0, PageSize: 0 };
                    const res = await fetch(api.searchMaterials, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(body)
                    });
                    if (!res.ok) throw new Error(await res.text());
                    const materials = await res.json();
                    // Update all maHangNoiBo selects in the table
                    const selects = qsa('.maHangNoiBo');
                    selects.forEach((sel) => {
                        // remove any custom wrapper
                        try {
                            const $sel = $(sel);
                            const next = sel.nextElementSibling;
                            if (next && next.classList && next.classList.contains('ms-container')) {
                                next.remove();
                            }
                            // preserve current selection if any, then rebuild options
                            const prevValue = sel.value;
                            sel.style.display = '';
                            sel.innerHTML = '';
                            const optDefault = document.createElement('option');
                            optDefault.value = '';
                            optDefault.textContent = T18.SelectInternalMaterialCode;
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
                            // restore previous selection if it still exists; otherwise keep default
                            try {
                                if (prevValue && Array.from(sel.options).some(o => o.value === prevValue)) {
                                    sel.value = prevValue;
                                } else {
                                    sel.selectedIndex = 0;
                                }
                            } catch (ex) {
                                sel.selectedIndex = 0;
                            }
                            // mark for rebuild
                            try { $sel.data('search-dropdown', false); } catch { }
                        } catch (err) {
                            console.warn('Error updating material select:', err);
                        }
                    });
                    // reinitialize searchable dropdowns
                    try { buildSearchableDropdown($(document)); } catch (ex) { }
                } catch (err) {
                    console.warn('Không thể tải danh sách vật tư:', err);
                }
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
                applyFiltersAndPagination();
            }
        });
        qs('#nextPage')?.addEventListener('click', (e) => {
            e.preventDefault();
            const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
            if (currentPage < totalPages) {
                currentPage++;
                applyFiltersAndPagination();
            }
        });
    }

    // Tìm kiếm 
    function buildSearchableDropdown($container) {
        $container.find('select.searchable-select').each(function () {
            const $select = $(this);
            if ($select.data('search-dropdown') === true) return;

            // Cache options
            const options = $select.find('option').map(function () {
                return { value: this.value, text: $(this).text(), selected: this.selected };
            }).get();

            // Build UI
            const $wrapper = $('<div class="ms-container"></div>');
            const $btn = $('<div class="ms-btn"><span class="ms-values"></span><span class="ms-placeholder"></span><span class="ms-caret">▾</span></div>');
            const $dropdown = $('<div class="ms-dropdown"></div>');
            const $search = $('<div class="ms-search"><input type="text" placeholder="Tìm..." /></div>');
            const $list = $('<div class="ms-list"></div>');

            // Populate list
            function renderList(query) {
                const q = (query || '').toLowerCase();
                $list.empty();
                let hasItems = false;
                options.forEach(function (opt) {
                    if (!q || opt.text.toLowerCase().includes(q)) {
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
            }

            function updateButtonText() {
                const val = $select.val();
                const found = options.find(o => o.value === val);
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
                    renderList('');
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

            $search.find('input').on('input', function () {
                renderList($(this).val());
            });

            // Mark enhanced
            $select.data('search-dropdown', true);
        });
    }

    // Initialize for existing selects
    buildSearchableDropdown($(document));

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

        // Nhà sản xuất
        setInputById('nsx', dto.nvchR_NhaSanXuat);

        // Nhà cung cấp (select)
        setSelectById('nhaCungCapTb', dto.chR_MaNCC || dto.nvchR_TenNCC);

        // Lấy báo giá
        const layBaoGiaValue = dto.biT_LayBaoGia === true ? 'true' : dto.biT_LayBaoGia === false ? 'false' : '';
        setSelectById('laybaogiaTb', layBaoGiaValue);

        // Lý do
        setInputById('lyDo', dto.nvchR_LyDo);

        // Helper function để format ngày
        const dateVN = (d) => {
            try {
                if (!d) return '';
                const dt = new Date(d);
                return dt.toISOString().slice(0, 10);
            } catch {
                return '';
            }
        };

        // Ngày muốn nhận
        setInputById('ngayMuonNhan', dateVN(dto.dtM_NgayMuonNhan));

        // Kỳ hạn chọn NCC
        setInputById('kyHanChonNCC', dateVN(dto.dtM_KyHan));

        // Gấp
        setSelectById('gapTb', dto.chR_Gap);

        // Người yêu cầu
        setInputById('nguoiYeuCauRow', dto.chR_CreateBy || (window.indexQuoteData && window.indexQuoteData.user) || '');
    }

    async function populateTableFromItems(items) {
        const tbody = qs('#quoteTableBody');
        if (!tbody) return;
        // capture a base row before clearing, so we keep structure
        const existing = qs('#quoteTableBody tr');
        // clear existing rows
        tbody.innerHTML = '';
        const template = document.createElement('tr');
        const baseRow = existing ? existing.cloneNode(true) : null;
        for (let i = 0; i < items.length; i++) {
            const dto = items[i] || {};
            const row = baseRow ? baseRow.cloneNode(true) : template.cloneNode(true);
            if (!baseRow) {
                continue;
            }
            // clean wrappers
            qsa('.ms-container', row).forEach(w => w.remove());
            qsa('select.searchable-select', row).forEach(s => { s.style.display = ''; $(s).data('search-dropdown', false); });
            // reset fields
            qsa('input', row).forEach(inp => { inp.value = ''; inp.classList.remove('is-invalid'); });
            qsa('select', row).forEach(sel => { sel.value = ''; sel.classList.remove('is-invalid'); });

            // populate với row index (i + 1)
            populateRowFromDto(row, dto, i + 1);
            tbody.appendChild(row);
        }
        try { buildSearchableDropdown($(tbody)); } catch { }
        renumberRows();
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
