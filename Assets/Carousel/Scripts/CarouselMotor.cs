using UnityEngine;

namespace Assets.Scripts.Scenes.MainScene.Subscenes.SelectSkinPanel
{
    public class CarouselMotor : MonoBehaviour
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
        private CyclicCarousel3D carousel;

        [SerializeField]
        private CarouselCenterOnChild carouselCenterOnChild;

        /// <summary>
        /// Effect to apply when dragging.
        /// </summary>

        public DragEffect dragEffect = DragEffect.MomentumAndSpring;


        /// <summary>
        /// Type of movement allowed by the scroll view.
        /// </summary>

        public Movement movement = Movement.Horizontal;

        /// <summary>
        /// Whether the drag operation will be started smoothly, or if if it will be precise (but will have a noticeable "jump").
        /// </summary>

        public bool smoothDragStart = true;

        /// <summary>
        /// Whether to use iOS drag emulation, where the content only drags at half the speed of the touch/mouse movement when the content edge is within the clipping area.
        /// </summary>	

        public bool iOSDragEmulation = true;

        /// <summary>
        /// How much momentum gets applied when the press is released after dragging.
        /// </summary>

        public float momentumAmount = 35f;

        /// <summary>
        /// Custom movement, if the 'movement' field is set to 'Custom'.
        /// </summary>

        public Vector2 customMovement = new Vector2(1f, 0f);


        /// <summary>
        /// Event callback to trigger when the drag process begins.
        /// </summary>

        public OnDragNotification onDragStarted;

        /// <summary>
        /// Event callback to trigger when the drag process finished. Can be used for additional effects, such as centering on some object.
        /// </summary>

        public OnDragNotification onDragFinished;

        /// <summary>
        /// Event callback triggered when the scroll view is moving as a result of momentum in between of OnDragFinished and OnStoppedMoving.
        /// </summary>

        public OnDragNotification onMomentumMove;

        /// <summary>
        /// Event callback to trigger when the scroll view's movement ends.
        /// </summary>

        public OnDragNotification onStoppedMoving;

        protected Transform mTrans;
        protected Vector3 mLastPos;
        protected Plane mPlane;
        protected bool mPressed = false;
        protected Vector3 mMomentum = Vector3.zero;
        protected float mScroll = 0f;
        protected bool mShouldMove = false;
        protected int mDragID = -10;
        protected Vector2 mDragStartOffset = Vector2.zero;
        protected bool mDragStarted = false;

        /// <summary>
        /// Whether the scroll view is being dragged.
        /// </summary>

        public bool isDragging { get { return mPressed && mDragStarted; } }

        /// <summary>
        /// Current momentum, exposed just in case it's needed.
        /// </summary>

        public Vector3 currentMomentum
        {
            get
            {
                return mMomentum;
            }
            set
            {
                mMomentum = value;
                mShouldMove = true;
            }
        }

        private void Awake()
        {
            mTrans = carousel.transform;
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
            if (smoothDragStart && pressed)
            {
                mDragStarted = false;
                mDragStartOffset = Vector2.zero;
            }

            if (enabled && gameObject.activeInHierarchy)
            {
                if (!pressed && mDragID == UICamera.currentTouchID) mDragID = -10;
                mShouldMove = true;
                mPressed = pressed;

                if (pressed)
                {
                    // Remove all momentum on press
                    mMomentum = Vector3.zero;
                    mScroll = 0f;

                    // Disable the spring movement
                    DisableSpring();

                    // Remember the hit position
                    mLastPos = UICamera.lastHit.point;

                    // Create the plane to drag along
                    mPlane = new Plane(carousel.transform.rotation * Vector3.back, mLastPos);

                   if (!smoothDragStart)
                    {
                        mDragStarted = true;
                        mDragStartOffset = Vector2.zero;
                        if (onDragStarted != null) onDragStarted();
                    }
                }
                else
                {
                    if (dragEffect != DragEffect.Momentum &&
                        dragEffect != DragEffect.MomentumAndSpring)
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
            if (enabled && gameObject.activeInHierarchy && mShouldMove)
            {
                if (mDragID == -10) mDragID = UICamera.currentTouchID;
                UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

                // Prevents the drag "jump". Contributed by 'mixd' from the Tasharen forums.
                if (smoothDragStart && !mDragStarted)
                {
                    mDragStarted = true;
                    mDragStartOffset = UICamera.currentTouch.totalDelta;
                    if (onDragStarted != null) onDragStarted();
                }

                Ray ray = smoothDragStart ?
                    UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos - mDragStartOffset) :
                    UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);

                float dist = 0f;

                if (mPlane.Raycast(ray, out dist))
                {
                    Vector3 currentPos = ray.GetPoint(dist);
                    Vector3 offset = currentPos - mLastPos;
                    mLastPos = currentPos;

                    if (offset.x != 0f || offset.y != 0f || offset.z != 0f)
                    {
                        offset = mTrans.InverseTransformDirection(offset);

                        if (movement == Movement.Horizontal)
                        {
                            offset.y = 0f;
                            offset.z = 0f;
                        }
                        else if (movement == Movement.Vertical)
                        {
                            offset.x = 0f;
                            offset.z = 0f;
                        }
                        else if (movement == Movement.Unrestricted)
                        {
                            offset.z = 0f;
                        }
                        else
                        {
                            offset.Scale((Vector3)customMovement);
                        }
                        offset = mTrans.TransformDirection(offset);
                    }

                    // Adjust the momentum
                    if (dragEffect == DragEffect.None) mMomentum = Vector3.zero;
                    else mMomentum = Vector3.Lerp(mMomentum, mMomentum + offset * (0.01f * momentumAmount), 0.67f);

                    carousel.MoveAbsolute(offset);
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

            if (!mShouldMove) return;

            // Apply momentum
            if (!mPressed)
            {
                if (mMomentum.magnitude > 0.01f || mScroll != 0f)
                {
                    if (movement == Movement.Horizontal)
                    {
                        mMomentum -= mTrans.TransformDirection(new Vector3(mScroll * 0.05f, 0f, 0f));
                    }
                    else if (movement == Movement.Vertical)
                    {
                        mMomentum -= mTrans.TransformDirection(new Vector3(0f, mScroll * 0.05f, 0f));
                    }
                    else if (movement == Movement.Unrestricted)
                    {
                        mMomentum -= mTrans.TransformDirection(new Vector3(mScroll * 0.05f, mScroll * 0.05f, 0f));
                    }
                    else
                    {
                        mMomentum -= mTrans.TransformDirection(new Vector3(
                            mScroll * customMovement.x * 0.05f,
                            mScroll * customMovement.y * 0.05f, 0f));
                    }
                    mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

                    // Move the scroll view
                    Vector3 offset = NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
                    carousel.MoveAbsolute(offset);

                    

                    if (onMomentumMove != null)
                        onMomentumMove();
                }
                else
                {
                    mScroll = 0f;
                    mMomentum = Vector3.zero;

                    if (carouselCenterOnChild != null 
                        && carouselCenterOnChild.enabled) return;

                    if (carouselCenterOnChild != null)
                        carouselCenterOnChild.Recenter();

                    mShouldMove = false;
                    if (onStoppedMoving != null)
                        onStoppedMoving();
                }
            }
            else
            {
                // Dampen the momentum
                mScroll = 0f;
                NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
            }
        }
    }
}