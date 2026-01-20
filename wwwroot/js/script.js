document.addEventListener('DOMContentLoaded', () => {
    // --- XỬ LÝ MENU SIDEBAR ---
    const menuLinks = document.querySelectorAll('.menu-link');

    menuLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            const submenu = link.nextElementSibling;

            // Kiểm tra nếu đây là menu cha (có submenu bên trong)
            if (submenu && submenu.classList.contains('submenu')) {
                e.preventDefault(); // Ngăn chặn chuyển trang

                // Đóng tất cả các menu khác đang mở
                const openMenus = document.querySelectorAll('.submenu.open');
                openMenus.forEach(openMenu => {
                    // Nếu menu đang mở KHÔNG PHẢI là cái mình  Đóng nó lại
                    if (openMenu !== submenu) {
                        openMenu.classList.remove('open');
                        
                        // Tìm cái nút cha của menu đó để tắt trạng thái active và xoay mũi tên lại
                        const parentLink = openMenu.previousElementSibling;
                        if (parentLink) {
                            parentLink.classList.remove('active-parent');
                            const chevron = parentLink.querySelector('.chevron');
                            if (chevron) chevron.style.transform = 'rotate(0deg)';
                        }
                    }
                });

                //  Mở hoặc Đóng menu hiện tại
                submenu.classList.toggle('open');
                link.classList.toggle('active-parent');
                
                // Xoay mũi tên của menu hiện tại
                const chevron = link.querySelector('.chevron');
                if(chevron) {
                    chevron.style.transform = submenu.classList.contains('open') ? 'rotate(90deg)' : 'rotate(0deg)';
                }
            }
        });
    });

    const notifBtn = document.getElementById('notifDropdownBtn');
    const notifMenu = document.getElementById('notifDropdown');
    
    // Lấy user dropdown cũ nếu cần xử lý đóng mở chéo (nếu có id userDropdown)
    const userDropdownContainer = document.getElementById('userDropdown'); 

    // Sự kiện click vào chuông
    if (notifBtn && notifMenu) {
        notifBtn.addEventListener('click', (e) => {
            e.stopPropagation(); // Ngăn chặn sự kiện nổi bọt
            
            // Đóng các dropdown khác nếu đang mở (ví dụ user dropdown)
            // Lưu ý: User dropdown trong CSS cũ dùng :hover nên có thể không cần đóng bằng JS, 
            // nhưng nếu bạn chuyển sang click thì code này sẽ hữu ích.
            
            // Toggle menu thông báo
            notifMenu.classList.toggle('active');
        });
    }

    // Sự kiện click ra ngoài để đóng menu thông báo
    document.addEventListener('click', (e) => {
        // Nếu click không trúng chuông VÀ không trúng menu nội dung
        if (notifMenu && !notifMenu.contains(e.target) && notifBtn && !notifBtn.contains(e.target)) {
            notifMenu.classList.remove('active');
        }
    });
    
    // Ngăn menu đóng khi người dùng click/thao tác bên trong nội dung thông báo
    if (notifMenu) {
        notifMenu.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }
});

    // --- XỬ LÝ USER DROPDOWN ---
    const userDropdown = document.getElementById('userDropdown');
    
    if (userDropdown) {
        userDropdown.addEventListener('click', (e) => {
            e.stopPropagation();
            userDropdown.classList.toggle('active');
        });

        document.addEventListener('click', () => {
            userDropdown.classList.remove('active');
        });
    }

    // --- MODAL ADD ---
    const modal = document.getElementById('addModal');
    const btnOpen = document.getElementById('btnOpenModal');
    const btnCloseList = document.querySelectorAll('.close-modal');
    const btnSave = document.getElementById('btnSaveModal');

    if (btnOpen) btnOpen.addEventListener('click', (e) => {
        e.preventDefault();
        modal.style.display = 'flex';
    });

    if (modal) {
        btnCloseList.forEach(btn => btn.addEventListener('click', (e) => {
            e.preventDefault();
            modal.style.display = 'none';
        }));
        
        window.addEventListener('click', (e) => {
            if(e.target === modal) modal.style.display = 'none';
        });
    }

    if (btnSave) {
        btnSave.addEventListener('click', (e) => {
            e.preventDefault();
            const name = document.getElementById('mName').value;
            const cohort = document.getElementById('mCohort').value;
            const start = document.getElementById('mStart').value;

            if (!name || !cohort || !start) { alert('Vui lòng nhập đủ thông tin!'); return; }

            const newProject = { name: name, cohort: cohort, startDate: start };
            localStorage.setItem('projectData', JSON.stringify(newProject));
            modal.style.display = 'none';
            alert('Đã tạo đợt! Đang chuyển trang...');
            window.location.href = 'edit.html';
        });
    }

    // --- LOGIC TRANG EDIT ---
    if (window.location.pathname.includes('edit.html')) {
        fillEditData();

        const mainStartInput = document.getElementById('eMainStart');
        if(mainStartInput){
            mainStartInput.addEventListener('change', () => {
                calculateDates(mainStartInput.value);
            });
        }
    }
// });

// --- CÁC HÀM HỖ TRỢ ---

function togglePhase(header) {
    header.classList.toggle('active');
    const content = header.nextElementSibling;
    content.classList.toggle('show');
    
    const icon = header.querySelector('i');
    if (content.classList.contains('show')) {
        icon.classList.remove('fa-chevron-down');
        icon.classList.add('fa-chevron-up');
    } else {
        icon.classList.remove('fa-chevron-up');
        icon.classList.add('fa-chevron-down');
    }
}

function fillEditData() {
    const rawData = localStorage.getItem('projectData');
    if (!rawData) return;
    const data = JSON.parse(rawData);
    
    const eName = document.getElementById('eName');
    if(eName) eName.value = data.name;
    const eCohort = document.getElementById('eCohort');
    if(eCohort) eCohort.value = data.cohort;
    const eMainStart = document.getElementById('eMainStart');
    if(eMainStart) {
        eMainStart.value = data.startDate;
        calculateDates(data.startDate);
    }
}

function calculateDates(startDateStr) {
    if(!startDateStr) return;
    const mainStart = new Date(startDateStr);
    
    const addDays = (date, days) => {
        const result = new Date(date);
        result.setDate(result.getDate() + days);
        return result.toISOString().split('T')[0];
    };

    const setVal = (id, val) => {
        const el = document.getElementById(id);
        if(el) el.value = val;
    };

    // 1. Giai đoạn Chuẩn bị
    const prepStartObj = new Date(mainStart);
    prepStartObj.setDate(prepStartObj.getDate() + 4);
    const prepStart = prepStartObj.toISOString().split('T')[0];
    const prepEnd = addDays(prepStartObj, 21);

    setVal('prepStart', prepStart); setVal('prepEnd', prepEnd);
    setVal('dkStart', prepStart); setVal('dkEnd', addDays(prepStartObj, 5));
    setVal('duyetNVStart', addDays(prepStartObj, 6)); setVal('duyetNVEnd', addDays(prepStartObj, 10));
    setVal('dxStart', addDays(prepStartObj, 11)); setVal('dxEnd', addDays(prepStartObj, 15));
    setVal('duyetDTStart', addDays(prepStartObj, 16)); setVal('duyetDTEnd', prepEnd);

    // 2. Giai đoạn Chính thức
    const offStartObj = new Date(prepEnd);
    offStartObj.setDate(offStartObj.getDate() + 1);
    const offStart = offStartObj.toISOString().split('T')[0];
    const offEnd = addDays(offStartObj, (13 * 7));

    setVal('offStart', offStart); setVal('offEnd', offEnd);
    setVal('deCuongStart', offStart); setVal('deCuongEnd', addDays(offStartObj, 7));

    // 3. Báo cáo
    const finalDeadline = addDays(mainStart, (15 * 7));
    setVal('reportStart', offEnd);
    setVal('reportEnd', finalDeadline);
}


