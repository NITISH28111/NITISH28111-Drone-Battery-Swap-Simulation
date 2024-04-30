using IndiePixel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class ObstacleAvoidance : IP_Drone_Controller
{
    // Ray for front sensor
    private Ray ray;

    // Vector to store initial position
    private Vector3 posInicial;

    // Movement speed
    private float speed = 10f;

    void Update()
    {
        // Update ray based on transform position and forward direction
        ray = new Ray(transform.position + Vector3.up, transform.forward);
        posInicial = transform.position;

        // Check for obstacle in front (up to 55 units away)
        if (Physics.Raycast(ray, out RaycastHit hit, 55f))
        {
            // Check if obstacle is the "Pick Up" object
            if (hit.collider.tag == "Pick Up")
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);
                // Move towards pick-up object
                transform.position = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
            }
            else
            {
                // Rotate if not pick-up object
                transform.Rotate(0, -80 * Time.deltaTime, 0);
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
        else
        {
            // Go forward if no obstacle detected
            transform.position += transform.forward * speed * Time.deltaTime;
            Debug.DrawLine(ray.origin, ray.origin + transform.forward * 55f, Color.white); // Optional: visualize forward ray
        }

        // Check for obstacles on the right (up to 20 units away)
        RaycastHit hit2;
        if (Physics.Raycast(posInicial, Quaternion.AngleAxis(45f, transform.up) * transform.forward, out hit2, 20f))
        {
            transform.Rotate(0, -80 * Time.deltaTime, 0); // Rotate left if obstacle detected right
            Debug.DrawLine(posInicial, hit2.point, Color.yellow);
        }

        // Check for obstacles on the left (up to 20 units away)
        RaycastHit hit3;
        if (Physics.Raycast(posInicial, Quaternion.AngleAxis(-45f, transform.up) * transform.forward, out hit3, 20f))
        {
            transform.Rotate(0, 80 * Time.deltaTime, 0); // Rotate right if obstacle detected left
            Debug.DrawLine(posInicial, hit3.point, Color.cyan);
        }
    }
}
