// all the resolve functions for the weaver
// NOTE: these functions should be made extensions, but right now they still
//       make heavy use of Weaver.fail and we'd have to check each one's return
//       value for null otherwise.
//       (original FieldType.Resolve returns null if not found too, so
//        exceptions would be a bit inconsistent here)
using Mono.Cecil;

namespace Mirror.Weaver
{
    public static class Resolvers
    {
        public static MethodReference ResolveMethod(TypeReference tr, AssemblyDefinition scriptDef, string name)
        {
            //Console.WriteLine("ResolveMethod " + t.ToString () + " " + name);
            if (tr == null)
            {
                Log.Error("Type missing for " + name);
                Weaver.fail = true;
                return null;
            }
            foreach (MethodDefinition methodRef in tr.Resolve().Methods)
            {
                if (methodRef.Name == name)
                {
                    return scriptDef.MainModule.ImportReference(methodRef);
                }
            }
            Log.Error("ResolveMethod failed " + tr.Name + "::" + name + " " + tr.Resolve());

            // why did it fail!?
            foreach (MethodDefinition methodRef in tr.Resolve().Methods)
            {
                Log.Error("Method " + methodRef.Name);
            }

            Weaver.fail = true;
            return null;
        }

        // TODO reuse ResolveMethod in here after Weaver.fail was removed
        public static MethodReference ResolveMethodInParents(TypeReference tr, AssemblyDefinition scriptDef, string name)
        {
            if (tr == null)
            {
                Log.Error("Type missing for " + name);
                Weaver.fail = true;
                return null;
            }
            foreach (MethodDefinition methodRef in tr.Resolve().Methods)
            {
                if (methodRef.Name == name)
                {
                    return scriptDef.MainModule.ImportReference(methodRef);
                }
            }
            // Could not find the method in this class,  try the parent
            return ResolveMethodInParents(tr.Resolve().BaseType, scriptDef, name);
        }

        // System.Byte[] arguments need a version with a string
        public static MethodReference ResolveMethodWithArg(TypeReference tr, AssemblyDefinition scriptDef, string name, string argTypeFullName)
        {
            foreach (var methodRef in tr.Resolve().Methods)
            {
                if (methodRef.Name == name)
                {
                    if (methodRef.Parameters.Count == 1)
                    {
                        if (methodRef.Parameters[0].ParameterType.FullName == argTypeFullName)
                        {
                            return scriptDef.MainModule.ImportReference(methodRef);
                        }
                    }
                }
            }
            Log.Error("ResolveMethodWithArg failed " + tr.Name + "::" + name + " " + argTypeFullName);
            Weaver.fail = true;
            return null;
        }

        // reuse ResolveMethodWithArg string version
        public static MethodReference ResolveMethodWithArg(TypeReference tr, AssemblyDefinition scriptDef, string name, TypeReference argType)
        {
            return ResolveMethodWithArg(tr, scriptDef, name, argType.FullName);
        }
    }
}