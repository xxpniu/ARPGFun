using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using XNet.Libs.Utility;
using Object = UnityEngine.Object;

namespace Core
{
	public class ResourcesManager : XSingleton<ResourcesManager>, IConfigLoader
	{
		OnDebug IConfigLoader.Printer => Debuger.LogWaring;
		List<T> IConfigLoader.Deserialize<T>()
		{
			var fileName = ExcelToJSONConfigManager.GetFileName<T>();
			var json = LoadText("Json/" + fileName);
			if (json == null) return null;
			var type = typeof(T);
			//ExcelConfigColIndexAttribute
			var properties = type.GetProperties(BindingFlags.Public)
				.Where(t => t.GetCustomAttribute<ExcelConfigColIndexAttribute>() is not null)
				.Select(t=> new
				{
					Index = t.GetCustomAttribute<ExcelConfigColIndexAttribute>(),
					Info = t
				})
				.OrderBy(t=>t.Index.Index).ToArray()
				;
			var jArr =  Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(json);
			var raw = jArr.Count;
			var list = new List<T>();
			for (var i = 0; i < raw; i++)
			{
				var item =  Activator.CreateInstance<T>();
				var rawData = (JArray)jArr[i];
				if (rawData.Count != properties.Length)
				{
					throw new ArgumentException($"raw != properties {rawData.Count} != {properties.Count()}");
				}

				var property = properties[i];

				if (property.Info.PropertyType == typeof(string))
				{
					property.Info.SetValue(item, rawData[i].Value<string>());
				}
				else if (property.Info.PropertyType == typeof(int))
				{
					property.Info.SetValue(item, rawData[i].Value<int>());
				}else if (property.Info.PropertyType == typeof(float))
				{
					property.Info.SetValue(item, rawData[i].Value<float>());
				}
				else
				{
					throw new InvalidCastException($"{property.Info.PropertyType} unsupported in type {typeof(T)}");
				}

				list.Add(item);
			}

			return list;

			//jValue.Values()
			//var table = JsonTool.Deserialize<List<T>>(json);
			//return table;
		}

		public delegate void CallBackDele<T>(T res);

		public string LoadText(string path)
		{
			var exPath = path[..path.LastIndexOf('.')];
			var asst = Resources.Load<TextAsset>(exPath);
			if (asst) return asst.text;
			Debuger.LogError($"{exPath} no found");
			return string.Empty;
		}

		public async Task<T> LoadResourcesWithExName<T>(string path, CallBackDele<T> call = null, CancellationToken? token = default)
			where T : Object
		{
			var res = $"Assets/AssetRes/{path}";
			token?.ThrowIfCancellationRequested();
			var asset = Addressables.LoadAssetAsync<T>(res);
			await asset.Task;
			token?.ThrowIfCancellationRequested();
			//if (token?.IsCancellationRequested == true) return null;
			call?.Invoke(asset.Result);
			return asset.Result;
		}



		public string ReadStreamingFile(string name)
		{
			var path = Path.Combine(Application.streamingAssetsPath, name);
			Debuger.Log($"Streaming->{path}");
			return File.ReadAllText(path);
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
			Debuger.Log(path);
			return Addressables.LoadSceneAsync(path);
		}
	}
}
