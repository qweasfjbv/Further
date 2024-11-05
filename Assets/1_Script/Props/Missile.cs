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
            // TODO : adjust rotation
            transform.rotation = Quaternion.Euler(root.rotation * (velocity - root.parent.GetComponent<Rigidbody>().velocity)); 
            gameObject.SetActive(true);

            GetComponent<Rigidbody>().velocity = velocity;
            Invoke(nameof(OnExpired), duration);
        }

        private void OnExpired()
        {
            transform.gameObject.SetActive(false);
            transform.parent = originalRoot;
        }


        // TODO : Collision Detection <- Attach Planet Stat on planet object
        private void OnEnable()
        {

        }


        private void OnDisable()
        {

        }


    }
}