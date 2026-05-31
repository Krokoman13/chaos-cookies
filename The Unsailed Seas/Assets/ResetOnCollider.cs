using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetOnCollider : MonoBehaviour
{
    [SerializeField] string tag;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (tag != other.tag) return;

        GameManager.instance.Restart();

        GameObject.Destroy(gameObject);
    }
}
