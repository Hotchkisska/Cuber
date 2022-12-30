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
					if(cur > fileLen){
						throw new Exception("CDF: Index too big (file is potentially corrupted)");
					}
					
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
		
		public int writeFile(string path, string tableName){
			List<byte> btw = new List<byte>();
			
			btw.Add((byte)67);
			btw.Add((byte)68);
			btw.Add((byte)70);
			
			byte TableNameLen = (byte)tableName.Length; //67 68 70
			byte[] TableHeaderBytes = System.Text.Encoding.ASCII.GetBytes(tableName);
			
			btw.Add(TableNameLen);
			for(int j = 0; j < TableHeaderBytes.Length; j++){
				btw.Add(TableHeaderBytes[j]);
			}

			foreach(var Pair in DataValues){			
				byte TypeByte = Pair.Value[0];
				int TypeInt = (int)TypeByte;
				byte ChunkNameLen = (byte)Pair.Key.Length; //Encoding.ASCII.GetBytes("ABC0000").Dump();
				
				int DataLenInt = 0;
				
				if(TypeInt == 1){
					DataLenInt = 4;
				}else if (TypeInt == 2){
					DataLenInt = 8;
				}else if (TypeInt == 3){
					DataLenInt = Pair.Value.Count;
				}
				
				byte[] DataLen = BitConverter.GetBytes(DataLenInt);
				
				byte[] ChunkNameBytes = System.Text.Encoding.ASCII.GetBytes(Pair.Key);
				
				byte[] ValueBytes = new byte[Pair.Value.Count - 1];
				
				for(int j = 1; j < Pair.Value.Count; j++){
					ValueBytes[j-1] = Pair.Value[j];
				}
				
				btw.Add(ChunkNameLen);
				for(int j = 0; j < ChunkNameBytes.Length; j++){
					btw.Add(ChunkNameBytes[j]);
				}
				btw.Add(TypeByte);
				
				List<byte> DataLenList = new List<byte>(DataLen);
				DataLenList.Reverse();
				
				for(int j = 0; j < DataLenList.Count; j++){
					btw.Add(DataLenList[j]);
				}
				
				List<byte> ValueBytesList = new List<byte>(ValueBytes);
				
				if(TypeInt == 1){
					ValueBytesList.Reverse();
				}
				
				for(int j = 0; j < ValueBytesList.Count; j++){
					btw.Add(ValueBytesList[j]);
				}
			}
			
			FileStream FileObj = new FileStream(path, FileMode.Create);
			FileObj.Write(btw.ToArray(), 0, btw.Count);
			FileObj.Close();
			
			return 0;
		}
		
		public int writeFileAppend(string path){
			List<byte> btw = new List<byte>();
			foreach(var Pair in DataValues){			
				byte TypeByte = Pair.Value[0];
				int TypeInt = (int)TypeByte;
				byte ChunkNameLen = (byte)Pair.Key.Length; //Encoding.ASCII.GetBytes("ABC0000").Dump();
				
				int DataLenInt = 0;
				
				if(TypeInt == 1){
					DataLenInt = 4;
				}else if (TypeInt == 2){
					DataLenInt = 8;
				}else if (TypeInt == 3){
					DataLenInt = Pair.Value.Count;
				}
				
				byte[] DataLen = BitConverter.GetBytes(DataLenInt);
				
				byte[] ChunkNameBytes = System.Text.Encoding.ASCII.GetBytes(Pair.Key);
				
				byte[] ValueBytes = new byte[Pair.Value.Count - 1];
				
				for(int j = 1; j < Pair.Value.Count; j++){
					ValueBytes[j-1] = Pair.Value[j];
				}
				
				btw.Add(ChunkNameLen);
				for(int j = 0; j < ChunkNameBytes.Length; j++){
					btw.Add(ChunkNameBytes[j]);
				}
				btw.Add(TypeByte);
				
				List<byte> DataLenList = new List<byte>(DataLen);
				DataLenList.Reverse();
				
				for(int j = 0; j < DataLenList.Count; j++){
					btw.Add(DataLenList[j]);
				}
				
				List<byte> ValueBytesList = new List<byte>(ValueBytes);
				
				if(TypeInt == 1){
					ValueBytesList.Reverse();
				}
				
				for(int j = 0; j < ValueBytesList.Count; j++){
					btw.Add(ValueBytesList[j]);
				}
			}
			
			FileStream FileObj = new FileStream(path, FileMode.Append);
			FileObj.Write(btw.ToArray(), 0, btw.Count);
			FileObj.Close();
			
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
		
		public int addDouble(string fieldName, double Value){
			byte[] ByteVal = BitConverter.GetBytes(Value);
			
			List<byte> ListVal = new List<byte>();
			ListVal.Add((byte)2);
			
			for(int j = 0; j < ByteVal.Length; j++){
				ListVal.Add(ByteVal[j]);
			}
			
			DataValues[fieldName] = ListVal;
			
			return 0;
		}
		
		public int addString(string fieldName, string Value){
			byte[] ByteVal = new byte[Value.Length+1];
			
			ByteVal[0] = (byte)3;
			
			for(int j = 1; j < Value.Length; j++){
				ByteVal[j] = (byte)Value[j-1];
			}
			
			DataValues[fieldName] = new List<byte>(ByteVal);
			
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