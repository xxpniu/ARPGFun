﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using XNet.Libs.Utility;
using Object = UnityEngine.Object;
using UniTask = Cysharp.Threading.Tasks.UniTask;

namespace App.Core.Core
{
	[Name("ResourcesManager")]
	public class ResourcesManager : XSingleton<ResourcesManager>, IConfigLoader
	{
		OnDebug IConfigLoader.Printer => Debuger.LogWaring;
		List<T> IConfigLoader.Deserialize<T>()
		{
			var fileName = ExcelToJSONConfigManager.GetFileName<T>();
			var json = LoadText("Json/" + fileName);
			return json == null ? null : Extends.GetListData<T>(json);
		}

		public delegate void CallBackDele<T>(T res);

		public string LoadText(string path)
		{
			var exPath = path.Substring(0,path.LastIndexOf('.'));
			var asst = Resources.Load<TextAsset>(exPath);
			if (asst) return asst.text;
			Debuger.LogError($"{exPath} no found");
			return string.Empty;
		}

		public async Task<T> LoadResourcesWithExName<T>(string path, CallBackDele<T> call = null, CancellationToken? token = default)
			where T : Object
		{
			var res = $"Assets/AssetRes/{path}";
			//Debuger.Log($"Load:{res}");
			token?.ThrowIfCancellationRequested();
			var asset = Addressables.LoadAssetAsync<T>(res);
			await asset.Task;
			token?.ThrowIfCancellationRequested();
			call?.Invoke(asset.Result);
			return asset.Result;
			
		}



		public async Task<string> ReadStreamingFile(string fileName)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				var webRequest = UnityWebRequest.Get($"jar:file://{Application.dataPath}/assets/{fileName}");
				await webRequest.SendWebRequest();
				var res = webRequest.downloadHandler.text;
				await UniTask.SwitchToMainThread();
				return res;
			}
			else
			{
				var path = Path.Combine(Application.streamingAssetsPath, fileName);
				var res = await File.ReadAllTextAsync(path);
				await UniTask.SwitchToMainThread();
				return res;
			}
		}

		public async Task<Sprite> LoadIcon(GoldShopData item, CallBackDele<Sprite> callBack = null,
			CancellationToken? token = default)
		{
			return await LoadSpriteAsset($"Icon/{item.Icon}.png", callBack, token);
		}

		public async Task<Sprite> LoadIcon(CharacterMagicData item, CallBackDele<Sprite> callBack =null,
			CancellationToken? token = default)
		{
			return await LoadSpriteAsset($"Icon/{item.IconKey}.png", callBack, token);
		}

		public async Task< Sprite> LoadIcon(ItemData item, CallBackDele<Sprite> callBack=null, CancellationToken? token = default)
		{
			return await LoadSpriteAsset($"Icon/{item.Icon}.png",callBack,token);
		}

		public async Task< Sprite> LoadIcon(BattleLevelData item, CallBackDele<Sprite> callBack=null, CancellationToken? token = default)
		{
			return  await LoadSpriteAsset($"Icon/{item.Icon}.png", callBack);  
		}

		private async Task<Sprite> LoadSpriteAsset(string path, CallBackDele<Sprite> callBack = null,
			CancellationToken? token = default)
		{
			var tex = await LoadResourcesWithExName<Texture2D>($"{path}", null, token);
			token?.ThrowIfCancellationRequested();
			var s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
			callBack?.Invoke(s);
			return s;
		}

		public async  Task<GameObject> LoadModel(ItemData item, CallBackDele<GameObject> call =null , CancellationToken? token = default)
		{
			return await LoadResourcesWithExName($"ItemModel/{item.ResModel}.prefab", call,token);
		}

		public AsyncOperationHandle<SceneInstance> LoadLevelAsync(BattleLevelData map)
		{
			var path = $"Assets/Levels/{map.LevelName}.unity";
			return Addressables.LoadSceneAsync(path);
		}
	}
}
