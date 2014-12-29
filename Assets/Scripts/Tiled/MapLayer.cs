using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tiled
{
    #region Enums

    public enum MapLayerType { Visual = 0, Collision }

    #endregion

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MapLayer : MonoBehaviour
    {
        #region Private Properties

        private Transform _transform;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        #endregion

        #region Public Properties

        public BoxCollider2D ColliderPrefab;

        public int TilesToLoadPerFrame = 10000;

        private MapLayerType _layerType;
        public MapLayerType LayerType
        {
            get
            {
                return _layerType;
            }
            set
            {
                if (value != _layerType)
                {
                    _layerType = value;
                    _meshRenderer.enabled = _layerType == MapLayerType.Visual;
                }
            }
        }

        private Map _map;
        public Map Map
        {
            get
            {
                return _map;
            }
            set
            {
                if (value != _map)
                {
                    _map = value;

                    if (_map != null)
                    {
                        _width = _map.Width;
                        _height = _map.Height;
                    }
                }
            }
        }

        private XElement _layerElement;
        public XElement LayerElement
        {
            get
            {
                return _layerElement;
            }
            set
            {
                if (value != _layerElement)
                {
                    _layerElement = value;

                    if (_layerElement != null)
                    {
                        this.name = _layerElement.Attribute("name").Value;
                    }
                    else
                    {
                        this.name = "MapLayer";
                    }
                }
            }
        }

        private int _width;
        public int Width
        {
            get
            {
                return _width;
            }
        }

        private int _height;
        public int Height
        {
            get
            {
                return _height;
            }
        }

        #endregion

        #region MonoBehaviour

        void Awake()
        {
            _transform = this.transform;
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshRenderer = this.GetComponent<MeshRenderer>();

            _meshRenderer.castShadows = false;
            _meshRenderer.receiveShadows = false;
        }

        #endregion

        #region Public Methods

        public void Load()
        {
            StopCoroutine("GenerateLayer");
            StartCoroutine("GenerateLayer");
        }

        #endregion

        #region Private Methods

        private IEnumerator GenerateLayer()
        {
            int x, y, count = 0;

            if (this.LayerType == MapLayerType.Visual)
            {
                int index;
                XElement tileElement;

                var vertices = new List<Vector3>();
                var triangles = new List<int>();
                var uv = new List<Vector2>();

                var texture = this.renderer.material.mainTexture;

                var dataElement = this.LayerElement.Element("data");
                var elements = dataElement.Elements("tile").ToArray();

                for (x = 0; x < this.Width; x++)
                {
                    for (y = 0; y < this.Height; y++)
                    {
                        tileElement = elements[(y * this.Width) + x];
                        index = int.Parse(tileElement.Attribute("gid").Value) - 1;

                        if (index >= 0)
                        {
                            vertices.Add(new Vector3(x, (this.Height - y), 0));
                            vertices.Add(new Vector3(x + 1, (this.Height - y), 0));
                            vertices.Add(new Vector3(x + 1, (this.Height - y) - 1, 0));
                            vertices.Add(new Vector3(x, (this.Height - y) - 1, 0));

                            float tileSizeX = (float)this.Map.TilePixelWidth / texture.width;
                            float tileSizeY = (float)this.Map.TilePixelWidth / texture.height;

                            int textureX = index % (texture.width / this.Map.TilePixelWidth);
                            int textureY = Mathf.FloorToInt((texture.height / this.Map.TilePixelWidth) - (index / (texture.width / this.Map.TilePixelWidth))) - 1;

                            uv.Add(new Vector2(textureX * tileSizeX, (textureY * tileSizeY) + tileSizeY));
                            uv.Add(new Vector2((textureX * tileSizeX) + tileSizeX, (textureY * tileSizeY) + tileSizeY));
                            uv.Add(new Vector2((textureX * tileSizeX) + tileSizeX, textureY * tileSizeY));
                            uv.Add(new Vector2(textureX * tileSizeX, textureY * tileSizeY));

                            triangles.Add(count * 4);
                            triangles.Add(count * 4 + 1);
                            triangles.Add(count * 4 + 2);
                            triangles.Add(count * 4);
                            triangles.Add(count * 4 + 2);
                            triangles.Add(count * 4 + 3);

                            count++;
                        }

                        if (count % this.TilesToLoadPerFrame == 0)
                        {
                            yield return null;
                        }
                    }
                }

                this.UpdateMesh(vertices.ToArray(), uv.ToArray(), triangles.ToArray());
            }
            else if (this.LayerType == MapLayerType.Collision && this.ColliderPrefab != null)
            {
                int width, height;
                BoxCollider2D boxCollider;
                var elements = this.LayerElement.Elements("object").ToArray();

                foreach (var element in elements)
                {
                    x = int.Parse(element.Attribute("x").Value) / this.Map.TilePixelWidth;
                    y = int.Parse(element.Attribute("y").Value) / this.Map.TilePixelWidth;
                    width = int.Parse(element.Attribute("width").Value) / this.Map.TilePixelWidth;
                    height = int.Parse(element.Attribute("height").Value) / this.Map.TilePixelWidth;

                    boxCollider = GameObject.Instantiate(this.ColliderPrefab) as BoxCollider2D;
                    boxCollider.size = new Vector2(width, height);
                    boxCollider.center = new Vector2(width * 0.5f, height * -0.5f);
                    boxCollider.transform.parent = _transform;
                    boxCollider.transform.localPosition = new Vector3(x, this.Height - y, 0);

                    count++;

                    if (count % this.TilesToLoadPerFrame == 0)
                    {
                        yield return null;
                    }
                }
            }
        }

        private void UpdateMesh(Vector3[] vertices, Vector2[] uv, int[] triangles)
        {

            _meshRenderer.enabled = false;

            _meshFilter.mesh.Clear();
            _meshFilter.mesh.vertices = vertices;
            _meshFilter.mesh.uv = uv;
            _meshFilter.mesh.triangles = triangles;

            _meshFilter.mesh.RecalculateNormals();

            _meshFilter.mesh.Optimize();

            _meshRenderer.enabled = true;
        }

        #endregion
    }
}
