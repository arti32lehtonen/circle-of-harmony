using System;
using UnityEngine;

namespace DefaultNamespace
{
    [Serializable]
    public class SpecimenWithValue<T>
    {
        public SpecimenEnum specimenId;
        public T value;
        
        public SpecimenWithValue(SpecimenEnum specimenId, T value)
        {
            this.specimenId = specimenId;
            this.value = value;
        }
    }

}