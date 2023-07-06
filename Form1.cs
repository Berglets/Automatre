using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Automatre.ActionDetails;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Automatre
{
    public partial class Form1 : Form
    {
        private bool autoRunning = false;
        private Thread autoThread = null;

        private bool nothingSelectedBefore = true;
        private bool recordingMouseCoordinates = true;

        private bool listenKeyMouseCoords = false;
        private bool listenKeyStart = false;
        private bool listenKeyAutoPress = false;

        private Keys selectedKeyMouseCoords = Keys.F3;
        private Keys selectedKeyStart = Keys.F4;
        private Keys selectedKeyAutoPress = Keys.A;

        private List<ActionDetails> actions = new List<ActionDetails>();

        //Mouse actions
        private const int MOUSEEVENT_LEFTDOWN = 0x00000002;
        private const int MOUSEEVENT_LEFTUP = 0x00000004;
        private const int MOUSEEVENT_MIDDLEDOWN= 0x00000020;
        private const int MOUSEEVENT_MIDDLEUP= 0x00000040;
        private const int MOUSEEVENT_RIGHTDOWN = 0x00000008;
        private const int MOUSEEVENT_RIGHTUP = 0x00000010;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Text = "Key Press";
            buttonEdit.Enabled = false;
            buttonRemove.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (recordingMouseCoordinates)
            {
                int x = Cursor.Position.X;
                int y = Cursor.Position.Y;
                labelMouseLocation.Text = x + ", " + y;
                numericUpDown11.Value = x;
                numericUpDown10.Value = y;
            }
            if(autoRunning)
            {
                if(!autoThread.IsAlive)
                {
                    autoRunning = false;
                    autoThread = null;
                    buttonStart.Enabled = true;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (selectedKeyMouseCoords == keyData)
            {
                recordingMouseCoordinates = recordingMouseCoordinates ? false : true;
            }

            if (selectedKeyStart == keyData)
            {
                this.Close();
            }

            if (listenKeyMouseCoords)
            {
                selectedKeyMouseCoords = keyData;
                buttonChangeHotkey.Enabled = true;
                label1.Text = "Press '" + selectedKeyMouseCoords.ToString() + "' to Toggle";
                listenKeyMouseCoords = false;
            }

            if (listenKeyAutoPress)
            {
                selectedKeyAutoPress = keyData;
                changeKeyAutoButton.Enabled = true;
                label8.Text = "Key: " + selectedKeyAutoPress;
                listenKeyAutoPress = false;
            }

            if (listenKeyStart)
            {
                selectedKeyStart = keyData;
                buttonStartHotkey.Enabled = true;
                label11.Text = "Press '" + selectedKeyStart.ToString() + "' to Start/Stop";
                listenKeyStart = false;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private ActionDetails getActionFromDisplay()
        {
            ActionDetails action = null;
            switch (comboBox1.Text)
            {
                case "Click":
                    ActionType actionType = ActionType.NONE;
                    switch (comboBox3.Text)
                    {
                        case "Right Click":
                            actionType = ActionType.CLICK_RIGHT;
                            break;
                        case "Left Click":
                            actionType = ActionType.CLICK_LEFT;
                            break;
                        case "Middle Click":
                            actionType = ActionType.CLICK_MIDDLE;
                            break;
                    }
                    action = new ActionDetails(actionType, (int)numericUpDown4.Value, (int)numericUpDown3.Value);
                    action.order = (int)numericUpDown5.Value;
                    break;
                case "Key Press":
                    action = new ActionDetails(selectedKeyAutoPress, (int)numericUpDown2.Value, (int)numericUpDown1.Value);
                    action.order = (int)numericUpDown6.Value;
                    break;
                case "Move Mouse":
                    action = new ActionDetails(ActionType.MOUSE_MOVE, (int)numericUpDown11.Value, (int)numericUpDown10.Value, (int)numericUpDown9.Value, (int)numericUpDown8.Value);
                    action.order = (int)numericUpDown7.Value;
                    break;
                case "Repeat":
                    action = new ActionDetails((int)numericUpDown12.Value, (int)numericUpDown13.Value, (int)numericUpDown16.Value, (int)numericUpDown15.Value);
                    action.order = (int)numericUpDown14.Value;
                    break;
            }
            return action;
        }

        private void displayAction(ActionDetails details)
        {
            //disable all action group controls
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;

            switch (details.action)
            {
                case ActionType.KEY_PRESS:
                    comboBox1.Text = "Key Press";
                    panel2.Visible = true;
                    numericUpDown2.Value = details.durationMS;
                    numericUpDown1.Value = details.repeatAmount;
                    numericUpDown6.Maximum = listView.Items.Count + 1;
                    numericUpDown6.Value = details.order;
                    label8.Text = "Key: " + details.keyPress;
                    selectedKeyAutoPress = details.keyPress;
                    break;
                case ActionType.MOUSE_MOVE:
                    comboBox1.Text = "Move Mouse";
                    panel3.Visible = true;
                    numericUpDown11.Value = details.cursorX;
                    numericUpDown10.Value = details.cursorY;
                    numericUpDown9.Value = details.durationMS;
                    numericUpDown8.Value = details.repeatAmount;
                    numericUpDown7.Maximum = listView.Items.Count + 1;
                    numericUpDown7.Value = details.order;
                    break;
                case ActionType.REPEAT:
                    comboBox1.Text = "Repeat";
                    panel4.Visible = true;
                    numericUpDown12.Value = details.repeatStart;
                    numericUpDown13.Value = details.repeatEnd;
                    numericUpDown16.Value = details.durationMS;
                    numericUpDown15.Value = details.repeatAmount;
                    numericUpDown14.Maximum = listView.Items.Count + 1;
                    numericUpDown14.Value = details.order;
                    break;
                default: //any click
                    comboBox1.Text = "Click";
                    panel1.Visible = true;
                    switch (details.action)
                    {
                        case ActionType.CLICK_RIGHT:
                            comboBox3.Text = "Right Click";
                            break;
                        case ActionType.CLICK_LEFT:
                            comboBox3.Text = "Left Click";
                            break;
                        case ActionType.CLICK_MIDDLE:
                            comboBox3.Text = "Middle Click";
                            break;
                    }
                    numericUpDown4.Value = details.durationMS;
                    numericUpDown3.Value = details.repeatAmount;
                    numericUpDown5.Maximum = listView.Items.Count + 1;
                    numericUpDown5.Value = details.order;
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionDetails detailsDefault = null;
            //find default action details
            switch (comboBox1.Text)
            {
                case "Click":
                    detailsDefault = new ActionDetails(ActionType.CLICK_LEFT, 1000, 1);
                    break;
                case "Key Press":
                    detailsDefault = new ActionDetails(Keys.A, 1000, 1);
                    break;
                case "Move Mouse":
                    detailsDefault = new ActionDetails(ActionType.MOUSE_MOVE, 555, 555, 1000, 1);
                    break;
                case "Repeat":
                    detailsDefault = new ActionDetails(1, 1, 1000, 1);
                    break;
            }
            detailsDefault.order = listView.Items.Count + 1;
            displayAction(detailsDefault);
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (nothingSelectedBefore)
            {
                buttonEdit.Enabled = true;
                buttonRemove.Enabled = true;
                nothingSelectedBefore = false;
            }
            if (listView.SelectedItems.Count != 0)
            {
                int i = listView.SelectedItems[0].Index;
                ActionDetails actionDetails = actions[i];
                displayAction(actionDetails);
            }
        }

        private void changeKeyAutoButton_Click(object sender, EventArgs e)
        {
            changeKeyAutoButton.Enabled = false;
            listenKeyAutoPress = true;
            label8.Text = "Press any Key";
        }

        private void buttonChangeHotkey_Click(object sender, EventArgs e)
        {
            buttonChangeHotkey.Enabled = false;
            listenKeyMouseCoords = true;
            label1.Text = "Press Any Key to Change";
        }

        private void buttonStartHotkey_Click(object sender, EventArgs e)
        {
            buttonStartHotkey.Enabled = false;
            listenKeyStart = true;
            label11.Text = "Press Any Key to Change";
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            Thread thread = new Thread(autoRun);
            thread.IsBackground = true;
            thread.Start(copyActionsList(actions, 0, actions.Count - 1));
            autoThread = thread;
            autoRunning = true;
        }

        private static void autoRun(object actionsToPerform)
        {
            List<ActionDetails> actions = (List<ActionDetails>)actionsToPerform;

            foreach(ActionDetails action in actions)
            {
                for(int i = 0; i < action.repeatAmount; i++)
                {
                    Thread.Sleep(action.durationMS);
                    performAction(action, actions);
                }
            }
        }

        private static void performAction(ActionDetails action, List<ActionDetails> actions)
        {
            switch(action.action)
            {
                case ActionType.MOUSE_MOVE:
                    Cursor.Position = new Point(action.cursorX, action.cursorY);
                    break;
                case ActionType.CLICK_LEFT:
                    mouse_event(MOUSEEVENT_LEFTDOWN | MOUSEEVENT_LEFTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                    break;
                case ActionType.CLICK_MIDDLE:
                    mouse_event(MOUSEEVENT_MIDDLEDOWN | MOUSEEVENT_MIDDLEUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                    break;
                case ActionType.CLICK_RIGHT:
                    mouse_event(MOUSEEVENT_RIGHTDOWN | MOUSEEVENT_RIGHTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                    break;
                case ActionType.REPEAT:
                    autoRun(copyActionsList(actions, action.repeatStart, action.repeatEnd));
                    break;
            }
        }

        private static List<ActionDetails> copyActionsList(List<ActionDetails> list, int start, int end)
        {
            List<ActionDetails> ret = new List<ActionDetails>();
            for(int i = start; i <= end; i++)
            {
                ret.Add(list[i].copy());
            }
            return ret;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            ActionDetails actionDetails = getActionFromDisplay();
            actions.Insert(actionDetails.order - 1, actionDetails);

            //increment all orders above this one
            foreach (ListViewItem i in listView.Items)
            {
                if (Int32.Parse(i.Text) >= actionDetails.order)
                {
                    i.Text = $"{Int32.Parse(i.Text) + 1}";
                }
            }

            for (int i = actionDetails.order; i < actions.Count; i++)
                actions[i].order += 1;

            //make visible on ListView
            ListViewItem item = new ListViewItem(actionDetails.order.ToString());
            item.SubItems.Add(actionDetails.Type);
            item.SubItems.Add(actionDetails.durationMS.ToString());
            item.SubItems.Add(actionDetails.repeatAmount.ToString());
            listView.Items.Insert(actionDetails.order - 1, item);

            //update actions
            comboBox1_SelectedIndexChanged(null, null);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            //TODO prevent from selecting multiple unless you want to edit multilpe i guess
            ActionDetails actionDetails = getActionFromDisplay();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count != 0)
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    //decrement all following order values
                    int ii = item.Index;
                    ActionDetails actionDetails = actions[ii];
                    foreach (ListViewItem i in listView.Items)
                    {
                        if (Int32.Parse(i.Text) > actionDetails.order)
                        {
                            i.Text = $"{Int32.Parse(i.Text) - 1}";
                        }
                    }
                    
                    for (int i = actionDetails.order + 1; i < actions.Count; i++)
                        actions[i].order -= 1;

                    //acutally delete
                    listView.Items.RemoveAt(ii);
                    actions.RemoveAt(ii);
                }
            }
        }




    }
}
