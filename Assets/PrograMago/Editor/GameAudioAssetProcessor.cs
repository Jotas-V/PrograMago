using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PrograMago.Editor
{
    public sealed class GameAudioAssetProcessor : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/PrograMago/Resources/Audio/")) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            bool music = Path.GetFileNameWithoutExtension(assetPath) == "Music";
            settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = music ? .8f : 1f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = !music;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = music;
        }
    }

    public sealed class GameAudioBuildCredits : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPostprocessBuild(BuildReport report)
        {
            string output = report.summary.outputPath;
            string directory = Directory.Exists(output) ? output : Path.GetDirectoryName(output);
            File.Copy("Assets/PrograMago/Resources/Audio/Credits.txt", Path.Combine(directory, "Audio-Credits.txt"), true);
        }
    }
}


