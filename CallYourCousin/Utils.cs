using System.Collections;
using UnityEngine;
using System.Collections.Generic;

#if Mini

public class PretendFsmVar<T>
{
    public T value;
    public T Value
    {
        get => value;
        set => this.value = value;
    }
}

#else

using HutongGames.PlayMaker;
using MSCLoader;

#endif

namespace CallYourCousin
{
    public static class Utils
    {
        public static Transform playerTransform { get; private set; }
        public static Camera mainCamera { get; private set; }

        public static Transform traffic { get; private set; }
        public static GameObject car { get; private set; }
        public static GameObject carCrashEvent { get; private set; }
        public static GameObject blood { get; private set; }

        public static AudioSource phonePickAudio { get; private set; }
        public static AudioSource phoneLoopAudio { get; private set; }
        public static AudioSource phoneHangAudio { get; private set; }
        public static AudioSource callerHangAudio { get; private set; }
        public static AudioSource phoneNoConnectionAudio { get; private set; }

        public static CallManager callManager;
        public static GameObject homeManager;
        public static GameObject payphonePub;
        public static GameObject payphoneAuto;
        public static AudioSource hornAudio { get; private set; }
        private static bool enableStressSimulation;
#if Mini
        public static PretendFsmVar<float> annoyance { get; private set; }
        public static PretendFsmVar<int> lastResetDay { get; private set; }
        public static PretendFsmVar<int> carCurrentWaypointIndex { get; private set; }
        public static PretendFsmVar<float> playerMoney { get; private set; }
        public static PretendFsmVar<float> playerStress { get; private set; }
        public static PretendFsmVar<string> subtitle { get; private set; }
        public static PretendFsmVar<string> interactionText { get; private set; }
        public static PretendFsmVar<bool> showUseIcon { get; private set; }
        public static PretendFsmVar<bool> playerStopped { get; private set; }
        public static PretendFsmVar<bool> isRallyDay { get; private set; }
#else
        public static SavedVariable<float> annoyance = new SavedVariable<float>("annoyance", 0.0f);
        public static SavedVariable<GameTime.Days> lastResetDay = new SavedVariable<GameTime.Days>("lastResetDay", GameTime.Days.Monday);

        public static FsmGameObject carCurrentRoute { get; private set; }
        public static FsmGameObject carCurrentWaypoint { get; private set; }
        public static FsmInt carCurrentWaypointIndex { get; private set; }
        public static FsmFloat playerMoney { get; private set; }
        public static FsmFloat playerStress { get; private set; }
        public static FsmString subtitle { get; private set; }
        public static FsmString interactionText { get; private set; }
        public static FsmBool showUseIcon { get; private set; }
        public static FsmBool playerStopped { get; private set; }
        public static FsmBool isRallyDay { get; private set; }
        public static PlayMakerFSM carNavigationFSM { get; private set; }
        public static PlayMakerFSM speechFSM { get; private set; }

#endif

        public static void AssignGameSpecificObjects()
        {
            playerTransform = GameObject.Find("PLAYER").transform;
            mainCamera = Camera.main;
#if !Mini
            // Assign MSC GameObjects and Transforms
            traffic = GameObject.Find("TRAFFIC").transform;
            car = traffic.Find("VehiclesDirtRoad/Rally/FITTAN").gameObject;
            carCrashEvent = car.transform.Find("CrashEvent").gameObject;
            blood = carCrashEvent.transform.Find("Blood").gameObject;

            // Assign Global FSM vars
            playerMoney = FsmVariables.GlobalVariables.GetFsmFloat("PlayerMoney");
            playerStress = FsmVariables.GlobalVariables.GetFsmFloat("PlayerStress");
            subtitle = FsmVariables.GlobalVariables.GetFsmString("GUIsubtitle");
            interactionText = FsmVariables.GlobalVariables.GetFsmString("GUIinteraction");
            showUseIcon = FsmVariables.GlobalVariables.GetFsmBool("GUIuse");
            playerStopped = FsmVariables.GlobalVariables.GetFsmBool("PlayerStop");
            isRallyDay = FsmVariables.GlobalVariables.GetFsmBool("RallyDay");
            speechFSM = playerTransform.Find("Pivot/AnimPivot/Camera/FPSCamera/SpeakDatabase").GetComponent<PlayMakerFSM>();
            carNavigationFSM = car.transform.Find("Navigation").gameObject.GetComponent<PlayMakerFSM>();

            carCurrentRoute = carNavigationFSM.FsmVariables.FindFsmGameObject("RouteCurrent");
            carCurrentWaypointIndex = carNavigationFSM.FsmVariables.GetFsmInt("WaypointStart");
            carCurrentWaypoint = carNavigationFSM.FsmVariables.FindFsmGameObject("Waypoint");
#endif

            // Game Audio Systems
            GameObject masterAudio = GameObject.Find("MasterAudio");
            phoneLoopAudio = masterAudio.transform.Find("Callers/phone_loopcall").GetComponent<AudioSource>();
            phonePickAudio = masterAudio.transform.Find("HouseFoley/phone_pick_up").GetComponent<AudioSource>();
            phoneHangAudio = masterAudio.transform.Find("HouseFoley/phone_hang_up").GetComponent<AudioSource>();
            callerHangAudio = masterAudio.transform.Find("Callers/phone_caller_hangup").GetComponent<AudioSource>();
            phoneNoConnectionAudio = masterAudio.transform.Find("Callers/phone_noconnection").GetComponent<AudioSource>();
        }

        public static bool EnableStressSimulation(bool enable)
        {
#if Mini
            return false;
#else
            enableStressSimulation = enable;
            return enableStressSimulation;
#endif
        }

        public static void AddHorn(AudioSource horn)
        {
            horn.transform.SetParent(car.transform);
            horn.transform.localPosition = horn.transform.localEulerAngles = Vector3.zero;
            hornAudio = horn;
        }

        public static IEnumerator PlayAudioWithoutSpatialBlend(AudioSource audioSource)
        {
            float spatialBlend = audioSource.spatialBlend;
            audioSource.spatialBlend = 0;
            audioSource.Play();
            while (audioSource.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            audioSource.spatialBlend = spatialBlend;
        }

        public static float GetCarDistanceFromPlayer()
        {
            return Vector3.Distance(car.transform.position, playerTransform.position);
        }

        public static void SetCarWaypoint(GameObject route, Transform waypoint)
        {

        }

        public static float IncreaseAnnoyance(float amount)
        {
            annoyance.Value = Mathf.Clamp(annoyance.Value + amount, 0.0f, 100.0f);
#if DEBUG
            ModConsole.Print("Cousin annoyance: " + annoyance.Value.ToString());
#endif
            return annoyance.Value;
        }

        public static float IncreasePlayerStress(float amount = 0.0f)
        {
            if (enableStressSimulation)
            {
                if (amount == 0.0f)
                    amount = UnityEngine.Random.Range(1.0f, 5.0f);
                playerStress.Value += amount;
#if !Mini
                speechFSM.SendEvent("SWEARING");
#endif
            }
            return playerStress.Value;
        }

    }
}

#if !Mini
public abstract class SavedVariableBase
{
    private static List<SavedVariableBase> allInstances = new List<SavedVariableBase>();

    protected SavedVariableBase()
    {
        allInstances.Add(this);
    }

    public abstract void ResetToDefault();
    public abstract void LoadValue(Mod mod);
    public abstract void SaveValue(Mod mod);
    public static void OnNewGame()
    {
        for (int i = 0; i < allInstances.Count; i++)
        {
            SavedVariableBase instance = allInstances[i];
            instance.ResetToDefault();
        }
    }

    public static void OnLoad(Mod mod)
    {
        for (int i = 0; i < allInstances.Count; i++)
        {
            SavedVariableBase instance = allInstances[i];
            instance.LoadValue(mod);
        }
        ModConsole.Print("Successfully loaded " + mod.Name + " variables from save file.");
    }

    public static void OnSave(Mod mod)
    {
        for (int i = 0; i < allInstances.Count; i++)
        {
            SavedVariableBase instance = allInstances[i];
            instance.SaveValue(mod);
        }
        ModConsole.Print("Successfully saved " + mod.Name + " variables to mods save file.");
    }
}

public class SavedVariable<T> : SavedVariableBase
{
    private static List<SavedVariable<T>> instances = new List<SavedVariable<T>>();
    private string id;
    private T defaultValue;
    private T value;

    public SavedVariable(string id, T value) : base()
    {
        this.id = id;
        this.defaultValue = this.value = value;
        instances.Add(this);
    }

    public T Value
    {
        get => value;
        set => this.value = value;
    }

    public override void ResetToDefault()
    {
        this.value = this.defaultValue;
#if DEBUG
        ModConsole.Print("Call Your Cousin: Reset Variable " + id + " | value: " + this.value.ToString());
#endif
    }

    public override void LoadValue(Mod mod)
    {
        this.value = SaveLoad.ReadValue<T>(mod, id);
#if DEBUG
        ModConsole.Print("Call Your Cousin: Loaded Variable " + id + " | value: " + this.value.ToString());
#endif
    }

    public override void SaveValue(Mod mod)
    {
        SaveLoad.WriteValue(mod, id, value);
#if DEBUG
        ModConsole.Print("Call Your Cousin: Saved Variable " + id + " | value: " + this.value.ToString());
#endif
    }
}
#endif