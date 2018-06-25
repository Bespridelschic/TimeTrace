using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.ApplicationModel.Resources;
using TimeTrace.Model.DBContext;
using TimeTrace.Model.Events;
using Windows.Storage;

namespace TimeTrace.Model.Notifications
{
	/// <summary>
	/// Pipeline of remind notifications
	/// </summary>
	public class NotificationPipeline : INotificationPipeline<Category>
	{
		#region Properties

		/// <summary>
		/// Queue of notifications in in ascending order
		/// </summary>
		private Queue<Category> Pipeline { get; set; }

		/// <summary>
		/// Timer for notifications
		/// </summary>
		private Timer Timer { get; set; }

		/// <summary>
		/// Localization resource loader
		/// </summary>
		private ResourceLoader ResourceLoader { get; }

		#endregion

		public NotificationPipeline()
		{
			ResourceLoader = ResourceLoader.GetForCurrentView("NotificationPipelineModel");

			Pipeline = new Queue<Category>();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			using (var db = new MainDatabaseContext())
			{
				var list = db.MapEvents
					.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete && i.NotificationTime > DateTime.UtcNow)
					.OrderBy(i => i.NotificationTime)
					.ToList();

				foreach (var item in list)
				{
					Pipeline.Enqueue(item);
				}
			}

			CountdownTime();
		}

		/// <summary>
		/// Eject notify from queue
		/// </summary>
		/// <returns></returns>
		public Category Eject()
		{
			if (Pipeline.Count > 0)
			{
				CountdownTime();
				return Pipeline.Dequeue();
			}

			return null;
		}

		/// <summary>
		/// Insert new notify to queue
		/// </summary>
		/// <param name="obj"></param>
		public void Insert(Category obj)
		{
			Pipeline.Enqueue(obj);
			var newQueue = new Queue<Category>(Pipeline.OrderBy(i => ((MapEvent)i).NotificationTime));

			Pipeline = newQueue;
			CountdownTime();
		}

		/// <summary>
		/// Start time count for clouse reminding
		/// </summary>
		public void CountdownTime()
		{
			if (Pipeline.Count > 0)
			{
				var firstQueueElement = (MapEvent)Pipeline.Peek();

				var time = firstQueueElement.NotificationTime.Subtract(DateTime.UtcNow).Ticks / 10_000;

				if (time > 0)
				{
					Timer = new Timer(time);
					Timer.Elapsed += (i, e) =>
					{
						Eject();
						Timer.Stop();
						Notify(firstQueueElement.Name, firstQueueElement.Start);
					};
					Timer.Start();
				}
			}
		}

		/// <summary>
		/// New notification for reminding
		/// </summary>
		/// <param name="name">Name of event</param>
		/// <param name="dateTime">Time for start event</param>
		public void Notify(string name, DateTime dateTime)
		{
			var toastContent = new ToastContent()
			{
				Visual = new ToastVisual()
				{
					BindingGeneric = new ToastBindingGeneric()
					{
						Children =
						{
							new AdaptiveText()
							{
								Text = ResourceLoader.GetString("Reminder"),
								HintMaxLines = 1
							},
							new AdaptiveText()
							{
								Text = name,
							},
							new AdaptiveText()
							{
								Text = $"{ResourceLoader.GetString("StartAt")} {dateTime.ToLocalTime()}"
							}
						},
						AppLogoOverride = new ToastGenericAppLogo()
						{
							//Source = "https://picsum.photos/48?image=883",
							Source = @"Assets/user-48.png",
							HintCrop = ToastGenericAppLogoCrop.Circle
						}
					}
				},
				Launch = "app-defined-string"
			};

			// Create the toast notification
			var toastNotif = new Windows.UI.Notifications.ToastNotification(toastContent.GetXml());

			// And send the notification
			Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

			CountdownTime();
		}
	}
}