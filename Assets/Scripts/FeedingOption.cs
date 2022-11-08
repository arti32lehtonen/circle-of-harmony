using UnityEngine;

namespace DefaultNamespace
{
    [System.Serializable]
    public class FeedingIncome
    {
        public int damage;
        public int score;
    }
    
    [System.Serializable]
    public class FeedingOptionInspector
    {
        public SpecimenEnum entityToEat;
        public FeedingIncome income;
    }
}