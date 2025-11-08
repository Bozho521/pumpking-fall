using UnityEngine;
using UnityEngine.SceneManagement;

public class AgeCounter : MonoBehaviour
{
    public float countInterval = 1f;

    private int currentCount = 0;

    private void Start()
    {
        StartCoroutine(CounterRoutine());
    }

    private System.Collections.IEnumerator CounterRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(countInterval);

            currentCount++;
            Debug.Log("Counter: " + currentCount);

            if (currentCount >= AgeManager.PlayerAge)
            {
                Die();
                yield break;
            }
        }
    }

    private void Die()
    {
        Debug.Log("Counter reached player age. Switching to EndScene_Test...");
        SceneManager.LoadScene("EndScene_Test");
    }
}
