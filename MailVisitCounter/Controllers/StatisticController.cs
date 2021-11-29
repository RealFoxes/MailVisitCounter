using MailVisitCounter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MailVisitCounter.Controllers
{
	[ApiController]
	[Route("api/stats")]
	public class StatisticController : ControllerBase
	{
		private MSSQlContext context;
		private readonly ILogger<CountController> logger;
		public StatisticController(MSSQlContext context, ILogger<CountController> logger)
		{
			this.context = context;
			this.logger = logger;
		}

		/// <summary>
		/// Gets the current number of visitors to the Mall
		/// </summary>
		/// <returns>Total visitors at the moment</returns>
		[HttpGet]
		[Route("visitors")]
		public string visitors() //Получает текущее количество посетителей в ТЦ
		{
			logger.LogInformation($"request: api/stats/visitorsByTime"); // Вместо повторяющейся строки можно было бы реализовать рефлексивный метод который получал бы наименование метода параметры, но в ТЗ такого не было же :)
			string dateTime = new ShortDateTime(DateTime.Now).ToString();
			var stat = context.MallVisitors.FirstOrDefault(s => s.Id == dateTime);
			if (stat == null) // if stat for this hour not found then searching last record for this day 
			{
				dateTime = dateTime.Substring(0,dateTime.LastIndexOf('.'));
				stat = context.MallVisitors.LastOrDefault(s => s.Id.Contains(dateTime));
			}
			if (stat == null)
			{
				Response.StatusCode = 500;
				return "Error on server";
			}
			return stat.Count.ToString();
		}
		/// <summary>
		/// Gets the number of visitors to the Mall by date
		/// </summary>
		/// <returns>Total visitors in at the specified moment</returns>
		[HttpGet]
		[Route("visitorsByTime")] //Получает количество посетителей в ТЦ по дате 
		public object visitorsByTime(string dateTime)
		{
			logger.LogInformation($"request: api/stats/visitorsByTime params: (string dateTime: {dateTime})");
			var stats = context.MallVisitors
				.Where(s => s.Id.Contains(dateTime));

			var _objectView = new { Date = dateTime.Substring(0,dateTime.LastIndexOf('.')), Visitors = new Dictionary<string, int>() };

			foreach (var stat in stats)
			{
				
				var str = stat.Id.Split('.')[3];

				_objectView.Visitors.Add(str, stat.Count);

			}
			return _objectView;
		}

		/// <summary>
		/// Gets statistics on how many entered and exited through the specified door at the current moment
		/// </summary>
		/// <param name="name">Name of door</param>
		/// <returns>DateTime, Door, VisitorsEnter, VisitorsOut</returns>
		[HttpGet]
		[Route("visitorsThroughDoor")]
		public string visitorsThroughDoor(string name) //Получает статистику сколько зашло и вышло через указанную дверь в текущий момент
		{
			logger.LogInformation($"request: api/stats/visitorsThroughDoor params: (string name: {name})");

			var dateTime = new ShortDateTime(DateTime.Now).ToString(); //key

			var door = context.Doors.Include(d => d.Statistics).FirstOrDefault(d => d.Name == name);

			if (door == null)
			{
				Response.StatusCode = 404;
				logger.LogInformation($"Door with name: {name} not found!");
				return $"Door with name: {name} not found!";
			}

			var stat = door.Statistics.FirstOrDefault(s => s.DateTime == dateTime);
			if (stat == null)
				stat = new Stat();

			return $"DateTime: {dateTime} Door: {door.Name} VisitorsEnter: {stat.VisitorsEnter} Visitors out: {stat.VisitorsOut}";
		}

		/// <summary>
		/// Gets statistics on how many entered and exited through the specified door at the specified time and date
		/// </summary>
		/// <param name="name">Name of door</param>
		/// <param name="dateTime">ShortDateTimeStruct Example: 2021.11.27.17 (2021-year, 11-month, 27-day, 17-hour)</param>
		/// <returns>Datetime, Door, VisitorsEnter, VisitorsOut</returns>
		[HttpGet]
		[Route("doorByTime")] 
		public string doorByTime(string name,string dateTime) //Получает статистику сколько зашло и вышло через указанную дверь по указанному времени и дате
		{
			logger.LogInformation($"request: api/stats/doorByTime params: (string name: {name}),(string dateTime: {dateTime})");
			var door = context.Doors.Include(d => d.Statistics).FirstOrDefault(d => d.Name == name);

			if (door == null)
			{
				Response.StatusCode = 404;
				logger.LogInformation($"Door with name: {name} not found!");
				return $"Door with name: {name} not found!";
			}

			var stat = door.Statistics.FirstOrDefault(s => s.DateTime == dateTime);
			if (stat == null)
				stat = new Stat();

			return $"DateTime: {dateTime} Door: {door.Name} VisitorsEnter: {stat.VisitorsEnter} Visitors out: {stat.VisitorsOut}";
		}

		/// <summary>
		/// Gets statistics on how many entered and exited through all the doors in the Mall on the specified date or time
		/// </summary>
		/// <param name="dateTime">ShortDateTimeStruct Example: 2021.11.27.17 (2021-year, 11-month, 27-day, 17-hour</param>
		/// <returns>VisitorsEnter, VisitorsOut</returns>
		/// /// <remarks>
		/// Get visitor by hour Sample request:
		///	
		///     POST api/stats/totalvisitorsbytime
		///     {
		///        "dateTime": 2021.11.27.17
		///     }
		/// 
		/// Get visitor by Day Sample request:
		/// 
		///     POST api/stats/visitorsbytime
		///     {
		///        "dateTime": 2021.11.27
		///     }
		///     
		/// </remarks>
		[HttpGet]
		[Route("totalVisitorsThroughDoorByTime")] 
		public string totalVisitorsThroughDoorByTime(string dateTime) //Получает статистику сколько зашло и вышло через все двери в ТЦ по укзанному дате или времени
		{
			logger.LogInformation($"request: api/stats/totalVisitorsThroughDoorByTime params: (string dateTime: {dateTime})");
			var stats = context.Stats.Where(s => s.DateTime.Contains(dateTime));
			if (stats.Count() == 0)
			{
				Response.StatusCode = 404;
				return "Data was not found it may because wrong DateTime";
			}

			int vEnt = 0, vOut = 0;
			foreach (var stat in stats)
			{
				if (stat == null) continue;

				vEnt += stat.VisitorsEnter;
				vOut += stat.VisitorsOut;
			}
			return $"VisitorsEnter: {vEnt} Visitors out: {vOut}";
		}
		/// <summary>
		/// Gets hourly statistics of how many entered and exited through the specified door by date
		/// </summary>
		/// <param name="dateTime">ShortDateTimeStruct Example: 2021.11.27.17 (2021-year, 11-month, 27-day, 17-hour</param>
		/// <returns>VisitorsEnter, VisitorsOut</returns>
		/// /// <remarks>
		/// Get visitor by hour Sample request:
		///	
		///     POST api/stats/totalvisitorsbytime
		///     {
		///        "dateTime": 2021.11.27.17
		///     }
		/// 
		/// Get visitor by Day Sample request:
		/// 
		///     POST api/stats/visitorsbytime
		///     {
		///        "dateTime": 2021.11.27
		///     }
		///     
		/// </remarks>
		[HttpGet]
		[Route("visitorsThroughDoorByTime")]
		public object visitorsThroughDoorByTime(string dateTime) //Получает почасовую статистику сколько зашло и вышло через указанную дверь по дате
		{
			logger.LogInformation($"request: api/stats/visitorsThroughDoorByTime params: (string dateTime: {dateTime})");
			var stats = context.Stats
				.Where(s => s.DateTime.Contains(dateTime))
				.AsEnumerable().GroupBy(s => s.DateTime);
			
			var _objectView = new { Date = dateTime.Substring(0,dateTime.LastIndexOf('.')), Visitors = new Dictionary<string, string>()};

			foreach (var group in stats)
			{
				int vEnt = 0, vOut = 0;

				foreach (var stat in group)
				{
					if (stat == null) continue;

					vEnt += stat.VisitorsEnter;
					vOut += stat.VisitorsOut;
				}

				var str = group.Key.Split('.')[3];

				_objectView.Visitors.Add(str, $"VisitorsEnter: {vEnt} VisitorsOut: {vOut}");

			}
			return _objectView;
		}
	}
}
