import { defineServer, defineRoom, monitor, playground } from "colyseus";
import { MyRoom } from "./rooms/MyRoom.js";

console.log("Rooms registered: my_room");

const server = defineServer({
  rooms: {
    my_room: defineRoom(MyRoom),
  },

  express: (app) => {
    app.use("/monitor", monitor());
    if (process.env.NODE_ENV !== "production") app.use("/", playground());
  },
});

export default server;
