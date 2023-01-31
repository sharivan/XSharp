# **X# Engine**

![](XSharp.png)

This project aims to recreate an engine very similar to the SNES Mega Man X trilogy. You can test the executable version of this project by downloading the current release accessing https://github.com/sharivan/XSharp/releases. You will need the .NET Frameworkd 4.8.1 or newer to run this program.

You can control the X using the following keys:

  - X: Dash.
  - C: Jump.
  - V: Shoot X-Buster.
  - Left Arrow: Move to Left.
  - Right Arrow: Move to Right.
  - Enter: Toggle pause.

Support to joypad was added, but there is no support to bind custom keys for now.

Shortcut Keys:

  - CTRL + |: Toggle frame advance.
  - |: Next frame, if in frame advance mode. Otherwise, start the frame advance mode.
  - F5: Save state.
  - F7: Load state.
  - =: Next save slot.
  - -: Previous save slot.
  - N: Toggle no clip.
  - M: Toggle no camera constraints.
  - 1: Toggle draw collision box.
  - 2: Toggle show colliders.
  - 3: Toggle draw map bounds.
  - 4: Toggle draw touching map bounds.
  - 5: Toggle draw highlighted pointing tiles (with mouse).
  - 6: Toggle draw axis from X.
  - 7: Toggle show info.
  - 8: Toggle show checkpoint bounds.
  - 9: Toggle show trigger bounds.
  - F1: Toggle background.
  - F2: Toggle down layer.
  - F3: Toggle up layer.
  - F4: Toggle sprites.

Modifications currently in progress:

- Addition of sound effects.
- More maps for testing.
- More types of enemies.
- Armors.

Some Pending Fixes:

- Fix some X sprite animations.
- Fix full charged X buster position.
- Fix ladders.
- Fix background position in some levels.
- Fix triggers and camera lock in some levels.

Future additions:

- Map editor.
- Bosses.
- Pause menu.
- Weapons.
- Demo recorder.
- Embedded console.
- Support for other render APIs like D3D11 and OpenGL.
- Add documentation.
