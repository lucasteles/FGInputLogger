using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FGInputLogger
{
    public partial class Map : Form
    {

        public Timer timer = new Timer();
        public bool getKeyState = false;
        public string selectedButton = "";

        public int IconSize = 30;
        public string Theme = "";
        public bool OK =  false;
        public Dictionary<int, List<int>> ImageMap = new Dictionary<int, List<int>>();

        private int myVar;



        public bool Vertical
        {
            get
            {
                return rdbVertical.Checked;
            }
        }

        public bool ShowFrames
        {
            get
            {
                return chkFrames.Checked;
            }
        }

        public bool SeparateDirections
        {
            get
            {
                return chkDirColumn.Checked;
            }
        }

        public Color GetBackColor
        {
            get
            {
                return lblColor.BackColor;
            }
        }

        public Map()
        {
            InitializeComponent();
        }

        private void Map_Load(object sender, EventArgs e)
        {
            var controls = SlimWrapper.Available();
            var names = controls.Select(c => c.Name).ToArray();

            if (controls.Count == 0)
            {
                MessageBox.Show("Connect a controller and restart the application");
                Environment.Exit(0);
            }

            for (int i = 1; i < 9; i++)
                ImageMap.Add(i, new List<int> { i });

            cmnButtons.SelectedIndex = 0;
            fillImageBox();
            refreshList();


            cmdDevices.DataSource = controls;
            cmdDevices.DisplayMember = "Name";
            cmdDevices.ValueMember = "Guid";


            timer.Tick += Timer_Tick;

            cmbTheme.DataSource = Directory.GetDirectories("themes").Select(x=>x.Split('\\')[1]).ToArray();
            cmbTheme.SelectedIndex = 0;

            foreach (Control item in this.Controls)
                if (item is Button)
                {
                     var but = (Button)item;
                     if(but.Tag != null && but.Tag.ToString()=="-")
                           item.Click += set_click;



                }

            
        }

        private void fillImageBox()
        {
            lstFiles.ItemChecked -= ItemChecked;
            var files = Directory.GetFiles("themes\\" + cmbTheme.SelectedValue);
            
            lstFiles.LargeImageList = new ImageList();
            lstFiles.Clear();

            lstFiles.LargeImageList.ImageSize = new Size(32, 32);
            lstFiles.LargeImageList.ColorDepth = ColorDepth.Depth24Bit;
            int i = 0;

            foreach (var item in files)
            {
                var filename = Path.GetFileNameWithoutExtension(item);
                int outN;
                if (int.TryParse(filename, out outN))
                {
                    var li = new ListViewItem("");
                    lstFiles.LargeImageList.Images.Add(Image.FromFile(item));
                    li.ImageIndex = i;
                    li.Tag = outN;
                    lstFiles.Items.Add(li);

                    
                    i++;
                }
            }


            lstFiles.ItemChecked += ItemChecked;
            refreshList();

        }
      

        private void refreshList()
        {

            
            var buttons = ImageMap[cmnButtons.SelectedIndex+1];

            foreach (ListViewItem item in lstFiles.Items)
            {
                if (buttons.Contains((int)item.Tag))
                    item.Checked = true;
                else
                    item.Checked = false;

                

            }

        }

        private void Timer_Tick(object sender, EventArgs e)
    {
            var buttons = SlimWrapper.GetInputs();

            if (getKeyState )
            {
                if (buttons.buttons.Count > 0)
                {
                    switch (selectedButton)
                    {
                        case "U":

                            Program.controller.Up = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "R":

                            Program.controller.Right = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "D":
                            Program.controller.Down = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "L":
                            Program.controller.Left = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "LP":
                            Program.controller.LP = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "MP":
                            Program.controller.MP = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "HP":
                            Program.controller.HP = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "PP":
                            Program.controller.PPP = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;
                        case "LK":
                            Program.controller.LK = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "MK":
                            Program.controller.MK = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "HK":
                            Program.controller.HK = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                        case "KK":
                            Program.controller.KKK = buttons.buttons;
                            lblPush.Visible = false;
                            getKeyState = false;
                            break;

                    }
                }
            }
            else
            {
                if (Program.controller.Up.Intersect(buttons.buttons).Any())
                    setButtonColor("U", true);
                else
                    setButtonColor("U", false);

                if (Program.controller.Down.Intersect(buttons.buttons).Any())
                    setButtonColor("D", true);
                else
                    setButtonColor("D", false);

                if (Program.controller.Left.Intersect(buttons.buttons).Any())
                    setButtonColor("L", true);
                else
                    setButtonColor("L", false);

                if (Program.controller.Right.Intersect(buttons.buttons).Any())
                    setButtonColor("R", true);
                else
                    setButtonColor("R", false);



                if (Program.controller.LP.Intersect(buttons.buttons).Any())
                    setButtonColor("LP", true);
                else
                    setButtonColor("LP", false);

                if (Program.controller.MP.Intersect(buttons.buttons).Any())
                    setButtonColor("MP", true);
                else
                    setButtonColor("MP", false);

                if (Program.controller.HP.Intersect(buttons.buttons).Any())
                    setButtonColor("HP", true);
                else
                    setButtonColor("HP", false);

                if (Program.controller.PPP.Intersect(buttons.buttons).Any())
                    setButtonColor("PP", true);
                else
                    setButtonColor("PP", false);


                if (Program.controller.LK.Intersect(buttons.buttons).Any())
                    setButtonColor("LK", true);
                else
                    setButtonColor("LK", false);

                if (Program.controller.MK.Intersect(buttons.buttons).Any())
                    setButtonColor("MK", true);
                else
                    setButtonColor("MK", false);

                if (Program.controller.HK.Intersect(buttons.buttons).Any())
                    setButtonColor("HK", true);
                else
                    setButtonColor("HK", false);

                if (Program.controller.KKK.Intersect(buttons.buttons).Any())
                    setButtonColor("KK", true);
                else
                    setButtonColor("KK", false);

            }

            txtInputs.Text = string.Join(",", buttons.buttons);

                        
    }

        private void set_click(object sender, EventArgs e)
        {
        
                selectedButton = ((Button)sender).Text;
                getKeyState = true;
            lblPush.Visible = true;
        }

        private void setButtonColor(string text, bool selected )
        {
            foreach (Control item in this.Controls)
            {
                if (!(item is Button))
                    continue;

                if (item.Text == text)
                {
                    if (selected)
                    {
                        item.BackColor = Color.Gray;
                    }
                    else
                    {
                        item.BackColor = Color.LightGray;
                    }

                    break;
                }
            }


        }

    

    private void btnSelect_Click(object sender, EventArgs e)
        {
            SlimWrapper.Acquire(this, (Guid)cmdDevices.SelectedValue);
           // Program.controller = new ControlMap();
            timer.Enabled = true;
            pictureBox1.Visible = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Visible)
            {
                MessageBox.Show("Select a joystick device!");
                cmdDevices.Focus();

                return;
            }


            OK = true;

            if (!int.TryParse(txtIconSize.Text, out IconSize))
                IconSize = 30;

            Theme = cmbTheme.SelectedValue.ToString();

            this.Close();

        }

        private void Map_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
        }

        private void cmbTheme_SelectedValueChanged(object sender, EventArgs e)
        {
            fillImageBox();
        }



        private void cmnButtons_SelectedValueChanged(object sender, EventArgs e)
        {
            refreshList();
        }


        private void ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var newButtons = new List<int>();

            foreach (ListViewItem item in lstFiles.Items)
            {
                if (item.Checked)
                {
                    newButtons.Add((int)item.Tag);
                }
            }

            newButtons.Sort();
            ImageMap[cmnButtons.SelectedIndex + 1] = newButtons;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var obj = new
            {
                Buttons = Program.controller,
                Images = ImageMap,
                IconSize = IconSize,
                Theme = cmbTheme.SelectedValue,
                Color = lblColor.BackColor,
                Vertical = Vertical,
                ShowFrames = ShowFrames,
                SeparateDirections = SeparateDirections
            };

            var filelocation = new SaveFileDialog();
            filelocation.FileName = "config.json";
            filelocation.Filter = "JSON|*.json";
            filelocation.Title = "Save the config file";
            filelocation.ShowDialog();

            if (filelocation.FileName != "")
            {
                using (var fs =  new StreamWriter((FileStream)filelocation.OpenFile()))
                {
                       var json = JsonConvert.SerializeObject(obj);
                    fs.Write(json);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "JSON|*.json";
            dialog.Title = "Load config file";
            dialog.ShowDialog();

            if (dialog.FileName != "")
            {
                using (var fs = new StreamReader((FileStream)dialog.OpenFile()))
                {
                    dynamic obj = JObject.Parse(fs.ReadToEnd());

                    Program.controller = obj.Buttons.ToObject<ControlMap>();
                    IconSize = (int)obj.IconSize;
                    cmbTheme.SelectedItem= obj.Theme.ToString();
                    ImageMap = obj.Images.ToObject<Dictionary<int, List<int>>>();
                    lblColor.BackColor = obj.Color.ToObject<Color>();
                    chkFrames.Checked = obj.ShowFrames.ToObject<bool>();
                    chkDirColumn.Checked = obj.SeparateDirections.ToObject<bool>();

                    rdbVertical.Checked = obj.Vertical.ToObject<bool>();
                    rdbHorizontal.Checked = !obj.Vertical.ToObject<bool>();

                }                
            }
        }

        private void btnPicker_Click(object sender, EventArgs e)
        {
            var colorDialog1 = new ColorDialog();
            DialogResult result =colorDialog1.ShowDialog();
            // See if user pressed ok.
            if (result == DialogResult.OK)
            {
                // Set form background to the selected color.
                lblColor.BackColor = colorDialog1.Color;
            }
        }

    }
}
