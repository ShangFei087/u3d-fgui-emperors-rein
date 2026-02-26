using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITipPopupHandel
{
    void OpenPopup(string msg);
    void OpenPopupOnce(string msg);
    void ClosePopup();
}
public class TipPopupHandler : Singleton<TipPopupHandler>
{

    ITipPopupHandel _iPopup;
    public ITipPopupHandel iPopup
    {
        get
        {
            if (_iPopup == null)
            {
                _iPopup = new TipPopupHelper01();
            }
            return _iPopup;
        }
        set
        {
            if (_iPopup != null)
            {
                _iPopup.ClosePopup();
            }
            _iPopup = value;
        }
    }
    public void OpenPopup(string msg)
    {
        iPopup.OpenPopup(msg);
    }

    public void OpenPopupOnce(string msg)
    {
        iPopup.OpenPopupOnce(msg);
    }

    public void ClosePopup()
    {
        iPopup.ClosePopup();
    }
}
