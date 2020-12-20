using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CPMproject.model;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Color = Microsoft.Msagl.Drawing.Color;
using Label = Microsoft.Msagl.Drawing.Label;

namespace CPMproject
{
    public partial class CPMHome : Form
    {
        List<Activity> lsactivity = new List<Activity>();
       GViewer viewer = new GViewer();
       Graph g = new Graph("graph");
        public CPMHome()
        {
            InitializeComponent();
        }
        int a;
        private void txtNumberofActivity_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtNumberofActivity.Text, out a))
            {
                data.RowCount = a;
            }
        }
        private void txtNumberofActivity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || e.KeyChar == 8)
                e.Handled = false;
            else
                e.Handled = true;
        }
        private void data_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            bool faild = true;
            DataGridViewColumn col = data.Columns[e.ColumnIndex] as DataGridViewColumn;
            if (col.Name == idactivity.Name || col.Name == timeactivity.Name)
            {

                DataGridViewTextBoxCell cell = data[e.ColumnIndex, e.RowIndex] as DataGridViewTextBoxCell;
                if (cell != null)
                {
                    {
                        char[] chars = e.FormattedValue.ToString().ToCharArray();
                        if (chars.Length == 0)
                        {
                            MessageBox.Show($"لايمكن ترك حقل {col.HeaderText} فارغا", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                        }
                        else
                            foreach (char c in chars)
                            {
                                if (char.IsDigit(c) == false)
                                {
                                    MessageBox.Show($"يجب ان تكون المدخلات ارقام فقط في حقل {col.HeaderText}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    e.Cancel = true;
                                    break;
                                }
                            }
                    }

                }

            }
            else if (col.Name == preactivity.Name)
            {
                DataGridViewTextBoxCell cell = data[e.ColumnIndex, e.RowIndex] as DataGridViewTextBoxCell;
                if (cell != null)
                {
                    if (e.FormattedValue.ToString() == "-")
                    {
                    }
                    else
                    {
                        string[] a = e.FormattedValue.ToString().Split(',');

                        foreach (var item in a)
                        {
                            for (int i = 0; i < e.RowIndex; i++)
                            {
                                if (data.Rows[i].Cells[nameactivity.Name].Value.ToString() == item)
                                {
                                    faild = false;
                                    break;
                                }
                            }
                            if (faild == true)
                            {
                                MessageBox.Show($"هذه الفعالية غير موجود ({item})", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                e.Cancel = true;
                                break;
                            }
                            faild = true;
                        }
                    }

                }
            }
            else if (col.Name == nameactivity.Name)
            {
                DataGridViewTextBoxCell cell = data[e.ColumnIndex, e.RowIndex] as DataGridViewTextBoxCell;
                if (cell != null)
                {
                    if (e.FormattedValue.ToString().ToCharArray().Length == 0)
                    {
                        MessageBox.Show($"لايمكن ترك حقل {col.HeaderText} فارغا", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                    }
                }
            }
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void CPMHome_Load(object sender, EventArgs e)
        {

        }
        private void data_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                foreach (var item in data.CurrentRow.Cells)
                {
                    if ((item as DataGridViewTextBoxCell).FormattedValue.ToString().ToCharArray().Length == 0)
                    {
                        MessageBox.Show("لا يمكن ترك هذه الصف فارغا والانتقال الى الصف التالي", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Data.ToString());
            }
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            lsactivity.Clear();
            lsactivity.Add(new Activity() { id = 0, description = "start" });
            #region load activity to list
            for (int i = 0; i < data.Rows.Count; i++)
            {
                Activity act = new Activity();
                act.id = int.Parse(data.Rows[i].Cells[0].Value.ToString());
                act.description = data.Rows[i].Cells[1].Value.ToString();
                act.duration = int.Parse(data.Rows[i].Cells[3].Value.ToString());
                if (data.Rows[i].Cells[2].Value.ToString() != "-")
                {
                    string[] a = data.Rows[i].Cells[2].Value.ToString().Split(',');
                    foreach (var item in a)
                    {
                        var sea = lsactivity.Single(x => x.description == item);
                        act.predecessors.Add(sea);
                        sea.successors.Add(act);
                    }
                }
                else
                {
                    var sea = lsactivity.Single(x => x.description == "start");
                    act.predecessors.Add(sea);
                    sea.successors.Add(act);
                }
                lsactivity.Add(act);
            }
            #endregion

            lsactivity.Add(new Activity() { id = lsactivity.Count, description = "finish", predecessors = lsactivity.FindAll(x => x.successors.Count == 0) });
            foreach (var item in lsactivity[lsactivity.Count - 1].predecessors)
            {
                item.successors.Add(lsactivity[lsactivity.Count - 1]);
            }

            getforward(lsactivity);
            getbackword(lsactivity);
            showresult(lsactivity);
            drawcpm(lsactivity);
        }

        private void data_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private List<Activity> getforward(List<Activity> lsac)
        {
            lsac[1].ef = lsac[1].es + lsac[1].duration;

            for (int i = 2; i <= lsac.Count - 1; i++)
            {
                foreach (Activity activity in lsac[i].predecessors)
                {
                    if (activity.description != "start")
                        if (lsac[i].es < activity.ef)
                            lsac[i].es = activity.ef;
                }

                lsac[i].ef = lsac[i].es + lsac[i].duration;
            }
            return lsac;
        }
        private List<Activity> getbackword(List<Activity> lsac)
        {

            lsac[lsac.Count - 1].lf = lsac[lsac.Count - 1].ef;
            lsac[lsac.Count - 1].ls = lsac[lsac.Count - 1].lf - lsac[lsac.Count - 1].duration;

            for (int i = lsac.Count - 2; i > 0; i--)
            {
                foreach (var item in lsac[i].successors)
                {
                    if (lsac[i].lf == 0)
                    {
                        lsac[i].lf = item.ls;
                    }
                    else
                       if (lsac[i].lf > item.ls)
                        lsac[i].lf = item.ls;
                }
                lsac[i].ls = lsac[i].lf - lsac[i].duration;
            }
            return lsac;
        }

        private List<Activity> getCriticalPath(List<Activity> list)
        {
            List<Activity> lsacti = new List<Activity>();
            foreach (Activity activity in list)
            {
                if ((activity.es - activity.ls == 0) && (activity.ef - activity.lf == 0))
                    lsacti.Add(activity);
            }
            lsacti.Add(list[list.Count - 1]);
            return lsacti;
        }

        private void showresult(List<Activity> lsac)
        {
            dataresult.Rows.Clear();
            foreach (var item in lsac.Where(x => x.description != "start").Where(x => x.description != "finish"))
            {
                dataresult.Rows.Add(item.id, item.description, string.Join(",", (item.predecessors.Select(x => x.description).ToArray())), string.Join(",", (item.successors.Select(x => x.description).ToArray())), item.duration, item.es, item.ef, item.ls, item.lf);
            }

        }

        void drawcpm(List<Activity> lsac)
        {
            g = new Microsoft.Msagl.Drawing.Graph("graph");
            g.Attr.BackgroundColor = Microsoft.Msagl.Drawing.Color.White;
            viewer.Graph = g;
            foreach (var item in lsac)
            {
                foreach (var it in item.successors)
                {
                    g.AddEdge($"{item.description}", it.description);
                }
            }

            viewer.CurrentLayoutMethod = LayoutMethod.MDS;
            viewer.Graph = g;
            foreach (var get in getCriticalPath(lsac))
            {
                if (get.description == "start" || get.description == "finish")
                {
                    g.FindNode($"{get.description}").Attr = new NodeAttr()
                    {
                        Color = Color.Blue,
                        Shape = Shape.Box,
                        FillColor = Color.White,


                    };
                    if (get.description == "finish")
                    {
                        g.FindNode($"{get.description}").LabelText = $"{get.description} CPM={get.lf}";


                    }

                }
                else
                    g.FindNode($"{get.description}").Attr = new NodeAttr()
                    {
                        Color = Color.Red,
                        Shape = Shape.Circle,
                        FillColor = Color.Silver,
                    };
            }
            viewer.Dock = DockStyle.Fill;
            splitContainer4.Panel1.Controls.Add(viewer);
        }
    }
}
