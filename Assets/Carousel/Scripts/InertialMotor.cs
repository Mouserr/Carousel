using UnityEngine;

namespace Carousel
{
    public class InertialMotor : MonoBehaviour
    {
        public enum Movement
        {
            Horizontal,
            Vertical,
            Unrestricted,
            Custom,
        }

        public enum DragEffect
        {
            None,
            Momentum,
            MomentumAndSpring,
        }
        public delegate void OnDragNotification();

        [SerializeField]
        private AbstractCarousel carousel;

        [SerializeField]
        private CarouselCenterOnChild carouselCenterOnChild;

        /// <summary>
        /// Effect to apply when dragging.
        /// </summary>

        public DragEffect DragEffectType = DragEffect.MomentumAndSpring;


        /// <summary>
        /// Type of movement allowed by the scroll view.
        /// </summary>

        public Movement MovementType = Movement.Horizontal;

        /// <summary>
        /// Whether the drag operation will be started smoothly, or if if it will be precise (but will have a noticeable "jump").
        /// </summary>

        public bool SmoothDragStart = true;

        /// <summary>
        /// Whether to use iOS drag emulation, where the content only drags at half the speed of the touch/mouse movement when the content edge is within the clipping area.
        /// </summary>	

        public bool IOSDragEmulation = true;

        /// <summary>
        /// How much momentum gets applied when the press is released after dragging.
        /// </summary>

        public float MomentumAmount = 35f;

        /// <summary>
        /// Custom movement, if the 'movement' field is set to 'Custom'.
        /// </summary>

        public Vector2 CustomMovement = new Vector2(1f, 0f);

       
        /// <summary>
        /// Event callback to trigger when the drag process begins.
        /// </summary>

        public OnDragNotification OnDragStarted;

        /// <summary>
        /// Event callback to trigger when the drag process finished. Can be used for additional effects, such as centering on some object.
        /// </summary>

        public OnDragNotification OnDragFinished;

        /// <summary>
        /// Event callback triggered when the scroll view is moving as a result of momentum in between of OnDragFinished and OnStoppedMoving.
        /// </summary>

        public OnDragNotification OnMomentumMove;

        /// <summary>
        /// Event callback to trigger when the scroll view's movement ends.
        /// </summary>

        public OnDragNotification OnStoppedMoving;

        protected IMoveable moveable;
        protected Transform trans;
        protected Vector3 lastPos;
        protected Plane plane;
        protected bool pressed = false;
        protected Vector3 momentum = Vector3.zero;
        protected float scroll = 0f;
        protected bool shouldMove = false;
        protected int dragID = -10;
        protected Vector2 dragStartOffset = Vector2.zero;
        protected bool dragStarted = false;

        /// <summary>
        /// Could be a carousel assigned on editor 
        /// or founded on Awake by GetComponent 
        /// or setted manually by a script
        /// </summary>

        public IMoveable Moveable
        {
            get { return moveable; }
            set
            {
                if (moveable != value)
                {
                    moveable = value;
                    trans = moveable.ContainerTransform;
                }
            }
        }


        /// <summary>
        /// Whether the scroll view is being dragged.
        /// </summary>

        public bool IsDragging { get { return pressed && dragStarted; } }

        /// <summary>
        /// Current momentum, exposed just in case it's needed.
        /// </summary>

        public Vector3 CurrentMomentum
        {
            get
            {
                return momentum;
            }
            set
            {
                momentum = value;
                shouldMove = true;
            }
        }

        private void Awake()
        {
            carouselCenterOnChild = carouselCenterOnChild ?? GetComponent<CarouselCenterOnChild>();
            carousel = carousel ?? GetComponent<AbstractCarousel>();
            if (carousel != null)
            {
                Moveable = carousel;
            }
        }

        /// <summary>
        /// Disable the spring movement.
        /// </summary>

        public void DisableSpring()
        {
            if (carouselCenterOnChild != null) carouselCenterOnChild.enabled = false;
        }

        /// <summary>
        /// Create a plane on which we will be performing the dragging.
        /// </summary>

        public void Press(bool pressed)
        {
            if (SmoothDragStart && pressed)
            {
                dragStarted = false;
                dragStartOffset = Vector2.zero;
            }

            if (enabled && gameObject.activeInHierarchy)
            {
                if (!pressed && dragID == UICamera.currentTouchID) dragID = -10;
                shouldMove = true;
                this.pressed = pressed;

                if (pressed)
                {
                    // Remove all momentum on press
                    momentum = Vector3.zero;
                    scroll = 0f;

                    // Disable the spring movement
                    DisableSpring();

                    // Remember the hit position
                    lastPos = UICamera.lastHit.point;

                    // Create the plane to drag along
                    plane = new Plane(trans.rotation * Vector3.back, lastPos);

                   if (!SmoothDragStart)
                    {
                        dragStarted = true;
                        dragStartOffset = Vector2.zero;
                        if (OnDragStarted != null) OnDragStarted();
                    }
                }
                else
                {
                    if (DragEffectType != DragEffect.Momentum &&
                        DragEffectType != DragEffect.MomentumAndSpring
                        && carouselCenterOnChild != null)
                    {
                        carouselCenterOnChild.Recenter();
                    }
                }
            }
        }

        /// <summary>
        /// Drag the object along the plane.
        /// </summary>

        public void Drag()
        {
            if (enabled && gameObject.activeInHierarchy && shouldMove)
            {
                if (dragID == -10) dragID = UICamera.currentTouchID;
                UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

                // Prevents the drag "jump". Contributed by 'mixd' from the Tasharen forums.
                if (SmoothDragStart && !dragStarted)
                {
                    dragStarted = true;
                    dragStartOffset = UICamera.currentTouch.totalDelta;
                    if (OnDragStarted != null) OnDragStarted();
                }

                Ray ray = SmoothDragStart ?
                    UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos - dragStartOffset) :
                    UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);

                float dist = 0f;

                if (plane.Raycast(ray, out dist))
                {
                    Vector3 currentPos = ray.GetPoint(dist);
                    Vector3 offset = currentPos - lastPos;
                    lastPos = currentPos;

                    if (offset.x != 0f || offset.y != 0f || offset.z != 0f)
                    {
                        offset = trans.InverseTransformDirection(offset);

                        if (MovementType == Movement.Horizontal)
                        {
                            offset.y = 0f;
                            offset.z = 0f;
                        }
                        else if (MovementType == Movement.Vertical)
                        {
                            offset.x = 0f;
                            offset.z = 0f;
                        }
                        else if (MovementType == Movement.Unrestricted)
                        {
                            offset.z = 0f;
                        }
                        else
                        {
                            offset.Scale((Vector3)CustomMovement);
                        }
                        offset = trans.TransformDirection(offset);
                    }

                    // Adjust the momentum
                    if (DragEffectType == DragEffect.None) momentum = Vector3.zero;
                    else momentum = Vector3.Lerp(momentum, momentum + offset * (0.01f * MomentumAmount), 0.67f);

                    Moveable.MoveAbsolute(offset);
                }
            }
        }
        /// <summary>
        /// Apply the dragging momentum.
        /// </summary>

        void LateUpdate()
        {
            if (!Application.isPlaying) return;
            float delta = Time.fixedDeltaTime;

            if (!shouldMove) return;

            // Apply momentum
            if (!pressed)
            {
                if (momentum.magnitude > 0.01f || scroll != 0f)
                {
                    if (MovementType == Movement.Horizontal)
                    {
                        momentum -= trans.TransformDirection(new Vector3(scroll * 0.05f, 0f, 0f));
                    }
                    else if (MovementType == Movement.Vertical)
                    {
                        momentum -= trans.TransformDirection(new Vector3(0f, scroll * 0.05f, 0f));
                    }
                    else if (MovementType == Movement.Unrestricted)
                    {
                        momentum -= trans.TransformDirection(new Vector3(scroll * 0.05f, scroll * 0.05f, 0f));
                    }
                    else
                    {
                        momentum -= trans.TransformDirection(new Vector3(
                            scroll * CustomMovement.x * 0.05f,
                            scroll * CustomMovement.y * 0.05f, 0f));
                    }
                    scroll = NGUIMath.SpringLerp(scroll, 0f, 20f, delta);

                    // Move the scroll view
                    Vector3 offset = NGUIMath.SpringDampen(ref momentum, 9f, delta);
                    Moveable.MoveAbsolute(offset);

                    

                    if (OnMomentumMove != null)
                        OnMomentumMove();
                }
                else
                {
                    scroll = 0f;
                    momentum = Vector3.zero;

                    if (carouselCenterOnChild != null)
                    {
                        if (carouselCenterOnChild.enabled) return;

                        carouselCenterOnChild.Recenter();
                    }

                    shouldMove = false;
                    if (OnStoppedMoving != null)
                        OnStoppedMoving();
                }
            }
            else
            {
                // Dampen the momentum
                scroll = 0f;
                NGUIMath.SpringDampen(ref momentum, 9f, delta);
            }
        }
    }
}