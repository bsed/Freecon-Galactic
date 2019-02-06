using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace VarViewer
{
    class ClassFieldView:FieldView
    {
        bool _expanded;
        bool _isEmptyCollection;

        public ClassFieldView(VarView myForm, object variableToView, FieldInfo vardata, int left, int top):base(myForm, variableToView, vardata, left, top)
        {

            _init();
            ValueBox.MouseClick += ValueBox_Click;
            IsValid = true;

        }

        public override void Update()
        {
            base.Update();
        
            if(_expanded && _isEmptyCollection)
            {
                if(((ICollection)Variable).Count > 0)
                {
                    _expanded = false;
                    _init();
                }
            }
        }

        void _init()
        {
            ValueBox.ReadOnly = true;
            ValueBox.Text = "Click to view";            
            ValueBox.BackColor = Color.Green;
        }

        protected void ValueBox_Click(object sender, EventArgs e)
        {
            var args = e as MouseEventArgs;
            if (args.Button == MouseButtons.Left)
            {
                if (!_expanded)
                {
                    int lineNumber = _form.AddClassToView(VariableData.GetValue(Variable));
                    _expanded = true;
                    ValueBox.BackColor = Color.Red;
                    if (lineNumber > -1)
                    {
                        ValueBox.Text = "Line " + lineNumber;
                    }
                    else if(lineNumber == -2)
                    {
                        _isEmptyCollection = true;
                        ValueBox.Text = "Empty Collection.";
                    }
                    else
                    {
                        ValueBox.Text = "Very Specific Error.";
                    }
                }
            }
            
        }

 
    }
}
