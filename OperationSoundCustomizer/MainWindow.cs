using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OperationSoundCustomizer.Condition;
using Windows.System;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Windows.ApplicationModel;
using Windows.Storage;

namespace OperationSoundCustomizer {

    sealed partial class MainWindow : IDisposable {
        readonly GlobalHookUser globalHookUser = new();
        // notifyIcon;


        public async static Task<MainWindow> CreateAsync() {
            MainWindow mainWindow = new();

            await AudioControl.AudioInit();
            await mainWindow.RegisterStatement();

            mainWindow.globalHookUser.Init();
            
            return mainWindow;
        }
        private MainWindow() {
        }

        static List<IStatement> GetDefaultStatements() {
            List<IStatement> statements = new();


            statements.Add(new PairListStatement(
                new WasDownNow(InputCode.Enter),
                new IsReleasedNow(InputCode.Enter),
                new(CreateSequenceNumberName("Enter-", 1, 8))
                ));

            var controlAndShift = InputCodeExtension.Control.Concat(InputCodeExtension.Shift).ToArray();

            statements.Add(new PairListStatement(
                new AnyCondition<WasDownNow>(controlAndShift),
                new AnyCondition<IsReleasedNow>(controlAndShift),
                new(CreateSequenceNumberName("Shift-", 1, 4))
                ));


            statements.Add(new PairListStatement(
                new WasDownNow(InputCode.Space),
                new IsReleasedNow(InputCode.Space),
                new(CreateSequenceNumberName("Space-", 1, 4))
                ));


            InputCode[] arrowAndNumkey = {
            InputCode.Up,InputCode.Down, InputCode.Left,InputCode.Right,
            InputCode.NumberPad0,
            InputCode.NumberPad1,
            InputCode.NumberPad2,
            InputCode.NumberPad3,
            InputCode.NumberPad4,
            InputCode.NumberPad5,
            InputCode.NumberPad6,
            InputCode.NumberPad7,
            InputCode.NumberPad8,
            InputCode.NumberPad9
            };

            statements.Add(new PairListStatement(
                new AnyCondition<WasDownNow>(arrowAndNumkey),
                new AnyCondition<IsReleasedNow>(arrowAndNumkey),
                new(CreateSequenceNumberName("Arrows-", 1, 16))
                ));


            statements.Add(new PairListStatement(
                new WasAnyDownNow(Device.Keyboard),
                new IsAnyReleasedNow(Device.Keyboard),
                new(CreateSequenceNumberName("Keyboard-", 1, 22))
                ));


            statements.Add(new PairListStatement(
                new WasDownNow(InputCode.LeftButton),
                new IsReleasedNow(InputCode.LeftButton),
                new(CreateSequenceNumberName("MouseL-", 1, 2))
                ));

            statements.Add(new PairListStatement(
                new WasDownNow(InputCode.RightButton),
                new IsReleasedNow(InputCode.RightButton),
                new(CreateSequenceNumberName("MouseR-", 1, 2))
                ));

            statements.Add(new PairListStatement(
                new WasDownNow(InputCode.MiddleButton),
                new IsReleasedNow(InputCode.MiddleButton) | new IsReleasedNow(InputCode.HWheel),
                new(CreateSequenceNumberName("MouseM-", 1, 2))
                ));
            return statements;
        }


        //いずれJSONで読み込むようにする...->その時が来た！
        async Task RegisterStatement() {
            string assetsPath = Package.Current.InstalledLocation.Path + "\\Assets";
            string fileName = "Statement.json";
            string folderName = "settings";

            var assetsFolder = await StorageFolder.GetFolderFromPathAsync(assetsPath);

            var settingFolder = await assetsFolder.TryGetItemAsync(folderName) as StorageFolder;
            if (settingFolder == null) {
                settingFolder = await assetsFolder.CreateFolderAsync(folderName);
            }

            var file = await settingFolder.TryGetItemAsync(fileName) as StorageFile;

            var settings = new JsonSerializerSettings {
                // 見やすいようにインデントで整形
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
            settings.Converters.Add(new StringEnumConverter { });


            if (file == null) {
                var defaultStatements = GetDefaultStatements();
                foreach (var s in defaultStatements) {
                    StatementManager.RegisterStatement(s);
                }

                string defaultSerialized = JsonConvert.SerializeObject(defaultStatements, settings);
                file=await settingFolder.CreateFileAsync(fileName);

                await FileIO.WriteTextAsync(file, defaultSerialized);
                return;
            }

            string serialized = await FileIO.ReadTextAsync(file);
            Debug.WriteLine(serialized);


            var statements = JsonConvert.DeserializeObject<List<IStatement>>(serialized, settings);



            foreach (var s in statements) {
                StatementManager.RegisterStatement(s);
            }
        }

        static RandomValues<AudioPair> CreateSequenceNumberName(string baseName, int begin, int end) {
            RandomValues<AudioPair> randomValue = new(new());
            for (int i = begin; i <= end; i += 2) {
                randomValue.List.Add(AudioControl.GetAudioPair(baseName + i.ToString("000") + ".wav", baseName + (i + 1).ToString("000")));
            }
            return randomValue;
        }

        public void Dispose() {
            StatementManager.ClearStatement();
            AudioControl.Dispose();
            globalHookUser.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
