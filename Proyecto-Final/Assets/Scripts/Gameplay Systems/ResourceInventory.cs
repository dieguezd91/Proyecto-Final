using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceItem
{
    public string name;
    public int amount;
    public Sprite icon;
}

public class ResourceInventory : MonoBehaviour
{
    private static ResourceInventory _instance;
    public static ResourceInventory Instance { get { return _instance; } }

    [Header("RESOURCES INVENTORY")]
    [SerializeField] private List<ResourceItem> resources = new List<ResourceItem>();

    public Action<string, int> onResourceChanged;

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

    public void AddResource(string resourceName, int amount)
    {
        if (string.IsNullOrEmpty(resourceName) || amount <= 0)
            return;

        ResourceItem existingResource = resources.Find(r => r.name == resourceName);

        if (existingResource != null)
        {
            existingResource.amount += amount;
        }
        else
        {
            ResourceItem newResource = new ResourceItem
            {
                name = resourceName,
                amount = amount,
                icon = null
            };

            resources.Add(newResource);
        }

        onResourceChanged?.Invoke(resourceName, GetResourceAmount(resourceName));
    }

    public bool UseResource(string resourceName, int amount)
    {
        if (string.IsNullOrEmpty(resourceName) || amount <= 0)
            return false;

        ResourceItem resource = resources.Find(r => r.name == resourceName);

        if (resource != null && resource.amount >= amount)
        {
            resource.amount -= amount;

            if (resource.amount <= 0)
            {
                resources.Remove(resource);
            }

            onResourceChanged?.Invoke(resourceName, GetResourceAmount(resourceName));

            return true;
        }

        return false;
    }

    public int GetResourceAmount(string resourceName)
    {
        ResourceItem resource = resources.Find(r => r.name == resourceName);
        return resource != null ? resource.amount : 0;
    }

    public List<ResourceItem> GetAllResources()
    {
        return new List<ResourceItem>(resources);
    }

    public bool HasEnoughResource(string resourceName, int requiredAmount)
    {
        return GetResourceAmount(resourceName) >= requiredAmount;
    }

    public void SetResourceIcon(string resourceName, Sprite icon)
    {
        ResourceItem resource = resources.Find(r => r.name == resourceName);

        if (resource != null)
        {
            resource.icon = icon;
        }
    }
}