using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DTileMap
{

    public enum CellCollision
    {
        None,
        Box,
        Angle45_LB,
        Angle45_RB,
        Angle45_LT,
        Angle45_RT,
        Angle22_LB1,
        Angle22_LB2,
        Angle22_RB1,
        Angle22_RB2,
        Angle22_LT1,
        Angle22_LT2,
        Angle22_RT1,
        Angle22_RT2,
        Half_B,
        Half_L,
        Half_R,
        Half_T,
        Angle67_LB1,
        Angle67_LB2,
        Angle67_RB1,
        Angle67_RB2,
        Angle67_LT1,
        Angle67_LT2,
        Angle67_RT1,
        Angle67_RT2,

        Count,
    }

    [System.Serializable]
    public class CellInfo
    {
        [SerializeField] Vector2Int _pos;
        [SerializeField] CellCollision _collision;

        public Vector2Int Position
        {
            get { return _pos; }
            set { _pos = value; }
        }
        public CellCollision Collision
        {
            get { return _collision; }
            set { _collision = value; }
        }

        public override string ToString()
        {
            return _pos.ToString() + " : " + _collision;
        }

        static Vector2 p00 = new Vector2(0f, 0f);
        static Vector2 p01 = new Vector2(0.5f, 0f);
        static Vector2 p02 = new Vector2(1f, 0f);
        static Vector2 p10 = new Vector2(0f, 0.5f);
        static Vector2 p11 = new Vector2(0.5f, 0.5f);
        static Vector2 p12 = new Vector2(1f, 0.5f);
        static Vector2 p20 = new Vector2(0f, 1f);
        static Vector2 p21 = new Vector2(0.5f, 1f);
        static Vector2 p22 = new Vector2(1f, 1f);
        static Vector2[][] _cellCollisionTable = new Vector2[(int)CellCollision.Count][]
        {
            null,   // None
            new Vector2[]{ p00,p20,p22,p02 },   // Box
            new Vector2[]{p00,p20,p02}, // Angle45_LB
            new Vector2[]{p02,p00,p22}, // Angle45_RB
            new Vector2[]{p20,p22,p00}, // Angle45_LT
            new Vector2[]{p22,p02,p20}, // Angle45_RT
            new Vector2[]{p00,p01,p20}, // Angle22_LB1
            new Vector2[]{p00,p02,p12,p02}, // Angle22_LB2
            new Vector2[]{p02,p00,p12}, // Angle22_RB1
            new Vector2[]{p00,p10,p22,p02}, // Angle22_RB2
            new Vector2[]{p20,p22,p10}, // Angle22_LT1
            new Vector2[]{p20,p22,p12,p00}, // Angle22_LT2
            new Vector2[]{p22,p12,p20}, // Angle22_RT1
            new Vector2[]{p22,p02,p10,p20}, // Angle22_RT2
            new Vector2[]{p00,p10,p12,p02}, // Half_B
            new Vector2[]{p00,p20,p21,p01}, // Half_L
            new Vector2[]{p22,p02,p01,p21}, // Half_R
            new Vector2[]{p20,p22,p12,p10}, // Half_T
            new Vector2[]{p00,p20,p01}, // Angle67_LB1
            new Vector2[]{p00,p20,p21,p02}, // Angle67_LB2
            new Vector2[]{p02,p01,p22}, // Angle67_RB1
            new Vector2[]{p02,p00,p21,p22}, // Angle67_RB2
            new Vector2[]{p20,p21,p00}, // Angle67_LT1
            new Vector2[]{p20,p22,p01,p00}, // Angle67_LT2
            new Vector2[]{p22,p02,p21}, // Angle67_RT1
            new Vector2[]{p22,p02,p01,p20}, // Angle67_RT2
        };
        // コリジョンの形状を取得
        public static Vector2[] GetShape(CellCollision collision)
        {
            return _cellCollisionTable[(int)collision];
        }
    }

    [CreateAssetMenu(fileName = "SpriteColliderObject", menuName = "ScriptableObjects/CreateSpriteColliderObject")]
    public class SpriteColliderObject : ScriptableObject
    {
        [SerializeField] Texture2D _tilemapTexture;
        [SerializeField] int _cellWidth = 16;
        [SerializeField] int _cellHeight = 16;
        [SerializeField] List<CellInfo> _listInfo = new List<CellInfo>();

        public Texture2D TilemapTexture => _tilemapTexture;
        public int CellWidth => _cellWidth;
        public int CellHeight => _cellHeight;
        public int CellCountX => Mathf.CeilToInt(_tilemapTexture.width / _cellWidth);
        public int CellCountY => Mathf.CeilToInt(_tilemapTexture.height / _cellHeight);

        // セル情報を取得
        public CellInfo Get(int idx)
        {
            return Get(new Vector2Int(idx % CellCountX, idx / CellCountX));
        }
        // セル情報を取得
        public CellInfo Get(Vector2Int pos)
        {
            var item = _listInfo.FirstOrDefault(t => t.Position == pos);
            if (item != null) return item;
            return null;
        }

        public List<CellInfo> GetCellInfoList()
        {
            return _listInfo;
        }

        public void SetRange(Vector2Int min, Vector2Int max, CellCollision col)
        {
            var maxWidth = Mathf.CeilToInt(_tilemapTexture.width / _cellWidth);
            var maxHeight = Mathf.CeilToInt(_tilemapTexture.height / _cellHeight);
            for (int y = min.y; y <= max.y; ++y)
            {
                if ((y < 0) || (y >= maxHeight)) continue;
                for (int x = min.x; x <= max.x; ++x)
                {
                    if ((x < 0) || (x >= maxWidth)) continue;

                    var pos = new Vector2Int(x, y);
                    var item = _listInfo.FirstOrDefault(t => t.Position == pos);
                    if (item != null)
                    {
                        item.Collision = col;
                    }
                    else
                    {
                        _listInfo.Add(new CellInfo() { Position = pos, Collision = col });
                    }
                }
            }
        }
    }
}