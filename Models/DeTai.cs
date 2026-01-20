using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class DeTai
{
    public int Id { get; set; }

    public string? MaDeTai { get; set; }

    public string? TenDeTai { get; set; }

    public string? MucTieuChinh { get; set; }

    public string? YeuCauTinhMoi { get; set; }

    public string? PhamViChucNang { get; set; }

    public string? CongNgheSuDung { get; set; }

    public string? SanPhamKetQuaDuKien { get; set; }

    public int? IdNguoiDeXuat { get; set; }

    public int? IdGvhd { get; set; }

    public int? IdDot { get; set; }

    public int? IdChuyenNganh { get; set; }

    public string? TrangThai { get; set; }

    public string? NhanXetDuyet { get; set; }

    public int? NguoiDuyet { get; set; }

    public virtual ICollection<BaoCaoNop> BaoCaoNops { get; set; } = new List<BaoCaoNop>();

    public virtual DeCuong? DeCuong { get; set; }

    public virtual ChuyenNganh? IdChuyenNganhNavigation { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual GiangVien? IdGvhdNavigation { get; set; }

    public virtual NguoiDung? IdNguoiDeXuatNavigation { get; set; }

    public virtual GiangVien? NguoiDuyetNavigation { get; set; }

    public virtual ICollection<SinhVienDeTai> SinhVienDeTais { get; set; } = new List<SinhVienDeTai>();
}
