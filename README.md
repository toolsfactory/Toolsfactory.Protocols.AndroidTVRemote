# Toolsfactory.Protocols.AndroidTVRemote

A .NET tool for discovering, pairing, and remotely controlling Android TV devices over the network.

## Features

- 🔍 **Automatic discovery** of Android TV devices via mDNS/zeroconf.
- 🔐 **Secure pairing process** using TLS with client certificates.
- 📄 **Generates `.apair` files** that contain all necessary credentials (device name, IP, and TLS cert/private key) to reconnect later.
- 🕹️ **Supports sending remote control commands** to Android TV devices after pairing.
- 🧠 Built to work with Android >= 11 (with their distinct pairing behaviors).
- 💬 Simple terminal interface for interactive pairing and device management.

## What is an `.apair` file?

`.apair` files are pairing profiles. They include:
- A **friendly name** for the device
- Its **unique ID**
- **IP address**
- A **TLS certificate and private key** used to authenticate with the TV over a secure channel

This file allows trusted reconnection to the same TV without repeating the pairing process, enabling long-term remote control.

## How it works

1. Simply run the program to engage the terminal interface.
2. The tool scans for Android TV devices using network discovery.
3. You select a device and assign it a friendly name.
4. The program launches a Python script that securely pairs with the device using a 6-digit PIN.
5. Once paired, the tool creates a `.apair` file which you can use for future authenticated control sessions.
6. The TLS-based connection (port 6466) ensures all communication is secure and trusted by the device.

## Requirements

- .NET 8.0+
- Python 3.9+ installed and available on the system path
- Python dependencies:
    - `androidtvremote2`
    - `cryptography`

## License

MIT License
