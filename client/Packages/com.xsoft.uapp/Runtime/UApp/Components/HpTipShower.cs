using BattleViews.Components;
using BattleViews.Views;
using UnityEngine;

[RequireComponent(typeof(UCharacterView))]
public class HpTipShower : MonoBehaviour
{
    public UCharacterView view;
    private int _nameBar = -1;
    private float _showHpBarTime = -1;

    // Start is called before the first frame update
    private void Awake()
    {
        view = GetComponent<UCharacterView>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (ThirdPersonCameraContollor.Current == null) return;
        //var over
        if (!(Vector3.Distance(transform.position, ThirdPersonCameraContollor.Current.LookPos) < 20)) return;
        //player
        if ((!(_showHpBarTime > Time.time)
             && view.TeamId != view.PerView.OwnerTeamIndex)
            || view.IsDeath
            || !ThirdPersonCameraContollor.Current) return;

        if (ThirdPersonCameraContollor.Current.InView(transform.position))
            //Debug.Log($"Print name");
            _nameBar = UUITipDrawer.S.DrawUUITipNameBar(_nameBar, view.Name, view.Level, view.HP, view.HpMax,
                view.TeamId == view.PerView.OwnerTeamIndex,
                view.GetBoneByName(UCharacterView.TopBone).position + Vector3.up * .05f,
                ThirdPersonCameraContollor.Current.currentCamera);
    }


    private void OnDead()
    {
        _showHpBarTime = -1;
    }

    private void OnHpChanged()
    {
        _showHpBarTime = Time.time + 3f;
    }
}