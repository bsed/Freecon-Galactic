using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace VarViewer
{
    abstract class FieldView
    {
        public bool EnableUpdating = true;

        public object Variable { get; protected set; }

        public FieldInfo VariableData { get; protected set; }

        public Label NameBox { get; set; }

        public TextBox ValueBox { get; set; }

        protected VarView _form { get; set; }

        public Color BaseColor { get; set; }

        public bool IsValid { get; set; }

        public FieldView(VarView myForm, object variableToView, FieldInfo vardata, int left, int top)
        {
            _form = myForm;

            string valueText = "";
            try
            {
                valueText = vardata.GetValue(variableToView).ToString();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                IsValid = false;
            }


            VariableData = vardata;
            Variable = variableToView;
            BaseColor = Color.White;

            NameBox = new Label();
            NameBox.Left = left;
            NameBox.Top = top;
            NameBox.Text = _trimFieldName(vardata.Name);
            NameBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            NameBox.BackColor = BaseColor;
            NameBox.Width = 100;
            NameBox.MouseClick += RightClickHandler;
 
            ValueBox = new TextBox();
            ValueBox.Left = left;
            ValueBox.Top = top + 20;
            ValueBox.Text = valueText;
            ValueBox.TextAlign = HorizontalAlignment.Center;
            ValueBox.MouseClick += RightClickHandler;



        }

        void RightClickHandler(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                _form.Controls.Remove(NameBox);
                _form.Controls.Remove(ValueBox);
                IsValid = false;

            }
        }

        public virtual void Update()
        {
 


        }

        protected string _trimFieldName(string propertyname)
        {
            if (propertyname.Contains('<'))
            {
                int startIndex = propertyname.IndexOf('<') + 1;
                int stopIndex = propertyname.IndexOf('>');
                return propertyname.Substring(startIndex, stopIndex - startIndex);
            }

            return propertyname;

        }

    }
}
