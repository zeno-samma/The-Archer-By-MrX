using System.IO;
using UnityEditor;
using UnityEngine;

public static class CharacterScreenshotExporter
{
    [MenuItem("Tools/Character/Export PNG")]
    private static void ExportPNG()
    {
        RenderTexture source = Selection.activeObject as RenderTexture;

        if (source == null)
        {
            Debug.LogError("Hãy chọn RangerRenderTexture trước!");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Export Character",
            "Ranger",
            "png",
            "Chọn nơi lưu ảnh"
        );

        if (string.IsNullOrEmpty(path)) return;

        RenderTexture temp = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.ARGB32
        );

        RenderTexture previous = RenderTexture.active;
        Texture2D texture = null;

        try
        {
            // Resolve MSAA 8x trước khi đọc ảnh
            Graphics.Blit(source, temp);
            RenderTexture.active = temp;

            texture = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false
            );

            texture.ReadPixels(
                new Rect(0, 0, source.width, source.height),
                0, 0
            );

            texture.Apply();

            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            Debug.Log("Export PNG thành công: " + path);
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temp);

            if (texture != null)
                Object.DestroyImmediate(texture);
        }
    }
}