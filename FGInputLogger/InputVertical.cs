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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FGInputLogger
{
    public partial class InputVertical : Form
    {

        List<List<object>> inputs = new List<List<object>>();
        List<List<int>> inputsTime = new List<List<int>>();
        
        List<int> old = new List<int>();
        List<int> delay = new List<int>();

        Dictionary<string, Image> Icons = new Dictionary<string, Image>();

        int inputSize = 30;
        int PlinkDelay = 1;
        string folder = "";
        int frame = 0;
        bool inDelay = false;
        bool SeparateDirections = false;
        bool ShowFrames = false;
        bool HasDelayInput = false;

       // public Timer timer = new Timer();
        public Dictionary<int, List<int>> ImageMap = new Dictionary<int, List<int>>();

        bool Vertical= true;

        Stopwatch watch;
        int contfps = 0;

   


        public InputVertical()
        {
            InitializeComponent();

            MinimumSize = new Size(1, 1);
                        
            var config =  new Map();

            config.ShowDialog();
            config.timer.Stop();
            if (!config.OK || Program.controller.Empty())
            {
                //timer.Stop();
                Environment.Exit(0);
            }
            this.inputSize = config.IconSize;
            this.folder = config.Theme;
            this.ImageMap = config.ImageMap;
            Vertical = config.Vertical;
            this.pictureBox1.BackColor = BackColor =  config.GetBackColor;
            SeparateDirections = config.SeparateDirections;
            ShowFrames = config.ShowFrames;

            if (Vertical)
                Size = new Size(200, int.MaxValue);
            else
                Size = new Size(int.MaxValue, 200);

            /*timer.Interval = 1000 / 60;
            timer.Tick += Timer_Tick;
            timer.Enabled = true;
            */

            watch = Stopwatch.StartNew();
           
        }

     

        public void Draw()
        {
          

            var buttons = SlimWrapper.GetInputs();
             var theInput = new List<object>();

            /*

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
            */
            


            if (!(old.All(x => buttons.buttons.Contains(x)) && buttons.buttons.All(x => old.Contains(x))) || buttons.buttons.Count > 0){


                var up = Any(buttons.buttons, Program.controller.Up);
                var down =  Any(buttons.buttons, Program.controller.Down );
                var left =  Any(buttons.buttons, Program.controller.Left ) ;
                var right = Any(buttons.buttons, Program.controller.Right) ;

                var vup = !  Any(old,Program.controller.Up);
                var vdown = !Any(old,Program.controller.Down);
                var vleft = ! Any(old,Program.controller.Left);
                var vright = !Any(old,Program.controller.Right);

                var lastIsDiagonal = false;

                if (inputs.Count>0)
                foreach (var i in inputs.First())
                    lastIsDiagonal = lastIsDiagonal || i.ToString().Contains("-");

                if ( (up && left)|| (up && right) || (down && left) || (down && right) )
                    lastIsDiagonal = false;

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
                        if (up && (vup || lastIsDiagonal) )
                            theInput.Add("up");

                        if (down && (vdown || lastIsDiagonal))
                            theInput.Add("down");

                        if (left && (vleft || lastIsDiagonal) )
                            theInput.Add("left");

                        if (right && (vright || lastIsDiagonal))
                            theInput.Add("right");

                    }


                if (up && left && !(vup || vleft))
                {
                    addTime("up-left");
                }
                else if (up && right && !(vup || vright))
                {
                    addTime("up-right");
                }
                else if (down && left && !(vdown || vleft))
                {
                    addTime("down-left");
                }
                else if (down && right && !(vdown || vright))
                {
                    addTime("down-right");
                }
                else
                {

                    if (up && !vup)
                        addTime("up");

                    if (down && !vdown)
                        addTime("down");

                    if (left && !vleft)
                        addTime("left");

                    if (right && !vright)
                        addTime("right");
                }



                if (Any(buttons.buttons, Program.controller.LP))
                    if (!Any(old, Program.controller.LP))
                        theInput.Add(1);
                    else
                        addTime(1);


                if (Any(buttons.buttons, Program.controller.MP))
                    if (!Any(old, Program.controller.MP))
                        theInput.Add(2);
                    else
                        addTime(2);

                if (Any(buttons.buttons, Program.controller.HP))
                    if (!Any(old, Program.controller.HP))
                        theInput.Add(3);
                    else
                        addTime(3);

                if (Any(buttons.buttons, Program.controller.PPP))
                    if (!Any(old, Program.controller.PPP))
                        theInput.Add(4);
                    else
                        addTime(4);

                if (Any(buttons.buttons, Program.controller.LK))
                    if (!Any(old, Program.controller.LK))
                        theInput.Add(5);
                    else
                        addTime(5);

                if (Any(buttons.buttons, Program.controller.MK))
                    if (!Any(old, Program.controller.MK))
                        theInput.Add(6);
                    else
                        addTime(6);

                if (Any(buttons.buttons, Program.controller.HK))
                    if (!Any(old, Program.controller.HK))
                        theInput.Add(7);
                    else
                        addTime(7);

                if (Any(buttons.buttons, Program.controller.KKK))
                    if (!Any(old, Program.controller.KKK))
                        theInput.Add(8);
                    else
                        addTime(8);


                if (theInput.Count > 0)
                    {
                        inputs.Insert(0, theInput);
                        inputsTime.Insert(0, Enumerable.Repeat(1, theInput.Count()).ToList());
                    }




                //if (buttons.buttons.Count > 0)
                 pictureBox1.Refresh();

                old = buttons.buttons;


                }
                else
                {
                    if (buttons.buttons.Count == 0)
                        old = buttons.buttons;

                }


         
            contfps++;

            watch.Stop();
            if (watch.ElapsedMilliseconds >= 1000)
            {

                Console.WriteLine(contfps);
                contfps = 0;
                watch.Restart();

            }
            else
                watch.Start();


            

            
        }

        private bool Any(IList<int> a, IList<int> b)
        {


            foreach (var item in a)
            {
                if (b.Contains(item))
                    return true;
            }


            return false;
        }

        private void addTime(object btn)
        {
            for (int i = 0; i < inputs.Count(); i++)
            {
                for (int j = 0; j < inputs[i].Count(); j++)
                {
                    if (inputs[i][j].Equals(btn))
                    {
                        inputsTime[i][j]++;
                        return;
                    }
                }
            }

        }

        private void InputVertical_Paint(object sender, PaintEventArgs e)
        {
            while (inputs.Count * inputSize >= (Vertical ? this.Height : this.Width) - inputSize * 2)  {
                inputs.Remove(inputs.Last());
                inputsTime.Remove(inputsTime.Last());
            }


            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var BlankColumn = (SeparateDirections ? inputSize : 0);

        

            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].Count() > 0)

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

                                Image img = null;

                                if (Icons.ContainsKey(file))
                                    img = Icons[file];
                                else
                                 if (File.Exists(file))
                                    {
                                        img = Image.FromFile(file);
                                        Icons.Add(file, img);
                                    }

                            if (img!=null)
                                {                                    
                                    using (ImageAttributes wrapMode = new ImageAttributes())
                                    {
                                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                        Rectangle rect;
                                        if (Vertical)
                                            rect = new Rectangle(BlankColumn + space + 5 + inputSize * j, i * inputSize + 5, inputSize, inputSize);
                           
                                        else
                                            rect = new Rectangle(i * inputSize + 5, space + 5 + inputSize * j + BlankColumn, inputSize, inputSize);


                                
                                        e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);
                                    }



                                space += inputSize;
                                }
                            }

                        }
                        else
                        {
                            var file = "themes/" + folder + "/" + inputs[i][j].ToString() + ".png";

                            Image img = null;

                        if (Icons.ContainsKey(file))
                            img = Icons[file];
                        else
                            if (File.Exists(file))
                            {
                                img = Image.FromFile(file);
                                Icons.Add(file, img);
                            }

                            if (img != null)
                            {
                                                               
                                using (ImageAttributes wrapMode = new ImageAttributes())
                                {
                                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                    Rectangle rect;
                                    if (Vertical)
                                        rect = new Rectangle( 5 + inputSize * j, i * inputSize + 5, inputSize, inputSize);
                                    else
                                        rect = new Rectangle( i * inputSize + 5,  5 + inputSize * j, inputSize, inputSize);

                             

                                 e.Graphics.DrawImage(img, rect, 0, 0, img.Height, img.Width, GraphicsUnit.Pixel, wrapMode);
                                }
                            }
                        }
                    
                    }
                if (ShowFrames)
                {
                    if (Vertical)
                        e.Graphics.DrawString(String.Join(",", inputsTime[i]), new Font(new FontFamily("arial"), inputSize / 3), Brushes.White, inputs[i].Count() * inputSize + inputSize / 2, i * inputSize + 4);
                    else
                        e.Graphics.DrawString(String.Join("\n", inputsTime[i]), new Font(new FontFamily("arial"), inputSize / 3), Brushes.White, i * inputSize + 4, inputs[i].Count() * inputSize + inputSize / 2);
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




