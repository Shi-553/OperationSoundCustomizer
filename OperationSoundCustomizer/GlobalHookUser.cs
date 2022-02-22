using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Input;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OperationSoundCustomizer
{
    public struct BoolSet : IEquatable<BoolSet>
    {
        public bool Currnet { private set; get; }
        public bool Prev { private set; get; }

        public bool WasDown => Currnet && !Prev;
        public bool IsReleased => !Currnet && Prev;

        public BoolSet(bool current, bool prev = false)
        {
            this.Currnet = current;
            Prev = prev;
        }

        public BoolSet GetUpdated(bool newCurrent)
        {
            return new(newCurrent, Currnet);
        }
        public BoolSet GetUpdated()
        {
            return new(Currnet, Currnet);
        }

        public override bool Equals(object obj)
        {
            return obj is BoolSet set && Equals(set);
        }

        public bool Equals(BoolSet other)
        {
            return Currnet == other.Currnet &&
                   Prev == other.Prev;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Currnet, Prev);
        }

        public static implicit operator bool(BoolSet b) => b.Currnet;

        public static bool operator ==(BoolSet left, BoolSet right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoolSet left, BoolSet right)
        {
            return !(left == right);
        }
    }

    class GlobalHookAccesser : IInputAccesser
    {
        private readonly Dictionary<InputCode, BoolSet> boolInputs = new();
        private readonly Dictionary<InputCode, float> floatInputs = new();
        private readonly Dictionary<InputCode, Point> pointInputs = new();

        public InputCode UpdatedCode { get; private set; }

        //イベント毎の初期化
        void EventInit()
        {
            //prevをcurrentと同じにする
            switch (UpdatedCode.GetInputType())
            {
                case InputType.Button:
                    BoolInputsUpdate(UpdatedCode);
                    break;

                case InputType.Float:
                    break;
                case InputType.Point:
                    break;
            }

            UpdatedCode = InputCode.None;
        }

        //prevをcurrentと同じにする
        void BoolInputsUpdate(InputCode code)
        {
            if (boolInputs.TryGetValue(code, out var set))
            {
                boolInputs[code] = set.GetUpdated();
                return;
            }
        }

        void BoolInputsSet(InputCode code, bool b)
        {
            UpdatedCode = code;
            if (boolInputs.TryGetValue(code, out var set))
            {
                boolInputs[code] = set.GetUpdated(b);
                return;
            }
            boolInputs[code] = new(b);
        }

        public void KeyboardMessageEvent(KeyboardMessageEventArgs e)
        {
            EventInit();

            //Debug.WriteLine(e.KeyboardStruct.flags.ToString());

            bool b = e.MessageType is KeyboardMessage.KeyDown or KeyboardMessage.SysKeyDown;

            BoolInputsSet((InputCode)e.KeyboardStruct.vkCode, b);
            Debug.WriteLine(UpdatedCode+": "+boolInputs[UpdatedCode].Currnet+"  "+ boolInputs[UpdatedCode].Prev);
        }

        public void MouseMessageEvent(MouseMessageEventArgs e)
        {
            EventInit();

            //Debug.WriteLine(e.MessageType);
            switch (e.MessageType)
            {
                case MouseMessage.MouseMove:
                    UpdatedCode = InputCode.Position;
                    pointInputs[UpdatedCode] = e.MouseStruct.pt;
                    break;
                case MouseMessage.LButtonDown:
                    BoolInputsSet(InputCode.LeftButton, true);
                    break;
                case MouseMessage.LButtonUp:
                    BoolInputsSet(InputCode.LeftButton, false);
                    break;
                case MouseMessage.RButtonDown:
                    BoolInputsSet(InputCode.RightButton, true);
                    break;
                case MouseMessage.RButtonUp:
                    BoolInputsSet(InputCode.RightButton, false);
                    break;

                case MouseMessage.MouseWheel:
                    UpdatedCode = InputCode.Wheel;
                    floatInputs[UpdatedCode] = e.MouseStruct.WheelDelta;
                    break;
                case MouseMessage.MouseHWheel:
                    UpdatedCode = InputCode.HWheel;
                    floatInputs[UpdatedCode] = e.MouseStruct.WheelDelta;
                    break;

                case MouseMessage.MButtonDown:
                    BoolInputsSet(InputCode.MiddleButton, true);
                    break;
                case MouseMessage.MButtonUp:
                    BoolInputsSet(InputCode.MiddleButton, false);
                    break;

                case MouseMessage.XButtonDown:
                    if (e.MouseStruct.IsXButton1)
                    {
                        BoolInputsSet(InputCode.XButton1, true);
                    }
                    if (e.MouseStruct.IsXButton2)
                    {
                        BoolInputsSet(InputCode.XButton2, true);
                    }
                    break;
                case MouseMessage.XButtonUp:
                    if (e.MouseStruct.IsXButton1)
                    {
                        BoolInputsSet(InputCode.XButton1, false);
                    }
                    if (e.MouseStruct.IsXButton2)
                    {
                        BoolInputsSet(InputCode.XButton2, false);
                    }
                    break;
            }
        }


        public T GetValue<T>(InputCode code)
        {
            return code.GetInputType() switch
            {
                InputType.Button => GetValueFromButton<T>(code),
                InputType.Float => GetValueFromFloat<T>(code),
                InputType.Point => GetValueFromPoint<T>(code),
                InputType.None or _ => default,
            };
        }
        private T GetValueFromButton<T>(InputCode code)
        {
            if (!boolInputs.TryGetValue(code, out var set))
            {
                set = boolInputs[code] = new();
            }

            var t = typeof(T);

            if (t == typeof(BoolSet))
            {
                return Unsafe.As<BoolSet, T>(ref set);
            }

            if (t == typeof(bool))
            {
                bool b = set;
                return Unsafe.As<bool, T>(ref b);
            }

            if (t == typeof(float))
            {
                float f = set ? 1.0f : 0.0f;

                return Unsafe.As<float, T>(ref f);
            }

            if (t == typeof(Point))
            {
                Point p = set ? new(1, 1) : new(0, 0);

                return Unsafe.As<Point, T>(ref p);
            }

            return default;
        }

        private T GetValueFromFloat<T>(InputCode code)
        {
            if (!floatInputs.TryGetValue(code, out var f))
            {
                f = 0;
            }

            var t = typeof(T);

            if (t == typeof(float))
            {
                return Unsafe.As<float, T>(ref f);
            }

            if (t == typeof(bool))
            {
                bool b = f != 0;
                return Unsafe.As<bool, T>(ref b);
            }

            if (t == typeof(BoolSet))
            {
                BoolSet set = new(f != 0);
                return Unsafe.As<BoolSet, T>(ref set);
            }

            if (t == typeof(Point))
            {
                Point p = new((int)f, (int)f);

                return Unsafe.As<Point, T>(ref p);
            }

            return default;
        }
        private T GetValueFromPoint<T>(InputCode code)
        {
            if (!pointInputs.TryGetValue(code, out var p))
            {
                p = new(0, 0);
            }

            var t = typeof(T);

            if (t == typeof(Point))
            {
                return Unsafe.As<Point, T>(ref p);
            }

            if (t == typeof(float))
            {
                float f = MathF.Sqrt((p.x * p.x) + (p.y * p.y));
                return Unsafe.As<float, T>(ref f);
            }

            if (t == typeof(bool))
            {
                bool b = p.x != 0 || p.y != 0;
                return Unsafe.As<bool, T>(ref b);
            }

            if (t == typeof(BoolSet))
            {
                BoolSet set = new(p.x != 0 || p.y != 0);
                return Unsafe.As<BoolSet, T>(ref set);
            }

            return default;
        }
    }
    public class StackAsyncActions
    {
        bool isProcessing = false;
        readonly Stack<Action> actions = new();
        readonly object lockObj = new();

        public Task Push(Action a)
        {
            return Task.Run(async () =>
            {

                lock (actions)
                {
                    actions.Push(a);
                }

                lock (lockObj)
                {
                    if (isProcessing)
                    {
                        return;
                    }
                    isProcessing = true;
                }

                while (true)
                {
                    Action action;

                    lock (actions)
                    {
                        if (!actions.TryPop(out action))
                        {
                            lock (lockObj)
                            {
                                isProcessing = false;
                            }
                            return;
                        }
                    }

                    await Task.Run(action);
                }
            });
        }
    }

    class GlobalHookUser : IDisposable
    {
        readonly MouseHookHelper mouseHook = new();
        readonly KeyboardHookHelper keyboardHook = new();
        readonly GlobalHookAccesser accesser = new();
        readonly StackAsyncActions actions = new();


        public void Init()
        {

            mouseHook.OnProc += MouseHook_OnProc; ;
            keyboardHook.OnProc += KeyboardHook_OnProc;

            keyboardHook.InstallHooks();
            mouseHook.InstallHooks();
        }

        private void MouseHook_OnProc(MouseHookHelper sender, MouseMessageEventArgs args)
        {
            actions.Push(() =>
            {
                accesser.MouseMessageEvent(args);

                StatementManager.ExecutingStatement(accesser);
            });
        }
        private void KeyboardHook_OnProc(KeyboardHookHelper sender, KeyboardMessageEventArgs args)
        {
            actions.Push(() =>
            {
                accesser.KeyboardMessageEvent(args);

                StatementManager.ExecutingStatement(accesser);
            });
        }


        public void Dispose()
        {
            mouseHook.Dispose();
            keyboardHook.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
