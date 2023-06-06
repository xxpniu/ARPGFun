using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using org.apache.zookeeper;
using UnityEditor;
using UnityEngine;




public class ZkViewer : EditorWindow
{
    public class ZKWatcher : Watcher
    {
        public async override Task process(WatchedEvent @event)
        {
            Debug.Log($"{@event}");
            await Task.CompletedTask;
        }
    }

    public class ZkTreeNode
    {
        public DataResult Data;
        public List<ZkTreeNode> Childs { get; } = new List<ZkTreeNode>();
        public int ChildNum { get; internal set; }

        public string Path;
        public ZkTreeNode Parent;
        public string name;
        public bool LoadedChilds = false;
    }

    private GUIStyle style;
    private GUIStyle dstyle;
    [MenuItem("Window/ZkViewer")]
    public static void OpenWindow()
    {
        var viewer = GetWindow<ZkViewer>("ZkViewer");

        viewer. style = new GUIStyle() { fixedHeight =25 };
        viewer.dstyle = new GUIStyle { fixedHeight = 25, normal = new GUIStyleState { background = Texture2D.grayTexture } };
        viewer.ShowModalUtility();
    }

    public string Host = "129.211.9.75:2181";

    [NonSerialized]
    private ZkTreeNode Root;

    [NonSerialized]
    private ZooKeeper zk;

    private void OnGUI()
    {

        line = 0;
        var size = Vector2.one * 200;

        GUILayout.BeginArea(new Rect(this.position.width-(size.x+5),5, size.x,size.y));
        GUILayout.BeginVertical();

        if (zk == null)
        {
            GUILayout.Label("ZkHost");
            Host = EditorGUILayout.TextField(Host);
            if (GUILayout.Button("Connect"))
            {
                zk?.closeAsync();
                zk = new ZooKeeper(Host, 3000, new ZKWatcher());
                Root = new ZkTreeNode() { Path = "/" };
                _= Task.Factory.StartNew(()=> LoadChildAsync(Root));
            }
        }
        else
        {
            GUILayout.Label(Host);
            if (GUILayout.Button("DisConnect"))
            {
                zk?.closeAsync();
                Root = null;
                zk = null;
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();

        if (Root == null)
        {
            return;
        }

        var w = this.position.width - (size.x + 25);

        scroll= EditorGUILayout.BeginScrollView(scroll,GUILayout.Width(w));
        DrawNode(Root, 0);
        EditorGUILayout.EndScrollView();
        
    }

    private Vector2 scroll = Vector2.zero;


    private int line = 0;

    private  void DrawNode(ZkTreeNode node, int top)
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal(style: line++ % 2 == 0 ? dstyle : style);

        GUILayout.Label("", GUILayout.Width(top * 20));
        GUILayout.Label($"{node.Path}{node.name}");

        if (node.ChildNum > 0)
        {
            if (!node.LoadedChilds)
            {
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    _= Task.Factory.StartNew(()=> LoadChildAsync(node));// LoadChildAsync(node);
                }
            }
            else
            {
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    node.Childs.Clear();
                    node.LoadedChilds = false;
                }
            }
        }

        if (GUILayout.Button("Data", GUILayout.Width(60)))
        {
            if (node.Data != null)
            {
                var str = Encoding.UTF8.GetString(node.Data.Data);
                EditorUtility.DisplayDialog(node.Path, str, "Ok");
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        foreach (var i in node.Childs)
        {
            DrawNode(i, top + 1);
        }
    }


    private async Task LoadChildAsync(ZkTreeNode node)
    {
        node.Childs.Clear();
        var childs = await zk.getChildrenAsync(node.Path);
        Debug.Log($"{node.Path} childs {childs.Children.Count}");
        foreach (var c in childs.Children)
        {
            var t = $"{node.Path}/{c}";
            if (node.Path == "/")
            {
                t = $"/{c}";
            }

            Debug.Log(t);
            var data = await zk.getDataAsync(t);
            var n = new ZkTreeNode
            {
                Path = t,
                Data = data,
                name = $"[{data.Stat.getNumChildren()}]",
                Parent = node,
                ChildNum = data.Stat.getNumChildren()
            };
            node.Childs.Add(n);
        }
        node.LoadedChilds = true;
    }
}

