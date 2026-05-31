using UnityEngine;
using UnityEngine.Events;

public class GameObjectToggle : MonoBehaviour
{
    [SerializeField] KeyCode toggleKeyCode = KeyCode.Space;

    [SerializeField] UnityEvent<bool> toggleEvent = null;

    [SerializeField] bool toggle = true;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleKeyCode))
        {
            toggle = !toggle;

            toggleEvent.Invoke(toggle);
        }
    }
}
