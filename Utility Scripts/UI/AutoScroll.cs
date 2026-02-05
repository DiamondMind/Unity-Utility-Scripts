using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 0.5f;

    private bool isAutoScrolling;

    public float startDelay = 2.0f;  
    public float wrapDelay = 2.0f;

    private void Start()
    {
        scrollRect.verticalNormalizedPosition = 1;
    }

    void Update()
    {
        if (isAutoScrolling)
        {
            // Calculate the new vertical position based on time and speed
            float newVerticalPosition = scrollRect.verticalNormalizedPosition - scrollSpeed * Time.deltaTime;

            // If the scroll position is less than 0, set it to 1 (wrap around)
            if (newVerticalPosition < 0f)
            {
                newVerticalPosition = 1f;

                Invoke(nameof(StartAutoScrollWithDelay), wrapDelay);
            }

            scrollRect.verticalNormalizedPosition = newVerticalPosition;
        }
    }

    public void StartScrolling()
    {
        Invoke(nameof(StartAutoScrollWithDelay), startDelay);
    }

    void StartAutoScrollWithDelay()
    {
        isAutoScrolling = true;
    }

    public void StopAutoScroll()
    {
        isAutoScrolling = false;
        scrollRect.verticalNormalizedPosition = 1;

    }
}
