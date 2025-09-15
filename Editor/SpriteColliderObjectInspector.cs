#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DTileMap;

namespace DEditor
{
    [CustomEditor(typeof(SpriteColliderObject))]
    public class SpriteColliderObjectInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            // --- ボタンを追加 ---
            var myScript = (SpriteColliderObject)target;
            if (GUILayout.Button("Open"))
            {
                //                myScript.MyFunction(); // ボタンが押された時の処理
                SpriteColliderEditor.Init();
            }
        }
    }
}


#endif