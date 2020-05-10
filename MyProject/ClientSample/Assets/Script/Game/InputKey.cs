using UnityEngine;

public class InputKey : MonoBehaviour
{
    public static bool InputAttack => Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0);
}
