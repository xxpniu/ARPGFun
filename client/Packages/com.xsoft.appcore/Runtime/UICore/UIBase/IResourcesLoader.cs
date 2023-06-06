  using System;
  using System.Threading.Tasks;
  using Core;
  using UnityEngine;
  using Object = UnityEngine.Object;
  
  
  public class UIResourcesLoader<T> where T :  UUIWindow, new()
  {
      private const string UI_PATH = "Windows/{0}.prefab";
      
      public static async Task<T> OpenUIAsync(Transform uiRoot, Action<T> callBack, T window = null)
      {
          var attrs = typeof(T).GetCustomAttributes(typeof(UIResourcesAttribute), false) as UIResourcesAttribute[];
          if (attrs is { Length: > 0 })
          {
              var name = attrs[0].Name;
              var res = await  ResourcesManager.S.LoadResourcesWithExName<GameObject>(string.Format(UI_PATH, name));
              var root = Object.Instantiate(res);
              window ??= new T();
             
              UUIWindow.TryToInitWindow(window, root, uiRoot);
             
              //IsDone = true;
              callBack?.Invoke(window);
          }
          else
          {
              throw new Exception("No found UIResourcesAttribute!");
          }

          return window;
      }



  }