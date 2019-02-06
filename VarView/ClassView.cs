using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace VarViewer
{
    class ClassView
    {
        protected List<FieldView> _fieldViews = new List<FieldView>();

        protected VarView _myform;

        protected object _myClass;

        public bool IsValid { get; set; }

        public int LineNumber { get; protected set; }

        public ClassView()
        {  }

        public ClassView(VarView myform, object classToView, int lineNumber)
        {
            LineNumber = lineNumber;       

            _myform = myform;
            _myClass = classToView;

            //Create line number object
            Label lineNumBox = new Label();
            lineNumBox.Text = lineNumber.ToString() + ": [" + _trimTypeName(_myClass.GetType().ToString()) + "]";
            lineNumBox.Left = 0;
            lineNumBox.Width = _myform.LineLabelWidth;
            lineNumBox.TextAlign = ContentAlignment.MiddleCenter;
            lineNumBox.Top = _myform.CurrentTextBoxY + 5;
            lineNumBox.BackColor = Color.White;
            _myform.Controls.Add(lineNumBox);

            FieldInfo[] fields = null;
            try
            {
                fields = classToView.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                fields = fields.Concat(classToView.GetType().BaseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)).ToArray();
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to create ClassView.");
                Console.WriteLine(e.Message);
                MessageBox.Show(e.Message, "Error");
                return;
            }

            foreach (var f in fields)
            {
                _myform.BackColor = Color.Black;
                //Create a new textbox for the name and value
                if (f.FieldType.IsPrimitive || f.FieldType == typeof(string))
                {
                    _createPrimitiveFieldView(f, _myClass);
                }
                else
                {
                    _createClassFieldView(f, _myClass);
                }
            }

            IsValid = true;

        }

        public void Update()
        {
            List<FieldView> toRemove = new List<FieldView>();
            foreach (FieldView pfv in _fieldViews)
            {
                if (pfv.IsValid)
                {
                    pfv.Update();
                }
                else
                {
                    toRemove.Add(pfv);
                }

            }

           foreach(var v in toRemove)
           {
               _fieldViews.Remove(v);
           }

           if (toRemove.Count != 0)
           {
               _myform.Invoke(new MethodInvoker(_shiftFieldViews));
           }
        }

        protected void _shiftFieldViews()
        {
            int currentX = _myform.LineLabelWidth + _myform.TextBoxPadding;
            foreach(FieldView fv in _fieldViews)
            {                
                fv.NameBox.Left = currentX;
                fv.ValueBox.Left = currentX;
               
                
                currentX += _myform.TextBoxdx;

            }



        }

        protected PrimitiveFieldView _createPrimitiveFieldView(FieldInfo f, object owner)
        {
            PrimitiveFieldView pfv = new PrimitiveFieldView(_myform, owner, f, _myform.CurrentTextBoxX, _myform.CurrentTextBoxY);
            _fieldViews.Add(pfv);

            _myform.Controls.Add(pfv.NameBox);
            _myform.Controls.Add(pfv.ValueBox);

            _myform.CurrentTextBoxX += _myform.TextBoxdx;

            return pfv;

        }

        protected ClassFieldView _createClassFieldView(FieldInfo f, object theclass)
        {
            ClassFieldView cfv = new ClassFieldView(_myform, theclass, f, _myform.CurrentTextBoxX, _myform.CurrentTextBoxY);
            _fieldViews.Add(cfv);
            _myform.Controls.Add(cfv.NameBox);
            _myform.Controls.Add(cfv.ValueBox);
            _myform.CurrentTextBoxX += _myform.TextBoxdx;

            return cfv;
        }

        protected string _trimTypeName(string typename)
        {
            int startIndex = typename.LastIndexOf('.') + 1;
            return typename.Substring(startIndex);
        }

        
    }
}
