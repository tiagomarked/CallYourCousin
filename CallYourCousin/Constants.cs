namespace CallYourCousin;

public class Constants
{
    public const float INSERT_COIN_AMOUNT = 1.0f;
    public const float CALL_PRICE = 5.0f;
    public const int SPAWN_WAYPOINT_DISTANCE = 200;
    public const float WAIT_SECONDS = 60.0f;
    public const float WAIT_MIN_DISTANCE = 10.0f;
    public const float WAIT_MAX_DISTANCE = 50.0f;
#if !DEBUG
    public const float FORGET_SECONDS = 300.0f; // 5 minutes
#else
    public const float FORGET_SECONDS = 30.0f;
#endif

    public class Interactions
    {
        public const string NOTE_READ = "Read Note";
        public const string PHONE_PICK_UP = "Pick Up Phone";
        public const string PHONE_PLACE = "Place Phone Back";
        public const string PHONE_NO_MONEY = "No Money On Phone";
        public const string PHONE_INSUFFICIENT_FUNDS = "Not Enough Money On Phone";
        public const string PLAYER_NO_MONEY = "Not Enough Money";
        public const string PHONE_MAKE_CALL = "Call Your Cousin";
        public const string PHONE_INSERT_COIN = "Insert 1mk";
        public const string PHONE_REFUND = "Get Coins";
    }

    public class Subtitles
    {
        public const string NOTE_TEXT = "\"Why did you write his number down? You know how much he drinks while driving. Be careful!\"";
        public const string NO_SIGNAL = "*I don't have any signal.*";
        public const string COUSIN_NO_ANSWER = "*No answer.*";
        public const string COUSIN_RALLY = "It's rally day, I can't go right now the roads are closed!";
        public const string COUSIN_NEARBY = "I'm already nearby!";
        public const string COUSIN_MID_PICKUP = "I'm already going to pick you up! There's no need to call me again.";
        public const string COUSIN_MISSED_PICKUP = "I went to pick you up and you didn't get in! Don't bother me again!";
        public const string COUSIN_ANNOYED = "Stop calling me, I have other stuff to do!";
        public const string COUSIN_CONFIRM_PICKUP = "You want me to pick you up? Ok, I'm on my way.";

    }
}