﻿using RappiSharp.VirtualMachine.Descriptors;
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
        private readonly int MARK_BIT = 1 << 31;

        private FreeList _freeList = new FreeList();

        int _typeDescriptorIndex = 0;
        Dictionary<TypeDescriptor, int> _descToPtr = new Dictionary<TypeDescriptor, int>();

        List<string> _unmanagedStrings = new List<string>();
        private CallStack _callStack;
        private IntPtr _limit;

        public RawHeap(int nofBytes, CallStack callStack)
        {
            if(IntPtr.Size != ALIGNMENT)
            {
                throw new VMException("Platform not supported, please run with 64bit");
            }
            _heap = Marshal.AllocHGlobal(nofBytes);
            _limit = _heap + nofBytes;
            _freeList.Add(new FreeEntry(_heap, nofBytes));
            _callStack = callStack;

            //add default string
            _unmanagedStrings.Add(string.Empty);
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
                for (int i = 0; i < parameterTypes.Length; i++)
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
                    var ptr = TakeFromFreeList(free, elementSize);
                    for(var i = 0; i < elementSize; i += ALIGNMENT)
                    {
                        Marshal.WriteInt64(ptr, i, 0x1234567811223344);
                    }
                    return ptr;
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
            foreach(var elem in _freeList)
            {
                //Console.WriteLine("pos: " + elem.Position + " size: " + elem.Size );
            }

            //Console.WriteLine("---------------------------");

            var position = free.Position;
            if(free.Size == elementSize)
            {
                _freeList.Remove(free);
            } else
            {
                free.Position = position + elementSize;
                free.Size -= elementSize;
            }
            WriteSize(free.Position, free.Size);
            return position;
        }

        private void RunGarbageCollection()
        {
            Mark();
            //Console.WriteLine("Mark DONE");
            Sweep(_heap);
            //Console.WriteLine("Sweep DONE");
        }

        private void Mark()
        {
            foreach(var root in GetRootSet())
            {
                Traverse(root);
            }
        }

        private void Traverse(IntPtr instance)
        {
            if (instance != IntPtr.Zero && !IsMarked(instance-3*ALIGNMENT))
            {
                SetMark(instance-3*ALIGNMENT);
                foreach (var next in GetPointers(instance))
                {
                    Traverse(next);
                }
            }
        }

        private IEnumerable<IntPtr> GetPointers(IntPtr current)
        {
            var type = GetType(current);       
            if(type is ArrayDescriptor && IsReferenceType(((ArrayDescriptor)type).ElementType))
            {
                return GetArrayElemPointers(current);
            }else if(type is ClassDescriptor)
            {
                return GetClassFieldPointers(current, (ClassDescriptor)type);
            }
            return Enumerable.Empty<IntPtr>();
        }

        private IEnumerable<IntPtr> GetClassFieldPointers(IntPtr current, ClassDescriptor type)
        {
            for(int i=0; i<type.FieldTypes.Length; i++)
            {
                if (IsReferenceType(type.FieldTypes[i]))
                {
                    yield return (IntPtr)Marshal.ReadInt64(current, type.FieldOffsets[i]);
                }
            }
        }

        private IEnumerable<IntPtr> GetArrayElemPointers(IntPtr current)
        {
            for(int i=0; i < GetArrayLength(current); i++)
            {
                yield return (IntPtr)Marshal.ReadInt64(current, i * ALIGNMENT);
            }
        }

        private void SetMark(IntPtr current)
        {
            //Console.WriteLine("Mark SET");
            var bytes = Marshal.ReadInt32(current) | MARK_BIT;
            Marshal.WriteInt32(current, bytes);
        }

        private void RemoveMark(IntPtr current)
        {
            var bytes = Marshal.ReadInt32(current) & ~MARK_BIT;
            Marshal.WriteInt32(current, bytes);
        }

        private bool IsMarked(IntPtr current)
        {
            return (Marshal.ReadInt32(current) & MARK_BIT) == MARK_BIT; 
        }

        private long ReadSize(IntPtr current)
        {
            return Marshal.ReadInt32(current, 4);
        }

        private void WriteSize(IntPtr current, long size)
        {
            Marshal.WriteInt32(current, 4, (int)size);
        }

        private void Sweep(IntPtr block)
        {
            var size = ReadSize(block);
            if(size == 0)
            {
                throw new VMException("GOT A SIZE OF 0");
            }
            var next = block + (int)size;

            if (!IsMarked(block))
            {
                var entry = new FreeEntry(block, size);
                if (!_freeList.Contains(entry))
                {
                    _freeList.Add(entry);
                }
            }else
            {
                RemoveMark(block);
            }

            if((long)next < (long)_limit)
            {
                Sweep(next);
            } else
            {
                _freeList.Merge();
                foreach(var f in _freeList)
                {
                    WriteSize(f.Position, f.Size);
                }
            }
        }

        private void RemoveMarks(IntPtr block)
        {
            var size = ReadSize(block);
            var next = block + (int)size;

            RemoveMark(block);
            if ((long)next < (long)_limit)
            {
                RemoveMarks(next);
            }
        }

        public int Allocate(string s)
        {
            //TODO: this is memory leaky, but proper runtime system would allocate string as char arrays in the heap anyway
            _unmanagedStrings.Add(s);

            return _unmanagedStrings.IndexOf(s);
        }

        public IntPtr Allocate(ArrayDescriptor type, int length)
        {
            var elementSize = ALIGNMENT;
            int size = 3*ALIGNMENT + elementSize * length;

            IntPtr address = AllocateBytes(size);
            WriteSize(address, size);
            int typeTag = MapToId(type);
            Marshal.WriteInt64(address, ALIGNMENT, typeTag);
            Marshal.WriteInt64(address, 2*ALIGNMENT, length);
            address += 3*ALIGNMENT;
            for(var i  = 0; i < length; i++)
            {
                Marshal.WriteInt64(address, i * ALIGNMENT, 0);
            }
            return address;
        }

        public IntPtr Allocate(ClassDescriptor type)
        {
            int size = 3*ALIGNMENT + type.TotalFieldSize; //might have to round this to alignment

            IntPtr address = AllocateBytes(size);
            WriteSize(address, size);
            int typeTag = MapToId(type);
            Marshal.WriteInt64(address, ALIGNMENT, typeTag);
            address += 3*ALIGNMENT;
            Initialize(address, type);
            return address;
        }

        public TypeDescriptor GetType(IntPtr ptr)
        {
            return MapToDescriptor(Marshal.ReadInt32(ptr, -2*ALIGNMENT));
        }

        public int GetArrayLength(IntPtr ptr)
        {
            return Marshal.ReadInt32(ptr - ALIGNMENT);
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

        public string LoadString(int index)
        {
            return _unmanagedStrings.ElementAt(index);
        }

        private object BytesToObject(long element, TypeDescriptor type)
        {
            if(type is ArrayDescriptor || type is ClassDescriptor)
            {
                return ((IntPtr)element);
            } else if(type == InbuiltType.String)
            {
                return (int)element;
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
            while((long)current <= (long)_limit) {
                byte[] bytes = new byte[ALIGNMENT];
                long bytes64;
                bytes64 = Marshal.ReadInt64(current);
                //builder.Append(current.ToString("x8"));
                builder.Append(current.ToString());
                builder.Append(" ");
                for(var i = 0; i < ALIGNMENT; i++)
                {
                    bytes[7-i] = Marshal.ReadByte(current + i);
                }
                current = current + ALIGNMENT;

                for(var i = 0; i < bytes.Length; i++)
                {
                    builder.Append("  ");
                    if(i%4 == 0)
                    {
                        builder.Append("  ");
                    }
                    builder.Append(bytes[i].ToString("x2"));
                }

                builder.Append(" -> ");
                builder.Append(bytes64);

                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
