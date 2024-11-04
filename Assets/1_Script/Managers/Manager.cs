using System.Resources;
using UnityEngine;


namespace Further.Managers
{
    public class Manager : MonoBehaviour
    {
        #region Singleton
        private static Manager instance = null;
        public static Manager Instance { get => instance; }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }

            _data.Init();

            _upgrade.Init();
        }
        #endregion

        private DataManager _data = new DataManager();
        private UpgradeManager _upgrade = new UpgradeManager();

        public static DataManager Data { get { return Instance._data; } }
        public static UpgradeManager Upgrade { get { return Instance._upgrade; } }

    }
}