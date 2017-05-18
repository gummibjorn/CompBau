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
        private readonly int ALIGNMENT = 8;
        private List<FreeEntry> _freeList = new List<FreeEntry>();

        int _typeDescriptorIndex = 0;
        Dictionary<TypeDescriptor, int> _descToPtr = new Dictionary<TypeDescriptor, int>();

        List<string> _unmanagedStrings = new List<string>();
        private CallStack _callStack;
        private IntPtr _limit;

        public RawHeap(int nofBytes, CallStack callStack)
        {
            _heap = Marshal.AllocHGlobal(nofBytes);
            _limit = _heap + nofBytes;
            _freeList.Add(new FreeEntry(_heap, nofBytes));
            _callStack = callStack;
        }


        IEnumerable<IntPtr> GetRootSet()
        {
            foreach (var frame in _callStack)
            {
                var localTypes = frame.Method.LocalTypes;
                for (int i = 0; i < localTypes.Length; i++)
                {
                    var value = frame.Locals[i];
                    if (!IntPtr.Zero.Equals(value) && IsReferenceType(localTypes[i]))
                    {
                        yield return (IntPtr)value;
                    }
                }
                var parameterTypes = frame.Method.ParameterTypes;
                for (int i = 0; i < localTypes.Length; i++)
                {
                    var value = frame.Arguments[i];
                    if (!IntPtr.Zero.Equals(value) && IsReferenceType(parameterTypes[i]))
                    {
                        yield return (IntPtr)value;
                    }
                }
                // scan parameters
                yield return (IntPtr)frame.ThisReference;
            }
        }

        private bool IsReferenceType(TypeDescriptor typeDescriptor)
        {
            return typeDescriptor is ArrayDescriptor || typeDescriptor is ClassDescriptor;
        }

        private IntPtr AllocateBytes(int elementSize, bool first = true)
        {
            foreach(var free in _freeList)
            {
                if(free.Size >= elementSize)
                {
                    return TakeFromFreeList(free, elementSize);
                }
            }
            if (first)
            {
                RunGarbageCollection();
                return AllocateBytes(elementSize, false);
            } else
            {
                throw new VMException("Out of Mana");
            }
        }

        private IntPtr TakeFromFreeList(FreeEntry free, int elementSize)
        {
            var position = free.Position;
            if(free.Size == elementSize)
            {
                _freeList.Remove(free);
            } else
            {
                free.Position = position + elementSize;
                free.Size -= elementSize;
            }
            return position;
        }

        private void RunGarbageCollection()
        {
            throw new NotImplementedException();
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

            IntPtr address = AllocateBytes(size);
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

        public IntPtr Allocate(ClassDescriptor type)
        {
            int size = 24 + type.TotalFieldSize; //might have to round this to alignment

            IntPtr address = AllocateBytes(size);
            Marshal.WriteInt64(address, 0, size);
            int typeTag = MapToId(type);
            Marshal.WriteInt64(address, 8, typeTag);
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
            var length = GetArrayLength(array);
            if (index >= length)
            {
                throw new VMException("Invalid array index");
            }
            var bytes = Marshal.ReadInt64(array, index * ALIGNMENT);
            return BytesToObject(bytes, type);
        }

        public void StoreField(IntPtr instance, int index, object value)
        {
            ClassDescriptor type = (ClassDescriptor)GetType(instance);
            var bytes = ObjectToBytes(value);
            Marshal.WriteInt64(instance, type.FieldOffsets[index], bytes);
        }

        public object LoadField(IntPtr instance, int index)
        {
            ClassDescriptor type = (ClassDescriptor)GetType(instance);
            return BytesToObject(Marshal.ReadInt64(instance, type.FieldOffsets[index]), type.FieldTypes[index]);
        }

        private void TypeCheck(TypeDescriptor valueType, TypeDescriptor targetType)
        {
            if(valueType != targetType)
            {
                throw new VMException("Invalid type");
            }
        }

        private long ObjectToBytes(object value)
        {
            if (value is IntPtr)
            {
                return (long)(IntPtr)value;
            }
            else if (value is int)
            {
                return (int)value; 
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
            for(var i = 0; i < type.FieldTypes.Length; i++) 
            {
                var f = type.FieldTypes[i];
                Marshal.WriteInt64(address, type.FieldOffsets[i], ObjectToBytes(Interpreter.DefaultValue(f)));
            }
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

    class FreeEntry
    {
        IntPtr _position;
        int _size;

        public FreeEntry(IntPtr position, int size)
        {
            _size = size;
            _position = position;
        }
        
        public IntPtr Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
            }
        }

        public int Size
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;
            }
        }
    }
}
