using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace JG_Prospect.JGCalender
{
    public partial class Calender : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [System.Web.Services.WebMethod]
        public static string[] GetCurrentDateEvents()
        {
            List<string> idList = new List<string>();
            List<ImproperCalendarEvent> tasksList = new List<ImproperCalendarEvent>();
            List<string> strEvents = new List<string>();

            foreach (CalendarEvent cevent in EventDAO.getTodayEvents())
            {
                string title = cevent.title;
                string startDate = String.Format("{0:s}", cevent.start);
                string endDate = String.Format("{0:s}", cevent.end);
                string address = cevent.address;
                strEvents.Add(string.Format("Title:{0} || Start:{1} || End:{2} || Address:{3}", title, startDate, endDate, address));
            }
            return strEvents.ToArray();
        }
    }
}