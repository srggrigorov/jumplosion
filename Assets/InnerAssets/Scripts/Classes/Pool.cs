using UnityEngine;
using Jumplosion.Scripts.Enums;
using UnityEngine.Pool;

namespace Jumplosion.Scripts.Classes
{
    [System.Serializable]
    public class Pool
    {
        public PoolObjectType ObjectType;
        public int defaultSize;
        public int maxSize;
        public bool CreateOnStart;
        public GameObject Prefab;
        public ObjectPool<GameObject> pool;
        public string containerName;
    }
}
