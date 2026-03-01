using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_TMS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsUnicodeFalse_ForUnicodeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HocKi",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_ki = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    nam_bat_dau = table.Column<int>(type: "int", nullable: true),
                    nam_ket_thuc = table.Column<int>(type: "int", nullable: true),
                    tuan_bat_dau = table.Column<int>(type: "int", nullable: true),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: true),
                    trang_thai = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HocKi__3213E83FE7F52CAF", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "KhoaHoc",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_khoa = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ten_khoa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    nam_nhap_hoc = table.Column<int>(type: "int", nullable: true),
                    nam_tot_nghiep = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KhoaHoc__3213E83F20FB196F", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MauThongBao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mau = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    tieu_de = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    noi_dung_thong_bao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MauThong__3213E83F74BE189B", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ho_ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mat_khau = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    sdt = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    microsoft_id = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    trang_thai = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NguoiDun__3213E83F87694B61", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_vai_tro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ten_vai_tro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VaiTro__3213E83FB62466F1", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "DotDoAn",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_dot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    id_khoa_hoc = table.Column<int>(type: "int", nullable: true),
                    id_hoc_ki = table.Column<int>(type: "int", nullable: true),
                    ngay_bat_dau_dot = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_dot = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_bat_dau_dk_nguyen_vong = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_dk_nguyen_vong = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_bat_dau_dk_duyet_nguyen_vong = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_dk_duyet_nguyen_vong = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_bat_dau_de_xuat_de_tai = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_de_xuat_de_tai = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_bat_dau_duyet_de_xuat_de_tai = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_duyet_de_xuat_de_tai = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_lap_hoi_dong_duyet_DXDT = table.Column<int>(type: "int", nullable: true),
                    ngay_duyet_DXDT = table.Column<int>(type: "int", nullable: true),
                    ngay_bat_dau_nop_de_cuong = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_nop_de_cuong = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_bat_dau_bao_cao_cuoi_ki = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_bao_cao_cuoi_ki = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_lap_HD_BCCK = table.Column<int>(type: "int", nullable: true),
                    ngay_nop_tai_lieu_BCCK = table.Column<int>(type: "int", nullable: true),
                    ngay_cong_bo_kq_BCCK = table.Column<int>(type: "int", nullable: true),
                    ngay_bat_dau_bao_cao_giua_ki = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc_bao_cao_giua_ki = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_lap_HD_BCGK = table.Column<int>(type: "int", nullable: true),
                    ngay_nop_tai_lieu_BCGK = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DotDoAn__3213E83F78F1A64D", x => x.id);
                    table.ForeignKey(
                        name: "FK_Dot_HocKi",
                        column: x => x.id_hoc_ki,
                        principalTable: "HocKi",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Dot_KhoaHoc",
                        column: x => x.id_khoa_hoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "BoMon",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stt = table.Column<int>(type: "int", nullable: true),
                    ten_bo_mon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ten_viet_tat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    id_nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateOnly>(type: "date", nullable: true),
                    id_nguoi_sua = table.Column<int>(type: "int", nullable: true),
                    ngay_sua = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BoMon__3213E83F3BEBEC59", x => x.id);
                    table.ForeignKey(
                        name: "FK_BoMon_NguoiDung_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BoMon_NguoiDung_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "LoaiPhieuCham",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_loai_phieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoaiPhie__3213E83F0DB7E487", x => x.id);
                    table.ForeignKey(
                        name: "FK_LPC_NguoiTao",
                        column: x => x.nguoi_tao,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_nguoi_nhan = table.Column<int>(type: "int", nullable: true),
                    tieu_de = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    link_lien_ket = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    trang_thai_xem = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ThongBao__3213E83FEB73E563", x => x.id);
                    table.ForeignKey(
                        name: "FK_TB_NguoiNhan",
                        column: x => x.id_nguoi_nhan,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_VaiTro",
                columns: table => new
                {
                    id_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    id_vai_tro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NguoiDun__EEF2E664D32C4259", x => new { x.id_nguoi_dung, x.id_vai_tro });
                    table.ForeignKey(
                        name: "FK_NDVT_NguoiDung",
                        column: x => x.id_nguoi_dung,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_NDVT_VaiTro",
                        column: x => x.id_vai_tro,
                        principalTable: "VaiTro",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoThongKe",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_dot = table.Column<int>(type: "int", nullable: false),
                    so_luong_sinh_vien = table.Column<int>(type: "int", nullable: false),
                    so_luong_de_tai = table.Column<int>(type: "int", nullable: false),
                    so_luong_task_tuan = table.Column<int>(type: "int", nullable: true),
                    ti_le_hoan_thanh = table.Column<double>(type: "float", nullable: true),
                    ngay_tinh = table.Column<DateTime>(type: "datetime", nullable: false),
                    chi_tiet_tuan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BaoCaoTh__3213E83FBEC73E3E", x => x.id);
                    table.ForeignKey(
                        name: "FK_BCTK_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhThongBao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    id_mau = table.Column<int>(type: "int", nullable: true),
                    loai_su_kien = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    moc_thoi_gian = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    so_ngay_chenh_lech = table.Column<int>(type: "int", nullable: true),
                    doi_tuong_nhan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    tieu_de_mau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    noi_dung_mau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CauHinhT__3213E83FAE9266C5", x => x.id);
                    table.ForeignKey(
                        name: "FK_CHTB_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CHTB_Mau",
                        column: x => x.id_mau,
                        principalTable: "MauThongBao",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHuongDan",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    ngay_hop = table.Column<DateOnly>(type: "date", nullable: true),
                    thoi_gian_hop = table.Column<TimeOnly>(type: "time", nullable: true),
                    hinh_thuc_hop = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    dia_diem_hop = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    thanh_vien_tham_du = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ten_gvhd = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    muc_tieu_buoi_hop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noi_dung_hop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    action_list = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NhatKyHu__3213E83F66EF0287", x => x.id);
                    table.ForeignKey(
                        name: "FK_NKHD_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "GiangVien",
                columns: table => new
                {
                    id_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    ma_gv = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    hoc_vi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    id_bo_mon = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GiangVie__75D6A11E69E021A6", x => x.id_nguoi_dung);
                    table.ForeignKey(
                        name: "FK_GV_BoMon",
                        column: x => x.id_bo_mon,
                        principalTable: "BoMon",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_GV_NguoiDung",
                        column: x => x.id_nguoi_dung,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "HoiDongBaoCao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoi_dong = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ten_hoi_dong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    loai_hoi_dong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    id_nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    id_bo_mon = table.Column<int>(type: "int", nullable: true),
                    ngay_bao_cao = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: true),
                    dia_diem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    thoi_gian_du_kien = table.Column<TimeOnly>(type: "time", nullable: true),
                    trang_thai = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HoiDongB__3213E83F5AD4E357", x => x.id);
                    table.ForeignKey(
                        name: "FK_HDBC_BoMon",
                        column: x => x.id_bo_mon,
                        principalTable: "BoMon",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_HDBC_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_HoiDongBaoCao_NguoiDung_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Nganh",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nganh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ten_nganh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ten_viet_tat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    id_nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateOnly>(type: "date", nullable: true),
                    id_nguoi_sua = table.Column<int>(type: "int", nullable: true),
                    ngay_sua = table.Column<DateOnly>(type: "date", nullable: true),
                    id_bo_mon = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Nganh__3213E83F4F091BFD", x => x.id);
                    table.ForeignKey(
                        name: "FK_Nganh_BoMon",
                        column: x => x.id_bo_mon,
                        principalTable: "BoMon",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Nganh_NguoiDung_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Nganh_NguoiDung_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CauHinhPhieuCham_Dot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    vai_tro_cham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    id_loai_phieu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CauHinhP__3213E83FF7593742", x => x.id);
                    table.ForeignKey(
                        name: "FK_CHPC_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CHPC_LoaiPhieu",
                        column: x => x.id_loai_phieu,
                        principalTable: "LoaiPhieuCham",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "TieuChiChamDiem",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_loai_phieu = table.Column<int>(type: "int", nullable: true),
                    ten_tieu_chi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    mo_ta_huong_dan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trong_so = table.Column<double>(type: "float", nullable: true),
                    diem_toi_da = table.Column<double>(type: "float", nullable: true),
                    stt_hien_thi = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TieuChiC__3213E83F976B366C", x => x.id);
                    table.ForeignKey(
                        name: "FK_TCCD_LoaiPhieu",
                        column: x => x.id_loai_phieu,
                        principalTable: "LoaiPhieuCham",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "LichSuGuiEmail",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cau_hinh = table.Column<int>(type: "int", nullable: true),
                    nguoi_nhan = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    thoi_gian_gui = table.Column<DateTime>(type: "datetime", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LichSuGu__3213E83F1E32A7EE", x => x.id);
                    table.ForeignKey(
                        name: "FK_LSGE_CauHinh",
                        column: x => x.id_cau_hinh,
                        principalTable: "CauHinhThongBao",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ThanhVien_HD_BaoCao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_hd_baocao = table.Column<int>(type: "int", nullable: true),
                    id_giang_vien = table.Column<int>(type: "int", nullable: true),
                    vai_tro = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ThanhVie__3213E83F87A0B37A", x => x.id);
                    table.ForeignKey(
                        name: "FK_TVHD_GV",
                        column: x => x.id_giang_vien,
                        principalTable: "GiangVien",
                        principalColumn: "id_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_TVHD_HD",
                        column: x => x.id_hd_baocao,
                        principalTable: "HoiDongBaoCao",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ctdt = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_ctdt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    stt_hien_thi = table.Column<int>(type: "int", nullable: true),
                    id_nganh = table.Column<int>(type: "int", nullable: true),
                    id_khoa_hoc = table.Column<int>(type: "int", nullable: true),
                    tong_tin_chi = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChuongTr__3213E83FA682D260", x => x.id);
                    table.ForeignKey(
                        name: "FK_CTDT_KhoaHoc",
                        column: x => x.id_khoa_hoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CTDT_Nganh",
                        column: x => x.id_nganh,
                        principalTable: "Nganh",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ChuyenNganh",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stt = table.Column<int>(type: "int", nullable: true),
                    ten_chuyen_nganh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ten_viet_tat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    id_nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateOnly>(type: "date", nullable: true),
                    id_nguoi_sua = table.Column<int>(type: "int", nullable: true),
                    ngay_sua = table.Column<DateOnly>(type: "date", nullable: true),
                    id_nganh = table.Column<int>(type: "int", nullable: true),
                    id_bo_mon = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChuyenNg__3213E83F894A4D66", x => x.id);
                    table.ForeignKey(
                        name: "FK_CN_BoMon",
                        column: x => x.id_bo_mon,
                        principalTable: "BoMon",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CN_Nganh",
                        column: x => x.id_nganh,
                        principalTable: "Nganh",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ChiTiet_CTDT",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ctdt = table.Column<int>(type: "int", nullable: true),
                    stt = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_phan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ten_hoc_phan = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    so_tin_chi = table.Column<int>(type: "int", nullable: true),
                    loai_hoc_phan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    dieu_kien_tien_quyet = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    hoc_ki_to_chuc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChiTiet___3213E83F9F0D490A", x => x.id);
                    table.ForeignKey(
                        name: "FK_CTDT_ChiTiet",
                        column: x => x.id_ctdt,
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DeTai",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_de_tai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ten_de_tai = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    muc_tieu_chinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    yeu_cau_tinh_moi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pham_vi_chuc_nang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cong_nghe_su_dung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    san_pham_ket_qua_du_kien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    id_nguoi_de_xuat = table.Column<int>(type: "int", nullable: true),
                    id_gvhd = table.Column<int>(type: "int", nullable: true),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    id_chuyen_nganh = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "CHO_DUYET"),
                    nhan_xet_duyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DeTai__3213E83F1E04ED43", x => x.id);
                    table.ForeignKey(
                        name: "FK_DeTai_ChuyenNganh",
                        column: x => x.id_chuyen_nganh,
                        principalTable: "ChuyenNganh",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DeTai_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DeTai_GVHD",
                        column: x => x.id_gvhd,
                        principalTable: "GiangVien",
                        principalColumn: "id_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_DeTai_NguoiDeXuat",
                        column: x => x.id_nguoi_de_xuat,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DeTai_NguoiDuyet",
                        column: x => x.nguoi_duyet,
                        principalTable: "GiangVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "SinhVien",
                columns: table => new
                {
                    id_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    mssv = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    id_chuyen_nganh = table.Column<int>(type: "int", nullable: true),
                    id_khoa_hoc = table.Column<int>(type: "int", nullable: true),
                    tin_chi_tich_luy = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SinhVien__75D6A11ECEB6A0A2", x => x.id_nguoi_dung);
                    table.ForeignKey(
                        name: "FK_SV_ChuyenNganh",
                        column: x => x.id_chuyen_nganh,
                        principalTable: "ChuyenNganh",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SV_KhoaHoc",
                        column: x => x.id_khoa_hoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SV_NguoiDung",
                        column: x => x.id_nguoi_dung,
                        principalTable: "NguoiDung",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DeCuong",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_de_tai = table.Column<int>(type: "int", nullable: true),
                    ly_do_chon_de_tai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gia_thuyet_nghien_cuu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    doi_tuong_nghien_cuu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pham_vi_nghien_cuu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phuong_phap_nghien_cuu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_nop = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DeCuong__3213E83F72FF5BD3", x => x.id);
                    table.ForeignKey(
                        name: "FK_DeCuong_DeTai",
                        column: x => x.id_de_tai,
                        principalTable: "DeTai",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "NhanXetHoiDongDeTai",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_de_tai = table.Column<int>(type: "int", nullable: false),
                    id_giang_vien = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    nhan_xet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanXetHoiDongDeTai", x => x.id);
                    table.ForeignKey(
                        name: "FK_NXHD_DeTai",
                        column: x => x.id_de_tai,
                        principalTable: "DeTai",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NXHD_GiangVien",
                        column: x => x.id_giang_vien,
                        principalTable: "GiangVien",
                        principalColumn: "id_nguoi_dung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoNop",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    id_de_tai = table.Column<int>(type: "int", nullable: true),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    stt = table.Column<int>(type: "int", nullable: true),
                    ten_bao_cao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    file_baocao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ngay_nop = table.Column<DateTime>(type: "datetime", nullable: true),
                    nhan_xet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BaoCaoNo__3213E83F9D634EFF", x => x.id);
                    table.ForeignKey(
                        name: "FK_BCN_DeTai",
                        column: x => x.id_de_tai,
                        principalTable: "DeTai",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BCN_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BCN_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "DangKyNguyenVong",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    so_tin_chi_tich_luy_hien_tai = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<int>(type: "int", unicode: false, nullable: true, defaultValue: 0),
                    ngay_dang_ky = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DangKyNg__3213E83F60A886FB", x => x.id);
                    table.ForeignKey(
                        name: "FK_DKNV_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DKNV_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "DonPhucKhao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    tieu_de_khieu_nai = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    noi_dung_khieu_nai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    minh_chung_link = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "CHO_XU_LY"),
                    phan_hoi_cua_gv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_gui = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ngay_xu_ly = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DonPhucK__3213E83FF2190074", x => x.id);
                    table.ForeignKey(
                        name: "FK_DPK_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DPK_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "KetQuaHocTap",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    stt = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_phan = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ten_hoc_phan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    so_tc = table.Column<double>(type: "float", nullable: true),
                    diem_so = table.Column<double>(type: "float", nullable: true),
                    diem_chu = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: true),
                    tong_so_tin_chi = table.Column<double>(type: "float", nullable: true),
                    GPA = table.Column<double>(type: "float", nullable: true),
                    ket_qua = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KetQuaHo__3213E83FA089A7BC", x => x.id);
                    table.ForeignKey(
                        name: "FK_KQHT_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "SinhVien_DeTai",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_de_tai = table.Column<int>(type: "int", nullable: true),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ngay_dang_ky = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    nhan_xet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SinhVien__3213E83FCD622524", x => x.id);
                    table.ForeignKey(
                        name: "FK_SVDT_DeTai",
                        column: x => x.id_de_tai,
                        principalTable: "DeTai",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SVDT_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "KeHoachCongViec",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stt = table.Column<int>(type: "int", nullable: true),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    id_dot = table.Column<int>(type: "int", nullable: true),
                    tuan = table.Column<int>(type: "int", nullable: true),
                    thu_trong_tuan = table.Column<byte>(type: "tinyint", nullable: true),
                    gio_bat_dau = table.Column<TimeOnly>(type: "time", nullable: true),
                    gio_ket_thuc = table.Column<TimeOnly>(type: "time", nullable: true),
                    gio_bat_dau_thuc_te = table.Column<TimeOnly>(type: "time", nullable: true),
                    gio_ket_thuc_thuc_te = table.Column<TimeOnly>(type: "time", nullable: true),
                    ten_cong_viec = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    mo_ta_cong_viec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    id_file_minh_chung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KeHoachC__3213E83F15422157", x => x.id);
                    table.ForeignKey(
                        name: "FK_KHCV_Dot",
                        column: x => x.id_dot,
                        principalTable: "DotDoAn",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_KHCV_MinhChung",
                        column: x => x.id_file_minh_chung,
                        principalTable: "BaoCaoNop",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_KHCV_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "PhienBaoVe",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_hd_baocao = table.Column<int>(type: "int", nullable: true),
                    id_sinh_vien_de_tai = table.Column<int>(type: "int", nullable: true),
                    stt_bao_cao = table.Column<int>(type: "int", nullable: true),
                    link_tai_lieu = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhienBao__3213E83FC20F68A8", x => x.id);
                    table.ForeignKey(
                        name: "FK_PBV_HD",
                        column: x => x.id_hd_baocao,
                        principalTable: "HoiDongBaoCao",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PBV_SVDT",
                        column: x => x.id_sinh_vien_de_tai,
                        principalTable: "SinhVien_DeTai",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DiemChiTiet",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_phien_bao_ve = table.Column<int>(type: "int", nullable: true),
                    id_nguoi_cham = table.Column<int>(type: "int", nullable: true),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    id_tieu_chi = table.Column<int>(type: "int", nullable: true),
                    diem_so = table.Column<double>(type: "float", nullable: true),
                    nhan_xet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DiemChiT__3213E83FCE2026C4", x => x.id);
                    table.ForeignKey(
                        name: "FK_DCT_NguoiCham",
                        column: x => x.id_nguoi_cham,
                        principalTable: "GiangVien",
                        principalColumn: "id_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_DCT_Phien",
                        column: x => x.id_phien_bao_ve,
                        principalTable: "PhienBaoVe",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DCT_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_DCT_TieuChi",
                        column: x => x.id_tieu_chi,
                        principalTable: "TieuChiChamDiem",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "KetQuaBaoVe_SinhVien",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_phien_bao_ve = table.Column<int>(type: "int", nullable: true),
                    id_sinh_vien = table.Column<int>(type: "int", nullable: true),
                    diem_tong_ket = table.Column<double>(type: "float", nullable: true),
                    diem_chu = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ket_qua = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KetQuaBa__3213E83FC3C8EAFE", x => x.id);
                    table.ForeignKey(
                        name: "FK_KQBV_Phien",
                        column: x => x.id_phien_bao_ve,
                        principalTable: "PhienBaoVe",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_KQBV_SinhVien",
                        column: x => x.id_sinh_vien,
                        principalTable: "SinhVien",
                        principalColumn: "id_nguoi_dung");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoNop_id_de_tai",
                table: "BaoCaoNop",
                column: "id_de_tai");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoNop_id_dot",
                table: "BaoCaoNop",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoNop_id_sinh_vien",
                table: "BaoCaoNop",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoThongKe_id_dot",
                table: "BaoCaoThongKe",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_BoMon_id_nguoi_sua",
                table: "BoMon",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_BoMon_id_nguoi_tao",
                table: "BoMon",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhPhieuCham_Dot_id_dot",
                table: "CauHinhPhieuCham_Dot",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhPhieuCham_Dot_id_loai_phieu",
                table: "CauHinhPhieuCham_Dot",
                column: "id_loai_phieu");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhThongBao_id_dot",
                table: "CauHinhThongBao",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhThongBao_id_mau",
                table: "CauHinhThongBao",
                column: "id_mau");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTiet_CTDT_id_ctdt",
                table: "ChiTiet_CTDT",
                column: "id_ctdt");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_id_khoa_hoc",
                table: "ChuongTrinhDaoTao",
                column: "id_khoa_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_id_nganh",
                table: "ChuongTrinhDaoTao",
                column: "id_nganh");

            migrationBuilder.CreateIndex(
                name: "UQ__ChuongTr__5AE49DFB4AFD3E38",
                table: "ChuongTrinhDaoTao",
                column: "ma_ctdt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganh_id_bo_mon",
                table: "ChuyenNganh",
                column: "id_bo_mon");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganh_id_nganh",
                table: "ChuyenNganh",
                column: "id_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyNguyenVong_id_dot",
                table: "DangKyNguyenVong",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyNguyenVong_id_sinh_vien",
                table: "DangKyNguyenVong",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "UQ__DeCuong__ED6A0B2C5DB52DF5",
                table: "DeCuong",
                column: "id_de_tai",
                unique: true,
                filter: "[id_de_tai] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeTai_id_chuyen_nganh",
                table: "DeTai",
                column: "id_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_DeTai_id_dot",
                table: "DeTai",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_DeTai_id_gvhd",
                table: "DeTai",
                column: "id_gvhd");

            migrationBuilder.CreateIndex(
                name: "IX_DeTai_id_nguoi_de_xuat",
                table: "DeTai",
                column: "id_nguoi_de_xuat");

            migrationBuilder.CreateIndex(
                name: "IX_DeTai_nguoi_duyet",
                table: "DeTai",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_DiemChiTiet_id_nguoi_cham",
                table: "DiemChiTiet",
                column: "id_nguoi_cham");

            migrationBuilder.CreateIndex(
                name: "IX_DiemChiTiet_id_phien_bao_ve",
                table: "DiemChiTiet",
                column: "id_phien_bao_ve");

            migrationBuilder.CreateIndex(
                name: "IX_DiemChiTiet_id_sinh_vien",
                table: "DiemChiTiet",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "IX_DiemChiTiet_id_tieu_chi",
                table: "DiemChiTiet",
                column: "id_tieu_chi");

            migrationBuilder.CreateIndex(
                name: "IX_DonPhucKhao_id_dot",
                table: "DonPhucKhao",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_DonPhucKhao_id_sinh_vien",
                table: "DonPhucKhao",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "IX_DotDoAn_id_hoc_ki",
                table: "DotDoAn",
                column: "id_hoc_ki");

            migrationBuilder.CreateIndex(
                name: "IX_DotDoAn_id_khoa_hoc",
                table: "DotDoAn",
                column: "id_khoa_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_GiangVien_id_bo_mon",
                table: "GiangVien",
                column: "id_bo_mon");

            migrationBuilder.CreateIndex(
                name: "UQ__GiangVie__0FE116127A5115CB",
                table: "GiangVien",
                column: "ma_gv",
                unique: true,
                filter: "[ma_gv] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HoiDongBaoCao_id_bo_mon",
                table: "HoiDongBaoCao",
                column: "id_bo_mon");

            migrationBuilder.CreateIndex(
                name: "IX_HoiDongBaoCao_id_dot",
                table: "HoiDongBaoCao",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_HoiDongBaoCao_id_nguoi_tao",
                table: "HoiDongBaoCao",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachCongViec_id_dot",
                table: "KeHoachCongViec",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachCongViec_id_file_minh_chung",
                table: "KeHoachCongViec",
                column: "id_file_minh_chung");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachCongViec_id_sinh_vien",
                table: "KeHoachCongViec",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaBaoVe_SinhVien_id_phien_bao_ve",
                table: "KetQuaBaoVe_SinhVien",
                column: "id_phien_bao_ve");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaBaoVe_SinhVien_id_sinh_vien",
                table: "KetQuaBaoVe_SinhVien",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaHocTap_id_sinh_vien",
                table: "KetQuaHocTap",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuGuiEmail_id_cau_hinh",
                table: "LichSuGuiEmail",
                column: "id_cau_hinh");

            migrationBuilder.CreateIndex(
                name: "IX_LoaiPhieuCham_nguoi_tao",
                table: "LoaiPhieuCham",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "UQ__MauThong__0BC941DA9A2615E3",
                table: "MauThongBao",
                column: "ma_mau",
                unique: true,
                filter: "[ma_mau] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_id_bo_mon",
                table: "Nganh",
                column: "id_bo_mon");

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_id_nguoi_sua",
                table: "Nganh",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_id_nguoi_tao",
                table: "Nganh",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "UQ__NguoiDun__AB6E61640A0D31B8",
                table: "NguoiDung",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_VaiTro_id_vai_tro",
                table: "NguoiDung_VaiTro",
                column: "id_vai_tro");

            migrationBuilder.CreateIndex(
                name: "IX_NhanXetHoiDongDeTai_id_de_tai",
                table: "NhanXetHoiDongDeTai",
                column: "id_de_tai");

            migrationBuilder.CreateIndex(
                name: "IX_NhanXetHoiDongDeTai_id_giang_vien",
                table: "NhanXetHoiDongDeTai",
                column: "id_giang_vien");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHuongDan_id_dot",
                table: "NhatKyHuongDan",
                column: "id_dot");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBaoVe_id_hd_baocao",
                table: "PhienBaoVe",
                column: "id_hd_baocao");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBaoVe_id_sinh_vien_de_tai",
                table: "PhienBaoVe",
                column: "id_sinh_vien_de_tai");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_id_chuyen_nganh",
                table: "SinhVien",
                column: "id_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_id_khoa_hoc",
                table: "SinhVien",
                column: "id_khoa_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ__SinhVien__763F1CDC1BE01D7E",
                table: "SinhVien",
                column: "mssv",
                unique: true,
                filter: "[mssv] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_DeTai_id_de_tai",
                table: "SinhVien_DeTai",
                column: "id_de_tai");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_DeTai_id_sinh_vien",
                table: "SinhVien_DeTai",
                column: "id_sinh_vien");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVien_HD_BaoCao_id_giang_vien",
                table: "ThanhVien_HD_BaoCao",
                column: "id_giang_vien");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVien_HD_BaoCao_id_hd_baocao",
                table: "ThanhVien_HD_BaoCao",
                column: "id_hd_baocao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_id_nguoi_nhan",
                table: "ThongBao",
                column: "id_nguoi_nhan");

            migrationBuilder.CreateIndex(
                name: "IX_TieuChiChamDiem_id_loai_phieu",
                table: "TieuChiChamDiem",
                column: "id_loai_phieu");

            migrationBuilder.CreateIndex(
                name: "UQ__VaiTro__4AE1754C997E6ABF",
                table: "VaiTro",
                column: "ma_vai_tro",
                unique: true,
                filter: "[ma_vai_tro] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCaoThongKe");

            migrationBuilder.DropTable(
                name: "CauHinhPhieuCham_Dot");

            migrationBuilder.DropTable(
                name: "ChiTiet_CTDT");

            migrationBuilder.DropTable(
                name: "DangKyNguyenVong");

            migrationBuilder.DropTable(
                name: "DeCuong");

            migrationBuilder.DropTable(
                name: "DiemChiTiet");

            migrationBuilder.DropTable(
                name: "DonPhucKhao");

            migrationBuilder.DropTable(
                name: "KeHoachCongViec");

            migrationBuilder.DropTable(
                name: "KetQuaBaoVe_SinhVien");

            migrationBuilder.DropTable(
                name: "KetQuaHocTap");

            migrationBuilder.DropTable(
                name: "LichSuGuiEmail");

            migrationBuilder.DropTable(
                name: "NguoiDung_VaiTro");

            migrationBuilder.DropTable(
                name: "NhanXetHoiDongDeTai");

            migrationBuilder.DropTable(
                name: "NhatKyHuongDan");

            migrationBuilder.DropTable(
                name: "ThanhVien_HD_BaoCao");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "ChuongTrinhDaoTao");

            migrationBuilder.DropTable(
                name: "TieuChiChamDiem");

            migrationBuilder.DropTable(
                name: "BaoCaoNop");

            migrationBuilder.DropTable(
                name: "PhienBaoVe");

            migrationBuilder.DropTable(
                name: "CauHinhThongBao");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "LoaiPhieuCham");

            migrationBuilder.DropTable(
                name: "HoiDongBaoCao");

            migrationBuilder.DropTable(
                name: "SinhVien_DeTai");

            migrationBuilder.DropTable(
                name: "MauThongBao");

            migrationBuilder.DropTable(
                name: "DeTai");

            migrationBuilder.DropTable(
                name: "SinhVien");

            migrationBuilder.DropTable(
                name: "DotDoAn");

            migrationBuilder.DropTable(
                name: "GiangVien");

            migrationBuilder.DropTable(
                name: "ChuyenNganh");

            migrationBuilder.DropTable(
                name: "HocKi");

            migrationBuilder.DropTable(
                name: "KhoaHoc");

            migrationBuilder.DropTable(
                name: "Nganh");

            migrationBuilder.DropTable(
                name: "BoMon");

            migrationBuilder.DropTable(
                name: "NguoiDung");
        }
    }
}
