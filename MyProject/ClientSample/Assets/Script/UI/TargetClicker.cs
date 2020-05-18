using System;
using UnityEngine;

public class TargetClicker : MonoBehaviour
{
    private Unit Unit;
    private Transform TargetTrf;
    private bool isPressed = false;
    private Action<Unit> OnSetTargetUnit;
    
    public void SetTrackingTarget(Unit unit, Transform targetTransform, Action<Unit> onSetTarget)
    {
        var canvas = GameObject.Find("Canvas");
        transform.SetParent(canvas.transform);
        Unit = unit;
        TargetTrf = targetTransform;
        OnSetTargetUnit = onSetTarget;
    }

    public void OnEnterTarget()
    {
    }

    public void OnExitTarget()
    {
        if (GameManager.Inst.TargetUnit != null)
            return;
        
        if (!isPressed)
        {
            OnSetTargetUnit?.Invoke(null);
        }
    }

    public void OnPointDown()
    {
        if (GameManager.Inst.TargetUnit != null)
            return;
        
        isPressed = true;
        OnSetTargetUnit?.Invoke(Unit);
    }

    public void OnPointUpTarget()
    {
        isPressed = false;
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
