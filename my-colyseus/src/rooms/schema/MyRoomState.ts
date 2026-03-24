import { Schema, type, MapSchema } from "@colyseus/schema";


export class Player extends Schema {
  @type("number") x = 0;
  @type("number") y = 0;
  @type("number") z = 0;
  @type("number") rotY = 0;
  @type("string") anim = "idle"; // idle/walk/sit
  @type("number") skin = 0;
  @type("boolean") ready = false;  // NEW: is player in the ready zone?
}

export class MyState extends Schema {
  @type({ map: Player })
  players = new MapSchema<Player>();

  @type("string") phase = "waiting";    // NEW: waiting | countdown | racing
  @type("number") countdown = 0;        // NEW: countdown timer value
}