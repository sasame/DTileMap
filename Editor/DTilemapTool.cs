#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace DEditor
{
    [EditorTool("DTilemapTool")]
    public class DTilemapTool : EditorTool
    {
        public Texture2D toolIcon;

        public override GUIContent toolbarIcon
        {
            get
            {
                return new GUIContent(toolIcon, "DTilemapTool");
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            // Sceneビュー上での描画や操作を記述
            Handles.color = Color.red;
            Handles.DrawWireCube(Vector3.zero, Vector3.one * 2);

            // Sceneビューのイベントを拾って独自操作も可能
/*            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Debug.Log("Clicked in SceneView!");
            }*/
        }
    }

}


#endif

