function startViewMap(lat, lng) {
    const url = `https://www.google.com/maps/dir/?api=1&origin=current_location&destination=${lat},${lng}&travelmode=motorcycling`;
    location.href = url;
}

let selectedId = 0;

function startSelected(id) {
    selectedId = id;
}

function complete(id) {
    fetch(`/api/orders/complete/${id}`, {
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

            if (data.code == "ORDER_COMPLETED_TRANSPORT_SUCCESS") {
                showSuccessToast(data.message, 4000);
                setTimeout(() => location.reload(), 1000);
                return;
            } else {
                showErrorToast(data.message, 4000);
            }
        });
}

function cancel() {
    fetch(`/api/orders/cancel/${selectedId}`, {
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

            if (data.code == "ORDER_CANCELED_SUCCESS") {
                const deleteModal = document.getElementById("deleteModal");
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
    const status = document.getElementById("status").value;

    const params = [];
    if (startTime && startTime < endTime) params.push(`start=${startTime}`);
    if (endTime && endTime > startTime)
        params.push(`end=${endTime}`);
    if (status)
        params.push(`status=${status}`);

    const queryString = params.length > 0 ? params.join("&") : "";
    location.href = `/recevied?${queryString}`;
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
