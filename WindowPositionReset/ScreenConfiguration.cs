using System;

namespace WindowPositionReset
{
	public class ScreenConfiguration: IComparable<ScreenConfiguration>, IEquatable<ScreenConfiguration>
	{
		public ScreenConfiguration(int count, int totalHeight, int totalWidth)
		{
			TotalHeight = totalHeight;
			TotalWidth = totalWidth;
			Count = count;
		}

		public int Count { get; }
		public int TotalWidth { get; }
		public int TotalHeight { get; }


		public int CompareTo(ScreenConfiguration other)
		{
			return other.Count - Count
			       + other.TotalWidth - TotalWidth
			       + other.TotalHeight - TotalHeight;
		}

		public bool Equals(ScreenConfiguration other)
		{
			return other.Count == Count && other.TotalHeight == TotalHeight && other.TotalWidth == TotalWidth;
		}
	}
}