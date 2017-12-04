using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace WindowPositionReset
{
	public class ScreenConfiguration: IComparable<ScreenConfiguration>, IEquatable<ScreenConfiguration>
	{
	    private readonly Screen[] _screens;

	    public ScreenConfiguration(Screen[] screens)
	    {
	        this._screens = screens ?? new Screen[0];
	    }

		public int CompareTo(ScreenConfiguration other)
		{
		    if (Equals(other))
		        return 0;

            return -1;
		}

		public bool Equals(ScreenConfiguration other)
		{
		    if (other?._screens.Length != _screens.Length)
		        return false;

		    for (int i = 0; i < _screens.Length; i++)
		    {
		        if (other._screens[i].Bounds != _screens[i].Bounds
		            || other._screens[i].DeviceName != _screens[i].DeviceName
		            || other._screens[i].Primary != _screens[i].Primary)
		            return false;
		    }

		    return true;
		}

	    public override string ToString()
	    {
            return "{ " + string.Join(",", _screens.Select(x => $"{{DeviceName: \"{x.DeviceName}\", Bounds: {x.Bounds}, Primary: {x.Primary}}}")) + " }";
	    }

	    public override int GetHashCode()
	    {
            int hash = this.ToString().GetHashCode();
	        return hash;
	    }

	    public override bool Equals(object obj)
	    {
	        if (obj is ScreenConfiguration blah)
	            return this.Equals(blah);
	        
            return false;
	    }
	}
}