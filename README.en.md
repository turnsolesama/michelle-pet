# Michele Desktop Pet — Bilingual v0.3.3

**[Download v0.3.3 · Windows bilingual ZIP](https://github.com/turnsolesama/michelle-pet/releases/download/v0.3.3/MichelePet-v0.3.3-win.zip)** · **[Full project ZIP](https://github.com/turnsolesama/michelle-pet/releases/download/v0.3.3/MichelePet-v0.3.3-Full-Project.zip)** · [Release and SHA-256 checksums](https://github.com/turnsolesama/michelle-pet/releases/tag/v0.3.3)

Both uploaded packages were downloaded again and verified for size, SHA-256, ZIP integrity and executable identity.

A fan-made Windows desktop companion inspired by Michele from Strinova. Choose **语言 / Language → English** from the character or tray menu. Chinese and English are included in the same app; switching updates menus, speech bubbles, tray text and application messages without restarting or moving the pet.

## Start

Extract the entire ZIP and double-click **MichelePet.exe** inside `v0.3.3`. No installation is needed; the app uses Windows .NET Framework 4.x. Exit any running Chinese or older edition first: all editions share a single-instance lock. Right-click the character or tray icon and choose **Exit** to close it.

The first launch defaults to Chinese. The **语言 / Language** submenu offers **简体中文** and **English**. Your choice is saved in `%LOCALAPPDATA%/MichelePet/language.txt` and restored next time. Switching preserves outfit, position, size, sleep, edge-peeking and Gaming Buddy states. Use `--lang=en` or `--lang=zh` for a one-launch override. If saving fails, the selected language still applies to the current session.

To start directly in Gaming Buddy mode, add `--companion` to the executable's shortcut target after the closing quote, or run `MichelePet.exe --companion`.

## Play

- Click her head or body for a happy expression and a short reply. Click her feet, or choose **Hop**, for a small jump.
- Drag her to move. In normal pet mode she settles at the bottom of the display's work area.
- Choose **Outfit** for four looks: Classic Uniform, Sweet Dreams, Heart Guardian and Starry Dreamweaver. These are descriptive English labels for this fan project, not verified official English skin names.
- Choose **Take a Nap** to rest; click her or choose **Wake Up** to wake her. Changing outfits while sleeping keeps her asleep.
- Drag near the left or right screen edge to tuck her away, or choose **Peek from Screen Edge**. Drag along the edge to adjust her height; click her or drag inward to bring her back.
- Choose **Size** for Small (160), Medium (240) or Large (360). Other options include **Pause Animation**, **Always on Top** and **Move to Bottom Right**.
- Double-click the tray icon to bring her to the display containing your pointer. Transparent parts of the character window let clicks pass through.

## Gaming Buddy

Choose **Gaming Buddy - Classic** to place Michele at a keyboard and mouse. Her keyboard hand follows W/A/S/D, Q/E/R/F, Shift, Ctrl and Space, including edge keys. With multiple game keys held, her hand follows the most recently pressed key; the others remain lit. Her mouse hand reacts to relative pointer movement and left/right clicks.

Other letters, numbers, common punctuation, Backspace, Enter and Tab trigger a general tapping animation and illuminate unlabelled key areas. These reactions do not map every character to an individual keycap. The app does not read text from your input fields or IME.

Drag Gaming Buddy anywhere within your display's work area: she stays where you leave her, without falling or automatically docking. **Keyboard / Mouse Input** turns input reactions off while keeping the seated pose. Dragging, opening the menu or pausing temporarily suspends input capture. Leaving the mode restores the previous outfit. Changing outfit, sleeping or manually docking also exits Gaming Buddy.

Only Classic Uniform has the keyboard/mouse pose. This is layered 2D animation, not a Live2D model. See [Gaming Buddy notes](docs/keyboard-companion.en.md).

## Privacy and artwork

The app keeps only temporary game-key and mouse states plus anonymous typing animation state. It does not store typed text or key history, intercept normal game input, connect to the network, add itself to startup or save typed content. The only saved preference is the interface language. Specific games, anti-cheat systems, exclusive fullscreen and mixed-DPI multi-monitor setups have not been comprehensively tested.

The character images are AI-assisted fan artwork with simplified details; the dialogue is original fan writing. This is an unofficial personal project. Character rights belong to their respective owners. Official reference collections and desktop diagnostic screenshots are excluded from distributed packages.

## Build from source

In the full project folder, run PowerShell:

```powershell
./build.ps1
```

The output is `releases/v0.3.3/MichelePet.exe`. One build contains both languages. User-facing strings and preference handling are in `src/PetText.cs`.

`MichelePet.exe --check <absolute-output-directory>` runs diagnostics and exits. Its output includes screenshots of your desktop; keep that directory private. Local test harness executables are not included in the runtime ZIP.

Source repository: [turnsolesama/michelle-pet](https://github.com/turnsolesama/michelle-pet). Both languages are distributed together in v0.3.3; older releases remain available.
