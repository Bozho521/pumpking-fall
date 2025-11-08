using UnityEngine;
using TMPro;

public class RandomTextDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private string[] messages;

    void Start()
    {
        DisplayRandomText();
    }

    public void DisplayRandomText()
    {
        if (messages.Length == 0 || textBox == null)
        {
            Debug.LogWarning("Messages list or TextBox not set!");
            return;
        }

        int randomIndex = Random.Range(0, messages.Length);
        textBox.text = messages[randomIndex];
    }
}
