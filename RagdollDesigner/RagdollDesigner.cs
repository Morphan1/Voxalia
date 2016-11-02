using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using System.Windows.Forms;

namespace RagdollDesigner
{
    public class RagdollDesigner
    {
        public RagdollDesignerForm Form;

        public void Start()
        {
            Form = new RagdollDesignerForm(this);
            Application.Run(Form);
        }
    }
}
