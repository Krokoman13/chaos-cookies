using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetOnCollider : MonoBehaviour
{
    [SerializeField] string tag;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (tag != other.tag) return;

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
