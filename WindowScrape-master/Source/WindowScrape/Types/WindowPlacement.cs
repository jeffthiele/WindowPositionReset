using System.Drawing;

namespace WindowPositionReset
{
	public class WindowPlacement
	{
		public Rectangle Position;
		public WindowShowStateEnum ShowState;

		public override string ToString()
		{
			return "{" + Position.ToString() + ", " + ShowState + "}";
		}
	}

	public enum WindowShowStateEnum
	{
		Hide = 0,
		Normal = 1,
		ShowNormal = 1,
		ShowMinimized = 2,
		Maximize = 3,
		ShowMaximized = 3,
		ShowNoActive = 4,
		Show = 5,
		Minimize = 6,
		ShowMinNoActive = 7,
		ShowNA = 8,
		ShowRestore = 9
	}
}