namespace RappiSharp.VirtualMachine.Descriptors
{
    internal sealed class ClassDescriptor : TypeDescriptor
    {
        public TypeDescriptor[] FieldTypes { get; set; }

        public ClassDescriptor[] BaseTypes { get; set; }

        public MethodDescriptor[] VirtualTable { get; set; }

        public ClassDescriptor Base
        {
            get
            {
                return Level == 0 ? null : BaseTypes[Level - 1];
            }
        }

        public int Level { get; set; }

        public int[] FieldOffsets { get; }
        public int TotalFieldSize { get; }
    }
}
