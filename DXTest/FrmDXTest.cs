using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using D2dControl;

namespace DXTest
{
    public partial class frmDXTest : Form
    {
        class SampleControl : D2dControl.D2dControl
        {

            private float x = 0;
            private float y = 0;
            private float w = 10;
            private float h = 10;
            private float dx = 1;
            private float dy = 1;

            private Random rnd = new Random();

            public SampleControl()
            {
                Fps = 60;
                resCache.Add("RedBrush", t => new SolidColorBrush(t, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f)));
                resCache.Add("GreenBrush", t => new SolidColorBrush(t, new RawColor4(0.0f, 1.0f, 0.0f, 1.0f)));
                resCache.Add("BlueBrush", t => new SolidColorBrush(t, new RawColor4(0.0f, 0.0f, 1.0f, 1.0f)));
            }

            public override void Render(RenderTarget target)
            {
                //target.Clear(new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));
                SharpDX.Direct2D1.Brush brush = null;
                //switch (rnd.Next(3))
                //{
                //    case 0: brush = resCache["RedBrush"] as SharpDX.Direct2D1.Brush; break;
                //    case 1: brush = resCache["GreenBrush"] as SharpDX.Direct2D1.Brush; break;
                    /*case 2:*/ brush = resCache["BlueBrush"] as SharpDX.Direct2D1.Brush; //break;
                //}
                target.DrawRectangle(new RawRectangleF(x, y, x + w, y + h), brush);

                x = x + dx;
                y = y + dy;
                if (x >= ActualWidth - w || x <= 0)
                {
                    dx = -dx;
                }
                if (y >= ActualHeight - h || y <= 0)
                {
                    dy = -dy;
                }
            }
        }

        private SampleControl control;

        public frmDXTest()
        {
            InitializeComponent();

            control = new SampleControl();
            control.Width = wpfHost.Width;
            control.Height = wpfHost.Height;

            wpfHost.Child = control;
        }

        private void frmDXTest_Resize(object sender, EventArgs e)
        {
            wpfHost.Width = ClientSize.Width;
            wpfHost.Height = ClientSize.Height;
            control.Width = wpfHost.Width;
            control.Height = wpfHost.Height;
        }
    }
}
