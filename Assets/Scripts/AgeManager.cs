using UnityEngine;
using TMPro;

public class AgeManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField ageInputField;

    public static int PlayerAge { get; private set; } = -1;

    private const string AgeKey = "PlayerAge";

    private void Awake()
    {
        if (PlayerPrefs.HasKey(AgeKey))
        {
            PlayerAge = PlayerPrefs.GetInt(AgeKey);
        }

        if (ageInputField == null)
        {
            ageInputField = GetComponent<TMP_InputField>();
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (PlayerAge >= 0 && ageInputField != null)
        {
            ageInputField.text = PlayerAge.ToString();
        }

        ageInputField.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string input)
    {
        string digitsOnly = "";
        foreach (char c in input)
        {
            if (char.IsDigit(c))
                digitsOnly += c;
        }

        if (digitsOnly != input)
        {
            ageInputField.text = digitsOnly;
            return;
        }

        if (int.TryParse(digitsOnly, out int value))
        {
            PlayerAge = value;
            PlayerPrefs.SetInt(AgeKey, PlayerAge);
            PlayerPrefs.Save();

            Debug.Log($"Player age updated: {PlayerAge}");
        }
        else
        {
            PlayerAge = -1;
        }
    }
}
