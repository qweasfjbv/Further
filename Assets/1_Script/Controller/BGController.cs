using System.Security.Cryptography;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Experimental.AI;


namespace Further
{
    public class BGController : MonoBehaviour
    {

        [SerializeField] private float baseRotateSpeed;


        private float rotateSpeed;
        private float degree;

        void Start()
        {
            rotateSpeed = baseRotateSpeed;
            degree = 0;
        }

        void Update()
        {
            degree += Time.deltaTime * baseRotateSpeed;
            transform.rotation = Quaternion.Euler(degree, 0, 0);
        }

        public void SetSpeed(float speed, float maxSpeed)
        {
            rotateSpeed = speed * maxSpeed * baseRotateSpeed;
        }
    }


}