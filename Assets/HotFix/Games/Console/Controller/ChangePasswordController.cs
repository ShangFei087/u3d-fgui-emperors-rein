using System.Collections.Generic;
using GameMaker;
using SBoxApi;
using System;
using FairyGUI;


namespace ConsoleSlot01
{
    public class ChangePasswordController
    {

        public ChangePasswordController(UserType userTp)
        {
            userType = userTp;
        }

        public enum UserType
        {
            None,
            Shift,
            Manager,
            Admin,
        }

        UserType userType = UserType.None;

        /// <summary> 用户权限 </summary>
        int permissions
        {
            get
            {
                switch (userType)
                {
                    case UserType.Admin:
                        return 3;
                    case UserType.Manager:
                        return 2;
                    case UserType.Shift:
                        return 1;
                    default:
                        return -1;
                }
            }
        }

        string titleEnterPwd
        {
            get
            {
                switch (userType)
                {
                    case UserType.Admin:
                        return "Reset Password: Admin";
                    case UserType.Manager:
                        return "Reset Password: Manager";
                    case UserType.Shift:
                        return "Reset Password: Shift";
                    default:
                        return "";
                }
            }
        }

        string titleRestPwd
        {
            get
            {
                switch (userType)
                {
                    case UserType.Admin:
                        return "Enter Password: Admin";
                    case UserType.Manager:
                        return "Enter Password: Manager";
                    case UserType.Shift:
                        return "Enter Password: Shift";
                    default:
                        return "";
                }
            }
        }

        int pwdCount
        {
            get
            {
                switch (userType)
                {
                    case UserType.Admin:
                        return 9;
                    case UserType.Manager:
                        return 8;
                    case UserType.Shift:
                        return 6;
                    default:
                        return 0;
                }
            }
        }


        void OnChangPassword(int pwd)
        {
            switch (userType)
            {
                case UserType.Admin:
                    SBoxModel.Instance.passwordAdmin = pwd;
                    return;
                case UserType.Manager:
                    SBoxModel.Instance.passwordManager = pwd;
                    return;
                case UserType.Shift:
                    SBoxModel.Instance.passwordShift = pwd;
                    return;
            }
        }


        public async void OnClickSetPassword()
        {

            EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard001,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["title"] = I18nMgr.T(titleRestPwd),
                        ["isPlaintext"] = false,
                    }));

            if (res.value != null)
            {
                string pwd = (string)res.value;
                DebugUtils.Log($"键盘输入结果 ：{pwd}");

                TryGoBackPermissions();  // 尝试放回旧的用户权限

                MachineDataManager02.Instance.RequestCheckPassword(int.Parse(pwd),
                async (res) =>
                {
                    SBoxPermissionsData data = res as SBoxPermissionsData;
                    if (data.result == 0 && data.permissions == permissions)
                    {

                        Func<string, string> checkPassword01Func = (res) =>
                        {
                            if (string.IsNullOrEmpty(res))
                                return I18nMgr.T("Please enter your new password first");

                            try
                            {
                                int num = int.Parse(res);
                            }
                            catch (Exception ex)
                            {
                                return I18nMgr.T("The input value must be a number");
                            }

                            if (res.Length < pwdCount)
                                return string.Format(I18nMgr.T("The {0} must be {1} digits long"), I18nMgr.T("Password"), pwdCount);

                            return null;
                        };



                        Func<string, string> checkPassword02Func = (res) =>
                        {
                            if (string.IsNullOrEmpty(res))
                                return I18nMgr.T("Please enter your password confirmation");

                            try
                            {
                                int num = int.Parse(res);
                            }
                            catch (Exception ex)
                            {
                                return I18nMgr.T("The input value must be a number");
                            }

                            if (res.Length < pwdCount)
                                return string.Format(I18nMgr.T("The {0} must be {1} digits long"), I18nMgr.T("Password"), pwdCount);

                            return null;
                        };


                        EventData res1 = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleSetParameter002,
                                new EventData<Dictionary<string, object>>("",
                                new Dictionary<string, object>()
                                {
                                    ["title"] = I18nMgr.T(titleRestPwd),
                                    ["paramName1"] = I18nMgr.T("input new password:"),
                                    ["paramName2"] = I18nMgr.T("confirm new password:"),
                                    ["checkParam1Func"] = checkPassword01Func,
                                    ["checkParam2Func"] = checkPassword02Func,
                                }
                            ));


                        if (res1.value != null)
                        {
                            List<string> lst = (List<string>)res1.value;
                            string pwd01 = lst[1];
                            string pwd02 = lst[0];

                            if (pwd02 != pwd01)
                            {
                                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("The passwords entered twice are not the same"));
                                return;
                            }
                            int password = int.Parse(pwd01);

                            MachineDataManager02.Instance.RequestChangePassword(password, (res) =>
                            {
                                SBoxPermissionsData data = res as SBoxPermissionsData;

                                if (data.result == 0)
                                {
                                    OnChangPassword(password); // 修改成功
                                    TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Account password successfully changed"));
                                }
                                else
                                {
                                    TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Account password modification failed"));
                                }

                            }, (err) =>
                            {
                                // 修改失败
                                //TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Account password modification failed"));
                                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T(err.msg));
                            });
                            //修改密码！
                        }

                    }
                    else
                    {
                        PopupErrorPassword();
                    }
                }, (error) =>
                {
                    PopupErrorPassword();
                });

            }

        }


        void PopupErrorPassword()
        {
            CommonPopupHandler.Instance.OpenPopupSingle(
               new CommonPopupInfo()
               {
                   isUseXButton = false,
                   buttonAutoClose1 = true,
                   buttonAutoClose2 = true,
                   type = CommonPopupType.YesNo,
                   text = I18nMgr.T("Error Password"),
                   buttonText1 = I18nMgr.T("Cancel"),
                   buttonText2 = I18nMgr.T("Confirm"),
               });
        }

        #region 切回旧的用户权限
        enum StepGoBackPermissions
        {
            CheckPopClose,
            GoBackPermissions,
        }
        static StepGoBackPermissions step;


        static void TryGoBackPermissions()
        {

            step = StepGoBackPermissions.CheckPopClose;
            Timers.inst.Remove(TaskGoBackPermissions);
            Timers.inst.Add(3, 1, TaskGoBackPermissions);
        }

        static void TaskGoBackPermissions(object param)
        {
            switch (step)
            {
                case StepGoBackPermissions.CheckPopClose:
                    {

                        if (PageManager.Instance.IndexOf(PageName.ConsolePopupConsoleKeyboard001) == -1
                            && PageManager.Instance.IndexOf(PageName.ConsolePopupConsoleSetParameter002) == -1)
                        {
                            step = StepGoBackPermissions.GoBackPermissions;
                            TaskGoBackPermissions(null);
                        }
                        else
                        {
                            step = StepGoBackPermissions.CheckPopClose;
                            Timers.inst.Add(3, 1, TaskGoBackPermissions);
                        }
                    }
                    break;
                case StepGoBackPermissions.GoBackPermissions:
                    {

                        int pwd = SBoxModel.Instance.curPermissions == 3 ? SBoxModel.Instance.passwordAdmin :
                            SBoxModel.Instance.curPermissions == 2 ? SBoxModel.Instance.passwordManager :
                            SBoxModel.Instance.curPermissions == 1 ? SBoxModel.Instance.passwordShift : -1;

                        if (pwd != -1)
                        {
                            MachineDataManager02.Instance.RequestCheckPassword(pwd, (res) =>
                            {
                                SBoxPermissionsData data = res as SBoxPermissionsData;

                                if (data.result == 0)
                                {
                                    DebugUtils.Log($"已切回用户权限; 密码: {pwd}  permissions:{data.permissions}");
                                    Timers.inst.Remove(TaskGoBackPermissions);
                                }
                                else
                                {
                                    DebugUtils.LogError($"已切回用户权限失败  result:{data.result}  permissions:{data.permissions}");
                                    step = StepGoBackPermissions.GoBackPermissions;
                                    Timers.inst.Add(3, 1, TaskGoBackPermissions);
                                }
                            }, (err) =>
                            {
                                DebugUtils.LogError(err.msg);
                                step = StepGoBackPermissions.GoBackPermissions;
                                Timers.inst.Add(3, 1, TaskGoBackPermissions);
                            });
                        }
                        else
                        {
                            Timers.inst.Remove(TaskGoBackPermissions);
                        }
                    }
                    break;
            }
        }


        #endregion

    }
}