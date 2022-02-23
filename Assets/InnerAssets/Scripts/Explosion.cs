using UnityEngine;
using Jumplosion.Scripts.Enums;

namespace Jumplosion.Scripts
{
    public class Explosion : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        [SerializeField] private PoolObjectType _objectType;

        private void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnParticleSystemStopped()
        {
            ObjectPooler.Instance.ReturnToPool(_objectType, this.gameObject);
        }
    }
}
