using System;

namespace RappiSharp.Compiler {
  internal struct Location : IEquatable<Location> {
    public int Start { get; }
    public int End { get; }

    public Location(int start, int end) {
      Start = start;
      End = end;
    }

    public bool Equals(Location other) {
      return Start == other.Start && End == other.End;
    }

    public override bool Equals(object obj) {
      return obj is Location && Equals((Location)obj);
    }

    public override int GetHashCode() {
      return Start * 31 + End;
    }

    public override string ToString() {
      return $"({Start}, {End})";
    }
  }
}
