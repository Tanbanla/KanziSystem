(function () {
    'use strict';
    var form = document.getElementById('materialForm');
    var submitBtn = document.getElementById('submitBtn');
    var btnText = document.getElementById('btnText');
    var btnSpinner = document.getElementById('btnSpinner');

    form.addEventListener('submit', function (event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        } else {
            // Show spinner and let form submit (or perform AJAX here)
            btnText.textContent = 'Saving...';
            btnSpinner.classList.remove('d-none');
            // If you want to prevent real submit for demo, uncomment:
            // event.preventDefault();
            const dto = {
                Material_Code: document.getElementById("MaterialCode").value, // Required
                Material_Name_VN: document.getElementById("MaterialNameVN").value, // Required
                Material_Name_EN: document.getElementById("MaterialNameEN").value,
                Material_Name_JP: document.getElementById("MaterialNameJP").value,
                Account_Code: document.getElementById("AccountCode").value,
                Account_Name_EN: document.getElementById("AccountNameEN").value,
                Account_Name_VN: document.getElementById("AccountNameVN").value,
                Unit: document.getElementById("Unit").value,
                Unit_Note: document.getElementById("UnitNote").value,
                Price: parseFloat(document.getElementById("Price").value) || null,
                Currency: document.getElementById("Currency").value,
                Group_Code: document.getElementById("GroupCode").value,
                GoodKind: document.getElementById("GoodKind").value,
            };

            fetch('/Material/Create_Material', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(dto)
            })
                .then(
                    response => { return response.json() }
                )
                .then(res => {

                    console.log("response", res)
                    if (res.success) {
                        // Close modal, reload page or update table
                        $('#createModal').modal('hide');
                        alert('Thêm mới thành công!');
                        location.reload();
                    }
                    else {
                        alert(res.message);
                    }
                })
                .catch((error) => {
                   
                });
        }
        form.classList.add('was-validated');
    }, false);
})();

function handleFileSelect(event) {
    const file = event.target.files[0];  // Lấy file đầu tiên (hoặc loop nếu multiple)
    if (!file) return;

    // Kiểm tra loại file (tùy chọn)
    const allowedTypes = ['text/csv', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
    if (!allowedTypes.includes(file.type)) {
        alert('Please select a valid CSV or Excel file.');
        return;
    }

    // Tạo FormData để gửi file
    const formData = new FormData();
    formData.append('file', file);

    // Gửi file lên server (endpoint mới, xem bước 3)
    fetch('/Material/ImportMaterials', {
        method: 'POST',
        body: formData  // Không cần Content-Type, browser tự set
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Import successful!');
                location.reload();  // Reload để cập nhật dữ liệu
            } else {
                alert('Import failed: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred during import.');
        });
}

// Thêm hàm này vào cuối file JS (sau code hiện tại)
function handleFileSelect(event) {
    const file = event.target.files[0];
    if (!file) return;

    // Kiểm tra loại file (tùy chọn)
    const allowedTypes = ['text/csv', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
    if (!allowedTypes.includes(file.type)) {
        alert('Please select a valid CSV or Excel file.');
        return;
    }

    // Tạo FormData để gửi file
    const formData = new FormData();
    formData.append('file', file);

    // Gửi file lên server (endpoint mới, xem bước 3 trong phản hồi trước)
    fetch('/Material/ImportMaterials', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Import successful!');
                location.reload();
            } else {
                alert('Import failed: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred during import.');
        });
}
