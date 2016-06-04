using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public class ServerData
	{
		private string _ServerName = string.Empty;
		public string ServerName
		{
			get
			{
				return _ServerName;
			}
			set
			{
				if (value.Trim() == string.Empty)
					throw new ArgumentException("ServerName 속성은 비어있을 수 없습니다.");

				_ServerName = value;
			}
		}

		private int? _MaxUserCount = null;
		public int? MaxUserCount
		{
			get
			{
				return _MaxUserCount;
			}
			set
			{
				if (value == null)
					throw new ArgumentException("MaxUserCount 속성은 null 일 수 없습니다.");
				if (value <= 1)
					throw new ArgumentException("MaxUserCount 속성은 1보다 큰 정수여야 합니다.");

				_MaxUserCount = value;
			}
		}

		private Image _ServerImage = null;
		public Image ServerImage
		{
			get
			{
				return _ServerImage;
			}
			set
			{
				if (value == null)
					throw new ArgumentException("ServerImage 속성은 null 일 수 없습니다.");

				_ServerImage = value;
			}
		}
	}
}
