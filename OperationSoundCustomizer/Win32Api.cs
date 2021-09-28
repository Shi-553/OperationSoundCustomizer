﻿using System;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.System;
using System.Text;
#if DEBUG
using System.Diagnostics;
#endif

namespace OperationSoundCustomizer {
    public class InputHookHelper : IDisposable {
        private readonly HookProc msProc;
        private readonly HookProc kbProc;

        public IntPtr MouseHook { get; private set; } = IntPtr.Zero;
        public IntPtr KeyboardHook { get; private set; } = IntPtr.Zero;

        public event TypedEventHandler<InputHookHelper, NewMouseMessageEventArgs> NewMouseMessage;
        public event TypedEventHandler<InputHookHelper, NewKeyboardMessageEventArgs> NewKeyboardMessage;

        public InputHookHelper() {
            msProc = LowLevelMouseProc;
            kbProc = LowLevelKeyboardProc;
        }

        public void InstallHooks() {
            InstallHooks(IntPtr.Zero);
        }

        public void InstallHooks(IntPtr hMod) {
#if DEBUG
            DebugLog("Installing Hooks");
#endif
            if (MouseHook == IntPtr.Zero)
                MouseHook = NativeMethods.SetWindowsHookEx(HookType.LowLevelMouse, msProc, hMod, 0);
            if (KeyboardHook == IntPtr.Zero)
                KeyboardHook = NativeMethods.SetWindowsHookEx(HookType.LowLevelKeyboard, kbProc, hMod, 0);
        }

        public void UninstallHooks() {
#if DEBUG
            DebugLog("Uninstalling Hooks");
#endif
            NativeMethods.UnhookWindowsHookEx(MouseHook);
            NativeMethods.UnhookWindowsHookEx(KeyboardHook);
            MouseHook = KeyboardHook = IntPtr.Zero;
        }

        private IntPtr LowLevelMouseProc(int nCode, UIntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                var st = Marshal.PtrToStructure<MouseLowLevelHookStruct>(lParam);
                NewMouseMessage?.Invoke(this, new NewMouseMessageEventArgs(st, (MouseMessage)wParam));
            }
            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                var st = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(lParam);
                NewKeyboardMessage?.Invoke(this, new NewKeyboardMessageEventArgs(st, (KeyboardMessage)wParam));
            }
            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
#if DEBUG
            DebugLog($"Dispose() called, disposing: {disposing}, disposedValue: {disposedValue}");
#endif
            if (!disposedValue) {
                UninstallHooks();
                disposedValue = true;
            }
        }

        ~InputHookHelper() => Dispose(false);

        public void Dispose() {
#if DEBUG
            DebugLog("Dispose() called");
#endif
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

#if DEBUG
        private void DebugLog(string message)
            => Debug.WriteLine($"[InputHookHelper:{GetHashCode():X}] {message}");
#endif
    }

    public class NewMouseMessageEventArgs : EventArgs {
        public MouseLowLevelHookStruct MouseStruct { get; private set; }
        public MouseMessage MessageType { get; private set; }

        public NewMouseMessageEventArgs(MouseLowLevelHookStruct mouseStruct, MouseMessage msg) {
            this.MouseStruct = mouseStruct;
            MessageType = msg;
        }
    }

    public class NewKeyboardMessageEventArgs : EventArgs {
        public KeyboardLowLevelHookStruct KeyboardStruct { get; private set; }
        public KeyboardMessage MessageType { get; private set; }

        public NewKeyboardMessageEventArgs(KeyboardLowLevelHookStruct keyboardStruct, KeyboardMessage msg) {
            this.KeyboardStruct = keyboardStruct;
            MessageType = msg;
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr HookProc(int nCode, UIntPtr wParam, IntPtr lParam);

    internal enum HookType {
        LowLevelKeyboard = 13,
        LowLevelMouse = 14
    }

    public enum MouseMessage {
        MouseMove = 0x0200,       //マウス カーソルが移動したことを示します。
        LButtonDown = 0x0201,     //左のマウス ボタンがいつ押されたかを示します。
        LButtonUp = 0x0202,       //左のマウス ボタンがいつ離されたかを示します。
        LButtonDblClk = 0x0203,   //マウスの左ボタンをダブルクリックしたことを示します。
        RButtonDown = 0x0204,     //マウスの右ボタンがいつ押されたかを示します。
        RButtonUp = 0x0205,       //マウスの右ボタンがいつ離されたかを示します。
        RButtonDblClk = 0x0206,   //マウスの右ボタンをダブルクリックしたことを示します。
        MButtonDown = 0x0207,     //中央のマウス ボタンがいつ押されたかを示します。
        MButtonUp = 0x0208,       //中央のマウス ボタンがいつ離されたかを示します。
        MButtonDblClk = 0x0209,   //マウスの中央ボタンをダブルクリックしたことを示します。
        MouseWheel = 0x020A,      //マウス ホイールが回転した事を示します。
        XButtonDown = 0x020B,     //マウスの 4 つ目以降のボタンがいつ押されたかを示します。
        XButtonUp = 0x020C,       //マウスの 4 つ目以降のボタンがいつ離されたかを示します。
        XButtonDblClk = 0x020D,   //マウスの 4 つ目以降のボタンをダブルクリックしたことを示します。
        MouseHWheel = 0x020E,     //マウス ホイールが回転した事を示します。
    }

    public enum KeyboardMessage {
        KeyDown = 0x100,
        KeyUp = 0x101,
        SysKeyDown = 0x104,
        SysKeyUp = 0x105
    }

    //https://docs.microsoft.com/ja-jp/windows/win32/api/winuser/ns-winuser-msllhookstruct?redirectedfrom=MSDN
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseLowLevelHookStruct {
        //モニターごとの 画面座標。 
        public Point pt;
        public int mouseData;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;

        public short MouseDataUpperWord => (short)(mouseData >> 16);
        //おそらく使われない
        public short MouseDataLowerWord => (short)(mouseData);

        public bool IsXButton1 => Convert.ToBoolean((MouseDataUpperWord >> 0) & 1);
        public bool IsXButton2 => Convert.ToBoolean((MouseDataUpperWord >> 1) & 1);

        public short WheelDelta => MouseDataUpperWord;

        //イベントが挿入されたものかどうか
        public bool IsInjected => Convert.ToBoolean((flags >> 0) & 1);
        //挿入されていた場合、加えて低いレベルからかどうか
        public bool IsLowerInjected => Convert.ToBoolean((flags >> 1) & 1);

        public override string ToString() {
            StringBuilder sb = new();
            sb.Append("{ ");
            sb.Append("pt:");
            sb.Append(pt);
            sb.Append(",mouseData:");
            sb.Append(mouseData);
            sb.Append(",flags:");
            sb.Append(flags);
            sb.Append(",time:");
            sb.Append(time);
            sb.Append(",dwExtraInfo:");
            sb.Append(dwExtraInfo);
            sb.Append(" }");

            return sb.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point {
        public int x;
        public int y;

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public override string ToString() {
            StringBuilder sb = new();
            sb.Append("{ ");
            sb.Append("x:");
            sb.Append(x);
            sb.Append(",y:");
            sb.Append(y);
            sb.Append(" }");

            return sb.ToString();
        }
    }


    //https://docs.microsoft.com/ja-jp/windows/win32/api/winuser/ns-winuser-kbdllhookstruct?redirectedfrom=MSDN
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardLowLevelHookStruct {
        //1から254の仮想キーコード
        public VirtualKey vkCode;
        //ハードウェアスキャンコード。 
        public int scanCode;
        //挿入されたものかとか拡張キーとか遷移じょうたいを表す（プロパティで用意）
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;


        //ファンクションキーやテンキーのキーなどの拡張キーであるかどうかを指定します
        public bool IsExtendedKey => Convert.ToBoolean((flags >> 0) & 1);

        //イベントが挿入されたものかどうか
        public bool IsInjected => Convert.ToBoolean((flags >> 4) & 1);
        //挿入されていた場合、加えて低いレベルからかどうか
        public bool IsLowerInjected => Convert.ToBoolean((flags >> 1) & 1);

        //Altキーが押された場合、値は1です
        public bool IsContext => Convert.ToBoolean((flags >> 5) & 1);

        //遷移状態。キーが押されている場合は0、離されている場合は1です。 
        public bool TransitionState => Convert.ToBoolean((flags >> 7) & 1);
    }


    internal class NativeMethods {
        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowsHookEx(HookType idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        internal static extern int UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(IntPtr _, int nCode, UIntPtr wParam, IntPtr lParam);
    }

    /*
    public class UsageProgram
    {
        static InputHookHelper helper = new InputHookHelper();
        static void Test(string[] args)
        {
            helper.NewMouseMessage += (sender, e) =>
                Debug.WriteLine($"{e.MessageType}, x: {e.Position.x}, y: {e.Position.y}");
            helper.NewKeyboardMessage += (sender, e) =>
                Debug.WriteLine($"{e.MessageType}, VirtKeyCode: {e.VirtKeyCode}");
            helper.InstallHooks();
            // Do something ...
            helper.UninstallHooks();
        }
    }
    */
}