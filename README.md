# MBDSW
Minecraft Bedrock Dedicated Server Wrapper

This is a .NET Core library for automating operations on the Bedrock Dedicated Server, somewhat loosely tied to Windows.

It includes a command line interpreter which can be found under the [Builds](https://github.com/pcaston2/MBDSW/tree/master/Builds)

Features:
- "Keep Alive" Mode: This will download the latest version, start the server, create incremental and full backups, as well as update when new versions are found.
- Find and install latest version
- Start/Stop Server
- Backup Server
- Restore from backups
- Send commands to server

**This is a work in progress, use at your own risk.**

Future:
- Web UI for managing server from anywhere
- Host multiple servers
- More!
