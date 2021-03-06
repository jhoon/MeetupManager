﻿using System;
using Cirrious.CrossCore;
using MeetupManager.Portable.Interfaces;
using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using System.Threading.Tasks;
using MeetupManager.Portable.Interfaces.Database;
using System.Collections.ObjectModel;

namespace MeetupManager.Portable.ViewModels
{
	public class StatisticsViewModel : BaseViewModel
	{
		IDataService dataService;
        public bool ShowPopUps { get; set; }
		private string groupId;

		public StatisticsViewModel (IMeetupService service, IDataService dataService) : base(service)
		{
			GroupsEventsCount = new Dictionary<long, int> ();
		    ShowPopUps = true;
			this.dataService = dataService;
		}

		public Dictionary<long, int> GroupsEventsCount
		{
			get; set;
		}

		public void Init(string gId, string gName)
		{
			groupId = gId;
			GroupName = gName;
		}

		public string GroupName{ get; set; }


		public DateTime FromUnixTime(long unixTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddMilliseconds(unixTime);
		}

		private IMvxCommand refreshCommand;

		public IMvxCommand RefreshCommand
		{
			get { return refreshCommand ?? (refreshCommand = new MvxCommand(async () => ExecuteRefreshCommand())); }
		}


		public async Task ExecuteRefreshCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;
			try
			{
				GroupsEventsCount.Clear ();
			

				var newMembers = await dataService.GetNewMembersForGroup (groupId);
				var rsvps = await dataService.GetRSVPsForGroup (groupId);

				foreach (var member in newMembers) {

					if (!GroupsEventsCount.ContainsKey (member.EventDate)) {
						GroupsEventsCount.Add (member.EventDate, 0);
					}

					GroupsEventsCount [member.EventDate]++;
				}
				foreach (var member in rsvps) {
					if (!GroupsEventsCount.ContainsKey (member.EventDate)) {
						GroupsEventsCount.Add (member.EventDate, 0);
					}

					GroupsEventsCount [member.EventDate]++;
				}

			    if (ShowPopUps)
			    {
			        if (GroupsEventsCount.Count == 0)
			            Mvx.Resolve<IMessageDialog>()
			                .SendMessage("There is no data for this group, please check in a few members first to a meetup.",
			                    "No Statistics");
			        else if (GroupsEventsCount.ContainsKey(0))
			            Mvx.Resolve<IMessageDialog>()
			                .SendMessage(
			                    "Data for group needs synced, please re-visit all meetups to synchronize data and return for in depth statistics.",
			                    "No Statistics");
			    }

			}
			catch(Exception ex) {
			}
			finally{
				IsBusy = false;
			}

		}
	}
}

