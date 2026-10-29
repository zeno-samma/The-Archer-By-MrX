using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    /// <summary>Read-only asset inspection. Never rebuilds or restyles a prefab.</summary>
    public static class UIInspectorAuthoringAudit
    {
        [MenuItem("Tools/MrX Studio/UI/Audit Inspector Authoring")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before auditing authored assets.");
            var report = new StringBuilder("# Kiểm kê prefab và khả năng chỉnh Inspector\n\n");
            report.AppendLine("Tạo lúc " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ". Chỉ đọc asset, không áp lại giao diện hoặc đổi save.\n");
            var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/The Archer" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)).OrderBy(p => p).ToArray();
            int ui = 0, missing = 0;
            var inventory = new StringBuilder("| Prefab | Có UI | Component script bị thiếu |\n| --- | --- | ---: |\n");
            foreach (var path in prefabs)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null) continue;
                bool hasUI = asset.GetComponentInChildren<RectTransform>(true) != null;
                int broken = asset.GetComponentsInChildren<Component>(true).Count(c => c == null);
                if (hasUI) ui++;
                missing += broken;
                inventory.AppendLine("| `" + path + "` | " + (hasUI ? "Có" : "—") + " | " + broken + " |");
            }
            var files = Directory.GetFiles("Assets/The Archer/Scripts", "*.cs", SearchOption.AllDirectories);
            var findings = new List<string>();
            var visibilityWrites = new List<string>();
            var presentationWrites = new List<string>();
            foreach (var file in files)
            {
                if (file.Replace('\\', '/').Contains("/Editor/")) continue;
                var blocks = new Stack<bool>(); bool editorOnly = false; int line = 0;
                foreach (var source in File.ReadLines(file))
                {
                    line++; string s = source.Trim();
                    if (s.StartsWith("#if ")) { blocks.Push(editorOnly); editorOnly |= s.Contains("UNITY_EDITOR") && !s.Contains("!UNITY_EDITOR"); continue; }
                    if (s.StartsWith("#endif")) { if (blocks.Count > 0) editorOnly = blocks.Pop(); continue; }
                    if (s.StartsWith("#else") || s.StartsWith("#elif")) { editorOnly = blocks.Count > 0 && blocks.Peek(); continue; }
                    if (editorOnly || s.StartsWith("//")) continue;
                    if (Regex.IsMatch(s, @"new\s+GameObject.*typeof\(RectTransform\)|AddComponent<(?:Image|RawImage|TextMeshProUGUI|Button|Canvas|ScrollRect|RectMask2D|VerticalLayoutGroup|HorizontalLayoutGroup|GridLayoutGroup|ContentSizeFitter|GraphicRaycaster)>"))
                        findings.Add("- `" + file.Replace('\\', '/') + ":" + line + "`: `" + s.Replace("`", "'") + "`");
                    if (!file.Replace('\\', '/').Contains("/UI/")) continue;
                    string entry = "- `" + file.Replace('\\', '/') + ":" + line + "`: `" + s.Replace("`", "'") + "`";
                    if (Regex.IsMatch(s, @"\.SetActive\(|\.enabled\s*=")) visibilityWrites.Add(entry);
                    if (Regex.IsMatch(s, @"\.(?:sprite|color|fontSize|fontSizeMin|fontSizeMax|enableAutoSizing|localScale|sizeDelta|anchoredPosition|anchorMin|anchorMax|pixelsPerUnitMultiplier|overrideSorting)\s*=")) presentationWrites.Add(entry);
                }
            }
            report.AppendLine("- Prefab đã đọc: **" + prefabs.Length + "**; prefab có RectTransform: **" + ui + "**.");
            report.AppendLine("- Component script bị thiếu: **" + missing + "**.");
            report.AppendLine("- Source C# đã quét: **" + files.Length + "**; dấu hiệu dựng UI runtime cần xem lại: **" + findings.Count + "**.\n");
            report.AppendLine("Quét source là kiểm tra tĩnh, không chứng minh mọi callback hay mọi thiết bị hoạt động. Các hàm authoring trong #if UNITY_EDITOR không được tính là dựng UI runtime. Danh sách động, số tiến trình, nội dung bản dịch và hiệu ứng vẫn cần script binding.\n");
            report.AppendLine("## Dấu hiệu dựng UI runtime\n");
            report.AppendLine(findings.Count == 0 ? "Không thấy trong các mẫu API UI được kiểm tra.\n" : string.Join("\n", findings.ToArray()));
            report.AppendLine("## Ghi trạng thái hiển thị runtime cần đối chiếu\n");
            report.AppendLine("Có " + visibilityWrites.Count + " dòng. Đây là danh sách cần phân loại, không đồng nghĩa mọi dòng là lỗi: popup/page/pool có vòng đời mở đóng; nội dung/progress có binding. Decoration phải qua InspectorVisibility; component tắt không được tự bật lại.\n");
            report.AppendLine(string.Join("\n", visibilityWrites.ToArray()));
            report.AppendLine("\n## Ghi trình bày runtime cần đối chiếu\n");
            report.AppendLine("Có " + presentationWrites.Count + " dòng. Kiểm tra mốc Inspector của animation, tùy chọn Use State Colors/Auto Fit, bố cục responsive đã author và nội dung từ database. Kiểm kê construction bằng 0 không chứng minh Inspector được tôn trọng.\n");
            report.AppendLine(string.Join("\n", presentationWrites.ToArray()));
            report.AppendLine("\n## Danh sách prefab\n"); report.Append(inventory);
            Directory.CreateDirectory("Docs");
            const string output = "Docs/UI Inspector Asset Audit.md";
            File.WriteAllText(output, report.ToString(), new UTF8Encoding(false));
            Debug.Log("Inspector authoring audit: " + prefabs.Length + " prefabs; missing scripts=" + missing + "; runtime UI construction candidates=" + findings.Count + ". " + Path.GetFullPath(output));
        }
    }
}
