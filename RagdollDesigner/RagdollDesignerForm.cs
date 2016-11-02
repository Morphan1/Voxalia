using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace RagdollDesigner
{
    public partial class RagdollDesignerForm : Form
    {
        public RagdollDesigner Designer;

        public GLControl GLCont;

        public Timer MainLoop;

        public RagdollDesignerForm(RagdollDesigner rd)
        {
            Designer = rd;
            InitializeComponent();
            Size size = panel1.Size;
            Point position = panel1.Location;
            panel1.Hide();
            GLCont = new GLControl(GraphicsMode.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
            GLCont.Location = position;
            GLCont.Size = panel1.Size;
            GLCont.Load += GLCont_Load;
            Controls.Add(GLCont);
        }

        private void GLCont_Load(object sender, EventArgs e)
        {
            GL.Viewport(GLCont.DisplayRectangle);
            MainLoop = new Timer();
            MainLoop.Interval = 16;
            MainLoop.Tick += MainLoop_Tick;
        }

        private void MainLoop_Tick(object sender, EventArgs e)
        {
            GLCont.SwapBuffers();
        }
    }
}
