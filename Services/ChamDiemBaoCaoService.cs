using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using DATN_TMS.Areas.GiangVien.Models;

namespace DATN_TMS.Services
{
    public class ChamDiemBaoCaoService : IChamDiemBaoCaoService
    {
        private readonly QuanLyDoAnTotNghiepContext _context;
        private string? _validationError;

        public ChamDiemBaoCaoService(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public string? GetValidationError(string key) => _validationError;

        public async Task<int> GetCurrentUserId(string? userEmail)
        {
            if (string.IsNullOrEmpty(userEmail)) return 0;
            var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.Email == userEmail);
            return nguoiDung?.Id ?? 0;
        }

        public async Task<bool> CheckCoHoiDong(int currentUserId)
        {
            return await _context.ThanhVienHdBaoCaos
                .Include(tv => tv.IdHdBaocaoNavigation)
                .AnyAsync(tv => tv.IdGiangVien == currentUserId &&
                                tv.IdHdBaocaoNavigation != null &&
                                tv.IdHdBaocaoNavigation.TrangThaiDuyet == "DA_DUYET" &&
                                tv.IdHdBaocaoNavigation.LoaiHoiDong != "DUYET_DE_TAI");
        }

        public async Task<List<HoiDongChamDiemViewModel>> GetDanhSachHoiDong(int currentUserId)
        {
            var hoiDongs = await _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdDotNavigation)
                .Include(hd => hd.ThanhVienHdBaoCaos)
                    .ThenInclude(tv => tv.IdGiangVienNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(hd => hd.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                .Where(hd => hd.TrangThaiDuyet == "DA_DUYET" &&
                             hd.LoaiHoiDong != "DUYET_DE_TAI" &&
                             hd.ThanhVienHdBaoCaos.Any(tv => tv.IdGiangVien == currentUserId))
                .OrderByDescending(hd => hd.NgayBaoCao)
                .ToListAsync();

            return hoiDongs.Select(hd =>
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var ngayKetThuc = hd.NgayKetThuc ?? hd.NgayBaoCao;
                var daHetHan = ngayKetThuc.HasValue && today > ngayKetThuc.Value;

                return new HoiDongChamDiemViewModel
                {
                    Id = hd.Id,
                    MaHoiDong = hd.MaHoiDong ?? "",
                    TenHoiDong = hd.TenHoiDong ?? "",
                    LoaiHoiDong = hd.LoaiHoiDong ?? "",
                    TenBoMon = hd.IdBoMonNavigation?.TenBoMon ?? "N/A",
                    NgayBaoCao = hd.NgayBaoCao,
                    DiaDiem = hd.DiaDiem ?? "",
                    SoLuongDeTai = hd.PhienBaoVes
                        .Where(pb => pb.IdSinhVienDeTaiNavigation?.IdDeTai != null)
                        .Select(pb => pb.IdSinhVienDeTaiNavigation!.IdDeTai)
                        .Distinct()
                        .Count(),
                    VaiTroTrongHoiDong = hd.ThanhVienHdBaoCaos
                        .FirstOrDefault(tv => tv.IdGiangVien == currentUserId)?.VaiTro ?? "",
                    DaChamDiem = hd.ThanhVienHdBaoCaos
                        .FirstOrDefault(tv => tv.IdGiangVien == currentUserId)?.DaChamDiem ?? false,
                    CoDenNgayBaoCao = hd.NgayBaoCao.HasValue &&
                                       DateOnly.FromDateTime(DateTime.Today) >= hd.NgayBaoCao.Value,
                    DaHetHanBaoCao = daHetHan
                };
            }).ToList();
        }

        public async Task<ChiTietHoiDongChamDiemViewModel?> GetChiTietHoiDong(int hoiDongId, int currentUserId)
        {
            _validationError = null;

            var hoiDong = await _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdDotNavigation)
                .Include(hd => hd.ThanhVienHdBaoCaos)
                    .ThenInclude(tv => tv.IdGiangVienNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(hd => hd.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                        .ThenInclude(svdt => svdt != null ? svdt.IdSinhVienNavigation : null)
                            .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .Include(hd => hd.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                        .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                            .ThenInclude(dt => dt != null ? dt.IdGvhdNavigation : null)
                                .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .FirstOrDefaultAsync(hd => hd.Id == hoiDongId && hd.TrangThaiDuyet == "DA_DUYET");

            if (hoiDong == null)
            {
                _validationError = "Không tìm thấy hội đồng hoặc hội đồng chưa được duyệt.";
                return null;
            }

            var thanhVien = hoiDong.ThanhVienHdBaoCaos.FirstOrDefault(tv => tv.IdGiangVien == currentUserId);
            if (thanhVien == null)
            {
                _validationError = "Bạn không phải là thành viên của hội đồng này.";
                return null;
            }

            if (hoiDong.NgayBaoCao.HasValue && DateOnly.FromDateTime(DateTime.Today) < hoiDong.NgayBaoCao.Value)
            {
                _validationError = $"Chưa đến ngày báo cáo ({hoiDong.NgayBaoCao.Value:dd/MM/yyyy}). Không thể xem chi tiết.";
                return null;
            }

            // Kiểm tra hết hạn báo cáo
            var todayCheck = DateOnly.FromDateTime(DateTime.Today);
            var ngayKetThucHD = hoiDong.NgayKetThuc ?? hoiDong.NgayBaoCao;
            var daHetHanBaoCao = ngayKetThucHD.HasValue && todayCheck > ngayKetThucHD.Value;

            var danhSachDeTai = new List<DeTaiChamDiemViewModel>();
            var laHoiDongGiuaKy = hoiDong.LoaiHoiDong == "GIUA_KY";

            var groupedByDeTai = hoiDong.PhienBaoVes
                .Where(pb => pb.IdSinhVienDeTaiNavigation?.IdDeTai != null)
                .GroupBy(pb => pb.IdSinhVienDeTaiNavigation!.IdDeTai)
                .OrderBy(g => g.Min(pb => pb.SttBaoCao));

            foreach (var group in groupedByDeTai)
            {
                var firstPhien = group.First();
                var dt = firstPhien.IdSinhVienDeTaiNavigation?.IdDeTaiNavigation;
                var idDeTai = group.Key ?? 0;

                var sinhViens = new List<SinhVienDeTaiChamDiemViewModel>();

                foreach (var pb in group.OrderBy(p => p.SttBaoCao))
                {
                    var svdt = pb.IdSinhVienDeTaiNavigation;
                    var sv = svdt?.IdSinhVienNavigation;
                    var idSinhVien = sv?.IdNguoiDung ?? 0;

                    var diemHoiDong = await _context.DiemChiTiets
                        .Include(d => d.IdNguoiChamNavigation)
                            .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                        .Where(d => d.IdPhienBaoVe == pb.Id && d.IdSinhVien == idSinhVien)
                        .ToListAsync();

                    var diemCuaBanList = diemHoiDong.Where(d => d.IdNguoiCham == currentUserId).ToList();
                    double? tongDiemCuaBan = diemCuaBanList.Any() ? diemCuaBanList.Sum(d => d.DiemSo ?? 0) : null;

                    string linkTaiLieu = pb.LinkTaiLieu ?? "";
                    if (string.IsNullOrEmpty(linkTaiLieu))
                    {
                        var loaiBaoCao = laHoiDongGiuaKy ? "GIUA_KY" : "CUOI_KY";
                        // Giữa kì: mỗi SV nộp riêng → lọc theo IdSinhVien
                        // Cuối kì: 1 SV nộp đại diện cả nhóm → chỉ lọc theo IdDeTai
                        var query = _context.BaoCaoNops
                            .Where(bc => bc.IdDeTai == idDeTai &&
                                        bc.LoaiBaoCao == loaiBaoCao &&
                                        bc.TrangThai == "DA_DUYET");
                        if (laHoiDongGiuaKy)
                        {
                            query = query.Where(bc => bc.IdSinhVien == idSinhVien);
                        }
                        var baoCaoDaDuyet = await query
                            .OrderByDescending(bc => bc.NgayNop)
                            .FirstOrDefaultAsync();
                        linkTaiLieu = baoCaoDaDuyet?.FileBaocao ?? "";
                    }

                    sinhViens.Add(new SinhVienDeTaiChamDiemViewModel
                    {
                        IdSinhVien = idSinhVien,
                        IdSinhVienDeTai = pb.IdSinhVienDeTai ?? 0,
                        PhienBaoVeId = pb.Id,
                        TenSinhVien = sv?.IdNguoiDungNavigation?.HoTen ?? "N/A",
                        Mssv = sv?.Mssv ?? "",
                        DiemGVHD = svdt?.DiemGvhd,
                        NhanXetGVHD = svdt?.NhanXetGvhd ?? "",
                        DiemCuaBan = tongDiemCuaBan,
                        LinkTaiLieu = linkTaiLieu,
                        DiemHoiDong = diemHoiDong.Select(d => new DiemThanhVienViewModel
                        {
                            IdNguoiCham = d.IdNguoiCham ?? 0,
                            TenNguoiCham = d.IdNguoiChamNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                            DiemSo = d.DiemSo ?? 0,
                            NhanXet = d.NhanXet ?? ""
                        }).ToList()
                    });
                }

                var xacNhan = laHoiDongGiuaKy ? null : await _context.XacNhanDiemChuTichs
                    .FirstOrDefaultAsync(x => x.IdPhienBaoVe == firstPhien.Id);

                danhSachDeTai.Add(new DeTaiChamDiemViewModel
                {
                    IdDeTai = idDeTai,
                    MaDeTai = dt?.MaDeTai ?? "",
                    TenDeTai = dt?.TenDeTai ?? "N/A",
                    TenGVHD = dt?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "N/A",
                    SttBaoCao = firstPhien.SttBaoCao ?? 0,
                    DanhSachSinhVien = sinhViens,
                    TrangThaiXacNhan = xacNhan?.TrangThai ?? "CHO_XAC_NHAN",
                    DiemTongKetCuoi = xacNhan?.DiemTongKetCuoi
                });
            }

            return new ChiTietHoiDongChamDiemViewModel
            {
                HoiDong = new HoiDongChamDiemViewModel
                {
                    Id = hoiDong.Id,
                    MaHoiDong = hoiDong.MaHoiDong ?? "",
                    TenHoiDong = hoiDong.TenHoiDong ?? "",
                    LoaiHoiDong = hoiDong.LoaiHoiDong ?? "",
                    TenBoMon = hoiDong.IdBoMonNavigation?.TenBoMon ?? "",
                    NgayBaoCao = hoiDong.NgayBaoCao,
                    DiaDiem = hoiDong.DiaDiem ?? "",
                    VaiTroTrongHoiDong = thanhVien.VaiTro ?? ""
                },
                ThanhViens = hoiDong.ThanhVienHdBaoCaos.Select(tv => new ThanhVienHoiDongChamDiemViewModel
                {
                    IdGiangVien = tv.IdGiangVien ?? 0,
                    TenGiangVien = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                    VaiTro = tv.VaiTro ?? "",
                    DaChamDiem = tv.DaChamDiem ?? false
                }).ToList(),
                DanhSachDeTai = danhSachDeTai,
                VaiTroHienTai = thanhVien.VaiTro ?? "",
                LaChuTich = thanhVien.VaiTro == "CHU_TICH",
                LaThuKy = thanhVien.VaiTro == "THU_KY",
                DaHetHanBaoCao = daHetHanBaoCao
            };
        }

        public async Task<ChamDiemPhienBaoVeViewModel?> GetChamDiemViewModel(int phienBaoVeId, int currentUserId)
        {
            _validationError = null;

            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdSinhVienNavigation : null)
                        .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                .FirstOrDefaultAsync(p => p.Id == phienBaoVeId);

            if (phien == null)
            {
                _validationError = "Không tìm thấy phiên bảo vệ.";
                return null;
            }

            var hoiDong = phien.IdHdBaocaoNavigation;
            if (hoiDong == null || hoiDong.TrangThaiDuyet != "DA_DUYET")
            {
                _validationError = "Hội đồng chưa được duyệt.";
                return null;
            }

            var thanhVien = hoiDong.ThanhVienHdBaoCaos?.FirstOrDefault(tv => tv.IdGiangVien == currentUserId);
            if (thanhVien == null)
            {
                _validationError = "Bạn không phải thành viên của hội đồng này.";
                return null;
            }

            if (hoiDong.NgayBaoCao.HasValue && DateOnly.FromDateTime(DateTime.Today) < hoiDong.NgayBaoCao.Value)
            {
                _validationError = "REDIRECT_DETAIL:" + hoiDong.Id + ":Chưa đến ngày báo cáo.";
                return null;
            }

            // Kiểm tra hết hạn báo cáo
            {
                var todayCheck = DateOnly.FromDateTime(DateTime.Today);
                var ngayKetThucHD = hoiDong.NgayKetThuc ?? hoiDong.NgayBaoCao;
                if (ngayKetThucHD.HasValue && todayCheck > ngayKetThucHD.Value)
                {
                    var ngayHetHanStr = ngayKetThucHD.Value.ToString("dd/MM/yyyy");
                    _validationError = "REDIRECT_DETAIL:" + hoiDong.Id + ":Đã hết hạn báo cáo (kết thúc ngày " + ngayHetHanStr + "). Không thể nhận xét hoặc chấm điểm.";
                    return null;
                }
            }

            var svdt = phien.IdSinhVienDeTaiNavigation;
            var laHoiDongGiuaKy = hoiDong.LoaiHoiDong == "GIUA_KY";
            var vaiTroThanhVien = thanhVien.VaiTro ?? "";
            var laPhanBien = vaiTroThanhVien == "PHAN_BIEN";
            var idDeTai = svdt?.IdDeTai ?? 0;

            var tatCaPhienBaoVeCungDeTai = await _context.PhienBaoVes
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(sv => sv != null ? sv.IdSinhVienNavigation : null)
                        .ThenInclude(s => s != null ? s.IdNguoiDungNavigation : null)
                .Where(p => p.IdHdBaocao == hoiDong.Id &&
                            p.IdSinhVienDeTaiNavigation != null &&
                            p.IdSinhVienDeTaiNavigation.IdDeTai == idDeTai)
                .ToListAsync();

            if (!laHoiDongGiuaKy)
            {
                var svChuaChamGVHD = tatCaPhienBaoVeCungDeTai
                    .Where(p => p.IdSinhVienDeTaiNavigation?.DiemGvhd == null)
                    .Select(p => p.IdSinhVienDeTaiNavigation?.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen)
                    .FirstOrDefault();

                if (svChuaChamGVHD != null)
                {
                    _validationError = "REDIRECT_DETAIL:" + hoiDong.Id + $":GVHD chưa chấm điểm cho sinh viên '{svChuaChamGVHD}'. Vui lòng đợi GVHD chấm điểm trước.";
                    return null;
                }
            }

            // Lấy tiêu chí chấm điểm
            CauHinhPhieuChamDot? cauHinh = null;
            string tenLoaiPhieu = "";

            if (laHoiDongGiuaKy)
            {
                cauHinh = await _context.CauHinhPhieuChamDots
                    .Include(c => c.IdLoaiPhieuNavigation).ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                    .Where(c => c.IdDot == hoiDong.IdDot && c.VaiTroCham == "GVHD_GIUA_KY")
                    .FirstOrDefaultAsync();
                if (cauHinh == null)
                    cauHinh = await _context.CauHinhPhieuChamDots
                        .Include(c => c.IdLoaiPhieuNavigation).ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                        .Where(c => c.VaiTroCham == "GVHD_GIUA_KY").FirstOrDefaultAsync();
                tenLoaiPhieu = "Phiếu chấm giữa kì";
            }
            else if (laPhanBien)
            {
                cauHinh = await _context.CauHinhPhieuChamDots
                    .Include(c => c.IdLoaiPhieuNavigation).ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                    .Where(c => c.IdDot == hoiDong.IdDot && c.VaiTroCham == "PhanBien")
                    .FirstOrDefaultAsync();
                if (cauHinh == null)
                    cauHinh = await _context.CauHinhPhieuChamDots
                        .Include(c => c.IdLoaiPhieuNavigation).ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                        .Where(c => c.VaiTroCham == "PhanBien").FirstOrDefaultAsync();
                tenLoaiPhieu = "Phiếu chấm phản biện";
            }
            else
            {
                cauHinh = await _context.CauHinhPhieuChamDots
                    .Include(c => c.IdLoaiPhieuNavigation).ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                    .Where(c => c.IdDot == hoiDong.IdDot && c.VaiTroCham == "HoiDong")
                    .FirstOrDefaultAsync();
                if (cauHinh == null)
                    cauHinh = await _context.CauHinhPhieuChamDots
                        .Include(c => c.IdLoaiPhieuNavigation).ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                        .Where(c => c.VaiTroCham == "HoiDong").FirstOrDefaultAsync();
                tenLoaiPhieu = "Phiếu chấm hội đồng bảo vệ";
            }

            if (cauHinh == null)
            {
                cauHinh = await _context.CauHinhPhieuChamDots
                    .Include(c => c.IdLoaiPhieuNavigation).ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                    .FirstOrDefaultAsync();
            }

            var tieuChis = cauHinh?.IdLoaiPhieuNavigation?.TieuChiChamDiems?.ToList() ?? new List<TieuChiChamDiem>();

            if (!tieuChis.Any())
            {
                LoaiPhieuCham? loaiPhieu = null;
                if (laHoiDongGiuaKy)
                    loaiPhieu = await _context.LoaiPhieuChams.Include(lp => lp.TieuChiChamDiems)
                        .FirstOrDefaultAsync(lp => lp.TenLoaiPhieu != null && (lp.TenLoaiPhieu.Contains("giữa kì") || lp.TenLoaiPhieu.Contains("giữa kỳ")));
                else if (laPhanBien)
                    loaiPhieu = await _context.LoaiPhieuChams.Include(lp => lp.TieuChiChamDiems)
                        .FirstOrDefaultAsync(lp => lp.TenLoaiPhieu != null && lp.TenLoaiPhieu.Contains("Phản biện"));
                else
                    loaiPhieu = await _context.LoaiPhieuChams.Include(lp => lp.TieuChiChamDiems)
                        .FirstOrDefaultAsync(lp => lp.TenLoaiPhieu != null && lp.TenLoaiPhieu.Contains("Hội đồng"));

                if (loaiPhieu != null)
                {
                    tieuChis = loaiPhieu.TieuChiChamDiems.ToList();
                    tenLoaiPhieu = loaiPhieu.TenLoaiPhieu ?? tenLoaiPhieu;
                }
            }

            var chiNhanXet = cauHinh?.IdLoaiPhieuNavigation?.ChiNhanXet ?? laHoiDongGiuaKy;
            var tenPhieuHienTai = cauHinh?.IdLoaiPhieuNavigation?.TenLoaiPhieu ?? tenLoaiPhieu;

            if (!tieuChis.Any())
            {
                var msg = laHoiDongGiuaKy
                    ? "Chưa có tiêu chí chấm điểm cho 'Phiếu chấm giữa kì' được cấu hình. Vui lòng vào Quản lý Tiêu chí để thiết lập."
                    : $"Chưa có tiêu chí chấm điểm cho '{tenLoaiPhieu}' được cấu hình. Vui lòng liên hệ BCN Khoa.";
                _validationError = "REDIRECT_DETAIL:" + hoiDong.Id + ":" + msg;
                return null;
            }

            var danhSachSinhVien = new List<SinhVienChamDiemInfo>();
            foreach (var p in tatCaPhienBaoVeCungDeTai.OrderBy(x => x.IdSinhVienDeTaiNavigation?.IdSinhVienNavigation?.Mssv))
            {
                var svInfo = p.IdSinhVienDeTaiNavigation;
                var sv = svInfo?.IdSinhVienNavigation;

                var diemDaChamSV = await _context.DiemChiTiets
                    .Where(d => d.IdPhienBaoVe == p.Id && d.IdNguoiCham == currentUserId && d.IdSinhVien == svInfo!.IdSinhVien)
                    .ToListAsync();

                danhSachSinhVien.Add(new SinhVienChamDiemInfo
                {
                    IdSinhVien = svInfo?.IdSinhVien ?? 0,
                    IdSinhVienDeTai = p.IdSinhVienDeTai ?? 0,
                    PhienBaoVeId = p.Id,
                    TenSinhVien = sv?.IdNguoiDungNavigation?.HoTen ?? "",
                    Mssv = sv?.Mssv ?? "",
                    DiemGVHD = svInfo?.DiemGvhd,
                    NhanXetGVHD = svInfo?.NhanXetGvhd ?? "",
                    DiemDaCham = tieuChis.Select(tc => new TieuChiChamDiemViewModel
                    {
                        Id = tc.Id,
                        TenTieuChi = tc.TenTieuChi ?? "",
                        MoTaHuongDan = tc.MoTaHuongDan ?? "",
                        TrongSo = tc.TrongSo ?? 0,
                        DiemToiDa = tc.DiemToiDa ?? 10,
                        DiemDaCham = diemDaChamSV.FirstOrDefault(d => d.IdTieuChi == tc.Id)?.DiemSo ?? 0,
                        NhanXetDaCham = diemDaChamSV.FirstOrDefault(d => d.IdTieuChi == tc.Id)?.NhanXet ?? ""
                    }).ToList()
                });
            }

            return new ChamDiemPhienBaoVeViewModel
            {
                PhienBaoVeId = phien.Id,
                IdDeTai = idDeTai,
                HoiDongId = hoiDong.Id,
                TenHoiDong = hoiDong.TenHoiDong ?? "",
                LoaiHoiDong = hoiDong.LoaiHoiDong ?? "",
                TenDeTai = svdt?.IdDeTaiNavigation?.TenDeTai ?? "",
                MaDeTai = svdt?.IdDeTaiNavigation?.MaDeTai ?? "",
                DanhSachSinhVien = danhSachSinhVien,
                TenSinhVien = danhSachSinhVien.FirstOrDefault()?.TenSinhVien ?? "",
                Mssv = danhSachSinhVien.FirstOrDefault()?.Mssv ?? "",
                DiemGVHD = danhSachSinhVien.FirstOrDefault()?.DiemGVHD ?? 0,
                NhanXetGVHD = danhSachSinhVien.FirstOrDefault()?.NhanXetGVHD ?? "",
                LaHoiDongGiuaKy = laHoiDongGiuaKy,
                VaiTroHienTai = vaiTroThanhVien,
                LaPhanBien = laPhanBien,
                TenLoaiPhieu = tenPhieuHienTai,
                ChiNhanXet = chiNhanXet,
                TieuChis = tieuChis.Select(tc => new TieuChiChamDiemViewModel
                {
                    Id = tc.Id,
                    TenTieuChi = tc.TenTieuChi ?? "",
                    MoTaHuongDan = tc.MoTaHuongDan ?? "",
                    TrongSo = tc.TrongSo ?? 0,
                    DiemToiDa = tc.DiemToiDa ?? 10
                }).ToList(),
                DaChamDiem = danhSachSinhVien.Any(sv => sv.DaChamDiem)
            };
        }

        public async Task<LuuDiemResult> LuuDiem(LuuDiemRequest request, int currentUserId)
        {
            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                .FirstOrDefaultAsync(p => p.Id == request.PhienBaoVeId);

            if (phien == null)
                return new LuuDiemResult { Success = false, Message = "Không tìm thấy phiên bảo vệ." };

            var hoiDongId = phien.IdHdBaocao ?? 0;
            var laHoiDongGiuaKy = phien.IdHdBaocaoNavigation?.LoaiHoiDong == "GIUA_KY";

            // Kiểm tra hết hạn báo cáo
            var hoiDongLuu = phien.IdHdBaocaoNavigation;
            if (hoiDongLuu != null)
            {
                var todayCheck = DateOnly.FromDateTime(DateTime.Today);
                var ngayKetThucHD = hoiDongLuu.NgayKetThuc ?? hoiDongLuu.NgayBaoCao;
                if (ngayKetThucHD.HasValue && todayCheck > ngayKetThucHD.Value)
                {
                    return new LuuDiemResult
                    {
                        Success = false,
                        Message = "Đã hết hạn báo cáo (kết thúc ngày " + ngayKetThucHD.Value.ToString("dd/MM/yyyy") + "). Không thể lưu nhận xét hoặc điểm."
                    };
                }
            }

            // Server-side validation cho giữa kì: nhận xét không được trống
            if (laHoiDongGiuaKy)
            {
                foreach (var diemSV in request.DanhSachDiemSinhVien)
                {
                    if (diemSV.DiemChiTiets.Any(d => string.IsNullOrWhiteSpace(d.NhanXet)))
                    {
                        return new LuuDiemResult { Success = false, Message = "Nhận xét không được để trống cho hội đồng giữa kì." };
                    }
                }
            }

            foreach (var diemSV in request.DanhSachDiemSinhVien)
            {
                var pb = await _context.PhienBaoVes
                    .Include(p => p.IdSinhVienDeTaiNavigation)
                    .FirstOrDefaultAsync(p => p.Id == diemSV.PhienBaoVeId);

                if (pb == null) continue;
                var svdt = pb.IdSinhVienDeTaiNavigation;
                if (svdt == null) continue;

                var diemCu = await _context.DiemChiTiets
                    .Where(d => d.IdPhienBaoVe == pb.Id && d.IdNguoiCham == currentUserId && d.IdSinhVien == svdt.IdSinhVien)
                    .ToListAsync();
                _context.DiemChiTiets.RemoveRange(diemCu);

                foreach (var diem in diemSV.DiemChiTiets)
                {
                    _context.DiemChiTiets.Add(new DiemChiTiet
                    {
                        IdPhienBaoVe = pb.Id,
                        IdNguoiCham = currentUserId,
                        IdSinhVien = svdt.IdSinhVien,
                        IdTieuChi = diem.IdTieuChi,
                        DiemSo = diem.DiemSo,
                        NhanXet = diem.NhanXet
                    });
                }
            }

            var thanhVien = phien.IdHdBaocaoNavigation?.ThanhVienHdBaoCaos?
                .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);
            if (thanhVien != null)
                thanhVien.DaChamDiem = true;

            await _context.SaveChangesAsync();

            var soSinhVien = request.DanhSachDiemSinhVien.Count;
            if (laHoiDongGiuaKy)
            {
                return new LuuDiemResult
                {
                    Success = true,
                    Message = $"Lưu nhận xét giữa kì thành công cho {soSinhVien} sinh viên!",
                    RedirectUrl = $"DETAIL:{hoiDongId}"
                };
            }
            return new LuuDiemResult
            {
                Success = true,
                Message = $"Lưu điểm thành công cho {soSinhVien} sinh viên!",
                RedirectUrl = $"BANGDIEM:{request.PhienBaoVeId}"
            };
        }

        public async Task<LuuDiemResult> ThuKyDieuChinhDiem(DieuChinhDiemRequest request, int currentUserId)
        {
            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.IdDotNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                .FirstOrDefaultAsync(p => p.Id == request.PhienBaoVeId);

            if (phien == null)
                return new LuuDiemResult { Success = false, Message = "Không tìm thấy phiên bảo vệ." };

            var hoiDong = phien.IdHdBaocaoNavigation;
            var thanhVien = hoiDong?.ThanhVienHdBaoCaos?
                .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);

            if (thanhVien?.VaiTro != "THU_KY")
                return new LuuDiemResult { Success = false, Message = "Chỉ Thư ký mới có quyền điều chỉnh điểm." };

            // Kiểm tra giai đoạn báo cáo
            var periodCheck = KiemTraGiaiDoanBaoCao(hoiDong);
            if (!periodCheck.ConTrong)
                return new LuuDiemResult { Success = false, Message = "Đã hết giai đoạn báo cáo. Không thể điều chỉnh điểm." };

            // Lấy IdSinhVien từ phiên bảo vệ nếu request không cung cấp
            var idSinhVien = request.IdSinhVien > 0
                ? request.IdSinhVien
                : phien.IdSinhVienDeTaiNavigation?.IdSinhVien ?? 0;

            if (idSinhVien > 0)
            {
                _context.LichSuCapNhatDiems.Add(new LichSuCapNhatDiem
                {
                    IdPhienBaoVe = request.PhienBaoVeId,
                    IdSinhVien = idSinhVien,
                    IdNguoiCapNhat = currentUserId,
                    LoaiCapNhat = "THU_KY_DIEU_CHINH",
                    DiemCu = request.DiemCu,
                    DiemMoi = request.DiemMoi,
                    LyDo = request.LyDo,
                    NgayCapNhat = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return new LuuDiemResult { Success = true, Message = "Đã điều chỉnh điểm thành công!" };
        }

        public async Task<LuuDiemResult> ThuKyChinhSuaDiemThanhVien(ThuKyChinhSuaDiemThanhVienRequest request, int currentUserId)
        {
            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.IdDotNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                .FirstOrDefaultAsync(p => p.Id == request.PhienBaoVeId);

            if (phien == null)
                return new LuuDiemResult { Success = false, Message = "Không tìm thấy phiên bảo vệ." };

            var hoiDong = phien.IdHdBaocaoNavigation;
            var thanhVien = hoiDong?.ThanhVienHdBaoCaos?
                .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);

            if (thanhVien?.VaiTro != "THU_KY")
                return new LuuDiemResult { Success = false, Message = "Chỉ Thư ký mới có quyền chỉnh sửa điểm thành viên." };

            // Kiểm tra giai đoạn báo cáo
            var periodCheck = KiemTraGiaiDoanBaoCao(hoiDong);
            if (!periodCheck.ConTrong)
                return new LuuDiemResult { Success = false, Message = "Đã hết giai đoạn báo cáo. Không thể chỉnh sửa điểm." };

            // Không cho sửa điểm GVHD (người chấm không phải thành viên hội đồng)
            var nguoiChamLaThanhVien = hoiDong?.ThanhVienHdBaoCaos?.Any(tv => tv.IdGiangVien == request.IdNguoiCham) ?? false;
            if (!nguoiChamLaThanhVien)
                return new LuuDiemResult { Success = false, Message = "Không thể chỉnh sửa điểm của giảng viên hướng dẫn." };

            // Lấy IdSinhVien
            var idSinhVien = request.IdSinhVien > 0
                ? request.IdSinhVien
                : phien.IdSinhVienDeTaiNavigation?.IdSinhVien ?? 0;

            // Cập nhật tất cả điểm chi tiết của người chấm cho sinh viên này → scale theo tỉ lệ
            var diemChiTiets = await _context.DiemChiTiets
                .Where(d => d.IdPhienBaoVe == request.PhienBaoVeId
                         && d.IdNguoiCham == request.IdNguoiCham
                         && d.IdSinhVien == idSinhVien)
                .ToListAsync();

            if (!diemChiTiets.Any())
                return new LuuDiemResult { Success = false, Message = "Không tìm thấy điểm của thành viên này." };

            var tongDiemCu = diemChiTiets.Sum(d => d.DiemSo ?? 0);
            if (tongDiemCu > 0 && request.DiemMoi >= 0)
            {
                var tiLe = request.DiemMoi / tongDiemCu;
                foreach (var dc in diemChiTiets)
                {
                    dc.DiemSo = Math.Round((dc.DiemSo ?? 0) * tiLe, 2);
                }
            }

            // Ghi lịch sử
            _context.LichSuCapNhatDiems.Add(new LichSuCapNhatDiem
            {
                IdPhienBaoVe = request.PhienBaoVeId,
                IdSinhVien = idSinhVien,
                IdNguoiCapNhat = currentUserId,
                LoaiCapNhat = "THU_KY_CHINH_SUA_DIEM_THANH_VIEN",
                DiemCu = request.DiemCu,
                DiemMoi = request.DiemMoi,
                LyDo = request.LyDo,
                NgayCapNhat = DateTime.Now
            });

            // Reset trạng thái xác nhận nếu đã xác nhận
            var idDeTai = phien.IdSinhVienDeTaiNavigation?.IdDeTai ?? 0;
            var tatCaPhienCungDeTai = await _context.PhienBaoVes
                .Where(p => p.IdHdBaocao == hoiDong!.Id &&
                            p.IdSinhVienDeTaiNavigation != null &&
                            p.IdSinhVienDeTaiNavigation.IdDeTai == idDeTai)
                .ToListAsync();

            foreach (var pb in tatCaPhienCungDeTai)
            {
                var xacNhan = await _context.XacNhanDiemChuTichs
                    .FirstOrDefaultAsync(x => x.IdPhienBaoVe == pb.Id);
                if (xacNhan != null && xacNhan.TrangThai == "DA_XAC_NHAN")
                {
                    xacNhan.TrangThai = "CHO_XAC_NHAN";
                }
            }

            await _context.SaveChangesAsync();
            return new LuuDiemResult { Success = true, Message = "Đã chỉnh sửa điểm thành viên hội đồng thành công! Chủ tịch cần xác nhận lại." };
        }

        public async Task<LuuDiemResult> ChuTichXacNhan(XacNhanDiemRequest request, int currentUserId)
        {
            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.IdDotNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                .FirstOrDefaultAsync(p => p.Id == request.PhienBaoVeId);

            if (phien == null)
                return new LuuDiemResult { Success = false, Message = "Không tìm thấy phiên bảo vệ." };

            var hoiDong = phien.IdHdBaocaoNavigation;
            var thanhVien = hoiDong?.ThanhVienHdBaoCaos?
                .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);

            if (thanhVien?.VaiTro != "CHU_TICH")
                return new LuuDiemResult { Success = false, Message = "Chỉ Chủ tịch mới có quyền xác nhận điểm." };

            // Kiểm tra giai đoạn báo cáo
            var periodCheck = KiemTraGiaiDoanBaoCao(hoiDong);
            if (!periodCheck.ConTrong)
                return new LuuDiemResult { Success = false, Message = "Đã hết giai đoạn báo cáo. Không thể xác nhận điểm." };

            var tatCaDaCham = hoiDong?.ThanhVienHdBaoCaos?
                .All(tv => tv.DaChamDiem == true) ?? false;

            if (!tatCaDaCham)
                return new LuuDiemResult { Success = false, Message = "Chưa tất cả thành viên hội đồng chấm điểm." };

            // Lấy tất cả phiên bảo vệ cùng đề tài để xác nhận cho từng SV
            var idDeTai = phien.IdSinhVienDeTaiNavigation?.IdDeTai ?? 0;
            var tatCaPhienCungDeTai = await _context.PhienBaoVes
                .Include(p => p.IdSinhVienDeTaiNavigation)
                .Where(p => p.IdHdBaocao == hoiDong!.Id &&
                            p.IdSinhVienDeTaiNavigation != null &&
                            p.IdSinhVienDeTaiNavigation.IdDeTai == idDeTai)
                .ToListAsync();

            // Tạo/cập nhật xác nhận cho từng phiên bảo vệ (mỗi SV)
            foreach (var pb in tatCaPhienCungDeTai)
            {
                var xacNhan = await _context.XacNhanDiemChuTichs
                    .FirstOrDefaultAsync(x => x.IdPhienBaoVe == pb.Id);

                if (xacNhan == null)
                {
                    xacNhan = new XacNhanDiemChuTich
                    {
                        IdPhienBaoVe = pb.Id,
                        IdChuTich = currentUserId,
                        TrangThai = "DA_XAC_NHAN",
                        DiemTongKetCuoi = request.DiemTongKet,
                        GhiChu = request.GhiChu,
                        NgayXacNhan = DateTime.Now
                    };
                    _context.XacNhanDiemChuTichs.Add(xacNhan);
                }
                else
                {
                    xacNhan.TrangThai = "DA_XAC_NHAN";
                    xacNhan.DiemTongKetCuoi = request.DiemTongKet;
                    xacNhan.GhiChu = request.GhiChu;
                    xacNhan.NgayXacNhan = DateTime.Now;
                }

                // Tạo lịch sử xác nhận cho từng sinh viên
                var idSinhVien = pb.IdSinhVienDeTaiNavigation?.IdSinhVien;
                if (idSinhVien.HasValue && idSinhVien.Value > 0)
                {
                    _context.LichSuCapNhatDiems.Add(new LichSuCapNhatDiem
                    {
                        IdPhienBaoVe = pb.Id,
                        IdSinhVien = idSinhVien.Value,
                        IdNguoiCapNhat = currentUserId,
                        LoaiCapNhat = "CHU_TICH_XAC_NHAN",
                        DiemMoi = request.DiemTongKet,
                        LyDo = request.GhiChu,
                        NgayCapNhat = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return new LuuDiemResult { Success = true, Message = "Đã xác nhận điểm thành công! Có thể chuyển sang đề tài tiếp theo." };
        }

        public async Task<BangDiemTongHopViewModel?> GetBangDiemTongHop(int phienBaoVeId, int currentUserId)
        {
            _validationError = null;

            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                        .ThenInclude(tv => tv != null ? tv.IdGiangVienNavigation : null)
                            .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.IdDotNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdSinhVienNavigation : null)
                        .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                        .ThenInclude(dt => dt != null ? dt.IdGvhdNavigation : null)
                            .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(p => p.DiemChiTiets)
                    .ThenInclude(d => d.IdNguoiChamNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .FirstOrDefaultAsync(p => p.Id == phienBaoVeId);

            if (phien == null)
            {
                _validationError = "Không tìm thấy phiên bảo vệ.";
                return null;
            }

            var hoiDong = phien.IdHdBaocaoNavigation;
            var svdt = phien.IdSinhVienDeTaiNavigation;
            var sv = svdt?.IdSinhVienNavigation;
            var dt = svdt?.IdDeTaiNavigation;
            var idDeTai = svdt?.IdDeTai ?? 0;

            var diemGVHD = svdt?.DiemGvhd ?? 0;
            var laHoiDongGiuaKy = hoiDong?.LoaiHoiDong == "GIUA_KY";

            // Lấy tất cả phiên bảo vệ cùng đề tài trong hội đồng
            var tatCaPhienCungDeTai = await _context.PhienBaoVes
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(s => s != null ? s.IdSinhVienNavigation : null)
                        .ThenInclude(sv2 => sv2 != null ? sv2.IdNguoiDungNavigation : null)
                .Include(p => p.DiemChiTiets)
                .Where(p => p.IdHdBaocao == hoiDong!.Id &&
                            p.IdSinhVienDeTaiNavigation != null &&
                            p.IdSinhVienDeTaiNavigation.IdDeTai == idDeTai)
                .ToListAsync();

            // Tổng hợp điểm hội đồng (dùng phiên đầu tiên cho backward compat)
            var diemHoiDong = phien.DiemChiTiets
                .GroupBy(d => d.IdNguoiCham)
                .Select(g => new { IdNguoiCham = g.Key, TongDiem = g.Sum(d => d.DiemSo ?? 0) })
                .ToList();

            var diemTBHoiDong = diemHoiDong.Any() ? diemHoiDong.Average(d => d.TongDiem) : 0;

            var chenhLechGVHD = Math.Abs(diemGVHD - diemTBHoiDong);
            var coChenhLechLon = false;

            if (diemHoiDong.Count > 1)
            {
                var diemMax = diemHoiDong.Max(d => d.TongDiem);
                var diemMin = diemHoiDong.Min(d => d.TongDiem);
                if (diemMax - diemMin > 1) coChenhLechLon = true;
            }

            if (!laHoiDongGiuaKy && chenhLechGVHD > 2) coChenhLechLon = true;

            var thanhVienHienTai = hoiDong?.ThanhVienHdBaoCaos?.FirstOrDefault(tv => tv.IdGiangVien == currentUserId);
            var xacNhan = await _context.XacNhanDiemChuTichs.FirstOrDefaultAsync(x => x.IdPhienBaoVe == phienBaoVeId);

            // Tính hết hạn hội đồng và giai đoạn báo cáo cuối kì
            var todayBD = DateOnly.FromDateTime(DateTime.Today);
            var ngayKetThucHD = hoiDong?.NgayKetThuc ?? hoiDong?.NgayBaoCao;
            var daHetHanBaoCao = ngayKetThucHD.HasValue && todayBD > ngayKetThucHD.Value;

            var dotDoAn = hoiDong?.IdDotNavigation;
            bool conTrongGiaiDoanBaoCao;
            if (laHoiDongGiuaKy)
            {
                conTrongGiaiDoanBaoCao = dotDoAn?.NgayBatDauBaoCaoGiuaKi != null &&
                    dotDoAn?.NgayKetThucBaoCaoGiuaKi != null &&
                    todayBD >= dotDoAn.NgayBatDauBaoCaoGiuaKi.Value &&
                    todayBD <= dotDoAn.NgayKetThucBaoCaoGiuaKi.Value;
            }
            else
            {
                conTrongGiaiDoanBaoCao = dotDoAn?.NgayBatDauBaoCaoCuoiKi != null &&
                    dotDoAn?.NgayKetThucBaoCaoCuoiKi != null &&
                    todayBD >= dotDoAn.NgayBatDauBaoCaoCuoiKi.Value &&
                    todayBD <= dotDoAn.NgayKetThucBaoCaoCuoiKi.Value;
            }

            var lichSuCapNhat = await _context.LichSuCapNhatDiems
                .Include(l => l.IdNguoiCapNhatNavigation)
                    .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Where(l => l.IdPhienBaoVe == phienBaoVeId)
                .OrderByDescending(l => l.NgayCapNhat)
                .ToListAsync();

            var thanhVienList = hoiDong?.ThanhVienHdBaoCaos?.Select(tv => new DiemThanhVienTongHopViewModel
            {
                IdGiangVien = tv.IdGiangVien ?? 0,
                TenGiangVien = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                VaiTro = tv.VaiTro ?? "",
                DaChamDiem = tv.DaChamDiem ?? false,
                TongDiem = phien.DiemChiTiets.Where(d => d.IdNguoiCham == tv.IdGiangVien).Sum(d => d.DiemSo ?? 0)
            }).ToList() ?? new List<DiemThanhVienTongHopViewModel>();

            // Build danh sách sinh viên cùng đề tài với điểm theo vai trò
            var danhSachSinhVien = new List<SinhVienBangDiemItem>();
            foreach (var p in tatCaPhienCungDeTai.OrderBy(p => p.IdSinhVienDeTaiNavigation?.IdSinhVienNavigation?.Mssv))
            {
                var svInfo = p.IdSinhVienDeTaiNavigation;
                var svNav = svInfo?.IdSinhVienNavigation;
                var diemGVHDSV = svInfo?.DiemGvhd ?? 0;

                // Tính điểm theo từng thành viên cho sinh viên này
                var diemTheoThanhVien = new Dictionary<int, double?>();
                var diemThanhVienSV = p.DiemChiTiets
                    .GroupBy(d => d.IdNguoiCham)
                    .Select(g => new { IdNguoiCham = g.Key, TongDiem = g.Sum(d => d.DiemSo ?? 0) })
                    .ToList();

                foreach (var tv in hoiDong?.ThanhVienHdBaoCaos ?? Enumerable.Empty<ThanhVienHdBaoCao>())
                {
                    var idGV = tv.IdGiangVien ?? 0;
                    var diemTV = diemThanhVienSV.FirstOrDefault(d => d.IdNguoiCham == tv.IdGiangVien);
                    diemTheoThanhVien[idGV] = diemTV != null && (tv.DaChamDiem ?? false) ? diemTV.TongDiem : null;
                }

                var diemTBHD_SV = diemThanhVienSV.Any() ? diemThanhVienSV.Average(d => d.TongDiem) : 0;
                var diemTBCuoi = laHoiDongGiuaKy
                    ? diemTBHD_SV
                    : Math.Round(diemGVHDSV * 0.3 + diemTBHD_SV * 0.7, 2);

                danhSachSinhVien.Add(new SinhVienBangDiemItem
                {
                    IdSinhVien = svNav?.IdNguoiDung ?? 0,
                    PhienBaoVeId = p.Id,
                    TenSinhVien = svNav?.IdNguoiDungNavigation?.HoTen ?? "",
                    Mssv = svNav?.Mssv ?? "",
                    DiemGVHD = diemGVHDSV,
                    DiemTheoThanhVien = diemTheoThanhVien,
                    DiemTBHoiDong = diemTBHD_SV,
                    DiemTrungBinhCuoi = diemTBCuoi
                });
            }

            return new BangDiemTongHopViewModel
            {
                PhienBaoVeId = phien.Id,
                HoiDongId = hoiDong?.Id ?? 0,
                TenHoiDong = hoiDong?.TenHoiDong ?? "",
                LoaiHoiDong = hoiDong?.LoaiHoiDong ?? "",
                TenSinhVien = sv?.IdNguoiDungNavigation?.HoTen ?? "",
                Mssv = sv?.Mssv ?? "",
                TenDeTai = dt?.TenDeTai ?? "",
                MaDeTai = dt?.MaDeTai ?? "",
                TenGVHD = dt?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                DiemGVHD = diemGVHD,
                NhanXetGVHD = svdt?.NhanXetGvhd ?? "",
                DanhSachSinhVien = danhSachSinhVien,
                DiemThanhViens = thanhVienList,
                DiemTBHoiDong = diemTBHoiDong,
                ChenhLechGVHD = chenhLechGVHD,
                CoChenhLechLon = coChenhLechLon,
                TrangThaiXacNhan = xacNhan?.TrangThai ?? "CHO_XAC_NHAN",
                DiemTongKetCuoi = xacNhan?.DiemTongKetCuoi,
                GhiChuXacNhan = xacNhan?.GhiChu ?? "",
                LichSuCapNhat = lichSuCapNhat.Select(l => new LichSuCapNhatViewModel
                {
                    TenNguoiCapNhat = l.IdNguoiCapNhatNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                    LoaiCapNhat = l.LoaiCapNhat ?? "",
                    DiemCu = l.DiemCu,
                    DiemMoi = l.DiemMoi,
                    LyDo = l.LyDo ?? "",
                    NgayCapNhat = l.NgayCapNhat
                }).ToList(),
                LaThuKy = thanhVienHienTai?.VaiTro == "THU_KY",
                LaChuTich = thanhVienHienTai?.VaiTro == "CHU_TICH",
                DaHetHanBaoCao = daHetHanBaoCao,
                ConTrongGiaiDoanBaoCao = conTrongGiaiDoanBaoCao
            };
        }

        /// <summary>
        /// Kiểm tra hội đồng còn trong giai đoạn báo cáo (DotDoAn) hay không.
        /// Dùng cho ràng buộc Thư ký / Chủ tịch.
        /// </summary>
        private static (bool ConTrong, string? Message) KiemTraGiaiDoanBaoCao(HoiDongBaoCao? hoiDong)
        {
            if (hoiDong == null)
                return (false, "Không tìm thấy hội đồng.");

            var today = DateOnly.FromDateTime(DateTime.Today);
            var dotDoAn = hoiDong.IdDotNavigation;

            // Ưu tiên dùng ngày của DotDoAn, fallback về ngày hội đồng
            if (dotDoAn != null)
            {
                DateOnly? ngayBatDau, ngayKetThuc;
                if (hoiDong.LoaiHoiDong == "GIUA_KY")
                {
                    ngayBatDau = dotDoAn.NgayBatDauBaoCaoGiuaKi;
                    ngayKetThuc = dotDoAn.NgayKetThucBaoCaoGiuaKi;
                }
                else
                {
                    ngayBatDau = dotDoAn.NgayBatDauBaoCaoCuoiKi;
                    ngayKetThuc = dotDoAn.NgayKetThucBaoCaoCuoiKi;
                }

                if (ngayBatDau.HasValue && ngayKetThuc.HasValue)
                {
                    if (today >= ngayBatDau.Value && today <= ngayKetThuc.Value)
                        return (true, null);
                    else
                        return (false, $"Ngoài giai đoạn báo cáo ({ngayBatDau.Value:dd/MM/yyyy} - {ngayKetThuc.Value:dd/MM/yyyy}).");
                }
            }

            // Fallback: dùng ngày hội đồng
            var ngayKetThucHD = hoiDong.NgayKetThuc ?? hoiDong.NgayBaoCao;
            if (ngayKetThucHD.HasValue && today <= ngayKetThucHD.Value)
                return (true, null);

            return (false, "Đã hết hạn báo cáo.");
        }
    }
}
