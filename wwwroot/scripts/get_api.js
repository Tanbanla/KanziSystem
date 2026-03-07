
let phongban = "";
let centercode = "";
// lấy thông tin người dùng
async function getEmployeeData() {
    let us = document.getElementById("us").innerHTML;
    const employeeId = us.trim();
    const url = `http://172.26.248.62:8507/api/Employee/by-adid/${employeeId}`;

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
        }
        const data = await response.json();
        phongban = data.Data[0].CHR_DEPT;
        centercode = data.Data[0].CHR_COST_CENTER_CODE;

        // lấy người phê duyệt theo quy trình
        let giatien = parseFloat(document.getElementById("thanhtien").value);
        document.getElementById("tongtienpheduyet").innerHTML = Math.round(giatien,3);
        if (giatien < 3000) {

            document.getElementById("ten_duthao").innerHTML = `<option>${data.Data[0].CHR_EMPLOYEE_NAME}</option>`;
            document.getElementById("cv_duthao").value = data.Data[0].CHR_EMPLOYEE_ADID;
            document.getElementById("mail_duthao").value = data.Data[0].CHR_EMPLOYEE_MAIL;

            loadToCombo("Section Manager", "thamtra");
            loadToCombo("Dept Manager", "pheduyet");
        }
        if (giatien >= 3000 && giatien < 10000) {
            document.getElementById("ten_duthao").innerHTML = `<option>${data.Data[0].CHR_EMPLOYEE_NAME}</option>`;
            document.getElementById("cv_duthao").value = data.Data[0].CHR_EMPLOYEE_ADID;
            document.getElementById("mail_duthao").value = data.Data[0].CHR_EMPLOYEE_MAIL;

            loadToCombo("Section Manager", "thamtra");
            loadToCombo_TBP("General Manager", "pheduyet");
        }
        if (giatien >= 10000) {

            loadToCombo("Section Manager", "duthao");
            loadToCombo_TBP("Dept Manager", "thamtra");
            loadToCombo_GD("Director", "pheduyet");
        }

        get_block();
        return data;

    } catch (error) {
        console.error("Không thể lấy dữ liệu:", error);
    }
}
// Lấy danh sách user kho và đồng ý
async function get_block() {
    var group_code = document.getElementById("group_code").value;
    const params = new URLSearchParams();
    params.append('group_code', group_code);

    fetch('/Import/_load_userinventory', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: params
    })
        .then(response => response.json())
        .then(data => {
            console.log(data);
            document.getElementById("ten_dongy").innerHTML = "";
            document.getElementById("ten_xuatkho").innerHTML = "";
            document.getElementById("cv_dongy").innerHTML = "";
            document.getElementById("cv_xuatkho").innerHTML = "";
            document.getElementById("mail_dongy").innerHTML = "";
            document.getElementById("mail_xuatkho").innerHTML = "";
            // Khởi tạo biến kiểm tra 
            var isFirstRole1 = true;
            var isFirstRole2 = true;

            for (var i = 0; i < data.length; i++) {
                if (data[i].role == "1") {
                    document.getElementById("ten_xuatkho").innerHTML += `<option value="${data[i].id_User}_${data[i].user_Name}">${data[i].user_Name}</option>`;
                    if (isFirstRole1) {
                        document.getElementById("cv_xuatkho").value = data[i].adid;
                        document.getElementById("mail_xuatkho").value = data[i].mail;
                        isFirstRole1 = false;
                    }
                }

                if (data[i].role == "2") {
                    document.getElementById("ten_dongy").innerHTML += `<option value="${data[i].id_User}_${data[i].user_Name}">${data[i].user_Name}</option>`;
                    if (isFirstRole2) {
                        document.getElementById("cv_dongy").value = data[i].adid;
                        document.getElementById("mail_dongy").value = data[i].mail;
                        isFirstRole2 = false;
                    }
                }
            }
        })
        .catch(error => console.error('Error:', error));
}
// Tạo sự kiện khi thay đổi user
async function get_useriv(id) {
    
    const params = new URLSearchParams();
    params.append('id', id.split('_')[0]);

    fetch('/Import/_getid_user', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: params
    })
        .then(response => response.json())
        .then(data => {
            if (data[0].role == "1") {
                document.getElementById("cv_xuatkho").value = data[0].adid;
                document.getElementById("mail_xuatkho").value = data[0].mail;
            }
            if (data[0].role == "2") {
                document.getElementById("cv_dongy").value = data[0].adid;
                document.getElementById("mail_dongy").value = data[0].mail;
            }
        })
        .catch(error => console.error('Error:', error));
}
// Hàm tạo Payload (dữ liệu gửi đi)
const createSearchData = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [
        { "Field": "CHR_DEPT", "Value": phongban, "Operator": "=", "LogicType": "AND" },
        { "Field": "CHR_COST_CENTER_CODE", "Value": centercode, "Operator": "=", "LogicType": "AND" },
        { "Field": "CHR_POSITION_GROUP", "Value": position, "Operator": "=", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});
// Hàm gọi API và gán vào Combobox
async function loadToCombo(position, comboId) {
    const response = await fetch('http://172.26.248.62:8507/api/Employee/search-by-condition', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData(position))
    });

    const result = await response.json();
    try {
        document.getElementById("ten_" + comboId).innerHTML = `<option>${result.Data.Data[0].CHR_EMPLOYEE_NAME}</option>`;
        document.getElementById("cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
    }
    catch { }    
}

// Hàm tạo Payload (dữ liệu gửi đi)
const createSearchData_GD = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [
        { "Field": "CHR_POSITION", "Value": "Director", "Operator": "=", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});
// Hàm gọi API và gán vào Combobox
async function loadToCombo_GD(position, comboId) {
    const response = await fetch('http://172.26.248.62:8507/api/Employee/search-by-condition', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_GD(position))
    });

    const result = await response.json();
    for (var i = 0; i < result.Data.Data.length; i++) {
        document.getElementById("ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
    }
    document.getElementById("cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
    document.getElementById("mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
}

// Hàm tạo Payload (dữ liệu gửi đi)
const createSearchData_TBP = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [
        { "Field": "CHR_POSITION", "Value": "General Manager", "Operator": "=", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});
// Hàm gọi API và gán vào Combobox
async function loadToCombo_TBP(position, comboId) {
    const response = await fetch('http://172.26.248.62:8507/api/Employee/search-by-condition', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_TBP(position))
    });
    const result = await response.json();
    for (var i = 0; i < result.Data.Data.length; i++) {
        document.getElementById("ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
    }
   
    document.getElementById("cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
    document.getElementById("mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
}
// lấy mail theo ADID
async function get_info(us, comboId) {
    const url = `http://172.26.248.62:8507/api/Employee/by-adid/${us}`;

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
        }

        const data = await response.json();
        console.log(data);
        document.getElementById("cv_" + comboId).value = data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("mail_" + comboId).value = data.Data[0].CHR_EMPLOYEE_MAIL;

    } catch (error) {
        console.error("Không thể lấy dữ liệu:", error);
    }
}
