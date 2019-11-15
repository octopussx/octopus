/*
 * EyesControls
 * enable eyes rolling over the default values,
 * and following speed
 * changing the lookat from person to target will reset the value.
 *
 * [19-11-14]
 */
namespace octopussy
{
    class EyesControls:MVRScript
    {
        private JSONStorableFloat
            RMaxUp,LMaxUp,
            RMaxDown,LMaxDown,
            /*RMaxRight,LMaxRight,
            RMaxLeft,LMaxLeft,*/
            RfollowSpeed,
            LfollowSpeed;

        private JSONStorableBool REnabled, LEnabled, syncUpDownEnabled, syncLeftRightEnabled;

        public override void Init()
        {
            const float maxangle = 50;
            const float defaultangle = 30;
            UIH ui = new UIH(this);
            Applies.To(this,"Person");
            LookAtWithLimits[] lookatwithlimits =containingAtom.gameObject.GetComponentsInChildren<LookAtWithLimits>();
            var L = lookatwithlimits[0];
            var R = lookatwithlimits[1];

            ui.BoolCheckbox(ref LEnabled, "Right enabled", true, b => L.enabled = b, true);
            ui.BoolCheckbox(ref REnabled, "Left enabled",true, b => R.enabled=b, false);

            ui.FloatSlider(ref LfollowSpeed, "Left followSpeed", defaultangle, f => setLRValue(f, ref R.smoothFactor, ref RfollowSpeed), 5, maxangle,false,true);
            ui.FloatSlider(ref RfollowSpeed, "Right followSpeed", defaultangle, f => setLRValue(f,ref L.smoothFactor, ref LfollowSpeed), 5, maxangle,true,true);
            
            ui.FloatSlider(ref LMaxUp, "Left max up", defaultangle, f => setValues(f, ref L.MaxUp, ref RMaxUp, ref LMaxDown, ref RMaxDown), 5, maxangle,false,true);
            ui.FloatSlider(ref RMaxUp, "Right max up", defaultangle, f => setValues(f, ref R.MaxUp, ref LMaxUp, ref RMaxDown, ref LMaxDown), 5, maxangle,true,true);
            ui.FloatSlider(ref LMaxDown, "Left max down", defaultangle, f => setValues(f, ref L.MaxDown, ref RMaxDown, ref LMaxUp, ref RMaxUp), 5, maxangle,false,true);
            ui.FloatSlider(ref RMaxDown, "Right max down", defaultangle, f => setValues(f, ref R.MaxDown, ref LMaxDown, ref RMaxUp, ref LMaxUp), 5, maxangle,true,true);

            ui.BoolCheckbox(ref syncLeftRightEnabled,"sync Left and Right",true);
            ui.BoolCheckbox(ref syncUpDownEnabled,"sync Up and Down",true);
            
            /* use this if you ever need to force disabling,
             e.g. if switching from Person To Target in the Auto Behaviour. But preferably manage the gaze target.
             JSONStorableAction ForceDisabling = new JSONStorableAction("force disabling", () => { R.enabled = L.enabled = false; });
            RegisterAction(ForceDisabling);
            */
        }
        
        public void setLRValue(float value, ref float f1, ref JSONStorableFloat f2)
        {
            f1 = value;
            if (syncLeftRightEnabled.val)
            {
                f2.val = value;
            }
        }

        public void setLRValue(float value, ref JSONStorableFloat f1, ref JSONStorableFloat f2)
        {
            f1.val = value;
            if (syncLeftRightEnabled.val)
            {
                f2.val = value;
            }
        }

        public void setValues(float value, ref float f1, ref JSONStorableFloat f2, ref JSONStorableFloat f3, ref JSONStorableFloat f4)
        {
            setLRValue(value, ref f1, ref f2);
            if (syncUpDownEnabled.val)
            {
                setLRValue(value, ref f3, ref f4);
            }
        }
    }
}
