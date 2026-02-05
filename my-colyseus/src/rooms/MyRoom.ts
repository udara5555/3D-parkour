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
    console.log("JOIN:", client.sessionId);
    const p = new Player();
    p.x = Math.random() * 4;
    p.z = Math.random() * 4;
    this.s.players.set(client.sessionId, p);
  }

  onAuth(client: Client, options: any) {
  console.log("AUTH:", client.sessionId);
  return true;
}

  onLeave(client: Client) {
    console.log("LEAVE:", client.sessionId);
    this.s.players.delete(client.sessionId);
  }
}
