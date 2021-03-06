﻿using System;
using System.Collections.Generic;
using log4net;

namespace NextCloudFileUploader.Utilities
{
	public static class Utils
	{
		private static readonly ILog Log = LogManager.GetLogger("NextCloudFileUploader.Program");

		/// <summary>
		/// Показывает прогресс выполняемого процесса
		/// </summary>
		/// <param name="message">Отображаемое сообщение</param>
		/// <param name="processed">Обработано объектов</param>
		/// <param name="total">Общее количество объектов</param>
		public static string ShowPercentProgress(string message, long processed, long total)
		{
			var percent = 100 * (processed + 1) / total;
			if (processed >= total - 1 && percent < 100)
				percent = 100;
			var info = $"{message}: {percent: ##0.#}% (выполнено {processed + 1} из {total})";
			Console.Write($"\r{message}: {percent: ##0.#}% (выполнено {processed + 1} из {total})");
			if (processed >= total - 1)
				Console.Write(Environment.NewLine);
			return info;
		}
		
		/// <summary>
		/// Показывает прогресс выполняемого процесса
		/// </summary>
		/// <param name="message">Отображаемое сообщение</param>
		/// <param name="processed">Обработано объектов</param>
		/// <param name="total">Общее количество объектов</param>
		/// <param name="processedByte">Отправлено байт</param>
		/// <param name="totalBytes">Общее количество байт</param>
		public static string ShowPercentProgress(string message, long processed, long total, long processedByte, long totalBytes)
		{
			var percent = 100 * (processed + 1) / total;
			if (processed >= total - 1 && percent < 100)
				percent = 100;
			var info =
					$"{message}: {percent: ##0.#}% (выполнено {processed + 1} из {total} / отправлено {processedByte} из {totalBytes} байт)";
			Console.Write($"\r{message}: {percent: ##0.#}% (выполнено {processed + 1} из {total} / отправлено {processedByte} из {totalBytes} байт)");
			if (processed >= total - 1)
				Console.Write(Environment.NewLine);
			return info;
		}
		
		/// <summary>
		/// Разбивает список на под-списки.
		/// </summary>
		/// <param name="bigList">Основной список, который следует разбить</param>
		/// <param name="nSize">Размер под-списка</param>
		/// <typeparam name="T">Тип элементов списка</typeparam>
		/// <returns>Возвращает под-списки основного списка указанного размера.</returns>
		public static IEnumerable<List<T>> SplitList<T>(List<T> bigList, int nSize)
		{
			for (var i = 0; i < bigList.Count; i += nSize)
			{
				yield return bigList.GetRange(i, Math.Min(nSize, bigList.Count - i));
			}
		}

		public static void LogInfoAndWriteToConsole(string message)
		{
			Console.WriteLine(message);
			Log.Info(message);
		}
	}
}