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
        public UIMenu ChatMenu;

        public bool ChatVisible = false;

        public List<ChatMessage> ChatMessages = new List<ChatMessage>();
        
        public UITextBox ChatBox;

        public UIScrollGroup ChatScroller;

        public bool[] Channels;

        public void InitChatSystem()
        {
            ChatMenu = new UIMenu(this);
            FontSet font = FontSets.Standard;
            ChatBox = new UITextBox("", "Enter a /command or a chat message...", () => 30, () => Window.Height - (int)font.font_default.Height - 10 - UIBottomHeight, () => Window.Width - (30 * 2), font);
            ChatBox.EnterPressed = EnterChatMessage;
            ChatMenu.Add(ChatBox);
            int minY = 10 + (int)font.font_default.Height;
            ChatScroller = new UIScrollGroup(ChatBox.X, () => minY, ChatBox.Width, () => ChatBox.Y() - minY);
            ChatMenu.Add(ChatScroller);
            Channels = new bool[(int)TextChannel.COUNT];
            Func<float> xer = () => 30;
            for (int i = 0; i < Channels.Length; i++)
            {
                Channels[i] = true;
                string n = ((TextChannel)i).ToString();
                int len = (int)FontSets.Standard.MeasureFancyText(n);
                UITextLink link = null;
                Func<float> fxer = xer;
                int chan = i;
                link = new UITextLink(null, "^r^t^0^h^o^2" + n, "^!^e^t^0^h^o^2" + n, "^2^e^t^0^h^o^0" + n, () => ToggleLink(link, n, chan), fxer, () => 10, FontSets.Standard);
                xer = () => fxer() + len + 10;
                ChatMenu.Add(link);
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
            if (ChatVisible)
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
                ChatMenu.TickAll();
            }
        }

        public void RenderChatSystem()
        {
            if (ChatVisible)
            {
                ChatMenu.RenderAll(gDelta);
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
            KeyHandler.GetKBState();
            ChatVisible = true;
            FixMouse();
        }

        /// <summary>
        /// NOTE: Do not call this without good reason, let's not annoy players!
        /// </summary>
        public void CloseChat()
        {
            KeyHandler.GetKBState();
            ChatVisible = false;
            WVis = false;
            FixMouse();
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
            ChatScroller.Clear();
            float by = 0;
            for (int i = 0; i < ChatMessages.Count; i++)
            {
                if (Channels[(int)ChatMessages[i].Channel])
                {
                    by += FontSets.Standard.font_default.Height;
                    float y = by;
                    ChatScroller.Add(new UILabel(ChatMessages[i].Channel.ToString() + ": " + ChatMessages[i].Text, () => 0, () => y, FontSets.Standard, ChatScroller.MaxWidth));
                }
            }
        }
    }
}
