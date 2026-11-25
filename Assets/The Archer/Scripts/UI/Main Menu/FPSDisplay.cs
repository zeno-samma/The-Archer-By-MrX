using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;
    [SerializeField] private TMPro.TMP_Text fpsText;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fpsText.text = string.Format("{0:0}", 1.0f / deltaTime);
    }

    // void OnGUI()
    // {
    //     int w = Screen.width, h = Screen.height;
    //     GUIStyle style = new GUIStyle();

    //     Rect rect = new Rect(20, 20, w, h * 2 / 100);
    //     style.alignment = TextAnchor.UpperLeft;
    //     style.fontSize = h * 4 / 100;
    //     style.normal.textColor = Color.green;
    //     float msec = deltaTime * 1000.0f;
    //     float fps = 1.0f / deltaTime;
    //     string text = string.Format("{0:0}", fps);
    //     GUI.Label(rect, text, style);
    // }
}