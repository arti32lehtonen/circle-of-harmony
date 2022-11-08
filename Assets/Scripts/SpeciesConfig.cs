using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Species", menuName = "Species", order = 1)]
    public class SpeciesConfig : ScriptableObject
    {
        public SpecimenEnum id;
        public string displayedName;
        [TextArea]
        public string hint;
        public Sprite image;
        public int hitPoints;
        public Array2DEditor.Array2DBool influenceArea;
        public List<FeedingOptionInspector> feedingOptions;
        public SpeciesConfig whatPlaceAfterDeath;
    }
}