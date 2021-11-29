using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailVisitCounter.Models
{
	public class Door
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<Stat> Statistics { get; set; }
		public Door()
		{
			Statistics = new List<Stat>();
		}
	}
}
