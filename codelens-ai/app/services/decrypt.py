import base64
import hashlib
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding
import os


AES_KEY = os.getenv("AES_KEY")

def decrypt_token(cipher_text:str)->str:
    key_bytes = hashlib.sha256(AES_KEY.encode("utf-8")).digest()
    full_bytes = base64.b64decode(cipher_text)

    iv = full_bytes[:16]
    encrypted_bytes = full_bytes[16:]

    ciper = Cipher(algorithms.AES(key_bytes), modes.CBC(iv))
    decryptor = ciper.decryptor()

    decrypted_padded = decryptor.update(encrypted_bytes) + decryptor.finalize()

    unpadder = padding.PKCS7(128).unpadder()
    decrypted = unpadder.update(decrypted_padded) + unpadder.finalize()

    return decrypted.decode("utf-8")
