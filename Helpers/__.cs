using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lowlevel_benchmark.Helpers;



/// <summary>
/// lolo: a static utils helper
/// </summary>
public unsafe static class __
{
	private static ThreadLocal<Random> _rand = new(() => new());
	/// <summary>
	/// get a thread-local Random
	/// </summary>
	public static Random Rand { get => _rand.Value; }
}