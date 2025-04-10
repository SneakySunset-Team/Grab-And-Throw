using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum EComponentType
{
    None,
    Door,
    Cube,
    Tile,
    Staircase,
    Window,
    Disk,
    Disk_S,
    Slope,
    Slope_S,
    Beam,
    Board,
    BigBoard,
    x2Beam,
    Proue,
    Terrasse,
    Voile,
    Boat,
    Player,
}


[RequireComponent(typeof(Rigidbody), typeof(ConstantForce))]
[SelectionBase]
public partial class GTGrabbableObject : SerializedMonoBehaviour, IGrabbable
{
    // ****** PUBLIC     ******************************************

    public EGrabbingState CurrentState { get; set; }
    public Action<IGrabber> OnGrabbedAction { get; set; }
    public Action<IGrabber> OnReleasedAction { get; set; }

    public virtual void OnGrabbed(IGrabber grabber, Transform grabberTransformParent, IGrabbable grabberGrabbable)
    {
        Transform grabberTransform = grabber.GetGrabberComponent<Transform>();
        _grabberGrabbable = grabberGrabbable;

        if (_tag)
        {
            FMODUnity.RuntimeManager.PlayOneShot($"event:/Chara/{GTPlayerManager.Instance.GetPlayerFmodName(_tag.GetPlayerTag())}/GetGrabbed");
        }

        if(grabberGrabbable != null)
        {
            grabberGrabbable.OnGrabbedAction += OnParentGrabbed;
            grabberGrabbable.OnReleasedAction += OnParentReleased;
        }
        ChangeState(EGrabbingState.Grabbed);


        transform.rotation = Quaternion.identity;
        this.transform.SetParent(grabberTransformParent, false);
        this.transform.position = grabberTransformParent.position;
        _holder = grabber;
        
        ReinitializeJoint();

        OnGrabbedAction?.Invoke(grabber);
        if(_stunEnum != null)
        {
            StopCoroutine(_stunEnum);
            _stunEnum = null;
        }
    }

    public void OnThrowed(Vector3 force, IGrabbable grabberGrabbable)
    {
        if (_tag)
        {
            FMODUnity.RuntimeManager.PlayOneShot($"event:/Chara/{GTPlayerManager.Instance.GetPlayerFmodName(_tag.GetPlayerTag())}/GetThrown");
        }

        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        if (CurrentState != EGrabbingState.Stunned)
            ChangeState(EGrabbingState.Thrown);

        if (_grabberGrabbable != null)
        {
            _grabberGrabbable.OnGrabbedAction -= OnParentGrabbed;
            _grabberGrabbable.OnReleasedAction -= OnParentReleased;
            _grabberGrabbable = null;
        }
        this.transform.SetParent(null, true);
        _rb.AddForce(force, ForceMode.Impulse);
        //StartCoroutine(ClearIgnoreCollisionsEnum());
    }

    public void OnReleased()
    {
        if (_myGrabber == null) return;


        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        if (CurrentState != EGrabbingState.Stunned)
            ChangeState(EGrabbingState.Passif);

        if (_grabberGrabbable != null)
        {
            _grabberGrabbable.OnGrabbedAction -= OnParentGrabbed;
            _grabberGrabbable.OnReleasedAction -= OnParentReleased;
            _grabberGrabbable = null;
        }
        this.transform.SetParent(null, true);
        _myGrabber = null;
        _grabberGrabbable = null;
        //StartCoroutine(ClearIgnoreCollisionsEnum());
    }

    public virtual void Stun(float stunDuration)
    {
        if (_stunEnum != null)
        {
            StopCoroutine(_stunEnum);
        }
        _stunEnum = StunCoroutine(stunDuration);
        StartCoroutine(_stunEnum);
    }

    public virtual void SetFocused(bool toFocused)
    {
        if(_normalMaterials == null || _normalMaterials.Length == 0)
            return;

        for (int j = 0; j < _renderers.Length; j++)
        {
            if(j >= _normalMaterials.Length || _normalMaterials[j] == null || _normalMaterials[j].Length == 0)
            {
                continue;
            }

            List<Material> materials = _normalMaterials[j].ToList();
            if (toFocused)
            {
                materials = new List<Material>();
                foreach (Material material in _normalMaterials[j])
                {
                    materials.Add(_highlightMaterial);
                }
            }

            _renderers[j].SetMaterials(materials);
        }
    }

    public void ChangeState(EGrabbingState newState)
    {
        OnStateExitEvent?.Invoke(CurrentState);
        CurrentState = newState;
        _grabbingState = newState;
        OnStateEnterEvent?.Invoke(newState);
    }

    public T GetGrabbableComponent<T>()
    {
        return GetComponent<T>();
    }

    public void ReinitializeJoint()
    {
        if (_joint != null)
        {
            Destroy(_joint);
        }
        _joint = gameObject.AddComponent<ConfigurableJoint>();
        _jointParams.InitializeJoint(ref _joint);
        _joint.connectedBody = _holder.GetGrabberComponent<Rigidbody>();
    }

    public (Transform, Transform) GetGrabPoints()
    {
        return (_GrabPointLeft, _GrabPointRight);
    }

    public EComponentType ComponentID => _ComponentID;

    public void ObjectSnapped()
    {
        if (!_isBlueprint)
            return;

        _renderers.ForEach(a=>a.gameObject.SetActive(false));
        _hasObjectSnapped = true;
    }

    public void ObjectUnsnapped()
    {
        if (!_isBlueprint)
            return;

        _renderers.ForEach(a => a.gameObject.SetActive(true));
        _hasObjectSnapped = false;
    }

    public GTPhysicsParams GetPhysicParams(EGrabbingState state)
    {
        if (_physicsDictionaryOverride != null && _physicsDictionaryOverride.ContainsKey(state))
        {
            return _physicsDictionaryOverride[state];
        }
        else if(_physicsDictionary != null && _physicsDictionary.PhysicParamsDictionary.ContainsKey(state))
        {
            var physParams = new GTPhysicsParams();
            physParams.SetParams(_physicsDictionary.PhysicParamsDictionary[state]);
            return   physParams;
        }

        return new GTPhysicsParams();
    }

    public bool HasObjectSnapped() => _hasObjectSnapped;

    // ****** UNITY      ******************************************

    [HideInInspector] public Action<EGrabbingState> OnStateEnterEvent;
    [HideInInspector] public Action<EGrabbingState> OnStateExitEvent;

    [SerializeField, ReadOnly] protected Material[][] _normalMaterials;
    [SerializeField, HideIf("_isBlueprint")] protected Material _highlightMaterial;

    [SerializeField, FoldoutGroup("Dictionaries")] private GTSO_PhysicsDictionary _physicsDictionary;
    [SerializeField, FoldoutGroup("Dictionaries"), DictionaryDrawerSettings()] private Dictionary<EGrabbingState, GTPhysicsParams> _physicsDictionaryOverride;
    [SerializeField, FoldoutGroup("Dictionaries")] private GTSO_ConfigurableJointParams _jointParams;
    [SerializeField, ReadOnly, FoldoutGroup("Dictionaries")] private EGrabbingState _grabbingState;

    [SerializeField, FoldoutGroup("Blueprint")] private bool _isBlueprint = false;
    [SerializeField, ShowIf("_isBlueprint"), FoldoutGroup("Blueprint")] private Material _blueprintMaterial;
    [SerializeField, FoldoutGroup("Blueprint")] private EComponentType _ComponentID;
    [SerializeField, ShowIf("_isBlueprint"), ReadOnly, FoldoutGroup("Blueprint")] private bool _hasObjectSnapped = false;

    [SerializeField] private Transform _GrabPointLeft;
    [SerializeField] private Transform _GrabPointRight;

    [SerializeField] private TrailRenderer _thrownTrailRenderer;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _constantForce = GetComponent<ConstantForce>();
        _myGrabber = GetComponent<IGrabber>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _normalMaterials = _renderers.Select(r => r.materials).ToArray();
    }

    protected virtual void Start()
    {
        _myColliders = GetComponentsInChildren<Collider>().Where(b=>b.isTrigger == false).ToArray();
        _tag = GetComponent<GTPlayerTag>();
        if (TryGetComponent<GTCharacter_Move>(out var mover))
        {
            OnStateEnter(EGrabbingState.Passif);
        }
    }

    private void OnEnable()
    {
        OnStateEnterEvent += OnStateEnter;
        OnStateExitEvent += OnStateExit;
    }

    private void OnDisable()
    {
        if(_myGrabber != null)
        {
            _myGrabber.Release();
        }
        OnStateEnterEvent -= OnStateEnter;
        OnStateExitEvent -= OnStateExit;
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentState == EGrabbingState.Thrown)
        {
            IGrabber otherGrabber = collision.transform.root.GetComponent<IGrabber>();
            if (otherGrabber != null && (otherGrabber == _myGrabber || otherGrabber == _holder || otherGrabber.GetGrabbedObject() != null)) return;

            ChangeState(EGrabbingState.Passif);
            _holder = null;
        }
    }

#if UNITY_EDITOR
    // M�thode pour l'�diteur uniquement pour g�rer les changements dans l'inspecteur
    private void OnValidate()
    {
        if (_previousIsBlueprint != _isBlueprint)
            UpdateComponentsVisibility();
    }

    private void UpdateComponentsVisibility()
    {
        _previousIsBlueprint = _isBlueprint;
        _rb = GetComponent<Rigidbody>();
        _constantForce = GetComponent<ConstantForce>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _normalMaterials = _renderers.Select(a => a.sharedMaterials).ToArray();
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        if (_isBlueprint)
        {
            _renderers.ForEach(a=>a.SetSharedMaterials(new List<Material>() { _blueprintMaterial }));
            CurrentState = EGrabbingState.Snapped;
            colliders.ForEach(c => c.enabled = false);

            _rb.isKinematic = true;
            _rb.useGravity = false;

            _constantForce.enabled = false;
        }
        else
        {
            // Enable GTGrabbableObject behavior

            for(int i = 0; i < _renderers.Length; i++)
            {
                if(_normalMaterials.GetLength(0) > i)
                {
                    _renderers[i].sharedMaterials = _normalMaterials[i];
                }
            }

            CurrentState = EGrabbingState.Passif;
            _constantForce.enabled = true;
            colliders.ForEach(c => c.enabled = true);
        }

        EditorUtility.SetDirty(this);
        SceneView.RepaintAll();
    }
#endif

    // ****** RESTRICTED ******************************************

    protected Rigidbody _rb;
    protected Renderer[] _renderers;
    private ConstantForce _constantForce;
    private IGrabber _holder;
    private IGrabbable _grabberGrabbable;
    private IGrabber _myGrabber;
    private IEnumerator _stunEnum;
    private Collider[] _myColliders;
    private Collider[] _ignoreColliders;
    private ConfigurableJoint _joint;
    private GTPlayerTag _tag;
    private FMOD.Studio.EventInstance _fmodStunEvent;
    private bool _previousIsBlueprint;

    private void OnParentReleased(IGrabber g)
    {
        ChangeState(EGrabbingState.Grabbed);
    }

    private void OnParentGrabbed(IGrabber g)
    {
        _rb.mass = 0;
        IgnoreCollisions();
    }

    private void OnStateEnter(EGrabbingState newState)
    {

        ChangePhysicState(newState);

        switch (newState)
        {
            case EGrabbingState.Thrown:
                if(_thrownTrailRenderer != null)
                {
                    _thrownTrailRenderer.enabled = true;
                }
                break;
            default:
                break;
        }

    }

    private void OnStateExit(EGrabbingState oldState)
    {
        switch (oldState)
        {
            case EGrabbingState.Thrown:
                if (_thrownTrailRenderer != null)
                {
                    _thrownTrailRenderer.enabled = false;
                }
                break;
            case EGrabbingState.Grabbed:
                SetFocused(false);
                break;
            default:
                break;
        }

        if (oldState == EGrabbingState.Grabbed)
        {
        }
    }

    private void ChangePhysicState(EGrabbingState newState)
    {
        GTPhysicsParams physParams = GetPhysicParams(newState);

        _myColliders.ForEach(collider => collider.material = physParams.PhysicsMaterial);
        _rb.isKinematic = !physParams.IsPhysicsEnabled;     

        if (!_rb.isKinematic)
        {
            _rb.useGravity = physParams.IsPhysicsGravityEnabled;
            _constantForce.force = new Vector3(0, physParams.GravityAdditionalStrength, 0);
            _rb.constraints = _rb.SetFlaggedConstraints(physParams.RigidbodyConstraints);
            _rb.mass = physParams.Mass;
        }
        else
        {
            _rb.useGravity = false;
            _constantForce.force = Vector3.zero;
            _rb.constraints = _rb.constraints = RigidbodyConstraints.FreezeAll;
            _rb.mass = physParams.Mass;
        }
    }

    private void IgnoreCollisions()
    {
        ClearIgnoreCollisions();
        _ignoreColliders = transform.root.GetComponentsInChildren<Collider>().Where(b => b.isTrigger == false && !_myColliders.Contains(b)).ToArray();
        foreach (Collider c in _ignoreColliders)
        {
            foreach(Collider myC in _myColliders)
            {
                Physics.IgnoreCollision(c, myC, true);
            }
        }
    }

    private IEnumerator ClearIgnoreCollisionsEnum()
    {
        yield return new WaitForSeconds(.3f);
        if(_ignoreColliders != null)
        {
            foreach (Collider c in _ignoreColliders)
            {
                foreach (Collider myC in _myColliders)
                {
                    Physics.IgnoreCollision(c, myC, false);
                }
            }
        }
    }

    private void ClearIgnoreCollisions()
    {
        if (_ignoreColliders != null)
        {
            foreach (Collider c in _ignoreColliders)
            {
                foreach (Collider myC in _myColliders)
                {
                    Physics.IgnoreCollision(c, myC, false);
                }
            }
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        if (_tag)
        {
            FMODUnity.RuntimeManager.PlayOneShot($"event:/Chara/{GTPlayerManager.Instance.GetPlayerFmodName(_tag.GetPlayerTag())}/GetHit");
            _fmodStunEvent = FMODUnity.RuntimeManager.CreateInstance("event:/Chara/Chara_Common/GetStunned");
            _fmodStunEvent.start();
        }
        ChangeState(EGrabbingState.Stunned);
        yield return new WaitForSeconds(duration);
        ChangeState(EGrabbingState.Passif);
        if (_tag)
        {
            _fmodStunEvent.release();
        }
        _stunEnum = null;
    }



}
