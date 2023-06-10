using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelConfig;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proto;
using UnityEngine;

namespace Core
{
    public static class Extends 
    {
        public static bool IsOk(this ErrorCode er)
        {
            return er == ErrorCode.Ok;
        }
      

        private static readonly JsonParser parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        private static readonly JsonFormatter format = new JsonFormatter(JsonFormatter.Settings.Default.WithFormatEnumsAsIntegers(true).WithFormatDefaultValues(true));

        public static string ToJson<T>(this T msg) where T : IMessage, new()
        {
            return format.Format(msg);
        }

        public static T Parser<T>(this string json) where T : IMessage, new()
        {
            return parser.Parse<T>(json);
        }

        public static List<T> GetListData<T>(string json) where T : JSONConfigBase
        {
            var fileName = ExcelToJSONConfigManager.GetFileName<T>();
			//var json = LoadText("Json/" + fileName);
			if (json == null) return null;
			var type = typeof(T);
			//ExcelConfigColIndexAttribute
			var properties = type.GetProperties(BindingFlags.Public| BindingFlags.Instance)
				.Where(t => t.GetCustomAttribute<ExcelConfigColIndexAttribute>() is not null)
				.Select(t=> new
				{
					Index = t.GetCustomAttribute<ExcelConfigColIndexAttribute>(),
					Info = t
				})
				.OrderBy(t=>t.Index.Index).ToArray()
				;
			var jArr =  JsonConvert.DeserializeObject<JArray>(json);
			var raw = jArr.Count;
			var list = new List<T>();
			for (var i = 0; i < raw; i++)
			{
				var item = Activator.CreateInstance<T>();
				var rawData = (JArray)jArr[i];
				if (rawData.Count != properties.Length)
				{
					throw new ArgumentException($"raw != properties {rawData.Count} != {properties.Count()}");
				}

				for (var index = 0; index < rawData.Count; index++)
				{
					var property = properties[index];
					try
					{
						if (property.Info.PropertyType == typeof(string))
						{
							property.Info.SetValue(item, rawData[index].Value<string>());
						}
						else if (property.Info.PropertyType == typeof(int))
						{
							var rawVal = rawData[index];
							switch (rawVal.Type)
							{
								case JTokenType.Float:
									property.Info.SetValue(item, (int)rawData[index]);
									break;
								case JTokenType.Integer:
									property.Info.SetValue(item, rawVal.Value<int>());
									break;
								case JTokenType.String:
								{
									var str = rawVal.Value<string>();
									if (!int.TryParse(str, out var v)) v = -1;
									property.Info.SetValue(item, v);
								}
									
									break;
								default:
									throw new InvalidCastException($"{rawVal.Type} can't case into int");
							}
						}
						else if (property.Info.PropertyType == typeof(float))
						{
							property.Info.SetValue(item, rawData[index].Value<float>());
						}
						else
						{
							throw new InvalidCastException(
								$"{property.Info.PropertyType} unsupported in type {typeof(T)}");
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						Debug.LogError($"{typeof(T)} {property.Info.PropertyType} {property.Info.Name} [{index}] of {rawData[index]} - {rawData}");
					}
				}

				list.Add(item);
			}

			return list;
        }
    }

}