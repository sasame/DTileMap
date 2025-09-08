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