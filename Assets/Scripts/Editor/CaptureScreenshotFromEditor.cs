using UnityEditor;
using UnityEngine;

/// <summary>
/// Unityエディタ上からGameビューのスクリーンショットを撮るEditor拡張
/// </summary>
public class CaptureScreenshotFromEditor : Editor
{
    /// <summary>
    /// キャプチャを撮る
    /// </summary>
    /// <remarks>
    /// Edit > CaptureScreenshot に追加。
    /// HotKeyは Ctrl + Shift + F12。
    /// </remarks>
    [MenuItem("Edit/CaptureScreenshot #%F12")]
    private static void CaptureScreenshot()
    {
        // 現在時刻からファイル名を決定
        var filename = System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";
        // キャプチャを撮る
        ScreenCapture.CaptureScreenshot(filename); // ← GameViewにフォーカスがない場合、この時点では撮られない
        // GameViewを取得してくる
        var assembly = typeof(UnityEditor.EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        var gameview = EditorWindow.GetWindow(type);
        // GameViewを再描画
        gameview.Repaint();

        Debug.Log("ScreenShot: " + filename);
    }
}