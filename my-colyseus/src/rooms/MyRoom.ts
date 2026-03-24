import { Room, Client } from "colyseus";
import { MyState, Player } from "./schema/MyRoomState.js";

const DEFAULT_SPEED = 5.0; // must match PlayerMovement.speed in Unity

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

    this.onMessage("player_ready", (client) => {
      const p = this.s.players.get(client.sessionId);
      if (!p) return;
      p.ready = true;
      console.log(client.sessionId, "is ready");
      this.checkAllReady();
    });

    this.onMessage("player_unready", (client) => {
      const p = this.s.players.get(client.sessionId);
      if (!p) return;
      p.ready = false;
      console.log(client.sessionId, "is NOT ready");
    });

    // NEW: player clicked during countdown
    this.onMessage("click", (client) => {
      const p = this.s.players.get(client.sessionId);
      if (!p) return;

      // only count clicks during countdown phase
      if (this.s.phase !== "countdown") return;

      p.clicks++;
      console.log(client.sessionId, "clicks:", p.clicks);
    });
  }

  checkAllReady() {
    const players = Array.from(this.s.players.values());
    if (players.length >= 1 && players.every(p => p.ready)) {
      this.startCountdown();
    }
  }

  startCountdown() {
    if (this.s.phase !== "waiting") return;

    // reset clicks for all players at start of countdown
    this.s.players.forEach((p) => {
      p.clicks = 0;
      p.speed = 1.0;
    });

    this.s.phase = "countdown";
    let t = 10;
    this.s.countdown = t;
    console.log("Countdown started!");

    const interval = this.clock.setInterval(() => {
      t--;
      this.s.countdown = t;
      if (t <= 0) {
        interval.clear();

        // calculate speed for each player: clicks * DEFAULT_SPEED
        this.s.players.forEach((p) => {
          p.speed = p.clicks * DEFAULT_SPEED;
          // minimum speed so player always moves even if they didn't click
          if (p.speed < DEFAULT_SPEED) p.speed = DEFAULT_SPEED;
          console.log("Player speed set to:", p.speed, "(clicks:", p.clicks + ")");
        });

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