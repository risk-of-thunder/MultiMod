using System;
using System.Collections.Generic;
using UnityEngine;


namespace ModTool.Shared
{
    /// <summary>
    /// Stores settings related to the game's API and code verification.
    /// </summary>
    /// 
    public enum RestrictionMode { Prohibited, Required }

    [Serializable]
    public abstract class RestrictionDef
    {
        public string message;

        public TypeName applicableBaseType;

        public RestrictionMode restrictionMode;

        protected RestrictionDef(TypeName applicableBaseType, string message, RestrictionMode restrictionMode)
        {
            this.applicableBaseType = applicableBaseType;
            this.message = message;
            this.restrictionMode = restrictionMode;
        }
    }

    [Serializable]
    public class InheritanceRestrictionDef : RestrictionDef
    {
        public TypeName type;

        public InheritanceRestrictionDef(TypeName type, TypeName applicableBaseType, string message, RestrictionMode restrictionMode)
    : base(applicableBaseType, message, restrictionMode)
        {
            this.type = type;
        }
    }

    [Serializable]
    public class MemberRestrictionDef : RestrictionDef
    {
        /// <summary>
        /// The member that will be checked for this Restriction.
        /// </summary>
        public MemberName memberName;

        /// <summary>
        /// Initialize a new MemberRestriction.
        /// </summary>
        /// <param name="memberName">The member that will be checked for this Restriction.</param>
        /// <param name="applicableBaseType">The base Type this Restriction will apply to.</param>
        /// <param name="message">The Message that will be shown upon failing the Restriction.</param>
        /// <param name="restrictionMode">Should it be required or prohibited?</param>
        public MemberRestrictionDef(MemberName memberName, TypeName applicableBaseType, string message, RestrictionMode restrictionMode)
            : base(applicableBaseType, message, restrictionMode)
        {
            this.memberName = memberName;
        }
    }

    /// <summary>
    /// A restriction that either requires or prohibits the use of a namespace.
    /// </summary>
    [Serializable]
    public class NamespaceRestrictionDef : RestrictionDef
    {
        /// <summary>
        /// The namespace that will be checked for this restriction.
        /// </summary>
        public string nameSpace;

        /// <summary>
        /// Should nested namespaces be restricted as well?
        /// </summary>
        public bool includeNested;

        /// <summary>
        /// Initialize a new NamespaceRestriction
        /// </summary>
        /// <param name="nameSpace">The namespace that will be checked for this restriction.</param>
        /// <param name="includeNested">Should nested namespaces be restricted as well?</param>
        /// <param name="applicableBaseType">The base Type this Restriction will apply to.</param>
        /// <param name="message">The Message that will be shown upon failing the Restriction.</param>
        /// <param name="restrictionMode">Should it be required or prohibited?</param>
        public NamespaceRestrictionDef(string nameSpace, bool includeNested, TypeName applicableBaseType, string message, RestrictionMode restrictionMode) : base(applicableBaseType, message, restrictionMode)
        {
            this.nameSpace = nameSpace;
            this.includeNested = includeNested;
        }
    }

    /// <summary>
    /// A restriction that either requires or prohibits the use of a Type
    /// </summary>
    [Serializable]
    public class TypeRestrictionDef : RestrictionDef
    {
        /// <summary>
        /// The Type that will be checked for this Restriction.
        /// </summary>
        public TypeName type;

        /// <summary>
        /// Initialize a new TypeRestriction.
        /// </summary>
        /// <param name="type">The Type that will be checked for this Restriction.</param>
        /// <param name="applicableBaseType">The base Type this Restriction will apply to.</param>
        /// <param name="message">The Message that will be shown upon failing the Restriction.</param>
        /// <param name="restrictionMode">Should it be required or prohibited?</param>
        public TypeRestrictionDef(TypeName type, TypeName applicableBaseType, string message, RestrictionMode restrictionMode)
            : base(applicableBaseType, message, restrictionMode)
        {
            this.type = type;
        }
    }


    public class CodeSettings : ScriptableSingleton<CodeSettings>
    {
        /// <summary>
        /// Restrictions related to inheritance of Types inside Mod Assemblies.
        /// </summary>
        public static List<InheritanceRestrictionDef> inheritanceRestrictions
        {
            get
            {
                return instance._inheritanceRestrictions;
            }
        }

        /// <summary>
        /// Restrictions related to the use of fields, properties and methods from other types.
        /// </summary>
        public static List<MemberRestrictionDef> memberRestrictions
        {
            get
            {
                return instance._memberRestrictions;
            }
        }

        /// <summary>
        /// Restrictions related to the use of Types for fields and properties.
        /// </summary>
        public static List<TypeRestrictionDef> typeRestrictions
        {
            get
            {
                return instance._typeRestrictions;
            }
        }

        /// <summary>
        /// Restrictions related to the use of entire namespaces.
        /// </summary>
        public static List<NamespaceRestrictionDef> namespaceRestrictions
        {
            get
            {
                return instance._namespaceRestrictions;
            }
        }

        /// <summary>
        /// List of the Game's api Assembly names.
        /// </summary>
        public static List<string> apiAssemblies
        {
            get
            {
                return instance._apiAssemblies;
            }
        }

        [SerializeField]
        private List<InheritanceRestrictionDef> _inheritanceRestrictions = new List<InheritanceRestrictionDef>();

        [SerializeField]
        private List<MemberRestrictionDef> _memberRestrictions = new List<MemberRestrictionDef>();

        [SerializeField]
        private List<TypeRestrictionDef> _typeRestrictions = new List<TypeRestrictionDef>();

        [SerializeField]
        private List<NamespaceRestrictionDef> _namespaceRestrictions = new List<NamespaceRestrictionDef>();

        [SerializeField]
        private List<string> _apiAssemblies = new List<string>();
        
        protected CodeSettings()
        {
            memberRestrictions.Add(new MemberRestrictionDef(new MemberName(new TypeName("UnityEngine", "Object"), "Instantiate"), null, "Please use ModBehaviour.Instantiate or ContentHandler.Instantiate instead to ensure proper object creation.", RestrictionMode.Prohibited));
            memberRestrictions.Add(new MemberRestrictionDef(new MemberName(new TypeName("UnityEngine", "GameObject"), "AddComponent"), null, "Please use ModBehaviour.AddComponent or ContentHandler.AddComponent instead to ensure proper component handling.", RestrictionMode.Prohibited));
            memberRestrictions.Add(new MemberRestrictionDef(new MemberName(new TypeName("UnityEngine", "GameObject"), ".ctor"), null, "Creating new GameObjects is not allowed", RestrictionMode.Prohibited));

            inheritanceRestrictions.Add(new InheritanceRestrictionDef(new TypeName("ModTool.Interface", "ModBehaviour"), new TypeName("UnityEngine", "MonoBehaviour"), "Please use ModTool.Interface.ModBehaviour instead of MonoBehaviour.", RestrictionMode.Required));
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            GetInstance();
        }
    }
}
