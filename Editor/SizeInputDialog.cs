using UnityEditor;
using UnityEngine;
using System;

public class SizeInputDialog : EditorWindow
{
    int width = 512;
    int height = 512;

    Action<int, int> onSubmit;

    public static void Show(Action<int, int> onSubmit,int defaultWidth,int defaultHeight)
    {
        var window = CreateInstance<SizeInputDialog>();
        window.titleContent = new GUIContent("Size Input");
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 100);
        window.onSubmit = onSubmit;
        window.width = defaultWidth;
        window.height = defaultHeight;
        window.ShowUtility();
    }

    void OnGUI()
    {
        GUILayout.Label("Enter Size", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {
            onSubmit?.Invoke(width, height);
            Close();
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        GUILayout.EndHorizontal();
    }
}