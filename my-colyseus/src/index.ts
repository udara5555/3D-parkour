/**
 * IMPORTANT:
 * ---------
 * Do not manually edit this file if you'd like to host your server on Colyseus Cloud
 *
 * If you're self-hosting, you can see "Raw usage" from the documentation.
 * 
 * See: https://docs.colyseus.io/server
 */
import { listen } from "@colyseus/tools";

// Import Colyseus config
import app from "./app.config.js";

import { MyRoom } from "./rooms/MyRoom.js";
import { Server } from "@colyseus/core"; // Import the Server class

const gameServer = new Server(); // Create an instance of the Server

gameServer.define("my_room", MyRoom);


// Create and listen on 2567 (or PORT environment variable.)
listen(app);
