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

-- 3. NHÓM ĐỢT & ĐỀ TÀI
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

    trang_thai VARCHAR(20) DEFAULT 'CHO_DUYET',
    nhan_xet_duyet NVARCHAR(MAX),
    nguoi_duyet INT
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
    id_sinh_vien INT, --(người thực hiện)
    ten_cong_viec NVARCHAR(200),
    mo_ta_cong_viec NVARCHAR(MAX),
    ngay_bat_dau DATETIME,
    ngay_ket_thuc DATETIME,
    ngay_bat_dau_thuc_te DATETIME,
    ngay_ket_thuc_thuc_te DATETIME,
    trang_thai VARCHAR(20),
    id_file_minh_chung INT
);

CREATE TABLE BaoCaoNop (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    id_de_tai INT,
    id_sinh_vien INT,
    stt INT,
    ten_bao_cao NVARCHAR(50),
    file_baocao VARCHAR(255), -- import file lên 
    ngay_nop DATETIME,
    nhan_xet NVARCHAR(MAX),
    trang_thai VARCHAR(20)
);

CREATE TABLE NhatKyHuongDan (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_dot INT,
    ngay_hop DATE,
    hinh_thuc_hop BIT DEFAULT 1,
    thoi_gian_hop TIME,
    dia_diem_hop NVARCHAR(20) NULL,
    id_ke_hoach_cong_viec INT
);

CREATE TABLE BaoCaoThongKe (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten_bao_cao NVARCHAR(200),
    id_dot INT,
    ngay_tao DATETIME,
    du_lieu_json NVARCHAR(MAX) 
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
ALTER TABLE NhatKyHuongDan ADD CONSTRAINT FK_NKHD_KHCV FOREIGN KEY (id_ke_hoach_cong_viec) REFERENCES KeHoachCongViec(id);

ALTER TABLE DonPhucKhao ADD CONSTRAINT FK_DPK_SinhVien FOREIGN KEY (id_sinh_vien) REFERENCES SinhVien(id_nguoi_dung);
ALTER TABLE DonPhucKhao ADD CONSTRAINT FK_DPK_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);
ALTER TABLE BaoCaoThongKe ADD CONSTRAINT FK_BCTK_Dot FOREIGN KEY (id_dot) REFERENCES DotDoAn(id);






