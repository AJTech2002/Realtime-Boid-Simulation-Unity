using System;
using System.Collections;
using UnityEngine;

public class CameraOrbitController : MonoBehaviour
{
    public Transform target;
    public float orbitSensitivity = 0.1f;
    public float lerpRate = 5f;
    public float maxVerticalAngle = 80f;
    public float minVerticalAngle = -80f;

    [Header("Zoom")] public float zoomSpeed = 5f; // Adjust this value to change zoom speed
    public float minZoom = 1f; // Minimum zoom level
    public float maxZoom = 10f; // Maximum zoom level
    public float smoothTime = 0.2f; // Smoothing time for zooming
    public float decayRate = 0.1f; // Decay rate for momentum

    private float targetZoom; // Target zoom level
    private float zoomVelocity; // Velocity for smooth damping

    private Vector3 previousPosition;
    private float verticalAngle = 0f; // To track the vertical angle independently

    private float lastVelocity;
    private Vector3 offset;

    private void Awake()
    {
        verticalAngle = transform.eulerAngles.x;
        presumedTargetPosition = target.position;
        offset = transform.position - target.position;
    }

    public Quaternion activeVerticalRotation;
    public Vector3 presumedTargetPosition;
    private Vector3 startPos;
    private bool enabled = true;
    private bool isRotating = false;

    void Update()
    {
        if (target != null)
        {
            if (Input.GetMouseButtonDown(2) ||
                (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))) // Middle mouse button pressed
            {
                previousPosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2) ||
                (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))) // Middle mouse button held down
            {
                if (enabled)
                {
                    isRotating = true;
                    Vector3 direction = previousPosition - Input.mousePosition;
                    previousPosition = Input.mousePosition;

                    // Horizontal rotation
                    transform.RotateAround(target.position, Vector3.up, direction.x * orbitSensitivity);

                    // Vertical rotation with constraints
                    verticalAngle -= direction.y * orbitSensitivity;
                    verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

                    // Apply rotation while maintaining the current horizontal rotation

                    lastVelocity = direction.x;
                }
            }
            else
            {
                if (Mathf.Abs(lastVelocity) > 0.05f)
                {
                    transform.RotateAround(target.position, Vector3.up, lastVelocity * orbitSensitivity);
                    lastVelocity = Mathf.Lerp(lastVelocity, 0f, 1 * Time.deltaTime);
                }
                else
                {
                    isRotating = false;
                }

            }


            if (Input.GetMouseButtonDown(1)) // Right mouse button pressed
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    presumedTargetPosition = hit.point;
                }
            }

            target.position = Vector3.Lerp(target.position, presumedTargetPosition, Time.deltaTime * lerpRate);


            Quaternion verticalRotation = Quaternion.Euler(verticalAngle, transform.eulerAngles.y, 0);
            transform.localPosition = target.position - (verticalRotation * Vector3.forward * (offset).magnitude);

            if (enabled)
            {
                transform.LookAt(target); // Ensure the camera is always looking at the target
            }

            // Get scroll wheel input
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            // Calculate new target zoom level based on scroll input
            targetZoom = Mathf.Clamp(Camera.main.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);

            // Smoothly interpolate towards the target zoom level
            Camera.main.orthographicSize = targetZoom;

            // Decay zoom velocity over time
            zoomVelocity = Mathf.Lerp(zoomVelocity, 0f, Time.deltaTime * decayRate);
        }
    }
}