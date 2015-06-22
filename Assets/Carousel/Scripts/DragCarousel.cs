using UnityEngine;

namespace Carousel
{
    public class DragCarousel : MonoBehaviour
    {
        public InertialMotor Carousel;

        /// <summary>
        /// Create a plane on which we will be performing the dragging.
        /// </summary>

        void OnPress(bool pressed)
        {
            if (Carousel && enabled && gameObject.activeInHierarchy)
            {
                Carousel.Press(pressed);
            }
        }

        void OnDrag(Vector2 delta)
        {
            if (Carousel && enabled && gameObject.activeInHierarchy)
            {
                Carousel.Drag();
            }
        }

      
    }
}