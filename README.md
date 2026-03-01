USE QuanLyDoAnTotNghiep;
GO

-- =====================================================================
-- PHẦN 1: TẠO CÁC BẢNG (CREATE TABLES)
-- =====================================================================

-- 1. NHÓM NGƯỜI DÙNG & PHÂN QUYỀN
CREATE TABLE NguoiDung (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ho_ten NVARCHAR(100),
    email VARCHAR(100) NOT NULL UNIQUE,
    mat_khau VARCHAR(255) NULL,
    sdt VARCHAR(20) NULL,
    microsoft_id VARCHAR(100) NULL, 
    avatar_url NVARCHAR(500) NULL,
    trang_thai INT DEFAULT 1
);

CREATE TABLE VaiTro (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_vai_tro VARCHAR(50) UNIQUE, 
    ten_vai_tro NVARCHAR(100)
);

CREATE TABLE NguoiDung_VaiTro (
    id_nguoi_dung INT,
    id_vai_tro INT,
    PRIMARY KEY (id_nguoi_dung, id_vai_tro)
);

-- 2. NHÓM ĐÀO TẠO
CREATE TABLE BoMon (
    id INT IDENTITY(1,1) PRIMARY KEY,
    stt INT,
    ten_bo_mon NVARCHAR(100),
    ten_viet_tat NVARCHAR(20),
    id_nguoi_tao INT,
    ngay_tao DATE,
    id_nguoi_sua INT,
    ngay_sua DATE
);

CREATE TABLE Nganh (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_nganh VARCHAR(20) NOT NULL,
    ten_nganh NVARCHAR(100),
    ten_viet_tat NVARCHAR(20),
    id_nguoi_tao INT,
    ngay_tao DATE,
    id_nguoi_sua INT,
    ngay_sua DATE,
    id_bo_mon INT
);

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
    id_bo_mon INT
);

CREATE TABLE KhoaHoc (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_khoa VARCHAR(20),
    ten_khoa NVARCHAR(50),
    nam_nhap_hoc INT,
    nam_tot_nghiep INT,
    trang_thai BIT DEFAULT 1 -- Đang đào tạo, đã tốt nghiệp
);

CREATE TABLE HocKi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_hoc_ki VARCHAR(20),
    nam_bat_dau INT,
    nam_ket_thuc INT,
    tuan_bat_dau INT,
    ngay_bat_dau DATE,
    trang_thai BIT DEFAULT 1 -- đang diễn ra, đã kết thúc
);

CREATE TABLE ChuongTrinhDaoTao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_ctdt VARCHAR(50) NOT NULL UNIQUE,
    ten_ctdt NVARCHAR(200),
    stt_hien_thi INT,
    id_nganh INT,
    id_khoa_hoc INT,
    tong_tin_chi INT, 
    trang_thai BIT DEFAULT 1,
    ngay_tao DATETIME DEFAULT GETDATE()
);

CREATE TABLE ChiTiet_CTDT (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_ctdt INT,
    stt INT,
    ma_hoc_phan VARCHAR(50),
    ten_hoc_phan NVARCHAR(150),
    so_tin_chi INT,
    loai_hoc_phan NVARCHAR(50),
    dieu_kien_tien_quyet NVARCHAR(255),
    hoc_ki_to_chuc INT
);

CREATE TABLE SinhVien (
    id_nguoi_dung INT PRIMARY KEY, 
    mssv VARCHAR(20) UNIQUE,
    id_chuyen_nganh INT,
    id_khoa_hoc INT,
    tin_chi_tich_luy FLOAT DEFAULT 0
);

CREATE TABLE GiangVien (
    id_nguoi_dung INT PRIMARY KEY,
    ma_gv VARCHAR(20) UNIQUE,
    hoc_vi NVARCHAR(50),
    id_bo_mon INT
);

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
    ket_qua BIT 
);

-- 3. NHÓM ĐỢT & ĐỀ TÀI (bổ sung luồng xét duyệt hội đồng)
-- Luồng xét duyệt:
--  * Chỉ thành viên hội đồng duyệt của đợt được ghi nhận xét và chọn Duyệt/Từ chối.
--  * Người đề xuất chỉ xem trạng thái và toàn bộ nhận xét, không có nút duyệt/từ chối.
--  * Trạng thái tổng của đề tài:
--      - Nếu bất kỳ nhận xét có trạng thái TU_CHOI -> đề tài = TU_CHOI.
--      - Nếu chưa đủ số nhận xét bằng số thành viên hội đồng -> đề tài = CHO_DUYET.
--      - Nếu đủ nhận xét và tất cả DA_DUYET -> đề tài = DA_DUYET.
--  * Thông báo:
--      - Gửi cho người đề xuất khi đề tài bị từ chối.
--      - Gửi cho người đề xuất khi đủ số thành viên duyệt và trạng thái chuyển DA_DUYET.

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

    ngay_lap_hoi_dong_duyet_DXDT INT,
    ngay_duyet_DXDT INT,

    ngay_bat_dau_nop_de_cuong DATE,
    ngay_ket_thuc_nop_de_cuong DATE,

    ngay_bat_dau_bao_cao_cuoi_ki DATE,
    ngay_ket_thuc_bao_cao_cuoi_ki DATE,

    ngay_lap_HD_BCCK INT,
    ngay_nop_tai_lieu_BCCK INT,
    ngay_cong_bo_kq_BCCK INT,

    ngay_bat_dau_bao_cao_giua_ki DATE,
    ngay_ket_thuc_bao_cao_giua_ki DATE,

    ngay_lap_HD_BCGK INT,
    ngay_nop_tai_lieu_BCGK INT,

    trang_thai BIT
);

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

    trang_thai VARCHAR(20) DEFAULT 'CHO_DUYET', -- tính từ bảng NhanXetHoiDongDeTai theo đủ nhận xét hội đồng
    nhan_xet_duyet NVARCHAR(MAX), -- giữ cho tương thích, nhưng nhận xét chính nằm ở bảng NhanXetHoiDongDeTai
    nguoi_duyet INT
);

-- Nhận xét/duyệt theo từng thành viên hội đồng duyệt đề tài
CREATE TABLE NhanXetHoiDongDeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_de_tai INT NOT NULL,
    id_giang_vien INT NOT NULL, -- thành viên hội đồng (GiangVien)
    trang_thai VARCHAR(20),      -- DA_DUYET | TU_CHOI | CHO_DUYET
    nhan_xet NVARCHAR(MAX),
    ngay_tao DATETIME DEFAULT GETDATE()
);

CREATE TABLE DeCuong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_de_tai INT UNIQUE,
    
    ly_do_chon_de_tai NVARCHAR(MAX),
    gia_thuyet_nghien_cuu NVARCHAR(MAX),
    doi_tuong_nghien_cuu NVARCHAR(MAX),
    pham_vi_nghien_cuu NVARCHAR(MAX),
    phuong_phap_nghien_cuu NVARCHAR(MAX),
    
    ngay_nop DATETIME DEFAULT GETDATE(),
    trang_thai VARCHAR(20) -- CHO_DUYET, DA_DUYET, CAN_SUA
);

-- BẢNG ĐĂNG KÝ NGUYỆN VỌNG - ĐÃ SỬA KIỂU DỮ LIỆU
CREATE TABLE DangKyNguyenVong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_sinh_vien INT,
    so_tin_chi_tich_luy_hien_tai INT,   
    trang_thai INT DEFAULT 0, -- 0: Chờ xử lý, 1: Đạt, 2: Không đạt
    ngay_dang_ky DATETIME DEFAULT GETDATE()
);

CREATE TABLE SinhVien_DeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_de_tai INT,
    id_sinh_vien INT,
    trang_thai VARCHAR(20),
    ngay_dang_ky DATETIME DEFAULT GETDATE(),
    nhan_xet NVARCHAR(MAX)
);

-- 4. NHÓM TIÊU CHÍ CHẤM
CREATE TABLE LoaiPhieuCham (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten_loai_phieu NVARCHAR(100),
    nguoi_tao INT
);

CREATE TABLE TieuChiChamDiem (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_loai_phieu INT,
    ten_tieu_chi NVARCHAR(200),
    mo_ta_huong_dan NVARCHAR(MAX),
    trong_so FLOAT,
    diem_toi_da FLOAT,
    stt_hien_thi INT
);

CREATE TABLE CauHinhPhieuCham_Dot (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    vai_tro_cham VARCHAR(50), 
    id_loai_phieu INT
);

-- 5. NHÓM HỘI ĐỒNG & CHẤM ĐIỂM
CREATE TABLE HoiDongBaoCao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_hoi_dong VARCHAR(20),
    ten_hoi_dong NVARCHAR(100),
    loai_hoi_dong VARCHAR(20), 
    id_dot INT,
    id_nguoi_tao INT,
    id_bo_mon INT,

    ngay_bao_cao DATE,
    dia_diem NVARCHAR(100),
    thoi_gian_du_kien TIME,
    trang_thai BIT,
    
    -- CỘT MỚI THÊM TỪ SQLQuery5.sql
    ngay_bat_dau DATE NULL,
    ngay_ket_thuc DATE NULL
);

CREATE TABLE ThanhVien_HD_BaoCao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_hd_baocao INT,
    id_giang_vien INT,
    vai_tro VARCHAR(50) 
);

CREATE TABLE PhienBaoVe (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_hd_baocao INT,
    id_sinh_vien_de_tai INT,
    stt_bao_cao INT,
    link_tai_lieu VARCHAR(255)
);

CREATE TABLE DiemChiTiet (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_phien_bao_ve INT,
    id_nguoi_cham INT,
    id_sinh_vien INT, 
    id_tieu_chi INT,  
    diem_so FLOAT,
    nhan_xet NVARCHAR(MAX)
);

CREATE TABLE KetQuaBaoVe_SinhVien (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_phien_bao_ve INT,
    id_sinh_vien INT,
    diem_tong_ket FLOAT,
    diem_chu VARCHAR(5),
    ket_qua VARCHAR(20)
);

-- 6. NHÓM EMAIL & BÁO CÁO
CREATE TABLE MauThongBao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_mau VARCHAR(50) UNIQUE,
    tieu_de NVARCHAR(200),
    noi_dung_thong_bao NVARCHAR(MAX)
);

CREATE TABLE CauHinhThongBao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_mau INT,
    loai_su_kien VARCHAR(50),
    moc_thoi_gian VARCHAR(20),
    so_ngay_chenh_lech INT,
    doi_tuong_nhan VARCHAR(50),
    tieu_de_mau NVARCHAR(200),
    noi_dung_mau NVARCHAR(MAX),
    trang_thai BIT DEFAULT 1
);

CREATE TABLE LichSuGuiEmail (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_cau_hinh INT,
    nguoi_nhan VARCHAR(100),
    thoi_gian_gui DATETIME,
    trang_thai VARCHAR(20)
);

CREATE TABLE ThongBao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_nguoi_nhan INT,
    tieu_de NVARCHAR(200),
    noi_dung NVARCHAR(MAX),
    link_lien_ket VARCHAR(255),
    trang_thai_xem BIT DEFAULT 0,
    ngay_tao DATETIME DEFAULT GETDATE()
);

CREATE TABLE DonPhucKhao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_sinh_vien INT,
    id_dot INT,
    tieu_de_khieu_nai NVARCHAR(200),
    noi_dung_khieu_nai NVARCHAR(MAX),
    minh_chung_link VARCHAR(255),
    trang_thai VARCHAR(20) DEFAULT 'CHO_XU_LY',
    phan_hoi_cua_gv NVARCHAR(MAX), 
    ngay_gui DATETIME DEFAULT GETDATE(),
    ngay_xu_ly DATETIME
);

CREATE TABLE KeHoachCongViec (
    id INT IDENTITY(1,1) PRIMARY KEY,
    stt INT,
    id_sinh_vien INT NOT NULL,
    id_dot INT NULL,           -- hoặc id_de_tai, id_hoc_ki nếu cần
    tuan INT NULL,             -- 1..16, tính từ mốc kết thúc duyệt đề tài
    thu_trong_tuan TINYINT NULL,   -- 2..8 (Thứ 2..CN) nếu dùng lưới giờ-thứ
    gio_bat_dau TIME NULL,       
    gio_ket_thuc TIME NULL,
    gio_bat_dau_thuc_te TIME NULL,       
    gio_ket_thuc_thuc_te TIME NULL,
    ten_cong_viec NVARCHAR(200),
    mo_ta_cong_viec NVARCHAR(MAX),
    trang_thai VARCHAR(20),
    id_file_minh_chung INT NULL,
    FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung),
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id)
);

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
    trang_thai VARCHAR(20)
);

CREATE TABLE NhatKyHuongDan (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT NULL,                   -- FK DotDoAn (nếu dùng)
    ngay_hop DATE,
    thoi_gian_hop TIME,
    hinh_thuc_hop NVARCHAR(50),        -- Online/Offline
    dia_diem_hop NVARCHAR(200),
    thanh_vien_tham_du NVARCHAR(MAX),
    ten_gvhd NVARCHAR(200),
    muc_tieu_buoi_hop NVARCHAR(MAX),
    noi_dung_hop NVARCHAR(MAX),
    -- Action list gộp vào 1 cột JSON, chứa mảng {task, owner, deadline}
    action_list NVARCHAR(MAX),         -- JSON string, ví dụ:
                                       -- [
                                       --   {"task":"T1","owner":"A","deadline":"2024-06-01"},
                                       --   {"task":"T2","owner":"B","deadline":"2024-06-05"}
                                       -- ]
    -- FK tùy chọn:
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id)
);

CREATE TABLE BaoCaoThongKe (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT NOT NULL,                 -- FK DotDoAn
    so_luong_sinh_vien INT NOT NULL,     -- tổng SV trong đợt
    so_luong_de_tai INT NOT NULL,        -- tổng đề tài trong đợt
    so_luong_task_tuan INT NULL,         -- tổng số task theo tuần (có thể là tổng mới nhất)
    ti_le_hoan_thanh FLOAT NULL,         -- % hoàn thành đề tài hoặc task
    ngay_tinh DATETIME NOT NULL DEFAULT(getdate()),
    -- nếu cần lưu chi tiết theo tuần dạng JSON:
    chi_tiet_tuan NVARCHAR(MAX) NULL     -- JSON: [{tuan:1, so_task:..., ti_le_hoan_thanh:...}, ...]
    FOREIGN KEY (id_dot) REFERENCES DotDoAn(id)
);

GO

-- =====================================================================
-- PHẦN 2: TẠO CÁC RÀNG BUỘC KHÓA NGOẠI (FOREIGN KEYS)
-- =====================================================================

-- 1. Người dùng & Phân quyền
ALTER TABLE NguoiDung_VaiTro ADD CONSTRAINT FK_NDVT_NguoiDung FOREIGN KEY (id_nguoi_dung) REFERENCES NguoiDung(id);
ALTER TABLE NguoiDung_VaiTro ADD CONSTRAINT FK_NDVT_VaiTro FOREIGN KEY (id_vai_tro) REFERENCES VaiTro(id);

-- 2. Sinh viên & Giảng viên
ALTER TABLE SinhVien ADD CONSTRAINT FK_SV_NguoiDung FOREIGN KEY (id_nguoi_dung) REFERENCES NguoiDung(id);
ALTER TABLE SinhVien ADD CONSTRAINT FK_SV_ChuyenNganh FOREIGN KEY (id_chuyen_nganh) REFERENCES ChuyenNganh(id);
ALTER TABLE SinhVien ADD CONSTRAINT FK_SV_KhoaHoc FOREIGN KEY (id_khoa_hoc) REFERENCES KhoaHoc(id);

ALTER TABLE GiangVien ADD CONSTRAINT FK_GV_NguoiDung FOREIGN KEY (id_nguoi_dung) REFERENCES NguoiDung(id);
ALTER TABLE GiangVien ADD CONSTRAINT FK_GV_BoMon FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id);

-- 3. Đào tạo
ALTER TABLE Nganh ADD CONSTRAINT FK_Nganh_BoMon FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id);
ALTER TABLE ChuyenNganh ADD CONSTRAINT FK_CN_Nganh FOREIGN KEY (id_nganh) REFERENCES Nganh(id);
ALTER TABLE ChuyenNganh ADD CONSTRAINT FK_CN_BoMon FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id);
ALTER TABLE KetQuaHocTap ADD CONSTRAINT FK_KQHT_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);

-- 4. CTDT
ALTER TABLE ChuongTrinhDaoTao ADD CONSTRAINT FK_CTDT_Nganh FOREIGN KEY (id_nganh) REFERENCES Nganh(id);
ALTER TABLE ChuongTrinhDaoTao ADD CONSTRAINT FK_CTDT_KhoaHoc FOREIGN KEY (id_khoa_hoc) REFERENCES KhoaHoc(id);
ALTER TABLE ChiTiet_CTDT ADD CONSTRAINT FK_CTDT_ChiTiet FOREIGN KEY (id_ctdt) REFERENCES ChuongTrinhDaoTao(id);

-- 5. Đợt & Đề tài
ALTER TABLE DotDoAn ADD CONSTRAINT FK_Dot_KhoaHoc FOREIGN KEY (id_khoa_hoc) REFERENCES KhoaHoc(id);
ALTER TABLE DotDoAn ADD CONSTRAINT FK_Dot_HocKi FOREIGN KEY (id_hoc_ki) REFERENCES HocKi(id);

ALTER TABLE DeTai ADD CONSTRAINT FK_DeTai_NguoiDeXuat FOREIGN KEY (id_nguoi_de_xuat) REFERENCES NguoiDung(id);
ALTER TABLE DeTai ADD CONSTRAINT FK_DeTai_GVHD FOREIGN KEY (id_gvhd) REFERENCES GiangVien(id_nguoi_dung);
ALTER TABLE DeTai ADD CONSTRAINT FK_DeTai_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE DeTai ADD CONSTRAINT FK_DeTai_ChuyenNganh FOREIGN KEY (id_chuyen_nganh) REFERENCES ChuyenNganh(id);
ALTER TABLE DeTai ADD CONSTRAINT FK_DeTai_NguoiDuyet FOREIGN KEY (nguoi_duyet) REFERENCES GiangVien(id_nguoi_dung);

-- Nhận xét hội đồng duyệt đề tài
ALTER TABLE NhanXetHoiDongDeTai ADD CONSTRAINT FK_NXHD_DeTai FOREIGN KEY (id_de_tai) REFERENCES DeTai(id);
ALTER TABLE NhanXetHoiDongDeTai ADD CONSTRAINT FK_NXHD_GiangVien FOREIGN KEY (id_giang_vien) REFERENCES GiangVien(id_nguoi_dung);

-- FK cho Bảng Đề Cương
ALTER TABLE DeCuong ADD CONSTRAINT FK_DeCuong_DeTai FOREIGN KEY (id_de_tai) REFERENCES DeTai(id);

-- FK cho Bảng Đăng Ký Nguyện Vọng
ALTER TABLE DangKyNguyenVong ADD CONSTRAINT FK_DKNV_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);
ALTER TABLE DangKyNguyenVong ADD CONSTRAINT FK_DKNV_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);

ALTER TABLE SinhVien_DeTai ADD CONSTRAINT FK_SVDT_DeTai FOREIGN KEY (id_de_tai) REFERENCES DeTai(id);
ALTER TABLE SinhVien_DeTai ADD CONSTRAINT FK_SVDT_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);

-- 6. Tiêu chí & Hội đồng
ALTER TABLE LoaiPhieuCham ADD CONSTRAINT FK_LPC_NguoiTao FOREIGN KEY (nguoi_tao) REFERENCES NguoiDung(id);
ALTER TABLE TieuChiChamDiem ADD CONSTRAINT FK_TCCD_LoaiPhieu FOREIGN KEY (id_loai_phieu) REFERENCES LoaiPhieuCham(id);
ALTER TABLE CauHinhPhieuCham_Dot ADD CONSTRAINT FK_CHPC_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE CauHinhPhieuCham_Dot ADD CONSTRAINT FK_CHPC_LoaiPhieu FOREIGN KEY (id_loai_phieu) REFERENCES LoaiPhieuCham(id);

ALTER TABLE HoiDongBaoCao ADD CONSTRAINT FK_HDBC_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE HoiDongBaoCao ADD CONSTRAINT FK_HDBC_BoMon FOREIGN KEY (id_bo_mon) REFERENCES BoMon(id); 
-- FK MỚI THÊM TỪ SQLQuery4.sql
ALTER TABLE HoiDongBaoCao ADD CONSTRAINT FK_HDBC_NguoiTao FOREIGN KEY (id_nguoi_tao) REFERENCES NguoiDung(id);

ALTER TABLE ThanhVien_HD_BaoCao ADD CONSTRAINT FK_TVHD_HD FOREIGN KEY (id_hd_baocao) REFERENCES HoiDongBaoCao(id);
ALTER TABLE ThanhVien_HD_BaoCao ADD CONSTRAINT FK_TVHD_GV FOREIGN KEY (id_giang_vien) REFERENCES GiangVien(id_nguoi_dung);

-- Phiên bảo vệ liên kết với SinhVien_DeTai
ALTER TABLE PhienBaoVe ADD CONSTRAINT FK_PBV_HD FOREIGN KEY (id_hd_baocao) REFERENCES HoiDongBaoCao(id);
ALTER TABLE PhienBaoVe ADD CONSTRAINT FK_PBV_SVDT FOREIGN KEY (id_sinh_vien_de_tai) REFERENCES SinhVien_DeTai(id);

ALTER TABLE DiemChiTiet ADD CONSTRAINT FK_DCT_Phien FOREIGN KEY (id_phien_bao_ve) REFERENCES PhienBaoVe(id);
ALTER TABLE DiemChiTiet ADD CONSTRAINT FK_DCT_NguoiCham FOREIGN KEY (id_nguoi_cham) REFERENCES GiangVien(id_nguoi_dung);
ALTER TABLE DiemChiTiet ADD CONSTRAINT FK_DCT_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);
ALTER TABLE DiemChiTiet ADD CONSTRAINT FK_DCT_TieuChi FOREIGN KEY (id_tieu_chi) REFERENCES TieuChiChamDiem(id);

ALTER TABLE KetQuaBaoVe_SinhVien ADD CONSTRAINT FK_KQBV_Phien FOREIGN KEY (id_phien_bao_ve) REFERENCES PhienBaoVe(id);
ALTER TABLE KetQuaBaoVe_SinhVien ADD CONSTRAINT FK_KQBV_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);

-- 7. Email & Báo cáo & Tiến độ
ALTER TABLE CauHinhThongBao ADD CONSTRAINT FK_CHTB_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE CauHinhThongBao ADD CONSTRAINT FK_CHTB_Mau FOREIGN KEY (id_mau) REFERENCES MauThongBao(id);
ALTER TABLE LichSuGuiEmail ADD CONSTRAINT FK_LSGE_CauHinh FOREIGN KEY (id_cau_hinh) REFERENCES CauHinhThongBao(id);
ALTER TABLE ThongBao ADD CONSTRAINT FK_TB_NguoiNhan FOREIGN KEY (id_nguoi_nhan) REFERENCES NguoiDung(id);

-- FK cho BaoCaoNop
ALTER TABLE BaoCaoNop ADD CONSTRAINT FK_BCN_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE BaoCaoNop ADD CONSTRAINT FK_BCN_DeTai FOREIGN KEY (id_de_tai) REFERENCES DeTai(id);
ALTER TABLE BaoCaoNop ADD CONSTRAINT FK_BCN_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);

-- FK cho KeHoachCongViec
ALTER TABLE KeHoachCongViec ADD CONSTRAINT FK_KHCV_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);
ALTER TABLE KeHoachCongViec ADD CONSTRAINT FK_KHCV_MinhChung FOREIGN KEY (id_file_minh_chung) REFERENCES BaoCaoNop(id);

-- FK cho NhatKyHuongDan
ALTER TABLE NhatKyHuongDan ADD CONSTRAINT FK_NKHD_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);

ALTER TABLE DonPhucKhao ADD CONSTRAINT FK_DPK_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);
ALTER TABLE DonPhucKhao ADD CONSTRAINT FK_DPK_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE BaoCaoThongKe ADD CONSTRAINT FK_BCTK_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);




-- =====================================================================
-- PHẦN 3: INSERT DỮ LIỆU MẪU
-- =====================================================================

-- 3.1 VAI TRÒ
INSERT INTO VaiTro (ma_vai_tro, ten_vai_tro) VALUES 
('BCN_KHOA', N'Ban Chủ nhiệm Khoa'),
('BO_MON', N'Bộ môn'),
('GIANG_VIEN', N'Giảng viên'),
('SINH_VIEN', N'Sinh viên'),
('CHUA_PHAN_QUYEN', N'Chưa phân quyền');

-- 3.2 NGƯỜI DÙNG
INSERT INTO NguoiDung (ho_ten, email, mat_khau, sdt, microsoft_id, avatar_url, trang_thai) VALUES 
(N'Nguyễn Văn A', 'nguyenvana@example.com', 'pass123', '0901000001', NULL, N'/images/avatars/user1.png', 1),
(N'Trần Thị B', 'tranthib@example.com', 'pass123', '0901000002', NULL, N'/images/avatars/user2.png', 1),
(N'Lê Văn C', 'levanc@example.com', 'pass123', '0901000003', 'MS_ID_003', N'/images/avatars/user3.png', 1),
(N'Phạm Thị D', 'phamthid@example.com', 'pass123', '0901000004', NULL, N'/images/avatars/user4.png', 1),
(N'Hoàng Văn E', 'hoangvane@example.com', 'pass123', '0901000005', NULL, NULL, 1),
(N'Vũ Thị F', 'vuthif@example.com', 'pass123', '0901000006', 'MS_ID_006', N'/images/avatars/user6.png', 1),
(N'Đặng Văn G', 'dangvang@example.com', 'pass123', '0901000007', NULL, N'/images/avatars/user7.png', 1),
(N'Bùi Thị H', 'buithih@example.com', 'pass123', '0901000008', NULL, NULL, 1),
(N'Đỗ Văn I', 'dovani@example.com', 'pass123', '0901000009', 'MS_ID_009', N'/images/avatars/user9.png', 1),
(N'Hồ Thị K', 'hothik@example.com', 'pass123', '0901000010', NULL, N'/images/avatars/user10.png', 1),
(N'Ngô Văn L', 'ngovanl@example.com', 'pass123', '0901000011', NULL, N'/images/avatars/user11.png', 1),
(N'Dương Thị M', 'duongthim@example.com', 'pass123', '0901000012', 'MS_ID_012', NULL, 1),
(N'Lý Văn N', 'lyvann@example.com', 'pass123', '0901000013', NULL, N'/images/avatars/user13.png', 1),
(N'Trương Thị O', 'truongthio@example.com', 'pass123', '0901000014', NULL, N'/images/avatars/user14.png', 1),
(N'Nguyễn Văn P', 'nguyenvanp@example.com', 'pass123', '0901000015', 'MS_ID_015', N'/images/avatars/user15.png', 1),
(N'Trần Thị Q', 'tranthiq@example.com', 'pass123', '0901000016', NULL, NULL, 1),
(N'Lê Văn R', 'levanr@example.com', 'pass123', '0901000017', NULL, N'/images/avatars/user17.png', 1),
(N'Phạm Thị S', 'phamthis@example.com', 'pass123', '0901000018', 'MS_ID_018', N'/images/avatars/user18.png', 1),
(N'Hoàng Văn T', 'hoangvant@example.com', 'pass123', '0901000019', NULL, N'/images/avatars/user19.png', 1),
(N'Vũ Thị U', 'vuthiu@example.com', 'pass123', '0901000020', NULL, N'/images/avatars/user20.png', 1),
(N'Võ Văn V', 'vovanv@example.com', 'pass123', '0901000021', 'MS_ID_021', N'/images/avatars/user21.png', 1);

-- 3.3 PHÂN QUYỀN
INSERT INTO NguoiDung_VaiTro (id_nguoi_dung, id_vai_tro) VALUES 
(1, 1), (2, 1), (3, 1), (4, 1),  -- BCN Khoa
(5, 2), (6, 2), (7, 2), (8, 2),  -- Bộ môn
(9, 3), (10, 3), (11, 3), (12, 3), (18, 3), (19, 3), (20, 3), (21, 3),  -- Giảng viên
(13, 4), (14, 4), (15, 4), (16, 4),  -- Sinh viên
(17, 5);  -- Chưa phân quyền

-- 3.4 BỘ MÔN
INSERT INTO BoMon (stt, ten_bo_mon, ten_viet_tat, id_nguoi_tao, ngay_tao, id_nguoi_sua, ngay_sua) VALUES 
(1, N'Kỹ thuật Phần mềm', N'KTPM', 1, '2023-01-15', NULL, NULL),
(2, N'Hệ thống Thông tin', N'HTTT', 1, '2023-01-15', 2, '2023-06-20'),
(3, N'Khoa học Máy tính', N'KHMT', 1, '2023-01-15', NULL, NULL),
(4, N'Mạng máy tính & Truyền thông', N'MMT&TT', 1, '2023-02-01', NULL, NULL),
(5, N'An toàn Thông tin', N'ATTT', 1, '2023-02-01', 3, '2024-01-10'),
(6, N'Khoa học Dữ liệu & Trí tuệ nhân tạo', N'DS&AI', 1, '2023-03-10', NULL, NULL),
(7, N'Toán - Tin ứng dụng', N'TTUD', 1, '2023-01-15', NULL, NULL);

-- 3.5 NGÀNH
INSERT INTO Nganh (ma_nganh, ten_nganh, ten_viet_tat, id_nguoi_tao, ngay_tao, id_nguoi_sua, ngay_sua, id_bo_mon) VALUES 
('7480101', N'Khoa học máy tính', N'KHMT', 1, '2023-01-20', NULL, NULL, 3),
('7480102', N'Mạng máy tính và TT dữ liệu', N'MMT&TT', 1, '2023-01-20', NULL, NULL, 4),
('7480103', N'Kỹ thuật phần mềm', N'KTPM', 1, '2023-01-20', NULL, NULL, 1),
('7480104', N'Hệ thống thông tin', N'HTTT', 1, '2023-01-20', 2, '2023-05-15', 2),
('7480110', N'Thương mại điện tử', N'TMĐT', 1, '2023-02-10', NULL, NULL, 2),
('7480201', N'An toàn thông tin', N'ATTT', 1, '2023-02-15', 3, '2023-06-01', 5),
('7480109', N'Khoa học dữ liệu', N'KHDL', 1, '2023-03-01', NULL, NULL, 6),
('7480108', N'Trí tuệ nhân tạo', N'AI', 1, '2023-03-01', NULL, NULL, 6);

-- 3.6 CHUYÊN NGÀNH
INSERT INTO ChuyenNganh (stt, ten_chuyen_nganh, ten_viet_tat, id_nguoi_tao, ngay_tao, id_nguoi_sua, ngay_sua, id_nganh, id_bo_mon) VALUES 
(1, N'Công nghệ Web', N'Web', 1, '2023-05-01', NULL, NULL, 3, 1),
(2, N'Lập trình thiết bị di động', N'Mobile', 1, '2023-05-01', NULL, NULL, 3, 1),
(3, N'Kiểm thử phần mềm', N'Tester', 1, '2023-05-01', 2, '2023-09-12', 3, 1),
(1, N'Hệ thống thông tin quản lý', N'MIS', 1, '2023-05-05', NULL, NULL, 4, 2),
(2, N'Phân tích dữ liệu kinh doanh', N'BA', 1, '2023-05-05', NULL, NULL, 4, 2),
(1, N'Quản trị mạng doanh nghiệp', N'NetAdmin', 1, '2023-06-01', NULL, NULL, 2, 4),
(2, N'Điện toán đám mây', N'Cloud', 1, '2023-06-01', NULL, NULL, 2, 4),
(1, N'Điều tra số', N'Forensic', 1, '2023-06-10', 3, '2023-11-20', 6, 5),
(2, N'Đánh giá an toàn hệ thống', N'Pentest', 1, '2023-06-10', NULL, NULL, 6, 5),
(1, N'Trí tuệ nhân tạo ứng dụng', N'AI-App', 1, '2023-07-01', NULL, NULL, 1, 3);

-- 3.7 KHÓA HỌC
INSERT INTO KhoaHoc (ma_khoa, ten_khoa, nam_nhap_hoc, nam_tot_nghiep, trang_thai) VALUES 
('K25', N'Khóa 25', 2021, 2025, 1),
('K26', N'Khóa 26', 2022, 2026, 1),
('K27', N'Khóa 27', 2023, 2027, 1),
('K28', N'Khóa 28', 2024, 2028, 1),
('K29', N'Khóa 29', 2025, 2029, 1),
('K30', N'Khóa 30', 2026, 2030, 1),
('K31', N'Khóa 31', 2027, 2031, 1);

-- 3.8 HỌC KỲ
INSERT INTO HocKi (ma_hoc_ki, nam_bat_dau, nam_ket_thuc, tuan_bat_dau, ngay_bat_dau, trang_thai) VALUES 
('HK1_099', 2023, 2024, 33, '2023-08-14', 0),
('HK2_099', 2023, 2024, 3,  '2024-01-15', 0),
('HK3_099', 2023, 2024, 25, '2024-06-17', 0),
('HK1_100', 2024, 2025, 33, '2024-08-12', 0),
('HK2_100', 2024, 2025, 3,  '2025-01-13', 0),
('HK3_100', 2024, 2025, 25, '2025-06-16', 0),
('HK1_101', 2025, 2026, 33, '2025-08-11', 0),
('HK2_101', 2025, 2026, 3,  '2026-01-12', 1),
('HK3_101', 2025, 2026, 25, '2026-06-15', 0);

-- 3.9 GIẢNG VIÊN
INSERT INTO GiangVien (id_nguoi_dung, ma_gv, hoc_vi, id_bo_mon) VALUES
(1, 'GV_001', N'Tiến sĩ', 1),
(2, 'GV_002', N'Thạc sĩ', 1),
(3, 'GV_003', N'Phó Giáo sư', 2),
(4, 'GV_004', N'Tiến sĩ', 3),
(5, 'GV_005', N'Tiến sĩ', 2),
(6, 'GV_006', N'Thạc sĩ', 4),
(7, 'GV_007', N'Thạc sĩ', 1),
(8, 'GV_008', N'Tiến sĩ', 5),
(9, 'GV_009', N'Thạc sĩ', 3),
(10, 'GV_010', N'Thạc sĩ', 2),
(11, 'GV_011', N'Tiến sĩ', 6),
(12, 'GV_012', N'Thạc sĩ', 7);

-- 3.10 SINH VIÊN
INSERT INTO SinhVien (id_nguoi_dung, mssv, id_chuyen_nganh, id_khoa_hoc, tin_chi_tich_luy) VALUES
(13, '2111001', 1, 3, 110.5),
(14, '2211002', 3, 4, 85.0),
(15, '2311003', 2, 5, 45.0),
(16, '2411004', 5, 6, 15.0);

-- 3.11 CHƯƠNG TRÌNH ĐÀO TẠO
INSERT INTO ChuongTrinhDaoTao (ma_ctdt, ten_ctdt, stt_hien_thi, id_nganh, id_khoa_hoc, tong_tin_chi, trang_thai, ngay_tao) VALUES 
('CTDT_K27', N'Chương trình đào tạo chuẩn Khóa 27', 1, 3, 3, 130, 1, '2023-08-01'),
('CTDT_K28', N'Chương trình đào tạo chuẩn Khóa 28', 1, 3, 4, 132, 1, '2024-08-01'),
('CTDT_K29', N'Chương trình đào tạo chuẩn Khóa 29', 1, 3, 5, 135, 1, '2025-08-01'),
('CTDT_K30', N'Chương trình đào tạo chuẩn Khóa 30', 1, 3, 6, 135, 1, '2026-08-01');

GO

-- 3.12 CHI TIẾT CTDT
DECLARE @id_k27 INT = (SELECT TOP 1 id FROM ChuongTrinhDaoTao WHERE ma_ctdt = 'CTDT_K27');
DECLARE @id_k28 INT = (SELECT TOP 1 id FROM ChuongTrinhDaoTao WHERE ma_ctdt = 'CTDT_K28');

INSERT INTO ChiTiet_CTDT (id_ctdt, stt, ma_hoc_phan, ten_hoc_phan, so_tin_chi, loai_hoc_phan, dieu_kien_tien_quyet, hoc_ki_to_chuc) VALUES
(@id_k27, 1, 'COMP1001', N'Nhập môn Lập trình', 3, N'Bắt buộc', NULL, 1),
(@id_k27, 2, 'MATH1001', N'Đại số tuyến tính', 3, N'Bắt buộc', NULL, 1),
(@id_k27, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, N'Bắt buộc', N'COMP1001', 2),
(@id_k27, 4, 'MATH1002', N'Toán rời rạc', 3, N'Bắt buộc', NULL, 2),
(@id_k27, 5, 'COMP2001', N'Cấu trúc dữ liệu và Giải thuật', 4, N'Bắt buộc', N'COMP1002', 3),
(@id_k27, 6, 'COMP2002', N'Kiến trúc máy tính', 3, N'Bắt buộc', NULL, 3),
(@id_k27, 7, 'COMP2003', N'Cơ sở dữ liệu', 4, N'Bắt buộc', N'COMP2001', 4),
(@id_k27, 8, 'COMP2004', N'Mạng máy tính', 3, N'Bắt buộc', N'COMP2002', 4),
(@id_k27, 9, 'COMP3001', N'Lập trình Web', 3, N'Bắt buộc', N'COMP2003', 5),
(@id_k27, 10, 'COMP3002', N'Công nghệ phần mềm', 3, N'Bắt buộc', N'COMP2003', 5),

(@id_k28, 1, 'COMP1001', N'Nhập môn Lập trình', 3, N'Bắt buộc', NULL, 1),
(@id_k28, 2, 'MATH1001', N'Đại số tuyến tính', 3, N'Bắt buộc', NULL, 1),
(@id_k28, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, N'Bắt buộc', N'COMP1001', 2),
(@id_k28, 4, 'MATH1002', N'Toán rời rạc', 3, N'Bắt buộc', NULL, 2),
(@id_k28, 5, 'COMP2001', N'Cấu trúc dữ liệu và Giải thuật', 4, N'Bắt buộc', N'COMP1002', 3);

-- 3.13 KẾT QUẢ HỌC TẬP
INSERT INTO KetQuaHocTap (id_sinh_vien, stt, ma_hoc_phan, ten_hoc_phan, so_tc, diem_so, diem_chu, tong_so_tin_chi, GPA, ket_qua) VALUES 
(13, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 9.0, 'A', 110.5, 3.6, 1),
(13, 2, 'MATH1001', N'Đại số tuyến tính', 3, 8.5, 'A', 110.5, 3.6, 1),
(13, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 7.5, 'B', 110.5, 3.6, 1),
(13, 4, 'MATH1002', N'Toán rời rạc', 3, 8.0, 'B', 110.5, 3.6, 1),
(13, 5, 'COMP2001', N'Cấu trúc dữ liệu và Giải thuật', 4, 9.5, 'A', 110.5, 3.6, 1),

(14, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 7.0, 'B', 85.0, 2.8, 1),
(14, 2, 'MATH1001', N'Đại số tuyến tính', 3, 6.5, 'C', 85.0, 2.8, 1),
(14, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.0, 'B', 85.0, 2.8, 1),

(15, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 10.0, 'A', 45.0, 3.9, 1),
(15, 2, 'MATH1001', N'Đại số tuyến tính', 3, 9.5, 'A', 45.0, 3.9, 1),

(16, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 3.5, 'F', 15.0, 1.2, 0),
(16, 2, 'MATH1001', N'Đại số tuyến tính', 3, 6.0, 'C', 15.0, 1.2, 1);

GO

-- 3.14 ĐỢT ĐỒ ÁN
DECLARE @id_k27 INT = (SELECT id FROM KhoaHoc WHERE ma_khoa = 'K27');
DECLARE @id_hk_active INT = (SELECT id FROM HocKi WHERE trang_thai = 1);

INSERT INTO DotDoAn (
    ten_dot, id_khoa_hoc, id_hoc_ki, ngay_bat_dau_dot, ngay_ket_thuc_dot,
    ngay_bat_dau_dk_nguyen_vong, ngay_ket_thuc_dk_nguyen_vong,
    ngay_bat_dau_dk_duyet_nguyen_vong, ngay_ket_thuc_dk_duyet_nguyen_vong,
    ngay_bat_dau_de_xuat_de_tai, ngay_ket_thuc_de_xuat_de_tai,
    ngay_bat_dau_duyet_de_xuat_de_tai, ngay_ket_thuc_duyet_de_xuat_de_tai,
    ngay_lap_hoi_dong_duyet_DXDT, ngay_duyet_DXDT,
    ngay_bat_dau_nop_de_cuong, ngay_ket_thuc_nop_de_cuong,
    ngay_bat_dau_bao_cao_cuoi_ki, ngay_ket_thuc_bao_cao_cuoi_ki,
    ngay_lap_HD_BCCK, ngay_nop_tai_lieu_BCCK, ngay_cong_bo_kq_BCCK,
    ngay_bat_dau_bao_cao_giua_ki, ngay_ket_thuc_bao_cao_giua_ki,
    ngay_lap_HD_BCGK, ngay_nop_tai_lieu_BCGK,
    trang_thai
) VALUES (
    N'Đợt ĐATN K27 - HK2 2025-2026', @id_k27, @id_hk_active, 
    '2026-01-15', '2026-06-15',
    '2026-02-01', '2026-02-05',
    '2026-02-06', '2026-02-10',
    '2026-01-15', '2026-01-25',
    '2026-01-26', '2026-01-30',
    3, 5,
    '2026-02-15', '2026-02-20',
    '2026-05-20', '2026-05-30',
    7, 3, 2,
    '2026-03-20', '2026-03-25',
    5, 3,
    1
);

GO

-- 3.15 ĐỀ TÀI
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

INSERT INTO DeTai (
    ma_de_tai, ten_de_tai, muc_tieu_chinh, yeu_cau_tinh_moi, 
    pham_vi_chuc_nang, cong_nghe_su_dung, san_pham_ket_qua_du_kien,
    id_nguoi_de_xuat, id_gvhd, id_dot, id_chuyen_nganh,
    trang_thai, nhan_xet_duyet, nguoi_duyet
) VALUES 
(
    'DT_27_001', N'Xây dựng hệ thống quản lý thư viện thông minh sử dụng RFID',
    N'Tự động hóa quy trình mượn trả sách và quản lý kho',
    N'Tích hợp thiết bị phần cứng RFID vào quy trình phần mềm',
    N'Quản lý sách, độc giả, mượn trả, thống kê báo cáo',
    N'.NET Core, ReactJS, SQL Server, RFID SDK',
    N'Website quản lý và App mobile cho độc giả',
    13, 1, @id_dot_active, 1, 
    'DA_DUYET', N'Đề tài có tính thực tiễn cao, đồng ý cho thực hiện.', 1
),
(
    'DT_27_002', N'Ứng dụng AI trong chẩn đoán bệnh phổi qua ảnh X-Quang',
    N'Hỗ trợ bác sĩ chẩn đoán nhanh bệnh viêm phổi',
    N'Áp dụng mô hình Deep Learning mới nhất (EfficientNet)',
    N'Upload ảnh, phân tích ảnh, trả kết quả chẩn đoán, lưu hồ sơ bệnh án',
    N'Python, PyTorch, Flask, PostgreSQL',
    N'Web app chẩn đoán bệnh',
    14, 3, @id_dot_active, 10, 
    'CHO_DUYET', NULL, NULL
),
(
    'DT_27_003', N'Xây dựng ứng dụng nghe nhạc trực tuyến',
    N'Cung cấp nền tảng nghe nhạc cho giới trẻ',
    N'Không có tính mới, trùng lặp nhiều với các đồ án cũ',
    N'Nghe nhạc, tạo playlist, tìm kiếm bài hát',
    N'Flutter, Firebase',
    N'Ứng dụng Android/iOS',
    15, 5, @id_dot_active, 2, 
    'TU_CHOI', N'Đề tài quá đơn giản, cần bổ sung chức năng gợi ý nhạc thông minh.', 1
),
(
    'DT_27_004', N'Hệ thống phát hiện tấn công DDoS dựa trên Machine Learning',
    N'Phát hiện và cảnh báo sớm các cuộc tấn công từ chối dịch vụ',
    N'Kết hợp thuật toán SVM và Random Forest để tăng độ chính xác',
    N'Giám sát lưu lượng mạng, phân tích gói tin, gửi cảnh báo qua Email/SMS',
    N'Python, Snort, Kibana, ElasticSearch',
    N'Phần mềm giám sát và Dashboard báo cáo',
    16, 9, @id_dot_active, 8, 
    'DA_DUYET', N'Đề tài phù hợp chuyên ngành, hướng đi tốt.', 2
);

GO

-- 3.16 ĐĂNG KÝ NGUYỆN VỌNG (ĐÃ ĐỔI SANG INT)
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

INSERT INTO DangKyNguyenVong (id_dot, id_sinh_vien, so_tin_chi_tich_luy_hien_tai, trang_thai, ngay_dang_ky) VALUES 
(@id_dot_active, 13, 110, 1, '2026-02-02 08:30:00'),  -- 1 = Đạt
(@id_dot_active, 14, 85, 2, '2026-02-02 09:15:00'),   -- 2 = Không đạt
(@id_dot_active, 15, 45, 2, '2026-02-02 10:00:00'),   -- 2 = Không đạt
(@id_dot_active, 16, 15, 2, '2026-02-02 10:30:00');   -- 2 = Không đạt

GO

-- 3.17 SINH VIÊN - ĐỀ TÀI
DECLARE @id_dt1 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_001');
DECLARE @id_dt2 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_002');
DECLARE @id_dt3 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_003');
DECLARE @id_dt4 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_004');

INSERT INTO SinhVien_DeTai (id_de_tai, id_sinh_vien, trang_thai, ngay_dang_ky, nhan_xet) VALUES 
(@id_dt1, 13, 'DA_DUYET', '2026-02-16 08:00:00', N'Đã tham gia nhóm thực hiện.'),
(@id_dt2, 14, 'CHO_DUYET', '2026-02-16 09:30:00', N'Đang chờ giảng viên xác nhận.'),
(@id_dt3, 15, 'TU_CHOI', '2026-02-16 10:15:00', N'Đề tài không đạt yêu cầu chuyên môn.'),
(@id_dt4, 16, 'DA_DUYET', '2026-02-16 11:00:00', N'Đã được phân công.');

GO

-- 3.18 ĐỀ CƯƠNG
DECLARE @id_dt1 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_001');
DECLARE @id_dt4 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_004');

INSERT INTO DeCuong (id_de_tai, ly_do_chon_de_tai, gia_thuyet_nghien_cuu, doi_tuong_nghien_cuu, pham_vi_nghien_cuu, phuong_phap_nghien_cuu, ngay_nop, trang_thai) VALUES 
(
    @id_dt1,
    N'Hiện nay việc quản lý thư viện bằng mã vạch tốn nhiều thời gian. Nhu cầu tự động hóa cao đang trở nên cấp thiết.',
    N'Ứng dụng công nghệ RFID sẽ giảm thiểu 50% thời gian chờ đợi của độc giả.',
    N'Quy trình nghiệp vụ thư viện và công nghệ thẻ thông minh RFID.',
    N'Xây dựng hệ thống phần mềm quản lý (Web App) và tích hợp đầu đọc thẻ RFID.',
    N'Sử dụng phương pháp phát triển phần mềm Agile Scrum.',
    '2026-02-20 08:00:00',
    'DA_DUYET'
),
(
    @id_dt4,
    N'Các cuộc tấn công DDoS ngày càng tinh vi, tường lửa truyền thống thường bỏ sót các mẫu tấn công mới.',
    N'Sự kết hợp giữa SVM và Random Forest sẽ cải thiện khả năng phát hiện tấn công DDoS.',
    N'Lưu lượng mạng (Network Traffic) và các đặc trưng của gói tin TCP/IP.',
    N'Tập trung phát hiện tấn công lớp ứng dụng (HTTP Flood) và lớp giao vận (SYN Flood).',
    N'Thu thập dữ liệu mẫu (Dataset CICIDS2017), tiền xử lý dữ liệu, huấn luyện mô hình.',
    '2026-02-20 09:30:00',
    'CHO_DUYET'
);

-- 3.19 LOẠI PHIẾU CHẤM
INSERT INTO LoaiPhieuCham (ten_loai_phieu, nguoi_tao) VALUES 
(N'Phiếu chấm của Giáo viên hướng dẫn', 1),
(N'Phiếu chấm của Hội đồng bảo vệ', 1);

GO

-- 3.20 TIÊU CHÍ CHẤM ĐIỂM
DECLARE @id_phieu_gvhd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%hướng dẫn%');
DECLARE @id_phieu_hd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%Hội đồng%');

INSERT INTO TieuChiChamDiem (id_loai_phieu, ten_tieu_chi, mo_ta_huong_dan, trong_so, diem_toi_da, stt_hien_thi) VALUES 
(@id_phieu_gvhd, N'Tinh thần và thái độ làm việc', N'Đánh giá sự chuyên cần, chủ động gặp gỡ GVHD và tuân thủ tiến độ.', 0.1, 1.0, 1),
(@id_phieu_gvhd, N'Kỹ năng giải quyết vấn đề', N'Khả năng tự nghiên cứu, tìm tòi công nghệ và xử lý các bài toán kỹ thuật.', 0.2, 2.0, 2),
(@id_phieu_gvhd, N'Kết quả thực hiện sản phẩm', N'Sản phẩm chạy ổn định, đáp ứng đủ các chức năng đã đăng ký trong đề cương.', 0.4, 4.0, 3),
(@id_phieu_gvhd, N'Chất lượng báo cáo thuyết minh', N'Trình bày đúng quy định, văn phong khoa học, nội dung logic.', 0.3, 3.0, 4),

(@id_phieu_hd, N'Hình thức và bố cục thuyết minh', N'Bố cục hợp lý, trình bày đẹp, đúng format chuẩn của khoa.', 0.1, 1.0, 1),
(@id_phieu_hd, N'Nội dung và kết quả đạt được', N'Đánh giá tính hoàn thiện, tính thực tiễn và độ phức tạp của đề tài.', 0.4, 4.0, 2),
(@id_phieu_hd, N'Kỹ năng trình bày và Demo', N'Trình bày tự tin, rõ ràng, Demo sản phẩm chạy tốt tại buổi bảo vệ.', 0.2, 2.0, 3),
(@id_phieu_hd, N'Trả lời câu hỏi phản biện', N'Hiểu rõ vấn đề, trả lời đúng trọng tâm và thuyết phục được hội đồng.', 0.3, 3.0, 4);

GO

-- 3.21 CẤU HÌNH PHIẾU CHẤM
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);
DECLARE @id_phieu_gvhd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%hướng dẫn%');
DECLARE @id_phieu_hd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%Hội đồng%');

INSERT INTO CauHinhPhieuCham_Dot (id_dot, vai_tro_cham, id_loai_phieu) VALUES 
(@id_dot_active, 'GVHD', @id_phieu_gvhd),
(@id_dot_active, 'HoiDong', @id_phieu_hd);

GO

-- 3.22 HỘI ĐỒNG BÁO CÁO
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

-- Đảm bảo bảng HoiDongBaoCao có cột ngay_bat_dau, ngay_ket_thuc trước khi insert (tránh lỗi khi DB cũ thiếu cột)
IF COL_LENGTH('HoiDongBaoCao', 'ngay_bat_dau') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ngay_bat_dau DATE NULL;
IF COL_LENGTH('HoiDongBaoCao', 'ngay_ket_thuc') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ngay_ket_thuc DATE NULL;

INSERT INTO HoiDongBaoCao (ma_hoi_dong, ten_hoi_dong, loai_hoi_dong, id_dot, id_nguoi_tao, id_bo_mon, ngay_bao_cao, dia_diem, thoi_gian_du_kien, trang_thai, ngay_bat_dau, ngay_ket_thuc) VALUES 
('HD_CNPM_01', N'Hội đồng tốt nghiệp Kỹ thuật phần mềm 1', 'CUOI_KY', @id_dot_active, 1, 1, '2026-05-25', N'Phòng A.05.01', '08:00:00', 1, '2026-05-25', '2026-05-25'),
('HD_AI_01', N'Hội đồng tốt nghiệp AI & Khoa học dữ liệu', 'CUOI_KY', @id_dot_active, 1, 2, '2026-05-25', N'Phòng F.10.02', '13:30:00', 1, '2026-05-25', '2026-05-25');

GO

-- 3.23 THÀNH VIÊN HỘI ĐỒNG
DECLARE @id_hd_cnpm INT = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_CNPM_01');
DECLARE @id_hd_ai INT = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_AI_01');

INSERT INTO ThanhVien_HD_BaoCao (id_hd_baocao, id_giang_vien, vai_tro) VALUES 
(@id_hd_cnpm, 1, 'CHU_TICH'),
(@id_hd_cnpm, 2, 'THU_KY'),
(@id_hd_cnpm, 5, 'PHAN_BIEN'),
(@id_hd_cnpm, 6, 'PHAN_BIEN'),
(@id_hd_cnpm, 4, 'UY_VIEN'),

(@id_hd_ai, 3, 'CHU_TICH'),
(@id_hd_ai, 10, 'THU_KY'),
(@id_hd_ai, 8, 'PHAN_BIEN'),
(@id_hd_ai, 4, 'PHAN_BIEN'),
(@id_hd_ai, 12, 'UY_VIEN');

GO

-- 3.24 BÁO CÁO NỘP
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);
DECLARE @id_dt_rfid INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_001');
DECLARE @id_dt_ddos INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_004');

INSERT INTO BaoCaoNop (id_dot, id_de_tai, id_sinh_vien, stt, ten_bao_cao, file_baocao, ngay_nop, nhan_xet, trang_thai) VALUES 
(@id_dot_active, @id_dt_rfid, 13, 1, N'Báo cáo tiến độ giữa kỳ', '/uploads/2026/HK2/DT001/midterm_report.pdf', '2026-03-20 08:00:00', N'Tiến độ tốt, đã hoàn thành phần cứng RFID.', 'DA_DUYET'),
(@id_dot_active, @id_dt_rfid, 13, 2, N'Báo cáo toàn văn Khóa luận', '/uploads/2026/HK2/DT001/final_thesis_v1.pdf', '2026-05-20 09:00:00', N'Đạt yêu cầu ra hội đồng bảo vệ.', 'DA_DUYET'),
(@id_dot_active, @id_dt_ddos, 16, 1, N'Báo cáo tiến độ giữa kỳ', '/uploads/2026/HK2/DT004/midterm_v1.pdf', '2026-03-20 14:00:00', N'Cần bổ sung thêm dữ liệu thực nghiệm.', 'YEU_CAU_SUA'),
(@id_dot_active, @id_dt_ddos, 16, 2, N'Báo cáo toàn văn Khóa luận', '/uploads/2026/HK2/DT004/final_thesis_submission.pdf', '2026-05-20 15:30:00', N'Đồng ý cho bảo vệ.', 'DA_DUYET');

GO

-- 3.25 HỘI ĐỒNG DUYỆT ĐỀ TÀI (phục vụ luồng duyệt/nhận xét đề tài)
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

-- Bảo đảm cột ngày bắt đầu/kết thúc tồn tại trước khi insert (phòng trường hợp DB cũ chưa có cột)
IF COL_LENGTH('HoiDongBaoCao', 'ngay_bat_dau') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ngay_bat_dau DATE NULL;
IF COL_LENGTH('HoiDongBaoCao', 'ngay_ket_thuc') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ngay_ket_thuc DATE NULL;

DECLARE @id_hd_duyet INT;
INSERT INTO HoiDongBaoCao (ma_hoi_dong, ten_hoi_dong, loai_hoi_dong, id_dot, id_nguoi_tao, id_bo_mon, ngay_bao_cao, dia_diem, thoi_gian_du_kien, trang_thai, ngay_bat_dau, ngay_ket_thuc)
VALUES ('HD_DUYET_DT_K27', N'Hội đồng duyệt đề tài K27', 'DUYET_DE_TAI', @id_dot_active, 1, 1, '2026-01-20', N'Văn phòng khoa', '08:00:00', 1, '2026-01-20', '2026-01-22');
SET @id_hd_duyet = SCOPE_IDENTITY();

-- 3.26 THÀNH VIÊN HỘI ĐỒNG DUYỆT
INSERT INTO ThanhVien_HD_BaoCao (id_hd_baocao, id_giang_vien, vai_tro) VALUES
(@id_hd_duyet, 1, 'CHU_TICH'),
(@id_hd_duyet, 2, 'THU_KY'),
(@id_hd_duyet, 5, 'PHAN_BIEN');

-- 3.27 NHẬN XÉT HỘI ĐỒNG DUYỆT ĐỀ TÀI
DECLARE @id_dt002 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_002');
INSERT INTO NhanXetHoiDongDeTai (id_de_tai, id_giang_vien, trang_thai, nhan_xet, ngay_tao) VALUES
(@id_dt002, 1, 'DA_DUYET', N'Đề tài đủ tính mới, đề nghị duyệt.', '2026-01-27 08:00:00'),
(@id_dt002, 2, 'DA_DUYET', N'Nội dung rõ, phạm vi khả thi.', '2026-01-27 09:00:00'),
(@id_dt002, 5, 'DA_DUYET', N'Chấp nhận cho thực hiện.', '2026-01-27 10:00:00');

-- 3.28 KẾ HOẠCH CÔNG VIỆC & NHẬT KÝ (minh chứng liên kết BaoCaoNop)
DECLARE @id_dt_rfid INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_001');
DECLARE @id_bcn_rfid_mid INT = (SELECT id FROM BaoCaoNop WHERE id_de_tai = @id_dt_rfid AND ten_bao_cao LIKE N'%tiến độ giữa kỳ%');
INSERT INTO KeHoachCongViec (
    stt, id_sinh_vien, id_dot, tuan, thu_trong_tuan, gio_bat_dau, gio_ket_thuc, gio_bat_dau_thuc_te, gio_ket_thuc_thuc_te,
    ten_cong_viec, mo_ta_cong_viec, trang_thai, id_file_minh_chung
) VALUES (
    1, 13, @id_dot_active, 8, 2, '08:00', '10:00', '08:15', '10:05',
    N'Hoàn thiện tích hợp RFID', N'Tích hợp đầu đọc RFID với module mượn trả', 'DA_HOAN_THANH', @id_bcn_rfid_mid
);

DECLARE @id_khcv INT = SCOPE_IDENTITY();
INSERT INTO NhatKyHuongDan (id_dot, ngay_hop, thoi_gian_hop, hinh_thuc_hop, dia_diem_hop, thanh_vien_tham_du, ten_gvhd, muc_tieu_buoi_hop, noi_dung_hop, action_list)
VALUES (@id_dot_active, '2026-03-05', '09:00', N'Trực tiếp', N'Phòng A.05.01', N'GVHD, SV nhóm RFID', N'TS. Nguyễn Văn A', N'Rà soát tiến độ tích hợp RFID', N'Trao đổi vướng mắc kỹ thuật, thống nhất kế hoạch kiểm thử thiết bị.', N'[{"task":"Kiểm thử đầu đọc RFID","owner":"SV","deadline":"2026-03-10"}]');

-- 3.29 THÔNG BÁO MẪU (phát sinh khi duyệt/từ chối đề tài)
INSERT INTO ThongBao (id_nguoi_nhan, tieu_de, noi_dung, link_lien_ket, trang_thai_xem, ngay_tao)
VALUES (14, N'Đề tài DT_27_002 đã được duyệt', N'Hội đồng đã duyệt đầy đủ, bạn có thể theo dõi tiếp.', '/BCNKhoa/QuanLyDeTai/Details/' + CAST(@id_dt002 AS VARCHAR(10)), 0, GETDATE());

GO

PRINT N'=====================================================================';
PRINT N'DATABASE CREATED & DATA INSERTED SUCCESSFULLY!';
PRINT N'Database: QuanLyDoAnTotNghiep';
PRINT N'Tables: 32 | Sample Data: Complete';
PRINT N'=====================================================================';

