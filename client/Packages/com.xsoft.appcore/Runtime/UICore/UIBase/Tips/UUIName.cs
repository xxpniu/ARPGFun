using TMPro;
using UnityEngine;
using UnityEngine.UI;

[UITipResources("UUIName")]
public class UUIName : UUITip
{
    public TextMeshProUGUI Name { get; private set; }

    protected override void OnCreate()
    {
        Name = FindChild<TextMeshProUGUI>("lb_Name");
    }

    public void ShowName(string name, bool owner)
    {
        Name.text = name;
        Name.color = owner ? Color.green : Color.red;
    }
}