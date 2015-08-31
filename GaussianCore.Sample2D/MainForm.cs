using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GaussianCore.Generic;
using GaussianCore.Spheric;

namespace GaussianCore.Sample2D
{
    public partial class MainForm : Form
    {
        #region Fields

        private ICoreManager _cm;

        private readonly Random _r = new Random(123);

        private const double FuncWidth = Math.PI*6;

        private readonly Image[] _imageBuf = new Image[2];

        private int _imageIndex;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitBufs(MainPictureBox.Width, MainPictureBox.Height);

            //_cm = CreateFixedSphericCores(100);
            //_cm = CreateVariableSphericCores(100);
            _cm = CreateFixedConfinedCores(100);
            //_cm = CreateVariableConfinedCores(100);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            InitBufs(MainPictureBox.Width, MainPictureBox.Height);
            MainPictureBox.Invalidate();
        }

        private void InitBufs(int width, int height)
        {
            for(var i = 0; i< _imageBuf.Length; i++)
            {
                _imageBuf[i] = new Bitmap(width, height);
            }
        }

        private void MainPictureBox_Paint(object sender, PaintEventArgs e)
        {
            var currImage = _imageBuf[_imageIndex];
            Refresh(currImage);
            MainPictureBox.Image = currImage;
            _imageIndex++;
            if (_imageIndex >= _imageBuf.Length)
            {
                _imageIndex = 0;
            }
        }

        private void Refresh(Image image)
        {
            using (var g = Graphics.FromImage(image))
            {
                PaintPdf(g, 0, -2, FuncWidth, 4, 100, 80);
                PaintCores(g, 0, -2, FuncWidth, 4);
                PaintCurves(g, 0, -2, FuncWidth, 4, 0.1);
            }
        }

        private void PaintCores(Graphics g, double xstart, double ystart,
            double xwidth, double ywidth)
        {
            var pen = new Pen(Color.Pink, 2);
            var w = ClientRectangle.Width;
            var h = ClientRectangle.Height;
            var pw = w / xwidth;
            var ph = h / ywidth;

            var cores = _cm as IEnumerable<Core>;
            if (cores != null)
            {
                foreach (var c in cores)
                {
                    var x = c.Components[0].Center;
                    var y = c.Components[1].Center;
                    var lx = c.Components[0].L;
                    var ly = c.Components[1].L;
                    var dx = Math.Sqrt(-Math.Log(2)/lx);
                    var dy = Math.Sqrt(-Math.Log(2)/ly);
                    var vx = XToVx(x, xstart, pw);
                    var vy = YToVy(y, ystart, ph);
                    var vdx = (float) (dx*pw);
                    var vdy = (float) (dy*ph);
                    g.DrawEllipse(pen, vx - vdx, vy - vdy, 2*vdx, 2*vdy);
                }
            }
            else
            {
                var gcores = (IEnumerable<GenericCore>)_cm;
                foreach (var c in gcores.Cast<GaussianConfinedCore>())
                {
                    var x = c.CentersInput[0];
                    var y = c.CentersOutput[0];
                    var lx = c.K[0];
                    var ly = c.L[0];
                    var dx = Math.Sqrt(-Math.Log(2) / lx);
                    var dy = Math.Sqrt(-Math.Log(2) / ly);
                    var vx = XToVx(x, xstart, pw);
                    var vy = YToVy(y, ystart, ph);
                    var vdx = (float)(dx * pw);
                    var vdy = (float)(dy * ph);
                    g.DrawEllipse(pen, vx - vdx, vy - vdy, 2 * vdx, 2 * vdy);
                }
            }
        }

        private void PaintPdf(Graphics g, double xstart, double ystart,
            double xwidth, double ywidth, int xcount, int ycount)
        {
            var w = ClientRectangle.Width;
            var h = ClientRectangle.Height;
            var pw = (float)(w / xwidth);
            var ph = (float)(h / ywidth);

            var xinc = xwidth / xcount;
            var yinc = ywidth / ycount;
            int i = 0, j;

            var data = new double[ycount, xcount];

            var max = 0.0;
            for (var y = ystart; i < ycount; i++, y += yinc)
            {
                j = 0;
                for (var x = xstart; j < xcount; j++, x += xinc)
                {
                    var a = _cm.GetIntensity(new[] { x }, new[] { y });
                    data[i, j] = a;
                    if (a > max)
                    {
                        max = a;
                    }
                }
            }

            i = 0;
            for (var y = ystart; i < ycount; i++, y += yinc)
            {
                j = 0;
                for (var x = xstart; j < xcount; j++, x += xinc)
                {
                    var a = data[i, j] / max; 
                    var p = (int)(a * 255);
                    var clr = Color.FromArgb(p, p, p);
                    var brush = new SolidBrush(clr);

                    var vx = XToVx(x, xstart, pw);
                    var vy = YToVy(y, ystart, ph);
                    g.FillRectangle(brush, vx, vy, pw, ph);
                }
            }
        }

        private void PaintCurves(Graphics g, double xstart, double ystart,
            double xwidth, double ywidth, double dx)
        {
            var w = ClientRectangle.Width;
            var h = ClientRectangle.Height;
            var pw = w / xwidth;
            var ph = h / ywidth;

            var penIdeal = new Pen(new SolidBrush(Color.Green));
            var pen = new Pen(new SolidBrush(Color.Red));
            var pinkPen = new Pen(new SolidBrush(Color.Orange));
            var lastpoint = new Point(0, h / 2);
            var lastipoint = new Point(0, h / 2);
            for (var x = xstart; x < xstart + xwidth; x += dx)
            {
                var y = _cm.GetExpectedY(new[] { x }, 0);
                var vx = (int)XToVx(x, xstart, pw);
                var vy = (int)YToVy(y, ystart, ph);
                var point = new Point(vx, vy);
                g.DrawLine(pen, lastpoint, point);

                var idealY = Math.Sin(x);
                var videaly = (int)((idealY + 2.0) * ph);
                var ipoint = new Point(vx, videaly);
                g.DrawLine(penIdeal, lastipoint, ipoint);

                var sig = _cm.GetExpectedSquareY(new[] { x }, 0);
                var va = Math.Sqrt(sig - y * y) / 2;
                var vva = (float)(va * ph);
                g.DrawLine(pinkPen, vx, vy - vva, vx, vy + vva);

                lastpoint = point;
                lastipoint = ipoint;
            }
        }

        public ICoreManager CreateFixedSphericCores(int count)
        {
            var cm = new FixedCoreManager { OutputStartingFrom = 1 };
            for (; count > 0; count--)
            {
                var x = _r.NextDouble() * FuncWidth;
                var y = Math.Sin(x);
                var core = new Core();
                var cx = new Component { Center = x };
                var cy = new Component { Center = y };
                core.Components.Add(cx);
                core.Components.Add(cy);
                cm.Cores.Add(core);
            }
            cm.UpdateCoreCoeffs();
            return cm;
        }

        public ICoreManager CreateVariableSphericCores(int count)
        {
            var cm = new VariableCoreManager { OutputStartingFrom = 1 };
            for (; count > 0; count--)
            {
                var x = _r.NextDouble() * FuncWidth;
                var y = Math.Sin(x);
                var core = new Core();
                var cx = new Component { Center = x };
                var cy = new Component { Center = y };
                core.Components.Add(cx);
                core.Components.Add(cy);
                cm.Cores.Add(core);
            }
            cm.UpdateCoreCoeffs();
            return cm;
        }

        public ICoreManager CreateFixedConfinedCores(int count)
        {
            var cm = new FixedConfinedCoreManager();
            for (; count > 0; count--)
            {
                var x = _r.NextDouble() * FuncWidth;
                var y = Math.Sin(x);
                var core = new GaussianConfinedCore(1,1);
                core.CentersInput[0] = x;
                core.CentersOutput[0] = y;
                cm.Cores.Add(core);
            }
            cm.UpdateCoreCoeffs();
            return cm;
        }

        public ICoreManager CreateVariableConfinedCores(int count)
        {
            var cm = new VariableConfinedCoreManager();
            for (; count > 0; count--)
            {
                var x = _r.NextDouble() * FuncWidth;
                var y = Math.Sin(x);
                var core = new GaussianConfinedCore(1, 1);
                core.CentersInput[0] = x;
                core.CentersOutput[0] = y;
                cm.AddCore(core);
            }
            cm.UpdateCoreCoeffs();
            return cm;
        }

        private float XToVx(double x, double x0, double pw)
        {
            return (float)((x - x0) * pw);
        }

        private float YToVy(double y, double y0, double ph)
        {
            return (float)((y - y0) * ph);
        }

#endregion
    }
}
