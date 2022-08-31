using System;
using System.Collections.Generic;
using CreativeVeinStudio.Simple_Pool_Manager.Models;
using UnityEngine;

namespace CreativeVeinStudio.Simple_Pool_Manager.Interfaces
{
    public interface IPoolActions
    {
        void SetNewCollectionParent(string poolName, Transform newParent);

        // Getters
        GameObject GetSpecificPoolItemInCollection(string poolName, int index);
        void GetSpecificPoolItemInCollection(string poolName, int index, Action<GameObject> callback);
        GameObject GetSpecificPoolItemInCollection(string poolName, GameObject item);
        GameObject GetNextAvailablePoolItem(string poolName);
        GameObject GetRandomPoolItem(string poolName,bool active=true);
        List<GameObject> GetAllItemInACategory(string poolName);
        List<GameObject> GetAllActiveCategoryItem(string poolName);
        Transform GetPoolItemParentTransform(string poolName);


        // Adding
        void AddNewItemToCollection(List<PoolProperties> newItemCol);
        void AddToInstantiatedList(PoolProperties poolObj, List<GameObject> items);
        void AddOverflowToCurrentPoolList(string poolName);


        // Disabling
        void DisablePoolByCategory(string poolName);
        void DisableAllExistingInPoolObjects();
        bool DisablePoolObject(GameObject poolObject);
        void DisablePoolObject(Transform poolObjectTransform);
        void DisablePoolObject(string poolName, Transform poolObjectTransform);
    }
}