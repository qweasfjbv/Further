
using System.Collections.Generic;
using UnityEngine;

namespace Further.Managers
{

    public enum PlayerStatType
    {
        MissileCooltime,
        MissileSize,
    }

    public enum PlayerAbilityType
    {

    }

    public class DataManager
    {
        private List<Dictionary<string, object>> upgradeData;
        
        public void Init()
        {
            upgradeData = CSVReader.Read("Data/Excel/UpgradeData");

            Debug.Log("TEXT CSV" + upgradeData[0]["1"]);
        }
    }
}