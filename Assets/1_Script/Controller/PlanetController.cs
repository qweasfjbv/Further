using UnityEditor;
using UnityEngine;

namespace Further
{
    public class PlanetController : MonoBehaviour
    {
        [SerializeField] private int planetId = 5;
        [SerializeField] private float radius = 20f;
        [SerializeField] private float mass = 100f;
        [SerializeField, Range(0, 360)] private float rotateAxis;
        [SerializeField] private float rotateSpeed;
        [SerializeField] private float bias;

        private Transform player = null;

       
        private Vector3 direction;
        private float distance;


        private void Start()
        {
            SetPlanetInfo(rotateAxis);
        }

        public void SetPlanetInfo(float rotateAxis)
        {
            transform.rotation = Quaternion.Euler(0, rotateAxis, 0);
        }

        private void Update()
        {
            ForceGravity();
            RotateSelf();
        }

        // To accelerate, reduce the force by half when moving away from the planet.
        private void ForceGravity()
        {

            Collider[] colls = Physics.OverlapSphere(transform.position, radius, Constants.LAYER_PLAYER);

            if (colls.Length > 0)
            {
                player = colls[0].transform;
                direction = transform.position - player.position;

                float weight = (Vector3.Dot(direction, player.GetComponent<Rigidbody>().velocity) > 0 ? 1.0f : 0.5f);
                player.GetComponent<PlayerController>().AddForces(planetId, direction.normalized * mass * weight * bias);

            }
            else
            {
                if (player != null)
                {
                    player.GetComponent<PlayerController>().AddForces(planetId, Vector3.zero);
                }
            }
        }

        private void RotateSelf()
        {
            transform.Rotate(new Vector3(0, 0, rotateSpeed *  Time.deltaTime)); 
        }

    }

}