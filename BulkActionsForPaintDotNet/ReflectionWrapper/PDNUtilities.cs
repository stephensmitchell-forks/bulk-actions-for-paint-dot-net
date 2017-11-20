using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDNBulkUpdater.ReflectionWrapper
{
    class PDNUtilities
    {
        public static DialogResult ErrorBox(System.Windows.Forms.Form form, string text)
        {
            return PaintDotNet.SystemLayer.UIUtil.MessageBox(form, text, "paint.net", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static object PDNAnchorEdgeToAnchorEdge(PDNAnchorEdge anchorEdge)
        {
            Type anchorEdgeType = Util.GetAssemblyPaintDotNet().GetType("PaintDotNet.AnchorEdge");
            Array anchorEdgeValues = System.Enum.GetValues(anchorEdgeType);

            for (int i = 0; i < anchorEdgeValues.Length; i++)
            {
                if (anchorEdgeValues.GetValue(i).ToString().Equals(anchorEdge.ToString()))
                    return anchorEdgeValues.GetValue(i);
            }

            throw new Exception("Could not convert anchor type: " + anchorEdge.ToString());
        }
    }
}
