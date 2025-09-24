using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HouseDoor : MonoBehaviour
{
    [SerializeField] private float cooldown = 0.5f;

    private WorldTransitionAnimator worldTransition;
    private float lastTriggerTime;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        worldTransition = FindObjectOfType<WorldTransitionAnimator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (worldTransition == null) return;
        if (Time.time - lastTriggerTime < cooldown) return;

        // Toggle simple: si está afuera, entra. Si está adentro, sale.
        if (worldTransition.IsInInterior)
        {
            worldTransition.ExitHouse();
        }
        else
        {
            worldTransition.EnterHouse();
        }

        lastTriggerTime = Time.time;
    }
}