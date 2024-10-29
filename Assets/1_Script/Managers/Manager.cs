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
            if (instance == null)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }

            _data.Init();
        }
        #endregion

        DataManager _data = new DataManager();
        public static DataManager Data { get { return Instance._data; } }

    }
}