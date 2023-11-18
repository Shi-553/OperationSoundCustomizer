using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Storage;
using System.Diagnostics;
using Windows.ApplicationModel;
using System.Linq;
using Windows.Storage.Search;
using Newtonsoft.Json;

namespace OperationSoundCustomizer
{
    public interface IAudio {
        public Task CreateGraph();

        public Task<bool> Start();

        [JsonIgnore]
        public bool IsPlaying { get; }
        [JsonIgnore]
        public StorageFile File { get; }
    }
    public class Audio : IAudio {

        [JsonIgnore]
        public AudioFileInputNode InputNode { get; private set; }
        [JsonIgnore]
        public StorageFile File { get; init; }

        public bool IsPlaying { get; private set; } = false;

        public Audio(StorageFile file) {
            this.File = file;
        }

        public Task CreateGraph() {

            return Task.Run(async () => {
                var createFileResult = await AudioControl.AudioGraph.CreateFileInputNodeAsync(File);

                InputNode = createFileResult.FileInputNode;
                InputNode.Stop();

                InputNode.AddOutgoingConnection(AudioControl.OutputNode);

                InputNode.FileCompleted += InputNode_FileCompleted;


                Debug.WriteLine(File);
            });
        }

        private void InputNode_FileCompleted(AudioFileInputNode sender, object args) {
            IsPlaying = false;
        }

        public Task<bool> Start() {
            if (InputNode == null) {
                return Task.FromResult(false);
            }
            IsPlaying = true;
            InputNode.Reset();
            InputNode.Start();

            Debug.WriteLine(File.Name);

            return Task.FromResult(true);
        }

    }

    public class NullAudio : IAudio {
        public static bool IsNull => true;

        readonly string message;
        public NullAudio(string message) {
            this.message = message;
            Debug.WriteLine("NullAudio!!:" + message);
        }

        public Task<bool> Start() {
            Debug.WriteLine("NullAudio!!:" + message);
            return Task.FromResult(false);
        }

        public Task CreateGraph() {
            throw new NotImplementedException();
        }

        public bool IsPlaying => false;

        public StorageFile File => throw new NotImplementedException();
    }

    //シリアライズ対応
    public class AudioWrapper : IAudio {
        [JsonIgnore]
        readonly IAudio audio;
        public string Path { get; init; }

        public AudioWrapper(IAudio audio) {
            this.audio = audio;
            Path = audio.File.Name;
        }
        [JsonConstructor]
        public AudioWrapper(string path) {
            this.Path = path;
            audio = AudioControl.GetAudio(path);
        }
        public bool IsPlaying => audio.IsPlaying;

        public StorageFile File => audio.File;

        public Task<bool> Start() {
            return audio.Start();
        }

        public Task CreateGraph() {
            return audio.CreateGraph();
        }
    }

    public class AudioPool : IAudio {
        [JsonIgnore]
        readonly List<Audio> audios = new();

        [JsonIgnore]
        public StorageFile File { get; init; }


        public Task CreateGraph() {
            return Task.WhenAll(audios.Select(a => a.CreateGraph()));
        }

        public AudioPool(StorageFile file, int poolCount = 1) {
            this.File = file;
            for (int i = 0; i < poolCount; i++) {
                Audio a = new(file);
                audios.Add(a);
            }
        }

        public bool IsPlaying => audios.Any(a => a.IsPlaying);

        public async Task<bool> Start() {
            var notPlaying = audios.Find(a => !a.IsPlaying);

            if (notPlaying != null) {
                return await notPlaying.Start();
            }

            Audio a = new(File);
            audios.Add(a);

            await a.CreateGraph();


            return await a.Start();
        }
    }


    public class AudioPair {

        public IAudio Down { private set; get; }
        public IAudio Up { private set; get; }

        public AudioPair(IAudio down, IAudio up) {
            Down = down;
            Up = up;
        }

        public Task<bool> Start(bool flag) {
            return flag ? Down.Start() : Up.Start();
        }
    }


    public static class AudioControl {
        private static readonly Dictionary<StorageFile, IAudio> audioMap = new();

        public static AudioGraph AudioGraph { get; private set; }
        public static AudioDeviceOutputNode OutputNode { get; private set; }

        public static async Task AudioInit() {
            AudioGraphSettings settings = new(AudioRenderCategory.SoundEffects) {
                QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency,
                MaxPlaybackSpeedFactor = 2
            };

            var result = await AudioGraph.CreateAsync(settings);
            switch (result.Status) {
                case AudioGraphCreationStatus.DeviceNotAvailable:
                    throw new Exception("Create AudioGraph - Device not available.");
                case AudioGraphCreationStatus.FormatNotSupported:
                    throw new Exception("Create AudioGraph - Format Not Supported.");
                case AudioGraphCreationStatus.UnknownFailure:
                    throw new Exception("Create AudioGraph - Unknown Failure.");
            }
            AudioGraph = result.Graph;
            AudioGraph.UnrecoverableErrorOccurred += AudioGraph_UnrecoverableErrorOccurred;

            var createDeviceResult = await AudioGraph.CreateDeviceOutputNodeAsync();
            switch (createDeviceResult.Status) {
                case AudioDeviceNodeCreationStatus.DeviceNotAvailable:
                    throw new Exception("CreateDeviceOutput - Device not available.");
                case AudioDeviceNodeCreationStatus.FormatNotSupported:
                    throw new Exception("CreateDeviceOutput - Format Not Supported.");
                case AudioDeviceNodeCreationStatus.UnknownFailure:
                    throw new Exception("CreateDeviceOutput - Unknown Failure.");
                case AudioDeviceNodeCreationStatus.AccessDenied:
                    throw new Exception("CreateDeviceOutput - Access Denied.");
            }

            OutputNode = createDeviceResult.DeviceOutputNode;
            AudioGraph.Start();

            var folderPath = Package.Current.InstalledLocation.Path + "\\Assets\\Sounds";
            var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);

            QueryOptions options = new(CommonFileQuery.DefaultQuery, new string[] { ".mp3", ".wav", ".wma", ".m4a" });

            var queryResult = folder.CreateFileQueryWithOptions(options);
            var files = await queryResult.GetFilesAsync();



            var pools = files.Select((file) => new AudioWrapper(new AudioPool(file))).ToArray();

            await Task.WhenAll(pools.Select(a => a.CreateGraph()));

            audioMap.Clear();
            foreach (var pool in pools) {
                audioMap.Add(pool.File, pool);
            }
        }

        public static void Dispose() {
            if (AudioGraph != null) {
                AudioGraph.Dispose();
            }
        }

        private static void AudioGraph_UnrecoverableErrorOccurred(AudioGraph sender, AudioGraphUnrecoverableErrorOccurredEventArgs args) {
            switch (args.Error) {
                case AudioGraphUnrecoverableError.AudioDeviceLost:
                    throw new Exception("Unrecoverable Error Occurred - Audio Device Lost.");
                case AudioGraphUnrecoverableError.AudioSessionDisconnected:
                    throw new Exception("Unrecoverable Error Occurred - Audio Session Disconnected.");
                case AudioGraphUnrecoverableError.UnknownFailure:
                    throw new Exception("Unrecoverable Error Occurred - Unknown Failure.");
            }
        }

        // NameかPathかDisplayName
        public static IAudio GetAudio(string name) {
            var first = audioMap.FirstOrDefault(pair =>
            pair.Key.Name == name ||
            pair.Key.Path == name ||
            pair.Key.DisplayName == name
            );

            if (first.Value == null) {
                return new NullAudio(name);
            }
            return first.Value;
        }

        // NameかPathかDisplayName
        public static AudioPair GetAudioPair(string name1, string name2) {
            return new(GetAudio(name1), GetAudio(name2));
        }

    }
}