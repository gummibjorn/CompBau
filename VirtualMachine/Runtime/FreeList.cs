using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RappiSharp.VirtualMachine.Runtime
{
    class FreeList : List<FreeEntry>
    {

        public void Merge()
        {
            this.Sort((a, b) => (int)((long)a.Position - (long)b.Position));
            var groups = new List<List<FreeEntry>>();
            for(int i = 0; i < this.Count(); i++)
            {
                var currentGroup = new List<FreeEntry>();
                currentGroup.Add(this[i]);
                groups.Add(currentGroup);

                while (i+1 < this.Count() && currentGroup.Last().DirectlyPrecedes(this[i+1])){
                    currentGroup.Add(this[i+1]);
                    i += 1;
                }
            }

            var newFreeList = groups.Select( g =>
                              {
                                  var length = g.Select(f => f.Size).Sum();
                                  return new FreeEntry(g.First().Position, length);
                              });

            Clear();
            foreach(var f in newFreeList)
            {
                Add(f);
            }
        }

        public void Add(IntPtr position, long size)
        {
            Add(new FreeEntry(position, size));
        }
    }

    class FreeEntry
    {
        IntPtr _position;
        long _size;

        public FreeEntry(IntPtr position, long size)
        {
            _size = size;
            _position = position;
        }

        public bool DirectlyPrecedes(FreeEntry next)
        {
            return (_position + (int)_size).Equals(next.Position);
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

        public long Size
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
