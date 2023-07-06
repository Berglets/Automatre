using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automatre
{
    public class ActionDetails
    {
        public String Type { get { return createTypeDisplay(); } set { } } //Only for display. Changes based on other properties.

        //shared by all actions
        public ActionType action;
        public int durationMS;
        public int repeatAmount;
        public int order;
        //dependent on ActionType
        public Keys keyPress;
        public int repeatStart;
        public int repeatEnd;
        public int cursorX;
        public int cursorY;


        public enum ActionType
        {
            CLICK_RIGHT, CLICK_LEFT, CLICK_MIDDLE, KEY_PRESS, REPEAT, MOUSE_MOVE, NONE
        }

        public ActionDetails(ActionType clickAction, int durationMS, int repeatAmount)
        {
            this.action = clickAction;
            this.durationMS = durationMS;
            this.repeatAmount = repeatAmount;
        }

        public ActionDetails(Keys keyPress, int durationMS, int repeatAmount)
        {
            this.action = ActionType.KEY_PRESS;
            this.keyPress = keyPress;
            this.durationMS = durationMS;
            this.repeatAmount = repeatAmount;
        }

        public ActionDetails(int repeatStart, int repeatEnd, int durationMS, int repeatAmount)
        {
            this.action = ActionType.REPEAT;
            this.durationMS = durationMS;
            this.repeatAmount = repeatAmount;
            this.repeatStart = repeatStart;
            this.repeatEnd = repeatEnd;
        }

        public ActionDetails(ActionType mouseMove, int cursorX, int cursorY, int durationMS, int repeatAmount)
        {
            this.action = ActionType.MOUSE_MOVE;
            this.durationMS = durationMS;
            this.repeatAmount = repeatAmount;
            this.cursorX = cursorX;
            this.cursorY = cursorY;
        }

        public ActionDetails copy()
        {
            ActionDetails action = new ActionDetails(this.action, this.durationMS, this.repeatAmount);
            action.order = this.order;
            action.cursorX = this.cursorX;
            action.cursorY = this.cursorY;
            action.repeatStart = this.repeatStart;
            action.repeatEnd = this.repeatEnd;  
            action.keyPress = this.keyPress;
            return action;
        }

        private String createTypeDisplay()
        {
            switch (action)
            {
                case ActionType.CLICK_LEFT: return "Click (Left)";
                case ActionType.CLICK_MIDDLE: return "Click (Middle)";
                case ActionType.CLICK_RIGHT: return "Click (Right)";
                case ActionType.KEY_PRESS: return $"Key Press ('{keyPress}')";
                case ActionType.REPEAT: return $"Repeat ({repeatStart} - {repeatEnd})";
                case ActionType.MOUSE_MOVE: return $"Mouse Move ({cursorX}, {cursorY})";
            }
            return "None";
        }
    }
}
