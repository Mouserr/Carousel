using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carousel
{
    public class CyclicCarousel3D : AbstractCarousel
    {
        [SerializeField]
        private float radius = 330;

        [SerializeField]
        private bool clockwiseOrder = false;

        private float firstPlaneZ = 0;
        private float lastPlaneZ = 1000;

        private float sumLength;
        private float offset;

        private float firstObjectShift;

		[ContextMenu("Sort")]
        public override void Sort()
        {
            Init();
            offset = radius * Mathf.PI / 2;
            sumLength = offset * contentObjects.Count;
            lastPlaneZ = offset * ((contentObjects.Count - 2) / 2 - 1);
            if (lastPlaneZ <= 0)
                lastPlaneZ = 0.001f;
            firstObjectShift = 0;
            for (int i = 0; i < contentObjects.Count; i++)
            {
                contentObjects[i].transform.localPosition = new Vector3(radius, 0, 0);
            }
            sortByFirst();
        }


        public override void MoveRelative(Vector3 relative)
        {
            Move(relative.x);
        }

        public void Move(float delta)
        {
            if (Math.Abs(delta) >= Math.Abs(offset))
            {
                moveObject(contentObjects[0], delta);
                sortByFirst();
            }
            else
            {
                foreach (var contentObject in contentObjects)
                {
                    moveObject(contentObject, delta);
                }
            }

            firstObjectShift += delta;
            if (firstObjectShift < 0)
            {
                firstObjectShift += (Mathf.Floor(-firstObjectShift / sumLength) + 1) * sumLength;
            }
            else
            {
                firstObjectShift %= sumLength;
            }
        }

        public override int GetClosestToCenterIndex(Vector3 centerOffset)
        {
            Vector3 relativeOffset = transform.InverseTransformPoint(centerOffset);
            return getClosestToCenterIndex(relativeOffset.x);
        }

        public override Vector3 GetDistanceForCenteringIndex(int centerIndex)
        {
            int orderCoef = (clockwiseOrder ? 1 : -1);
            float objectShift = firstObjectShift + orderCoef * offset * centerIndex;

            float delta1 = offset - objectShift;
            float delta2 = orderCoef * sumLength - objectShift + offset;
            float shortestDelta = delta1;
            if (Math.Abs(delta1) > Math.Abs(delta2))
            {
                shortestDelta = delta2;
            }
            return new Vector3(shortestDelta, 0);
        }

        private void sortByFirst()
        {
            for (int i = 1; i < contentObjects.Count; i++)
            {
                contentObjects[i].transform.localPosition = contentObjects[i - 1].transform.localPosition;
                moveObject(contentObjects[i], (clockwiseOrder ? 1 : -1) * offset);
            }
        }

        private int getClosestToCenterIndex(float centerOffset)
        {
            if (!isInited) return -1;
            float centerShift = offset - firstObjectShift;
            if (centerShift < 0)
            {
                centerShift += (Mathf.Floor(-centerShift / sumLength) + 1) * sumLength;
            }

            int number = Mathf.RoundToInt(centerShift / offset);
            number %= contentObjects.Count;
            return (clockwiseOrder ? number : (contentObjects.Count - number) % contentObjects.Count);
        }

        private void moveObject(GameObject movingObject, float delta)
        {
            int prevStep = 0;
            while (Math.Abs(delta) >= float.Epsilon)
            {
                int deltaSign = Math.Sign(delta);
                int xSign = Math.Sign(movingObject.transform.localPosition.x);
                if ((movingObject.transform.localPosition.z > firstPlaneZ
                        && movingObject.transform.localPosition.z < lastPlaneZ)
                    || (movingObject.transform.localPosition.z == firstPlaneZ
                        && xSign != deltaSign)
                     || (movingObject.transform.localPosition.z == lastPlaneZ
                        && xSign == deltaSign))
                {
                    if (movingObject.transform.localPosition.x <= 0)
                    {
                        if (prevStep == 1)
                        {
                            //DebugController.LogError("cycle on " + prevStep);
                            return;
                        }
                        prevStep = 1;
                        delta = moveStraight(movingObject, delta);
                    }
                    else
                    {
                        if (prevStep == 2)
                        {
                            //DebugController.LogError("cycle on " + prevStep);
                            return;
                        }
                        prevStep = 2;
                        delta = -moveStraight(movingObject, -delta);

                    }
                }
                else
                {
                    if (movingObject.transform.localPosition.z <= firstPlaneZ)
                    {
                        if (prevStep == 3)
                        {
                            //DebugController.LogError("cycle on " + prevStep);
                            return;
                        }
                        prevStep = 3;
                        delta = moveCircle(movingObject, delta, true);
                    }
                    else
                    {
                        if (prevStep == 4)
                        {
                            //DebugController.LogError("cycle on " + prevStep);
                            return;
                        }
                        prevStep = 4;
                        delta = moveCircle(movingObject, delta, false);
                    }
                }
            }
        }

        private float moveCircle(GameObject movingObject, float delta, bool firstPlane)
        {
            float circleCoef = radius * (!firstPlane && contentObjects.Count % 2 != 0 ? 1.5f : 1);
            float deltaAngel = delta / circleCoef;

            float cos = movingObject.transform.localPosition.x / radius;
            float currentAngel = Mathf.Acos(Mathf.Clamp(cos, -1, 1));

            if (!firstPlane)
            {
                currentAngel = -currentAngel;
            }

            float destAngel = currentAngel + deltaAngel;
            if (firstPlane)
            {
                destAngel = Mathf.Clamp(destAngel, 0, Mathf.PI);
            }
            else
            {
                destAngel = Mathf.Clamp(destAngel, -Mathf.PI, 0);
            }

            movingObject.transform.localPosition = new Vector3(
                Mathf.Clamp(radius * Mathf.Cos(destAngel), -radius, radius),
                movingObject.transform.localPosition.y,
                (firstPlane ? firstPlaneZ : lastPlaneZ)
                    - radius * Mathf.Sin(destAngel));

            return (deltaAngel - (destAngel - currentAngel)) * circleCoef;
        }

        private float moveStraight(GameObject movingObject, float delta)
        {
            float current = movingObject.transform.localPosition.z;
            float dest = current + delta;
            if (dest < firstPlaneZ)
            {
                dest = firstPlaneZ;
            }
            else if (dest > lastPlaneZ)
            {
                dest = lastPlaneZ;
            }

            movingObject.transform.localPosition =
                  new Vector3(movingObject.transform.localPosition.x,
                      movingObject.transform.localPosition.y,
                      dest);

            return delta - (dest - current);
        }
    }
}