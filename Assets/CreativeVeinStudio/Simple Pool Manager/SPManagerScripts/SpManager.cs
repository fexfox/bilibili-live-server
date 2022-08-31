using System;
using System.Collections.Generic;
using System.Linq;
using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
using CreativeVeinStudio.Simple_Pool_Manager.Models;
using UnityEngine;

namespace CreativeVeinStudio.Simple_Pool_Manager
{
    public class SpManager : MonoBehaviour, IPoolActions
    {
        [SerializeField] public List<PoolProperties> collection = new List<PoolProperties>();
        [SerializeField] public List<GameObject> prefabs = new List<GameObject>();
        public string searchTxt = "";

        private readonly Dictionary<string, PoolProperties>
            _instantiatedList = new Dictionary<string, PoolProperties>();

        private PoolProperties _pool;

        #region Unity Functions

        private void Awake()
        {
            InstantiatePoolGameObjects();
        }

        #endregion

        //****************************
        // Initiations
        //****************************

        #region INIT Functions

        private void InstantiatePoolGameObjects()
        {
            var newList = new List<GameObject>();

            foreach (var poolCollection in collection)
            {
                foreach (GameObject poolItem in poolCollection.Items)
                {
                    for (int i = 0; i < poolCollection.InstantiateQty; i++)
                    {
                        var position = transform.position;
                        GameObject instantiatedGo = poolCollection.ParentTransform != null
                            ? Instantiate(poolItem, position, Quaternion.identity, poolCollection.ParentTransform)
                            : Instantiate(poolItem, position, Quaternion.identity);
                        instantiatedGo.name = instantiatedGo.name.Replace("(Clone)", "");
                        instantiatedGo.SetActive(false);
                        instantiatedGo.transform.localPosition = Vector3.zero;
                        newList.Add(instantiatedGo);
                    }
                }

                AddToInstantiatedList(poolCollection, newList);
                newList.Clear();
            }

            ActivateFirstItem();
        }

        private void ActivateFirstItem()
        {
            foreach (var poolCollection in collection.Where(poolCollection => poolCollection.ActiveFirstItem))
            {
                _instantiatedList[poolCollection.PoolName].Items.FirstOrDefault()?.SetActive(true);
            }
        }

        #endregion

        //****************************
        // Set Actions
        //****************************

        #region Set Functions

        public void SetNewCollectionParent(string poolName, Transform newParent)
        {
            if (DoesPoolExist(poolName))
            {
                _instantiatedList[poolName].Items.ForEach(x =>
                {
                    x.transform.parent = newParent;
                    x.transform.localPosition = Vector3.zero;
                    x.transform.rotation = newParent.rotation;
                });
            }
            else
            {
                Debug.Log(
                    $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
            }
        }

        #endregion

        //****************************
        // Get Actions
        //****************************

        #region Get functions

        /// <summary>
        /// Allows you to retrieve a specific pool item from the given pool by an index
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="index"></param>
        public GameObject GetSpecificPoolItemInCollection(string poolName, int index)
        {
            if (DoesPoolExist(poolName)) return _instantiatedList[poolName].Items[index];
            Debug.Log(
                $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
            return null;
        }


        /// <summary>
        /// Allows you to retrieve a specific pool item from the given pool by an index and returns it through a callback 
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        public void GetSpecificPoolItemInCollection(string poolName, int index, Action<GameObject> callback)
        {
            if (!DoesPoolExist(poolName))
            {
                Debug.Log(
                    $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
                return;
            }

            callback(_instantiatedList[poolName].Items[index]);
        }

        /// <summary>
        /// Retrieves the specific item provided from the specified Pool. 
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public GameObject GetSpecificPoolItemInCollection(string poolName, GameObject item)
        {
            if (!DoesPoolExist(poolName))
            {
                Debug.Log(
                    $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
                return null;
            }

            GameObject go = _instantiatedList[poolName].Items.FirstOrDefault(x => x.name.Equals(item.name));
            if (go == null)
            {
                Debug.Log(string.Format($"The item passed: {item.name}, does not exist in the pool: {poolName}"));
            }

            return go;
        }

        /// <summary>
        /// Retrieves the available pool item that is NOT active from the specified Pool
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public GameObject GetNextAvailablePoolItem(string poolName)
        {
            if (!DoesPoolExist(poolName))
            {
                Debug.Log(
                    $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
                return null;
            }

            GameObject go = _instantiatedList[poolName].Items.FirstOrDefault(item => !item.activeSelf);
            if (go) return go;
            AddOverflowToCurrentPoolList(poolName);
            return GetNextAvailablePoolItem(poolName);
        }

        /// <summary>
        /// Retrieves a random pool object from the specified Pool
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public GameObject GetRandomPoolItem(string poolName,bool active=true)
        {
            if (!DoesPoolExist(poolName))
            {
                Debug.Log(
                    $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
                return null;
            }

            _pool = _instantiatedList[poolName];

            var inactiveGo = _pool.Items.Where(itm => !itm.activeSelf).ToArray();
            var rand = UnityEngine.Random.Range(0, (inactiveGo.Count() - 1));
            var go = inactiveGo.ElementAt(rand);

            if (!go)
            {
                AddOverflowToCurrentPoolList(poolName);
                inactiveGo = _pool.Items.Where(itm => !itm.activeSelf).ToArray();
                rand = UnityEngine.Random.Range(0, (inactiveGo.Count() - 1));
                go = inactiveGo.ElementAt(rand);
            }

            go.SetActive(active);
            _pool = null;
            return go;
        }

        /// <summary>
        /// Retrieves all of the items in a specified Pool
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public List<GameObject> GetAllItemInACategory(string poolName)
        {
            if (DoesPoolExist(poolName)) return _instantiatedList[poolName].Items;
            Debug.Log(
                $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
            return null;
        }


        /// <summary>
        /// Retrieves all ACTIVE items in a specified Pool (ONLY the items that are enabled)
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public List<GameObject> GetAllActiveCategoryItem(string poolName)
        {
            if (DoesPoolExist(poolName)) return _instantiatedList[poolName].Items.Where(x => x.activeSelf).ToList();
            Debug.Log(
                $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
            return null;
        }

        /// <summary>
        /// Retrieves the parent's Transform for the specified pool
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public Transform GetPoolItemParentTransform(string poolName)
        {
            if (DoesPoolExist(poolName)) return _instantiatedList[poolName].ParentTransform;
            Debug.Log(
                $"No {poolName} object in the pool list. Please make sure you have the right colName or have created it");
            return null;
        }

        private bool DoesPoolExist(string poolName)
        {
            return _instantiatedList.ContainsKey(poolName);
        }

        #endregion

        //****************************
        // Add to pool Actions
        //****************************

        #region Add to pool Actions

        /// <summary>
        /// Allows you to add a list of pool items through Script by creating a PoolProperties list and passing it in.
        /// </summary>
        /// <param name="newItemCol"></param>
        public void AddNewItemToCollection(List<PoolProperties> newItemCol)
        {
            foreach (PoolProperties obj in newItemCol)
            {
                if (collection.Any(x => x.PoolName == obj.PoolName))
                {
                    collection.FirstOrDefault(x => x.PoolName == obj.PoolName)?.Items.Add(obj.Items.FirstOrDefault());
                }
                else
                {
                    collection.Add(obj);
                }
            }

            InstantiatePoolGameObjects();
        }

        /// <summary>
        /// Adds additional items to the existing collection
        /// </summary>
        /// <param name="poolObj"></param>
        /// <param name="items"></param>
        public void AddToInstantiatedList(PoolProperties poolObj, List<GameObject> items)
        {
            if (DoesPoolExist(name))
            {
                items.ForEach(_instantiatedList[poolObj.PoolName].Items.Add);
            }
            else
            {
                _instantiatedList.Add(
                    poolObj.PoolName,
                    new PoolProperties(
                        poolObj.PoolName,
                        false,
                        poolObj.InstantiateQty,
                        poolObj.ParentTransform,
                        new List<GameObject>(items),
                        poolObj.ToggleShowHide));
            }
        }

        public void AddOverflowToCurrentPoolList(string poolName)
        {
            Debug.Log($"Added extra items to : {poolName}, consider increasing the Qty to create in the pool");

            var currentPoolDetails = collection.FirstOrDefault(x => x.PoolName == poolName);
            if (currentPoolDetails == null) return;
            var currentPoolQty = currentPoolDetails.InstantiateQty;
            currentPoolDetails.InstantiateQty += currentPoolQty;

            for (int i = 0; i < currentPoolQty; i++)
            {
                foreach (var instantiatedGo in from item in currentPoolDetails.Items
                         let position = transform.position
                         select currentPoolDetails.ParentTransform != null
                             ? Instantiate(item, position, Quaternion.identity, currentPoolDetails.ParentTransform)
                             : Instantiate(item, position, Quaternion.identity))
                {
                    instantiatedGo.SetActive(false);
                    instantiatedGo.transform.localPosition = Vector3.zero;

                    _instantiatedList[poolName].Items.Add(instantiatedGo);
                }
            }
        }

        #endregion

        //****************************
        // Disable Actions
        //****************************

        #region Disable Actions

        /// <summary>
        /// Disables the Pool item that is located on the specified pool by its Transform
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="poolObjectTransform"></param>
        public void DisablePoolObject(string poolName, Transform poolObjectTransform)
        {
            int index = _instantiatedList[poolName].Items.IndexOf(poolObjectTransform.gameObject);
            if (index < 0)
            {
                Debug.Log($"Object {poolObjectTransform.name}. Doesnt exist in the specified Pool: {poolName}");
            }

            _instantiatedList[poolName].Items[index].SetActive(false);
        }


        /// <summary>
        /// Disables the Pool item that is provided to this function as a Transform and returns whether it was disabled or not
        /// </summary>
        /// <param name="poolObjectTransform"></param>
        public void DisablePoolObject(Transform poolObjectTransform)
        {
            foreach (var list in _instantiatedList)
            {
                var index = list.Value.Items.IndexOf(poolObjectTransform.gameObject);
                if (index > 0)
                {
                    list.Value.Items[index].SetActive(false);
                }
            }
        }

        /// <summary>
        /// Disables the Pool item that is provided to this function as a GameObject and returns whether it was disabled or not
        /// </summary>
        /// <param name="poolObject"></param>
        /// <returns></returns>
        public bool DisablePoolObject(GameObject poolObject)
        {
            foreach (var list in _instantiatedList)
            {
                //int indx = list.Value.items.IndexOf(poolObject);
                var item = list.Value.Items.Find(x => x.Equals(poolObject));
                if (!item) continue;

                item.SetActive(false);
                item.transform.SetParent(list.Value.ParentTransform);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Disables ALL pool objects in the Pool Manager
        /// </summary>
        public void DisableAllExistingInPoolObjects()
        {
            foreach (var list in _instantiatedList)
            {
                try
                {
                    list.Value.Items.FindAll(x => x.activeSelf).ForEach(x => x.SetActive(false));
                }
                catch (Exception ex)
                {
                    Debug.Log("Error: " + ex.Message);
                    Debug.Break();
                }
            }
        }

        /// <summary>
        /// Disables all the objects in a given Pool
        /// </summary>
        /// <param name="poolName"></param>
        public void DisablePoolByCategory(string poolName)
        {
            if (!DoesPoolExist(poolName))
            {
                Debug.Log(
                    $"No {poolName} object in the pool list. Please make sure you have the right collection name or have created it");
                return;
            }

            _instantiatedList[poolName].Items.FindAll(x => x.activeSelf).ForEach(x => x.SetActive(false));
        }

        #endregion


        #region Editor Functions

        // used for the Editor Script
        public void AddToCollection(string colName, bool activeFirstItem, int instQty, Transform parentTrans,
            GameObject item)
        {
            collection.Add(new PoolProperties(
                colName,
                activeFirstItem,
                instQty,
                parentTrans,
                new List<GameObject> {item},
                false
            ));
        }

        // used for the Editor Script
        public void AddToCollection(object poolInfo)
        {
            if (!(poolInfo is PoolProperties info)) return;
            collection.Add(new PoolProperties(
                info.PoolName,
                info.ActiveFirstItem,
                info.InstantiateQty,
                info.ParentTransform ? info.ParentTransform : null,
                new List<GameObject>(info.Items),
                false
            ));
        }

        #endregion
    }
}