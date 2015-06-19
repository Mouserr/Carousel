using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Scenes.MainScene.Subscenes.SelectSkinPanel
{
    public class CyclicCarousel3D : MonoBehaviour
    {
        [SerializeField]
        private float frontRadius = 330;

        private float firstPlaneZ = 0;
        private float lastPlaneZ = 1000;
        
        private bool isInited;
        private float sumLength;
        private float offset;

        private List<GameObject> contentObjects; 
        private float firstObjectShift;

        void Start()
        {
            Sort();
        }

        public Coroutine Sort(float? newRadius = null)
        {
            Init();
            if (newRadius.HasValue)
            {
                this.frontRadius = newRadius.Value;
            }
            offset = this.frontRadius * Mathf.PI / 2;
            sumLength = offset*contentObjects.Count;
            lastPlaneZ = offset * ((contentObjects.Count - 2) / 2 - 1);
            if (lastPlaneZ < 0)
                lastPlaneZ = 50;
            firstObjectShift = 0;

            for (int i = 0; i < contentObjects.Count; i++)
            {
                contentObjects[i].transform.localPosition = new Vector3(frontRadius, 0, 0);
            }
            sortByFirst();

            return null;
        }

     

        public void MoveAbsolute(Vector3 absolute)
        {
            Vector3 a = transform.InverseTransformPoint(absolute);
            Vector3 b = transform.InverseTransformPoint(Vector3.zero);
            MoveRelative(a - b);
        }

        public void MoveRelative(Vector3 relative)
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

        public int GetClosestToCenterIndex(Vector3 centerOffset)
        {
            Vector3 relativeOffset = transform.InverseTransformPoint(centerOffset);
            return getClosestToCenterIndex(relativeOffset.x);
        }

        public float GetDistanceToIndex(int centerIndex)
        {
            float objectShift = firstObjectShift + offset*centerIndex;

            float delta1 = offset - objectShift;
            float delta2 = sumLength - objectShift + offset;
            float shortestDelta = delta1;
            if (Math.Abs(delta1) > Math.Abs(delta2))
            {
                shortestDelta = delta2;
            }
            return shortestDelta;
        }

        public GameObject GetObjectByIndex(int index)
        {
            if (index == -1) return null;
            return contentObjects[index];
        }

        public int GetIndexByObject(GameObject target)
        {
            return contentObjects.FindIndex(x => x == target);
        }

        private void sortByFirst()
        {
            for (int i = 1; i < contentObjects.Count; i++)
            {
                contentObjects[i].transform.localPosition = contentObjects[i - 1].transform.localPosition;
                moveObject(contentObjects[i], offset);
            }
        }

        private int getClosestToCenterIndex(float centerOffset)
        {
            if (!isInited) return -1;
            float centerShift = offset - firstObjectShift;
            if (centerShift < 0)
            {
                centerShift += (Mathf.Floor(-centerShift/sumLength) + 1)*sumLength;
            }

            int number = Mathf.RoundToInt(centerShift/offset);
            number %= contentObjects.Count;
            return number;
        }

        private void Init()
        {
            if (isInited) return;
            isInited = true;
            contentObjects = new List<GameObject>();
            foreach (Transform content in transform)
            {
                contentObjects.Add(content.gameObject);
            }
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
            float circleCoef = frontRadius*(!firstPlane && contentObjects.Count%2 != 0 ? 1.5f : 1);
            float deltaAngel = delta / circleCoef;

            float cos = movingObject.transform.localPosition.x / frontRadius;
            float currentAngel = Mathf.Acos(Mathf.Clamp(cos, -1, 1));
          
            if (!firstPlane)
            {
                currentAngel = - currentAngel;
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
                Mathf.Clamp(frontRadius * Mathf.Cos(destAngel), -frontRadius, frontRadius),
                movingObject.transform.localPosition.y,
                (firstPlane ? firstPlaneZ : lastPlaneZ) 
                    - frontRadius * Mathf.Sin(destAngel));

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