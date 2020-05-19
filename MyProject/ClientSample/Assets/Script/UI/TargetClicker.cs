using System;
using UnityEngine;
using UnityEngine.UI;

public class TargetClicker : MonoBehaviour
{
    [SerializeField]
    private Text txtName;
    [SerializeField] 
    private Slider hpBar;
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
        txtName.text = unit.DATA.name + unit.DATA.playerId.ToString();
        txtName.gameObject.SetActive(false);
    }

    public void SetHp(int maxHp, int currentHp)
    {
        hpBar.value = currentHp;
        hpBar.maxValue = maxHp;
    }

    public void OnEnterTarget()
    {
        if (GameManager.Inst.TargetUnit != null)
            return;
        
        txtName.gameObject.SetActive(true);
    }

    public void OnExitTarget()
    {
        if (GameManager.Inst.TargetUnit != null)
            return;
        
        if (!isPressed)
        {
            OnSetTargetUnit?.Invoke(null);
        }
        
        txtName.gameObject.SetActive(false);
    }

    public void OnPointDown()
    {
        if (GameManager.Inst.TargetUnit != null)
            return;
        
        isPressed = true;
        OnSetTargetUnit?.Invoke(Unit);
        
        txtName.gameObject.SetActive(true);
    }

    public void OnPointUpTarget()
    {
        isPressed = false;
        OnSetTargetUnit?.Invoke(null);
        
        txtName.gameObject.SetActive(false);
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
