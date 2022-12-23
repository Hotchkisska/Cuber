using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CDFcore;

namespace CDFcoreShell{	
    class Program{	
		
		static void Loop(){
			Console.WriteLine("CDF core v.1");
			while(true){
				Console.Write(">>> ");
				string com = Console.ReadLine();
				string[] words = com.Split(new char[] { ' ' });
				//Console.WriteLine(com);
				
				if(com == "help" || com == "h" || com == "?"){
					
				}else if(words[0] == "read"){
					if(words.Length == 2){
						string cdfPath = words[1];
						
						CDFfile obj = new CDFfile();
						obj.readFile(cdfPath);
						foreach(var Pair in obj.DataValues){
							int Type = Pair.Value[0];
							string TypeString = "UNDEF";
							
							if(Type == 1){
								TypeString = "INT";
							}else if(Type == 2){
								TypeString = "FLOAT";
							}else if(Type == 3){
								TypeString = "STRING";
							}
							
							Console.Write(TypeString + " " + Pair.Key + " ");
							
							if(Type == 1){
								Console.Write(obj.getInt(Pair.Key));
								Console.Write("\n");
							}else if(Type == 2){
								Console.Write(obj.getDouble(Pair.Key));
								Console.Write("\n");
							}else if(Type == 3){
								Console.Write(System.Text.Encoding.ASCII.GetString(Pair.Value.ToArray()).Trim());
								Console.Write("\n");
							}
						}
					}
				}/* else if(words[0] == "add"){
					if(words.Length == 4){
						string Type = words[1].ToLower();
						string fieldName = words[2];
						string Value = words[3];
						
						if(Type == "int"){						
							int IntVal = 0;
							int.TryParse(Value, IntVal);
							obj.addInt(IntVal);
						}
					}
				} */
			}
		}
		
        static void Main(string[] args){
			if(args.Length == 0){
				Loop();
			}
        }
    }
}