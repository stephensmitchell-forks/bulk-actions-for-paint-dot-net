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
    }
}
