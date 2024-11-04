using System;
using System.Collections.Generic;
using UnityEngine;

namespace Further.Managers
{
    public class UpgradeManager
    {
        public Action<PlayerStatType, int> OnUpgradeStat { get; set; }

        private List<int> playerStatLevels =new List<int>();
        private const int STAT_MAX_LEVEL = 6;

        public void Init()
        {
            ResetPlayerStat();
        }

        public void ResetPlayerStat()
        {
            playerStatLevels.Clear();
            for (int i = 0; i < Manager.Data.GetUpgradeTypeCount(); i++)
            {
                playerStatLevels.Add(0);
            }
        }

        public int GetStatLevel(PlayerStatType type)
        {
            return playerStatLevels[(int)type];
        }

        public void UpgradeStat(PlayerStatType type)
        {
            if (playerStatLevels[(int)type] == STAT_MAX_LEVEL)
            {
                Debug.Log($"StatType : {type.ToString()} is on max level already");
            }

            playerStatLevels[(int)type]++;
            Debug.Log($"Player State Upgraded : ({type.ToString()}, {playerStatLevels[(int)type]})");

            OnUpgradeStat.Invoke(type, playerStatLevels[(int)type]);
        }
    }
}