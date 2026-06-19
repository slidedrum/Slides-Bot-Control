using sInputSystem;
using static sInputSystem.sInputAnchor;
using static sInputSystem.sInputSystemDefaults;

namespace BotControl.smenu
{
    internal class sMenuInputs
    {
        public static sSequenceDefinition OnHeld
        {
            get
            {
                sInputDefinition[] Inputs = [Press.Slot(0), LongPress.Slot(1)];
                sSequenceDefinition sequence = new(
                    inputs: Inputs,
                    Callback: null,
                    Identifier: "HoverOnHeld",
                    autoAnchor: false,
                    RisingEdgeOnly: false,
                    ExclusiveSelf: false
                    );
                Inputs[1].AddAnchor(new sInputAnchor(Inputs[0], Anchorpoint.Start, Anchorpoint.Inside));
                Inputs[1].AddAnchor(new sInputAnchor(Inputs[0], Anchorpoint.Start, Anchorpoint.Inside, ThisStart: TapThreshold, ThisEnd: TapThreshold));
                return sequence;
            }
        }
    }
}
