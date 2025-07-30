using UnityEngine;

public class SimpleToggle : MonoBehaviour
{
    [SerializeField] private GameObject Options;

    public void EnableFeature()
    {
        SoundManager.Instance.PlayOneShot("ButtonClick");
        Options.SetActive(true);
    }

    public void DisableFeature()
    {
        SoundManager.Instance.PlayOneShot("ButtonClick");
        Options.SetActive(false);
    }
}
