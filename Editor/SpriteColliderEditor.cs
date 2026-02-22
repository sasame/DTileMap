#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DEditor;
using DTileMap;

public class SpriteColliderEditor : DGridBaseWindow
{
    enum DragType
    {
        None,
        Region,
        Move,
        //            File,
        //            Item,
    }
    DragType _dragType = DragType.None;
    // Icons
    Dictionary<string, Texture2D> _dicIcons = new Dictionary<string, Texture2D>();
    Texture2D[] _arrayIcons;
    // GUISkin
    GUISkin _guiSkin;
    Vector2Int _regionFrom;
    Vector2Int _regionTo;

    [MenuItem("Window/SpriteColliderEditor")]
    public static void Init()
    {
        var window = EditorWindow.GetWindow(typeof(SpriteColliderEditor));
        window.minSize = Vector2.one * 300f;
        window.Show();
    }

    void Awake()
    {
        _guiSkin = Resources.Load<GUISkin>("DGUISkin");
        MinScale = 0.000001f;
    }

    private void OnEnable()
    {
        string[] patternNames = new string[]
        {
            CellCollision.None.ToString(),
            CellCollision.Box.ToString(),
            CellCollision.Angle45_LB.ToString(),
            CellCollision.Angle45_RB.ToString(),
            CellCollision.Angle45_LT.ToString(),
            CellCollision.Angle45_RT.ToString(),
            CellCollision.Angle22_LB1.ToString(),
            CellCollision.Angle22_LB2.ToString(),
            CellCollision.Angle22_RB1.ToString(),
            CellCollision.Angle22_RB2.ToString(),
            CellCollision.Angle22_LT1.ToString(),
            CellCollision.Angle22_LT2.ToString(),
            CellCollision.Angle22_RT1.ToString(),
            CellCollision.Angle22_RT2.ToString(),
            CellCollision.Half_B.ToString(),
            CellCollision.Half_L.ToString(),
            CellCollision.Half_R.ToString(),
            CellCollision.Half_T.ToString(),
            CellCollision.Angle67_LB1.ToString(),
            CellCollision.Angle67_LB2.ToString(),
            CellCollision.Angle67_RB1.ToString(),
            CellCollision.Angle67_RB2.ToString(),
            CellCollision.Angle67_LT1.ToString(),
            CellCollision.Angle67_LT2.ToString(),
            CellCollision.Angle67_RT1.ToString(),
            CellCollision.Angle67_RT2.ToString(),
            CellCollision.Edge_B.ToString(),
            CellCollision.Edge_L.ToString(),
            CellCollision.Edge_R.ToString(),
            CellCollision.Edge_T.ToString(),
        };
        _arrayIcons = new Texture2D[patternNames.Length];
        for (int i = 0; i < _arrayIcons.Length; ++i)
        {
            _dicIcons[patternNames[i]] = Resources.Load<Texture2D>("ColliderPatern/" + patternNames[i]);
            _arrayIcons[i] = _dicIcons[patternNames[i]];
        }
        //            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ColliderPatern/pikachu.png");
    }
    private void OnDisable()
    {
        /*            for (int i = 0; i < _arrayIcons.Length; ++i)
                    {
                        if (_arrayIcons[i]) DestroyImmediate(_arrayIcons[i]);
                    }
                    _arrayIcons = null;*/
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
                    case DragType.Move:
                        ViewMove(e);
                        e.Use();
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
                //
                //                    modifySelection();
                _regionFrom = Vector2Int.FloorToInt(GetLocalPosition(e.mousePosition));
                _regionTo = Vector2Int.FloorToInt(GetLocalPosition(e.mousePosition));
                if (e.keyCode == KeyCode.LeftControl)
                {
                    _dragType = DragType.Move;
                }
                else
                {
                    _dragType = DragType.Region;
                }
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            if (e.button == 0)
            {
                /*                    switch (_dragType)
                                    {
                                        case DragType.Item:
                                            if (_dragFile != null)
                                            {
                                                _dragEndIndex = getIndex(_dragFile, e.mousePosition);
                                                modifySelection();
                                            }
                                            break;
                                    }*/
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

    bool drawDropdownMenu(SpriteColliderObject spCollider)
    {
        bool useButton = false;
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
        // File menu
        // File
        if (GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(70)))
        {
            GenericMenu toolsMenu = new GenericMenu();
            toolsMenu.AddItem(new GUIContent("Save"), false, () =>
            {
                EditorUtility.SetDirty(spCollider);
                AssetDatabase.SaveAssetIfDirty(spCollider);
            });
            toolsMenu.DropDown(new Rect(0, 0, 0, 16));
            EditorGUIUtility.ExitGUI();
        }

        // Edit
        if (GUILayout.Button("Edit", EditorStyles.toolbarDropDown, GUILayout.Width(70)))
        {
            GenericMenu toolsMenu = new GenericMenu();
            toolsMenu.AddItem(new GUIContent("None"), false, () =>
            {
                var minPos = Vector2Int.Min(_regionFrom, _regionTo);
                var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
                Undo.RecordObject(spCollider, "Change Collider");
                spCollider.SetRange(minPos, maxPos, CellCollision.None);
                EditorUtility.SetDirty(spCollider);
            });
            toolsMenu.AddItem(new GUIContent("Box"), false, () =>
            {
                var minPos = Vector2Int.Min(_regionFrom, _regionTo);
                var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
                Undo.RecordObject(spCollider, "Change Collider");
                spCollider.SetRange(minPos, maxPos, CellCollision.Box);
                EditorUtility.SetDirty(spCollider);
            });
            toolsMenu.DropDown(new Rect(0, 0, 0, 16));
            EditorGUIUtility.ExitGUI();
        }
        // Attribute
        if (GUILayout.Button("Attribute", EditorStyles.toolbarDropDown, GUILayout.Width(70)))
        {
            GenericMenu toolsMenu = new GenericMenu();
            toolsMenu.AddItem(new GUIContent("None"), false, () =>
            {
                var minPos = Vector2Int.Min(_regionFrom, _regionTo);
                var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
                spCollider.SetRangeAttribute(minPos, maxPos, "");
            });
            toolsMenu.AddItem(new GUIContent("Grass"), false, () =>
            {
                var minPos = Vector2Int.Min(_regionFrom, _regionTo);
                var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
                spCollider.SetRangeAttribute(minPos, maxPos, "Grass");
            });
            toolsMenu.AddItem(new GUIContent("Stone"), false, () =>
            {
                var minPos = Vector2Int.Min(_regionFrom, _regionTo);
                var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
                spCollider.SetRangeAttribute(minPos, maxPos, "Stone");
            });
            toolsMenu.AddItem(new GUIContent("SlipStone"), false, () =>
            {
                var minPos = Vector2Int.Min(_regionFrom, _regionTo);
                var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
                spCollider.SetRangeAttribute(minPos, maxPos, "SlipStone");
            });
            toolsMenu.DropDown(new Rect(0, 0, 0, 16));
            EditorGUIUtility.ExitGUI();
        }

        /*            else if (GUILayout.Button("Decimal", EditorStyles.toolbarDropDown, GUILayout.Width(70)))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        toolsMenu.AddItem(new GUIContent("2"), _decimal == 2, () =>
                        {
                            _decimal = 2;
                        });
                        toolsMenu.AddItem(new GUIContent("10"), _decimal == 10, () =>
                        {
                            _decimal = 10;
                        });
                        toolsMenu.AddItem(new GUIContent("16"), _decimal == 16, () =>
                        {
                            _decimal = 16;
                        });
                        toolsMenu.DropDown(new Rect(0, 0, 0, 16));
                        EditorGUIUtility.ExitGUI();
                    }
                    else if (GUILayout.Button("Endian", EditorStyles.toolbarDropDown, GUILayout.Width(70)))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        toolsMenu.AddItem(new GUIContent("LittleEndian"), _isLittleEndian, () =>
                        {
                            _isLittleEndian = true;
                        });
                        toolsMenu.AddItem(new GUIContent("BigEndian"), !_isLittleEndian, () =>
                        {
                            _isLittleEndian = false;
                        });
                        toolsMenu.DropDown(new Rect(0, 0, 0, 16));
                        EditorGUIUtility.ExitGUI();
                    }
                    else if (GUILayout.Button("Encoding", EditorStyles.toolbarDropDown, GUILayout.Width(70)))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        foreach (var e in System.Text.Encoding.GetEncodings())
                        {
                            toolsMenu.AddItem(new GUIContent(e.Name), _encoding == e.GetEncoding(), () =>
                            {
                                _encoding = e.GetEncoding();
                            });
                        }
                        toolsMenu.DropDown(new Rect(0, 0, 0, 16));
                        EditorGUIUtility.ExitGUI();
                    }
                    */
        EditorGUILayout.EndHorizontal();
        return useButton;
    }

    void drawSelectionRect()
    {
        var minPos = Vector2Int.Min(_regionFrom, _regionTo);
        var maxPos = Vector2Int.Max(_regionFrom, _regionTo);
        Vector3 from = GetCanvasPosition(minPos);
        Vector3 to = GetCanvasPosition(maxPos + Vector2.one);
        Vector3 size = (to - from);
        Handles.color = (_dragType == DragType.Region) ? Color.green : Color.magenta;
        Handles.DrawLine(from, from + Vector3.right * size.x, 1f);
        Handles.DrawLine(from, from + Vector3.up * size.y, 1f);
        Handles.DrawLine(from + Vector3.right * size.x, from + size, 1f);
        Handles.DrawLine(from + Vector3.up * size.y, from + size, 1f);
        Handles.color = Color.gray;
    }

    void OnGUI()
    {
        Event e = Event.current;
        GUI.skin = _guiSkin;

        EditorGUI.BeginChangeCheck();

        // grid
        DrawGrid();
        // string
        /*            foreach (var b in _binaryFiles)
                    {
                        drawHexString(b);
                    }
                    drawSelectInfo();
        */
//        Debug.Log(Selection.activeObject);
        if (Selection.activeObject)
        {
            var spCollider = Selection.activeObject as SpriteColliderObject;
            if (spCollider && spCollider.TilemapTexture)
            {
                var tex = spCollider.TilemapTexture;
                {
                    var from = GetCanvasPosition(new Vector2(0f, tex.height / spCollider.CellHeight));
                    var size = GetCanvasPosition(new Vector2(tex.width / spCollider.CellWidth, 0f)) - from;
                    GUI.DrawTexture(new Rect(from.x, from.y, size.x, size.y), tex);
                }

                var list = spCollider.GetCellInfoList();
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                float iconSize = 0.6f;
                foreach (var item in list)
                {
                    var pos = GetCanvasPosition(item.Position);
                    var size = GetCanvasSize(Vector2.one);
                    GUI.DrawTexture(new Rect(pos.x + size.x * (1f - iconSize) * 0.5f, pos.y - size.y + ((1f - iconSize) * size.y * 0.5f), size.x * iconSize, size.y * iconSize), _arrayIcons[(int)item.Collision]);
                }
                GUI.color = Color.white;

                bool useMenu = drawDropdownMenu(spCollider);
                var minPos = Vector2Int.Min(_regionFrom, _regionTo);
                var maxPos = Vector2Int.Max(_regionFrom, _regionTo);

                GUILayout.BeginHorizontal();
                for (int idCollision = 0; idCollision < _arrayIcons.Length; ++idCollision)
                {
                    var item = _arrayIcons[idCollision];
                    if (GUILayout.Button(item))
                    {
                        Undo.RecordObject(spCollider, "Change Collider");
                        spCollider.SetRange(minPos, maxPos, (CellCollision)idCollision);
                        useMenu = true;
                    }
                }
                GUILayout.EndHorizontal();
                //                    GUILayout.Toolbar(1, _arrayIcons);

                if (useMenu == false)
                {
                    guiControl(e);
                }

                var att = new HashSet<string>();
                spCollider.ApplyRange(minPos, maxPos, cellInfo => {
                    if (!string.IsNullOrEmpty(cellInfo.Attribute)) att.Add(cellInfo.Attribute);
                });
                string s = "";
                foreach(var a in att) s += a + " ";
                GUI.Label(new Rect(0, 150, 500, 32), s);
            }

            drawSelectionRect();
        }

        GUI.Label(new Rect(0, 100, 500, 32), _regionFrom + " : " + _regionTo);

        if (EditorGUI.EndChangeCheck())
        {
        }
    }
}

#endif
