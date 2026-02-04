import { Room, Client } from "colyseus";
import { MyState, Player } from "./schema/MyRoomState.js";

export class MyRoom extends Room {

  private get s(): MyState {
    return this.state as MyState;
  }

  onCreate(options: any) {
    this.setState(new MyState());

    this.onMessage("move", (client: Client, data: any) => {
      const p = this.s.players.get(client.sessionId);
      if (!p) return;

      p.x = data.x;
      p.y = data.y;
      p.z = data.z;
      p.rotY = data.rotY;
      p.anim = data.anim ?? "idle";
    });
  }

  onJoin(client: Client) {
    this.s.players.set(client.sessionId, new Player());
  }

  onLeave(client: Client) {
    this.s.players.delete(client.sessionId);
  }
}
