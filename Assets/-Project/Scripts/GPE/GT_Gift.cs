using System.Collections;
using UnityEngine;

public class GT_Gift : GTGrabbableObject
{
    [SerializeField] private GTGrabbableObject[] _grabbableObjects = new GTGrabbableObject[0];
    [SerializeField] private int _minObjectsToSpawnCount = 0;
    [SerializeField] private int _maxObjectsToSpawnCount = 3;
    [SerializeField] private int _explosionForce = 5;
    [SerializeField] private int _explosionRadius = 5;

    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentState == EGrabbingState.Thrown)
            Open();
    }

    private void Open()
    {
        int objCount = Random.Range(_minObjectsToSpawnCount, _maxObjectsToSpawnCount);

        this.gameObject.SetActive(false);

        for (int i = 0; i < objCount; i++)
        {
            int j = Random.Range(0, _grabbableObjects.Length);

            GTGrabbableObject newObject = Instantiate(_grabbableObjects[j], transform.position, Quaternion.identity);

            if (newObject.GetComponent<Rigidbody>() != null)
            {
                newObject.Stun(1f);
                newObject.GetComponent<Rigidbody>().AddExplosionForce(_explosionForce, transform.position - Vector3.up, _explosionRadius);
            }
        }
    }
}
