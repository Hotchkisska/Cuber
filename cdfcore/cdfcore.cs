using System;
using System.IO;
using System.Collections.Generic;

namespace CDFcore{
	public class CDFfile{
		string CDFpath;
		string tableName;
			
		List<byte> CDFbytesList = new List<byte>();
		
		public Dictionary<string, int> IntValues = new Dictionary<string, int>();
		public Dictionary<string, double> DoubleValues = new Dictionary<string, double>();
		public Dictionary<string, string> StringValues = new Dictionary<string, string>();
		
		public CDFfile(){
			
		}
		
		public int readFile(string cdfPath){
			if(!File.Exists(cdfPath)){
				throw new Exception("CDF: No such file");
			}
			
			this.CDFpath = cdfPath;
			
			FileStream CDFfileObj = new FileStream(cdfPath, FileMode.Open);
			
			int fileLen = (int)CDFfileObj.Length;
			byte [] CDFbytesArr = new byte[fileLen];
			
			CDFbytesList.Capacity = fileLen+1;
			
			CDFfileObj.Read(CDFbytesArr, 0, fileLen);
			CDFfileObj.Close();
			
			for(int i = 0; i < fileLen; i++){
				CDFbytesList.Add(CDFbytesArr[i]);
			}
			
			byte[] Sign = new byte[3];
			
			for(int i = 0; i < 3; i++){
				Sign[i] = CDFbytesList[i];
			}
			
			string CDFheader = System.Text.Encoding.ASCII.GetString(Sign).Trim();
			
			if(CDFheader == "CDF"){
				int CDFtableNameLen = (int)CDFbytesList[3];
				int cur = 4+CDFtableNameLen;
				while (cur < fileLen){
					int ChunkNameLen = (int)CDFbytesList[cur];

					byte[] ChunkNameBytes = new byte[ChunkNameLen];
					for(int j = 0; j < ChunkNameLen; j++){
						ChunkNameBytes[j] = CDFbytesList[cur+j+1];
					}
					
					string ChunkName = System.Text.Encoding.ASCII.GetString(ChunkNameBytes).Trim();
					
					cur = cur + ChunkNameLen + 1;
					
					if(cur > fileLen){
						throw new Exception("CDF: Index too big (file is potentially corrupted)");
					}
					
					int TypeN = (int)CDFbytesList[cur];
					
					cur++;
					
					if(cur > fileLen){
						throw new Exception("CDF: Index too big (file is potentially corrupted)");
					}
					
					List<byte> LenBytes = new List<byte>();
					LenBytes.Capacity = 4;
					for(int j = 0; j < 4; j++){

						LenBytes.Add(CDFbytesList[cur+j]);
					}
					
					LenBytes.Reverse();
					
					int DataLen = BitConverter.ToInt32(LenBytes.ToArray(), 0);

					cur = cur + 4;
					
					if(cur > fileLen){
						throw new Exception("CDF: Index too big (file is potentially corrupted)");
					}
					
					List<byte> DataBytes = new List<byte>(DataLen);
					for(int j = 0; j < DataLen; j++){
						DataBytes.Add(CDFbytesList[j+cur]);
					}
					
					if(TypeN == 1){
						DataBytes.Reverse();
						int Value = BitConverter.ToInt32(DataBytes.ToArray(), 0);
						IntValues[ChunkName] = Value;
					}else if(TypeN == 2){
						double Value = BitConverter.ToDouble(DataBytes.ToArray(), 0);
						DoubleValues[ChunkName] = Value;
					}else if(TypeN == 3){
						string Value = System.Text.Encoding.ASCII.GetString(DataBytes.ToArray()).Trim();
						StringValues[ChunkName] = Value;
					}
					
					cur = cur + DataLen;
					
					if(cur > fileLen){
						throw new Exception("CDF: Index too big (file is potentially corrupted)");
					}
				}
			}else{
				throw new Exception("CDF: Wrong file header (" + CDFheader + ")");
			}
			
			return 0;
		}
		
		public string getTableName(){
			byte[] Sign = new byte[3];
			for(int i = 0; i < 3; i++){
				Sign[i] = CDFbytesList[i];
			}
			
			string CDFheader = System.Text.Encoding.ASCII.GetString(Sign).Trim();
			
			if(CDFheader != "CDF"){
				return "Invalid";
			}else{
				int TableNameLen = (int)CDFbytesList[3];
				byte[] TableName = new byte[TableNameLen];
				
				for(int i = 0; i < TableNameLen; i++){
					TableName[i] = CDFbytesList[i+4];
				}
				
				string CDFtableName = System.Text.Encoding.ASCII.GetString(TableName).Trim();
				
				tableName = CDFtableName;
				return CDFtableName;
			}
		}
		
		public int writeFile(string path){
			return 0;
		}
		
		public int popData(){
			IntValues.Clear();
			DoubleValues.Clear();
			StringValues.Clear();
			
			return 0;
		}
		
		public int addInt(string fieldName, int data){
			IntValues[fieldName] = data;
			return 0;
		}
		
		public int addDouble(string fieldName, double data){
			DoubleValues[fieldName] = data;
			return 0;
		}
		
		public int addString(string fieldName, string data){
			StringValues[fieldName] = data;
			return 0;
		}
		
		public int getInt(string fieldName){
			if(IntValues.ContainsKey(fieldName)){
				return IntValues[fieldName];
			}else{
				return 0;
			}
		}
		
		public double getDouble(string fieldName){
			if(DoubleValues.ContainsKey(fieldName)){
				return DoubleValues[fieldName];
			}else{
				return 0.0;
			}
		}
		
		public string getString(string fieldName){
			if(StringValues.ContainsKey(fieldName)){
				return StringValues[fieldName];
			}else{
				return "";
			}
		}
	}
}