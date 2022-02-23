using UnityEngine;
using Jumplosion.Scripts.Enums;
using System.Collections;
using Jumplosion.Scripts.Player;

namespace Jumplosion.Scripts
{
    [System.Serializable]
    public class Projectile : MonoBehaviour
    {
        public PoolObjectType ObjectType { get; set; }
        [SerializeField] private float _speed;
        public float Speed { get => _speed; set => _speed = value; }
        [SerializeField] private float _explosionForce;
        public float ExplosionForce { get => _explosionForce; set => _explosionForce = value; }
        [SerializeField] private float _ExplosionRadius;
        public float ExplosionRadius { get => _ExplosionRadius; private set => _ExplosionRadius = value; }
        public float UpwardModifier;
        [SerializeField] private Rigidbody _rigidbody;
        private bool insidePlayer = true;
        private ObjectPooler _objectPooler;

        private void Awake()
        {
            _objectPooler = ObjectPooler.Instance;
        }

        private void FixedUpdate()
        {
            _rigidbody.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 6 && insidePlayer) return;
            if (!gameObject.activeInHierarchy) return;
            Vector3 collisionPoint = collision.GetContact(0).point;
            Collider[] colliders = Physics.OverlapSphere(collisionPoint, ExplosionRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent<Ragdoll>(out Ragdoll ragdoll))
                {
                    StartCoroutine(ragdoll.SetActive(true));
                    ragdoll.AddExplosionForce(_explosionForce, collisionPoint, ExplosionRadius, UpwardModifier);
                    continue;
                }

                Rigidbody rb = collider.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    rb.AddExplosionForce(_explosionForce, collisionPoint, ExplosionRadius, UpwardModifier, ForceMode.Impulse);
                }
            }
            GameObject explosion = _objectPooler.Spawn(PoolObjectType.Explosion);
            explosion.transform.position = collisionPoint;
            insidePlayer = true;
            _objectPooler.Pools.Find(x => x.ObjectType == ObjectType).pool.Release(this.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer == 6) insidePlayer = false;
        }

        private IEnumerator TurnOffOnTimer(float time)
        {
            yield return new WaitForSeconds(time);
            _objectPooler.ReturnToPool(ObjectType, this.gameObject);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnEnable()
        {
            StartCoroutine(TurnOffOnTimer(10));
        }

    }
}