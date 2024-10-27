using TMPro;
using UnityEngine;

namespace Further.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI velocityText;
        [SerializeField] private TextMeshProUGUI velocityUnitText;

        private Rigidbody rigid;


        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            velocityText.text = ((int)rigid.velocity.magnitude).ToString();
        }


    }

}