using System;
using System.IO;
using System.Collections.Generic;

namespace CDFcore{
	public class CDFfile{
		string CDFpath;
		string tableName;
			
		List<byte> CDFbytesList = new List<byte>();
		
		public Dictionary<string, List<byte>> DataValues = new Dictionary<string, List<byte>>();
		
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
					
					
					
					List<byte> DataList = new List<byte>();
					//DataList.Add((byte)TypeN);
					
					for(int j = 0; j < DataBytes.Count; j++){
						DataList.Add(DataBytes[j]);
					}
					
					if(TypeN == 1){
						DataList.Reverse();
					}
					
					DataList.Insert(0, (byte)TypeN);
					
					DataValues[ChunkName] = DataList;
					
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
		
		public int addInt(string fieldName, int Value){
			byte[] ByteVal = BitConverter.GetBytes(Value);
			
			List<byte> ListVal = new List<byte>();
			ListVal.Add((byte)1);
			
			for(int j = 0; j < ByteVal.Length; j++){
				ListVal.Add(ByteVal[j]);
			}
			
			DataValues[fieldName] = ListVal;
			
			return 0;
		}
		
		public int popData(){
			DataValues.Clear();
			
			return 0;
		}
		
		public int getInt(string fieldName){
			if(DataValues.ContainsKey(fieldName)){			
				return BitConverter.ToInt32(DataValues[fieldName].ToArray(), 1);
			}else{
				return 0;
			}
		}
		
		public double getDouble(string fieldName){
			if(DataValues.ContainsKey(fieldName)){				
				return BitConverter.ToDouble(DataValues[fieldName].ToArray(), 1);
			}else{
				return 0;
			}
		}
		
		public string getString(string fieldName){
			if(DataValues.ContainsKey(fieldName)){
				List<byte> Value = new List<byte>();
				
				for(int j = 1; j < DataValues[fieldName].Count; j++){
					Value.Add(DataValues[fieldName][j]);
				}
				
				return System.Text.Encoding.ASCII.GetString(Value.ToArray()).Trim();
			}else{
				return "";
			}
		}
	}
}