using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class ActionSheetGallery : ContentPage
	{
		public ActionSheetGallery ()
		{
			AutomationId = "ActionSheetPage";

			var extras = new string[] {
				"Extra One",
				"Extra Two",
				"Extra Three",
				"Extra Four",
				"Extra Five",
				"Extra Six",
				"Extra Seven",
				"Extra Eight",
				"Extra Nine",
				"Extra Ten",
				"Extra Eleven",
			};


			var extras25 = new string[] {
				"Extra One",
				"Extra Two",
				"Extra Three",
				"Extra Four",
				"Extra Five",
				"Extra Six",
				"Extra Seven",
				"Extra Eight",
				"Extra Nine",
				"Extra Ten",
				"Extra Eleven",
				"Extra Twelve",
				"Extra Thirteen",
				"Extra Fourteen",
				"Extra Fifteen",
				"Extra Sixteen",
				"Extra Seventeen",
				"Extra Eightteen",
				"Extra Nineteen",
				"Extra Twenty",
				"Extra Twenty-one",
				"Extra Twenty-two",
				"Extra Twenty-three",
				"Extra Twenty-four",
				"Extra Twenty-five",
			};

			Content = new ScrollView {
				Content = new StackLayout {
					Spacing = 0,
					Children = {
						MakeActionSheetButton (this, "ActionSheet Cancel", null, "Cancel", null),
						MakeActionSheetButton (this, "ActionSheet Cancel Extras", null, "Cancel", null, extras),
						MakeActionSheetButton (this, "ActionSheet Cancel Destruction", null, "Cancel", "Destruction"),
						MakeActionSheetButton (this, "ActionSheet Cancel Destruction Extras", null, "Cancel", "Destruction", extras),
						MakeActionSheetButton (this, "ActionSheet Destruction", null, null, "Destruction"),
						MakeActionSheetButton (this, "ActionSheet Destruction Extras", null, null, "Destruction", extras),
						MakeActionSheetButton (this, "ActionSheet Extras", null, null, null, extras),
						MakeActionSheetButton (this, "ActionSheet Title", "Title", null, null),
						MakeActionSheetButton (this, "ActionSheet Title Cancel", "Title", "Cancel", null),
						MakeActionSheetButton (this, "ActionSheet Title Cancel Extras", "Title", "Cancel", null, extras),
						MakeActionSheetButton (this, "ActionSheet Title Cancel Destruction", "Title", "Cancel", "Destruction"),
						MakeActionSheetButton (this, "ActionSheet Title Cancel Destruction Extras", "Title", "Cancel", "Destruction", extras),
						MakeActionSheetButton (this, "ActionSheet Title Destruction", "Title", null, "Destruction"),
						MakeActionSheetButton (this, "ActionSheet Title Destruction Extras", "Title", null, "Destruction", extras),
						MakeActionSheetButton (this, "ActionSheet Title Extras", "Title", null, null, extras),
						MakeActionSheetButton (this, "ActionSheet Title Cancel Destruction 25xExtras", "Title", "Cancel", "Destruction", extras25),
					}
				}
			};
		}

		static Button MakeActionSheetButton (Page page, string buttonText, string title, string cancel, string destruction, params string[] extras)
		{
			var actionSheetButton = new Button {
				Text = buttonText
			};

			actionSheetButton.Clicked += async (sender, e) => await page.DisplayActionSheet (title, cancel, destruction, extras);
				
			return actionSheetButton;
		}
	}
}
