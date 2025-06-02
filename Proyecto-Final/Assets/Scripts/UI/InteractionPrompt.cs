using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private float showDistance = 2.5f;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject promptCanvas;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    private void Update()
    {
        if (player == null || promptCanvas == null)
            return;

        float dist = Vector2.Distance(player.position, transform.position);

        if (dist <= showDistance)
        {
            promptCanvas.SetActive(true);
        }
        else
        {
            promptCanvas.SetActive(false);
        }
    }
}
