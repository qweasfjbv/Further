using Further.Managers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Further.UI.Popup
{
    public class UpgradePopupItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemText;
        [SerializeField] private Image itemImage;

        public void SetUpgradeItem(PlayerStatType type, int level, Action complete)
        {
            if (itemImage != null)
            {
                // itemImage.sprite = Manager.Data.GetSprite(type);
            }

            if (itemText != null)
            {
                itemText.text = $"{type.ToString()} : {level}";
            }

            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() => Manager.Upgrade.UpgradeStat(type));
            GetComponent<Button>().onClick.AddListener(() => complete());
        }
    }
}