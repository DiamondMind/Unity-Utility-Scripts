using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DiamondMind.Prototypes.Tools.ObjectPool;

namespace DiamondMind.Prototypes.Tools
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [Tooltip("Log the total amount of objects instantiated")]
        public bool logTotal;
        public List<Pool> pools;

        Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(this.gameObject);
            }
            else if(Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            int grandTotal = 0;
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for(int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.name = pool.prefab.name;    // remove the (Clone) suffix added after instattiation to avoid errors
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform, true);
                    objectPool.Enqueue(obj);
                    if (pool.hideInHierachy) obj.hideFlags = HideFlags.HideInHierarchy;
                    grandTotal++;
                }

                poolDictionary.Add(pool.name, objectPool);
                if (logTotal) Debug.Log($"{pool.name} pool instantiated: {pool.size} objects");
            }

            if (logTotal)
            {
                Debug.Log($"Grand total of objects instantiated: {grandTotal}");
                Debug.Log($"Total time of instanting pools is {Time.time}");
            }
        }

        /// <summary>
        /// Retrieves an object from the pool by its name. If the pool is empty, a new object is instantiated.
        /// </summary>
        /// <param name="name">The exact name of the object to retrieve from the pool.</param>
        /// <param name="position">Optional position to set the object's position. If null, the position remains unchanged.</param>
        /// <param name="rotation">Optional rotation to set the object's rotation. If null, the rotation remains unchanged.</param>
        /// <returns>The retrieved or newly instantiated GameObject.</returns>
        public GameObject Get(string name, Vector3? position, Quaternion? rotation)
        {
            if (!poolDictionary.ContainsKey(name))
            {
                Debug.LogWarning("Pool with name " + name + " doesn't exist.");
                return null;
            }

            GameObject obj;

            if (poolDictionary[name].Count > 0)
            {
                obj = poolDictionary[name].Dequeue();
            }
            else
            {
                // Instantiate a new object if the pool is empty
                Pool pool = pools.Find(p => p.name == name);
                obj = Instantiate(pool.prefab);
                obj.name = pool.prefab.name;
                Debug.Log("Instantiated " + obj.name + " You might want to increase the pool size");
            }

            if (position.HasValue)
            {
                obj.transform.position = position.Value;
            }
            if (rotation.HasValue)
            {
                obj.transform.rotation = rotation.Value;
            }

            obj.hideFlags = HideFlags.None;
            obj.SetActive(true);

            return obj;
        }

        /// <summary>
        /// Retrieves an object from the pool by its id. If the pool is empty, a new object is instantiated.
        /// </summary>
        /// <param name="name">The exact name of the object to retrieve from the pool.</param>
        /// <param name="position">Optional position to set the object's position. If null, the position remains unchanged.</param>
        /// <param name="rotation">Optional rotation to set the object's rotation. If null, the rotation remains unchanged.</param>
        /// <returns>The retrieved or newly instantiated GameObject.</returns>        
        public  GameObject Get(int id, Vector3? position, Quaternion? rotation)
        {
            Pool pool = pools.Find(p => p.id == id);
            if(pool.prefab == null)
            {
                Debug.LogWarning("Pool with id " + id + " does't exist");
                return null;
            }

            string name = pool.name;
            if(!poolDictionary.ContainsKey(name))
            {
                Debug.LogWarning("Pool with name " + name + " does't exist");
                return null;
            }

            GameObject obj;
            if (poolDictionary[name].Count > 0)
            {
                obj = poolDictionary[name].Dequeue();
            }
            else
            {
                // Instantiate new object if the pool is empty
                obj = Instantiate(pool.prefab);
                obj.name = pool.prefab.name;
                Debug.Log("Instantiated " + obj.name + " You might want to increase the pool size");
            }

            if (position.HasValue)
            {
                obj.transform.position = position.Value;
            }
            if (rotation.HasValue)
            {
                obj.transform.rotation = rotation.Value;
            }

            obj.hideFlags = HideFlags.None;
            obj.SetActive(true);
            return obj;
        }

        public void Return(string name, GameObject obj)
        {
            if(!poolDictionary.ContainsKey(name))
            {
                Debug.LogWarning("Pool with name " + name + " doesn't exist.");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            Pool pool = pools.Find(p => p.name == name);
            if (pool.hideInHierachy)
                obj.hideFlags = HideFlags.HideInHierarchy;
            obj.transform.SetParent(this.transform, true);
            poolDictionary[name].Enqueue(obj);
        }

        [System.Serializable]
        public struct Pool
        {
            [Tooltip("Primary Identification: Must be thesame with the name of the prefab gameobject")]
            public string name;
            [Tooltip("Secondary Identification: Must be thesame with the name of the prefab gameobject")]
            public int id;
            public GameObject prefab;
            public int size;
            public bool hideInHierachy;
        }

    }
}
