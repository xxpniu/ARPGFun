﻿using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Layout.EditorAttributes;
using Object = UnityEngine.Object;
using Layout.LayoutElements;
using System.IO;
using BattleViews.Components;
using BattleViews.Views;
using Core;
using Layout;
using Layout.LayoutEffects;
using Proto;
using DamageType = Layout.LayoutElements.DamageType;

public class DrawerHandlerAttribute:Attribute
{
	public DrawerHandlerAttribute(Type p)
	{
		HandleType = p;
	}

	public Type HandleType{ set; get; }
}

public static class PropertyDrawer
{

	public static string ASSET_ROOT = $"/Resources/";
	public static string ASSET = $"Assets{ASSET_ROOT}";
	public static string RES_ROOT = $"Assets/AssetRes/";

	private static void Init()
	{
		_handlers = new Dictionary<Type, MethodInfo> ();
		var type = typeof(PropertyDrawer);
		var methods = type.GetMethods ();
		foreach (var i in methods) {
            if (!(i.GetCustomAttributes(typeof(DrawerHandlerAttribute), false)
                is DrawerHandlerAttribute[] atts) || atts.Length == 0)
                continue;
            _handlers.Add (atts [0].HandleType, i);
		}

		var att = typeof(UCharacterView).GetCustomAttributes(typeof(BoneNameAttribute),false) as BoneNameAttribute[];
		List<string> tnames = new List<string> ();
		foreach (var i in att) 
		{
			tnames.Add (i.Name);
			//tbones.Add (i.BoneName);
		}
		//bones = tbones.ToArray();
		names = tnames.ToArray ();
	}

	//private static string[] bones;
	private static string[] names;

	private static Dictionary<Type,MethodInfo> _handlers;

    private static readonly Dictionary<string, object> _dic = new Dictionary<string, object>();

	public static void DrawObject(object obj,string key)
	{

        if(!string.IsNullOrEmpty(key))
        {
            if (_dic.TryGetValue(key, out object last))
            {
                _dic[key] = obj;
            }
            else
            {
                _dic.Add(key, obj);
            }

            if (last != obj)
            {
                GUI.FocusControl(string.Empty);
                Input.ResetInputAxes();
            }
        }

        if (_handlers == null) {
			Init (); 
		}
		var members = obj.GetType().GetMembers();

		foreach (var i in members) 
		{
			

			if (i is FieldInfo || i is PropertyInfo)
			{
				DrawProperty(i, obj);
				continue;
			}
			
		}
	}

	private static object GetMemberValue(this MemberInfo info, object obj)
	{
		if (info is FieldInfo field)
		{
			return field.GetValue(obj);
		}
		if (info is PropertyInfo property)
		{
			return property.GetValue(obj);
		}
		return null;
	}

	private static void SetMemberValue(this MemberInfo info, object obj, object value)
	{
		if (info is FieldInfo field)
		{
			 field.SetValue(obj,value);
		}
		if (info is PropertyInfo property)
		{
			 property.SetValue(obj,value);
		}

	}

	private static Type GetFieldType(this MemberInfo info)
	{
		if (info is FieldInfo field)
		{
			return field.FieldType;
		}
		if (info is PropertyInfo property)
		{
			return property.PropertyType;
		}
		return null;
	}


	private static void DrawProperty(MemberInfo field,object obj)
	{
		 
		var hide = field.GetCustomAttributes (typeof(HideInEditorAttribute), false) as  HideInEditorAttribute[];
		if (hide.Length > 0) return;
		
		var label = field.GetCustomAttributes (typeof(LabelAttribute), false) as  LabelAttribute[];
		var name = field.Name;
		if (label.Length > 0) name = label [0].DisplayName;

		foreach (var i in _handlers) 
		{
			var attrs = field.GetCustomAttributes (i.Key, false);
			if (attrs.Length > 0) {
				i.Value.Invoke (null, new object[]{ obj,field,name , attrs[0] });
				return;
			}
		}

		var fType = field.GetFieldType();


		if (fType == typeof(int))
		{
			GUILayout.Label(name);
			var value = EditorGUILayout.IntField((int)GetMemberValue(field, obj));
			field.SetMemberValue(obj, value);
		}
		else if (fType == typeof(bool))
		{
			EditorGUILayout.BeginHorizontal();
			var value = EditorGUILayout.Toggle((bool)field.GetMemberValue(obj), GUILayout.Width(50));
			field.SetMemberValue(obj, value);
			GUILayout.Label(name);
			EditorGUILayout.EndHorizontal();
		}
		else if (fType == typeof(string))
		{
			GUILayout.Label(name);
			var value = EditorGUILayout.TextField((string)field.GetMemberValue(obj));
			field.SetMemberValue(obj, value);
		}
		else if (fType == typeof(long))
		{
			GUILayout.Label(name);
			var value = EditorGUILayout.LongField((long)field.GetMemberValue(obj));
			field.SetMemberValue(obj, value);
		}
		else if (fType == typeof(float))
		{
			GUILayout.Label(name);
			var value = EditorGUILayout.FloatField((float)field.GetMemberValue(obj));
			field.SetMemberValue(obj, value);
		}
		else if (fType == typeof(Layout.Vector3))
		{
			//GUILayout.Label (name);
			var v = (Layout.Vector3)field.GetMemberValue(obj);
			var value = EditorGUILayout.Vector3Field(name, new UnityEngine.Vector3(v.x, v.y, v.z));
			field.SetMemberValue(obj, new Layout.Vector3 { x = value.x, y = value.y, z = value.z });
		}
		else if (fType == typeof(FieldValue))
		{
			GUILayout.Label(name);
			if (!(field.GetMemberValue(obj) is FieldValue value))
			{
				value = 1000;
			}

			GUILayout.BeginHorizontal();
			value.type = (FieldValue.ValueType)EditorGUILayout.EnumPopup(value.type);
			switch (value.type)
			{
				case FieldValue.ValueType.Range:
					{
						value.min = EditorGUILayout.IntField(value.min);
						value.max = EditorGUILayout.IntField(value.max);
					}
					break;
				default:
					{
						value.min = EditorGUILayout.IntField(value.min);
					}
					break;
			}
			GUILayout.EndHorizontal();

			field.SetMemberValue(obj, value);
		}
		else if (fType == typeof(DamageRange))
		{
			GUILayout.Label(name);
			if (!(field.GetMemberValue(obj) is DamageRange value)) value = new DamageRange();
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label(GetLable(typeof(DamageRange).GetField("fiterType")));
			value.fiterType = (FilterType)EditorGUILayout.EnumPopup((Enum)value.fiterType);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(GetLable(typeof(DamageRange).GetField("damageType")));
			value.damageType = (DamageType)EditorGUILayout.EnumPopup((Enum)value.damageType);
			GUILayout.EndHorizontal();
			if (value.damageType == DamageType.Area)
			{
				var fieldNames = new string[] { "radius", "angle", "offsetAngle", "offsetPosition" };
				foreach (var i in fieldNames)
				{
					DrawProperty(typeof(DamageRange).GetField(i), value);
				}
			}
			GUILayout.EndVertical();
			field.SetMemberValue(obj, value);

		}
		else if (fType == typeof(ValueSourceOf))
		{
			GUILayout.Label(name);
			if (!(field.GetMemberValue(obj) is ValueSourceOf value))value = new ValueSourceOf();
			GUILayout.BeginHorizontal();
			//GUILayout.Label(GetLable(typeof(ValueSourceOf).GetField("ValueForm")));
			value.ValueForm = (GetValueFrom)EditorGUILayout.EnumPopup((Enum)value.ValueForm);
			if (value.ValueForm == GetValueFrom.CurrentConfig)
			{
                value.Value = EditorGUILayout.IntField(value.Value);
			}
			GUILayout.EndHorizontal();
			
			field.SetMemberValue(obj, value);
		}
		else if (fType.IsEnum)
		{
			GUILayout.Label(name);
			var value = EditorGUILayout.EnumPopup((Enum)field.GetMemberValue(obj));
			field.SetMemberValue(obj, value);
		}

	}

	private static string GetLable(FieldInfo field)
	{
		var label = field.GetCustomAttributes(typeof(LabelAttribute), false) as LabelAttribute[];
		var name = field.Name;
		if (label.Length > 0)
		{
			name = label[0].DisplayName;
		}

		return name;
	}

	[DrawerHandler(typeof(EnumMaskerAttribute))]
	public static void EnumMasker(object obj, MemberInfo field, string label, object attr)
	{
		GUILayout.Label(label);
		var masker = EditorGUILayout.EnumFlagsField((Enum)field.GetMemberValue(obj));
		field.SetMemberValue(obj, masker);
	}


	[DrawerHandler(typeof(EditorResourcePathAttribute))]
	public static void ResourcesSelect(object obj, MemberInfo field, string label, object attr)
	{ 
		var resources = RES_ROOT;
		var path = (string)field.GetMemberValue (obj);
		var res = AssetDatabase.LoadAssetAtPath<Object> ($"{resources}{path}");
		GUILayout.Label (label);
		res= EditorGUILayout.ObjectField (res,typeof(Object), false);
		path = AssetDatabase.GetAssetPath (res);
		path = path.Replace (resources, "");
		field.SetMemberValue (obj, path);
	}
	[DrawerHandler(typeof(EditorStreamingPathAttribute))]
	public static void StreamingSelect(object obj, MemberInfo field, string label, object attr)
	{
		var resources = ASSET;
		var path = (string)field.GetMemberValue(obj);
		var res = AssetDatabase.LoadAssetAtPath<Object>($"{resources}{path}");
		GUILayout.Label(label);
		res = EditorGUILayout.ObjectField(res, typeof(Object), false);
		path = AssetDatabase.GetAssetPath(res);
		path = path.Replace(resources, "");
		field.SetMemberValue(obj, path);
	}


	[DrawerHandler(typeof(LayoutPathAttribute))]
	public static void LayoutPathSelect(object obj, MemberInfo field,string label, object attr)
	{
		var resources = ASSET;
		var path = (string)field.GetMemberValue (obj);
		var aPath = $"{resources}{path}";
		var res = AssetDatabase.LoadAssetAtPath<TextAsset> (aPath);
		GUILayout.Label (label);
		GUILayout.BeginHorizontal ();
		res= EditorGUILayout.ObjectField (res,typeof(TextAsset), false,GUILayout.Width(100)) as TextAsset;
		path = AssetDatabase.GetAssetPath (res);
		path = path.Replace (resources, "");
		field.SetMemberValue (obj, path);

		if (GUILayout.Button ("New"))
        {
			var fPath = EditorUtility.SaveFilePanel ("Create Layout", $"{Application.dataPath}{ASSET_ROOT}Layouts/", "layout", "xml");
			if (!string.IsNullOrEmpty (fPath)) {
				path = fPath.Replace($"{Application.dataPath}{ASSET_ROOT}", "");
				var layout = new TimeLine ();
				var xml = XmlParser.Serialize (layout);
				File.WriteAllText (fPath, xml, XmlParser.UTF8);
				AssetDatabase.Refresh ();
				field.SetMemberValue (obj, path);
			}
		}

		if (!string.IsNullOrEmpty (path)) {
			if (GUILayout.Button ("Open")) {
				//Open layout window
				LayoutEditorWindow.OpenLayout (path);
			}
		}
		GUILayout.EndHorizontal ();

		//GUILayout.EndVertical ();
	}

	[DrawerHandler(typeof(EditorEffectsAttribute))]
	public static void EffectGroupSelect(object obj, MemberInfo field,string label, object attr)
	{
		//GUILayout.BeginVertical ();
		if (GUILayout.Button ("Open Effect Group",GUILayout.MinWidth(150))) {
			var effectGroup = field.GetMemberValue(obj) as List<EffectBase>; 
			EffectGroupEditorWindow.ShowEffectGroup (effectGroup);
		}
	}

	private static int indexOfBone=-1;
	//EditorBoneAttribute
	[DrawerHandler(typeof(EditorBoneAttribute))]
	public static void BoneSelected(object obj, MemberInfo field,string label, object attr)
	{
		var boneName = (string)field.GetMemberValue (obj);
		indexOfBone = -1;
		for (var i = 0; i < names.Length; i++) {
			if (names [i] == boneName) {
				indexOfBone = i;
				break;
			}
		}

		GUILayout.Label (label);
		indexOfBone = EditorGUILayout.Popup (indexOfBone, names);
		if(indexOfBone>=0 && indexOfBone<names.Length)
		{
			field.SetMemberValue (obj, names [indexOfBone]);
	    }
	}
}


