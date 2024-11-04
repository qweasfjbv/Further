using UnityEngine;
using Further.UI.Popup;

namespace Further.UI
{
    public class PopupCanvas : MonoBehaviour
    {
        [SerializeField] private UpgradePopup upgradePopup;

        [ContextMenu("PopupUpgradeUI")]
        public void PopupUpgradeUI() {

            // TODO : SOUND - POPUP UI
            upgradePopup.SetUpgradeUI(OnComplete);
            upgradePopup.gameObject.SetActive(true);

        }

        private void OnComplete()
        {
            // TODO : SOUND - SELECT
            upgradePopup.gameObject.SetActive(false);
        }


    }
}