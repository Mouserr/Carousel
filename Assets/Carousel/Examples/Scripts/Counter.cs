using System;
using Carousel;
using UnityEngine;
using System.Collections;

public class Counter : MonoBehaviour
{
    [SerializeField]
    private CarouselCenterOnChild centerOnChild;
    [SerializeField]
    private AbstractCarousel carousel;

    private UILabel label;

	// Use this for initialization
	void Start () {
	    centerOnChild.onFinished += OnFinished;
	    label = GetComponent<UILabel>();
        centerOnChild.CenterOn(carousel.GetObjectByIndex(3));
	}

    private void OnFinished()
    {
        updateLabel();
    }

    private void updateLabel()
    {
        label.text = carousel.GetIndexByObject(centerOnChild.CenteredObject).ToString();
    }
}
