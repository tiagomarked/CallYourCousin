#if !Mini
using HutongGames.PlayMaker;
#endif
using UnityEngine;
using System.Collections;

namespace CallYourCousin;

public class Home : MonoBehaviour
{
    public int targetWaypointIndex;
    public Transform noteTrigger;
    bool playerInTriggerArea = false;

#if !Mini
    GameObject electricAppliances;
    GameObject phoneRing;
    FsmBool phonePaid;
    FsmBool phoneInUse;
    FsmFloat raycastDistance;
#endif

    private void Start()
    {
#if !Mini
        phonePaid = GameObject.Find("Systems").transform.Find("PhoneBills").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("PhonePaid");
        Transform building = GameObject.Find("YARD").transform.Find("Building");
        electricAppliances = building.Find("Dynamics/HouseElectricity/ElectricAppliances").gameObject;
        phoneRing = building.Find("LIVINGROOM/Telephone/Logic/Ring").gameObject;

        GameObject phoneHandle = GameObject.Find("Telephone").transform.Find("Logic/UseHandle").gameObject;
        PlayMakerFSM phoneHandleFSM = phoneHandle.GetComponent<PlayMakerFSM>();
        phoneInUse = phoneHandleFSM.FsmVariables.FindFsmBool("Use");
        raycastDistance = phoneHandleFSM.FsmVariables.FindFsmFloat("Raycast");
#endif
    }

    private void OnTriggerStay()
    {
        if (!playerInTriggerArea)
            StartCoroutine(PlayerInteractionChecks());
    }

    private void OnTriggerExit()
    {
        playerInTriggerArea = false;
    }


    private IEnumerator PlayerInteractionChecks()
    {
        playerInTriggerArea = true;
        while (playerInTriggerArea)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Transform targetTransform = hit.transform;
                if (targetTransform == noteTrigger)
                    OnNoteTrigger();
                else
                {
                    if (Utils.interactionText.Value == Constants.Interactions.NOTE_READ || Utils.interactionText.Value == Constants.Interactions.PHONE_MAKE_CALL)
                        Utils.interactionText.Value = "";
                    if (Utils.subtitle.Value == Constants.Subtitles.NOTE_TEXT)
                        Utils.subtitle.Value = "";
                }

            }
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnNoteTrigger()
    {
#if Mini
        if (!Utils.callManager.onCall)
            Debug.Log("On Note Trigger");
#else
        if (phoneRing.activeSelf || Utils.phoneNoConnectionAudio.isPlaying || Utils.callManager.onCall)
            return;
        Utils.showUseIcon.Value = true;
        Utils.interactionText.Value = phoneInUse.Value ? Constants.Interactions.PHONE_MAKE_CALL : Constants.Interactions.NOTE_READ;
#endif
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnNoteInteract();
        }
    }
    private void OnNoteInteract()
    {
#if Mini
        Debug.Log("On Note Interact");
#else
        if (!phoneInUse.Value)
        {
            Utils.subtitle.Value = Constants.Subtitles.NOTE_TEXT;
            return;
        }
        if (!phonePaid.Value || !electricAppliances.activeSelf)
        {
            Utils.IncreasePlayerStress();
            Utils.subtitle.Value = Constants.Subtitles.NO_SIGNAL;
            return;
        }

        raycastDistance.Value = 0.0f;
        if (Utils.subtitle.Value == Constants.Subtitles.NOTE_TEXT)
            Utils.subtitle.Value = "";
#endif
        StartCoroutine(Utils.callManager.MakeCall(targetWaypointIndex, OnCallEnd, homeRoute: true));
    }

    private void OnCallEnd(bool completedCall = true)
    {
#if Mini
        Debug.Log("On Call End");
#else
        raycastDistance.Value = 7.0f;
#endif
    }
}