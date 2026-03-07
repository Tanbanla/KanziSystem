async function _Tainhap() {

    var khoi = document.getElementById("khoi").value;
    var malinhkien = document.getElementById("MaLinhKien").value;
    var soluong = document.getElementById("SoLuong").value;
    var kho = document.getElementById("Kho").value;
    /*var vitri = document.getElementById("ViTri").value;*/
    var thoigian = document.getElementById("ThoiGian").value;
    var giatien = document.getElementById("GiaTien").value;
    var ghichu = document.getElementById("ghichu").value;
    var nguoichuyen = document.getElementById("us").innerHTML;
    var phongban = document.getElementById("dept_us").textContent;

    const params = new URLSearchParams();
    params.append('khoi', khoi);
    params.append('malinhkien', malinhkien);
    params.append('kho', kho);
    params.append('soluong', soluong);
   /* params.append('vitri', vitri);*/
    params.append('thoigian', thoigian);
    params.append('giatien', giatien);
    params.append('ghichu', ghichu);
    params.append('nguoichuyen', nguoichuyen);
    params.append('phongban', phongban);
    if (khoi == "" || malinhkien == "" || soluong == "" || kho == "" || thoigian == "" || giatien == "") {
        alert("Không được để dữ liệu trống !");
    }
    else {
        fetch('/Master/Tainhap', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                alert(data);
                _load_tainhap();
            })
            .catch(error => {
                console.error('There was a problem with the fetch operation:', error);
            });
    }
   
}
async function _load_name_inv() {
    document.getElementById("MaLinhKien").value = "";
    document.getElementById("SoLuong").value = "";
    document.getElementById("GiaTien").value = "";
    document.getElementById("ghichu").value = "";
    var group_code = document.getElementById("khoi").value;
    const params = new URLSearchParams({ group_code: group_code });
    fetch('/Import/_load_material', {
        method: 'POST',
        body: params
    })
        .then(res => res.json())
        .then(data => {
            document.getElementById("list_mlk").innerHTML = "";
            data.forEach((item) => {
                document.getElementById("list_mlk").innerHTML += `<option>${item}</option>`;
            });
        });
}
async function _load_material(id) {
    let idd = id.split(':')[0];
    var groupcode = document.getElementById("khoi").value;
    const params = new URLSearchParams();
    params.append('Material_Code', idd);
    params.append('Group_Code', groupcode);

    fetch('/Import/_info_material', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("donvi").innerHTML = "";
            const uniqueInventories = [...new Set(data.map(item => (item.unit+ " (" + item.unit_Note + ") ")))];

            const options = uniqueInventories
                .map(name => `<option>${name}</option>`)
                .join("");

            document.getElementById("donvi").innerHTML = options;
        });
}
async function _load_wh() {
    fetch('/Master/load_warehouse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },        
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
async function _load_warehouse() {

    fetch('/Master/load_warehouse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        /* body: params.toString()*/
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
           
            document.getElementById("Kho").innerHTML = ``;
            data.forEach((item) => {
                document.getElementById("Kho").innerHTML += `<option>${item.chR_WAREHOUSE}</option>`;
                document.getElementById("khoo").innerHTML += `<option>${item.chR_WAREHOUSE}</option>`;
            });
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
async function _get_location() {

    fetch('/Master/Get_location', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("ViTri").innerHTML = ``;
            data.forEach((item) => {                
                document.getElementById("ViTri").innerHTML += `<option>${item.split(':')[1]}</option>`;
            });
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
async function _load_tainhap() {
    var Group_Code = document.getElementById("khoi_tk").value;
    var MaNguyenLieu = document.getElementById("manvl").value;
    var Material_Name = document.getElementById("tennvl").value;
    var Kho = document.getElementById("khoo").value;
   
    const params = new URLSearchParams();
    params.append('Group_Code', Group_Code);
    params.append('MaNguyenLieu', MaNguyenLieu);
    params.append('Material_Name', Material_Name);
    params.append('Kho', Kho);
  

    fetch('/Master/Load_tainhap', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
           
            document.getElementById("entriesBody").innerHTML = "";
            data.forEach((item) => {
                var btn = `<td class="text-center"><button type="button" class="btn btn-outline-success" onclick="load_modal('${item.id_Kho}')"><i class="fa fa-pen"></i> sửa</button></td>
                        <td class="text-center"><button class="btn btn-outline-danger" onclick="del_tainhap('${item.id_Kho}')" ><i class="fa fa-trash"></i> xóa</button> </td>`
                document.getElementById("entriesBody").innerHTML += `<tr><td>${item.group_Code}</td><td id="ma_${item.id_Kho}">${item.maNguyenLieu}</td><td id="ten_${item.id_Kho}">${item.material_Name}</td><td id="kho_${item.id_Kho}">${item.kho}</td><td>${item.hientai}</td><td id="sl_${item.id_Kho}">${item.qtY_RE_IMPORT}</td><td id="dv_${item.id_Kho}">${item.unit}</td><td id="gt_${item.id_Kho}">${item.giA_TAI_NHAP}</td><td>${parseFloat(item.qtY_RE_IMPORT) + parseFloat(item.hientai) }</td><td>${item.dtM_UPDATE}</td>${btn}</tr>`
            });
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
async function _ref() {
    document.getElementById("khoi_tk").value = "";
    document.getElementById("manvl").value = "";
    document.getElementById("tennvl").value = "";
    document.getElementById("khoo").value = "";
    _load_tainhap();
}
async function del_tainhap(id) {
    var x = confirm("Bạn có muốn xóa số lượng tái nhập ?");
    if (x) {
        const params = new URLSearchParams();
        params.append('id', id);

        fetch('/Master/del_tainhap', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                alert(data);
                _load_tainhap();
            })
            .catch(error => {
                console.error('There was a problem with the fetch operation:', error);
            });
    }  
}
async function load_modal(id) {
    document.getElementById("ma_tn").textContent = document.getElementById("ma_" + id).innerHTML + "-" + document.getElementById("ten_" + id).innerHTML;
    document.getElementById("soluong_tn").value = document.getElementById("sl_" + id).innerHTML;
    document.getElementById("donvi_tn").value = document.getElementById("dv_" + id).innerHTML;
    document.getElementById("giatien_tn").value = document.getElementById("gt_" + id).innerHTML;
    document.getElementById("kho_tn").value = document.getElementById("kho_" + id).innerHTML;
    document.getElementById("id_tn").innerHTML = id;
    document.getElementById("modal-13").click();
}
async function editt() {

    var id = document.getElementById("id_tn").textContent;
    var soluong = document.getElementById("soluong_tn").value;
    var donvi = document.getElementById("donvi_tn").value;
    var giatien = document.getElementById("giatien_tn").value;
    var kho = document.getElementById("kho_tn").value; 

    const params = new URLSearchParams();
    params.append('id', id);
    params.append('soluong', soluong);
    params.append('donvi', donvi);
    params.append('giatien', giatien);
    params.append('kho', kho);

    fetch('/Master/edit_tainhap', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            alert(data);          
            document.querySelectorAll('.close').forEach(button => button.click());
            _load_tainhap();
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
