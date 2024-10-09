#if !Mini
using MSCLoader;
#endif
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CallYourCousin
{

    public class CousinResponse
    {
        public bool available;
        public string message;
        public bool completedCall;
        public CousinResponse(bool available, string message, bool completedCall = true)
        {
            this.available = available;
            this.message = message;
            this.completedCall = completedCall;

        }
    }

    public class CallManager : MonoBehaviour
    {
        public AudioSource dialSound;
        public bool onCall { get; private set; } = false;
        bool rideRequested = false;
        bool missedRide = false;

        void Start()
        {
            TimeScheduler.ScheduleAction(0, 0, DecreaseAnnoyance);
        }

        void DecreaseAnnoyance()
        {
            if (GameTime.Day == Utils.lastResetDay.Value)
                return;
#if DEBUG
            ModConsole.Print("Running Decrease Annoyance");
#endif
            Utils.IncreaseAnnoyance(-UnityEngine.Random.Range(5, 30));
            Utils.lastResetDay.Value = GameTime.Day;
        }

        public IEnumerator MakeCall(int waypoint, Action<bool> onCallEnd, bool homeRoute = false)
        {
            onCall = true;
            dialSound.Play();
            while (dialSound.isPlaying)
            {
                if (Utils.phoneLoopAudio.isPlaying)
                    Utils.phoneLoopAudio.Stop();
                yield return new WaitForEndOfFrame();
            }

            CousinResponse response = GetCousinResponse();
            Utils.subtitle.Value = response.message;

            if (response.available)
            {
                StartCoroutine(GoToPickup(waypoint, homeRoute));
                Utils.IncreaseAnnoyance(UnityEngine.Random.Range(5.0f, 20.0f));
            }

            if (response.completedCall)
            {
                Utils.IncreaseAnnoyance(UnityEngine.Random.Range(0.0f, 5.0f));
                yield return new WaitForSeconds(3.0f);
                StartCoroutine(Utils.PlayAudioWithoutSpatialBlend(Utils.callerHangAudio));
                while (Utils.callerHangAudio.isPlaying)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            if (!response.available)
            {
                Utils.IncreasePlayerStress();
                if (!response.completedCall)
                    Utils.subtitle.Value = response.message;
            }

            onCall = false;
            StartCoroutine(Utils.PlayAudioWithoutSpatialBlend(Utils.phoneNoConnectionAudio));
            onCallEnd(response.completedCall);
        }

        CousinResponse GetCousinResponse()
        {
            bool dontPickUp = Utils.blood.activeSelf; // If cousin is dead (blood object active), don't pick up.
            if (!dontPickUp)
            {
                // if cousin is alive, calculate chance to pick up the phone based on how annoyed he is at you.
                int diceRoll = UnityEngine.Random.Range(0, Mathf.Clamp((int)Utils.annoyance.Value, 35, 80));
#if DEBUG
                ModConsole.Print("Pick Up Dice Roll: " + diceRoll.ToString() + " || Cousin Annoyance: " + Utils.annoyance.Value);
#endif
                dontPickUp = diceRoll > 30;
            }

            if (dontPickUp)
                return new CousinResponse(false, Constants.Subtitles.COUSIN_NO_ANSWER, completedCall: false);

            if (Utils.isRallyDay.Value)
                return new CousinResponse(false, Constants.Subtitles.COUSIN_RALLY);

            if (missedRide)
                return new CousinResponse(false, Constants.Subtitles.COUSIN_MISSED_PICKUP);

            float cousinDistanceFromPlayer = Utils.GetCarDistanceFromPlayer();
            if (cousinDistanceFromPlayer < 90f)
                return new CousinResponse(false, Constants.Subtitles.COUSIN_NEARBY);

            if (rideRequested)
                return new CousinResponse(false, Constants.Subtitles.COUSIN_MID_PICKUP);

            if (Utils.annoyance.Value >= 100f)
                return new CousinResponse(false, Constants.Subtitles.COUSIN_ANNOYED);

            return new CousinResponse(true, Constants.Subtitles.COUSIN_CONFIRM_PICKUP);
        }


        IEnumerator GoToPickup(int targetWaypointIndex, bool homeRoute = false)
        {
#if Mini
            Debug.Log("Go To Pickup");
            yield return null;
#else
            rideRequested = true;
            string routeName = homeRoute ? "RouteHome" : "RouteRoad";
            GameObject route = Utils.carNavigationFSM.FsmVariables.FindFsmGameObject(routeName).Value;
            yield return StartCoroutine(CartToWaypoint(route, targetWaypointIndex, spawnOffset: Constants.SPAWN_WAYPOINT_DISTANCE));
            yield return StartCoroutine(CheckForCarArrival(route, targetWaypointIndex));
            rideRequested = false;
        }


        IEnumerator CartToWaypoint(GameObject route, int targetWaypointIndex, int spawnOffset = 0)
        {
#if Mini
            Debug.Log("Car To Waypoint");
            yield return null;
#else
            Transform fittanTransform = Utils.car.transform;
            Rigidbody fittanRigidbody = fittanTransform.GetComponent<Rigidbody>();

            int spawnWaypointIndex = Mathf.Max(0, targetWaypointIndex - spawnOffset);

            Transform spawnWaypointTransform = route.transform.Find(spawnWaypointIndex.ToString());

            if (Utils.carCurrentRoute.Value != route)
            {
                Utils.carCurrentRoute.Value = route;
                Utils.carNavigationFSM.SendEvent("RESET");
            }

            float stopTimer = 0.0f;
            while (stopTimer < 2.0f)
            {
                // Apparently RESET event can force waypoints, so set them every fixed update on this loop
                Utils.carCurrentWaypointIndex.Value = Mathf.Min(spawnWaypointIndex + 3, route.transform.childCount - 1);
                Utils.carCurrentWaypoint.Value = spawnWaypointTransform.gameObject;
                StopCar();
                stopTimer += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            fittanRigidbody.isKinematic = true;
            yield return new WaitForFixedUpdate();

            fittanTransform.position = spawnWaypointTransform.position + new Vector3(0.0f, 0.25f, 0.0f);
            fittanTransform.rotation = spawnWaypointTransform.rotation;

            yield return new WaitForFixedUpdate();
            fittanRigidbody.isKinematic = false;

            StopCar(false);
#endif
        }

        IEnumerator CheckForCarArrival(GameObject route, int targetWaypointIndex)
        {
            int spawnWaypointIndex = Utils.carCurrentWaypointIndex.Value;
            Vector3 comparePosition = Utils.car.transform.position;

            // Wait for car to arrive at the desired waypoint.
            while (Utils.carCurrentWaypointIndex.Value < targetWaypointIndex)
            {
                if (Utils.carCurrentWaypointIndex.Value < spawnWaypointIndex)
                    Utils.carCurrentWaypointIndex.Value = spawnWaypointIndex;
                yield return new WaitForFixedUpdate();
            }

            // Car has arrived at waypoint
            Utils.hornAudio.Play();

            // If player spends more than WAIT_SECONDS, WAIT_MIN_DISTANCE meters away from the car, consider he didn't show up for the ride.
            // Otherwise, if player is more than WAIT_MAX_DISTANCE away from the car, also consider he didn't show up and don't wait.
            bool enteredCar = false;
            float waitTimer = 0.0f;
            while (waitTimer < Constants.WAIT_SECONDS)
            {
                StopCar();
                if (Utils.playerTransform.root == Utils.traffic)
                {
                    enteredCar = true;
                    break;
                }
                float distanceFromPlayer = Vector3.Distance(Utils.car.transform.position, Utils.playerTransform.position);
                if (distanceFromPlayer > Constants.WAIT_MAX_DISTANCE)
                    break;
                else if (distanceFromPlayer > Constants.WAIT_MIN_DISTANCE)
                    waitTimer += Time.deltaTime;
                else
                    waitTimer = 0.0f;
                yield return new WaitForFixedUpdate();
            }

            StopCar(false);
            // Called for pick up but didn't get in.
            if (!enteredCar)
            {
                missedRide = true;
                Utils.IncreaseAnnoyance(UnityEngine.Random.Range(15.0f, 50.0f));
                StartCoroutine(ForgetMissedRide());
            }

#endif
        }

        private void StopCar(bool stop = true)
        {
#if Mini
            Debug.Log("StopCar");
#else
            PlayMakerFSM throttleController = Utils.car.GetComponents<PlayMakerFSM>().Where(fsm => fsm.FsmName == "Throttle").Single();
            throttleController.Fsm.GetFsmBool("PlayerStop").Value = stop;
            string eventName = stop ? "FULLSTOP" : "CRUISE";
            throttleController.SendEvent(eventName);
#endif
        }

        IEnumerator ForgetMissedRide()
        {
            float elapsedTime = 0.0f;
            while (elapsedTime < Constants.FORGET_SECONDS)
            {
                elapsedTime += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            missedRide = false;
        }
    }
}