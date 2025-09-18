using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ENROLLMENT_V3.ControlesDesko
{
    public partial class StateLabel : UserControl
    {
        public StateLabel()
        {
            InitializeComponent();
        }
        private String goodText = "Good";
        [
        Category("Appearance"),
        Description("Text for state 'Good'")
        ]
        public String GoodText { get { return goodText; } set { goodText = value; updateAppearence(); } }

        private String badText = "Bad";
        [
        Category("Appearance"),
        Description("Text for state 'Bad'")
        ]
        public String BadText { get { return badText; } set { badText = value; updateAppearence(); } }

        private String attentionText = "Attention";
        [
        Category("Appearance"),
        Description("Text for state 'Attention'")
        ]
        public String AttentionText { get { return attentionText; } set { attentionText = value; updateAppearence(); } }

        private String offText = "Off";
        [
        Category("Appearance"),
        Description("Text for state 'Off'")
        ]
        public String OffText { get { return offText; } set { offText = value; updateAppearence(); } }


        private void updateAppearence()
        {
            switch (state)
            {
                case LabelState.Good:
                    labelState.BackColor = Color.Lime;
                    labelState.Text = goodText;
                    break;
                case LabelState.Bad:
                    labelState.BackColor = Color.Red;
                    labelState.Text = badText;
                    break;
                case LabelState.Attention:
                    labelState.BackColor = Color.Yellow;
                    labelState.Text = attentionText;
                    break;
                case LabelState.Off:
                    labelState.BackColor = Color.LightGray;
                    labelState.Text = offText;
                    break;
            }
        }

        public String Text { get { return labelState.Text; } set { labelState.Text = value; } }

        private LabelState state = LabelState.Off;
        [
        Category("Appearance"),
        Description("Current state")
        ]
        public LabelState State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
                updateAppearence();
            }
        }

        private void StateLabel_Load(object sender, EventArgs e)
        {
            state = LabelState.Off;
        }

        private void labelState_Click(object sender, EventArgs e)
        {

        }
    }

    public enum LabelState
    {
        Good,
        Bad,
        Attention,
        Off
    }
}
