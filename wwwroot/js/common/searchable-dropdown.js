(function () {
    'use strict';

    function callRemoteApi(url, body) {
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body || {})
        }).then(function (r) {
            if (!r.ok) {
                return r.text().then(function (t) { throw (t || ('HTTP ' + r.status)); });
            }
            return r.json();
        });
    }

    function buildSearchableDropdown(container) {
        var root = (window.jQuery && container && container.jquery) ? container[0] : (container || document);
        var selects = root.querySelectorAll ? root.querySelectorAll('select.searchable-select') : [];

        selects.forEach(function (select) {
            if (select.dataset.searchDropdown === 'true') return;

            var options = Array.from(select.options).map(function (opt) {
                return { value: opt.value, text: opt.textContent || opt.innerText || '', selected: opt.selected };
            });

            var remoteEnabled = String(select.dataset.remote || '').toLowerCase() === 'true';
            var remoteState = {
                enabled: remoteEnabled,
                api: select.dataset.api || '/InputQuotation/GetSearchMaterial',
                pageSize: Math.max(1, parseInt(select.dataset.pageSize || '50', 10) || 50),
                nextPage: 1,
                query: '',
                loading: false,
                lastPage: false,
                requestToken: 0,
                debounceTimer: null
            };

            var wrapper = document.createElement('div'); wrapper.className = 'ms-container';
            var btn = document.createElement('div'); btn.className = 'ms-btn';
            btn.innerHTML = '<span class="ms-values"></span><span class="ms-placeholder"></span><span class="ms-caret">▾</span>';
            var dropdown = document.createElement('div'); dropdown.className = 'ms-dropdown';
            var search = document.createElement('div'); search.className = 'ms-search';
            var T = window.i18nInputQuote || {};
            search.innerHTML = '<input type="text" placeholder="' + (T.SearchEllipsis || 'Tìm...') + '" />';
            var list = document.createElement('div'); list.className = 'ms-list';

            function rebuildOptionsFromSelect() {
                options = Array.from(select.options).map(function (opt) {
                    return { value: opt.value, text: opt.textContent || opt.innerText || '', selected: opt.selected };
                });
            }

            function clearRemoteOptionsKeepPlaceholder() {
                var placeholder = select.options.length > 0 ? select.options[0] : null;
                select.innerHTML = '';
                if (placeholder) {
                    select.appendChild(placeholder);
                    placeholder.selected = !select.value;
                }
                rebuildOptionsFromSelect();
            }

            function appendRemoteItems(items) {
                var existingValues = new Set(Array.from(select.options).map(function (o) { return o.value; }));
                (items || []).forEach(function (it) {
                    var code = String(it.Material_Code || it.material_Code || it.materialCode || '').trim();
                    if (!code || existingValues.has(code)) return;
                    var name = String(it.Material_Name_VN || it.material_Name_VN || it.materialNameVN || '').trim();
                    var text = name ? (code + ' - ' + name) : code;
                    var opt = document.createElement('option');
                    opt.value = code;
                    opt.text = text;
                    select.appendChild(opt);
                    existingValues.add(code);
                });
                rebuildOptionsFromSelect();
            }

            function initRemotePaginationFromCurrentOptions() {
                var nonEmptyCount = options.filter(function (o) { return !!o.value; }).length;
                remoteState.nextPage = Math.floor(nonEmptyCount / remoteState.pageSize) + 1;
                remoteState.lastPage = nonEmptyCount > 0 && nonEmptyCount < remoteState.pageSize;
            }

            function renderList(query, scrollTop) {
                var q = (query || '').toLowerCase();
                list.innerHTML = '';
                var hasItems = false;

                options.forEach(function (opt) {
                    if (!q || opt.text.toLowerCase().includes(q)) {
                        var item = document.createElement('div');
                        item.className = 'ms-item';
                        item.dataset.value = opt.value;
                        item.textContent = opt.text;
                        if (select.value === opt.value || opt.selected) item.classList.add('selected');
                        list.appendChild(item);
                        hasItems = true;
                    }
                });

                if (!hasItems) {
                    var empty = document.createElement('div');
                    empty.className = 'ms-empty';
                    empty.textContent = (window.i18nInputQuote && window.i18nInputQuote.NoResults) || 'Không có kết quả';
                    list.appendChild(empty);
                }

                if (remoteState.enabled && remoteState.loading) {
                    var loading = document.createElement('div');
                    loading.className = 'ms-empty';
                    loading.textContent = (window.i18nInputQuote && window.i18nInputQuote.Loading) || 'Đang tải...';
                    list.appendChild(loading);
                }

                if (typeof scrollTop === 'number') {
                    list.scrollTop = scrollTop;
                }
            }

            function updateButtonText() {
                var val = select.value;
                var found = options.find(function (o) { return o.value === val; });
                var valuesEl = btn.querySelector('.ms-values');
                var placeholderEl = btn.querySelector('.ms-placeholder');
                if (found && found.text) {
                    valuesEl.textContent = found.text;
                    placeholderEl.textContent = '';
                } else {
                    valuesEl.textContent = '';
                    var tx = window.i18nInputQuote || {};
                    placeholderEl.textContent = tx.SelectPlaceholder || '-- Chọn --';
                }
            }

            async function fetchRemoteMaterials(reset) {
                if (!remoteState.enabled || remoteState.loading || (!reset && remoteState.lastPage)) return;

                var pageToLoad = reset ? 1 : remoteState.nextPage;
                var token = ++remoteState.requestToken;
                var preserveScroll = !reset;
                var currentScrollTop = preserveScroll ? list.scrollTop : 0;

                if (reset) {
                    remoteState.lastPage = false;
                    clearRemoteOptionsKeepPlaceholder();
                }

                remoteState.loading = true;
                renderList(remoteState.query, preserveScroll ? currentScrollTop : null);

                try {
                    var body = {
                        MaHang: '',
                        Name: remoteState.query || '',
                        NhomHang: '',
                        PageIndex: pageToLoad,
                        PageSize: remoteState.pageSize
                    };
                    var url = (window.apiBaseUrl || '') + remoteState.api;
                    var res = await callRemoteApi(url, body);

                    if (token !== remoteState.requestToken) return;

                    var rows = Array.isArray(res) ? res : (Array.isArray(res && res.data) ? res.data : []);
                    appendRemoteItems(rows);

                    if (rows.length < remoteState.pageSize) {
                        remoteState.lastPage = true;
                    } else {
                        remoteState.nextPage = pageToLoad + 1;
                    }
                } catch (err) {
                    console.error(err);
                } finally {
                    if (token === remoteState.requestToken) {
                        remoteState.loading = false;
                        renderList(remoteState.query, preserveScroll ? currentScrollTop : null);
                        updateButtonText();
                    }
                }
            }

            select.style.display = 'none';
            select.parentNode.insertBefore(wrapper, select.nextSibling);
            wrapper.appendChild(btn);
            wrapper.appendChild(dropdown);
            dropdown.appendChild(search);
            dropdown.appendChild(list);

            dropdown._wrapper = wrapper;
            dropdown._detached = false;

            select.addEventListener('change', updateButtonText);
            updateButtonText();
            if (remoteState.enabled) initRemotePaginationFromCurrentOptions();
            renderList('');

            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                document.querySelectorAll('.ms-dropdown.open').forEach(function (d) {
                    if (d !== dropdown) {
                        d.classList.remove('open');
                        if (d._detached) {
                            d._wrapper.appendChild(d);
                            d.style.position = d.style.top = d.style.left = d.style.width = d.style.zIndex = '';
                            d._detached = false;
                        }
                    }
                });

                if (dropdown.classList.contains('open')) {
                    dropdown.classList.remove('open');
                    if (dropdown._detached) {
                        dropdown._wrapper.appendChild(dropdown);
                        dropdown.style.position = dropdown.style.top = dropdown.style.left = dropdown.style.width = dropdown.style.zIndex = '';
                        dropdown._detached = false;
                    }
                    return;
                }

                var rect = btn.getBoundingClientRect();
                var top = rect.top + window.scrollY + btn.offsetHeight;
                var left = rect.left + window.scrollX;
                document.body.appendChild(dropdown);
                dropdown.style.position = 'absolute';
                dropdown.style.top = top + 'px';
                dropdown.style.left = left + 'px';
                dropdown.style.width = btn.offsetWidth + 'px';
                dropdown.style.zIndex = 3000;
                dropdown.classList.add('open');
                dropdown._detached = true;

                var inp = search.querySelector('input');
                if (inp) { inp.value = ''; inp.focus(); }
                remoteState.query = '';

                if (remoteState.enabled) {
                    var currentCount = options.filter(function (o) { return !!o.value; }).length;
                    if (currentCount === 0) fetchRemoteMaterials(true);
                    else renderList('');
                } else {
                    renderList('');
                }
            });

            dropdown.addEventListener('click', function (e) { e.stopPropagation(); });

            list.addEventListener('click', function (ev) {
                var it = ev.target.closest('.ms-item');
                if (!it) return;
                select.value = it.dataset.value;
                try { select.dispatchEvent(new Event('change', { bubbles: true })); } catch (ex) { }
                updateButtonText();
                dropdown.classList.remove('open');
                if (dropdown._detached) {
                    dropdown._wrapper.appendChild(dropdown);
                    dropdown.style.position = dropdown.style.top = dropdown.style.left = dropdown.style.width = dropdown.style.zIndex = '';
                    dropdown._detached = false;
                }
            });

            var inputEl = search.querySelector('input');
            if (inputEl) {
                inputEl.addEventListener('input', function () {
                    if (!remoteState.enabled) {
                        renderList(this.value);
                        return;
                    }

                    remoteState.query = (this.value || '').trim();
                    if (remoteState.debounceTimer) clearTimeout(remoteState.debounceTimer);

                    remoteState.debounceTimer = setTimeout(function () {
                        remoteState.nextPage = 1;
                        fetchRemoteMaterials(true);
                    }, 300);
                });
            }

            if (remoteState.enabled) {
                list.addEventListener('scroll', function () {
                    if (!dropdown.classList.contains('open')) return;
                    if (remoteState.loading || remoteState.lastPage) return;
                    var nearBottom = (list.scrollTop + list.clientHeight) >= (list.scrollHeight - 24);
                    if (!nearBottom) return;
                    fetchRemoteMaterials(false);
                });
            }

            select.dataset.searchDropdown = 'true';
        });

        if (!window.__kanziSearchableDocClickBound) {
            window.__kanziSearchableDocClickBound = true;
            document.addEventListener('click', function () {
                document.querySelectorAll('.ms-dropdown').forEach(function (d) {
                    if (d.classList.contains('open')) {
                        d.classList.remove('open');
                        if (d._detached) {
                            d._wrapper.appendChild(d);
                            d.style.position = d.style.top = d.style.left = d.style.width = d.style.zIndex = '';
                            d._detached = false;
                        }
                    }
                });
            });
        }
    }

    window.KanziSearchableDropdown = window.KanziSearchableDropdown || {};
    window.KanziSearchableDropdown.init = buildSearchableDropdown;
})();
