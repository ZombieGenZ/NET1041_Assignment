let isPassword = false;

function togglePassword(fieldId) {
  const field = document.getElementById(fieldId);
  const button = field.nextElementSibling;
  const icon = button.querySelector("i");

  if (field.type === "password") {
    field.type = "text";
    icon.classList.remove("bi-eye");
    icon.classList.add("bi-eye-slash");
  } else {
    field.type = "password";
    icon.classList.remove("bi-eye-slash");
    icon.classList.add("bi-eye");
  }
}

document.addEventListener("DOMContentLoaded", () => {
  checkUserPassword();
});

function checkUserPassword() {
  fetch(`/api/users/get-user-password`)
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi lấy dử liệu. Vui lòng thử lại sau.",
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

      if (data.code == "GET_USER_PASSWORD_SUCCESS") {
        if (data.password) {
          isPassword = true;
          document.getElementById("div_oldPassword").style.display = "block";
        }
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function ChangeInfomation() {
  const username = document.getElementById("name").value;
  const email = document.getElementById("email").value;
  const phone = document.getElementById("phone").value;
  const dateOfBirth = document.getElementById("dateOfBirth").value;

  if (!username || !email || !phone || !dateOfBirth) {
    let errMsg = [];
    if (!username) errMsg.push("tên đăng nhập");
    if (!email) errMsg.push("địa chỉ email");
    if (!phone) errMsg.push("số điện thoại");
    if (!dateOfBirth) errMsg.push("ngày sinh");
    showWarningToast(`Vui lòng điền đầy đủ ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (!isValidEmail(email)) {
    showWarningToast(
      "Địa chỉ email không hợp lệ. Vui lòng kiểm tra lại.",
      4000
    );
    return;
  }

  if (!isValidPhoneNumber(phone)) {
    showWarningToast(
      "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.",
      4000
    );
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

  fetch(`/api/users/change-infomation`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Name: username,
      Email: email,
      Phone: phone,
      DateOfBirth: dateOfBirth,
    }),
  })
    .then((response) => {
      if (response.status === 422 || response.status === 401) {
        return response.json();
      }

      if (!response.ok) {
        return showErrorToast(
          "Lỗi khi thay đổi thông tin. Vui lòng thử lại sau.",
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

      if (data.code === "CHANGE_INFOMATION_SUCCESS") {
        showSuccessToast(data.message, 4000);
        setTimeout(() => location.reload(), 1000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function ChangePassword() {
  const oldPassword = !isPassword
    ? document.getElementById("oldPassword").value
    : "";
  const password = document.getElementById("newPassword").value;
  const confirmPassword = document.getElementById("confirmPassword").value;

  if (!password || !confirmPassword) {
    let errMsg = [];
    if (!password) errMsg.push("mật khẩu");
    if (!confirmPassword) errMsg.push("xác nhận mật khẩu");
    showWarningToast(`Vui lòng điền đầy đủ ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (isPassword && !oldPassword) {
    showWarningToast("Vui lòng nhập mật khẩu cũ để thay đổi mật khẩu.", 4000);
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
      OldPassword: isPassword ? oldPassword : null,
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
          "Lỗi khi thay đổi thông tin. Vui lòng thử lại sau.",
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
        setTimeout(() => location.reload(), 1000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function isValidPhoneNumber(
  phoneNumber,
  pattern = /^(0|\+84)(3|5|7|8|9)\d{8}$/
) {
  if (
    phoneNumber === null ||
    typeof phoneNumber === "undefined" ||
    phoneNumber.trim() === ""
  ) {
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
