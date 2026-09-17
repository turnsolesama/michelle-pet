# MichellePet

- Build: `./build.ps1` on Windows using the system .NET Framework compiler. Preserve historical releases; do not overwrite published archives.
- Diagnostic: `releases/v0.3.2/MichellePet.exe --check <absolute temporary directory>`. It creates desktop-region screenshots; keep diagnostic output local.
- Edge regression: compile tests/EdgeRegression.cs with System.Drawing.dll and System.Windows.Forms.dll, then pass the EXE under test as its argument. This exercises behavior methods, not physical mouse input.
- Keep source, runtime assets and build outputs consistent. Preserve per-frame UpdateLayeredWindow SIZE submission and separate window placement from animation rendering.
- Never publish official reference collections, desktop screenshots, local galleries, credentials or personal configuration.
- Before publishing a release, fetch its uploaded package, verify byte size, SHA-256, ZIP integrity, single-app root and EXE identity. Publish download links only after verification.
- Classic-uniform keyboard/mouse companion is implemented as layered 2D. Keep full key reach, fixed palm scale and free placement without falling or auto-docking. True Live2D remains planned; do not call this a Live2D model.
- Compile tests/CompanionRegression.cs, CompanionPlacementRegression.cs and EdgeRegression.cs with the framework compiler and references to System.Drawing.dll and System.Windows.Forms.dll; run against the release EXE. NativeInputProbe is a local acceptance harness, never a release asset.
