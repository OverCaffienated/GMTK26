using UnityEngine;

public class Parallax : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 previousCameraPosition;

    [Tooltip("0 = Normal (Default), 0.5 = Background (Slower), -0.5 = Foreground (Faster), 1 = Sky (Moves with camera)")]
    public float parallaxMultiplier;

    void Start()
    {
        mainCamera = Camera.main;
        previousCameraPosition = mainCamera.transform.position;
    }

    void LateUpdate()
    {
        // Calculate how much the camera moved since the last frame
        Vector3 deltaMovement = mainCamera.transform.position - previousCameraPosition;

        // Move this object based on that camera movement and the multiplier
        transform.position += new Vector3(deltaMovement.x * parallaxMultiplier, deltaMovement.y * parallaxMultiplier, 0);

        // Save the new camera position for the next frame
        previousCameraPosition = mainCamera.transform.position;
    }
}