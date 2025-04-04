using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GTPlayerManager : GTSingleton<GTPlayerManager>
{
    void Start()
    {
        
    }

    [SerializeField] private Dictionary<EPlayerTag, GTPlayerParams> _playerData;
    [SerializeField] private CinemachineTargetGroup _targetGroup;
    [SerializeField] private EPlayerTag[] _playerTagOrder;
    private int _playerTagCurrentNum;
    private int _playerStartTargetIndex;
    private PlayerInputManager _playerInputManager;

    protected override void Awake()
    {
        base.Awake();
        _playerInputManager = GetComponent<PlayerInputManager>();
        _playerInputManager.playerPrefab = _playerData[_playerTagOrder[_playerTagCurrentNum]].PlayerPrefab;
        _playerTagCurrentNum++;
        _playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDestroy()
    {
        _playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        _playerInputManager.playerPrefab = _playerData[_playerTagOrder[_playerTagCurrentNum]].PlayerPrefab;
        _playerTagCurrentNum++;
    }

    public void SetPlayerPosition(Transform playerTransform)
    {
        EPlayerTag playerTag = playerTransform.GetComponent<GTPlayerTag>().GetPlayerTag();
        playerTransform.position =

        playerTransform.position = _playerData[playerTag].PlayerStart == null ?
            transform.position : _playerData[playerTag].PlayerStart[_playerStartTargetIndex].position;
    }

    public void RegisterPlayerStart(GTPlayerStart playerStart)
    {
        if (!_playerData.ContainsKey(playerStart.TargetPlayerTag))
        {
            Debug.LogError($"No Key {playerStart.TargetPlayerTag} in PlayerParams");
            return;
        }
        _playerData[playerStart.TargetPlayerTag].AddPlayerStart(playerStart);
    }

    public void RegisterPlayer(GTPlayerController player)
    {
        EPlayerTag playerTag = player.GetComponent<GTPlayerTag>().GetPlayerTag();
        if (_targetGroup)
            _targetGroup.AddMember(player.transform, 1, 3);

        if (!_playerData.ContainsKey(playerTag))
        {
            Debug.LogError($"There is no Key : {playerTag} in the PlayerParams Dictionary");
            return;
        }
        else if (_playerData[playerTag].Player != null)
        {
            Debug.LogError($"Player is already registered under the player tag : {playerTag}");
            return;
        }
            
        _playerData[playerTag].SetPlayer(player);
        SetPlayerPosition(player.transform);
    }

    public void UnregisterPlayer(GTPlayerController player)
    {
        EPlayerTag playerTag = player.GetComponent<GTPlayerTag>().GetPlayerTag();
        if (!_playerData.ContainsKey(playerTag))
        {
            Debug.LogError($"No player is registered under the player tag : {playerTag}");
            return;
        }

        _playerData.Remove(playerTag);
    }

    public string GetPlayerFmodName(EPlayerTag tag)
    {
        return _playerData[tag].PlayerFmodRepertory;
    }

    public void SetPlayerStartIndex(int index) => _playerStartTargetIndex = index;
}

[System.Serializable]
public class GTPlayerParams
{
    [HideInInspector] public GTPlayerController Player { get; private set; }
    [HideInInspector] public Dictionary<int, Transform> PlayerStart { get; private set; }
    [HideInInspector] public InputDevice InputDevice { get; private set; }
    [field : SerializeField] public GameObject PlayerPrefab {  get; private set; }
    [field : SerializeField] public string PlayerFmodRepertory { get; private set; }
    public void SetPlayer(GTPlayerController inPlayer) => Player = inPlayer;
    public void SetInputDevice(InputDevice inInputDevice) => InputDevice = inInputDevice;
    public void AddPlayerStart(GTPlayerStart playerStart)
    {
        if(PlayerStart == null) 
            PlayerStart = new Dictionary<int, Transform>();

        if (PlayerStart.ContainsKey(playerStart.PlayerStartIndex))
        {
            Debug.LogError($"2 Player Starts on index {playerStart.PlayerStartIndex} for tag {playerStart.TargetPlayerTag}");
            return;
        }
        PlayerStart.Add(playerStart.PlayerStartIndex, playerStart.transform);
    }
}
