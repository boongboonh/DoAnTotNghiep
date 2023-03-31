using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parallaxBackground : MonoBehaviour
{
    public float[] parallaxSpeeds;  // Speeds of each parallax layer, from left to right
    public float smoothing = 1f;   // Smoothing factor for parallax movement

    private GameObject lastCameraPosition;  // Position of the camera in the previous frame

    void Start()
    {
        lastCameraPosition = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void FixedUpdate()
    {
        Vector3 deltaMovement = transform.position - lastCameraPosition.transform.position;  // Delta movement of the camera since the last frame
        for (int i = 0; i < parallaxSpeeds.Length; i++)
        {
            float parallaxSpeed = parallaxSpeeds[i];
            if (parallaxSpeed != 0)
            {
                Transform layer = transform.GetChild(i);  // Get the layer object by index
                float layerMovement = -deltaMovement.x * parallaxSpeed * smoothing;  // Calculate the movement of the layer
                layer.Translate(layerMovement, 0, 0);  // Move the layer
            }
        }
        lastCameraPosition.transform.position = transform.position;  // Update the camera position for the next frame
    }

}
