document.addEventListener('DOMContentLoaded', function () {
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('log_password');

    togglePassword.addEventListener('click', function () {
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);
        this.querySelector('i').classList.toggle('bi-eye');
        this.querySelector('i').classList.toggle('bi-eye-slash');
    });
});

function Login() {
    const emailOrPhone = document.getElementById("log_emailorphone").value;
    const password = document.getElementById("log_password").value;

    if (!emailOrPhone || !password) {
        let errMsg = [];
        if (!emailOrPhone) errMsg.push("địa chỉ email hoặc số điện thoại");
        if (!password) errMsg.push("mật khẩu");
        showWarningToast(`Vui lòng điền đầy đủ ${errMsg.join(", ")}`, 4000);
        return;
    }

    if (!isValidEmail(emailOrPhone)) {
        if (!isValidPhoneNumber(emailOrPhone)) {
            showWarningToast("Địa chỉ email hoặc số điện thoại không hợp lệ. Vui lòng kiểm tra lại.", 4000);
            return;
        }
    }

    fetch(`/api/users/login`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    EmailOrPhone: emailOrPhone,
                    Password: password
                })
            })
        .then(response => {
            if (response.status === 422 || response.status === 401) {
                return response.json();
            }

            if (!response.ok) {
                return showErrorToast("Lỗi khi đăng ký. Vui lòng thử lại sau.", 4000);
            }

            return response.json();
        })
        .then(data => {
            if (data.code === "INPUT_DATA_ERROR") {
                showWarningToast(data.message, 4000);
                return;
            }

            if (data.code === "LOGIN_SUCCESS") {
                showSuccessToast(data.message, 4000);
                setTimeout(() => location.href = "/", 1000);
                return;
            } else {
                showErrorToast(data.message, 4000);
            }
        });
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function isValidPhoneNumber(phoneNumber, pattern = /^(0|\+84)(3|5|7|8|9)\d{8}$/) {
    if (phoneNumber === null || typeof phoneNumber === 'undefined' || phoneNumber.trim() === '') {
        return false;
    }

    try {
        return pattern.test(phoneNumber);
    } catch (e) {
        return false;
    }
}

function checkPasswordStrength(password) {
    const minLength = 8;
    let hasUppercase = false;
    let hasLowercase = false;
    let hasNumber = false;
    let hasSpecialChar = false;

    if (password.length < minLength) {
        return false;
    }

    for (let i = 0; i < password.length; i++) {
        const char = password[i];

        if (char >= 'A' && char <= 'Z') {
            hasUppercase = true;
        } else if (char >= 'a' && char <= 'z') {
            hasLowercase = true;
        } else if (char >= '0' && char <= '9') {
            hasNumber = true;
        } else {
            hasSpecialChar = true;
        }
    }

    return hasUppercase && hasLowercase && hasNumber && hasSpecialChar;
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