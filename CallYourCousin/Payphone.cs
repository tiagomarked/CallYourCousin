using System.Collections;
using UnityEngine;

namespace CallYourCousin;

public class Payphone : MonoBehaviour
{
    public int[] targetWaypointIndexes;

    public Transform pickTrigger;
    public Transform placeTrigger;
    public Transform coinTrigger;
    public Transform refundTrigger;
    public Transform dialTrigger;

    public AudioSource insertCoinAudio;
    public AudioSource refundAudio;

    [SerializeField] float phoneMoney;

    private void Start()
    {
        StartCoroutine(UsePhone(false));
    }

    private void OnTriggerStay()
    {
        PlayerInteractionChecks();
    }

    private IEnumerator OnMovementDisabled()
    {
        // By disabling player movement, player collision is not detected and OnTriggerStay stops working.
        // Continue interaction checks while movement is locked.
        while (Utils.playerStopped.Value)
        {
            PlayerInteractionChecks();
            yield return new WaitForEndOfFrame();
        }
    }

    private void PlayerInteractionChecks()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool showUseIcon = true;
        if (Physics.Raycast(ray, out hit))
        {
            Transform targetTransform = hit.transform;
            if (targetTransform == pickTrigger)
                OnPickTrigger();
            else if (targetTransform == placeTrigger)
                OnPlaceTrigger();
            else if (targetTransform == dialTrigger)
                OnDialTrigger();
            else if (targetTransform == coinTrigger)
                OnCoinTrigger();
            else if (targetTransform == refundTrigger)
                OnRefundTrigger();
            else
                showUseIcon = false;
        }
        else
        {
            showUseIcon = false;
        }
        Utils.showUseIcon.Value = showUseIcon;
        if (!showUseIcon && Utils.interactionText.Value != "")
            Utils.interactionText.Value = "";

    }

    private void OnPickTrigger()
    {
        Utils.interactionText.Value = Constants.Interactions.PHONE_PICK_UP;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(UsePhone());
        }
    }

    private void OnPlaceTrigger()
    {
        Utils.interactionText.Value = Constants.Interactions.PHONE_PLACE;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(UsePhone(false));
        }
    }

    private IEnumerator UsePhone(bool usingPhone = true)
    {
        Utils.playerStopped.Value = usingPhone;
        AudioSource audio = usingPhone ? Utils.phonePickAudio : Utils.phoneHangAudio;
        StartCoroutine(Utils.PlayAudioWithoutSpatialBlend(audio));
        GameObject triggerToEnable = usingPhone ? placeTrigger.gameObject : pickTrigger.gameObject;
        GameObject triggerToDisable = usingPhone ? pickTrigger.gameObject : placeTrigger.gameObject;
        triggerToDisable.SetActive(false);
        yield return new WaitForSeconds(.1f);
        triggerToEnable.SetActive(true);
        if (usingPhone)
        {
            StartCoroutine(Utils.PlayAudioWithoutSpatialBlend(Utils.phoneLoopAudio));
            StartCoroutine(OnMovementDisabled());
        }
        else
        {
            Utils.phoneLoopAudio.Stop();
            Utils.phoneNoConnectionAudio.Stop();
        }
    }


    private void OnDialTrigger()
    {
        if (Utils.callManager.onCall || Utils.phoneNoConnectionAudio.isPlaying)
            return;
        if (Utils.interactionText.Value != Constants.Interactions.PHONE_INSUFFICIENT_FUNDS)
            Utils.interactionText.Value = Constants.Interactions.PHONE_MAKE_CALL;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (phoneMoney < Constants.CALL_PRICE)
            {
                Utils.interactionText.Value = Constants.Interactions.PHONE_INSUFFICIENT_FUNDS;
                return;
            }
            placeTrigger.gameObject.SetActive(false);
            int chooseIndex = targetWaypointIndexes.Length > 1 ? UnityEngine.Random.Range(0, targetWaypointIndexes.Length) : 0;
            StartCoroutine(Utils.callManager.MakeCall(targetWaypointIndexes[chooseIndex], OnCallEnd));
            phoneMoney -= Constants.CALL_PRICE;
        }
    }

    private void OnCallEnd(bool completedCall = true)
    {
        if (!completedCall) // Refund money
            phoneMoney += Constants.CALL_PRICE;
        placeTrigger.gameObject.SetActive(true);
    }

    private void OnCoinTrigger()
    {
        if (Utils.interactionText.Value != Constants.Interactions.PLAYER_NO_MONEY)
            Utils.interactionText.Value = Constants.Interactions.PHONE_INSERT_COIN;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Utils.playerMoney.Value >= Constants.INSERT_COIN_AMOUNT)
            {
                phoneMoney += Constants.INSERT_COIN_AMOUNT;
                Utils.playerMoney.Value -= Constants.INSERT_COIN_AMOUNT;
                insertCoinAudio.Play();
            }
            else
            {
                Utils.interactionText.Value = Constants.Interactions.PLAYER_NO_MONEY;
                Utils.IncreasePlayerStress();
            }
        }
    }

    private void OnRefundTrigger()
    {
        if (Utils.interactionText.Value != Constants.Interactions.PHONE_NO_MONEY)
            Utils.interactionText.Value = Constants.Interactions.PHONE_REFUND;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (phoneMoney > 0.0f)
            {
                Utils.playerMoney.Value += phoneMoney;
                phoneMoney = 0.0f;
                refundAudio.Play();
            }
            else
            {
                Utils.interactionText.Value = Constants.Interactions.PHONE_NO_MONEY;
            }
        }
    }
}