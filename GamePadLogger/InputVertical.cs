using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GamePadLogger
{
    public partial class InputVertical : Form
    {

        List<List<object>> inputs = new List<List<object>>();
        List<int> old = new List<int>();
        List<int> delay = new List<int>();

        int inputSize = 30;
        int delayInput = 3;
        string folder = "";
        int frame = 0;


        public Timer timer = new Timer();

        public InputVertical()
        {
            InitializeComponent();


           
            //ControlBox = false;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(1, 1);
            Size = new Size(200, 600);


           var config =  new Map();

            config.ShowDialog();
            config.timer.Stop();
            if (!config.OK)
            {
                timer.Stop();
                Environment.Exit(0);
            }
            this.inputSize = config.IconSize;
            this.folder = config.Theme;

            timer.Interval = 1000 / 60;
            timer.Tick += Timer_Tick;
            timer.Enabled = true;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
             var buttons = SlimWrapper.GetInputs();
            var theInput = new List<object>();


            if (frame >= delayInput)
            {
                frame = 0;
                if ( delay.Count > 0)
                    buttons.buttons.AddRange(delay);
                delay = new List<int>();
            }
            else
            {
                if (buttons.buttons.Count>0)
                     delay.AddRange(buttons.buttons);
                else
                    delay = new List<int>();
                frame++;
            }

                if (!(old.All(x => buttons.buttons.Contains(x)) && buttons.buttons.All(x => old.Contains(x))) && buttons.buttons.Count > 0)
                {
                    var up = Program.controller.Up.Intersect(buttons.buttons).Any() ;
                    var down = Program.controller.Down.Intersect(buttons.buttons).Any();
                    var left = Program.controller.Left.Intersect(buttons.buttons).Any() ;
                    var right = Program.controller.Right.Intersect(buttons.buttons).Any() ;

                var vup = !old.Intersect(Program.controller.Up).Any();
                var vdown = !old.Intersect(Program.controller.Down).Any();
                var vleft = !old.Intersect(Program.controller.Left).Any();
                var vright = !old.Intersect(Program.controller.Right).Any();


                if (up && left && (vup||vleft) )
                    {
                        theInput.Add("up-left");
                    }
                    else if (up && right && (vup || vright))
                    {
                        theInput.Add("up-right");
                    }
                    else if (down && left && (vdown || vleft))
                    {
                        theInput.Add("down-left");
                    }
                    else if (down && right && (vdown || vright))
                    {
                        theInput.Add("down-right");
                    }
                    else
                    {
                        if (up )
                            theInput.Add("up");

                        if (down )
                            theInput.Add("down");

                        if (left )
                            theInput.Add("left");

                        if (right )
                            theInput.Add("right");

                    }

                    if (Program.controller.LP.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.LP).Any())
                        theInput.Add(1);
                    if (Program.controller.MP.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.MP).Any())
                        theInput.Add(2);
                    if (Program.controller.HP.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.HP).Any())
                        theInput.Add(3);
                    if (Program.controller.PPP.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.PPP).Any())
                        theInput.Add(4);
                    if (Program.controller.LK.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.LK).Any())
                        theInput.Add(5);
                    if (Program.controller.MK.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.MK).Any())
                        theInput.Add(6);
                    if (Program.controller.HK.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.HK).Any())
                        theInput.Add(7);
                    if (Program.controller.KKK.Intersect(buttons.buttons).Any() && !old.Intersect(Program.controller.KKK).Any())
                        theInput.Add(8);


                    if (theInput.Count > 0)
                        inputs.Insert(0, theInput);

                    if (buttons.buttons.Count > 0)
                    {
                       
                        Refresh();
                    }

                    old = buttons.buttons;


                }
                else
                {
                    if (buttons.buttons.Count == 0)
                        old = buttons.buttons;

                }
            
           
            
        }

        private void InputVertical_Paint(object sender, PaintEventArgs e)
        {
            while (inputs.Count * inputSize >= this.Height - inputSize * 2)  {
                inputs.Remove(inputs.Last());
            }


            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].Count > 0)
                    for (int j = 0; j < inputs[i].Count; j++)
                    {
                        var file = "themes/" + folder + "/" + inputs[i][j].ToString() + ".png";

                        if (File.Exists(file))
                        {
                            var img = Image.FromFile(file);
                            img = ResizeImage(img, inputSize, inputSize);
                            e.Graphics.DrawImage(img, 5 + inputSize * j, i * inputSize + 5);
                        
                        }
                    
                    }
               
            }
           

        }



        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            //a holder for the result 
            Bitmap result = new Bitmap(width, height);

            //use a graphics object to draw the resized image into the bitmap 
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality 
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap 
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap 
            return result;
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            InputVertical_Paint(sender, e);
        }
    }
}






/*
        private void InputVertical_KeyDown(object sender, KeyEventArgs e)
        {
            if (inputs.Count * inputSize >= this.Height- inputSize*2)
            {
                
                inputs.Remove(inputs.First());

            }

            switch (e.KeyCode)
            {   
               case Keys.Left:
                    inputs.Add("left");
                    break;
                case Keys.Up:
                    inputs.Add("up");
                    break;
                case Keys.Right:
                    inputs.Add("right");
                    break;
                case Keys.Down:
                    inputs.Add("down");
                    break;

                case Keys.Q:
                    inputs.Add(1);
                    break;

                case Keys.W:
                    inputs.Add(2);
                    break;

                case Keys.E:
                    inputs.Add(3);
                    break;

                case Keys.R:
                    inputs.Add(4);
                    break;

                case Keys.A:
                    inputs.Add(5);
                    break;


                case Keys.S:
                    inputs.Add(6);
                    break;


                case Keys.D:
                    inputs.Add(7);
                    break;


                case Keys.F:
                    inputs.Add(8);
                    break;
            }

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Refresh();
        }
        */
