using System.Net;
using System.IO;

namespace LoveLivePractice.Net {
	public static class SyncUtil {
		public static string Get(string uri) {
			// Do not validate SSL
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			request.UnsafeAuthenticatedConnectionSharing = true;

			using (var reader = new StreamReader(request.GetResponse().GetResponseStream())) {
				return reader.ReadToEnd();
			}
		}

		public static byte[] GetBytes(string uri) {
			// Do not validate SSL
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.None;
			request.UnsafeAuthenticatedConnectionSharing = true;

			using (var stream = request.GetResponse().GetResponseStream()) {
				return ReadAllBytes(stream);
			}
		}
		public static byte[] ReadAllBytes(Stream input) {
			byte[] buffer = new byte[1024];

			using (var ms = new MemoryStream()) {
				int readBytes = input.Read(buffer, 0, buffer.Length);

				while (readBytes > 0) {
					ms.Write(buffer, 0, readBytes);
					readBytes = input.Read(buffer, 0, buffer.Length);
				}

				return ms.ToArray();
			}
		}
	}
}