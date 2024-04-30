using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace IndiePixel
{
    [RequireComponent(typeof(IP_Drone_Inputs))]
    public class IP_Drone_Controller : IP_Base_Rigidbody
    {
        #region Variables
        [Header("Control Properties")]
        [SerializeField]protected float minMaxPitch = 30f;
        [SerializeField] protected float minMaxRoll = 30f;
        [SerializeField] protected float yawPower = 6f;
        [SerializeField] protected float lerpspeed = 2f;
       

        protected IP_Drone_Inputs input;
        private List<IEngine> engines = new List<IEngine>();

        protected float finalPitch;
        protected float finalRoll;
        protected float yaw;
        protected float finalYaw;


        #endregion

        #region Main Methods
        // Start is called before the first frame update
        void Start()
        {
            input = GetComponent<IP_Drone_Inputs>();
            engines = GetComponentsInChildren<IEngine>().ToList<IEngine>();
        }

        #endregion

        #region Custom Methods
        protected override void HandlePhysics()
        {
            HandleEngines();
            HandleControls();
        }

        protected virtual void HandleEngines()
        {
            //rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude));
            foreach(IEngine engine in engines)
            {
                engine.UpdateEngine(rb, input);
            }
        }

        protected virtual void HandleControls()
        {
            float pitch = input.Cyclic.y * minMaxPitch;
            float roll = -input.Cyclic.x * minMaxRoll;
            yaw += input.Pedals * yawPower;

            finalPitch = Mathf.Lerp(finalPitch, pitch, Time.deltaTime * lerpspeed);
            finalRoll = Mathf.Lerp(finalRoll, roll, Time.deltaTime * lerpspeed);
            finalYaw = Mathf.Lerp(finalYaw, yaw, Time.deltaTime * lerpspeed);

            Quaternion rot = Quaternion.Euler(finalPitch, finalYaw, finalRoll);
            rb.MoveRotation(rot);

            // Obstacle avoidance using multiple raycasts
            float avoidanceForce = 10f;
            float avoidanceAngle = 30f; // angle between raycasts
            int numRaycasts = 6; // number of raycasts
            for (int i = 0; i < numRaycasts; i++)
            {
                float angle = i * (360f / numRaycasts) + avoidanceAngle;
                Vector3 rayDir = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
                if (Physics.Raycast(rb.position, rayDir, out RaycastHit hit, 15f))
                {
                    // If we hit an obstacle, adjust the drone's movement
                    if (hit.collider.gameObject != gameObject)
                    {
                        rb.AddForce(-rayDir * avoidanceForce);
                        Debug.Log("Ray detected object: " + hit.collider.gameObject.name);
                    }
                }

                // Visualize the ray
                Debug.DrawRay(transform.position, rayDir * 15f, Color.red);
            }
        }
        #endregion
    }
}
