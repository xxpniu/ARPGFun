using System;
using System.Collections.Generic;
using App.Core.Core;
using EConfig;
using ExcelConfig;
using GameLogic.Game;
using Google.Protobuf;
using org.apache.zookeeper.data;
using Proto;

public static class ExtendTools
{

    private static readonly Dictionary<HeroPropertyType, Func<int, string>> ValueFormat  =new Dictionary<HeroPropertyType, Func<int, string>>();

    static ExtendTools()
    {
        string ProFormat(int value)
        {
            return $"{value / 100}%";
        }

        string SpeedFormat(int value)
        {
            return $"{value / 100}m/s";
        }

        string DisFormat(int value)
        {
            return $"{value / 100}m";
        }

        string ValueFormat(int value)
        {
            return $"{value}";
        }

        string AttackFormat(int value)
        {
            return $"{value / 1000f}t/s";
        }

        //ValueFormat.Add(HeroPropertyType.CastSpeed, tf);
        ExtendTools.ValueFormat.Add(HeroPropertyType.Crt, ProFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.CrtDamageRate, ProFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.Damage, ValueFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.Defance, ValueFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.Dodge, ProFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.Hit, ProFormat);
        //ValueFormat.Add(HeroPropertyType.HitBack, prof);
        //ValueFormat.Add(HeroPropertyType.HitBurn, prof);
        //ValueFormat.Add(HeroPropertyType.HitFreeze, prof);
        //ValueFormat.Add(HeroPropertyType.HitImmob, prof);
        //ValueFormat.Add(HeroPropertyType.Hpdrain, prof);
        ExtendTools.ValueFormat.Add(HeroPropertyType.AttackSpeed, AttackFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.MaxHp, ValueFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.MaxMp, ValueFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.MoveSpeed, SpeedFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.MpDrain, ProFormat);
        ExtendTools.ValueFormat.Add(HeroPropertyType.ViewDistance, DisFormat);
        //ValueFormat.Add(HeroPropertyType.VsBoss, prof);
        // ValueFormat.Add(HeroPropertyType.VsElite, prof);

    }

    public static string ToValueString(this GameLogic.Game.ComplexValue value, HeroPropertyType p)
    {
        return ValueFormat[p].Invoke(value);
    }


    public static string GetAsFormatKeys(this string key, params string[] keys)
    {
        var list = new List<string>();
        if (keys != null)
        {
            foreach (var i in keys)
            {
                list.Add(LanguageManager.S[i]);
            }
        }
        return LanguageManager.S.Format(key, list.ToArray());
    }

    public static string GetAsKeyFormat(this string key, params string[] words)
    {
        return LanguageManager.S.Format(key, words);
    }

    public static string GetLanguageWord(this string key)
    {
        return LanguageManager.S[key];
    }

    public static string ToWord(this HeroPropertyType p)
    {
        var stat = ExcelToJSONConfigManager.GetId<StatData>((int)p);
        return stat.WordKey.GetLanguageWord();
    }

  
}
