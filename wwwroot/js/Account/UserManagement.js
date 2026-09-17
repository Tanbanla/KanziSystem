(() => {
    const config = window.userManagementConfig || { roles: [], endpoints: {} };
    const roles = config.roles || [];
    const texts = config.texts || {};
    const text = (key, fallback) => texts[key] || fallback;
    const format = (template, ...args) => String(template).replace(/{(\d+)}/g, (_, index) => args[index] ?? '');
    const table = document.getElementById('userTable');
    let users = [];
    let currentPage = 1;
    let pageSize = 10;
    let totalCount = 0;
    const esc = value => String(value ?? '').replace(/[&<>'"]/g, c => ({
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        "'": '&#39;',
        '"': '&quot;'
    }[c]));
    const roleName = code => (roles.find(x => x.code === code)?.name || code || text('user', 'User'));
    const getUserValue = (user, name) => {
        const key = Object.keys(user).find(x => x.toLowerCase() === name.toLowerCase());
        return key ? user[key] : '';
    };
    const notify = (message, success = true) => {
        const el = document.getElementById('userAlert');
        el.className = `alert alert-${success ? 'success' : 'danger'}`;
        el.textContent = message;
        setTimeout(() => el.classList.add('d-none'), 4000);
    };
    const value = id => document.getElementById(id).value.trim();

    async function loadUsers(resetPage = false) {
        if (resetPage) currentPage = 1;
        table.innerHTML = `<tr><td colspan="8" class="text-center empty-state"><i class="fas fa-spinner fa-spin fa-2x mb-3"></i><br>${esc(text('loading', 'Loading data...'))}</td></tr>`;
        const body = {
            adid: value('searchAdid'),
            fullname: value('searchName'),
            section: value('searchSection'),
            role: value('searchRole'),
            status: value('searchStatus') ? Number(value('searchStatus')) : null,
            pageIndex: currentPage,
            pageSize: pageSize
        };
        try {
            const response = await fetch(config.endpoints.search, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            });
            const result = await response.json();
            if (!response.ok) throw new Error(result.message || text('loadError', 'Unable to load the user list.'));
            users = Array.isArray(result) ? result : (result.data || result.Data || []);
            totalCount = Array.isArray(result) ? users.length : (result.totalCount ?? result.TotalCount ?? users.length);
            const lastPage = Math.max(1, Math.ceil(totalCount / pageSize));
            if (currentPage > lastPage) {
                currentPage = lastPage;
                return loadUsers();
            }
            render();
        } catch (error) {
            table.innerHTML = `<tr><td colspan="8" class="text-center text-danger py-5"><i class="fas fa-exclamation-circle mr-1"></i>${esc(error.message || text('loadError', 'Unable to load the user list.'))}</td></tr>`;
        }
    }

    function render() {
        document.getElementById('userCount').textContent = `${totalCount} ${text('users', 'users')}`;
        if (!users.length) {
            table.innerHTML = `<tr><td colspan="8" class="text-center empty-state"><i class="fas fa-users fa-2x mb-3"></i><br>${esc(text('noUsers', 'No matching users found.'))}</td></tr>`;
            renderPagination();
            return;
        }
        table.innerHTML = users.map((u, index) => {
            const userId = getUserValue(u, 'CHR_USERID');
            const fullName = getUserValue(u, 'FULLNAME');
            const employeeId = getUserValue(u, 'CHR_EMPLOYEE_ID');
            const section = getUserValue(u, 'CHR_SECTION');
            const email = getUserValue(u, 'dia_chi_mail');
            const role = getUserValue(u, 'Role');
            const lockStatus = Number(getUserValue(u, 'INT_LOCK'));
            return `<tr>
            <td class="text-center" ><strong>${esc(userId)}</strong></td>
            <td class="text-center">${esc(fullName)}</td>
            <td class="text-center">${esc(employeeId)}</td>
            <td class="text-center">${esc(section)}</td>
            <td class="text-center">${esc(email)}</td>
            <td class="text-center"><span class="badge badge-role">${esc(roleName(role))}</span>
            </td>
            <td class="text-center">
                <span class="status-dot ${
                    lockStatus === 1 ? 'status-locked' :
                    lockStatus === 2 ? 'status-deleted' :
                    'status-active'
                }"></span>
                ${
                    lockStatus === 1 ? text('locked', 'Locked') :
                    lockStatus === 2 ? text('deleted', 'Deleted') :
                    text('active', 'Active')
                }
            </td>
            <td class="text-center"><button class="btn btn-sm btn-outline-primary mr-1 edit-user" data-index="${index}" title="${esc(text('edit', 'Edit'))}"><i class="fas fa-edit"></i></button><button class="btn btn-sm btn-outline-danger delete-user" data-id="${esc(userId)}" title="${esc(text('lockUser', 'Lock user'))}"><i class="fas fa-user-slash"></i></button></td></tr>`;
        }).join('');
        renderPagination();
    }

    function renderPagination() {
        const pagination = document.getElementById('userPagination');
        const summary = document.getElementById('pageSummary');
        const totalPages = Math.ceil(totalCount / pageSize);
        if (!totalCount) {
            summary.textContent = format(text('showing', 'Showing {0}-{1} of {2} {3}'), 0, 0, 0, text('users', 'users'));
            pagination.innerHTML = '';
            return;
        }
        const start = (currentPage - 1) * pageSize + 1;
        const end = Math.min(currentPage * pageSize, totalCount);
        summary.textContent = format(text('showing', 'Showing {0}-{1} of {2} {3}'), start, end, totalCount, text('users', 'users'));
        if (totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }
        let html = `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}"><button class="page-link" data-page="${currentPage - 1}" aria-label="${esc(text('previousPage', 'Previous page'))}">&laquo;</button></li>`;
        for (let page = 1; page <= totalPages; page++) {
            if (totalPages > 7 && page > 2 && page < totalPages - 1 && Math.abs(page - currentPage) > 1) {
                if (page === 3 || page === totalPages - 2) html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
                continue;
            }
            html += `<li class="page-item ${page === currentPage ? 'active' : ''}"><button class="page-link" data-page="${page}">${page}</button></li>`;
        }
        html += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}"><button class="page-link" data-page="${currentPage + 1}" aria-label="${esc(text('nextPage', 'Next page'))}">&raquo;</button></li>`;
        pagination.innerHTML = html;
    }

    function openForm(user = null) {
        document.getElementById('userForm').reset();
        document.getElementById('formError').classList.add('d-none');
        document.getElementById('isEdit').value = user ? 'true' : 'false';
        document.getElementById('modalTitle').textContent = user ? text('updateUser', 'Update user') : text('addUser', 'Add user');
        if (user) {
            const get = key => getUserValue(user, key);
            document.getElementById('userId').value = get('CHR_USERID');
            document.getElementById('userId').readOnly = true;
            document.getElementById('fullName').value = get('FULLNAME');
            document.getElementById('employeeId').value = get('CHR_EMPLOYEE_ID');
            document.getElementById('email').value = get('dia_chi_mail');
            document.getElementById('section').value = get('CHR_SECTION');
/*            document.getElementById('department').value = get('phong_ban');*/
            document.getElementById('active').value = get('INT_LOCK');
            document.getElementById('role').value = user.role || user.Role || 'User';
        } else {
            document.getElementById('userId').readOnly = false;
            document.getElementById('active').value = 'true';
        }
        $('#userModal').modal('show');
    }
    let employeeSearchTimer;
    let employeeSearchRequest = 0;

    async function SearchEmployee() {

        const edit = document.getElementById('isEdit').value === 'true';
        if (edit) return;
        const adid = value('userId');
        const error = document.getElementById('formError');
        if (!adid) return;

        const requestId = ++employeeSearchRequest;
        try {
            const endpoint = new URL(config.endpoints.searchEmployee, window.location.origin);
            endpoint.searchParams.set('adidOrMnv', adid);

            const response = await fetch(endpoint, { method: 'GET' });
            const result = await response.json();
            if (!response.ok || result.success === false) {
                throw new Error(result.message || text('employeeError', 'Unable to get employee information.'));
            }

            if (requestId !== employeeSearchRequest) return;

            const employee = Array.isArray(result) ? result[0] : (result.data || result.Data || [])[0];
            if (!employee) {
                throw new Error(text('employeeNotFound', 'No employee information found for this ADID/employee ID.'));
            }

            const employeeValue = (...names) => {
                for (const name of names) {
                    const item = getUserValue(employee, name);
                    if (item !== null && item !== undefined && String(item).trim() !== '') return item;
                }
                return '';
            };

            document.getElementById('fullName').value = employeeValue('CHR_EMPLOYEE_NAME');
            document.getElementById('employeeId').value = employeeValue('CHR_EMPLOYEE_ID');
            document.getElementById('email').value = employeeValue('CHR_EMPLOYEE_MAIL');
            document.getElementById('section').value = employeeValue('CHR_SEC_CODE');
            error.classList.add('d-none');
        } catch (e) {
            if (requestId !== employeeSearchRequest) return;
            error.textContent = e.message;
            error.classList.remove('d-none');
        }
    }

    const userIdInput = document.getElementById('userId');
    userIdInput.addEventListener('blur', SearchEmployee);
    userIdInput.addEventListener('keydown', e => {
        if (e.key === 'Enter') {
            e.preventDefault();
            clearTimeout(employeeSearchTimer);
            SearchEmployee();
        }
    });

    document.getElementById('searchForm').addEventListener('submit', e => {
        e.preventDefault();
        loadUsers(true);
    });
    document.getElementById('pageSize').addEventListener('change', e => {
        pageSize = Number(e.target.value);
        loadUsers(true);
    });
    document.getElementById('userPagination').addEventListener('click', e => {
        const button = e.target.closest('[data-page]');
        if (!button || button.parentElement.classList.contains('disabled')) return;
        const page = Number(button.dataset.page);
        const totalPages = Math.ceil(totalCount / pageSize);
        if (page >= 1 && page <= totalPages) {
            currentPage = page;
            loadUsers();
        }
    });
    document.getElementById('resetSearch').addEventListener('click', () => {
        document.getElementById('searchForm').reset();
        loadUsers(true);
    });
    document.getElementById('addUserButton').addEventListener('click', () => openForm());
    table.addEventListener('click', e => {
        const edit = e.target.closest('.edit-user');
        if (edit) openForm(users[Number(edit.dataset.index)]);
        const del = e.target.closest('.delete-user');
        if (del) deleteUser(del.dataset.id);
    });
    async function deleteUser(id) {
        if (!confirm(format(text('confirmLock', 'Are you sure you want to lock user "{0}"?'), id))) return;
        try {
            const response = await fetch(config.endpoints.delete, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(id)
            });
            const result = await response.json();
            if (!response.ok) throw new Error(result.message || text('lockError', 'Unable to lock the user.'));
            notify(text('lockedUser', 'User locked.'));
            loadUsers(true);
        } catch (error) {
            notify(error.message, false);
        }
    }
    document.getElementById('userForm').addEventListener('submit', async e => {
        e.preventDefault();
        const error = document.getElementById('formError');
        if (!e.target.checkValidity()) {
            e.target.classList.add('was-validated');
            return;
        }
        const edit = document.getElementById('isEdit').value === 'true';
        const info = {
            CHR_USERID: value('userId'),
            FULLNAME: value('fullName'),
            CHR_EMPLOYEE_ID: value('employeeId'),
            dia_chi_mail: value('email'),
            CHR_SECTION: value('section'),
            VCHR_PASSWORD: value('password') || null,
            INT_LOCK: document.getElementById('active').value,
            cho_phep_hoat_dong: document.getElementById('active').value === 'true',
            phan_quyen: 0
        };
        try {
            const response = await fetch(edit ? config.endpoints.update : config.endpoints.register, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    infor: info,
                    role: value('role')
                })
            });
            const result = await response.json();
            if (!response.ok) throw new Error(result.message || text('saveError', 'Unable to save the information.'));
            $('#userModal').modal('hide');
            notify(edit ? text('updated', 'User updated.') : text('added', 'User added.'));
            loadUsers(true);
        } catch (ex) {
            error.textContent = ex.message;
            error.classList.remove('d-none');
        }
    });
    loadUsers();
})();

