using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private float showDistance;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject promptCanvas;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        promptCanvas.SetActive(false);
    }

    private void Update()
    {
        if (player == null || promptCanvas == null)
            return;

        float dist = Vector2.Distance(player.position, transform.position);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            promptCanvas.SetActive(true);

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        promptCanvas.SetActive(false);
    }
}
