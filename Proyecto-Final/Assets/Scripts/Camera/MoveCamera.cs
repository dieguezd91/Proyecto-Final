using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform mainChar;
    [SerializeField] float vel;

    private void Update()
    {
        Vector3 aux = Vector3.Lerp(transform.position, mainChar.position, vel * Time.deltaTime);
        aux.z = transform.position.z;
        transform.position = aux;
    }
}