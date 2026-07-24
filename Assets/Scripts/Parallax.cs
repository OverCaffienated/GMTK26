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
        Vector3 deltaMovement = mainCamera.transform.position - previousCameraPosition;

        transform.position += new Vector3(deltaMovement.x * parallaxMultiplier, deltaMovement.y * parallaxMultiplier, 0);

        previousCameraPosition = mainCamera.transform.position;
    }
}