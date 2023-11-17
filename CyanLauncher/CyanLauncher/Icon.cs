using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyanLauncher
{
    public class Icon : Panel
    {
        public string exePath = "";
        public string name = "";
        public string imgPath = "";
        public string as_admin = "false";
        public bool as_admin_bool = false;
        public bool notFound = false;
        public PictureBox picture;
        public Bitmap pic;
        Bitmap pic_big;
        //ToolTip tooltip = new ToolTip();
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem editIcon;
        private ToolStripMenuItem removeIcon;
        private ToolStripMenuItem run_otherWay;

        Point prevPos;
        public Icon(string exePath, string name, string imgPath, string as_admin)
        {
            this.exePath = exePath;
            this.name = name;
            this.imgPath = imgPath;
            this.as_admin = as_admin;
            if (as_admin == "true") this.as_admin_bool = true;
            if (!File.Exists(exePath) && !Directory.Exists(exePath)) notFound = true;
            RefreshImg();
            BackgroundImageLayout = ImageLayout.Center;
            picture = new PictureBox()
            {
                Image = System.Drawing.Bitmap.FromFile(imgPath),
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage,
            };
            //tooltip.Active = true;
            //tooltip.SetToolTip(this, name);
            SizeChanged += (o, e) => { picture.Size = Size; };
            //Controls.Add(picture);
            MouseDown += (o, e) => { BringToFront(); prevPos = Program.frontal.PointToClient(MousePosition); };
            MouseUp += (o, e) => 
            {
                if (prevPos == Program.frontal.PointToClient(MousePosition))
                {
                    if(e.Button == MouseButtons.Left) Mouse_Click(true);
                }
                else { Program.frontal.IconRelocation(); }
            };
            //picture.MouseClick += Mouse_Click;
            MouseEnter += (o, e) =>
            {
                Location = new Point(Location.X - 3, Location.Y - 3);
                SetSize("big");
            };
            MouseLeave += (o, e) =>
            {
                Location = new Point(Location.X + 3, Location.Y + 3);
                SetSize("small");
            };
            contextMenuStrip = new ContextMenuStrip
            {
                ImageScalingSize = new System.Drawing.Size(24, 24),
                Name = "contextMenuStrip1",
                Size = new System.Drawing.Size(141, 34)
            };
            editIcon = new ToolStripMenuItem
            {
                Size = new System.Drawing.Size(140, 30),
                Text = "Edit",
                Image = Properties.Resources.Edit_icon
            };
            editIcon.Click += new System.EventHandler(EditIcon_Click);
            removeIcon = new ToolStripMenuItem
            {
                Size = new System.Drawing.Size(140, 30),
                Text = "Remove Program",
                Image = Properties.Resources.delete_icon
            };
            removeIcon.Click += new System.EventHandler(RemoveIcon_Click);

            if (Path.GetExtension(exePath) != ".lnk")
            {
                Image admin_image = Properties.Resources.admin;
                if (as_admin_bool) admin_image = Properties.Resources.admin_no;
                run_otherWay = new ToolStripMenuItem
                {
                    Size = new System.Drawing.Size(140, 30),
                    Text = "Run as administrator",
                    Image = admin_image
                };
                if (as_admin_bool) run_otherWay.Text = "Run without admin privileges";
                run_otherWay.Click += new EventHandler(Run_otherWay_Click);
                contextMenuStrip.Items.AddRange(new ToolStripItem[] { editIcon, run_otherWay, removeIcon });
            }
            else
            {
                contextMenuStrip.Items.AddRange(new ToolStripItem[] { editIcon, removeIcon });
            }
            ContextMenuStrip = contextMenuStrip;
        }

        public void RefreshImg()
        {
            string withoutExt = Path.Combine(new string[] { Directory.GetParent(imgPath).FullName, Path.GetFileNameWithoutExtension(imgPath) });
            string specImg = withoutExt + Program.iconSize.Width + ".ico";
            string specImg_big = withoutExt + Program.iconSize.Width + "_big.ico";

            if (!File.Exists(specImg)) Compress(imgPath, Program.iconSize.Width, Program.iconSize.Height, specImg);
            if (!File.Exists(specImg_big)) Compress(imgPath, Program.iconSize.Width + 6, Program.iconSize.Height + 6, specImg_big);

            pic = new Bitmap(specImg);
            pic_big = new Bitmap(specImg_big);
            //pic = new Bitmap(pic, new Size(Program.iconSize.Width, Program.iconSize.Height));
            //pic_big = new Bitmap(pic, new Size(Program.iconSize.Width + 6, Program.iconSize.Height + 6));
            BackgroundImage = pic;
        }
        private void addNotFound(Graphics g, int width, int height)
        {
            GraphicsPath p = new GraphicsPath();
            int fontSize = (int)(width * 0.15f);
            StringFormat format = new StringFormat(StringFormatFlags.FitBlackBox);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Far;
            p.AddString(
                "-NotFound-",             // text to draw
                FontFamily.GenericSansSerif,  // or any other font family
                (int)FontStyle.Bold,      // font style (bold, italic, etc.)
                fontSize,       // em size
                new Rectangle(0, (int)((float)height / 100 * 30), width, (int)((float)height / 100 * 40)),
                format);          // set options here (e.g. center alignment)
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;
            Pen pen = new Pen(Brushes.White);
            pen.Width = 5.0f;
            g.DrawPath(pen, p);
            g.FillPath(Brushes.Red, p); //if you want it filled as well
        }

        private void addAdmin(Graphics g, int width, int height)
        {
            int admin_lenght = (int)((float)width * 0.4);
            Image image = Properties.Resources.admin;
            if (Path.GetExtension(exePath) == ".lnk") image = Properties.Resources.link;
            g.DrawImage(image, width - admin_lenght, 0, admin_lenght, admin_lenght);
        }

        public void SetSize(string size)
        {
            if(size == "big")
            {
                int new_width = Program.iconSize.Width + 6;
                int new_height = Program.iconSize.Height + 6;
                int fontSize = (int)(new_width * 0.1f);
                Size = new Size(new_width, new_height);
                BackgroundImage = pic_big;
                using (Graphics g = Graphics.FromImage(BackgroundImage))
                {
                    GraphicsPath p = new GraphicsPath();
                    StringFormat format = new StringFormat(StringFormatFlags.FitBlackBox);
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;
                    p.AddString(
                        name,             // text to draw
                        FontFamily.GenericSansSerif,  // or any other font family
                        (int)FontStyle.Bold,      // font style (bold, italic, etc.)
                        fontSize,       // em size
                        new Rectangle(0, (int)((float)new_height / 100 * 70), new_width, (int)((float)new_height / 100 * 30)),  // location where to draw text
                        format);          // set options here (e.g. center alignment)
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit; 
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    Pen pen = new Pen(Brushes.Black);
                    pen.Width = 4.0f;
                    g.DrawPath(pen, p);
                    g.FillPath(Brushes.White, p); //if you want it filled as well

                    if (as_admin_bool) addAdmin(g, new_width, new_height);
                    if (notFound) addNotFound(g, new_width, new_height);
                }
            }
            else
            {
                Size = new Size(Program.iconSize.Width, Program.iconSize.Height);
                BackgroundImage = pic;
                using (Graphics g = Graphics.FromImage(BackgroundImage))
                {
                    if (as_admin_bool) addAdmin(g, Program.iconSize.Width, Program.iconSize.Height);
                    if (notFound) addNotFound(g, Program.iconSize.Width, Program.iconSize.Height);
                }
            }
        }

        private void EditIcon_Click(object sender, EventArgs e)
        {
            Program.frontal.addIcon_Click(this);
        }
        private void RemoveIcon_Click(object sender, EventArgs e)
        {
            Program.frontal.RemoveIcon(this);
            Dispose();
        }

        private void Run_otherWay_Click(object sender, EventArgs e)
        {
            Mouse_Click(false);
        }

        private void Mouse_Click(object sender, EventArgs e)
        {
            Mouse_Click(true);
        }
        private void Mouse_Click(bool right)
        {
            if (Directory.Exists(exePath))
            {
                Console.WriteLine("Executing '" + exePath + "' from explorer.exe");
                Process.Start("explorer.exe", exePath);
            }
            else
            {
                bool aus = as_admin_bool;
                if (!right)
                {
                    if (aus == true) aus = false; else aus = true;
                }
                if (!File.Exists(exePath))
                {
                    MessageBox.Show("File or directory not found!");
                    return;
                }
                if (aus)
                {
                    Console.WriteLine("Executing '" + exePath + "' as admin user");
                    Process process = new Process()
                    {
                        StartInfo = new ProcessStartInfo(exePath)
                        {
                            WindowStyle = ProcessWindowStyle.Normal,
                            WorkingDirectory = Path.GetDirectoryName(exePath)
                        }
                    };
                    process.Start();
                }
                else { 
                    Console.WriteLine("Executing '" + exePath + "' as restricted user");
                    ProcessHelper.RunAsRestrictedUser(exePath);
                }
            }

            try
            {
                if (Program.vanish) Program.frontal.Close();
            }
            catch (Exception) 
            { 
                Console.WriteLine("EXC");
            }
        }
        public static void Compress(string image_path, int width, int height, string dest_path = "")
        {
            try
            {
                if (dest_path == "") dest_path = image_path;
                //StreamReader streamReader = new StreamReader(image_path);
                //Bitmap btm = (Bitmap)Bitmap.FromStream(streamReader.BaseStream);
                //streamReader.Close();
                Bitmap btm = new Bitmap(image_path);

                btm = new Bitmap(btm, new Size(width, height));
                btm.Save(dest_path, ImageFormat.Icon);
            }
            catch (Exception e) { Console.WriteLine("EXCEPTION: "+ e.Message); }
        }

        public void Print()
        {
            Console.WriteLine("Path: " + exePath + ", name: " + name + ", imgPath: " + imgPath);
        }
    }
}
