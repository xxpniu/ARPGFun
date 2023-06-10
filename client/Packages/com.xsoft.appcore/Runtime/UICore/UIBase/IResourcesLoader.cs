  using System;
  using System.Reflection;
  using System.Threading.Tasks;
  using Core;
  using UnityEngine;
  using Object = UnityEngine.Object;
  
  
  public class UIResourcesLoader<T> where T :  UUIWindow, new()
  {
      private const string UI_PATH = "Windows/{0}.prefab";
      public static async Task<T> OpenUIAsync(Transform uiRoot, Action<T> callBack, T window = null)
      {
          if (typeof(T).GetCustomAttribute<UIResourcesAttribute>( false)  is {} att)
          {
              var name = att.Name;
              var res = await  ResourcesManager.S.LoadResourcesWithExName<GameObject>(string.Format(UI_PATH, name));
              var root = Object.Instantiate(res);
              window ??= new T();
              UUIWindow.TryToInitWindow(window, root, uiRoot);
              //IsDone = true;
              callBack?.Invoke(window);
          }
          else  throw new Exception("No found UIResourcesAttribute!");

          return window;
      }
      
  }