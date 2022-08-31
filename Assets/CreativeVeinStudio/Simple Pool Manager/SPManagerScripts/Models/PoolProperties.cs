using System.Collections.Generic;
using UnityEngine;

namespace CreativeVeinStudio.Simple_Pool_Manager.Models
{
    [System.Serializable]
    public class PoolProperties
    {
        [SerializeField] private string poolName;
        [SerializeField] private bool activeFirstItem;
        [SerializeField] private int instantiateQty;
        [SerializeField] private Transform parentTransform;
        [SerializeField] private List<GameObject> item;
        [SerializeField] private bool toggleShowHide;

        public PoolProperties()
        {
        }

        public PoolProperties(string poolName, bool activeFirstItem, int instantiateQty, Transform parentTransform,
            List<GameObject> item, bool toggleShowHide)
        {
            this.poolName = poolName;
            this.activeFirstItem = activeFirstItem;
            this.instantiateQty = instantiateQty;
            this.parentTransform = parentTransform;
            this.item = item;
            this.toggleShowHide = toggleShowHide;
        }

        public string PoolName
        {
            get => poolName;
            set => poolName = value;
        }

        public bool ActiveFirstItem
        {
            get => activeFirstItem;
            set => activeFirstItem = value;
        }

        public int InstantiateQty
        {
            get => instantiateQty;
            set => instantiateQty = value;
        }

        public List<GameObject> Items
        {
            get => item;
            set => item = value;
        }

        public Transform ParentTransform
        {
            get => parentTransform;
            set => parentTransform = value;
        }

        public bool ToggleShowHide
        {
            get => toggleShowHide;
            set => toggleShowHide = value;
        }
    }
}