using System.Collections;
using System.Collections.Generic;
using BattleViews.Components;
using BattleViews.Views;
using UnityEngine;


[RequireComponent(typeof(UBattleItem))]
public class HpItemNameShower : MonoBehaviour
{
    public UBattleItem Item;
    private int id = -1;

    // Start is called before the first frame update
    void Awake()
    {
        Item = GetComponent<UBattleItem>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!(Vector3.Distance(this.transform.position, ThirdPersonCameraContollor.Current.LookPos) < 10)) return;
        var owner = Item.IsOwner(Item.PerView.OwnerIndex);
        id = UUITipDrawer.S.DrawItemName(id, Item.Config.Name, owner,
            this.transform.position + Vector3.up * .8f, ThirdPersonCameraContollor.Current.currentCamera);
    }
}
