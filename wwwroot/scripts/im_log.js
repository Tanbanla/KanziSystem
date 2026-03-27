
function _load_log() {
    var madon = document.getElementById("madon").value;
    var ngay_tu = document.getElementById("ngay_tu").value;
    var ngay_den = document.getElementById("ngay_den").value;
    var kho = document.getElementById("kho").value;
    var manguyenlieu = document.getElementById("manguyenlieu").value;
    var loai = document.getElementById("loai").value;
    /*var phong = document.getElementById("phong").value;*/
    //var khoii = document.getElementById("khoii").value;
    var us = document.getElementById("us").innerHTML;

    const params = new URLSearchParams();
    params.append('madon', madon);
    params.append('ngay_tu', ngay_tu);
    params.append('ngay_den', ngay_den);
    params.append('kho', kho);
    params.append('manguyenlieu', manguyenlieu);
    params.append('loai', loai);
  /*  params.append('phong', phong);*/
   // params.append('khoii', khoii);
    params.append('us', us);

    fetch('/Import/_get_log', {
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
            document.getElementById("list_log").innerHTML = "";
            console.log(data);
            data.forEach((item) => {
                document.getElementById("list_log").innerHTML += `<tr>
                    <td>${item.maNguyenLieu}</td>
                    <td>${item.hanhdong}</td>
                    <td>${item.soluong}</td>
                    <td>${item.soluongPO}</td>
                    <td>${item.soPO}</td>
                    <td>${item.donviPO}</td>
                    <td>${item.loai}</td>
                    <td>${item.ngaynhaokho}</td>
                    <td>${item.nguoicapnhat}</td>
                    <td>${item.kho}</td>
                    <td>${item.khoi}</td>                 
                    <td>${item.vitri}</td>
                    <td>${item.phong}</td>
                </tr>`;
            });
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}


async function downloadExcel() {
    // 1. Lấy dữ liệu
    const dateToElement = document.getElementById("date_to");
    const dateFromElement = document.getElementById("date_from");

    if (!dateToElement.value || !dateFromElement.value) {
        alert("Vui lòng chọn ngày!");
        return;
    }

    const parra = new URLSearchParams();
    parra.append('date_to', dateToElement.value);
    parra.append('date_from', dateFromElement.value);

    fetch('/Import/download_log', {

        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: parra.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`Yêu cầu thất bại với mã trạng thái: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
        })
        .catch(error => {
            console.error('Lỗi phán định:', error);
            alert("Phán định thất bại: " + error.message);
        });
}
