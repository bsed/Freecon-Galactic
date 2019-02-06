using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace VarViewer
{
    public partial class VarView : Form
    {
        public int TextBoxPadding = 10;
        public int CurrentTextBoxX = 0;
        public int CurrentTextBoxY = 10;
        public int TextBoxdx = 100;
        public int TextBoxdy = 45;
        public int TextBoxHeight = 10;

        public int LineLabelWidth = 100;

        public int UpdateInterval = 100;

        List<ClassView> _memberVarViews = new List<ClassView>();

        System.Timers.Timer _updateTimer;


        public VarView()
        {
            InitializeComponent();
            _initialize();
        
        }

        public VarView(object varToView)
        {
            InitializeComponent();
            _initialize();
            AddClassToView(varToView);

          
            
        }


        void _initialize()
        {
            this.AutoScroll = true;

            _updateTimer = new System.Timers.Timer();

            _updateTimer.Interval = UpdateInterval;
            _updateTimer.Elapsed += _updateTimer_Tick;
            _updateTimer.Start();

            Width = 1000;
            Height = 1000;
                       

            //CheckForIllegalCrossThreadCalls = false;//Yolo
        }

        
        public new void Show()
        {
            Thread t = new Thread(_showDialogThreaded);
            t.Start();

        }
        void _showDialogThreaded()
        {
            ShowDialog();
        }


        void _updateTimer_Tick(object sender, EventArgs e)
        {
            foreach(ClassView cv in _memberVarViews)
            {
                cv.Update();
            }
        }

        public int AddClassToView(object var)
        {
            CurrentTextBoxX = LineLabelWidth + TextBoxPadding; 
            ClassView fv = null;

            if (var.GetType().GetInterface("ICollection") != null)
            {
                if(((ICollection)var).Count == 0)
                {
                    return -2;
                }
                else
                    fv = new VarViewer.ListView(this, var, _memberVarViews.Count);
            }
            else
            {
                fv = new ClassView(this, var, _memberVarViews.Count);
            }
            if (fv.IsValid)
            {
                _memberVarViews.Add(fv);
                CurrentTextBoxY += TextBoxdy;
                return _memberVarViews.Count - 1;
            }
            else
                return -1;
        }




    }
    


    
    

    class ClassVariableView
    {



    }
}