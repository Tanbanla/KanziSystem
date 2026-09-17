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
    let quoteItems = JSON.parse(localStorage.getItem(quoteStorageKey) || '[]');
    if (!Array.isArray(quoteItems)) quoteItems = [];

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

    const renderQuoteItems = () => {
        const body = $('InforTableBody');
        if (!body) return;
        if (!quoteItems.length) {
            body.innerHTML = '<tr><td colspan="14" class="text-center text-muted py-4">Không có dữ liệu</td></tr>';
            return;
        }
        body.innerHTML = quoteItems.map((item, index) => `<tr>
            <td class="text-center">${index + 1}</td>
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
            <td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger btn-delete-quote" data-index="${index}"><i class="fas fa-trash"></i></button></td>
        </tr>`).join('');
    };

    const validateAndCollect = () => {
        const errors = [];
        ['selectPhanLoai', 'selectChungLoai', 'selectTenVN', 'selectTenEN', 'selectSoLuong', 'selectDonVi', 'NgayMuonNhan', 'LuaChonNcc']
            .forEach((id, index) => required(id, ['Phân loại hàng', 'Chủng loại hàng', 'Tên tiếng Việt', 'Tên tiếng Anh', 'Số lượng', 'Đơn vị', 'Ngày muốn nhận hàng', 'Hạn chọn nhà cung cấp'][index], errors));
        const suppliers = Array.from(document.querySelectorAll('#supplierTableBody tr')).filter(row => row.querySelector('.supplier-quote-checkbox')).map(row => ({
            code: row.querySelector('input[name^="supplierCode_"]')?.value || '',
            name: row.cells[2]?.textContent.trim() || '',
            selected: row.querySelector('.supplier-quote-checkbox').checked,
            reason: row.querySelector('.supplier-reason')?.value.trim() || ''
        }));
        if (suppliers.filter(s => s.selected).length > 5) errors.push('Chỉ được chọn tối đa 5 nhà cung cấp.');
        suppliers.filter(s => !s.selected).forEach(s => {
            const row = Array.from(document.querySelectorAll('#supplierTableBody tr'))
                .find(candidate => candidate.querySelector('input[name^="supplierCode_"]')?.value === s.code);
            const reason = row?.querySelector('.supplier-reason');
            setInvalid(reason, !s.reason);
            if (!s.reason) errors.push(`Vui lòng nhập lý do không xin báo giá cho NCC ${s.name || s.code}.`);
        });
        const materialCode = $('searchMahangNB');
        const nccCode = $('selectMaHangNCC');
        if (!valueOf('searchMahangNB') && !valueOf('selectMaHangNCC')) {
            setInvalid(materialCode, true);
            setInvalid(nccCode, true);
            errors.push('Mã hàng NCC bắt buộc khi chưa có Mã hàng nội bộ.');
        } else {
            setInvalid(materialCode, false);
            setInvalid(nccCode, false);
        }
        if (errors.length) return { errors };
        return { item: { internalCode: valueOf('searchMahangNB'), category: valueOf('selectChungLoai'), classification: valueOf('selectPhanLoai'), vietnameseName: valueOf('selectTenVN'), englishName: valueOf('selectTenEN'), supplierItemCode: valueOf('selectMaHangNCC'), quantity: valueOf('selectSoLuong'), unit: valueOf('selectDonVi'), supplierDeadline: valueOf('LuaChonNcc'), desiredDate: valueOf('NgayMuonNhan'), urgent: valueOf('selectGap') === 'true', suppliers } };
    };

    const setFieldValue = (id, value) => {
        const field = $(id);
        if (field) field.value = value ?? '';
        if (field?.tagName === 'SELECT') {
            field.dispatchEvent(new Event('change', { bubbles: true }));
        }
    };

    const fillMaterialFields = material => {
        setFieldValue('selectTenVN', material?.Material_Name_VN ?? material?.material_Name_VN);
        setFieldValue('selectTenEN', material?.Material_Name_EN ?? material?.material_Name_EN);
        setFieldValue('selectDonVi', material?.Unit ?? material?.unit);
        setFieldValue('selectPhanLoai', material?.GoodKind ?? material?.goodKind ?? material?.LoaiHang ?? material?.loaiHang);
        setFieldValue('selectChungLoai', material?.Category_VN ?? material?.category_VN);
        setFieldValue('selectHinhDang', material?.Shape ?? material?.shape);
        setFieldValue('selectChatLieu', material?.Material ?? material?.material);
        setFieldValue('selectThanhPhan', material?.Composition ?? material?.composition);
        setFieldValue('selectKichThuoc', material?.Dimension ?? material?.dimension);
        setFieldValue('selectDungcho', material?.UsedFor ?? material?.usedFor);
        setFieldValue('selectDungde', material?.Purpose ?? material?.purpose);
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
        if (!category) return;

        try {
            const response = await fetch(getApiUrl('/Quote/GetNCCByCategory'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(category)
            });
            renderSuppliers(getResponseData(await readJsonResponse(response)));
        } catch (error) {
            console.error('Không thể tải danh sách nhà cung cấp', error);
            const body = $('supplierTableBody');
            if (body) body.innerHTML = '<tr><td colspan="6" class="text-center text-danger py-4">Không thể tải danh sách nhà cung cấp</td></tr>';
        }
    };

    const initializeQuoteModal = () => {
        const btnAdd = $('btnAdd');
        const modal = $('addInforModal');
        const form = $('addInforForm');

        if (!btnAdd || !modal) return;

        $('searchMahangNB')?.addEventListener('change', event => loadMaterial(event.target.value));
        $('selectChungLoai')?.addEventListener('change', event => loadSuppliers(event.target.value));

        btnAdd.addEventListener('click', () => {
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

        $('btnSaveHistoryEdit')?.addEventListener('click', () => {
            const result = validateAndCollect();
            if (result.errors) {
                document.querySelector('#addInforModal .is-invalid')?.focus();
                return;
            }
            quoteItems.push(result.item);
            localStorage.setItem(quoteStorageKey, JSON.stringify(quoteItems));
            renderQuoteItems();
            clearQuoteForm(form);
            closeQuoteModal(modal);
        });
    };

    const initializeQuoteLoading = () => {
        const loading = $('globalLoading');
        const messageEl = loading?.querySelector('.quote-loading-message');

        window.showQuoteLoading = (message = 'Đang xử lý...') => {
            if (!loading) return;

            messageEl && (messageEl.textContent = message);
            renderQuoteItems();
            $('InforTableBody')?.addEventListener('click', event => {
                const button = event.target.closest('.btn-delete-quote');
                if (!button) return;
                quoteItems.splice(Number(button.dataset.index), 1);
                localStorage.setItem(quoteStorageKey, JSON.stringify(quoteItems));
                renderQuoteItems();
            });
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
                link.href = `${window.apiBaseUrl || ''}/template/TemPlateQuote.xlsx`;
                link.download = 'Mau_Quote.xlsx';

                document.body.appendChild(link);
                link.click();
                link.remove();
            } finally {
                setTimeout(hideQuoteLoading, 300);
            }
        });

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

        initializeQuoteModal();
        initializeQuoteLoading();
    };

    document.addEventListener('DOMContentLoaded', initializeQuotePage);
})();
