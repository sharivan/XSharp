# **X# Engine**

![](XSharp.png)

This project aims to recreate an engine very similar to the SNES Mega Man X trilogy. You can test the executable version of this project by downloading the current release accessing https://github.com/sharivan/XSharp/releases. You will need the runtime of .NET 7.0 or newer to run this program.

A video showing the version v0.2.1-alpha released on March 23, 2023 can be watched at https://www.youtube.com/watch?v=5SOOVi5R4s8

You can control the X using the following keys:

  - X: Dash.
  - C: Jump.
  - V: Shoot X-Buster.
  - Left Arrow: Move to Left.
  - Right Arrow: Move to Right.
  - Enter: Toggle pause.

Support to joypad was added (tested using DS4 joypad with DS4Windows only), but there is no support to bind custom keys for now. You can use joy2key or similar key mapping applications for map your keyboard or joypad with your prefered config if you want.

Shortcut Keys:

  - Pause/Break: Toggle frame advance.
  - |: Next frame, if in frame advance mode. Otherwise, start the frame advance mode.
  - F5: Save state.
  - F7: Load state.
  - =: Next save slot.
  - -: Previous save slot.
  - N: Toggle no clip.
  - M: Toggle no camera constraints.
  - 1: Toggle draw hitbox.
  - 2: Toggle show colliders.
  - 3: Toggle draw level bounds.
  - 4: Toggle draw touching tilemap bounds.
  - 5: Toggle draw highlighted pointing tiles (with mouse cursor).
  - 6: Toggle draw axis from X origin.
  - 7: Toggle show info.
  - 8: Toggle show checkpoint bounds.
  - 9: Toggle show trigger bounds.
  - F1: Toggle background.
  - F2: Toggle foreground down layer.
  - F3: Toggle foreground up layer.
  - F4: Toggle sprites.

Modifications currently in progress:

- Level editor.
- Own level format (instead loading from rom of original games). This step is neeeded to make the level editor.
- More levels for testing.
- More kind of enemies.
- More bosses.
- Armors.
- Water graphics.
- Slippery physics (present in Crystal Snail and Blizzard Buffalo stages).
- Conveyor physics (present in Mammoth and Sigma 3 stages).
- Embedded console.
- Lua scripting support, allowing devs and mappers customize levels, enemies, etc.

Some Pending Fixes:

- Fix background position in some levels.
- Fix triggers and camera lock in some levels.
- Fix camera transitions.

Future additions:

- Abstraction of dependencies like graphical API, S.O. API and others. This is needed to make this project portable for other systems in future.
- Further polishing of physics.
- Pause menu.
- Weapons.
- Demo recorder.
- Full documentation.
