public class GameKey
{
    public const string TURN_PLAYER = "turn-player";
    public const string TURN_OBJECT = "turn-object";

    public const string EVENT_FOLLOW = "event-follow";
}

public class GameTag
{
    public const string MOVE = "Move";
}

public enum TypeCharacter
{
    Angle,
    Bunny,
    Cat,
    Frog,
    Mow,
}

public enum TypeTurn
{
    Wait,
    PlayerControl,
    ObjectControl,
}