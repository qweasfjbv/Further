using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Further
{
    [RequireComponent (typeof (Rigidbody))]
    public class PlayerController : MonoBehaviour
    {

        [Header("Cameras")]
        [SerializeField] private BGController bgCamera;
        [SerializeField] private Transform mainCamera;

        [Header("Velocity")]
        [SerializeField] private float startMoveSpeed;
        [SerializeField] private float maxMoveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float idleRotationSpeed;

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
                /** Rotate **/
                transform.Rotate(new Vector3(0, 0, idleRotationSpeed * Time.deltaTime));

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

            UpdateCameras();
        }

        private void FixedUpdate()
        {
            rigid.velocity += CalcForce();
            if (rigid.velocity.sqrMagnitude > maxMoveSpeed * maxMoveSpeed)
            {
                rigid.velocity = rigid.velocity * (maxMoveSpeed / rigid.velocity.magnitude);
            }
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

        private float verticalCameraOffset;
        private void UpdateCameras()
        {
            verticalCameraOffset = rigid.velocity.y / maxMoveSpeed;
            mainCamera.transform.position = transform.position + new Vector3(0, verticalCameraOffset * 15, -20);
            bgCamera.SetSpeed(-rigid.velocity.y, maxMoveSpeed);
        }
    }
}