using System;
using System.Xml;
using System.IO;

namespace SimpleConfigLib
{
	/// <summary>
	/// Summary description for SimpleConfig.
	/// </summary>
	public class SimpleConfig
	{
		private string _sConfig;
		private XmlDocument _oDoc=new XmlDocument();
		private readonly string ROOT="config";
		private readonly string LISTPOSTFIX="_";

		public SimpleConfig(string sPath, string sFile)
		{
			_sConfig=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+"\\"+sPath;
			if(!Directory.Exists(_sConfig))
				Directory.CreateDirectory(_sConfig);
			_sConfig+="\\"+sFile;
			try 
			{
				_oDoc.Load(_sConfig);
			}
			catch(FileNotFoundException e) 
			{
				XmlTextWriter oWriter=new XmlTextWriter(_sConfig,null);
				oWriter.WriteStartDocument();
				oWriter.WriteStartElement(ROOT);
				oWriter.WriteEndElement();
				oWriter.WriteEndDocument();
				oWriter.Flush();
				oWriter.Close();
				_oDoc.Load(_sConfig);
			}
		}

		public string getValue(string sName, string sKey, string sParam, string sDefault) 
		{
			XmlNode oNode=getXmlNode(sName,sKey);
			//check for node
			if(oNode==null) 
			{
				return sDefault;
			}
			//get value
			oNode=getXmlNode(oNode,sParam);
			if(oNode==null) 
			{
				return sDefault;
			}
			//return value
			return oNode.Attributes["key"].Value;
		}
		public string[] getValues(string sName, string sKey, string sParam, string[] sDefaults) 
		{
			XmlNode oNode=getXmlNode(sName,sKey);
			//check for node
			if(oNode==null) 
			{
				return sDefaults;
			}
			//get values
			oNode=getXmlNode(oNode,sParam);
			if(oNode==null) 
			{
				return sDefaults;
			}
			//return values
			XmlNodeList oNodes=getXmlNodeList(oNode,sParam+LISTPOSTFIX);
			string[] sVals=new string[oNodes.Count];
			int i=0;
			foreach(XmlNode oNode_ in oNodes) 
			{
				sVals[i++]=oNode_.Attributes["key"].Value;
			}
			return sVals;
		}

		public void setValue(string sName, string sKey, string sParam, string sVal) 
		{
			//check if node exists
			XmlNode oNode=getXmlNode(sName,sKey);
			if(oNode==null) 
			{
				oNode=createXmlNode(sName,sKey);
			}
			setXmlParam(oNode, sParam, sVal);
		}
		public void setValues(string sName, string sKey, string sParam, string[] sVals) 
		{
			//check if node exists
			XmlNode oNode=getXmlNode(sName,sKey);
			if(oNode==null) 
			{
				oNode=createXmlNode(sName,sKey);
			}
			setXmlParams(oNode, sParam, sVals);
		}
		//create
		private XmlNode createXmlNode(string sName, string sKey) 
		{
			return createXmlNode(_oDoc.SelectSingleNode("//"+ROOT),sName,sKey);
		}
		private XmlNode createXmlNode(XmlNode oNode, string sName, string sKey) 
		{
			XmlElement oNew=_oDoc.CreateElement(sName);
			oNew.SetAttribute("key",sKey);
			oNode.AppendChild(oNew);
			_oDoc.Save(_sConfig);
			return getXmlNode(oNode,sName,sKey);
		}
		private XmlNodeList createXmlNodeList(XmlNode oNode, string sName, string[] sKeys) 
		{
			XmlElement oNew=_oDoc.CreateElement(sName);
			oNode.AppendChild(oNew);
			foreach(string sKey in sKeys) 
			{
				createXmlNode(oNew,sName+LISTPOSTFIX,sKey);
			}
			_oDoc.Save(_sConfig);
			return getXmlNodeList(oNode,sName+LISTPOSTFIX);
		}
		//get
		private XmlNode getXmlNode(string sName, string sKey) 
		{
			return getXmlNode(_oDoc.SelectSingleNode("//"+ROOT),sName,sKey);
		}
		private XmlNode getXmlNode(XmlNode oNode, string sName, string sKey) 
		{
			return oNode.SelectSingleNode(sName+"[@key=\""+sKey+"\"]");
		}
		private XmlNode getXmlNode(XmlNode oNode, string sName) 
		{
			return oNode.SelectSingleNode(sName);
		}
		private XmlNodeList getXmlNodeList(XmlNode oNode, string sName) 
		{
			return oNode.SelectNodes(sName);
		}
		//set
		private XmlNode setXmlParam(XmlNode oNode, string sParam, string sVal) 
		{
			//check it param exisits
			XmlNode oParam=getXmlNode(oNode,sParam);
			if(oParam!=null) 
			{
				//remove param
				oNode.RemoveChild(oParam);
			}
			//set param
			return createXmlNode(oNode,sParam,sVal);
		}
		private XmlNodeList setXmlParams(XmlNode oNode, string sParam, string[] sVals) 
		{
			//check it param exisits
			XmlNode oParam=getXmlNode(oNode,sParam);
			if(oParam!=null) 
			{
				//check if it has children
				XmlNodeList oKids=getXmlNodeList(oParam,sParam+LISTPOSTFIX);
				foreach(XmlNode oKid in oKids) 
				{
					//remove param
					oParam.RemoveChild(oKid);
				}
				//remove param
				oNode.RemoveChild(oParam);
			}
			//set param
			return createXmlNodeList(oNode,sParam,sVals);
		}
	}
}
