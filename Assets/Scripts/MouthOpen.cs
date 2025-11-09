using UnityEngine;
using UnityEngine.InputSystem; 

public class MouthOpen : MonoBehaviour
{
    [Header("Assign the two objects you want to toggle between")]
    public GameObject defaultObject;
    public GameObject alternateObject;

    [Tooltip("Which key should trigger the swap?")]
    public Key swapKey = Key.O; 

    void Start()
    {
        if (defaultObject != null)
            defaultObject.SetActive(true);
        if (alternateObject != null)
            alternateObject.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return; 

        if (Keyboard.current[swapKey].isPressed)
        {
            if (defaultObject != null) defaultObject.SetActive(false);
            if (alternateObject != null) alternateObject.SetActive(true);
        }
        else
        {
            if (defaultObject != null) defaultObject.SetActive(true);
            if (alternateObject != null) alternateObject.SetActive(false);
        }
    }
}
