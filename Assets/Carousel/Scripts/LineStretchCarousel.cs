using System.Collections.Generic;
using UnityEngine;

namespace Carousel
{
    public class LineStretchCarousel : AbstractCarousel
    {
        [SerializeField]
        protected Vector3 offset = new Vector3(200, 0, 0);
        [SerializeField]
        protected bool considerBounds;
        [SerializeField]
        protected Vector3 boundStart;
        [SerializeField]
        protected Vector3 boundEnd;


        protected Vector3 sumDistanse;
        

        private float sumLength;
        private float boundsLength;
        private Vector3 centerPoint;


        public override void Sort()
        {
            Init();
            for (int i = 0; i < contentObjects.Count; i++)
            {
                contentObjects[i].transform.localPosition = i*offset;
            }
            sumDistanse = contentObjects.Count*offset;
            sumLength = sumDistanse.sqrMagnitude;
            boundsLength = (boundEnd - boundStart).sqrMagnitude;
            centerPoint = boundStart + (boundEnd - boundStart) / 2;
        }

        public override void MoveRelative(Vector3 relative)
        {
            transform.localPosition += relative;
        }

        public override int GetClosestToCenterIndex(Vector3 centerOffset)
        {
            Vector3 delta = centerPoint - transform.localPosition;

            return Mathf.Clamp(Mathf.RoundToInt(delta.sqrMagnitude / offset.sqrMagnitude), 
                0, 
                contentObjects.Count - 1);
        }

        public override Vector3 GetDistanceForCenteringIndex(int centerIndex)
        {
            return centerPoint - (transform.localPosition 
                + GetObjectByIndex(centerIndex).transform.localPosition);
        }
    }
}