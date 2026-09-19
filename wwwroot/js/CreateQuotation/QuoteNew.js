(() => {
    'use strict';

    const $ = id => document.getElementById(id);

    const getModalController = modal => {
        const { bootstrap, jQuery } = window;

        if (bootstrap?.Modal) {
            return bootstrap.Modal.getInstance?.(modal)
                || new bootstrap.Modal(modal);
        }

        if (jQuery?.fn?.modal) {
            return {
                show: () => jQuery(modal).modal('show')
            };
        }

        return null;
    };

    const closeQuoteModal = modal => {
        if (!modal) return;

        modal.classList.remove('show');
        modal.style.display = 'none';
        modal.setAttribute('aria-hidden', 'true');
        modal.removeAttribute('aria-modal');
        modal.removeAttribute('role');

        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('padding-right');
        document.querySelectorAll('.modal-backdrop').forEach(backdrop => backdrop.remove());
    };

    const getApiUrl = path => `${window.apiBaseUrl || ''}${path}`;

    const readJsonResponse = async response => {
        if (!response.ok) {
            const message = await response.text().catch(() => response.statusText);
            throw new Error(message || response.statusText || 'Không thể tải dữ liệu');
        }

        return response.json();
    };

    const getResponseData = response => Array.isArray(response)
        ? response
        : (response?.data ?? response?.Data ?? []);

    const quoteStorageKey = 'quote-v2-items';
    let quoteItems = [];
    let editingQuoteIndex = null;
    let currentPage = 1;
    let rowsPerPage = 20;
    const clearQuoteItems = () => {
        quoteItems = [];
        localStorage.removeItem(quoteStorageKey);
    };
    clearQuoteItems();

    const valueOf = id => ($(id)?.value || '').toString().trim();
    const setInvalid = (field, invalid) => {
        if (!field) return;
        field.classList.toggle('is-invalid', invalid);
        if (field.tagName === 'SELECT') {
            field.nextElementSibling?.querySelector('.ms-btn')?.classList.toggle('is-invalid', invalid);
        }
    };

    const required = (id, name, errors) => {
        const field = $(id);
        const valid = valueOf(id) !== '';
        setInvalid(field, !valid);
        if (!valid) errors.push(`${name} không được để trống.`);
    };

    const normalizeSearchValue = value => String(value ?? '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase()
        .trim();

    const getFilteredQuoteItems = () => {
        const filters = {
            internalCode: normalizeSearchValue(valueOf('tableSearchInternalCode')),
            category: normalizeSearchValue(valueOf('tableSearchCategory')),
            supplierItemCode: normalizeSearchValue(valueOf('tableSearchSupplierCode'))
        };

        const hasFilter = Object.values(filters).some(Boolean);
        const items = quoteItems
            .map((item, index) => ({ item, index }))
            .filter(({ item }) => !hasFilter || (
                (!filters.internalCode || normalizeSearchValue(item.internalCode).includes(filters.internalCode)) &&
                (!filters.category || normalizeSearchValue(item.category).includes(filters.category)) &&
                (!filters.supplierItemCode || normalizeSearchValue(item.supplierItemCode).includes(filters.supplierItemCode))
            ));

        return { items, hasFilter };
    };

    const renderQuoteItems = () => {
        const body = $('InforTableBody');
        if (!body) return;
        const { items, hasFilter } = getFilteredQuoteItems();
        const result = $('tableSearchResult');
        const paginationInfo = $('paginationInfo');
        const pageInfo = $('pageInfo');
        const paginationControls = $('paginationControls');
        const pageNumberInfo = $('pageNumberInfo');
        const totalPages = Math.max(1, Math.ceil(items.length / rowsPerPage));

        currentPage = Math.min(Math.max(currentPage, 1), totalPages);
        const startIndex = (currentPage - 1) * rowsPerPage;
        const pageItems = items.slice(startIndex, startIndex + rowsPerPage);

        if (result) {
            result.textContent = hasFilter
                ? `Hiển thị ${items.length}/${quoteItems.length} mặt hàng`
                : (quoteItems.length ? `Tổng số: ${quoteItems.length} mặt hàng` : '');
        }

        if (paginationInfo) {
            paginationInfo.style.display = items.length ? 'flex' : 'none';
        }

        if (pageInfo) {
            pageInfo.textContent = items.length
                ? `Hiển thị ${startIndex + 1}-${Math.min(startIndex + rowsPerPage, items.length)} / ${items.length}`
                : '';
        }

        if (paginationControls) {
            paginationControls.style.display = totalPages > 1 ? 'flex' : 'none';
        }

        if (pageNumberInfo) {
            pageNumberInfo.textContent = `${currentPage}/${totalPages}`;
        }

        const previousPage = $('prevPage');
        const nextPage = $('nextPage');
        previousPage?.classList.toggle('disabled', currentPage <= 1);
        nextPage?.classList.toggle('disabled', currentPage >= totalPages);

        if (!pageItems.length) {
            const message = hasFilter ? 'Không tìm thấy mặt hàng phù hợp' : 'Không có dữ liệu';
            body.innerHTML = `<tr><td colspan="14" class="text-center text-muted py-4">${message}</td></tr>`;
            return;
        }
        body.innerHTML = pageItems.map(({ item, index }, pageIndex) => `<tr>
            <td class="text-center">${startIndex + pageIndex + 1}</td>
            <td class="text-center">${escapeHtml(item.internalCode)}</td>
            <td class="text-center">${escapeHtml(item.category)}</td>
            <td class="text-center">${escapeHtml(item.classification)}</td>
            <td>${escapeHtml(item.vietnameseName)}</td>
            <td>${escapeHtml(item.englishName)}</td>
            <td class="text-center">${escapeHtml(item.supplierItemCode)}</td>
            <td class="text-center">${escapeHtml(item.quantity)}</td>
            <td class="text-center">${escapeHtml(item.unit)}</td>
            <td class="text-center">${escapeHtml(item.supplierDeadline)}</td>
            <td class="text-center">${escapeHtml(item.desiredDate)}</td>
            <td class="text-center">${item.urgent ? 'Có' : 'Không'}</td>
            <td class="text-center">${escapeHtml(item.suppliers.filter(s => s.selected).map(s => s.code).join(', '))}</td>
           <td class="text-center">
                <button type="button"
                    class="btn btn-sm btn-outline-primary btn-edit-quote me-1"
                    data-index="${index}">
                    <i class="fas fa-edit"></i>
                </button>

                <button type="button"
                    class="btn btn-sm btn-outline-danger btn-delete-quote"
                    data-index="${index}">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        </tr>`).join('');
    };

    const validateAndCollect = () => {
        const errors = [];
        ['selectPhanLoai', 'selectChungLoai', 'selectTenVN', 'selectTenEN', 'selectSoLuong', 'editDonVi', 'NgayMuonNhan', 'LuaChonNcc']
            .forEach((id, index) => required(id, ['Phân loại hàng', 'Chủng loại hàng', 'Tên tiếng Việt', 'Tên tiếng Anh', 'Số lượng', 'Đơn vị', 'Ngày muốn nhận hàng', 'Hạn chọn nhà cung cấp'][index], errors));
        const suppliers = Array.from(document.querySelectorAll('#supplierTableBody tr')).filter(row => row.querySelector('.supplier-quote-checkbox')).map(row => ({
            code: row.querySelector('input[name^="supplierCode_"]')?.value || '',
            name: row.cells[2]?.textContent.trim() || '',
            selected: row.querySelector('.supplier-quote-checkbox').checked,
            reason: row.querySelector('.supplier-reason')?.value.trim() || ''
        }));

        const supplierContainer = $('supplierTableContainer');
        let supplierError = false;

        if (suppliers.filter(s => s.selected).length > 5) {
            errors.push('Chỉ được chọn tối đa 5 nhà cung cấp.');
            supplierError = true;
        }

        suppliers.filter(s => !s.selected).forEach(s => {
            const row = Array.from(document.querySelectorAll('#supplierTableBody tr'))
                .find(candidate =>
                    candidate.querySelector('input[name^="supplierCode_"]')?.value === s.code
                );

            const reason = row?.querySelector('.supplier-reason');

            setInvalid(reason, !s.reason);

            if (!s.reason) {
                errors.push(` Vui lòng nhập lý do không xin báo giá cho NCC ${s.name || s.code}.`);
                supplierError = true;
            }
        });
        const materialCode = $('searchMahangNB');
        const nccCode = $('selectMaHangNCC');
        if (!valueOf('searchMahangNB') && !valueOf('selectMaHangNCC')) {
            setInvalid(materialCode, true);
            setInvalid(nccCode, true);
            errors.push(' Mã hàng NCC bắt buộc khi chưa có Mã hàng nội bộ.');
        } else {
            setInvalid(materialCode, false);
            setInvalid(nccCode, false);
        }
        supplierContainer.classList.toggle(
            'supplier-table-error',
            supplierError
        );
        if (errors.length) return { errors };
        return { item: {
            internalCode: valueOf('searchMahangNB'),
            category: valueOf('selectChungLoai'),
            classification: valueOf('selectPhanLoai'),
            vietnameseName: valueOf('selectTenVN'),
            englishName: valueOf('selectTenEN'),
            supplierItemCode: valueOf('selectMaHangNCC'),
            quantity: valueOf('selectSoLuong'),
            unit: valueOf('editDonVi'),
            supplierDeadline: valueOf('LuaChonNcc'),
            desiredDate: valueOf('NgayMuonNhan'),
            urgent: valueOf('selectGap') === 'true',
            receiver: valueOf('NguoiNhanHang'),
            receiverPhone: valueOf('SDTNguoiNhanHang'),
            deliveryLocation: valueOf('DiaDiemGiaoHang'),
            equipmentCode: valueOf('selectMaThietBi'),
            reason: valueOf('LyDoXinBaoGia'),
            shape: valueOf('selectHinhDang'),
            material: valueOf('selectChatLieu'),
            composition: valueOf('selectThanhPhan'),
            dimension: valueOf('selectKichThuoc'),
            usedFor: valueOf('selectDungcho'),
            purpose: valueOf('selectDungde'),
            rohs: valueOf('selectROHS'),
            cocq: valueOf('selectCOCQ'),
            msds: valueOf('selectMSDS'),
            safety: valueOf('selectAnToan'),
            boxLink: valueOf('LinkBox'),
            suppliers
        } };
    };

    const setFieldValue = (id, value, dispatchChange = true) => {
        const field = $(id);
        const normalizedValue = value ?? '';
        if (field?.tagName === 'SELECT' && normalizedValue && !Array.from(field.options).some(option => option.value === String(normalizedValue))) {
            field.add(new Option(String(normalizedValue), String(normalizedValue)));
        }
        if (field) field.value = normalizedValue;
        if (dispatchChange && field?.tagName === 'SELECT') {
            field.dispatchEvent(new Event('change', { bubbles: true }));
        }
    };

    const refreshSearchableSelect = id => {
        const field = $(id);
        const button = field?.nextElementSibling?.querySelector('.ms-btn');
        if (!field || !button) return;

        const option = Array.from(field.options).find(item => item.value === field.value);
        button.querySelector('.ms-values').textContent = option?.textContent || '';
        button.querySelector('.ms-placeholder').textContent = option ? '' : '-- Chọn --';
    };

    const fillMaterialFields = material => {
        setFieldValue('selectTenVN', material?.Material_Name_VN ?? material?.material_Name_VN);
        setFieldValue('selectTenEN', material?.Material_Name_EN ?? material?.material_Name_EN);
        setFieldValue('editDonVi', material?.Unit ?? material?.unit);
        setFieldValue('selectPhanLoai', material?.GoodKind ?? material?.goodKind ?? material?.LoaiHang ?? material?.loaiHang);
        setFieldValue('selectChungLoai', material?.Category_VN ?? material?.category_VN);
        setFieldValue('selectHinhDang', material?.Shape ?? material?.shape);
        setFieldValue('selectChatLieu', material?.Material ?? material?.material);
        setFieldValue('selectThanhPhan', material?.Composition ?? material?.composition);
        setFieldValue('selectKichThuoc', material?.Dimension ?? material?.dimension);
        setFieldValue('selectDungcho', material?.UsedFor ?? material?.usedFor);
        setFieldValue('selectDungde', material?.Purpose ?? material?.purpose);
        setFieldValue('selectMaHangNCC', material?.code_Suppiler)
    };

    const clearSupplierTable = () => {
        const body = $('supplierTableBody');
        if (body) {
            body.innerHTML = '<tr><td colspan="6" class="text-center text-muted py-4">Chưa có thông tin nhà cung cấp</td></tr>';
        }
    };

    const clearQuoteForm = form => {
        if (!form) return;

        form.reset();
        form.querySelectorAll('.is-invalid').forEach(field => field.classList.remove('is-invalid'));
        form.querySelectorAll('.ms-btn.is-invalid').forEach(field => field.classList.remove('is-invalid'));
        form.querySelectorAll('select').forEach(select => {
            select.dispatchEvent(new Event('change', { bubbles: true }));
        });
        clearSupplierTable();
    };

    const renderSuppliers = suppliers => {
        const body = $('supplierTableBody');
        if (!body) return;

        body.innerHTML = '';
        if (!Array.isArray(suppliers) || suppliers.length === 0) {
            clearSupplierTable();
            return;
        }

        suppliers.forEach((supplier, index) => {
            const code = supplier?.chR_MaNCC ?? '';
            const name = supplier?.nvchR_TenNCC ?? '';
            const manufacturer = supplier?.nvchR_SanXuat ?? '';
            const row = document.createElement('tr');
            row.innerHTML = `
                <td class="text-center">${index + 1}</td>
                <td class="text-center">${escapeHtml(code)}</td>
                <td class="text-center">${escapeHtml(name)}</td>
                <td class="text-center">${escapeHtml(manufacturer)}</td>
                <td class="text-center supplier-checkbox-cell">
                    <input type="checkbox" class="form-check-input supplier-quote-checkbox"
                           name="supplierQuote_${index}" value="true" checked
                           aria-label="Lấy báo giá từ ${escapeHtml(name)}">
                    <input type="hidden" name="supplierCode_${index}" value="${escapeHtml(code)}">
                </td>
                <td>
                    <textarea type="text" class="form-control form-control-sm supplier-reason"
                           name="supplierReason_${index}" placeholder="Nhập lý do không lấy báo giá" disabled></textarea>
                </td>`;
            body.appendChild(row);
        });

        body.querySelectorAll('.supplier-quote-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', () => {
                const reason = checkbox.closest('tr')?.querySelector('.supplier-reason');
                if (!reason) return;
                reason.disabled = checkbox.checked;
                if (checkbox.checked) reason.value = '';
            });
        });
    };

    const escapeHtml = value => String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');

    const loadMaterial = async code => {
        if (!code) return;

        try {
            const response = await fetch(getApiUrl('/Quote/GetSearchMaterial'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ MaHang: code, Name: '', NhomHang: '', PageIndex: 1, PageSize: 1 })
            });
            const materials = getResponseData(await readJsonResponse(response));
            fillMaterialFields(materials[0]);
        } catch (error) {
            console.error('Không thể tải thông tin mặt hàng', error);
        }
    };

    const loadSuppliers = async category => {
        clearSupplierTable();
        if (!category) return [];

        try {
            const response = await fetch(getApiUrl('/Quote/GetNCCByCategory'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(category)
            });
            const suppliers = getResponseData(await readJsonResponse(response));
            renderSuppliers(suppliers);
            return suppliers;
        } catch (error) {
            console.error('Không thể tải danh sách nhà cung cấp', error);
            const body = $('supplierTableBody');
            if (body) body.innerHTML = '<tr><td colspan="6" class="text-center text-danger py-4">Không thể tải danh sách nhà cung cấp</td></tr>';
            return [];
        }
    };

    const setQuoteModalMode = editing => {
        const title = $('addInforModalLabel');
        const saveButton = $('btnSaveHistoryEdit');

        if (title) title.textContent = editing ? 'Chỉnh sửa mặt hàng báo giá' : 'Thêm mặt hàng báo giá';
        if (saveButton) saveButton.innerHTML = editing
            ? '<i class="fas fa-save me-1"></i> Cập nhật thông tin'
            : '<i class="fas fa-save me-1"></i> Lưu thông tin';
    };

    const applySupplierValues = suppliers => {
        const savedSuppliers = Array.isArray(suppliers) ? suppliers : [];

        document.querySelectorAll('#supplierTableBody tr').forEach(row => {
            const code = row.querySelector('input[name^="supplierCode_"]')?.value || '';
            const saved = savedSuppliers.find(supplier =>
                String(supplier?.code ?? supplier?.chR_MaNCC ?? '').trim().toLowerCase() === code.trim().toLowerCase());
            const checkbox = row.querySelector('.supplier-quote-checkbox');
            const reason = row.querySelector('.supplier-reason');

            if (!checkbox || !reason) return;
            checkbox.checked = saved?.selected ?? false;
            reason.value = saved?.reason || '';
            reason.disabled = checkbox.checked;
        });
    };

    const fillQuoteForm = async item => {
        const fields = {
            searchMahangNB: item.internalCode,
            selectPhanLoai: item.classification,
            selectChungLoai: item.category,
            selectTenVN: item.vietnameseName,
            selectTenEN: item.englishName,
            selectMaThietBi: item.equipmentCode,
            selectMaHangNCC: item.supplierItemCode,
            selectSoLuong: item.quantity,
            editDonVi: item.unit,
            LyDoXinBaoGia: item.reason,
            selectHinhDang: item.shape,
            selectChatLieu: item.material,
            selectThanhPhan: item.composition,
            selectKichThuoc: item.dimension,
            selectDungcho: item.usedFor,
            selectDungde: item.purpose,
            selectROHS: item.rohs,
            selectCOCQ: item.cocq,
            selectMSDS: item.msds,
            selectAnToan: item.safety,
            NgayMuonNhan: item.desiredDate,
            LuaChonNcc: item.supplierDeadline,
            selectGap: item.urgent ? 'true' : 'false',
            NguoiNhanHang: item.receiver,
            SDTNguoiNhanHang: item.receiverPhone,
            DiaDiemGiaoHang: item.deliveryLocation,
            LinkBox: item.boxLink
        };

        Object.entries(fields).forEach(([id, value]) => setFieldValue(id, value, false));
        await loadSuppliers(item.category);
        Object.entries(fields).forEach(([id, value]) => setFieldValue(id, value, false));
        ['searchMahangNB', 'selectPhanLoai', 'selectChungLoai', 'selectGap']
            .forEach(refreshSearchableSelect);
        applySupplierValues(item.suppliers);
    };

    const initializeQuoteModal = () => {
        const btnAdd = $('btnAdd');
        const modal = $('addInforModal');
        const form = $('addInforForm');

        if (!btnAdd || !modal) return;

        $('searchMahangNB')?.addEventListener('change', event => loadMaterial(event.target.value));
        $('selectChungLoai')?.addEventListener('change', event => loadSuppliers(event.target.value));

        btnAdd.addEventListener('click', () => {
            editingQuoteIndex = null;
            setQuoteModalMode(false);
            clearQuoteForm(form);
            getModalController(modal)?.show();
        });

        $('btnClearAddInforModal')?.addEventListener('click', () => clearQuoteForm(form));

        form.querySelectorAll('input, select, textarea').forEach(field => {
            field.addEventListener('input', () => setInvalid(field, false));
            field.addEventListener('change', () => setInvalid(field, false));
        });

        modal.querySelectorAll('.quote-modal-close, .quote-modal-close-action')
            .forEach(button => button.addEventListener('click', () => closeQuoteModal(modal)));

        modal.addEventListener('shown.bs.modal', () => {
            modal
                .querySelector('select, input:not([type="file"]), textarea')
                ?.focus();
        });

        $('InforTableBody')?.addEventListener('click', event => {
            const button = event.target.closest('.btn-delete-quote');
            if (button) {
                quoteItems.splice(Number(button.dataset.index), 1);
                localStorage.setItem(quoteStorageKey, JSON.stringify(quoteItems));
                renderQuoteItems();
                return;
            }

            const editButton = event.target.closest('.btn-edit-quote');
            if (!editButton) return;

            const index = Number(editButton.dataset.index);
            const item = quoteItems[index];
            if (!item) return;

            editingQuoteIndex = index;
            setQuoteModalMode(true);
            clearQuoteForm(form);
            fillQuoteForm(item);
            getModalController(modal)?.show();
        });

        $('btnSaveHistoryEdit')?.addEventListener('click', () => {
            const result = validateAndCollect();
            if (result.errors) {
                document.querySelector('#addInforModal .is-invalid')?.focus();
                return;
            }
            if (editingQuoteIndex === null) {
                quoteItems.push(result.item);
            } else {
                quoteItems[editingQuoteIndex] = result.item;
            }
            currentPage = Math.ceil(quoteItems.length / rowsPerPage) || 1;
            localStorage.setItem(quoteStorageKey, JSON.stringify(quoteItems));
            renderQuoteItems();
            editingQuoteIndex = null;
            setQuoteModalMode(false);
            clearQuoteForm(form);
            closeQuoteModal(modal);
        });
    };

    const initializeQuoteLoading = () => {
        const loading = $('globalLoading');
        const messageEl = loading?.querySelector('.quote-loading-message');
        const quoteNotification = window.QuoteNotification
            ? new window.QuoteNotification()
            : null;
        const showQuoteNotification = (message, type = 'danger') =>
            quoteNotification?.show(message, type);

        const normalizeImportedDate = value => {
            if (!value) return '';
            const text = String(value);
            const isoDate = text.match(/^\d{4}-\d{2}-\d{2}/)?.[0];
            if (isoDate) return isoDate;

            const vietnameseDate = text.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
            return vietnameseDate
                ? `${vietnameseDate[3]}-${vietnameseDate[2]}-${vietnameseDate[1]}`
                : text;
        };

        const getImportedField = (source, ...names) => {
            if (!source || typeof source !== 'object') return '';

            const normalizedNames = names.map(name => String(name)
                .replace(/[^a-zA-Z0-9]/g, '')
                .toLowerCase());
            const entry = Object.entries(source).find(([key]) => normalizedNames.includes(
                key.replace(/[^a-zA-Z0-9]/g, '').toLowerCase()
            ));

            return entry?.[1] ?? '';
        };

        const toBoolean = value => {
            if (typeof value === 'boolean') return value;
            return ['true', '1', 'yes', 'y', 'o', 'có', 'co'].includes(
                String(value ?? '').trim().toLowerCase()
            );
        };

        const mapImportedSupplier = supplier => ({
            code: getImportedField(supplier, 'MaNcc', 'CHR_MaNCC', 'Code', 'SupplierCode'),
            name: getImportedField(supplier, 'TenNcc', 'NVCHR_TenNCC', 'Name', 'SupplierName'),
            selected: toBoolean(getImportedField(supplier, 'BIT_LayBaoGia', 'Selected', 'LayBaoGia')),
            reason: getImportedField(supplier, 'NVCHR_LyDo', 'LyDo', 'Reason')
        });

        const mapImportedItem = item => {
            const vendors = getImportedField(item, 'Vendors', 'Suppliers');
            const importedVendors = Array.isArray(vendors) ? vendors : [];
            const singleVendorCode = getImportedField(item, 'CHR_MaNCC', 'MaNcc');

            return {
                internalCode: getImportedField(item, 'CHR_MaHangNoiBo', 'MaHangNoiBo', 'InternalCode'),
                category: getImportedField(item, 'NVCHR_ChungLoai', 'ChungLoai', 'Category'),
                classification: getImportedField(item, 'CHR_Phanloai', 'Phanloai', 'Classification'),
                vietnameseName: getImportedField(item, 'NVCHR_NameVN', 'NameVN', 'VietnameseName'),
                englishName: getImportedField(item, 'CHR_NameEN', 'NameEN', 'EnglishName'),
                supplierItemCode: getImportedField(item, 'CHR_MaHangNCC', 'MaHangNCC', 'SupplierItemCode'),
                quantity: getImportedField(item, 'INT_SoLuong', 'SoLuong', 'Quantity'),
                unit: getImportedField(item, 'NVCHR_DonVi', 'DonVi', 'Unit'),
                supplierDeadline: normalizeImportedDate(getImportedField(item, 'DTM_KyHan', 'KyHan', 'SupplierDeadline')),
                desiredDate: normalizeImportedDate(getImportedField(item, 'DTM_NgayMuonNhan', 'NgayMuonNhan', 'DesiredDate')),
                urgent: toBoolean(getImportedField(item, 'CHR_Gap', 'Gap', 'Urgent')),
                receiver: getImportedField(item, 'NVCHR_NguoiNhan', 'NguoiNhan', 'Receiver'),
                receiverPhone: getImportedField(item, 'CHR_SDT', 'SDT', 'ReceiverPhone'),
                deliveryLocation: getImportedField(item, 'NVCHR_DiaDiemNH', 'DiaDiemNH', 'DeliveryLocation'),
                equipmentCode: getImportedField(item, 'CHR_MaThietBi', 'MaThietBi', 'EquipmentCode'),
                reason: getImportedField(item, 'NVCHR_ReasonQuotation', 'ReasonQuotation', 'Reason'),
                shape: getImportedField(item, 'NVCHR_HinhDang', 'HinhDang', 'Shape'),
                material: getImportedField(item, 'NVCHR_ChatLieu', 'ChatLieu', 'Material'),
                composition: getImportedField(item, 'NVCHR_ThanhPhan', 'ThanhPhan', 'Composition'),
                dimension: getImportedField(item, 'NVCHR_KichThuoc', 'KichThuoc', 'Dimension'),
                usedFor: getImportedField(item, 'NVCHR_DongMay', 'DongMay', 'UsedFor'),
                purpose: getImportedField(item, 'NVCHR_TinhNang', 'TinhNang', 'Purpose'),
                rohs: getImportedField(item, 'NVCHR_Rohs', 'Rohs', 'ROHS'),
                cocq: getImportedField(item, 'NVCHR_COCQ', 'COCQ'),
                msds: getImportedField(item, 'NVCHR_MSDS', 'MSDS'),
                safety: getImportedField(item, 'NVCHR_AnToan', 'AnToan', 'Safety'),
                boxLink: getImportedField(item, 'Link_box', 'LinkBox', 'BoxLink'),
                suppliers: importedVendors.length
                    ? importedVendors.map(mapImportedSupplier)
                    : (singleVendorCode ? [mapImportedSupplier(item)] : [])
            };
        };

        const importExcelFile = async file => {
            if (!file) {
                throw new Error('Vui lòng chọn file Excel để import');
            }

            const formData = new FormData();
            formData.append('file', file);

            const response = await fetch(getApiUrl('/Quote/ImportExcel'), {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const contentType = response.headers.get('content-type') || '';
                if (contentType.includes('spreadsheetml') || contentType.includes('application/octet-stream')) {
                    const blob = await response.blob();
                    const objectUrl = window.URL.createObjectURL(blob);
                    const link = document.createElement('a');
                    link.href = objectUrl;
                    link.download = getDownloadFileName(response);
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                    window.URL.revokeObjectURL(objectUrl);
                    throw new Error('File có lỗi dữ liệu. File lỗi đã được tải xuống, vui lòng kiểm tra cột Thông tin lỗi.');
                }

                const message = await response.text().catch(() => response.statusText);
                throw new Error(message || response.statusText || 'Import file thất bại');
            }

            const importedItems = getResponseData(await response.json());
            if (!Array.isArray(importedItems) || !importedItems.length) {
                throw new Error('File không có dữ liệu hợp lệ');
            }

            quoteItems = importedItems.map(mapImportedItem);
            localStorage.setItem(quoteStorageKey, JSON.stringify(quoteItems));
            renderQuoteItems();
        };

        const buildExportPayload = () => {
            const section = $('searchPhongBan');
            const sectionOption = section?.selectedOptions?.[0];
            const sectionName = sectionOption?.textContent?.split(' - ').slice(1).join(' - ').trim() || '';

            return quoteItems.map(item => ({
                CHR_MaHangNoiBo: item.internalCode || null,
                CHR_MaHangNCC: item.supplierItemCode || null,
                CHR_MaThietBi: item.equipmentCode || null,
                CHR_NameEN: item.englishName || null,
                CHR_Phanloai: item.classification || null,
                CHR_SectionCode: valueOf('searchPhongBan') || null,
                CHR_SectionName: sectionName || null,
                CHR_Gap: item.urgent ? 'true' : 'false',
                DTM_KyHan: item.supplierDeadline || null,
                DTM_NgayMuonNhan: item.desiredDate || null,
                NVCHR_AnToan: item.safety || null,
                NVCHR_COCQ: item.cocq || null,
                NVCHR_ChatLieu: item.material || null,
                NVCHR_ChungLoai: item.category || null,
                NVCHR_DonVi: item.unit || null,
                NVCHR_DongMay: item.usedFor || null,
                NVCHR_HinhDang: item.shape || null,
                NVCHR_KichThuoc: item.dimension || null,
                NVCHR_MSDS: item.msds || null,
                NVCHR_NameVN: item.vietnameseName || null,
                NVCHR_Rohs: item.rohs || null,
                NVCHR_ThanhPhan: item.composition || null,
                NVCHR_TinhNang: item.purpose || null,
                NVCHR_UserRequest: valueOf('NguoiThaoTac') || null,
                INT_SoLuong: item.quantity ? Number(item.quantity) : null,
                NVCHR_ReasonQuotation: item.reason || null,
                NVCHR_DiaDiemNH: item.deliveryLocation || null,
                NVCHR_NguoiNhan: item.receiver || null,
                CHR_SDT: item.receiverPhone || null,
                Link_box: item.boxLink || null,
                Vendors: (item.suppliers || []).map(supplier => ({
                    MaNcc: supplier.code || null,
                    TenNcc: supplier.name || null,
                    BIT_LayBaoGia: Boolean(supplier.selected),
                    NVCHR_LyDo: supplier.reason || null
                }))
            }));
        };
        const buildInsertPayload = approval => quoteItems.map(item => ({
            CHR_Gap: item.urgent ? 'true' : 'false',
            CHR_MaHangNoiBo: item.internalCode || '',
            CHR_MaHangNCC: item.supplierItemCode || null,
            CHR_MaThietBi: item.equipmentCode || null,
            CHR_NameEN: item.englishName || null,
            CHR_Phanloai: item.classification || null,
            CHR_SectionCode: valueOf('searchPhongBan') || null,
            CHR_SectionName: getSectionName() || null,
            DTM_KyHan: item.supplierDeadline || null,
            DTM_NgayMuonNhan: item.desiredDate || null,
            NVCHR_AnToan: item.safety || null,
            NVCHR_COCQ: item.cocq || null,
            NVCHR_ChatLieu: item.material || null,
            NVCHR_ChungLoai: item.category || null,
            NVCHR_DonVi: item.unit || null,
            NVCHR_DongMay: item.usedFor || null,
            NVCHR_HinhDang: item.shape || null,
            NVCHR_KichThuoc: item.dimension || null,
            NVCHR_MSDS: item.msds || null,
            NVCHR_NameVN: item.vietnameseName || null,
            NVCHR_Rohs: item.rohs || null,
            NVCHR_ThanhPhan: item.composition || null,
            NVCHR_TinhNang: item.purpose || null,
            NVCHR_UserRequest: valueOf('NguoiThaoTac') || null,
            CHR_UserApproval: approval,
            INT_SoLuong: item.quantity ? Number(item.quantity) : null,
            NVCHR_ReasonQuotation: item.reason || null,
            NVCHR_DiaDiemNH: item.deliveryLocation || null,
            NVCHR_NguoiNhan: item.receiver || null,
            CHR_SDT: item.receiverPhone || null,
            Link_box: item.boxLink || null,
            Vendors: (item.suppliers || []).map(supplier => ({
                MaNcc: supplier.code || null,
                TenNcc: supplier.name || null,
                BIT_LayBaoGia: Boolean(supplier.selected),
                NVCHR_LyDo: supplier.reason || null
            })),
            WfSection: valueOf('selectSection') || null,
            WfType: valueOf('selectType') || null,
        }));
        const getDownloadFileName = response => {
            const contentDisposition = response.headers.get('content-disposition');
            const filenameMatch = contentDisposition?.match(/filename\*?=(?:UTF-8''|\")?([^;\"']+)/i);

            return filenameMatch?.[1]
                ? decodeURIComponent(filenameMatch[1].replace(/\"/g, '').trim())
                : 'TableQuote.xlsx';
        };

        const downloadExportedTable = async () => {
            if (!quoteItems.length) {
                throw new Error('Danh sách báo giá trống');
            }

            const response = await fetch(getApiUrl('/Quote/ExportExcel'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(buildExportPayload())
            });

            if (!response.ok) {
                const message = await response.text().catch(() => response.statusText);
                throw new Error(message || response.statusText || 'Xuất file thất bại');
            }

            const blob = await response.blob();
            const objectUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = objectUrl;
            link.download = getDownloadFileName(response);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(objectUrl);
        };

        window.showQuoteLoading = (message = 'Đang xử lý...') => {
            if (!loading) return;

            messageEl && (messageEl.textContent = message);
            renderQuoteItems();
            loading.style.display = 'flex';
            loading.setAttribute('aria-hidden', 'false');
        };

        window.hideQuoteLoading = () => {
            if (!loading) return;

            loading.style.display = 'none';
            loading.setAttribute('aria-hidden', 'true');

            if (messageEl) {
                messageEl.textContent = 'Đang xử lý...';
            }
        };

        $('btnDownloadExcel')?.addEventListener('click', () => {
            try {
                showQuoteLoading('Đang tải file mẫu...');

                const link = document.createElement('a');
                link.href = `${window.apiBaseUrl || ''}/template/TemplateQuationN.xlsx`;
                link.download = 'Mau_Quote.xlsx';

                document.body.appendChild(link);
                link.click();
                link.remove();
            } finally {
                setTimeout(hideQuoteLoading, 300);
            }
        });

        $('btnExportTablefile')?.addEventListener('click', async () => {
            try {
                showQuoteLoading('Đang xuất dữ liệu bảng...');
                await downloadExportedTable();
            } catch (error) {
                console.error('Error exporting quote table', error);
                showQuoteNotification(error.message || 'Không thể xuất dữ liệu bảng');
            } finally {
                hideQuoteLoading();
            }
        });
        $('btnImportfile')?.addEventListener('click', () => {
            const fileInput = document.createElement('input');
            fileInput.type = 'file';
            fileInput.accept = '.xlsx,.xls';
            fileInput.style.display = 'none';

            fileInput.addEventListener('change', async () => {
                const file = fileInput.files?.[0];
                fileInput.remove();

                if (!file) return;

                try {
                    showQuoteLoading('Đang nhập dữ liệu bảng...');
                    await importExcelFile(file);
                } catch (error) {
                    console.error('Error importing quote table', error);
                    showQuoteNotification(error.message || 'Không thể import dữ liệu Excel');
                } finally {
                    hideQuoteLoading();
                }
            }, { once: true });

            document.body.appendChild(fileInput);
            fileInput.click();
        });
        const getSectionName = () => {
            const option = $('searchPhongBan')?.selectedOptions?.[0];
            return option?.textContent?.split(' - ').slice(1).join(' - ').trim() || '';
        };

        const getApproverValue = approver => approver?.chR_UserAdid
            ?? approver?.chr_UserAdid
            ?? approver?.CHR_UserAdid
            ?? '';

        const getApproverText = approver => {
            const name = approver?.nvchR_UserName ?? approver?.NVCHR_UserName ?? '';
            const position = approver?.nvchR_Position ?? approver?.NVCHR_Position ?? '';
            const adid = getApproverValue(approver);
            return [name, position, adid].filter(Boolean).join(' - ') || adid;
        };

        const sendQuotation = async approval => {
            if (!approval) throw new Error('Vui lòng chọn người phê duyệt.');

            const response = await fetch(getApiUrl('/Quote/InsertQuotation'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(buildInsertPayload(approval))
            });
            await readJsonResponse(response);
            clearQuoteItems();
            renderQuoteItems();
        };

        $('btnSendQuotation')?.addEventListener('click', async () => {
            const sectionCode = valueOf('searchPhongBan');
            if (!sectionCode) {
                showQuoteNotification('Vui lòng chọn mã chi phí.');
                return;
            }
            if (!valueOf('selectSection')) {
                showQuoteNotification('Vui lòng chọn phòng ban xin báo giá.');
                return;
            }
            if (!valueOf('selectType')) {
                showQuoteNotification('Vui lòng chọn loại hàng.');
                return;
            }
            if (!quoteItems.length) {
                showQuoteNotification('Danh sách báo giá trống.');
                return;
            }

            const approverSelect = $('quoteApproverSelect');
            const approverModal = $('quoteApproverModal');
            try {
                showQuoteLoading('Đang tải danh sách người phê duyệt...');
                const response = await fetch(getApiUrl('/Quote/GetListApprovel'), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Step: 2, SectionCost: sectionCode })
                });
                const approvers = getResponseData(await readJsonResponse(response));
                approverSelect.innerHTML = '<option value="">Chọn người phê duyệt</option>';
                approvers.forEach(approver => {
                    const value = getApproverValue(approver);
                    if (value) approverSelect.add(new Option(getApproverText(approver), value));
                });
                if (approverSelect.options.length === 1) throw new Error('Không tìm thấy người phê duyệt phù hợp.');
                getModalController(approverModal)?.show();
            } catch (error) {
                showQuoteNotification(error.message || 'Không thể tải danh sách người phê duyệt.');
            } finally {
                hideQuoteLoading();
            }
        });

        $('btnConfirmSendQuotation')?.addEventListener('click', async () => {
            try {
                showQuoteLoading('Đang lưu yêu cầu báo giá...');
                await sendQuotation(valueOf('quoteApproverSelect'));
                closeQuoteModal($('quoteApproverModal'));
                showQuoteNotification('Đã gửi yêu cầu báo giá thành công.', 'success');
            } catch (error) {
                showQuoteNotification(error.message || 'Không thể lưu yêu cầu báo giá.');
            } finally {
                hideQuoteLoading();
            }
        });

        ['btnCloseQuoteApproverModal', 'btnCancelQuoteApproverModal']
            .forEach(id => $(id)?.addEventListener('click', () => closeQuoteModal($('quoteApproverModal'))));


        const masterButton = $('btnDownMaster');
        if (!masterButton) return;

        masterButton.addEventListener('click', async () => {
            const endpoints = [
                {
                    url: (window.apiBaseUrl || '') + '/Master/ExportExcelMasterVendor',
                    defaultName: 'ExportMasterVendor.xlsx'
                },
                {
                    url: (window.apiBaseUrl || '') + '/Master/ExportExcelMasterMaterial',
                    defaultName: 'ExportMasterMaterial.xlsx'
                }
            ];

            try {
                showQuoteLoading('Đang tải dữ liệu master...');

                for (const endpoint of endpoints) {
                    const response = await fetch(endpoint.url, { method: 'GET' });

                    if (!response.ok) {
                        const responseText = await response.text().catch(() => response.statusText);
                        throw new Error(responseText || response.statusText || 'Xuất file thất bại');
                    }

                    const blob = await response.blob();
                    let filename = endpoint.defaultName;
                    const contentDisposition = response.headers.get('content-disposition');

                    if (contentDisposition) {
                        const filenameMatch = contentDisposition.match(/filename\*?=(?:UTF-8''|\")?([^;\"']+)/i);
                        if (filenameMatch && filenameMatch[1]) {
                            filename = decodeURIComponent(filenameMatch[1].replace(/\"/g, '').trim());
                        }
                    }

                    const objectUrl = window.URL.createObjectURL(blob);
                    const link = document.createElement('a');
                    link.href = objectUrl;
                    link.download = filename;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                    window.URL.revokeObjectURL(objectUrl);
                }
            } catch (error) {
                console.error('Error downloading master data', error);
            } finally {
                hideQuoteLoading();
            }
        });
    };

    const initializeQuotePage = () => {
        try {
            window.KanziSearchableDropdown?.init?.(document);
        } catch (_) { }

        const modal = $('addInforModal');

        if (modal && modal.parentElement !== document.body) {
            document.body.appendChild(modal);
        }

        const modalNofication = $('quoteValidationModal');
        if (modalNofication && modalNofication.parentElement !== document.body) {
            document.body.appendChild(modalNofication);
        }

        const quoteNotifications = $('quoteNotifications');
        if (quoteNotifications && quoteNotifications.parentElement !== document.body) {
            document.body.appendChild(quoteNotifications);
        }

        const quoteApproverModal = $('quoteApproverModal');
        if (quoteApproverModal && quoteApproverModal.parentElement !== document.body) {
            document.body.appendChild(quoteApproverModal);
        }

        initializeQuoteModal();
        initializeQuoteLoading();

        ['tableSearchInternalCode', 'tableSearchCategory', 'tableSearchSupplierCode']
            .forEach(id => $(id)?.addEventListener('input', () => {
                currentPage = 1;
                renderQuoteItems();
            }));
        $('btnClearTableSearch')?.addEventListener('click', () => {
            ['tableSearchInternalCode', 'tableSearchCategory', 'tableSearchSupplierCode']
                .forEach(id => { if ($(id)) $(id).value = ''; });
            currentPage = 1;
            renderQuoteItems();
            $('tableSearchInternalCode')?.focus();
        });

        $('rowsPerPageSelect')?.addEventListener('change', event => {
            rowsPerPage = Number(event.target.value) || 20;
            currentPage = 1;
            renderQuoteItems();
        });

        $('prevPage')?.addEventListener('click', event => {
            event.preventDefault();
            if (currentPage <= 1) return;
            currentPage -= 1;
            renderQuoteItems();
        });

        $('nextPage')?.addEventListener('click', event => {
            event.preventDefault();
            const totalItems = getFilteredQuoteItems().items.length;
            const totalPages = Math.max(1, Math.ceil(totalItems / rowsPerPage));
            if (currentPage >= totalPages) return;
            currentPage += 1;
            renderQuoteItems();
        });
    };

    document.addEventListener('DOMContentLoaded', initializeQuotePage);
})();
