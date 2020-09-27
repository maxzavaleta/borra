using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using RestSharp;

namespace sovos1
{
    class Program
    {
        const string URL = "http://spsa.paperless.com.pe/axis2/services/Online.OnlineHttpSoap11Endpoint";
        static void Main(string[] args)
        {
            recovery();
            //generation();
        }

        static void recovery(){
            var ruc="20394077101";
            var login="admin_hpo";
            var clave="abc123";

            var tipoDoc="1";
            var folio="FA04-00119478";
            var tipoRetorno="3"; //1 url, 2 xml, 3 sunat

            string dataXml = File.ReadAllText("recovery.xml");

            dataXml=dataXml.Replace("@@ruc@@",ruc);
            dataXml=dataXml.Replace("@@login@@",login);
            dataXml=dataXml.Replace("@@clave@@",clave);
            dataXml=dataXml.Replace("@@tipoDoc@@",tipoDoc);
            dataXml=dataXml.Replace("@@folio@@",folio);
            dataXml=dataXml.Replace("@@tipoRetorno@@",tipoRetorno);
            
            var client = new RestClient(URL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "text/plain");
            request.AddHeader("Accept", "text/xml");
            request.AddParameter("text/plain", dataXml ,  ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response.Content);
            string dataXmlRet = doc.InnerText;
            dataXmlRet = Regex.Replace(dataXmlRet, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");

            XDocument docd = XDocument.Parse(dataXmlRet);

            Console.WriteLine(docd.Element("Respuesta").Element("Codigo").Value);
            Console.WriteLine(docd.Element("Respuesta").Element("Mensaje").Value);
        }
        static void generation(){

            var ruc="20394077101";
            var login="admin_hpo";
            var clave="abc123";

            string dataXml = File.ReadAllText("generation.xml");
            string dataText = File.ReadAllText("data.txt");

            dataXml=dataXml.Replace("@@ruc@@",ruc);
            dataXml=dataXml.Replace("@@login@@",login);
            dataXml=dataXml.Replace("@@clave@@",clave);
            dataXml=dataXml.Replace("@@data@@",dataText);
            
            var client = new RestClient(URL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "text/plain");
            request.AddHeader("Accept", "text/xml");
            request.AddParameter("text/plain", dataXml ,  ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response.Content);
            string dataXmlRet = doc.InnerText;
            dataXmlRet = Regex.Replace(dataXmlRet, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");

            XDocument docd = XDocument.Parse(dataXmlRet);

            Console.WriteLine(docd.Element("Respuesta").Element("Codigo").Value);
            Console.WriteLine(docd.Element("Respuesta").Element("Mensaje").Value);
        }

        static void call(){
            var url="http://spsa.paperless.com.pe/axis2/services/Online.OnlineHttpSoap11Endpoint";
            
            var ruc="20394077101";
            var login="admin_hpo";
            var clave="abc123";

            var tipoDoc="1";
            var folio="FA04-00119478";
            var tipoRetorno="3";

            var textBody= new StringBuilder();

            textBody.Append("<x:Envelope");
            textBody.Append("    xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\"");
            textBody.Append("    xmlns:ws=\"http://ws.online.asp.core.paperless.cl\">");
            textBody.Append("    <x:Header/>");
            textBody.Append("    <x:Body>");
            textBody.Append("        <ws:OnlineRecovery>");
            textBody.Append("            <ws:ruc>"+ruc+"</ws:ruc>");
            textBody.Append("            <ws:login>"+login+"</ws:login>");
            textBody.Append("            <ws:clave>"+clave+"</ws:clave>");
            textBody.Append("            <ws:tipoDoc>"+tipoDoc+"</ws:tipoDoc>");
            textBody.Append("            <ws:folio>"+folio+"</ws:folio>");
            textBody.Append("            <ws:tipoRetorno>"+tipoRetorno+"</ws:tipoRetorno>");
            textBody.Append("        </ws:OnlineRecovery>");
            textBody.Append("    </x:Body>");
            textBody.Append("</x:Envelope>");

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "text/plain");
            request.AddHeader("Accept", "text/xml");
            request.AddParameter("text/plain", textBody.ToString() ,  ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response.Content);
            string dataXml = doc.InnerText;
            dataXml = Regex.Replace(dataXml, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");

            XDocument docd = XDocument.Parse(dataXml);

            Console.WriteLine(docd.Element("Respuesta").Element("Codigo").Value);
            Console.WriteLine(docd.Element("Respuesta").Element("Mensaje").Value);
        }
    }
}
