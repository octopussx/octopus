
// UI helpers by u/Alazii
using System.Collections.Generic;

namespace octopussy
{
    using UnityEngine;
    using UnityEngine.UI;

    internal class UIH
    {
        readonly MVRScript script;

        public UIH(MVRScript script)
        {
            this.script = script;
        }


        public void FloatSlider(ref JSONStorableFloat output, string name, float start,
            JSONStorableFloat.SetFloatCallback callback, float min, float max, bool right=false, bool constraint=false)
        {
            output = new JSONStorableFloat(name, start, callback, min, max, constraint, true)
                {storeType = JSONStorableParam.StoreType.Full};
            script.RegisterFloat(output);
            script.CreateSlider(output, right);
        }

        public void BoolCheckbox(ref JSONStorableBool output, string name, bool start,
            JSONStorableBool.SetBoolCallback callback=null, bool right=false)
        {
            output = new JSONStorableBool(name, start, callback);
            script.RegisterBool(output);
            script.CreateToggle(output, right);
        }
        public void UIStringMessage(string message, bool right=false)
        {
            script.CreateTextField(new JSONStorableString("", message), right=false);
        }
        public void StringTextbox(ref JSONStorableString output, string name, string start,
            JSONStorableString.SetStringCallback callback, bool right=false)
        {
            output = new JSONStorableString(name, start, callback);

            script.RegisterString(output);
            var textfield = script.CreateTextField(output, right);
            var input = textfield.gameObject.AddComponent<InputField>();
            input.textComponent = textfield.UItext;
            textfield.backgroundColor = Color.white;
            output.inputField = input;
        }
        public void Button(string label, UnityEngine.Events.UnityAction handler, bool right=false)
        {
            script.CreateButton(label, right).button.onClick.AddListener(handler);
        }

        public static void Button(MVRScript script, string label, UnityEngine.Events.UnityAction handler, bool right = false)
        {
            script.CreateButton(label, right).button.onClick.AddListener(handler);
        }

        private void ColorPicker(List<Material> mat, string shaderParamName, string displayname = null)
        {
            if (displayname == null) displayname = shaderParamName;
            var picker = new JSONStorableColor(displayname, HSVColorPicker.RGBToHSV(1f, 1f, 1f),
                c => mat.ForEach(m => m.SetColor(shaderParamName, c.colorPicker.currentColor))
            );
            script.RegisterColor(picker);
            script.CreateColorPicker(picker, true);
        }

        private void FloatSlider(List<Material> mat, float startvalue, float minval, float maxval, string shaderParamName, string displayname = null)
        {
            if (displayname == null) displayname = shaderParamName;
            var jf = new JSONStorableFloat(
                displayname, startvalue,
                f => mat.ForEach(m => m.SetFloat(shaderParamName, f)),
                minval, maxval, true);

            script.RegisterFloat(jf);
            script.CreateSlider(jf);
        }
    }
}
