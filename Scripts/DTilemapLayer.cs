using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCode;

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
        // コリジョンの有無
        [SerializeField] bool _collision;

        private Mesh _mesh;
        [SerializeField] RenderTexture _rtTilemap;

        public SpriteColliderObject SpriteCollider => _spriteCollider;
        public int Width => _width;
        public int Height => _height;
        public float TileSize => _tileSize;
        public int[] Tiles
        {
            get
            {
                return _tiles;
            }
        }
        // コリジョンの有無
        public bool Collision
        {
            get { return _collision; }
        }

        void initMesh()
        {
            //            _tiles = new int[_width * _height];
            if (_mesh) return;
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        void Awake()
        {
            initMesh();
            RebuildMesh();
            RenderTilemapToRT();
        }

        private void OnEnable()
        {
        }
        private void OnValidate()
        {
            initMesh();
            RebuildMesh();
        }

        void OnDestroy()
        {
            if (_rtTilemap != null)
            {
                DestroyImmediate(_rtTilemap);
            }
        }

        void RenderTilemapToRT()
        {
            if (_rtTilemap == null)
            {
                var w = _width * _spriteCollider.CellWidth;
                var h = _height * _spriteCollider.CellHeight;
                _rtTilemap = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
            }

            // RenderTextureを有効にする
            RenderTexture.active = _rtTilemap;

            // ビューポートを設定
            GL.PushMatrix();
            GL.LoadProjectionMatrix(Matrix4x4.Ortho(0, 1, 0, 1, -1, 1));

            // メッシュを描画
            Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);

            GL.PopMatrix();

            RenderTexture.active = null;
        }
        public void ResizeTiles()
        {
            _tiles = new int[_width * _height];
        }

        public void SetTile(int x, int y, int tileId)
        {
            if (!((x >= 0) && (x < _width))) return;
            if (!((y >= 0) && (y < _height))) return;
            if (_mesh == null) initMesh();
            if (_tiles.Length != _width * _height)
            {
                ResizeTiles();
                initMesh();
            }

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
                    //                    float uvSizeX = 1f / tilesX;
                    //                    float uvSizeY = 1f / tilesY;

                    float u0 = tx / (float)tilesX;
                    float v0 = ty / (float)tilesY;
                    float u1 = (tx + 1) / (float)tilesX;
                    float v1 = (ty + 1) / (float)tilesY;

                    uvs.Add(new Vector2(u0, v0));
                    uvs.Add(new Vector2(u1, v0));
                    uvs.Add(new Vector2(u1, v1));
                    uvs.Add(new Vector2(u0, v1));

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

        // RaycastHit2Dを使わずに、DTilemapLayerの独自データを返す方が良いかも
        public UnityEngine.RaycastHit2D Raycast(Vector2 from, Vector2 to)
        {
            var hit = default(UnityEngine.RaycastHit2D);
            var localFrom = transform.worldToLocalMatrix.MultiplyPoint(from) / _tileSize;
            var localTo = transform.worldToLocalMatrix.MultiplyPoint(to) / _tileSize;
            Vector2 worldFrom = from;
            float amountA, amountB;
            {
                var basePos = new Vector2Int(Mathf.FloorToInt(localFrom.x), Mathf.FloorToInt(localFrom.y));
                var cellInfo = _spriteCollider.Get(basePos);
                var shape = CellInfo.GetShape(cellInfo.Collision);
                if (shape != null)
                {
                    for (int idLine = 0; idLine <= shape.Length; ++idLine)
                    {
                        var p0 = shape[idLine] + basePos;
                        var p1 = shape[idLine % shape.Length] + basePos;
                        if (Vector2Ext.IsCrossLine(localFrom, localTo, p0, p1, out amountA, out amountB))
                        {
                            hit.point = transform.localToWorldMatrix.MultiplyPoint(Vector2.Lerp(p0, p1, amountB));
                            var normal = transform.localToWorldMatrix.MultiplyVector(Vector2.Perpendicular(p1 - p0));
                            hit.normal = normal;
                            hit.distance = (hit.point - worldFrom).magnitude;
                            //                            hit.transform = transform;
                        }
                    }
                }
            }

            return hit;
        }

    }

}