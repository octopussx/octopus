using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/*
 Octopussy selectors
 One liners to select items and perform callbacks
 usage : only needs a call to the constructors
 [19-12-03]
    example : from your own plugin :
    AtomStorableSelector(this, (string s)=>{callback})
    FloatSelector(this, this.insideRestore,(s)=>{callback})
    ActionSelector(this, callback, "action")
*/
namespace octopussy
{
    public delegate void CallbackAction(string id);

    public class Callbacks
    {
        readonly List<CallbackAction> _callbacks;

        public Callbacks(CallbackAction f)
        {
            _callbacks = new List<CallbackAction>();
            if (f != null) Add(f);
        }

        public void Add(CallbackAction f)
        {
            _callbacks.Add(f);
        }

        protected void callbacks(string x)
        {
            _callbacks.ForEach(f => f(x));
        }
    }

    public class AtomSelector : Callbacks
    {
        public JSONStorableStringChooser atomChoices;
        public Atom atom;
        public UIDynamicPopup popup;
        public string filter;

        public List<string> atomsUIDList
        {
            get
            {
                var A = new List<string> { "None" };
                if (string.IsNullOrEmpty(filter))
                {
                    A.AddRange(SuperController.singleton.GetAtomUIDs());
                }
                else
                {
                    SuperController.singleton.GetAtoms().Where(
                        s => s.type.StartsWith(filter)).ToList()
                        .ForEach(a => A.Add(a.uid));
                }
                return A;
            }
        }


        protected void SyncAtomChoices()
        {
            atomChoices.choices = atomsUIDList;
        }

        List<string> atoms = new List<string>();

        public void SyncAtom(string atomUID)
        {
            if (atomUID == "None")
            {
                atom = null;
                //this.callback(null);
            }
            else
            {
                atom = SuperController.singleton.GetAtomByUid(atomUID);
                base.callbacks(atomUID);
            }
        }

        public AtomSelector(MVRScript script, CallbackAction call, string name, bool right = false) : base(call)
        {
            Init(script, call, "", "None", name, right);
        }
        /*public AtomSelector(MVRScript script, CallbackAction call, bool right= false) : base(call)
        {
            Init(script, call, "", "None", "Atom", right);
        }*/
        /*public AtomSelector(MVRScript script, string filter, CallbackAction call, string startVal = "None", bool right=false) : base(call)
        {
            Init(script, call, filter, startVal, "atom", right);
        }*/

        public AtomSelector(MVRScript script, string filter, CallbackAction call, string startVal = "None", bool right = false, string name="atom") : base(call)
        {
            Init(script, call, filter, startVal, name, right);
        }

        public AtomSelector(MVRScript script, CallbackAction call, string startVal = "None") : base(call)
        {
            Init(script, call, "", startVal);
        }


        public void setCallback(CallbackAction call)
        {
            //throw new System.Exception("todo");
        }

        public void Init(MVRScript script, CallbackAction call, string filter = "",
            string startVal = "None", string name = "atom", bool right = false)
        {
            this.filter = filter;
            atomChoices = new JSONStorableStringChooser(name, atomsUIDList, startVal, name, SyncAtom);
            script.RegisterStringChooser(atomChoices);
            popup = script.CreateScrollablePopup(atomChoices, right);
            popup.popupPanelHeight = System.String.IsNullOrEmpty(filter) ? 800f : 250f;

            popup.popup.onOpenPopupHandlers += SyncAtomChoices;
        }
    }

    public class AtomStorableSelector : Callbacks
    {
        readonly AtomSelector atomSelected;
        public JSONStorable storable;
        protected JSONStorableStringChooser storableChooser;

        protected void SyncAtom(string uid)
        {
            List<string> storables = new List<string>();
            if (atomSelected.atom != null)
            {
                storables.AddRange(atomSelected.atom.GetStorableIDs());
            }
            storableChooser.choices = storables;
            storableChooser.val = "None";
        }

        protected void SyncStorable(string storableID)
        {
            if (atomSelected.atom != null && storableID != null && storableID != "None")
            {
                storable = atomSelected.atom.GetStorableByID(storableID);
                base.callbacks(storableID);
            }
            else
            {
                base.callbacks(null);
            }
        }

        public AtomStorableSelector(
            MVRScript plug, CallbackAction call, string startAtom = "None") : base(call)
        {
            try
            {
                atomSelected = new AtomSelector(plug, this.SyncAtom, startAtom);
                storableChooser = new JSONStorableStringChooser("element", null, null, "Element", SyncStorable);
                plug.RegisterStringChooser(storableChooser);
                UIDynamicPopup dp = plug.CreateScrollablePopup(storableChooser);
                dp.popupPanelHeight = 960f;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public void Sync(string atomUID)
        {
            atomSelected.SyncAtom(atomUID);
        }

    }

    public class FloatSelector
    {
        public AtomStorableSelector storableSelected;
        public JSONStorableFloat floatTarget;
        protected JSONStorableStringChooser floatChooser;
        protected JSONStorableFloat currentValue;
        readonly UIDynamicSlider currentValueSlider;
        readonly bool insideRestore;

        protected void SyncStorable(string storableID)
        {
            try
            {
                var floatTargetChoices = new List<string>();
                if (storableSelected.storable != null)
                {
                    floatTargetChoices.Add("None");
                    floatTargetChoices.AddRange(storableSelected.storable.GetFloatParamNames());
                    floatChooser.choices = floatTargetChoices;
                    floatChooser.val = "None";
                }
                SuperController.LogMessage("slider false...");
                currentValueSlider.gameObject.SetActive(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        protected void SyncfloatTarget(string floatTargetName)
        {
            floatTarget = null;
            if (storableSelected.storable != null && floatTargetName != null && floatTargetName != "None")
            {
                floatTarget = storableSelected.storable.GetFloatJSONParam(floatTargetName);
                if (floatTarget != null)
                {
                    SuperController.LogMessage(">> " + floatTarget.val);
                    if (!insideRestore)
                    {
                        currentValue.val = floatTarget.val;
                    }
                    currentValue.min = floatTarget.min;
                    currentValue.max = floatTarget.max;

                    currentValueSlider.gameObject.SetActive(true);
                }
            }
            else
            {
                floatTarget = null;
                currentValueSlider.gameObject.SetActive(false);
            }
        }

        protected void setValue(float f)
        {
            floatTarget?.SetVal(currentValue.val);
        }


        public FloatSelector(
            MVRScript plug, bool insideRestore,
            CallbackAction call,
            string startAtom = null,
            AtomStorableSelector _storableSelected = null)
        {
            try
            {
                currentValue = new JSONStorableFloat("current value", 0f, setValue, -1f, 1f, false);
                plug.RegisterFloat(currentValue);

                currentValueSlider = plug.CreateSlider(currentValue, true);
                currentValueSlider.gameObject.SetActive(false);

                this.insideRestore = insideRestore;
                if (_storableSelected == null)
                    storableSelected = new AtomStorableSelector(plug, SyncStorable, startAtom);
                else
                {
                    storableSelected = _storableSelected;
                    storableSelected.Add(SyncStorable);
                }

                floatChooser = new JSONStorableStringChooser("floatTarget", null, null, "Value", SyncfloatTarget);
                UIDynamicPopup dp = plug.CreateScrollablePopup(floatChooser);
                dp.popupPanelHeight = 820f;
                plug.RegisterStringChooser(floatChooser);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public void Sync(string atomuid)
        {
            storableSelected.Sync(atomuid);
        }
    }

    public class ActionSelector : Callbacks
    {
        protected JSONStorableStringChooser actionChooser;
        string actionName;

        readonly AtomStorableSelector storableSelected;
        readonly UIDynamicButton actionButton;
        readonly JSONStorableBool skipSaveRestore;
        static readonly string[] skipedAction = new string[2] { "Save", "Restore" };

        //UIDynamicToggle skipSaveRestoreToggle;

        protected void SyncStorable(string storableID)
        {
            try
            {
                var actionTargetChoices = new List<string> { "None" };
                if (storableSelected.storable != null)
                {
                    if (skipSaveRestore.val)
                    {
                        SuperController.LogMessage("skip..");
                        actionTargetChoices = storableSelected.storable.GetActionNames()
                            .Where(action => !skipedAction.Any(action.StartsWith)).ToList();
                    }
                    else
                    {
                        actionTargetChoices.AddRange(storableSelected.storable.GetActionNames());
                    }
                    actionChooser.choices = actionTargetChoices;
                    actionChooser.val = "None";
                }
                actionButton.gameObject.SetActive(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        protected void SyncActionTarget(string actionTargetName)
        {
            actionName = null;
            if (storableSelected.storable != null && actionTargetName != null && actionTargetName != "None")
            {
                actionName = actionTargetName;
                actionButton.gameObject.SetActive(true);
                actionButton.button.enabled = true;
            }
            else
            {
                actionName = null;
                actionButton.gameObject.SetActive(false);
                actionButton.button.enabled = false;
            }
            actionButton.buttonText.text = actionName;

        }

        public void CallAction()
        {
            storableSelected.storable.CallAction(actionName);
            base.callbacks(actionName);
        }


        public ActionSelector(
            MVRScript plug,
            CallbackAction call = null, string name = "action",
            string startAtom = null,
            AtomStorableSelector _storableSelected = null) : base(call)
        {
            try
            {
                actionButton = plug.CreateButton(null, true);
                actionButton.button.onClick.AddListener(CallAction);
                //actionButton.button.gameObject.SetActive(false);
                actionButton.button.enabled = false;

                skipSaveRestore = new JSONStorableBool("skip saverestore", false);
                plug.RegisterBool(skipSaveRestore);
                if (_storableSelected == null)
                    storableSelected = new AtomStorableSelector(plug, SyncStorable, startAtom);
                else
                {
                    SuperController.LogMessage("...");
                    storableSelected = _storableSelected;
                    storableSelected.Add(SyncStorable);
                }

                actionChooser = new JSONStorableStringChooser(name,
                    null, null, name, SyncActionTarget);
                plug.RegisterStringChooser(actionChooser);
                UIDynamicPopup dp = plug.CreateScrollablePopup(actionChooser);
                dp.popupPanelHeight = 820f;

                plug.CreateToggle(skipSaveRestore).toggle.onValueChanged.AddListener(
                    b => {
                        if (storableSelected.storable != null && storableSelected.storable.name != "None")
                        {
                            SyncStorable(storableSelected.storable.name);
                        }
                    });
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public void Sync(string atomuid)
        {
            storableSelected.Sync(atomuid);
        }
    }
}
