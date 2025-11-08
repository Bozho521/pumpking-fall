using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private LayerMask deathLayers;
    [SerializeField] private GameObject playerGraphics;

    private PlayerFreeFallController playerFreeFallController;
    private bool _dead;

    private void Start()
    {
        playerFreeFallController = FindAnyObjectByType<PlayerFreeFallController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckAndMaybeKill(other.gameObject.layer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckAndMaybeKill(collision.collider.gameObject.layer);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CheckAndMaybeKill(hit.gameObject.layer);
    }

    private void CheckAndMaybeKill(int otherLayer)
    {
        if (((1 << otherLayer) & deathLayers) != 0)
        {
            OnKill();
        }
        
        //non death behaviour here
    }
    
    public void OnKill()
    {
        if (_dead) return;
        
        _dead = true;
        playerGraphics.SetActive(false);
        playerFreeFallController.enabled = false;

        Debug.Log("Player killed.");

        SceneManager.LoadScene(2);
    }
}