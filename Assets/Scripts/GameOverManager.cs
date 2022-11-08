using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameOverManager : MonoBehaviour
    {
        public SavannaGrid grid;
        public BuildingOptionManager optionManager;
        public Menu.MainSceneExit exitButton;

        public bool CheckSavannaFull()
        {
            var emptyCellAmount = grid.GetEmptyCellAmount();
            return (emptyCellAmount == 0);
        }

        public bool CheckNoBuildingOptionsLeft()
        {
            var optionStatistics = optionManager.GetOptionStatistics();
            
            int totalOptionAmount = 0;
            foreach (var specimenWithAmount in optionStatistics)
            {
                if (specimenWithAmount.Key == SpecimenEnum.Grass)
                {
                    continue;
                }
                totalOptionAmount += specimenWithAmount.Value;
            }
            
            return (totalOptionAmount == 0);
        }

        public void StartGameOverWindow()
        {
            var score = optionManager.scoreBoard.GetScore();
            exitButton.TurnOnGameOver(score);
        }

        public void PlaySound()
        {
            // only used it because of some Unity bug
            GetComponent<AudioSource>().Play();
        }
        
        
    }
}