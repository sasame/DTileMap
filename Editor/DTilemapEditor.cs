#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DTileMap;


[CustomEditor(typeof(DTilemapLayer))]
public class DTileMapEditor : Editor
{
    Vector3[] _tmpLines = null;
    bool _isPainting = false;

    void paint(Vector3Int pos)
    {
//        Debug.Log(pos.x + ":" + pos.y);
        DTilemapLayer tilemap = (DTilemapLayer)target;
        var spCollider = tilemap.SpriteCollider;
        var window = EditorWindow.GetWindow<DTilemapEditorWindow>();
        if (window)
        {
            var sel = window.GetSelctionTile();
            for (int y = sel.Item1.y; y <= sel.Item2.y; ++y)
            {
                for (int x = sel.Item1.x; x <= sel.Item2.x; ++x)
                {
                    var ofsx = x - sel.Item1.x;
                    var ofsy = y - sel.Item1.y;
                    tilemap.SetTile(pos.x + ofsx, pos.y + ofsy, x + y * spCollider.CellCountX);
                }
            }
        }
    }

    public void OnSceneGUI()
    {
        DTilemapLayer tilemap = (DTilemapLayer)target;
        var prevMatrix = Handles.matrix;
        Handles.matrix = tilemap.transform.localToWorldMatrix;
        displayCollider();
        if (Tools.current != Tool.Custom)
        {
            Handles.matrix = prevMatrix;
            return;
        }

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
            var pos3D = ray.origin + ray.direction * enter;
//            basePos = Vector3Int.RoundToInt(tilemap.transform.localToWorldMatrix.MultiplyPoint((Vector3)basePos));
            basePos = Vector3Int.FloorToInt(tilemap.transform.worldToLocalMatrix.MultiplyPoint(pos3D));
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

            var window = EditorWindow.GetWindow<DTilemapEditorWindow>();
            if (window)
            {
                var sel = window.GetSelctionTile();
                var dif = sel.Item2 - sel.Item1;
                var sizeX = dif.x + 1f;
                var sizeY = dif.y + 1f;
                Handles.DrawAAConvexPolygon(basePos, basePos + Vector3.right * sizeX, basePos + new Vector3(dif.x + 1f, dif.y + 1f, 0f), basePos + Vector3.up * sizeY);
            }
            else
            {
                Handles.DrawAAConvexPolygon(basePos, basePos + Vector3.right, basePos + new Vector3(1f, 1f, 0f), basePos + Vector3.up);
            }

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
        Handles.matrix = prevMatrix;
    }

    void colliderLine(Vector3 a, Vector3 b)
    {
        Handles.DrawLine(a, b);
    }

    void drawPolygon(Vector2 a, Vector2 b, Vector2 c)
    {
        Handles.DrawAAConvexPolygon(a, b, c);
    }
    void drawPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Handles.DrawAAConvexPolygon(a, b, c, d);
    }

    void displayCollider()
    {
        DTilemapLayer tilemap = (DTilemapLayer)target;
        var spCollider = tilemap.SpriteCollider;
        if (!spCollider) return;

        var w = tilemap.Width;
        var h = tilemap.Height;
        var tiles = tilemap.Tiles;
        var tileSize = tilemap.TileSize;
        var margin = 0.1f;
        var amount = 1f - margin * 2f;
        Handles.color = new Color(0.5f,1f,0.5f,0.5f);
        var ofs00 = new Vector3(margin, margin, 0f) * tileSize;
        var ofs10 = new Vector3(margin + amount * 0.5f, margin, 0f) * tileSize;
        var ofs12 = new Vector3(margin + amount * 0.5f, margin + amount, 0f) * tileSize;
        var ofs20 = new Vector3(margin + amount, margin, 0f) * tileSize;
        var ofs01 = new Vector3(margin, margin + amount * 0.5f, 0f) * tileSize;
        var ofs02 = new Vector3(margin, margin + amount, 0f) * tileSize;
        var ofs21 = new Vector3(margin + amount, margin + amount * 0.5f, 0f) * tileSize;
        var ofs22 = new Vector3(margin + amount, margin + amount, 0f) * tileSize;
        for (int y=0;y<h;++y)
        {
            for (int x = 0; x < w; ++x)
            {
                var basePos = new Vector3(x, y, 0f) * tileSize;
                var t = tiles[x + y * w];
                var cellInfo = spCollider.Get(new Vector2Int(t % spCollider.CellWidth, t / spCollider.CellWidth));
                if (cellInfo!=null)
                {
                    var c = cellInfo.Collision;
                    switch(c)
                    {
                        case CellCollision.Box:
                            drawPolygon(basePos + ofs00, basePos + ofs02, basePos + ofs22, basePos + ofs20);
                            break;
                        case CellCollision.Angle45_LB:
                            drawPolygon(basePos + ofs00, basePos + ofs02, basePos + ofs02);
                            break;
                        case CellCollision.Angle45_RB:
                            drawPolygon(basePos + ofs20, basePos + ofs00, basePos + ofs02);
                            break;
                        case CellCollision.Angle45_LT:
                            drawPolygon(basePos + ofs02, basePos + ofs22, basePos + ofs00);
                            break;
                        case CellCollision.Angle45_RT:
                            drawPolygon(basePos + ofs22, basePos + ofs20, basePos + ofs02);
                            break;
                        case CellCollision.Angle22_LB1:
                            drawPolygon(basePos + ofs00, basePos + ofs01, basePos + ofs20);
                            break;
                        case CellCollision.Angle22_LB2:
                            drawPolygon(basePos + ofs00, basePos + ofs02, basePos + ofs21, basePos + ofs20);
                            break;
                        case CellCollision.Angle22_RB1:
                            drawPolygon(basePos + ofs20, basePos + ofs00, basePos + ofs21);
                            break;
                        case CellCollision.Angle22_RB2:
                            drawPolygon(basePos + ofs20, basePos + ofs00, basePos + ofs01, basePos + ofs22);
                            break;
                        case CellCollision.Angle22_LT1:
                            drawPolygon(basePos + ofs02, basePos + ofs20, basePos + ofs01);
                            break;
                        case CellCollision.Angle22_LT2:
                            drawPolygon(basePos + ofs02, basePos + ofs20, basePos + ofs21, basePos + ofs00);
                            break;
                        case CellCollision.Angle22_RT1:
                            drawPolygon(basePos + ofs22, basePos + ofs21, basePos + ofs02);
                            break;
                        case CellCollision.Angle22_RT2:
                            drawPolygon(basePos + ofs22, basePos + ofs20, basePos + ofs01, basePos + ofs02);
                            break;
                        case CellCollision.Half_B:
                            drawPolygon(basePos + ofs00, basePos + ofs01, basePos + ofs21, basePos + ofs20);
                            break;
                        case CellCollision.Half_L:
                            drawPolygon(basePos + ofs00, basePos + ofs02, basePos + ofs12, basePos + ofs10);
                            break;
                        case CellCollision.Half_R:
                            drawPolygon(basePos + ofs20, basePos + ofs10, basePos + ofs12, basePos + ofs02);
                            break;
                        case CellCollision.Half_T:
                            drawPolygon(basePos + ofs02, basePos + ofs22, basePos + ofs21, basePos + ofs01);
                            break;
                            //        Angle67_LB1,
                            //        Angle67_LB2,
                            //        Angle67_RB1,
                            //        Angle67_RB2,
                            //        Angle67_LT1,
                            //        Angle67_LT2,
                            //        Angle67_RT1,
                            //        Angle67_RT2,
                    }
                }
            }
        }
        Handles.color = Color.gray;
    }

    void makeCollider()
    {
        DTilemapLayer tilemap = (DTilemapLayer)target;

        var tile = tilemap.Tiles;
        var collider = tilemap.GetComponent<PolygonCollider2D>();
        if (!collider)
        {
            collider = tilemap.gameObject.AddComponent<PolygonCollider2D>();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var tilemap = (DTilemapLayer)target;
        if (GUILayout.Button("Open"))
        {
            DTilemapEditorWindow.Init();
        }
        if (GUILayout.Button("Make Collider"))
        {
            makeCollider();
        }
    }

}

#endif
