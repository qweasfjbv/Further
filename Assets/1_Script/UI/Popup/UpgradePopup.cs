using Further.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Further.UI.Popup
{
    public class UpgradePopup : MonoBehaviour
    {
        [SerializeField] private List<UpgradePopupItem> popupItems = new List<UpgradePopupItem>(3);

        public void SetUpgradeUI(Action complete)
        {
            int[] stats = new int[3] { 0, 0, 0 };

            // HACK : Randomly select stats, more logic needed 
            int count = System.Enum.GetValues(typeof(PlayerStatType)).Length;
            while ((stats[0] == stats[1]) || (stats[0] == stats[2]) || (stats[1]== stats[2]))
            {
                stats[0] = UnityEngine.Random.Range(0, count);
                stats[1] = UnityEngine.Random.Range(0, count);
                stats[2] = UnityEngine.Random.Range(0, count);
            }
            
            for(int i=0; i<popupItems.Count; i++)
            {
                popupItems[i].SetUpgradeItem((PlayerStatType)stats[i], Manager.Upgrade.GetStatLevel((PlayerStatType)stats[i]) + 1, complete);
                popupItems[i].gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            foreach(var item in popupItems)
            {
                item.gameObject.SetActive(false);
            }
        }

    }
}