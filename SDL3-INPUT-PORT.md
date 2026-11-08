# 1.3.3 with SDL3 gamepad input

Base: tag 1.3.3, e2143d43bcb6762340d8a01f20e7b5fdf104f02f.
Input donor: Canary-1.3.352, a61a42681.

The Avalonia GUI uses the donor SDL3 gamepad, Joy-Con and paired Joy-Con implementations and Ryujinx.SDL3-CS 2026.707.0. CPU, graphics, HLE, Input/Motion, audio backends and their existing dependency versions remain at 1.3.3. Headless retains SDL2 input.

Compatibility adaptations:
- SDL3 initializes only events, joystick and gamepad subsystems. SDL2 retains window/video/audio initialization; GUI disables SDL2 gamepad initialization to prevent competing device readers.
- Keep 1.3.3 IGamepad interface and normal rumble; donor raw-HID HD rumble is omitted to avoid new HLE and vibration interfaces.
- Keep legacy GamepadSDL2 config enum for compatibility. It routes through the injected SDL3 driver in this GUI build.
- Dispatch both libraries' events on the UI thread.
- Do not destroy SDL-owned gamepad properties returned by SDL_GetGamepadProperties.
- Limit mapping database HTTP fetch to five seconds; retain local fallback.
- Update obsolete NuGet feed URLs without changing old dependency versions.

## Build on Apple Silicon macOS

Use .NET SDK 9 (validated with 9.0.320). From the repository root:

```sh
dotnet publish src/Ryujinx -c Release -r osx-arm64 --self-contained true \
  -p:DebugType=embedded -p:Version=1.3.3 \
  -p:SourceRevisionId=sdl3-input-test -o "$PWD/artifacts/publish"

repo_root="$PWD"
mkdir -p "$repo_root/artifacts/macos"
cd distribution/macos
bash create_app_bundle.sh "$repo_root/artifacts/publish" \
  "$repo_root/artifacts/macos" "$repo_root/distribution/macos/entitlements.xml"
```

The packaging script replaces `Ryujinx.app` in its output directory. Use a separate output directory for testing. The installed local build uses bundle identifier `org.ryujinx.local.sdl3input133`; changing bundle metadata requires re-signing with the same entitlements.

A sibling `portable` directory keeps test configuration and saves separate from the normal user profile. Copy data only after closing the emulator. Keep keys, firmware, games and save data out of this repository.

## Validation

- Release publish and macOS bundle signature verification passed.
- CPU, graphics, HLE, Input/Motion and audio backend sources match tag 1.3.3.
- Startup and Nintendo Switch Pro Controller recognition passed.
- User tested Bluetooth motion in Tears of the Kingdom on an M2 Pro MacBook Pro (32 GB): motion direction is correct and FPS is stable at 29–30.
- USB motion and switching between USB and Bluetooth have not been confirmed for this custom build.

This is an input-only backport, not a complete upgrade to 1.3.352. Preserve the 1.3.3 core when applying future controller fixes.
