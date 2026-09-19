# MichelePet / 米雪儿桌宠

- Build: `./build.ps1` on Windows using the system .NET Framework compiler. Preserve historical releases; do not overwrite published archives.
- Bilingual build: `./build.ps1` outputs `releases/v0.3.7/MichelePet.exe`. Runtime text is in `src/PetText.cs`. Language switching must preserve pet state; persistence tests must use isolated temporary paths. Run LanguageRegression and EnglishLayoutRegression against the release EXE with an absolute private evidence directory. They use method-driven actions, not physical input.
- Diagnostic: `releases/v0.3.7/MichelePet.exe --check <absolute temporary directory>`. It creates desktop-region screenshots; keep diagnostic output local.
- Edge regression: compile tests/EdgeRegression.cs with System.Drawing.dll and System.Windows.Forms.dll, then pass the EXE under test as its argument. This exercises behavior methods, not physical mouse input.
- Keep source, runtime assets and build outputs consistent. Preserve per-frame UpdateLayeredWindow SIZE submission and separate window placement from animation rendering.
- Never publish official reference collections, desktop screenshots, local galleries, credentials or personal configuration.
- Before publishing a release, fetch its uploaded package, verify byte size, SHA-256, ZIP integrity, single-app root and EXE identity. Publish download links only after verification.
- Classic-uniform and Feline Energy dorm keyboard/mouse companions are implemented as layered 2D. Keep full key reach, fixed palm scale and free placement without falling or auto-docking. True Live2D remains planned; do not call this a Live2D model.
- Compile tests/CompanionRegression.cs, CompanionPlacementRegression.cs and EdgeRegression.cs with the framework compiler and references to System.Drawing.dll and System.Windows.Forms.dll; run against the release EXE. NativeInputProbe is a local acceptance harness, never a release asset.

- Two companion outfits: run CompanionOutfitRegression with the EXE and an absolute private evidence directory. Preserve bare dorm hands versus uniform gloves, equal fixed palm scale per outfit, all key reach, style-switch placement and the prior normal skin on exit. Official dorm reference PNG stays local under assets/references and must never enter release packages.
