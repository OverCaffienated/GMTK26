using UnityEngine;

public class PerfectParallax : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Vector2 parallaxMultiplier;

    private Vector3 startPosition;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        startPosition = transform.position;
    }

    private void LateUpdate()
    {
        float distanceX = (cam.transform.position.x - startPosition.x) * parallaxMultiplier.x;
        float distanceY = (cam.transform.position.y - startPosition.y) * parallaxMultiplier.y;

        transform.position = new Vector3(startPosition.x + distanceX, startPosition.y + distanceY, transform.position.z);
    }
}