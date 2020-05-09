using UnityEngine;

public class InputKey : MonoBehaviour
{
    public static bool InputAttack => Input.GetKey(KeyCode.A) && Input.GetMouseButtonDown(0);
}
