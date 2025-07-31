document.addEventListener("DOMContentLoaded", function () {
  const togglePassword = document.getElementById("togglePassword");
  const passwordInput = document.getElementById("rp_password");

  togglePassword.addEventListener("click", function () {
    const type =
      passwordInput.getAttribute("type") === "password" ? "text" : "password";
    passwordInput.setAttribute("type", type);
    this.querySelector("i").classList.toggle("bi-eye");
    this.querySelector("i").classList.toggle("bi-eye-slash");
  });

  const toggleConfirmPassword = document.getElementById(
    "toggleConfirmPassword"
  );
  const confirmPasswordInput = document.getElementById("rp_confirmPassword");

  toggleConfirmPassword.addEventListener("click", function () {
    const type =
      confirmPasswordInput.getAttribute("type") === "password"
        ? "text"
        : "password";
    confirmPasswordInput.setAttribute("type", type);
    this.querySelector("i").classList.toggle("bi-eye");
    this.querySelector("i").classList.toggle("bi-eye-slash");
  });
});

function ResetPassword() {
  const password = document.getElementById("rp_password").value;
  const confirmPassword = document.getElementById("rp_confirmPassword").value;

  if (!password || !confirmPassword) {
    let errMsg = [];
    if (!password) errMsg.push("mật khẩu mới");
    if (!confirmPassword) errMsg.push("xác nhận mật khẩu mới");
    showWarningToast(`Vui lòng điền đầy đủ ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (password !== confirmPassword) {
    showWarningToast("Mật khẩu không khớp. Vui lòng kiểm tra lại.", 4000);
    return;
  }

  if (!checkPasswordStrength(password)) {
    showErrorToast(
      "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.",
      4000
    );
    return;
  }

  fetch(`/api/users/change-password`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Token: new URLSearchParams(window.location.search).get("token"),
      NewPassword: password,
      ConfirmPassword: confirmPassword,
    }),
  })
    .then((response) => {
      if (response.status === 422 || response.status === 401) {
        return response.json();
      }

      if (!response.ok) {
        return showErrorToast(
          "Lỗi khi thay đổi mật khẩu. Vui lòng thử lại sau.",
          4000
        );
      }

      return response.json();
    })
    .then((data) => {
      if (data.code === "INPUT_DATA_ERROR") {
        showWarningToast(data.message, 4000);
        return;
      }

      if (data.code === "CHANGE_PASSWORD_SUCCESS") {
        showSuccessToast(data.message, 4000);
        setTimeout(() => (location.href = "/"), 1000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
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

    if (char >= "A" && char <= "Z") {
      hasUppercase = true;
    } else if (char >= "a" && char <= "z") {
      hasLowercase = true;
    } else if (char >= "0" && char <= "9") {
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
    close: true,
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
    close: true,
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
    close: true,
  }).showToast();
}
