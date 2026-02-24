using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeController))]
public class DeathPenaltyExecutor : MonoBehaviour
{
    private LifeController _lifeController;

    private void Awake()     => _lifeController = GetComponent<LifeController>();
    private void OnEnable()  => _lifeController.onDeath.AddListener(OnPlayerDied);
    private void OnDisable() => _lifeController.onDeath.RemoveListener(OnPlayerDied);

    private void OnPlayerDied()
    {
        if (InventoryManager.Instance == null) return;

        List<MaterialItem> available = InventoryManager.Instance.GetAllMaterials();
        if (available.Count == 0) return;

        var roulette = new WeightedRoulette<MaterialItem>();
        foreach (MaterialItem item in available)
            roulette.Add(item, 1f);

        int typesToPick = Random.Range(1, available.Count + 1);

        for (int i = 0; i < typesToPick; i++)
        {
            MaterialItem selected = roulette.Roll();
            roulette.SetWeight(selected, 0f);

            int amount = Random.Range(1, selected.amount + 1);
            InventoryManager.Instance.UseMaterial(selected.type, amount);
        }
    }
}
