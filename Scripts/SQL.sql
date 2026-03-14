

-- 1) Gỡ tất cả khóa ngoại
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = @sql + N'
ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) +
N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
FROM sys.foreign_keys fk
JOIN sys.tables t       ON fk.parent_object_id = t.object_id
JOIN sys.schemas s      ON t.schema_id = s.schema_id;

PRINT @sql;  -- kiểm tra trước khi EXEC
EXEC sp_executesql @sql;

-- 2) Drop toàn bộ bảng
SET @sql = N'';
SELECT @sql = @sql + N'
DROP TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N';'
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id;

PRINT @sql;  -- kiểm tra trước khi EXEC
EXEC sp_executesql @sql;




USE QuanLyDoAnTotNghiep;
GO

-- =====================================================================
-- PHẦN 1: TẠO CÁC BẢNG (CREATE TABLES)
-- =====================================================================

-- NHÓM NGƯỜI DÙNG & PHÂN QUYỀN
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
    ma_chuyen_nganh VARCHAR(20) NOT NULL UNIQUE,
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
    trang_thai BIT DEFAULT 1
);

CREATE TABLE HocKi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_hoc_ki VARCHAR(20),
    nam_bat_dau INT,
    nam_ket_thuc INT,
    ngay_bat_dau DATE,
    trang_thai BIT DEFAULT 1 
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

CREATE TABLE KhoiKienThuc (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_ctdt INT,
    ten_khoi NVARCHAR(200),
    tong_tin_chi INT,
    ghi_chu NVARCHAR(500)
);

CREATE TABLE ChiTiet_CTDT (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_ctdt INT,
    id_khoi_kien_thuc INT,
    stt INT,
    ma_hoc_phan VARCHAR(50),
    ten_hoc_phan NVARCHAR(150),
    so_tin_chi INT,
    loai_hoc_phan NVARCHAR(50),
    dieu_kien_tien_quyet NVARCHAR(255),
    hoc_ki_to_chuc INT,
    ghi_chu NVARCHAR(500)
);

CREATE TABLE MonHoc (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_khoi_kien_thuc INT,
    id_ctdt INT,
    ma_mon VARCHAR(50),
    ten_mon NVARCHAR(150),
    so_tin_chi INT,
    loai_hoc_phan NVARCHAR(50),
    dieu_kien_tien_quyet NVARCHAR(255),
    hoc_ki_to_chuc INT,
    ghi_chu NVARCHAR(500)
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

    ngay_bat_dau_dk_de_tai DATE,
    ngay_ket_thuc_dk_de_tai DATE,

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
    ma_de_tai NVARCHAR(20),
    ten_de_tai NVARCHAR(255),
    muc_tieu_chinh NVARCHAR(MAX),
    yeu_cau_tinh_moi NVARCHAR(MAX),
    pham_vi_chuc_nang NVARCHAR(MAX),
    cong_nghe_su_dung NVARCHAR(MAX),
    san_pham_ket_qua_du_kien NVARCHAR(MAX),
    nhiem_vu_cu_the NVARCHAR(MAX),

    id_nguoi_de_xuat INT,
    id_gvhd INT,

    id_dot INT,
    id_chuyen_nganh INT,

    trang_thai NVARCHAR(20) DEFAULT N'CHO_DUYET', -- tính từ bảng NhanXetHoiDongDeTai theo đủ nhận xét hội đồng
    nhan_xet_duyet NVARCHAR(MAX), --nhận xét chính nằm ở bảng NhanXetHoiDongDeTai
    nguoi_duyet INT
);

-- Nhận xét/duyệt theo từng thành viên hội đồng duyệt đề tài
CREATE TABLE NhanXetHoiDongDeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_de_tai INT NOT NULL,
    id_giang_vien INT NOT NULL, -- thành viên hội đồng
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
    trang_thai VARCHAR(20) 
);


CREATE TABLE DangKyNguyenVong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_sinh_vien INT,
    so_tin_chi_tich_luy_hien_tai INT,   
    trang_thai INT DEFAULT 0, 
    ngay_dang_ky DATETIME DEFAULT GETDATE()
);

CREATE TABLE SinhVien_DeTai (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_de_tai INT,
    id_sinh_vien INT,
    trang_thai NVARCHAR(50),
    ngay_dang_ky DATETIME DEFAULT GETDATE(),
    nhan_xet NVARCHAR(MAX),
    diem_gvhd FLOAT,              
    nhan_xet_gvhd NVARCHAR(MAX) 
);

-- NHÓM TIÊU CHÍ CHẤM
CREATE TABLE LoaiPhieuCham (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten_loai_phieu NVARCHAR(100),
    chi_nhan_xet BIT DEFAULT 0, -- 1: chỉ ghi nhận xét (không chấm điểm)
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
    ma_hoi_dong NVARCHAR(20),
    ten_hoi_dong NVARCHAR(100),
    loai_hoi_dong NVARCHAR(20), 
    id_dot INT,
    id_nguoi_tao INT,
    id_bo_mon INT,

    ngay_bao_cao DATE,
    dia_diem NVARCHAR(100),
    thoi_gian_du_kien TIME,
    trang_thai BIT,

    ngay_bat_dau DATE NULL,
    ngay_ket_thuc DATE NULL,

    trang_thai_duyet VARCHAR(20) DEFAULT 'CHO_DUYET',
    id_nguoi_duyet INT NULL,
    ngay_duyet DATETIME NULL,
    ghi_chu_duyet NVARCHAR(500) NULL
);

CREATE TABLE ThanhVien_HD_BaoCao (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_hd_baocao INT,
    id_giang_vien INT,
    vai_tro NVARCHAR(50),
    da_cham_diem BIT DEFAULT 0 -- Đánh dấu thành viên đã chấm điểm xong
);

CREATE TABLE PhienBaoVe (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_hd_baocao INT,
    id_sinh_vien_de_tai INT,
    stt_bao_cao INT,
    link_tai_lieu VARCHAR(255)
);

CREATE TABLE LichSuCapNhatDiem (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_phien_bao_ve INT NOT NULL,
    id_sinh_vien INT NOT NULL,
    id_nguoi_cap_nhat INT NOT NULL, -- GV thực hiện cập nhật (thư ký/chủ tịch)
    loai_cap_nhat VARCHAR(50), -- THU_KY_DIEU_CHINH | CHU_TICH_XAC_NHAN
    diem_cu FLOAT NULL,
    diem_moi FLOAT NULL,
    ly_do NVARCHAR(500),
    ngay_cap_nhat DATETIME DEFAULT GETDATE()
);

-- Bảng xác nhận điểm của Chủ tịch
CREATE TABLE XacNhanDiemChuTich (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_phien_bao_ve INT NOT NULL,
    id_chu_tich INT NOT NULL,
    trang_thai VARCHAR(20) DEFAULT 'CHO_XAC_NHAN', -- CHO_XAC_NHAN | DA_XAC_NHAN
    diem_tong_ket_cuoi FLOAT NULL,
    ghi_chu NVARCHAR(500),
    ngay_xac_nhan DATETIME NULL
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

-- NHÓM EMAIL & BÁO CÁO
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
    doi_tuong_nhan NVARCHAR(50),
    tieu_de_mau NVARCHAR(200),
    noi_dung_mau NVARCHAR(MAX),
    trang_thai BIT DEFAULT 1
);

CREATE TABLE LichSuGuiEmail (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_cau_hinh INT,
    nguoi_nhan NVARCHAR(100),
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
    id_dot INT NULL,           
    ngay_bat_dau DATE NULL,       
    ngay_ket_thuc DATE NULL,
    ngay_bat_dau_thuc_te DATE NULL,       
    ngay_ket_thuc_thuc_te DATE NULL,
    ten_cong_viec NVARCHAR(200),
    mo_ta_cong_viec NVARCHAR(MAX),
    trang_thai NVARCHAR(20),
    ghi_chu NVARCHAR(MAX),
    nhan_xet_gv NVARCHAR(MAX),
    nguoi_phu_trach NVARCHAR(200) NULL,
    id_file_minh_chung INT NULL
);

CREATE TABLE FileMinhChung_KeHoach (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_ke_hoach INT NOT NULL,
    id_sinh_vien INT NULL,
    ten_file NVARCHAR(255),
    duong_dan NVARCHAR(500),
    loai_file NVARCHAR(20),              -- PDF, IMAGE
    ngay_nop DATETIME
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
    trang_thai VARCHAR(20),
    loai_bao_cao VARCHAR(20),         -- DE_CUONG, GIUA_KY, CUOI_KY
    ghi_chu_gui NVARCHAR(MAX),
    ngay_sua_doi_cuoi DATETIME
);

CREATE TABLE NhatKyHuongDan (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT NULL,                   
    ngay_hop DATE,
    thoi_gian_hop TIME,
    hinh_thuc_hop NVARCHAR(50),        
    dia_diem_hop NVARCHAR(200),
    thanh_vien_tham_du NVARCHAR(MAX),
    ten_gvhd NVARCHAR(200),
    muc_tieu_buoi_hop NVARCHAR(MAX),
    noi_dung_hop NVARCHAR(MAX),
    action_list NVARCHAR(MAX)         
);

CREATE TABLE BaoCaoThongKe (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT NOT NULL,                 
    so_luong_sinh_vien INT NOT NULL,    
    so_luong_de_tai INT NOT NULL,      
    so_luong_task_tuan INT NULL,         
    ti_le_hoan_thanh FLOAT NULL,
    ngay_tinh DATETIME NOT NULL DEFAULT(getdate()),
    chi_tiet_tuan NVARCHAR(MAX) NULL    

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
ALTER TABLE ChiTiet_CTDT ADD CONSTRAINT FK_CTDT_KhoiKienThuc FOREIGN KEY (id_khoi_kien_thuc) REFERENCES KhoiKienThuc(id);
ALTER TABLE KhoiKienThuc ADD CONSTRAINT FK_KhoiKienThuc_CTDT FOREIGN KEY (id_ctdt) REFERENCES ChuongTrinhDaoTao(id);
ALTER TABLE MonHoc ADD CONSTRAINT FK_MonHoc_KhoiKienThuc FOREIGN KEY (id_khoi_kien_thuc) REFERENCES KhoiKienThuc(id);
ALTER TABLE MonHoc ADD CONSTRAINT FK_MonHoc_CTDT FOREIGN KEY (id_ctdt) REFERENCES ChuongTrinhDaoTao(id);

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
ALTER TABLE HoiDongBaoCao ADD CONSTRAINT FK_HDBC_NguoiDuyet FOREIGN KEY (id_nguoi_duyet) REFERENCES NguoiDung(id);

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

-- FK cho bảng LichSuCapNhatDiem và XacNhanDiemChuTich
ALTER TABLE LichSuCapNhatDiem ADD CONSTRAINT FK_LSCND_Phien FOREIGN KEY (id_phien_bao_ve) REFERENCES PhienBaoVe(id);
ALTER TABLE LichSuCapNhatDiem ADD CONSTRAINT FK_LSCND_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);
ALTER TABLE LichSuCapNhatDiem ADD CONSTRAINT FK_LSCND_NguoiCapNhat FOREIGN KEY (id_nguoi_cap_nhat) REFERENCES GiangVien(id_nguoi_dung);

ALTER TABLE XacNhanDiemChuTich ADD CONSTRAINT FK_XNDC_Phien FOREIGN KEY (id_phien_bao_ve) REFERENCES PhienBaoVe(id);
ALTER TABLE XacNhanDiemChuTich ADD CONSTRAINT FK_XNDC_ChuTich FOREIGN KEY (id_chu_tich) REFERENCES GiangVien(id_nguoi_dung);

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
ALTER TABLE KeHoachCongViec ADD CONSTRAINT FK_KHCV_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE KeHoachCongViec ADD CONSTRAINT FK_KHCV_MinhChung FOREIGN KEY (id_file_minh_chung) REFERENCES BaoCaoNop(id);

-- FK cho FileMinhChung_KeHoach
ALTER TABLE FileMinhChung_KeHoach ADD CONSTRAINT FK_FMCK_KeHoach FOREIGN KEY (id_ke_hoach) REFERENCES KeHoachCongViec(id) ON DELETE CASCADE;
ALTER TABLE FileMinhChung_KeHoach ADD CONSTRAINT FK_FMCK_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);

-- FK cho NhatKyHuongDan
ALTER TABLE NhatKyHuongDan ADD CONSTRAINT FK_NKHD_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);

-- FK cho BaoCaoThongKe
ALTER TABLE BaoCaoThongKe ADD CONSTRAINT FK_BCTK_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);

ALTER TABLE DonPhucKhao ADD CONSTRAINT FK_DPK_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);
ALTER TABLE DonPhucKhao ADD CONSTRAINT FK_DPK_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);




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
-- BCN Khoa (ID 1)
(N'Bùi Minh Phụng', 'phung.bm@vlu.edu.vn', 'pass123', '0901000001', NULL, N'/images/avatars/bcn1.png', 1),
-- Giảng viên (ID 2-6)
(N'Phan Thị Hồng', 'hong.pt@vlu.edu.vn', 'pass123', '0902000001', NULL, N'/images/avatars/gv1.png', 1),
(N'Đặng Đình Hòa', 'hoa.dd@vlu.edu.vn', 'pass123', '0902000002', NULL, N'/images/avatars/gv2.png', 1),
(N'Nguyễn Minh Tân', 'tan.nm@vlu.edu.vn', 'pass123', '0902000003', NULL, N'/images/avatars/gv3.png', 1),
(N'Võ Anh Tiến', 'tien.va@vlu.edu.vn', 'pass123', '0902000004', NULL, N'/images/avatars/gv4.png', 1),
(N'Huỳnh Thanh Tuấn', 'tuan.ht@vlu.edu.vn', 'pass123', '0902000005', NULL, N'/images/avatars/gv5.png', 1),
-- Bộ môn (ID 7-10) - Trưởng/Phó bộ môn cũng là giảng viên
(N'Trần Văn Hùng', 'hung.tv@vlu.edu.vn', 'pass123', '0903000001', NULL, N'/images/avatars/bm1.png', 1),
(N'Lê Thị Mai', 'mai.lt@vlu.edu.vn', 'pass123', '0903000002', NULL, N'/images/avatars/bm2.png', 1),
(N'Nguyễn Hoàng Long', 'long.nh@vlu.edu.vn', 'pass123', '0903000003', NULL, N'/images/avatars/bm3.png', 1),
(N'Phạm Văn Đức', 'duc.pv@vlu.edu.vn', 'pass123', '0903000004', NULL, N'/images/avatars/bm4.png', 1),
-- Giảng viên bổ sung (ID 11-12)
(N'Trương Minh Quang', 'quang.tm@vlu.edu.vn', 'pass123', '0902000006', NULL, N'/images/avatars/gv6.png', 1),
(N'Lý Hoàng Anh', 'anh.lh@vlu.edu.vn', 'pass123', '0902000007', NULL, N'/images/avatars/gv7.png', 1),
-- Sinh viên K27 (ID 13-22)
(N'Ngô Thị Tuyết Như', 'nhu.2174802010311@vanlanguni.vn', 'pass123', '0909311311', NULL, N'/images/avatars/sv1.png', 1),
(N'Trương Quốc Duy', 'duy.2174802010297@vanlanguni.vn', 'pass123', '0909297297', NULL, N'/images/avatars/sv2.png', 1),
(N'Nguyễn Tấn Duy', 'duy.2174802010316@vanlanguni.vn', 'pass123', '0909316316', NULL, N'/images/avatars/sv3.png', 1),
(N'Lê Anh Duy', 'duy.2174802010307@vanlanguni.vn', 'pass123', '0909307307', NULL, N'/images/avatars/sv4.png', 1),
(N'Nguyễn Thanh Liêm', 'liem.2174802010288@vanlanguni.vn', 'pass123', '0909288288', NULL, N'/images/avatars/sv5.png', 1),
(N'Vũ Huy Hoàng', 'hoang.2174802010303@vanlanguni.vn', 'pass123', '0909303303', NULL, N'/images/avatars/sv6.png', 1),
(N'Hà Hoàng Nam', 'nam.2174802010300@vanlanguni.vn', 'pass123', '0909300300', NULL, N'/images/avatars/sv7.png', 1),
(N'Lê Ngọc Phúc', 'phuc.2174802010295@vanlanguni.vn', 'pass123', '0909295295', NULL, N'/images/avatars/sv8.png', 1),
(N'Huỳnh Thị Thanh Trúc', 'truc.2174802010272@vanlanguni.vn', 'pass123', '0909272272', NULL, N'/images/avatars/sv9.png', 1),
(N'Phan Đăng Khanh', 'khanh.2174802010299@vanlanguni.vn', 'pass123', '0909299299', NULL, N'/images/avatars/sv10.png', 1);

-- 3.3 PHÂN QUYỀN
INSERT INTO NguoiDung_VaiTro (id_nguoi_dung, id_vai_tro) VALUES 
(1, 1),  -- BCN Khoa: Bùi Minh Phụng
(2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (11, 3), (12, 3),  -- Giảng viên
(7, 2), (8, 2), (9, 2), (10, 2),  -- Bộ môn (Trưởng/Phó BM)
(13, 4), (14, 4), (15, 4), (16, 4), (17, 4), (18, 4), (19, 4), (20, 4), (21, 4), (22, 4);  -- Sinh viên K27

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
('7480201', N'Công nghệ thông tin', N'CNTT', 1, '2023-01-20', NULL, NULL, 1);

-- 3.6 CHUYÊN NGÀNH
INSERT INTO ChuyenNganh (stt, ma_chuyen_nganh, ten_chuyen_nganh, ten_viet_tat, id_nguoi_tao, ngay_tao, id_nguoi_sua, ngay_sua, id_nganh, id_bo_mon) VALUES 
(1, 'CN_PM', N'Công nghệ phần mềm', N'CNPM', 1, '2023-05-01', NULL, NULL, 1, 1),
(2, 'CN_DL', N'Công nghệ dữ liệu', N'CNDL', 1, '2023-05-01', NULL, NULL, 1, 2),
(3, 'CN_AI', N'Trí tuệ nhân tạo', N'AI', 1, '2023-05-01', NULL, NULL, 1, 3),
(4, 'CN_ANMIOT', N'An ninh Mạng và IoT', N'ANM&IoT', 1, '2023-05-05', NULL, NULL, 1, 4);

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
INSERT INTO HocKi (ma_hoc_ki, nam_bat_dau, nam_ket_thuc, ngay_bat_dau, trang_thai) VALUES 
('HK1_099', 2023, 2024, '2023-08-14', 0),
('HK2_099', 2023, 2024, '2024-01-15', 0),
('HK1_100', 2024, 2025, '2024-08-12', 0),
('HK2_100', 2024, 2025, '2025-01-13', 0),
('HK1_101', 2025, 2026, '2025-08-11', 0),
('HK2_101', 2025, 2026, '2026-01-12', 1);

-- 3.9 GIẢNG VIÊN
INSERT INTO GiangVien (id_nguoi_dung, ma_gv, hoc_vi, id_bo_mon) VALUES
-- BCN Khoa (cũng là giảng viên)
(1, 'GV_001', N'Tiến sĩ', 1),       -- Bùi Minh Phụng - BCN Khoa
-- Giảng viên chính
(2, 'GV_002', N'Thạc sĩ', 1),       -- Phan Thị Hồng
(3, 'GV_003', N'Thạc sĩ', 1),       -- Đặng Đình Hòa
(4, 'GV_004', N'Thạc sĩ', 2),       -- Nguyễn Minh Tân
(5, 'GV_005', N'Thạc sĩ', 2),       -- Võ Anh Tiến
(6, 'GV_006', N'Thạc sĩ', 3),       -- Huỳnh Thanh Tuấn
-- Bộ môn (Trưởng/Phó BM)
(7, 'GV_007', N'Tiến sĩ', 1),       -- Trần Văn Hùng - Trưởng BM KTPM
(8, 'GV_008', N'Thạc sĩ', 2),       -- Lê Thị Mai - Trưởng BM HTTT
(9, 'GV_009', N'Tiến sĩ', 3),       -- Nguyễn Hoàng Long - Trưởng BM KHMT
(10, 'GV_010', N'Thạc sĩ', 4),      -- Phạm Văn Đức - Trưởng BM MMT&TT
-- Giảng viên bổ sung
(11, 'GV_011', N'Thạc sĩ', 1),      -- Trương Minh Quang
(12, 'GV_012', N'Thạc sĩ', 2);      -- Lý Hoàng Anh

-- 3.10 SINH VIÊN (10 SV K27 - Công nghệ phần mềm)
INSERT INTO SinhVien (id_nguoi_dung, mssv, id_chuyen_nganh, id_khoa_hoc, tin_chi_tich_luy) VALUES
(13, '2174802010311', 1, 3, 115.0),  -- Ngô Thị Tuyết Như
(14, '2174802010297', 1, 3, 112.5),  -- Trương Quốc Duy
(15, '2174802010316', 1, 3, 110.0),  -- Nguyễn Tấn Duy
(16, '2174802010307', 1, 3, 108.5),  -- Lê Anh Duy
(17, '2174802010288', 1, 3, 118.0),  -- Nguyễn Thanh Liêm
(18, '2174802010303', 1, 3, 105.5),  -- Vũ Huy Hoàng
(19, '2174802010300', 1, 3, 107.0),  -- Hà Hoàng Nam
(20, '2174802010295', 1, 3, 109.5),  -- Lê Ngọc Phúc
(21, '2174802010272', 1, 3, 113.0),  -- Huỳnh Thị Thanh Trúc
(22, '2174802010299', 1, 3, 111.0);  -- Phan Đăng Khanh

-- 3.11 CHƯƠNG TRÌNH ĐÀO TẠO
INSERT INTO ChuongTrinhDaoTao (ma_ctdt, ten_ctdt, stt_hien_thi, id_nganh, id_khoa_hoc, tong_tin_chi, trang_thai, ngay_tao) VALUES 
('CTDT_K27', N'Chương trình đào tạo chuẩn Khóa 27', 1, 1, 3, 130, 1, '2023-08-01'),
('CTDT_K28', N'Chương trình đào tạo chuẩn Khóa 28', 1, 1, 4, 132, 1, '2024-08-01'),
('CTDT_K29', N'Chương trình đào tạo chuẩn Khóa 29', 1, 1, 5, 135, 1, '2025-08-01'),
('CTDT_K30', N'Chương trình đào tạo chuẩn Khóa 30', 1, 1, 6, 135, 1, '2026-08-01');

GO

-- 3.12 KHỐI KIẾN THỨC & MÔN HỌC & CHI TIẾT CTDT
DECLARE @id_k27 INT = (SELECT TOP 1 id FROM ChuongTrinhDaoTao WHERE ma_ctdt = 'CTDT_K27');
DECLARE @id_k28 INT = (SELECT TOP 1 id FROM ChuongTrinhDaoTao WHERE ma_ctdt = 'CTDT_K28');

-- 3.12A Khối kiến thức
DECLARE @khoi_cb INT, @khoi_chuyen INT, @khoi_tuchon INT;
INSERT INTO KhoiKienThuc (id_ctdt, ten_khoi, tong_tin_chi, ghi_chu)
VALUES (@id_k27, N'Khối kiến thức đại cương', 18, N'Giai đoạn 1: tên khối ở cột B, tổng TC ở cột E');
SET @khoi_cb = SCOPE_IDENTITY();

INSERT INTO KhoiKienThuc (id_ctdt, ten_khoi, tong_tin_chi, ghi_chu)
VALUES (@id_k27, N'Khối chuyên ngành bắt buộc', 30, N'Giai đoạn 2 – Case A: cột A có số dấu chấm, tên khối ở cột B (bỏ ngoặc), TC ở cột E');
SET @khoi_chuyen = SCOPE_IDENTITY();

INSERT INTO KhoiKienThuc (id_ctdt, ten_khoi, tong_tin_chi, ghi_chu)
VALUES (@id_k27, N'Khối tự chọn', 4, N'Giai đoạn 2 – Case B: tên khối ở cột A (bỏ số đầu), TC cộng từ dòng kế cột B');
SET @khoi_tuchon = SCOPE_IDENTITY();

-- 3.12B Chi tiết CTDT (liên kết khối kiến thức)
INSERT INTO ChiTiet_CTDT (id_ctdt, id_khoi_kien_thuc, stt, ma_hoc_phan, ten_hoc_phan, so_tin_chi, loai_hoc_phan, dieu_kien_tien_quyet, hoc_ki_to_chuc) VALUES
(@id_k27, @khoi_cb, 1, 'COMP1001', N'Nhập môn Lập trình', 3, N'Bắt buộc', NULL, 1),
(@id_k27, @khoi_cb, 2, 'MATH1001', N'Đại số tuyến tính', 3, N'Bắt buộc', NULL, 1),
(@id_k27, @khoi_cb, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, N'Bắt buộc', N'COMP1001', 2),
(@id_k27, @khoi_cb, 4, 'MATH1002', N'Toán rời rạc', 3, N'Bắt buộc', NULL, 2),
(@id_k27, @khoi_chuyen, 5, 'COMP2001', N'Cấu trúc dữ liệu và Giải thuật', 4, N'Bắt buộc', N'COMP1002', 3),
(@id_k27, @khoi_chuyen, 6, 'COMP2002', N'Kiến trúc máy tính', 3, N'Bắt buộc', NULL, 3),
(@id_k27, @khoi_chuyen, 7, 'COMP2003', N'Cơ sở dữ liệu', 4, N'Bắt buộc', N'COMP2001', 4),
(@id_k27, @khoi_chuyen, 8, 'COMP2004', N'Mạng máy tính', 3, N'Bắt buộc', N'COMP2002', 4),
(@id_k27, @khoi_chuyen, 9, 'COMP3001', N'Lập trình Web', 3, N'Bắt buộc', N'COMP2003', 5),
(@id_k27, @khoi_chuyen, 10, 'COMP3002', N'Công nghệ phần mềm', 3, N'Bắt buộc', N'COMP2003', 5),

(@id_k28, NULL, 1, 'COMP1001', N'Nhập môn Lập trình', 3, N'Bắt buộc', NULL, 1),
(@id_k28, NULL, 2, 'MATH1001', N'Đại số tuyến tính', 3, N'Bắt buộc', NULL, 1),
(@id_k28, NULL, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, N'Bắt buộc', N'COMP1001', 2),
(@id_k28, NULL, 4, 'MATH1002', N'Toán rời rạc', 3, N'Bắt buộc', NULL, 2),
(@id_k28, NULL, 5, 'COMP2001', N'Cấu trúc dữ liệu và Giải thuật', 4, N'Bắt buộc', N'COMP1002', 3);

-- 3.12C Môn học (liên kết khối kiến thức)
INSERT INTO MonHoc (id_khoi_kien_thuc, id_ctdt, ma_mon, ten_mon, so_tin_chi, loai_hoc_phan, hoc_ki_to_chuc)
VALUES
(@khoi_cb, @id_k27, 'COMP1001', N'Nhập môn lập trình', 3, N'Bắt buộc', 1),
(@khoi_cb, @id_k27, 'MATH1001', N'Giải tích 1', 3, N'Bắt buộc', 1),
(@khoi_cb, @id_k27, 'PHYS1001', N'Vật lý đại cương', 4, N'Bắt buộc', 1),
(@khoi_chuyen, @id_k27, 'COMP2001', N'Cấu trúc dữ liệu', 4, N'Bắt buộc', 3),
(@khoi_chuyen, @id_k27, 'COMP2002', N'Kiến trúc máy tính', 3, N'Bắt buộc', 3),
(@khoi_chuyen, @id_k27, 'COMP3001', N'Lập trình Web', 3, N'Bắt buộc', 5),
(@khoi_tuchon, @id_k27, 'ELECT01', N'Chuyên đề tự chọn 1', 2, N'Tự chọn', 5),
(@khoi_tuchon, @id_k27, 'ELECT02', N'Chuyên đề tự chọn 2', 2, N'Tự chọn', 5);

-- 3.13 KẾT QUẢ HỌC TẬP
INSERT INTO KetQuaHocTap (id_sinh_vien, stt, ma_hoc_phan, ten_hoc_phan, so_tc, diem_so, diem_chu, tong_so_tin_chi, GPA, ket_qua) VALUES 
-- Ngô Thị Tuyết Như (SV 13)
(13, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 9.0, 'A', 115.0, 3.65, 1),
(13, 2, 'MATH1001', N'Đại số tuyến tính', 3, 8.5, 'A', 115.0, 3.65, 1),
(13, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 9.0, 'A', 115.0, 3.65, 1),
(13, 4, 'MATH1002', N'Toán rời rạc', 3, 8.0, 'B', 115.0, 3.65, 1),
(13, 5, 'COMP2001', N'Cấu trúc dữ liệu và Giải thuật', 4, 9.5, 'A', 115.0, 3.65, 1),

-- Trương Quốc Duy (SV 14)
(14, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 8.5, 'A', 112.5, 3.50, 1),
(14, 2, 'MATH1001', N'Đại số tuyến tính', 3, 8.0, 'B', 112.5, 3.50, 1),
(14, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.5, 'A', 112.5, 3.50, 1),
(14, 4, 'COMP2001', N'Cấu trúc dữ liệu và Giải thuật', 4, 8.0, 'B', 112.5, 3.50, 1),

-- Nguyễn Tấn Duy (SV 15)
(15, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 9.5, 'A', 110.0, 3.70, 1),
(15, 2, 'MATH1001', N'Đại số tuyến tính', 3, 9.0, 'A', 110.0, 3.70, 1),
(15, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.5, 'A', 110.0, 3.70, 1),

-- Lê Anh Duy (SV 16)
(16, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 7.5, 'B', 108.5, 3.20, 1),
(16, 2, 'MATH1001', N'Đại số tuyến tính', 3, 7.0, 'B', 108.5, 3.20, 1),
(16, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.0, 'B', 108.5, 3.20, 1),

-- Nguyễn Thanh Liêm (SV 17)
(17, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 9.5, 'A', 118.0, 3.80, 1),
(17, 2, 'MATH1001', N'Đại số tuyến tính', 3, 9.0, 'A', 118.0, 3.80, 1),
(17, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 9.5, 'A', 118.0, 3.80, 1),

-- Vũ Huy Hoàng (SV 18)
(18, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 7.0, 'B', 105.5, 3.00, 1),
(18, 2, 'MATH1001', N'Đại số tuyến tính', 3, 6.5, 'C', 105.5, 3.00, 1),
(18, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 7.5, 'B', 105.5, 3.00, 1),

-- Hà Hoàng Nam (SV 19)
(19, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 8.0, 'B', 107.0, 3.15, 1),
(19, 2, 'MATH1001', N'Đại số tuyến tính', 3, 7.5, 'B', 107.0, 3.15, 1),
(19, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.5, 'A', 107.0, 3.15, 1),

-- Lê Ngọc Phúc (SV 20)
(20, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 8.5, 'A', 109.5, 3.40, 1),
(20, 2, 'MATH1001', N'Đại số tuyến tính', 3, 8.0, 'B', 109.5, 3.40, 1),
(20, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.0, 'B', 109.5, 3.40, 1),

-- Huỳnh Thị Thanh Trúc (SV 21)
(21, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 9.0, 'A', 113.0, 3.55, 1),
(21, 2, 'MATH1001', N'Đại số tuyến tính', 3, 8.5, 'A', 113.0, 3.55, 1),
(21, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.5, 'A', 113.0, 3.55, 1),

-- Phan Đăng Khanh (SV 22)
(22, 1, 'COMP1001', N'Nhập môn Lập trình', 3, 8.0, 'B', 111.0, 3.30, 1),
(22, 2, 'MATH1001', N'Đại số tuyến tính', 3, 7.5, 'B', 111.0, 3.30, 1),
(22, 3, 'COMP1002', N'Kỹ thuật Lập trình', 3, 8.5, 'A', 111.0, 3.30, 1);

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
    ngay_bat_dau_dk_de_tai, ngay_ket_thuc_dk_de_tai,
    ngay_lap_hoi_dong_duyet_DXDT, ngay_duyet_DXDT,
    ngay_bat_dau_nop_de_cuong, ngay_ket_thuc_nop_de_cuong,
    ngay_bat_dau_bao_cao_cuoi_ki, ngay_ket_thuc_bao_cao_cuoi_ki,
    ngay_lap_HD_BCCK, ngay_nop_tai_lieu_BCCK, ngay_cong_bo_kq_BCCK,
    ngay_bat_dau_bao_cao_giua_ki, ngay_ket_thuc_bao_cao_giua_ki,
    ngay_lap_HD_BCGK, ngay_nop_tai_lieu_BCGK,
    trang_thai
) VALUES (
    N'Đợt ĐATN K27 - HK1 2025-2026', @id_k27, @id_hk_active,
    '2025-01-15', '2025-08-30',
    '2025-01-20', '2025-01-25',
    '2025-01-26', '2025-01-31',
    '2025-02-01', '2025-02-15',
    '2025-03-01', '2025-03-15',
    '2025-02-16', '2025-02-28',
    3, 5,
    '2025-03-20', '2025-04-05',
    '2025-07-01', '2025-07-20',
    7, 3, 2,
    '2025-05-15', '2025-05-25',
    5, 3,
    0
);

GO

-- 3.15 ĐỀ TÀI
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

INSERT INTO DeTai (
    ma_de_tai, ten_de_tai, muc_tieu_chinh, yeu_cau_tinh_moi, 
    pham_vi_chuc_nang, cong_nghe_su_dung, san_pham_ket_qua_du_kien, nhiem_vu_cu_the,
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
    N'1. Nghiên cứu công nghệ RFID và tích hợp SDK
2. Xây dựng module quản lý sách và độc giả
3. Phát triển chức năng mượn trả tự động
4. Thiết kế giao diện web responsive
5. Xây dựng app mobile cho độc giả tra cứu',
    13, 2, @id_dot_active, 1, 
    'DA_DUYET', N'Đề tài có tính thực tiễn cao, đồng ý cho thực hiện.', 1
),
(
    'DT_27_002', N'Ứng dụng AI trong chẩn đoán bệnh phổi qua ảnh X-Quang',
    N'Hỗ trợ bác sĩ chẩn đoán nhanh bệnh viêm phổi',
    N'Áp dụng mô hình Deep Learning mới nhất (EfficientNet)',
    N'Upload ảnh, phân tích ảnh, trả kết quả chẩn đoán, lưu hồ sơ bệnh án',
    N'Python, PyTorch, Flask, PostgreSQL',
    N'Web app chẩn đoán bệnh',
    N'1. Thu thập và tiền xử lý dữ liệu X-Quang
2. Huấn luyện mô hình EfficientNet
3. Đánh giá và tối ưu mô hình
4. Xây dựng API Flask phục vụ inference
5. Phát triển giao diện web cho bác sĩ',
    14, 4, @id_dot_active, 3, 
    'DA_DUYET', N'Đề tài có hướng tiếp cận tốt, đồng ý cho thực hiện.', 1
),
(
    'DT_27_003', N'Xây dựng ứng dụng nghe nhạc trực tuyến',
    N'Cung cấp nền tảng nghe nhạc cho giới trẻ',
    N'Không có tính mới, trùng lặp nhiều với các đồ án cũ',
    N'Nghe nhạc, tạo playlist, tìm kiếm bài hát',
    N'Flutter, Firebase',
    N'Ứng dụng Android/iOS',
    N'1. Thiết kế UI/UX cho ứng dụng
2. Xây dựng chức năng phát nhạc
3. Phát triển tính năng playlist
4. Tích hợp Firebase backend',
    15, 6, @id_dot_active, 1, 
    'TU_CHOI', N'Đề tài quá đơn giản, cần bổ sung chức năng gợi ý nhạc thông minh.', 1
),
(
    'DT_27_004', N'Hệ thống phát hiện tấn công DDoS dựa trên Machine Learning',
    N'Phát hiện và cảnh báo sớm các cuộc tấn công từ chối dịch vụ',
    N'Kết hợp thuật toán SVM và Random Forest để tăng độ chính xác',
    N'Giám sát lưu lượng mạng, phân tích gói tin, gửi cảnh báo qua Email/SMS',
    N'Python, Snort, Kibana, ElasticSearch',
    N'Phần mềm giám sát và Dashboard báo cáo',
    N'1. Nghiên cứu các loại tấn công DDoS
2. Thu thập dataset CICIDS2017
3. Huấn luyện mô hình SVM và Random Forest
4. Tích hợp Snort để bắt gói tin
5. Xây dựng Dashboard Kibana hiển thị cảnh báo
6. Phát triển module gửi thông báo Email/SMS',
    16, 5, @id_dot_active, 4, 
    'DA_DUYET', N'Đề tài phù hợp chuyên ngành, hướng đi tốt.', 7
);

GO

-- 3.16 ĐĂNG KÝ NGUYỆN VỌNG (ĐÃ ĐỔI SANG INT)
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

INSERT INTO DangKyNguyenVong (id_dot, id_sinh_vien, so_tin_chi_tich_luy_hien_tai, trang_thai, ngay_dang_ky) VALUES 
(@id_dot_active, 13, 115, 1, '2025-01-22 08:30:00'),  -- Ngô Thị Tuyết Như - Đạt
(@id_dot_active, 14, 113, 1, '2025-01-22 08:45:00'),  -- Trương Quốc Duy - Đạt
(@id_dot_active, 15, 110, 1, '2025-01-22 09:00:00'),  -- Nguyễn Tấn Duy - Đạt
(@id_dot_active, 16, 109, 1, '2025-01-22 09:15:00'),  -- Lê Anh Duy - Đạt
(@id_dot_active, 17, 118, 1, '2025-01-22 09:30:00'),  -- Nguyễn Thanh Liêm - Đạt
(@id_dot_active, 18, 106, 1, '2025-01-22 09:45:00'),  -- Vũ Huy Hoàng - Đạt
(@id_dot_active, 19, 107, 1, '2025-01-22 10:00:00'),  -- Hà Hoàng Nam - Đạt
(@id_dot_active, 20, 110, 1, '2025-01-22 10:15:00'),  -- Lê Ngọc Phúc - Đạt
(@id_dot_active, 21, 113, 1, '2025-01-22 10:30:00'),  -- Huỳnh Thị Thanh Trúc - Đạt
(@id_dot_active, 22, 111, 1, '2025-01-22 10:45:00');  -- Phan Đăng Khanh - Đạt

GO

-- 3.17 SINH VIÊN - ĐỀ TÀI
DECLARE @id_dt1 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_001');
DECLARE @id_dt2 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_002');
DECLARE @id_dt3 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_003');
DECLARE @id_dt4 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_004');

INSERT INTO SinhVien_DeTai (id_de_tai, id_sinh_vien, trang_thai, ngay_dang_ky, nhan_xet) VALUES 
-- Đề tài 1: Quản lý thư viện RFID (Nhóm: Ngô Thị Tuyết Như, Trương Quốc Duy)
(@id_dt1, 13, 'DA_DUYET', '2025-03-05 08:00:00', N'Nhóm trưởng - Đã được duyệt.'),
(@id_dt1, 14, 'DA_DUYET', '2025-03-05 08:30:00', N'Thành viên - Đã được duyệt.'),
-- Đề tài 2: AI chẩn đoán X-Quang (Nhóm: Nguyễn Tấn Duy, Lê Anh Duy)
(@id_dt2, 15, 'DA_DUYET', '2025-03-05 09:00:00', N'Nhóm trưởng - Đã được duyệt.'),
(@id_dt2, 16, 'DA_DUYET', '2025-03-05 09:30:00', N'Thành viên - Đã được duyệt.'),
-- Đề tài 3: Ứng dụng nghe nhạc (Đề tài bị từ chối)
(@id_dt3, 17, 'TU_CHOI', '2025-03-05 10:00:00', N'Đề tài không đạt yêu cầu chuyên môn.'),
(@id_dt3, 18, 'TU_CHOI', '2025-03-05 10:30:00', N'Đề tài không đạt yêu cầu chuyên môn.'),
-- Đề tài 4: Phát hiện DDoS (Nhóm: Hà Hoàng Nam, Lê Ngọc Phúc)
(@id_dt4, 19, 'DA_DUYET', '2025-03-05 11:00:00', N'Nhóm trưởng - Đã được duyệt.'),
(@id_dt4, 20, 'DA_DUYET', '2025-03-05 11:30:00', N'Thành viên - Đã được duyệt.'),
-- Sinh viên chờ duyệt
(@id_dt1, 21, N'Chờ GVHD duyệt', '2025-03-06 08:00:00', NULL),
(@id_dt2, 22, N'Chờ GVHD duyệt', '2025-03-06 08:30:00', NULL);

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
    '2025-03-25 08:00:00',
    'DA_DUYET'
),
(
    @id_dt4,
    N'Các cuộc tấn công DDoS ngày càng tinh vi, tường lửa truyền thống thường bỏ sót các mẫu tấn công mới.',
    N'Sự kết hợp giữa SVM và Random Forest sẽ cải thiện khả năng phát hiện tấn công DDoS.',
    N'Lưu lượng mạng (Network Traffic) và các đặc trưng của gói tin TCP/IP.',
    N'Tập trung phát hiện tấn công lớp ứng dụng (HTTP Flood) và lớp giao vận (SYN Flood).',
    N'Thu thập dữ liệu mẫu (Dataset CICIDS2017), tiền xử lý dữ liệu, huấn luyện mô hình.',
    '2025-03-25 09:30:00',
    'CHO_DUYET'
);

-- 3.19 LOẠI PHIẾU CHẤM
INSERT INTO LoaiPhieuCham (ten_loai_phieu, chi_nhan_xet, nguoi_tao) VALUES 
(N'Phiếu chấm giữa kì', 1, 1),
(N'Phiếu chấm của Giáo viên hướng dẫn', 0, 1),
(N'Phiếu chấm của Phản biện', 0, 1),
(N'Phiếu chấm Hội đồng bảo vệ', 0, 1);

GO

-- 3.20 TIÊU CHÍ CHẤM ĐIỂM
DECLARE @id_phieu_giuaki INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%giữa kì%');
DECLARE @id_phieu_gvhd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%hướng dẫn%');
DECLARE @id_phieu_phanbien INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%Phản biện%');
DECLARE @id_phieu_hd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%Hội đồng%');

-- Tiêu chí phiếu chấm giữa kì (chỉ nhận xét, không chấm điểm)
INSERT INTO TieuChiChamDiem (id_loai_phieu, ten_tieu_chi, mo_ta_huong_dan, trong_so, diem_toi_da, stt_hien_thi) VALUES 
(@id_phieu_giuaki, N'Tiến độ thực hiện', N'Đánh giá mức độ hoàn thành so với kế hoạch đã đăng ký trong đề cương.', NULL, NULL, 1),
(@id_phieu_giuaki, N'Phương pháp và cách tiếp cận', N'Nhận xét về phương pháp nghiên cứu, cách giải quyết vấn đề của sinh viên.', NULL, NULL, 2),
(@id_phieu_giuaki, N'Kết quả sơ bộ đạt được', N'Nhận xét về các kết quả, sản phẩm sơ bộ đã thực hiện được đến thời điểm báo cáo.', NULL, NULL, 3),
(@id_phieu_giuaki, N'Thái độ và tinh thần làm việc', N'Đánh giá sự chuyên cần, chủ động liên hệ GVHD, tuân thủ tiến độ.', NULL, NULL, 4),
(@id_phieu_giuaki, N'Đề xuất và góp ý', N'Ghi nhận xét, góp ý hướng cải thiện cho giai đoạn tiếp theo.', NULL, NULL, 5),

-- Tiêu chí phiếu chấm GVHD (có chấm điểm)
(@id_phieu_gvhd, N'Tinh thần và thái độ làm việc', N'Đánh giá sự chuyên cần, chủ động gặp gỡ GVHD và tuân thủ tiến độ.', 10, 1.0, 1),
(@id_phieu_gvhd, N'Kỹ năng giải quyết vấn đề', N'Khả năng tự nghiên cứu, tìm tòi công nghệ và xử lý các bài toán kỹ thuật.', 20, 2.0, 2),
(@id_phieu_gvhd, N'Kết quả thực hiện sản phẩm', N'Sản phẩm chạy ổn định, đáp ứng đủ các chức năng đã đăng ký trong đề cương.', 40, 4.0, 3),
(@id_phieu_gvhd, N'Chất lượng báo cáo thuyết minh', N'Trình bày đúng quy định, văn phong khoa học, nội dung logic.', 30, 3.0, 4),

-- Tiêu chí phiếu chấm Phản biện (có chấm điểm)
(@id_phieu_phanbien, N'Hình thức và bố cục thuyết minh', N'Bố cục hợp lý, trình bày đẹp, đúng format chuẩn của khoa.', 10, 1.0, 1),
(@id_phieu_phanbien, N'Nội dung và kết quả đạt được', N'Đánh giá tính hoàn thiện, tính thực tiễn và độ phức tạp của đề tài.', 40, 4.0, 2),
(@id_phieu_phanbien, N'Tính mới và sáng tạo', N'Đánh giá mức độ sáng tạo, tính mới so với các công trình tương tự.', 20, 2.0, 3),
(@id_phieu_phanbien, N'Câu hỏi phản biện', N'Đánh giá khả năng trả lời câu hỏi phản biện của sinh viên.', 30, 3.0, 4),

-- Tiêu chí phiếu chấm Hội đồng bảo vệ (có chấm điểm)
(@id_phieu_hd, N'Hình thức và bố cục thuyết minh', N'Bố cục hợp lý, trình bày đẹp, đúng format chuẩn của khoa.', 10, 1.0, 1),
(@id_phieu_hd, N'Nội dung và kết quả đạt được', N'Đánh giá tính hoàn thiện, tính thực tiễn và độ phức tạp của đề tài.', 40, 4.0, 2),
(@id_phieu_hd, N'Kỹ năng trình bày và Demo', N'Trình bày tự tin, rõ ràng, Demo sản phẩm chạy tốt tại buổi bảo vệ.', 20, 2.0, 3),
(@id_phieu_hd, N'Trả lời câu hỏi phản biện', N'Hiểu rõ vấn đề, trả lời đúng trọng tâm và thuyết phục được hội đồng.', 30, 3.0, 4);

GO

-- 3.21 CẤU HÌNH PHIẾU CHẤM
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);
DECLARE @id_phieu_giuaki INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%giữa kì%');
DECLARE @id_phieu_gvhd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%hướng dẫn%');
DECLARE @id_phieu_phanbien INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%Phản biện%');
DECLARE @id_phieu_hd INT = (SELECT TOP 1 id FROM LoaiPhieuCham WHERE ten_loai_phieu LIKE N'%Hội đồng%');

INSERT INTO CauHinhPhieuCham_Dot (id_dot, vai_tro_cham, id_loai_phieu) VALUES 
(@id_dot_active, 'GVHD_GIUA_KY', @id_phieu_giuaki),
(@id_dot_active, 'GVHD', @id_phieu_gvhd),
(@id_dot_active, 'PhanBien', @id_phieu_phanbien),
(@id_dot_active, 'HoiDong', @id_phieu_hd);

GO

-- 3.22 HỘI ĐỒNG BÁO CÁO
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

-- Đảm bảo bảng HoiDongBaoCao có các cột mới trước khi insert
IF COL_LENGTH('HoiDongBaoCao', 'ngay_bat_dau') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ngay_bat_dau DATE NULL;
IF COL_LENGTH('HoiDongBaoCao', 'ngay_ket_thuc') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ngay_ket_thuc DATE NULL;
IF COL_LENGTH('HoiDongBaoCao', 'trang_thai_duyet') IS NULL
    ALTER TABLE HoiDongBaoCao ADD trang_thai_duyet VARCHAR(20) DEFAULT 'CHO_DUYET';
IF COL_LENGTH('HoiDongBaoCao', 'id_nguoi_duyet') IS NULL
    ALTER TABLE HoiDongBaoCao ADD id_nguoi_duyet INT NULL;
IF COL_LENGTH('HoiDongBaoCao', 'ngay_duyet') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ngay_duyet DATETIME NULL;
IF COL_LENGTH('HoiDongBaoCao', 'ghi_chu_duyet') IS NULL
    ALTER TABLE HoiDongBaoCao ADD ghi_chu_duyet NVARCHAR(500) NULL;

-- Đảm bảo bảng ThanhVien_HD_BaoCao có cột da_cham_diem
IF COL_LENGTH('ThanhVien_HD_BaoCao', 'da_cham_diem') IS NULL
    ALTER TABLE ThanhVien_HD_BaoCao ADD da_cham_diem BIT DEFAULT 0;

INSERT INTO HoiDongBaoCao (ma_hoi_dong, ten_hoi_dong, loai_hoi_dong, id_dot, id_nguoi_tao, id_bo_mon, ngay_bao_cao, dia_diem, thoi_gian_du_kien, trang_thai, ngay_bat_dau, ngay_ket_thuc, trang_thai_duyet) VALUES 
('HD_CNPM_01', N'Hội đồng tốt nghiệp Kỹ thuật phần mềm 1', 'CUOI_KY', @id_dot_active, 1, 1, '2025-07-15', N'Phòng A.05.01', '08:00:00', 1, '2025-07-15', '2025-07-15', 'CHO_DUYET'),
('HD_AI_01', N'Hội đồng tốt nghiệp AI & Khoa học dữ liệu', 'CUOI_KY', @id_dot_active, 1, 2, '2025-07-15', N'Phòng F.10.02', '13:30:00', 1, '2025-07-15', '2025-07-15', 'CHO_DUYET');

GO

-- 3.23 THÀNH VIÊN HỘI ĐỒNG
DECLARE @id_hd_cnpm INT = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_CNPM_01');
DECLARE @id_hd_ai INT = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_AI_01');

INSERT INTO ThanhVien_HD_BaoCao (id_hd_baocao, id_giang_vien, vai_tro) VALUES 
-- Hội đồng CNPM: Chủ tịch - BCN Khoa, TV - các GV
(@id_hd_cnpm, 1, 'CHU_TICH'),   -- Bùi Minh Phụng (BCN Khoa)
(@id_hd_cnpm, 7, 'THU_KY'),     -- Trần Văn Hùng (Trưởng BM KTPM)
(@id_hd_cnpm, 2, 'PHAN_BIEN'),  -- Phan Thị Hồng
(@id_hd_cnpm, 3, 'PHAN_BIEN'),  -- Đặng Đình Hòa
(@id_hd_cnpm, 11, 'UY_VIEN'),   -- Trương Minh Quang

-- Hội đồng AI: Chủ tịch - Trưởng BM, TV - các GV
(@id_hd_ai, 9, 'CHU_TICH'),     -- Nguyễn Hoàng Long (Trưởng BM KHMT)
(@id_hd_ai, 8, 'THU_KY'),       -- Lê Thị Mai (Trưởng BM HTTT)
(@id_hd_ai, 4, 'PHAN_BIEN'),    -- Nguyễn Minh Tân
(@id_hd_ai, 5, 'PHAN_BIEN'),    -- Võ Anh Tiến
(@id_hd_ai, 12, 'UY_VIEN');     -- Lý Hoàng Anh

GO

-- 3.24 BÁO CÁO NỘP
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);
DECLARE @id_dt_rfid INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_001');
DECLARE @id_dt_ddos INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_004');

INSERT INTO BaoCaoNop (id_dot, id_de_tai, id_sinh_vien, stt, ten_bao_cao, file_baocao, ngay_nop, nhan_xet, trang_thai, loai_bao_cao, ghi_chu_gui, ngay_sua_doi_cuoi) VALUES 
-- SV 13 (đề tài RFID): Đề cương đã duyệt, Giữa kỳ đã duyệt, Cuối kỳ chờ duyệt
(@id_dot_active, @id_dt_rfid, 13, 1, N'Đề cương đồ án', '/uploads/2025/HK2/DT001/de_cuong_v1.pdf', '2025-04-01 08:00:00', N'Đề cương đạt yêu cầu.', 'DA_DUYET', 'DE_CUONG', N'Em nộp đề cương phiên bản cuối.', '2025-04-01 08:00:00'),
(@id_dot_active, @id_dt_rfid, 13, 2, N'Báo cáo giữa kỳ', '/uploads/2025/HK2/DT001/midterm_report.pdf', '2025-05-20 08:00:00', N'Tiến độ tốt, đã hoàn thành phần cứng RFID.', 'DA_DUYET', 'GIUA_KY', N'Báo cáo tiến độ giữa kỳ.', '2025-05-20 08:00:00'),
(@id_dot_active, @id_dt_rfid, 13, 3, N'Báo cáo cuối kỳ', '/uploads/2025/HK2/DT001/final_thesis_v1.pdf', '2025-07-05 09:00:00', NULL, 'CHO_DUYET', 'CUOI_KY', N'Nộp báo cáo toàn văn.', '2025-07-05 09:00:00'),
-- SV 14 (Trương Quốc Duy - đề tài RFID)
(@id_dot_active, @id_dt_rfid, 14, 1, N'Đề cương đồ án', '/uploads/2025/HK2/DT001/de_cuong_v2.pdf', '2025-04-01 09:00:00', N'Đề cương đạt yêu cầu.', 'DA_DUYET', 'DE_CUONG', N'Đề cương phiên bản cuối.', '2025-04-01 09:00:00'),
(@id_dot_active, @id_dt_rfid, 14, 2, N'Báo cáo giữa kỳ', '/uploads/2025/HK2/DT001/midterm_report_v2.pdf', '2025-05-20 09:00:00', N'Tiến độ tốt.', 'DA_DUYET', 'GIUA_KY', N'Báo cáo giữa kỳ.', '2025-05-20 09:00:00'),
-- SV 19 (Hà Hoàng Nam - đề tài DDoS)
(@id_dot_active, @id_dt_ddos, 19, 1, N'Đề cương đồ án', '/uploads/2025/HK2/DT004/de_cuong_v1.pdf', '2025-04-02 10:00:00', N'Chấp nhận.', 'DA_DUYET', 'DE_CUONG', NULL, '2025-04-02 10:00:00'),
(@id_dot_active, @id_dt_ddos, 19, 2, N'Báo cáo giữa kỳ', '/uploads/2025/HK2/DT004/midterm_v1.pdf', '2025-05-20 14:00:00', N'Cần bổ sung thêm dữ liệu thực nghiệm.', 'TU_CHOI', 'GIUA_KY', N'Báo cáo giữa kỳ lần 1.', '2025-05-20 14:00:00'),
(@id_dot_active, @id_dt_ddos, 19, 3, N'Báo cáo cuối kỳ', NULL, NULL, NULL, 'CHUA_NOP', 'CUOI_KY', NULL, NULL),
-- SV 20 (Lê Ngọc Phúc - đề tài DDoS)
(@id_dot_active, @id_dt_ddos, 20, 1, N'Đề cương đồ án', '/uploads/2025/HK2/DT004/de_cuong_v2.pdf', '2025-04-02 11:00:00', N'Chấp nhận.', 'DA_DUYET', 'DE_CUONG', NULL, '2025-04-02 11:00:00');

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
VALUES ('HD_DUYET_DT_K27', N'Hội đồng duyệt đề tài K27', 'DUYET_DE_TAI', @id_dot_active, 1, 1, '2025-03-05', N'Văn phòng khoa', '08:00:00', 1, '2025-03-05', '2025-03-07');
SET @id_hd_duyet = SCOPE_IDENTITY();

-- 3.26 THÀNH VIÊN HỘI ĐỒNG DUYỆT
INSERT INTO ThanhVien_HD_BaoCao (id_hd_baocao, id_giang_vien, vai_tro) VALUES
(@id_hd_duyet, 1, 'CHU_TICH'),   -- Bùi Minh Phụng (BCN Khoa)
(@id_hd_duyet, 7, 'THU_KY'),     -- Trần Văn Hùng (Trưởng BM KTPM)
(@id_hd_duyet, 3, 'PHAN_BIEN'); -- Đặng Đình Hòa

-- 3.27 NHẬN XÉT HỘI ĐỒNG DUYỆT ĐỀ TÀI
DECLARE @id_dt002 INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_002');
INSERT INTO NhanXetHoiDongDeTai (id_de_tai, id_giang_vien, trang_thai, nhan_xet, ngay_tao) VALUES
(@id_dt002, 1, 'DA_DUYET', N'Đề tài đủ tính mới, đề nghị duyệt.', '2025-03-07 08:00:00'),
(@id_dt002, 7, 'DA_DUYET', N'Nội dung rõ, phạm vi khả thi.', '2025-03-07 09:00:00'),
(@id_dt002, 3, 'DA_DUYET', N'Chấp nhận cho thực hiện.', '2025-03-07 10:00:00');

-- 3.28 KẾ HOẠCH CÔNG VIỆC & NHẬT KÝ (minh chứng liên kết BaoCaoNop)
DECLARE @id_dt_rfid INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_001');
DECLARE @id_dt_ai INT = (SELECT id FROM DeTai WHERE ma_de_tai = 'DT_27_002');
DECLARE @id_bcn_rfid_mid INT = (SELECT id FROM BaoCaoNop WHERE id_de_tai = @id_dt_rfid AND ten_bao_cao LIKE N'%tiến độ giữa kỳ%');

-- Kế hoạch SV 13 (Ngô Thị Tuyết Như - đề tài RFID)
INSERT INTO KeHoachCongViec (
    stt, id_sinh_vien, id_dot, ngay_bat_dau, ngay_ket_thuc, ngay_bat_dau_thuc_te, ngay_ket_thuc_thuc_te,
    ten_cong_viec, mo_ta_cong_viec, trang_thai, ghi_chu, nguoi_phu_trach, id_file_minh_chung
) VALUES (
    1, 13, @id_dot_active, '2025-04-10', '2025-04-25', '2025-04-11', '2025-04-24',
    N'Hoàn thiện tích hợp RFID', N'Tích hợp đầu đọc RFID với module mượn trả', N'Đã duyệt', NULL, N'2174802010311 - Ngô Thị Tuyết Như', @id_bcn_rfid_mid
),
(
    2, 13, @id_dot_active, '2025-04-26', '2025-05-10', NULL, NULL,
    N'Viết tài liệu hướng dẫn sử dụng', N'Soạn tài liệu User Guide cho hệ thống RFID', N'Chưa thực hiện', NULL, N'2174802010311 - Ngô Thị Tuyết Như', NULL
);

-- Kế hoạch SV 14 (Trương Quốc Duy - đề tài RFID)
INSERT INTO KeHoachCongViec (
    stt, id_sinh_vien, id_dot, ngay_bat_dau, ngay_ket_thuc, ngay_bat_dau_thuc_te, ngay_ket_thuc_thuc_te,
    ten_cong_viec, mo_ta_cong_viec, trang_thai, ghi_chu, nguoi_phu_trach, id_file_minh_chung
) VALUES (
    1, 14, @id_dot_active, '2025-04-10', '2025-04-30', '2025-04-12', '2025-04-28',
    N'Thiết kế giao diện Web RFID', N'Xây dựng giao diện quản lý sách và độc giả', N'Đã duyệt', N'Giao diện đã hoàn thành.', N'2174802010297 - Trương Quốc Duy', NULL
),
(
    2, 14, @id_dot_active, '2025-05-01', '2025-05-20', '2025-05-02', '2025-05-18',
    N'Phát triển API Backend', N'Xây dựng API .NET Core cho hệ thống', N'Chờ GV duyệt', N'API đã hoàn thành 90%.', N'2174802010297 - Trương Quốc Duy', NULL
);

-- Kế hoạch SV 15 (Nguyễn Tấn Duy - đề tài AI X-Quang)
INSERT INTO KeHoachCongViec (
    stt, id_sinh_vien, id_dot, ngay_bat_dau, ngay_ket_thuc, ngay_bat_dau_thuc_te, ngay_ket_thuc_thuc_te,
    ten_cong_viec, mo_ta_cong_viec, trang_thai, ghi_chu, nguoi_phu_trach, id_file_minh_chung
) VALUES (
    1, 15, @id_dot_active, '2025-04-10', '2025-04-30', '2025-04-12', '2025-04-28',
    N'Thu thập và tiền xử lý dữ liệu X-Quang', N'Download dataset ChestXray14, lọc ảnh viêm phổi, augmentation dữ liệu.', N'Đã duyệt', N'Dữ liệu đã được chuẩn hóa 224x224.', N'2174802010316 - Nguyễn Tấn Duy', NULL
),
(
    2, 15, @id_dot_active, '2025-05-01', '2025-05-20', '2025-05-02', '2025-05-18',
    N'Huấn luyện mô hình EfficientNet', N'Fine-tune EfficientNet-B3 trên tập dữ liệu phổi, đánh giá accuracy/F1.', N'Chờ GV duyệt', N'Đạt accuracy 94.2% trên tập test.', N'2174802010316 - Nguyễn Tấn Duy', NULL
),
(
    3, 15, @id_dot_active, '2025-05-21', '2025-06-10', NULL, NULL,
    N'Xây dựng giao diện web Flask', N'Tạo trang upload ảnh X-Quang, hiển thị kết quả chẩn đoán, lưu hồ sơ.', N'Đang thực hiện', NULL, N'2174802010316 - Nguyễn Tấn Duy', NULL
),
(
    4, 15, @id_dot_active, '2025-06-11', '2025-06-30', NULL, NULL,
    N'Viết báo cáo cuối kỳ', N'Soạn toàn bộ báo cáo đồ án tốt nghiệp theo mẫu khoa.', N'Chưa thực hiện', NULL, N'2174802010316 - Nguyễn Tấn Duy', NULL
);

DECLARE @id_khcv INT = SCOPE_IDENTITY();
INSERT INTO NhatKyHuongDan (id_dot, ngay_hop, thoi_gian_hop, hinh_thuc_hop, dia_diem_hop, thanh_vien_tham_du, ten_gvhd, muc_tieu_buoi_hop, noi_dung_hop, action_list)
VALUES (@id_dot_active, '2025-04-15', '09:00', N'Trực tiếp', N'Phòng A.05.01', N'GVHD, SV nhóm RFID', N'ThS. Phan Thị Hồng', N'Rà soát tiến độ tích hợp RFID', N'Trao đổi vướng mắc kỹ thuật, thống nhất kế hoạch kiểm thử thiết bị.', N'[{"task":"Kiểm thử đầu đọc RFID","owner":"Ngô Thị Tuyết Như","deadline":"2025-04-25"}]');

-- 3.29 THÔNG BÁO MẪU (phát sinh khi duyệt/từ chối đề tài)
INSERT INTO ThongBao (id_nguoi_nhan, tieu_de, noi_dung, link_lien_ket, trang_thai_xem, ngay_tao)
VALUES (15, N'Đề tài DT_27_002 đã được duyệt', N'Hội đồng đã duyệt đầy đủ, bạn có thể theo dõi tiếp.', '/BCNKhoa/QuanLyDeTai/Details/' + CAST(@id_dt002 AS VARCHAR(10)), 0, GETDATE());

GO

-- 3.30 HỘI ĐỒNG GIỮA KỲ + PHIÊN BẢO VỆ + ĐIỂM CHI TIẾT + KẾT QUẢ BẢO VỆ
DECLARE @id_dot_active INT = (SELECT TOP 1 id FROM DotDoAn WHERE trang_thai = 1);

-- Hội đồng giữa kỳ
DECLARE @id_hd_giuaky INT;
INSERT INTO HoiDongBaoCao (ma_hoi_dong, ten_hoi_dong, loai_hoi_dong, id_dot, id_nguoi_tao, id_bo_mon, ngay_bao_cao, dia_diem, thoi_gian_du_kien, trang_thai, ngay_bat_dau, ngay_ket_thuc)
VALUES ('HD_GK_CNPM_01', N'Hội đồng báo cáo giữa kỳ KTPM', 'GIUA_KY', @id_dot_active, 1, 1, '2025-05-25', N'Phòng A.05.01', '08:00:00', 1, '2025-05-25', '2025-05-25');
SET @id_hd_giuaky = SCOPE_IDENTITY();

-- Thành viên hội đồng giữa kỳ
INSERT INTO ThanhVien_HD_BaoCao (id_hd_baocao, id_giang_vien, vai_tro) VALUES
(@id_hd_giuaky, 7, 'CHU_TICH'),   -- Trần Văn Hùng (Trưởng BM KTPM)
(@id_hd_giuaky, 2, 'THU_KY'),     -- Phan Thị Hồng
(@id_hd_giuaky, 3, 'PHAN_BIEN'); -- Đặng Đình Hòa

-- Phiên bảo vệ cho SV 13 (Ngô Thị Tuyết Như - đề tài RFID) — giữa kỳ
DECLARE @id_svdt_rfid INT = (SELECT TOP 1 id FROM SinhVien_DeTai WHERE id_sinh_vien = 13 AND trang_thai = 'DA_DUYET');
DECLARE @id_pbv_gk INT;
INSERT INTO PhienBaoVe (id_hd_baocao, id_sinh_vien_de_tai, stt_bao_cao, link_tai_lieu)
VALUES (@id_hd_giuaky, @id_svdt_rfid, 1, '/uploads/2025/HK2/DT001/midterm_report.pdf');
SET @id_pbv_gk = SCOPE_IDENTITY();

-- Phiên bảo vệ cho SV 13 — cuối kỳ (hội đồng HD_CNPM_01)
DECLARE @id_hd_cuoiky INT = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_CNPM_01');
DECLARE @id_pbv_ck INT;
INSERT INTO PhienBaoVe (id_hd_baocao, id_sinh_vien_de_tai, stt_bao_cao, link_tai_lieu)
VALUES (@id_hd_cuoiky, @id_svdt_rfid, 1, '/uploads/2025/HK2/DT001/final_thesis_v1.pdf');
SET @id_pbv_ck = SCOPE_IDENTITY();

-- Phiên bảo vệ cho SV 19 (Hà Hoàng Nam - đề tài DDoS) — cuối kỳ (hội đồng HD_AI_01)
DECLARE @id_svdt_ddos INT = (SELECT TOP 1 id FROM SinhVien_DeTai WHERE id_sinh_vien = 19 AND trang_thai = 'DA_DUYET');
DECLARE @id_hd_ai INT = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_AI_01');
DECLARE @id_pbv_ck2 INT;
INSERT INTO PhienBaoVe (id_hd_baocao, id_sinh_vien_de_tai, stt_bao_cao, link_tai_lieu)
VALUES (@id_hd_ai, @id_svdt_ddos, 1, '/uploads/2025/HK2/DT004/final_thesis_submission.pdf');
SET @id_pbv_ck2 = SCOPE_IDENTITY();

-- Lấy ID tiêu chí phiếu GVHD và Hội đồng
DECLARE @tc_gvhd_1 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%Tinh thần%');
DECLARE @tc_gvhd_2 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%Kỹ năng giải quyết%');
DECLARE @tc_gvhd_3 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%Kết quả thực hiện%');
DECLARE @tc_gvhd_4 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%Chất lượng báo cáo%');

DECLARE @tc_hd_1 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%Hình thức%');
DECLARE @tc_hd_2 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%Nội dung và kết quả%');
DECLARE @tc_hd_3 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%trình bày và Demo%');
DECLARE @tc_hd_4 INT = (SELECT TOP 1 id FROM TieuChiChamDiem WHERE ten_tieu_chi LIKE N'%phản biện%');

-- Điểm chi tiết — SV 13 (Ngô Thị Tuyết Như), giữa kỳ (GV 7 - Trần Văn Hùng chấm)
INSERT INTO DiemChiTiet (id_phien_bao_ve, id_nguoi_cham, id_sinh_vien, id_tieu_chi, diem_so, nhan_xet) VALUES
(@id_pbv_gk, 7, 13, @tc_hd_1, 0.8, N'Bố cục tốt'),
(@id_pbv_gk, 7, 13, @tc_hd_2, 3.5, N'Nội dung đầy đủ, cần bổ sung demo'),
(@id_pbv_gk, 7, 13, @tc_hd_3, 1.6, N'Trình bày rõ ràng'),
(@id_pbv_gk, 7, 13, @tc_hd_4, 2.4, N'Trả lời tốt');

-- Điểm chi tiết — SV 13 (Ngô Thị Tuyết Như), cuối kỳ (GV 1 - Bùi Minh Phụng chấm)
INSERT INTO DiemChiTiet (id_phien_bao_ve, id_nguoi_cham, id_sinh_vien, id_tieu_chi, diem_so, nhan_xet) VALUES
(@id_pbv_ck, 1, 13, @tc_hd_1, 0.9, N'Hình thức chuẩn'),
(@id_pbv_ck, 1, 13, @tc_hd_2, 3.8, N'Sản phẩm hoàn thiện, demo tốt'),
(@id_pbv_ck, 1, 13, @tc_hd_3, 1.8, N'Tự tin, demo mượt'),
(@id_pbv_ck, 1, 13, @tc_hd_4, 2.7, N'Trả lời chính xác, thuyết phục');

-- Điểm chi tiết — SV 19 (Hà Hoàng Nam), cuối kỳ (GV 9 - Nguyễn Hoàng Long chấm)
INSERT INTO DiemChiTiet (id_phien_bao_ve, id_nguoi_cham, id_sinh_vien, id_tieu_chi, diem_so, nhan_xet) VALUES
(@id_pbv_ck2, 9, 19, @tc_hd_1, 0.7, N'Cần cải thiện format'),
(@id_pbv_ck2, 9, 19, @tc_hd_2, 3.0, N'Kết quả thực nghiệm đạt yêu cầu'),
(@id_pbv_ck2, 9, 19, @tc_hd_3, 1.5, N'Demo ổn'),
(@id_pbv_ck2, 9, 19, @tc_hd_4, 2.0, N'Cần chuẩn bị kỹ hơn');

-- Kết quả bảo vệ — SV 13 (Ngô Thị Tuyết Như), giữa kỳ
INSERT INTO KetQuaBaoVe_SinhVien (id_phien_bao_ve, id_sinh_vien, diem_tong_ket, diem_chu, ket_qua)
VALUES (@id_pbv_gk, 13, 8.3, 'B+', 'DAT');

-- Kết quả bảo vệ — SV 13 (Ngô Thị Tuyết Như), cuối kỳ
INSERT INTO KetQuaBaoVe_SinhVien (id_phien_bao_ve, id_sinh_vien, diem_tong_ket, diem_chu, ket_qua)
VALUES (@id_pbv_ck, 13, 9.2, 'A', 'DAT');

-- Kết quả bảo vệ — SV 19 (Hà Hoàng Nam), cuối kỳ
INSERT INTO KetQuaBaoVe_SinhVien (id_phien_bao_ve, id_sinh_vien, diem_tong_ket, diem_chu, ket_qua)
VALUES (@id_pbv_ck2, 19, 7.2, 'B', 'DAT');

-- 3.31 CẬP NHẬT TRẠNG THÁI DUYỆT HỘI ĐỒNG
-- Cập nhật hội đồng cuối kỳ CNPM thành đã duyệt
UPDATE HoiDongBaoCao 
SET trang_thai_duyet = 'DA_DUYET', 
    id_nguoi_duyet = 1, 
    ngay_duyet = '2025-07-10 10:00:00',
    ghi_chu_duyet = N'Hội đồng đủ thành viên, đã duyệt'
WHERE ma_hoi_dong = 'HD_CNPM_01';

-- Cập nhật hội đồng AI thành đã duyệt  
UPDATE HoiDongBaoCao 
SET trang_thai_duyet = 'DA_DUYET', 
    id_nguoi_duyet = 1, 
    ngay_duyet = '2025-07-10 10:30:00',
    ghi_chu_duyet = N'Hội đồng đủ thành viên, đã duyệt'
WHERE ma_hoi_dong = 'HD_AI_01';

-- Cập nhật hội đồng giữa kỳ thành đã duyệt
UPDATE HoiDongBaoCao 
SET trang_thai_duyet = 'DA_DUYET', 
    id_nguoi_duyet = 1, 
    ngay_duyet = '2025-05-20 08:00:00',
    ghi_chu_duyet = N'Đã duyệt hội đồng giữa kỳ'
WHERE ma_hoi_dong = 'HD_GK_CNPM_01';

-- 3.32 CẬP NHẬT TRẠNG THÁI ĐÃ CHẤM ĐIỂM CHO THÀNH VIÊN HỘI ĐỒNG
-- Cập nhật thành viên đã chấm điểm cho hội đồng giữa kỳ
UPDATE ThanhVien_HD_BaoCao SET da_cham_diem = 1 
WHERE id_hd_baocao = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_GK_CNPM_01')
  AND id_giang_vien = 7;

-- Cập nhật thành viên đã chấm điểm cho hội đồng cuối kỳ CNPM
UPDATE ThanhVien_HD_BaoCao SET da_cham_diem = 1 
WHERE id_hd_baocao = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_CNPM_01')
  AND id_giang_vien = 1;

-- Cập nhật thành viên đã chấm điểm cho hội đồng AI
UPDATE ThanhVien_HD_BaoCao SET da_cham_diem = 1 
WHERE id_hd_baocao = (SELECT id FROM HoiDongBaoCao WHERE ma_hoi_dong = 'HD_AI_01')
  AND id_giang_vien = 9;

-- 3.33 XÁC NHẬN ĐIỂM CHỦ TỊCH (cho các phiên đã hoàn thành)
-- Phiên giữa kỳ SV 13 - Chủ tịch (GV 7) xác nhận
INSERT INTO XacNhanDiemChuTich (id_phien_bao_ve, id_chu_tich, trang_thai, diem_tong_ket_cuoi, ghi_chu, ngay_xac_nhan)
VALUES (@id_pbv_gk, 7, 'DA_XAC_NHAN', 8.3, N'Sinh viên hoàn thành tốt giai đoạn giữa kỳ', '2025-05-25 11:00:00');

-- Phiên cuối kỳ SV 13 - Chủ tịch (GV 1) xác nhận
INSERT INTO XacNhanDiemChuTich (id_phien_bao_ve, id_chu_tich, trang_thai, diem_tong_ket_cuoi, ghi_chu, ngay_xac_nhan)
VALUES (@id_pbv_ck, 1, 'DA_XAC_NHAN', 9.2, N'Đề tài xuất sắc, sản phẩm hoàn thiện', '2025-07-15 11:30:00');

-- 3.34 LỊCH SỬ CẬP NHẬT ĐIỂM (mẫu điều chỉnh điểm)
-- Thư ký điều chỉnh điểm phiên giữa kỳ
INSERT INTO LichSuCapNhatDiem (id_phien_bao_ve, id_sinh_vien, id_nguoi_cap_nhat, loai_cap_nhat, diem_cu, diem_moi, ly_do, ngay_cap_nhat)
VALUES (@id_pbv_gk, 13, 2, 'THU_KY_DIEU_CHINH', 8.1, 8.3, N'Điều chỉnh sau khi thống nhất các thành viên hội đồng', '2025-05-25 10:45:00');

-- Chủ tịch xác nhận điểm cuối kỳ
INSERT INTO LichSuCapNhatDiem (id_phien_bao_ve, id_sinh_vien, id_nguoi_cap_nhat, loai_cap_nhat, diem_cu, diem_moi, ly_do, ngay_cap_nhat)
VALUES (@id_pbv_ck, 13, 1, 'CHU_TICH_XAC_NHAN', NULL, 9.2, N'Xác nhận điểm tổng kết cuối cùng', '2025-07-15 11:30:00');

-- Thư ký điều chỉnh điểm phiên cuối kỳ AI (có chênh lệch)
INSERT INTO LichSuCapNhatDiem (id_phien_bao_ve, id_sinh_vien, id_nguoi_cap_nhat, loai_cap_nhat, diem_cu, diem_moi, ly_do, ngay_cap_nhat)
VALUES (@id_pbv_ck2, 19, 8, 'THU_KY_DIEU_CHINH', 7.0, 7.2, N'Thống nhất điểm sau đối thoại với GVHD', '2025-07-15 15:00:00');

GO

PRINT N'=====================================================================';
PRINT N'DATABASE CREATED & DATA INSERTED SUCCESSFULLY!';
PRINT N'Database: QuanLyDoAnTotNghiep';
PRINT N'Tables: 34 | Sample Data: Complete';
PRINT N'Bao gồm: LichSuCapNhatDiem, XacNhanDiemChuTich';
PRINT N'=====================================================================';
