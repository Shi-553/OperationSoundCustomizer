using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Storage;

using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.Storage.Search;
using System.Collections;
using Newtonsoft.Json;

namespace OperationSoundCustomizer {
    public class RandomHelper {
        public static Random Random { get; } = new Random();


        internal static int GetRandomIndex(int max) {
            return Random.Next(max);
        }

        internal static T GetRandom<T>(params T[] list) => GetRandom((IReadOnlyList<T>)list);

        internal static T GetRandom<T>(IReadOnlyList<T> list) {
            return list[GetRandomIndex(list.Count)];
        }

        internal static double GetRandomBetweenIndex(int max) {
            return Random.NextDouble() * max;
        }

        internal static T GetRandomBetween<T>(params T[] list) => GetRandomBetween((IReadOnlyList<T>)list);

        internal static T GetRandomBetween<T>(IReadOnlyList<T> list) {
            var rand = Random.NextDouble();
            var randLen = rand * list.Count;
            var floor = Convert.ToInt32(Math.Floor(randLen));
            var ceiling = Convert.ToInt32(Math.Ceiling(randLen));

            return ((dynamic)list[floor] * rand) + ((dynamic)list[ceiling] * (1 - rand));
        }
    }

    public interface IValue<T> {
        public T GetNextValue();
    }
    public class CommonValue<T> : IValue<T> {
        public T Value { get; set; }
        public T GetNextValue() {
            return Value;
        }
    }

    public interface IValueList<T> : IValue<T> {
        public List<T> List { get; }
    }

    public class SequenceValues<T> : IValueList<T> {
        public List<T> List { get; init; }

        public SequenceValues(List<T> ts) {
            List = ts;
        }
        int index = -1;
        void Update() {
            index = (index + 1) % List.Count;
        }

        public T GetNextValue() {
            Update();
            return List[index];
        }
    }

    //途中の値のこともある
    public class RandomBetweenValues<T> : IValueList<T> {
        public List<T> List { get; init; }
        public RandomBetweenValues(List<T> ts) {
            List = ts;
        }
        public T GetNextValue() {
            return RandomHelper.GetRandomBetween(List.ToArray());
        }
    }
    //要素のどれかになる
    public class RandomValues<T> : IValueList<T> {
        public List<T> List { get; init; }
        public RandomValues(List<T> ts) {
            List = ts;
        }

        public T GetNextValue() {
            return RandomHelper.GetRandom(List.ToArray());
        }

    }


    public class LockValues<T> {
        public IValueList<T> Values { get; init; }

        [JsonIgnore]
        readonly Dictionary<int, T> lockValues = new();

        public LockValues(IValueList<T> list) { Values = list; }

        public T LockValue(int id, bool isOverride = false) {
            if (!isOverride && lockValues.TryGetValue(id, out var val)) {
                return val;
            }
            var value = Values.GetNextValue();
            lockValues[id] = value;
            return value;
        }

        public bool UnlockValue(int id, out T val) {
            return lockValues.Remove(id, out val);
        }
    }



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
        IAudio audio;
        public string path { get; init; }

        public AudioWrapper(IAudio audio) {
            this.audio = audio;
            path = audio.File.Name;
        }
        [JsonConstructor]
        public AudioWrapper(string path) {
            this.path = path;
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
        private static Dictionary<StorageFile, IAudio> audioMap = new();

        public static AudioGraph AudioGraph { get; private set; }
        public static AudioDeviceOutputNode OutputNode { get; private set; }

        public static async Task AudioInit() {
            AudioGraphSettings settings = new(AudioRenderCategory.SoundEffects) {
                QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency,
                MaxPlaybackSpeedFactor = 2
            };

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            AudioGraph = result.Graph;
            AudioGraph.UnrecoverableErrorOccurred += AudioGraph_UnrecoverableErrorOccurred;

            var createDeviceResult = await AudioGraph.CreateDeviceOutputNodeAsync();

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
            AudioGraph.Dispose();
        }

        private static void AudioGraph_UnrecoverableErrorOccurred(AudioGraph sender, AudioGraphUnrecoverableErrorOccurredEventArgs args) {
            switch (args.Error) {
                case AudioGraphUnrecoverableError.AudioDeviceLost:
                case AudioGraphUnrecoverableError.AudioSessionDisconnected:
                    break;
                case AudioGraphUnrecoverableError.UnknownFailure:
                    break;
                default:
                    break;
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