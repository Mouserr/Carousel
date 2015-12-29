using System.Collections.Generic;
using UnityEngine;

namespace Carousel
{
	public class LineStretchCarousel : AbstractCarousel
	{
		[SerializeField]
		private Vector3 offset = new Vector3(200, 0, 0);
		[SerializeField]
		private Vector3 centerPoint = Vector3.zero;

		protected Vector3 sumDistanse;
		
		public override void Sort()
		{
			Init();
			for (int i = 0; i < contentObjects.Count; i++)
			{
				contentObjects[i].transform.localPosition = i * offset;
			}
			sumDistanse = contentObjects.Count * offset;
		}

		public override void MoveRelative(Vector3 relative)
		{
			transform.localPosition += relative;
		}

		public override int GetClosestToCenterIndex(Vector3 centerOffset)
		{
			Vector3 delta = centerPoint - transform.localPosition;
			if (Vector3.Dot(offset, delta) < 0)
			{
				return 0;
			}

			float pos = delta.magnitude / offset.magnitude;
			return Mathf.Clamp(Mathf.RoundToInt(pos),
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