document.addEventListener('DOMContentLoaded', () => {
    // Elements
    const btnSearch = document.getElementById('btnSearch');
    const btnClear = document.getElementById('btnClear');
    const btnAddNew = document.getElementById('btnAddNew');
    const flowContainer = document.getElementById('flowContainer');
    const formContainer = document.getElementById('formContainer');
    const formTitle = document.getElementById('formTitle');
    const approverForm = document.getElementById('approverForm');
    const cancelFormBtn = document.getElementById('cancelForm');
    const totalCountElement = document.getElementById('totalCount');
    const approvalSteps = window.approvalSteps || [];
    const modalAdidEl = document.getElementById('modalAdid');
    const modalNameEl = document.getElementById('modalName');
    const modalPositionEl = document.getElementById('modalPosition');
    const modalSectionEl = document.getElementById('modalSectionCode');
    const modalLookupStatusEl = document.getElementById('modalLookupStatus');
    const ImportExcelBtn = document.getElementById('ImportExcelBtn');

    // Khởi tạo dropdown có tìm kiếm cho các select có class 'searchable-select'
    buildSearchableDropdown(document);
    // helper to get value by possible keys (case-insensitive) for dynamic objects
    function getVal(obj, keys) {
        if (!obj) return null;
        try {
            const lower = Object.keys(obj).reduce((acc, k) => { acc[k.toLowerCase()] = obj[k]; return acc; }, {});
            for (const k of keys) {
                const v = lower[k.toLowerCase()];
                if (v !== undefined && v !== null && String(v).trim() !== '') return String(v).trim();
            }
        } catch (e) {
            return null;
        }
        return null;
    }

    // Event Listeners
    if (btnSearch) btnSearch.addEventListener('click', handleSearch);
    if (btnClear) btnClear.addEventListener('click', handleClear);
    if (btnAddNew) btnAddNew.addEventListener('click', showAddForm);
    if (cancelFormBtn) cancelFormBtn.addEventListener('click', hideForm);
    if (approverForm) approverForm.addEventListener('submit', handleFormSubmit);
    if (ImportExcelBtn) ImportExcelBtn.addEventListener('click', ImportExcel);
    if (modalAdidEl) {
        // Only trigger lookup when user presses Enter and input is non-empty
        modalAdidEl.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                const curAdid = modalAdidEl.value.trim();
                if (curAdid) lookupEmployee(curAdid);
            }
        });
    }

    // Functions
    function handleSearch() {
        loadApprovers();
    }

    function handleClear() {
        document.getElementById('searchAdid').value = '';
        document.getElementById('searchSection').value = '';
        document.getElementById('searchStep').value = '';
        hideForm();
        showEmptyState();
    }
    async function ImportExcel() {
        // Tạo input file ẩn
        const fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.accept = '.xlsx, .xls';
        fileInput.style.display = 'none';
        document.body.appendChild(fileInput);

        fileInput.addEventListener('change', function () {
            const file = fileInput.files[0];
            if (!file) return;
            const T = window.i18nSendApprover || {};
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
            // Gửi request ImportSectionExcel
            try { showLoading((window.i18nSendApprover && window.i18nSendApprover.LoadingData) || 'Đang xử lý...'); } catch { }
            fetch((window.apiBaseUrl || '') + '/Master/UploadFileApprovelUser', {
                method: 'POST',
                body: formData
            })
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => { throw new Error(text || 'Lỗi server'); });
                    }
                    // Thành công
                    return response.json().then(data => {
                        showDialog({ title: T.Notification || 'Thông báo', message: (T.DataUpdatedSuccessfully || 'Nhập file thành công'), type: 'success' });
                    });
                })
                .catch(error => {
                    const T = window.i18nSendApprover || {};
                    showDialog({ title: T.Notification || 'Thông báo', message: (error && error.message) ? error.message : (T.ErrorPrefix || 'Không thể xuất file'), type: 'error' });
                })
                .finally(() => {
                    try { hideLoading(); } catch { }
                    document.body.removeChild(fileInput);
                });
        });

        // Trigger the file picker
        fileInput.click();
    }
    function showAddForm() {
        clearForm();
        const T = window.i18nSendApprover || {};
        formTitle.textContent = T.AddApproverTitle || 'Thêm người phê duyệt mới';
        formContainer.style.display = 'block';
        formContainer.scrollIntoView({ behavior: 'smooth' });

        // Pre-fill với giá trị từ bộ lọc nếu có
        const curSection = document.getElementById('searchSection').value;
        const curStep = document.getElementById('searchStep').value;
        if (curSection) document.getElementById('modalSectionCode').value = curSection;
        if (curStep) document.getElementById('modalStep').value = curStep;
    }

    function showEditForm(approver) {
        // Support dynamic property names from server response
        const idVal = getVal(approver, ['id', 'iD', 'ID']) || '0';
        const adidVal = getVal(approver, ['chr_useradid', 'chR_UserAdid', 'CHr_UserAdid', 'chr_userAdid', 'adid', 'chR_UserAdId']);
        const nameVal = getVal(approver, ['nvchr_username', 'nvchr_userName', 'nvchR_UserName', 'name', 'fullName', 'nvchr_name']);
        const posVal = getVal(approver, ['nvchr_position', 'nvchR_Position', 'position', 'chucdanh']);
        const secVal = getVal(approver, ['chr_code_section', 'chr_code_sec', 'chr_codeSection', 'chr_code', 'chr_code_sec']);
        const stepVal = getVal(approver, ['id_baogia_step', 'id_baogiaStep', 'id_baoGiaStep', 'ID_BaoGiaStep', 'id_baogia_step', 'iD_BaoGiaStep']);

        document.getElementById('approverId').value = idVal;
        document.getElementById('modalAdid').value = adidVal || '';
        document.getElementById('modalName').value = nameVal || '';
        document.getElementById('modalPosition').value = posVal || '';
        document.getElementById('modalSectionCode').value = secVal || '';
        document.getElementById('modalStep').value = stepVal || '';

        const T = window.i18nSendApprover || {};
        formTitle.textContent = T.EditApproverTitle || 'Sửa người phê duyệt';
        formContainer.style.display = 'block';
        formContainer.scrollIntoView({ behavior: 'smooth' });
        // If ADID present, try lookup to refresh data
        const curAdid = document.getElementById('modalAdid').value.trim();
        if (curAdid) lookupEmployee(curAdid);
    }

    function hideForm() {
        formContainer.style.display = 'none';
        clearForm();
    }

    function clearForm() {
        document.getElementById('approverId').value = '0';
        document.getElementById('modalAdid').value = '';
        document.getElementById('modalName').value = '';
        document.getElementById('modalPosition').value = '';
        document.getElementById('modalSectionCode').value = '';
        document.getElementById('modalStep').value = '';
        if (modalLookupStatusEl) modalLookupStatusEl.textContent = '';

        // Reset validation
        if (approverForm) {
            const inputs = approverForm.querySelectorAll('.form-control, .form-select');
            inputs.forEach(input => {
                input.classList.remove('is-invalid');
            });
        }
    }

    async function lookupEmployee(adidOrMnv) {
        if (!adidOrMnv) {
            if (modalLookupStatusEl) modalLookupStatusEl.textContent = '';
            return;
        }
        const T = window.i18nSendApprover || {};
        if (modalLookupStatusEl) modalLookupStatusEl.textContent = (T.LookupSearching || 'Đang tìm...');
        try {
            // Controller action is named GetEmployeeWorkingByIdAsync in code but MVC strips the "Async" suffix
            // so the correct route is /Master/GetEmployeeWorkingById
            const url = (window.apiBaseUrl || '') + '/Master/GetEmployeeWorkingById?adidOrMnv=' + encodeURIComponent(adidOrMnv);
            const resp = await fetch(url, { method: 'GET' });
            if (!resp.ok) {
                if (modalLookupStatusEl) modalLookupStatusEl.textContent = (T.LookupNotFound || 'Không tìm thấy');
                return;
            }
            // safe parse: protect against empty body
            const text = await resp.text();
            if (!text) {
                if (modalLookupStatusEl) modalLookupStatusEl.textContent = (T.LookupNotFound || 'Không tìm thấy');
                return;
            }
            const result = JSON.parse(text);
            if (!result || !result.success) {
                if (modalLookupStatusEl) modalLookupStatusEl.textContent = (T.LookupNotFound || 'Không tìm thấy');
                return;
            }
            const items = result.data || [];
            if (!items || items.length === 0) {
                if (modalLookupStatusEl) modalLookupStatusEl.textContent = (T.LookupNotFound || 'Không tìm thấy');
                return;
            }
            const first = items[0];
            // helper to get value by possible keys (case-insensitive)
            const getVal = (obj, keys) => {
                if (!obj) return null;
                const lower = Object.keys(obj).reduce((acc, k) => { acc[k.toLowerCase()] = obj[k]; return acc; }, {});
                for (const k of keys) {
                    const v = lower[k.toLowerCase()];
                    if (v !== undefined && v !== null && String(v).trim() !== '') return String(v).trim();
                }
                return null;
            };

            const name = getVal(first, ['CHR_EMPLOYEE_NAME']);
            const position = getVal(first, ['CHR_POSITION_GROUP']);
            const sectionCode = getVal(first, ['CHR_SEC_CODE']);
            const sectionName = getVal(first, ['CHR_SEC_NAME']);

            if (name && modalNameEl) modalNameEl.value = name;
            if (position && modalPositionEl) modalPositionEl.value = position;
            // chọn phòng (Comment vì có trường hợp khác phòng nhưng cũng duyệt)
            //if (sectionCode && modalSectionEl) {
            //    // try set by value
            //    const opt = modalSectionEl.querySelector(`option[value="${sectionCode}"]`);
            //    if (opt) {
            //        modalSectionEl.value = sectionCode;
            //    } else {
            //        // try match by option text
            //        let matched = null;
            //        Array.from(modalSectionEl.options).forEach(o => {
            //            if (!matched && o.text && o.text.trim().toLowerCase() === sectionCode.toLowerCase()) matched = o.value;
            //        });
            //        if (matched) modalSectionEl.value = matched;
            //    }
            //} else if (sectionName && modalSectionEl) {
            //    // try match by option text
            //    let matched = null;
            //    Array.from(modalSectionEl.options).forEach(o => {
            //        if (!matched && o.text && o.text.trim().toLowerCase() === sectionName.toLowerCase()) matched = o.value;
            //    });
            //    if (matched) modalSectionEl.value = matched;
            //}

            if (modalLookupStatusEl) modalLookupStatusEl.textContent = (T.LookupFound || 'Tìm thấy: {0}{1}').replace('{0}', (name || adidOrMnv)).replace('{1}', (position ? ' - ' + position : ''));
        } catch (err) {
            console.error('Lookup employee error', err);
            const T = window.i18nSendApprover || {};
            if (modalLookupStatusEl) modalLookupStatusEl.textContent = (T.ErrorLoading || 'Lỗi khi tìm');
        }
    }

    async function loadApprovers() {
        const adid = document.getElementById('searchAdid').value.trim();
        const section = document.getElementById('searchSection').value;
        const step = document.getElementById('searchStep').value;

        try {
            const resp = await fetch((window.apiBaseUrl || '') + '/Master/GetApprovers', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    SectionCode: section,
                    Adid: adid,
                    IdStep: step ? parseInt(step) : null,
                    PageIndex: 1,
                    PageSize: 1000
                })
            });

            const result = await resp.json();
            if (!result.success) {
                const T = window.i18nSendApprover || {};
                showError((T.ErrorLoading || 'Không thể tải dữ liệu') + ': ' + (result.message || 'Lỗi không xác định'));
                return;
            }

            renderFlow(result.data || []);
            updateTotalCount(result.data?.length || 0);
        } catch (error) {
            console.error('Error loading approvers:', error);
            const T = window.i18nSendApprover || {};
            showError(T.ErrorConnection || 'Lỗi kết nối đến server');
        }
    }

    function renderFlow(items) {
        flowContainer.innerHTML = '';

        if (approvalSteps && approvalSteps.length) {
            // Tạo map stepId => approvers
            const map = new Map();
            approvalSteps.forEach(st => map.set(st.INT_StepNumber ?? st.inT_StepNumber, []));

            (items || []).forEach(item => {
                const sid = item.iD_BaoGiaStep;
                if (!map.has(sid)) map.set(sid, []);
                map.get(sid).push(item);
            });

            // Render theo thứ tự steps đã sort
            approvalSteps.forEach(st => {
                const stepGroup = {
                    stepId: st.INT_StepNumber ?? st.inT_StepNumber,
                    stepName: st.CHR_StepName ?? st.name,
                    approvers: map.get(st.INT_StepNumber ?? st.inT_StepNumber) || []
                };
                const stepCard = createStepCard(stepGroup);
                stepCard.classList.add('step-card');
                flowContainer.appendChild(stepCard);
            });

            const total = (items || []).length;
            if (total === 0) updateTotalCount(0);
            return;
        }

        // Fallback: không có -> gom theo dữ liệu trả về như cũ
        if (!items || items.length === 0) {
            showEmptyState();
            return;
        }

        const groupedByStep = items.reduce((groups, item) => {
            const stepId = item.ID_BaoGiaStep;
            if (!groups[stepId]) {
                groups[stepId] = {
                    stepId: stepId,
                    stepName: item.NVCHR_StepName ?? item.nvchr_stepName,
                    approvers: []
                };
            }
            groups[stepId].approvers.push(item);
            return groups;
        }, {});

        const sortedSteps = Object.values(groupedByStep).sort((a, b) => a.stepId - b.stepId);
        sortedSteps.forEach(stepGroup => {
            const stepCard = createStepCard(stepGroup);
            stepCard.classList.add('step-card');
            flowContainer.appendChild(stepCard);
        });
    }

    function createStepCard(stepGroup) {
        const card = document.createElement('div');
        card.className = 'card mb-3 border-0 shadow-sm';

        // Header
        const header = document.createElement('div');
        header.className = 'card-header bg-light text-dark d-flex justify-content-between align-items-center';
        const T = window.i18nSendApprover || {};
        header.innerHTML = `
            <div>
                <i class="fas fa-step-forward me-2"></i>
                <strong>${stepGroup.stepName}</strong>
                <span class="badge bg-white text-secondary border ms-2">${(T.TotalApproverBadge || '{0} người').replace('{0}', stepGroup.approvers.length)}</span>
            </div>
            <button class="btn btn-sm btn-outline-primary btn-add-to-step" data-step="${stepGroup.stepId}">
                <i class="fas fa-user-plus me-1"></i> ${T.AddToThisStep || 'Thêm vào bước này'}
            </button>
        `;
        card.appendChild(header);

        // Body với list
        const body = document.createElement('div');
        body.className = 'card-body p-0';

        const list = document.createElement('div');
        list.className = 'list-group list-group-flush';

        stepGroup.approvers.forEach(approver => {
            const listItem = createApproverListItem(approver);
            list.appendChild(listItem);
        });

        body.appendChild(list);
        card.appendChild(body);

        // Thêm event listener cho nút "Thêm vào bước này" (guard nếu không tồn tại)
        const addBtn = card.querySelector('.btn-add-to-step');
        if (addBtn) {
            addBtn.addEventListener('click', (e) => {
                clearForm();
                if (formTitle) formTitle.textContent = 'Thêm người phê duyệt mới';
                const modalStepEl = document.getElementById('modalStep');
                if (modalStepEl) modalStepEl.value = stepGroup.stepId;
                if (formContainer) {
                    formContainer.style.display = 'block';
                    formContainer.scrollIntoView({ behavior: 'smooth' });
                }
            });
        }

        return card;
    }

    function createApproverListItem(approver) {
        const T = window.i18nSendApprover || {};
        const item = document.createElement('div');
        item.className = 'list-group-item d-flex justify-content-between align-items-start';
        item.innerHTML = `
            <div class="flex-grow-1">
                <div class="fw-bold"> ${approver.nvchR_UserName || (T.NoName || 'Không có tên')}</div>
                <div class="small text-muted">
                    <i class="fas fa-id-card me-1"></i> ${approver.chR_UserAdid || 'N/A'}
                    <span class="mx-2">•</span>
                    <i class="fas fa-briefcase me-1"></i> ${approver.nvchR_Position || (T.NoPosition || 'Không có chức danh')}
                    <span class="mx-2">•</span>
                    <i class="fas fa-building me-1"></i> ${approver.chR_NameSection || (T.NoSection || 'Không có phòng')}
                </div>
            </div>
        `;

        const actions = document.createElement('div');
        actions.className = 'btn-group btn-group-sm';
        actions.innerHTML = `
            <button class="btn btn-outline-primary btn-edit" title="${T.Edit || 'Sửa'}">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-outline-danger btn-delete" title="${T.Delete || 'Xóa'}">
                <i class="fas fa-trash"></i>
            </button>
        `;

        // Thêm event listeners (guard nếu button không tồn tại)
        const editBtn = actions.querySelector('.btn-edit');
        const deleteBtn = actions.querySelector('.btn-delete');
        if (editBtn) editBtn.addEventListener('click', () => showEditForm(approver));
        if (deleteBtn) deleteBtn.addEventListener('click', () => deleteApprover(approver.id));

        item.appendChild(actions);
        return item;
    }

    function updateTotalCount(count) {
        const T = window.i18nSendApprover || {};
        totalCountElement.textContent = (T.TotalApprovers || '{0} người phê duyệt').replace('{0}', count);
    }

    function showEmptyState() {
        const T = window.i18nSendApprover || {};
        flowContainer.innerHTML = `
            <div class="text-center text-muted py-5">
                <i class="fas fa-inbox fa-3x mb-3"></i>
                <h5>${T.EmptyTitle || 'Không có dữ liệu'}</h5>
                <p>${T.EmptyMessage || 'Không tìm thấy người phê duyệt nào phù hợp với tiêu chí tìm kiếm'}</p>
            </div>
        `;
        updateTotalCount(0);
    }

    function showError(message) {
        const T = window.i18nSendApprover || {};
        flowContainer.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
            </div>
        `;
        updateTotalCount(0);
    }
    // thêm  người phê duyệt
    async function handleFormSubmit(e) {
        e.preventDefault();

        // Validate form
        if (!validateForm()) {
            return;
        }

        const approverId = parseInt(document.getElementById('approverId').value);
        const approverData = {
            ID: approverId,
            CHR_UserAdid: document.getElementById('modalAdid').value.trim(),
            NVCHR_UserName: document.getElementById('modalName').value.trim(),
            NVCHR_Position: document.getElementById('modalPosition').value.trim(),
            CHR_CodeSection: document.getElementById('modalSectionCode').value,
            CHR_NameSection: document.getElementById('modalSectionCode').options[document.getElementById('modalSectionCode').selectedIndex].text,
            NVCHR_StepName: approvalSteps.find(st => (st.INT_StepNumber ?? st.inT_StepNumber) === parseInt(document.getElementById('modalStep').value))?.CHR_StepName || '',
            CHR_Status: 'ON',
            ID_BaoGiaStep: parseInt(document.getElementById('modalStep').value)
        };

        try {
            const endpoint = approverId > 0 ? (window.apiBaseUrl || '') + '/Master/UpdateApprover' : (window.apiBaseUrl || '') + '/Master/SaveApprover';
            const method = 'POST';

            const resp = await fetch(endpoint, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(approverData)
            });

            const result = await resp.json();
            if (result.success) {
                hideForm();
                loadApprovers();
                const T = window.i18nSendApprover || {};
                showToast('success', T.UpdateSuccess || 'Cập nhật thành công!');
            } else {
                const T = window.i18nSendApprover || {};
                showToast('error', result.message || T.GenericError || 'Có lỗi xảy ra');
            }
        } catch (error) {
            console.error('Error saving approver:', error);
            const T = window.i18nSendApprover || {};
            showToast('error', T.ErrorConnection || 'Lỗi kết nối đến server');
        }
    }

    function validateForm() {
        let isValid = true;
        const requiredFields = ['modalAdid', 'modalName', 'modalSectionCode', 'modalStep'];

        requiredFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (!field.value.trim()) {
                field.classList.add('is-invalid');
                isValid = false;
            } else {
                field.classList.remove('is-invalid');
            }
        });

        return isValid;
    }

    async function deleteApprover(id) {
        const T = window.i18nSendApprover || {};
        const confirmed = await confirmDialog({
            title: T.ConfirmDeleteTitle || 'Xác nhận xóa',
            message: T.ConfirmDeleteMessage || 'Bạn có chắc chắn muốn xóa người phê duyệt này không?',
            confirmText: T.ConfirmDeleteConfirm || 'Xóa',
            cancelText: T.ConfirmDeleteCancel || 'Hủy',
            confirmType: 'danger'
        });
        if (!confirmed) return;

        try {
            const resp = await fetch((window.apiBaseUrl || '') + '/Master/DeleteApprover', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    id: id
                })
            });
            const result = await resp.json();
            if (result.success) {
                loadApprovers();
                showDialog({ title: (T.DeleteSuccessTitle || 'Thành công'), message: (T.DeleteSuccessMessage || 'Xóa thành công!'), type: 'success' });
            } else {
                showDialog({ title: (T.DeleteErrorTitle || 'Lỗi'), message: result.message || (T.DeleteErrorMessage || 'Không thể xóa'), type: 'error' });
            }
        } catch (error) {
            console.error('Error deleting approver:', error);
            const T = window.i18nSendApprover || {};
            showDialog({ title: (T.DeleteErrorTitle || 'Lỗi'), message: (T.ErrorConnection || 'Lỗi kết nối đến server'), type: 'error' });
        }
    }

    // Custom dialog helpers
    function getDialogEls() {
        const overlay = document.getElementById('cmDialogOverlay');
        const titleEl = document.getElementById('cmDialogTitle');
        const bodyEl = document.getElementById('cmDialogBody');
        const footerEl = document.getElementById('cmDialogFooter');
        return { overlay, titleEl, bodyEl, footerEl };
    }

    function showDialog({ title = (window.i18nSendApprover && window.i18nSendApprover.Notification) || 'Thông báo', message = '', type = 'info', buttons } = {}) {
        const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
        if (!overlay) return alert(message);
        titleEl.textContent = title;
        bodyEl.innerHTML = `<div class="d-flex align-items-start gap-2">
            <i class="fas ${type === 'success' ? 'fa-check-circle text-success' : type === 'error' ? 'fa-exclamation-circle text-danger' : 'fa-info-circle text-primary'}"></i>
            <div>${message}</div>
        </div>`;
        footerEl.innerHTML = '';
        const okBtn = document.createElement('button');
        okBtn.className = 'cm-btn cm-btn-primary';
        const T = window.i18nSendApprover || {};
        okBtn.textContent = (buttons && buttons.okText) || (T.DialogOk || 'Đồng ý');
        okBtn.addEventListener('click', () => hideDialog());
        footerEl.appendChild(okBtn);
        overlay.style.display = 'flex';
        attachDialogCloseHandlers();
    }

    function hideDialog() {
        const { overlay } = getDialogEls();
        if (overlay) overlay.style.display = 'none';
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

    function confirmDialog({ title = 'Xác nhận', message = '', confirmText = 'OK', cancelText = 'Hủy', confirmType = 'primary' } = {}) {
        return new Promise(resolve => {
            const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
            if (!overlay) return resolve(confirm(message));
            titleEl.textContent = title;
            bodyEl.textContent = message;
            footerEl.innerHTML = '';
            const cancelBtn = document.createElement('button');
            cancelBtn.className = 'cm-btn cm-btn-outline';
            cancelBtn.textContent = cancelText;
            cancelBtn.addEventListener('click', () => { hideDialog(); if (typeof window.__cmPendingResolve === 'function') { const r = window.__cmPendingResolve; window.__cmPendingResolve = null; r(false); } else { resolve(false); } });
            const confirmBtn = document.createElement('button');
            confirmBtn.className = `cm-btn ${confirmType === 'danger' ? 'cm-btn-danger' : 'cm-btn-primary'}`;
            confirmBtn.textContent = confirmText;
            confirmBtn.addEventListener('click', () => { hideDialog(); if (typeof window.__cmPendingResolve === 'function') { const r = window.__cmPendingResolve; window.__cmPendingResolve = null; r(true); } else { resolve(true); } });
            footerEl.appendChild(cancelBtn);
            footerEl.appendChild(confirmBtn);
            // expose resolver so overlay/close handlers can resolve when clicked
            window.__cmPendingResolve = resolve;
            overlay.style.display = 'flex';
            attachDialogCloseHandlers();
        });
    }

    // Lightweight toast helper (fallback if global showToast is not provided)
    function showToast(type, message, timeout = 3000) {
        try {
            let container = document.getElementById('cmToastContainer');
            if (!container) {
                container = document.createElement('div');
                container.id = 'cmToastContainer';
                container.style.position = 'fixed';
                container.style.top = '1rem';
                container.style.right = '1rem';
                container.style.zIndex = '2000';
                container.style.display = 'flex';
                container.style.flexDirection = 'column';
                container.style.gap = '0.5rem';
                document.body.appendChild(container);
            }

            const toast = document.createElement('div');
            toast.className = 'cm-toast';
            toast.style.minWidth = '200px';
            toast.style.maxWidth = '320px';
            toast.style.padding = '0.75rem 1rem';
            toast.style.borderRadius = '0.375rem';
            toast.style.boxShadow = '0 2px 6px rgba(0,0,0,0.15)';
            toast.style.color = '#fff';
            toast.style.fontSize = '0.95rem';
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 200ms ease, transform 200ms ease';
            toast.style.transform = 'translateY(-6px)';

            if (type === 'success') {
                toast.style.background = '#198754';
            } else if (type === 'error' || type === 'danger') {
                toast.style.background = '#dc3545';
            } else if (type === 'warning') {
                toast.style.background = '#ffc107';
                toast.style.color = '#000';
            } else {
                toast.style.background = '#0d6efd';
            }

            toast.textContent = message || '';
            container.appendChild(toast);

            // force reflow then show
            void toast.offsetWidth;
            toast.style.opacity = '1';
            toast.style.transform = 'translateY(0)';

            const remove = () => {
                toast.style.opacity = '0';
                toast.style.transform = 'translateY(-6px)';
                setTimeout(() => { try { container.removeChild(toast); } catch (e) { } }, 220);
            };

            const tId = setTimeout(remove, timeout);
            // allow click to dismiss early
            toast.addEventListener('click', () => {
                clearTimeout(tId);
                remove();
            });
        } catch (err) {
            // fallback to alert if something goes wrong
            try { console.error(err); alert(message); } catch (e) { }
        }
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
    // Initialize
    hideForm();
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
            const T = window.i18nQuotationResults || {};
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
                    const empty = document.createElement('div'); empty.className = 'ms-empty'; empty.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.NoResults) || 'Không có kết quả';
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
                    placeholderEl.textContent = (window.i18nQuotationResults && window.i18nQuotationResults.SelectPlaceholder) || '-- Chọn --';
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
});
