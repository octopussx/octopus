/*
 * syntactic sugar to indicate the type a plugin expect
 */

namespace octopussy
{
    internal class Applies
    {
        // by object
        public static T To<T>(MVRScript script, bool silently = false) where T : JSONStorable
        {
            T atom = script.containingAtom.GetStorableByID(script.containingAtom.type) as T;
            if (atom == null && !silently)
            {
                SuperController.LogError(script.name + " is not applicable to " + script.containingAtom.type);
                /*throw new System.Exception( script.name + " is not applicable to " + script.containingAtom.type);*/
            }
            return atom;
        }

        // by type name
        public static bool To(MVRScript script, string type, bool silently = false)
        {
            bool valid = type == script.containingAtom.type;
            if (!valid && !silently)
            {
                SuperController.LogError(script.name + " is not applicable to " + script.containingAtom.type);
                /*throw new System.Exception( script.name + " is not applicable to " + script.containingAtom.type);*/
            }
            return valid;
        }
    }
}
