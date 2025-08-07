document.addEventListener("DOMContentLoaded", () => {
    const listDom = document.getElementById("shipperList");
    listDom.innerHTML = "";
    fetch("/api/users/shipper")
        .then((response) => {
            if (response.status === 422 || response.status === 401) {
                return response.json();
            }

            if (!response.ok) {
                return showErrorToast(
                    "Lỗi khi tải dử liệu. Vui lòng thử lại sau.",
                    4000
                );
            }

            return response.json();
        })
        .then((data) => {
            if (data.length > 0) {
                let list = "";
                for (var item of data) {
                    list += `<option value="${item.id}">${item.name} (${item.email}) - Đang giao ${item.totalDeliveryOrders} đơn hàng</option>`;
                }
                listDom.innerHTML = list;
            }
        });
});

let selectedId = 0;

function startSelected(id) {
    selectedId = id;
}

function confirmTransport(id) {
    fetch(`/api/orders/confirm-transport/${id}`, {
            method: "PUT",
        })
        .then((response) => {
            if (!response.ok) {
                if (response.status === 422 || response.status === 401) {
                    return response.json();
                }

                if (!response.ok) {
                    return showErrorToast(
                        "Lỗi khi hủy đơn hàng. Vui lòng thử lại sau.",
                        4000
                    );
                }

                return response.json();
            }
            return response.json();
        })
        .then((data) => {
            if (data.code == "INPUT_DATA_ERROR") {
                showWarningToast(data.message, 4000);
                return;
            }

            if (data.code == "ORDER_CONFIRM_TRANSPORT_SUCCESS") {
                showSuccessToast(data.message, 4000);
                setTimeout(() => location.reload(), 1000);
                return;
            } else {
                showErrorToast(data.message, 4000);
            }
        });
}

function assignment() {
    const user = document.getElementById("shipperList").value;

    if (!user) {
        showWarningToast("Vui lòng chọn người muốn phân công giao hàng");
        return;
    }

    fetch(`/api/orders/assignment`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                ShipperId: user,
                OrderId: selectedId
            })
        })
        .then((response) => {
            if (!response.ok) {
                if (response.status === 422 || response.status === 401) {
                    return response.json();
                }

                if (!response.ok) {
                    return showErrorToast(
                        "Lỗi khi phân công giao hàng. Vui lòng thử lại sau.",
                        4000
                    );
                }

                return response.json();
            }
            return response.json();
        })
        .then((data) => {
            if (data.code == "INPUT_DATA_ERROR") {
                showWarningToast(data.message, 4000);
                return;
            }

            if (data.code == "ORDER_ASSIGNMENT_TRANSPORT_SUCCESS") {
                const deleteModal = document.getElementById("assignmentModal");
                const modal = bootstrap.Modal.getInstance(deleteModal);
                if (modal) {
                    modal.hide();
                } else {
                    new bootstrap.Modal(createModal).hide();
                }
                showSuccessToast(data.message, 4000);
                setTimeout(() => location.reload(), 1000);
                return;
            } else {
                showErrorToast(data.message, 4000);
            }
        });
}

function search() {
    const startTime = document.getElementById("startTime").value;
    const endTime = document.getElementById("endTime").value;

    const params = [];
    if (startTime && startTime < endTime) params.push(`start=${startTime}`);
    if (endTime && endTime > startTime)
        params.push(`end=${endTime}`);

    const queryString = params.length > 0 ? params.join("&") : "";
    location.href = `/delivery?${queryString}`;
}

/**
* Hiển thị thông báo Thành công (màu xanh lá gradient).
* @param {string} message - Nội dung thông báo.
* @param {number} [duration=3000] - Thời gian hiển thị (mili giây).
*/
function showSuccessToast(message, duration = 3000) {
    Toastify({
        text: message,
        duration: duration,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #00b099, #96c93d)", // Màu xanh lá gradient
        stopOnFocus: true,
        close: true
    }).showToast();
}

/**
 * Hiển thị thông báo Cảnh báo (màu cam/vàng gradient).
 * @param {string} message - Nội dung thông báo.
 * @param {number} [duration=5000] - Thời gian hiển thị (mili giây).
 */
function showWarningToast(message, duration = 5000) {
    Toastify({
        text: message,
        duration: duration,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #ffc400, #ff8c00)", // Màu cam/vàng gradient
        stopOnFocus: true,
        close: true
    }).showToast();
}

/**
 * Hiển thị thông báo Lỗi (màu đỏ gradient).
 * @param {string} message - Nội dung thông báo.
 * @param {number} [duration=7000] - Thời gian hiển thị (mili giây).
 */
function showErrorToast(message, duration = 7000) {
    Toastify({
        text: message,
        duration: duration,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #ff5f6d, #ffc371)", // Màu đỏ/cam gradient
        stopOnFocus: true,
        close: true
    }).showToast();
}
