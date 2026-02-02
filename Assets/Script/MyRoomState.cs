using Colyseus.Schema;

public class Player : Schema
{
    [Type(0, "number")] public float x = 0;
    [Type(1, "number")] public float y = 0;
}

public class MyRoomState : Schema
{
    [Type(0, "map", typeof(MapSchema<Player>))]
    public MapSchema<Player> players = new MapSchema<Player>();
}
