using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;

namespace Jumplosion.Scripts.Player
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance;
        public EventSystem EventSystem { get; private set; }
        public CinemachineInputProvider CameraProvider { get; private set; }
        public bool TouchedUI { get; private set; } = false;
        public Vector3 ActiveTouchPosition { get; private set; } = Vector3.zero;

        public Action<UnityEngine.InputSystem.EnhancedTouch.Touch> TouchBegan;
        public Action<UnityEngine.InputSystem.EnhancedTouch.Touch> TouchMoved;
        public Action<UnityEngine.InputSystem.EnhancedTouch.Touch> TouchEnded;

        private Camera _mainCamera;
        public CinemachineFreeLook Cinemachine { get; set; }
        private UnityEngine.InputSystem.EnhancedTouch.Touch _activeTouch =
        new UnityEngine.InputSystem.EnhancedTouch.Touch();



        private void Awake()
        {
            EnhancedTouchSupport.Enable();
            if (Instance == null) Instance = this;
            else Destroy(this);
            EventSystem = EventSystem.current;
            _mainCamera = Camera.main;
            CameraProvider = _mainCamera.GetComponentInChildren<CinemachineInputProvider>();
            Cinemachine = _mainCamera.GetComponentInChildren<CinemachineFreeLook>();
        }

        //Проверки на касание пришлось сделать в Update, а не с помощью Events,
        //так как событие IsPointerOverGameObject некорректно срабатывает при вызове
        //из других событий InputSystems.
        private void Update()
        {
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count < 1) return;
            _activeTouch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];
            TouchedUI = (EventSystem.IsPointerOverGameObject(_activeTouch.touchId)) ? true : false;
            ActiveTouchPosition = new Vector3(_activeTouch.screenPosition.x, _activeTouch.screenPosition.y, 0);
            switch (_activeTouch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    TouchBegan.Invoke(_activeTouch);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                    TouchMoved.Invoke(_activeTouch);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    TouchEnded.Invoke(_activeTouch);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Ended:
                    TouchEnded.Invoke(_activeTouch);
                    break;
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        public Vector3 GetTouchWorldPosition(Vector3 touchPosition)
        {
            RaycastHit hit;
            Ray ray = _mainCamera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                return hit.point;
            else { TouchedUI = true; return Vector3.zero; }
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }
    }
}

