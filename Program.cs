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
        static void Main(string[] args)
        {

            String sfolios = File.ReadAllText("data.in");
            string [] folios= sfolios.Split("\r\n");

            foreach (var item in folios)
            {

                if (item!=""){
                    string [] sLine = item.Split("\t");
                    string ruc = sLine[1];

                    string [] conf = getConf(ruc);

                    if (conf!=null){
                        if(conf.Length==6){
                            RetSovos retSovos1=  recovery(ruc, sLine[0],conf[1],conf[2]);

                            saveData(sLine[0],ruc,conf[4],conf[5],retSovos1.code, retSovos1.message );

                            if (retSovos1.message.StartsWith("Respuesta SUNAT: 2028 - 2028") 
                                || retSovos1.message.StartsWith("Respuesta SUNAT: 3127 - 3127")
                                || retSovos1.message.StartsWith("Respuesta SUNAT: 3031 - 3031") 
                                || retSovos1.message.StartsWith("Respuesta SUNAT: 3035 - 3035")
                                //|| retSovos1.message.StartsWith("Respuesta SUNAT: 3202 - ")
                                //|| retSovos1.message.StartsWith("Documento no encontrado, RUC ")
                                //|| retSovos1.message.StartsWith("Respuesta SUNAT: 1008 - 1008")
                                //|| retSovos1.message.StartsWith("No se encontro respuesta SUNAT")

                            ){
                                generation(sLine[0], sLine[1],conf[1],conf[2],conf[3]);
                            }                            
                        }
                    }
                }
                //
            }
        }
        static string[] getConf(string ruc){
            String content = File.ReadAllText("conf.env");
            string [] lines= content.Split("\r\n");
            foreach (var line in lines)
            {
                if(!line.StartsWith("#")){
                    string [] data = line.Split("|");
                    if(data[0].Equals(ruc)){
                        return data;
                    }
                }
            }
            return null;
        }

        static RetSovos recovery(string ruc,string folio, string login, string clave){

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

            string message=docd.Element("Respuesta").Element("Mensaje").Value;
            int i  =message.IndexOf("\n");
            if(i>0){
                message = message.Substring(0,i);
            }
            //Console.WriteLine(i);

            Console.WriteLine("Folio {0}: Status: {1} - {2}",
                folio,
                docd.Element("Respuesta").Element("Codigo").Value,
                message);

            RetSovos retSovos = new RetSovos();
            retSovos.code=  Int32.Parse(docd.Element("Respuesta").Element("Codigo").Value)  ;
            retSovos.message = docd.Element("Respuesta").Element("Mensaje").Value;
            return retSovos;
        }
        static void generation(string folio, string ruc,string login, string clave, string conString){

            string dataXml = File.ReadAllText( "generation.xml");
            string dataText = getData(folio,ruc,conString);
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

        static String getData(String folio, String ruc, string conString){
            //String conString = "user id=ecthp;Password=SE1010;Data Source=10.20.11.11:1542/hpt01";
            //String conString = "user id=tiendas_adm;Password=tdatpsa;Data Source=10.20.11.21:1525/pmm";
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

        static void saveData(string folio, string ruc, string conString,  string tableName, int retCode, string retMessage){
            using(OracleConnection cn = new OracleConnection(conString)){
                cn.Open();
                string update = "update " + tableName + " set codigo_sovos=" + retCode + ", mensaje_sovos='" + retMessage + "' where"
                 + " folio='" + folio + "' and ruc_emisor='" + ruc + "' and tipodoc='TFC'";
                OracleCommand cmd = cn.CreateCommand();
                cmd.CommandText= update;
                cmd.CommandType= System.Data.CommandType.Text;

                cmd.ExecuteNonQuery();

            }

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
    public class RetSovos{
        public int code { get; set; }
        public string  message { get; set; }
    }
}
