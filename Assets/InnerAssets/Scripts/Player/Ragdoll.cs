using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jumplosion.Scripts
{
    public class Ragdoll : MonoBehaviour
    {
        [SerializeField] private float _minVelocity;
        [SerializeField] private Transform _hipsTransform;
        private Vector3 _hipsLocalPosition;
        private List<Quaternion> _bonesRotations = new List<Quaternion>();
        private List<Collider> _colliders = new List<Collider>();
        public List<Collider> Colliders { get => _colliders; set => _colliders = value; }
        public CharacterController CharacterController { get; private set; }
        public Animator Animator { get; private set; }
#nullable enable
        public PlayerController? PlayerController { get; private set; }
#nullable disable
        public Rigidbody HeaviestRigidbody { get; private set; }
        public bool IsActive { get; private set; } = false;

        private void Awake()
        {
            Initialize();
        }

        public IEnumerator SetActive(bool value)
        {
            if (value && !IsActive)
            {
                CharacterController.enabled = false;
                Animator.enabled = false;
                if (PlayerController) PlayerController.enabled = false;
                foreach (Collider collider in Colliders)
                {
                    collider.isTrigger = false;
                    collider.attachedRigidbody.isKinematic = false;
                }
                IsActive = true;
                yield break;
            }
            else if (!value && IsActive)
            {
                yield return new WaitForFixedUpdate();
                if (HeaviestRigidbody.velocity.magnitude > _minVelocity) yield break;

                RaycastHit hit;
                transform.position = (Physics.Raycast(_hipsTransform.position, Vector3.down, out hit, 10f)) ? hit.point : _hipsTransform.position;
                _hipsTransform.localPosition = _hipsLocalPosition;

                for (int i = 0; i < Colliders.Count; i++)
                {
                    Colliders[i].attachedRigidbody.isKinematic = true;
                    Colliders[i].isTrigger = true;
                    Colliders[i].transform.localRotation = _bonesRotations[i];
                }
                CharacterController.enabled = true;
                if (PlayerController) PlayerController.enabled = true;
                Animator.enabled = true;
                IsActive = false;
            }
        }

        public void AddExplosionForce(float force, Vector3 position, float radius, float upwardModifier)
        {
            foreach (Collider collider in Colliders)
            {
                collider.attachedRigidbody.AddExplosionForce(force, position, radius, 0.5f, ForceMode.Impulse);
            }
        }

        private void FixedUpdate()
        {
            if (IsActive && HeaviestRigidbody.velocity.magnitude <= _minVelocity) StartCoroutine(SetActive(false));
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            if (_hipsTransform.position.y < -20)
            {
                float cmValueX = InputManager.Instance.Cinemachine.m_XAxis.Value;
                float cmValueY = InputManager.Instance.Cinemachine.m_YAxis.Value;
                transform.position = Vector3.up * 20;
                _hipsTransform.localPosition = _hipsLocalPosition;
                InputManager.Instance.Cinemachine.m_YAxis.Value = cmValueY;
                InputManager.Instance.Cinemachine.m_XAxis.Value = cmValueX;
            }

        }

        public void Initialize()
        {

            _colliders.AddRange(GetComponentsInChildren<Collider>());
            _colliders.RemoveAll((x) => x.attachedRigidbody == null);
            CharacterController = GetComponent<CharacterController>();
            Animator = GetComponent<Animator>();
            if (TryGetComponent<PlayerController>(out PlayerController playerController))
            {
                PlayerController = playerController;
            }
            HeaviestRigidbody = _colliders[0].attachedRigidbody;
            for (int i = 0; i < _colliders.Count; i++)
            {
                HeaviestRigidbody = (_colliders[i].attachedRigidbody.mass >= HeaviestRigidbody.mass) ?
                _colliders[i].attachedRigidbody : HeaviestRigidbody;
                _bonesRotations.Add(_colliders[i].gameObject.transform.localRotation);
            }
            _hipsLocalPosition = _hipsTransform.localPosition;
        }
    }
}