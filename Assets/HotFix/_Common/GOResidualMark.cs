using FairyGUI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class GOResidualMark : MonoBehaviour
{

    #region 管理器
    static float gapTimeS = 60f * 5;  // 5分钟   // 10f;//10秒
    static bool isTimer = false;
    static List<GOResidualMark> _grmLst = new List<GOResidualMark>();
    public static List<GOResidualMark> grmLst
    {
        get
        {
            if (!isTimer)
            {
                isTimer = true;
                Timers.inst.Remove(ClearGos);
                Timers.inst.Add(5f, 0, ClearGos);
            }
            return _grmLst;
        }
    }

    static void ClearGos(object arg)
    {
        int idx = grmLst.Count;
        while (--idx >= 0)
        {
            GOResidualMark grm = grmLst[idx];
            if (grm == null)
            {
                grmLst.Remove(grm);
                continue;
            }
            if (grm.CheckResidual(false))
            {
                grmLst.Remove(grm);
                grm.DisposeGObject();
            }       
        }
    }

    #endregion



    public float runTimeS = 0;
    public int referenceCount = 0;
    public string mark = "";

    public bool isInit = false;

    GObject gObject;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (isInit) return;
        isInit = true;

        runTimeS = Time.unscaledTime;
        if (!grmLst.Contains(this))
        {
            grmLst.Add(this);
        }
    }


    [Button]
    public bool CheckResidual(bool isLog = true)
    {
        bool isResidual = transform.parent == null   
            && gameObject.active == false 
            && referenceCount <= 0 
            && Time.unscaledTime - runTimeS > gapTimeS;

        if(isLog)
            DebugUtils.Log($"isResidual: {isResidual}  referenceCount:{referenceCount}  {Time.unscaledTime} - {runTimeS} = {Time.unscaledTime - runTimeS} ");

        if (transform.parent != null)
            runTimeS = Time.unscaledTime;

        return isResidual;
    }


    public void InitParam(GObject obj,string mark = null) 
    {
        Init();

        this.gObject = obj;
        if (mark != null)
        {
            this.mark = mark;
        }
    }

    public void OnDestroy()
    {
        if (grmLst.Contains(this))
        {
            grmLst.Remove(this);
        }
    }

    [Button]
    public void DisposeGObject()
    {
        GetGObject().Dispose();     
    }


    public GObject GetGObject()
    {
        return gObject;
    }

}
