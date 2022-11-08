using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = System.Random;

namespace DefaultNamespace
{
    [Serializable]
    public class BuilderContainersInspector{
        public SpecimenEnum specimenId;
        public BuildingButtonManager button;
    }
    
    public class BuildingOptionManager : MonoBehaviour
    {
        public List<BuilderContainersInspector> containersInspector;
        public SpecimenBuilder builder;
        public GameOverManager gameOverManager;
        public UndoButton undoButton;
        public GlobalSettingsConfig globalConfig;
        public ScoreBoard scoreBoard;
        public Sprite buttonOnImage;
        public Sprite buttonOffImage;
        public AudioClip buttonClickSound;
        public AudioClip buttonAddSound;
        public ButtonHintManager hintManager;
        public TutorialManager tutorialManager;
        
        public float addEffectLatency;
        
        private int packIndex = 0;
        private Dictionary<SpecimenEnum, BuildingButtonManager> _idToOption;
        
        private bool _isUpdatingNow = false;
        
        private RandomGeneration _randomGenerator = new RandomGeneration();
        private Tuple<SpecimenEnum, int> _lastChosenSpecimenPack;

        private void Awake()
        {
            _idToOption = new Dictionary<SpecimenEnum, BuildingButtonManager>();
            foreach (var container in containersInspector)
            {
                _idToOption[container.specimenId] = container.button;
                container.button.SetAmount(0);

                var audioComponent = container.button.gameObject.AddComponent<AudioSource>();
                audioComponent.clip = buttonClickSound;
            }
        }

        private void Start()
        {
            var nextPack = GetNextPack();
            StartCoroutine(AddNewPack(nextPack));
        }

        public BuildingButtonManager GetButton(SpecimenEnum specimenId)
        {
            return _idToOption[specimenId];
        }
        
        public void AddValueToButton(SpecimenEnum specimenId, int value)
        {
            _idToOption[specimenId].AddToAmount(value);
        }

        public void HandleNewBuildingAction(SpeciesConfig config)
        {
            var idOption = _idToOption[config.id];
            if (idOption.GetAmount() == 0)
            {
                throw new InvalidOperationException("Can't build if amount = 0");
            }

            // need to handle result after building
            builder.CreateNewFlyingSpecimen(config);
        }

        public void DecreaseOptionAmount(SpecimenEnum specimenId)
        {
            AddValueToButton(specimenId, -1);
        }

        public void HandleFinishBuilding(SpecimenEnum specimenId, BuildActionState newState)
        {
            undoButton.AddNewAction(newState);
            if (gameOverManager.CheckSavannaFull())
            {
                gameOverManager.StartGameOverWindow();
            }
        }

        public void HandleBeatenScore()
        {
            undoButton.ResetActionHistory();
            
            packIndex += 1;
            var nextPack = GetNextPack();
            
            foreach (KeyValuePair<SpecimenEnum, int> item in nextPack)
            {
                if (item.Value == 0)
                {
                    continue;
                }

                var newOptionEvent = new StatsActionState(
                    item.Key, item.Value, "new_option", packIndex, "");
                
                builder.statsManager.UpdateStats(newOptionEvent);
            }
            
            var mainPackScoreEvent = new StatsActionState(
                SpecimenEnum.Grass, nextPack.scoreToBeat, 
                "new_option_score", packIndex, nextPack.packId);
            builder.statsManager.UpdateStats(mainPackScoreEvent);

            StartCoroutine(AddNewPack(nextPack));
        }

        public int GetPackIndex()
        {
            return packIndex;
        }

        private SpecimenPack GetNextPack()
        {
            if (packIndex < globalConfig.startProgressPacks.Count)
            {
                var randomConfig = globalConfig.startProgressPacks[packIndex].ChooseRandomPack();
                return new SpecimenPack(randomConfig);
            }
            
            var newPack = ChooseRandomPack();
            return newPack;
        }

        private IEnumerator AddNewPack(SpecimenPack newPack)
        {
            while (_isUpdatingNow)
            {
                yield return new WaitForSeconds(2 * addEffectLatency);
            }

            _isUpdatingNow = true;
            
            scoreBoard.UpdateMaxValue(newPack.scoreToBeat);
            foreach (KeyValuePair<SpecimenEnum, int> item in newPack)
            {
                if (item.Value == 0)
                {
                    continue;
                }
                yield return new WaitForSeconds(addEffectLatency);
                
                _idToOption[item.Key].SwitchOveredEffect(true, true);
                yield return new WaitForSeconds(0.2f);
                _idToOption[item.Key].SwitchOveredEffect(false, true);
                AddValueToButton(item.Key, item.Value);
                GetComponent<AudioSource>().Play();
            }
            
            builder.StopPlacingSpecimen();
            tutorialManager.ShowNextTutorial();
            _isUpdatingNow = false;
        }
        
        public Dictionary<SpecimenEnum, int> GetOptionStatistics()
        {
            var specimenToPoints = new Dictionary<SpecimenEnum, int>(); 
            foreach (SpecimenEnum enumValue in Enum.GetValues(typeof(SpecimenEnum)))
            {
                specimenToPoints[enumValue] = 0;
            }

            foreach (var specimenWithButton in  _idToOption)
            {
                int amount = specimenWithButton.Value.GetAmount();
                specimenToPoints[specimenWithButton.Key] = amount;
            }

            return specimenToPoints;
        }

        private Dictionary<int, SpecimenEnum> GetRelevantPackMapping()
        {
            var indexToSpecimenId = new Dictionary<int, SpecimenEnum>();
            int index = 0;
            foreach (var packOfPacks in globalConfig.progressPacks)
            {
                bool needToExclude = false;

                if (_lastChosenSpecimenPack != null)
                {
                    if (_lastChosenSpecimenPack.Item1 == SpecimenEnum.BigPredator)
                    {
                        needToExclude = (
                            packOfPacks.specimenId == SpecimenEnum.BigPredator ||
                            packOfPacks.specimenId == SpecimenEnum.SmallPredator);
                    } else if (_lastChosenSpecimenPack.Item1 == packOfPacks.specimenId)
                    {
                        needToExclude = true;
                    }
                }

                if (needToExclude)
                {
                    continue;
                }
                
                indexToSpecimenId[index] = packOfPacks.specimenId;
                index++;
            }

            return indexToSpecimenId;
        }

        private SpecimenPack ChooseRandomPack()
        {
            var specimenStatistics = builder.grid.GetGridStatistics(
                hpInsteadAmount: false);

            var indexToSpecimenId = GetRelevantPackMapping();

            var possibleFoodSpecimenStats = new float[indexToSpecimenId.Count];
            float maxValue = 0f;
            for (int i = 0; i < indexToSpecimenId.Count; i++)
            {
                possibleFoodSpecimenStats[i] = 
                    specimenStatistics[indexToSpecimenId[i]] * 
                    globalConfig.progressPacks[i].importanceCoeff;
                if (possibleFoodSpecimenStats[i] > maxValue)
                {
                    maxValue = possibleFoodSpecimenStats[i];
                }
            }

            var specimensWhoseMost = new List<SpecimenEnum>(); 
            for (int i = 0; i < possibleFoodSpecimenStats.Length; i++)
            {
                if (Math.Abs(possibleFoodSpecimenStats[i] - maxValue) < 1e-10)
                {
                    specimensWhoseMost.Add(indexToSpecimenId[i]);
                }
            }
            SpecimenEnum chosenSpecimen = _randomGenerator.RandomChoose(specimensWhoseMost);

            foreach (var packOfPacks in globalConfig.progressPacks)
            {
                if (packOfPacks.specimenId == chosenSpecimen)
                {
                    var randomPackConfigWithIndex = 
                        packOfPacks.configs.ChooseRandomPackWithIndex();
                    _lastChosenSpecimenPack = new Tuple<SpecimenEnum, int>(
                        chosenSpecimen, randomPackConfigWithIndex.Item2);
                    return new SpecimenPack(randomPackConfigWithIndex.Item1);
                }
            }

            throw new InvalidOperationException("at least one element should fit");
        }

        private SpecimenPack GenerateRandomPack()
        {
            var randomGenerator = new RandomGeneration();
            var idIndexes = new SpecimenEnum[Enum.GetValues(typeof(SpecimenEnum)).Length];
            int i = 0;
            foreach (SpecimenEnum preyId in Enum.GetValues(typeof(SpecimenEnum)))
            {
                idIndexes[i] = preyId;
                i++;
            }
            idIndexes = randomGenerator.ShuffleArray(idIndexes);
            
            SpecimenPack newPack = new SpecimenPack();

            newPack.specimenToAmount[idIndexes[0]] = randomGenerator.GenerateUniformInt(2, 3);
            newPack.specimenToAmount[idIndexes[1]] = randomGenerator.GenerateUniformInt(1, 3);
            newPack.specimenToAmount[idIndexes[2]] = randomGenerator.GenerateUniformInt(1, 3);
            newPack.scoreToBeat = 30;

            return newPack;
        }

    }    
}
