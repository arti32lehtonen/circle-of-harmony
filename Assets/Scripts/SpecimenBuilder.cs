using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace DefaultNamespace
{
    public class SpecimenBuilder : MonoBehaviour
    {
        public GlobalSettingsConfig globalConfig;
        public SavannaGrid grid;
        public GameObject canvasPrefub;
        public ScoreBoard scoreBoard;
        public BuildingOptionManager buildingManager;
        public StatsManager statsManager;
        
        // to find GameObject in hierarchy
        private const string  _canvasPrefubName = "SpecimenUI";
        
        private Specimen _flyingSpecimen;
        private List<Tuple<Specimen, Vector2Int>> _previouslyConnectedSpecimens;
        private Vector3 _mousePositionOffset;
        private bool _isAvailableToPlace;

        public void Start()
        {
            var intFieldIndex = new Random().Next(0, globalConfig.fieldInitializationConfigs.Count);
            
            FillWithSpecimen(globalConfig.fieldInitializationConfigs[intFieldIndex]);
            Invoke("SwitchSoundMute", 1.2f);
        }

        private void SwitchSoundMute()
        {
            var audioComponent = GetComponent<AudioSource>();
            audioComponent.mute = !audioComponent.mute;
        }
        
        private static Vector3 GetMouseWorldPosition()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        public GameObject InitializeNewSpecimenObject(SpeciesConfig config)
        {
            return Specimen.InitializeNewSpecimenObject(
                config,
                globalConfig,
                grid.GetCellSize(),
                canvasPrefub,
                _canvasPrefubName
            );
        }
        
        public void CreateNewFlyingSpecimen(SpeciesConfig config)
        {
            if (_flyingSpecimen != null)
            {
                Destroy(_flyingSpecimen.gameObject);
            }

            Vector3 mouseCoordinates = GetMouseWorldPosition();
            mouseCoordinates.z = 0.0f;
            var specimenObject = InitializeNewSpecimenObject(config);
            specimenObject.transform.position = mouseCoordinates;
            
            _mousePositionOffset = specimenObject.transform.position - GetMouseWorldPosition();
            _flyingSpecimen = specimenObject.GetComponent<Specimen>();
            _flyingSpecimen.specimenUI.SetSilentMode(true);
            grid.SetSilentUIModeForAllSpecimen(true);
            
            scoreBoard.ShowScore(true, 0);
        }
        
        public void FillRandomlyWithSpecimen(SpeciesConfig config, float percent)
        {
            var elementsAmount = grid.gridSize.x * grid.gridSize.y;
            var specimenAmount = (int) (elementsAmount * percent);
            int[] indexes = new int[elementsAmount];
            for (int i = 0; i < elementsAmount; i++)
            {
                indexes[i] = i;
            }
            Random rnd = new Random();
            int[] randomIndexes = indexes.OrderBy(x => rnd.Next()).ToArray();
            for (int i = 0; i < specimenAmount; i++)
            {
                var cellIndex = randomIndexes[i];
                var coords = new Vector2Int(
                    cellIndex / grid.gridSize.y,
                    cellIndex % grid.gridSize.y);

                var newSpecimenObject = InitializeNewSpecimenObject(config);
                var specimenComponent = newSpecimenObject.GetComponent<Specimen>();
                SetGridCell(coords, specimenComponent, true);
            }
        }

        public void FillWithSpecimen(FieldInitializationConfig initializationConfig)
        {
            for (int i = 0; i < grid.gridSize.x; i++)
            {
                for (int j = 0; j < grid.gridSize.y; j++)
                {
                    if (!initializationConfig.initializationField.GetCell(i, j))
                    {
                        continue;
                    }
                    
                    var coords = new Vector2Int(i,j);
                    var newSpecimenObject = InitializeNewSpecimenObject(
                        initializationConfig.initializeSpecimen);
                    var specimenComponent = newSpecimenObject.GetComponent<Specimen>();
                    SetGridCell(coords, specimenComponent, true);
                    
                    var stateForStats = new StatsActionState(
                        initializationConfig.initializeSpecimen.id,
                        0,
                        "place_init",
                        coords,
                        new List<string>(),
                        -1);
                    statsManager.UpdateStats(stateForStats);
                }
            }
        }

        public void ChangeAvailabilityStatus(
            bool newStatus, Vector2Int gridCoords, Vector2Int gridSize)
        {
            if (newStatus)
            {
                // transform.GetComponent<Renderer>().material.color = Color.blue;
                _flyingSpecimen.GetComponent<SpriteRenderer>().material.color = Color.blue;
                _flyingSpecimen.influenceArea.SetActivationStatus(
                    true, gridCoords, grid);
            }
            else
            {
                // transform.GetComponent<Renderer>().material.color = Color.red;
                _flyingSpecimen.GetComponent<SpriteRenderer>().material.color = Color.red;
                _flyingSpecimen.influenceArea.SetActivationStatus(
                    false, gridCoords, grid);
            }

            _isAvailableToPlace = newStatus;
        }

        public void SetGridCell(Vector2Int gridCoords, Specimen specimen, bool changePosition = false)
        {
            var audioComponent = GetComponent<AudioSource>();
            audioComponent.Play();
            
            grid.SetCell(gridCoords, specimen, changePosition);

            var spriteComponent = specimen.GetComponent<SpriteRenderer>();
            spriteComponent.material.color = Color.white;
            spriteComponent.sortingOrder = 0;
        }

        private int Feed(
            List<Tuple<Specimen, Vector2Int>> connectedSpecimens,
            List<Tuple<SpecimenEnum, Vector2Int>> deadSpecimensCoordWithId)
        {
            FeedingIncome feedingIncome;

            int addScore = 0;
            foreach (var specimenWithCoords in connectedSpecimens)
            {
                var specimen = specimenWithCoords.Item1;
                var specimenCoords = specimenWithCoords.Item2;
                
                feedingIncome = _flyingSpecimen.GetPossibleFeedingIncome(specimen);
                
                if (feedingIncome == null)
                {
                    continue;
                }

                addScore += feedingIncome.score;
                specimen.hitPoints = specimen.CalculateHitPointsIfDamage(feedingIncome.damage);
                
                if (specimen.hitPoints == 0)
                {
                    grid.SetCell(specimenCoords, null);

                    if (specimen.whatPlaceAfterDeath != null)
                    {
                        var newSpecimenObject = InitializeNewSpecimenObject(
                            specimen.whatPlaceAfterDeath);
                        
                        SetGridCell(
                            specimenCoords,
                            newSpecimenObject.GetComponent<Specimen>(), 
                            true);
                    }

                    deadSpecimensCoordWithId.Add(
                        new Tuple<SpecimenEnum, Vector2Int>(specimen.id, specimenCoords));
                    Destroy(specimen.gameObject);
                }
            }
            
            scoreBoard.AddValueToScore(addScore);
            return addScore;
        }

        public void ResetPreviouslyConnectedSpecimens()
        {
            if (_previouslyConnectedSpecimens == null)
            {
                return;
            }

            foreach (var specimenWithCoords in _previouslyConnectedSpecimens)
            {
                specimenWithCoords.Item1.specimenUI.SetSilentMode(true);
                //specimenWithCoords.Item1.specimenUI.DeactivateUI();
            }
            _previouslyConnectedSpecimens = null;
        }

        public void ShowPossibleScore(List<Tuple<Specimen, Vector2Int>> connectedSpecimens)
        {
            int addScore = 0;
            foreach (var specimenWithCoords in connectedSpecimens)
            {
                var specimen = specimenWithCoords.Item1;
                var feedingIncome = _flyingSpecimen.GetPossibleFeedingIncome(specimen);
                if (feedingIncome != null)
                {
                    bool willBeAlive = feedingIncome.damage < specimen.hitPoints;
                    var maxDamage = willBeAlive ? feedingIncome.damage : specimen.hitPoints;
                    
                    specimenWithCoords.Item1.specimenUI.SetSilentMode(false);
                    specimen.specimenUI.ShowPossibleDamage(
                        maxDamage, feedingIncome.score, !willBeAlive);
                    
                    addScore += feedingIncome.score;
                }
            }
            scoreBoard.ShowScore(true, addScore);
        }

        public void StopPlacingSpecimen()
        {
            if (_flyingSpecimen != null)
            {
                ResetPreviouslyConnectedSpecimens();
                grid.SetSilentUIModeForAllSpecimen(false);
                scoreBoard.ShowScore(false, 0);
                Destroy(_flyingSpecimen.gameObject);
                _flyingSpecimen = null;
            }
        }

        private void ShowSpecimenOutsideGrid(Vector2Int gridMouseCoords, Vector3 newMousePosition)
        {
            ChangeAvailabilityStatus(false, gridMouseCoords, grid.gridSize);
            _flyingSpecimen.transform.position = newMousePosition;
            ResetPreviouslyConnectedSpecimens();
            scoreBoard.ShowScore(true, 0);
        }

        private List<Tuple<Specimen, Vector2Int>> UpdateConnectedSpecimensInfo(
            Vector2Int gridMouseCoords)
        {
            List<Tuple<Specimen, Vector2Int>> connectedSpecimens = grid.GetConnectedSpecimens(
                gridMouseCoords, _flyingSpecimen.influenceArea);
            
            if (_previouslyConnectedSpecimens != null)
            {
                bool isSame = 
                    _previouslyConnectedSpecimens.Count == connectedSpecimens.Count &&
                    !_previouslyConnectedSpecimens.Except(connectedSpecimens).Any();
                
                if (!isSame)
                {
                    ResetPreviouslyConnectedSpecimens();
                    _previouslyConnectedSpecimens = connectedSpecimens;
                }
            }
            else
            {
                _previouslyConnectedSpecimens = connectedSpecimens;
            }

            return connectedSpecimens;
        }

        private void PlaceSpecimenOnGrid(
            Vector2Int gridMouseCoords, List<Tuple<Specimen, Vector2Int>> connectedSpecimens)
        {
            SetGridCell(gridMouseCoords, _flyingSpecimen);
            buildingManager.DecreaseOptionAmount(_flyingSpecimen.id);
            var deadSpecimensCoordToId = new List<Tuple<SpecimenEnum, Vector2Int>>();
            var addedScore = Feed(connectedSpecimens, deadSpecimensCoordToId);
            ResetPreviouslyConnectedSpecimens();
            var stateIfNeedUndo = new BuildActionState(gridMouseCoords, addedScore, deadSpecimensCoordToId);
            buildingManager.HandleFinishBuilding(_flyingSpecimen.id, stateIfNeedUndo);
            
            var diedIds = new List<string>();
            foreach (var idWithCoord in deadSpecimensCoordToId)
            {
                diedIds.Add(idWithCoord.Item1.ToString());
            }
            
            var stateForStats = new StatsActionState(
                _flyingSpecimen.id,
                addedScore,
                "place",
                gridMouseCoords,
                diedIds,
                buildingManager.GetPackIndex());
            statsManager.UpdateStats(stateForStats);

            _flyingSpecimen.influenceArea.DestroyVisualComponents();
            //_flyingSpecimen.specimenUI.SetSilentMode(false);
            grid.SetSilentUIModeForAllSpecimen(false);

                
            _flyingSpecimen = null;
        }
        
        private void Update()
        {
            // no specimen is currently being built 
            if (_flyingSpecimen == null)
            {
                return;
            }
            
            if (Input.GetMouseButton(1))
            {
                StopPlacingSpecimen();
                return;
            }

            Vector3 newMousePosition = GetMouseWorldPosition() + _mousePositionOffset;
            bool isMouseInsideGrid = grid.CheckWorldCoordsInsideGrid(newMousePosition);
            if (!isMouseInsideGrid)
            {
                // if a mouse is outside the grid, then gridMouseCoords has no influence
                ShowSpecimenOutsideGrid(Vector2Int.zero, newMousePosition);
                return;
            }
            
            Vector2Int gridMouseCoords = grid.TransformWorldToGridCoords(newMousePosition);
            if (grid.GetCell(gridMouseCoords) != null)
            {
                ShowSpecimenOutsideGrid(gridMouseCoords, newMousePosition);
                return;
            }
            
            Vector3 newSpecimenWorldCoords = grid.TransformGridToWorldCoords(gridMouseCoords);                    
            _flyingSpecimen.transform.position = newSpecimenWorldCoords;
            ChangeAvailabilityStatus(true, gridMouseCoords, grid.gridSize);

            var connectedSpecimens = UpdateConnectedSpecimensInfo(gridMouseCoords);
            ShowPossibleScore(connectedSpecimens);

            if (Input.GetMouseButtonDown(0))
            {
                PlaceSpecimenOnGrid(gridMouseCoords, connectedSpecimens);
            }

        }
    }
}