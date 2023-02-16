using DataLib.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SupportTooling
{
    public class FileService
    {
        private readonly string cryptoKey = "YP1afDJA";
        private readonly string initVector = "8O14lnfc";
        public AllData GetFileData(string filePath)
        {
            return DecryptAndDeserialize(filePath);
        }

        public void WriteFile(string filePath, AllData data)
        {
            EncryptAndSerialize(filePath, data);
        }
        private AllData DecryptAndDeserialize(string filePath)
        {
            var formatter = new BinaryFormatter();
            var algorithm = GetSymmetricAlgorithm();

            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (CryptoStream cs = new CryptoStream(fs, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    return (AllData)formatter.Deserialize(cs);
                }
            }
        }
        private SymmetricAlgorithm GetSymmetricAlgorithm()
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = ASCIIEncoding.ASCII.GetBytes(cryptoKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(initVector);

            return des;
        }
        private void EncryptAndSerialize(string filePath, AllData data)
        {
            var formatter = new BinaryFormatter();
            var algorithm = GetSymmetricAlgorithm();

            using (FileStream fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cs = new CryptoStream(fs, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    formatter.Serialize(cs, data);
                }
            }
        }

    }
}
