﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NextCloudFileUploader.Entities;
using NextCloudFileUploader.Utilities;
using NextCloudFileUploader.WebDav;

namespace NextCloudFileUploader.Services
{
	public class FileService
	{
		private WebDavProvider _webDavProvider;
		private IDbConnection _dbConnection;

		public FileService(WebDavProvider webDavProvider, IDbConnection dbConnection)
		{
			_webDavProvider = webDavProvider;
			_dbConnection = dbConnection;
		}

        /// <summary>
        /// Выгружает файлы в хранилище.
        /// </summary>
        /// <param name="fileList">Список файлов, которые следует выгрузить</param>
        /// /// <param name="totalBytes">Объем всех файлов в байтах</param>
        public async Task<bool> UploadFiles(IEnumerable<EntityFile> fileList, long totalBytes)
		{
			var files = fileList.ToList();
			var current = 0;
			var uploadedBytes = 0;

			foreach (var file in files)
			{
				try
				{
					uploadedBytes += file.Data.Length;
					var result = await _webDavProvider.PutWithHttp(file, current, files.Count, uploadedBytes, totalBytes);
				}
				catch (Exception ex)
				{
					// TODO: Добавить логирование при ошибке
					ExceptionHandler.LogExceptionToConsole(ex);
					throw ex;
				}
				finally
				{
					Interlocked.Increment(ref current);
				}
			}

			return true;
		}

		/// <summary>
		/// Выбирает из базы все файлы по имени сущности за исключением файлов, загруженных при предыдущем запуске.
		/// </summary>
		/// <param name="entity">Название сущности</param>
		/// <returns>Возвращает файлы для выгрузки в хранилище</returns>
		public IEnumerable<EntityFile> GetFilesFromDb(string entity)
		{
			try
			{
				if ((_dbConnection.State & ConnectionState.Open) == 0)
					_dbConnection.Open();

				var cmdSqlCommand = "";
				if (entity.Equals("Account") || entity.Equals("Contact"))
					cmdSqlCommand = 
							$@"SELECT {Program.Top} d.Number, d.Entity, d.EntityId, d.FileId, d.Version, f.Data FROM [dbo].[DODocuments] d
								WITH (NOLOCK)
								INNER JOIN [dbo].[{entity}File] f ON f.Id = d.FileId
								WHERE f.{entity}Id = d.EntityId
									AND d.Version = f.Version
									AND d.Entity = '{entity}File'
									AND d.EntityId is not null 
									AND d.FileId is not null 
									AND f.Data is not null 
								ORDER BY d.Number;";
				if (entity.Equals("Contract"))
					cmdSqlCommand = 
							$@"SELECT {Program.Top} d.Number, d.Entity, d.EntityId, d.FileId, d.Version, fv.PTData as 'Data' FROM [dbo].[DODocuments] d 
								WITH (NOLOCK)
								INNER JOIN [dbo].[PTFileVersion] fv ON fv.PTFile = d.FileId
									AND d.Version = fv.PTVersion
								INNER JOIN [dbo].[{entity}File] cf ON cf.{entity}Id = d.EntityId
								WHERE fv.PTFile = cf.Id
									AND d.Entity = '{entity}File'
									AND d.EntityId is not null 
									AND d.FileId is not null 
									AND cf.Data is not null 
								ORDER BY d.Number;";

				return _dbConnection.Query<EntityFile>(cmdSqlCommand).ToList();
			}
			catch (Exception ex)
			{
				ExceptionHandler.LogExceptionToConsole(ex);
				throw ex;
			}
			finally
			{
				if ((_dbConnection.State & ConnectionState.Open) != 0) _dbConnection.Close();
			}
		}
	}
}