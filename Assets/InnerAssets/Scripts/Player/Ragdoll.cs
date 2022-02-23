using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jumplosion.Scripts.Player
{
    public class Ragdoll : MonoBehaviour
    {
        [SerializeField] private float _minVelocity;
        [SerializeField] private Transform _hipsTransform;
        [SerializeField] private LayerMask _layerMask;
        private Vector3 _hipsLocalPosition;
        private List<Quaternion> _bonesRotations = new List<Quaternion>();
        private List<Vector3> _bonesLocalPositions = new List<Vector3>();
        private List<Collider> _colliders = new List<Collider>();
        public List<Collider> Colliders { get => _colliders; private set => _colliders = value; }
        private CharacterController _characterController;
#nullable enable
        private PlayerController? _playerController;
#nullable disable
        public Rigidbody HeaviestRigidbody { get; private set; }
        public bool IsActive { get; private set; } = false;
        private AnimationController _animationController;

        private void Awake()
        {
            Initialize();
        }

        public IEnumerator SetActive(bool value)
        {
            if (value && !IsActive)
            {
                _characterController.enabled = false;
                _animationController.EnableRagdoll(true);
                if (_playerController) _playerController.enabled = false;
                _colliders.ForEach(collider =>
                {
                    collider.isTrigger = false;
                    if (collider.attachedRigidbody != null)
                        collider.attachedRigidbody.isKinematic = false;
                });
                IsActive = true;
                yield break;
            }
            else if (!value && IsActive)
            {
                yield return new WaitForFixedUpdate();
                if (HeaviestRigidbody.velocity.magnitude > _minVelocity) yield break;

                RaycastHit hit;
                transform.position = (Physics.Raycast(_hipsTransform.position, Vector3.down, out hit, 10f, _layerMask)) ? hit.point : _hipsTransform.position;
                _hipsTransform.localPosition = _hipsLocalPosition;

                for (int i = 0; i < _colliders.Count; i++)
                {
                    _colliders[i].isTrigger = true;
                    if (_colliders[i].attachedRigidbody != null)
                    {
                        _colliders[i].attachedRigidbody.isKinematic = true;
                    }
                    _colliders[i].transform.localRotation = _bonesRotations[i];
                    _colliders[i].transform.localPosition = _bonesLocalPositions[i];
                }
                _characterController.enabled = true;
                if (_playerController) _playerController.enabled = true;
                _animationController.EnableRagdoll(false);
                IsActive = false;
            }
        }

        public void AddExplosionForce(float force, Vector3 position, float radius, float upwardModifier)
        {
            _colliders.ForEach(collider =>
            {
                collider.attachedRigidbody?.AddExplosionForce(force, position, radius, 0.5f, ForceMode.Impulse);
            });
        }

        private void FixedUpdate()
        {
            if (IsActive && HeaviestRigidbody.velocity.magnitude <= _minVelocity) StartCoroutine(SetActive(false));
        }

        //Временный апдейт для перезагрузки сцены и возвпащения игрока и ботов при падении
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
            _animationController = GetComponent<AnimationController>();
            _colliders.AddRange(GetComponentsInChildren<Collider>());
            _colliders.RemoveAll((x) => x.TryGetComponent<CharacterController>(out CharacterController cc) == true);
            _characterController = GetComponent<CharacterController>();
            if (TryGetComponent<PlayerController>(out PlayerController playerController))
            {
                _playerController = playerController;
            }
            for (int i = 0; i < _colliders.Count; i++)
            {
                _bonesRotations.Add(_colliders[i].gameObject.transform.localRotation);
                _bonesLocalPositions.Add(_colliders[i].gameObject.transform.localPosition);
                if (_colliders[i].attachedRigidbody == null) continue;
                if (HeaviestRigidbody == null) HeaviestRigidbody = _colliders[i].attachedRigidbody;
                else
                {
                    HeaviestRigidbody = (_colliders[i].attachedRigidbody.mass >= HeaviestRigidbody.mass) ?
                    _colliders[i].attachedRigidbody : HeaviestRigidbody;
                }
            }
            _hipsLocalPosition = _hipsTransform.localPosition;
        }
    }
}