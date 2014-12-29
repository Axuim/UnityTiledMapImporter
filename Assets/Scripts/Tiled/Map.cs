using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tiled
{
    public class Map : MonoBehaviour
    {
        #region Private Properties

        private List<MapLayer> _mapLayers = new List<MapLayer>();

        #endregion

        #region Public Properties

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

        private int _tilePixelWidth;
        public int TilePixelWidth
        {
            get
            {
                return _tilePixelWidth;
            }
        }

        public MapLayer MapLayerPrefab;
        public TextAsset TiledMap;
        public Material Tileset;
        
        public Vector3 MapLayerOffset = new Vector3(0, 0, -0.1f);

        #endregion

        #region Public Methods

        public void Load()
        {
            if (this.TiledMap != null)
            {
                var xDocument = XDocument.Parse(this.TiledMap.text);

                var mapElement = xDocument.Element("map");
                if (mapElement != null)
                {
                    _width = int.Parse(mapElement.Attribute("width").Value);
                    _height = int.Parse(mapElement.Attribute("height").Value);
                    _tilePixelWidth = int.Parse(mapElement.Attribute("tilewidth").Value);

                    var transform = this.transform;

                    var layerElements = mapElement.Elements("layer");

                    foreach (var layerElement in layerElements)
                    {
                        var mapLayer = GameObject.Instantiate(this.MapLayerPrefab, transform.position + (this.MapLayerOffset * _mapLayers.Count), Quaternion.identity) as MapLayer;
                        mapLayer.LayerElement = layerElement;
                        mapLayer.LayerType = MapLayerType.Visual;
                        mapLayer.renderer.material = this.Tileset;
                        mapLayer.Map = this;
                        mapLayer.transform.parent = transform;
                        _mapLayers.Add(mapLayer);
                    }

                    var collisionElement = mapElement.Elements("objectgroup").FirstOrDefault(e => e.Attribute("name").Value == "Collision");

                    if (collisionElement != null)
                    {
                        var collisionLayer = GameObject.Instantiate(this.MapLayerPrefab, transform.position, Quaternion.identity) as MapLayer;
                        collisionLayer.LayerElement = collisionElement;
                        collisionLayer.LayerType = MapLayerType.Collision;
                        collisionLayer.Map = this;
                        collisionLayer.transform.parent = transform;
                        _mapLayers.Add(collisionLayer);
                    }

                    foreach (var mapLayer in _mapLayers)
                    {
                        mapLayer.Load();
                    }
                }
            }
        }

        #endregion
    }
}
