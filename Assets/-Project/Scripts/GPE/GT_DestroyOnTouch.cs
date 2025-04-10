using UnityEngine;

public class GT_DestroyOnTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.root.TryGetComponent(out GTPlayerController player))
        {
            player.KillPlayer();
        }
    }
}
