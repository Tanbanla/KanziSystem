
const API_BASE_URL = "http://172.26.248.62:8507/api"; // Thay đổi IP server tại đây
const LOCAL_BASE_URL = "/ipcs";                       
//const LOCAL_BASE_URL = "";                       
const API_ROUTES = {
    Request: {
        LayPhongBan: `${LOCAL_BASE_URL}/Request/_layphongban`
    },
    Import: {
        LoadUserInventory: `${LOCAL_BASE_URL}/Import/_load_userinventory`,
        GetIdUser: `${LOCAL_BASE_URL}/Import/_getid_user`
    },
    User: {
        LoadUQ: `${LOCAL_BASE_URL}/User/Load_UQ`
    },
    Employee: {
        SearchByCondition: `${API_BASE_URL}/Employee/search-by-condition`,
        ByAdid: (id) => `${API_BASE_URL}/Employee/by-adid/${id}` // Dùng function để truyền tham số động
    }
};


let phongban_GA = "";
let centercode_GA = "";
let cost_GA = "";

// thông tin người dùng GA
async function getEmployeeData_GA() {
    var ph = document.getElementById("name_dept").value;
    if (ph == "") {
        alert("Chưa chọn phòng");
    }
    if (!kiemTraViTriKhopNhau()) {

    }
    const formData = new URLSearchParams();
    formData.append('ph', ph);

    // --- Sử dụng Route tập trung ---
    fetch(API_ROUTES.Request.LayPhongBan, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData.toString()
    })
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(data => {
            // tìm cost phòng theo like
            cost_GA = data + "%";
        })
        .catch(err => console.error('Fetch error:', err));

    let us = document.getElementById("us").innerHTML;
    const employeeId = us.trim();

    // --- Sử dụng Route tập trung (truyền ID) ---
    const url = API_ROUTES.Employee.ByAdid(employeeId);

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
        }
        const data = await response.json();
        phongban_GA = data.Data[0].CHR_DEPT;
        centercode_GA = data.Data[0].CHR_COST_CENTER_CODE;

        // lấy người phê duyệt theo quy trình
        let giatien = parseFloat(document.getElementById("thanhtien").value);
        document.getElementById("tongtienpheduyet").innerHTML = giatien.toFixed(2);
        if (giatien < 3000) {

            document.getElementById("GA_ten_duthao").innerHTML = `<option>${data.Data[0].CHR_EMPLOYEE_NAME}</option>`;
            document.getElementById("GA_cv_duthao").value = data.Data[0].CHR_EMPLOYEE_ADID;
            document.getElementById("GA_mail_duthao").value = data.Data[0].CHR_EMPLOYEE_MAIL;

            loadToCombo_GA("Section Manager", "thamtra");
            loadToCombo_PTBP_GA("10 : Deputy General Manager", "pheduyet");
        }
        if (giatien >= 3000 && giatien < 10000) {
            document.getElementById("GA_ten_duthao").innerHTML = `<option>${data.Data[0].CHR_EMPLOYEE_NAME}</option>`;
            document.getElementById("GA_cv_duthao").value = data.Data[0].CHR_EMPLOYEE_ADID;
            document.getElementById("GA_mail_duthao").value = data.Data[0].CHR_EMPLOYEE_MAIL;

            loadToCombo_GA("Section Manager", "thamtra");
            loadToCombo_TBP_GA("General Manager", "pheduyet");
        }
        if (giatien >= 10000) {

            loadToCombo_GA("Section Manager", "duthao");
            loadToCombo_TBP_GA("General Manager", "thamtra");
            loadToCombo_GD_GA("Director", "pheduyet");
        }

        get_block_GA();
        return data;

    } catch (error) {
        console.error("Không thể lấy dữ liệu:", error);
    }
}

// Lấy danh sách user kho và đồng ý
async function get_block_GA() {
    var group_code = document.getElementById("group_code").value;
    const params = new URLSearchParams();
    params.append('group_code', group_code);

    // --- Sử dụng Route tập trung ---
    fetch(API_ROUTES.Import.LoadUserInventory, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: params
    })
        .then(response => response.json())
        .then(data => {
            document.getElementById("GA_ten_xuatkho").innerHTML = "";
            document.getElementById("GA_ten_dongy_QLSC").innerHTML = "";
            document.getElementById("GA_ten_dongy_QLTC").innerHTML = "";
            document.getElementById("GA_cv_xuatkho").innerHTML = "";
            document.getElementById("GA_cv_dongy_QLSC").innerHTML = "";
            document.getElementById("GA_cv_dongy_QLTC").innerHTML = "";
            document.getElementById("GA_mail_xuatkho").innerHTML = "";
            document.getElementById("GA_mail_dongy_QLSC").innerHTML = "";
            document.getElementById("GA_mail_dongy_QLTC").innerHTML = "";
            // Khởi tạo biến kiểm tra 
            var isFirstRole1 = true;
            var isFirstRole2 = true;
            var isFirstRole3 = true;

            for (var i = 0; i < data.length; i++) {
                if (data[i].role == "1") {
                    document.getElementById("GA_ten_xuatkho").innerHTML += `<option value="${data[i].id_User}_${data[i].user_Name}">${data[i].user_Name}</option>`;
                    if (isFirstRole1) {
                        document.getElementById("GA_cv_xuatkho").value = data[i].adid;
                        document.getElementById("GA_mail_xuatkho").value = data[i].mail;
                        isFirstRole1 = false;
                    }
                }
                if (data[i].role == "2") {
                    document.getElementById("GA_ten_dongy_QLSC").innerHTML += `<option value="${data[i].id_User}_${data[i].user_Name}">${data[i].user_Name}</option>`;
                    if (isFirstRole2) {
                        document.getElementById("GA_cv_dongy_QLSC").value = data[i].adid;
                        document.getElementById("GA_mail_dongy_QLSC").value = data[i].mail;
                        isFirstRole2 = false;
                    }
                }
                if (data[i].role == "3") {
                    document.getElementById("GA_ten_dongy_QLTC").innerHTML += `<option value="${data[i].id_User}_${data[i].user_Name}">${data[i].user_Name}</option>`;
                    if (isFirstRole3) {
                        document.getElementById("GA_cv_dongy_QLTC").value = data[i].adid;
                        document.getElementById("GA_mail_dongy_QLTC").value = data[i].mail;
                        isFirstRole3 = false;
                    }
                }
            }
        })
        .catch(error => console.error('Error:', error));
}

// Tạo sự kiện khi thay đổi user
async function get_useriv_GA(id) {
    const params = new URLSearchParams();
    params.append('id', id.split('_')[0]);

    // --- Sử dụng Route tập trung ---
    fetch(API_ROUTES.Import.GetIdUser, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: params
    })
        .then(response => response.json())
        .then(data => {
            if (data[0].role == "1") {
                document.getElementById("GA_cv_xuatkho").value = data[0].adid;
                document.getElementById("GA_mail_xuatkho").value = data[0].mail;
            }
            if (data[0].role == "2") {
                document.getElementById("GA_cv_dongy").value = data[0].adid;
                document.getElementById("GA_mail_dongy").value = data[0].mail;
            }
        })
        .catch(error => console.error('Error:', error));
}

// Hàm tạo Payload (dữ liệu gửi đi)
const createSearchData_GA = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [

        { "Field": "CHR_SECTION", "Value": cost_GA, "Operator": "like", "LogicType": "AND" },
        { "Field": "CHR_POSITION_GROUP", "Value": position, "Operator": "=", "LogicType": "AND" },
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});

// Hàm gọi API và gán vào Combobox
async function loadToCombo_GA(position, comboId) {

    // --- Sử dụng Route tập trung ---
    const response = await fetch(API_ROUTES.Employee.SearchByCondition, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_GA(position))
    });

    const result = await response.json();
    if (result.Data.Data.length == 0) {
        //loadToCombo_TBP_GA("Section Manager", "thamtra");
        loadToCombo_TBP_GA("General Manager", "pheduyet");
    }
    try {
        document.getElementById("GA_ten_" + comboId).innerHTML = "";
        for (var i = 0; i < result.Data.Data.length; i++) {
            document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
            // lấy danh sách user ủy quyền
            var usUQ = result.Data.Data[i].CHR_EMPLOYEE_ADID;
            const params = new URLSearchParams();
            params.append('adid', usUQ);

            // --- Sử dụng Route tập trung ---
            const rsUQ = await fetch(API_ROUTES.User.LoadUQ, {
                method: 'POST',
                body: params
            });
            const kq = await rsUQ.json();
            for (var a = 0; a < kq.length; a++) {
                document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
            }
        }
        //document.getElementById("ten_" + comboId).innerHTML = `<option>${result.Data.Data[0].CHR_EMPLOYEE_NAME}</option>`;
        document.getElementById("GA_cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("GA_mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;

    }
    catch {

    }
}

const createSearchData_PTBP_GA = (position) => ({   
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [

        { "Field": "CHR_DEPT", "Value": phongban_GA, "Operator": "like", "LogicType": "AND" },
        { "Field": "CHR_POSITION", "Value": position, "Operator": "=", "LogicType": "AND" },
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});
// Hàm gọi API và gán vào Combobox
async function loadToCombo_PTBP_GA(position, comboId) {
    const response = await fetch(CONFIG.ROUTES.employeeSearch, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_PTBP_GA(position))
    });

    const result = await response.json();

    if (result.Data.Data.length == 0) {

        //alert("Cost phòng ban chưa được đăng ký hoặc bị hủy !");
        loadToCombo_TBP_GA("General Manager", "pheduyet");
        //loadToCombo_TBP("Section Manager", "thamtra");
    }
    try {
        document.getElementById("GA_ten_" + comboId).innerHTML = "";
        for (var i = 0; i < result.Data.Data.length; i++) {
            document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
            // lấy danh sách user ủy quyền
            var usUQ = result.Data.Data[i].CHR_EMPLOYEE_ADID;
            const params = new URLSearchParams();
            params.append('adid', usUQ);

            const rsUQ = await fetch(CONFIG.ROUTES.loadUQ, {
                method: 'POST',
                body: params
            });
            const kq = await rsUQ.json();
            for (var a = 0; a < kq.length; a++) {
                document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
            }
        }

        document.getElementById("GA_cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("GA_mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
        // xử lý trường hợp ủy quyền
    }
    catch {

    }
}

// Hàm tạo Payload (dữ liệu gửi đi)
createSearchData_GD_GA = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [
        { "Field": "CHR_POSITION_GROUP", "Value": position, "Operator": "=", "LogicType": "AND" },
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});

// Hàm gọi API và gán vào Combobox
async function loadToCombo_GD_GA(position, comboId) {
    // --- Sử dụng Route tập trung ---
    const response = await fetch(API_ROUTES.Employee.SearchByCondition, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_GD_GA(position))
    });

    const result = await response.json();
    document.getElementById("GA_ten_" + comboId).innerHTML = "";
    for (var i = 0; i < result.Data.Data.length; i++) {
        document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;

        // lấy danh sách user ủy quyền
        var usUQ = result.Data.Data[i].CHR_EMPLOYEE_ADID;
        const params = new URLSearchParams();
        params.append('adid', usUQ);

        // --- Sử dụng Route tập trung ---
        const rsUQ = await fetch(API_ROUTES.User.LoadUQ, {
            method: 'POST',
            body: params
        });
        const kq = await rsUQ.json();
        for (var a = 0; a < kq.length; a++) {
            document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
        }
    }
    document.getElementById("GA_cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
    document.getElementById("GA_mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
}

// Hàm tạo Payload (dữ liệu gửi đi)
const createSearchData_TBP_GA = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [
        { "Field": "CHR_POSITION", "Value": position, "Operator": "=", "LogicType": "AND" },
        { "Field": "CHR_DEPT", "Value": phongban_GA, "Operator": "=", "LogicType": "AND" },
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});

// Hàm gọi API và gán vào Combobox
async function loadToCombo_TBP_GA(position, comboId) {
    // --- Sử dụng Route tập trung ---
    const response = await fetch(API_ROUTES.Employee.SearchByCondition, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_TBP_GA(position))
    });
    const result = await response.json();
    document.getElementById("GA_ten_" + comboId).innerHTML = "";
    for (var i = 0; i < result.Data.Data.length; i++) {
        document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;

        // lấy danh sách user ủy quyền
        var usUQ = result.Data.Data[i].CHR_EMPLOYEE_ADID;
        const params = new URLSearchParams();
        params.append('adid', usUQ);

        // --- Sử dụng Route tập trung ---
        const rsUQ = await fetch(API_ROUTES.User.LoadUQ, {
            method: 'POST',
            body: params
        });
        const kq = await rsUQ.json();
        for (var a = 0; a < kq.length; a++) {
            document.getElementById("GA_ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
        }
    }

    document.getElementById("GA_cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
    document.getElementById("GA_mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
}

// lấy mail theo ADID
async function get_info_GA(us, comboId) {
    // --- Sử dụng Route tập trung (truyền ID) ---
    const url = API_ROUTES.Employee.ByAdid(us);

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
        }

        const data = await response.json();
        console.log(data);
        document.getElementById("GA_cv_" + comboId).value = data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("GA_mail_" + comboId).value = data.Data[0].CHR_EMPLOYEE_MAIL;

    } catch (error) {
        console.error("Không thể lấy dữ liệu:", error);
    }
}
