using UnityEngine;
using UnityEngine.VFX;

public class GTPlayerController : MonoBehaviour
{
    [SerializeField] private Renderer _playerRenderer;
    [SerializeField] private VisualEffect _spawnVfx;
    private IGrabber _grabber;
    private IGrabbable _grabbable;

    private void Start()
    {
        GTPlayerManager.Instance.RegisterPlayer(this);
        _grabbable = GetComponent<IGrabbable>();
        _grabber = GetComponent<IGrabber>();
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

    public void KillPlayer()
    {
        _grabber.Release();
        _grabber.DisconnectFromSurface();
        GTPlayerManager.Instance.SetPlayerPosition(transform);
    }
}
