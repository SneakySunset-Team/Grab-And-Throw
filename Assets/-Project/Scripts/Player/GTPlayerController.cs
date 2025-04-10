using UnityEngine;
using UnityEngine.VFX;

public class GTPlayerController : MonoBehaviour
{
    [SerializeField] private Renderer _playerRenderer;
    [SerializeField] private VisualEffect _spawnVfx;

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

    public void OnSpawn()
    {
        _spawnVfx.Play();
    }
}
