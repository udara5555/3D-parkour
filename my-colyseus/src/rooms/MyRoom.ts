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
      const p = this.s.players.get(client.sessionId);
      if (!p) return;

      p.x = data.x ?? p.x;
      p.y = data.y ?? p.y;
      p.z = data.z ?? p.z;
      p.rotY = data.rotY ?? p.rotY;
      p.anim = data.anim ?? p.anim;
    });

    this.onMessage("skin", (client, data) => {
      const p = this.s.players.get(client.sessionId);
      if (!p) return;
      p.skin = data.skin;
    });

    // NEW: player entered the ready zone
    this.onMessage("player_ready", (client) => {
      const p = this.s.players.get(client.sessionId);
      if (!p) return;
      p.ready = true;
      console.log(client.sessionId, "is ready");
      this.checkAllReady();
    });

    // NEW: player left the ready zone
    this.onMessage("player_unready", (client) => {
      const p = this.s.players.get(client.sessionId);
      if (!p) return;
      p.ready = false;
      console.log(client.sessionId, "is NOT ready");
    });
  }

  // NEW: check if all players are ready → start countdown
  checkAllReady() {
    const players = Array.from(this.s.players.values());

    if (players.length >= 1 && players.every(p => p.ready)) {
      this.startCountdown();
    }
  }

  // NEW: 5 second countdown then switch to racing
  startCountdown() {
    if (this.s.phase !== "waiting") return; // prevent double trigger

    this.s.phase = "countdown";
    let t = 5;
    this.s.countdown = t;
    console.log("Countdown started!");

    const interval = this.clock.setInterval(() => {
      t--;
      this.s.countdown = t;
      if (t <= 0) {
        interval.clear();
        this.s.phase = "racing";
        console.log("RACE STARTED!");
      }
    }, 1000);
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

    // NEW: if someone leaves during countdown, reset back to waiting
    if (this.s.phase === "countdown") {
      const remaining = Array.from(this.s.players.values());
      if (remaining.length < 2) {
        this.s.phase = "waiting";
        this.s.countdown = 0;
        console.log("Not enough players, resetting to waiting");
      }
    }
  }
}