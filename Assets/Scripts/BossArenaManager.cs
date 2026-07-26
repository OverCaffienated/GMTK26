using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [SerializeField] private AdvancedEnemyAI bossEnemy;
    [SerializeField] private GameObject[] arenaWalls;

    private bool arenaActive = false;
    private BoxCollider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        SetWallsActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!arenaActive && collision.CompareTag("Player"))
        {
            arenaActive = true;
            SetWallsActive(true);
            if (triggerCollider != null) triggerCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (arenaActive && bossEnemy == null)
        {
            SetWallsActive(false);
            Destroy(gameObject);
        }
    }

    private void SetWallsActive(bool state)
    {
        foreach (GameObject wall in arenaWalls)
        {
            if (wall != null) wall.SetActive(state);
        }
    }
}