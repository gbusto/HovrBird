using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class EncryptedDataManager
{
    public static void WriteData(string fullPath, byte[] key, byte[] iv, byte[] serializedData)
    {
        byte[] encryptedData;

        using (AesManaged crypto = new AesManaged())
        {
            crypto.Key = key;
            crypto.IV = iv;

            ICryptoTransform encryptor = crypto.CreateEncryptor(crypto.Key, crypto.IV);

            using (FileStream fsEncrypt = new FileStream(fullPath, FileMode.OpenOrCreate))
            {
                using (CryptoStream csEncrypt = new CryptoStream(fsEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(serializedData, 0, serializedData.Length);
                }
            }
        }
    }

    public static byte[] ReadData(string fullPath, byte[] key, byte[] iv)
    {
        FileInfo fileInfo = new FileInfo(fullPath);

        byte[] encBytes;

        using (FileStream fStream = new FileStream(fullPath, FileMode.OpenOrCreate))
        {
            encBytes = new byte[fileInfo.Length];
            fStream.Read(encBytes, 0, encBytes.Length);
        }

        byte[] decryptedData;

        using (AesManaged crypto = new AesManaged())
        {
            crypto.Key = key;
            crypto.IV = iv;

            ICryptoTransform decryptor = crypto.CreateDecryptor(crypto.Key, crypto.IV);

            using (FileStream fsDecrypt = new FileStream(fullPath, FileMode.Open))
            {
                using (CryptoStream csDecrypt = new CryptoStream(fsDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        byte[] dataBuf = new byte[512];
                        int bytesRead = csDecrypt.Read(dataBuf, 0, dataBuf.Length);
                        while (bytesRead > 0)
                        {
                            memStream.Write(dataBuf, 0, bytesRead);
                            bytesRead = csDecrypt.Read(dataBuf, 0, dataBuf.Length);
                        }
                        decryptedData = memStream.ToArray();
                    }
                }
            }
        }

        return decryptedData;
    }
}
