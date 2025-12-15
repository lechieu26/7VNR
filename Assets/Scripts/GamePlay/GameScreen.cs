using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameScreen
{
    private HashSet<KeyCode> InterestedKeys { get; } = new()
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Mouse0,
        KeyCode.Space,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
    };
    private event Action<KeyCode> onKeyPressed;
    private event Action<Vector3> onMousePressed;
    public abstract void Show();
    public abstract void Hide();
    public virtual void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            KeyPressed(KeyCode.LeftArrow);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            KeyPressed(KeyCode.RightArrow);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            KeyPressed(KeyCode.UpArrow);
        }

        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = Input.mousePosition;
            onMousePressed?.Invoke(mousePos);
        }
    }

    public void Subscribe(Action<KeyCode> method)
    {
        onKeyPressed += method;
    }
    public void SubscribeMousePress(Action<Vector3> method)
    {
        onMousePressed += method;
    }
    public void Unsubscribe(Action<KeyCode> method)
    {
        onKeyPressed -= method;
    }
    public void UnsubscribeMousePress(Action<Vector3> method)
    {
        onMousePressed -= method;
    }

    protected void KeyPressed(KeyCode key)
    {
        onKeyPressed?.Invoke(key);
    }

}
