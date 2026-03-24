import { Schema, type, MapSchema } from "@colyseus/schema";


export class Player extends Schema {
  @type("number") x = 0;
  @type("number") y = 0;
  @type("number") z = 0;
  @type("number") rotY = 0;
  @type("string") anim = "idle";
  @type("number") skin = 0;
  @type("boolean") ready = false;
  @type("number") clicks = 0;      // NEW: clicks during countdown
  @type("number") speed = 1.0;     // NEW: speed multiplier (default 1.0)
}

export class MyState extends Schema {
  @type({ map: Player })
  players = new MapSchema<Player>();

  @type("string") phase = "waiting";
  @type("number") countdown = 0;
}