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

        List<string> _unmanagedStrings = new List<string>();

        public RawHeap(int nofBytes)
        {
            _heap = Marshal.AllocHGlobal(nofBytes);
            _freePtr = _heap; //???
            _limit = _heap + nofBytes;
        }

        private void CheckHeapSize(int elementSize)
        {
            if(((int)_freePtr + elementSize) > (int)_limit)
            {
                throw new VMException("Out of Mana");
            }
        }

        public int Allocate(string s)
        {
            _unmanagedStrings.Add(s);

            return _unmanagedStrings.LastIndexOf(s);
        }

        public IntPtr Allocate(ArrayDescriptor type, int length)
        {
            var elementSize = ALIGNMENT; //TODO
            int size = 24 + elementSize * length;
            //int size = 24 + Math.Round(size, ALIGNMENT);
            
            CheckHeapSize(size);

            IntPtr address = _freePtr;
            _freePtr += size;
            Marshal.WriteInt64(address, 0, size);
            int typeTag = MapToId(type);
            Marshal.WriteInt64(address, 8, typeTag);
            Marshal.WriteInt64(address, 16, length);
            address += 24;
            for(var i  = 0; i < length; i++)
            {
                Marshal.WriteInt64(address, i * ALIGNMENT, 0); //TODO make a default value
            }
            return address;
        }

        IntPtr Allocate(ClassDescriptor type)
        {
            //int size = 24 + Math.Round(type.TotalFieldSize, ALIGNMENT);
            int size = 0;

            CheckHeapSize(size);
            
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
            return Marshal.ReadInt32(ptr - 8);
        }

        public void StoreElement(IntPtr array, int index, object value, TypeDescriptor type)
        {
            Marshal.WriteInt64(array, index * ALIGNMENT, ObjectToBytes(value));
        }

        public object LoadElement(IntPtr array, int index, TypeDescriptor type)
        {
            var bytes = Marshal.ReadInt64(array, index * ALIGNMENT);
            return BytesToObject(bytes, type);
        }

        private long ObjectToBytes(object value)
        {
            if (value is IntPtr)
            {
                return (long)(IntPtr)value;
            }
            else if (value is int)
            {
                return (int)value; //whooooo

            }
            else if (value is string)
            {
                return Allocate((string)value);
            }
            else if (value is char)
            {
                return (char)value;
            }
            else if (value is bool)
            {
                return (bool)value ? 1 : 0;
            }
            throw new VMException($"Cannot convert {value} to bytes");
        }

        private object BytesToObject(long element, TypeDescriptor type)
        {
            if(type is ArrayDescriptor || type is ClassDescriptor)
            {
                return ((IntPtr)element);
            } else if(type == InbuiltType.String)
            {
                return _unmanagedStrings.ElementAt((int)element);
            } else if (type == InbuiltType.Int)
            {
                return ((int)element);
            }else if (type == InbuiltType.Char)
            {
                return ((char)element);
            }else if (type == InbuiltType.Bool)
            {
                return element==1;
            }
            throw new VMException($"Cannot load type {type}");
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

        public override string ToString()
        {
            var current = _heap;
            var builder = new StringBuilder();
            while((int)current <= (int)_limit) {
                byte[] bytes = new byte[8];
                builder.Append(current.ToString("x8"));
                builder.Append(" ");
                for(var i = 0; i < ALIGNMENT; i++)
                {
                    bytes[i] = Marshal.ReadByte(current + i);
                }
                current = current + 8;

                for(var i = 0; i < bytes.Length; i++)
                {
                    if(i%4 == 0)
                    {
                        builder.Append(" ");
                    }
                    builder.Append(bytes[i].ToString("x2"));
                }

                /*
                for(var i = 0; i < bytes.Length; i++)
                {
                    if(i%4 == 0)
                    {
                        builder.Append(" ");
                    }
                    builder.Append((char)bytes[i]);
                }
                */

                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
