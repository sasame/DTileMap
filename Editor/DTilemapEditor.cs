#if UNITY_EDITOR

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DTileMap;




class TilemapMakeCollider
{
    const float SCALE = 10f;
    public struct Edge
    {
        public Vector2Int a;
        public Vector2Int b;
        public Edge(Vector2Int p0, Vector2Int p1)
        {
            a = p0;
            b = p1;
        }
        public Edge CreateInverse()
        {
            return new Edge(b, a);
        }
        public Vector2 Direction
        {
            get { return b - a; }
        }
    }
    List<Edge> _polygons = new List<Edge>();
    List<Edge> _lines = new List<Edge>();
    List<List<Vector2>> _polygonList = new List<List<Vector2>>();
    List<List<Vector2>> _edgeList = new List<List<Vector2>>();

    public List<List<Vector2>> PolygonList
    {
        get { return _polygonList; }
    }
    public List<List<Vector2>> EdgeList
    {
        get { return _edgeList; }
    }

    public void Add(Vector2 a, Vector2 b)
    {
        var e = new Edge(Vector2Int.RoundToInt(a * SCALE), Vector2Int.RoundToInt(b * SCALE));
        _polygons.Add(e);
    }
    public void AddLine(Vector2 a, Vector2 b)
    {
        var e = new Edge(Vector2Int.RoundToInt(a * SCALE), Vector2Int.RoundToInt(b * SCALE));
        _lines.Add(e);
    }

    public void RemoveOverlapEdge()
    {
        var hashEdge = new HashSet<Edge>(_polygons);
        // 重なっている部分のエッジを消す
        for(int idEdge=0;idEdge< _polygons.Count;++idEdge)
        {
            var e = _polygons[idEdge];
            var inv = e.CreateInverse();
            if (hashEdge.Contains(inv))
            {
                hashEdge.Remove(e);
                hashEdge.Remove(inv);
            }
        }
        _polygons = new List<Edge>(hashEdge);

        // make path
        var dicEdges = new Dictionary<Vector2Int, List<Edge>>();
        foreach(var e in hashEdge)
        {
            if (dicEdges.ContainsKey(e.a)) dicEdges[e.a].Add(e);
            else dicEdges[e.a] = new List<Edge>() { e };
//            if (dicEdges.ContainsKey(e.b)) dicEdges[e.b].Add(e);
//            else dicEdges[e.b] = new List<Edge>() { e };
        }

        // connect edge
        _polygonList.Clear();
        int errorCheck = 0;
        while (hashEdge.Count>0)
        {
            var first = hashEdge.First();
            hashEdge.Remove(first);
            var prev = first;
            var path = new List<Vector2>();
            path.Add((Vector2)prev.a / SCALE);

            do
            {
                ++errorCheck;
                if (errorCheck > 10000) return;

                var nexts = dicEdges[prev.b];
                float maxAngle = float.MinValue;
                Edge? maxEdge = null;
                foreach (var n in nexts)
                {
                    var ang = Vector2.SignedAngle(prev.Direction.normalized, n.Direction.normalized);
                    if (maxAngle < ang)
                    {
                        maxAngle = ang;
                        maxEdge = n;
                    }
                }
                if (maxEdge.HasValue)
                {
                    prev = maxEdge.Value;
                    if (Mathf.Abs(maxAngle) > 1f) path.Add((Vector2)prev.a / SCALE);
                }
                else
                {
                    break;
                }
                hashEdge.Remove(prev);
                if (prev.b == first.a) break;
            } while (hashEdge.Count>0);

            _polygonList.Add(path);
        }
    }

    public void MergeLines()
    {
        HashSet<Edge> hashEdge = new HashSet<Edge>(_lines);
        // make path
        var dicEdgesA = new Dictionary<Vector2Int, Edge>();
        var dicEdgesB = new Dictionary<Vector2Int, Edge>();
        foreach (var e in _lines)
        {
            dicEdgesA[e.a] = e;
        }
        foreach (var e in _lines)
        {
            dicEdgesB[e.b] = e;
        }

        int errorCheck = 0;
        while (hashEdge.Count > 0)
        {
            if (errorCheck++ > 10000) return;

            var first = hashEdge.First();
            var list = new List<Vector2>() { first.a, first.b };
            hashEdge.Remove(first);
            // to prev
            var prev = first;
            while (dicEdgesB.ContainsKey(prev.a))
            {
                if (errorCheck++ > 10000) return;

                prev = dicEdgesB[prev.a];
                hashEdge.Remove(prev);
                list.Insert(0, prev.a);
            }
            // to next
            var next = first;
            while (dicEdgesA.ContainsKey(next.b))
            {
                if (errorCheck++ > 10000) return;

                next = dicEdgesA[next.b];
                hashEdge.Remove(next);
                list.Add(next.b);
            }

            var path = list.ConvertAll(v => v / SCALE).ToList();
            _edgeList.Add(path);
        }

/*        foreach (var l in _lines)
        {
            _pathList.Add(new List<Vector2>() { (Vector2)l.a / SCALE, (Vector2)l.b / SCALE });
        }*/
    }
}




[CustomEditor(typeof(DTilemapLayer))]
public class DTileMapEditor : Editor
{
    Vector3[] _tmpLines = null;
    bool _isPainting = false;

    void paint(Vector3Int pos)
    {
        if (!EditorWindow.HasOpenInstances<DTilemapEditorWindow>()) return;
//        Debug.Log(pos.x + ":" + pos.y);
        DTilemapLayer tilemap = (DTilemapLayer)target;
        var spCollider = tilemap.SpriteCollider;
        var window = EditorWindow.GetWindow<DTilemapEditorWindow>();
        if (window)
        {
            Undo.RecordObject(tilemap, "Change Tile");

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
            EditorUtility.SetDirty(tilemap);
        }
    }

    public void OnSceneGUI()
    {
        DTilemapLayer tilemap = (DTilemapLayer)target;
        var prevMatrix = Handles.matrix;
        Handles.matrix = tilemap.transform.localToWorldMatrix;
//        displayCollider();
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

            if (EditorWindow.HasOpenInstances<DTilemapEditorWindow>())
            {
                var window = EditorWindow.GetWindow<DTilemapEditorWindow>();
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
//            e.Use(); // ← Unityの選択イベントを奪う
        }
        else if (e.type == EventType.MouseDrag && e.button == 0 && _isPainting)
        {
            // ドラッグ中もペイント
            paint(basePos);
//            e.Use(); // ← Unityの選択イベントを奪う
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            // ボタン離したら終了
            _isPainting = false;
//            e.Use();
        }
        else if (e.type == EventType.Used)
        {
            //
//            e.Use();
        }
        else if (e.type == EventType.Layout)
        {
        }
        else if (e.type == EventType.MouseMove)
        {
        }
        else
        {
//            Debug.Log("other event:" + e.type);
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
        if (!tilemap) return;
        if (tilemap.Width * tilemap.Height != tilemap.Tiles.Length) return;

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

        var collider = tilemap.GetComponent<PolygonCollider2D>();
        if (!collider)
        {
            collider = Undo.AddComponent<PolygonCollider2D>(tilemap.gameObject);
        }

        var tiles = tilemap.Tiles;
        var spCollider = tilemap.SpriteCollider;
        if (spCollider == null) return;
        List<Vector2[]> shapes = new List<Vector2[]>();
        Vector2 ofs = Vector2.zero;
        var maker = new TilemapMakeCollider();
        for (int y = 0; y < tilemap.Height; ++y)
        {
            ofs.y = y * tilemap.TileSize;
            for (int x = 0; x < tilemap.Width; ++x)
            {
                ofs.x = x * tilemap.TileSize;
                int idx = y * tilemap.Width + x;
                int tileIdx = tiles[idx];
                var cellInfo = spCollider.Get(tileIdx);
                if (cellInfo == null) continue;
                var shape = CellInfo.GetShape(cellInfo.Collision);
                if (shape != null)
                {
                    if (shape.Length == 2)
                    {
                        maker.AddLine(shape[0] + ofs, shape[1] + ofs);
                    }
                    else
                    {
                        for (int idEdge = 0; idEdge < shape.Length; ++idEdge)
                        {
                            maker.Add(shape[idEdge] + ofs, shape[(idEdge + 1) % shape.Length] + ofs);
                        }
                    }
                }
            }
        }
        maker.RemoveOverlapEdge();
        maker.MergeLines();

        collider.pathCount = maker.PolygonList.Count;
        for (int idEdge = 0; idEdge < maker.PolygonList.Count; ++idEdge)
        {
            collider.SetPath(idEdge, maker.PolygonList[idEdge]);
        }

        // エッジコライダーをいったん消す
        var edges = tilemap.GetComponents<EdgeCollider2D>();
        foreach (var edge in edges)
        {
            DestroyImmediate(edge);
        }
        // エッジコライダーを追加
        for (int idEdge = 0; idEdge < maker.EdgeList.Count; ++idEdge)
        {
            var edge = tilemap.gameObject.AddComponent<EdgeCollider2D>();
            edge.SetPoints(maker.EdgeList[idEdge]);
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
