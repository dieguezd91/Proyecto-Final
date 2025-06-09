using UnityEngine;

public class ManaPickup : MonoBehaviour
{
    [SerializeField] private float manaAmountMin = 5f;
    [SerializeField] private float manaAmountMax = 15f;
    [SerializeField] private float attractionRadius = 1.5f;
    [SerializeField] private float attractionSpeed = 5f;
    [SerializeField] private Sprite manaSprite;

    private Transform player;
    private float manaAmount;

    private void Start()
    {
        manaAmount = Random.Range(manaAmountMin, manaAmountMax);
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < attractionRadius)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, attractionSpeed * Time.deltaTime);

            if (dist < 0.3f)
            {
                ManaSystem manaSystem = player.GetComponent<ManaSystem>();
                if (manaSystem != null)
                {
                    manaSystem.AddMana(manaAmount);
                    SoundManager.Instance.PlayOneShot("PickUp");

                }

                FloatingTextController pickupHandler = player.GetComponentInChildren<FloatingTextController>();
                if (pickupHandler != null)
                {
                    pickupHandler.ShowPickup("Mana", Mathf.RoundToInt(manaAmount), manaSprite);
                }

                Destroy(gameObject);
            }
        }
    }
}