
using System.Collections.Generic;

namespace Further.Managers
{
    public class DataManager
    {
        private List<Dictionary<string, object>> upgradeData;
        
        public void Init()
        {
            upgradeData = CSVReader.Read("Data/Excel/UpgradeData");
        }

        public int GetUpgradeTypeCount()
        {
            return upgradeData.Count;
        }

        // Unboxing to Int or Float needed
        public object GetUpgradeData(PlayerStatType type, int level)
        {
            return upgradeData[(int)type][level.ToString()];
        }
    }
}