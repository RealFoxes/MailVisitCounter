using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MailVisitCounter.Models
{
	public class Stat
	{
		public int Id { get; set; } 
		public string DateTime { get; set; } //ShortDateTimeStruct
		public int VisitorsOut { get; set; }
		public int VisitorsEnter { get; set; }
	}
}
