using Further.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Further
{
    public class PlayerUpgradeController : MonoBehaviour
    {
        private Array playerStatArray;

        private void Start()
        {
            playerStatArray = Enum.GetValues(typeof(PlayerStatType));
            foreach (var type in playerStatArray)
            {
                UpdatePlayerStat((PlayerStatType)type, 0);
            }

            Manager.Upgrade.OnUpgradeStat = UpdatePlayerStat;
        }


        private void UpdatePlayerStat(PlayerStatType statType, int level)
        {

            object obj = Manager.Data.GetUpgradeData(statType, level);
            int getIntValue = 1;
            float getFloatValue = 1.0f;

            if (obj is int)
            {
                getIntValue = (int)obj;
                getFloatValue = (float)getIntValue;
            }
            else if (obj is float)
            {
                getFloatValue = (float)obj;
                getIntValue = (int)(getFloatValue);
            }
            else
            {
                Debug.LogError("UpdatePlayerStat : obj can't be casted to float or int");
                return;
            }

            switch (statType)
            {
                case PlayerStatType.Attack:
                    break;
                case PlayerStatType.MissileCooltime:
                    GetComponent<PlayerWeaponController>().FireCooltime = getFloatValue;
                    break;
                case PlayerStatType.BoostThrust:
                    GetComponent<PlayerController>().BoostForce = getFloatValue;
                    break;
                case PlayerStatType.Health:
                    GetComponent<PlayerController>().Health = getIntValue;
                    break;
                case PlayerStatType.Sight:
                    GetComponent<PlayerController>().MainCameraSize = getFloatValue;
                    break;
                case PlayerStatType.Gravity:
                    GetComponent<PlayerController>().GravityWeight = getIntValue;
                    break;
                case PlayerStatType.MissileCount:
                    GetComponent<PlayerWeaponController>().MissileCount = getIntValue;
                    break;
                case PlayerStatType.MissileSize:
                    GetComponent<PlayerWeaponController>().MissileSize = getFloatValue;
                    break;
                case PlayerStatType.BoostAttack:
                    GetComponent<PlayerController>().BoostAttack = getFloatValue;
                    break;
                case PlayerStatType.MeteorCount:
                    // TODO : New meteor script needed
                    break;
                case PlayerStatType.MagnetSize:
                    // TODO : New Magnet script needed
                    break;
                case PlayerStatType.BulletBoundCount:
                    GetComponent<PlayerWeaponController>().MissileBoundCount = getIntValue;
                    break;
                case PlayerStatType.MaxSpeed:
                    GetComponent<PlayerController>().MaxMoveSpeed = getFloatValue;
                    break;
                default:
                    Debug.LogError("PlayerStatType doesn't match! : " + statType.ToString());
                    break;

            }

    }




    }
}