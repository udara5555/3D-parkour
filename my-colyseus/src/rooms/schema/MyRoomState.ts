import { Schema, type, MapSchema } from "@colyseus/schema";


export class Player extends Schema {
  @type("number") x = 0;
  @type("number") y = 0;
  @type("number") z = 0;
  @type("number") rotY = 0;
  @type("string") anim = "idle"; // idle/walk/sit
}

export class MyState extends Schema {
  @type({ map: Player })
  players = new MapSchema<Player>();
}