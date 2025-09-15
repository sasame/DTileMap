using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DTileMap
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class DTilemapLayer : MonoBehaviour
    {
        [SerializeField] SpriteColliderObject _spriteCollider;
        [SerializeField] int _width = 16;
        [SerializeField] int _height = 16;
        [SerializeField] float _tileSize = 1f;
        [SerializeField] int[] _tiles; // タイルIDs

        private Mesh _mesh;

        public SpriteColliderObject SpriteCollider => _spriteCollider;
        public int Width => _width;
        public int Height => _height;
        public float TileSize => _tileSize;
        public int[] Tiles => _tiles;

        void initMesh()
        {
            _tiles = new int[_width * _height];
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        void Awake()
        {
            initMesh();
            RebuildMesh();
        }

        public void SetTile(int x, int y, int tileId)
        {
            if (!((x >= 0) && (x < _width))) return;
            if (!((y >= 0) && (y < _height))) return;
            if (_mesh == null) initMesh();
            if (_tiles.Length != _width * _height) initMesh();

            int idx = x + y * _width;
            _tiles[idx] = tileId;
            RebuildMesh(); // 今は全更新（後で最適化可）
        }

        void RebuildMesh()
        {
            int tilesX = Mathf.CeilToInt(_spriteCollider.TilemapTexture.width / _spriteCollider.CellWidth);
            int tilesY = Mathf.CeilToInt(_spriteCollider.TilemapTexture.height / _spriteCollider.CellHeight);

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int idx = x + y * _width;
                    int tileId = _tiles[idx];
                    if (tileId < 0) continue; // -1 = 空白

                    // Quad頂点
                    int vIndex = vertices.Count;
                    float px = x * _tileSize;
                    float py = y * _tileSize;

                    vertices.Add(new Vector3(px, py, 0));
                    vertices.Add(new Vector3(px + _tileSize, py, 0));
                    vertices.Add(new Vector3(px + _tileSize, py + _tileSize, 0));
                    vertices.Add(new Vector3(px, py + _tileSize, 0));

                    // UV計算
                    int tx = tileId % tilesX;
                    int ty = tileId / tilesX;
                    float uvSizeX = 1f / tilesX;
                    float uvSizeY = 1f / tilesY;

                    float u0 = tx * uvSizeX;
                    float v0 = ty * uvSizeY;

                    uvs.Add(new Vector2(u0, v0));
                    uvs.Add(new Vector2(u0 + uvSizeX, v0));
                    uvs.Add(new Vector2(u0 + uvSizeX, v0 + uvSizeY));
                    uvs.Add(new Vector2(u0, v0 + uvSizeY));

                    // インデックス
                    triangles.Add(vIndex + 0);
                    triangles.Add(vIndex + 2);
                    triangles.Add(vIndex + 1);
                    triangles.Add(vIndex + 0);
                    triangles.Add(vIndex + 3);
                    triangles.Add(vIndex + 2);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetUVs(0, uvs);
            _mesh.SetTriangles(triangles, 0);
        }


    }

}