using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class CauHinhThongBao
{
    public int Id { get; set; }

    public int? IdDot { get; set; }

    public int? IdMau { get; set; }

    public string? LoaiSuKien { get; set; }

    public string? MocThoiGian { get; set; }

    public int? SoNgayChenhLech { get; set; }

    public string? DoiTuongNhan { get; set; }

    public string? TieuDeMau { get; set; }

    public string? NoiDungMau { get; set; }

    public bool? TrangThai { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual MauThongBao? IdMauNavigation { get; set; }

    public virtual ICollection<LichSuGuiEmail> LichSuGuiEmails { get; set; } = new List<LichSuGuiEmail>();
}
