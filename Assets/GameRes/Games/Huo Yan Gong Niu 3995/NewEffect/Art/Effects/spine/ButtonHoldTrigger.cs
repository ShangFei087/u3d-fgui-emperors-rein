//============================================
// Describe: 
// Author:XC
// 2025-03-05 18:07:22:2025-03-05 18:07:22
//==============================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonHoldTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [System.Serializable]
    public class MyEvent : UnityEvent { }

    public MyEvent onHoldEvents;   // 持续触发的事件列表
    public MyEvent onReleaseEvents; // 松开触发的事件列表
    public float holdInterval = 0.1f; // 触发间隔
    public float timer = 0f;

    private bool isHolding = false;

    void Update()
    {
        if (isHolding)
        {
            timer += Time.deltaTime;
            if (timer >= holdInterval)
            {
                onHoldEvents.Invoke();
                //isHolding = false;
                timer = 0f;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        onReleaseEvents.Invoke(); // 松开时触发
        timer = 0f;
    }
}