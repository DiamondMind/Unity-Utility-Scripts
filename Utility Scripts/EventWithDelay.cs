using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DiamondMind.Prototypes.Tools
{
    public class EventWithDelay : MonoBehaviour
    {
        [Header("---------- Events On Start Settings ----------")]
        public bool triggerOnStart;
        public bool triggerAllOnStart;
        [Tooltip(" Name of single event to trigger on start")]
        public string singleEventName;

        [Header("---------- Events ----------")]
        [SerializeField] private EventWithDelayObject[] events;

        private void Start()
        {
            if (triggerOnStart)
            {
                if (triggerAllOnStart)
                    TriggerAllEvents();
                else
                    TriggerSingleEvent(singleEventName);
            }
        }

        public void TriggerAllEvents()
        {
            foreach (var currentEvent in events)
            {
                StartCoroutine(DoEventWithDelay(currentEvent));
            }
        }

        public void TriggerSingleEvent(string eventName)
        {
            int index = FindEventIndexByName(eventName);
            if (IsValidIndex(index))
            {
                StartCoroutine(DoEventWithDelay(events[index]));
            }
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < events.Length;
        }

        private int FindEventIndexByName(string eventName)
        {
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].eventName == eventName)
                {
                    return i;
                }
            }
            return -1; // Event not found
        }

        IEnumerator DoEventWithDelay(EventWithDelayObject currentEvent)
        {
            yield return new WaitForSeconds(currentEvent.delay);
            currentEvent.onDoEvent.Invoke();
        }

        [System.Serializable]
        public class EventWithDelayObject
        {
            public string eventName;
            public float delay;
            public UnityEvent onDoEvent;
        }
    }

}
