using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PicMaker : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private static int _counter;

    [MenuItem("Custom Tools/PicMaker")]
    public static void ShowExample()
    {
        PicMaker wnd = GetWindow<PicMaker>();
        wnd.titleContent = new GUIContent("PickMaker");


    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = GetWindow<PicMaker>().rootVisualElement;
        var asset = m_VisualTreeAsset.Instantiate();
        root.Add(asset);
        var picBtn = root.Q<Button>("pic-btn");

        picBtn.clicked += PicMaker.MakePic;
        var resetCounter = new Button(ResetCounter);
        resetCounter.text = "Reset counter";
        root.Add(resetCounter);
        _counter = 0;

    }
    
    [MenuItem("Custom Tools/Make Pic #p")]
    private static void MakePic()
    {
        Debug.Log("Make Screenshot");
        var size = Handles.GetMainGameViewSize();
        var x = size.x;
        var y = size.y;
        var name = $"{size.x}x{size.y}_{_counter++}.png";

        ScreenCapture.CaptureScreenshot(name);
    }

    private void ResetCounter()
    {
        _counter = 0;
        Debug.Log($"Counter reset to: {_counter}");
    }

}
