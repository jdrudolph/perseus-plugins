using System.Drawing;
using BaseLib.Param;

namespace PerseusApi.Generic{
	/// <summary>
	/// This interface is the base from which all other activities are derived. 
	/// It provides properties that are common to all activities. 
	/// </summary>
	public interface IActivity{
		/// <summary>
		/// This is the name that appears in the drop-down menu of Perseus to start this activity.
		/// </summary>
		string Name { get; }
        /// <summary>
        /// The context help that will appear in tool tips etc. 
        /// </summary>
        string Description { get; }
        /// <summary>
		/// This number controls the order in which activities are displayed in the drop down menu in Perseus.
		/// </summary>
		float DisplayOrder { get; }
		/// <summary>
		/// If false is returned, the activity will not be available.
		/// </summary>
		bool IsActive { get; }
		/// <summary>
		/// A shortcut button will be displayed in the top button row. This also requires that an image is returned by <code>ButtonImage</code>>. 
		/// </summary>
		bool HasButton { get; }
		/// <summary>
		/// The image for the menu entry and the shortcut button.
		/// </summary>
		Bitmap DisplayImage { get; }

		/// <summary>
		/// Specifies the maximal number of threads that this acticity can make use of simultaneously.
		/// </summary>
		/// <param name="parameters">The parameters of the activity. The maximal usable number of threads might depend on the parameter settings.</param>
		/// <returns></returns>
		int GetMaxThreads(Parameters parameters);
	}
}