//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using FreneticScript.CommandSystem;
using OpenTK;
using OpenTK.Input;
using Voxalia.ClientGame.ClientMainSystem;
using FreneticScript;

namespace Voxalia.ClientGame.UISystem
{
    /// <summary>
    /// Handles keyboard input.
    /// TODO: Make non-static
    /// </summary>
    public class KeyHandler
    {
        public static Dictionary<Key, CommandScript> Binds;
        public static Dictionary<Key, CommandScript> InverseBinds;

        public static Dictionary<string, Key> namestokeys;
        public static Dictionary<Key, string> keystonames;

        public static bool Modified = false;

        /// <summary>
        /// Prepare key handler.
        /// </summary>
        public static void Init()
        {
            KeyPresses = new Queue<Key>();
            KeyPressList = new List<Key>();
            KeyUps = new Queue<Key>();
            Binds = new Dictionary<Key, CommandScript>();
            InverseBinds = new Dictionary<Key, CommandScript>();
            BindKey(Key.W, "+forward");
            BindKey(Key.S, "+backward");
            BindKey(Key.A, "+leftward");
            BindKey(Key.D, "+rightward");
            BindKey(Key.X, "stance crouch");
            BindKey(Key.C, "stance stand");
            BindKey(Key.P, "toggle g_firstperson");
            BindKey(Key.Space, "+upward");
            BindKey(Key.CapsLock, "+walk");
            BindKey(Key.ShiftLeft, "+sprint");
            BindKey(Key.LControl, "+movedown");
            // TODO: clean up F buttons
            BindKey(Key.F1, "toggle u_showhud");
            BindKey(Key.F2, "toggle u_highlight_targetblock;toggle u_highlight_placeblock");
            BindKey(Key.F3, "toggle u_debug");
            BindKey(Key.F12, "screenshot");
            BindKey(Key.F35, "+attack");
            BindKey(Key.F34, "+secondary");
            BindKey(Key.F, "+use");
            BindKey(Key.Q, "+itemleft");
            BindKey(Key.E, "+itemright");
            BindKey(Key.PageUp, "+itemup");
            BindKey(Key.PageDown, "+itemdown");
            BindKey(Key.Left, "+itemleft");
            BindKey(Key.Right, "+itemright");
            BindKey(Key.Up, "+itemup");
            BindKey(Key.Down, "+itemdown");
            BindKey(Key.B, "drop");
            BindKey(Key.G, "throw");
            BindKey(Key.R, "weaponreload");
            BindKey(Key.T, "talk");
            BindKey(Key.Y, "talk");
            BindKey(Key.Enter, "talk");
            BindKey(Key.Slash, "talk /");
            BindKey(Key.I, "inventory");
            BindKey(Key.V, "devel fly");
            BindKey(Key.Escape, "quit");
            BindKey(Key.F32, "itemnext");
            BindKey(Key.F31, "itemprev");
            BindKey(Key.Number1, "itemsel 0");
            BindKey(Key.Number2, "itemsel 1");
            BindKey(Key.Number3, "itemsel 2");
            BindKey(Key.Number4, "itemsel 3");
            BindKey(Key.Number5, "itemsel 4");
            BindKey(Key.Number6, "itemsel 5");
            BindKey(Key.Number7, "itemsel 6");
            BindKey(Key.Number8, "itemsel 7");
            BindKey(Key.Number9, "itemsel 8");
            BindKey(Key.Number0, "itemsel 9");
            BindKey(Key.Minus, "itemsel 10");
            BindKey(Key.Plus, "itemsel 11");
            BindKey(Key.Keypad0, "+quickitem click 0");
            BindKey(Key.Keypad1, "+quickitem click 1");
            BindKey(Key.Keypad2, "+quickitem click 2");
            BindKey(Key.Keypad3, "+quickitem click 3");
            BindKey(Key.Keypad4, "+quickitem click 4");
            BindKey(Key.Keypad5, "+quickitem click 5");
            BindKey(Key.Keypad6, "+quickitem click 6");
            BindKey(Key.Keypad7, "+quickitem click 7");
            BindKey(Key.Keypad8, "+quickitem click 8");
            BindKey(Key.Keypad9, "+quickitem click 9");
            BindKey(Key.KeypadMinus, "+quickitem click 10");
            BindKey(Key.KeypadPlus, "+quickitem click 11");
            namestokeys = new Dictionary<string, Key>();
            keystonames = new Dictionary<Key, string>();
            RegKey("a", Key.A); RegKey("b", Key.B); RegKey("c", Key.C);
            RegKey("d", Key.D); RegKey("e", Key.E); RegKey("f", Key.F);
            RegKey("g", Key.G); RegKey("h", Key.H); RegKey("i", Key.I);
            RegKey("j", Key.J); RegKey("k", Key.K); RegKey("l", Key.L);
            RegKey("m", Key.M); RegKey("n", Key.N); RegKey("o", Key.O);
            RegKey("p", Key.P); RegKey("q", Key.Q); RegKey("r", Key.R);
            RegKey("s", Key.S); RegKey("t", Key.T); RegKey("u", Key.U);
            RegKey("v", Key.V); RegKey("w", Key.W); RegKey("x", Key.X);
            RegKey("y", Key.Y); RegKey("z", Key.Z); RegKey("1", Key.Number1);
            RegKey("2", Key.Number2); RegKey("3", Key.Number3); RegKey("4", Key.Number4);
            RegKey("5", Key.Number5); RegKey("6", Key.Number6); RegKey("7", Key.Number7);
            RegKey("8", Key.Number8); RegKey("9", Key.Number9); RegKey("0", Key.Number0);
            RegKey("lalt", Key.AltLeft); RegKey("ralt", Key.AltRight);
            RegKey("f1", Key.F1); RegKey("f2", Key.F2); RegKey("f3", Key.F3);
            RegKey("f4", Key.F4); RegKey("f5", Key.F5); RegKey("f6", Key.F6);
            RegKey("f7", Key.F7); RegKey("f8", Key.F8); RegKey("f9", Key.F9);
            RegKey("f10", Key.F10); RegKey("f11", Key.F11); RegKey("f12", Key.F12);
            RegKey("enter", Key.Enter); RegKey("end", Key.End); RegKey("home", Key.Home);
            RegKey("insert", Key.Insert); RegKey("delete", Key.Delete); RegKey("pause", Key.Pause);
            RegKey("lshift", Key.ShiftLeft); RegKey("rshift", Key.ShiftRight); RegKey("tab", Key.Tab);
            RegKey("caps", Key.CapsLock); RegKey("lctrl", Key.ControlLeft); RegKey("rctrl", Key.ControlRight);
            RegKey("comma", Key.Comma); RegKey("dot", Key.Period); RegKey("slash", Key.Slash);
            RegKey("backslash", Key.BackSlash); RegKey("dash", Key.Minus); RegKey("equals", Key.Plus);
            RegKey("backspace", Key.BackSpace); RegKey("semicolon", Key.Semicolon); RegKey("quote", Key.Quote);
            RegKey("lbracket", Key.BracketLeft); RegKey("rbracket", Key.BracketRight); RegKey("kp1", Key.Keypad1);
            RegKey("kp2", Key.Keypad2); RegKey("kp3", Key.Keypad3); RegKey("kp4", Key.Keypad4);
            RegKey("kp5", Key.Keypad5); RegKey("kp6", Key.Keypad6); RegKey("kp7", Key.Keypad7);
            RegKey("kp8", Key.Keypad8); RegKey("kp9", Key.Keypad9); RegKey("kp0", Key.Keypad0);
            RegKey("kpenter", Key.KeypadEnter); RegKey("kpmultiply", Key.KeypadMultiply);
            RegKey("kpadd", Key.KeypadAdd); RegKey("kpsubtract", Key.KeypadSubtract);
            RegKey("kpdivide", Key.KeypadDivide); RegKey("kpperiod", Key.KeypadPeriod);
            RegKey("space", Key.Space); RegKey("escape", Key.Escape);
            RegKey("pageup", Key.PageUp); RegKey("pagedown", Key.PageDown);
            RegKey("left", Key.Left); RegKey("right", Key.Right);
            RegKey("up", Key.Up); RegKey("down", Key.Down);
            // Perhaps not the best way to do this, but it works so well!
            RegKey("mouse1", Key.F35);
            RegKey("mouse2", Key.F34);
            RegKey("mouse3", Key.F33);
            RegKey("mousewheelup", Key.F32);
            RegKey("mousewheeldown", Key.F31);
            RegKey("mouse4", Key.F30);
            RegKey("mouse5", Key.F29);
        }

        static void RegKey(string name, Key key)
        {
            namestokeys.Add(name, key);
            keystonames.Add(key, name);
        }

        /// <summary>
        /// Gets all text that was written since the information was last retrieved.
        /// </summary>
        public static KeyHandlerState GetKBState()
        {
            KeyHandlerState KB = new KeyHandlerState();
            lock (Locker)
            {
                KB.KeyboardString = _KeyboardString;
                _KeyboardString = "";
                KB.InitBS = _InitBS;
                _InitBS = 0;
                KB.ControlDown = _ControlDown;
                KB.CopyPressed = _CopyPressed;
                _CopyPressed = false;
                KB.EndDelete = _EndDelete;
                _EndDelete = 0;
                KB.LeftRights = _LeftRights;
                _LeftRights = 0;
                KB.Pages = _Pages;
                _Pages = 0;
                KB.Scrolls = _Scrolls;
                _Scrolls = 0;
                KB.TogglerPressed = _TogglerPressed;
                _TogglerPressed = false;
            }
            return KB;
        }

        static string _KeyboardString = "";

        static int _InitBS = 0;

        static int _EndDelete = 0;

        static bool _ControlDown = false;

        static bool _CopyPressed = false;

        public static bool _TogglerPressed = false;

        static int _Pages = 0;

        static int _Scrolls = 0;

        static int _LeftRights = 0;

        static bool _BindsValid = false;

        static Object Locker = new Object();

        static Queue<Key> KeyPresses;

        static Queue<Key> KeyUps;

        /// <summary>
        /// Called every time a key is pressed, adds to the Keyboard String.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Holds the pressed key.</param>
        public static void PrimaryGameWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Client.Central.Window.Focused)
            {
                return;
            }
            if (Char.IsControl(e.KeyChar))
            {
                return;
            }
            lock (Locker)
            {
                if (e.KeyChar == '`' || e.KeyChar == '~')
                {
                    _TogglerPressed = true;
                    return;
                }
                _KeyboardString += e.KeyChar;
            }
        }

        public static void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Client.Central.Window.Focused)
            {
                return;
            }
            lock (Locker)
            {
                if (_BindsValid)
                {
                    switch (e.Button)
                    {
                        case MouseButton.Left:
                            KeyPresses.Enqueue(Key.F35);
                            break;
                        case MouseButton.Right:
                            KeyPresses.Enqueue(Key.F34);
                            break;
                        case MouseButton.Middle:
                            KeyPresses.Enqueue(Key.F33);
                            break;
                        case MouseButton.Button1:
                            KeyPresses.Enqueue(Key.F30);
                            break;
                        case MouseButton.Button2:
                            KeyPresses.Enqueue(Key.F29);
                            break;
                            // TODO: More mouse buttons?
                    }
                }
            }
        }

        public static void Mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            if (!Client.Central.Window.Focused)
            {
                return;
            }
            if (e.DeltaPrecise != 0)
            {
                lock (Locker)
                {
                    if (_BindsValid)
                    {
                        Key k = e.DeltaPrecise < 0 ? Key.F31 : Key.F32;
                        if (!KeyPresses.Contains(k))
                        {
                            KeyPresses.Enqueue(k);
                        }
                    }
                }
            }
        }

        public static void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!Client.Central.Window.Focused)
            {
                return;
            }
            lock (Locker)
            {
                if (_BindsValid)
                {
                    switch (e.Button)
                    {
                        case MouseButton.Left:
                            KeyUps.Enqueue(Key.F35);
                            break;
                        case MouseButton.Right:
                            KeyUps.Enqueue(Key.F34);
                            break;
                        case MouseButton.Middle:
                            KeyUps.Enqueue(Key.F33);
                            break;
                        case MouseButton.Button1:
                            KeyUps.Enqueue(Key.F30);
                            break;
                        case MouseButton.Button2:
                            KeyUps.Enqueue(Key.F29);
                            break;
                            // TODO: More mouse buttons?
                    }
                }
            }
        }

        /// <summary>
        /// Called every time a key is pressed down, handles control codes for the Keyboard String.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Holds the pressed key.</param>
        public static void PrimaryGameWindow_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!Client.Central.Window.Focused)
            {
                return;
            }
            lock (Locker)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        _KeyboardString += "\n";
                        break;
                    case Key.Tab:
                        _KeyboardString += "\t";
                        break;
                    case Key.PageUp:
                        _Pages++;
                        break;
                    case Key.PageDown:
                        _Pages--;
                        break;
                    case Key.Up:
                        _Scrolls++;
                        break;
                    case Key.Down:
                        _Scrolls--;
                        break;
                    case Key.Left:
                        _LeftRights--;
                        break;
                    case Key.Right:
                        _LeftRights++;
                        break;
                    case Key.End:
                        _LeftRights = 9000;
                        break;
                    case Key.Home:
                        _LeftRights = -9000;
                        break;
                    case Key.ControlLeft:
                    case Key.ControlRight:
                        _ControlDown = true;
                        break;
                    case Key.C:
                        if (_ControlDown)
                        {
                            _CopyPressed = true;
                        }
                        break;
                    case Key.BackSpace:
                        if (_KeyboardString.Length == 0)
                        {
                            _InitBS++;
                        }
                        else
                        {
                            _KeyboardString = _KeyboardString.Substring(0, _KeyboardString.Length - 1);
                        }
                        break;
                    case Key.Delete:
                        _EndDelete++;
                        break;
                    case Key.V:
                        if (_ControlDown)
                        {
                            string copied;
                            copied = System.Windows.Forms.Clipboard.GetText().Replace('\r', ' ');
                            if (copied.Length > 0 && copied.EndsWith("\n"))
                            {
                                copied = copied.Substring(0, copied.Length - 1);
                            }
                            _KeyboardString += copied;
                            for (int i = 0; i < _KeyboardString.Length; i++)
                            {
                                if (_KeyboardString[i] < 32 && _KeyboardString[i] != '\n')
                                {
                                    _KeyboardString = _KeyboardString.Substring(0, i) +
                                        _KeyboardString.Substring(i + 1, _KeyboardString.Length - (i + 1));
                                    i--;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                if (_BindsValid)
                {
                    KeyPresses.Enqueue(e.Key);
                }
            }
        }

        /// <summary>
        /// Called every time a key is released, handles control codes for the Keyboard String.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Holds the pressed key.</param>
        public static void PrimaryGameWindow_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (!Client.Central.Window.Focused)
            {
                return;
            }
            lock (Locker)
            {
                switch (e.Key)
                {
                    case Key.ControlLeft:
                    case Key.ControlRight:
                        _ControlDown = false;
                        break;
                    default:
                        break;
                }
                if (_BindsValid)
                {
                    KeyUps.Enqueue(e.Key);
                }
            }
        }
        
        public static List<Key> KeyPressList;

        /// <summary>
        /// Updates the keyboard state.
        /// </summary>
        public static void Tick()
        {
            lock (Locker)
            {
                KeyPressList.Clear();
                _BindsValid = IsValid();
                while (KeyPresses.Count > 0)
                {
                    Key key = KeyPresses.Dequeue();
                    CommandScript script;
                    if (Binds.TryGetValue(key, out script))
                    {
                        CommandQueue queue = script.ToQueue(Client.Central.Commands.CommandSystem);
                        queue.CommandStack.Peek().Debug = DebugMode.MINIMAL;
                        queue.Execute();
                    }
                    KeyPressList.Add(key);
                }
                while (KeyUps.Count > 0)
                {
                    Key key = KeyUps.Dequeue();
                    CommandScript script;
                    if (InverseBinds.TryGetValue(key, out script))
                    {
                        CommandQueue queue = script.ToQueue(Client.Central.Commands.CommandSystem);
                        queue.CommandStack.Peek().Debug = DebugMode.MINIMAL;
                        queue.Execute();
                    }
                }
            }
        }

        public static Key GetKeyForName(string name)
        {
            Key key;
            if (namestokeys.TryGetValue(name.ToLowerFast(), out key))
            {
                return key;
            }
            return Key.Unknown;
        }

        /// <summary>
        /// Binds a key to a command.
        /// </summary>
        /// <param name="key">The key to bind.</param>
        /// <param name="bind">The command to bind to it (null to unbind).</param>
        public static void BindKey(Key key, string bind)
        {
            Binds.Remove(key);
            InverseBinds.Remove(key);
            if (bind != null)
            {
                CommandScript script = CommandScript.SeparateCommands("bind_" + key, bind, Client.Central.Commands.CommandSystem, false);
                script.Debug = DebugMode.MINIMAL;
                Binds[key] = script;
                if (script.Created.Entries.Length == 1 && script.Created.Entries[0].Marker == 1)
                {
                    CommandEntry fixedentry = script.Created.Entries[0].Duplicate();
                    fixedentry.Marker = 2;
                    CommandScript nscript = new CommandScript("inverse_bind_" + key, new List<CommandEntry>() { fixedentry }) { Debug = DebugMode.MINIMAL };
                    InverseBinds[key] = nscript;
                }
            }
            Modified = true;
        }

        /// <summary>
        /// Binds a key to a command.
        /// </summary>
        /// <param name="key">The key to bind.</param>
        /// <param name="bind">The command to bind to it (null to unbind).</param>
        public static void BindKey(Key key, List<CommandEntry> bind, int adj)
        {
            Binds.Remove(key);
            InverseBinds.Remove(key);
            if (bind != null)
            {
                CommandScript script = new CommandScript("_bind_for_" + keystonames[key], bind, adj);
                script.Debug = DebugMode.MINIMAL;
                Binds[key] = script;
                if (script.Created.Entries.Length == 1 && script.Created.Entries[0].Marker == 1)
                {
                    CommandEntry fixedentry = script.Created.Entries[0].Duplicate();
                    fixedentry.Marker = 2;
                    CommandScript nscript = new CommandScript("inverse_bind_" + key, new List<CommandEntry>() { fixedentry }) { Debug = DebugMode.MINIMAL };
                    InverseBinds[key] = nscript;
                }
            }
            Modified = true;
        }

        /// <summary>
        /// Checks whether the system is listening to keyboard input.
        /// </summary>
        /// <returns>Whether the keyboard is useable.</returns>
        public static bool IsValid()
        {
            return Client.Central.Window.Focused && !UIConsole.Open && !Client.Central.InvShown() && !Client.Central.ChatVisible;
        }

        /*
        /// <summary>
        /// Checks whether a key is pressed down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether it is down.</returns>
        public static bool IsDown(Key key)
        {
            return IsValid() && CurrentKeyboard.IsKeyDown(key);
        }
        */

        /// <summary>
        /// Checks whether a key was just pressed this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether it was just pressed.</returns>
        public static bool IsPressed(Key key)
        {
            //return IsValid() && CurrentKeyboard.IsKeyDown(key) && !PreviousKeyboard.IsKeyDown(key);
            return IsValid() && KeyPressList.Contains(key);
        }

        /// <summary>
        /// Returns the script this key is bound to.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>A script, or null.</returns>
        public static CommandScript GetBind(Key key)
        {
            CommandScript script;
            if (Binds.TryGetValue(key, out script))
            {
                return script;
            }
            return null;
        }
    }

    public class KeyHandlerState
    {
        /// <summary>
        /// All text that was written since the information was last retrieved.
        /// </summary>
        public string KeyboardString = "";

        /// <summary>
        /// How many backspaces were pressed, excluding ones that modified the KeyboardString.
        /// </summary>
        public int InitBS = 0;

        /// <summary>
        /// How many deletes were pressed.
        /// </summary>
        public int EndDelete = 0;

        /// <summary>
        /// Whether the control key is currently down, primarily for internal purposes.
        /// </summary>
        public bool ControlDown = false;

        /// <summary>
        /// Whether COPY (CTRL+C) was pressed.
        /// </summary>
        public bool CopyPressed = false;

        /// <summary>
        /// Whether the console toggling key (~) was pressed.
        /// </summary>
        public bool TogglerPressed = false;

        /// <summary>
        /// The number of times PageUp was pressed minus the number of times PageDown was pressed.
        /// </summary>
        public int Pages = 0;

        /// <summary>
        /// The number of times the UP arrow was pressed minus the number of times the DOWN arrow was pressed.
        /// </summary>
        public int Scrolls = 0;

        /// <summary>
        /// The number of times the RIGHT arrow was pressed minus the number of times the LEFT arrow was pressed.
        /// </summary>
        public int LeftRights = 0;

        /// <summary>
        /// Defaults the keyboard state.
        /// </summary>
        public void Clear()
        {
            Pages = 0;
            Scrolls = 0;
            LeftRights = 0;
            TogglerPressed = false;
            CopyPressed = false;
            ControlDown = false;
            EndDelete = 0;
            InitBS = 0;
            KeyboardString = "";
        }
    }
}
