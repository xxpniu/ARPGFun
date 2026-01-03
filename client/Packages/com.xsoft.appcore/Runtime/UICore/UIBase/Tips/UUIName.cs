using UnityEngine;
using UnityEngine.UI;

[UITipResources("UUIName")]
public class UUIName : UUITip
{
    public Text Name { get; private set; }

    protected override void OnCreate()
    {
        Name = FindChild<Text>("lb_Name");
    }

    public void ShowName(string name, bool owner)
    {
        Name.text = name;
        Name.color = owner ? Color.green : Color.red;
    }
}