 
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    public class SwipeButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {

        public class ButtonSwipeEvent : UnityEvent<Vector2?>
        {

        }

        private ButtonSwipeEvent m_OnClick = new();
        private ButtonSwipeEvent m_OnDrag = new();

        public ButtonSwipeEvent OnSwipeClickEvent
        {
            private set => m_OnClick = value;
            get => m_OnClick;
        }

        public ButtonSwipeEvent OnDragging
        {
            private set => m_OnDrag = value;
            get => m_OnDrag;
        }

        public UnityEvent OnSwipeStarted { private set; get; } = new();

        public UnityEvent OnSwipeEnd { private set; get; } = new();

        private Vector2? _startPosition;
        private Vector2? _last;

        public bool interactable = true;

        public float threshold = 100f;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
            _startPosition = eventData.position;
            OnSwipeStarted.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!interactable) return;
            if (!_startPosition.HasValue) return;
            _last =  eventData.position - _startPosition.Value;
            OnDragging.Invoke(_last);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;
            var dir = _last;
            if (dir != null && dir.Value.sqrMagnitude < threshold)
            {
                dir = null;
            }

            _last = _startPosition = null;
            OnSwipeClickEvent.Invoke(dir);
            OnSwipeEnd.Invoke();
        }
    }