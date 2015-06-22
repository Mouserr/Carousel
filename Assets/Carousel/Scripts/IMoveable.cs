using UnityEngine;

namespace Carousel
{
    public interface IMoveable
    {
        Transform ContainerTransform { get; }

        void MoveAbsolute(Vector3 absolute);
    }
}