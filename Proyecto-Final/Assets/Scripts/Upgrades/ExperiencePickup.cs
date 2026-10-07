using UnityEngine;

// Created by LifeController.Drop alongside materials and mana. Death never grants XP directly.
public sealed class ExperiencePickup : MonoBehaviour
{
    private int amount;
    private PlayerExperienceSystem collector;
    private bool collected;

    public static GameObject Spawn(Vector3 position, int amount, Sprite existingIcon)
    {
        var item = new GameObject("Experience pickup");
        item.transform.position = position;
        item.transform.localScale = Vector3.one * 0.3f;
        var renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = existingIcon;
        renderer.color = new Color(0.55f, 1f, 0.7f);
        renderer.sortingOrder = 20;
        var body = item.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.drag = 5;
        var trigger = item.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 2f;
        item.AddComponent<ExperiencePickup>().amount = amount;
        return item;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || collector != null) return;
        collector = other.GetComponentInParent<PlayerExperienceSystem>();
    }
    private void Update()
    {
        if (collector == null || collected || UpgradeRuntime.GameplayBlocked) return;
        transform.position = Vector3.MoveTowards(transform.position, collector.transform.position, 8 * Time.deltaTime);
        if (Vector3.Distance(transform.position, collector.transform.position) > 0.15f) return;
        collected = true;
        collector.AddExperience(amount);
        SoundManager.Instance?.PlayOneShot("PickUp");
        Destroy(gameObject);
    }
}
