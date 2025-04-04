using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GTBlueprintRecipeUI : MonoBehaviour
{

    // ****** PUBLIC      ******************************************
    public void UpdateRecipe(Dictionary<EComponentType, int> missingComponents)
    {
        foreach (var item in _recipeItems)
        {
            EComponentType componentId = item.Key;
            GTRecipeItemUI recipeItemUI = item.Value;

            if (missingComponents.ContainsKey(componentId))
            {
                recipeItemUI.UpdateCount(missingComponents[componentId]);
            }
            else
            {
                recipeItemUI.UpdateCount(0);
            }
        }
    }

    public void NewRecipe()
    {
        foreach (Transform child in _recipeItemsContainer)
        {
            Destroy(child.gameObject);
        }

        _recipeItems.Clear();
    }

    public void InitializeRecipe(Dictionary<EComponentType, int> components)
    {
        NewRecipe();


        foreach (var component in components)
        {
            EComponentType componentId = component.Key;
            int count = component.Value;

            GameObject recipeItemGO = Instantiate(_recipeItemPrefab, _recipeItemsContainer);
            GTRecipeItemUI recipeItemUI = recipeItemGO.GetComponent<GTRecipeItemUI>();

            recipeItemUI.Initialize(componentId, count);

            _recipeItems.Add(componentId, recipeItemUI);
        }
    }

    // ****** UNITY      ******************************************

    [SerializeField] private Transform _recipeItemsContainer;
    [SerializeField] private GameObject _recipeItemPrefab;


    // ****** RESTRICTED      ******************************************

    private Dictionary<EComponentType, GTRecipeItemUI> _recipeItems = new Dictionary<EComponentType, GTRecipeItemUI>();
}
