using UnityEngine;

namespace Further.Props
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Missile : MonoBehaviour
    {
        private Transform originalRoot;

        public void OnFire(Transform root, float duration, Vector3 velocity)
        {
            originalRoot = root;
            transform.parent = null;
            transform.position = root.position;
            transform.rotation = root.rotation; 
            gameObject.SetActive(true);

            GetComponent<Rigidbody>().velocity = velocity;
            Invoke(nameof(OnExpired), duration);
        }

        private void OnExpired()
        {
            transform.gameObject.SetActive(false);
            transform.parent = originalRoot;
        }

        private void OnEnable()
        {

        }


        private void OnDisable()
        {

        }


    }
}