using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    public bool isOccupied = false; // Indica si ya hay una planta aquí
    private GameObject currentPlant; // La planta actual en esta parcela

    public void Plant(GameObject plantPrefab)
    {
        if (isOccupied) return; // No plantar si ya está ocupado

        currentPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        isOccupied = true;
    }

}

