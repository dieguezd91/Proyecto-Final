using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MaterialItem
{
    public MaterialType type;
    public int amount;
    public Sprite icon;
}

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance { get { return _instance; } }

    [Header("MATERIALS INVENTORY")]
    [SerializeField] private List<MaterialItem> materials = new();

    [Header("DATABASE")]
    [SerializeField] private MaterialDatabaseSO materialDatabase;

    public Action<MaterialType, int> onMaterialChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMaterial(MaterialType type, int amount)
    {
        if (type == MaterialType.None || amount <= 0)
            return;

        MaterialItem existing = materials.Find(m => m.type == type);
        if (existing != null)
        {
            existing.amount += amount;
        }
        else
        {
            Sprite icon = null;

            if (materialDatabase != null)
            {
                CraftingMaterialSO data = materialDatabase.GetMaterial(type);
                if (data != null)
                    icon = data.materialIcon;
            }

            materials.Add(new MaterialItem
            {
                type = type,
                amount = amount,
                icon = icon
            });
        }

        onMaterialChanged?.Invoke(type, GetMaterialAmount(type));
    }

    public bool UseMaterial(MaterialType type, int amount)
    {
        if (type == MaterialType.None || amount <= 0)
            return false;

        MaterialItem material = materials.Find(m => m.type == type);
        if (material != null && material.amount >= amount)
        {
            material.amount -= amount;
            if (material.amount <= 0)
                materials.Remove(material);

            onMaterialChanged?.Invoke(type, GetMaterialAmount(type));
            return true;
        }
        return false;
    }

    public void ClearAllMaterials()
    {
        materials.Clear();
        onMaterialChanged?.Invoke(MaterialType.None, 0);
    }

    public int GetMaterialAmount(MaterialType type)
    {
        MaterialItem mat = materials.Find(m => m.type == type);
        return mat?.amount ?? 0;
    }

    public List<MaterialItem> GetAllMaterials() => new(materials);

    public bool HasEnoughMaterial(MaterialType type, int requiredAmount)
    {
        return GetMaterialAmount(type) >= requiredAmount;
    }

    public void SetMaterialIcon(MaterialType type, Sprite icon)
    {
        MaterialItem mat = materials.Find(m => m.type == type);
        if (mat != null)
            mat.icon = icon;
    }

    public string GetMaterialName(MaterialType type)
    {
        if (materialDatabase != null)
        {
            CraftingMaterialSO data = materialDatabase.GetMaterial(type);
            if (data != null)
            {
                return data.materialName;
            }
        }
        return type.ToString();
    }
}