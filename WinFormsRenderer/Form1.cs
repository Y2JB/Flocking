using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading;
using System.Numerics;

using Flocking;


namespace WinFormRender
{
    public partial class Form1 : Form
    {
        Stopwatch timer = new Stopwatch();
        long elapsedMs;
        //int seconds;
        long elapsedFpsMs;
        int framesDrawn;
        int fps;

        Rectangle gameRect;
        Rectangle fpsRect;
        Point fpsPt;

        Bitmap frame;

        //MenuStrip menu;


        BufferedGraphicsContext gfxBufferedContext;
        BufferedGraphics gfxBuffer;

        FlockingSimulation simulation; 

        public Form1()
        {
            InitializeComponent();           
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            int canvasWidth = 1280;
            int canvasHeight = 960;

            frame = new Bitmap(canvasWidth, canvasHeight);
            
            // Set the window client area to the size of the canvas
            ClientSize = new Size(canvasWidth, canvasHeight);

            DoubleBuffered = true;

            timer.Start();

            Application.Idle += new EventHandler(OnApplicationIdle);

            simulation = new FlockingSimulation(canvasWidth, canvasHeight, 100);
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Gets a reference to the current BufferedGraphicsContext
            gfxBufferedContext = BufferedGraphicsManager.Current;

            // Creates a BufferedGraphics instance associated with this form, and with dimensions the same size as the drawing surface of Form1.
            gfxBuffer = gfxBufferedContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);
        }


        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            fpsRect = new Rectangle(ClientRectangle.Width - 75, 5, 75, 30);
            fpsPt = new Point(ClientRectangle.Width - 75, 10);

            gameRect = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height);

            /*
            if (menu != null)
            {
                fpsPt.Y = 10 + menu.Height;
                fpsRect.Y = 5 + menu.Height;
                gameRect.Y = menu.Height;
                gameRect.Height -= menu.Height;
            }
            */

            if (gfxBufferedContext != null)
            {
                gfxBuffer = gfxBufferedContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);
            }
        }


        private void OnKeyDown(Object o, KeyEventArgs a)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, KeyEventArgs>(OnKeyDown), o, a);
                return;
            }
            //if (a.KeyCode == Keys.Up) gba.Joypad.UpdateKeyState(Joypad.GbaKey.Up, true);
            //else if (a.KeyCode == Keys.Down) gba.Joypad.UpdateKeyState(Joypad.GbaKey.Down, true);

        }


        private void OnKeyUp(Object o, KeyEventArgs a)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, KeyEventArgs>(OnKeyUp), o, a);
                return;
            }

            //if (a.KeyCode == Keys.Up) gba.Joypad.UpdateKeyState(Joypad.GbaKey.Up, false);
            //else if (a.KeyCode == Keys.Down) gba.Joypad.UpdateKeyState(Joypad.GbaKey.Down, false);
        }


        private void OnApplicationIdle(object sender, EventArgs e)
        {
            while (IsApplicationIdle())
            {
                if (timer.ElapsedMilliseconds - elapsedFpsMs >= 1000)
                {
                    elapsedFpsMs = timer.ElapsedMilliseconds;
                    fps = framesDrawn;
                    framesDrawn = 0;
                }

                // 60hz
                if (timer.ElapsedMilliseconds - elapsedMs > 16.66666)
                {                   
                    elapsedMs = timer.ElapsedMilliseconds;

                    simulation.Update();
                    Draw();
                }
                
            }
        }


        public static float ConvertRadiansToDegrees(double radians)
        {
            float degrees = (float) ((180 / Math.PI) * radians);
            return (degrees);
        }

        private void drawBoid(Bird boid)
        {
            var state = gfxBuffer.Graphics.Save();

            Vector2 direction = new Vector2(boid.DirectionX, boid.DirectionY);
            direction = Vector2.Normalize(direction);
            
            float angle = ConvertRadiansToDegrees((float) Math.Atan2(direction.Y, direction.X));           

            gfxBuffer.Graphics.TranslateTransform(boid.PositionX, boid.PositionY);
            gfxBuffer.Graphics.RotateTransform(angle);
            gfxBuffer.Graphics.TranslateTransform(-boid.PositionX, -boid.PositionY);

            Pen pen = new Pen(new SolidBrush(Color.White));
            gfxBuffer.Graphics.DrawLine(pen, boid.PositionX,        boid.PositionY,     boid.PositionX - 15, boid.PositionY + 5);
            gfxBuffer.Graphics.DrawLine(pen, boid.PositionX - 15,   boid.PositionY + 5, boid.PositionX - 15, boid.PositionY - 5);
            gfxBuffer.Graphics.DrawLine(pen, boid.PositionX - 15,   boid.PositionY - 5, boid.PositionX, boid.PositionY);


            gfxBuffer.Graphics.Restore(state);

            /*
            ctx.fillStyle = "#558cf4";
            ctx.beginPath();
            ctx.moveTo(boid.x, boid.y);
            ctx.lineTo(boid.x - 15, boid.y + 5);
            ctx.lineTo(boid.x - 15, boid.y - 5);
            ctx.lineTo(boid.x, boid.y);
            ctx.fill();
            ctx.setTransform(1, 0, 0, 1, 0, 0);*/
        }

        
        private void DrawBirds()
        {
            foreach(Bird bird in simulation.Birds)
            {
                //gfxBuffer.Graphics.FillRectangle(new SolidBrush(Color.Green), new Rectangle((int) bird.PositionX, (int) bird.PositionY, 10, 10));
                drawBoid(bird);
            }
        }


        private void Draw()
        {
            // Clear the draw buffer
            gfxBuffer.Graphics.Clear(Color.Black);


            gfxBuffer.Graphics.FillRectangle(new SolidBrush(Color.BlueViolet), new Rectangle(0, 0, frame.Width, frame.Height));

            DrawBirds();

            framesDrawn++;




            // Don't scale or fps plummets 
            //gfxBuffer.Graphics.DrawImage(frame, new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height));
            gfxBuffer.Graphics.DrawImage(frame, new Rectangle(0, 0, frame.Width, frame.Height));


            gfxBuffer.Graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(ClientRectangle.Width -75, 5, 70, 30));
            gfxBuffer.Graphics.DrawString(String.Format("{0:D2} fps", fps), new Font("Verdana", 8),  new SolidBrush(Color.Black), new Point(ClientRectangle.Width - 75, 10));

            gfxBuffer.Render();  
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);
    }




}
