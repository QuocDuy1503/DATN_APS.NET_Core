-- =====================================================================
-- SCRIPT TẠO BẢNG CHO HỆ THỐNG QUẢN LÝ ĐỒ ÁN TỐT NGHIỆP (.NET 8 / Razor Pages)
-- Ghi chú:
-- 1) Các bảng chính được sắp xếp theo thứ tự phụ thuộc khóa ngoại.
-- 2) Kiểu ngày theo mô hình: DateOnly -> DATE, TimeOnly -> TIME, DateTime -> DATETIME.
-- 3) Các cột Id mặc định dùng IDENTITY trừ những cột được đánh dấu ValueGeneratedNever (GiangVien, SinhVien).
-- 4) Một số cột int trong DotDoAn (ngày_lập hội đồng, ngày nộp, …) giữ nguyên INT như trong mô hình.
-- =====================================================================

-- 1. VAI TRÒ
CREATE TABLE VaiTro (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_vai_tro VARCHAR(50) NOT NULL UNIQUE,
    ten_vai_tro NVARCHAR(100)
);

-- 2. NGƯỜI DÙNG
CREATE TABLE NguoiDung (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ho_ten NVARCHAR(100),
    email VARCHAR(100) NOT NULL UNIQUE,
    mat_khau VARCHAR(255),
    sdt VARCHAR(20),
    microsoft_id VARCHAR(100),
    avatar_url NVARCHAR(500),
    trang_thai BIT NOT NULL DEFAULT(1)
);

-- 3. NGƯỜI DÙNG - VAI TRÒ (many-to-many)
CREATE TABLE NguoiDung_VaiTro (
    id_nguoi_dung INT NOT NULL,
    id_vai_tro INT NOT NULL,
    PRIMARY KEY (id_nguoi_dung, id_vai_tro),
    FOREIGN KEY (id_nguoi_dung) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_vai_tro) REFERENCES VaiTro(id)
);

-- 4. KHOA HỌC
CREATE TABLE KhoaHoc (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_khoa VARCHAR(20),
    ten_khoa NVARCHAR(50),
    nam_nhap_hoc INT,
    nam_tot_nghiep INT,
    trang_thai BIT NOT NULL DEFAULT(1)
);

-- 5. HỌC KỲ
CREATE TABLE HocKi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_hoc_ki VARCHAR(20),
    nam_bat_dau INT,
    nam_ket_thuc INT,
    tuan_bat_dau INT,
    ngay_bat_dau DATE,
    trang_thai BIT NOT NULL DEFAULT(1)
);

-- 6. BỘ MÔN
CREATE TABLE BoMon (
    id INT IDENTITY(1,1) PRIMARY KEY,
    stt INT,
    ten_bo_mon NVARCHAR(100),
    ten_viet_tat NVARCHAR(20),
    id_nguoi_tao INT,
    ngay_tao DATE,
    id_nguoi_sua INT,
    ngay_sua DATE,
    FOREIGN KEY (id_nguoi_tao) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_nguoi_sua) REFERENCES NguoiDung(id)
);

-- 7. NGÀNH
CREATE TABLE Nganh (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_nganh VARCHAR(20),
    ten_nganh NVARCHAR(100),
    ten_viet_tat NVARCHAR(20),
    id_nguoi_tao INT,
    ngay_tao DATE,
    id_nguoi_sua INT,
    ngay_sua DATE,
    id_bo_mon INT,
    FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id),
    FOREIGN KEY (id_nguoi_tao) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_nguoi_sua) REFERENCES NguoiDung(id)
);

-- 8. CHUYÊN NGÀNH
CREATE TABLE ChuyenNganh (
    id INT IDENTITY(1,1) PRIMARY KEY,
    stt INT,
    ten_chuyen_nganh NVARCHAR(100),
    ten_viet_tat NVARCHAR(20),
    id_nguoi_tao INT,
    ngay_tao DATE,
    id_nguoi_sua INT,
    ngay_sua DATE,
    id_nganh INT,
    id_bo_mon INT,
    FOREIGN KEY (id_nganh) REFERENCES Nganh(id),
    FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id),
    FOREIGN KEY (id_nguoi_tao) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_nguoi_sua) REFERENCES NguoiDung(id)
);

-- 9. GIẢNG VIÊN (IdNguoiDung không tự tăng)
CREATE TABLE GiangVien (
    id_nguoi_dung INT PRIMARY KEY,
    ma_gv VARCHAR(20) NOT NULL UNIQUE,
    hoc_vi NVARCHAR(50),
    id_bo_mon INT,
    FOREIGN KEY (id_nguoi_dung) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id)
);

-- 10. SINH VIÊN (IdNguoiDung không tự tăng)
CREATE TABLE SinhVien (
    id_nguoi_dung INT PRIMARY KEY,
    mssv VARCHAR(20) NOT NULL UNIQUE,
    id_chuyen_nganh INT,
    id_khoa_hoc INT,
    tin_chi_tich_luy FLOAT NOT NULL DEFAULT(0),
    FOREIGN KEY (id_nguoi_dung) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_chuyen_nganh) REFERENCES ChuyenNganh(id),
    FOREIGN KEY (id_khoa_hoc) REFERENCES KhoaHoc(id)
);

-- 11. CHƯƠNG TRÌNH ĐÀO TẠO
CREATE TABLE ChuongTrinhDaoTao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_ctdt VARCHAR(50) NOT NULL UNIQUE,
    ten_ctdt NVARCHAR(200),
    stt_hien_thi INT,
    id_nganh INT,
    id_khoa_hoc INT,
    tong_tin_chi INT,
    trang_thai BIT NOT NULL DEFAULT(1),
    ngay_tao DATETIME NOT NULL DEFAULT(getdate()),
    FOREIGN KEY (id_nganh) REFERENCES Nganh(id),
    FOREIGN KEY (id_khoa_hoc) REFERENCES KhoaHoc(id)
);

-- 12. CHI TIẾT CTDT
CREATE TABLE ChiTiet_CTDT (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_ctdt INT,
    stt INT,
    ma_hoc_phan VARCHAR(50),
    ten_hoc_phan NVARCHAR(150),
    so_tin_chi INT,
    loai_hoc_phan NVARCHAR(50),
    dieu_kien_tien_quyet NVARCHAR(255),
    hoc_ki_to_chuc INT,
    FOREIGN KEY (id_ctdt) REFERENCES ChuongTrinhDaoTao(id)
);

-- 13. HỌC TẬP
CREATE TABLE KetQuaHocTap (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_sinh_vien INT,
    stt INT,
    ma_hoc_phan VARCHAR(20),
    ten_hoc_phan NVARCHAR(100),
    so_tc FLOAT,
    diem_so FLOAT,
    diem_chu VARCHAR(2),
    tong_so_tin_chi FLOAT,
    GPA FLOAT,
    ket_qua BIT,
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung)
);

-- 14. DOT ĐỒ ÁN
CREATE TABLE DotDoAn (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten_dot NVARCHAR(200),
    id_khoa_hoc INT,
    id_hoc_ki INT,
    ngay_bat_dau_dot DATE,
    ngay_ket_thuc_dot DATE,
    ngay_bat_dau_dk_nguyen_vong DATE,
    ngay_ket_thuc_dk_nguyen_vong DATE,
    ngay_bat_dau_dk_duyet_nguyen_vong DATE,
    ngay_ket_thuc_dk_duyet_nguyen_vong DATE,
    ngay_bat_dau_de_xuat_de_tai DATE,
    ngay_ket_thuc_de_xuat_de_tai DATE,
    ngay_bat_dau_duyet_de_xuat_de_tai DATE,
    ngay_ket_thuc_duyet_de_xuat_de_tai DATE,
    -- [Deprecated - cấu hình qua QuanLyThongBao] ngay_lap_hoi_dong_duyet_DXDT INT,
    -- [Deprecated - cấu hình qua QuanLyThongBao] ngay_duyet_DXDT INT,
    ngay_bat_dau_nop_de_cuong DATE,
    ngay_ket_thuc_nop_de_cuong DATE,
    ngay_bat_dau_bao_cao_cuoi_ki DATE,
    ngay_ket_thuc_bao_cao_cuoi_ki DATE,
    -- [Deprecated - cấu hình qua QuanLyThongBao] ngay_lap_HD_BCCK INT,
    -- [Deprecated - cấu hình qua QuanLyThongBao] ngay_nop_tai_lieu_BCCK INT,
    -- [Deprecated - cấu hình qua QuanLyThongBao] ngay_cong_bo_kq_BCCK INT,
    ngay_bat_dau_bao_cao_giua_ki DATE,
    ngay_ket_thuc_bao_cao_giua_ki DATE,
    -- [Deprecated - cấu hình qua QuanLyThongBao] ngay_lap_HD_BCGK INT,
    -- [Deprecated - cấu hình qua QuanLyThongBao] ngay_nop_tai_lieu_BCGK INT,
    trang_thai BIT,
    FOREIGN KEY (id_khoa_hoc) REFERENCES KhoaHoc(id),
    FOREIGN KEY (id_hoc_ki) REFERENCES HocKi(id)
);

-- 15. ĐĂNG KÝ NGUYỆN VỌNG
CREATE TABLE DangKyNguyenVong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_sinh_vien INT,
    so_tin_chi_tich_luy_hien_tai INT,
    trang_thai INT NOT NULL DEFAULT(0),
    ngay_dang_ky DATETIME NOT NULL DEFAULT(getdate()),
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung)
);

-- 16. ĐỀ TÀI
CREATE TABLE DeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_de_tai VARCHAR(20),
    ten_de_tai NVARCHAR(255),
    muc_tieu_chinh NVARCHAR(MAX),
    yeu_cau_tinh_moi NVARCHAR(MAX),
    pham_vi_chuc_nang NVARCHAR(MAX),
    cong_nghe_su_dung NVARCHAR(MAX),
    san_pham_ket_qua_du_kien NVARCHAR(MAX),
    id_nguoi_de_xuat INT,
    id_gvhd INT,
    id_dot INT,
    id_chuyen_nganh INT,
    trang_thai VARCHAR(20) NOT NULL DEFAULT('CHO_DUYET'),
    nhan_xet_duyet NVARCHAR(MAX),
    nguoi_duyet INT,
    FOREIGN KEY (id_nguoi_de_xuat) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_gvhd) REFERENCES GiangVien(id_nguoi_dung),
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_chuyen_nganh) REFERENCES ChuyenNganh(id),
    FOREIGN KEY (nguoi_duyet) REFERENCES NguoiDung(id)
);

-- 17. ĐỀ CƯƠNG (1-1 với Đề tài)
CREATE TABLE DeCuong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_de_tai INT NOT NULL UNIQUE,
    ly_do_chon_de_tai NVARCHAR(MAX),
    gia_thuyet_nghien_cuu NVARCHAR(MAX),
    doi_tuong_nghien_cuu NVARCHAR(MAX),
    pham_vi_nghien_cuu NVARCHAR(MAX),
    phuong_phap_nghien_cuu NVARCHAR(MAX),
    ngay_nop DATETIME NOT NULL DEFAULT(getdate()),
    trang_thai VARCHAR(20),
    FOREIGN KEY (id_de_tai) REFERENCES DeTai(id)
);

-- 18. SINH VIÊN - ĐỀ TÀI
CREATE TABLE SinhVien_DeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_de_tai INT,
    id_sinh_vien INT,
    trang_thai VARCHAR(20),
    ngay_dang_ky DATETIME NOT NULL DEFAULT(getdate()),
    nhan_xet NVARCHAR(MAX),
    FOREIGN KEY (id_de_tai) REFERENCES DeTai(id),
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung)
);

-- 19. KẾ HOẠCH CÔNG VIỆC
CREATE TABLE KeHoachCongViec (
    id INT IDENTITY(1,1) PRIMARY KEY,
    stt INT,
    id_sinh_vien INT,
    ten_cong_viec NVARCHAR(200),
    mo_ta_cong_viec NVARCHAR(MAX),
    ngay_bat_dau DATETIME,
    ngay_ket_thuc DATETIME,
    ngay_bat_dau_thuc_te DATETIME,
    ngay_ket_thuc_thuc_te DATETIME,
    trang_thai VARCHAR(20),
    id_file_minh_chung INT,
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung)
);

-- 20. NHẬT KÝ HƯỚNG DẪN
CREATE TABLE NhatKyHuongDan (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    ngay_hop DATE,
    hinh_thuc_hop BIT,
    thoi_gian_hop TIME,
    dia_diem_hop NVARCHAR(20),
    id_ke_hoach_cong_viec INT,
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_ke_hoach_cong_viec) REFERENCES KeHoachCongViec(id)
);

-- 21. HỘI ĐỒNG BÁO CÁO
CREATE TABLE HoiDongBaoCao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_hoi_dong VARCHAR(20),
    ten_hoi_dong NVARCHAR(100),
    loai_hoi_dong VARCHAR(20),
    id_dot INT,
    id_nguoi_tao INT,
    id_bo_mon INT,
    ngay_bao_cao DATE,
    ngay_bat_dau DATE,
    ngay_ket_thuc DATE,
    dia_diem NVARCHAR(100),
    thoi_gian_du_kien TIME,
    trang_thai BIT,
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_nguoi_tao) REFERENCES NguoiDung(id),
    FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id)
);

-- 22. THÀNH VIÊN HỘI ĐỒNG
CREATE TABLE ThanhVien_HD_BaoCao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_hd_baocao INT,
    id_giang_vien INT,
    vai_tro VARCHAR(50),
    FOREIGN KEY (id_hd_baocao) REFERENCES HoiDongBaoCao(id),
    FOREIGN KEY (id_giang_vien) REFERENCES GiangVien(id_nguoi_dung)
);

-- 23. PHIÊN BẢO VỆ
CREATE TABLE PhienBaoVe (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_hd_baocao INT,
    id_sinh_vien_de_tai INT,
    stt_bao_cao INT,
    link_tai_lieu VARCHAR(255),
    trang_thai_cham VARCHAR(20) NOT NULL DEFAULT('PENDING'), -- PENDING/DANG_CHAM/DA_CHAM
    FOREIGN KEY (id_hd_baocao) REFERENCES HoiDongBaoCao(id),
    FOREIGN KEY (id_sinh_vien_de_tai) REFERENCES SinhVien_DeTai(id)
);

-- 24. KẾT QUẢ BẢO VỆ SINH VIÊN
CREATE TABLE KetQuaBaoVe_SinhVien (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_phien_bao_ve INT,
    id_sinh_vien INT,
    id_de_tai INT,
    id_sinh_vien_de_tai INT,
    diem_tong_ket FLOAT,
    diem_chu VARCHAR(5),
    ket_qua VARCHAR(20),
    nhan_xet NVARCHAR(MAX),
    FOREIGN KEY (id_phien_bao_ve) REFERENCES PhienBaoVe(id),
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung),
    FOREIGN KEY (id_de_tai) REFERENCES DeTai(id),
    FOREIGN KEY (id_sinh_vien_de_tai) REFERENCES SinhVien_DeTai(id)
);

-- 25. LOẠI PHIẾU CHẤM
CREATE TABLE LoaiPhieuCham (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten_loai_phieu NVARCHAR(100),
    nguoi_tao INT,
    FOREIGN KEY (nguoi_tao) REFERENCES NguoiDung(id)
);

-- 26. TIÊU CHÍ CHẤM ĐIỂM
CREATE TABLE TieuChiChamDiem (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_loai_phieu INT,
    ten_tieu_chi NVARCHAR(200),
    mo_ta_huong_dan NVARCHAR(MAX),
    trong_so FLOAT,
    diem_toi_da FLOAT,
    stt_hien_thi INT,
    FOREIGN KEY (id_loai_phieu) REFERENCES LoaiPhieuCham(id)
);

-- 27. CẤU HÌNH PHIẾU CHẤM THEO ĐỢT
CREATE TABLE CauHinhPhieuCham_Dot (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    vai_tro_cham VARCHAR(50),
    id_loai_phieu INT,
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_loai_phieu) REFERENCES LoaiPhieuCham(id)
);

-- 28. MẪU THÔNG BÁO & CẤU HÌNH THÔNG BÁO
CREATE TABLE MauThongBao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_mau VARCHAR(50) NOT NULL UNIQUE,
    tieu_de NVARCHAR(200),
    noi_dung_thong_bao NVARCHAR(MAX)
);

CREATE TABLE CauHinhThongBao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_mau INT,
    loai_su_kien VARCHAR(50),
    doi_tuong_nhan VARCHAR(50),
    moc_thoi_gian VARCHAR(20),
    so_ngay_chenh_lech INT,
    tieu_de_mau NVARCHAR(200),
    noi_dung_mau NVARCHAR(MAX),
    trang_thai BIT NOT NULL DEFAULT(1),
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_mau) REFERENCES MauThongBao(id),
    UNIQUE (id_dot)
);

-- 29. THÔNG BÁO
CREATE TABLE ThongBao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_nguoi_nhan INT,
    tieu_de NVARCHAR(200),
    noi_dung NVARCHAR(MAX),
    link_lien_ket VARCHAR(255),
    ngay_tao DATETIME NOT NULL DEFAULT(getdate()),
    trang_thai_xem BIT NOT NULL DEFAULT(0),
    FOREIGN KEY (id_nguoi_nhan) REFERENCES NguoiDung(id)
);

-- 30. BÁO CÁO NỘP (dùng cho minh chứng & hồ sơ)
CREATE TABLE BaoCaoNop (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_de_tai INT,
    id_sinh_vien INT,
    stt INT,
    ten_bao_cao NVARCHAR(50),
    file_baocao VARCHAR(255),
    ngay_nop DATETIME,
    nhan_xet NVARCHAR(MAX),
    trang_thai VARCHAR(20),
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_de_tai) REFERENCES DeTai(id),
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung)
);

-- 31. KẾT QUẢ/THỐNG KÊ BÁO CÁO
CREATE TABLE BaoCaoThongKe (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    ten_bao_cao NVARCHAR(200),
    du_lieu_json NVARCHAR(MAX),
    loai_bao_cao VARCHAR(50),
    thoi_diem DATETIME NOT NULL DEFAULT(getdate()),
    ngay_tao DATETIME,
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id)
);

-- 31b. TIẾN ĐỘ ĐỀ TÀI THEO ĐỢT (dùng cho thống kê chi tiết)
CREATE TABLE TienDoDeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT NOT NULL,
    id_de_tai INT NOT NULL,
    tong_task INT NOT NULL DEFAULT(0),
    task_done INT NOT NULL DEFAULT(0),
    task_in_progress INT NOT NULL DEFAULT(0),
    task_new INT NOT NULL DEFAULT(0),
    diem_trung_binh FLOAT NULL,
    last_updated DATETIME NOT NULL DEFAULT(getdate()),
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_de_tai) REFERENCES DeTai(id),
    UNIQUE (id_dot, id_de_tai)
);

-- 32. ĐƠN PHÚC KHẢO
CREATE TABLE DonPhucKhao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_sinh_vien INT,
    tieu_de_khieu_nai NVARCHAR(200),
    noi_dung_khieu_nai NVARCHAR(MAX),
    minh_chung_link VARCHAR(255),
    ngay_gui DATETIME NOT NULL DEFAULT(getdate()),
    ngay_xu_ly DATETIME,
    phan_hoi_cua_gv NVARCHAR(MAX),
    trang_thai VARCHAR(20) NOT NULL DEFAULT('CHO_XU_LY'),
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id),
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung)
);

-- 33. ĐIỂM CHI TIẾT
CREATE TABLE DiemChiTiet (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_phien_bao_ve INT,
    id_sinh_vien INT,
    id_tieu_chi INT,
    id_nguoi_cham INT,
    diem_so FLOAT,
    nhan_xet NVARCHAR(MAX),
    FOREIGN KEY (id_phien_bao_ve) REFERENCES PhienBaoVe(id),
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung),
    FOREIGN KEY (id_tieu_chi) REFERENCES TieuChiChamDiem(id),
    FOREIGN KEY (id_nguoi_cham) REFERENCES NguoiDung(id)
);

-- 34b. ĐIỂM HỘI ĐỒNG THEO ĐỀ TÀI (tổng hợp)
CREATE TABLE DiemHoiDong_DeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_phien_bao_ve INT NOT NULL,
    id_de_tai INT NOT NULL,
    tong_diem FLOAT,
    nhan_xet NVARCHAR(MAX),
    FOREIGN KEY (id_phien_bao_ve) REFERENCES PhienBaoVe(id),
    FOREIGN KEY (id_de_tai) REFERENCES DeTai(id),
    UNIQUE (id_phien_bao_ve, id_de_tai)
);

-- 34. LỊCH SỬ GỬI EMAIL
CREATE TABLE LichSuGuiEmail (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_cau_hinh INT,
    nguoi_nhan VARCHAR(100),
    thoi_gian_gui DATETIME,
    trang_thai VARCHAR(20),
    FOREIGN KEY (id_cau_hinh) REFERENCES CauHinhThongBao(id)
);

-- 35. RÀNG BUỘC THAM CHIẾU TÔN ĐỌNG (bổ sung khóa ngoại sau khi có bảng phụ thuộc)
ALTER TABLE KeHoachCongViec
ADD CONSTRAINT FK_KHCV_MinhChung FOREIGN KEY (id_file_minh_chung) REFERENCES BaoCaoNop(id);

-- Hoàn tất tạo bảng
