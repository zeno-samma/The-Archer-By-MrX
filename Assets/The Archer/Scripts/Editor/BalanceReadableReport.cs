using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;

namespace MrX.EditorTools
{
    // Offline report, not a runtime game interface. No network or player-save writes.
    public static class BalanceReadableReport
    {
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        private static string H(string value) => WebUtility.HtmlEncode(value ?? "Không rõ");
        private static string N(float value) => value.ToString("0.#", Vi);
        private static float Median(IEnumerable<float> values)
        {
            var sorted = values.OrderBy(x => x).ToArray();
            return sorted.Length == 0 ? 0 : (sorted[(sorted.Length - 1) / 2] + sorted[sorted.Length / 2]) / 2;
        }
        private static string Duration(float seconds)
        {
            return seconds < 60 ? N(seconds) + " giây" : ((int)seconds / 60) + " phút " + ((int)seconds % 60) + " giây";
        }
        public static string Render(ConsoleTimer.RunMeasurement[] runs, int excluded)
        {
            var stages = new Dictionary<string, string>();
            foreach (var guid in AssetDatabase.FindAssets("t:StageData"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<StageData>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null && !string.IsNullOrEmpty(asset.StageId)) stages[asset.StageId] = asset.StageName;
            }
            var html = new StringBuilder(@"<!doctype html><html lang='vi'><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>
<title>Báo cáo cân bằng — MrX Studio</title><style>
*{box-sizing:border-box}body{margin:0;background:#f3f6fb;color:#24324b;font:16px/1.6 system-ui,sans-serif}main{max-width:1180px;margin:auto;padding:32px 24px}h1{font-size:32px;margin:4px 0}h2{font-size:23px;margin:0 0 10px}h3{font-size:18px}p{margin:8px 0}.muted{color:#64748b}.eyebrow{font-size:12px;font-weight:700;letter-spacing:2px;color:#3564ca}.cards{display:grid;grid-template-columns:repeat(3,1fr);gap:16px;margin:24px 0}.card,section{background:white;border:1px solid #dce4ef;border-radius:16px;padding:22px;margin-bottom:20px}.cards .card{margin:0}.number{font-size:32px;font-weight:750;color:#2554b3}.notice{background:#fff5d9;border-left:4px solid #d49b20;padding:14px 18px;border-radius:8px;margin:16px 0}.good{background:#ebf8f1}.pill{display:inline-block;border-radius:20px;padding:3px 10px;background:#eaf1ff;color:#295bb9;font-size:13px}.table{overflow:auto}table{border-collapse:collapse;width:100%;font-size:14px}th,td{text-align:left;vertical-align:top;padding:14px 10px;border-bottom:1px solid #e6ebf3}th{background:#f1f5fb;white-space:nowrap}td:first-child{min-width:110px}.bar{height:7px;background:#dce7f8;border-radius:8px;margin-top:7px;max-width:150px}.bar i{display:block;background:#4e86e5;height:100%;border-radius:8px}summary{cursor:pointer;font-weight:650}li{margin:7px 0}input{font:inherit;padding:12px 16px;border-radius:10px;border:1px solid #cbd6e5;width:100%;margin:8px 0 24px}small{font-size:12px}@media(max-width:680px){.cards{grid-template-columns:1fr}main{padding:20px 12px}h1{font-size:26px}}
</style><main><div class='eyebrow'>MRX STUDIO · CÔNG CỤ PHÁT TRIỂN</div><h1>Trận đấu đang có nhịp độ thế nào?</h1><p class='muted'>Đọc phần “Cần xem trước” của từng màn. Mở bảng chi tiết khi cần đối chiếu số liệu.</p>");
            int wins = runs.Count(r => r.outcome == "FinishAllRoom");
            int deaths = runs.Count(r => r.outcome == "Die");
            html.Append("<div class='cards'><div class='card'>Phiên đã ghi nhận<div class='number'>").Append(runs.Length)
                .Append("</div><small>Chỉ các phiên được bộ lọc chấp nhận</small></div><div class='card'>Thắng / Chết<div class='number'>")
                .Append(wins).Append(" / ").Append(deaths).Append("</div><small>Còn lại: ").Append(runs.Length - wins - deaths)
                .Append(" phiên thoát hoặc buộc dừng</small></div><div class='card'>Thời gian một run thắng<div class='number'>")
                .Append(wins > 0 ? Duration(Median(runs.Where(r => r.outcome == "FinishAllRoom").Select(r => r.activeSeconds))) : "Chưa có")
                .Append("</div><small>Mức điển hình; không trộn thời gian run thua</small></div></div>");
            html.Append("<p class='muted'>Đã bỏ qua ").Append(excluded).Append(" báo cáo chơi tiếp, kiểm thử cô lập hoặc không thuộc kết quả hỗ trợ. Các báo cáo thử cũ chưa có cờ nhận diện vẫn có thể lọt vào.</p>");
            html.Append("<div class='notice'><strong>Đừng vội chỉnh độ khó khi mới có 1–2 lần chơi.</strong> Báo cáo chỉ ra nơi cần kiểm tra, không tự kết luận nguyên nhân. So sánh các run có cùng level, trang bị và nâng cấp; dữ liệu cũ chưa lưu các thông tin này.</div>");
            if (runs.Length == 0) html.Append("<section><h2>Chưa có dữ liệu phù hợp</h2><p>Chơi một run và kết thúc bằng thắng, chết hoặc về menu, sau đó chạy lại công cụ. Bấm Stop Play trực tiếp không tạo báo cáo kết thúc.</p></section>");
            else html.Append("<label for='search'><strong>Tìm màn hoặc nhân vật</strong></label><input id='search' placeholder='Ví dụ: Ranger hoặc tên màn…' oninput='filterReports(this.value)'>");
            foreach (var group in runs.GroupBy(r => new { r.stage, r.hero, r.difficulty }))
            {
                string stage = stages.TryGetValue(group.Key.stage ?? "", out var name) ? name : "Màn chưa có tên (" + group.Key.stage + ")";
                string hero = (group.Key.hero ?? "Không rõ nhân vật").Replace("(Clone)", "").Trim();
                hero += " · " + (group.Key.difficulty == 0 ? "Thường" : group.Key.difficulty == 1 ? "Khó" : "Ác mộng");
                var waves = group.SelectMany(r => r.waves).Where(w => w != null).ToArray();
                var buckets = waves.GroupBy(w => new { w.room, w.wave, w.boss }).OrderBy(g => g.Key.room).ThenBy(g => g.Key.wave).ToArray();
                html.Append("<section class='report' data-search='").Append(H(stage + " " + hero)).Append("'><span class='pill'>")
                    .Append(group.Count()).Append(" phiên</span><h2>").Append(H(stage)).Append(" · ").Append(H(hero)).Append("</h2>");
                html.Append("<h3>Cần xem trước</h3><ul>");
                if (group.Count() < 5) html.Append("<li><strong>Ít dữ liệu:</strong> mới có ").Append(group.Count()).Append(" phiên. Hãy thu thêm dữ liệu trước khi quyết định tăng/giảm HP hoặc sát thương.</li>");
                var cleared = buckets.Where(g => g.Any(w => w.cleared)).OrderByDescending(g => Median(g.Where(w => w.cleared).Select(w => w.seconds))).FirstOrDefault();
                if (cleared != null) html.Append("<li><strong>Đợt đã vượt qua lâu nhất:</strong> phòng ").Append(cleared.Key.room).Append(", đợt ")
                    .Append(cleared.Key.wave).Append(cleared.Key.boss ? " (boss)" : " (quái thường)").Append(" — thường mất ")
                    .Append(Duration(Median(cleared.Where(w => w.cleared).Select(w => w.seconds)))).Append(". Xem lại HP quái, đường đi và thời gian phải né; chưa đủ để kết luận quái quá trâu.</li>");
                var painful = buckets.OrderByDescending(g => g.Average(w => w.damageTaken)).FirstOrDefault();
                if (painful != null && painful.Average(w => w.damageTaken) > 0)
                    html.Append("<li><strong>Mất nhiều HP nhất:</strong> phòng ").Append(painful.Key.room).Append(", đợt ").Append(painful.Key.wave)
                        .Append(" — trung bình mất ").Append(N((float)painful.Average(w => w.damageTaken))).Append(" HP mỗi lần gặp. Kiểm tra dấu hiệu báo đòn và các đòn đánh chồng nhau.</li>");
                else html.Append("<li><strong>Chưa ghi nhận mất HP ở các đợt:</strong> kiểm tra có đang dùng bất tử/test hay không; không suy ra game quá dễ chỉ từ số 0.</li>");
                int incomplete = waves.Count(w => !w.cleared);
                if (incomplete > 0) html.Append("<li>Có ").Append(incomplete).Append(" bản ghi đợt chưa vượt qua. Có thể do chết, thoát hoặc buộc dừng; không coi tất cả là chết.</li>");
                html.Append("</ul><details><summary>Xem từng đợt · ").Append(buckets.Length).Append(" dòng</summary><div class='table'><table><thead><tr><th>Vị trí</th><th>Đã vượt qua</th><th>Thường mất bao lâu?</th><th>HP mất mỗi lần gặp</th><th>Đọc kết quả</th></tr></thead><tbody>");
                float longest = Math.Max(1, buckets.Select(g => Median(g.Where(w => w.cleared).Select(w => w.seconds))).DefaultIfEmpty(1).Max());
                foreach (var bucket in buckets)
                {
                    int count = bucket.Count(), passes = bucket.Count(w => w.cleared);
                    float seconds = Median(bucket.Where(w => w.cleared).Select(w => w.seconds));
                    html.Append("<tr><td><strong>Phòng ").Append(bucket.Key.room).Append(" · Đợt ").Append(bucket.Key.wave).Append("</strong><br>")
                        .Append(bucket.Key.boss ? "Boss" : "Quái thường / rương").Append("</td><td>").Append(passes).Append(" / ").Append(count)
                        .Append(" lần gặp</td><td>").Append(passes == 0 ? "Chưa có lần vượt qua" : Duration(seconds));
                    if (passes > 0) html.Append("<div class='bar'><i style='width:").Append((seconds / longest * 100).ToString("0", CultureInfo.InvariantCulture)).Append("%'></i></div>");
                    html.Append("</td><td>").Append(N((float)bucket.Average(w => w.damageTaken))).Append(" HP<br><small>")
                        .Append(N((float)bucket.Average(w => w.hitsTaken))).Append(" lần trúng đòn</small></td><td>")
                        .Append(count < 5 ? "Ít mẫu — xem thêm vài run." : passes == 0 ? "Chưa có dữ liệu thời gian clear; xem kết quả các phiên." : "Đối chiếu cùng loại đợt và cùng bộ trang bị.")
                        .Append("</td></tr>");
                }
                html.Append("</tbody></table></div></details></section>");
            }
            html.Append(@"<section><h2>Cách hiểu các con số</h2><p><strong>Thường mất bao lâu:</strong> lấy thời gian ở giữa các lần đã vượt qua (trung vị). Một lần chơi quá lâu sẽ ít làm lệch con số này. Không có lần vượt qua thì không có thời gian clear, không phải 0 giây.</p><p><strong>Đã vượt qua:</strong> số lần hoàn tất đợt trên tổng số lần gặp đợt đó; không phải tỷ lệ thắng cả màn.</p><p><strong>HP mất:</strong> lượng máu thực sự bị trừ, sau né và giảm sát thương. Đây là HP tuyệt đối, chưa phải phần trăm máu tối đa.</p><p><strong>Thời gian:</strong> không tính lúc pause bằng timeScale=0. Đợt quái tính từ bắt đầu spawn đến quái cuối chết; đợt chỉ có rương tính tới khi hoàn tất rương.</p><p><strong>Thanh xanh:</strong> so độ dài thời gian giữa các đợt trong cùng nhóm, không phải thanh đánh giá tốt/xấu. Boss thường lâu hơn quái thường.</p></section><p class='muted'>Dữ liệu gốc và CSV vẫn được giữ. Báo cáo này không thay đổi cấu hình game hoặc save. Muốn cập nhật số liệu: chạy lại Summarize Run Reports.</p></main><script>function filterReports(value){value=value.toLocaleLowerCase('vi');document.querySelectorAll('.report').forEach(function(el){el.hidden=!el.dataset.search.toLocaleLowerCase('vi').includes(value)})}</script></html>");
            return html.ToString();
        }
    }
}
