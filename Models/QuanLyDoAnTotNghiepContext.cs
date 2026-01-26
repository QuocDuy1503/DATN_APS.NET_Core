using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Models;

public partial class QuanLyDoAnTotNghiepContext : DbContext
{
    public QuanLyDoAnTotNghiepContext()
    {
    }

    public QuanLyDoAnTotNghiepContext(DbContextOptions<QuanLyDoAnTotNghiepContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaoCaoNop> BaoCaoNops { get; set; }

    public virtual DbSet<BaoCaoThongKe> BaoCaoThongKes { get; set; }

    public virtual DbSet<BoMon> BoMons { get; set; }

    public virtual DbSet<CauHinhPhieuChamDot> CauHinhPhieuChamDots { get; set; }

    public virtual DbSet<CauHinhThongBao> CauHinhThongBaos { get; set; }

    public virtual DbSet<ChiTietCtdt> ChiTietCtdts { get; set; }

    public virtual DbSet<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; }

    public virtual DbSet<ChuyenNganh> ChuyenNganhs { get; set; }

    public virtual DbSet<DangKyNguyenVong> DangKyNguyenVongs { get; set; }

    public virtual DbSet<DeCuong> DeCuongs { get; set; }

    public virtual DbSet<DeTai> DeTais { get; set; }

    public virtual DbSet<DiemChiTiet> DiemChiTiets { get; set; }

    public virtual DbSet<DonPhucKhao> DonPhucKhaos { get; set; }

    public virtual DbSet<DotDoAn> DotDoAns { get; set; }

    public virtual DbSet<GiangVien> GiangViens { get; set; }

    public virtual DbSet<HocKi> HocKis { get; set; }

    public virtual DbSet<HoiDongBaoCao> HoiDongBaoCaos { get; set; }

    public virtual DbSet<KeHoachCongViec> KeHoachCongViecs { get; set; }

    public virtual DbSet<KetQuaBaoVeSinhVien> KetQuaBaoVeSinhViens { get; set; }

    public virtual DbSet<KetQuaHocTap> KetQuaHocTaps { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHocs { get; set; }

    public virtual DbSet<LichSuGuiEmail> LichSuGuiEmails { get; set; }

    public virtual DbSet<LoaiPhieuCham> LoaiPhieuChams { get; set; }

    public virtual DbSet<MauThongBao> MauThongBaos { get; set; }

    public virtual DbSet<Nganh> Nganhs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhatKyHuongDan> NhatKyHuongDans { get; set; }

    public virtual DbSet<PhienBaoVe> PhienBaoVes { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<SinhVienDeTai> SinhVienDeTais { get; set; }

    public virtual DbSet<ThanhVienHdBaoCao> ThanhVienHdBaoCaos { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TieuChiChamDiem> TieuChiChamDiems { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }
    public object DangKyDoAns { get; internal set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaoCaoNop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BaoCaoNo__3213E83F9D634EFF");

            entity.ToTable("BaoCaoNop");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FileBaocao)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("file_baocao");
            entity.Property(e => e.IdDeTai).HasColumnName("id_de_tai");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.NgayNop)
                .HasColumnType("datetime")
                .HasColumnName("ngay_nop");
            entity.Property(e => e.NhanXet).HasColumnName("nhan_xet");
            entity.Property(e => e.Stt).HasColumnName("stt");
            entity.Property(e => e.TenBaoCao)
                .HasMaxLength(50)
                .HasColumnName("ten_bao_cao");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdDeTaiNavigation).WithMany(p => p.BaoCaoNops)
                .HasForeignKey(d => d.IdDeTai)
                .HasConstraintName("FK_BCN_DeTai");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.BaoCaoNops)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_BCN_Dot");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.BaoCaoNops)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_BCN_SinhVien");
        });

        modelBuilder.Entity<BaoCaoThongKe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BaoCaoTh__3213E83FBEC73E3E");

            entity.ToTable("BaoCaoThongKe");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DuLieuJson).HasColumnName("du_lieu_json");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.NgayTao)
                .HasColumnType("datetime")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TenBaoCao)
                .HasMaxLength(200)
                .HasColumnName("ten_bao_cao");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.BaoCaoThongKes)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_BCTK_Dot");
        });

        modelBuilder.Entity<BoMon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BoMon__3213E83F3BEBEC59");

            entity.ToTable("BoMon");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdNguoiSua).HasColumnName("id_nguoi_sua");
            entity.Property(e => e.IdNguoiTao).HasColumnName("id_nguoi_tao");
            entity.Property(e => e.NgaySua).HasColumnName("ngay_sua");
            entity.Property(e => e.NgayTao).HasColumnName("ngay_tao");
            entity.Property(e => e.Stt).HasColumnName("stt");
            entity.Property(e => e.TenBoMon)
                .HasMaxLength(100)
                .HasColumnName("ten_bo_mon");
            entity.Property(e => e.TenVietTat)
                .HasMaxLength(20)
                .HasColumnName("ten_viet_tat");
        });

        modelBuilder.Entity<CauHinhPhieuChamDot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CauHinhP__3213E83FF7593742");

            entity.ToTable("CauHinhPhieuCham_Dot");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdLoaiPhieu).HasColumnName("id_loai_phieu");
            entity.Property(e => e.VaiTroCham)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("vai_tro_cham");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.CauHinhPhieuChamDots)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_CHPC_Dot");

            entity.HasOne(d => d.IdLoaiPhieuNavigation).WithMany(p => p.CauHinhPhieuChamDots)
                .HasForeignKey(d => d.IdLoaiPhieu)
                .HasConstraintName("FK_CHPC_LoaiPhieu");
        });

        modelBuilder.Entity<CauHinhThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CauHinhT__3213E83FAE9266C5");

            entity.ToTable("CauHinhThongBao");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DoiTuongNhan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("doi_tuong_nhan");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdMau).HasColumnName("id_mau");
            entity.Property(e => e.LoaiSuKien)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("loai_su_kien");
            entity.Property(e => e.MocThoiGian)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("moc_thoi_gian");
            entity.Property(e => e.NoiDungMau).HasColumnName("noi_dung_mau");
            entity.Property(e => e.SoNgayChenhLech).HasColumnName("so_ngay_chenh_lech");
            entity.Property(e => e.TieuDeMau)
                .HasMaxLength(200)
                .HasColumnName("tieu_de_mau");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.CauHinhThongBaos)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_CHTB_Dot");

            entity.HasOne(d => d.IdMauNavigation).WithMany(p => p.CauHinhThongBaos)
                .HasForeignKey(d => d.IdMau)
                .HasConstraintName("FK_CHTB_Mau");
        });

        modelBuilder.Entity<ChiTietCtdt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTiet___3213E83F9F0D490A");

            entity.ToTable("ChiTiet_CTDT");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DieuKienTienQuyet)
                .HasMaxLength(255)
                .HasColumnName("dieu_kien_tien_quyet");
            entity.Property(e => e.HocKiToChuc).HasColumnName("hoc_ki_to_chuc");
            entity.Property(e => e.IdCtdt).HasColumnName("id_ctdt");
            entity.Property(e => e.LoaiHocPhan)
                .HasMaxLength(50)
                .HasColumnName("loai_hoc_phan");
            entity.Property(e => e.MaHocPhan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ma_hoc_phan");
            entity.Property(e => e.SoTinChi).HasColumnName("so_tin_chi");
            entity.Property(e => e.Stt).HasColumnName("stt");
            entity.Property(e => e.TenHocPhan)
                .HasMaxLength(150)
                .HasColumnName("ten_hoc_phan");

            entity.HasOne(d => d.IdCtdtNavigation).WithMany(p => p.ChiTietCtdts)
                .HasForeignKey(d => d.IdCtdt)
                .HasConstraintName("FK_CTDT_ChiTiet");
        });

        modelBuilder.Entity<ChuongTrinhDaoTao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChuongTr__3213E83FA682D260");

            entity.ToTable("ChuongTrinhDaoTao");

            entity.HasIndex(e => e.MaCtdt, "UQ__ChuongTr__5AE49DFB4AFD3E38").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("id_khoa_hoc");
            entity.Property(e => e.IdNganh).HasColumnName("id_nganh");
            entity.Property(e => e.MaCtdt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ma_ctdt");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.SttHienThi).HasColumnName("stt_hien_thi");
            entity.Property(e => e.TenCtdt)
                .HasMaxLength(200)
                .HasColumnName("ten_ctdt");
            entity.Property(e => e.TongTinChi).HasColumnName("tong_tin_chi");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.ChuongTrinhDaoTaos)
                .HasForeignKey(d => d.IdKhoaHoc)
                .HasConstraintName("FK_CTDT_KhoaHoc");

            entity.HasOne(d => d.IdNganhNavigation).WithMany(p => p.ChuongTrinhDaoTaos)
                .HasForeignKey(d => d.IdNganh)
                .HasConstraintName("FK_CTDT_Nganh");
        });

        modelBuilder.Entity<ChuyenNganh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChuyenNg__3213E83F894A4D66");

            entity.ToTable("ChuyenNganh");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdBoMon).HasColumnName("id_bo_mon");
            entity.Property(e => e.IdNganh).HasColumnName("id_nganh");
            entity.Property(e => e.IdNguoiSua).HasColumnName("id_nguoi_sua");
            entity.Property(e => e.IdNguoiTao).HasColumnName("id_nguoi_tao");
            entity.Property(e => e.NgaySua).HasColumnName("ngay_sua");
            entity.Property(e => e.NgayTao).HasColumnName("ngay_tao");
            entity.Property(e => e.Stt).HasColumnName("stt");
            entity.Property(e => e.TenChuyenNganh)
                .HasMaxLength(100)
                .HasColumnName("ten_chuyen_nganh");
            entity.Property(e => e.TenVietTat)
                .HasMaxLength(20)
                .HasColumnName("ten_viet_tat");

            entity.HasOne(d => d.IdBoMonNavigation).WithMany(p => p.ChuyenNganhs)
                .HasForeignKey(d => d.IdBoMon)
                .HasConstraintName("FK_CN_BoMon");

            entity.HasOne(d => d.IdNganhNavigation).WithMany(p => p.ChuyenNganhs)
                .HasForeignKey(d => d.IdNganh)
                .HasConstraintName("FK_CN_Nganh");
        });

        modelBuilder.Entity<DangKyNguyenVong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DangKyNg__3213E83F60A886FB");

            entity.ToTable("DangKyNguyenVong");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_dang_ky");
            entity.Property(e => e.SoTinChiTichLuyHienTai).HasColumnName("so_tin_chi_tich_luy_hien_tai");
            entity.Property(e => e.TrangThai)
                .IsUnicode(false)
                .HasDefaultValue(0)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.DangKyNguyenVongs)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_DKNV_Dot");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.DangKyNguyenVongs)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_DKNV_SinhVien");
        });

        modelBuilder.Entity<DeCuong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeCuong__3213E83F72FF5BD3");

            entity.ToTable("DeCuong");

            entity.HasIndex(e => e.IdDeTai, "UQ__DeCuong__ED6A0B2C5DB52DF5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DoiTuongNghienCuu).HasColumnName("doi_tuong_nghien_cuu");
            entity.Property(e => e.GiaThuyetNghienCuu).HasColumnName("gia_thuyet_nghien_cuu");
            entity.Property(e => e.IdDeTai).HasColumnName("id_de_tai");
            entity.Property(e => e.LyDoChonDeTai).HasColumnName("ly_do_chon_de_tai");
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_nop");
            entity.Property(e => e.PhamViNghienCuu).HasColumnName("pham_vi_nghien_cuu");
            entity.Property(e => e.PhuongPhapNghienCuu).HasColumnName("phuong_phap_nghien_cuu");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdDeTaiNavigation).WithOne(p => p.DeCuong)
                .HasForeignKey<DeCuong>(d => d.IdDeTai)
                .HasConstraintName("FK_DeCuong_DeTai");
        });

        modelBuilder.Entity<DeTai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeTai__3213E83F1E04ED43");

            entity.ToTable("DeTai");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CongNgheSuDung).HasColumnName("cong_nghe_su_dung");
            entity.Property(e => e.IdChuyenNganh).HasColumnName("id_chuyen_nganh");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdGvhd).HasColumnName("id_gvhd");
            entity.Property(e => e.IdNguoiDeXuat).HasColumnName("id_nguoi_de_xuat");
            entity.Property(e => e.MaDeTai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_de_tai");
            entity.Property(e => e.MucTieuChinh).HasColumnName("muc_tieu_chinh");
            entity.Property(e => e.NguoiDuyet).HasColumnName("nguoi_duyet");
            entity.Property(e => e.NhanXetDuyet).HasColumnName("nhan_xet_duyet");
            entity.Property(e => e.PhamViChucNang).HasColumnName("pham_vi_chuc_nang");
            entity.Property(e => e.SanPhamKetQuaDuKien).HasColumnName("san_pham_ket_qua_du_kien");
            entity.Property(e => e.TenDeTai)
                .HasMaxLength(255)
                .HasColumnName("ten_de_tai");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("CHO_DUYET")
                .HasColumnName("trang_thai");
            entity.Property(e => e.YeuCauTinhMoi).HasColumnName("yeu_cau_tinh_moi");

            entity.HasOne(d => d.IdChuyenNganhNavigation).WithMany(p => p.DeTais)
                .HasForeignKey(d => d.IdChuyenNganh)
                .HasConstraintName("FK_DeTai_ChuyenNganh");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.DeTais)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_DeTai_Dot");

            entity.HasOne(d => d.IdGvhdNavigation).WithMany(p => p.DeTaiIdGvhdNavigations)
                .HasForeignKey(d => d.IdGvhd)
                .HasConstraintName("FK_DeTai_GVHD");

            entity.HasOne(d => d.IdNguoiDeXuatNavigation).WithMany(p => p.DeTais)
                .HasForeignKey(d => d.IdNguoiDeXuat)
                .HasConstraintName("FK_DeTai_NguoiDeXuat");

            entity.HasOne(d => d.NguoiDuyetNavigation).WithMany(p => p.DeTaiNguoiDuyetNavigations)
                .HasForeignKey(d => d.NguoiDuyet)
                .HasConstraintName("FK_DeTai_NguoiDuyet");
        });

        modelBuilder.Entity<DiemChiTiet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DiemChiT__3213E83FCE2026C4");

            entity.ToTable("DiemChiTiet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiemSo).HasColumnName("diem_so");
            entity.Property(e => e.IdNguoiCham).HasColumnName("id_nguoi_cham");
            entity.Property(e => e.IdPhienBaoVe).HasColumnName("id_phien_bao_ve");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.IdTieuChi).HasColumnName("id_tieu_chi");
            entity.Property(e => e.NhanXet).HasColumnName("nhan_xet");

            entity.HasOne(d => d.IdNguoiChamNavigation).WithMany(p => p.DiemChiTiets)
                .HasForeignKey(d => d.IdNguoiCham)
                .HasConstraintName("FK_DCT_NguoiCham");

            entity.HasOne(d => d.IdPhienBaoVeNavigation).WithMany(p => p.DiemChiTiets)
                .HasForeignKey(d => d.IdPhienBaoVe)
                .HasConstraintName("FK_DCT_Phien");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.DiemChiTiets)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_DCT_SinhVien");

            entity.HasOne(d => d.IdTieuChiNavigation).WithMany(p => p.DiemChiTiets)
                .HasForeignKey(d => d.IdTieuChi)
                .HasConstraintName("FK_DCT_TieuChi");
        });

        modelBuilder.Entity<DonPhucKhao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DonPhucK__3213E83FF2190074");

            entity.ToTable("DonPhucKhao");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.MinhChungLink)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("minh_chung_link");
            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_gui");
            entity.Property(e => e.NgayXuLy)
                .HasColumnType("datetime")
                .HasColumnName("ngay_xu_ly");
            entity.Property(e => e.NoiDungKhieuNai).HasColumnName("noi_dung_khieu_nai");
            entity.Property(e => e.PhanHoiCuaGv).HasColumnName("phan_hoi_cua_gv");
            entity.Property(e => e.TieuDeKhieuNai)
                .HasMaxLength(200)
                .HasColumnName("tieu_de_khieu_nai");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("CHO_XU_LY")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.DonPhucKhaos)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_DPK_Dot");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.DonPhucKhaos)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_DPK_SinhVien");
        });

        modelBuilder.Entity<DotDoAn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DotDoAn__3213E83F78F1A64D");

            entity.ToTable("DotDoAn");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdHocKi).HasColumnName("id_hoc_ki");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("id_khoa_hoc");
            entity.Property(e => e.NgayBatDauBaoCaoCuoiKi).HasColumnName("ngay_bat_dau_bao_cao_cuoi_ki");
            entity.Property(e => e.NgayBatDauBaoCaoGiuaKi).HasColumnName("ngay_bat_dau_bao_cao_giua_ki");
            entity.Property(e => e.NgayBatDauDeXuatDeTai).HasColumnName("ngay_bat_dau_de_xuat_de_tai");
            entity.Property(e => e.NgayBatDauDkDuyetNguyenVong).HasColumnName("ngay_bat_dau_dk_duyet_nguyen_vong");
            entity.Property(e => e.NgayBatDauDkNguyenVong).HasColumnName("ngay_bat_dau_dk_nguyen_vong");
            entity.Property(e => e.NgayBatDauDot).HasColumnName("ngay_bat_dau_dot");
            entity.Property(e => e.NgayBatDauDuyetDeXuatDeTai).HasColumnName("ngay_bat_dau_duyet_de_xuat_de_tai");
            entity.Property(e => e.NgayBatDauNopDeCuong).HasColumnName("ngay_bat_dau_nop_de_cuong");
            entity.Property(e => e.NgayCongBoKqBcck).HasColumnName("ngay_cong_bo_kq_BCCK");
            entity.Property(e => e.NgayDuyetDxdt).HasColumnName("ngay_duyet_DXDT");
            entity.Property(e => e.NgayKetThucBaoCaoCuoiKi).HasColumnName("ngay_ket_thuc_bao_cao_cuoi_ki");
            entity.Property(e => e.NgayKetThucBaoCaoGiuaKi).HasColumnName("ngay_ket_thuc_bao_cao_giua_ki");
            entity.Property(e => e.NgayKetThucDeXuatDeTai).HasColumnName("ngay_ket_thuc_de_xuat_de_tai");
            entity.Property(e => e.NgayKetThucDkDuyetNguyenVong).HasColumnName("ngay_ket_thuc_dk_duyet_nguyen_vong");
            entity.Property(e => e.NgayKetThucDkNguyenVong).HasColumnName("ngay_ket_thuc_dk_nguyen_vong");
            entity.Property(e => e.NgayKetThucDot).HasColumnName("ngay_ket_thuc_dot");
            entity.Property(e => e.NgayKetThucDuyetDeXuatDeTai).HasColumnName("ngay_ket_thuc_duyet_de_xuat_de_tai");
            entity.Property(e => e.NgayKetThucNopDeCuong).HasColumnName("ngay_ket_thuc_nop_de_cuong");
            entity.Property(e => e.NgayLapHdBcck).HasColumnName("ngay_lap_HD_BCCK");
            entity.Property(e => e.NgayLapHdBcgk).HasColumnName("ngay_lap_HD_BCGK");
            entity.Property(e => e.NgayLapHoiDongDuyetDxdt).HasColumnName("ngay_lap_hoi_dong_duyet_DXDT");
            entity.Property(e => e.NgayNopTaiLieuBcck).HasColumnName("ngay_nop_tai_lieu_BCCK");
            entity.Property(e => e.NgayNopTaiLieuBcgk).HasColumnName("ngay_nop_tai_lieu_BCGK");
            entity.Property(e => e.TenDot)
                .HasMaxLength(200)
                .HasColumnName("ten_dot");
            entity.Property(e => e.TrangThai).HasColumnName("trang_thai");

            entity.HasOne(d => d.IdHocKiNavigation).WithMany(p => p.DotDoAns)
                .HasForeignKey(d => d.IdHocKi)
                .HasConstraintName("FK_Dot_HocKi");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.DotDoAns)
                .HasForeignKey(d => d.IdKhoaHoc)
                .HasConstraintName("FK_Dot_KhoaHoc");
        });

        modelBuilder.Entity<GiangVien>(entity =>
        {
            entity.HasKey(e => e.IdNguoiDung).HasName("PK__GiangVie__75D6A11E69E021A6");

            entity.ToTable("GiangVien");

            entity.HasIndex(e => e.MaGv, "UQ__GiangVie__0FE116127A5115CB").IsUnique();

            entity.Property(e => e.IdNguoiDung)
                .ValueGeneratedNever()
                .HasColumnName("id_nguoi_dung");
            entity.Property(e => e.HocVi)
                .HasMaxLength(50)
                .HasColumnName("hoc_vi");
            entity.Property(e => e.IdBoMon).HasColumnName("id_bo_mon");
            entity.Property(e => e.MaGv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_gv");

            entity.HasOne(d => d.IdBoMonNavigation).WithMany(p => p.GiangViens)
                .HasForeignKey(d => d.IdBoMon)
                .HasConstraintName("FK_GV_BoMon");

            entity.HasOne(d => d.IdNguoiDungNavigation).WithOne(p => p.GiangVien)
                .HasForeignKey<GiangVien>(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GV_NguoiDung");
        });

        modelBuilder.Entity<HocKi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HocKi__3213E83FE7F52CAF");

            entity.ToTable("HocKi");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MaHocKi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_hoc_ki");
            entity.Property(e => e.NamBatDau).HasColumnName("nam_bat_dau");
            entity.Property(e => e.NamKetThuc).HasColumnName("nam_ket_thuc");
            entity.Property(e => e.NgayBatDau).HasColumnName("ngay_bat_dau");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trang_thai");
            entity.Property(e => e.TuanBatDau).HasColumnName("tuan_bat_dau");
        });

        modelBuilder.Entity<HoiDongBaoCao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HoiDongB__3213E83F5AD4E357");

            entity.ToTable("HoiDongBaoCao");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiaDiem)
                .HasMaxLength(100)
                .HasColumnName("dia_diem");
            entity.Property(e => e.IdBoMon).HasColumnName("id_bo_mon");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdNguoiTao).HasColumnName("id_nguoi_tao");
            entity.Property(e => e.LoaiHoiDong)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("loai_hoi_dong");
            entity.Property(e => e.MaHoiDong)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_hoi_dong");
            entity.Property(e => e.NgayBaoCao).HasColumnName("ngay_bao_cao");
            entity.Property(e => e.TenHoiDong)
                .HasMaxLength(100)
                .HasColumnName("ten_hoi_dong");
            entity.Property(e => e.ThoiGianDuKien).HasColumnName("thoi_gian_du_kien");
            entity.Property(e => e.TrangThai).HasColumnName("trang_thai");

            entity.HasOne(d => d.IdBoMonNavigation).WithMany(p => p.HoiDongBaoCaos)
                .HasForeignKey(d => d.IdBoMon)
                .HasConstraintName("FK_HDBC_BoMon");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.HoiDongBaoCaos)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_HDBC_Dot");
        });

        modelBuilder.Entity<KeHoachCongViec>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KeHoachC__3213E83F15422157");

            entity.ToTable("KeHoachCongViec");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdFileMinhChung).HasColumnName("id_file_minh_chung");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.MoTaCongViec).HasColumnName("mo_ta_cong_viec");
            entity.Property(e => e.NgayBatDau)
                .HasColumnType("datetime")
                .HasColumnName("ngay_bat_dau");
            entity.Property(e => e.NgayBatDauThucTe)
                .HasColumnType("datetime")
                .HasColumnName("ngay_bat_dau_thuc_te");
            entity.Property(e => e.NgayKetThuc)
                .HasColumnType("datetime")
                .HasColumnName("ngay_ket_thuc");
            entity.Property(e => e.NgayKetThucThucTe)
                .HasColumnType("datetime")
                .HasColumnName("ngay_ket_thuc_thuc_te");
            entity.Property(e => e.Stt).HasColumnName("stt");
            entity.Property(e => e.TenCongViec)
                .HasMaxLength(200)
                .HasColumnName("ten_cong_viec");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdFileMinhChungNavigation).WithMany(p => p.KeHoachCongViecs)
                .HasForeignKey(d => d.IdFileMinhChung)
                .HasConstraintName("FK_KHCV_MinhChung");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.KeHoachCongViecs)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_KHCV_SinhVien");
        });

        modelBuilder.Entity<KetQuaBaoVeSinhVien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KetQuaBa__3213E83FC3C8EAFE");

            entity.ToTable("KetQuaBaoVe_SinhVien");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiemChu)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("diem_chu");
            entity.Property(e => e.DiemTongKet).HasColumnName("diem_tong_ket");
            entity.Property(e => e.IdPhienBaoVe).HasColumnName("id_phien_bao_ve");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.KetQua)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ket_qua");

            entity.HasOne(d => d.IdPhienBaoVeNavigation).WithMany(p => p.KetQuaBaoVeSinhViens)
                .HasForeignKey(d => d.IdPhienBaoVe)
                .HasConstraintName("FK_KQBV_Phien");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.KetQuaBaoVeSinhViens)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_KQBV_SinhVien");
        });

        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KetQuaHo__3213E83FA089A7BC");

            entity.ToTable("KetQuaHocTap");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiemChu)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("diem_chu");
            entity.Property(e => e.DiemSo).HasColumnName("diem_so");
            entity.Property(e => e.Gpa).HasColumnName("GPA");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.KetQua).HasColumnName("ket_qua");
            entity.Property(e => e.MaHocPhan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_hoc_phan");
            entity.Property(e => e.SoTc).HasColumnName("so_tc");
            entity.Property(e => e.Stt).HasColumnName("stt");
            entity.Property(e => e.TenHocPhan)
                .HasMaxLength(100)
                .HasColumnName("ten_hoc_phan");
            entity.Property(e => e.TongSoTinChi).HasColumnName("tong_so_tin_chi");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.KetQuaHocTaps)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_KQHT_SinhVien");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhoaHoc__3213E83F20FB196F");

            entity.ToTable("KhoaHoc");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_khoa");
            entity.Property(e => e.NamNhapHoc).HasColumnName("nam_nhap_hoc");
            entity.Property(e => e.NamTotNghiep).HasColumnName("nam_tot_nghiep");
            entity.Property(e => e.TenKhoa)
                .HasMaxLength(50)
                .HasColumnName("ten_khoa");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trang_thai");
        });

        modelBuilder.Entity<LichSuGuiEmail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LichSuGu__3213E83F1E32A7EE");

            entity.ToTable("LichSuGuiEmail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdCauHinh).HasColumnName("id_cau_hinh");
            entity.Property(e => e.NguoiNhan)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nguoi_nhan");
            entity.Property(e => e.ThoiGianGui)
                .HasColumnType("datetime")
                .HasColumnName("thoi_gian_gui");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdCauHinhNavigation).WithMany(p => p.LichSuGuiEmails)
                .HasForeignKey(d => d.IdCauHinh)
                .HasConstraintName("FK_LSGE_CauHinh");
        });

        modelBuilder.Entity<LoaiPhieuCham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiPhie__3213E83F0DB7E487");

            entity.ToTable("LoaiPhieuCham");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NguoiTao).HasColumnName("nguoi_tao");
            entity.Property(e => e.TenLoaiPhieu)
                .HasMaxLength(100)
                .HasColumnName("ten_loai_phieu");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.LoaiPhieuChams)
                .HasForeignKey(d => d.NguoiTao)
                .HasConstraintName("FK_LPC_NguoiTao");
        });

        modelBuilder.Entity<MauThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MauThong__3213E83F74BE189B");

            entity.ToTable("MauThongBao");

            entity.HasIndex(e => e.MaMau, "UQ__MauThong__0BC941DA9A2615E3").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MaMau)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ma_mau");
            entity.Property(e => e.NoiDungThongBao).HasColumnName("noi_dung_thong_bao");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");
        });

        modelBuilder.Entity<Nganh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nganh__3213E83F4F091BFD");

            entity.ToTable("Nganh");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdBoMon).HasColumnName("id_bo_mon");
            entity.Property(e => e.IdNguoiSua).HasColumnName("id_nguoi_sua");
            entity.Property(e => e.IdNguoiTao).HasColumnName("id_nguoi_tao");
            entity.Property(e => e.MaNganh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_nganh");
            entity.Property(e => e.NgaySua).HasColumnName("ngay_sua");
            entity.Property(e => e.NgayTao).HasColumnName("ngay_tao");
            entity.Property(e => e.TenNganh)
                .HasMaxLength(100)
                .HasColumnName("ten_nganh");
            entity.Property(e => e.TenVietTat)
                .HasMaxLength(20)
                .HasColumnName("ten_viet_tat");

            entity.HasOne(d => d.IdBoMonNavigation).WithMany(p => p.Nganhs)
                .HasForeignKey(d => d.IdBoMon)
                .HasConstraintName("FK_Nganh_BoMon");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NguoiDun__3213E83F87694B61");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__AB6E61640A0D31B8").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("mat_khau");
            entity.Property(e => e.MicrosoftId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("microsoft_id");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("sdt");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(1)
                .HasColumnName("trang_thai");

            entity.HasMany(d => d.IdVaiTros).WithMany(p => p.IdNguoiDungs)
                .UsingEntity<Dictionary<string, object>>(
                    "NguoiDungVaiTro",
                    r => r.HasOne<VaiTro>().WithMany()
                        .HasForeignKey("IdVaiTro")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NDVT_VaiTro"),
                    l => l.HasOne<NguoiDung>().WithMany()
                        .HasForeignKey("IdNguoiDung")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NDVT_NguoiDung"),
                    j =>
                    {
                        j.HasKey("IdNguoiDung", "IdVaiTro").HasName("PK__NguoiDun__EEF2E664D32C4259");
                        j.ToTable("NguoiDung_VaiTro");
                        j.IndexerProperty<int>("IdNguoiDung").HasColumnName("id_nguoi_dung");
                        j.IndexerProperty<int>("IdVaiTro").HasColumnName("id_vai_tro");
                    });
        });

        modelBuilder.Entity<NhatKyHuongDan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NhatKyHu__3213E83F66EF0287");

            entity.ToTable("NhatKyHuongDan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiaDiemHop)
                .HasMaxLength(20)
                .HasColumnName("dia_diem_hop");
            entity.Property(e => e.HinhThucHop)
                .HasDefaultValue(true)
                .HasColumnName("hinh_thuc_hop");
            entity.Property(e => e.IdDot).HasColumnName("id_dot");
            entity.Property(e => e.IdKeHoachCongViec).HasColumnName("id_ke_hoach_cong_viec");
            entity.Property(e => e.NgayHop).HasColumnName("ngay_hop");
            entity.Property(e => e.ThoiGianHop).HasColumnName("thoi_gian_hop");

            entity.HasOne(d => d.IdDotNavigation).WithMany(p => p.NhatKyHuongDans)
                .HasForeignKey(d => d.IdDot)
                .HasConstraintName("FK_NKHD_Dot");

            entity.HasOne(d => d.IdKeHoachCongViecNavigation).WithMany(p => p.NhatKyHuongDans)
                .HasForeignKey(d => d.IdKeHoachCongViec)
                .HasConstraintName("FK_NKHD_KHCV");
        });

        modelBuilder.Entity<PhienBaoVe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PhienBao__3213E83FC20F68A8");

            entity.ToTable("PhienBaoVe");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdHdBaocao).HasColumnName("id_hd_baocao");
            entity.Property(e => e.IdSinhVienDeTai).HasColumnName("id_sinh_vien_de_tai");
            entity.Property(e => e.LinkTaiLieu)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("link_tai_lieu");
            entity.Property(e => e.SttBaoCao).HasColumnName("stt_bao_cao");

            entity.HasOne(d => d.IdHdBaocaoNavigation).WithMany(p => p.PhienBaoVes)
                .HasForeignKey(d => d.IdHdBaocao)
                .HasConstraintName("FK_PBV_HD");

            entity.HasOne(d => d.IdSinhVienDeTaiNavigation).WithMany(p => p.PhienBaoVes)
                .HasForeignKey(d => d.IdSinhVienDeTai)
                .HasConstraintName("FK_PBV_SVDT");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.IdNguoiDung).HasName("PK__SinhVien__75D6A11ECEB6A0A2");

            entity.ToTable("SinhVien");

            entity.HasIndex(e => e.Mssv, "UQ__SinhVien__763F1CDC1BE01D7E").IsUnique();

            entity.Property(e => e.IdNguoiDung)
                .ValueGeneratedNever()
                .HasColumnName("id_nguoi_dung");
            entity.Property(e => e.IdChuyenNganh).HasColumnName("id_chuyen_nganh");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("id_khoa_hoc");
            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("mssv");
            entity.Property(e => e.TinChiTichLuy)
                .HasDefaultValue(0.0)
                .HasColumnName("tin_chi_tich_luy");

            entity.HasOne(d => d.IdChuyenNganhNavigation).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.IdChuyenNganh)
                .HasConstraintName("FK_SV_ChuyenNganh");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.IdKhoaHoc)
                .HasConstraintName("FK_SV_KhoaHoc");

            entity.HasOne(d => d.IdNguoiDungNavigation).WithOne(p => p.SinhVien)
                .HasForeignKey<SinhVien>(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SV_NguoiDung");
        });

        modelBuilder.Entity<SinhVienDeTai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SinhVien__3213E83FCD622524");

            entity.ToTable("SinhVien_DeTai");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdDeTai).HasColumnName("id_de_tai");
            entity.Property(e => e.IdSinhVien).HasColumnName("id_sinh_vien");
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_dang_ky");
            entity.Property(e => e.NhanXet).HasColumnName("nhan_xet");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.IdDeTaiNavigation).WithMany(p => p.SinhVienDeTais)
                .HasForeignKey(d => d.IdDeTai)
                .HasConstraintName("FK_SVDT_DeTai");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.SinhVienDeTais)
                .HasForeignKey(d => d.IdSinhVien)
                .HasConstraintName("FK_SVDT_SinhVien");
        });

        modelBuilder.Entity<ThanhVienHdBaoCao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThanhVie__3213E83F87A0B37A");

            entity.ToTable("ThanhVien_HD_BaoCao");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdGiangVien).HasColumnName("id_giang_vien");
            entity.Property(e => e.IdHdBaocao).HasColumnName("id_hd_baocao");
            entity.Property(e => e.VaiTro)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("vai_tro");

            entity.HasOne(d => d.IdGiangVienNavigation).WithMany(p => p.ThanhVienHdBaoCaos)
                .HasForeignKey(d => d.IdGiangVien)
                .HasConstraintName("FK_TVHD_GV");

            entity.HasOne(d => d.IdHdBaocaoNavigation).WithMany(p => p.ThanhVienHdBaoCaos)
                .HasForeignKey(d => d.IdHdBaocao)
                .HasConstraintName("FK_TVHD_HD");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThongBao__3213E83FEB73E563");

            entity.ToTable("ThongBao");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdNguoiNhan).HasColumnName("id_nguoi_nhan");
            entity.Property(e => e.LinkLienKet)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("link_lien_ket");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.NoiDung).HasColumnName("noi_dung");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");
            entity.Property(e => e.TrangThaiXem)
                .HasDefaultValue(false)
                .HasColumnName("trang_thai_xem");

            entity.HasOne(d => d.IdNguoiNhanNavigation).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.IdNguoiNhan)
                .HasConstraintName("FK_TB_NguoiNhan");
        });

        modelBuilder.Entity<TieuChiChamDiem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TieuChiC__3213E83F976B366C");

            entity.ToTable("TieuChiChamDiem");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiemToiDa).HasColumnName("diem_toi_da");
            entity.Property(e => e.IdLoaiPhieu).HasColumnName("id_loai_phieu");
            entity.Property(e => e.MoTaHuongDan).HasColumnName("mo_ta_huong_dan");
            entity.Property(e => e.SttHienThi).HasColumnName("stt_hien_thi");
            entity.Property(e => e.TenTieuChi)
                .HasMaxLength(200)
                .HasColumnName("ten_tieu_chi");
            entity.Property(e => e.TrongSo).HasColumnName("trong_so");

            entity.HasOne(d => d.IdLoaiPhieuNavigation).WithMany(p => p.TieuChiChamDiems)
                .HasForeignKey(d => d.IdLoaiPhieu)
                .HasConstraintName("FK_TCCD_LoaiPhieu");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VaiTro__3213E83FB62466F1");

            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.MaVaiTro, "UQ__VaiTro__4AE1754C997E6ABF").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ma_vai_tro");
            entity.Property(e => e.TenVaiTro)
                .HasMaxLength(100)
                .HasColumnName("ten_vai_tro");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
