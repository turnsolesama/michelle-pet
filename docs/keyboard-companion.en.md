# Gaming Buddy notes

v0.3.5 adds **Gaming Buddy Outfit → Classic Uniform / Feline Energy - Dorm**. Both use rounded keycaps, a cat-ear mouse mat and paw decorations; the dorm outfit has separate bare hands and an ivory/orange mouse. Changing the companion outfit preserves placement, size, pause and input-link state. Selecting a normal walking outfit exits Gaming Buddy. Companion outfit choice lasts for the current session.

The current implementation uses WinForms, Windows Raw Input and layered PNG artwork. The body and desk stay fixed; the forearms and hands move independently. Palm scale stays constant while the forearms connect the wrists to the sleeves. Keyboard targeting follows the rotated keycaps, including Shift, Ctrl and Space.

## Input behavior

- Input reactions are registered only while Gaming Buddy is enabled and its input link is active.
- W/A/S/D, Q/E/R/F, Shift, Ctrl and Space have individual targets. Short taps leave a brief animation; held keys stay pressed. The most recent held game key takes priority.
- Other supported typing keys trigger an anonymous three-lane tapping animation. Individual characters and text are not retained. Function keys, Alt and Windows keys do not trigger this animation.
- The mouse uses relative motion and left/right button states. Movement is bounded so the hand stays near the mouse.
- Menus, dragging and paused animation suspend input; disabling the link or leaving the mode clears its state.

Gaming Buddy can remain anywhere within the display's work area. The ordinary pet's falling and automatic edge docking are suspended in this mode. Entering sleep, selecting an outfit or manually docking exits the mode.

## Editing and future work

The runtime artwork is in `assets/companion/classic-atlas.png`, `hands-v3.png`, `dorm-body.png` and `dorm-hands.png`. Their JSON manifests describe the artwork and pivots; `src/CompanionRenderer.cs` contains the drawing geometry. The normal outfit PNGs and transparency cleanup data are in `assets/skins`.

Possible future improvements include more outfit-specific desk poses, individual finger animation, blinking and hair physics. A true Live2D version would require separately layered art and a rig; the existing sprite atlas is not a ready-to-use Live2D model. This edition does not add a Live2D runtime or require an external editor.

Real-game compatibility, exclusive fullscreen, anti-cheat behavior and mixed-DPI multi-monitor input remain outside the current acceptance coverage.
