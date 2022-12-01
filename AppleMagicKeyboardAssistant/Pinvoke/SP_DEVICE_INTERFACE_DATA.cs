using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SP_DEVICE_INTERFACE_DATA
    {
        public Int32 CbSize;
        public Guid InterfaceClassGuid;
        public Int32 Flags;
        public UIntPtr Reserved;
    }
}
