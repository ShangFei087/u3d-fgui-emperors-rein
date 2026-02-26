using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using FairyGUI;
public interface IContorller
{
    /// <summary> 创建时调用一次  </summary>
    //void Init(GObject goTarget);  //Awake
    void Init();

    /// <summary> 反复调用，变更绑定对象 </summary>
    void InitParam(params object[] parameters);

    /// <summary> 销毁时调用一次 </summary>
    void Dispose();
    
}

/*

    public void InitParam(params object[] parameters)
    {
        foreach (var param in parameters)
        {
            Console.WriteLine(param);
        }
    }

    obj.InitParam(goTarget, 123, "hello", true);

*/
