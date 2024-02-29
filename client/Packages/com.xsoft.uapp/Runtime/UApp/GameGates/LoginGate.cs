using System;
using System.Threading.Tasks;
using Windows;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNet.Libs.Utility;

namespace UApp.GameGates
{
    public class LoginGate:UGate
    {
        protected override async Task JoinGate(params object[] args)
        {
            GateManager.Try()?.Reset();
            ChatManager.Try()?.Reset();
            
            await SceneManager.LoadSceneAsync("null");
            UUIManager.Singleton.HideAll();

            await UUIManager.S.CreateWindowAsync<UUILogin>((ui) => { ui.ShowWindow(); });
        }
        
        public static async Task<L2C_Reg> DoReg(string username, string password, Action<L2C_Reg> callback = default)
        {
            L2C_Reg r ;
            try
            {
                r = await C<LoginServerService.LoginServerServiceClient>.RequestOnceAsync(
                    ip: UApplication.S.LoginServer,
                    expression: async serviceClient =>
                        await serviceClient.RegAsync(new C2L_Reg
                            {
                                Password = password,
                                UserName = username,
                                Version = 0
                            }
                        ),
                      deadTime: DateTime.UtcNow.AddSeconds(10)
                    );

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                r = new L2C_Reg
                {
                    Code = ErrorCode.Exception
                };
            }

            await UniTask.SwitchToMainThread();
            //UUIManager.Try()?.ShowMask(false);
            callback?.Invoke(r);
            return r;
        }

        public static async Task<L2C_Login> DoLogin(string userName, string pwd, Action<L2C_Login> callback = default)
        {
            //UUIManager.Try()?.ShowMask(true);
            L2C_Login r = null;
            try
            {
                r = await C<LoginServerService.LoginServerServiceClient>
                    .RequestOnceAsync(UApplication.S.LoginServer,
                        expression: async (c) => await c.LoginAsync(new C2L_Login
                        {
                            Password = pwd,
                            UserName = userName,
                            Version = 0
                        }), DateTime.UtcNow.AddSeconds(10));
            }
            catch (Exception ex)
            {
                r = new L2C_Login
                {
                    Code = ErrorCode.Exception
                };
                Debug.LogException(ex);
            }

            await UniTask.SwitchToMainThread();
            callback?.Invoke(r);
            //UUIManager.Try()?.ShowMask(false);
            return r;
        }
    }
}