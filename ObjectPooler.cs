using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler<T> where T : Object
{
    public bool Expandable { get; set; }
    public int TotalSize { get; private set; }
    public T LatestSpawn { get; private set; }
    
    private List<GameObject> _objectList = new();

    private readonly T _objectBase;
    private readonly Transform _poolBase;
    
    private delegate GameObject ObjectOnExceed();
    private readonly ObjectOnExceed _getObjectOnExceed;
    
    public ObjectPooler(T poolObject, int poolSize, bool expandable)
    {
        _objectBase = poolObject;
        TotalSize = poolSize;
        Expandable = expandable;

        var parent = new GameObject(_objectBase.name + " Pool");
        _poolBase = parent.transform;
        
        _getObjectOnExceed = Expandable ? AddNewObject : GetOldestObject;
        CreatePool(TotalSize);
    }
    
    public void SpawnObject(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        var currentObject = GetAvailableObject();
        
        currentObject.transform.position = spawnPosition;
        currentObject.transform.rotation = spawnRotation;
        currentObject.transform.SetAsLastSibling();
        currentObject.SetActive(true);
        
        LatestSpawn = GetLatestSpawn(currentObject);
    }
    
    private T GetLatestSpawn(GameObject gameObject) => gameObject is T type ? type : gameObject.GetComponent<T>();
    
    private GameObject GetAvailableObject()
    {
        GameObject currentObject = null;

        foreach (var obj in _objectList)
        {
            if (obj.activeInHierarchy)
                continue;

            currentObject = obj;
            break;
        }

        if (currentObject == null)
        {
            currentObject = _getObjectOnExceed();
        }
        
        return currentObject;
    }

    private void CreatePool(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Object.Instantiate(_objectBase, _poolBase);
            var currentObject = _poolBase.GetChild(i).gameObject;
            currentObject.name += " " + _poolBase.childCount;
            
            _objectList.Add(currentObject);
            currentObject.SetActive(false);
        }
    }

    private GameObject GetOldestObject()
    {
        return _poolBase.GetChild(0).gameObject;
    }
    
    private GameObject AddNewObject()
    {
        Object.Instantiate(_objectBase, _poolBase);
        var childCount = _poolBase.childCount;
        var currentObject = _poolBase.GetChild(childCount - 1).gameObject;
        currentObject.name += " " + childCount;
        TotalSize++;
        
        _objectList.Add(currentObject);
        return currentObject;
    }
}
