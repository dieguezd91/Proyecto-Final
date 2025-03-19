using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Plant : MonoBehaviour
{
    public float growthTime; // Tiempo total para crecer completamente
    protected float timer = 0f;
    private Vector3 initialScale;
    private Vector3 finalScale = Vector3.one; // Tamaño final (1,1,1)

    protected virtual void Start()
    {
        initialScale = Vector3.one * 0.2f; // Comienza en tamaño chico
        transform.localScale = initialScale;
    }


    protected virtual void Update()
    {
        if (timer < growthTime)
        {
            timer += Time.deltaTime;
            float progress = timer / growthTime;
            transform.localScale = Vector3.Lerp(initialScale, finalScale, progress);
        }
    }
}
