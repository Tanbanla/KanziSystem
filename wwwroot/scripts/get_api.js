const CONFIG = (() => {
    const API_BASE = 'http://172.26.248.62:8507';

    return {
        API_BASE,
        ROUTES: {
            // External API (absolute)
            employeeByAdid: (adid) => `${API_BASE}/api/Employee/by-adid/${adid}`,
            employeeSearch: `${API_BASE}/api/Employee/search-by-condition`,

            // Internal (Razor Pages / Controllers) - relative paths
            layPhongBan: '/ipcs/Request/_layphongban',
            loadUserInventory: '/ipcs/Import/_load_userinventory',
            getIdUser: '/ipcs/Import/_getid_user',
            loadUQ: '/ipcs/User/Load_UQ'
        }
    };
})();

//const CONFIG = (() => {
//    const API_BASE = 'http://172.26.248.62:8507/api';

//    return {
//        API_BASE,
//        ROUTES: {
//            // External API (absolute)
//            employeeByAdid: (adid) => `${API_BASE}/Employee/by-adid/${adid}`,
//            employeeSearch: `${API_BASE}/Employee/search-by-condition`,

//            // Internal (Razor Pages / Controllers) - relative paths
//            layPhongBan: '/Request/_layphongban',
//            loadUserInventory: '/Import/_load_userinventory',
//            getIdUser: '/Import/_getid_user',
//            loadUQ: '/User/Load_UQ'
//        }
//    };
//})();

let phongban = "";
let centercode = "";
let cost = "";

// lấy thông tin người dùng
async function getEmployeeData() {

     // lấy thông tin phòng ban
     var ph = document.getElementById("name_dept").value;
   
     if (ph == "") {
         alert("Chưa chọn phòng");
    }
    if (!kiemTraViTriKhopNhau()) {
     
    }
    const formData = new URLSearchParams();
    formData.append('ph', ph);
     fetch(CONFIG.ROUTES.layPhongBan, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData.toString()
    })
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(data => {
            // tìm cost phòng theo like
            cost = data + "%";

        })
        .catch(err => console.error('Fetch error:', err));

       // lấy thông tin quản lý API
    let us = document.getElementById("us").innerHTML;

    const employeeId = us.trim();
    const url = CONFIG.ROUTES.employeeByAdid(employeeId);

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
        }
        const data = await response.json();
        phongban = data.Data[0].CHR_DEPT;
        //costphongban = document.getElementById("name_dept").value;
        //cost = cost + " : " + phongban.split(':')[1];

        centercode = data.Data[0].CHR_COST_CENTER_CODE;

        // lấy người phê duyệt theo quy trình
        let giatien = parseFloat(document.getElementById("thanhtien").value);
        document.getElementById("tongtienpheduyet").innerHTML = giatien;
        if (giatien < 3000) {

            document.getElementById("ten_duthao").innerHTML = `<option>${data.Data[0].CHR_EMPLOYEE_NAME}</option>`;
            document.getElementById("cv_duthao").value = data.Data[0].CHR_EMPLOYEE_ADID;
            document.getElementById("mail_duthao").value = data.Data[0].CHR_EMPLOYEE_MAIL;

            loadToCombo("Section Manager", "thamtra");            
            loadToCombo_PTBP("10 : Deputy General Manager", "pheduyet");
           
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
            loadToCombo_TBP("General Manager", "thamtra");
            loadToCombo_GD("Director", "pheduyet");
        }

        get_block();
        return data;

    }
    catch (error) {
        console.error("Không thể lấy dữ liệu:", error);
    }
}
// Lấy danh sách user kho và đồng ý
async function get_block() {
    var group_code = document.getElementById("group_code").value;
    const params = new URLSearchParams();
    params.append('group_code', group_code);

    fetch(CONFIG.ROUTES.loadUserInventory, {
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

    fetch(CONFIG.ROUTES.getIdUser, {
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

        { "Field": "CHR_SECTION", "Value": cost, "Operator": "like", "LogicType": "AND" },
        { "Field": "CHR_POSITION_GROUP", "Value": position, "Operator": "=", "LogicType": "AND" },
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});
// Hàm gọi API và gán vào Combobox
async function loadToCombo(position, comboId) {
    const response = await fetch(CONFIG.ROUTES.employeeSearch, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData(position))
    });
   
    const result = await response.json();
 
    //if (result.Data.Data.length == 0) {

    //    //alert("Cost phòng ban chưa được đăng ký hoặc bị hủy !");
    //    loadToCombo_PTBP("10 : Deputy General Manager", "pheduyet");
    //    //loadToCombo_TBP("Section Manager", "thamtra");
    //}
    try {
        document.getElementById("ten_" + comboId).innerHTML = "";
        for (var i = 0; i < result.Data.Data.length; i++) {
            document.getElementById("ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
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
                document.getElementById("ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
            }
        }   
        
        document.getElementById("cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
        // xử lý trường hợp ủy quyền
    }
    catch {
       
    }    
}

const createSearchData_PTBP = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [

        { "Field": "CHR_DEPT", "Value": phongban, "Operator": "like", "LogicType": "AND" },
        { "Field": "CHR_POSITION", "Value": position, "Operator": "=", "LogicType": "AND" },
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});
// Hàm gọi API và gán vào Combobox
async function loadToCombo_PTBP(position, comboId) {
    const response = await fetch(CONFIG.ROUTES.employeeSearch, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_PTBP(position))
    });

    const result = await response.json();

    if (result.Data.Data.length == 0) {

        //alert("Cost phòng ban chưa được đăng ký hoặc bị hủy !");
        loadToCombo_TBP("General Manager", "pheduyet");
        //loadToCombo_TBP("Section Manager", "thamtra");
    }
    try {
        document.getElementById("ten_" + comboId).innerHTML = "";
        for (var i = 0; i < result.Data.Data.length; i++) {
            document.getElementById("ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
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
                document.getElementById("ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
            }
        }

        document.getElementById("cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
        // xử lý trường hợp ủy quyền
    }
    catch {

    }
}

// Hàm tạo Payload (dữ liệu gửi đi)
const createSearchData_GD = (position) => ({
    "SearchTerm": "",
    "SearchFields": ["CHR_EMPLOYEE_NAME"],
    "PageNumber": 1, "PageSize": 10,
    "Filters": [
        { "Field": "CHR_POSITION", "Value": "Director", "Operator": "=", "LogicType": "AND" },   
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
}); 
// Hàm gọi API và gán vào Combobox
async function loadToCombo_GD(position, comboId) {
    const response = await fetch(CONFIG.ROUTES.employeeSearch, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_GD(position))
    });

    const result = await response.json();
    for (var i = 0; i < result.Data.Data.length; i++) {
        document.getElementById("ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
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
            document.getElementById("ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
        }
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
        { "Field": "CHR_POSITION", "Value": position, "Operator": "=", "LogicType": "AND" },
        { "Field": "CHR_DEPT", "Value": phongban, "Operator": "=", "LogicType": "AND" },
        { "Field": "DTM_LEAVE_DATE", "Value": "", "Operator": "is null", "LogicType": "AND" }
    ],
    "SortOptions": [{ "Field": "ID", "SortDirection": "DESC" }]
});
// Hàm gọi API và gán vào Combobox
async function loadToCombo_TBP(position, comboId) {
    const response = await fetch(CONFIG.ROUTES.employeeSearch, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createSearchData_TBP(position))
    });

    const result = await response.json();
    if (result.Data.Data.length == 0) {

        //alert("Cost phòng ban chưa được đăng ký hoặc bị hủy !");
        loadToCombo_GD("Director", "pheduyet");
        //loadToCombo_TBP("Section Manager", "thamtra");
    }
    document.getElementById("ten_" + comboId).innerHTML = "";
    for (var i = 0; i < result.Data.Data.length; i++) {
        document.getElementById("ten_" + comboId).innerHTML += `<option value="${result.Data.Data[i].CHR_EMPLOYEE_ADID}">${result.Data.Data[i].CHR_EMPLOYEE_NAME}</option>`;
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
            document.getElementById("ten_" + comboId).innerHTML += `<option value="${kq[a].chR_ADID_NguoiduocUQ}">${kq[a].chR_TEN_NguoiduocUQ}</option>`;
        }
    }   
    document.getElementById("cv_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_ADID;
    document.getElementById("mail_" + comboId).value = result.Data.Data[0].CHR_EMPLOYEE_MAIL;
}
// lấy mail theo ADID
 async function get_info(us, comboId) {
    const url = CONFIG.ROUTES.employeeByAdid(us);

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
        }

        const data = await response.json();
    
        document.getElementById("cv_" + comboId).value = data.Data[0].CHR_EMPLOYEE_ADID;
        document.getElementById("mail_" + comboId).value = data.Data[0].CHR_EMPLOYEE_MAIL;

    } catch (error) {
        console.error("Không thể lấy dữ liệu:", error);
    }
}

 async function GA_get_info(us, comboId) {
    const url = CONFIG.ROUTES.employeeByAdid(us);

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

 async function GA_get_useriv(id) {

    const params = new URLSearchParams();
    params.append('id', id.split('_')[0]);

     fetch(CONFIG.ROUTES.getIdUser, {
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


