using System.Collections;
using System.Collections.Generic;
using BattleViews.Components;
using BattleViews.Views;
using UnityEngine;


[RequireComponent(typeof(UCharacterView))]
public class HpTipShower : MonoBehaviour
{
    public UCharacterView view;
    private float showHpBarTime=-1;
    private int nameBar = -1;

    // Start is called before the first frame update
    void Awake()
    {
        view = GetComponent<UCharacterView>();
    }


    private void OnDead()
    {
        showHpBarTime = -1;
    }

    private void OnHpChanged()
    {
        showHpBarTime = Time.time + 3f;
    }

    // Update is called once per frame
    private void Update()
    {
        if(ThirdPersonCameraContollor.Current ==null) return;
        //var over
        if (!(Vector3.Distance(this.transform.position, ThirdPersonCameraContollor.Current.LookPos) < 20)) return;
        //player
        if ((!(showHpBarTime > Time.time) 
             && view.TeamId != view.PerView.OwerTeamIndex) 
            || view.IsDeath 
            || !ThirdPersonCameraContollor.Current) return;
        
        if (ThirdPersonCameraContollor.Current.InView(this.transform.position))
        {
            //Debug.Log($"Print name");
            nameBar = UUITipDrawer.S.DrawUUITipNameBar(nameBar,view. Name,  view.Level,view.HP, view.HpMax,
                view. TeamId == view.PerView.OwerTeamIndex,
                view. GetBoneByName(UCharacterView. TopBone).position + Vector3.up * .05f, ThirdPersonCameraContollor.Current.currentCamera);
        }
    }
}
