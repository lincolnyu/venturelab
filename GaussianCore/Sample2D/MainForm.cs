using GaussianCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sample2D
{
    public partial class MainForm : Form
    {
        private ICoreManager _cm;

        private readonly Random _r = new Random(123);

        private const double FuncWidth = Math.PI*6;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //_cm = CreateGridCore(100);
            //_cm = CreateDistanceCore(100);
            _cm = CreateGaussianConfinedCores(100);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintPdf(g);
            PaintCores(g);
            PaintCurves(g);
        }

        private void PaintCores(Graphics g)
        {
            var pen = new Pen(Color.Pink, 2);
            var w = ClientRectangle.Width;
            var h = ClientRectangle.Height;
            var pw = w / FuncWidth;
            var ph = h / 4;

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
                    var vx = XToVx(x, 0, pw);
                    var vy = YToVy(y, -2.0, ph);
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
                    var vx = XToVx(x, 0, pw);
                    var vy = YToVy(y, -2.0, ph);
                    var vdx = (float)(dx * pw);
                    var vdy = (float)(dy * ph);
                    g.DrawEllipse(pen, vx - vdx, vy - vdy, 2 * vdx, 2 * vdy);
                }
            }
        }

        private void PaintPdf(Graphics g)
        {
            var w = ClientRectangle.Width;
            var h = ClientRectangle.Height;
            var pw = (float)(w / FuncWidth);
            var ph = h / 4;
            const double cutoffa = 100;
            for (var y = -2.0; y < 2; y += 0.05)
            {
                for (var x = -0.0; x < FuncWidth; x += 0.05)
                {
                    var a = _cm.GetIntensity(new[] {x}, new[] {y});
                    if (a > cutoffa) a = cutoffa;
                    var p = (int)(a * 255/ cutoffa);
                    var clr = Color.FromArgb(p, p, p);
                    var brush = new SolidBrush(clr);

                    var vx = XToVx(x, 0, pw);
                    var vy = YToVy(y, -2.0, ph);
                    g.FillRectangle(brush, vx, vy, pw, ph);
                }
            }
        }

        private void PaintCurves(Graphics g)
        {
            var w = ClientRectangle.Width;
            var h = ClientRectangle.Height;
            var pw = w / FuncWidth;
            var ph = h / 4;

            var penIdeal = new Pen(new SolidBrush(Color.Green));
            var pen = new Pen(new SolidBrush(Color.Red));
            var pinkPen = new Pen(new SolidBrush(Color.Orange));
            var lastpoint = new Point(0, h / 2);
            var lastipoint = new Point(0, h / 2);
            for (var x = 0.0; x < FuncWidth; x += 0.1)
            {
                var y = _cm.GetExpectedY(new[] { x }, 0);
                var vx = (int)XToVx(x, 0, pw);
                var vy = (int)YToVy(y, -2.0, ph);
                var point = new Point(vx, vy);
                g.DrawLine(pen, lastpoint, point);

                var idealY = Math.Sin(x);
                var videaly = (int)((idealY+2.0) * ph);
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

        public ICoreManager CreateGridCore(int count)
        {
            var cm = new GridCoreManager {OutputStartingFrom = 1};
            for (;count > 0; count--)
            {
                var x = _r.NextDouble() * FuncWidth;
                var y = Math.Sin(x);
                var core = new Core();
                var cx = new Component {Center = x};
                var cy = new Component {Center = y};
                core.Components.Add(cx);
                core.Components.Add(cy);
                cm.Cores.Add(core);
            }
            cm.UpdateCoreCoeffs();
            return cm;
        }

        public ICoreManager CreateDistanceCore(int count)
        {
            var cm = new DistanceCoreManager { OutputStartingFrom = 1 };
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

        public ICoreManager CreateGaussianConfinedCores(int count)
        {
            var cm = new ConfinedCoreManager();
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

        private float XToVx(double x, double x0, double pw)
        {
            return (float)((x - x0) * pw);
        }

        private float YToVy(double y, double y0, double ph)
        {
            return (float)((y - y0) * ph);
        }
    }
}
