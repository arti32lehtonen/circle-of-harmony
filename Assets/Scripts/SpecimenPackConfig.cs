using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "ProgressOption", menuName = "ProgressOption", order = 2)]
    public class SpecimenPackConfig : ScriptableObject, IEnumerable
    {
        public int scoreToBeat;
        public List<SpecimenWithValue<int>> specimenToAmount;

        private IEnumerator<SpecimenWithValue<int>> GetEnumerator()
        {
            foreach (var pack in specimenToAmount)
            {
                yield return pack;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    [Serializable]
    public class SpecimenPackConfigList
    {
        [SerializeField]
        private List<SpecimenPackConfig> configs;

        public SpecimenPackConfig ChooseRandomPack()
        {
            var randomIndex = new Random().Next(0, configs.Count);
            return configs[randomIndex];
        }

        public Tuple<SpecimenPackConfig, int> ChooseRandomPackWithIndex()
        {
            var randomIndex = new Random().Next(0, configs.Count);
            return new Tuple<SpecimenPackConfig, int>(configs[randomIndex], randomIndex);
        }
    }
    
    /// <summary>
    /// We need this class only in the inspector
    /// </summary>
    [Serializable]
    public class SpecimenPackConfigListWithId
    {
        public SpecimenEnum specimenId;
        public float importanceCoeff = 1.0f;
        public SpecimenPackConfigList configs;
        
    }

    public class SpecimenPack : IEnumerable
    {
        public int scoreToBeat;
        public Dictionary<SpecimenEnum, int> specimenToAmount;
        public string packId;

        public SpecimenPack()
        {
            int speciesAmount = Enum.GetValues(typeof(SpecimenEnum)).Length;
            specimenToAmount = new Dictionary<SpecimenEnum, int>();
            foreach (SpecimenEnum specimenId in Enum.GetValues(typeof(SpecimenEnum)))
            {
                specimenToAmount[specimenId] = 0;
            }
        }

        public SpecimenPack(SpecimenPackConfig config)
        {
            scoreToBeat = config.scoreToBeat;
            specimenToAmount = new Dictionary<SpecimenEnum, int>();
            
            foreach (var specimenWithValue in config.specimenToAmount)
            {
                specimenToAmount[specimenWithValue.specimenId] = specimenWithValue.value;
            }

            packId = config.name;
        }
        
        private IEnumerator<KeyValuePair<SpecimenEnum, int>> GetEnumerator()
        {
            foreach (var pack in specimenToAmount)
            {
                yield return pack;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}