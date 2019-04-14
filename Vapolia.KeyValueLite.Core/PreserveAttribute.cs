using System;
using Vapolia.KeyValueLite;

namespace Vapolia.KeyValueLite
{
    [AttributeUsage(System.AttributeTargets.All)]
    internal sealed class PreserveAttribute : Attribute
    {
        public PreserveAttribute() { }
        public bool Conditional { get; set; }
        public bool AllMembers { get; set;}
    }
}

