function resett() {
    document.getElementById("show_kho_iv").innerHTML = "";
    document.getElementById("poNumber").value = "";
}

function ImportWarehouse() {
    const url = '/Delivery/ImportWarehouse';
    let table = document.getElementById('show_kho_iv');
    //Scan table with row selected will import to warehouse
    let dateInput = document.getElementById('idTimeDelivery').value;
    let warehouseName = document.getElementById('IdWarehouse').value;

    if (dateInput == '' || warehouseName == '--') {
        alert("Bạn cần nhập đầy đủ : ngày nhận và kho nhận");
        return;
    }

    let [year, month, day] = dateInput.split('-');
    let formattedDate = `${month}/${day}/${year}`;

    for (let i = 0; i < table.rows.length; i++) {
        let row = table.rows[i];
        let cbxSelect = row.cells[0].querySelector('input[type="checkbox"]');
        let txtLuongVeKho = row.cells[10].querySelector('input[type="number"]');
        if (cbxSelect && cbxSelect.checked) {
            let payload = {
                PO_Detail_Id: row.cells[4].innerHTML,
                Id_nhapkho: row.cells[1].innerHTML,
                benXacNhanTruoc: 'STOCK',
                luongvethuctekho: txtLuongVeKho ? txtLuongVeKho.value : '',
                NgayNhap: formattedDate,
                KhoNhan: warehouseName,
                Mahang: row.cells[5].innerHTML,
                Soluong: row.cells[9].innerHTML
            };
            const options = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            };

            fetch(url, options)
                .then(response => response.json())
                .then(result => {
                    alert("Nhận hàng PO " + row.cells[4].innerHTML + " (Mã " + row.cells[1].innerHTML + "): " + result);
                })
                .catch(err => console.error(err));
        }
    }
}

async function SearchPoDel() {
    let GetPO = document.getElementById('poNumber').value;
    let GetDept = document.getElementById('IdDept').value;
    const url = '/Delivery/SearchDataPo';
    let payload =
    {
        PoNumber: GetPO,
        Department: GetDept
    };
    let options =
    {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    };
    try {
        const response = await fetch(url, options);
        const result = await response.json();
        console.log('SearchPoDelivery Data : ', result);
        let opt = document.getElementById('show_kho_iv');
        opt.innerHTML = "";
        result.forEach((value) => {
            opt.innerHTML += `<tr><td><input type="checkbox" class="item" value="${value.pO_Detail_Id}" /></td><td>${value.pO_Detail_Id}</td><td>${value.id_Goc}</td><td>${value.benxacnhantruoc}</td><td>${value.soPO}</td><td>${value.mahang}</td><td>${value.good_Code}</td><td>${value.tentienganh}</td><td>${value.tentiengviet}</td><td id="soluong_${value.pO_Detail_Id}">${value.soluong}</td>`
                + `<td><input type='number' class="form-control" id="luongvethucte_${value.pO_Detail_Id}" onblur="Check_luongvethucte('${value.pO_Detail_Id}')" value='${value.luongvethucte}'></input></td><td>${value.luongvekho}</td><td>${value.luongvekhoNgaynhap}</td><td>${value.luongvekhoNguoinhap}</td><td>${value.dovi}</td><td>${value.dongia}</td><td>${value.dieukiengiaohang}</td>`
                + `<td>${value.diadiemgiaohang}</td><td>${value.phuongthucvanchuyen}</td><td>${value.sotien}</td><td>${value.vat}</td><td>${value.maphongyeucau}</td>`
                + `<td>${value.tenphongyeucau}</td><td>${value.ngaygiaohangdukien}</td><td>${value.noigiaodukien}</td>`
                + `<td>${value.thoigianthanhtoan}</td><td>${value.loaitien}</td>`
                + `<td>${value.tygia}</td><td>${value.doisangUSD}</td><td>${value.danhmuc}</td>`
                + `<td>${value.code_Request}</td><td>${value.tinhtrangtokhai}</td>`
                + `<td>${value.tenNCC} </td><td>${value.id_LichsuNhap} </td><td>${value.luongvekhoKhonhap}</td><td>${value.invoice}</td><td>${value.tinhtranghaiquanPO}</td></tr>`;
        });
    }
    catch (error) {
        console.error('Error : ', error);
    }
}
function Check_luongvethucte(id) {
    let soluong = document.getElementById("soluong_" + id).innerHTML;
    let luongthucte = document.getElementById("luongvethucte_" + id).value;

    if (parseFloat(luongthucte) > parseFloat(soluong)) {
        document.getElementById("luongvethucte_" + id).value = soluong;
    }
}
