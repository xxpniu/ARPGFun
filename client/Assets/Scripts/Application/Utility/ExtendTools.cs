using System;
using System.Collections.Generic;
using Core;
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
         string prof(int value)
        {
            return $"{ value / 100}%";
        }

         string speedf(int value)
        {
            return $"{ value / 100}m/s";
        }

         string disf(int value)
        {
            return $"{ value / 100}m";
        }

         string deff(int value)
        {
            return $"{ value }";
        }
         string tf(int value)
        {
            return $"{ value / 1000}s";
        }
         string at(int value)
        {
            return $"{value / 1000f}t/s";
        }

        ValueFormat.Add(HeroPropertyType.CastSpeed, tf);
        ValueFormat.Add(HeroPropertyType.Crt, prof);
        ValueFormat.Add(HeroPropertyType.CrtDamageRate, prof);
        ValueFormat.Add(HeroPropertyType.Damage, deff);
        ValueFormat.Add(HeroPropertyType.Defance, deff);
        ValueFormat.Add(HeroPropertyType.Dodge, prof);
        ValueFormat.Add(HeroPropertyType.Hit, prof);
        ValueFormat.Add(HeroPropertyType.HitBack, prof);
        ValueFormat.Add(HeroPropertyType.HitBurn, prof);
        ValueFormat.Add(HeroPropertyType.HitFreeze, prof);
        ValueFormat.Add(HeroPropertyType.HitImmob, prof);
        ValueFormat.Add(HeroPropertyType.Hpdrain, prof);
        ValueFormat.Add(HeroPropertyType.AttackSpeed, at);
        ValueFormat.Add(HeroPropertyType.MaxHp, deff);
        ValueFormat.Add(HeroPropertyType.MaxMp, deff);
        ValueFormat.Add(HeroPropertyType.MoveSpeed, speedf);
        ValueFormat.Add(HeroPropertyType.MpDrain, prof);
        ValueFormat.Add(HeroPropertyType.ViewDistance, disf);
        ValueFormat.Add(HeroPropertyType.VsBoss, prof);
        ValueFormat.Add(HeroPropertyType.VsElite, prof);

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
