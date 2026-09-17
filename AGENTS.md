# MichellePet

- Build: `./build.ps1` on Windows using the system .NET Framework compiler. Preserve historical releases; do not overwrite published archives.
- Diagnostic: `releases/v0.2.2/MichellePet.exe --check <absolute temporary directory>`. It creates desktop-region screenshots; keep diagnostic output local.
- Edge regression: compile tests/EdgeRegression.cs with System.Drawing.dll and System.Windows.Forms.dll, then pass the EXE under test as its argument. This exercises behavior methods, not physical mouse input.
- Keep source, runtime assets and build outputs consistent. Preserve per-frame UpdateLayeredWindow SIZE submission and separate window placement from animation rendering.
- Never publish official reference collections, desktop screenshots, local galleries, credentials or personal configuration.
- Before publishing a release, fetch its uploaded package, verify byte size, SHA-256, ZIP integrity, single-app root and EXE identity. Publish download links only after verification.
- Keyboard/mouse companion and Live2D work is planned, not implemented. Do not advertise it as an existing feature.
