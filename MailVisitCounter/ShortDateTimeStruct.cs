using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailVisitCounter
{
	//В дальнейшем эту структуру использую как ключ
	public struct ShortDateTime
	{
		public int Year { get; set; }
		public int Month { get; set; }
		public int Day { get; set; }
		public int Hour { get; set; }
		public ShortDateTime(DateTime dateTime)
		{
			this.Year = dateTime.Year;
			this.Month = dateTime.Month;
			this.Day = dateTime.Day;
			this.Hour = dateTime.Hour;
		}
		public override string ToString()
		{
			return $"{Year}.{Month}.{Day}.{Hour}";
		}
	}
}
