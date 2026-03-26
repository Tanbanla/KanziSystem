function _insert_request() {
    // truyền list
   
    let dataList = [];
    // Chọn tất cả các dòng có class 'input-row'
    const rows = document.querySelectorAll('.input-row');
    rows.forEach((row) => {
        // Lấy dữ liệu từ các phần tử bên trong dòng hiện tại
        let rowData = {
            Material_Code: (row.querySelector('.tenhang').value).split(':')[0],
            Material_Name: (row.querySelector('.tenhang').value).split(':')[1],
            Account_Code: (row.querySelector('.stk').value).split(':')[0],
            Account_Name: (row.querySelector('.stk').value).split(':')[1],
            Amount: row.querySelector('.sl').value,
            Unit: row.querySelector('.dv').value,
            Price: row.querySelector('.dg').value,
            Currency: row.querySelector('.nt').value,
            Total_exchange: row.querySelector('.tcp').value,
            Total: row.querySelector('.tcp').value,
            Aim: row.querySelector('.md').value,
            Phongchiuchiphi: (row.querySelector('.pcp').value).split(':')[0],
            //vt: row.querySelector('.vt').value,
            //gc: row.querySelector('.gc').value
        };

        // Thêm đối tượng vào danh sách tổng
        dataList.push(rowData);
    });

    var name_dept = document.getElementById("name_dept").value;
    var Cost_Center = name_dept.split(':')[0];
    var Declaration = document.getElementById("loaichiphi").value;
    var Dealine = document.getElementById("thoihan").value;
    var Total_exchange = document.getElementById("thanhtien").value;
    var Exchange_rate = document.getElementById("rate").value;
    var Currency = "USD";
    var Total = document.getElementById("thanhtien").value;
    var Kind = "IN";
    var Type = document.getElementById("typee").value;
    var Status = "WAITCONFIRM";
    var Place = name_dept.split(':')[1];
    var Loaihinhtokhai = "LOAIKHAC";
    var Group_Code = document.getElementById("group_code").value;
    var Chophepin = '1';
    var urgentValue = document.querySelector('input[name="value"]:checked').value;
    var Urgent = urgentValue;
    var User_Create = document.getElementById("us").innerHTML;

    var adid_dt = document.getElementById("cv_duthao").value;
    var adid_tt = document.getElementById("cv_thamtra").value;
    var adid_pd = document.getElementById("cv_pheduyet").value;
    var adid_dy = document.getElementById("cv_dongy").value;
    var adid_xk = document.getElementById("cv_xuatkho").value;

    var ten_dt = document.getElementById("ten_duthao").value;
    var ten_tt = document.getElementById("ten_thamtra").value;
    var ten_pd = document.getElementById("ten_pheduyet").value;
    var ten_dy = document.getElementById("ten_dongy").value;
    var ten_xk = document.getElementById("ten_xuatkho").value;

    var mail_dt = document.getElementById("mail_duthao").value;
    var mail_tt = document.getElementById("mail_thamtra").value;
    var mail_pd = document.getElementById("mail_pheduyet").value;
    var mail_dy = document.getElementById("mail_dongy").value;
    var mail_xk = document.getElementById("mail_xuatkho").value;
    var adidnguoitao = document.getElementById("us").innerHTML;
    var mailnguoitao = document.getElementById("email_us").textContent;
    if (Place == "" || name_dept == "" || Dealine == "") {
        alert("Vui lòng điền đủ thông tin vào đơn !");
    }
    else {
        $.ajax({
            url: '/Request/_Insert_request',
            type: 'POST',
            dataType: 'JSON',
            data: {
                Cost_Center: Cost_Center, Declaration: Declaration, Dealine: Dealine, Total_exchange: Total_exchange, Exchange_rate: Exchange_rate, Currency: Currency, Total: Total, Kind: Kind,
                Type: Type, Status: Status, Place: Place, Loaihinhtokhai: Loaihinhtokhai, Group_Code: Group_Code, Chophepin: Chophepin, Urgent: Urgent, User_Create: User_Create, rq: dataList,
                adid_dt: adid_dt, adid_tt: adid_tt, adid_pd: adid_pd, ten_dt: ten_dt, ten_tt: ten_tt, ten_pd: ten_pd, mail_dt: mail_dt, mail_tt: mail_tt, mail_pd: mail_pd, adid_dy: adid_dy, ten_dy: ten_dy,
                mail_dy: mail_dy, adid_xk: adid_xk, ten_xk: ten_xk, mail_xk: mail_xk, adidnguoitao: adidnguoitao, mailnguoitao: mailnguoitao
            },
            success: function (response) {
                /*    send_mail(mail_dt, Urgent);*/
                alert(response);
                location.reload();
                document.querySelectorAll('.close').forEach(button => button.click());
                _load_confirm();
            }
        })
    }
  
}
function _insert_request_GA() {
    // truyền list

    let dataList = [];
    // Chọn tất cả các dòng có class 'input-row'
    const rows = document.querySelectorAll('.input-row');
    rows.forEach((row) => {
        // Lấy dữ liệu từ các phần tử bên trong dòng hiện tại
        let rowData = {
            Material_Code: (row.querySelector('.tenhang').value).split(':')[0],
            Material_Name: (row.querySelector('.tenhang').value).split(':')[1],
            Account_Code: (row.querySelector('.stk').value).split(':')[0],
            Account_Name: (row.querySelector('.stk').value).split(':')[1],
            Amount: row.querySelector('.sl').value,
            Unit: row.querySelector('.dv').value,
            Price: row.querySelector('.dg').value,
            Currency: row.querySelector('.nt').value,
            Total_exchange: row.querySelector('.tcp').value,
            Total: row.querySelector('.tcp').value,
            Aim: row.querySelector('.md').value,
            Phongchiuchiphi: (row.querySelector('.pcp').value).split(':')[0],
            //vt: row.querySelector('.vt').value,
            //gc: row.querySelector('.gc').value
        };

        // Thêm đối tượng vào danh sách tổng
        dataList.push(rowData);
    });

    var name_dept = document.getElementById("name_dept").value;
    var Cost_Center = name_dept.split(':')[0];
    var Declaration = document.getElementById("loaichiphi").value;
    var Dealine = document.getElementById("thoihan").value;
    var Total_exchange = document.getElementById("thanhtien").value;
    var Exchange_rate = document.getElementById("rate").value;
    var Currency = "VND";
    var Total = document.getElementById("thanhtien").value;
    var Kind = "IN";
    var Type = document.getElementById("typee").value;
    var Status = "WAITCONFIRM";
    var Place = name_dept.split(':')[1];
    var Loaihinhtokhai = "LOAIKHAC";
    var Group_Code = document.getElementById("group_code").value;
    var Chophepin = '1';
    var urgentValue = document.querySelector('input[name="value"]:checked').value;
    var Urgent = urgentValue;
    var User_Create = document.getElementById("us").innerHTML;

    var adid_dt = document.getElementById("GA_cv_duthao").value;
    var adid_tt = document.getElementById("GA_cv_thamtra").value;
    var adid_pd = document.getElementById("GA_cv_pheduyet").value;
  
    var adid_xk = document.getElementById("GA_cv_xuatkho").value;
    var adid_qlsc = document.getElementById("GA_cv_dongy_QLSC").value;
    var adid_qltc = document.getElementById("GA_cv_dongy_QLTC").value;

    var ten_dt = document.getElementById("GA_ten_duthao").value;
    var ten_tt = document.getElementById("GA_ten_thamtra").value;
    var ten_pd = document.getElementById("GA_ten_pheduyet").value;
 
    var ten_xk = document.getElementById("GA_ten_xuatkho").value;
    var ten_qlsc = document.getElementById("GA_ten_dongy_QLSC").value;
    var ten_qltc = document.getElementById("GA_ten_dongy_QLTC").value;

    var mail_dt = document.getElementById("GA_mail_duthao").value;
    var mail_tt = document.getElementById("GA_mail_thamtra").value;
    var mail_pd = document.getElementById("GA_mail_pheduyet").value;
   
    var mail_xk = document.getElementById("GA_mail_xuatkho").value;
    var mail_qlsc = document.getElementById("GA_mail_dongy_QLSC").value;
    var mail_qltc = document.getElementById("GA_mail_dongy_QLTC").value;

    var adidnguoitao = document.getElementById("us").innerHTML;
    var mailnguoitao = document.getElementById("email_us").textContent;
    if (Place == "" || name_dept == "" || Dealine == "") {
        alert("Vui lòng điền đủ thông tin vào đơn !");
    }
    else {
        $.ajax({
            url: '/Request/_Insert_request_GA',
            type: 'POST',
            dataType: 'JSON',
            data: {
                Cost_Center: Cost_Center, Declaration: Declaration, Dealine: Dealine, Total_exchange: Total_exchange, Exchange_rate: Exchange_rate, Currency: Currency, Total: Total, Kind: Kind,
                Type: Type, Status: Status, Place: Place, Loaihinhtokhai: Loaihinhtokhai, Group_Code: Group_Code, Chophepin: Chophepin, Urgent: Urgent, User_Create: User_Create, rq: dataList,
                adid_dt: adid_dt, adid_tt: adid_tt, adid_pd: adid_pd, ten_dt: ten_dt, ten_tt: ten_tt, ten_pd: ten_pd, mail_dt: mail_dt, mail_tt: mail_tt, mail_pd: mail_pd, ten_qlsc: ten_qlsc,
                adid_xk: adid_xk, ten_xk: ten_xk, mail_xk: mail_xk, adidnguoitao: adidnguoitao, mailnguoitao: mailnguoitao, adid_qlsc: adid_qlsc, adid_qltc: adid_qltc, ten_qltc: ten_qltc,
                mail_qlsc: mail_qlsc, mail_qltc: mail_qltc
            },
            success: function (response) {
                /*    send_mail(mail_dt, Urgent);*/
                alert(response);
                location.reload();
                document.querySelectorAll('.close').forEach(button => button.click());
                _load_confirm();
            }
        })
    }

}
async function _load_rate() {
    fetch('/Request/_get_rate', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("rate").value = data;
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
async function _load_phongchiuphi() {
    fetch('/Request/_get_phongchiuphi', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            const selectElements = document.querySelectorAll('.pcp'); // Khuyên dùng querySelectorAll
            selectElements.forEach(select => {
                // Xóa trắng hoặc giữ lại option mặc định trước khi gán
                select.innerHTML = '<option></option>';

                data.forEach(item => {
                    select.innerHTML += `<option value="${item}">${item}</option>`;
                });
            });
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
// lấy mail theo ADID
async function get_mail(us) {
    const url = `http://172.26.248.62:8507/api/Employee/by-adid/${us}`;

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
        }

        const data = await response.json();

        // lấy người phê duyệt theo quy trình
        let maill = data.Data[0].CHR_EMPLOYEE_MAIL;

        return data;

    } catch (error) {
        console.error("Không thể lấy dữ liệu:", error);
    }
}
// gửi mail
async function send_mail(mail_to, Urgent) {
    $.ajax({
        url: '/Request/_send_mail',
        type: 'POST',
        dataType: 'JSON',
        data: {
            mail_to: mail_to, Urgent: Urgent
        },
        success: function (response) {
            alert("Đăng ký thành công !");
            location.reload();
            document.querySelectorAll('.close').forEach(button => button.click());           
            _load_confirm();
        }
    })
}

async function _load_confirm() {

    var Urgent = document.getElementById("trangthaidon").value;
    var Total = document.getElementById("giadonhang").value;
    var Code_Request = document.getElementById("mnl").value;
    var INT_STEP = document.getElementById("tinhtrangdon").value;

    var us = document.getElementById("us").innerHTML;
    const formData = new URLSearchParams();
    formData.append('us', us);
    formData.append('Urgent', Urgent);
    formData.append('Total', Total);
    formData.append('Code_Request', Code_Request);
    formData.append('INT_STEP', INT_STEP);

    fetch('/Request/_get_confirm', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData.toString()
    })
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(data => {
            console.log(data);
            const tbody = document.getElementById('list_approve');
            if (!data || data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="21" style="text-align:center">Không có dữ liệu</td></tr>';
                return;
            }
            // Cấu hình trạng thái: [Tên hiển thị, Badge class, Vị trí bước hiện tại/từ chối]
            const stepMap = {
                "0": ["Đợi người dự thảo", "warning", 1],
                "1": ["Đợi người thẩm tra", "info", 2],
                "2": ["Đợi người phê duyệt", "pink", 3],
                "3": ["Đợi bên tiếp nhận", "success", 4],
                "4": ["Đợi phụ trách kho", "light", 5],
                "5": ["Hoàn thành", "secondary", 6], // Vị trí 6 để tất cả 5 chấm đều success
                "6": ["Bị từ chối", "danger", 1],
                "7": ["Bị từ chối", "danger", 2],
                "8": ["Bị từ chối", "danger", 3],
                "9": ["Bị từ chối", "danger", 4],
                "10": ["Bị từ chối", "danger", 5]
            };
            tbody.innerHTML = data.map(item => {
                const s = item.inT_STEP;
                const config = stepMap[s] || ["Không xác định", "dark", 0];
                const isReject = parseInt(s) >= 6;
                const currentStep = config[2];

                // Tạo các dấu chấm (chamtron)
                const dots = Array.from({ length: 5 }, (_, i) => {
                    const stepIdx = i + 1;
                    if (isReject && stepIdx === currentStep) return "<span class='text-danger'><b>×</b></span>";
                    if (stepIdx < currentStep) return "<span class='text-success'><b>✓</b></span>";
                    if (stepIdx === currentStep) return `<span class='text-warning'>${s === "5" ? "✓" : "◉"}</span>`;
                    return "<span></span>";
                });

                const urg = item.urgent == "True" ? "<b class='text-danger'><i>* Gấp</i></b>" : "Thông thường";
                const trangthai = `<div class="badge badge-pill badge-${config[1]} mb-1">${config[0]}</div>`;

                return `
                <tr>
                    <td><input type="checkbox" class="item" value="${item.code_Request}_${item.inT_STEP}"/></td>
                    <td class="text-center" id="${item.code_Request}" onclick="_modal_info(this.id, '${item.inT_STEP}')"><button class="btn btn-outline-primary"><i class="fa fa-info"></i></button></td>
                    <td>${urg}</td>
                    <td>${trangthai}</td>
                    <td>${item.code_Request}</td>
                    <td>${item.cost_Center}</td>
                    <td>${item.create_Date}</td>
                    <td>${item.dealine}</td>
                    <td>${item.total}</td>
                    <td>${item.user_Create}</td>
                    <td>${item.chR_TEN_NGUOIYEUCAU} ${dots[0]}</td>
                    <td>${item.chR_TEN_NGUOITHAMTRA} ${dots[1]}</td>
                    <td>${item.chR_TEN_NGUOIPHEDUYET} ${dots[2]}</td>
                    <td>${item.chR_TEN_XACNHAN} ${dots[3]}</td>
                    <td>${item.chR_TEN_XUATKHO} ${dots[4]}</td>
                  
                </tr>`;
            }).join('');
        })
        .catch(err => console.error('Fetch error:', err));
}
async function _load_confirm_GA() {

    var Urgent = document.getElementById("trangthaidon").value;
    var Total = document.getElementById("giadonhang").value;
    var Code_Request = document.getElementById("mnl").value;
    var INT_STEP = document.getElementById("tinhtrangdon").value;

    var us = document.getElementById("us").innerHTML;
    const formData = new URLSearchParams();
    formData.append('us', us);
    formData.append('Urgent', Urgent);
    formData.append('Total', Total);
    formData.append('Code_Request', Code_Request);
    formData.append('INT_STEP', INT_STEP);

    fetch('/Request/_get_confirm_GA', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData.toString()
    })
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(data => {
            console.log(data);
            const tbody = document.getElementById('list_approve');
            if (!data || data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="21" style="text-align:center">Không có dữ liệu</td></tr>';
                return;
            }
            // Cấu hình trạng thái: [Tên hiển thị, Badge class, Vị trí bước hiện tại/từ chối]
            const stepMap = {
                "0": ["Đợi người dự thảo", "warning", 1],
                "1": ["Đợi người thẩm tra", "info", 2],
                "2": ["Đợi người phê duyệt", "pink", 3],
                "3": ["Đợi đảm nhiệm kho", "success", 4],
                "4": ["Đợi QLSC phòng tiếp nhận", "light", 5],
                "5": ["Đợi QLTC phòng tiếp nhận", "secondary", 6], // Vị trí 6 để tất cả 5 chấm đều success
                "6": ["Bị từ chối", "danger", 1],
                "7": ["Bị từ chối", "danger", 2],
                "8": ["Bị từ chối", "danger", 3],
                "9": ["Bị từ chối", "danger", 4],
                "10": ["Bị từ chối", "danger", 5]
            };
            tbody.innerHTML = data.map(item => {
                const s = item.inT_STEP;
                const config = stepMap[s] || ["Không xác định", "dark", 0];
                const isReject = parseInt(s) >= 6;
                const currentStep = config[2];

                // Tạo các dấu chấm (chamtron)
                const dots = Array.from({ length: 6 }, (_, i) => {
                    const stepIdx = i + 1;
                    if (isReject && stepIdx === currentStep) return "<span class='text-danger'><b>×</b></span>";
                    if (stepIdx < currentStep) return "<span class='text-success'><b>✓</b></span>";
                    if (stepIdx === currentStep) return `<span class='text-warning'>${s === "6" ? "✓" : "◉"}</span>`;
                    return "<span></span>";
                });

                const urg = item.urgent == "True" ? "<b class='text-danger'><i>* Gấp</i></b>" : "Thông thường";
                const trangthai = `<div class="badge badge-pill badge-${config[1]} mb-1">${config[0]}</div>`;

                return `
                <tr>
                    <td><input type="checkbox" class="item" value="${item.code_Request}_${item.inT_STEP}"/></td>
                    <td class="text-center" id="${item.code_Request}" onclick="_modal_info(this.id, '${item.inT_STEP}')"><button class="btn btn-outline-primary"><i class="fa fa-info"></i></button></td>
                    <td>${urg}</td>
                    <td>${trangthai}</td>
                    <td>${item.code_Request}</td>
                    <td>${item.cost_Center}</td>
                    <td>${item.create_Date}</td>
                    <td>${item.dealine}</td>
                    <td>${item.total}</td>
                    <td>${item.user_Create}</td>
                    <td>${item.chR_TEN_NGUOIYEUCAU} ${dots[0]}</td>
                    <td>${item.chR_TEN_NGUOITHAMTRA} ${dots[1]}</td>
                    <td>${item.chR_TEN_NGUOIPHEDUYET} ${dots[2]}</td>
                     <td>${item.chR_TEN_XUATKHO} ${dots[3]}</td>
                    <td>${item.chR_TEN_QLSC} ${dots[4]}</td>
                    <td>${item.chR_TEN_QLTC} ${dots[5]}</td>
                   
                  
                </tr>`;
            }).join('');
        })
        .catch(err => console.error('Fetch error:', err));
}
function _modal_info(cost_request, step) {
    $.ajax({
        url: '/Request/_get_request',
        type: 'POST',
        dataType: 'JSON',
        data: {
            cost_request: cost_request
        },
        success: function (response) {
            document.getElementById("modal-7").click();
            document.getElementById("load_detail_0").innerHTML = "";
            document.getElementById("madonhang_0").innerHTML = "*" + response[0].code_Request + "*";
            document.getElementById("mbp_0").innerHTML = response[0].cost_Center_Group;
            document.getElementById("mpb_yc_0").innerHTML = response[0].cost_Center;
            document.getElementById("tenphongban_0").innerHTML = response[0].name_Dept;
            document.getElementById("nyc_0").innerHTML = response[0].creat_Date.split(' ')[0];
            document.getElementById("thmm_0").innerHTML = response[0].dealine.split(' ')[0];
            document.getElementById("khoi_0").innerHTML = response[0].group_Code.split(' ')[0];
            document.getElementById("id_request_0").innerHTML = response[0].id_Request;
            document.getElementById("urgent_0").innerHTML = response[0].urgent;
            document.getElementById("step_0").innerHTML = step;
            if (step == "0") {
                document.getElementById("regency_0").innerHTML = "NGUOIYEUCAU";
            }
            if (step == "1") {
                document.getElementById("regency_0").innerHTML = "NGUOITHAMTRA";
            }
            if (step == "2") {
                document.getElementById("regency_0").innerHTML = "NGUOIPHEDUYET";
            }
            if (step == "3") {
                document.getElementById("regency_0").innerHTML = "XACNHAN";
            }
            if (step == "4") {
                document.getElementById("regency_0").innerHTML = "XUATKHO";
            }
            for (var i = 0; i < response.length; i++) {
                var tongtien = response[i].total_exchange;
                document.getElementById("load_detail_0").innerHTML += `<td>${i + 1}</td><td>${response[i].material_Code}</td><td>${response[i].material_Name}</td><td>${response[i].unit}</td><td>${response[i].account_Code}</td><td>${response[i].account_Name}</td><td>${response[i].amount}</td><td>${response[i].unit}</td><td>${response[i].price}</td><td>${response[i].currency}</td><td>${tongtien}</td><td>${response[i].aim}</td>`;
            }
        }
    })
}
function _update_request(id) {

    var id_request = document.getElementById("id_request" + id).innerHTML;
    var regency = document.getElementById("regency" + id).innerHTML;
    var step = document.getElementById("step" + id).innerHTML;
    var urgent = document.getElementById("urgent" + id).innerHTML;

    const params = new URLSearchParams();
    params.append('id_request', id_request);
    params.append('regency', regency);
    params.append('step', step);
    params.append('urgent', urgent);

    fetch('/Request/_update_request', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    }).then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
        .then(data => {
            alert(data);
            /*  document.querySelectorAll('.close').forEach(button => button.click());*/
            document.getElementById("cls_" + id).innerHTML = "";
            var us = document.getElementById("us").innerHTML;
            _load_confirm(us);
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });

}
function _update_request_GA(id) {

    var id_request = document.getElementById("id_request" + id).innerHTML;
    var regency = document.getElementById("regency" + id).innerHTML;
    var step = document.getElementById("step" + id).innerHTML;
    var urgent = document.getElementById("urgent" + id).innerHTML;

    const params = new URLSearchParams();
    params.append('id_request', id_request);
    params.append('regency', regency);
    params.append('step', step);
    params.append('urgent', urgent);

    fetch('/Request/_update_request_GA', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    }).then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
        .then(data => {
            alert(data);
            /*   document.querySelectorAll('.close').forEach(button => button.click());*/
            document.getElementById("cls_" + id).innerHTML = "";
            var us = document.getElementById("us").innerHTML;
            _load_confirm_GA(us);
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });

}
function _update_request_all(request) {

    var us = document.getElementById("us").innerHTML;
    const params = new URLSearchParams();
    params.append('us', us);
    params.append('madon', request);
 
    fetch('/Request/_update_dongytatca', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    }).then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
        .then(data => {
           
            _load_confirm(us);
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });

}
function _update_request_all_GA(request) {

    var us = document.getElementById("us").innerHTML;
    const params = new URLSearchParams();
    params.append('us', us);
    params.append('madon', request);

    fetch('/Request/_update_dongytatca_GA', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    }).then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
        .then(data => {

            _load_confirm_GA(us);
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });

}
function _dongy_all() {

    const checkboxes = document.querySelectorAll('input.item');
  
    checkboxes.forEach((item) => {
        _update_request_all(item.value);
    });
    alert("Hoàn thành !");
}
function _dongy_all_GA() {

    const checkboxes = document.querySelectorAll('input.item');

    checkboxes.forEach((item) => {
        _update_request_all_GA(item.value);
    });
    alert("Hoàn thành !");
}
function _reject(id) {
    var id_request = document.getElementById("id_request" + id).innerHTML;
    var regency = document.getElementById("regency" + id).innerHTML;
    var step = document.getElementById("step" + id).innerHTML;
    var urgent = document.getElementById("urgent" + id).innerHTML;
    var reason = document.getElementById("reason" + id).value;

    const params = new URLSearchParams();
    params.append('id_request' + id, id_request);
    params.append('regency' + id, regency);
    params.append('step' + id, step);
    params.append('urgent' + id, urgent);
    params.append('reason' + id, reason);

    fetch('/Request/_reject', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    }).then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
        .then(data => {
            alert("Từ chối đơn yêu cầu !");
            document.getElementById("cls_" + id).innerHTML = "";
            var us = document.getElementById("us").innerHTML;
            _load_confirm(us);

        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
function _reset() {
    document.getElementById("trangthaidon").value = "";
    document.getElementById("giadonhang").value = "";
    document.getElementById("mnl").value = "";
    document.getElementById("tinhtrangdon").value = "";
    _load_confirm();
}

