#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DTileMap;


[CustomEditor(typeof(TileMap))]
public class DTileMapEditor : Editor
{
    Vector3[] _tmpLines = null;
    bool _isPainting = false;

    void paint(Vector3Int pos)
    {
        Debug.Log(pos.x + ":" + pos.y);
        TileMap tilemap = (TileMap)target;
        tilemap.SetTile(pos.x, pos.y, 2);
    }

    public void OnSceneGUI()
    {
        if (Tools.current != Tool.Custom) return;

        TileMap tilemap = (TileMap)target;
        Handles.color = Color.green;
        Handles.PositionHandle(Vector3.zero, Quaternion.identity);

        Event e = Event.current;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); // ← 選択操作を無効化

        Vector3Int basePos = Vector3Int.zero;
        // 
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        var plane = new Plane(Vector3.forward, 0f);
        float enter;
        if (plane.Raycast(ray, out enter))
        {
            basePos = Vector3Int.FloorToInt(ray.origin + ray.direction * enter);
        }

        if (e.type == EventType.Repaint)
        {

            _tmpLines = new Vector3[(tilemap.Width + 1) * 2 + (tilemap.Height + 1) * 2];
            int ofs = 0;
            // 縦ライン
            for (int i = 0; i <= tilemap.Width; ++i)
            {
                _tmpLines[ofs++] = new Vector3(i, 0f, 0f);
                _tmpLines[ofs++] = new Vector3(i, tilemap.Height, 0f);
            }
            // 横ライン
            for (int i = 0; i <= tilemap.Height; ++i)
            {
                _tmpLines[ofs++] = new Vector3(0f, i, 0f);
                _tmpLines[ofs++] = new Vector3(tilemap.Width, i, 0f);
            }
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Handles.DrawLines(_tmpLines);

            Handles.DrawAAConvexPolygon(basePos, basePos + Vector3.right, basePos + new Vector3(1f, 1f, 0f), basePos + Vector3.up);
        }
        else if (e.type == EventType.MouseDown && e.button == 0)
        {
            // 左クリック押し始め → ペイント開始
            _isPainting = true;
            paint(basePos);
            e.Use(); // ← Unityの選択イベントを奪う
        }
        else if (e.type == EventType.MouseDrag && e.button == 0 && _isPainting)
        {
            // ドラッグ中もペイント
            paint(basePos);
            e.Use(); // ← Unityの選択イベントを奪う
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            // ボタン離したら終了
            _isPainting = false;
            e.Use();
        }
        else if (e.type == EventType.Used)
        {
            //
            e.Use();
        }
        else if (e.type == EventType.Layout)
        {
        }
        else if (e.type == EventType.MouseMove)
        {
        }
        else
        {
            Debug.Log("other event:" + e.type);
        }
        SceneView.RepaintAll();
    }
}

#endif
