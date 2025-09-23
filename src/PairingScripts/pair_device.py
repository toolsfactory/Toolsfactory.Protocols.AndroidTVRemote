#!/usr/bin/env python3
import asyncio
import argparse
import sys
try:
    from androidtvremote2 import AndroidTVRemote, InvalidAuth, CannotConnect
except ModuleNotFoundError as e:
    print("Missing dependency 'androidtvremote2'. This script is meant to be run via the AndroidTV Remote Tool which auto-installs dependencies.")
    raise

async def pair_and_connect(host: str, certfile: str, keyfile: str, client_name: str):
    # Create the remote instance (port 6467 is the pairing port by default)
    remote = AndroidTVRemote(
        client_name=client_name,
        certfile=certfile,
        keyfile=keyfile,
        host=host,
    )

    # 1) Generate a new cert/key if none exist
    if await remote.async_generate_cert_if_missing():
        print("Generated new client certificate.")

    # 2) Start pairing (this sends the ClientHello + early data under the hood)
    print("Starting pairing... Check your TV for a PIN.")
    await remote.async_start_pairing()

    # 3) Read the PIN from stdin and finish pairing. Limit to 3 attempts.
    attempts = 0
    max_attempts = 3
    while True:
        code = input("Enter the 6-digit PIN from the TV: ").strip()
        try:
            await remote.async_finish_pairing(code)
            print("Pairing successful!")
            break
        except InvalidAuth:
            attempts += 1
            if attempts >= max_attempts:
                print("Pairing unsuccessful. Too many incorrect PIN attempts.")
                sys.exit(1)
            else:
                print("Invalid PIN, please try again")

    # 4) Now connect to the remote (this opens port 6466 and negotiates TLS)
    try:
        await remote.async_connect()
        print("Connected to Android TV!")
    except CannotConnect as e:
        print(f"Failed to connect: {e}")
        return

    # 5) Cleanly disconnect
    remote.disconnect()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Pair & connect to Android TV via androidtvremote2"
    )
    parser.add_argument("host", help="IP address of your Android TV")
    parser.add_argument(
        "--certfile",
        default="client-cert.pem",
        help="PEM file with your client certificate",
    )
    parser.add_argument(
        "--keyfile",
        default="client-key.pem",
        help="PEM file with your client private key",
    )
    parser.add_argument(
        "--name",
        default="PythonScript",
        help="Client-name shown during pairing on the TV",
    )
    args = parser.parse_args()

    asyncio.run(
        pair_and_connect(
            host=args.host,
            certfile=args.certfile,
            keyfile=args.keyfile,
            client_name=args.name,
        )
    )
    