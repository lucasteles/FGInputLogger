using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FGInputLogger
{
    public partial class InputVertical : Form
    {

        List<List<object>> inputs = new List<List<object>>();
        List<int> old = new List<int>();
        List<int> delay = new List<int>();

        int inputSize = 30;
        int PlinkDelay = 1;
        string folder = "";
        int frame = 0;
        bool inDelay = false;
        bool HasDelayInput = false;

        public Timer timer = new Timer();
        public Dictionary<int, List<int>> ImageMap = new Dictionary<int, List<int>>();

        bool Vertical= true;

        public InputVertical()
        {
            InitializeComponent();


            //FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(1, 1);



           var config =  new Map();

            config.ShowDialog();
            config.timer.Stop();
            if (!config.OK || Program.controller.Empty())
            {
                timer.Stop();
                Environment.Exit(0);
            }
            this.inputSize = config.IconSize;
            this.folder = config.Theme;
            this.ImageMap = config.ImageMap;
            Vertical = config.Vertical;
            this.pictureBox1.BackColor = BackColor =  config.GetBackColor;


            if (Vertical)
                Size = new Size(200, int.MaxValue);
            else
                Size = new Size(int.MaxValue, 200);

            timer.Interval = 1000 / 60;
            timer.Tick += Timer_Tick;
            timer.Enabled = true;
            

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
             var buttons = SlimWrapper.GetInputs();
             var theInput = new List<object>();



            if (buttons.buttons.Count > 0)
            {
                if (!inDelay)
                {
                    inDelay = true;
                    delay = new List<int>();
                    delay.AddRange(buttons.buttons);

                    frame = 0;
                }
                else
                {

                    foreach (var b in buttons.buttons)
                    {
                        if (!delay.Contains(b))
                        {
                            HasDelayInput = true;
                        }
                    }

                    if (HasDelayInput)
                    {

                        foreach (var b in delay)
                        {
                            if (!buttons.buttons.Contains(b))
                            {
                                buttons.buttons.Add(b);
                            }
                        }
                        buttons.buttons.Sort();
                        old = new List<int>();
                       inDelay = false;
                        delay = new List<int>();
                        frame = -1;
                        HasDelayInput = false;
                    }

                    frame++;
                }

                

            }
            else if (inDelay)
            {

                frame++;               

            }


            if (frame >= PlinkDelay)
            {
                inDelay = false;
                delay = new List<int>();
                frame = 0;
                HasDelayInput = false;
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
                        Refresh();


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
            while (inputs.Count * inputSize >= (Vertical ? this.Height : this.Width) - inputSize * 2)  {
                inputs.Remove(inputs.Last());
            }


            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].Count > 0)

                    inputs[i] = inputs[i].Distinct().ToList();
                    for (int j = 0; j < inputs[i].Count; j++)
                    {

                        int imageMapId = 0;
                        if (int.TryParse(inputs[i][j].ToString(), out imageMapId))
                        {
                            int space = 0;

                                                       
                            foreach (var ix in ImageMap[imageMapId])
                            {
                                                             
                                var file = "themes/" + folder + "/" + ix.ToString() + ".png";

                                if (File.Exists(file))
                                {
                                    var img = Image.FromFile(file);
                                    

                                    using (ImageAttributes wrapMode = new ImageAttributes())
                                    {
                                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                        Rectangle rect;
                                        if (Vertical)
                                            rect = new Rectangle(space + 5 + inputSize * j, i * inputSize + 5, inputSize, inputSize);
                           
                                        else
                                            rect = new Rectangle(i * inputSize + 5, space + 5 + inputSize * j, inputSize, inputSize);

                                        
                                        e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);
                                    }

                                    

                                    space += inputSize;
                                }
                            }

                        }
                        else
                        {
                            var file = "themes/" + folder + "/" + inputs[i][j].ToString() + ".png";

                            if (File.Exists(file))
                            {
                                var img = Image.FromFile(file);
                                
                                using (ImageAttributes wrapMode = new ImageAttributes())
                                {
                                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                    Rectangle rect;
                                    if (Vertical)
                                        rect = new Rectangle(5 + inputSize * j, i * inputSize + 5, inputSize, inputSize);
                                    else
                                        rect = new Rectangle( i * inputSize + 5, 5 + inputSize * j, inputSize, inputSize);


                                    
                                    e.Graphics.DrawImage(img, rect, 0, 0, img.Height, img.Width, GraphicsUnit.Pixel, wrapMode);
                                }
                            }
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




