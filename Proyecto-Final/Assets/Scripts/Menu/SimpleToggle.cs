using UnityEngine;

public class SimpleToggle : MonoBehaviour
{
    [SerializeField] private GameObject Options;

    public void EnableFeature()
    {
        Options.SetActive(true);
    }

    public void DisableFeature()
    {
        Options.SetActive(false);
    }
}
