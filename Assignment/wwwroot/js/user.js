document.addEventListener("DOMContentLoaded", () => {
  LoadData();

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
});

function LoadData(search) {
  if (!search || search.trim() === "") {
    document.getElementById("search").value = "";
  }
  const listDom = document.getElementById("UserData");
  listDom.innerHTML = "";
  listDom.innerHTML =
    '<tr><td class="text-center" colspan="9">Không có dử liệu nào phù hợp</td></tr>';
  fetch(`/api/users${!search ? "" : `/?${search}`}`)
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
                            <td>${item.name}</td>
                            <td>${item.email}</td>
                            <td>${item.phone}</td>
                            <td>${item.totalAccumulatedPoints.toLocaleString(
                              "vi-VN"
                            )}</td>
                            <td>${convertNumberToVietnameseWord(item.rank)}</td>
                            <td>${getRankHtml(item.top)}</td>
                            <td>${convertNumberToVietnameseRole(item.role)}</td>
                            <td class="text-end button-cell">
                              ${
                                item.penaltyIsBanned
                                  ? `<button class="btn btn-warning" onClick='startSelected(${item.id})' data-bs-toggle="modal" data-bs-target="#unBanModal">Mở khóa tài khoản</button>`
                                  : `<button class="btn btn-danger" onclick='startSelected(${item.id})' data-bs-toggle="modal" data-bs-target="#banModal">Khóa tài khoản</button>`
                              }
                            </td>
                        </tr>
                    `;
        }
        listDom.innerHTML = list;
      } else {
        listDom.innerHTML =
          '<tr><td class="text-center" colspan="9">Không có dử liệu nào phù hợp</td></tr>';
      }
    });
}

let selectedId = 0;

function startSelected(id) {
  selectedId = id;
  clearInput();
  const isLifeTimeCheckbox = document.getElementById("isLifeTime");
  const divEndTime = document.getElementById("div_endTime");
  if (isLifeTimeCheckbox && divEndTime) {
    if (isLifeTimeCheckbox.checked) {
      divEndTime.style.display = "none";
    } else {
      divEndTime.style.display = "block";
    }
  }
}

function ban() {
  const reason = document.getElementById("reason").value;
  const isLifeTime = document.getElementById("isLifeTime").checked;
  const endTimeValue = document.getElementById("endTimeValue").value;
  const endTimeType = document.getElementById("endTimeType").value;

  const endTime = endTimeValue + endTimeType;

  if (!reason) {
    showWarningToast(`Không được để trống lý do trừng phạt`, 4000);
    return;
  }

  if (!isLifeTime) {
    if (!isValidDurationString(endTime)) {
      showWarningToast(`Thời hạn không hợp lệ`, 4000);
      return;
    }
  }

  fetch("/api/users/ban", {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      UserId: selectedId,
      Reason: reason,
      IsLifeTime: isLifeTime,
      EndTime: isLifeTime ? null : endTime,
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi trừng phạt. Vui lòng thử lại sau.",
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

      if (data.code == "BANNED_SUCCESS") {
        LoadData();
        clearInput();
        const createAndUpdateModal = document.getElementById("banModal");
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

function unBan() {
  fetch(`/api/users/unban/${selectedId}`, {
    method: "PUT",
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi bỏ trừng phạt. Vui lòng thử lại sau.",
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

      if (data.code == "UNBANNED_SUCCESS") {
        LoadData();
        clearInput();
        const createAndUpdateModal = document.getElementById("unBanModal");
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

function clearInput() {
  document.getElementById("reason").value = "";
  document.getElementById("isLifeTime").checked = false;
  document.getElementById("endTimeValue").value = 1;
  document.getElementById("endTimeType").value = "mo";
}

const durationRegex = /^(\d+)([smhdw]|mo|y)$/i;

function isValidDurationString(input) {
  if (input === null || input === undefined || input.trim() === "") {
    return false;
  }
  return durationRegex.test(input);
}

function convertNumberToVietnameseWord(number) {
  switch (number) {
    case 1:
      return "Đồng";
    case 2:
      return "Bạc";
    case 3:
      return "Vàng";
    case 4:
      return "Kim cương";
    default:
      return "Không cần";
  }
}

function convertNumberToVietnameseRole(role) {
  switch (role) {
    case 1:
      return "Người giao hàng";
    case 2:
      return "Quản trị viên";
    default:
      return "Khách hàng";
  }
}

function getRankHtml(rank) {
  let color;
  let icon;

  if (rank === 1) {
    color = "gold";
    icon = "🥇";
  } else if (rank === 2) {
    color = "silver";
    icon = "🥈";
  } else if (rank === 3) {
    color = "bronze";
    icon = "🥉";
  } else {
    color = "gray";
    icon = "🎖️";
  }

  return `<span style="color: ${color};">${icon} ${rank}</span>`;
}
