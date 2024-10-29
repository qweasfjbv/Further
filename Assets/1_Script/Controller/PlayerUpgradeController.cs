using Further.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Further
{
    public class PlayerUpgradeController : MonoBehaviour
    {

        private List<int> upgradeLevel;
        private Array playerStatArray;

        private void Start()
        {
            playerStatArray = System.Enum.GetValues(typeof(PlayerStatType));
            upgradeLevel = new List<int>(playerStatArray.Length);

            foreach (var type in playerStatArray)
            {
                UpdatePlayerStat((PlayerStatType)type, 0);
            }
        }


        private void UpdatePlayerStat(PlayerStatType statType, int level)
        {
            int getIntValue = (int)Manager.Data.GetUpgradeData(statType, level);
            float getFloatValue = (float)Manager.Data.GetUpgradeData(statType, level);

            switch (statType)
            {   
                case PlayerStatType.MissileCooltime:

                    break;
                case PlayerStatType.BoostThrust:
                    break;
                case PlayerStatType.Health:
                    break;
                case PlayerStatType.Sight:
                    break;
                case PlayerStatType.Gravity:
                    break;
                case PlayerStatType.MissileCount:
                    break;
                case PlayerStatType.MissileSize:
                    break;
                case PlayerStatType.BoostAttack:
                    break;
                case PlayerStatType.MeteorCount:
                    break;
                case PlayerStatType.MagnetSize:
                    break;
                case PlayerStatType.BulletBoundCount:
                    break;

                default:
                    Debug.LogError("PlayerStatType doesn't match!");
                    break;

            }

    }




    }
}