using UnityEngine;

public class InputKey : MonoBehaviour
{
    public static bool InputMove => Input.GetMouseButtonDown(0);
    public static bool InputCancle => Input.GetMouseButtonUp(0);
    public static bool InputAttack => Input.GetMouseButton(0);
    public static bool InputChangeDirection => Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0);
}
