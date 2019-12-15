using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkNode
{
    public class RoutingInfo
    {
        public List<RouterLabel> routerLabels;
        public List<RouterDeletedLabel> routerDeletedLabels;
        public List<RouterAction> routerActions;


        public RoutingInfo()
        {
            routerLabels = new List<RouterLabel>();
            routerDeletedLabels = new List<RouterDeletedLabel>();
            routerActions = new List<RouterAction>();
        }
    }

    public class RouterLabel
    {
        public int inputPort;
        public int label;
        public int action;
        public int labelsStackId;

        public RouterLabel(int inputPort, int label, int action, int labelsStackId)
        {
            this.inputPort = inputPort;
            this.label = label;
            this.action = action;
            this.labelsStackId = labelsStackId;
        }
    }

    public class RouterDeletedLabel
    {
        public int labelStack;
        public int labelsStackId;

        public RouterDeletedLabel(int labelStack, int labelsStackId)
        {
            this.labelStack = labelStack;
            this.labelsStackId = labelsStackId;
        }
    }

    public class RouterAction
    {
        public int actionId;
        public string actionString;
        public int outLabel;
        public int outPort;
        public int nextActionId;

        public RouterAction(int actionId, string action, string outLabel, string outPort, string nextActionId)
        {
            this.actionId = actionId;
            this.actionString = action;
            this.outLabel = outLabel.Equals("-") ? 0 : Int32.Parse(outLabel); ;
            this.outPort = outPort.Equals("-") ? 0 : Int32.Parse(outPort); ;
            this.nextActionId = nextActionId.Equals("-") ? 0 : Int32.Parse(nextActionId);
        }
    }


    enum Actions
    {
        SWAP,
        PUSH,
        POP
    }
}
