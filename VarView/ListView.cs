using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace VarViewer
{
    class ListView:ClassView
    {
        List<FieldView> _views = new List<FieldView>();

        bool _wasEmpty;//TODO: Use this to repopulate the list when it becomes non empty

        public ListView(VarView myform, dynamic classToView, int lineNumber)
        {
            _myform = myform;
            _myClass = classToView;
            LineNumber = lineNumber;


            
            
            int count = 0;
            
            foreach (object obj in classToView)
            {
                object varToView = obj;


                //Create line number object
                Label lineNumBox = new Label();
                lineNumBox.Text = "Class " + lineNumber.ToString();
                lineNumBox.Left = 0;
                lineNumBox.Width = _myform.LineLabelWidth;
                lineNumBox.TextAlign = ContentAlignment.MiddleCenter;
                lineNumBox.Top = _myform.CurrentTextBoxY + 5;
                lineNumBox.BackColor = Color.White;
                _myform.Controls.Add(lineNumBox);


                //Dirty way to handle a key value pair
                Type valueType = obj.GetType();
                if (valueType.IsGenericType)
                {
                    Type baseType = valueType.GetGenericTypeDefinition();
                    if (baseType == typeof(KeyValuePair<,>))
                    {
                        varToView = obj.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public).GetValue(obj);
                        FieldInfo f = obj.GetType().GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
                        var key = obj.GetType().GetProperty("Key", BindingFlags.Instance | BindingFlags.Public).GetValue(obj);

                        var cfv = _createClassFieldView(f, obj);
                        cfv.NameBox.Text = varToView.GetType().ToString() + ": " + key.ToString();

                        IsValid = true;
                        continue;

                    }
                }

                FieldInfo[] fields = null;
                try
                {
                    fields = varToView.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    fields = fields.Concat(varToView.GetType().BaseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)).ToArray();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to create ClassView.");
                    Console.WriteLine(e.Message);
                    return;
                }

                foreach (var f in fields)
                {
                    _myform.BackColor = Color.Black;
                    //Create a new textbox for the name and value
                    if (f.FieldType.IsPrimitive || f.FieldType == typeof(string))
                    {
                        _createPrimitiveFieldView(f, varToView);
                    }
                    else
                    {
                        _createClassFieldView(f, varToView);
                    }
                }

                IsValid = true;

                count++;
                if(count == 10)
                    return;
            }       
        
        
            if(count == 0)//If the list is empty
            {
                HackString tx = new HackString();//lolhacks
                tx.Text = "Empty Collection.";
                FieldInfo field = tx.GetType().GetField("Text");
                
                _createPrimitiveFieldView(field, tx);
                IsValid = true;
            }
        
        }

        class HackString
        {
            public string Text;
        }
    }
}
