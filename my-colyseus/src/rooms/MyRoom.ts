import { Room, Client } from "colyseus";
import { MyState, Player } from "./schema/MyRoomState.js";




export class MyRoom extends Room {
  private get s(): MyState {
    return this.state as MyState;
  }

  onCreate(options: any) {
    this.setState(new MyState());
    console.log("ROOM CREATED:", this.roomId);

    this.onMessage("move", (client: Client, data: any) => {

      //console.log("MOVE", client.sessionId, data);

      const p = this.s.players.get(client.sessionId);
      if (!p) return;

      p.x = data.x ?? p.x;
      p.y = data.y ?? p.y;
      p.z = data.z ?? p.z;
      p.rotY = data.rotY ?? p.rotY;
      p.anim = data.anim ?? p.anim;

      this.onMessage("skin", (client, data) => {
        //console.log("SKIN", client.sessionId, data);
        const p = this.s.players.get(client.sessionId);
        if (!p) return;
          p.skin = data.skin;
      });

    });
  }

  onJoin(client: Client) {
    console.log("JOIN:", client.sessionId, "roomId:", this.roomId);

    const p = new Player();
    p.x = Math.random() * 4;
    p.z = Math.random() * 4;

    this.s.players.set(client.sessionId, p);
  }

  onLeave(client: Client) {
    console.log("LEAVE:", client.sessionId);
    this.s.players.delete(client.sessionId);
  }

  
}
