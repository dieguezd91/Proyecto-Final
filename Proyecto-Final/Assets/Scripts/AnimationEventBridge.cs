using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    private LifeController lifeController;

    void Awake()
    {
        lifeController = GetComponentInParent<LifeController>();
    }

    public void OnDeathAnimationEnd()
    {
        Debug.Log("Animation event fired in child!");

        if (lifeController != null)
            lifeController.OnDeathAnimationEnd();
    }
}
