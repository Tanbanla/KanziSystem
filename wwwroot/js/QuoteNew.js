((w) => {
    "use strict";

    const app = {
        // Configuration and State
        config: {
            api: {
                insertListBaoGia: `${w.apiBaseUrl || ''}/Quote/InsertDanhSachBaoGia`,
                getMaterials: (keyword) => `${w.apiBaseUrl || ''}/Quote/GetMaterialsByNameOrCode?keyword=${encodeURIComponent(keyword || '')}`,
                searchMaterials: `${w.apiBaseUrl || ''}/Quote/GetSearchMaterial`,
                getSuppliersByMaHang: `${w.apiBaseUrl || ''}/Quote/GetNhaCungCapByMaHang`,
                uploadQuoteExcel: `${w.apiBaseUrl || ''}/Quote/UploadQuoteExcel`,
                exportAutoRender: `${w.apiBaseUrl || ''}/Quote/ExportAutoRender`,
                getNCCByCategory: `${w.apiBaseUrl || ''}/Quote/GetNCCByCategory`,
                exportRenderOutSide: `${w.apiBaseUrl || ''}/Quote/ExportRenderOutSide`,
                exportTable: `${w.apiBaseUrl || ''}/Quote/ExportTable`,
                searchApprover: `${w.apiBaseUrl || ''}/Quote/GetListApprovel`,
                downloadMasterMaterial: `${w.apiBaseUrl || ''}/Master/ExportExcelMasterMaterial`,
                downloadMasterVendor: `${w.apiBaseUrl || ''}/Master/ExportExcelMasterVendor`,
                templateUrl: `${w.apiBaseUrl || ''}/template/TemPlateQuote.xlsx`,
                checkNCC: `${w.apiBaseUrl || ''}/Quote/CheckNCC`
            },
            selectors: {
                container: '#quote-request',
                tableBody: '#quoteTableBody',
                form: '#quoteForm',
                approverSelect: '#approverSelect',
                globalLoading: '#globalLoading',
                paginationControls: '#paginationControls',
                pageInfo: '#pageInfo',
                pageNumberInfo: '#pageNumberInfo',
                paginationInfo: '#paginationInfo',
                prevPage: '#prevPage',
                nextPage: '#nextPage',
                rowsPerPageSelect: '#rowsPerPageSelect',
                filterInputs: '.filter-input',
                // Buttons
                btnAddRow: '#btnAddRow',
                btnReset: '#btnReset',
                btnCreate: '#btnCreate',
                btnAuto: '#btnAuto',
                btnDownExcelTable: '#btnDownExcelTable',
                btnClearFilters: '#btnClearFilters',
                btnUploadExcel: '#btnUploadExcel',
                excelUpload: '#excelUpload',
                btnDownloadExcel: '#btnDownloadExcel',
                btnDownMaster: '#btnDownMaster',
                // Row specific
                row: 'tr',
                removeRowButton: '.btn-remove-row',
                sectionSelect: '.tenPhongBanTb',
                categorySelect: '.chungLoaiTb',
                materialSelect: '.maHangNoiBo',
                supplierSelect: '.nhaCungCapTb',
                getQuoteSelect: '.laybaogiaTb',
                urgentSelect: '.gapTb',
                rohsSelect: '.rohsTb',
                reasonInput: 'input[id^="lyDo_"]',
                // Dialogs
                dialogOverlay: '#cmDialogOverlay',
                dialogTitle: '#cmDialogTitle',
                dialogBody: '#cmDialogBody',
                dialogFooter: '#cmDialogFooter',
                dialogClose: '[data-cm-action="close"]',
                dialogBackdrop: '.cm-dialog-backdrop',
                // Auto-render modal
                arModalOverlay: '#arModalOverlay',
                arCloseBtn: '#arCloseBtn',
                arCancelBtn: '#arCancelBtn',
                arExportBtn: '#arExportBtn',
                arError: '#arError',
                arTabHasCode: '#arTabHasCode',
                arTabNoCode: '#arTabNoCode',
                arTabHasCodeBody: '#arTabHasCodeBody',
                arTabNoCodeBody: '#arTabNoCodeBody',
                arSection: '#arSection',
                arSectionName: '#arSectionName',
                arMaterialList: '#arMaterialList',
                arSearch: '#arSearch',
                arSelectAll: '#arSelectAll',
                arSection2: '#arSection2',
                arSectionName2: '#arSectionName2',
                arCategoryList: '#arCategoryList',
                arSearch2: '#arSearch2',
                arSelectAll2: '#arSelectAll2',
            },
            i18n: w.i18nQuote || {},
            userData: w.indexQuoteData || {}
        },

        state: {
            currentPage: 1,
            rowsPerPage: 5,
            allQuoteItems: [],
            filteredQuoteItems: [],
            filteredDOMRows: [],
            firstSectionName: '',
            isSubmitting: false,
        },

        // DOM element cache
        elements: {},

        // Initialization
        init() {
            if (!document.querySelector(this.config.selectors.container)) return;
            this.cacheElements();
            this.wireEvents();
            this.updatePaginationState();
            this.applyFiltersAndPagination();
            this.initSearchableDropdowns(this.elements.container);
            this.renumberRows();
        },

        cacheElements() {
            const { selectors } = this.config;
            for (const key in selectors) {
                const sel = selectors[key];
                if (!sel) continue;
                try {
                    const s = sel.toString().trim();
 
                    if (s.startsWith('#')) {
                        this.elements[key] = document.querySelector(s);
                    } else {

                        const nodes = Array.from(document.querySelectorAll(s));
                        if (nodes.length === 1) this.elements[key] = nodes[0];
                        else this.elements[key] = nodes; 
                    }
                } catch (e) {
                    try { this.elements[key] = document.querySelector(sel); } catch (ex) { this.elements[key] = null; }
                }
            }
        },

        // Event Wiring
        wireEvents() {
            const { elements: els, config: { selectors } } = this;

            // Main container for delegation
            els.container.addEventListener('click', this.handleContainerClick.bind(this));
            els.container.addEventListener('change', this.handleContainerChange.bind(this));
            els.container.addEventListener('input', this.handleContainerInput.bind(this));

            // Focusin for capturing previous value
            els.tableBody?.addEventListener('focusin', (e) => {
                if (e.target.matches(selectors.sectionSelect)) {
                    e.target.dataset.prev = e.target.value || '';
                }
            }, true);

            // Document-level click for closing dropdowns
            document.addEventListener('click', this.handleDocumentClick.bind(this));
        },

        handleContainerClick(e) {
            const { selectors } = this.config;
            const target = e.target;

            const buttonActions = {
                [selectors.btnAddRow]: this.addRow,
                [selectors.btnReset]: this.resetForm,
                [selectors.btnCreate]: this.submitForm,
                [selectors.btnAuto]: this.exportAutoRender,
                [selectors.btnDownExcelTable]: this.exportTable,
                [selectors.btnClearFilters]: this.clearFilters,
                [selectors.btnUploadExcel]: () => this.elements.excelUpload?.click(),
                [selectors.btnDownloadExcel]: this.downloadTemplate,
                [selectors.btnDownMaster]: this.downloadMasterData,
                [selectors.prevPage]: () => this.changePage(-1),
                [selectors.nextPage]: () => this.changePage(1),
            };

            for (const selector in buttonActions) {
                if (target.closest(selector)) {
                    e.preventDefault();
                    buttonActions[selector].call(this);
                    return;
                }
            }

            if (target.closest(selectors.removeRowButton)) {
                this.removeRow(target.closest(selectors.removeRowButton));
            }
        },

        handleContainerChange(e) {
            const { selectors } = this.config;
            const target = e.target;

            if (target.matches(selectors.excelUpload)) {
                this.handleExcelUpload(e);
            } else if (target.matches(selectors.rowsPerPageSelect)) {
                this.updatePaginationState();
                this.applyFiltersAndPagination();
            } else if (target.matches(selectors.sectionSelect)) {
                this.handleSectionChange(target);
            } else if (target.matches(selectors.materialSelect)) {
                this.autofillFromMaterialSelect(target);
            } else if (target.matches(selectors.categorySelect)) {
                this.handleCategoryChange(target);
            } else if (target.matches(selectors.supplierSelect)) {
                this.handleSupplierChange(target);
            }
        },

        handleContainerInput(e) {
            const { selectors } = this.config;
            const target = e.target;

            if (target.matches(selectors.filterInputs)) {
                this.state.currentPage = 1;
                this.applyFiltersAndPagination();
            } else if (target.matches('.hinhDang, .chatLieu, .thanhPhan, .kichThuoc, .viTriSuDung, .tinhNang')) {
                this.updateTenThuTucHaiQuan(target.closest('tr'));
            }
        },

        handleDocumentClick(e) {
 
            if (!e.target.closest('.ms-container')) {
                document.querySelectorAll('.ms-dropdown.open').forEach(dropdown => {
                    this.closeSearchableDropdown(dropdown);
                });
            }
        },


        addRow() {
            const lastRow = this.elements.tableBody.lastElementChild;
            if (!lastRow) return;

            const newRow = lastRow.cloneNode(true);

            const inputs = newRow.querySelectorAll('input, textarea');
            for (let i = 0; i < inputs.length; i++) {
                inputs[i].value = '';
                inputs[i].classList.remove('is-invalid');
            }

            const selects = newRow.querySelectorAll('select');
            for (let i = 0; i < selects.length; i++) {
                const sel = selects[i];
                if (sel.matches(this.config.selectors.rohsSelect)) sel.value = 'No Need';
                else if (sel.matches(this.config.selectors.getQuoteSelect)) sel.value = 'true';
                else if (sel.matches(this.config.selectors.urgentSelect)) sel.value = 'false';
                else sel.value = '';
                sel.classList.remove('is-invalid');
            }

            const containers = newRow.querySelectorAll('.ms-container');
            for (let i = containers.length - 1; i >= 0; i--) {
                containers[i].remove();
            }

            const searchableSelects = newRow.querySelectorAll('select.searchable-select');
            for (let i = 0; i < searchableSelects.length; i++) {
                searchableSelects[i].style.display = '';
                searchableSelects[i].dataset.searchDropdown = 'false';
            }

            this.elements.tableBody.appendChild(newRow);
            this.initSearchableDropdowns(newRow);
            this.renumberRowsOptimized();
            this.applyFiltersAndPagination();
        },

        removeRow(btn) {
            const tr = btn.closest('tr');
            if (!tr) return;

            if (this.elements.tableBody.querySelectorAll('tr').length <= 1) return;

            if (this.state.allQuoteItems.length > 0) {
                const start = (this.state.currentPage - 1) * this.state.rowsPerPage;
                const rowIndex = Array.from(this.elements.tableBody.querySelectorAll('tr')).indexOf(tr);
                const globalIndex = start + rowIndex;

                if (globalIndex >= 0 && globalIndex < this.state.allQuoteItems.length) {
                    this.state.allQuoteItems.splice(globalIndex, 1);
                    this.state.filteredQuoteItems = [...this.state.allQuoteItems];
                    this.renderQuotePage();
                    return;
                }
            }

            tr.remove();
            this.renumberRows();
            this.applyFiltersAndPagination();
        },

        resetForm() {
            this.elements.form.reset();
            const tbody = this.elements.tableBody;
            if (tbody) {
                while (tbody.children.length > 5) {
                    tbody.removeChild(tbody.lastElementChild);
                }
                tbody.querySelectorAll('tr').forEach(tr => {
                    tr.querySelectorAll('input, textarea').forEach(inp => {
                        inp.value = '';
                        inp.classList.remove('is-invalid');
                    });
                    tr.querySelectorAll('select').forEach(sel => {
                        if (sel.matches(this.config.selectors.rohsSelect)) sel.value = 'No Need';
                        else if (sel.matches(this.config.selectors.getQuoteSelect)) sel.value = 'true';
                        else if (sel.matches(this.config.selectors.urgentSelect)) sel.value = 'false';
                        else sel.selectedIndex = 0;
                        sel.classList.remove('is-invalid');
                    });
                });
            }

            this.state.allQuoteItems = [];
            this.state.filteredQuoteItems = [];
            this.elements.form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));

            document.querySelectorAll('.ms-dropdown.open').forEach(d => this.closeSearchableDropdown(d));
            this.initSearchableDropdowns(this.elements.container);
            this.renumberRows();
            this.applyFiltersAndPagination();
        },

        async submitForm() {
            if (this.state.isSubmitting) return;
            this.state.isSubmitting = true;
            this.showLoading(this.config.i18n.Exporting);

            try {
                const approverVal = this.elements.approverSelect?.value.trim();
                if (!approverVal) {
                    this.showDialog({ title: this.config.i18n.ErrorTitle, message: this.config.i18n.SelectApprover, type: 'error' });
                    return;
                }

                let rowsValid = true;
                let reasonCheck = true;

                const visibleRows = Array.from(this.elements.tableBody.querySelectorAll('tr'));
                visibleRows.forEach(tr => {
                    if (this.isRowEmpty(tr)) return;
                    if (!this.validateRow(tr)) rowsValid = false;
                    if (!this.checkReasonForRejection(tr)) reasonCheck = false;
                });

                if (!rowsValid) {
                    this.showDialog({ title: this.config.i18n.ErrorTitle, message: this.config.i18n.MsgFillRequired, type: 'error' });
                    return;
                }
                if (!reasonCheck) {
                    this.showDialog({ title: this.config.i18n.ErrorTitle, message: this.config.i18n.MsgEnterReasonReject, type: 'error' });
                    return;
                }

                this.syncUIToState();
                const payload = this.preparePayload(approverVal);

                if (payload.length === 0) {
                    this.showDialog({ title: this.config.i18n.ErrorTitle, message: this.config.i18n.MsgInvalidData, type: 'error' });
                    return;
                }

                const res = await fetch(this.config.api.insertListBaoGia, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload),
                });

                if (!res.ok) throw new Error(await res.text());

                await res.json();
                this.showDialog({ title: this.config.i18n.SuccessTitle, message: this.config.i18n.MsgSubmitSuccess, type: 'success' });
                this.resetForm();

            } catch (err) {
                this.showDialog({ title: this.config.i18n.ErrorTitle, message: err.message, type: 'error' });
            } finally {
                this.state.isSubmitting = false;
                this.hideLoading();
            }
        },

        syncUIToState() {
            if (this.state.allQuoteItems.length === 0) return;
            const start = (this.state.currentPage - 1) * this.state.rowsPerPage;
            this.elements.tableBody.querySelectorAll('tr').forEach((tr, idx) => {
                const globalIdx = start + idx;
                if (this.state.filteredQuoteItems[globalIdx]) {
                    const collected = this.collectRowData(tr);
                    Object.assign(this.state.filteredQuoteItems[globalIdx], collected);
                    // Find and update in allQuoteItems as well
                    const masterIndex = this.state.allQuoteItems.findIndex(item => item === this.state.filteredQuoteItems[globalIdx]);
                    if (masterIndex > -1) {
                        Object.assign(this.state.allQuoteItems[masterIndex], collected);
                    }
                }
            });
        },

        preparePayload(approverVal) {
            let payload = [];
            if (this.state.allQuoteItems.length > 0) {
                payload = this.state.allQuoteItems.filter(item => !this.isRowEmpty(item, true));
            } else {
                payload = Array.from(this.elements.tableBody.querySelectorAll('tr'))
                    .map(tr => this.collectRowData(tr))
                    .filter(item => !this.isRowEmpty(item, true));
            }

            if (payload.length === 0) return [];

            const firstItemWithSection = payload.find(it => it.CHR_SectionCode);
            const sectionForPayload = firstItemWithSection ? firstItemWithSection.CHR_SectionCode : this.elements.tableBody.querySelector(this.config.selectors.sectionSelect)?.value;
            const maDon = this.generateRequestCode(sectionForPayload);

            payload.forEach(item => {
                item.CHR_MaDon = maDon;
                item.CHR_UserApproval = approverVal;
                item.ID_StepBaoGia = 2;
                if (!item.CHR_SectionName || item.CHR_SectionName === '#N/A') {
                    item.CHR_SectionName = this.state.firstSectionName;
                }
            });

            return payload;
        },

        // Pagination and Filtering
        applyFiltersAndPagination() {
            if (this.state.allQuoteItems.length > 0) {
                this.filterInMemory();
                this.renderQuotePage();
            } else {
                this.filterDOM();
                this.paginateDOM();
            }
        },

        filterInMemory() {
            const filters = this.elements.filterInputs.map(inp => inp.value.toLowerCase().trim());
            if (filters.every(f => !f)) {
                this.state.filteredQuoteItems = [...this.state.allQuoteItems];
                return;
            }

            const searchFields = ['chR_MaHangNoiBo', 'chR_MaHangNCC', 'nvchR_NameVN', 'chR_NameEN',
                                'nvchR_DonVi', 'chR_MaNCC', 'nvchR_TenNCC', 'nvchR_ChungLoai', 'chR_Phanloai'];

            this.state.filteredQuoteItems = this.state.allQuoteItems.filter(dto => {
                // Build search string once per DTO
                let combined = '';
                for (let i = 0; i < searchFields.length; i++) {
                    const val = dto[searchFields[i]];
                    if (val) combined += String(val).toLowerCase() + ' ';
                }

                // Check all filters (every filter must match)
                return filters.every(filter => !filter || combined.includes(filter));
            });
        },

        filterDOM() {
            const filters = this.elements.filterInputs.map(inp => inp.value.toLowerCase().trim());
            const allRows = Array.from(this.elements.tableBody.querySelectorAll('tr'));

            this.state.filteredDOMRows = allRows.filter(tr => {
                const tds = Array.from(tr.querySelectorAll('td'));
                return filters.every((filter, idx) => {
                    if (!filter) return true;
                    const td = tds[idx];
                    if (!td) return true;
                    return this.getCellText(td).toLowerCase().includes(filter);
                });
            });
        },

        paginateDOM() {
            const { currentPage, rowsPerPage, filteredDOMRows } = this.state;
            const totalRows = filteredDOMRows.length;
            const totalPages = Math.ceil(totalRows / rowsPerPage) || 1;
            this.state.currentPage = Math.min(currentPage, totalPages);

            const start = (this.state.currentPage - 1) * rowsPerPage;
            const end = start + rowsPerPage;

            this.elements.tableBody.querySelectorAll('tr').forEach(tr => tr.style.display = 'none');
            filteredDOMRows.slice(start, end).forEach(tr => tr.style.display = '');

            this.updatePaginationUI(totalRows, totalPages);
        },

        renderQuotePage() {
            const { currentPage, rowsPerPage, filteredQuoteItems } = this.state;
            const totalItems = filteredQuoteItems.length;
            const totalPages = Math.ceil(totalItems / rowsPerPage) || 1;
            this.state.currentPage = Math.min(currentPage, totalPages);

            const start = (this.state.currentPage - 1) * rowsPerPage;
            const end = start + rowsPerPage;
            const pageItems = filteredQuoteItems.slice(start, end);

            const baseRow = this.elements.tableBody.querySelector('tr');
            if (!baseRow && pageItems.length > 0) {
                console.error("Table has no template row to clone.");
                return;
            }

            // Batch render: Use requestAnimationFrame để không block UI
            requestAnimationFrame(() => {
                this.elements.tableBody.innerHTML = '';
                const frag = document.createDocumentFragment();

                // Optimize: Pre-cache selectors trong loop
                const rowTemplate = baseRow.cloneNode(true);
                pageItems.forEach((dto, i) => {
                    const row = rowTemplate.cloneNode(true);
                    this.resetRowOptimized(row);
                    this.populateRowFromDtoOptimized(row, dto, start + i + 1);
                    frag.appendChild(row);
                });

                this.elements.tableBody.appendChild(frag);

                // Defer DOM-heavy operations
                requestAnimationFrame(() => {
                    this.initSearchableDropdowns(this.elements.tableBody);
                    this.renumberRowsOptimized();
                    this.updatePaginationUI(totalItems, totalPages);
                });
            });
        },

        updatePaginationState() {
            const newRowsPerPage = parseInt(this.elements.rowsPerPageSelect?.value, 10);
            if (!isNaN(newRowsPerPage) && newRowsPerPage > 0) {
                this.state.rowsPerPage = newRowsPerPage;
            }
            this.state.currentPage = 1;
        },

        updatePaginationUI(totalEntries, totalPages) {
            const { currentPage, rowsPerPage } = this.state;
            const { i18n } = this.config;

            if (totalPages > 1) {
                this.elements.paginationInfo.style.display = '';
                this.elements.paginationControls.style.display = '';
                this.elements.prevPage.classList.toggle('disabled', currentPage === 1);
                this.elements.nextPage.classList.toggle('disabled', currentPage === totalPages);
            } else {
                this.elements.paginationInfo.style.display = 'none';
                this.elements.paginationControls.style.display = 'none';
            }

            const startEntry = totalEntries === 0 ? 0 : (currentPage - 1) * rowsPerPage + 1;
            const endEntry = Math.min(currentPage * rowsPerPage, totalEntries);

            this.elements.pageInfo.textContent = `${i18n.Showing || 'Showing'} ${startEntry} ~ ${endEntry} ${i18n.Of || 'Of'} ${totalEntries}`;
            this.elements.pageNumberInfo.textContent = `${currentPage}/${totalPages}`;
        },

        changePage(delta) {
            const totalCount = this.state.allQuoteItems.length > 0 ? this.state.filteredQuoteItems.length : this.state.filteredDOMRows.length;
            const totalPages = Math.ceil(totalCount / this.state.rowsPerPage) || 1;
            const newPage = this.state.currentPage + delta;

            if (newPage >= 1 && newPage <= totalPages) {
                this.state.currentPage = newPage;
                this.applyFiltersAndPagination();
            }
        },

        clearFilters() {
            this.elements.filterInputs.forEach(inp => inp.value = '');
            this.state.currentPage = 1;
            this.applyFiltersAndPagination();
        },

        // Data Handling and Autofill
        async handleExcelUpload(e) {
            const file = e.target.files?.[0];
            if (!file) return;

            this.showLoading(this.config.i18n.Exporting);
            try {
                const fd = new FormData();
                fd.append('file', file);
                const res = await fetch(this.config.api.uploadQuoteExcel, { method: 'POST', body: fd });
                if (!res.ok) throw new Error(await res.text());
                const items = await res.json();

                if (!Array.isArray(items)) {
                    throw new Error(this.config.i18n.MsgInvalidData);
                }
                this.populateTableFromItems(items);

            } catch (err) {
                this.showDialog({ title: this.config.i18n.ErrorTitle, message: err.message || this.config.i18n.MsgCannotReadFile, type: 'error' });
            } finally {
                this.hideLoading();
                e.target.value = ''; // Reset file input
            }
        },

        populateTableFromItems(items) {
            this.state.allQuoteItems = items;
            this.state.filteredQuoteItems = [...items];
            this.state.currentPage = 1;

            const sections = new Set(items.map(it => it.chR_SectionCode || it.CHR_SectionCode).filter(Boolean));
            if (sections.size > 1) {
                this.showDialog({ title: this.config.i18n.ErrorTitle, message: 'Không được upload dữ liệu chứa nhiều mã phòng khác nhau trong cùng 1 đơn. Vui lòng kiểm tra file Excel.', type: 'error' });
                return;
            }

            if (sections.size === 1) {
                this.loadApprovers(sections.values().next().value);
            }

            this.renderQuotePage();
            this.showDialog({ title: this.config.i18n.SuccessTitle, message: this.config.i18n.MsgLoadedRows.replace('{0}', items.length), type: 'success' });
        },

        async handleSectionChange(selectElement) {
            const newValue = selectElement.value;
            const prevValue = selectElement.dataset.prev || '';

            const sections = new Set(
                Array.from(this.elements.tableBody.querySelectorAll(this.config.selectors.sectionSelect))
                    .map(sel => sel.value)
                    .filter(Boolean)
            );

            if (sections.size > 1) {
                selectElement.value = prevValue;
                this.updateSearchableSelectDisplay(selectElement);
                this.showDialog({ title: this.config.i18n.ErrorTitle, message: 'Không được chọn 2 mã phòng khác nhau trong cùng 1 đơn', type: 'error' });
                return;
            }

            if (sections.size === 1) {
                await this.loadApprovers(newValue);
            } else {
                this.elements.approverSelect.innerHTML = `<option value="">${this.config.i18n.SelectApprover || '-- Select --'}</option>`;
            }
        },

        async handleCategoryChange(selectElement) {
            const tr = selectElement.closest('tr');
            if (!tr) return;

            await this.autoAddRowByCategory(selectElement);
            await this.refreshMaterialOptions(tr, selectElement.value);
        },

        async handleSupplierChange(selectElement) {
            const tr = selectElement.closest('tr');
            if (!tr) return;

            const maNcc = selectElement.value;
            const categorySelect = tr.querySelector(this.config.selectors.categorySelect);
            const category = categorySelect?.value || '';

            if (!maNcc || !category) return;

            // Kiểm tra xem NCC này có cung cấp chủng loại này hay không
            try {
                const isValid = await this.checkNccCategory(maNcc, category);
                if (!isValid) {
                    // NCC không cung cấp chủng loại này - revert choice
                    selectElement.value = '';
                    this.updateSearchableSelectDisplay(selectElement);
                    this.showDialog({
                        title: this.config.i18n.ErrorTitle,
                        message: `Nhà cung cấp này không cung cấp chủng loại hàng được chọn. Vui lòng chọn nhà cung cấp khác.`,
                        type: 'error'
                    });
                }
            } catch (err) {
                console.warn('Lỗi kiểm tra NCC:', err);
            }
        },

        async checkNccCategory(maNcc, category) {
            try {
                const res = await fetch(this.config.api.checkNCC, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ maNcc, catergory: category })
                });

                if (!res.ok) {
                    return false;
                }

                const result = await res.json();
                return result && result.success !== false;
            } catch (err) {
                console.warn('Lỗi gọi API CheckNCC:', err);
                return true; // Allow nếu có lỗi để không block user
            }
        },

        async autofillFromMaterialSelect(selectEl) {
            const tr = selectEl.closest('tr');
            const code = selectEl.value;
            if (!code || !tr) return;

            // Avoid autofill if a supplier is already chosen
            if (tr.querySelector(this.config.selectors.supplierSelect)?.value) return;

            try {
                const res = await fetch(this.config.api.getMaterials(code));
                if (!res.ok) throw new Error(await res.text());
                const materials = await res.json();
                const material = Array.isArray(materials) ? materials.find(m => m.material_Code === code) : null;
                if (!material) return;

                const fieldMap = {
                    'input[name^="tenHangEN_"]': material.material_Name_EN,
                    'input[name^="donVi_"]': material.unit || material.material_Unit,
                    'input[name^="tenHangVN_"]': material.nameVI,
                    '.hinhDang': material.shape,
                    '.chatLieu': material.material,
                    '.thanhPhan': material.composition,
                    '.kichThuoc': material.dimension,
                    '.viTriSuDung': material.usedFor,
                    '.tinhNang': material.purpose,
                    '.tenPhanLoaiTb': material.loaiHang || material.LoaiHang,
                };

                for (const selector in fieldMap) {
                    const el = tr.querySelector(selector);
                    if (el && fieldMap[selector]) {
                        el.value = fieldMap[selector];
                    }
                }

                const categorySelect = tr.querySelector(this.config.selectors.categorySelect);
                if (categorySelect && material.category_VN) {
                    this.setSelectValueByText(categorySelect, material.category_VN);
                    this.updateSearchableSelectDisplay(categorySelect);
                    await this.autoAddRowByCategory(categorySelect);
                }

            } catch (err) {
                console.warn('Không thể tự động điền thông tin vật tư:', err);
                this.showDialog({ title: 'Lỗi', message: err.message, type: 'error' });
            }
        },

        async autoAddRowByCategory(selectEl) {
            const tr = selectEl.closest('tr');
            const categoryCode = selectEl.value;
            if (!tr || !categoryCode) return;

            try {
                const res = await fetch(this.config.api.getNCCByCategory, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(categoryCode)
                });
                if (!res.ok) throw new Error(await res.text());
                const suppliers = await res.json();

                if (!Array.isArray(suppliers) || suppliers.length === 0) return;

                const getSupCode = (s) => s?.chR_MaNCC || (typeof s === 'string' ? s : '') || '';

                // Set first supplier on current row
                const firstSupplier = suppliers[0];
                const supSel = tr.querySelector(this.config.selectors.supplierSelect);
                if (supSel) {
                    supSel.value = getSupCode(firstSupplier);
                    this.updateSearchableSelectDisplay(supSel);
                }
                const nccCodeInput = tr.querySelector('input[name^="maHangNCC_"]');
                if (nccCodeInput && firstSupplier.nvchR_CodeByNCC) nccCodeInput.value = firstSupplier.nvchR_CodeByNCC;
                const nsxInput = tr.querySelector('input[name^="nsx_"]');
                if (nsxInput && firstSupplier.nvchR_MakeIn) nsxInput.value = firstSupplier.nvchR_MakeIn;

                if (suppliers.length > 1) {
                    const currentRowData = this.collectRowData(tr);
                    let insertAfter = tr;
                    for (let i = 1; i < suppliers.length; i++) {
                        const newRow = tr.cloneNode(true);
                        this.resetRow(newRow);
                        this.populateRowFromDto(newRow, currentRowData, 0); // 0 for temp number

                        const sup = suppliers[i];
                        const supSelNew = newRow.querySelector(this.config.selectors.supplierSelect);
                        if (supSelNew) supSelNew.value = getSupCode(sup);

                        const nccCodeInputNew = newRow.querySelector('input[name^="maHangNCC_"]');
                        if (nccCodeInputNew) nccCodeInputNew.value = sup.nvchR_CodeByNCC || '';

                        const nsxInputNew = newRow.querySelector('input[name^="nsx_"]');
                        if (nsxInputNew) nsxInputNew.value = sup.nvchR_MakeIn || '';

                        insertAfter.after(newRow);
                        insertAfter = newRow;
                    }
                    this.initSearchableDropdowns(this.elements.tableBody);
                    this.renumberRows();
                }
            } catch (err) {
                console.warn('Không thể lấy NCC cho mã hàng:', err);
            }
        },

        async refreshMaterialOptions(tr, category) {
            const sel = tr.querySelector(this.config.selectors.materialSelect);
            if (!sel) return;

            try {
                const body = { MaHang: '', Name: '', NhomHang: category, PageIndex: 0, PageSize: 0 };
                const res = await fetch(this.config.api.searchMaterials, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)
                });
                if (!res.ok) throw new Error(await res.text());
                const materials = await res.json();

                const prevValue = sel.value;
                sel.innerHTML = `<option value="">${this.config.i18n.SelectInternalMaterialCode || ''}</option>`;
                if (Array.isArray(materials)) {
                    materials.forEach(m => {
                        if (m.material_Code) {
                            const o = new Option(`${m.material_Code} - ${m.material_Name_VN || ''}`, m.material_Code);
                            sel.add(o);
                        }
                    });
                }
                sel.value = prevValue;
                if (sel.selectedIndex === -1) sel.selectedIndex = 0;

                // Rebuild searchable dropdown for this select
                const wrapper = sel.nextElementSibling;
                if (wrapper && wrapper.classList.contains('ms-container')) wrapper.remove();
                sel.style.display = '';
                sel.dataset.searchDropdown = 'false';
                this.initSearchableDropdowns(tr);

            } catch (err) {
                console.warn('Không thể tải danh sách vật tư:', err);
            }
        },

        // Validation
        validateRow(tr) {
            let ok = true;
            const requiredFields = [
                { selector: this.config.selectors.sectionSelect },
                { selector: this.config.selectors.categorySelect },
                { selector: 'input[name^="tenHangVN_"]' },
                { selector: 'input[name^="tenHangEN_"]' },
                { selector: 'input[type="number"]' },
                { selector: 'input[name^="donVi_"]' },
                { selector: this.config.selectors.supplierSelect },
                { selector: this.config.selectors.getQuoteSelect },
                { selector: 'input[name^="ngayMuonNhan_"]' },
            ];

            requiredFields.forEach(field => {
                const el = tr.querySelector(field.selector);
                if (el) {
                    const isValid = el.value.trim() !== '';
                    el.classList.toggle('is-invalid', !isValid);
                    if (el.matches('.searchable-select')) {
                        el.nextElementSibling?.querySelector('.ms-btn')?.classList.toggle('is-invalid', !isValid);
                    }
                    if (!isValid) ok = false;
                }
            });

            const internalCodeEl = tr.querySelector(this.config.selectors.materialSelect);
            const supplierCodeEl = tr.querySelector('input[name^="maHangNCC_"]');
            if (internalCodeEl && supplierCodeEl) {
                const hasInternal = internalCodeEl.value.trim() !== '';
                const hasSupplier = supplierCodeEl.value.trim() !== '';
                if (!hasInternal && !hasSupplier) {
                    ok = false;
                    internalCodeEl.classList.add('is-invalid');
                    supplierCodeEl.classList.add('is-invalid');
                    internalCodeEl.nextElementSibling?.querySelector('.ms-btn')?.classList.add('is-invalid');
                } else {
                    internalCodeEl.classList.remove('is-invalid');
                    supplierCodeEl.classList.remove('is-invalid');
                    internalCodeEl.nextElementSibling?.querySelector('.ms-btn')?.classList.remove('is-invalid');
                }
            }

            return ok;
        },

        checkReasonForRejection(tr) {
            const getQuoteSelect = tr.querySelector(this.config.selectors.getQuoteSelect);
            const reasonInput = tr.querySelector(this.config.selectors.reasonInput);
            if (getQuoteSelect?.value === 'false') {
                if (!reasonInput?.value.trim()) {
                    reasonInput?.classList.add('is-invalid');
                    return false;
                }
            }
            reasonInput?.classList.remove('is-invalid');
            return true;
        },

        // Utility and Helper functions
        isRowEmpty(row, isDto = false) {
            if (!row) return true;
            if (isDto) {
                // Check DTO properties
                return Object.values(row).every(val => val === null || val === '' || val === 0 || val === false);
            }
            // Check DOM row
            const inputs = Array.from(row.querySelectorAll('input, textarea'));
            if (inputs.some(inp => inp.type !== 'hidden' && inp.type !== 'file' && inp.type !== 'checkbox' && inp.type !== 'radio' && inp.value.trim() !== '')) {
                return false;
            }
            const selects = Array.from(row.querySelectorAll('select'));
            const ignoreVals = new Set(['', 'true', 'false', 'No Need']);
            if (selects.some(sel => !ignoreVals.has(sel.value.trim()))) {
                return false;
            }
            return true;
        },

        renumberRowsOptimized() {
            // Optimize: Cache nodelist once, update in single pass
            const rows = this.elements.tableBody.querySelectorAll('tr');
            const rowCount = rows.length;

            for (let idx = 0; idx < rowCount; idx++) {
                const tr = rows[idx];
                const noCell = tr.children[0];
                if (noCell) noCell.textContent = String(idx + 1);
                this.assignRowIdsOptimized(tr, idx + 1);
            }
        },

        assignRowIdsOptimized(tr, index) {
            // Optimize: Pre-build selector map, minimize DOM queries
            const idMap = {
                '.tenPhongBanTb': 'tenPhongBanTb',
                '.chungLoaiTb': 'chungLoai',
                '.tenPhanLoaiTb': 'tenPhanLoaiTb',
                '.maHangNoiBo': 'maHangNoiBo',
                'input[name^="maThietBi"]': 'maThietBi',
                'input[name^="maHangNCC"]': 'maHangNCC',
                'input[name^="tenHangVN"]': 'tenHangVN',
                'input[name^="tenHangEN"]': 'tenHangEN',
                'input[name^="soLuong"]': 'soLuong',
                'input[name^="donVi"]': 'donVi',
                '.hinhDang': 'hinhDang',
                '.chatLieu': 'chatLieu',
                '.thanhPhan': 'thanhPhan',
                '.kichThuoc': 'kichThuoc',
                '.viTriSuDung': 'viTriSuDung',
                '.tinhNang': 'tinhNang',
                '.rohsTb': 'rohsTb',
                '.CoCqTb': 'CoCqTb',
                'input[name^="msds"]': 'msds',
                'input[name^="tieuChuanAnToan"]': 'tieuChuanAnToan',
                'input[name^="fileThietKe"]': 'fileThietKe',
                'input[name^="nsx"]': 'nsx',
                '.nhaCungCapTb': 'nhaCungCapTb',
                '.laybaogiaTb': 'laybaogiaTb',
                'input[name^="lyDo"]': 'lyDo',
                '.gapTb': 'gapTb',
                'input[name^="nguoiYeuCauRow"]': 'nguoiYeuCauRow',
                'input[name^="ngayMuonNhan"]': 'ngayMuonNhan',
                'input[name^="kyHanChonNCC"]': 'kyHanChonNCC',
            };

            for (const selector in idMap) {
                const el = tr.querySelector(selector);
                if (el) el.id = `${idMap[selector]}_${index}`;
            }
        },

        collectRowData(tr) {
            const getVal = (selector) => tr.querySelector(selector)?.value || '';
            const getSelText = (selector) => {
                const el = tr.querySelector(selector);
                if (!el) return '';
                if (el.nextElementSibling?.classList.contains('ms-container')) {
                    const text = el.nextElementSibling.querySelector('.ms-values')?.textContent.trim();
                    if (text) {
                        this.state.firstSectionName = text;
                        return text;
                    }
                }
                return el.options[el.selectedIndex]?.text || '';
            };

            const toVietnamISOString = (date) => {
                const pad = (num) => String(num).padStart(2, '0');
                return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}+07:00`;
            };

            const createDateVN = new Date(new Date().getTime() + 7 * 3600 * 1000);

            const obj = {
                ID: 0,
                CHR_MaDon: '',
                CHR_MaThietBi: getVal('input[name^="maThietBi_"]'),
                CHR_Phanloai: getVal('.tenPhanLoaiTb'),
                CHR_MaHangNoiBo: getVal('.maHangNoiBo'),
                CHR_MaHangNCC: getVal('input[name^="maHangNCC_"]'),
                NVCHR_NameVN: getVal('input[name^="tenHangVN_"]'),
                CHR_NameEN: getVal('input[name^="tenHangEN_"]'),
                INT_SoLuong: getVal('input[type="number"]'),
                NVCHR_DonVi: getVal('input[name^="donVi_"]'),
                NVCHR_ChungLoai: getVal('.chungLoaiTb'),
                NVCHR_HinhDang: getVal('.hinhDang'),
                NVCHR_ChatLieu: getVal('.chatLieu'),
                NVCHR_ThanhPhan: getVal('.thanhPhan'),
                NVCHR_KichThuoc: getVal('.kichThuoc'),
                NVCHR_DongMay: getVal('.viTriSuDung'),
                NVCHR_TinhNang: getVal('.tinhNang'),
                NVCHR_Rohs: getVal('.rohsTb'),
                NVCHR_COCQ: getVal('.CoCqTb'),
                NVCHR_MSDS: getVal('input[name^="msds_"]'),
                NVCHR_AnToan: getVal('input[name^="tieuChuanAnToan_"]'),
                NVCHR_FileThietKe: getVal('input[name^="fileThietKe_"]'),
                NVCHR_NhaSanXuat: getVal('input[name^="nsx_"]'),
                CHR_MaNCC: getVal('.nhaCungCapTb'),
                NVCHR_TenNCC: getSelText('.nhaCungCapTb'),
                BIT_LayBaoGia: getVal('.laybaogiaTb') === 'true',
                NVCHR_LyDo: getVal('input[name^="lyDo_"]'),
                DTM_NgayMuonNhan: getVal('input[name^="ngayMuonNhan_"]') || null,
                DTM_KyHan: getVal('input[name^="kyHanChonNCC_"]') || null,
                CHR_Gap: getVal('.gapTb'),
                CHR_SectionCode: getVal('.tenPhongBanTb'),
                CHR_SectionName: getSelText('.tenPhongBanTb'),
                NVCHR_UserRequest: getVal('input[name^="nguoiYeuCauRow_"]') || this.config.userData.user,
                CHR_CreateBy: this.config.userData.user ?? '',
                DTM_CreateDate: toVietnamISOString(createDateVN),
                CHR_UserApproval: this.elements.approverSelect?.value || '',
                ID_StepBaoGia: 2,
                ID_Status: 'CREATE',
                INT_SoLanUpdate: 0,
                DTM_UpdateLater: null,
                DTM_Deadline: null,
                BIT_IsTemplate: false
            };

            obj.INT_SoLuong = parseFloat(obj.INT_SoLuong) || null;
            obj.DTM_NgayMuonNhan = this.formatDateToISO(obj.DTM_NgayMuonNhan);
            obj.DTM_KyHan = this.formatDateToISO(obj.DTM_KyHan);

            return obj;
        },

        populateRowFromDtoOptimized(tr, dto, rowIndex) {
            if (rowIndex) {
                tr.querySelector('td:first-child').textContent = rowIndex;
            }
            const lastCell = tr.querySelector('td:last-child');
            if (lastCell && !lastCell.querySelector('.btn-remove-row')) {
                lastCell.innerHTML = `<button type="button" class="btn btn-sm btn-link text-danger px-0 btn-remove-row" title="Remove Row"><i class="fas fa-times"></i></button>`;
            }

            // Optimize: Pre-compile field mappings, minimize DOM queries
            const fieldConfig = {
                '.tenPhongBanTb': { value: dto.chR_SectionCode || dto.chR_SectionName, isSelect: true },
                '.chungLoaiTb': { value: dto.nvchR_ChungLoai, isSelect: true },
                '.tenPhanLoaiTb': { value: dto.chR_Phanloai, isSelect: false },
                'input[name^="maThietBi"]': { value: dto.chR_MaThietBi, isSelect: false },
                '.maHangNoiBo': { value: dto.chR_MaHangNoiBo, isSelect: true },
                'input[name^="maHangNCC"]': { value: dto.chR_MaHangNCC, isSelect: false },
                'input[name^="tenHangVN"]': { value: dto.nvchR_NameVN, isSelect: false },
                'input[name^="tenHangEN"]': { value: dto.chR_NameEN, isSelect: false },
                'input[name^="soLuong"]': { value: dto.inT_SoLuong, isSelect: false },
                'input[name^="donVi"]': { value: dto.nvchR_DonVi, isSelect: false },
                '.hinhDang': { value: dto.nvchR_HinhDang, isSelect: false },
                '.chatLieu': { value: dto.nvchR_ChatLieu, isSelect: false },
                '.thanhPhan': { value: dto.nvchR_ThanhPhan, isSelect: false },
                '.kichThuoc': { value: dto.nvchR_KichThuoc, isSelect: false },
                '.viTriSuDung': { value: dto.nvchR_DongMay, isSelect: false },
                '.tinhNang': { value: dto.nvchR_TinhNang, isSelect: false },
                '.rohsTb': { value: dto.nvchR_Rohs, isSelect: true },
                '.CoCqTb': { value: dto.nvchR_COCQ, isSelect: true },
                'input[name^="msds"]': { value: dto.nvchR_MSDS, isSelect: false },
                'input[name^="tieuChuanAnToan"]': { value: dto.nvchR_AnToan, isSelect: false },
                'input[name^="fileThietKe"]': { value: dto.nvchR_FileThietKe, isSelect: false },
                'input[name^="nsx"]': { value: dto.nvchR_NhaSanXuat, isSelect: false },
                '.nhaCungCapTb': { value: dto.chR_MaNCC || dto.nvchR_TenNCC, isSelect: true },
                '.laybaogiaTb': { value: dto.biT_LayBaoGia === true ? 'true' : 'false', isSelect: true },
                'input[name^="lyDo"]': { value: dto.nvchR_LyDo, isSelect: false },
                'input[name^="ngayMuonNhan"]': { value: this.dateToInputValue(dto.dtM_NgayMuonNhan), isSelect: false },
                'input[name^="kyHanChonNCC"]': { value: this.dateToInputValue(dto.dtM_KyHan), isSelect: false },
                '.gapTb': { value: String(dto.chR_Gap), isSelect: true },
                'input[name^="nguoiYeuCauRow"]': { value: dto.nvchR_UserRequest || this.config.userData.user, isSelect: false },
            };

            // Batch update: Single pass through config
            for (const selector in fieldConfig) {
                const config = fieldConfig[selector];
                const el = tr.querySelector(selector);
                if (!el) continue;

                if (config.isSelect) {
                    this.setSelectValueByText(el, config.value);
                } else {
                    el.value = config.value ?? '';
                }
            }
        },

        updateTenThuTucHaiQuan(tr) {
            if (!tr) return;
            const getVal = (className) => tr.querySelector(`.${className}`)?.value || '';
            const classMaterial = getVal('tenPhanLoaiTb');
            const categorySel = tr.querySelector('.chungLoaiTb');
            const categoryVN = categorySel ? (categorySel.options[categorySel.selectedIndex]?.text || '') : '';
            const shape = getVal('hinhDang');
            const material = getVal('chatLieu');
            const composition = getVal('thanhPhan');
            const dimension = getVal('kichThuoc');
            const usedFor = getVal('viTriSuDung');
            const purpose = getVal('tinhNang');

            let tenHangVN = "";
            if (classMaterial === "NO LIST") {
                tenHangVN = `Có hình dáng dạng ${shape} & ${usedFor} & ${purpose}`;
            } else if (!["A", "E", "I"].includes(classMaterial)) {
                tenHangVN = `${categoryVN} có hình dáng dạng ${shape} chất liệu ${material} thành phần hóa chất ${composition} có kích thước ${dimension} dung để ${usedFor} cho ${purpose}`;
            }

            const vnInput = tr.querySelector('input[name^="tenHangVN_"]');
            if (vnInput) vnInput.value = tenHangVN;
        },

        // UI Helpers (Dialog, Loading, Searchable Dropdown)
        showLoading(message) {
            if (!this.elements.globalLoading) return;
            const msgEl = this.elements.globalLoading.querySelector('.loader-msg');
            if (msgEl) msgEl.textContent = message || 'Đang xử lý...';
            this.elements.globalLoading.style.display = 'flex';
            this.elements.globalLoading.setAttribute('aria-hidden', 'false');
        },

        hideLoading() {
            if (this.elements.globalLoading) {
                this.elements.globalLoading.style.display = 'none';
                this.elements.globalLoading.setAttribute('aria-hidden', 'true');
            }
        },

        showDialog({ title = 'Thông báo', message = '', type = 'info' }) {
            const { dialogOverlay, dialogTitle, dialogBody, dialogFooter } = this.elements;
            if (!dialogOverlay) return alert(message);

            dialogTitle.textContent = title;
            const iconClass = type === 'success' ? 'fa-check-circle text-success' : type === 'error' ? 'fa-exclamation-circle text-danger' : 'fa-info-circle text-primary';
            dialogBody.innerHTML = `<div class="d-flex align-items-start gap-2"><i class="fas ${iconClass}"></i><div>${message}</div></div>`;

            dialogFooter.innerHTML = '';
            const okBtn = document.createElement('button');
            okBtn.className = 'cm-btn cm-btn-primary';
            okBtn.textContent = this.config.i18n.DialogOk || 'Đồng ý';
            okBtn.onclick = () => this.hideDialog();
            dialogFooter.appendChild(okBtn);

            dialogOverlay.style.display = 'flex';
            dialogOverlay.setAttribute('aria-hidden', 'false');

            // Attach close handlers
            dialogOverlay.querySelector(this.config.selectors.dialogClose)?.addEventListener('click', () => this.hideDialog(), { once: true });
            dialogOverlay.querySelector(this.config.selectors.dialogBackdrop)?.addEventListener('click', () => this.hideDialog(), { once: true });
        },

        hideDialog() {
            if (this.elements.dialogOverlay) {
                this.elements.dialogOverlay.style.display = 'none';
                this.elements.dialogOverlay.setAttribute('aria-hidden', 'true');
            }
        },

        initSearchableDropdowns(container) {
            container.querySelectorAll('select.searchable-select:not([data-search-dropdown="true"])').forEach(select => {
                this.buildSearchableDropdown(select);
            });
        },

        buildSearchableDropdown(select) {
            select.dataset.searchDropdown = 'true';
            select.style.display = 'none';

            const wrapper = document.createElement('div');
            wrapper.className = 'ms-container';

            const btn = document.createElement('div');
            btn.className = 'ms-btn';
            btn.innerHTML = `<span class="ms-values"></span><span class="ms-placeholder"></span><span class="ms-caret">▾</span>`;

            const dropdown = document.createElement('div');
            dropdown.className = 'ms-dropdown';
            dropdown.innerHTML = `<div class="ms-search"><input type="text" placeholder="${this.config.i18n.SearchEllipsis || 'Tìm...'}"></div><div class="ms-list" style="max-height:320px; overflow:auto;"></div>`;

            wrapper.append(btn, dropdown);
            select.after(wrapper);

            const list = dropdown.querySelector('.ms-list');
            const searchInput = dropdown.querySelector('.ms-search input');
            const isRemote = select.classList.contains('maHangNoiBo');

            const remoteState = {
                pageIndex: 1, pageSize: 200, loading: false, lastQuery: '', hasMore: true, options: [], controller: null
            };


            const valuesEl = btn.querySelector('.ms-values');
            const placeholderEl = btn.querySelector('.ms-placeholder');

            const updateButtonText = () => {
                const selectedOption = select.options[select.selectedIndex];
                if (selectedOption && selectedOption.value) {
                    valuesEl.textContent = selectedOption.text;
                    placeholderEl.textContent = '';
                } else {
                    valuesEl.textContent = '';
                    placeholderEl.textContent = this.config.i18n.SelectPlaceholder || '-- Chọn --';
                }
            };

            const renderList = (options, query = '') => {
                list.innerHTML = '';
                const q = query.toLowerCase();
                let hasItems = false;

                const frag = document.createDocumentFragment();

                for (let i = 0; i < options.length; i++) {
                    const opt = options[i];
                    const optText = opt.text.toLowerCase();

                    if (!q || optText.includes(q)) {
                        const item = document.createElement('div');
                        item.className = 'ms-item';
                        item.dataset.value = opt.value;
                        item.textContent = opt.text;
                        if (select.value === opt.value) {
                            item.classList.add('selected');
                        }
                        frag.appendChild(item);
                        hasItems = true;
                    }
                }

                if (!hasItems) {
                    const empty = document.createElement('div');
                    empty.className = 'ms-empty';
                    empty.textContent = this.config.i18n.NoResults || 'Không có kết quả';
                    frag.appendChild(empty);
                } else if (isRemote && remoteState.hasMore) {
                    const loading = document.createElement('div');
                    loading.className = 'ms-loading';
                    loading.textContent = 'Loading more...';
                    frag.appendChild(loading);
                }

                list.appendChild(frag);
            };

            const loadRemote = async (query, append = false) => {
                if (remoteState.loading && append) return;
                if (!append && remoteState.controller) remoteState.controller.abort();

                remoteState.loading = true;
                if (query !== remoteState.lastQuery) {
                    remoteState.pageIndex = 1;
                    remoteState.hasMore = true;
                    remoteState.options = [];
                }
                if (!remoteState.hasMore) {
                    remoteState.loading = false;
                    return;
                }

                const prevLoading = list.querySelector('.ms-loading');
                if (prevLoading) prevLoading.remove();

                const loading = document.createElement('div');
                loading.className = 'ms-loading';
                loading.textContent = 'Loading...';
                list.appendChild(loading);

                try {
                    remoteState.controller = new AbortController();
                    const category = select.closest('tr')?.querySelector('.chungLoaiTb')?.value || '';
                    const body = { MaHang: query, Name: query, NhomHang: category, PageIndex: remoteState.pageIndex, PageSize: remoteState.pageSize };
                    const res = await fetch(this.config.api.searchMaterials, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(body),
                        signal: remoteState.controller.signal
                    });
                    if (!res.ok) throw new Error('Failed to load');
                    const data = await res.json();
                    const newItems = data.map(m => ({ value: m.material_Code, text: `${m.material_Code} - ${m.material_Name_VN}` }));

                    if (!append) remoteState.options = [];

                    for (let i = 0; i < newItems.length; i++) {
                        const item = newItems[i];
                        if (!remoteState.options.some(o => o.value === item.value)) {
                            remoteState.options.push(item);
                        }
                        if (!Array.from(select.options).some(o => o.value === item.value)) {
                            select.add(new Option(item.text, item.value));
                        }
                    }

                    remoteState.hasMore = newItems.length === remoteState.pageSize;
                    if (remoteState.hasMore) remoteState.pageIndex++;
                    remoteState.lastQuery = query;

                } catch (err) {
                    if (err.name !== 'AbortError') console.warn('Error loading remote materials:', err);
                } finally {
                    remoteState.loading = false;
                    const loadingEl = list.querySelector('.ms-loading');
                    if (loadingEl) loadingEl.remove();
                    renderList(remoteState.options, searchInput.value);
                }
            };

            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const isOpen = dropdown.classList.contains('open');
                document.querySelectorAll('.ms-dropdown.open').forEach(d => this.closeSearchableDropdown(d));
                if (!isOpen) {
                    this.openSearchableDropdown(dropdown, btn);
                    searchInput.value = '';
                    searchInput.focus();
                    if (isRemote) {
                        loadRemote('');
                    } else {
                        const domOptions = Array.from(select.options).map(o => ({ value: o.value, text: o.text }));
                        renderList(domOptions);
                    }
                }
            });

            list.addEventListener('click', (e) => {
                if (e.target.classList.contains('ms-item')) {
                    select.value = e.target.dataset.value;
                    select.dispatchEvent(new Event('change', { bubbles: true }));
                    updateButtonText();
                    this.closeSearchableDropdown(dropdown);
                }
            });

            let searchTimer;
            searchInput.addEventListener('input', () => {
                clearTimeout(searchTimer);
                searchTimer = setTimeout(() => {
                    if (isRemote) {
                        loadRemote(searchInput.value);
                    } else {
                        const domOptions = Array.from(select.options).map(o => ({ value: o.value, text: o.text }));
                        renderList(domOptions, searchInput.value);
                    }
                }, 300);
            });

            if (isRemote) {
                list.addEventListener('scroll', () => {
                    if (list.scrollTop + list.clientHeight >= list.scrollHeight - 40) {
                        if (!remoteState.loading && remoteState.hasMore) {
                            loadRemote(remoteState.lastQuery, true);
                        }
                    }
                });
            }

            updateButtonText();
        },

        openSearchableDropdown(dropdown, btn) {
            const rect = btn.getBoundingClientRect();
            dropdown.classList.add('open');
            dropdown.style.position = 'fixed';
            dropdown.style.top = `${rect.bottom}px`;
            dropdown.style.left = `${rect.left}px`;
            dropdown.style.width = `${rect.width}px`;
            dropdown.style.zIndex = '3000';
        },

        closeSearchableDropdown(dropdown) {
            dropdown.classList.remove('open');
            dropdown.style.position = '';
            dropdown.style.top = '';
            dropdown.style.left = '';
            dropdown.style.width = '';
            dropdown.style.zIndex = '';
        },

        updateSearchableSelectDisplay(select) {
            const wrapper = select.nextElementSibling;
            if (!wrapper || !wrapper.classList.contains('ms-container')) return;
            const valuesEl = wrapper.querySelector('.ms-values');
            const placeholderEl = wrapper.querySelector('.ms-placeholder');
            const selectedOption = select.options[select.selectedIndex];
            if (selectedOption && selectedOption.value) {
                valuesEl.textContent = selectedOption.text;
                placeholderEl.textContent = '';
            } else {
                valuesEl.textContent = '';
                placeholderEl.textContent = this.config.i18n.SelectPlaceholder || '-- Chọn --';
            }
        },

        setSelectValueByText(select, textOrValue) {
            if (!select) return;
            const val = String(textOrValue ?? '').toLowerCase();
            let option = Array.from(select.options).find(o => o.value === textOrValue);
            if (!option) {
                option = Array.from(select.options).find(o => o.text.toLowerCase() === val || o.text.toLowerCase().startsWith(val));
            }
            if (option) {
                select.value = option.value;
            }
        },

        // ... other utility functions
        getCellText(td) {
            const select = td.querySelector('select');
            if (select) return select.options[select.selectedIndex]?.text || '';
            const input = td.querySelector('input:not([type="date"])');
            if (input) return input.value;
            return td.textContent.trim();
        },

        resetRowOptimized(row) {
            // Optimize: Batch update, minimize reflows
            const inputs = row.querySelectorAll('input, textarea');
            const selects = row.querySelectorAll('select');
            const containers = row.querySelectorAll('.ms-container');

            // Update inputs
            for (let i = 0; i < inputs.length; i++) {
                inputs[i].value = '';
                inputs[i].classList.remove('is-invalid');
            }

            // Update selects
            for (let i = 0; i < selects.length; i++) {
                selects[i].selectedIndex = 0;
                selects[i].classList.remove('is-invalid');
            }

            // Remove containers
            for (let i = containers.length - 1; i >= 0; i--) {
                containers[i].remove();
            }

            // Reset searchable-select display
            const searchableSelects = row.querySelectorAll('select.searchable-select');
            for (let i = 0; i < searchableSelects.length; i++) {
                searchableSelects[i].style.display = '';
                searchableSelects[i].dataset.searchDropdown = 'false';
            }
        },

        formatDateToISO(dateString) {
            if (!dateString) return null;
            try {
                const [year, month, day] = dateString.split('-').map(Number);
                if (!isNaN(year) && !isNaN(month) && !isNaN(day)) {
                    return new Date(Date.UTC(year, month - 1, day, 7, 0, 0)).toISOString();
                }
            } catch (e) {
                console.error('Error parsing date:', e);
            }
            return null;
        },

        dateToInputValue(date) {
            if (!date) return '';
            try {
                const dt = new Date(date);
                if (isNaN(dt)) return '';
                const pad = (num) => String(num).padStart(2, '0');
                return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}`;
            } catch (e) {
                return '';
            }
        },

        generateRequestCode(section) {
            const now = new Date(new Date().getTime() + 7 * 3600 * 1000);
            const pad = (num) => String(num).padStart(2, '0');
            const yyyy = now.getFullYear();
            const MM = pad(now.getMonth() + 1);
            const dd = pad(now.getDate());
            const sec = (section || 'GEN').toString().trim().replace(/[^a-zA-Z0-9_-]/g, '_');
            return `RQ_${sec}_${yyyy}_${MM}_${dd}`;
        },

        async loadApprovers(sectionCode) {
            if (!this.elements.approverSelect) return;
            this.showLoading();
            try {
                const body = { Step: 2, SectionCost: sectionCode || '' };
                const res = await fetch(this.config.api.searchApprover, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                });
                if (!res.ok) throw new Error(await res.text());
                const data = await res.json();

                this.elements.approverSelect.innerHTML = `<option value="">${this.config.i18n.SelectApprover || '-- Select Approver --'}</option>`;
                if (Array.isArray(data)) {
                    data.forEach(a => {
                        if (a?.chR_UserAdid) {
                            const text = a.nvchR_UserName ? `${a.nvchR_UserName} (${a.chR_UserAdid})` : a.chR_UserAdid;
                            this.elements.approverSelect.add(new Option(text, a.chR_UserAdid));
                        }
                    });
                }
            } catch (err) {
                console.warn('Không thể tải danh sách approver:', err);
            } finally {
                this.hideLoading();
            }
        },

        async downloadFile(url, defaultName) {
            try {
                this.showLoading(this.config.i18n.Exporting);
                const res = await fetch(url);
                if (!res.ok) {
                    const errorText = await res.text().catch(() => res.statusText);
                    throw new Error(errorText);
                }
                const blob = await res.blob();
                let fileName = defaultName;
                const cd = res.headers.get('content-disposition');
                if (cd) {
                    const match = cd.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                    if (match && match[1]) {
                        fileName = match[1].replace(/["']/g, '').trim();
                    }
                }
                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(link.href);
            } catch (err) {
                this.showDialog({ title: this.config.i18n.ErrorTitle, message: `${this.config.i18n.MsgExportFailed}: ${err.message}`, type: 'error' });
            } finally {
                this.hideLoading();
            }
        },

        downloadTemplate() {
            this.downloadFile(this.config.api.templateUrl, 'Mau_Quote.xlsx');
        },

        async downloadMasterData() {
            const { i18n, api } = this.config;
            this.showLoading(i18n.Exporting);
            try {
                await this.downloadFile(api.downloadMasterVendor, 'ExportMasterVendor.xlsx');
                await this.downloadFile(api.downloadMasterMaterial, 'ExportMasterMaterial.xlsx');
                this.showDialog({ title: i18n.SuccessTitle, message: i18n.ExportSuccess || 'Xuất file hoàn tất', type: 'success' });
            } catch (err) {
                // Error is shown in downloadFile
            } finally {
                this.hideLoading();
            }
        },

        async exportTable() {
            this.showLoading(this.config.i18n.Exporting);
            try {
                this.syncUIToState();
                const payload = this.state.allQuoteItems.length > 0 ? this.state.allQuoteItems : Array.from(this.elements.tableBody.querySelectorAll('tr')).map(tr => this.collectRowData(tr));

                const res = await fetch(this.config.api.exportTable, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (!res.ok) throw new Error(await res.text());

                const blob = await res.blob();
                let fileName = 'TableQuote.xlsx';
                const cd = res.headers.get('content-disposition');
                if (cd) {
                    const match = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(cd);
                    if (match && match[1]) fileName = match[1].replace(/['"]/g, '').trim();
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
                this.showDialog({ title: this.config.i18n.ErrorTitle, message: err.message || 'Không thể xuất file', type: 'error' });
            } finally {
                this.hideLoading();
            }
        },

        exportAutoRender() {
            // Initialize modal elements if not cached
            if (!this.elements.arModalOverlay) {
                this.cacheAutoRenderElements();
            }

            const { arModalOverlay, arCloseBtn, arCancelBtn, arExportBtn, arError, arTabHasCode, arTabNoCode, arTabHasCodeBody, arTabNoCodeBody, arSection, arSectionName, arMaterialList, arSearch, arSelectAll, arSection2, arSectionName2, arCategoryList, arSearch2, arSelectAll2 } = this.elements;

            if (!arModalOverlay) {
                this.showDialog({ title: this.config.i18n.ErrorTitle, message: this.config.i18n.MsgCannotOpenAutoRender || 'Không thể mở hộp thoại Auto render', type: 'error' });
                return;
            }

            // State for modal
            let currentTab = 'hasCode'; // 'hasCode' | 'noCode'
            const materialState = {
                pageIndex: 1,
                pageSize: 200,
                loading: false,
                lastQuery: '',
                hasMore: true,
                items: []
            };

            // Helpers
            const hideAr = () => {
                arModalOverlay.style.display = 'none';
                arModalOverlay.setAttribute('aria-hidden', 'true');
            };
            const showAr = () => {
                arModalOverlay.style.display = 'flex';
                arModalOverlay.setAttribute('aria-hidden', 'false');
            };
            const setError = (msg) => {
                if (msg) {
                    arError.textContent = msg;
                    arError.style.display = '';
                } else {
                    arError.textContent = '';
                    arError.style.display = 'none';
                }
            };
            const setBusy = (busy) => {
                arExportBtn.disabled = !!busy;
                arCancelBtn.disabled = !!busy;
                arCloseBtn.disabled = !!busy;
                arExportBtn.textContent = busy ? (this.config.i18n.Exporting || 'Đang xuất...') : (this.config.i18n.ExportExcel || 'Xuất Excel');
            };

            const switchTab = (tab) => {
                currentTab = tab;
                const activeClass = 'cm-btn cm-btn-primary';
                const inactiveClass = 'cm-btn cm-btn-outline';
                if (tab === 'hasCode') {
                    arTabHasCodeBody.style.display = '';
                    arTabNoCodeBody.style.display = 'none';
                    arTabHasCode.className = activeClass;
                    arTabNoCode.className = inactiveClass;
                } else {
                    arTabHasCodeBody.style.display = 'none';
                    arTabNoCodeBody.style.display = '';
                    arTabHasCode.className = inactiveClass;
                    arTabNoCode.className = activeClass;
                }
                setError('');
            };

            // Populate Section options
            const populateSections = () => {
                arSection.innerHTML = '';
                arSection2.innerHTML = '';
                const ph = document.createElement('option');
                ph.value = '';
                arSection.appendChild(ph);
                const ph2 = document.createElement('option');
                ph2.value = '';
                arSection2.appendChild(ph2);
                const srcDeptSel = this.elements.tableBody.querySelector(this.config.selectors.sectionSelect);
                if (srcDeptSel) {
                    Array.from(srcDeptSel.options).forEach((o) => {
                        if (!o || !o.text) return;
                        const opt = document.createElement('option');
                        opt.value = o.value || '';
                        opt.textContent = o.text || '';
                        arSection.appendChild(opt);
                        const opt2 = document.createElement('option');
                        opt2.value = o.value || '';
                        opt2.textContent = o.text || '';
                        arSection2.appendChild(opt2);
                    });
                }
                const updateSectionName = () => {
                    const txt = arSection.options[arSection.selectedIndex]?.text || '';
                    const parts = txt.split(' - ');
                    arSectionName.textContent = parts.length > 1 ? parts.slice(1).join(' - ') : '';
                };
                const updateSectionName2 = () => {
                    const txt = arSection2.options[arSection2.selectedIndex]?.text || '';
                    const parts = txt.split(' - ');
                    arSectionName2.textContent = parts.length > 1 ? parts.slice(1).join(' - ') : '';
                };
                arSection.onchange = updateSectionName;
                updateSectionName();
                arSection2.onchange = updateSectionName2;
                updateSectionName2();
            };

            // Load materials
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

            const loadMaterialPage = async (query = '') => {
                if (materialState.loading) return;
                if ((query || '') !== (materialState.lastQuery || '')) {
                    materialState.pageIndex = 1;
                    materialState.hasMore = true;
                    materialState.items = [];
                    arMaterialList.innerHTML = '';
                }
                if (!materialState.hasMore) return;
                materialState.loading = true;
                const body = { MaHang: query, Name: query || '', NhomHang: '', PageIndex: materialState.pageIndex, PageSize: materialState.pageSize };
                try {
                    const res = await fetch(this.config.api.searchMaterials, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
                    if (!res.ok) throw new Error(await res.text());
                    const data = await res.json();
                    const pageItems = Array.isArray(data) ? data.map(o => ({ code: o.material_Code || '', text: (o.material_Code || '') + ' - ' + (o.material_Name_VN || '') })) : [];
                    pageItems.forEach(it => {
                        if (!it.code) return;
                        materialState.items.push(it);
                        arMaterialList.appendChild(createItemEl(it));
                    });
                    if (pageItems.length < materialState.pageSize) materialState.hasMore = false;
                    else materialState.pageIndex++;
                    materialState.lastQuery = query || '';
                } catch (err) {
                    console.warn('Không thể tải danh sách vật tư (paged):', err);
                } finally {
                    materialState.loading = false;
                }
            };

            // Populate categories
            const populateCategories = () => {
                arCategoryList.innerHTML = '';
                const srcCategorySel = this.elements.tableBody.querySelector(this.config.selectors.categorySelect);
                if (srcCategorySel) {
                    Array.from(srcCategorySel.options).forEach((o) => {
                        const val = o.value || '';
                        const text = o.text || '';
                        if (!val) return;
                        const wrap = document.createElement('label');
                        wrap.style.display = 'flex';
                        wrap.style.minWidth = '350px';
                        wrap.style.alignItems = 'center';
                        wrap.style.gap = '8px';
                        wrap.style.padding = '4px 2px';
                        wrap.style.cursor = 'pointer';
                        wrap.dataset.search = (val + ' ' + text).toLowerCase();
                        const cb = document.createElement('input');
                        cb.type = 'checkbox';
                        cb.value = val;
                        const span = document.createElement('span');
                        span.textContent = text;
                        wrap.appendChild(cb);
                        wrap.appendChild(span);
                        arCategoryList.appendChild(wrap);
                    });
                }
            };

            // Event handlers
            arTabHasCode.onclick = () => switchTab('hasCode');
            arTabNoCode.onclick = () => switchTab('noCode');
            arCancelBtn.onclick = hideAr;
            arCloseBtn.onclick = hideAr;
            if (arModalOverlay.querySelector('[data-ar-action="overlay"]')) {
                arModalOverlay.querySelector('[data-ar-action="overlay"]').onclick = hideAr;
            }

            // Search and select all for materials
            let searchTimer = null;
            arSearch.oninput = () => {
                const q = (arSearch.value || '').toString();
                clearTimeout(searchTimer);
                searchTimer = setTimeout(() => loadMaterialPage(q), 300);
            };
            arSelectAll.onchange = () => {
                const visibleItems = Array.from(arMaterialList.querySelectorAll('label')).filter(el => el.style.display !== 'none');
                visibleItems.forEach(el => {
                    const cb = el.querySelector('input[type="checkbox"]');
                    if (cb) cb.checked = arSelectAll.checked;
                });
            };

            // Search and select all for categories
            arSearch2.oninput = () => {
                const q = (arSearch2.value || '').toLowerCase();
                Array.from(arCategoryList.children).forEach((el) => {
                    const s = el.dataset.search || '';
                    el.style.display = !q || s.includes(q) ? '' : 'none';
                });
            };
            arSelectAll2.onchange = () => {
                const visibleItems = Array.from(arCategoryList.querySelectorAll('label')).filter(el => el.style.display !== 'none');
                visibleItems.forEach(el => {
                    const cb = el.querySelector('input[type="checkbox"]');
                    if (cb) cb.checked = arSelectAll2.checked;
                });
            };

            // Infinite scroll for materials
            arMaterialList.addEventListener('scroll', () => {
                if (arMaterialList.scrollTop + arMaterialList.clientHeight >= arMaterialList.scrollHeight - 40) {
                    if (!materialState.loading && materialState.hasMore) {
                        loadMaterialPage(materialState.lastQuery);
                    }
                }
            });

            // Export handler
            arExportBtn.onclick = async () => {
                setError('');
                try {
                    setBusy(true);
                    this.showLoading(this.config.i18n.Exporting);
                    let sectionCode = '';
                    let sectionText = '';
                    let sectionName = '';
                    let selectedIds = [];
                    let endpoint = '';

                    if (currentTab === 'hasCode') {
                        sectionCode = arSection.value || '';
                        sectionText = arSection.options[arSection.selectedIndex]?.text || '';
                        sectionName = sectionText.split(' - ').slice(1).join(' - ');
                        selectedIds = Array.from(arMaterialList.querySelectorAll('input[type="checkbox"]:checked')).map(cb => cb.value);
                        endpoint = this.config.api.exportAutoRender;
                        if (!sectionCode) {
                            setError(this.config.i18n.MsgSelectSectionRequired || 'Vui lòng chọn mã phòng ban');
                            return;
                        }
                        if (selectedIds.length === 0) {
                            setError(this.config.i18n.MsgSelectAtLeastOneMaterial || 'Vui lòng chọn ít nhất một mã hàng nội bộ');
                            return;
                        }
                    } else {
                        sectionCode = arSection2.value || '';
                        sectionText = arSection2.options[arSection2.selectedIndex]?.text || '';
                        sectionName = sectionText.split(' - ').slice(1).join(' - ');
                        selectedIds = Array.from(arCategoryList.querySelectorAll('input[type="checkbox"]:checked')).map(cb => cb.value);
                        endpoint = this.config.api.exportRenderOutSide;
                        if (!sectionCode) {
                            setError(this.config.i18n.MsgSelectSectionRequired || 'Vui lòng chọn mã phòng ban');
                            return;
                        }
                        if (selectedIds.length === 0) {
                            setError(this.config.i18n.MsgSelectAtLeastOneCategory || 'Vui lòng chọn ít nhất một chủng loại hàng');
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
                    this.showDialog({ title: this.config.i18n.SuccessTitle, message: this.config.i18n.MsgExportedExcel || 'Đã xuất file Excel tự động.', type: 'success' });
                } catch (e) {
                    setError(e.message || this.config.i18n.MsgCannotExport || 'Không thể xuất file');
                } finally {
                    this.hideLoading();
                    setBusy(false);
                }
            };

            // Initialize
            populateSections();
            populateCategories();
            loadMaterialPage('');
            switchTab('hasCode');
            showAr();
        },

        cacheAutoRenderElements() {
            const selectors = this.config.selectors;
            this.elements.arModalOverlay = document.querySelector(selectors.arModalOverlay);
            this.elements.arCloseBtn = document.querySelector(selectors.arCloseBtn);
            this.elements.arCancelBtn = document.querySelector(selectors.arCancelBtn);
            this.elements.arExportBtn = document.querySelector(selectors.arExportBtn);
            this.elements.arError = document.querySelector(selectors.arError);
            this.elements.arTabHasCode = document.querySelector(selectors.arTabHasCode);
            this.elements.arTabNoCode = document.querySelector(selectors.arTabNoCode);
            this.elements.arTabHasCodeBody = document.querySelector(selectors.arTabHasCodeBody);
            this.elements.arTabNoCodeBody = document.querySelector(selectors.arTabNoCodeBody);
            this.elements.arSection = document.querySelector(selectors.arSection);
            this.elements.arSectionName = document.querySelector(selectors.arSectionName);
            this.elements.arMaterialList = document.querySelector(selectors.arMaterialList);
            this.elements.arSearch = document.querySelector(selectors.arSearch);
            this.elements.arSelectAll = document.querySelector(selectors.arSelectAll);
            this.elements.arSection2 = document.querySelector(selectors.arSection2);
            this.elements.arSectionName2 = document.querySelector(selectors.arSectionName2);
            this.elements.arCategoryList = document.querySelector(selectors.arCategoryList);
            this.elements.arSearch2 = document.querySelector(selectors.arSearch2);
            this.elements.arSelectAll2 = document.querySelector(selectors.arSelectAll2);
        }
    };

    document.addEventListener('DOMContentLoaded', () => app.init());

})(window);
