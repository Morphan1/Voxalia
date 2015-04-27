using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;
using Assimp.Configs;

namespace ShadowOperations.Shared
{
    public class ModelHandler
    {
        public Scene LoadModel(byte[] data, string ext)
        {
            using (AssimpContext ACont = new AssimpContext())
            {
                ACont.SetConfig(new NormalSmoothingAngleConfig(66f));
                return ACont.ImportFileFromStream(new DataStream(data), PostProcessSteps.Triangulate | PostProcessSteps.SortByPrimitiveType, ext);
            }
        }
    }
}
