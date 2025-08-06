document.addEventListener("DOMContentLoaded", () => {
  LoadData();

  const autoGeneratorCodeCheckbox =
    document.getElementById("autoGeneratorCode");
  const divCode = document.getElementById("div_code");
  if (autoGeneratorCodeCheckbox && divCode) {
    autoGeneratorCodeCheckbox.addEventListener("change", function () {
      if (this.checked) {
        divCode.style.display = "none";
      } else {
        divCode.style.display = "block";
      }
    });
    if (autoGeneratorCodeCheckbox.checked) {
      divCode.style.display = "none";
    } else {
      divCode.style.display = "block";
    }
  }

  const typeSelect = document.getElementById("type");
  const divUser = document.getElementById("div_user");
  if (typeSelect && divUser) {
    typeSelect.addEventListener("change", function () {
      if (this.value === "0") {
        divUser.style.display = "none";
      } else if (this.value === "1") {
        divUser.style.display = "block";
      }
    });
    if (typeSelect.value === "0") {
      divUser.style.display = "none";
    } else if (typeSelect.value === "1") {
      divUser.style.display = "block";
    }
  }

  const discountTypeSelect = document.getElementById("discountType");
  const discount = document.getElementById("discount");
  const max = document.getElementById("div_max");
  if (discountTypeSelect && discount) {
    discountTypeSelect.addEventListener("change", function () {
      if (this.value === "0") {
        max.style.display = "none";
        discount.placeholder = "ví dụ: 100,000";
      } else if (this.value === "1") {
        max.style.display = "block";
        discount.placeholder = "ví dụ: 100%";
      }
    });
    if (discountTypeSelect.value === "0") {
      max.style.display = "none";
      discount.placeholder = "ví dụ: 100,000";
    } else if (discountTypeSelect.value === "1") {
      max.style.display = "block";
      discount.placeholder = "ví dụ: 100%";
    }
  }

  const isLifeTimeCheckbox = document.getElementById("isLifeTime");
  const divEndTime = document.getElementById("div_endTime");
  if (isLifeTimeCheckbox && divEndTime) {
    isLifeTimeCheckbox.addEventListener("change", function () {
      if (this.checked) {
        divEndTime.style.display = "none";
      } else {
        divEndTime.style.display = "block";
      }
    });
    if (isLifeTimeCheckbox.checked) {
      divEndTime.style.display = "none";
    } else {
      divEndTime.style.display = "block";
    }
  }

  const unlimitedPercentageDiscountCheckbox = document.getElementById(
    "unlimitedPercentageDiscount"
  );
  const maximumPercentageReduction = document.getElementById(
    "div_maximumPercentageReduction"
  );
  if (unlimitedPercentageDiscountCheckbox && maximumPercentageReduction) {
    unlimitedPercentageDiscountCheckbox.addEventListener("change", function () {
      if (this.checked) {
        maximumPercentageReduction.style.display = "none";
      } else {
        maximumPercentageReduction.style.display = "block";
      }
    });
    if (unlimitedPercentageDiscountCheckbox.checked) {
      maximumPercentageReduction.style.display = "none";
    } else {
      maximumPercentageReduction.style.display = "block";
    }
  }

  const listDom = document.getElementById("userList");
  listDom.innerHTML = "";
  fetch("/api/users")
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
          list += `<option value="${item.id}">${item.name} (${item.email})</option>`;
        }
        listDom.innerHTML = list;
      }
    });
});

function LoadData(search) {
  if (!search || search.trim() === "") {
    document.getElementById("search").value = "";
  }
  const listDom = document.getElementById("VoucherData");
  listDom.innerHTML = "";
  listDom.innerHTML =
    '<tr><td class="text-center" colspan="11">Không có dử liệu nào phù hợp</td></tr>';
  fetch(`/api/vouchers${!search ? "" : `/?${search}`}`)
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
          list += `
                        <tr>
                            <td>${item.id}</td>
                            <td>${item.code}</td>
                            <td>${item.name}</td>
                            <td>${
                              item.type === 0 ? "Công khai" : "Riêng tư"
                            }</td>
                            <td>${item.type === 0 ? "" : item.user?.name}</td>
                            <td>${
                                item.discountType === 0
                                ? item.discount.toLocaleString("vi-VN", {
                                    style: "currency",
                                    currency: "VND",
                                  })
                                : `${item.discount}%`
                            }</td>
                            <td>${item.used}</td>
                            <td>${item.quantity}</td>
                            <td>${formatDDMMYYYY(new Date(item.startTime))}</td>
                            <td>${
                              item.endTime == null
                                ? ""
                                : formatDDMMYYYY(new Date(item.endTime))
                            }</td>
                            <td class="text-end button-cell">
                                <button class="btn btn-warning" onclick='startUpdate(${
                                  item.id
                                }, "${item.code}", "${item.name}", "${
            item.description
          }", ${item.type}, ${item.userId}, ${item.discountType}, ${
            item.discount
          }, ${item.quantity}, "${item.startTime}", ${item.isLifeTime}, ${
            !item.endTime ? null : `"${item.endTime}"`
          }, ${item.minimumRequirements}, ${
            item.unlimitedPercentageDiscount
          }, ${
            item.maximumPercentageReduction
          })' data-bs-toggle="modal" data-bs-target="#createAndUpdateModal">Sửa</button>
                                <button class="btn btn-danger" onClick='startDelete(${
                                  item.id
                                })' data-bs-toggle="modal" data-bs-target="#deleteModal">Xóa</button>
                            </td>
                        </tr>
                    `;
        }
        listDom.innerHTML = list;
      } else {
        listDom.innerHTML =
          '<tr><td class="text-center" colspan="11">Không có dử liệu nào phù hợp</td></tr>';
      }
    });
}

function Search() {
  const searchValue = document.getElementById("search").value.trim();

  const customEncodeUriComponent = (value) => {
    return encodeURIComponent(value).replace(/%20/g, "+");
  };

  const params = [];
  if (searchValue) params.push(`text=${customEncodeUriComponent(searchValue)}`);

  const queryString = params.length > 0 ? params.join("&") : "";
  if (queryString) {
    LoadData(queryString);
  } else {
    LoadData();
  }
}

function startCreate() {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Tạo mã giảm giá mới";
  document.getElementById("createAndUpdateButton").textContent =
    "Tạo mã giảm giá";
  document.getElementById("createAndUpdateButton").classList.add("btn-success");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", create);

  document.getElementById("div_autoGeneratorCode").style.display = "block";

  {
    const autoGeneratorCodeCheckbox =
      document.getElementById("autoGeneratorCode");
    const divCode = document.getElementById("div_code");
    if (autoGeneratorCodeCheckbox && divCode) {
      if (autoGeneratorCodeCheckbox.checked) {
        divCode.style.display = "none";
      } else {
        divCode.style.display = "block";
      }
    }

    const typeSelect = document.getElementById("type");
    const divUser = document.getElementById("div_user");
    if (typeSelect && divUser) {
      if (typeSelect.value === "0") {
        divUser.style.display = "none";
      } else if (typeSelect.value === "1") {
        divUser.style.display = "block";
      }
    }

    const discountTypeSelect = document.getElementById("discountType");
    const discount = document.getElementById("discount");
    const max = document.getElementById("div_max");
    if (discountTypeSelect && discount) {
      if (discountTypeSelect.value === "0") {
        max.style.display = "none";
        discount.placeholder = "ví dụ: 100,000";
      } else if (discountTypeSelect.value === "1") {
        max.style.display = "block";
        discount.placeholder = "ví dụ: 100%";
      }
    }

    const isLifeTimeCheckbox = document.getElementById("isLifeTime");
    const divEndTime = document.getElementById("div_endTime");
    if (isLifeTimeCheckbox && divEndTime) {
      if (isLifeTimeCheckbox.checked) {
        divEndTime.style.display = "none";
      } else {
        divEndTime.style.display = "block";
      }
    }

    const unlimitedPercentageDiscountCheckbox = document.getElementById(
      "unlimitedPercentageDiscount"
    );
    const maximumPercentageReduction = document.getElementById(
      "div_maximumPercentageReduction"
    );
    if (unlimitedPercentageDiscountCheckbox && maximumPercentageReduction) {
      if (unlimitedPercentageDiscountCheckbox.checked) {
        maximumPercentageReduction.style.display = "none";
      } else {
        maximumPercentageReduction.style.display = "block";
      }
    }
  }
}

function create() {
  const autoGeneratorCode =
    document.getElementById("autoGeneratorCode").checked;
  const code = document.getElementById("code").value;
  const name = document.getElementById("name").value;
  const description = document.getElementById("description").value;
  const type = document.getElementById("type").value;
  const user = document.getElementById("userList").value;
  const discountType = document.getElementById("discountType").value;
  const discount = document.getElementById("discount").value;
  const quantity = document.getElementById("quantity").value;
  const startTime = document.getElementById("startTime").value;
  const isLifeTime = document.getElementById("isLifeTime").checked;
  const endTime = document.getElementById("endTime").value;
  const minimumRequirements = document.getElementById(
    "minimumRequirements"
  ).value;
  const unlimitedPercentageDiscount = document.getElementById(
    "unlimitedPercentageDiscount"
  ).checked;
  const maximumPercentageReduction = document.getElementById(
    "maximumPercentageReduction"
  ).value;

  if (!name || !description || !startTime) {
    const errMsg = [];
    if (!name) {
      errMsg.push("tên mã giảm giá");
    }
    if (!description) {
      errMsg.push("mô tả mã giảm giá");
    }
    if (!startTime) {
      errMsg.push("thời gian bắt đầu");
    }
    showWarningToast(`Vui lòng nhập ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (!autoGeneratorCode) {
    if (!code) {
      showWarningToast(`Vui lòng nhập mã giảm giá`, 4000);
      return;
    }
  }

  if (type == "1") {
    if (!user || isNaN(user)) {
      showWarningToast(`Vui lòng nhập người sở hửu`, 4000);
      return;
    }
  }

  if (discountType == "1") {
    if (discount < 0 || discount > 100) {
      showWarningToast(`% giảm giá chỉ có thể từ 0 đến 100%`, 4000);
      return;
    }
  } else {
    if (discount <= 0) {
      showWarningToast(`Số tiền giảm giá không được bé hơn hoặc bằng 0`, 4000);
      return;
    }
  }

  if (
    isNaN(Number(quantity)) ||
    (!isNaN(Number(quantity)) && Number(quantity) <= 0)
  ) {
    showWarningToast(
      `Số lượng mã giảm giá không được bé hơn hoặc bằng 0"`,
      4000
    );
    return;
  }

  if (!isLifeTime) {
    if (!endTime) {
      showWarningToast(`Vui lòng nhập thời gian kết thúc`, 4000);
      return;
    }

    if (new Date(startTime) > new Date(endTime)) {
      showWarningToast(
        `Thời gian bắt đầu không được lớn hơn thời gian kết thúc`,
        4000
      );
      return;
    }
  }

  if (
    isNaN(Number(minimumRequirements)) ||
    (!isNaN(Number(minimumRequirements)) && Number(minimumRequirements) < 0)
  ) {
    showWarningToast(`Yêu cầu tối thiểu phải lớn hơn 0`, 4000);
    return;
  }

  if (discountType == "1") {
    if (!unlimitedPercentageDiscount) {
      if (
        isNaN(Number(maximumPercentageReduction)) ||
        (!isNaN(Number(maximumPercentageReduction)) &&
          Number(maximumPercentageReduction) < 1)
      ) {
        showWarningToast(`Số tiền giảm tối đa phải lớn hơn 0`, 4000);
        return;
      }
    }
  }

  fetch("/api/vouchers", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      AutoGeneratorCode: autoGeneratorCode,
      Code: code,
      Name: name,
      Description: description,
      Type: Number(type),
      UserId: Number(user),
      Discount: Number(discount),
      DiscountType: Number(discountType),
      Quantity: Number(quantity),
      StartTime: new Date(startTime),
      IsLifeTime: isLifeTime,
      EndTime: isLifeTime ? null : new Date(endTime),
      MinimumRequirements: Number(minimumRequirements),
      UnlimitedPercentageDiscount: unlimitedPercentageDiscount,
      MaximumPercentageReduction: Number(maximumPercentageReduction),
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi tạo mã giảm giá. Vui lòng thử lại sau.",
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

      if (data.code == "CREATE_VOUCHER_SUCCESS") {
        LoadData();
        clearInput();
        const createAndUpdateModal = document.getElementById(
          "createAndUpdateModal"
        );
        const modal = bootstrap.Modal.getInstance(createAndUpdateModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        showSuccessToast(data.message, 4000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function startUpdate(
  id,
  code,
  name,
  description,
  type,
  user,
  discountType,
  discount,
  quantity,
  startTime,
  isLifeTime,
  endTime,
  minimumRequirements,
  unlimitedPercentageDiscount,
  maximumPercentageReduction
) {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Cập nhật mã giảm giá";
  document.getElementById("createAndUpdateButton").textContent =
    "Cập nhật mã giảm giá";
  document.getElementById("createAndUpdateButton").classList.add("btn-warning");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", update);

  document.getElementById("div_autoGeneratorCode").style.display = "none";

  selectedId = id;
  document.getElementById("autoGeneratorCode").checked = false;
  document.getElementById("code").value = code;
  document.getElementById("name").value = name;
  document.getElementById("description").value = description;
  document.getElementById("type").value = type;
  document.getElementById("userList").value = !user ? "" : user;
  document.getElementById("discountType").value = discountType;
  document.getElementById("discount").value = discount;
  document.getElementById("quantity").value = quantity;
  document.getElementById("startTime").value = formatDateToYYYYMMDD(
    new Date(startTime)
  );
  document.getElementById("isLifeTime").checked = isLifeTime;
  document.getElementById("endTime").value = !endTime
    ? ""
    : formatDateToYYYYMMDD(new Date(endTime));
  document.getElementById("minimumRequirements").value = minimumRequirements;
  document.getElementById("unlimitedPercentageDiscount").checked =
    unlimitedPercentageDiscount;
  document.getElementById("maximumPercentageReduction").value =
    maximumPercentageReduction;

  {
    const autoGeneratorCodeCheckbox =
      document.getElementById("autoGeneratorCode");
    const divCode = document.getElementById("div_code");
    if (autoGeneratorCodeCheckbox && divCode) {
      if (autoGeneratorCodeCheckbox.checked) {
        divCode.style.display = "none";
      } else {
        divCode.style.display = "block";
      }
    }

    const typeSelect = document.getElementById("type");
    const divUser = document.getElementById("div_user");
    if (typeSelect && divUser) {
      if (typeSelect.value === "0") {
        divUser.style.display = "none";
      } else if (typeSelect.value === "1") {
        divUser.style.display = "block";
      }
    }

    const discountTypeSelect = document.getElementById("discountType");
    const discount = document.getElementById("discount");
    const max = document.getElementById("div_max");
    if (discountTypeSelect && discount) {
      if (discountTypeSelect.value === "0") {
        max.style.display = "none";
        discount.placeholder = "ví dụ: 100,000";
      } else if (discountTypeSelect.value === "1") {
        max.style.display = "block";
        discount.placeholder = "ví dụ: 100%";
      }
    }

    const isLifeTimeCheckbox = document.getElementById("isLifeTime");
    const divEndTime = document.getElementById("div_endTime");
    if (isLifeTimeCheckbox && divEndTime) {
      if (isLifeTimeCheckbox.checked) {
        divEndTime.style.display = "none";
      } else {
        divEndTime.style.display = "block";
      }
    }

    const unlimitedPercentageDiscountCheckbox = document.getElementById(
      "unlimitedPercentageDiscount"
    );
    const maximumPercentageReduction = document.getElementById(
      "div_maximumPercentageReduction"
    );
    if (unlimitedPercentageDiscountCheckbox && maximumPercentageReduction) {
      if (unlimitedPercentageDiscountCheckbox.checked) {
        maximumPercentageReduction.style.display = "none";
      } else {
        maximumPercentageReduction.style.display = "block";
      }
    }
  }
}

function update() {
  const autoGeneratorCode =
    document.getElementById("autoGeneratorCode").checked;
  const code = document.getElementById("code").value;
  const name = document.getElementById("name").value;
  const description = document.getElementById("description").value;
  const type = document.getElementById("type").value;
  const user = document.getElementById("userList").value;
  const discountType = document.getElementById("discountType").value;
  const discount = document.getElementById("discount").value;
  const quantity = document.getElementById("quantity").value;
  const startTime = document.getElementById("startTime").value;
  const isLifeTime = document.getElementById("isLifeTime").checked;
  const endTime = document.getElementById("endTime").value;
  const minimumRequirements = document.getElementById(
    "minimumRequirements"
  ).value;
  const unlimitedPercentageDiscount = document.getElementById(
    "unlimitedPercentageDiscount"
  ).checked;
  const maximumPercentageReduction = document.getElementById(
    "maximumPercentageReduction"
  ).value;

  if (!code || !name || !description || !startTime) {
    const errMsg = [];
    if (!code) {
      errMsg.push("mã giảm giá");
    }
    if (!name) {
      errMsg.push("tên mã giảm giá");
    }
    if (!description) {
      errMsg.push("mô tả mã giảm giá");
    }
    if (!startTime) {
      errMsg.push("thời gian bắt đầu");
    }
    showWarningToast(`Vui lòng nhập ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (type == "1") {
    if (!user || isNaN(user)) {
      showWarningToast(`Vui lòng nhập người sở hửu`, 4000);
      return;
    }
  }

  if (discountType == "1") {
    if (discount < 0 || discount > 100) {
      showWarningToast(`% giảm giá chỉ có thể từ 0 đến 100%`, 4000);
      return;
    }
  } else {
    if (discount <= 0) {
      showWarningToast(`Số tiền giảm giá không được bé hơn hoặc bằng 0`, 4000);
      return;
    }
  }

  if (
    isNaN(Number(quantity)) ||
    (!isNaN(Number(quantity)) && Number(quantity) < 0)
  ) {
    showWarningToast(`Số lượng mã giảm giá không được bé hơn 0"`, 4000);
    return;
  }

  if (!isLifeTime) {
    if (!endTime) {
      showWarningToast(`Vui lòng nhập thời gian kết thúc`, 4000);
      return;
    }

    if (new Date(startTime) > new Date(endTime)) {
      showWarningToast(
        `Thời gian bắt đầu không được lớn hơn thời gian kết thúc`,
        4000
      );
      return;
    }
  }

  if (
    isNaN(Number(minimumRequirements)) ||
    (!isNaN(Number(minimumRequirements)) && Number(minimumRequirements) < 0)
  ) {
    showWarningToast(`Yêu cầu tối thiểu phải lớn hơn 0`, 4000);
    return;
  }

  if (discountType == "1") {
    if (!unlimitedPercentageDiscount) {
      if (
        isNaN(Number(maximumPercentageReduction)) ||
        (!isNaN(Number(maximumPercentageReduction)) &&
          Number(maximumPercentageReduction) < 1)
      ) {
        showWarningToast(`Số tiền giảm tối đa phải lớn hơn 0`, 4000);
        return;
      }
    }
  }

  fetch(`/api/vouchers/${selectedId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      AutoGeneratorCode: autoGeneratorCode,
      Code: code,
      Name: name,
      Description: description,
      Type: Number(type),
      UserId: Number(user),
      Discount: Number(discount),
      DiscountType: Number(discountType),
      Quantity: Number(quantity),
      StartTime: new Date(startTime),
      IsLifeTime: isLifeTime,
        EndTime: isLifeTime ? null : new Date(endTime),
      MinimumRequirements: Number(minimumRequirements),
      UnlimitedPercentageDiscount: unlimitedPercentageDiscount,
      MaximumPercentageReduction: Number(maximumPercentageReduction),
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi cập nhật mã giảm giá. Vui lòng thử lại sau.",
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

      if (data.code == "UPDATE_VOUCHER_SUCCESS") {
        LoadData();
        clearInput();
        const createAndUpdateModal = document.getElementById(
          "createAndUpdateModal"
        );
        const modal = bootstrap.Modal.getInstance(createAndUpdateModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        showSuccessToast(data.message, 4000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function startDelete(id) {
  selectedId = id;
}

function del() {
  fetch(`/api/vouchers/${selectedId}`, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
    },
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi xóa mã giảm giá. Vui lòng thử lại sau.",
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

      if (data.code == "DELETE_VOUCHER_SUCCESS") {
        LoadData();
        const deleteModal = document.getElementById("deleteModal");
        const modal = bootstrap.Modal.getInstance(deleteModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        showSuccessToast(data.message, 4000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function clearEvent() {
  document
    .getElementById("createAndUpdateButton")
    .removeEventListener("click", create);
  document
    .getElementById("createAndUpdateButton")
    .removeEventListener("click", update);
}

function clearClass() {
  document
    .getElementById("createAndUpdateButton")
    .classList.remove("btn-success");
  document
    .getElementById("createAndUpdateButton")
    .classList.remove("btn-warning");
}

function clearInput() {
  document.getElementById("autoGeneratorCode").checked = true;
  document.getElementById("code").value = "";
  document.getElementById("name").value = "";
  document.getElementById("description").value = "";
  document.getElementById("type").value = "0";
  document.getElementById("userList").value = "0";
  document.getElementById("discountType").value = "0";
  document.getElementById("discount").value = 0;
  document.getElementById("quantity").value = 0;
  document.getElementById("startTime").value = "";
  document.getElementById("isLifeTime").checked = false;
  document.getElementById("endTime").value = "";
  document.getElementById("minimumRequirements").value = 0;
  document.getElementById("unlimitedPercentageDiscount").checked = false;
  document.getElementById("maximumPercentageReduction").value = 0;
}

function formatDDMMYYYY(date) {
  const day = String(date.getDate()).padStart(2, "0");
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();
  return `${day}/${month}/${year}`;
}

function formatDateToYYYYMMDD(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
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
