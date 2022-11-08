using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [Serializable]
    public class SpecimenAppearanceOptions
    {
        public SpecimenEnum preyId;
        public List<SpecimenWithValue<float>> hunterOptions;
    }
    
    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "GlobalSettings", order = 0)]
    public class GlobalSettingsConfig : ScriptableObject
    {
        public Sprite influenceAreaSprite;
        public Color influenceAreaColor;
        public string influenceAreaSortingLayer;
        public string specimenSortingLayer;

        public SpeciesConfig grassConfig;
        public float initialGrassProportion = 0f;

        public List<FieldInitializationConfig> fieldInitializationConfigs;
        
        public List<SpecimenPackConfigList> startProgressPacks;
        public List<SpecimenPackConfigListWithId> progressPacks;
        
        // hit points displayer
        public Color hitPointsUIAnimationColor;
        public float hitPointsUIAnimationDelay;
    }
}