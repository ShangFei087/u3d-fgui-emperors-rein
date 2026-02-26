using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeviceRemoteControl : MonoSingleton<DeviceRemoteControl>
{
    public void CheckMqttRemoteControl()
    {
        if (SBoxModel.Instance.isUseRemoteControl)
        {
            string[] addr = SBoxModel.Instance.remoteControlSetting.Split(':');
            MqttRemoteController.Instance.Init(addr[0], int.Parse(addr[1]), MqttAppType.IsGameApp, SBoxModel.Instance.MachineId,
                SBoxModel.Instance.remoteControlAccount, SBoxModel.Instance.remoteControlPassword);
            //MqttRemoteButtonController.Instance.Init("192.168.3.174", 1883, MqttMachineTypeEnum.IsGameApp , SBoxModel.Instance.machineID);

            CheckMqttRemoteControlConnect();
        }
        else
        {
            MqttRemoteController.Instance.Close();
            ClearCheckRemotrControlConnect();
        }
    }


    void CheckMqttRemoteControlConnect()
    {
        _CheckMqttRemoteControlConnect();

        coCheckRemotrControlConnect = StartCoroutine(_CheckMqttRemoteControlConnect());
    }

    Coroutine coCheckRemotrControlConnect = null;
    void ClearCheckRemotrControlConnect()
    {
        if(coCheckRemotrControlConnect != null)
            StopCoroutine(coCheckRemotrControlConnect);
        coCheckRemotrControlConnect = null;
    }


    IEnumerator _CheckMqttRemoteControlConnect()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5f);

            if (SBoxModel.Instance.isConnectRemoteControl != MqttRemoteController.Instance.isConnected)
            {
                SBoxModel.Instance.isConnectRemoteControl = MqttRemoteController.Instance.isConnected;

                //DebugUtils.LogError(SBoxModel.Instance.isConnectRemoteControl);
            }

            if (SBoxModel.Instance.isUseRemoteControl == false)
            {
                ClearCheckRemotrControlConnect();
                yield break;
            }
        }
    }

}
