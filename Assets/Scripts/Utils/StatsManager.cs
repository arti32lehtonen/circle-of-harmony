using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace DefaultNamespace
{
    public class StatsActionState
    {
        public string specimenId;
        public int earnedPoints;
        public string actionType;
        public int xCoord;
        public int yCoord;
        public List<string> diedSpecimens;
        public int packIndex;
        public string comment;

        public StatsActionState(
            SpecimenEnum specimenId, int earnedPoints,
            string actionType, Vector2Int coords, List<string> diedSpecimens,
            int packIndex, string comment = "")
        {
            this.specimenId = specimenId.ToString();
            this.earnedPoints = earnedPoints;
            this.actionType = actionType;
            this.xCoord = coords.x;
            this.yCoord = coords.y;
            this.diedSpecimens = diedSpecimens;
            this.packIndex = packIndex;
            this.comment = comment;
        }

        public StatsActionState(
            SpecimenEnum specimenId, int earnedPoints, string actionType, 
            int packIndex, string packId) :
            this(specimenId, earnedPoints, actionType, Vector2Int.zero, 
                new List<String>(), packIndex, packId)
        { }
}
    
    public class StatsManager : MonoBehaviour
    {
        private List<StatsActionState> events = new List<StatsActionState>();

        public string statsPathPrefix;
        public bool isActive = true;
        
        public void UpdateStats(StatsActionState action)
        {
            if (isActive)
            {
                events.Add(action);
            }
        }

        public void SaveData()
        {
            if (isActive)
            {
                string pointsData = JsonConvert.SerializeObject(events);
                DateTime now = DateTime.Now;
                string path = 
                    $"{statsPathPrefix}{now.Year}{now.Month}{now.Day}{now.Hour}{now.Minute}.txt";
            
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(pointsData);
                }
            }
        }
        
    }
}