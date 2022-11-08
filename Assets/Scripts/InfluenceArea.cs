using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DefaultNamespace
{
    public class InfluenceArea
    {
        public bool[,] maskArea;
        public Vector2Int AreaSize => 
            new Vector2Int(maskArea.GetLength(0), maskArea.GetLength(1));
        private GameObject drawnArea;
        private Dictionary<Vector2Int, GameObject> drawnAreaSprites;
        private Vector3 _scale;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalConfig"> Config to create the drawnArea object.</param>
        /// <param name="config">Config to set area paremeters.</param>
        /// <param name="parent">If present, then drawnArea will be attached to it.</param>
        /// <param name="setActive">If false, then drawnArea GameObject will be non active.</param>
        public InfluenceArea(
            GlobalSettingsConfig globalConfig, SpeciesConfig config,
            Transform parent = null, bool setActive = false)
        {
            maskArea = config.influenceArea.GetCells();
            _scale = parent == null ? Vector3.one : parent.transform.localScale;
            
            InitializeVisualComponents(globalConfig, setActive, parent);

            if (parent != null)
            {
                drawnArea.transform.parent = parent;
                drawnArea.transform.localPosition = new Vector3(0, 0, 0);
            }
        }

        private void InitializeVisualComponents(
            GlobalSettingsConfig config, bool setActive = false, Transform parent = null)
        {
            drawnArea = new GameObject("InfluenceArea");
            drawnArea.SetActive(setActive);
            drawnAreaSprites = new Dictionary<Vector2Int, GameObject>();

            for (int i = 0; i < AreaSize.x; i++)
            {
                for (int j = 0; j < AreaSize.y; j++)
                {
                    if (!maskArea[i, j])
                    {
                        continue;
                    }
                    
                    GameObject spritePrefub = new GameObject(
                        $"sprite_{i}{j}", typeof(SpriteRenderer));
                    spritePrefub.transform.localScale = _scale;

                    var spriteComponent = spritePrefub.GetComponent<SpriteRenderer>();
                    spriteComponent.color = config.influenceAreaColor;
                    spriteComponent.sprite = config.influenceAreaSprite;
                    spriteComponent.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    spriteComponent.sortingLayerName = config.influenceAreaSortingLayer;

                    var transformComponent = spriteComponent.transform;
                    transformComponent.parent = drawnArea.transform;
                    transformComponent.localPosition = Vector3.Scale(
                        TransformArrayCoordsToLocal(i, j), _scale);
                    
                    drawnAreaSprites[new Vector2Int(i, j)] = spritePrefub;
                }
            }
        }

        public void DestroyVisualComponents()
        {
            Object.Destroy(drawnArea);
        }
        
        /// <summary>
        /// Transform array indexation to the local indexation
        /// in interval [-AreaSize / 2, AreaSize / 2] 
        /// </summary>
        /// <param name="i">Row coord of the array</param>
        /// <param name="j">Column coord of the array</param>
        /// <param name="invertY">Set to true, to align with standard arrays</param>
        /// <returns></returns>
        public Vector3 TransformArrayCoordsToLocal(int i, int j, bool invertY = false)
        {
            Vector2Int areaHalfSize = AreaSize / 2;
            int yCoord = invertY ? i - areaHalfSize.x : areaHalfSize.x - i;
            var localCoords = new Vector3(
                (j - areaHalfSize.y), yCoord, 0);
            return localCoords;
        }
        
        public Vector3 TransformArrayCoordsToLocal(Vector2Int coords, bool invertY = false)
        {
            return TransformArrayCoordsToLocal(coords.x, coords.y, invertY);
        }

        public IEnumerable<KeyValuePair<Vector2Int, GameObject>> AreaSpritesIterator()
        {
            foreach(KeyValuePair<Vector2Int, GameObject> entry in drawnAreaSprites)
            {
                yield return entry;
            }
        }

        public void SetActivationStatus(
            bool availableToPlace, Vector2Int gridCoords, SavannaGrid grid)
        {
            if (!availableToPlace)
            {
                drawnArea.SetActive(false);
                return;
            }
            
            drawnArea.SetActive(true);
            foreach (var entry in AreaSpritesIterator())
            {
                var shiftLocalCoords = TransformArrayCoordsToLocal(entry.Key, true);
                // shiftLocalCoords is always integer
                var targetCoords = new Vector2Int(
                    gridCoords.x + (int) shiftLocalCoords.x,
                    gridCoords.y + (int) shiftLocalCoords.y);
                bool isInsideGrid = grid.CheckGridCoordsValid(targetCoords);
                drawnAreaSprites[new Vector2Int(entry.Key.x, entry.Key.y)].SetActive(isInsideGrid);
            }
        }
    }
}