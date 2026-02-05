using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace DiamondMind.Prototypes.Tools
{
    public class ToggleMover : MonoBehaviour
    {
        [Header("---------- References ----------")]
        [Tooltip("The target RectTransform to move")]
        [SerializeField] private RectTransform targetRect;
        [Tooltip("The button that triggers the toggle")]
        [SerializeField] private Button button;
        [Tooltip("The message for the SoundManager")]
        [SerializeField] private string message;

        [Header("---------- Positions ----------")]
        [Tooltip("Is the target currently at position A?")]
        [SerializeField] private bool isAtPositionA = true;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private Vector2 positionA;
        [SerializeField] private Vector2 positionB;

        [Header("---------- Events ----------")]
        public UnityEvent onMoveComplete;

        bool isMoving = false;



        public void TogglePosition()
        {
            if (isMoving)
                return;

            Vector2 startPosition = isAtPositionA ? positionA : positionB;
            Vector2 targetPosition = isAtPositionA ? positionB : positionA;

            StartCoroutine(MoveRect(startPosition, targetPosition));
        }

        private IEnumerator MoveRect(Vector2 start, Vector2 target)
        {
            isMoving = true;
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveDuration);

                targetRect.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            targetRect.anchoredPosition = target;
            isAtPositionA = !isAtPositionA;
            isMoving = false;
        }

    }
}
