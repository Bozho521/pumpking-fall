using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouthOpen : MonoBehaviour
{
    [Header("Assign the two objects you want to toggle between")]
    public GameObject defaultObject;
    public GameObject alternateObject;

    [Tooltip("Which key should trigger the swap/boost?")]
    public Key swapKey = Key.O;

    [Header("Boost Settings")]
    public float maxBoost = 5f;
    public float drainRate = 1f;
    public float rechargeRate = 0.5f;
    public float boostMultiplier = 5f;

    [Header("References")]
    public Image boostBar;
    public PlayerFreeFallController playerController;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip boostSound;

    private float currentBoost;
    private bool boostLocked = false;
    private float boostThreshold => maxBoost / 2f;
    private bool boostSoundPlaying = false;

    private void Start()
    {
        currentBoost = maxBoost;

        SetMouthState(false);

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerFreeFallController>();
            if (playerController == null)
                Debug.LogWarning("MouthOpen: PlayerFreeFallController not found in scene!");
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateBoostUI();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        bool keyHeld = Keyboard.current[swapKey].isPressed;

        if (!keyHeld)
        {
            currentBoost += rechargeRate * Time.deltaTime;
            currentBoost = Mathf.Min(maxBoost, currentBoost);

            if (currentBoost >= boostThreshold)
                boostLocked = false;

            boostSoundPlaying = false;
        }

        if (keyHeld && !boostLocked && currentBoost > 0)
        {
            BoostActive();

            if (!boostSoundPlaying && boostSound != null)
            {
                audioSource.PlayOneShot(boostSound);
                boostSoundPlaying = true;
            }

            if (currentBoost <= 0)
                boostLocked = true;
        }
        else
        {
            BoostInactive();
        }

        UpdateBoostUI();
    }

    private void BoostActive()
    {
        currentBoost -= drainRate * Time.deltaTime;
        currentBoost = Mathf.Max(0, currentBoost);

        SetMouthState(true);

        if (playerController != null)
            playerController.externalSpeedMultiplier = boostMultiplier;

        if (audioSource != null && boostSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = boostSound;
            audioSource.Play();
        }
    }

    private void BoostInactive()
    {
        currentBoost += rechargeRate * Time.deltaTime;
        currentBoost = Mathf.Min(maxBoost, currentBoost);

        SetMouthState(false);

        if (playerController != null)
            playerController.externalSpeedMultiplier = 1f;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }


    private void SetMouthState(bool open)
    {
        if (defaultObject != null) defaultObject.SetActive(!open);
        if (alternateObject != null) alternateObject.SetActive(open);
    }

    private void UpdateBoostUI()
    {
        if (boostBar != null)
            boostBar.fillAmount = currentBoost / maxBoost;
    }

    public bool CanBoost() => currentBoost > 0;
}
