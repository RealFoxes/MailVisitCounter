using MailVisitCounter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailVisitCounter.Controllers
{
	[ApiController]
	[Route("api")]
	public class CountController : ControllerBase
	{
		private MSSQlContext context;
		private readonly ILogger<CountController> logger;

		public CountController(MSSQlContext context, ILogger<CountController> logger)
		{
			this.context = context;
			this.logger = logger;
		}

		/// <summary>
		/// Allows you to change the number of visitors who entered or exited the specified door
		/// </summary>
		/// <param name="name">Name of door</param>
		/// <param name="isEnter">If Enter in then true otherwise false</param>
		/// <returns>Door, VisitorsEnter, VisitorsOut</returns>
		/// <remarks>
		/// Sample request:
		///
		///     POST api/count
		///     {
		///        "name": North,
		///        "isEnter": true
		///     }
		///
		/// </remarks>
		/// <response code="404">Door not found</response>
		[HttpPost]
		[Route("count")]
		public string Count(string name, bool isEnter)
		{
			logger.LogInformation("Attempt to record a visitor...");
			logger.LogInformation($"Params: string name: {name} , bool isEnter: {isEnter}");

			var dateTime = new ShortDateTime(DateTime.Now).ToString(); //key

			var door = context.Doors.Include(d => d.Statistics).FirstOrDefault(d => d.Name == name);
			if (door == null)
			{
				Response.StatusCode = 404;
				logger.LogInformation($"Door with name: {name} not found!");
				return $"Door with name: {name} not found!";
			}

			var stat = door.Statistics.FirstOrDefault(s => s.DateTime == dateTime);
			if(stat == null)
			{
				stat = new Stat { DateTime = dateTime };
				
				door.Statistics.Add(stat);
				context.Add(stat);
			}

			var mailCounter = context.MallVisitors.FirstOrDefault(v => v.Id == dateTime);
			if (mailCounter == null)
			{
				mailCounter = new MallVisitors { Id = dateTime, Count = 0 };
				context.Add(mailCounter);
			}

			if (isEnter)
			{
				stat.VisitorsEnter++;
				mailCounter.Count++;
			}
			else
			{
				stat.VisitorsOut++;
				mailCounter.Count--;
			}

			context.SaveChanges();

			logger.LogInformation($"Door: {door.Name} VisitorsEnter: {stat.VisitorsEnter} Visitors out: {stat.VisitorsOut} TotalVisitors: {mailCounter.Count}");
			return $"Door: {door.Name} VisitorsEnter: {stat.VisitorsEnter} Visitors out: {stat.VisitorsOut} TotalVisitors: {mailCounter.Count}";
		}
	}
}
