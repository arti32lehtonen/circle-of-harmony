using Array2DEditor;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "FieldInitializationConfig", menuName = "FieldInitializationConfig", order = 4)]
    public class FieldInitializationConfig : ScriptableObject
    {
        public SpeciesConfig initializeSpecimen;
        public Array2DBool initializationField;
    }
}