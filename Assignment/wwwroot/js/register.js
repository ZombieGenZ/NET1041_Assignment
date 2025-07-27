document.addEventListener('DOMContentLoaded', function () {
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('reg_password');

    togglePassword.addEventListener('click', function () {
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);
        this.querySelector('i').classList.toggle('bi-eye');
        this.querySelector('i').classList.toggle('bi-eye-slash');
    });

    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');
    const confirmPasswordInput = document.getElementById('reg_confirmPassword');

    toggleConfirmPassword.addEventListener('click', function () {
        const type = confirmPasswordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        confirmPasswordInput.setAttribute('type', type);
        this.querySelector('i').classList.toggle('bi-eye');
        this.querySelector('i').classList.toggle('bi-eye-slash');
    });
});

function Register() {
    const username = document.getElementById("reg_username").value;
    const email = document.getElementById("reg_email").value;
    const phone = document.getElementById("reg_phone").value;
    const dateOfBirth = document.getElementById("reg_dateofbirth").value;
    const password = !googleId && !facebookId && !githubId && !discordId ? document.getElementById("reg_password").value : "";
    const confirmPassword = !googleId && !facebookId && !githubId && !discordId ? document.getElementById("reg_confirmPassword").value : "";

    if (!username || !email || !phone || !dateOfBirth) {
        let errMsg = [];
        if (!username) errMsg.push("tên đăng nhập");
        if (!email) errMsg.push("địa chỉ email");
        if (!phone) errMsg.push("số điện thoại");
        if (!dateOfBirth) errMsg.push("ngày sinh");
        showWarningToast(`Vui lòng điền đầy đủ ${errMsg.join(", ")}`, 4000);
        return;
    }

    if (!googleId && !facebookId && !githubId && !discordId) {
        if (!password || !confirmPassword) {
            let errMsg = [];
            if (!password) errMsg.push("mật khẩu");
            if (!confirmPassword) errMsg.push("xác nhận mật khẩu");
            showWarningToast(`Vui lòng điền đầy đủ ${errMsg.join(", ")}`, 4000);
            return;
        }
    }

    if (!isValidEmail(email)) {
        showWarningToast("Địa chỉ email không hợp lệ. Vui lòng kiểm tra lại.", 4000);
        return;
    }

    if (!isValidPhoneNumber(phone)) {
        showWarningToast("Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.", 4000);
        return;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const dateOfBirthDate = new Date(dateOfBirth);
    if (isNaN(dateOfBirthDate.getTime()) || dateOfBirthDate >= today) {
        showWarningToast("Ngày sinh không hợp lệ. Vui lòng kiểm tra lại.", 4000);
        return;
    }

    //const eighteenYearsAgo = new Date(
    //    today.getFullYear() - 18,
    //    today.getMonth(),
    //    today.getDate()
    //);
    //eighteenYearsAgo.setHours(0, 0, 0, 0);

    //if (dateOfBirthDate > eighteenYearsAgo) {
    //    showWarningToast("Bạn phải đủ 18 tuổi để đăng ký.", 4000);
    //    return;
    //}

    if (!googleId && !facebookId && !githubId && !discordId) {
        if (password !== confirmPassword) {
            showWarningToast("Mật khẩu không khớp. Vui lòng kiểm tra lại.", 4000);
            return;
        }

        if (!checkPasswordStrength(password)) {
            showErrorToast("Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.", 4000);
            return;
        }
    }

    fetch(`/api/users/register`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    GoogleId: googleId,
                    FacebookId: facebookId,
                    GithubId: githubId,
                    DiscordId: discordId,
                    Name: username,
                    Email: email,
                    Phone: phone,
                    DateOfBirth: dateOfBirth,
                    Password: password,
                    ConfirmPassword: confirmPassword
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

            if (data.code === "REGISTER_SUCCESS") {
                showSuccessToast(data.message, 4000);
                setTimeout(() => location.href = "/", 1000);
                return;
            } else {
                showErrorToast(data.message, 4000);
            }
        });
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

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
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