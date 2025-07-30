let selectedId = 0;
let orderId = 0;

let connection = null;
window.addEventListener("load", function () {
    initConnection();
});
window.addEventListener("beforeunload", function () {
    if (connection) {
        connection.stop();
    }
});

function initConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/realtime-hub")
        .withAutomaticReconnect()
        .build();

    connection.onreconnected(() => {
        // xử lý sự kiện khi kết nối lại thành công
    });

    connection.onclose(() => {
        // xử lý sự kiện khi mất kết nối
    });
}

async function connectPaymentRealtime() {
    try {
        await connection.start();

        if (!connection) {
            return;
        }

        await connection.invoke("ConnectPaymentTracking", orderId);

        connection.off("Paid");

        connection.on("Paid", (data) => {
            if (data.ok) {
                document.getElementById("status_payment").innerHTML = `
               
                                        <div class="status-section">
                                        Trạng thái: Đã thanh toán&nbsp;
                                        <span style="color: green; font-size: 1.2em;"><i class="fa-solid fa-circle-check"></i></span>
                                    </div>
                `;
                showSuccessToast("Thanh toán thành công!");
            }
        });
    } catch (e) {
        console.log(e);
    }
}

async function leavePaymentRealtime() {
    try {
        await connection.stop();
    } catch (e) {
        console.log(e);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    var paymentModal = document.getElementById("paymentModal");
    paymentModal.addEventListener("hidden.bs.modal", function () {
        leavePaymentRealtime();
    });
});

function startPayment(id, totalBill, totalPrice, discount, fee, vat) {
    document.getElementById(
        "paymentQrCode"
    ).src = `https://qr.sepay.vn/img?acc=0978266980&bank=MBBank&amount=${totalBill}&des=DH${id}&template=compact`;
    document.getElementById("bank").textContent = "MBBank";
    document.getElementById("account_name").textContent = "NGUYEN DUC ANH";
    document.getElementById("account_no").textContent = "0978266980";
    document.getElementById(
        "bill"
    ).textContent = `${totalBill.toLocaleString("vi-VN")} ₫`;
    document.getElementById("orderId").textContent = `DH${id}`;
    document.getElementById("orderId2").textContent = `DH${id}`;
    document.getElementById(
        "totalPrice"
    ).textContent = `${totalPrice.toLocaleString("vi-VN")}₫`;
    if (Number(discount.replace(",", ".")) > 0) {
        document.getElementById(
            "discount"
        ).innerHTML = `<p>Giảm giá <span class="float-end text-danger">-${Number(discount.replace(",", ".")).toLocaleString(
            "vi-VN"
        )}₫</span></p>`;
    }
    document.getElementById(
        "totalBill"
    ).textContent = `${totalBill.toLocaleString("vi-VN")}₫`;
    document.getElementById("fee").textContent = `${fee.toLocaleString(
        "vi-VN"
    )}₫`;
    document.getElementById("vat").textContent = `${vat.toLocaleString(
        "vi-VN"
    )}₫`;
    document.getElementById("status_payment").innerHTML = `
                                    <div class="status-section">
                                        Trạng thái: Chờ thanh toán...
                                        <div id="status" class="spinner-border spinner-border-sm text-secondary" role="status">
                                            <span class="visually-hidden">Loading...</span>
                                        </div>
                                    </div>
                                   
          `;
    orderId = `DH${id}`;
    connectPaymentRealtime();
}

function startSelected(id) {
    selectedId = id;
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
    location.href = `/history?${queryString}`;
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
