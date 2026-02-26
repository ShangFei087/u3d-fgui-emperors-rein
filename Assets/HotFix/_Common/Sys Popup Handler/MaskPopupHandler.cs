using System.Collections;
using System;
using GameMaker;

public interface IMaskPopupHandel
{
    void OpenPopup(string data = null);
    void ClosePopup();
    void SetContent(string data);
}

public class MaskPopupHandler : Singleton<MaskPopupHandler>
{

    IMaskPopupHandel _iPopup;
    public IMaskPopupHandel iPopup
    {
        get
        {
            if (_iPopup == null)
            {
                _iPopup = new MaskPopupHelper01();
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


    public void OpenPopup(string data = "")
    {
        iPopup.OpenPopup(data);
    }

    public void ClosePopup()
    {
        iPopup.ClosePopup();
    }

    public void SetContent(string data)
    {
        iPopup.SetContent(data);
    }
}

