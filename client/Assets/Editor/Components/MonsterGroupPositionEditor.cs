using UnityEngine;
using System.Collections;
using UnityEditor;
//using org.vxwo.csharp.json;
using System.Collections.Generic;
using System.Linq;
using Core;
using Proto;

[CustomEditor(typeof(MonsterGroupPosition))]
public class MonsterGroupPositionEditor : Editor {

    public struct IdNameMapping
    {
        public int Index { set; get; }
        public string Name { set; get; }
    }

    void OnEnable()
    {
        labelStyle = new GUIStyle
        {
            fontSize = 22
        };
        labelStyle.normal.textColor = Color.green;
    }
    private GUIStyle labelStyle;
    //private readonly string label = "怪物点";


    void OnSceneGUI()
    {
        var t = this.target  as MonsterGroupPosition;
        var defaultColor = Handles.color;
        //Handles.color = Color.green;
        //Handles.Label(t.transform.position+ (Vector3.up)*0.5f, $"{t.EType}-{t.ConfigID}", labelStyle);
        Handles.ArrowHandleCap(1, t.transform.position, t.transform.rotation,1, EventType.MouseDrag);
        Handles.color = defaultColor;
    }
    

    private readonly string[] Lables = new string[] { "NONE","怪物刷新组","地图元素刷新组" ,"NPC","玩家初始化位置","怪物表","传送点" };

    private List<IdNameMapping> Mappings;

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        var tar = this.target as MonsterGroupPosition;

        EditorGUILayout.BeginVertical();
        var last = tar.EType;

        tar.EType= (Proto.MapElementType)EditorGUILayout.Popup("类型", (int)tar.EType, Lables);

        if (last != tar.EType || Mappings ==null )
        {
            InitMappings(tar.EType);
        }

        tar.GroupID = EditorGUILayout.IntField("组ID", tar.GroupID);


        switch (tar.EType)
        {
            case Proto.MapElementType.MetMonster:
            case MapElementType.MetMonsterGroup:
                {
                    tar.ConfigID = EditorGUILayout.IntField("ID", tar.ConfigID);
                    tar.ConfigID = EditorGUILayout.IntPopup("配置数据", tar.ConfigID,
                        Mappings.Select(t => t.Name).ToArray(), Mappings.Select(t => t.Index).ToArray());
                }
                break;
            case MapElementType.MetTransport:
                {
                    tar.ConfigID = EditorGUILayout.IntField("ID", tar.ConfigID);
                    tar.ConfigID = EditorGUILayout.IntPopup("配置数据", tar.ConfigID,
                        Mappings.Select(t => t.Name).ToArray(), Mappings.Select(t => t.Index).ToArray());
                }
        
                break;
           
            case Proto.MapElementType.MetNpc:
                {
                   
                }
                break;
            case Proto.MapElementType.MetPlayerInit:
                break;
            
        }


        EditorGUILayout.EndVertical();
    }

    private void InitMappings(MapElementType ty)
    {
        switch (ty)
        {
            case Proto.MapElementType.MetMonster:
                {
                    var config = GetConfig<EConfig.MonsterData>();
                    var languages = GetConfig<EConfig.LanguageData>();

                    var list = config.Select(t => new IdNameMapping { Index = t.ID, Name = languages.SingleOrDefault(s => s.Key == t.NameKey)?.ZH })
                        .OrderBy(t => t.Index).ToList();

                    Mappings = list;

                }
                break;
            case Proto.MapElementType.MetMonsterGroup:
                {
                    var config = GetConfig<EConfig.MonsterGroupData>();
                    //var languages = GetConfig<EConfig.LanguageData>();
                    var list = config.Select(t => new IdNameMapping { Index = t.ID, Name = $"IDs:{t.MonsterID}" })
                        .OrderBy(t => t.Index).ToList();
                    Mappings = list;
                }
                break;
            case Proto.MapElementType.MetElementGroup:
                {
                    var config = GetConfig<EConfig.MapElementGroup>();
                    var languages = GetConfig<EConfig.LanguageData>();
                    var list = config.Select(t => new IdNameMapping { Index = t.ID, Name  = $"EGroup{t.ID}"})
                        .OrderBy(t => t.Index).ToList();
                    Mappings = list;
                }
                break;
            case MapElementType.MetTransport:
                {
                    var config = GetConfig<EConfig.MapElementData>();
                    var languages = GetConfig<EConfig.LanguageData>();
                    var list = config.Where(t=>t.METype == (int)(MapLevelElementType.MletTransport))
                        .Select(t => new IdNameMapping { Index = t.ID,
                        Name = $"Trans{t.ID}:{languages.SingleOrDefault(s => s.Key == t.NameKey)?.ZH}" })
                        .OrderBy(t => t.Index).ToList();
                    Mappings = list;
                }
                break;
            default:
                break;

        }
    }


    
    public List<T> GetConfig<T>() where T : ExcelConfig.JSONConfigBase, new()
    {
        var fileName = ExcelConfig.ExcelToJSONConfigManager.GetFileName<T>();
        var json = LoadText($"Json/{fileName}");
        return json == null ? default : Extends.GetListData<T>(json);
    }

    public string LoadText(string path)
    {
        var exPath = path[..path.LastIndexOf('.')];
        var asst = Resources.Load<TextAsset>(exPath);
        if (asst) return asst.text;
        Debug.LogError($"{exPath} no found");
        return string.Empty;
    }
}
