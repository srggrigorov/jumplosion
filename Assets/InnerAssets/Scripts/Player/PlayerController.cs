using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Jumplosion.Scripts
{
    [RequireComponent(typeof(Ragdoll), typeof(CharacterController), typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private GameObject _target; //Цель с большим радиусом
        [SerializeField] private Transform _targetCenter;
        [SerializeField] private float _targetCenterRadius;
        [SerializeField] private Transform _shootPoint; //Место вылета снаряда
        [SerializeField] private Projectile _projectile;
        [SerializeField] private Toggle _cameraToggle;
        [SerializeField] private TMP_Text _cameraToggleText;
        private Action<UnityEngine.InputSystem.EnhancedTouch.Touch> TouchBegan;
        private Action<UnityEngine.InputSystem.EnhancedTouch.Touch> TouchMoved;
        private Action<UnityEngine.InputSystem.EnhancedTouch.Touch> TouchEnded;
        private bool _readyToShoot = false;
        private Ragdoll _ragdoll;
        private InputManager _inputManager;

        private void Awake()
        {
            _inputManager = InputManager.Instance;
            _ragdoll = GetComponent<Ragdoll>();

        }
        private void Start()
        {
            _cameraToggle.onValueChanged.AddListener((value) =>
            {
                _inputManager.CameraProvider.enabled = value;
                _cameraToggleText.text = (value) ? "Камера" : "Стрельба";
            });


            TouchBegan = touch => Move_target();

            TouchMoved = touch => Move_target();

            TouchEnded = touch =>
            {
                Move_target();
                if (_target.activeInHierarchy) _target.SetActive(false);
                ShootProjectile(_projectile.ProjectileGameObject);
            };

            _inputManager.TouchBegan += TouchBegan;
            _inputManager.TouchMoved += TouchMoved;
            _inputManager.TouchEnded += TouchEnded;

        }


        private void OnDestroy()
        {
            _cameraToggle.onValueChanged.RemoveAllListeners();
            _inputManager.TouchBegan -= TouchBegan;
            _inputManager.TouchMoved -= TouchMoved;
            _inputManager.TouchEnded -= TouchEnded;
        }

        private void Move_target()
        {
            if (_inputManager.TouchedUI || _cameraToggle.isOn || !_characterController.enabled)
            { _target.SetActive(false); _readyToShoot = false; return; }
            Vector3 _targetPosition = _inputManager.GetTouchWorldPosition(_inputManager.ActiveTouchPosition);
            transform.LookAt(new Vector3(_targetPosition.x, transform.position.y, _targetPosition.z));
            _target.transform.position = _targetPosition;
            _target.transform.localScale = Vector3.one * 2 * _projectile.ExplosionRadius;
            _targetCenter.localScale = Vector3.one * 2 * _targetCenterRadius / _target.transform.localScale.x;
            if (!_target.activeInHierarchy) _target.SetActive(true);
            _readyToShoot = true;
        }

        public void ShootProjectile(GameObject _projectile)
        {
            if (!_readyToShoot) return;
            GameObject newProjectile = Instantiate(_projectile, _shootPoint.position, _shootPoint.rotation);
            newProjectile.transform.LookAt(_target.transform.position);
            _readyToShoot = false;
        }
    }
}
