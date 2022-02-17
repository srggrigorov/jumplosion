using UnityEngine;
using System.Collections;

namespace Jumplosion.Scripts
{
    [System.Serializable]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private GameObject _projectileGameObject;
        public GameObject ProjectileGameObject { get => _projectileGameObject; set => _projectileGameObject = value; }
        [SerializeField] private float _speed;
        public float Speed { get => _speed; set => _speed = value; }
        [SerializeField] private float _explosionForce;
        public float ExplosionForce { get => _explosionForce; set => _explosionForce = value; }
        [SerializeField] private float _ExplosionRadius;
        public float ExplosionRadius { get => _ExplosionRadius; private set => _ExplosionRadius = value; }

        public float UpwardModifier;
        private Collider _projectileCollider;

        private void Awake()
        {
            _projectileGameObject = this.gameObject;
            _projectileCollider = GetComponent<Collider>();
        }

        private void Start()
        {
            Destroy(gameObject, 10);
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
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
            Destroy(gameObject);
        }
    }
}