using UnityEngine;

public class GTPlayerStart : MonoBehaviour
{
    [field: SerializeField] public EPlayerTag TargetPlayerTag { get; private set; }
    [field: SerializeField] public int PlayerStartIndex { get; private set; }

    private void Start()
    {
        GTPlayerManager.Instance.RegisterPlayerStart(this) ;
    }
}
