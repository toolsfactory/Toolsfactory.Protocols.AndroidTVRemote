from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.serialization import load_pem_private_key

with open("temp-key.pem", "rb") as f:
    key = load_pem_private_key(f.read(), password=None)

with open("temp-key.pem", "wb") as f:
    f.write(key.private_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PrivateFormat.PKCS8,
        encryption_algorithm=serialization.NoEncryption()
    ))

print("Rewrote temp-key.pem to PKCS#8 format.")
