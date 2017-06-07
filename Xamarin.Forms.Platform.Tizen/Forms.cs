using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xamarin.Forms.Internals;
using ElmSharp;
using Tizen.Applications;
using TSystemInfo = Tizen.System.SystemInfo;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class Forms
	{
		static Lazy<string> s_profile = new Lazy<string>(() =>
		{
			//TODO : Fix me if elm_config_profile_get() unavailable
			return Elementary.GetProfile();
		});

		static Lazy<int> s_dpi = new Lazy<int>(() =>
		{
			int dpi = 0;
			if (s_profile.Value == "tv")
			{
				// Use fixed DPI value (72) if TV profile
				return 72;
			}
			TSystemInfo.TryGetValue<int>("http://tizen.org/feature/screen.dpi", out dpi);
			return dpi;
		});

		static Lazy<double> s_elmScale = new Lazy<double>(() =>
		{
			// 72.0 is from EFL which is using fixed DPI value (72.0) to determine font size internally. Thus, we are restoring the size by deviding the DPI by 72.0 here.
			return s_dpi.Value / 72.0 / Elementary.GetScale();
		});

		class TizenDeviceInfo : DeviceInfo
		{
			readonly Size pixelScreenSize;

			readonly Size scaledScreenSize;

			readonly double scalingFactor;

			readonly string profile;

			public override Size PixelScreenSize
			{
				get
				{
					return this.pixelScreenSize;
				}
			}

			public override Size ScaledScreenSize
			{
				get
				{
					return this.scaledScreenSize;
				}
			}

			public override double ScalingFactor
			{
				get
				{
					return this.scalingFactor;
				}
			}

			public string Profile
			{
				get
				{
					return this.profile;
				}
			}

			public TizenDeviceInfo(FormsApplication formsApplication)
			{
				int width = 0;
				int height = 0;

				TSystemInfo.TryGetValue("http://tizen.org/feature/screen.width", out width);
				TSystemInfo.TryGetValue("http://tizen.org/feature/screen.height", out height);

				scalingFactor = 1.0;  // scaling is disabled, we're using pixels as Xamarin's geometry units
				if (_useDeviceIndependentPixel)
				{
					scalingFactor = s_dpi.Value / 160.0;
				}

				pixelScreenSize = new Size(width, height);
				scaledScreenSize = new Size(width / scalingFactor, height / scalingFactor);
				profile = s_profile.Value;
			}
		}

		static bool _useDeviceIndependentPixel = false;

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		public static FormsApplication Context
		{
			get;
			internal set;
		}

		public static bool IsInitialized
		{
			get;
			private set;
		}

		internal static TizenTitleBarVisibility TitleBarVisibility
		{
			get;
			private set;
		}

		internal static void SendViewInitialized(this VisualElement self, EvasObject nativeView)
		{
			EventHandler<ViewInitializedEventArgs> viewInitialized = Forms.ViewInitialized;
			if (viewInitialized != null)
			{
				viewInitialized.Invoke(self, new ViewInitializedEventArgs
				{
					View = self,
					NativeView = nativeView
				});
			}
		}

		public static void SetTitleBarVisibility(TizenTitleBarVisibility visibility)
		{
			TitleBarVisibility = visibility;
		}

		public static void Init(FormsApplication application)
		{
			Init(application, false);
		}

		public static void Init(FormsApplication application, bool useDeviceIndependentPixel)
		{
			_useDeviceIndependentPixel = useDeviceIndependentPixel;
			SetupInit(application);
		}

		static void SetupInit(FormsApplication application)
		{
			Color.SetAccent(GetAccentColor());

			Context = application;

			if (!IsInitialized)
			{
				Internals.Log.Listeners.Add(new XamarinLogListener());
				if (System.Threading.SynchronizationContext.Current == null)
				{
					TizenSynchronizationContext.Initialize();
				}
				Elementary.Initialize();
				Elementary.ThemeOverlay();
			}

			// In .NETCore, AppDomain feature is not supported.
			// The list of assemblies returned by AppDomain.GetAssemblies() method should be registered manually.
			// The assembly of the executing application and referenced assemblies of it are added into the list here.
			// TODO: AppDomain is comming back in NETStandard2.0. This logic should be changed at that time.
			TizenPlatformServices.AppDomain.CurrentDomain.RegisterAssemblyRecursively(application.GetType().GetTypeInfo().Assembly);

			Device.PlatformServices = new TizenPlatformServices(); ;
			if (Device.info != null)
			{
				((TizenDeviceInfo)Device.info).Dispose();
				Device.info = null;
			}

			Device.Info = new Forms.TizenDeviceInfo(application);

			if (!Forms.IsInitialized)
			{
				Registrar.RegisterAll(new Type[]
				{
					typeof(ExportRendererAttribute),
					typeof(ExportImageSourceHandlerAttribute),
					typeof(ExportCellAttribute),
					typeof(ExportHandlerAttribute)
				});
			}

			// TODO: We should consider various tizen profiles such as TV, Wearable and so on.
			string profile = ((TizenDeviceInfo)Device.Info).Profile;
			if (profile == "mobile")
			{
				Device.SetIdiom(TargetIdiom.Phone);
			}
			else if (profile == "tv")
			{
				Device.SetIdiom(TargetIdiom.TV);
			}
			else if (profile == "desktop")
			{
				Device.SetIdiom(TargetIdiom.Desktop);
			}
			else
			{
				Device.SetIdiom(TargetIdiom.Unsupported);
			}

			ExpressionSearch.Default = new TizenExpressionSearch();
			IsInitialized = true;
		}

		static Color GetAccentColor()
		{
			// On Windows Phone, this is the complementary color chosen by the user.
			// Good Windows Phone applications use this as part of their styling to provide a native look and feel.
			// On iOS and Android this instance is set to a contrasting color that is visible on the default
			// background but is not the same as the default text color.

			// TODO: implement me
			return Color.Black;
		}

		/// <summary>
		/// Converts the dp into pixel considering current DPI value.
		/// </summary>
		/// <remarks>
		/// Use this API if you want to get pixel size without scaling factor.
		/// </remarks>
		/// <param name="dp"></param>
		/// <returns></returns>
		internal static int ConvertToPixel(double dp)
		{
			return (int)Math.Round(dp * s_dpi.Value / 160.0);
		}

		/// <summary>
		/// Converts the dp into pixel by using scaling factor.
		/// </summary>
		/// <remarks>
		/// Use this API if you want to get pixel size from dp by using scaling factor.
		/// If the scaling factor is 1.0 by user setting, the same value is returned. This is mean that user doesn't want to pixel independent metric.
		/// </remarks>
		/// <param name="dp"></param>
		/// <returns></returns>
		internal static int ConvertToScaledPixel(double dp)
		{
			return (int)Math.Round(dp * Device.Info.ScalingFactor);
		}

		/// <summary>
		/// Converts the pixel into dp value by using scaling factor.
		/// </summary>
		/// <remarks>
		/// If the scaling factor is 1.0 by user setting, the same value is returned. This is mean that user doesn't want to pixel independent metric.
		/// </remarks>
		/// <param name="pixel"></param>
		/// <returns></returns>
		internal static double ConvertToScaledDP(int pixel)
		{
			return pixel / Device.Info.ScalingFactor;
		}

		/// <summary>
		/// Converts the pixel into dp value by using scaling factor.
		/// </summary>
		/// <remarks>
		/// If the scaling factor is 1.0 by user setting, the same value is returned. This is mean that user doesn't want to pixel independent metric.
		/// </remarks>
		/// <param name="pixel"></param>
		/// <returns></returns>
		internal static double ConvertToScaledDP(double pixel)
		{
			return pixel / Device.Info.ScalingFactor;
		}

		/// <summary>
		/// Converts the sp into EFL's font size metric (EFL point).
		/// </summary>
		/// <param name="sp"></param>
		/// <returns></returns>
		internal static int ConvertToEflFontPoint(double sp)
		{
			return (int)Math.Round(sp * s_elmScale.Value);
		}

		/// <summary>
		/// Convert the EFL's point into sp.
		/// </summary>
		/// <param name="eflPt"></param>
		/// <returns></returns>
		internal static double ConvertToDPFont(int eflPt)
		{
			return eflPt / s_elmScale.Value;
		}
	}

	class TizenExpressionSearch : ExpressionVisitor, IExpressionSearch
	{
		List<object> _results;
		Type _targetType;

		public List<T> FindObjects<T>(Expression expression) where T : class
		{
			_results = new List<object>();
			_targetType = typeof(T);
			Visit(expression);
			return _results.Select(o => o as T).ToList();
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.Expression is ConstantExpression && node.Member is FieldInfo)
			{
				var container = ((ConstantExpression)node.Expression).Value;
				var value = ((FieldInfo)node.Member).GetValue(container);

				if (_targetType.IsInstanceOfType(value))
					_results.Add(value);
			}
			return base.VisitMember(node);
		}
	}
}