using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoseAnnotationTool
{
    public partial class Form1 : Form
    {
        class Joint {
            public bool Use { get; set; }
            public PointF Position { get; set; }
            public int ParentIndex { get; set; }
            public Joint(float x, float y, int parent, bool use = true)
            {
                Position = new PointF(x, y);
                ParentIndex = parent;
                Use = use;
            }
        }


        Datasets datasets = null;

        string ImagePath = "";
        Bitmap Img = null;

        CheckBox[] checkboxes = new CheckBox[14];
        Joint[] kps = new Joint[14];
        Color[] jointColors= new Color[14];
        Color[] boneColors = new Color[13];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkboxes[0] = checkBox1;
            checkboxes[1] = checkBox2;
            checkboxes[2] = checkBox3;
            checkboxes[3] = checkBox4;
            checkboxes[4] = checkBox5;
            checkboxes[5] = checkBox6;
            checkboxes[6] = checkBox7;
            checkboxes[7] = checkBox8;
            checkboxes[8] = checkBox9;
            checkboxes[9] = checkBox10;
            checkboxes[10] = checkBox11;
            checkboxes[11] = checkBox12;
            checkboxes[12] = checkBox13;
            checkboxes[13] = checkBox14;

            // edge color
            jointColors[0] = Color.FromArgb(255, 233, 163, 201);
            jointColors[1] = Color.FromArgb(255, 233, 163, 201);
            jointColors[2] = Color.FromArgb(255, 233, 163, 201);
            jointColors[3] = Color.FromArgb(255, 197, 27, 125);
            jointColors[4] = Color.FromArgb(255, 197, 27, 125);
            jointColors[5] = Color.FromArgb(255, 197, 27, 125);
            jointColors[6] = Color.FromArgb(255, 145, 191, 219);
            jointColors[7] = Color.FromArgb(255, 145, 191, 219);
            jointColors[8] = Color.FromArgb(255, 145, 191, 219);
            jointColors[9] = Color.FromArgb(255, 69, 117, 180);
            jointColors[10] = Color.FromArgb(255, 69, 117, 180);
            jointColors[11] = Color.FromArgb(255, 69, 117, 180);
            jointColors[12] = Color.FromArgb(255, 118, 42, 131);
            jointColors[13] = Color.FromArgb(255, 118, 42, 131);

            boneColors[0] = Color.FromArgb(255, 233, 163, 201);
            boneColors[1] = Color.FromArgb(255, 233, 163, 201);
            boneColors[2] = Color.FromArgb(255, 233, 163, 201);
            boneColors[3] = Color.FromArgb(255, 197, 27, 125);
            boneColors[4] = Color.FromArgb(255, 197, 27, 125);
            boneColors[5] = Color.FromArgb(255, 197, 27, 125);
            boneColors[6] = Color.FromArgb(255, 145, 191, 219);
            boneColors[7] = Color.FromArgb(255, 145, 191, 219);
            boneColors[8] = Color.FromArgb(255, 145, 191, 219);
            boneColors[9] = Color.FromArgb(255, 69, 117, 180);
            boneColors[10] = Color.FromArgb(255, 69, 117, 180);
            boneColors[11] = Color.FromArgb(255, 69, 117, 180);
            boneColors[12] = Color.FromArgb(255, 118, 42, 131);

            kps[0] = new Joint(110, 100, 1);
            kps[1] = new Joint(120, 100, 2);
            kps[2] = new Joint(130, 100, 8);
            kps[3] = new Joint(140, 100, 9);
            kps[4] = new Joint(150, 100, 3);
            kps[5] = new Joint(160, 100, 4);
            kps[6] = new Joint(170, 100, 7);
            kps[7] = new Joint(180, 100, 8);
            kps[8] = new Joint(190, 100, 12);
            kps[9] = new Joint(200, 100, 12);
            kps[10] = new Joint(300, 100, 9);
            kps[11] = new Joint(400, 100, 10);
            kps[12] = new Joint(500, 100, 13);
            kps[13] = new Joint(600, 100, -1);

            datasets = new Datasets();
            foreach (var f in datasets.FileList)
            {
                string relative = f.Substring(datasets.DataDir.Length);
                var tokens = relative.Split('/', '\\').Where(t => string.IsNullOrWhiteSpace(t) == false).ToList();

                var cur = treeView1.Nodes;
                TreeNode leaf = null;
                foreach (var t in tokens)
                {
                    bool found = false;
                    foreach (TreeNode n in cur)
                    {
                        if (n.Text == t)
                        {
                            cur = n.Nodes;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        leaf = cur.Add(t);
                        cur = leaf.Nodes;
                    }
                }

                if (leaf != null)
                {
                    string annotPath = System.IO.Path.ChangeExtension(f, ".txt");
                    if (System.IO.File.Exists(annotPath))
                    {
                        leaf.ForeColor = Color.FromArgb(255, 69, 117, 255);
                    }
                }
            }

        }

        private void checkBoxJoint_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkboxes.Length; i++)
            {
                if (sender == checkboxes[i])
                {
                    kps[i].Use = checkboxes[i].Checked;
                    saveAnnotation(ImagePath);
                    break;
                }
            }
            pictureBox1.Invalidate();
        }

        private void checkBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var cb in checkboxes)
            {
                cb.Checked = checkBox15.Checked;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

            if (e.Node.Nodes.Count > 0)
            {
                return;
            }

            // 終端ノードなら画像読み込み
            List<string> tokens = new List<string>();
            tokens.Add(e.Node.Text);
            var n = e.Node;
            while (n.Parent != null)
            {
                n = n.Parent;
                tokens.Add(n.Text);
            }

            tokens.Add(datasets.DataDir);
            tokens.Reverse();
            string path = System.IO.Path.Combine(tokens.ToArray());
            loadImage(path);
            loadAnnotation(path);
            pictureBox1.Invalidate();
        }
        void loadImage(string path)
        {
            if (Img != null)
            {
                Img.Dispose();
                Img = null;
            }
            ImagePath = path;
            using (var bmp = Bitmap.FromFile(path))
            {
                Img = bmp.Clone() as Bitmap;
            }
        }

        void saveAnnotation(string imagePath)
        {
            // text to save
            string text = imagePath + "\n";
            foreach (var kp in kps)
            {
                text += "0," + kp.Position.X + "," + kp.Position.Y + "," + kp.Use + "\n";
            }

            // save file
            string target = System.IO.Path.ChangeExtension(imagePath, "txt");
            System.IO.File.WriteAllText(target, text);
        }

        void loadAnnotation(string imagePath)
        {
            string target = System.IO.Path.ChangeExtension(imagePath, "txt");
            if (System.IO.File.Exists(target) == false)
            {
                return;
            }
            var lines = System.IO.File.ReadAllLines(target);

            string imgPath = lines[0];
            System.Diagnostics.Debug.Assert(imgPath == imagePath);

            for (int i = 0; i < kps.Length; i++)
            {
                var tokens = lines[i + 1].Split(',');
                string person = tokens[0];
                float x = float.Parse(tokens[1]);
                float y = float.Parse(tokens[2]);
                bool use = bool.Parse(tokens[3]);
                kps[i] = new Joint(x, y, kps[i].ParentIndex, use);
            }
        }

        float lastRatio_ = 1.0f;

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (Img != null)
            {
                float w = e.Graphics.VisibleClipBounds.Width;
                float h = e.Graphics.VisibleClipBounds.Height;

                float rx = w / Img.Width;
                float ry = h / Img.Height;

                float ratio = Math.Min(rx, ry);
                lastRatio_ = ratio;
                float iw = Img.Width * ratio;
                float ih = Img.Height * ratio;

                e.Graphics.DrawImage(Img, 0, 0, iw, ih);

                for (int i = 0; i < kps.Length; i++)
                {
                    var kp = kps[i];
                    float x = kp.Position.X;
                    float y = kp.Position.Y;
                    x *= ratio;
                    y *= ratio;
                    float r = ratio * 10;

                    if (kp.Use)
                    {
                        e.Graphics.DrawEllipse(new Pen(jointColors[i]), x - r, y - r, 2 * r, 2 * r);
                    }

                    if (kp.ParentIndex >= 0)
                    {
                        var parent = kps[kp.ParentIndex];
                        if (kp.Use && parent.Use)
                        {
                            float px = parent.Position.X;
                            float py = parent.Position.Y;
                            px *= ratio;
                            py *= ratio;
                            e.Graphics.DrawLine(new Pen(boneColors[i], 4 * ratio), x, y, px, py);
                        }
                    }

                    if (kp.Use)
                    {
                        if (kp == HoveringJoint)
                        {
                            e.Graphics.DrawEllipse(new Pen(Color.Red, 2), x - 2 * r, y - 2 * r, 4 * r, 4 * r);
                        }
                        if (kp == SelectingJoint)
                        {
                            e.Graphics.DrawEllipse(new Pen(Color.Red, 2), x - 2 * r, y - 2 * r, 4 * r, 4 * r);
                        }
                    }
                }
            }
        }

        Joint HoveringJoint = null;
        Joint SelectingJoint = null;

        Joint findJointFromPosition(PointF screenPt)
        {
            foreach (var kp in kps)
            {
                if (kp.Use)
                {
                    float x = kp.Position.X;
                    float y = kp.Position.Y;
                    x *= lastRatio_;
                    y *= lastRatio_;
                    float r = lastRatio_ * 10;
                    float dx = screenPt.X - x;
                    float dy = screenPt.Y - y;
                    float distSq = dx * dx + dy * dy;
                    if (distSq <= r * r * 1.5f * 1.5f)
                    {
                        return kp;
                    }
                }
            }
            return null;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectingJoint == null)
            {
                HoveringJoint = findJointFromPosition(e.Location);
            }
            else
            {
                HoveringJoint = null;
                int mx = e.X;
                int my = e.Y;
                SelectingJoint.Position = new PointF(mx / lastRatio_, my / lastRatio_);
            }

            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            SelectingJoint = findJointFromPosition(e.Location);
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            SelectingJoint = null;
            saveAnnotation(ImagePath);
            pictureBox1.Invalidate();
        }
        
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        
    }
}
