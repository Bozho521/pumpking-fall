using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AgeCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerTxt;
    [SerializeField] private PlayerCollision playerCollision;

    [SerializeField] private  float countInterval = 1f;
    [SerializeField, Range(1,99)] private int ageCount;
    
    
    private int currentCount = 0;
    
    public Action onPlayerDie;

    private void Start()
    {
        StartCoroutine(CounterRoutine());

        // if (AgeManager == null)
        // {
        //     ageCount = AgeManager.PlayerAge;
        // }

    }

    private void OnEnable()
    {
        if (!playerCollision)
        {
            playerCollision = FindAnyObjectByType<PlayerCollision>();
        }
        
        onPlayerDie += playerCollision.OnKill;
    }

    private void OnDisable()
    {
        onPlayerDie -= playerCollision.OnKill;
    }


    private IEnumerator CounterRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(countInterval);

            currentCount++;
            Debug.Log("Counter: " + currentCount);

            timerTxt.text = currentCount.ToString();
            
            if (currentCount >= ageCount)
            {
                onPlayerDie?.Invoke();
                yield break;
            }
        }
    }
    
    
    
}
