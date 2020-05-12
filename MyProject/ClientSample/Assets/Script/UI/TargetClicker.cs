// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class TargetClicker : MonoBehaviour
// {
//     private Unit Unit;
//     private Transform TargetTrf;
//     private bool isPressed = false;
//     public Text PlayerName;
//     
//     public void SetTrackingTarget(Unit unit, Transform target)
//     {
//         var canvas = GameObject.Find("Canvas");
//         this.transform.SetParent(canvas.transform);
//         Unit = unit;
//         TargetTrf = target.transform;
//
//         PlayerName.text = unit.DATA.playerId.ToString();
//     }
//
//     public void OnEnterTarget()
//     {
//         PlayerName.gameObject.SetActive(true);
//         GameManager.Inst.myPlayer.TargetUnit = Unit;
//     }
//
//     public void OnExitTarget()
//     {
//         if (!isPressed)
//         {
//             PlayerName.gameObject.SetActive(false);    
//             GameManager.Inst.myPlayer.TargetUnit = null;
//         }
//     }
//
//     public void OnPointDown()
//     {
//         isPressed = true;
//         PlayerName.gameObject.SetActive(true);
//
//         if (GameManager.Inst.myPlayer.TargetUnit != null && GameManager.Inst.myPlayer.DATA.playerId != Unit.DATA.playerId)
//         {
//             GameManager.Inst.myPlayer.TargetUnit = Unit;
//         }
//         else
//         {
//             GameManager.Inst.myPlayer.TargetUnit = null;
//         }
//     }
//
//     public void OnPointUpTarget()
//     {
//         isPressed = false;
//         PlayerName.gameObject.SetActive(false);
//         
//         if (GameManager.Inst.myPlayer.TargetUnit != null)
//         {
//             GameManager.Inst.myPlayer.TargetUnit = null;
//         }
//     }
//     
//     void Update()
//     {
//         if (TargetTrf == null)
//         {
//             return;
//         }
//         
//         transform.position = Camera.main.WorldToScreenPoint(TargetTrf.position);
//     }
// }
