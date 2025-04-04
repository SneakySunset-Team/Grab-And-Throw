using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class GTBlueprintsController : MonoBehaviour
{
    // ****** UNITY      ******************************************
    
    [SerializeField] private GTBlueprintRecipeUI _blueprintRecipeUI;
    [SerializeField] private GTGrabbableObject _finalObject;
    [SerializeField] private GameObject _boxColliderVisual;
    [SerializeField] private bool _instanciateFinalObject;

    private void Awake()
    {
        _models = GetComponentsInChildren<GTBlueprint>();

        InitializeCurrentBlueprint();
    }


    // ****** RESTRICTED      ******************************************

    private int _currentCheckedModelIndex = 0;

    private GTBlueprint[] _models;

    private void InitializeCurrentBlueprint()
    {
        if (_models.Length == 0)
            return;

        _models.ForEach(x=>x.gameObject.SetActive(false));

        _models[_currentCheckedModelIndex].gameObject.SetActive(true);
        _models[_currentCheckedModelIndex].OnRecipeInited += (Dictionary<EComponentType, int> missingComponents) => _blueprintRecipeUI.InitializeRecipe(missingComponents);
        _models[_currentCheckedModelIndex].InitializeMissingComponents();
        _models[_currentCheckedModelIndex].OnRecipeStateChanged += OnRecipeStateChanged;

        BoxCollider colliderToThrowIn = _models[_currentCheckedModelIndex].GetComponent<BoxCollider>();

        _boxColliderVisual.transform.position = colliderToThrowIn.center + colliderToThrowIn.transform.position;
        _boxColliderVisual.transform.localScale = colliderToThrowIn.size;
    }

    private void OnRecipeStateChanged(Dictionary<EComponentType, int> missingComponents)
    {
        _blueprintRecipeUI.UpdateRecipe(missingComponents);

        if (_models[_currentCheckedModelIndex].RecipeCompleted)
        {
            MoveToNextBlueprint();
        }
    }

    private void MoveToNextBlueprint()
    {
        _models[_currentCheckedModelIndex].OnRecipeStateChanged -= OnRecipeStateChanged;

        _currentCheckedModelIndex++;

        if (_currentCheckedModelIndex >= _models.Length)
        {
            Debug.Log("All blueprints completed!");

            _boxColliderVisual.SetActive(false);

            if (_finalObject != null)
            {
                if (_instanciateFinalObject) { Instantiate(_finalObject, this.transform.position, this.transform.rotation); }
                else {_finalObject.gameObject.SetActive(true); }
                Destroy(this.gameObject);
            }

            return;
        }

        InitializeCurrentBlueprint();
    }
}
