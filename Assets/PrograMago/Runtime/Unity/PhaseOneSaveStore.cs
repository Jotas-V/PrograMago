using System;
using System.IO;
using PrograMago.Domain;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed class PhaseOneSaveStore
    {
        private readonly string filePath;

        public PhaseOneSaveStore(string filePath)
        {
            this.filePath = string.IsNullOrWhiteSpace(filePath)
                ? throw new ArgumentException("Caminho do registro obrigatório.", nameof(filePath))
                : filePath;
        }

        public PhaseOneSaveData Load()
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            return JsonUtility.FromJson<PhaseOneSaveData>(File.ReadAllText(filePath));
        }

        public void Save(PhaseOneSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporaryPath = filePath + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data));
            if (File.Exists(filePath))
            {
                File.Replace(temporaryPath, filePath, null);
            }
            else
            {
                File.Move(temporaryPath, filePath);
            }
        }
    }
}
