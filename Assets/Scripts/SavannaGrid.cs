using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace DefaultNamespace
{
    public class SavannaGrid : MonoBehaviour
    {
        public Vector2Int gridSize;
        public float gridLineWidth = 0.1f;
        public GlobalSettingsConfig globalConfig;
        
        private Specimen[,] grid;
        
        // used for transformation operations
        private Vector3 _upperLeftCornerWorldCoords;
        private Vector3 _bottomRightCornerWorldCoords;
        private Vector2 _cellSize;

        private Specimen _flyingSpecimen;
        private Vector3 _mousePositionOffset;

        private int _emptyCellAmount;

        private void Awake()
        {
            grid = new Specimen[gridSize.x, gridSize.y];
            var gridCenterPosition = transform.position;

            _upperLeftCornerWorldCoords = new Vector3(
                gridCenterPosition.x - transform.localScale[0] / 2,
                gridCenterPosition.y + transform.localScale[1] / 2,
                0);
            _bottomRightCornerWorldCoords = new Vector3(
                gridCenterPosition.x + transform.localScale[0] / 2,
                gridCenterPosition.y - transform.localScale[1] / 2,
                0);
            _cellSize = new Vector2(
                (_bottomRightCornerWorldCoords.x - _upperLeftCornerWorldCoords.x) / gridSize.x,
                (_upperLeftCornerWorldCoords.y - _bottomRightCornerWorldCoords.y) / gridSize.y);

            _emptyCellAmount = gridSize.x * gridSize.y;
            
            DrawBorders();
        }

        private void DrawBorders()
        {
            //for (int y = 0; y < gridSize.y + 1; y++)
            for (int y = 1; y < gridSize.y; y++)
            {
                Vector3 startCoord = TransformGridToWorldCoords(
                    new Vector2Int(0, y), false);
                Vector3 endCoord = TransformGridToWorldCoords(
                    new Vector2Int(gridSize.x, y), false);
                
                DrawLine(startCoord, endCoord, Color.gray, $"gridV_{y}");
            }

            //for (int x = 0; x < gridSize.x + 1; x++)
            for (int x = 1; x < gridSize.x; x++)
            {
                Vector3 startCoord = TransformGridToWorldCoords(
                    new Vector2Int(x, 0), false);
                Vector3 endCoord = TransformGridToWorldCoords(
                    new Vector2Int(x, gridSize.y), false);
                
                DrawLine(startCoord, endCoord, Color.gray, $"gridH_{x}");
            }
            
        }
        
        public void DrawLine(Vector3 start, Vector3 end, Color color, string lineName)
        {
            GameObject myLine = new GameObject(lineName);
            myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.sortingOrder = 1;
            lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = gridLineWidth;
            lr.endWidth = gridLineWidth;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.sortingLayerName = "Default";
            lr.sortingOrder = 3;
        }

        public Vector2Int TransformWorldToGridCoords(Vector3 worldCoords)
        {
            bool isInsideGrid = CheckWorldCoordsInsideGrid(worldCoords);
            if (!isInsideGrid)
            {
                throw new System.ArgumentException("World coordinates don't match grid");
            }
            
            Vector2Int gridCoords = new Vector2Int(
                Mathf.FloorToInt((worldCoords.x - _upperLeftCornerWorldCoords.x) / _cellSize.x),
                Mathf.FloorToInt((_upperLeftCornerWorldCoords.y - worldCoords.y) / _cellSize.y)
            );
            
            return gridCoords;
        }

        public Vector3 TransformGridToWorldCoords(Vector2Int gridCoords, bool centered = true)
        {
            Vector3 worldCoords;
            if (centered)
            {
                worldCoords = new Vector3(
                    _upperLeftCornerWorldCoords.x + gridCoords.x * _cellSize.x + _cellSize.x / 2,
                    _upperLeftCornerWorldCoords.y - gridCoords.y * _cellSize.y - _cellSize.y / 2,
                    0
                );
            }
            else
            {
                worldCoords = new Vector3(
                    _upperLeftCornerWorldCoords.x + gridCoords.x * _cellSize.x,
                    _upperLeftCornerWorldCoords.y - gridCoords.y * _cellSize.y,
                    0
                );
            }

            return worldCoords;
        }

        public bool CheckWorldCoordsInsideGrid(Vector3 worldCoords)
        {
            bool isMouseInsideGrid = (
                worldCoords.x >= _upperLeftCornerWorldCoords.x &&
                worldCoords.x <= _bottomRightCornerWorldCoords.x &&
                worldCoords.y <= _upperLeftCornerWorldCoords.y &&
                worldCoords.y >= _bottomRightCornerWorldCoords.y);

            return isMouseInsideGrid;
        }

        public bool CheckGridCoordsValid(Vector2Int gridCoords)
        {
            bool isGridCoordsValid = (
                gridCoords.x >= 0 && gridCoords.x < gridSize.x &&
                gridCoords.y >= 0 && gridCoords.y < gridSize.y);
            return isGridCoordsValid;
        }

        public Specimen GetCell(Vector2Int coords)
        {
            return grid[coords.x, coords.y];
        }
        
        public void SetCell(Vector2Int coords, Specimen newSpecimen, bool changePosition = false)
        {
            var addToAmount = 0;
            if (newSpecimen != null)
            {
                addToAmount = grid[coords.x, coords.y] == null ? -1 : 0;
            }
            else
            {
                addToAmount = grid[coords.x, coords.y] == null ? 0 : 1;
            }
            
            grid[coords.x, coords.y] = newSpecimen;
            if (changePosition)
            {
                newSpecimen.transform.position = TransformGridToWorldCoords(coords);
            }

            _emptyCellAmount += addToAmount;
        }

        public Vector2 GetCellSize()
        {
            return _cellSize;
        }

        public List<Tuple<Specimen, Vector2Int>> GetConnectedSpecimens(
            Vector2Int coords, InfluenceArea searchArea)
        {
            var connectedSpecimens = new List<Tuple<Specimen, Vector2Int>>(); 
            var iterator = searchArea.AreaSpritesIterator();
            foreach (var entry in iterator)
            {
                var shiftLocalCoords = searchArea.TransformArrayCoordsToLocal(
                    entry.Key, true);
                var targetCoords = new Vector2Int(
                    coords.x + (int) shiftLocalCoords.x, coords.y + (int) shiftLocalCoords.y);
                if (CheckGridCoordsValid(targetCoords))
                {
                    var targetCell = GetCell(targetCoords);
                    if (targetCell != null)
                    {
                        var specimenWithCoords = new Tuple<Specimen, Vector2Int>(
                            GetCell(targetCoords), targetCoords);
                        connectedSpecimens.Add(specimenWithCoords);
                    }
                }
            }

            return connectedSpecimens;
        }

        public Dictionary<SpecimenEnum, int> GetGridStatistics(bool hpInsteadAmount = false)
        {
            Dictionary<SpecimenEnum, int> specimenToPoints = new Dictionary<SpecimenEnum, int>();

            foreach (SpecimenEnum enumValue in Enum.GetValues(typeof(SpecimenEnum)))
            {
                specimenToPoints[enumValue] = 0;
            }
            
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (grid[x, y] == null)
                    {
                        continue;
                    }

                    if (hpInsteadAmount)
                    {
                        specimenToPoints[grid[x, y].id] += grid[x, y].hitPoints;
                    }
                    else
                    {
                        specimenToPoints[grid[x, y].id] += 1;
                    }
                }
            }

            return specimenToPoints;
        }

        public int GetEmptyCellAmount()
        {
            return _emptyCellAmount;
        }

        public void SetSilentUIModeForAllSpecimen(bool isSilent)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (grid[x, y] != null)
                    {
                        grid[x, y].specimenUI.SetSilentMode(isSilent);
                    }
                }
            }
        }
    }
}
