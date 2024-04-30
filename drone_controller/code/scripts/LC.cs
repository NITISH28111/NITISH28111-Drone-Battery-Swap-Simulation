using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LC : MonoBehaviour
{
    public Transform droneA; // Reference to Drone A
    public List<Transform> waypoints;
    public List<Transform> chargingStations;
    public float speed = 10f;
    public float battery = 100f;
    public float chargeRate = 10f;
    public float hoverPower = 100f;
    public float propulsionPower = 200f;
    public float payloadPower = 0f;
    public float initialBatteryDrainRate = 0.15f; // Starting battery drain rate
    public float activeBatteryDrainRate = 0.15f; // Battery drain rate while flying
    public float batteryDrainRate = 0.15f; // Initial battery drain rate

    public float heightFromGround; // New variable to store the height from the ground
    public int obstnum = 0;
    public float totalDistanceCovered;

    public Rigidbody rb;

    private int currentWaypointIndex = 0;
    private int currentChargingStationIndex = 0;
    private bool isCharging = false;
    private Vector3 lastPosition;
    private bool waitingForTakeoff = true;
    public bool leave = false;
    public bool var = false;
    public bool isHovering = false;

    private void Start()
    {
        Random.InitState(0);
        battery = 100f;
        rb = GetComponent<Rigidbody>();
        SetPowerConsumption(false);
    }
 

    private void FixedUpdate()
    {
        float time = Time.deltaTime;
        lastPosition = transform.position;

        Debug.Log($"Drone B: Time: {Time.time}, Battery: {battery}");

        // Check battery of Drone A and set waiting flag
        waitingForTakeoff = (droneA.GetComponent<LUC>().battery > 70f) && (leave==false);

        // Update power consumption based on waiting state
        SetPowerConsumption(!waitingForTakeoff);

        // Only move and consume power if not waiting for takeoff
        if (!waitingForTakeoff && battery>50f)
        {
            if (battery < 100f && isCharging)
            {
                ChargeBattery();
            }
            else
            {
                if (isHovering == false)
                {
                    MoveToWaypoint();
                }
            }
        }

        if(battery <= 50f && var==false)
        {
            droneA.GetComponent<LUC>().secondloop = true;
            isHovering = true;
            propulsionPower = 0f;
            rb.velocity = Vector3.zero;
            rb.AddForce(Physics.gravity * -rb.mass);

            if (droneA.GetComponent<LUC>().leave2)
            {
                isHovering = false;
                propulsionPower = 250f;
                enabled = true;
                speed = 10f;
                MoveToChargingStation();
            }
        }

         else if (battery < 100f && isCharging)
        {
            ChargeBattery();
        }

        CheckBatteryLevel();

        float powerConsumed = (hoverPower + propulsionPower + payloadPower) * time;
        if (!isCharging)
        {
            battery -= powerConsumed * batteryDrainRate * time;
        }

        if (droneA.GetComponent<LUC>().enabled==false)
        {
            enabled = false;
        }

        AvoidObstacle();

        heightFromGround = CalculateHeightFromGround();
        float distanceMoved = Vector3.Distance(lastPosition, transform.position);

        if (distanceMoved > 0)
        {
            totalDistanceCovered += distanceMoved;
        }
    }

    private float CalculateHeightFromGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            return hit.distance;
        }
        return 0;
    }

    private IEnumerator Wait()
    {
        Debug.Log("started");
        var = true;
        yield return new WaitForSeconds(5);
        leave = true;
        isHovering = false;
        propulsionPower = 250f;
        speed = 7f;
        var = false;
        currentWaypointIndex = 1;
        MoveToWaypoint();
        Debug.Log("ended");
    }
    private void MoveToWaypoint()
    {
        if (var == false)
        {
            float time = Time.deltaTime;
            if (currentWaypointIndex >= waypoints.Count)
            {
                Debug.Log("Drone B has reached all waypoints.");
                enabled = false;
                return;
            }

            float distance = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
            if (distance <= 0.1f)
            {
                if (currentWaypointIndex == 0)
                {

                    isHovering = true;
                    propulsionPower = 0f;
                    rb.velocity = Vector3.zero;
                    rb.AddForce(Physics.gravity * -rb.mass);
                    StartCoroutine(Wait());
                }

                else
                {
                    currentWaypointIndex++;
                }

            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, speed * time);
            }
        }
        AvoidObstacle();
    }

    private void MoveToChargingStation()
    {
        float time = Time.deltaTime;

        if (currentChargingStationIndex >= chargingStations.Count )
        {
            
                enabled = false;
                return;
            
        }

        float distanceToChargingStation = Vector3.Distance(transform.position, chargingStations[currentChargingStationIndex].position);
        if (distanceToChargingStation <= 0.1f)
        {
            currentChargingStationIndex++;

            isCharging = true;
            rb.isKinematic = true; // Disable physics simulation (landing)
            hoverPower = 0f; // Stop hover power consumption
            propulsionPower = 0f; // Stop propulsion power consumption
            ChargeBattery();
        
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, chargingStations[currentChargingStationIndex].position, speed * time);
        }
        AvoidObstacle();
    }

    private void ChargeBattery()
    {
        if (isCharging && battery < 100f)
        {
            batteryDrainRate = 0f;
            battery = Mathf.Min(battery + chargeRate * Time.deltaTime, 100f);
            if (battery >= 100f)
            {
                isCharging = false;
                rb.isKinematic = false;
                hoverPower = 50f;
                propulsionPower = 250f;
                batteryDrainRate = initialBatteryDrainRate; // Reset to initial drain rate when charged
            }
        }
    }

    private void CheckBatteryLevel()
    {
        if (battery < 0f)
        {
            battery = 0f;
            Debug.Log("Drone B: Battery depleted. Landing.");
            enabled = false;


        }
    }

    private void SetPowerConsumption(bool isActive)
    {
        hoverPower = isActive ? 50f : 0f;
        if (isHovering == false)
        {
            propulsionPower = isActive ? 250f : 0f;
        }
        else if(isHovering == true)
        {
            propulsionPower = isActive ? 0f : 0f;
        }
        batteryDrainRate = isActive ? 0.15f : 0f;
    }

    private void AvoidObstacle()
    {
        float avoidanceForce = 10f;
        float avoidanceAngle = 30f;
        int numRaycasts = 6;

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
                    obstnum++;
                }
            }

            // Visualize the ray
            Debug.DrawRay(transform.position, rayDir * 15f, Color.green);
        }
    }
}