using UnityEngine;

public class GTPlayerController : MonoBehaviour
{
    [SerializeField] private Renderer _playerRenderer;

    private void Start()
    {
        GTPlayerManager.Instance.RegisterPlayer(this);
    }

    public void SetMaterial(Material material)
    {
        _playerRenderer.material = material;
    }

    private void OnDestroy()
    {
        GTPlayerManager.Instance.UnregisterPlayer(this);
    }
}
