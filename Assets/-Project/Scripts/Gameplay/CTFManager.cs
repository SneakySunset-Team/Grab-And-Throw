using UnityEngine;
using TMPro;

public class CTFManager : MonoBehaviour
{
    [SerializeField] private GameObject _flag;
    [SerializeField] private Transform _flagSpawnPoint;

    [SerializeField] private TMP_Text _displayPoint;
    public int[] _teamsPoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _displayPoint.text = _teamsPoints[0].ToString() + " : " + _teamsPoints[1].ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        _flag.transform.position = _flagSpawnPoint.position;
        _flag.transform.rotation = Quaternion.Euler(0,0,0) ;
    }

    public void AddPointToTeam(int idexTeam)
    {
        _teamsPoints[idexTeam]++;
        _displayPoint.text = _teamsPoints[0].ToString() + " : " + _teamsPoints[1].ToString();
    }
}
