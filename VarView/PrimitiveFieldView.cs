using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace VarViewer
{
    class PrimitiveFieldView : FieldView
    {

        public PrimitiveFieldView(VarView myForm, object variableToView, FieldInfo vardata, int left, int top)
            : base(myForm, variableToView, vardata, left, top)
        {
            ValueBox.KeyDown += tb_KeyDown;
            ValueBox.LostFocus += ValueBox_LostFocus;
            IsValid = true;
        }

       
        public override void Update()
        {
            ValueBox.BeginInvoke(new MethodInvoker(_updateValueBox));
            
        }

        void _updateValueBox()
        {
            try
            {

                if (!ValueBox.Focused)
                    ValueBox.Text = VariableData.GetValue(Variable).ToString();


            }
            catch (Exception e)
            {
                Console.WriteLine("Error in PrimitiveFieldView.Update():");
                Console.WriteLine(e.Message);
            }

        }

        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                try
                {
                    VariableData.SetValue(Variable, Convert.ChangeType(((TextBox)sender).Text, VariableData.FieldType));
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    ValueBox.BackColor = Color.FromArgb(255, 0, 255, 0);//Custom green color

                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                    ValueBox.BackColor = Color.Red;
                }

            }
        }

        void ValueBox_LostFocus(object sender, EventArgs e)
        {
            ValueBox.BackColor = BaseColor;
        }       

    }
}
