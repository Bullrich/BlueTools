using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// By @JavierBullrich

namespace Blue.Utility
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField]
        public PoolValues[] values;
        private GameObject[] poolContainer;

        private List<GameObject>[] pooledObjects;

        /// <summary>Call the methods by it's instance</summary>
        public static PoolManager instance;

        private void Start()
        {
            instance = this;
            poolContainer = new GameObject[values.Length];
            pooledObjects = new List<GameObject>[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                CreatePool(i);
            }
        }

        private void CreatePool(int index)
        {
            poolContainer[index] = new GameObject();
            poolContainer[index].name = values[index].pooledObject.name + " container";
            pooledObjects[index] = new List<GameObject>();
            values[index].objectName = values[index].pooledObject.name;
            for (int i = 0; i < values[index].pooledAmount; i++)
            {
                GameObject obj = (GameObject)Instantiate(values[index].pooledObject);
                obj.SetActive(false);
                pooledObjects[index].Add(obj);
                obj.transform.SetParent(poolContainer[index].transform);
            }
        }
        /// <summary>Get a non active pooled object, or, if the pool was allowed to grow, it instance a new object and returns it</summary>
        /// <param name="objectToGet">The pooled object name (be careful, it is the Prefab name, not the one assigned in the "object name" string field.</param>
        /// <returns></returns>
        public GameObject GetPooledObject(string objectToGet)
        {
            int index = 0;
            for (int i = 0; i < values.Length; i++)
                if (values[i].objectName == objectToGet)
                    index = i;

            for (int i = 0; i < pooledObjects[index].Count; i++)
            {
                if (!pooledObjects[index][i].activeInHierarchy)
                {
                    return pooledObjects[index][i];
                }
            }

            if (values[index].willGrow)
            {
                GameObject obj = (GameObject)Instantiate(values[index].pooledObject);
                pooledObjects[index].Add(obj);
                obj.transform.SetParent(poolContainer[index].transform);
                return obj;
            }
            return null;
        }
    }
    [System.Serializable]
    public class PoolValues
    {
        [HideInInspector]
        public string objectName = "pooledObject name";
        [Tooltip("Object that will ocupy a pool")]
        public GameObject pooledObject;
        [Range(1, 35)]
        public int pooledAmount = 20;
        [Tooltip("If the pooled amount is surpassed, can the game spawn a new one?")]
        public bool willGrow = true;
    }
}