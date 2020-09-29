using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Oracle.ManagedDataAccess.Client;
using RestSharp;

namespace sovos1
{
    class Program
    {
        const string URL = "http://spsa.paperless.com.pe/axis2/services/Online.OnlineHttpSoap11Endpoint";
        static string ruc;
        static string login;
        static string clave;
        static void Main(string[] args)
        {
            ruc="20536557858"; //HP
            //ruc="20394077101"; //HPO
            login="admin_hpo";
            clave="abc123";
            string [] folios={
"FA18-00516964",
"FA18-00527684",
"FA20-00340987",
"FA20-00364797",
"FA24-00339505",
"FA24-00294784",
"FA25-00039022",
"FA30-00184374"

            };

            foreach (var item in folios)
            {
                string message = recovery(item);
                if (message.StartsWith("Respuesta SUNAT: 2028 - 2028") || message.StartsWith("Respuesta SUNAT: 3127 - 3127")
                  || message.StartsWith("Respuesta SUNAT: 3031 - 3031") || message.StartsWith("Respuesta SUNAT: 3035 - 3035")
                  ){
                    Console.WriteLine("Procesar "+ item);
                    generation(item, ruc);
                }
                //
            }
        }

        static String recovery(string folio){
            //var ruc="20394077101";
            //var login="admin_hpo";
            //var clave="abc123";


            var tipoDoc="1";
            //var folio="FA01-00447663";
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

            Console.WriteLine("Folio {0}: Status: {1} - {2}",folio,docd.Element("Respuesta").Element("Codigo").Value,docd.Element("Respuesta").Element("Mensaje").Value);
            return docd.Element("Respuesta").Element("Mensaje").Value;
        }
        static void generation(string folio, string ruc){

            string dataXml = File.ReadAllText( "generation.xml");
            string dataText = getData(folio,ruc);
            if(dataText!=""){
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
        }

        static String getData(String folio, String ruc){
            String conString = "user id=ecthp;Password=SE1010;Data Source=10.20.11.11:1542/hpt01";
            String retData="";
            using(OracleConnection cn = new OracleConnection(conString)){
                cn.Open();
                String query = "select databody from TRX_DATA_ELECTRONIC c where c.ruc='" +
                ruc + "' and c.folio='" + folio+ "'" ;

                OracleCommand cmd = cn.CreateCommand();
                cmd.CommandText=query;
                cmd.CommandType= System.Data.CommandType.Text;

                OracleDataReader dr = cmd.ExecuteReader();

                if(dr.Read()){
                    //Console.WriteLine (dr.GetString(0));
                    retData=dr.GetString(0);
                }
                dr.Close();
            }
            return retData;
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
