using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

namespace Jumplosion.Scripts.Player
{
    [RequireComponent(typeof(Animator), typeof(RigBuilder))]
    public class AnimationController : MonoBehaviour
    {
        private Animator _animator;
        [SerializeField] private Rig _aimingRig;
        [SerializeField] private GameObject _weapon;
        [SerializeField] private Transform _weaponRagdollTransform;
        [SerializeField] private Transform _weaponPivot;
        [SerializeField] private Vector3 _weaponPivotLocalPos;
        private int _isAimingHash;
        private bool _aimingEnabled = false;
        [Space(10), Header("Локальные позиции оружия")]
        [SerializeField] private Transform _weaponAimPos;
        [SerializeField] private Vector3 _straightLocalPos;
        [SerializeField] private Vector3 _downLocalPos;
        [SerializeField] private Vector3 _upLocalPos;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _isAimingHash = Animator.StringToHash("IsAiming");
        }

        public void EnableAiming(bool value)
        {
            if (_aimingEnabled == value) return;
            if (_animator.GetBool(_isAimingHash) != value)
                _animator.SetBool(_isAimingHash, value);
            float targetValue = value ? 1 : 0;
            if (!_aimingRig) return;
            DOVirtual.Float
            (
                _aimingRig.weight,
                targetValue,
                Mathf.Abs(_aimingRig.weight - targetValue) * 0.2f,
                (weight) => _aimingRig.weight = weight
            );
            _aimingEnabled = value;
        }

        public void EnableRagdoll(bool value)
        {
            Transform weaponTransform = _weapon.transform;
            if (value && weaponTransform.parent == _weaponPivot)
            {
                weaponTransform.SetParent(_weaponRagdollTransform);
                weaponTransform.localPosition = Vector3.zero;
            }
            else if (!value && weaponTransform.parent == _weaponRagdollTransform)
            {
                weaponTransform.SetParent(_weaponPivot);
                weaponTransform.localPosition = _weaponPivotLocalPos;
                weaponTransform.localEulerAngles = Vector3.zero;
            }
            _animator.enabled = !value;
        }

//Update ниже нужен для корректного распложения РПГ на плече игрока
        private void Update()
        {
            Transform aimPosTransform = _weaponAimPos;
            if (DOTween.IsTweening(aimPosTransform)) return;
            float weaponAngle = _weaponPivot.eulerAngles.x;
            if (weaponAngle >= 40 && weaponAngle <= 80)
            {
                if (aimPosTransform.localPosition != _downLocalPos)
                    aimPosTransform.DOLocalMove(_downLocalPos, 0.15f);
            }
            else if (weaponAngle <= 330 && weaponAngle >= 270)
            {
                if (aimPosTransform.localPosition != _upLocalPos)
                    aimPosTransform.DOLocalMove(_upLocalPos, 0.15f);
            }
            else
            {
                if (aimPosTransform.localPosition != _straightLocalPos)
                    aimPosTransform.DOLocalMove(_straightLocalPos, 0.15f);
            }
        }
    }
}
