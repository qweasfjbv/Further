using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Further
{
    [RequireComponent (typeof (Rigidbody))]
    public class PlayerController : MonoBehaviour
    {

        [SerializeField] private float baseGravity;

        private Rigidbody rigid;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        private void Start()
        {

        }

        float vert, horz;
        private void Update()
        {
            horz = Input.GetAxisRaw("Horizontal");
            vert = Input.GetAxisRaw("Vertical");

            if (horz == 0 && vert == 0)
            {
                AddForces(-1, new Vector3(0, 0, 0));
            }
            else
            {
                AddForces(-1, new Vector3(horz, vert, 0) * baseGravity);
            }


        }

        private void FixedUpdate()
        {
            rigid.velocity += CalcForce();
        }

        // Store all the affected gravities
        private Dictionary<int, Vector3> forces =new Dictionary<int, Vector3>();
        

        public void AddForces(int id, Vector3 force)
        {
            if (force.sqrMagnitude <= 0.1f) forces.Remove(id);
            else
            {
                if (forces.ContainsKey(id))
                {
                    forces[id] = force;
                }
                else
                {
                    forces.Add(id, force);
                }
            }
        }


        private Vector3 CalcForce()
        {
            Vector3 sumV = Vector3.zero;
            foreach (var pair in forces)
            {
                sumV += pair.Value;
            }
            sumV.z = 0;
            return sumV * Time.deltaTime;
        }
    }
}