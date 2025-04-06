using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GTRecipeItemUI : MonoBehaviour
{
    // ****** PUBLIC      ******************************************
    public void Initialize(EComponentType componentId, int count)
    {
        _componentId = componentId;

        UpdateIcon(componentId);
        UpdateCount(count);
    }

    public void UpdateCount(int count)
    {
        _countText.text = $"x{count}";

        if (count <= 0)
        {
            _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, 0.5f);
            _countText.color = new Color(_countText.color.r, _countText.color.g, _countText.color.b, 0.5f);
        }
        else
        {
            _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, 1f);
            _countText.color = new Color(_countText.color.r, _countText.color.g, _countText.color.b, 1f);
        }
    }


    // ****** UNITY      ******************************************

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _countText;


    // ****** RESTRICTED      ******************************************

    private EComponentType _componentId;

    private void UpdateIcon(EComponentType componentId)
    {
        if (GTBlueprintResourceManager.Instance.GetComponentIcon(componentId) != null)
        {
            _icon.sprite = GTBlueprintResourceManager.Instance.GetComponentIcon(componentId);
        }
        else
        {
            Debug.Log("No icon");
        }
    }
}
