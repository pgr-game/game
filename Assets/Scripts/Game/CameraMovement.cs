using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float zoomSpeed, minZoom, maxZoom;
    Vector3 touchStart;
    public float zoomOutMin = 1;
    public float zoomOutMax = 8;
    private Vector3 dragOrigin;

    void Start()
    {
        
    }

    void Update()
    {
        if (!PauseMenu.isPaused)
        {
            if (!Application.isMobilePlatform)
            {
                panCamera();
                zoomCamera();
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                    float difference = currentMagnitude - prevMagnitude;
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - (difference * 0.01f), minZoom, maxZoom);
                }
                else if (Input.GetMouseButton(0))
                {
                    Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Camera.main.transform.position += direction;
                }
            }
        }
    }

    private void panCamera() {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
                cam.transform.position += difference;
            }
        }
        //cam.transform.position = Mathf.Clamp(cam.transform.position, minZoom, maxZoom);
    }

     void ZoomOrthoCamera(Vector3 zoomTowards, float amount)
    {
        // Calculate how much we will have to move towards the zoomTowards position
        float multiplier = (1.0f / cam.orthographicSize * amount);

        // Move camera
        cam.transform.position += (zoomTowards - transform.position) * multiplier; 

        // Zoom camera
        cam.orthographicSize -= amount;

        // Limit zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    private void zoomCamera() {
        // Scroll forward
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && cam.orthographicSize > minZoom)
            {
                ZoomOrthoCamera(cam.ScreenToWorldPoint(Input.mousePosition), 1 * zoomSpeed);
            }

            // Scoll back
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && cam.orthographicSize < maxZoom)
            {
                ZoomOrthoCamera(cam.ScreenToWorldPoint(Input.mousePosition), -1 * zoomSpeed);
            }
        }
    }
}
