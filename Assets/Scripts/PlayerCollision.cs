using UnityEngine;

[DisallowMultipleComponent]
public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private LayerMask deathLayers;

    private bool _dead;
    
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

    
    //todo : call it after the 't' seconds to force kill the player
    public void OnKill()
    {
        if (_dead) return;
        
        _dead = true;

        Debug.Log("Player killed.");
    }
}