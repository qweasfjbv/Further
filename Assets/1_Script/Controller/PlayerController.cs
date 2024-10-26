using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Further
{
    [RequireComponent (typeof (Rigidbody))]
    public class PlayerController : MonoBehaviour
    {

        [Header("Velocity")]
        [SerializeField] private float startMoveSpeed;
        [SerializeField] private float maxMoveSpeed;
        [SerializeField] private float rotationSpeed;

        [Header("Forces")]
        [SerializeField] private float boostForce;
        [SerializeField] private ParticleSystem boostParticle;

        /** Components **/
        private Rigidbody rigid;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }


        /* */
        private float vert, horz;
        private bool isParticle = false;

        private void Start()
        {
        }


        private void Update()
        {
            horz = Input.GetAxisRaw("Horizontal");
            vert = Input.GetAxisRaw("Vertical");



            if (horz == 0 && vert == 0)
            {

                /** Particle **/
                if (boostParticle.isPlaying)
                {
                    isParticle = false;
                    boostParticle.Stop();
                }

                /** Forces **/
                AddForces(-1, new Vector3(0, 0, 0));
            }
            else
            {
                /** Rotate **/
                float angle = -Mathf.Atan2(horz, vert) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                /** Particle */
                if (!isParticle)
                {
                    isParticle = true;
                    boostParticle.Play();
                }

                /** Forces **/
                AddForces(-1, new Vector3(horz, vert, 0) * boostForce);
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