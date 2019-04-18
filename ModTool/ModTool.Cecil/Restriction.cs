using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ModTool.Shared;

namespace ModTool.Cecil
{
    /// <summary>
    /// Base class for restrictions. A restriction either requires or prohibits something.
    /// </summary>
    [Serializable]
    public static class Restriction
    {
        /// <summary>
        /// Verify a member with this Restriction.
        /// </summary>
        /// <param name="member">A member.</param>
        /// <param name="excludedAssemblies">A List of Assembly names that should be ignored.</param>
        /// <returns>False if the Member fails the verification.</returns>
        public static bool Verify(RestrictionDef def, MemberReference member, List<string> excludedAssemblies)
        {
            if (Restriction.Applicable(def, member))
            {
                bool present;

                if (member is MethodReference)
                    present = PresentInMethodRecursive(member as MethodReference, excludedAssemblies);
                else
                    present = Present(member);

                if (present)
                {
                    if (def.restrictionMode == RestrictionMode.Prohibited)
                    {
                        LogMessage(def, member);
                        return false;
                    }
                }
                else if (def.restrictionMode == RestrictionMode.Required)
                {
                    LogMessage(def, member);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Is the Restriction present in the member?
        /// </summary>
        /// <param name="member">A member.</param>
        /// <returns>True if the Restriction is present in the member.</returns>
        static bool Present(MemberReference member)
        {
            return false;
        }                

        /// <summary>
        /// Is the restriction present in a local variable?
        /// </summary>
        /// <param name="variable">A local variable</param>
        /// <returns>True if the restriction is present in the local variable.</returns>
        static bool PresentMethodVariable(VariableReference variable)
        {
            return false;
        }     
                
        static bool PresentInMethodRecursive(MethodReference method, List<string> excludedAssemblies)
        {
            HashSet<string> visited = new HashSet<string>();
            return PresentInMethodRecursive(method, excludedAssemblies, visited);
        }

        static bool PresentInMethodRecursive(MethodReference method, List<string> apiAssemblies, HashSet<string> visited)
        {
            MethodDefinition resolvedMethod = null;
            
            try
            {
                resolvedMethod = method.Resolve();
            }
            catch (AssemblyResolutionException e)
            {
                LogUtility.LogWarning(e.Message);
            }

            if (resolvedMethod != null)
            {
                if (!resolvedMethod.HasBody)
                    return false;

                if (visited.Contains(resolvedMethod.FullName))
                    return false;

                visited.Add(resolvedMethod.FullName);

                foreach (VariableDefinition variable in resolvedMethod.Body.Variables)
                {
                    if (PresentMethodVariable(variable))
                        return true;
                }

                foreach (Instruction instruction in resolvedMethod.Body.Instructions)
                {
                    if (instruction.Operand == null)
                        continue;
                                        
                    if (instruction.Operand is MemberReference)
                    {
                        MemberReference member = instruction.Operand as MemberReference;
                        
                        if (member.Module.Assembly.Name.Name == "ModTool.Interface")
                            continue;

                        if (Present(member))
                            return true;

                        if (apiAssemblies.Contains(member.Module.Assembly.Name.Name))
                            continue;

                        if (member.DeclaringType == null)
                            continue;

                        if (member.DeclaringType.Namespace.StartsWith("System"))
                            continue;

                        if (member.DeclaringType.Namespace.StartsWith("Unity"))
                            continue;
                        
                        if (member is MethodReference)
                        {
                            if (PresentInMethodRecursive(member as MethodReference, apiAssemblies, visited))
                                return true;
                        }
                    }
                }
            }

            return false;
        }
                
        /// <summary>
        /// Log this Restriction's message.
        /// </summary>
        /// <param name="member">The Member to include in the message.</param>
        static void LogMessage(RestrictionDef def, MemberReference member)
        {
            LogUtility.LogWarning(def.restrictionMode + ": " + member.FullName + " - " + def.message);
        }
                
        /// <summary>
        /// Is this Restriction applicable to the member?
        /// </summary>
        /// <param name="member">A member.</param>
        /// <returns>True if the Restriction is applicable.</returns>
        static bool Applicable(RestrictionDef def, MemberReference member)
        {            
            if(member is TypeReference)
            {
                return Applicable(def, member as TypeReference);
            }

            if (member.DeclaringType == null)
                return false;

            return Applicable(def, member.DeclaringType);
        }

        /// <summary>
        /// Is this Restriction applicable to the Type?
        /// </summary>
        /// <param name="type">A Type.</param>
        /// <returns>True if the Restriction is applicable.</returns>
        static bool Applicable(RestrictionDef def, TypeReference type)
        {
            if (def.applicableBaseType == null)
                return true;

            if (string.IsNullOrEmpty(def.applicableBaseType.name))
                return true;

            try
            {
                return type.Resolve().IsSubClassOf(def.applicableBaseType);
            }
            catch (AssemblyResolutionException e)
            {
                LogUtility.LogWarning(e.Message);
            }

            return false;
        }        
    }

    /// <summary>
    /// A restriction that either requires or prohibits the use of a namespace.
    /// </summary>
    [Serializable]
    public static class NamespaceRestriction
    {
        public static bool Present(NamespaceRestrictionDef def, MemberReference member)
        {
            if (member is TypeReference)
                return CheckNamespace(def, (member as TypeReference).Namespace);
            if (member is FieldReference)
                return CheckNamespace(def, (member as FieldReference).FieldType.Namespace);
            if (member is PropertyReference)
                return CheckNamespace(def, (member as PropertyReference).PropertyType.Namespace);

            if (member.DeclaringType == null)
                return false;

            return CheckNamespace(def, member.DeclaringType.Namespace);
        }

        static bool PresentMethodVariable(NamespaceRestrictionDef def, VariableReference variable)
        {
            return CheckNamespace(def, variable.VariableType.Namespace);
        }

        static bool CheckNamespace(NamespaceRestrictionDef def, string nameSpace)
        {
            if (def.includeNested)
                return nameSpace.StartsWith(def.nameSpace);
            else
                return nameSpace == def.nameSpace;
        }
    }

    /// <summary>
    /// A restriction that either requires or prohibits the use of a Type
    /// </summary>
    [Serializable]
    public static class TypeRestriction 
    {
        public static bool Present(TypeRestrictionDef def, MemberReference member)
        {
            if (member is FieldReference)
            {
                FieldReference field = member as FieldReference;
                return field.FieldType.Name == def.type.name && field.FieldType.Namespace == def.type.nameSpace;
            }

            if (member is PropertyReference)
            {
                PropertyReference property = member as PropertyReference;
                return property.PropertyType.Name == def.type.name && property.PropertyType.Namespace == def.type.nameSpace;
            }

            if (member.DeclaringType == null)
                return false;

            return member.DeclaringType.Name == def.type.name && member.DeclaringType.Namespace == def.type.nameSpace;
        }

        static bool PresentMethodVariable(TypeRestrictionDef def, VariableReference variable)
        {
            return (variable.VariableType.Name == def.type.name && variable.VariableType.Namespace == def.type.nameSpace);
        }
    }

    /// <summary>
    /// A restriction that either requires or prohibits inheritance from a class
    /// </summary>
    [Serializable]
    public static class InheritanceRestriction
    {
 
        public static bool Present(InheritanceRestrictionDef def, MemberReference member)
        {
            if (member is TypeReference)
            {
                TypeDefinition typeDefinition = (member as TypeReference).Resolve();
                return typeDefinition.IsSubClassOf(def.type);
            }

            //TODO: What if a Type derives from Type inside Unity that derives from MonoBehaviour?

            return false;
        }
    }
    

    /// <summary>
    /// A restriction that either requires or prohibits the use of a given Type's member
    /// </summary>
    [Serializable]
    public static class MemberRestriction
    {
        public static bool Present(MemberRestrictionDef def, MemberReference member)
        {
            if (member.DeclaringType == null)
                return member.Name == def.memberName.name;

            return (member.Name == def.memberName.name && member.DeclaringType.Name == def.memberName.type.name);
        }
    }
}
