import os
import sys
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.serialization import load_pem_private_key

KEY_PATH = "temp-key.pem"
TMP_PATH = KEY_PATH + ".new"

try:
    # 1) Load existing key
    with open(KEY_PATH, "rb") as f:
        key = load_pem_private_key(f.read(), password=None)

    # 2) Export as PKCS#8 (unencrypted, same as before)
    data = key.private_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PrivateFormat.PKCS8,
        encryption_algorithm=serialization.NoEncryption()
    )

    # 3) Write atomically: temp file -> fsync -> replace
    fd = os.open(TMP_PATH, os.O_WRONLY | os.O_CREAT | os.O_TRUNC, 0o600)
    try:
        with os.fdopen(fd, "wb") as tmp:
            tmp.write(data)
            tmp.flush()
            os.fsync(tmp.fileno())
    except Exception:
        # Ensure the raw fd is closed if fdopen failed before context manager took ownership
        try:
            os.close(fd)
        except Exception:
            pass
        raise

    os.replace(TMP_PATH, KEY_PATH)
    print("Rewrote temp-key.pem to PKCS#8 format.")
except Exception as e:
    # Best-effort cleanup
    try:
        if os.path.exists(TMP_PATH):
            os.remove(TMP_PATH)
    except Exception:
        pass
    print(f"Failed to rewrite key: {e}", file=sys.stderr)
    sys.exit(1)
