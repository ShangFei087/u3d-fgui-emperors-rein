using System;

using FairyGUI;

using UnityEngine;



/// <summary>

/// 封装 PagController 与 FGUI 锚点绑定。

/// </summary>

public sealed class PagSlotBinding : IDisposable

{

    public PagController Controller { get; }

    public string InstanceKey => Controller.InstanceKey;

    public GComponent FguiAnchor { get; private set; }



    public PagSlotBinding(string instanceKey, string gamePagFolder = null)

    {

        Controller = new PagController(instanceKey, gamePagFolder);

    }



    public void Attach(GComponent fguiAnchor)

    {

        FguiAnchor = fguiAnchor;

        Controller.Attach(fguiAnchor);

    }



    public void ConfigureFgui(int maxDisplaySide, int fps)

    {

        Controller.SetRenderTarget(PagController.PagRenderTarget.FguiTexture);

        Controller.ConfigureFguiFrame(maxDisplaySide, fps);

    }



    public bool PreparePlay(bool useFguiTexture, int maxDisplaySide, int fps)

    {

        if (FguiAnchor == null)

        {

            Debug.LogWarning($"[PAG] PreparePlay skipped: FguiAnchor is null, instance={InstanceKey}");

            return false;

        }



        if (useFguiTexture)

        {

            ConfigureFgui(maxDisplaySide, fps);

            Controller.PrepareFguiLayoutBeforePlay();

            if (Controller.FguiLoader == null)

            {

                Debug.LogError($"[PAG] PreparePlay failed: pagEffect not bound, instance={InstanceKey}, anchor={FguiAnchor.name}");

                return false;

            }

        }

        else

        {

            Controller.SetRenderTarget(PagController.PagRenderTarget.Overlay);

        }



        return true;

    }



    public void Stop(bool hideFgui = true)

    {

        Controller.StopPag();

        if (hideFgui)

        {

            Controller.SetFguiVisible(false);

        }

    }



    public void PrepareBetweenPlaybackCycles()

    {

        Controller.PrepareBetweenPlaybackCycles();

    }



    public bool Play(string pagFile, string positionType, string layoutExtra, int repeatCount = 1)

    {

        Controller.SetRepeatCount(repeatCount);

        return Controller.PlayPag(pagFile, positionType, layoutExtra ?? string.Empty);

    }



    public void Dispose()

    {

        Controller.Dispose();

        FguiAnchor = null;

    }

}


