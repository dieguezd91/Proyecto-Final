using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Crafting/Material Database")]
public class MaterialDatabaseSO : ScriptableObject
{
    public List<CraftingMaterialSO> allMaterials;

    public CraftingMaterialSO GetMaterial(MaterialType type)
    {
        return allMaterials.Find(m => m.materialType == type);
    }
}