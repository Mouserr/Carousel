using System;
using UnityEngine;

namespace Assets.Scripts.Scenes.MainScene.Subscenes.SelectSkinPanel
{
    public class CarouselCenterOnChild : MonoBehaviour
    {
        public delegate void OnFinished();
        public delegate void OnCenterCallback(GameObject gameObject);

        [SerializeField]
        private CyclicCarousel3D carousel;
        [SerializeField]
        private CarouselMotor carouselMotor;
        /// <summary>
        /// The strength of the spring.
        /// </summary>

        public float springStr = 8f;

        /// <summary>
        /// Callback to be triggered when the centering operation completes.
        /// </summary>

        public OnFinished onFinished;

        /// <summary>
        /// Callback triggered whenever the script begins centering on a new child object.
        /// </summary>

        public OnCenterCallback onCenter;

        private int targetIndex;
        private GameObject centeredObject;

        /// <summary>
        /// Game object that the carousel is currently centered on.
        /// </summary>

        public GameObject CenteredObject { get { return centeredObject; } }

        public void Recenter()
        {
            // Offset this value by the momentum
            Vector3 momentum = carouselMotor.currentMomentum * carouselMotor.momentumAmount;
            Vector3 moveDelta = NGUIMath.SpringDampen(ref momentum, 9f, 2f);
            Vector3 centerOffset = - moveDelta * 0.001f; // Magic number based on what "feels right"
            targetIndex = carousel.GetClosestToCenterIndex(Vector3.zero);
            centeredObject = carousel.GetObjectByIndex(targetIndex);
            enabled = true;
            // Notify the listener
            if (onCenter != null) onCenter(centeredObject);
        }

        public void CenterOn(GameObject newCenter)
        {
            targetIndex = carousel.GetIndexByObject(newCenter);
            if (targetIndex == -1)
            {
                Debug.LogError("Can't center on " + newCenter, newCenter);
                return;
            }
            centeredObject = newCenter;
            enabled = true;
        }

        protected virtual void AdvanceTowardsPosition()
        {
            float delta = Time.fixedDeltaTime;

            bool trigger = false;
            float before = carousel.GetDistanceToIndex(targetIndex);
            float after = NGUIMath.SpringLerp(0, before, springStr, delta);
           
            if (Math.Abs(after) < 0.001)
            {
                after = 0;
                enabled = false;
                trigger = true;
            }
            carousel.Move(after);

            if (trigger && onFinished != null)
            {
                onFinished();
            }
        }

        /// <summary>
        /// Advance toward the target position.
        /// </summary>

        void Update()
        {
            AdvanceTowardsPosition();
        }
    }
}