import { Room, Client } from "colyseus";
import { MyState, Player } from "./schema/MyRoomState.js";


export class MyRoom extends Room<{ state: MyState }> {
  onCreate(options: any) {
    this.setState(new MyState());

    this.onMessage("move", (client: Client, data: any) => {
      const p = this.state.players.get(client.sessionId);
      if (!p) return;

      p.x = data.x;
      p.y = data.y;
      p.z = data.z;
      p.rotY = data.rotY;
      p.anim = data.anim ?? "idle";
    });
  }

  onJoin(client: Client) {
    this.state.players.set(client.sessionId, new Player());
  }

  onLeave(client: Client) {
    this.state.players.delete(client.sessionId);
  }
}
