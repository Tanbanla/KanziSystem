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
    var Kind = "OUT";
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

    var ten_dt = document.getElementById("ten_duthao").value;
    var ten_tt = document.getElementById("ten_thamtra").value;
    var ten_pd = document.getElementById("ten_pheduyet").value;

    var mail_dt = document.getElementById("mail_duthao").value;
    var mail_tt = document.getElementById("mail_thamtra").value;
    var mail_pd = document.getElementById("mail_pheduyet").value;

    $.ajax({
        url: '/Request/_Insert_request',
        type: 'POST',
        dataType: 'JSON',
        data: {
            Cost_Center: Cost_Center, Declaration: Declaration, Dealine: Dealine, Total_exchange: Total_exchange, Exchange_rate: Exchange_rate, Currency: Currency, Total: Total, Kind: Kind,
            Type: Type, Status: Status, Place: Place, Loaihinhtokhai: Loaihinhtokhai, Group_Code: Group_Code, Chophepin: Chophepin, Urgent: Urgent, User_Create: User_Create, rq: dataList,
            adid_dt: adid_dt, adid_tt: adid_tt, adid_pd: adid_pd, ten_dt: ten_dt, ten_tt: ten_tt, ten_pd: ten_pd, mail_dt: mail_dt, mail_tt: mail_tt, mail_pd: mail_pd
        },
        success: function (response)
        {
            send_mail(mail_dt, Urgent);
          /*  document.getElementById("dong").click();*/
        }
    })   
}
function _load_rate() {
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
function _load_phongchiuphi() {
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
async function send_mail(adid, Urgent) {
    $.ajax({
        url: '/Request/_send_mail',
        type: 'POST',
        dataType: 'JSON',
        data: {
            adid: adid, Urgent: Urgent
        },
        success: function (response) {

        }
    })
}
function _load_confirm(us) {

    const formData = new URLSearchParams();
    formData.append('us', us);
 
    fetch('/Request/_get_confirm', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData.toString()
    })
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(data => {
            const tbody = document.getElementById('list_approve');
            if (!data || data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="21" style="text-align:center">Không có dữ liệu</td></tr>';
                return;
            }
            // Cấu hình trạng thái: [Tên hiển thị, Badge class, Vị trí bước hiện tại/từ chối]
            const stepMap = {
                "0": ["Đợi người dự thảo", "warning", 1],
                "1": ["Đợi người thẩm tra", "info", 2],
                "2": ["Đợi người phê duyệt", "info", 3],
                "3": ["Đợi bên tiếp nhận", "secondary", 4],
                "4": ["Đợi phụ trách kho", "warning", 5],
                "5": ["Hoàn thành", "success", 6], // Vị trí 6 để tất cả 5 chấm đều success
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
             
                const urg = item.urgent == "True" ? "<b class='text-danger'>Gấp</b>" : "Thông thường";
                const trangthai = `<div class="badge badge-pill badge-${config[1]} mb-1">${config[0]}</div>`;

                return `
                <tr>
                    <td><input type="checkbox" /></td>
                    <td id="${item.code_Request}" onclick="_modal_info(this.id, '${item.inT_STEP}')"><i class="fa fa-info text-info"></i></td>
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
                    <td>${dots[3]}</td>
                    <td>${dots[4]}</td>
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
            console.log(response);
            document.getElementById("modal-7").click();
            document.getElementById("load_detail").innerHTML = "";
            document.getElementById("madonhang").innerHTML = "*" + response[0].code_Request + "*" ;
            document.getElementById("mbp").innerHTML = response[0].cost_Center_Group;
            document.getElementById("mpb_yc").innerHTML = response[0].cost_Center;
            document.getElementById("tenphongban").innerHTML = response[0].name_Dept;
            document.getElementById("nyc").innerHTML = response[0].creat_Date.split(' ')[0];
            document.getElementById("thmm").innerHTML = response[0].dealine.split(' ')[0];
            document.getElementById("khoi").innerHTML = response[0].group_Code.split(' ')[0];
            document.getElementById("id_request").innerHTML = response[0].id_Request;
            document.getElementById("step").innerHTML = step;
            if (step == "0") {
                document.getElementById("regency").innerHTML = "NGUOIYEUCAU";
            }
            if (step == "1") {
                document.getElementById("regency").innerHTML = "NGUOITHAMTRA";
            }
            if (step == "2") {
                document.getElementById("regency").innerHTML = "NGUOIPHEDUYET";
            }
            if (step == "3") {
                document.getElementById("regency").innerHTML = "XACNHAN";
            }
          
            for (var i = 0; i < response.length; i++) {
                var tongtien = response[i].total_exchange;
                document.getElementById("load_detail").innerHTML += `<td>${i + 1}</td><td>${response[i].material_Code}</td><td>${response[i].material_Name}</td><td>${response[i].unit}</td><td>${response[i].account_Code}</td><td>${response[i].account_Name}</td><td>${response[i].amount}</td><td>${response[i].unit}</td><td>${response[i].price}</td><td>${response[i].currency}</td><td>${tongtien}</td><td>${response[i].aim}</td>`;
            }
        }
    })
}
function _update_request() {

    var id_request = document.getElementById("id_request").innerHTML;
    var regency = document.getElementById("regency").innerHTML;
    var step = document.getElementById("step").innerHTML;

    const params = new URLSearchParams();
    params.append('id_request', id_request);
    params.append('regency', regency);
    params.append('step', step);

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
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });

}