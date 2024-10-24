using System.IO;
using UnityEngine;

namespace Further
{
    public class PlanetController : MonoBehaviour
    {
        [SerializeField] private int planetId = 5;
        [SerializeField] private float radius = 20f;
        [SerializeField] private float mass = 100f;

        private Transform player = null;


        private Vector3 direction;
        private float distance;
        private void Update()
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, radius, Constants.LAYER_PLAYER);
           
            if (colls.Length > 0)
            {
                player = colls[0].transform;
                direction = transform.position - player.position;
                distance = direction.sqrMagnitude;

                player.GetComponent<PlayerController>().AddForces(planetId, direction.normalized *mass/ distance);
            }
            else
            {
                if (player != null)
                {
                    player.GetComponent<PlayerController>().AddForces(planetId, Vector3.zero);
                }
            }

        }

    }

}