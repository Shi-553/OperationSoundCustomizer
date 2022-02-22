using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationSoundCustomizer {
    public enum Device {
        None,
        Mouse,
        Keyboard,
        Gamepad
    }
    public enum InputType {
        None,
        Button,
        Float,
        Point
    }
    public static class InputCodeExtension {
        public static Device GetDevice(this InputCode Value) {
            switch (Value) {
                case InputCode.None:
                    return Device.None;

                case InputCode.LeftButton:
                case InputCode.RightButton:
                case InputCode.MiddleButton:
                case InputCode.XButton1:
                case InputCode.XButton2:
                case InputCode.Wheel:
                case InputCode.HWheel:
                case InputCode.Position:
                    return Device.Mouse;

                case InputCode.GamepadA:
                case InputCode.GamepadB:
                case InputCode.GamepadX:
                case InputCode.GamepadY:
                case InputCode.GamepadRightShoulder:
                case InputCode.GamepadLeftShoulder:
                case InputCode.GamepadLeftTrigger:
                case InputCode.GamepadRightTrigger:
                case InputCode.GamepadDPadUp:
                case InputCode.GamepadDPadDown:
                case InputCode.GamepadDPadLeft:
                case InputCode.GamepadDPadRight:
                case InputCode.GamepadMenu:
                case InputCode.GamepadView:
                case InputCode.GamepadLeftThumbstickButton:
                case InputCode.GamepadRightThumbstickButton:
                case InputCode.GamepadLeftThumbstickUp:
                case InputCode.GamepadLeftThumbstickDown:
                case InputCode.GamepadLeftThumbstickRight:
                case InputCode.GamepadLeftThumbstickLeft:
                case InputCode.GamepadRightThumbstickUp:
                case InputCode.GamepadRightThumbstickDown:
                case InputCode.GamepadRightThumbstickRight:
                case InputCode.GamepadRightThumbstickLeft:
                    return Device.Gamepad;

                default:
                    return  Device.Keyboard ;
            }
        }
        public static InputType GetInputType(this InputCode Value) {
            switch (Value) {
                case InputCode.Wheel:
                case InputCode.HWheel:
                    return InputType.Float;

                case InputCode.Position:
                    return InputType.Point;


                default:
                    return InputType.Button;
            }
        }

        //     The Shift key.
        public static readonly InputCode[] Shift = { InputCode.LeftShift, InputCode.RightShift };

        //     The Ctrl key. 
        public static readonly InputCode[] Control = { InputCode.LeftControl, InputCode.RightControl };

        //     The menu key or button.
        public static readonly InputCode[] Menu = { InputCode.LeftMenu, InputCode.RightMenu };

        //     The left Windows key.
        public static readonly InputCode[] Windows = { InputCode.LeftWindows, InputCode.RightWindows };
    }


    public enum InputCode {
        //     No virtual key value.
        None = 0,


        //Mouse

        //     The left mouse button.
        LeftButton = 1,
        //     The right mouse button.
        RightButton = 2,
        //     The middle mouse button.
        MiddleButton = 4,
        //     An additional "extended" device key or button (for example, an additional mouse
        //     button).
        XButton1 = 5,
        //     An additional "extended" device key or button (for example, an additional mouse
        //     button).
        XButton2 = 6,

        Wheel = 250,
        HWheel = 251,
        Position = 252,

        //KeyBoard

        //     The cancel key or button
        Cancel = 3,
        //     The virtual back key or button.
        Back = 8,
        //     The Tab key.
        Tab = 9,
        //     The Clear key or button.
        Clear = 12,
        //     The Enter key.
        Enter = 13,
        //     The Pause key or button.
        Pause = 19,
        //     The Caps Lock key or button.
        CapitalLock = 20,
        //     The Kana symbol key-shift button
        Kana = 21,
        //     The Hangul symbol key-shift button.
        Hangul = Kana,
        //     The Junja symbol key-shift button.
        Junja = 23,
        //     The Final symbol key-shift button.
        Final = 24,
        //     The Hanja symbol key shift button.
        Hanja = 25,
        //     The Kanji symbol key-shift button.
        Kanji = Hanja,
        //     The Esc key.
        Escape = 27,
        //     The convert button or key.
        Convert = 28,
        //     The nonconvert button or key.
        NonConvert = 29,
        //     The accept button or key.
        Accept = 30,
        //     The mode change key.
        ModeChange = 31,
        //     The Spacebar key or button.
        Space = 32,
        //     The Page Up key.
        PageUp = 33,
        //     The Page Down key.
        PageDown = 34,
        //     The End key.
        End = 35,
        //     The Home key.
        Home = 36,
        //     The Left Arrow key.
        Left = 37,
        //     The Up Arrow key.
        Up = 38,
        //     The Right Arrow key.
        Right = 39,
        //     The Down Arrow key.
        Down = 40,
        //     The Select key or button.
        Select = 41,
        //     The Print key or button.
        Print = 42,
        //     The execute key or button.
        Execute = 43,
        //     The snapshot key or button.
        Snapshot = 44,
        //     The Insert key.
        Insert = 45,
        //     The Delete key.
        Delete = 46,
        //     The Help key or button.
        Help = 47,
        //     The number "0" key.
        Number0 = 48,
        //     The number "1" key.
        Number1 = 49,
        //     The number "2" key.
        Number2 = 50,
        //     The number "3" key.
        Number3 = 51,
        //     The number "4" key.
        Number4 = 52,
        //     The number "5" key.
        Number5 = 53,
        //     The number "6" key.
        Number6 = 54,
        //     The number "7" key.
        Number7 = 55,
        //     The number "8" key.
        Number8 = 56,
        //     The number "9" key.
        Number9 = 57,
        //     The letter "A" key.
        A = 65,
        //     The letter "B" key.
        B = 66,
        //     The letter "C" key.
        C = 67,
        //     The letter "D" key.
        D = 68,
        //     The letter "E" key.
        E = 69,
        //     The letter "F" key.
        F = 70,
        //     The letter "G" key.
        G = 71,
        //     The letter "H" key.
        H = 72,
        //     The letter "I" key.
        I = 73,
        //     The letter "J" key.
        J = 74,
        //     The letter "K" key.
        K = 75,
        //     The letter "L" key.
        L = 76,
        //     The letter "M" key.
        M = 77,
        //     The letter "N" key.
        N = 78,
        //     The letter "O" key.
        O = 79,
        //     The letter "P" key.
        P = 80,
        //     The letter "Q" key.
        Q = 81,
        //     The letter "R" key.
        R = 82,
        //     The letter "S" key.
        S = 83,
        //     The letter "T" key.
        T = 84,
        //     The letter "U" key.
        U = 85,
        //     The letter "V" key.
        V = 86,
        //     The letter "W" key.
        W = 87,
        //     The letter "X" key.
        X = 88,
        //     The letter "Y" key.
        Y = 89,
        //     The letter "Z" key.
        Z = 90,
        //     The left Windows key.
        LeftWindows = 91,
        //     The right Windows key.
        RightWindows = 92,
        //     The application key or button.
        Application = 93,
        //     The sleep key or button.
        Sleep = 95,
        //     The number "0" key as located on a numeric pad.
        NumberPad0 = 96,
        //     The number "1" key as located on a numeric pad.
        NumberPad1 = 97,
        //     The number "2" key as located on a numeric pad.
        NumberPad2 = 98,
        //     The number "3" key as located on a numeric pad.
        NumberPad3 = 99,
        //     The number "4" key as located on a numeric pad.
        NumberPad4 = 100,
        //     The number "5" key as located on a numeric pad.
        NumberPad5 = 101,
        //     The number "6" key as located on a numeric pad.
        NumberPad6 = 102,
        //     The number "7" key as located on a numeric pad.
        NumberPad7 = 103,
        //     The number "8" key as located on a numeric pad.
        NumberPad8 = 104,
        //     The number "9" key as located on a numeric pad.
        NumberPad9 = 105,
        //     The multiply (*) operation key as located on a numeric pad.
        Multiply = 106,
        //     The add (+) operation key as located on a numeric pad.
        Add = 107,
        //     The separator key as located on a numeric pad.
        Separator = 108,
        //     The subtract (-) operation key as located on a numeric pad.
        Subtract = 109,
        //     The decimal (.) key as located on a numeric pad.
        Decimal = 110,
        //     The divide (/) operation key as located on a numeric pad.
        Divide = 111,
        //     The F1 function key.
        F1 = 112,
        //     The F2 function key.
        F2 = 113,
        //     The F3 function key.
        F3 = 114,
        //     The F4 function key.
        F4 = 115,
        //     The F5 function key.
        F5 = 116,
        //     The F6 function key.
        F6 = 117,
        //     The F7 function key.
        F7 = 118,
        //     The F8 function key.
        F8 = 119,
        //     The F9 function key.
        F9 = 120,
        //     The F10 function key.
        F10 = 121,
        //     The F11 function key.
        F11 = 122,
        //     The F12 function key.
        F12 = 123,
        //     The F13 function key.
        F13 = 124,
        //     The F14 function key.
        F14 = 125,
        //     The F15 function key.
        F15 = 126,
        //     The F16 function key.
        F16 = 127,
        //     The F17 function key.
        F17 = 128,
        //     The F18 function key.
        F18 = 129,
        //     The F19 function key.
        F19 = 130,
        //     The F20 function key.
        F20 = 131,
        //     The F21 function key.
        F21 = 132,
        //     The F22 function key.
        F22 = 133,
        //     The F23 function key.
        F23 = 134,
        //     The F24 function key.
        F24 = 135,
        //     The navigation up button.
        NavigationView = 136,
        //     The navigation menu button.
        NavigationMenu = 137,
        //     The navigation up button.
        NavigationUp = 138,
        //     The navigation down button.
        NavigationDown = 139,
        //     The navigation left button.
        NavigationLeft = 140,
        //     The navigation right button.
        NavigationRight = 141,
        //     The navigation accept button.
        NavigationAccept = 142,
        //     The navigation cancel button.
        NavigationCancel = 143,
        //     The Num Lock key.
        NumberKeyLock = 144,
        //     The Scroll Lock (ScrLk) key.
        Scroll = 145,
        //     The left Shift key.
        LeftShift = 160,
        //     The right Shift key.
        RightShift = 161,
        //     The left Ctrl key.
        LeftControl = 162,
        //     The right Ctrl key.
        RightControl = 163,
        //     The left menu key.
        LeftMenu = 164,
        //     The right menu key.
        RightMenu = 165,
        //     The go back key.
        GoBack = 166,
        //     The go forward key.
        GoForward = 167,
        //     The refresh key.
        Refresh = 168,
        //     The stop key.
        Stop = 169,
        //     The search key.
        Search = 170,
        //     The favorites key.
        Favorites = 171,
        //     The go home key.
        GoHome = 172,

        //GamePad

        //     The gamepad A button.
        GamepadA = 195,
        //     The gamepad B button.
        GamepadB = 196,
        //     The gamepad X button.
        GamepadX = 197,
        //     The gamepad Y button.
        GamepadY = 198,
        //     The gamepad right shoulder.
        GamepadRightShoulder = 199,
        //     The gamepad left shoulder.
        GamepadLeftShoulder = 200,
        //     The gamepad left trigger.
        GamepadLeftTrigger = 201,
        //     The gamepad right trigger.
        GamepadRightTrigger = 202,
        //     The gamepad d-pad up.
        GamepadDPadUp = 203,
        //     The gamepad d-pad down.
        GamepadDPadDown = 204,
        //     The gamepad d-pad left.
        GamepadDPadLeft = 205,
        //     The gamepad d-pad right.
        GamepadDPadRight = 206,
        //     The gamepad menu button.
        GamepadMenu = 207,
        //     The gamepad view button.
        GamepadView = 208,
        //     The gamepad left thumbstick button.
        GamepadLeftThumbstickButton = 209,
        //     The gamepad right thumbstick button.
        GamepadRightThumbstickButton = 210,
        //     The gamepad left thumbstick up.
        GamepadLeftThumbstickUp = 211,
        //     The gamepad left thumbstick down.
        GamepadLeftThumbstickDown = 212,
        //     The gamepad left thumbstick right.
        GamepadLeftThumbstickRight = 213,
        //     The gamepad left thumbstick left.
        GamepadLeftThumbstickLeft = 214,
        //     The gamepad right thumbstick up.
        GamepadRightThumbstickUp = 215,
        //     The gamepad right thumbstick down.
        GamepadRightThumbstickDown = 216,
        //     The gamepad right thumbstick right.
        GamepadRightThumbstickRight = 217,
        //     The gamepad right thumbstick left.
        GamepadRightThumbstickLeft = 218
    }



}
