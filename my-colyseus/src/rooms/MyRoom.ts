import { Room, Client } from "@colyseus/core";
import { MyState, Player } from "./schema/MyRoomState.js";

export class MyRoom extends Room<{ state: MyState }> {

  onCreate() {
    this.setState(new MyState());

    this.onMessage("move", (client: Client, data: { x: number; y: number; z: number; rotY: number }) => {
      const p = this.state.players.get(client.sessionId);
      if (!p) return;

      p.x = data.x;
      p.y = data.y;
      p.z = data.z;
      p.rotY = data.rotY;
    });
  }

  onJoin(client: Client) {
    this.state.players.set(client.sessionId, new Player());
  }

  onLeave(client: Client) {
    this.state.players.delete(client.sessionId);
  }
}
