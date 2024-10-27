#if !Mini
using HutongGames.PlayMaker;
using MSCLoader;
#endif
using System.Collections;
using UnityEngine;

namespace CallYourCousin;

public class TestingCheats : MonoBehaviour
{
    bool fastforward = false;
    bool followCousin = false;

    void Start()
    {
#if !Mini
        ModConsole.Warning("Call Your Cousin: Testing Cheats Enabled!");
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
            TogglePlayerMoney();
        else if (Input.GetKeyDown(KeyCode.F7))
            ToggleFollowCousin();
        else if (Input.GetKeyDown(KeyCode.F8))
            ToggleFastForward();
        else if (Input.GetKeyDown(KeyCode.F9))
            MakePlayerTired();
        else if (Input.GetKeyDown(KeyCode.F10))
            PlayerToHome();
        else if (Input.GetKeyDown(KeyCode.F11))
            PlayerToPub();
        else if (Input.GetKeyDown(KeyCode.F12))
            PlayerToAutoShop();
    }

    void TogglePlayerMoney()
    {
        bool noMoney = Utils.playerMoney.Value < 1;
        Utils.playerMoney.Value = noMoney ? 3000.0f : 0.0f;
    }

    void ToggleFollowCousin()
    {
        if (followCousin)
            followCousin = false;
        else
        {
            StartCoroutine(FollowCousin());
        }
    }


    IEnumerator FollowCousin()
    {
        Vector3 originalPos = Utils.playerTransform.position;
        followCousin = true;
        while (followCousin)
        {
            Utils.playerTransform.position = Utils.car.transform.position + new Vector3(0, 4, 0);
            yield return new WaitForEndOfFrame();
        }
        Utils.playerTransform.position = originalPos;
    }

    void ToggleFastForward()
    {
        fastforward = !fastforward;
        Time.timeScale = fastforward ? 3.0f : 1.0f;
    }

    void MakePlayerTired()
    {
#if !Mini
        FsmVariables.GlobalVariables.FindFsmFloat("PlayerHunger").Value = 0;
        FsmVariables.GlobalVariables.FindFsmFloat("PlayerThirst").Value = 0;
        FsmVariables.GlobalVariables.FindFsmFloat("PlayerUrine").Value = 0;
        FsmVariables.GlobalVariables.FindFsmFloat("PlayerStress").Value = 0;
        FsmVariables.GlobalVariables.FindFsmFloat("PlayerDirtiness").Value = 0;
        FsmVariables.GlobalVariables.FindFsmFloat("PlayerFatigue").Value = 100;
#endif
    }

    void PlayerToHome()
    {
        Utils.playerTransform.position = new Vector3(-8.84f, 0.075f, 9.08f);
    }

    void PlayerToPub()
    {
        Utils.playerTransform.position = Utils.payphonePub.transform.position + new Vector3(-1, 2, -1);
    }

    void PlayerToAutoShop()
    {
        Utils.playerTransform.position = Utils.payphoneAuto.transform.position + new Vector3(-1, 2, -1);
    }
}