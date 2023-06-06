﻿using System;
using UnityEditor;
using UnityEngine;
using Layout.LayoutElements;
using System.IO;
using System.Collections.Generic;
using Core;
using Layout.EditorAttributes;

public class LayoutEditorWindow:EditorWindow
{
	static LayoutEditorWindow ()
	{
		_layouts.Clear ();
		var list = typeof(LayoutBase).Assembly.GetTypes ();
		foreach (var i in list) {
			if (i.IsSubclassOf (typeof(LayoutBase))) {
				var attrs = i.GetCustomAttributes (typeof(EditorLayoutAttribute), false) as EditorLayoutAttribute[];
				if (attrs.Length == 0)
					continue;

				_layouts.Add (i, attrs [0].Name);
			}
		}
    }

    [MenuItem("GAME/Editor/LayoutEditor &2")]
	public static void Init()
	{
		LayoutEditorWindow window = (LayoutEditorWindow)GetWindow(typeof(LayoutEditorWindow), false, "Layout Editor");
		window.minSize = new Vector2 (200, 100);

		//window.position= new Rect( new Vector2 (Screen.width / 2 - 225, Screen.height / 2 -125),window.minSize);
		//window.Show ();
	}

    private static readonly string SAVE_PATH = $"{PropertyDrawer.ASSET_ROOT}Layouts/";


    public static void OpenLayout(string layout)
	{
		Init ();
		LayoutEditorWindow window = GetWindow<LayoutEditorWindow> ();
		window.path = $"{Application.dataPath}{PropertyDrawer.ASSET_ROOT}{layout}";
		window.line = XmlParser.DeSerialize<TimeLine>(File.ReadAllText( window.path,XmlParser.UTF8));
		window.shortPath = layout;
	}

	//提供给调试的显示
	public static float? currentRunTime = null;

	private string path;
	private TimeLine line;
	private string shortPath;

	private static readonly Dictionary<Type,string> _layouts = new Dictionary<Type, string> ();
    private void PlayLayout()
    {
        if (line == null)
            return;
        if (!EditorApplication.isPlaying)
            return;

        var testMaigc = new Layout.MagicData();
        testMaigc.Containers.Add(new Layout.EventContainer
            {
                type = Layout.EventType.EVENT_START,
                layoutPath = shortPath,
                line = line
            }
        );
        var g = EditorStarter.G();
        if (g == null) return;
        g.ReleaseMagic(testMaigc);
        lastStep = 0;
        //time = 0;
    }


    private void GetPlayingInfo()
    {
        if (!EditorApplication.isPlaying)
            return;
        currentRunTime = EditorStarter.G()
            ?.currentReleaser?.GetLayoutTimeByPath(this.shortPath);

    }

	private float lastStep;
	//private float time;
	private DateTime lastTime;
	private float s = 0.02f;
	Vector2 scrollProperty;

	void OnGUI()
    {
        GetPlayingInfo();
        int offset = 2;
        Repaint();
        var group = new Rect(5, position.height - 30, 250, 25);
        GUI.Box(new Rect(3, position.height - 55, 226, 50), "Operator");
        GUILayout.BeginArea(group);

        GUILayout.BeginHorizontal(GUILayout.Width(250));

        if (GUILayout.Button("Run", GUILayout.Width(50)))
        {
            //release
            PlayLayout();
        }

        if (GUILayout.Button("Open", GUILayout.Width(50)))
        {
            Open();
        }
        if (GUILayout.Button("Save", GUILayout.Width(50)))
        {
            Save();
        }

        if (GUILayout.Button("Save As", GUILayout.Width(50)))
        {
            SaveAs();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (line == null)
            return;
        int topHeight = 30;
        int leftWidth = 240;
        Color color = Color.black;
        var rectTop = new Rect(0, 0, position.width - leftWidth, topHeight);


        currentzTime = Mathf.Min(currentzTime, line.Time);
        if (currentObj == line)
        {
            if (Event.current.type == EventType.MouseDrag)
            {
                if (rectTop.Contains(Event.current.mousePosition))
                {
                    currentzTime = line.Time * (Event.current.mousePosition.x / rectTop.width);
                    if (EditorApplication.isPaused)
                    {
                        if (currentzTime < s && Math.Abs(lastStep) > 0.000001)
                        {
                            PlayLayout();
                        }
                        s = Time.deltaTime;
                        var now = (lastStep - 1) * s;
                        if (now <= currentzTime)
                        {
                            if ((DateTime.Now - lastTime).TotalSeconds >= s)
                            {
                                lastTime = DateTime.Now;
                                lastStep++;
                                EditorApplication.Step();
                                //currentzTime = now;
                            }
                        }

                    }
                    Event.current.Use();
                }
            }
        }


        if (Event.current.type == EventType.ContextClick)
        {
            if (rectTop.Contains(Event.current.mousePosition))
            {
                GenericMenu m = new GenericMenu();
                //m.AddItem (new GUIContent ("查看属性"), false, ShowProperty, line);
                m.AddSeparator("");
                foreach (var i in _layouts)
                {
                    m.AddItem(new GUIContent(i.Value), false, CreateLayout, i.Key);
                }
                m.ShowAsContext();

                Event.current.Use();
            }
        }


        if (currentObj != line)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if (rectTop.Contains(Event.current.mousePosition))
                {
                    ShowProperty(line);
                    Event.current.Use();
                }
            }
        }

        int layoutHeight = 35;

        var viewLayout = new Rect(0, topHeight, rectTop.width + 18, position.height - topHeight);
        var vewSize = new Rect(0, 0, rectTop.width, line.Layouts.Count * (layoutHeight+1));


        _scroll = GUI.BeginScrollView(viewLayout, _scroll, vewSize);
        for (var i = 0; i < line.Layouts.Count; i++)
        {
            var l = line.Layouts[i];
            var points = line.FindPointByGuid(l.GUID);

            var rect = new Rect(0, i * (layoutHeight+1), rectTop.width, layoutHeight);
            //var name = string.Empty;
            var attrs = l.GetType().GetCustomAttributes(typeof(EditorLayoutAttribute), false) as  EditorLayoutAttribute[];
            if (attrs.Length > 0)
                name = attrs[0].Name;
            GLDraw.DrawBox(rect, currentObj == l? Color.green:color, 1); 

            foreach (var point in points)
            {
                var time = point.Time;
                int withd = 1;
                float xt = rect.width * (time / line.Time);
                var rectTick = new Rect(xt - 5, rect.yMin, 10, rect.height);

                //GLDraw.DrawBox(rectTick, Color.yellow, 2);

                if (Event.current.type == EventType.ContextClick)
                {
                    if (rectTick.Contains(Event.current.mousePosition))
                    {
                        GenericMenu m = new GenericMenu();
                        m.AddItem(new GUIContent("DeletePoint"), false, DeletePoint, point);
                        m.ShowAsContext();
                        Event.current.Use();
                    }
                }

                if (currentPoint == point)
                {
                    withd = 2;
                }
                else
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (rectTick.Contains(Event.current.mousePosition))
                        {
                            currentPoint = point;
                        }
                    }
                }


                GLDraw.DrawLine(new Vector2(xt, rect.y), new Vector2(xt, rect.y + rect.height), color, 1);
                GLDraw.DrawTick(new Vector2(xt, rect.y), color, withd, rect.height);
                GUI.Label(new Rect(xt, rect.y + 2, 200, 16), string.Format("{0:0.00}s", time));

            }

            if (Event.current.type == EventType.MouseDrag)
            {
                
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (currentPoint != null && currentPoint.GUID == l.GUID)
                    {
                        var rT = line.Time * Event.current.mousePosition.x / rect.width;
                        currentPoint.Time = rT;
                        Event.current.Use();
                    }
                }
            }

            if (Event.current.type == EventType.ContextClick)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    GenericMenu m = new GenericMenu();
                    m.AddItem(new GUIContent("AddPoint"), false, AddPoint, l);
                    m.AddSeparator("-");
                    m.AddItem(new GUIContent("DeleteLayout"), false, DeleteLayout, l);
                    //m.AddSeparator ("");
                    m.ShowAsContext();

                    Event.current.Use();
                }
            }

            if (currentObj != l)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (currentObj != l)
                        {
                            ShowProperty(l);//	currentObj ;
                        }
                        Event.current.Use();
                    }
                }
            }


            GUI.Label(new Rect(2, rect.y + 16, 300, 20), l.ToString());


        }
        GUI.EndScrollView();




        GUILayout.BeginArea(new Rect(position.width - leftWidth, 0, leftWidth, position.height));
        scrollProperty = GUILayout.BeginScrollView(scrollProperty);
        GUILayout.BeginVertical(GUILayout.Width(leftWidth - 25), GUILayout.Height(position.height - 2));
        if (currentObj != null)
            PropertyDrawer.DrawObject(currentObj,"LAYOUT");
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();



        //selected
        if (currentObj == line)
        {
			
            GLDraw.DrawBox(
                new Rect(rectTop.x + offset, rectTop.y + offset, rectTop.width - offset * 2, rectTop.height - offset * 2),
                Color.green, 2);
        }
        //show Lines
        GUI.Label(new Rect(0, 0, 200, 20), string.Format("Time: {1:0.0}s of {0:0.0}s ", line.Time, currentzTime));
        var count = (int)((rectTop.width / 100) * 10);
        float x = 0;
        for (var i = 0; i <= count; i++)
        {
            //pre100FixeTick
            float h = 0;
            if (i % 10 == 0)
            {
                h = rectTop.height;
            }
            else if (i % 5 == 0)
            {
                h = rectTop.height * 0.5f;
            }
            else
            {
                h = rectTop.height * 0.2f;
            }

            GLDraw.DrawLine(new Vector2(x, topHeight), new Vector2(x, topHeight - h), color, 1);
            if (h == rectTop.height)
                GUI.Label(new Rect(x, 16, 20, 20), string.Format("{0:0.0}", line.Time * ((float)i / (float)count)));
            x += (100 / pre100FixeTick);
        }
        {
            var w = rectTop.width * (currentzTime / line.Time);
            GLDraw.DrawLine(new Vector2(rectTop.width, 0), new Vector2(rectTop.width, position.height), color, 1);
            GLDraw.DrawLine(new Vector2(0, topHeight), new Vector2(rectTop.width, topHeight), color, 1);
            GLDraw.DrawLine(new Vector2(w, 0), new Vector2(w, position.height), Color.green, 1);
        }
        if (currentRunTime != null)
        {
		
            var w = rectTop.width * (currentRunTime.Value / line.Time);
            GUI.Label(new Rect(w, topHeight, 50, 20), string.Format("{0:0.0}s", 
                    (float)currentRunTime.Value));
            GLDraw.DrawLine(new Vector2(w, 0), new Vector2(w, position.height), Color.yellow, 2);


        }

    }



	private readonly float pre100FixeTick = 10;
	private Vector2 _scroll = Vector2.zero;

	//private LayoutBase current;

	private void SaveAs()
	{
		if (line == null)  return;
		path = EditorUtility.SaveFilePanel ("Save", $"{Application.dataPath}{SAVE_PATH}" ,"layout", "xml");
		if (!string.IsNullOrEmpty (path)) {
			shortPath = path.Replace (Application.dataPath + "/Resources/", "");
			var xml = XmlParser.Serialize (line);
			File.WriteAllText (path, xml, XmlParser.UTF8);
			ShowNotification ( new GUIContent("Save To:" + path));
			AssetDatabase.Refresh ();
		}
	}

	private void Save()
	{
		if (line == null)
			return;
		if (!string.IsNullOrEmpty (path)) {
			var xml = XmlParser.Serialize (line);
			File.WriteAllText (path, xml, XmlParser.UTF8);
			ShowNotification ( new GUIContent("Save To:" + path));
			AssetDatabase.Refresh ();
		} else {
			SaveAs ();
		}
	}
		
	private void Open()
    {
        if (line != null)
        {
            if (!EditorUtility.DisplayDialog("Cancel", 
                "Open new file will lost current edit. Do you want over edit?", 
                "Yes", "Cancel"))
                return;
        }

        path = EditorUtility.OpenFilePanel("Open Layout",$"{Application.dataPath}{SAVE_PATH}", "xml");
        Open(path);
    }

    private void Open(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            line = XmlParser.DeSerialize<TimeLine>(File.ReadAllText(path, XmlParser.UTF8));
            shortPath = path.Replace(PropertyDrawer.ASSET_ROOT, "");
            currentObj = null;
        } 
    }

	private object currentObj;
    private TimePoint currentPoint;

	private void DeleteLayout(object userstate)
	{
		var layoutBase = userstate as LayoutBase;
		line.RemoveByGuid (layoutBase.GUID);
	}

    private void AddPoint(object userstate)
    {
        var layoutBase = userstate as LayoutBase;
        this.line.Points.Add(new TimePoint (){ Time =currentzTime, GUID = layoutBase.GUID });
    }

    private void DeletePoint(object userstate)
    {
        var point = userstate as TimePoint;
        this.line.Points.Remove(point);
    }

	private void ShowProperty(object obj)
	{
		if (currentObj != obj) {
			GUI.UnfocusWindow ();
		}
		currentObj = obj;
	}

	private void CreateLayout(object userState)
	{
		var type = userState as Type;

		var layout = LayoutBase.CreateInstance (type);
		line.Layouts.Add (layout);
		line.Points.Add (new TimePoint (){ Time =currentzTime, GUID = layout.GUID });
	}

	private float currentzTime =0f;
}


