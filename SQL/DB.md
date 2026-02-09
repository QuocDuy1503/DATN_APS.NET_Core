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

PRINT N'=====================================================================';
PRINT N'DATABASE CREATED & DATA INSERTED SUCCESSFULLY!';
PRINT N'Database: QuanLyDoAnTotNghiep';
PRINT N'Tables: 32 | Sample Data: Complete';
PRINT N'=====================================================================';