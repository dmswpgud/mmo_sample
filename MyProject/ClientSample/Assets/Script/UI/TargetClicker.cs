using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetClicker : MonoBehaviour
{
    private Unit Unit;
    private Transform TargetTrf;
    private bool isPressed = false;
    public Text PlayerName;
    private Action<Unit> OnSetTargetUnit;
    
    public void SetTrackingTarget(Unit unit, Transform targetTransform, Action<Unit> onSetTarget)
    {
        var canvas = GameObject.Find("Canvas");
        transform.SetParent(canvas.transform);
        Unit = unit;
        TargetTrf = targetTransform;
        PlayerName.text = unit.DATA.name;
        OnSetTargetUnit = onSetTarget;
    }

    public void OnEnterTarget()
    {
        PlayerName.gameObject.SetActive(true);
    }

    public void OnExitTarget()
    {
        if (!isPressed)
        {
            PlayerName.gameObject.SetActive(false);    
            OnSetTargetUnit?.Invoke(null);
        }
    }

    public void OnPointDown()
    {
        isPressed = true;
        PlayerName.gameObject.SetActive(true);

        OnSetTargetUnit?.Invoke(Unit);
    }

    public void OnPointUpTarget()
    {
        isPressed = false;
        PlayerName.gameObject.SetActive(false);
        
        OnSetTargetUnit?.Invoke(null);
    }
    
    void Update()
    {
        if (TargetTrf == null)
        {
            return;
        }
        
        transform.position = Camera.main.WorldToScreenPoint(TargetTrf.position);
    }
}
