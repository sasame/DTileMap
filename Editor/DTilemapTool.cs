#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using DTileMap;

namespace DEditor
{
    [EditorTool("Tilemap Tool",typeof(DTilemapLayer))]
    public class DTilemapTool : EditorTool
    {
        public Texture2D _toolIcon;
        Vector3[] _tmpLines = null;
        bool _isPainting = false;

        public override GUIContent toolbarIcon
        {
            get
            {
                if (_toolIcon == null)
                {
                    _toolIcon = Resources.Load<Texture2D>("TilemapToolIcon");
                }
                return new GUIContent("Tile",_toolIcon,"Tilemap Tool");
            }
        }

        void paint(Vector3Int pos,bool remove)
        {
            if (!EditorWindow.HasOpenInstances<DTilemapEditorWindow>()) return;
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
                        if (remove)
                        {
                            tilemap.SetTile(pos.x + ofsx, pos.y + ofsy, -1);
                        }
                        else
                        {
                            tilemap.SetTile(pos.x + ofsx, pos.y + ofsy, x + y * spCollider.CellCountX);
                        }
                    }
                }
                tilemap.RebuildMesh();
            }
        }

        public override void OnToolGUI(EditorWindow editorWindow)
        {
            // Sceneビュー上での描画や操作を記述
            Handles.color = Color.red;
            Handles.DrawWireCube(Vector3.zero, Vector3.one * 2);

            DTilemapLayer tilemap = (DTilemapLayer)target;
            var prevMatrix = Handles.matrix;
            Handles.matrix = tilemap.transform.localToWorldMatrix;
            //        displayCollider();
            if (Tools.current != Tool.Custom)
            {
                Debug.Log(Tools.current);
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
                    var window = Resources.FindObjectsOfTypeAll<DTilemapEditorWindow>().FirstOrDefault();
                    //                var window = EditorWindow.GetWindow<DTilemapEditorWindow>();
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
                Undo.RecordObject(tilemap, "Change Tile");
                paint(basePos, e.control);
                //            e.Use(); // ← Unityの選択イベントを奪う
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && _isPainting)
            {
                // ドラッグ中もペイント
                paint(basePos,e.control);
                //            e.Use(); // ← Unityの選択イベントを奪う
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                // ボタン離したら終了
                _isPainting = false;
                EditorUtility.SetDirty(tilemap);
                //            e.Use();
            }
            else if (e.type == EventType.Used)
            {
                //
                //            e.Use();
            }
            else if (e.type == EventType.Layout)
            {
                tilemap.RebuildMesh();
            }
            else if (e.type == EventType.MouseMove)
            {
                SceneView.RepaintAll();
            }
            else
            {
                //            Debug.Log("other event:" + e.type);
            }

            //        SceneView.RepaintAll();
            Handles.matrix = prevMatrix;
        }
    }

}


#endif

