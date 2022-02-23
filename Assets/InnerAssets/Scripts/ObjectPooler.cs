using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Jumplosion.Scripts.Classes;
using Jumplosion.Scripts.Enums;

namespace Jumplosion.Scripts
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance;
        public List<Pool> Pools = new List<Pool>();


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            Pools.ForEach
            (
                objPool =>
                {
                    objPool.pool = new ObjectPool<GameObject>
                (
                    () => { return Instantiate(objPool.Prefab); },
                    poolingObj => poolingObj.SetActive(true),
                    poolingObj => poolingObj.SetActive(false),
                    poolingObj => Destroy(poolingObj),
                    false, objPool.defaultSize, objPool.maxSize
                );
                    if (objPool.CreateOnStart)
                    {
                        GameObject container = new GameObject(objPool.containerName);
                        for (int i = 0; i < objPool.defaultSize; i++)
                        {
                            objPool.pool.Release(Instantiate(objPool.Prefab, container.transform));
                        }
                    }
                }
            );
        }

        public GameObject Spawn(PoolObjectType objectType)
        {
            return Pools.Find(x => x.ObjectType == objectType).pool.Get();
        }

        public void ReturnToPool(PoolObjectType objectType, GameObject objectToReturn)
        {
            Pools.Find(x => x.ObjectType == objectType).pool.Release(objectToReturn);
        }


    }
}
