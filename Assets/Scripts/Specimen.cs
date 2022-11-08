using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public enum SpecimenEnum
    {
        Grass,
        Bug,
        Rodent,
        BigHerbivore,
        SmallPredator,
        BigPredator,
    }
    
    public class Specimen : MonoBehaviour
    {
        public SpecimenEnum id;
        public string displayedName;
        public int hitPoints;
        public InfluenceArea influenceArea;

        public Vector2Int GizmosSize = Vector2Int.one;
        public Vector2Int lastGridCoords = Vector2Int.zero;
        
        public Dictionary<SpecimenEnum, FeedingIncome> feedingOptions;
        public SpeciesConfig whatPlaceAfterDeath = null;

        public HitPointsDisplayer specimenUI;
        
        public void LoadFromConfig(GlobalSettingsConfig globalConfig, SpeciesConfig config)
        {
            id = config.id;
            displayedName = config.displayedName;
            hitPoints = config.hitPoints;
            whatPlaceAfterDeath = config.whatPlaceAfterDeath;
            
            influenceArea = new InfluenceArea(globalConfig, config, transform, false);
            feedingOptions = new Dictionary<SpecimenEnum, FeedingIncome>();
            foreach (var option in config.feedingOptions)
            {
                feedingOptions[option.entityToEat] = option.income;
            }
            
            var spriteComponent = transform.GetComponent<SpriteRenderer>();
            spriteComponent.sprite = config.image;
            spriteComponent.sortingLayerName = globalConfig.specimenSortingLayer;
        }

        public void InitializeUI(GameObject canvasPrefab, string objectName, GlobalSettingsConfig config)
        {
            var canvasObject = Instantiate(canvasPrefab, transform, false);
            canvasObject.name = objectName;
            var healthUIComponent = canvasObject.AddComponent<HitPointsDisplayer>();
            healthUIComponent.Initialize(canvasObject, this, config);
            specimenUI = healthUIComponent;
            specimenUI.DeactivateUI();
        }
        
        /// <summary>
        /// Initialize specimen GameObject with all components. 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="globalConfig"></param>
        /// <param name="localScale">Object scale</param>
        /// <param name="objectUIPrefub">Prefab object for UI</param>
        /// <param name="objectUIName">Name of the UI game object in the Unity</param>
        /// <returns></returns>
        public static GameObject InitializeNewSpecimenObject(
            SpeciesConfig config, GlobalSettingsConfig globalConfig, Vector2 localScale,
            GameObject objectUIPrefab, string objectUIName)
        {
            var specimenObject = new GameObject(config.displayedName);
            var spriteComponent = specimenObject.AddComponent<SpriteRenderer>();
            spriteComponent.transform.localScale = new Vector3(localScale.x, localScale.y, 1);
            spriteComponent.sortingOrder = 1;
            
            var specimenComponent = specimenObject.AddComponent<Specimen>();
            specimenComponent.LoadFromConfig(globalConfig, config);
            specimenComponent.InitializeUI(objectUIPrefab, objectUIName, globalConfig);

            var colliderComponent = specimenObject.AddComponent<BoxCollider2D>();

            return specimenObject;
        }


        public FeedingIncome GetPossibleFeedingIncome(Specimen targetSpecimen)
        {
            if (!feedingOptions.ContainsKey(targetSpecimen.id))
            {
                return null;
            }

            return feedingOptions[targetSpecimen.id];
        }

        public int CalculateHitPointsIfDamage(int possibleDamage)
        {
            return Mathf.Max(0, hitPoints - possibleDamage);
        }

        public void OnMouseEnter()
        {
            specimenUI.ShowCurrentHitPoints();
        }

        public void OnMouseExit()
        {
            specimenUI.DeactivateUI();
        }

        private void OnDrawGizmos()
        {
            float addGizmosX = (GizmosSize.x % 2 == 0) ? 0.5f : 0;
            float addGizmosY = (GizmosSize.y % 2 == 0) ? 0.5f : 0;


            for (int x = 0; x < GizmosSize.x; x++)
            {
                for (int y = 0; y < GizmosSize.y; y++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        Gizmos.color = new Color(0.88f, 0f, 1f, 0.3f);
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 0.68f, 0f, 0.3f);
                    }
                    
                    float xGlobal = x - GizmosSize.x / 2 + addGizmosX;
                    float yGlobal = y - GizmosSize.y / 2 + addGizmosY;

                    Gizmos.DrawCube(
                        transform.position + new Vector3(xGlobal, yGlobal, 0), 
                        new Vector3(1f, 1f, 0.5f));
                }
            }
        }
    }
}