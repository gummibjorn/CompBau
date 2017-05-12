using RappiSharp.VirtualMachine.Descriptors;
using RappiSharp.VirtualMachine.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RappiSharp.VirtualMachine.Runtime
{
    class RawHeap
    {

        IntPtr _heap;
        IntPtr _freePtr;
        IntPtr _limit;
        private readonly int ALIGNMENT = 8;

        int _typeDescriptorIndex = 0;
        Dictionary<TypeDescriptor, int> _descToPtr = new Dictionary<TypeDescriptor, int>();

        public RawHeap(int nofBytes)
        {
            _heap = Marshal.AllocHGlobal(nofBytes);
            _freePtr = _heap; //???
            _limit = _heap + nofBytes;
        }

        public IntPtr Allocate(ArrayDescriptor type, int length)
        {
            var elementSize = ALIGNMENT; //TODO
            int size = 24 + elementSize * length;
            //int size = 24 + Math.Round(size, ALIGNMENT);
            if(((int)_freePtr + size) > (int)_limit)
            {
                throw new VMException("Out of Mana");
            }
            IntPtr address = _freePtr;
            _freePtr += size;
            Marshal.WriteInt64(_heap, 0, size);
            int typeTag = MapToId(type);
            Marshal.WriteInt64(_heap, 8, typeTag);
            Marshal.WriteInt64(_heap, 16, length);
            address += 24;
            for(var i  = 0; i < length; i++)
            {
                Marshal.WriteInt64(_heap, i * ALIGNMENT, 0); //TODO make a default value
            }
            return address;
        }

        IntPtr Allocate(ClassDescriptor type)
        {
            //int size = 24 + Math.Round(type.TotalFieldSize, ALIGNMENT);
            int size = 0;
            if(((int)_freePtr + size) > (int)_limit)
            {
                throw new VMException("Out of Mana");
            }
            IntPtr address = _freePtr;
            _freePtr += size;
            Marshal.WriteInt64(_heap, (int)address, size);
            int typeTag = MapToId(type);
            Marshal.WriteInt64(_heap, (int)address+8, typeTag);
            address += 24;
            Initialize(address, type);
            return address;
        }

        public TypeDescriptor GetType(IntPtr ptr)
        {
            return MapToDescriptor(Marshal.ReadInt32(ptr, -16));
        }

        public int GetArrayLength(IntPtr ptr)
        {
            return Marshal.ReadInt32(_heap, (int)ptr - 8);
        }

        public void StoreElement(IntPtr array, int index, long element)
        {
            Marshal.WriteInt64(array, index * ALIGNMENT, element);
        }

        public long LoadElement(IntPtr array, int index)
        {
            return Marshal.ReadInt64(array, index * ALIGNMENT);
        }

        private int MapToId(TypeDescriptor type)
        {
            if (_descToPtr.ContainsKey(type))
            {
                return _descToPtr[type];
            } else
            {
                var index = _typeDescriptorIndex;
                _typeDescriptorIndex += 1;
                _descToPtr[type] = index;
                return index;
            }
        }

        private TypeDescriptor MapToDescriptor(int id)
        {
            foreach(var entry in _descToPtr)
            {
                if (entry.Value == id) return entry.Key;
            }
            throw new VMException("DRAMA");
        }

        private void Initialize(IntPtr address, ClassDescriptor type)
        {
            throw new NotImplementedException();
        }
    }
}
