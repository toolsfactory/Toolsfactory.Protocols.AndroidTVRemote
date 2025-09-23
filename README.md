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

`.apair` file has the following JSON structure:

```json
{
  "DeviceName": "Living Room TV",
  "DeviceId": "LRoomTV1",
  "DeviceHostName": "192.168.1.23",
  "Certificate": "-----BEGIN CERTIFICATE-----...-----END CERTIFICATE-----\n-----BEGIN PRIVATE KEY-----...-----END PRIVATE KEY-----"
}
```
This file allows trusted reconnection to the same TV without repeating the pairing process, enabling long-term remote control.

## How it works

1. Run the program to engage the terminal interface.
2. The tool scans for Android TV devices using network discovery.
3. You select a device and assign it a friendly name.
4. The program launches a Python script that securely pairs with the device using a 6-digit PIN.
5. Once paired, the tool creates an `.apair` file which you can use for future authenticated control sessions.
6. The TLS-based connection (port 6466) ensures all communication is secure and trusted by the device.

The tool is designed to be used entirely from the terminal. However, it can be used with the following command structure:

```shell
dotnet run -- <command> [options]
```

Where `<command>` is one of the available features listed below.

| Command                       | Description                                    |
|------------------------------|------------------------------------------------|
| `menu`                       | Interactive menu interface                     |
| `scan`                       | Discover Android TV devices on the network     |
| `pair --host --file`         | Pair with a device and save a profile          |
| `interactive --config`       | Connect to and control a paired device         |

---

## Requirements

- [.NET 8.0+](https://dotnet.microsoft.com/en-us/download)
- [Python 3.9+](https://www.python.org/downloads/) available on your system path
- Python package: `androidtvremote2`

## License

MIT License
