using Colyseus;
using UnityEngine;

public class ColyseusManager : MonoBehaviour
{
    ColyseusClient client;
    ColyseusRoom<MyRoomState> room;

    async void Start()
    {
        client = new ColyseusClient("http://localhost:2567");
        room = await client.JoinOrCreate<MyRoomState>("my_room");
        Debug.Log("Connected: " + room.Id);
    }
}
