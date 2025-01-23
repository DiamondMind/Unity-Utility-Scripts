using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothScroll : MonoBehaviour
{
    public Scrollbar scrollbar;
    public float speed = 1.0f;
    [Range(2, 10)]public int nosOfSteps = 5;

    float stepDistance;
    float currentValue = 0f;
    bool isScrolling;

    private void Start()
    {
        scrollbar.value = 0;
        stepDistance = 1.0f / (nosOfSteps - 1);
    }

    private IEnumerator SmoothChangeCoroutine(float target)
    {
        //Debug.Log("Smooth changing from " + scrollbar.value + " to " + target + " over " + speed + " seconds.");
        isScrolling = true;
        currentValue = target;
        float elapsedTime = 0f;
        float startValue = scrollbar.value;

        while (elapsedTime < speed)
        {
            scrollbar.value = Mathf.Lerp(startValue, target, elapsedTime / speed);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        scrollbar.value = target;
        isScrolling = false;
    }

    public void SmoothChangeScrollbarValue(float targetValue)
    {
        StartCoroutine(SmoothChangeCoroutine(targetValue));
    }

    public void ScrollLeft()
    {
        if (isScrolling || currentValue == 0f)
            return;

        StartCoroutine(SmoothChangeCoroutine(currentValue - stepDistance));
    }

    public void ScrollRight()
    {
        if (isScrolling || currentValue == 1f)
            return;

        StartCoroutine(SmoothChangeCoroutine(currentValue + stepDistance));
    }
}
