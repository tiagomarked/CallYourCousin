#if !Mini
using MSCLoader;
using System;
using UnityEngine;

namespace CallYourCousin;

public class CallYourCousin : Mod
{
    public override string ID => "CallYourCousin";
    public override string Name => "Call Your Cousin";
    public override string Author => "sykovanz";
    public override string Version => "2.0";
    public override string Description => "Call Your Cousin™️ from your home phone or the two payphones located near Teimo's pub and Freetari's to get a free ride!";
    public static string buildUUID = Guid.NewGuid().ToString();

    public override void ModSetup()
    {
        SetupFunction(Setup.OnNewGame, Mod_OnNewGame);
        SetupFunction(Setup.PreLoad, Mod_Preload);
        SetupFunction(Setup.OnLoad, Mod_OnLoad);
        SetupFunction(Setup.OnSave, Mod_OnSave);
        SetupFunction(Setup.ModSettings, Mod_Settings);
    }

    SettingsCheckBox enableTownPayphoneCheckbox;
    SettingsCheckBox enableFleetariPayphoneCheckbox;
    SettingsCheckBox enableStressSimulation;
    private void Mod_Settings()
    {
        Settings.AddText(this, "Any changes are only applied the next time you load a save.");
        enableTownPayphoneCheckbox = Settings.AddCheckBox(this, "enableTownPayphoneCheckbox", "Enable Town Payphone", true);
        enableFleetariPayphoneCheckbox = Settings.AddCheckBox(this, "enableFleetariPayphoneCheckbox", "Enable Fleetari Payphone", true);
        enableStressSimulation = Settings.AddCheckBox(this, "enableStressSimulation", "Enable Stress Simulation", true);
    }

    private void Mod_OnNewGame()
    {
        SavedVariableBase.OnNewGame();
    }

    private void Mod_Preload()
    {
        TimeScheduler.StartScheduler();
    }

    private void Mod_OnLoad()
    {
        TimeScheduler.LoadScheduler();
        SavedVariableBase.OnLoad(this);

        Utils.AssignGameSpecificObjects();
        Utils.EnableStressSimulation(enableStressSimulation.GetValue());

        AssetBundle ab = LoadAssets.LoadBundle(Properties.Resources.callyourcousin_assets);
        GameObject controllerPrefab = ab.LoadAsset<GameObject>("CallYourCousinController.prefab");
        GameObject cycController = GameObject.Instantiate(controllerPrefab);
        Utils.callManager = cycController.GetComponent<CallManager>();
#if DEBUG
        cycController.AddComponent<TestingCheats>();
#endif
        GameObject homeManagerPrefab = ab.LoadAsset<GameObject>("CycHomeManager.prefab");
        Utils.homeManager = GameObject.Instantiate(homeManagerPrefab);

        GameObject payphonePubPrefab = ab.LoadAsset<GameObject>("PayphonePub.prefab");
        Utils.payphonePub = GameObject.Instantiate(payphonePubPrefab);
        Utils.payphonePub.SetActive(enableTownPayphoneCheckbox.GetValue());

        GameObject payphoneAutoPrefab = ab.LoadAsset<GameObject>("PayphoneAuto.prefab");
        Utils.payphoneAuto = GameObject.Instantiate(payphoneAutoPrefab);
        Utils.payphoneAuto.SetActive(enableFleetariPayphoneCheckbox.GetValue());

        GameObject hornPrefab = ab.LoadAsset<GameObject>("HornCYC.prefab");
        AudioSource horn = GameObject.Instantiate(hornPrefab).GetComponent<AudioSource>();
        Utils.AddHorn(horn);

        ab.Unload(false);
    }

    private void Mod_OnSave()
    {
        SavedVariableBase.OnSave(this);
        TimeScheduler.SaveScheduler();
        TimeScheduler.StopScheduler();
    }
}

#endif