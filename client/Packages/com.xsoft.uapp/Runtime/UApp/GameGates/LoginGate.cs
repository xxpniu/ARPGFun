using System;
using System.Threading.Tasks;
using Windows;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;
using XNet.Libs.Utility;

namespace UApp.GameGates
{
    public class LoginGate:UGate
    {
        protected override async Task JoinGate(params object[] args)
        {
            GateManager.Reset();
            ChatManager.Reset();
            
            await SceneManager.LoadSceneAsync("null");
            UUIManager.Singleton.HideAll();

            await UUIManager.S.CreateWindowAsync<UUILogin>((ui) => { ui.ShowWindow(); });
        }

        public async void GoLogin(string username, string password, Action<L2C_Login> callback)
        {
            await DoLogin(username, password, callback);
        }

        public async void GoReg(string username, string password, Action<L2C_Reg> callback)
        { 
            await DoReg(username, password, callback);
        }

        private async Task<L2C_Reg> DoReg(string username, string password, Action<L2C_Reg> callback)
        {
            var channel = new LogChannel(UApplication.S.LoginServer);

            var req = new C2L_Reg
            {
                Password = password,
                UserName = username,
                Version = 0
            };
            var client = channel.CreateClient<LoginServerService.LoginServerServiceClient>();
            var r = await client.RegAsync(req);
            await channel.ShutdownAsync();
            await UniTask.SwitchToMainThread();
            callback?.Invoke(r);
            return r;
        }

        public async Task<L2C_Login> DoLogin(string userName, string pwd, Action<L2C_Login> callback = default)
        {
            try
            {
                var channel = new LogChannel(UApplication.S.LoginServer);

                var req = new C2L_Login
                {
                    Password = pwd,
                    UserName = userName,
                    Version = 0
                };

                Debug.Log($"Request:{req}");
                var client = channel.CreateClient<LoginServerService.LoginServerServiceClient>();
                var r = await client.LoginAsync(req,
                    deadline: DateTime.UtcNow.AddSeconds(10));
                await channel.ShutdownAsync();
                await UniTask.Yield();
                callback?.Invoke(r);
                return r;
            }
            catch(Exception ex)
            {
                var res = new L2C_Login
                {
                    Code = ErrorCode.Error
                };
                Debug.LogException(ex);
                callback?.Invoke(res);

                return res;
            }
        }
    }
}