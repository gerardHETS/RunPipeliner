using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RunPipeliner.DataAccess.PipelinerAPI
{
    /// <summary>
    /// Exception when HTTP error occurs. Contains HTTP Code status and message.
    /// </summary>
    public class HTTPErrorException : Exception
	{
		protected internal static readonly String WRONG_DATA_MESSAGE = "Wrong data in response.";
		protected internal static readonly int WRONG_DATA_CODE = 500;
		protected internal static readonly String WRONG_REQUEST_DATA = "Wrong data in request.";
		protected internal static readonly int WRONG_REQUEST_CODE = 400;

		/// <summary>
		/// HTTP Error Code.
		/// </summary>
		public readonly int CODE;

		/// <summary>
		/// HTTP Error Message.
		/// </summary>
		public readonly String MESSAGE;

		protected internal HTTPErrorException(int code, String message)
			: base(String.Format("HTTP Error {0}: {1}", code, message))
		{
			CODE = code;
			MESSAGE = message;
		}

		protected internal HTTPErrorException(int code, String message, Exception ex)
			: base(String.Format("HTTP Error {0}: {1}", code, message), ex)
		{
			CODE = code;
			MESSAGE = message;
		}
	}

	/// <summary>
	/// Connector for Pipeliner's REST calls. For more information see:
	/// https://workspace.pipelinersales.com/community/api/
	/// 
	/// The MIT License (MIT)
	/// 
	/// Copyright (c) 2014-2018 Pipelinersales, Inc.
	/// 
	/// Permission is hereby granted, free of charge, to any person obtaining a copy
	/// of this software and associated documentation files (the "Software"), to deal
	/// in the Software without restriction, including without limitation the rights
	/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	/// copies of the Software, and to permit persons to whom the Software is
	/// furnished to do so, subject to the following conditions:
	/// 
	/// The above copyright notice and this permission notice shall be included in
	/// all copies or substantial portions of the Software.
	/// 
	/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	/// THE SOFTWARE.
	/// </summary>
	/// <version>1.8.0</version>
	public sealed class PipelinerRestAPI
	{
		private static readonly String VERSION = "1.8.0";
		private static readonly String USER_AGENT = "Pipeliner_C#_API_Client/" + VERSION;

		public static readonly int FLAG_ROLLBACK_ON_ERROR = 0;
		public static readonly int FLAG_IGNORE_ON_ERROR = 1;
		public static readonly int FLAG_INSERT_OR_UPDATE = 2;
		public static readonly int FLAG_IGNORE_AND_RETURN_ERRORS = 8;
		public static readonly int FLAG_VALIDATE_ONLY_UPDATED_FIELDS = 256;
		public static readonly int FLAG_IGNORE_READONLY = 2 ^ 9;

		public static readonly int FLAG_USE_VALIDATION_LEVEL = 2 ^ 24;

		public static readonly int VALIDATION_STANDARD = 1;
		public static readonly int VALIDATION_SKIP_UNCHANGED = 3;
		public static readonly int VALIDATION_SKIP_FORM_CUSTOM_FIELD = 5;
		public static readonly int VALIDATION_SKIP_RECALCULATIONS = 9;
		public static readonly int VALIDATION_SKIP_KPI = 17;
		public static readonly int VALIDATION_SKIP_UTF8MB4_CHARS = 33;

		//private const String REST_URL = "/rest_services/v1/";
		private const String REST_URL = "/api/v100/rest/spaces/";



		private String serviceUrl;
		private String authHeader;

		/// <summary>
		/// Creates a new RestAPI connector which provides all REST calls as methods.
		/// </summary>
		/// <param name="serviceUrl">Service URL can be obtained in API Access section from Customers Portal.</param>
		public PipelinerRestAPI(String serviceUrl)
		{
			this.serviceUrl = serviceUrl + REST_URL;
		}

		/// <summary>
		/// Sets username and password for connection.
		/// </summary>
		/// <param name="username">API Token obtained from API Access in customers portal.</param>
		/// <param name="password">API Password obtained from API Access in customers portal.</param>
		public void setCredentials(String username, String password)
		{
			this.authHeader = Convert.ToBase64String(
				System.Text.Encoding.UTF8.GetBytes(
				String.Format("{0}:{1}", username, password)));
		}

		/// <summary>
		/// Returns current revision in server database.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <returns type="System.String">Newest revision in server database.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String result = service.getCurrentRevision(teamPipelineID);
		/// Console.WriteLine(result);
		/// </example>
		public string getCurrentRevision(String teamPipeline)
		{
			String url = String.Format("{0}{1}/TaskTypes?limit=1&revision=true", serviceUrl, teamPipeline);

			using (HttpWebResponse response = getResponse(createConnection(url)))
			{
				return response.Headers.Get("Revision");
			}
		}

		/// <summary>
		/// Creates or updated a new entity in collection. If ID is provided in data, then update is performed for existing entity.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <param name="entityName">Name of object.</param>
		/// <param name="data">Serialized JSON string with created or updated fields.</param>
		/// <param name="revision">If set, then the record will be validated against server side current revision.</param>
		/// <param name="validationLevel">More specific validation parameters can be modified by combination of flags:
		///     SKIP_UNCHANGED[3] - This will skip validation for all unchanged fields.
		///     SKIP_FORM_CUSTOM_FIELD[5] - Validations specified in form settings will be skipped.
		///     SKIP_RECALCULATION[9] - Recalculations for auto-calculated fields will be skipped as well as calculated validations.
		///     SKIP_KPI[17] - KPI for insights will not be created.
		///     SKIP_UTF8MB4_CHARS[33] - when an utf8mb4 characters were used, then the error will not be raised and characters will be removed instead.
		/// </param>
		/// <returns type="System.String">ID of created or updated entity.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String entity = "{\"ID\": \"PY-7FFFFFFF-12345678-1234-1234-1234-1234567890\", \"FIRST_NAME\":\"Richard\"}"
		/// String result = service.setEntity(teamPipelineID, "Contacts", entity);
		/// Console.WriteLine(result);
		/// </example>
		public String setEntity(String teamPipeline, String entityName, String data, String revision = null, int validationLevel = 1)
		{
			String revisionString = revision != null ? "revision=" + revision : "";
			String validationString = validationLevel != VALIDATION_STANDARD ? "validation_level=" + validationLevel.ToString() : "";
			String paramsString = String.Join("&", new String[] { revisionString, validationString });

			HttpWebRequest request = createConnection(
				String.Format("{0}{1}/{2}{3}",
				serviceUrl, teamPipeline, entityName, paramsString.Equals("&") ? "" : "?" + paramsString));

			using (HttpWebResponse response = sendPOSTRequest(request, data))
			{
				if (response.StatusCode.Equals(HttpStatusCode.Created))
				{
					//return response.Headers["Location"].Split('/').Last();
					return readContent(response);
				}
				else
				{
					return readContent(response);
				}
			}
		}


		public string updateEntity(String teamPipeline, String entityName, String data, String pipelinerID, String revision = null, int validationLevel = 1)
		{
			String revisionString = revision != null ? "revision=" + revision : "";
			String validationString = validationLevel != VALIDATION_STANDARD ? "validation_level=" + validationLevel.ToString() : "";
			String paramsString = String.Join("&", new String[] { revisionString, validationString });

			HttpWebRequest request = createConnection(
				String.Format("{0}{1}/{2}/{3}",
				serviceUrl, teamPipeline, entityName, pipelinerID));

			using (HttpWebResponse response = sendPATCHRequest(request, data))
			{
				if (response.StatusCode.Equals(HttpStatusCode.Created))
				{
					return response.Headers["Location"].Split('/').Last();
				}
				else
				{
					return readContent(response);
				}
			}
		}



		/// <summary>
		/// Retrieves list of entities in dynamic.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <param name="entityName">Name of object.</param>
		/// <param name="data">Newtonson JOBject which contains parameters for filtering.</param>
		/// <returns type="dynamic">Dynamic JSONObject with response body.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// JObject input = JObject.Parse("{\"limit\": 1,\"filter\": {\"terms\": {\"FIRST_NAME\": \"Todd\"}}}");
		/// dynamic searchResult = service.searchEntities(teamPipelineID, "Contacts", input);
		/// Console.WriteLine((int) result.COUNT);
		/// </example>
		public dynamic searchEntities(String teamPipeline, String entityName, JObject data)
		{
			HttpWebRequest request = createConnection(
				String.Format("{0}{1}/search/{2}",
				serviceUrl, teamPipeline, entityName));

			JObject result = new JObject();
			using (HttpWebResponse response = sendPOSTRequest(request, data.ToString()))
			{

				result.Add("Result", parseJsonContent(readContent(response), true));
				result.Add("COUNT", getItemCounts(response));

				if (data["revision"] != null && (bool)data["revision"] == true)
				{
					result.Add("REVISION", getRevision(response));
				}

			}

			return result;
		}

		/// <summary>
		/// Creates a new entities. If ID is provided in data, then update is performed for existing entity.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <param name="entity">Name of object.</param>
		/// <param name="data">String array with serialized JSON strings as batch entities.</param>
		/// <param name="flag" default="FLAG_ROLLBACK_ON_ERROR">The processing of command can be modified by combination of flags:
		///     FLAG_ROLLBACK_ON_ERROR[0] – if any error is occurred no entity will be processed ,entire batch will be rollbacked and exception thrown.
		///     FLAG_IGNORE_ON_ERROR[1] – it any error occurs for an entity , this entity is ignored and system continues with the next one.
		///     FLAG_GET_NO_DELETED_ID[4] - The method returns list of ID, which cannot be deleted. Could be used with combination with FLAG_IGNORE_ON_ERROR.
		///     FLAG_IGNORE_AND_RETURN_ERRORS[8] – if any error occurs for an entity, this entity is ignored and system continues with the next one.
		///     FLAG_VALIDATE_ONLY_UPDATED_FIELDS[256] - flag will validate only updated fields in entity instead of all fields.
		///     FLAG_IGNORE_READONLY[512] - setting this flag, read-only custom fields can be overridden through server API.</param>
		/// <param name="revision" default="null">If set, then records will be validated against server side current revision.</param>
		/// <param name="validationLevel">More specific validation parameters can be modified by combination of flags:
		///     SKIP_UNCHANGED[3] - This will skip validation for all unchanged fields.
		///     SKIP_FORM_CUSTOM_FIELD[5] - Validations specified in form settings will be skipped.
		///     SKIP_RECALCULATION[9] - Recalculations for auto-calculated fields will be skipped as well as calculated validations.
		///     SKIP_KPI[17] - KPI for insights will not be created.
		///     SKIP_UTF8MB4_CHARS[33] - when an utf8mb4 characters were used, then the error will not be raised and characters will be removed instead.
		/// </param>
		/// <returns type="System.String[]">Array of results for requested entities.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String entities[] = new String[]{"{\"ID\": \"PY-7FFFFFFF-12345678-1234-1234-1234-1234567890\", \"FIRST_NAME\":\"Richard\"}"};
		/// String result[] = service.setEntities(teamPipelineID, "Contact", entities);
		/// Console.WriteLine(result[0]);
		/// </example>
		public String[] setEntities(String teamPipeline, String entity, String[] data, int flag = 0, String revision = null, int validationLevel = 1)
		{
			String revisionString = revision != null ? "revision=" + revision : "";
			String validationString = validationLevel != VALIDATION_STANDARD ? "validation_level=" + validationLevel.ToString() : "";
			String paramsString = String.Join("&", new String[] { revisionString, validationString });

			try
			{
				return batchMethod(teamPipeline, "setEntities", entity, data, flag, paramsString.Equals("&") ? "" : "&" + paramsString);
			}
			catch (IOException ex)
			{
				throw new HTTPErrorException(HTTPErrorException.WRONG_DATA_CODE, HTTPErrorException.WRONG_DATA_MESSAGE, ex);
			}
		}

		/// <summary>
		/// Deletes entity with given primary key.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <param name="entityName">Name of object.</param>
		/// <param name="id">The unique identificator of entity.</param>
		/// <returns type="System.String">ID of deleted entity.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String id = "PY-7FFFFFFF-12345678-1234-1234-1234-1234567890";
		/// String result = service.deleteEntity(teamPipelineID, "Contacts", id);
		/// Console.WriteLine(result);
		/// </example>
		public String deleteEntity(String teamPipeline, String entityName, String id)
		{
			HttpWebRequest request = createConnection(
				String.Format("{0}{1}/{2}/{3}",
				serviceUrl, teamPipeline, entityName, id));

			request.Method = "DELETE";
			getResponseContent(request);
			return id;
		}

		/// <summary>
		/// Deletes entities with given primary key.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <param name="entity">Name of object.</param>
		/// <param name="data">Array with IDs to delete.</param>
		/// <param name="flag" default="FLAG_ROLLBACK_ON_ERROR">The processing of command can be modified by combination of flags:
		///     FLAG_ROLLBACK_ON_ERROR[0] – if any error is occurred no entity will be processed ,entire batch will be rollbacked and exception thrown.
		///     FLAG_IGNORE_ON_ERROR[1] – it any error occures for an entity , this entity is ignored and system continues with the next one.
		///     FLAG_GET_NO_DELETED_ID[4] - The method returns list of ID, which cannot be deleted. Could be used with combination with FLAG_IGNORE_ON_ERROR.
		///     FLAG_IGNORE_AND_RETURN_ERRORS[8] – if any error occurs for an entity, this entity is ignored and system continues with the next one.</param>
		/// <returns type="System.String[]">Array of result for deleted entities.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String ids = new String[]{"PY-7FFFFFFF-12345678-1234-1234-1234-1234567890"};
		/// String result[] = service.deleteEntities(teamPipelineID, "Contact", ids);
		/// Console.WriteLine(result[0]);
		/// </example>
		public String[] deleteEntities(String teamPipeline, String entity, String[] data, int flag = 0)
		{
			String[] dataNew = new String[data.Length];
			try
			{
				for (int i = 0; i < dataNew.Length; i++)
				{
					dataNew[i] = "\"" + data[i] + "\"";
				}
				return batchMethod(teamPipeline, "deleteEntities", entity, dataNew, flag, "");
			}
			catch (IOException ex)
			{
				throw new HTTPErrorException(HTTPErrorException.WRONG_DATA_CODE, HTTPErrorException.WRONG_DATA_MESSAGE, ex);
			}
		}

		/// <summary>
		/// Retrieves list of entities in dynamic object.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <param name="entityName">Name of object.</param>
		/// <param name="query" default="null">Params for requested entity. Query is equal to params followed after '?'.</param>
		/// <returns type="dynamic">Dynamic object with response body.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String query = "limit=5&amp;filter=FIRST_NAME::Todd"
		/// dynamic result = service.getEntities(teamPipelineID, "Contacts", query);
		/// Console.WriteLine(result[0].FIRST_NAME);
		/// </example>
		public dynamic getEntities(String teamPipeline, String entityName, String query = null)
		{
			/* String content = miscMethod(String.Format("{0}{1}/{2}{3}",
				serviceUrl, teamPipeline, entityName, safeQuery(query))); */

			String content = miscMethod(String.Format("{0}{1}/{2}{3}",
				serviceUrl, teamPipeline, entityName, (query)));


			//return parseJsonContent(content, false);
			return content;
		}

		/// <summary>
		/// Retrieves list of collections in the pipeline.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <returns type="System.String[]">String array of collections.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String[] retVal = service.getCollections(teamPipelineID);
		/// Console.WriteLine(retVal[0]);
		/// </example>
		public string getCollections(String teamPipeline)
		{
			String content = miscMethod(String.Format("{0}{1}", serviceUrl, teamPipeline));
			//return parseJsonContent(content, true).ToObject<String[]>();

			return content;
		}

		/// <summary>
		/// Retrieves the datacenter URL for team space.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <returns type="System.String">Datacenter URL for current team space.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String retVal = service.getTeamPipelineUrl(teamPipelineID);
		/// Console.WriteLine(retVal);
		/// </example>
		public String getTeamPipelineUrl(String teamPipeline)
		{
			String content = miscMethod(
				String.Format("{0}{1}/{2}", serviceUrl, teamPipeline, "teamPipelineUrl"));

			content = content.Replace("\n", "").Trim();
			return content.Substring(1, content.Length - 2);
		}

		/// <summary>
		/// Retrieves the current server's UTC datetime for queried team space.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <returns type="System.DateTime">Current server's UTC date as datetime.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// String retVal = service.getServerAPIUtcDateTime(teamPipelineID);
		/// Console.WriteLine(retVal);
		/// </example>
		public DateTime getServerAPIUtcDateTime(String teamPipeline)
		{
			String content = miscMethod(String.Format("{0}{1}/{2}", serviceUrl, teamPipeline, "serverAPIUtcDateTime"));
			content = content.Replace("\n", "").Trim();
			return DateTime.Parse(content.Substring(1, content.Length - 2));
		}

		/// <summary>
		/// Retrieves the list of API error codes.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <returns type="System.Collections.Generic.Dictionary&lt;int, String&gt;">Dictionary with server API errors as integer keys and string 
		/// of API error codes as values.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// Dictionary&lt;int, String&gt; retVal = service.getErrorCodes(teamPipelineID);
		/// Console.WriteLine(retVal[301]);
		/// </example>
		public Dictionary<int, String> getErrorCodes(String teamPipeline)
		{
			String content = miscMethod(String.Format("{0}{1}/{2}", serviceUrl, teamPipeline, "errorCodes"));
			return parseJsonContent(content, false).ToObject<Dictionary<int, String>>();
		}

		/// <summary>
		/// Retrieves the dictionary list of public entities.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <returns type="System.Collections.Generic.Dictionary&lt;String, String&gt;">Dictionary with server API collections as keys and entity 
		/// names as values.</returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// Dictionary&lt;String, String&gt; retVal = service.getEntityPublic(teamPipelineID);
		/// Console.WriteLine(retVal["Accounts"]);
		/// </example>
		public Dictionary<String, String> getEntityPublic(String teamPipeline)
		{
			String content = miscMethod(String.Format("{0}{1}/{2}", serviceUrl, teamPipeline, "entityPublic"));
			return parseJsonContent(content, false).ToObject<Dictionary<String, String>>();
		}

		/// <summary>
		/// Returns list of fields for specific entity.
		/// </summary>
		/// <param name="teamPipeline">Name of team space to be used.</param>
		/// <param name="entityName">Name of entity class or entity collection.</param>
		/// <param name="query" default="null">REST query string for filtering (e.g. "filter=CUSTOM::TRUE")</param>
		/// <returns type="dynamic">
		/// List of dictionaries which contains information about fields:
		///     API_NAME - field name used in API as key.
		///     CALC_FORMULA - calculated formula for autocalculated fields, or None if not set.
		///     CHOICES - if dropdown or radio button, then dictionary is set with IDs of datasets as keys and their names as values, otherwise None is set.
		///     CUSTOM - true if custom field, false if system field.
		///     DEFAULT - default value for field. If not set, then None.
		///     DESCRIPTION - description of field.
		///     ID - unique identificator of field. Used as ID_FIELD in Data entity.
		///     MAX_VALUE - max allowed value. For decimal types, it is maximal number, for unicode types it is maximal length of string.
		///     MIN_VALUE - min allowed value for decimal an datetime types.
		///     NAME - field name shown in Pipeliner.
		///     PL_TYPE - Pipeliner field type.
		///     READONLY - true if field is readonly, otherwise false.
		///     REQUIRED - true if field is required, otherwise false.
		///     TYPE - Type of field. Supported types - [string, unicode, datetime, long, Decimal, list]
		/// </returns>
		/// <exception cref="HTTPErrorException">Raises on HTTP error code from server with API error message.</exception>
		/// <example>
		/// dynamic retVal = service.getFields(teamPipelineID, "Contacts");
		/// Console.WriteLine(retVal[0].API_NAME);
		/// </example>
		public dynamic getFields(String teamPipeline, String entityName, String query = null)
		{
			String content = miscMethod(
				String.Format("{0}{1}/{2}/{3}{4}",
				serviceUrl, teamPipeline, "getFields",
				entityName, safeQuery(query)));

			//return parseJsonContent(content, false);
			return content;
		}

		/*********************************************************************/

		private String safeQuery(String query)
		{
			if (query == null || query == String.Empty)
			{
				return String.Empty;
			}
			else if (!query.StartsWith("?"))
			{
				query = String.Format("?{0}", query);
			}
			return query.Replace(" ", "%20");
		}

		private dynamic parseJsonContent(String content, bool array)
		{
			try
			{
				if (array)
				{
					return JArray.Parse(content);
				}
				else
				{
					return JObject.Parse(content);
				}
			}
			catch (JsonReaderException ex)
			{
				throw new HTTPErrorException(
					HTTPErrorException.WRONG_DATA_CODE,
					HTTPErrorException.WRONG_DATA_MESSAGE, ex);
			}
		}

		private String readContent(HttpWebResponse response)
		{
			String content = String.Empty;
			try
			{
				using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
				{
					return streamReader.ReadToEnd();
				}
			}
			catch (IOException)
			{
				return response.StatusDescription;
			}
		}

		private String readErrorContent(HttpWebResponse response)
		{
			String content = readContent(response);
			try
			{
				return (String)JObject.Parse(content)["message"];
			}
			catch (Newtonsoft.Json.JsonReaderException)
			{
				return content;
			}
		}

		private HttpWebResponse getResponse(HttpWebRequest request)
		{
			HttpWebResponse response = null;

			try
			{
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (System.Net.WebException ex)
			{
				response = (HttpWebResponse)ex.Response;
				if (response != null)
				{
					throw new HTTPErrorException((int)response.StatusCode, readErrorContent(response));
				}
				else
				{
					throw new HTTPErrorException(408, ex.Message);
				}

			}

			return response;
		}

		private String getResponseContent(HttpWebRequest request)
		{
			using (HttpWebResponse response = getResponse(request))
			{
				return readContent(response);
			}
		}

		private String miscMethod(string url)
		{
			HttpWebRequest request = createConnection(url);
			String content = getResponseContent(request);
			return content;
		}

		private String[] batchMethod(String teamPipeline, String method, String entity, String[] data, int flag, String revision)
		{
			HttpWebRequest conn = createConnection(
				String.Format("{0}{1}/{2}?entityName={3}&flag={4}{5}",
				serviceUrl, teamPipeline, method, entity, flag, revision
			));

			StringBuilder jData = new StringBuilder('[');
			for (int i = 0; i < data.Length; i++)
			{
				jData.Append(data[i]);
				if (i != data.Length - 1)
				{
					jData.Append(',');
				}
			}

			jData.Insert(0, '[');
			jData.Append(']');

			dynamic parsedContent = null;
			using (HttpWebResponse response = sendPOSTRequest(conn, jData.ToString()))
			{
				parsedContent = parseJsonContent(readContent(response), true);
			}

			int contentCount = Enumerable.Count(parsedContent);
			String[] result = new String[contentCount];

			if ((flag & FLAG_IGNORE_AND_RETURN_ERRORS) == 0)
			{
				result = parsedContent.ToObject<String[]>();
			}
			else
			{

				for (int i = 0; i < contentCount; i++)
				{
					try
					{
						String err_message = (String)parsedContent[i].message;
						result[i] = String.Format("serverApiError #{0}: {1}", (String)parsedContent[i].errorcode, err_message);
					}
					catch (RuntimeBinderException)
					{
						result[i] = parsedContent[i].ToString();
					}
				}
			}

			return result;
		}

		private HttpWebRequest createConnection(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.UserAgent = USER_AGENT;
			request.Headers.Add("Authorization", "Basic " + authHeader);
			return request;
		}

		private HttpWebResponse sendPOSTRequest(HttpWebRequest request, String data)
		{
			byte[] bdata = Encoding.UTF8.GetBytes(data);

			request.Method = "POST";
			request.ContentLength = bdata.Length;

			try
			{
				using (Stream stream = request.GetRequestStream())
				{
					stream.Write(bdata, 0, bdata.Length);
				}
			}
			catch (IOException ex)
			{
				throw new HTTPErrorException(HTTPErrorException.WRONG_DATA_CODE, HTTPErrorException.WRONG_REQUEST_DATA, ex);
			}

			return getResponse(request);
		}


		private HttpWebResponse sendPATCHRequest(HttpWebRequest request, String data)
		{
			byte[] bdata = Encoding.UTF8.GetBytes(data);

			request.Method = "PATCH";
			request.ContentLength = bdata.Length;

			try
			{
				using (Stream stream = request.GetRequestStream())
				{
					stream.Write(bdata, 0, bdata.Length);
				}
			}
			catch (IOException ex)
			{
				throw new HTTPErrorException(HTTPErrorException.WRONG_DATA_CODE, HTTPErrorException.WRONG_REQUEST_DATA, ex);
			}

			return getResponse(request);
		}

		private String getRevision(HttpWebResponse response)
		{
			return response.GetResponseHeader("Revision");
		}

		private int getItemCounts(HttpWebResponse response)
		{
			return int.Parse(Regex.Match(response.GetResponseHeader("Content-Range"), @"items \d+-\d+/(\d+)").Groups[1].Value);
		}
	}
}
