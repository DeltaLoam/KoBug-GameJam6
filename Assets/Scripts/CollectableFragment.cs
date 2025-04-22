using UnityEngine;

public class CollectableFragment : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FragmentManager.instance.AddFragment(1);
            Destroy(gameObject);
        }
    }
}
