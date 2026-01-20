document.addEventListener("DOMContentLoaded", () => {
    const weekSelect = document.getElementById("weekSelect");
    const modal = document.getElementById("addModal");

    if (!weekSelect || !modal) {
        console.error("Không tìm thấy weekSelect hoặc addModal");
        return;
    }

    // MỞ FORM KHI CHỌN "THÊM TUẦN"
    weekSelect.addEventListener("change", () => {
        if (weekSelect.value === "__add__") {
            modal.classList.remove("hidden");
            weekSelect.value = "";
        }
    });
});

// ĐÓNG FORM
function closeWeekForm() {
    document.getElementById("addModal").classList.add("hidden");
}

// LƯU TUẦN
function saveWeek() {
    const weekNum = document.getElementById("weekNumber").value;
    const start = document.getElementById("startDate").value;
    const end = document.getElementById("endDate").value;
    const select = document.getElementById("weekSelect");

    if (!weekNum || !start || !end) {
        alert("Nhập đầy đủ thông tin tuần");
        return;
    }

    const option = document.createElement("option");
    option.text = `Tuần ${weekNum} (${start} → ${end})`;
    option.value = `week-${weekNum}`;

    select.insertBefore(option, select.lastElementChild);
    select.selectedIndex = select.options.length - 2;

    closeWeekForm();
}
// ĐỔI MÀU KHI THAY ĐỔI TRẠNG THÁI

document.querySelectorAll(".status-select").forEach(select => {
    updateStatusColor(select);
    select.addEventListener("change", () => updateStatusColor(select));
});

function updateStatusColor(select) {
    select.classList.remove("todo", "doing", "done");
    select.classList.add(select.value);
}
// THÊM CÔNG VIỆC
// ===== THÊM CÔNG VIỆC =====

// MỞ MODAL
document.getElementById("btnOpenModal").addEventListener("click", () => {
    document.getElementById("taskModal").classList.remove("hidden");
});

// ĐÓNG MODAL
function closeTaskModal() {
    document.getElementById("taskModal").classList.add("hidden");
}

// LƯU CÔNG VIỆC → THÊM VÀO BẢNG
function saveTask() {
    const name = document.getElementById("taskName").value.trim();
    const owner = document.getElementById("taskOwner").value.trim();
    const start = document.getElementById("taskStart").value;
    const end = document.getElementById("taskEnd").value;
    const week = document.getElementById("taskWeek").value;


    if (!name || !owner || !start || !end) {
        alert("Vui lòng nhập đầy đủ thông tin");
        return;
    }

    const tbody = document.querySelector(".table-responsive tbody");
    const newId = tbody.children.length + 1;

    const tr = document.createElement("tr");
    tr.dataset.week = week;
    tr.innerHTML = `
    <td>${newId}</td>
    <td>${name}</td>
    <td>${owner}</td>
    <td>${formatDate(start)}</td>
    <td>${formatDate(end)}</td>
    <td>
      <select class="status-select ${status}">
        <option value="todo" ${status === "todo" ? "selected" : ""}>Chưa thực hiện</option>
        <option value="doing" ${status === "doing" ? "selected" : ""}>Đang thực hiện</option>
        <option value="done" ${status === "done" ? "selected" : ""}>Hoàn thành</option>
      </select>
    </td>
    <td></td>
  `;

    tbody.appendChild(tr);

    // gán lại màu trạng thái
    const select = tr.querySelector(".status-select");
    updateStatusColor(select);
    select.addEventListener("change", () => updateStatusColor(select));

    closeTaskModal();
}

// FORMAT DATE
function formatDate(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleDateString("vi-VN");
}
let tasks = JSON.parse(localStorage.getItem("tasks")) || [];
let taskAutoId = Number(localStorage.getItem("taskAutoId")) || 1;
document.getElementById("weekSelect").addEventListener("change", function () {
    const selectedWeek = this.value;
    const rows = document.querySelectorAll(".table-responsive tbody tr");

    rows.forEach(row => {
        if (!selectedWeek || row.dataset.week === selectedWeek) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
});
const params = new URLSearchParams(window.location.search);
const topicId = params.get('topicId');

// demo
document.getElementById('topic-code').innerText = topicId;
