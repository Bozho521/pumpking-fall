using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AgeCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI timerTxt;
    [SerializeField] private PlayerCollision playerCollision;

    [Header("Counter Settings")]
    [SerializeField] private float countInterval = 1f;

    private int currentCount = 0;
    private int targetAge;

    public Action onPlayerDie;

    private void Start()
    {
        targetAge = AgeManager.PlayerAge;

        if (targetAge <= 0)
        {
            Debug.LogWarning("No valid player age found in AgeManager. Defaulting to 10.");
            targetAge = 10;
        }

        if (timerTxt != null)
        {
            timerTxt.text = "";
        }
        StartCoroutine(CounterRoutine());
    }

    private void OnEnable()
    {
        if (!playerCollision)
        {
            playerCollision = FindAnyObjectByType<PlayerCollision>();
        }

        if (playerCollision)
            onPlayerDie += playerCollision.OnKill;
    }

    private void OnDisable()
    {
        if (playerCollision)
            onPlayerDie -= playerCollision.OnKill;
    }

    private IEnumerator CounterRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(countInterval);

            currentCount++;
            Debug.Log("Counter: " + currentCount);

            if (timerTxt)
                timerTxt.text = currentCount.ToString();

            if (currentCount >= targetAge)
            {
                Debug.Log("Counter reached player age. Death and scene switch.");
                onPlayerDie?.Invoke();
                Die();
                yield break;
            }
        }
    }

    private void Die()
    {
        // some delay or animation of death?
        SceneManager.LoadScene("3_EndScene_Test");
    }
}
