using UnityEngine;

public class GT_DestroyOnTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.root.TryGetComponent<GTPlayerTag>(out GTPlayerTag tag))
        {
            GTPlayerManager.Instance.SetPlayerPosition(tag.transform);
        }
    }
}
