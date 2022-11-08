using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class BuildActionState
    {
        public Vector2Int specimenCoords;
        public int addedScore;
        public List<Tuple<SpecimenEnum, Vector2Int>> lastDeaths;

        public BuildActionState(Vector2Int specimenCoords, int addedScore,
            List<Tuple<SpecimenEnum, Vector2Int>> lastDeaths)
        {
            this.specimenCoords = specimenCoords;
            this.addedScore = addedScore;
            this.lastDeaths = lastDeaths;
        }
    }
    
    public class UndoButton : MonoBehaviour
    {
        public Sprite buttonOnImage;
        public Sprite buttonOffImage;
        private Stack<BuildActionState> _lastActions = new Stack<BuildActionState>();

        public BuildingOptionManager optionManager;

        public void Awake()
        {
            SetButtonState(false);
        }

        public void SetButtonState(bool isActive)
        {
            var imageComponent = GetComponent<Image>();
            transform.GetComponent<Button>().interactable = isActive;

            
            if (isActive)
            {
                imageComponent.sprite = buttonOnImage;
            }
            else
            {
                imageComponent.sprite = buttonOffImage;
            }
        }

        public void AddNewAction(BuildActionState newBuildActionState)
        {
            _lastActions.Push(newBuildActionState);
            
            // need to change button state
            if (_lastActions.Count == 1)
            {
                SetButtonState(true);
            }
        }
        
        public void ReverseLastAction()
        {
            GetComponent<AudioSource>().Play();
            
            if (_lastActions.Count == 0)
            {
                return;
            }

            var lastAction = _lastActions.Pop();
            var grid = optionManager.builder.grid;
            
            var placedSpecimen = grid.GetCell(lastAction.specimenCoords);
            optionManager.AddValueToButton(placedSpecimen.id, 1);

            // restore hp for connected elements
            var connectedSpecimens = grid.GetConnectedSpecimens(
                lastAction.specimenCoords, placedSpecimen.influenceArea);
            foreach (var specimenWithCoords in connectedSpecimens)
            {
                if (placedSpecimen.feedingOptions.ContainsKey(specimenWithCoords.Item1.id))
                {
                    var damage = placedSpecimen.feedingOptions[specimenWithCoords.Item1.id].damage;
                    if (damage > 0)
                    {
                        specimenWithCoords.Item1.hitPoints += damage;
                    }
                }
            }

            foreach (var specimenIdWithCoords in lastAction.lastDeaths)
            {
                var currentCellObject = grid.GetCell(specimenIdWithCoords.Item2);
                if (currentCellObject != null)
                {
                    grid.SetCell(specimenIdWithCoords.Item2, null);
                    Destroy(currentCellObject.gameObject);
                }

                var specimenConfig = optionManager.GetButton(specimenIdWithCoords.Item1).config;
                var newSpecimenObject = optionManager.builder.InitializeNewSpecimenObject(specimenConfig);
                var newSpecimen = newSpecimenObject.GetComponent<Specimen>();
                // this works only if all animal damage is equal to 1
                // if you want to rewrite it, you also need to store damage in state class
                newSpecimen.hitPoints = 1;
                
                grid.SetCell(
                    specimenIdWithCoords.Item2,
                    newSpecimen, 
                    true);
            }
            
            // revert ScoreBoard
            optionManager.scoreBoard.HandleUndoAction(lastAction.addedScore);
            
            var diedIds = new List<string>();
            foreach (var idWithCoord in lastAction.lastDeaths)
            {
                diedIds.Add(idWithCoord.Item1.ToString());
            }
            
            // log in stats manager
            var stateForStats = new StatsActionState(
                placedSpecimen.id,
                lastAction.addedScore,
                "undo_place",
                lastAction.specimenCoords,
                diedIds,
                optionManager.GetPackIndex());
            optionManager.builder.statsManager.UpdateStats(stateForStats);
            
            // destroy old object
            grid.SetCell(lastAction.specimenCoords, null);
            Destroy(placedSpecimen.gameObject);

            if (_lastActions.Count == 0)
            {
                SetButtonState(false);
            }
        }

        public void ResetActionHistory()
        {
            while (_lastActions.Count > 0)
            {
                _lastActions.Pop();
            }

            SetButtonState(false);
        }
    }
}