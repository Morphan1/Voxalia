//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;
using Voxalia.Shared;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public UIGroup ChatMenu;

        public List<ChatMessage> ChatMessages = new List<ChatMessage>();
        
        public UIInputBox ChatBox;

        public UIScrollBox ChatScroller;

        public bool[] Channels;

        public void InitChatSystem()
        {
            FontSet font = FontSets.Standard;
            int minY = 10 + (int)font.font_default.Height;
            ChatMenu = new UIGroup(UIAnchor.TOP_CENTER, () => Window.Width, () => Window.Height - minY - UIBottomHeight, () => 0, () => 0);
            ChatScroller = new UIScrollBox(UIAnchor.TOP_CENTER, () => ChatMenu.GetWidth() - (30 * 2), () => ChatMenu.GetHeight() - minY, () => 0, () => minY);
            ChatBox = new UIInputBox("", "Enter a /command or a chat message...", font, UIAnchor.TOP_CENTER, ChatScroller.GetWidth, () => 0, () => (int)ChatScroller.GetHeight() + minY);
            ChatBox.EnterPressed = EnterChatMessage;
            ChatMenu.AddChild(ChatBox);
            ChatMenu.AddChild(ChatScroller);
            Channels = new bool[(int)TextChannel.COUNT];
            Func<int> xer = () => 30;
            for (int i = 0; i < Channels.Length; i++)
            {
                Channels[i] = true;
                string n = ((TextChannel)i).ToString();
                int len = (int)FontSets.Standard.MeasureFancyText(n);
                UITextLink link = null;
                Func<int> fxer = xer;
                int chan = i;
                link = new UITextLink(null, "^r^t^0^h^o^2" + n, "^!^e^t^0^h^o^2" + n, "^2^e^t^0^h^o^0" + n, FontSets.Standard, () => ToggleLink(link, n, chan), UIAnchor.TOP_LEFT, fxer, () => 10);
                xer = () => fxer() + len + 10;
                ChatMenu.AddChild(link);
            }
        }

        void EnterChatMessage()
        {
            CloseChat();
            if (ChatBox.Text.Length == 0)
            {
                return;
            }
            if (ChatBox.Text.StartsWith("/"))
            {
                Commands.ExecuteCommands(ChatBox.Text);
            }
            else
            {
                CommandPacketOut packet = new CommandPacketOut("say\n" + ChatBox.Text);
                Network.SendPacket(packet);
            }
            ChatBox.Text = "";
            ChatBox.MinCursor = 0;
            ChatBox.MaxCursor = 0;
        }

        void ToggleLink(UITextLink link, string n, int chan)
        {
            char c = '2';
            Channels[chan] = !Channels[chan];
            if (!Channels[chan])
            {
                c = '&';
            }
            link.Text = "^r^t^0^h^o^" + c + n;
            link.TextHover = "^!^e^t^0^h^o^" + c + n;
            link.TextClick = "^" + c + "^e^t^0^h^o^0" + n;
            UpdateChats();
        }

        bool WVis = false;

        public void TickChatSystem()
        {
            if (IsChatVisible())
            {
                if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Escape)) // TODO: Better method for this!
                {
                    CloseChat();
                    return;
                }
                if (!WVis)
                {
                    KeyHandler.GetKBState();
                    WVis = true;
                }
                ChatBox.Selected = true;
            }
        }

        public void SetChatText(string text)
        {
            ChatBox.Text = text;
        }

        public string GetChatText()
        {
            return ChatBox.Text;
        }

        public void ShowChat()
        {
            if (!IsChatVisible())
            {
                KeyHandler.GetKBState();
                TheGameScreen.AddChild(ChatMenu);
                FixMouse();
            }
        }

        /// <summary>
        /// NOTE: Do not call this without good reason, let's not annoy players!
        /// </summary>
        public void CloseChat()
        {
            if (IsChatVisible())
            {
                KeyHandler.GetKBState();
                TheGameScreen.RemoveChild(ChatMenu);
                WVis = false;
                FixMouse();
            }
        }

        public bool IsChatVisible()
        {
            return TheGameScreen.HasChild(ChatMenu);
        }

        public void WriteMessage(TextChannel channel, string message)
        {
            UIConsole.WriteLine(channel + ": " + message);
            ChatMessage cm = new ChatMessage() { Channel = channel, Text = message };
            ChatMessages.Add(cm);
            if (ChatMessages.Count > 550)
            {
                ChatMessages.RemoveRange(0, 50);
            }
            UpdateChats();
        }
        
        public void UpdateChats()
        {
            ChatScroller.RemoveAllChildren();
            float by = 0;
            for (int i = 0; i < ChatMessages.Count; i++)
            {
                if (Channels[(int)ChatMessages[i].Channel])
                {
                    by += FontSets.Standard.font_default.Height;
                    int y = (int)by;
                    ChatScroller.AddChild(new UILabel(ChatMessages[i].Channel.ToString() + ": " + ChatMessages[i].Text, FontSets.Standard, UIAnchor.TOP_LEFT, () => 0, () => y, () => (int)ChatScroller.GetWidth()));
                }
            }
        }
    }
}
