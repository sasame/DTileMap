#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DEditor;
using DTileMap;

/// <summary>
/// タイルマップエディターウィンドウ
/// </summary>
public class DTilemapEditorWindow : DGridBaseWindow
{
    enum DragType
    {
        None,
        Region,
    }
    DragType _dragType = DragType.None;
    Vector2Int _regionFrom;
    Vector2Int _regionTo;

    // GUISkin
    GUISkin _guiSkin;

    [MenuItem("Window/DTilemapEditorWindow")]
    public static void Init()
    {
        var window = EditorWindow.GetWindow(typeof(DTilemapEditorWindow));
        window.minSize = Vector2.one * 300f;
        window.Show();
    }
    void Awake()
    {
        _guiSkin = Resources.Load<GUISkin>("DGUISkin");
        MinScale = 0.000001f;
    }

    public (Vector2Int,Vector2Int) GetSelctionTile()
    {
        var minPos = Vector2Int.Min(_regionFrom, _regionTo);
        var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
        return (minPos, maxPos);
    }

    bool guiControl(Event e)
    {
        if (e.type == EventType.ScrollWheel)
        {
            ViewScaling(e);
            e.Use();
        }
        else if (e.type == EventType.ContextClick)
        {
        }
        else if (e.type == EventType.MouseDrag)
        {
            if (e.button == 0)
            {
                switch (_dragType)
                {
                    case DragType.None:
                        ViewMove(e);
                        break;
                    case DragType.Region:
                        _regionTo = Vector2Int.FloorToInt(GetLocalPosition(e.mousePosition));
                        break;
                }
                //                    modifySelection();
                e.Use();
                return true;
            }
            else if (e.button == 2)
            {
                ViewMove(e);
                e.Use();
                return true;
            }
        }
        else if (e.type == EventType.MouseDown)
        {
            if (e.button == 0)
            {
                _regionFrom = Vector2Int.FloorToInt(GetLocalPosition(e.mousePosition));
                _regionTo = Vector2Int.FloorToInt(GetLocalPosition(e.mousePosition));
                _dragType = DragType.Region;
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            if (e.button == 0)
            {
                _dragType = DragType.None;
                e.Use();
                return true;
            }
        }
        else if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.A)
            {
                //                    ViewFraming(getAllDataRect());
            }
            e.Use();
        }
        else if (e.type == EventType.MouseMove)
        {
            e.Use();
        }
        return false;
    }

    void drawSelectionRect()
    {
        var minPos = Vector2Int.Min(_regionFrom, _regionTo);
        var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
        float bold = 2f;
        Vector3 from = GetCanvasPosition(minPos);
        Vector3 to = GetCanvasPosition(maxPos + Vector2.one);
        Vector3 size = (to - from);
        Handles.color = (_dragType == DragType.Region) ? Color.green : Color.magenta;
        Handles.DrawLine(from, from + Vector3.right * size.x, bold);
        Handles.DrawLine(from, from + Vector3.up * size.y, bold);
        Handles.DrawLine(from + Vector3.right * size.x, from + size, bold);
        Handles.DrawLine(from + Vector3.up * size.y, from + size, bold);
        Handles.color = Color.gray;
    }

    void OnGUI()
    {
        Event e = Event.current;
        GUI.skin = _guiSkin;

        EditorGUI.BeginChangeCheck();

        // grid
        DrawGrid();

        if (Selection.activeObject)
        {
            var layer = Selection.activeGameObject.GetComponent<DTilemapLayer>();
            if (layer)
            {
//                Debug.Log("exist layer:" + layer.name);
                var spCollider = layer.SpriteCollider;
                var tex = spCollider.TilemapTexture;
                if (tex)
                {
                    var from = GetCanvasPosition(new Vector2(0f, tex.height / spCollider.CellHeight));
                    var size = GetCanvasPosition(new Vector2(tex.width / spCollider.CellWidth, 0f)) - from;
                    GUI.color = Color.white;
                    GUI.DrawTexture(new Rect(from.x, from.y, size.x, size.y), tex);
                }
                guiControl(e);
                GUI.Label(new Rect(0, 100, 500, 32), _regionFrom + " : " + _regionTo);

                drawSelectionRect();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
        }
    }

}

#endif