using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        public readonly bool ImplementModelInterface;
        public EntityAttribute(bool implementModelInterface = false)
        {
            ImplementModelInterface = implementModelInterface;
        }
    }
}