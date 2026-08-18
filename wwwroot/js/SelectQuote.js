if (typeof window.buildSearchableDropdown !== 'function') {
    function sqText(key, fallback) {
        const i18n = window.i18nSelectQuote || window.i18nQuotationResults || {};
        const value = i18n[key];
        return value == null || value === '' ? fallback : value;
    }

    // show dialog
    function getDialogEls() {
        const overlay = document.getElementById('cmDialogOverlay');
        const titleEl = document.getElementById('cmDialogTitle');
        const bodyEl = document.getElementById('cmDialogBody');
        const footerEl = document.getElementById('cmDialogFooter');
        return { overlay, titleEl, bodyEl, footerEl };
    }
    function showDialog({ title = sqText('Notification', 'Thông báo'), message = '', type = 'info', buttons } = {}) {
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
        okBtn.textContent = (buttons && buttons.okText) || sqText('DialogOk', 'Đồng ý');
        okBtn.addEventListener('click', () => hideDialog());
        footerEl.appendChild(okBtn);

        overlay.setAttribute('aria-hidden', 'false');
        overlay.style.display = 'flex';
        attachDialogCloseHandlers();
    }
    function showPrompt({ title = sqText('Notification', 'Thông báo'), message = '', placeholder = '', defaultValue = '' } = {}) {
        return new Promise((resolve) => {
            const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
            if (!overlay) {
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
            const inp = document.createElement('input');
            inp.type = 'text';
            inp.className = 'form-control';
            inp.placeholder = placeholder || '';
            inp.value = defaultValue || '';
            container.appendChild(inp);
            bodyEl.appendChild(container);

            footerEl.innerHTML = '';
            const btnCancel = document.createElement('button');
            btnCancel.className = 'cm-btn cm-btn-outline';
            btnCancel.textContent = sqText('Cancel', 'Hủy');
            btnCancel.addEventListener('click', () => {
                hideDialog();
                resolve(null);
            });
            const btnOk = document.createElement('button');
            btnOk.className = 'cm-btn cm-btn-primary';
            btnOk.textContent = sqText('Confirm', 'Đồng ý');
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
            setTimeout(() => { try { inp.focus(); inp.select(); } catch { } }, 50);
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
            if (msgEl) msgEl.textContent = sqText('ProcessingText', 'Đang xử lý...');
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
    (function () {
        function run() {
            initEnhancements();
        }
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', run);
        } else { run(); }
    })();
    function showModal() {
        const modalEl = document.getElementById('detailModal');
        if (!modalEl) return;
        try {
            const bs = window.bootstrap;
            if (bs && bs.Modal) {
                const m = bs.Modal.getOrCreateInstance(modalEl);
                m.show();
            } else {
                // Fallback: manually show modal
                modalEl.style.display = 'block';
                modalEl.classList.add('show');
                modalEl.setAttribute('aria-hidden', 'false');
                // prevent body scroll
                document.body.classList.add('modal-open');
            }
        } catch {
            // Fallback: manually show modal
            modalEl.style.display = 'block';
            modalEl.classList.add('show');
            modalEl.setAttribute('aria-hidden', 'false');
            document.body.classList.add('modal-open');
        }
    }
    function hideEditModal() {
        const modalEl = document.getElementById('detailModal');
        if (!modalEl) return;
        // Accessibility: if focus is inside modal, blur and move focus before hiding (to avoid aria-hidden ancestor with focused descendant)
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
    // Pagination and search logic for SelectQuoteSection page
    (function () {
        const locale = sqText('Locale', 'vi-VN');
        const TAB_ACTIVE = 'active';
        const TAB_ALL = 'all';

        const state = {
            currentTab: TAB_ACTIVE,
            tabs: {
                [TAB_ACTIVE]: { pageIndex: 1, pageSize: 20, totalCount: 0, selectedRows: new Set(), items: [] },
                [TAB_ALL]: { pageIndex: 1, pageSize: 20, totalCount: 0, selectedRows: new Set(), items: [] }
            }
        };

        const toRowKey = (maDon, maHang) => `${maDon || ''}|${maHang || ''}`;
        const getTabState = (tab) => state.tabs[tab] || state.tabs[TAB_ACTIVE];

        function getTabElements(tab) {
            const suffix = tab === TAB_ACTIVE ? 'Active' : 'All';
            return {
                tbody: document.getElementById(`sectionRequestBody${suffix}`),
                pageInfo: document.getElementById(`sectionPageInfo${suffix}`),
                pagination: document.getElementById(`sectionPagination${suffix}`),
                pageSize: document.getElementById(`sectionPageSizeSelect${suffix}`),
                selectAll: document.getElementById(`selectAll${suffix}`)
            };
        }

        function getFiltersPayload(tab) {
            const tabState = getTabState(tab);
            return {
                MaDon: document.getElementById('searchMaDon')?.value || '',
                Section: document.getElementById('searchPhongBan')?.value || '',
                MaVatTu: document.getElementById('searchMaterial')?.value || '',
                MaNcc: document.getElementById('searchSupplier')?.value || '',
                PageIndex: tabState.pageIndex,
                PageSize: tabState.pageSize
            };
        }

        function parseResponse(data) {
            const items = (data && data.data)
                ? (Array.isArray(data.data) ? data.data : (Array.isArray(data.data.data) ? data.data.data : []))
                : (Array.isArray(data) ? data : []);

            const total = (data && data.data && typeof data.data.totalCount === 'number')
                ? data.data.totalCount
                : (data && typeof data.totalCount === 'number' ? data.totalCount : items.length);

            return { items, total };
        }

        function parseDate(val) {
            if (!val) return null;
            const d = new Date(val);
            return Number.isNaN(d.getTime()) ? null : d;
        }

        function formatDateText(val) {
            const d = parseDate(val);
            return d ? d.toLocaleDateString(locale) : '';
        }

        function getExpiryStatus(item, tab) {
            const expiry = parseDate(item.DTM_ExpiryDate || item.dtm_ExpiryDate || item.ExpiryDate || item.NgayHetHan || item.DTM_KyHan || item.dtm_KyHan);
            if (!expiry) {
                return { label: sqText('Done', 'Hoàn thành'), className: 'bg-secondary' };
            }

            const today = new Date();
            today.setHours(0, 0, 0, 0);
            const e = new Date(expiry);
            e.setHours(0, 0, 0, 0);
            const days = Math.ceil((e - today) / (1000 * 60 * 60 * 24));

            if (days < 0) {
                if (tab === TAB_ACTIVE) {
                    return { label: sqText('StatusActive', 'Còn hiệu lực'), className: 'bg-success' };
                }
                return { label: sqText('StatusExpired', 'Hết hạn'), className: 'bg-danger' };
            }
            if (days < 30) {
                return { label: sqText('StatusSoonExpired', 'Sắp hết hạn'), className: 'bg-warning text-dark' };
            }
            return { label: sqText('StatusActive', 'Còn hiệu lực'), className: 'bg-success' };
        }

        function updateDashboard() {
            const activeState = getTabState(TAB_ACTIVE);
            const allState = getTabState(TAB_ALL);

            const countActive = document.getElementById('countActiveTab');
            if (countActive) {
                countActive.textContent = String(activeState.totalCount || 0);
                countActive.style.color = '#fff';
            }
            const countAll = document.getElementById('countAllTab');
            if (countAll) {
                countAll.textContent = String(allState.totalCount || 0);
                countAll.style.color = '#fff';
            }

            const summary = document.getElementById('summaryText');
            const currentState = getTabState(state.currentTab);
            if (summary) {
                summary.textContent = sqText('SummaryRecords', '{0} bản ghi').replace('{0}', currentState.totalCount || 0);
            }
        }
        function costCell(flUsed) {
            if (flUsed != null && flUsed !== '') {
                const value = parseFloat(flUsed);
                return `<td class="text-center">${value.toFixed(4).replace(/\.?0+$/, '')} USD</td>`;
            }
            return `<td></td>`;
        }
        function renderTable(tab, items) {
            const tabState = getTabState(tab);
            const els = getTabElements(tab);
            const tbody = els.tbody;
            if (!tbody) return;
            const isActiveTab = tab === TAB_ACTIVE;

            tbody.innerHTML = '';

            if (!items || !items.length) {
                tbody.innerHTML = `<tr><td colspan="${isActiveTab ? 9 : 11}" class="text-center text-muted py-3">${sqText('NoDataText', 'Không có dữ liệu')}</td></tr>`;
                return;
            }

            items.forEach(item => {
                const tr = document.createElement('tr');
                const orderCode = item.CHR_MaDon || '';
                const section = item.PhongYeuCau || '';
                const materialCode = item.CHR_MaHangNoiBo || '';
                const itemName = item.TenHang || '';
                const itemNameEN = item.TenHangEN || '';
                const unit = item.DonVi || '';
                const price = item.FL_USD || 0;
                const requester = item.NguoiYeuCau || '';
                const wantDate = item.DTM_NgayMuonNhan ? new Date(item.DTM_NgayMuonNhan).toLocaleDateString(locale) : '';
                const expiryDate = formatDateText(item.DTM_ExpiryDate || item.dtm_ExpiryDate || item.ExpiryDate || item.NgayHetHan || item.DTM_KyHan || item.dtm_KyHan);
                const supplier = item.ShortName || item.NVCHR_NameNCC || '';
                const processStatus = item.TrangThai === "Done" ? sqText('Done', 'Hoàn thành') : (item.TrangThai || '');
                const rowKey = toRowKey(orderCode, materialCode);
                const checkedAttr = tabState.selectedRows.has(rowKey) ? 'checked' : '';
                const status = getExpiryStatus(item, tab);
                const links = item.NVCHR_File || '';

                if (isActiveTab) {
                    tr.innerHTML = `
                        <td class="text-center">
                            <input type="checkbox" class="row-select" data-tab="${tab}" data-madon="${orderCode}" data-material="${materialCode}" ${checkedAttr}>
                        </td>
                        <td class="text-center">${orderCode}</td>
                        <td class="text-center">${materialCode}</td>
                        <td>${itemName}</td>
                        <td>${itemNameEN}</td>
                        <td>${supplier}</td>
                        ${costCell(price)}
                        <td class="text-center">${expiryDate}</td>
                        <td class="text-center"><span class="badge status-pill ${status.className}">${status.label}</span></td>
                        <td class="text-center table-actions">
                            <button type="button" class="btn btn-sm btn-outline-primary btn-view-detail" data-madon="${orderCode}" data-material="${materialCode}">
                                <i class="fas fa-info"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-success btn-view-download" data-links="${links}">
                                <i class="fas fa-download"></i>
                            </button>
                        </td>`;
                } else { //${sqText('DetailButtonText', 'Chi tiết')}
                    tr.innerHTML = `
                        <td class="text-center">
                            <input type="checkbox" class="row-select" data-tab="${tab}" data-madon="${orderCode}" data-material="${materialCode}" ${checkedAttr}>
                        </td>
                        <td class="text-center">${orderCode}</td>
                        <td class="text-center">${section}</td>
                        <td class="text-center">${materialCode}</td>
                        <td>${itemName}</td>
                        <td>${itemNameEN}</td>
                        <td class="text-center">${unit}</td>
                        <td class="text-center">${requester}</td>
                        <td class="text-center">${wantDate}</td>
                        <td class="text-center"><span class="badge bg-info text-dark">${processStatus}</span></td>
                        <td class="text-center table-actions">
                            <button type="button" class="btn btn-sm btn-outline-primary btn-view-detail" data-madon="${orderCode}" data-material="${materialCode}">
                                <i class="fas fa-circle-info me-1"></i>${sqText('DetailButtonText', 'Chi tiết')}
                            </button>
                        </td>`;
                }
                tbody.appendChild(tr);
            });

            tbody.querySelectorAll('.btn-view-detail').forEach(btn => {
                btn.addEventListener('click', function () {
                    const maDon = this.dataset.madon;
                    const maHang = this.dataset.material;
                    if (typeof showDetail === 'function') showDetail(maDon, maHang);
                });
            });

            tbody.querySelectorAll('.btn-view-download').forEach(btn => {
                btn.addEventListener('click', function () {
                    const link = this.dataset.links;

                    if (typeof download === 'function') download(link);
                });
            });


            if (els.selectAll) {
                const allRows = tbody.querySelectorAll('.row-select');
                els.selectAll.checked = allRows.length > 0 && [...allRows].every(x => x.checked);
            }
        }

        async function download(link) {
            if (!link) {
                showDialog({ message: sqText('QuoteFileLinkMissing', 'Link file báo giá không có giá trị'), type: 'info' });
                return;
            }
            try {
                showLoading(sqText('LoadingData', 'Đang tải dữ liệu...'));

                const response = await fetch((window.apiBaseUrl || '') + '/SelectQuote/DownloadQuoteFile', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(link)
                });

                if (!response.ok) {
                    const errText = await response.text();
                    throw new Error(errText || "Download thất bại");
                }

                const blob = await response.blob();

                // tạo link download
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;

                // lấy tên file từ path
                a.download = link.split('/').pop() || 'download';
                document.body.appendChild(a);
                a.click();

                a.remove();
                window.URL.revokeObjectURL(url);

            } catch (e) {
                showDialog({ message: `${sqText('DetailLoadErrorPrefix', 'Lỗi tải chi tiết')}: ${e && e.message ? e.message : e}`, type: 'error' });
            } finally {
                hideLoading();
            }
        }

        async function showDetail(maDon, maHang) {
            if (!maDon || !maHang) {
                showDialog({ message: sqText('OrderMaterialRequired', 'Mã đơn và Mã hàng không được để trống'), type: 'info' });
                return;
            }

            try {
                showLoading(sqText('LoadingData', 'Đang tải dữ liệu...'));
                const res = await fetch((window.apiBaseUrl || '') + '/SelectQuote/GetQuoteDetails', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ maDon, maHang })
                });

                if (!res.ok) {
                    const txt = await res.text();
                    throw new Error(txt || sqText('ServerError', 'Server error'));
                }

                const data = await res.json();
                const items = Array.isArray(data) ? data : [];
                window._selectQuoteGroups = window._selectQuoteGroups || {};
                window._selectQuoteGroups[maDon] = items;
                showGroupDetail(maDon);
            } catch (e) {
                showDialog({ message: `${sqText('DetailLoadErrorPrefix', 'Lỗi tải chi tiết')}: ${e && e.message ? e.message : e}`, type: 'error' });
            } finally {
                hideLoading();
            }
        }
        window.showDetail = showDetail;

        function showGroupDetail(maDon) {
            const groups = window._selectQuoteGroups || {};
            const items = groups[maDon] || [];
            // populate modal header basic info from first item
            const first = items[0] || {};
            document.getElementById('madonhang').textContent = maDon;
            document.getElementById('khoi').textContent = first.CHR_SectionName || '';
            document.getElementById('mpb_yc').textContent = first.CHR_SectionCode || '';
            document.getElementById('tenphongban').textContent = first.CHR_SectionName || '';
            document.getElementById('nyc').textContent = first.DTM_NgayMuonNhan ? new Date(first.DTM_NgayMuonNhan).toLocaleDateString(locale) : '';
            document.getElementById('thmm').textContent = first.DTM_KyHan ? new Date(first.DTM_KyHan).toLocaleDateString(locale) : '';
            document.getElementById('requester').textContent = first.CHR_CreateBy || '-';
            document.getElementById('id_request').textContent = first.ID || maDon;
            document.getElementById('step').textContent = first.ID_StepBaoGia || '';
            document.getElementById('regency').textContent = first.ID_Status || '';

            // urgent badge
            const ub = document.getElementById('urgent-badge');
            const gap = first.CHR_Gap;
            const isUrgent = gap === true || String(gap).toLowerCase() === 'true' || String(gap) === '1' || String(gap).toLowerCase() === 'o';
            if (ub) ub.style.display = isUrgent ? '' : 'none';

            const body = document.getElementById('detailModalBody');
            if (!body) return;
            body.innerHTML = '';
            // render rows using DOM for safety
            const frag = document.createDocumentFragment();
            // helper to render mismatch styling for vendor comparison fields
            const mismatchStyle = (v) => {
                if (v === false || v === 0 || v === '0' || String(v).toLowerCase() === 'false') {
                    return 'color: #a00; background-color: #ffecec;';
                }
                return '';
            };
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
                    if (!isNaN(d.getTime())) return d.toLocaleDateString(locale);
                } catch { }
                return String(val || '');
            };
            const fmtNum = v => { try { return v != null && v !== '' ? Number(v).toLocaleString() : ''; } catch { return v || ''; } };
            items.forEach((d, idx) => {
                const tr = document.createElement('tr');
                const pick = getVal(d, 'BIT_Select', 'bit_Select');
                tr.className = 'text-center' + (pick === false || String(pick).toLowerCase() === 'false' ? ' table-secondary' : '');

                const addTd = (txt, cls, style) => { const td = document.createElement('td'); td.textContent = txt == null ? '' : String(txt); if (cls) td.className = cls; if (style) td.style.cssText = style; return td; };

                tr.appendChild(addTd(idx + 1));
                tr.appendChild(addTd(getVal(d, 'chR_MaHangNoiBo', 'CHR_MaHangNoiBo', 'CHR_MaHangNoiBo')));
                tr.appendChild(addTd(getVal(d, 'chR_MaHangNoiBo', 'CHR_MaHangNoiBo')));
                tr.appendChild(addTd(getVal(d, 'nvchR_ChungLoai', 'NVCHR_ChungLoai')));
                tr.appendChild(addTd(getVal(d, 'chR_Phanloai', 'CHR_Phanloai')));
                tr.appendChild(addTd(getVal(d, 'chR_MaHangNCC', 'CHR_MaHangNCC')));
                // name VN/EN combined
                const nameVN = getVal(d, 'nvchR_NameVN', 'NVCHR_NameVN') || '';
                const nameEN = getVal(d, 'nvchR_NameEN', 'NVCHR_NameEN') || '';
                tr.appendChild(addTd((nameVN + (nameEN ? ' / ' + nameEN : '')).trim(), 'text-start'));

                tr.appendChild(addTd(getVal(d, 'inT_SoLuong', 'INT_SoLuong') || '', 'text-center'));
                tr.appendChild(addTd(getVal(d, 'nvchR_DonVi', 'NVCHR_DonVi') || '', 'text-center'));
                tr.appendChild(addTd(getVal(d, 'nvchR_HinhDang', 'NVCHR_HinhDang')));
                tr.appendChild(addTd(getVal(d, 'nvchR_ChatLieu', 'NVCHR_ChatLieu')));
                tr.appendChild(addTd(getVal(d, 'nvchR_ThanhPhan', 'NVCHR_ThanhPhan')));
                tr.appendChild(addTd(getVal(d, 'nvchR_KichThuoc', 'NVCHR_KichThuoc')));
                tr.appendChild(addTd(getVal(d, 'nvchR_DongMay', 'NVCHR_DongMay')));
                tr.appendChild(addTd(getVal(d, 'nvchR_TinhNang', 'NVCHR_TinhNang')));

                tr.appendChild(addTd(getVal(d, 'nvchR_FileThietKe', 'NVCHR_FileThietKe')));
                tr.appendChild(addTd(getVal(d, 'nvchR_NhaSanXuat', 'NVCHR_NhaSanXuat')));
                tr.appendChild(addTd(getVal(d, 'chR_MaNCC', 'CHR_MaNCC')));
                tr.appendChild(addTd(getVal(d, 'nvchR_TenNCC', 'NVCHR_TenNCC')));
                tr.appendChild(addTd(getVal(d, 'nvchR_Rohs', 'NVCHR_Rohs')));
                tr.appendChild(addTd(getVal(d, 'nvchR_COCQ', 'NVCHR_COCQ')));
                tr.appendChild(addTd(getVal(d, 'nvchR_MSDS', 'NVCHR_MSDS')));
                tr.appendChild(addTd(getVal(d, 'nvchR_AnToan', 'NVCHR_AnToan')));

                tr.appendChild(addTd(formatDate(getVal(d, 'dtM_KyHan', 'DTM_KyHan')), 'text-center'));
                const gap = getVal(d, 'chR_Gap', 'CHR_Gap');
                const gapLabel = gap != null && gap !== '' ? (String(gap).toLowerCase() === 'true' || String(gap) === '1' ? 'O' : 'X') : '';
                tr.appendChild(addTd(gapLabel, 'text-center'));
                const lay = getVal(d, 'biT_LayBaoGia', 'BIT_LayBaoGia');
                const layLabel = lay != null && lay !== '' ? (String(lay).toLowerCase() === 'true' || String(lay) === '1' ? 'O' : 'X') : '';
                tr.appendChild(addTd(layLabel, 'text-center'));
                tr.appendChild(addTd(getVal(d, 'nvchR_LyDo', 'NVCHR_LyDo')));

                // Vendor input columns (read-only)
                tr.appendChild(addTd(getVal(d, 'CHR_MaNCC', 'chR_MaNCC')));
                tr.appendChild(addTd(getVal(d, 'NVCHR_NameNCC', 'nvchR_NameNCC'), 'text-start'));
                tr.appendChild(addTd(getVal(d, 'CHR_MaHangNCC', 'chR_MaHangNCC'), null, mismatchStyle(getVal(d, 'IsMatch_MaHangNCC', 'IsMatch_MaHangNCC'))));
                tr.appendChild(addTd(getVal(d, 'NVCHR_TenHangHQ', 'nvchR_TenHangHQ'), 'text-start', mismatchStyle(getVal(d, 'IsMatch_NameVN', 'IsMatch_NameVN'))));
                tr.appendChild(addTd(getVal(d, 'NameENByNCC', 'nameENByNCC'), null, mismatchStyle(getVal(d, 'IsMatch_NameEN', 'IsMatch_NameEN'))));
                tr.appendChild(addTd(getVal(d, 'soluong', 'INT_SoLuong', 'soluong') || '', 'text-center', mismatchStyle(getVal(d, 'IsMatch_SoLuong', 'IsMatch_SoLuong'))));
                tr.appendChild(addTd(getVal(d, 'donvi', 'NVCHR_DonVi') || '', 'text-center', mismatchStyle(getVal(d, 'IsMatch_DonVi', 'IsMatch_DonVi'))));
                tr.appendChild(addTd(fmtNum(getVal(d, 'FL_USD', 'fl_usd')), 'text-end'));
                tr.appendChild(addTd(fmtNum(getVal(d, 'FL_VND', 'fl_vnd')), 'text-end'));
                tr.appendChild(addTd(getVal(d, 'NVCHR_MOQ', 'nvchr_MOQ')));
                tr.appendChild(addTd(getVal(d, 'DTM_LeadTime', 'dtm_LeadTime')));
                tr.appendChild(addTd(formatDate(getVal(d, 'DTM_ShipTime', 'dtm_ShipTime')), null, mismatchStyle(getVal(d, 'IsMatch_Ngay', 'IsMatch_Ngay'))));
                tr.appendChild(addTd(getVal(d, 'VCHR_Rohs', 'vchr_Rohs'), null, mismatchStyle(getVal(d, 'IsMatch_Rohs', 'IsMatch_Rohs'))));
                tr.appendChild(addTd(getVal(d, 'VCHR_COCQ', 'vchr_COCQ'), null, mismatchStyle(getVal(d, 'IsMatch_COCQ', 'IsMatch_COCQ'))));
                tr.appendChild(addTd(getVal(d, 'VCHR_MSDS', 'vchr_MSDS'), null, mismatchStyle(getVal(d, 'IsMatch_MSDS', 'IsMatch_MSDS'))));
                tr.appendChild(addTd(getVal(d, 'VCHR_AnToan', 'vchr_AnToan'), null, mismatchStyle(getVal(d, 'IsMatch_AnToan', 'IsMatch_AnToan'))));
                tr.appendChild(addTd(getVal(d, 'VCHR_CamKet', 'vchr_CamKet'), null, mismatchStyle(getVal(d, 'IsMatchCamKet', 'IsMatchCamKet'))));
                tr.appendChild(addTd(getVal(d, 'NVCHR_DeliveryTerm', 'nvchr_DeliveryTerm')));
                tr.appendChild(addTd(getVal(d, 'NVCHR_PaymentTerm', 'nvchr_PaymentTerm')));
                tr.appendChild(addTd(getVal(d, 'NVCHR_File', 'nvchr_File')));
                tr.appendChild(addTd(formatDate(getVal(d, 'DTM_EffectiveDate', 'dtm_EffectiveDate'))));
                tr.appendChild(addTd(formatDate(getVal(d, 'DTM_ExpiryDate', 'dtm_ExpiryDate'))));

                // System total (try VND then USD)
                const totalSys = (getVal(d, 'FL_VND', 'fl_vnd') || getVal(d, 'TotalVND')) ? (fmtNum(getVal(d, 'FL_VND', 'fl_vnd') || getVal(d, 'TotalVND')) + ' VND') : (getVal(d, 'FL_USD', 'fl_usd') ? (fmtNum(getVal(d, 'FL_USD', 'fl_vnd')) + ' USD') : '');
                tr.appendChild(addTd(totalSys, 'text-center'));

                // PIC selection and reason (display only)
                //const pick = getVal(d, 'BIT_Select', 'bit_Select');
                const pickLabel = pick === true || String(pick).toLowerCase() === 'true' ? 'O' : (pick === false || String(pick).toLowerCase() === 'false' ? 'X' : '');
                tr.appendChild(addTd(pickLabel, 'text-center'));
                tr.appendChild(addTd(getVal(d, 'NVCHR_ReasonPick', 'nvchr_ReasonPick') || getVal(d, 'NVCHR_LyDo', 'nvchr_LyDo')));
                tr.appendChild(addTd(getVal(d, 'NVCHR_Note', 'nvchr_Note')));
                // approval
                tr.appendChild(addTd(getVal(d, 'userQlsc')));
                tr.appendChild(addTd((getVal(d, 'lyDoQlsc') === null || getVal(d, 'lyDoQlsc') === "") ? "OK" : "NG"));
                tr.appendChild(addTd(getVal(d, 'lyDoQlsc')));

                tr.appendChild(addTd(getVal(d, 'userQltc')));
                tr.appendChild(addTd((getVal(d, 'lyDoQltc') === null || getVal(d, 'lyDoQltc') === "") ? "OK" : "NG"));
                tr.appendChild(addTd(getVal(d, 'lyDoQltc')));

                tr.appendChild(addTd(getVal(d, 'userDeft')));
                tr.appendChild(addTd((getVal(d, 'lyDoDeft') === null || getVal(d, 'lyDoDeft') === "") ? "OK" : "NG"));
                tr.appendChild(addTd(getVal(d, 'lyDoDeft')));

                frag.appendChild(tr);
            });
            body.appendChild(frag);

            // show modal
            showModal();
        }

        function renderPagination(tab) {
            const tabState = getTabState(tab);
            const els = getTabElements(tab);
            const container = els.pagination;
            const pageInfo = els.pageInfo;
            if (!container) return;
            container.innerHTML = '';
            const total = tabState.totalCount || 0;
            const last = Math.max(1, Math.ceil(total / tabState.pageSize));

            const createBtn = (text, disabled, cb) => {
                const b = document.createElement('button');
                b.type = 'button';
                b.className = 'btn btn-sm btn-outline-primary';
                b.textContent = text;
                if (disabled) b.disabled = true;
                b.addEventListener('click', cb);
                return b;
            };

            container.appendChild(createBtn('<<', tabState.pageIndex <= 1, () => {
                tabState.pageIndex = 1;
                doSearch(tab);
            }));
            container.appendChild(createBtn('<', tabState.pageIndex <= 1, () => {
                tabState.pageIndex = Math.max(1, tabState.pageIndex - 1);
                doSearch(tab);
            }));

            const info = document.createElement('span');
            info.className = 'btn btn-sm disabled';
            container.appendChild(info);

            info.textContent = sqText('PageLabelTemplate', 'Trang {0} / {1}')
                .replace('{0}', tabState.pageIndex)
                .replace('{1}', last);

            container.appendChild(createBtn('>', tabState.pageIndex >= last, () => {
                tabState.pageIndex = Math.min(last, tabState.pageIndex + 1);
                doSearch(tab);
            }));
            container.appendChild(createBtn('>>', tabState.pageIndex >= last, () => {
                tabState.pageIndex = last;
                doSearch(tab);
            }));

            if (pageInfo) {
                const showing = Math.max(0, Math.min(tabState.pageSize, total - (tabState.pageIndex - 1) * tabState.pageSize));
                pageInfo.textContent = sqText('PageInfoTemplate', 'Hiển thị {0} / {1}')
                    .replace('{0}', showing)
                    .replace('{1}', total);
            }
        }

        // export file
        async function exportFile() {
            try {
                showLoading();
                const payload = {
                    MaDon: document.getElementById('searchMaDon')?.value || '',
                    Section: document.getElementById('searchPhongBan')?.value || '',
                    MaVatTu: document.getElementById('searchMaterial')?.value || '',
                    MaNcc: document.getElementById('searchSupplier')?.value || '',
                    PageIndex: getTabState(state.currentTab).pageIndex,
                    PageSize: getTabState(state.currentTab).pageSize
                };
                const res = await fetch((window.apiBaseUrl || '') + '/SelectQuote/ExportSelectedGroups', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
                });
                if (!res.ok) {
                    const txt = await res.text();
                    throw new Error(txt || 'Server error');
                }
                const blob = await res.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = 'QuoteSection.xlsx';
                document.body.appendChild(a);
                a.click();
                a.remove();
            } catch (e) {
                console.error('Search error', e);
                showDialog({ message: `${sqText('SearchErrorPrefix', 'Lỗi tìm kiếm')}: ${e && e.message ? e.message : e}`, type: 'error' });
            } finally {
                hideLoading();
            }
        }

        async function doSearch(tab) {
            try {
                showLoading();
                tab = tab || state.currentTab;
                const payload = getFiltersPayload(tab);
                const endpoint = tab === TAB_ACTIVE
                    ? '/SelectQuote/GetActiveQuotes'
                    : '/SelectQuote/SearchQuoteSection';

                const res = await fetch((window.apiBaseUrl || '') + endpoint, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
                });
                if (!res.ok) {
                    const txt = await res.text();
                    throw new Error(txt || 'Server error');
                }
                const data = await res.json();
                const parsed = parseResponse(data);
                const tabState = getTabState(tab);
                tabState.items = parsed.items;
                tabState.totalCount = parsed.total;
                renderTable(tab, parsed.items);
                renderPagination(tab);
                updateDashboard();
            } catch (e) {
                console.error('Search error', e);
                showDialog({ message: `${sqText('SearchErrorPrefix', 'Lỗi tìm kiếm')}: ${e && e.message ? e.message : e}`, type: 'error' });
            } finally {
                hideLoading();
            }
        }

        async function refreshBothTabs() {
            await doSearch(TAB_ACTIVE);
            await doSearch(TAB_ALL);
        }

        async function searchBaoGiaConHieuLuc(resetPage) {
            if (resetPage) getTabState(TAB_ACTIVE).pageIndex = 1;
            state.currentTab = TAB_ACTIVE;
            await doSearch(TAB_ACTIVE);
        }

        async function searchTatCaBaoGia(resetPage) {
            if (resetPage) getTabState(TAB_ALL).pageIndex = 1;
            state.currentTab = TAB_ALL;
            await doSearch(TAB_ALL);
        }

        window.searchBaoGiaConHieuLuc = searchBaoGiaConHieuLuc;
        window.searchTatCaBaoGia = searchTatCaBaoGia;

        function activateTab(tab) {
            const activeBtn = document.getElementById('tab-active-quotes');
            const allBtn = document.getElementById('tab-all-quotes');
            const activePane = document.getElementById('pane-active-quotes');
            const allPane = document.getElementById('pane-all-quotes');

            const isActive = tab === TAB_ACTIVE;
            state.currentTab = isActive ? TAB_ACTIVE : TAB_ALL;

            if (activeBtn) {
                activeBtn.classList.toggle('active', isActive);
                activeBtn.setAttribute('aria-selected', isActive ? 'true' : 'false');
            }
            if (allBtn) {
                allBtn.classList.toggle('active', !isActive);
                allBtn.setAttribute('aria-selected', !isActive ? 'true' : 'false');
            }
            if (activePane) {
                activePane.classList.toggle('show', isActive);
                activePane.classList.toggle('active', isActive);
            }
            if (allPane) {
                allPane.classList.toggle('show', !isActive);
                allPane.classList.toggle('active', !isActive);
            }

            updateDashboard();
        }

        // wire buttons and controls
        document.addEventListener('DOMContentLoaded', function () {
            // search button
            document.getElementById('btnSearch')?.addEventListener('click', function () {
                getTabState(TAB_ACTIVE).pageIndex = 1;
                getTabState(TAB_ALL).pageIndex = 1;
                refreshBothTabs();
            });
            // clear
            document.getElementById('btnClear')?.addEventListener('click', function () {
                const form = document.getElementById('filterForm'); if (form) form.reset();
                // reset any enhanced selects
                document.querySelectorAll('select.searchable-select').forEach(s => { s.value = ''; try { s.dispatchEvent(new Event('change', { bubbles: true })); } catch { } });
                getTabState(TAB_ACTIVE).pageIndex = 1;
                getTabState(TAB_ALL).pageIndex = 1;
                refreshBothTabs();
            });
            // Export file button
            document.getElementById('btnExportExcel')?.addEventListener('click', function () {
                exportFile();
            });
            // page size
            [TAB_ACTIVE, TAB_ALL].forEach(tab => {
                const els = getTabElements(tab);
                const tabState = getTabState(tab);
                if (els.pageSize) {
                    els.pageSize.value = tabState.pageSize.toString();
                    els.pageSize.addEventListener('change', function () {
                        tabState.pageSize = parseInt(els.pageSize.value, 10) || 20;
                        tabState.pageIndex = 1;
                        doSearch(tab);
                    });
                }
            });

            document.addEventListener('change', function (e) {
                const row = e.target.closest('.row-select');
                if (!row) return;
                const tab = row.dataset.tab || TAB_ACTIVE;
                const tabState = getTabState(tab);
                const key = toRowKey(row.dataset.madon, row.dataset.material);
                if (row.checked) tabState.selectedRows.add(key);
                else tabState.selectedRows.delete(key);

                const els = getTabElements(tab);
                if (els.selectAll && els.tbody) {
                    const allRows = els.tbody.querySelectorAll('.row-select');
                    els.selectAll.checked = allRows.length > 0 && [...allRows].every(x => x.checked);
                }
            });

            document.querySelectorAll('.select-all').forEach(el => {
                el.addEventListener('change', function () {
                    const tab = this.dataset.tab || TAB_ACTIVE;
                    const els = getTabElements(tab);
                    const tabState = getTabState(tab);
                    if (!els.tbody) return;
                    const rows = els.tbody.querySelectorAll('.row-select');
                    rows.forEach(r => {
                        r.checked = this.checked;
                        const key = toRowKey(r.dataset.madon, r.dataset.material);
                        if (this.checked) tabState.selectedRows.add(key);
                        else tabState.selectedRows.delete(key);
                    });
                });
            });

            document.querySelectorAll('#quoteTabs button[data-tab]').forEach(tabBtn => {
                tabBtn.addEventListener('click', function (e) {
                    e.preventDefault();
                    const key = this.getAttribute('data-tab') || TAB_ACTIVE;
                    activateTab(key);
                    if (key === TAB_ACTIVE) {
                        searchBaoGiaConHieuLuc(false);
                    } else {
                        searchTatCaBaoGia(false);
                    }
                });
            });
            // export selected
            //const btnExportSelected = document.getElementById('btnExportSelected');
            //if (btnExportSelected) {
            //    btnExportSelected.addEventListener('click', function () {
            //        const selected = getSelectedOrderCodes();
            //        if (!selected.length) {
            //            showDialog({ message: sqText('ExportSelectAtLeastOne', 'Vui lòng chọn ít nhất một nhóm để xuất.'), type: 'info' });
            //            return;
            //        }
            //        // call export API with selected MaDon
            //        fetch((window.apiBaseUrl || '') + '/SelectQuote/ExportSelectedGroups', {
            //            method: 'POST',
            //            headers: { 'Content-Type': 'application/json' },
            //            body: JSON.stringify(selected)
            //        })
            //            .then(res => {
            //                if (!res.ok) throw new Error(sqText('ExportFailed', 'Xuất file thất bại'));
            //                return res.blob();
            //            })
            //            .then(blob => {
            //                const url = window.URL.createObjectURL(blob);
            //                const a = document.createElement('a');
            //                a.href = url;
            //                a.download = `SelectedGroups_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.xlsx`;
            //                document.body.appendChild(a);
            //                a.click();
            //                a.remove();
            //                window.URL.revokeObjectURL(url);
            //            })
            //            .catch(err => {
            //                showDialog({ message: `${sqText('ExportErrorPrefix', 'Lỗi xuất file')}: ${err.message}`, type: 'error' });
            //            });
            //    });
            //}

            activateTab(TAB_ACTIVE);
            refreshBothTabs();
        });
    })()
};
