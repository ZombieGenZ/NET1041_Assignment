let connection = null;
let id = 0;
window.addEventListener("load", async () => {
  initConnection();
  await connectChatRealtime();
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

function LoadData() {
  const listDom = document.getElementById("messageList");
  listDom.innerHTML = "";
  listDom.innerHTML = '<p class="text-center">Đang tải dử liệu...</p>';
  fetch(`/api/chat${isIdentified ? "" : `/?chatid=${currentChatId}`}`)
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
      if (data.code == "INPUT_DATA_ERROR") {
        listDom.innerHTML = "";
        return showErrorToast(data.message, 4000);
      }

      if (data.code == "OK") {
        if (data.data.length > 0) {
          id = data.roomId;
          let list = "";
          for (var item of data.data) {
            if (!item.isFromUser) {
              list += `
                    <div class="message received">
                        <div class="message-bubble">
                            <div>${item.message}</div>
                            <small class="text-light d-block mt-1" style="font-size: 0.75rem; opacity: 0.8;">${formatDateTime(
                              new Date(item.createdAt)
                            )}</small>
                        </div>
                    </div>
                `;
            } else {
              list += `
                    <div class="message sent">
                        <div class="message-bubble">
                            <div>${item.message}</div>
                            <small class="text-light d-block mt-1" style="font-size: 0.75rem; opacity: 0.8;">${formatDateTime(
                              new Date(item.createdAt)
                            )}</small>
                        </div>
                    </div>
                `;
            }
          }
          listDom.innerHTML = list;
          setTimeout(() => {
            const messageList = document.getElementById("messageList");
            const lastMessage = messageList.lastElementChild;
            if (lastMessage) {
              lastMessage.scrollIntoView({ behavior: "smooth", block: "end" });
            }
          }, 100);
        } else {
          listDom.innerHTML = "";
        }
      } else {
        listDom.innerHTML = "";
      }
    });
}

function sendMessage() {
  const message = document.getElementById("messageInput").value.trim();

  if (!message) {
    return;
  }

  if (!connection) {
    return showErrorToast(
      "Không thể kết nối đến máy chủ. Vui lòng thử lại sau."
    );
  }

  connection.invoke("SendMessage", null, currentChatId, message);
  document.getElementById("messageInput").value = "";
}

function formatDateTime(date) {
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();

  return `${hours}:${minutes} ${day}/${month}/${year}`;
}

async function connectChatRealtime() {
    try {
    await LoadData();

    await connection.start();

    if (!connection) {
      return;
    }

    if (isIdentified) {
      await connection.invoke("ConnectChatTracking", null, null);
    } else {
      await connection.invoke("ConnectChatTracking", currentChatId, null);
    }

    connection.off("NewMessage");

    connection.on("NewMessage", (data) => {
      if (!data.isFromUser) {
        document.getElementById("messageList").innerHTML += `
              <div class="message received">
                  <div class="message-bubble">
                      <div>${data.message}</div>
                      <small class="text-light d-block mt-1" style="font-size: 0.75rem; opacity: 0.8;">${formatDateTime(
                        new Date()
                      )}</small>
                  </div>
              </div>
          `;
      } else {
        document.getElementById("messageList").innerHTML += `
              <div class="message sent">
                  <div class="message-bubble">
                      <div>${data.message}</div>
                      <small class="text-light d-block mt-1" style="font-size: 0.75rem; opacity: 0.8;">${formatDateTime(
                        new Date()
                      )}</small>
                  </div>
              </div>
          `;
      }
      setTimeout(() => {
        const messageList = document.getElementById("messageList");
        const lastMessage = messageList.lastElementChild;
        if (lastMessage) {
          lastMessage.scrollIntoView({ behavior: "smooth", block: "end" });
        }
      }, 100);
    });
  } catch (e) {
    console.log(e);
  }
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
