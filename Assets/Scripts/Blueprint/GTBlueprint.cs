using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GTBlueprint : MonoBehaviour
{
    // ****** PUBLIC      ******************************************
    public GTGrabbableObject[] Recipe => _platformsToSnap;
    public bool RecipeCompleted { get; private set; }

    public event Action<Dictionary<EComponentType, int>> OnRecipeStateChanged;
    public event Action<Dictionary<EComponentType, int>> OnRecipeInited;

    public void InitializeMissingComponents()
    {
        _platformsToSnap = GetComponentsInChildren<GTGrabbableObject>();
        _collider = GetComponent<Collider>();
        _collider.enabled = true;
        _missingComponents.Clear();

        foreach (var platform in _platformsToSnap)
        {
            if (_missingComponents.ContainsKey(platform.ComponentID))
            {
                _missingComponents[platform.ComponentID]++;
            }
            else
            {
                _missingComponents.Add(platform.ComponentID, 1);
            }
        }

        OnRecipeInited?.Invoke(_missingComponents);
    }


    // ****** UNITY      ******************************************

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null)
            return;

        GTGrabbableObject _gTGrabbableObjectComponent = other.GetComponentInParent<GTGrabbableObject>();

        if (_gTGrabbableObjectComponent.CurrentState == EGrabbingState.Grabbed
            || _gTGrabbableObjectComponent.CurrentState == EGrabbingState.Snapped)
        {
            return;
        }

        bool wasObjectSnapped = false;

        foreach (var platform in _platformsToSnap.Where(x => x.ComponentID == _gTGrabbableObjectComponent.ComponentID && !x.HasObjectSnapped()))
        {
            _gTGrabbableObjectComponent.ChangeState(EGrabbingState.Snapped);
            _gTGrabbableObjectComponent.transform.position = platform.transform.position;
            _gTGrabbableObjectComponent.transform.rotation = platform.transform.rotation;
            _gTGrabbableObjectComponent.transform.parent = this.transform.root;
            platform.ObjectSnapped();

            if (_missingComponents.ContainsKey(_gTGrabbableObjectComponent.ComponentID))
            {
                _missingComponents[_gTGrabbableObjectComponent.ComponentID]--;
                if (_missingComponents[_gTGrabbableObjectComponent.ComponentID] <= 0)
                {
                    _missingComponents.Remove(_gTGrabbableObjectComponent.ComponentID);
                }
            }

            wasObjectSnapped = true;
            break;
        }

        if (wasObjectSnapped)
        {
            RecipeCompleted = _missingComponents.Count == 0;
            OnRecipeStateChanged?.Invoke(_missingComponents);
        }
    }


    // ****** RESTRICTED      ******************************************

    private Dictionary<EComponentType, int> _missingComponents = new Dictionary<EComponentType, int>();

    private GTGrabbableObject[] _platformsToSnap;
    private Collider _collider;
}
