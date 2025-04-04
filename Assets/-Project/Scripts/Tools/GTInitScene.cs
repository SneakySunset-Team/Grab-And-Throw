using UnityEngine;

public class GTInitScene : MonoBehaviour
{
    [SerializeField] private GTPlayerManager _managerPrefab;
    private void Awake()
    {
        if(GTPlayerManager.Instance == null)
        {
            Instantiate(_managerPrefab);
        }
    }
}
