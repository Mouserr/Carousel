using System.Collections.Generic;
using UnityEngine;

namespace Carousel
{
    public abstract class AbstractCarousel : MonoBehaviour, IMoveable
    {
        [SerializeField]
        protected bool sortOnAwake = true;

        protected bool isInited;
        protected List<GameObject> contentObjects;

        public virtual Transform ContainerTransform
        {
            get { return transform; }
        }

        public virtual void MoveAbsolute(Vector3 absolute)
        {
            Vector3 a = ContainerTransform.InverseTransformPoint(absolute);
            Vector3 b = ContainerTransform.InverseTransformPoint(Vector3.zero);
            MoveRelative(a - b);
        }

        public abstract void Sort();
        public abstract void MoveRelative(Vector3 relative);
        public abstract int GetClosestToCenterIndex(Vector3 centerOffset);
        public abstract Vector3 GetDistanceForCenteringIndex(int centerIndex);

        protected virtual void Awake()
        {
            if (sortOnAwake)
                Sort();
        }

        public virtual void Init(bool forse = false)
        {
			if (isInited && !forse) return;
            isInited = true;
            contentObjects = new List<GameObject>();
            foreach (Transform content in transform)
            {
                contentObjects.Add(content.gameObject);
            }
        }

        public virtual GameObject GetObjectByIndex(int index)
        {
            if (index == -1) return null;
            return contentObjects[index];
        }

        public virtual int GetIndexByObject(GameObject target)
        {
            return contentObjects.FindIndex(x => x == target);
        }
    }
}