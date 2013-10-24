using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;

namespace Bitcoin_Tool.RPC
{
	public class BitcoinRPC
	{
		public JavaScriptSerializer jss = new JavaScriptSerializer();

		string host = "127.0.0.1";
		string port = "8332";
		string user = "";
		string pass = "";

		public BitcoinRPC(string host, string port, string user, string pass)
		{
			this.host = host;
			this.port = port;
			this.user = user;
			this.pass = pass;
		}

		public BitcoinRPC(string user, string pass)
		{
			this.user = user;
			this.pass = pass;
		}

		public T doRequest<T>(string method, object[] args)
		{
			return jss.ConvertToType<T>(doRequest(method, args));
		}

		public object doRequest(string method, object[] args)
		{
			Dictionary<string, object> req = new Dictionary<string, object>();
			req.Add("jsonrpc", "1.0");
			req.Add("id", "1");
			req.Add("method", method);
			req.Add("params", args);

			string json = jss.Serialize(req);

			WebClient wc = new WebClient();
			wc.Credentials = new NetworkCredential(user, pass);
			wc.Headers.Add("Content-type", "application/json-rpc");
			try
			{
				json = wc.UploadString("http://" + host + ":" + port, json);
			}
			catch (WebException wex)
			{
				json = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
				Dictionary<string, object> err = jss.Deserialize<Dictionary<string, object>>(json);
				if (!err.ContainsKey("error"))
				{
					throw new BitcoinRPCException(new BitcoinRPCError(-1, "Connection Error"), wex);
				}
				throw new BitcoinRPCException(jss.ConvertToType<BitcoinRPCError>(err["error"]), wex);
			}

			return jss.Deserialize<Dictionary<string, object>>(json)["result"];
		}
	}
}
