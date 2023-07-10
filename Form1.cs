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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Automatre
{
    public partial class Form1 : Form
    {
        private bool autoRunning = false;
        private bool paused = false;
        private Thread autoThread = null;

        private List<ActionDetails> actions = new List<ActionDetails>();

        private bool recordingMouseCoordinates = true;
        private bool updating = false;

        private bool listenKeyMouseCoords = false;
        private bool listenKeyStart = false;
        private bool listenKeyPause = false;
        private bool listenKeyAutoPress = false;
        
        private Keys selectedKeyAutoPress = Keys.A;

        private Keys selectedKeyMouseCoords = Keys.F2;
        private Keys selectedKeyStart = Keys.F4;
        private Keys selectedKeyPause = Keys.F3;

        const int HOTKEY_ID_START = 1;
        const int HOTKEY_ID_PAUSE = 2;
        const int HOTKEY_ID_MOUSECOORDTOGGLE = 3;

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
            RegisterHotKey(this.Handle, HOTKEY_ID_START, 0, (int) selectedKeyStart);
            RegisterHotKey(this.Handle, HOTKEY_ID_PAUSE, 0, (int) selectedKeyPause);
            RegisterHotKey(this.Handle, HOTKEY_ID_MOUSECOORDTOGGLE, 0, (int) selectedKeyMouseCoords);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Text = "Click";
            comboBox3.Text = "Left Click";
            label8.Text = "Key: " + selectedKeyAutoPress;

            label11.Text = "Press " + selectedKeyStart + " to Start/Stop";
            label22.Text = "Press " + selectedKeyPause + " to Pause/Unpause";
            label1.Text = "Press " + selectedKeyMouseCoords + " to Toggle";
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
            if(autoRunning && !autoThread.IsAlive)
            {
                autoRunning = false;
                autoThread = null;

                buttonStart.Text = "Start";
                buttonPause.Enabled = false;
                addButton.Enabled = true;
            }
        }

        //only process when on form
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (listenKeyMouseCoords)
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID_MOUSECOORDTOGGLE);
                selectedKeyMouseCoords = keyData;
                RegisterHotKey(this.Handle, HOTKEY_ID_MOUSECOORDTOGGLE, 0, (int)selectedKeyMouseCoords);

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
                UnregisterHotKey(this.Handle, HOTKEY_ID_START);
                selectedKeyStart = keyData;
                RegisterHotKey(this.Handle, HOTKEY_ID_START, 0, (int)selectedKeyStart);

                buttonStartHotkey.Enabled = true;
                label11.Text = "Press '" + selectedKeyStart.ToString() + "' to Start/Stop";
                listenKeyStart = false;
            }

            if (listenKeyPause)
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID_PAUSE);
                selectedKeyPause = keyData;
                RegisterHotKey(this.Handle, HOTKEY_ID_START, 0, (int)selectedKeyPause);

                buttonPauseHotkey.Enabled = true;
                label22.Text = "Press '" + selectedKeyPause.ToString() + "' to Pause";
                listenKeyPause = false;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        //handle key presses outside of form
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID_START)
            {
                buttonStart_Click(null, null);
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID_PAUSE)
            {
                if (buttonPause.Enabled == true)
                {
                    buttonPause_Click(null, null);
                }
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID_MOUSECOORDTOGGLE)
            {
                recordingMouseCoordinates = recordingMouseCoordinates ? false : true;
            }


            base.WndProc(ref m);
        }

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
                    action.order = (int)orderClick.Value;
                    break;
                case "Key Press":
                    action = new ActionDetails(selectedKeyAutoPress, (int)numericUpDown2.Value, (int)numericUpDown1.Value);
                    action.order = (int)orderKeyPress.Value;
                    break;
                case "Move Mouse":
                    action = new ActionDetails(ActionType.MOUSE_MOVE, (int)numericUpDown11.Value, (int)numericUpDown10.Value, (int)numericUpDown9.Value, (int)numericUpDown8.Value);
                    action.order = (int)orderMouseMove.Value;
                    break;
                case "Repeat":
                    action = new ActionDetails((int)numericUpDown12.Value, (int)numericUpDown13.Value, (int)numericUpDown16.Value, (int)numericUpDown15.Value);
                    action.order = (int)orderRepeat.Value;
                    break;
            }
            return action;
        }

        private void displayPanel(ActionType type)
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;

            switch (type)
            {
                case ActionType.REPEAT: 
                    panel4.Visible = true;
                    break;
                case ActionType.MOUSE_MOVE: 
                    panel3.Visible = true;
                    break;
                case ActionType.KEY_PRESS: 
                    panel2.Visible = true;
                    break;
                default: 
                    panel1.Visible = true;
                    break;
            }
            updateMaximums();
        }

        private void updateMaximums()
        {
            int max = listView.Items.Count + 1;
            orderRepeat.Maximum = max;
            orderMouseMove.Maximum = max;
            orderKeyPress.Maximum = max;
            orderClick.Maximum = max;
        }

        private void displayAction(ActionDetails details)
        {
            displayPanel(details.action);

            switch (details.action)
            {
                case ActionType.KEY_PRESS:
                    comboBox1.Text = "Key Press";
                    numericUpDown2.Value = details.durationMS;
                    numericUpDown1.Value = details.repeatAmount;
                    orderKeyPress.Value = details.order;
                    label8.Text = "Key: " + details.keyPress;
                    selectedKeyAutoPress = details.keyPress;
                    break;
                case ActionType.MOUSE_MOVE:
                    comboBox1.Text = "Move Mouse";
                    numericUpDown11.Value = details.cursorX;
                    numericUpDown10.Value = details.cursorY;
                    numericUpDown9.Value = details.durationMS;
                    numericUpDown8.Value = details.repeatAmount;
                    orderMouseMove.Value = details.order;
                    break;
                case ActionType.REPEAT:
                    comboBox1.Text = "Repeat";
                    numericUpDown12.Value = details.repeatStart;
                    numericUpDown13.Value = details.repeatEnd;
                    numericUpDown16.Value = details.durationMS;
                    numericUpDown15.Value = details.repeatAmount;
                    orderRepeat.Value = details.order;
                    break;
                default: //any click
                    comboBox1.Text = "Click";
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
                    orderClick.Value = details.order;
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.Text)
            {
                case "Key Press": displayPanel(ActionType.KEY_PRESS); break;
                case "Move Mouse": displayPanel(ActionType.MOUSE_MOVE); break;
                case "Repeat": displayPanel(ActionType.REPEAT); break;
                default: displayPanel(ActionType.CLICK_RIGHT); break;
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count <= 0 || autoRunning)
            {
                buttonEdit.Enabled = false;
                buttonRemove.Enabled = false;
            } 
            else
            {
                buttonEdit.Enabled = true;
                buttonRemove.Enabled = true;
                int i = listView.SelectedItems[0].Index;
                displayAction(actions[i]);
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

        private void buttonPauseHotkey_Click(object sender, EventArgs e)
        {
            buttonPauseHotkey.Enabled = false;
            listenKeyPause = true;
            label22.Text = "Press Any Key to Change";
        }

        private static void autoRun(object actionsToPerform)
        {
            List<ActionDetails> actions = (List<ActionDetails>)actionsToPerform;
            
            for(int i = 0; i < actions.Count; i++)
            {
                ActionDetails action = actions[i];
                for (int j = 0; j < action.repeatAmount; j++)
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
                    autoRun(copyActionsList(actions, action.repeatStart - 1, action.repeatEnd - 1));
                    break;
                case ActionType.KEY_PRESS:
                    SendKeys.SendWait(action.keyPress.ToString());
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

            //increment order above all after this order
            for (int i = actionDetails.order - 1; i < actions.Count; i++)
            {
                actions[i].order += 1;
                listView.Items[i].Text = $"{actions[i].order}";
            }

            actions.Insert(actionDetails.order - 1, actionDetails);

            //make visible on ListView
            ListViewItem item = new ListViewItem(actionDetails.order.ToString());
            item.SubItems.Add(actionDetails.Type);
            item.SubItems.Add(actionDetails.durationMS.ToString());
            item.SubItems.Add(actionDetails.repeatAmount.ToString());
            listView.Items.Insert(actionDetails.order - 1, item);

            updateMaximums();
            updateOrdersToMatch(actionDetails.order + 1);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count != 0)
            {
                ActionDetails actionDetails = getActionFromDisplay();

                foreach (ListViewItem item in listView.SelectedItems)
                {
                    removeItem(item.Index);
                    displayAction(actionDetails);
                    addButton_Click(null, null);

                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count != 0)
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    removeItem(item.Index);
                }
            }
        }
        
        private void removeItem(int indexRemove)
        {
            //acutally delete
            listView.Items.RemoveAt(indexRemove);
            actions.RemoveAt(indexRemove);

            //decrement all following order values
            for (int i = indexRemove; i < actions.Count; i++)
            {
                actions[i].order -= 1;
                listView.Items[i].Text = $"{actions[i].order}";
            }

            updateMaximums();
        }

        private void updateOrdersToMatch(int val)
        {
            if (!updating)
            {
                updating = true;
                orderClick.Value = val;
                orderRepeat.Value = val;
                orderMouseMove.Value = val;
                orderKeyPress.Value = val;
                updating = false;
            }
        }

        private void orderClick_ValueChanged(object sender, EventArgs e)
        {
            updateOrdersToMatch((int)orderClick.Value);
        }

        private void orderRepeat_ValueChanged(object sender, EventArgs e)
        {
            updateOrdersToMatch((int)orderRepeat.Value);
        }

        private void orderMouseMove_ValueChanged(object sender, EventArgs e)
        {
            updateOrdersToMatch((int)orderMouseMove.Value);
        }

        private void orderKeyPress_ValueChanged(object sender, EventArgs e)
        {
            updateOrdersToMatch((int)orderKeyPress.Value);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (autoRunning)
            {
                autoThread.Abort();
            }
            else
            {
                buttonStart.Text = "Stop";
                listView.SelectedItems.Clear();
                addButton.Enabled = false;
                buttonEdit.Enabled = false;
                buttonRemove.Enabled = false;
                buttonPause.Enabled = true;

                Thread thread = new Thread(autoRun);
                thread.IsBackground = true;
                thread.Start(copyActionsList(actions, 0, actions.Count - 1));
                autoThread = thread;
                autoRunning = true;
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if(autoRunning && !paused)
            {
                autoThread.Suspend();
                paused = true;
                buttonPause.Text = "Unpause";
            } else
            {
                autoThread.Resume();
                paused = false;
                buttonPause.Text = "Pause";
            }
        }
    }
}
