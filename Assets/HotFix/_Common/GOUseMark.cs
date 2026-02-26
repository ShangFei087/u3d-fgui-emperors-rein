using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOUseMark : MonoBehaviour
{
    static int number = 0;

    public static int GetNumber()
    {
        if (++number > 10000)
            number = 0;
        return number;
    }

    public float useTimeS = 0;
    public string mark = "";
    public void InitParam(string mark)
    {
        this.mark = $"{mark}-{GetNumber()}";
        ToUse();
    }
    public void ToUse()
    {
        useTimeS = Time.unscaledTime;
    }

}
