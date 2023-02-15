using Characters;
using UnityEngine;

public class HandContact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player") || other.transform.CompareTag("Enemy"))
        {
            if (other.transform.gameObject.TryGetComponent(out ICharacter character))
            {
                character.Hit(other.name);
            }
                
            else Debug.LogWarning("The hit target does not inherit from the ICharacter interface!");
        }
    }
}
