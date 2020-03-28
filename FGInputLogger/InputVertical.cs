using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace FGInputLogger
{
    public partial class InputVertical : Form
    {

        List<List<object>> inputs = new List<List<object>>();
        List<List<int>> inputsTime = new List<List<int>>();

        List<int> old = new List<int>();
        List<int> delay = new List<int>();
        List<int> delayBuff = new List<int>();

        Dictionary<object, int> pressed = new Dictionary<object, int>();

        Dictionary<string, Image> Icons = new Dictionary<string, Image>();
        Dictionary<string, SoundPlayer> Sounds = new Dictionary<string, SoundPlayer>();

        private object _lock = new object();

        int inputSize = 30;
        int PlinkDelay = 1;
        string folder = "";
        int frame = 0;
        bool inDelay = false;
        bool SeparateDirections = false;
        bool ShowFrames = false;
        bool HasDelayInput = false;
        Thread thread;
        public long milliseconds;
        public long milliseconds2;
        public Dictionary<int, List<int>> ImageMap = new Dictionary<int, List<int>>();
        bool stopThread = false;
        bool Vertical = true;

        public bool HorizontalLeftToRight { get; private set; }

        bool Sound = false;

        int contfps = 0;




        public InputVertical()
        {
            InitializeComponent();

            MinimumSize = new Size(1, 1);

            var config = new Map();

            config.ShowDialog();
            config.timer.Stop();
            if (!config.OK || Program.controller.Empty())
            {
                Environment.Exit(0);
            }
            this.inputSize = config.IconSize;
            this.folder = config.Theme;
            this.ImageMap = config.ImageMap;
            Vertical = config.Vertical;
            HorizontalLeftToRight = config.HorizontalLeftToRight;
            this.pictureBox1.BackColor = BackColor = config.GetBackColor;
            SeparateDirections = config.SeparateDirections;
            ShowFrames = config.ShowFrames;
            Sound = config.PlaySounds;

            if (Vertical)
                Size = new Size(250, Screen.PrimaryScreen.Bounds.Height);
            else
                Size = new Size(Screen.PrimaryScreen.Bounds.Width, 250);

            var soundPath = "themes/" + folder + "/sounds";

            if (Directory.Exists(soundPath))
            {
                var soundFiles = Directory.GetFiles(soundPath, "*.wav");
                foreach (var item in soundFiles)
                    Sounds.Add(Path.GetFileNameWithoutExtension(item), new SoundPlayer(new MemoryStream(File.ReadAllBytes(item))));
            }




            milliseconds = milliseconds2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            thread = new Thread(new ThreadStart(MainLoop));
            thread.Start();
        }


        private void MainLoop()
        {
            while (!stopThread)
            {



                try
                {
                    Draw();
                }
                catch
                { }


                var diff = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - milliseconds2;

                if (diff >= (1000))
                {
                    Console.WriteLine(contfps);
                    contfps = 0;
                    milliseconds2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }

                Thread.Sleep(1000 / 60);
            }
        }

        public void Draw()
        {

            lock (_lock)
            {
                var buttons = SlimWrapper.GetInputs();
                var theInput = new List<object>();

                if (Any(buttons.buttons, Program.controller.Clear))
                {
                    inputs = new List<List<object>>();
                    inputsTime = new List<List<int>>();
                    old = new List<int>();

                }

                #region "plink"
                //-------------------------------------------------------------


                if (buttons.buttons.Count() > 0 && buttons.buttons.Count() != old.Count())
                {
                    if (!inDelay)
                    {
                        inDelay = true;
                        delay = new List<int>();
                        delay.AddRange(buttons.buttons);
                        delayBuff = old;
                        frame = 0;
                    }
                    else
                    {

                        foreach (var b in buttons.buttons)
                        {
                            if (!delay.Contains(b) && !old.Contains(b))
                            {
                                HasDelayInput = true;
                                break;
                            }
                        }


                        if (HasDelayInput)
                        {

                            foreach (var b in delay)
                                if (!buttons.buttons.Contains(b))
                                    buttons.buttons.Add(b);



                            inDelay = false;
                            buttons.buttons.Sort();
                            old = new List<int>();

                            delay = new List<int>();
                            frame = -1;


                            /*for (int i = 0; i < delayBuff.Count(); i++)
                            {
                                buttons.buttons.Remove(delayBuff[i]);
                                old.Add(delayBuff[i]);
                            }*/
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

                //-------------------------------------------------------------
                #endregion

                if (!(old.All(x => buttons.buttons.Contains(x)) && buttons.buttons.All(x => old.Contains(x))) || buttons.buttons.Count > 0)
                {


                    var up = Any(buttons.buttons, Program.controller.Up);
                    var down = Any(buttons.buttons, Program.controller.Down);
                    var left = Any(buttons.buttons, Program.controller.Left);
                    var right = Any(buttons.buttons, Program.controller.Right);

                    var vup = !Any(old, Program.controller.Up) || ValidButtonChange(buttons.buttons, old);
                    var vdown = !Any(old, Program.controller.Down) || ValidButtonChange(buttons.buttons, old);
                    var vleft = !Any(old, Program.controller.Left) || ValidButtonChange(buttons.buttons, old);
                    var vright = !Any(old, Program.controller.Right) || ValidButtonChange(buttons.buttons, old);


                    var lastIsDiagonal = false;

                    if (inputs.Count > 0)
                        foreach (var i in inputs.First().ToList())
                            lastIsDiagonal = lastIsDiagonal || i.ToString().Contains("-");

                    if ((up && left) || (up && right) || (down && left) || (down && right))
                        lastIsDiagonal = false;

                    void Add(object obj, List<int> lst)
                    {
                        theInput.Add(obj);

                        if (Sound && Sounds.ContainsKey(obj.ToString()))
                            Player.Play(Sounds[obj.ToString()]);

                        if (HasDelayInput && pressed.ContainsKey(obj) && pressed[obj] > 0 && lst != null && Any(delayBuff, lst))
                        {
                            theInput.Remove(obj);
                        }


                        int _;
                        if (pressed.ContainsKey(obj))
                            if (int.TryParse(obj.ToString(), out _))
                            {
                                pressed.Remove(obj);
                            }
                            else
                            {
                                if (!ValidButtonChange(buttons.buttons, old) && pressed[obj] > 0)
                                    pressed.Remove(obj);
                            }

                    };

                    if (up && left && (vup || vleft))
                    {
                        Add("up-left", null);

                    }
                    else if (up && right && (vup || vright))
                    {
                        Add("up-right", null);
                    }
                    else if (down && left && (vdown || vleft))
                    {
                        Add("down-left", null);
                    }
                    else if (down && right && (vdown || vright))
                    {
                        Add("down-right", null);
                    }
                    else
                    {
                        if (up && (vup || lastIsDiagonal))
                            Add("up", null);

                        if (down && (vdown || lastIsDiagonal))
                            Add("down", null);

                        if (left && (vleft || lastIsDiagonal))
                            Add("left", null);

                        if (right && (vright || lastIsDiagonal))
                            Add("right", null);

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
                            Add(1, Program.controller.LP);
                        else
                            addTime(1);


                    if (Any(buttons.buttons, Program.controller.MP))
                        if (!Any(old, Program.controller.MP))
                            Add(2, Program.controller.MP);
                        else
                            addTime(2);

                    if (Any(buttons.buttons, Program.controller.HP))
                        if (!Any(old, Program.controller.HP))
                            Add(3, Program.controller.HP);
                        else
                            addTime(3);

                    if (Any(buttons.buttons, Program.controller.PPP))
                        if (!Any(old, Program.controller.PPP))
                            Add(4, Program.controller.PPP);
                        else
                            addTime(4);

                    if (Any(buttons.buttons, Program.controller.LK))
                        if (!Any(old, Program.controller.LK))
                            Add(5, Program.controller.LK);
                        else
                            addTime(5);

                    if (Any(buttons.buttons, Program.controller.MK))
                        if (!Any(old, Program.controller.MK))
                            Add(6, Program.controller.MK);
                        else
                            addTime(6);

                    if (Any(buttons.buttons, Program.controller.HK))
                        if (!Any(old, Program.controller.HK))
                            Add(7, Program.controller.HK);
                        else
                            addTime(7);

                    if (Any(buttons.buttons, Program.controller.KKK))
                        if (!Any(old, Program.controller.KKK))
                            Add(8, Program.controller.KKK);
                        else
                            addTime(8);




                    if (theInput.Count > 0)
                    {
                        inputs.Insert(0, theInput);

                        var time = new List<int>();
                        for (int i = 0; i < theInput.Count(); i++)
                        {
                            if (pressed.ContainsKey(theInput[i]) && pressed[theInput[i]] > 0)
                            {
                                time.Add(-1);
                            }
                            else
                                time.Add(1);

                        }
                        inputsTime.Insert(0, time);
                    }



                    pictureBox1.BeginInvoke((Action)(() =>
                   {
                       pictureBox1.Refresh();
                   }));

                    old = buttons.buttons;



                }
                else
                {
                    if (buttons.buttons.Count == 0)
                    {
                        old = buttons.buttons;

                    }


                }

                if (HasDelayInput)
                    HasDelayInput = false;
            }

        }

        private bool ValidButtonChange(IList<int> atual, IList<int> old)
        {
            var ret = false;


            foreach (var item in atual)
            {

                if (Program.controller.Up.Contains(item) ||
                Program.controller.Down.Contains(item) ||
                Program.controller.Left.Contains(item) ||
                Program.controller.Right.Contains(item))
                    break;

                if (!old.Contains(item))
                    ret = true;
            }

            return ret;
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
                    if (inputs[i][j].Equals(btn) && inputsTime[i][j] >= 0)
                    {
                        inputsTime[i][j]++;
                        if (!pressed.ContainsKey(btn))
                            pressed.Add(btn, 0);
                        else
                            pressed[btn]++;


                        return;
                    }
                }
            }

        }

        private void InputVertical_Paint(object sender, PaintEventArgs e)
        {
            var padding = (inputSize * 2);

            lock (_lock)
            {

                while (inputs.Count * inputSize >= (Vertical ? this.Height : this.Width))
                {
                    inputs.Remove(inputs.Last());
                    inputsTime.Remove(inputsTime.Last());
                }


                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;




                for (int i = 0; i < inputs.Count; i++)
                {
                    if (inputs[i].Count() > 0)
                        inputs[i] = inputs[i].Distinct().ToList();

                    int adjust = 0;
                    for (int j = 0; j < inputs[i].Count; j++)
                    {

                        var BlankColumn = (SeparateDirections && HasButtonsOnly(inputs[i]) ? inputSize : 0);


                        int imageMapId = 0;
                        if (int.TryParse(inputs[i][j].ToString(), out imageMapId))
                        {
                            int space = 0;
                            foreach (var ix in ImageMap[imageMapId].ToList())
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




                                if (img != null)
                                {
                                    using (ImageAttributes wrapMode = new ImageAttributes())
                                    {
                                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                        Rectangle rect;
                                        if (Vertical)
                                        {
                                            rect = new Rectangle(adjust + BlankColumn + space + 5 + (inputSize * j), i * inputSize + 5, inputSize, inputSize);
                                        }
                                        else
                                        {
                                            var x = adjust + i * inputSize + 5;
                                            var y = space + 5 + inputSize * j + BlankColumn;

                                            if (HorizontalLeftToRight)
                                                rect = new Rectangle(x, y, inputSize, inputSize);
                                            else
                                                rect = new Rectangle(Width - padding - x, y, inputSize, inputSize);
                                        }

                                        e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);

                                    }
                                    space += inputSize;
                                }
                            }

                            adjust += (ImageMap[imageMapId].Count() - 1) * inputSize;

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
                                        rect = new Rectangle(5 + inputSize * j, i * inputSize + 5, inputSize, inputSize);
                                    else
                                    {
                                        var x = (i * inputSize) + 5;
                                        var y = 5 + inputSize * j;
                                        if (HorizontalLeftToRight)
                                            rect = new Rectangle(x, y, inputSize, inputSize);
                                        else
                                            rect = new Rectangle(Width - padding - x, y, inputSize, inputSize);


                                    }

                                    e.Graphics.DrawImage(img, rect, 0, 0, img.Height, img.Width, GraphicsUnit.Pixel, wrapMode);
                                }
                            }
                        }

                    }
                    if (ShowFrames)
                    {

                        if (Vertical)
                        {
                            var fontSize = inputSize / 2;
                            var font = new Font(new FontFamily("Consolas"), fontSize);

                            e.Graphics
                                .DrawString(String.Join(",", inputsTime[i]).Replace("-1", "*"),
                                font, Brushes.White,
                                CountIcons(inputs[i]) * inputSize + inputSize / 2,
                                i * inputSize + 4);
                        }
                        else
                        {
                            var text = String.Join("\n", inputsTime[i]).Replace("-1", "*");
                            var maxText = inputsTime[i].Select(t => t.ToString()).OrderByDescending(t => t.Length).First().Length;
                            var fontSize = inputSize / Math.Max(maxText, 2);
                            var font = new Font(new FontFamily("Consolas"), fontSize);
                            var x = (i * inputSize) + 4;
                            var y = (CountIcons(inputs[i]) * inputSize) + inputSize / 2;

                            if (HorizontalLeftToRight)
                                e.Graphics.DrawString(text, font, Brushes.White, x, y);
                            else

                                e.Graphics.DrawString(text, font, Brushes.White, Width - padding - x, y);
                        }
                    }
                }
            }

        }
        private int CountIcons(List<object> buttons)
        {
            int ret = 0;

            foreach (var item in buttons)
            {
                int imageMapId = 0;
                if (int.TryParse(item.ToString(), out imageMapId))
                {
                    ret += ImageMap[imageMapId].Count();
                }
                else
                    ret++;

            }

            return ret;
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

        private void InputVertical_FormClosed(object sender, FormClosedEventArgs e)
        {
            stopThread = true;

        }

        private bool HasButtonsOnly(IList<Object> list)
        {

            var ret = true;
            int _;
            for (int i = 0; i < list.Count(); i++)
                if (!int.TryParse(list[i].ToString(), out _))
                {
                    ret = false;
                    break;
                }

            return ret;

        }
    }
}




