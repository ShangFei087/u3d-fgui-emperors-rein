using UnityEngine;

public class KeyboardInput : MonoBehaviour
{
    void Update()
    {
        // 检测按键按下
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("空格键被按下");
        }

        // 检测按键持续按下
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("W键被按住");
            // 移动角色等操作
        }

        // 检测按键释放
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Debug.Log("ESC键被释放");
        }
    }


}