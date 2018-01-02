using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model
{
    internal static class GlobalProperties
    {
		/// <summary>
		/// Языки приложения
		/// </summary>
		public enum Language
		{
			Russian,
			English
		}

		/// <summary>
		/// Цвета приложения
		/// </summary>
		public enum Color
		{
			System,
			Custom
		}

		/// <summary>
		/// Язык используемый в приложении
		/// </summary>
		public static Language AppLanguage { get; set; }

		/// <summary>
		/// Текущий тип цветовой палитры
		/// </summary>
		public static Color AppColor { get; set; }

		/// <summary>
		/// Инициализация глобальных свойств
		/// </summary>
		static GlobalProperties()
		{
			AppLanguage = Language.Russian;
			AppColor = Color.System;
		}
    }
}
