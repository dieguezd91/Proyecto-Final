using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingMaterial : MonoBehaviour
{
    public MaterialType _materialType;
    public CraftingMaterialSO _materialData;

    public CraftingMaterial(MaterialType materialType, CraftingMaterialSO materialData)
    {
        materialType = _materialType;
        _materialData = materialData;
    }
}
