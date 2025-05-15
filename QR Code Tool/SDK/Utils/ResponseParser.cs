/* Лицензионное соглашение на использование набора средств разработки
 * «SDK Яндекс.Диска» доступно по адресу: http://legal.yandex.ru/sdk_agreement
 */

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace QR_Code_Tool.SDK.Utils
{
    /// <summary>
    /// Represents the parser for response's results.
    /// </summary>
    internal static class ResponseParser
    {
        /// <summary>
        /// Parses the link.
        /// </summary>
        /// <param name="responseText">The response text.</param>
        /// <returns>The parsed link.</returns>
        public static string ParseLink(string responseText)
        {
            var xmlBytes = Encoding.UTF8.GetBytes(responseText);
            using (var xmlStream = new MemoryStream(xmlBytes))
            {
                using (var reader = XmlReader.Create(xmlStream))
                {
                    reader.ReadToFollowing("public_url");
                    var url = reader.ReadElementContentAsString();
                    return url;
                }
            }
        }

        /// <summary>
        /// Parses the token.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <returns>The parsed access token.</returns>
        public static string ParseToken(Stream responseStream)
        {
            using (var reader = new StreamReader(responseStream))
            {
                var responseText = reader.ReadToEnd();
                return ParseToken(responseText);
            }
        }

        /// <summary>
        /// Parses the token.
        /// </summary>
        /// <param name="resultString">The result string.</param>
        /// <returns>The parsed access token.</returns>
        public static string ParseToken(string resultString)
        {
            var parseToken = Regex.Match(resultString, WebdavResources.TokenRegexPattern).Value;
            return parseToken;
        }
    }
}