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
    [SerializeField] private GameObject deathSpawnPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1, 0);

    private int currentCount = 0;
    private int targetAge;

    public Action onPlayerDie;

    private void Start()
    {
        targetAge = AgeManager.PlayerAge > 0 ? AgeManager.PlayerAge : 99;

        if (timerTxt != null)
            timerTxt.text = "";

        StartCoroutine(CounterRoutine());
    }

    private void OnEnable()
    {
        if (!playerCollision)
            playerCollision = FindAnyObjectByType<PlayerCollision>();

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
            Debug.Log($"Counter: {currentCount}");

            if (timerTxt)
                timerTxt.text = currentCount.ToString();

            if (currentCount >= targetAge)
            {
                Debug.Log("Counter reached player age. Death sequence triggered.");

                Debug.Log($"Player current position: {PlayerLocator.Instance.CurrentPosition}");

                if (deathSpawnPrefab && PlayerLocator.Instance != null)
                {
                    Vector3 spawnPos = PlayerLocator.Instance.CurrentPosition - spawnOffset;

                    Instantiate(deathSpawnPrefab, spawnPos, deathSpawnPrefab.transform.rotation);

                    Debug.Log($"Spawned prefab at {spawnPos} relative to player with default rotation.");
                }
                else
                {
                    Debug.LogWarning("Death prefab missing or PlayerLocator not found in scene!");
                }


                yield break;
            }
        }
    }
}
